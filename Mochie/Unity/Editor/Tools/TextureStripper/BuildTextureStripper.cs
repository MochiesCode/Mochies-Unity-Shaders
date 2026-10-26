using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Object = UnityEngine.Object;

namespace Mochie {
    /// <summary>
    /// Report detailing the results of an optimization / stripping pass.
    /// </summary>
    public class TextureStripperReport {
        public int TotalRenderersChecked = 0;
        public int TotalMaterialsChecked = 0;
        public int MochieMaterialsFound = 0;
        public int MaterialsModified = 0;
        public int TexturesStrippedCount = 0;
        public List<string> StrippedDetails = new List<string>();
        /// <summary>The stripped copies that were swapped onto renderers.</summary>
        public List<Material> CreatedClones = new List<Material>();
        /// <summary>Originals that something other than a renderer also uses, so their renderers keep them as they are.</summary>
        public List<Material> KeptOriginals = new List<Material>();
    }

    /// <summary>
    /// A material and the texture slots it doesn't use.
    /// </summary>
    public class MaterialScanResult {
        public Material material;
        public List<UnusedTextureInfo> unusedTextures;
    }

    /// <summary>
    /// Non-destructive texture stripping engine. Swaps Mochie materials on renderers for copies with unused texture
    /// references removed, so the original assets are never touched.
    /// </summary>
    public static class BuildTextureStripper {

        /// <summary>
        /// Strips unused texture references on the specified GameObject hierarchy using in-memory material copies.
        /// </summary>
        public static TextureStripperReport StripUnusedTextures(GameObject root) {
            return StripUnusedTextures(new[] { root }, root != null ? $"'{root.name}'" : "");
        }

        /// <summary>
        /// Strips unused texture references on every renderer under the given roots, swapping in in-memory material copies.
        /// All roots share one material cache, so a material used under several roots gets a single copy and those
        /// renderers can still be batched and instanced together. Materials that something other than a renderer also uses
        /// keep the original.
        /// </summary>
        public static TextureStripperReport StripUnusedTextures(IList<GameObject> roots, string label) {
            var settings = BuildTextureStripperSettings.instance;
            var report = new TextureStripperReport();
            List<GameObject> validRoots = roots.Where(r => r != null).ToList();
            if (validRoots.Count == 0) return report;

            HashSet<string> animatedProperties = settings.CheckAnimations ? GetAnimatedPropertyNames(validRoots) : null;

            // Original material -> copy (or the original itself when nothing is unused)
            var materialMap = new Dictionary<Material, Material>();
            // Original material -> the textures left out of its copy
            var copiedMaterials = new Dictionary<Material, List<UnusedTextureInfo>>();
            var checkedMaterials = new HashSet<Material>();

            foreach (GameObject root in validRoots) {
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true)) {
                    report.TotalRenderersChecked++;
                    foreach (Material mat in renderer.sharedMaterials) {
                        if (mat == null || !checkedMaterials.Add(mat)) continue;
                        if (!MochieShaderStripRules.IsMochieMaterial(mat)) continue;

                        Material target = mat;
                        List<UnusedTextureInfo> unusedList = MochieShaderStripRules.GetUnusedTextures(mat, animatedProperties, settings.StripZeroValueFeatures);
                        if (unusedList.Count > 0) {
                            target = Object.Instantiate(mat);
                            target.name = mat.name;
                            foreach (var unused in unusedList) {
                                target.SetTexture(unused.propertyName, null);
                            }
                            copiedMaterials[mat] = unusedList;
                        }
                        materialMap[mat] = target;
                    }
                }
            }

            report.TotalMaterialsChecked = checkedMaterials.Count;
            report.MochieMaterialsFound = materialMap.Count;

            if (copiedMaterials.Count > 0) {
                RemapMaterials(validRoots, materialMap);

                // Whatever else uses an original (a script changing its properties or swapping it back in, an animation material
                // swap, a prefab spawned at runtime) has to see the material the renderers draw, and the original ships either way
                report.KeptOriginals = FindStillReferencedOriginals(validRoots, materialMap);
                if (report.KeptOriginals.Count > 0) {
                    // Copy -> original
                    var restore = report.KeptOriginals.ToDictionary(mat => materialMap[mat], mat => mat);
                    RemapMaterials(validRoots, restore);
                    foreach (Material mat in report.KeptOriginals) {
                        materialMap[mat] = mat;
                        copiedMaterials.Remove(mat);
                    }
                    foreach (Material copy in restore.Keys) {
                        Object.DestroyImmediate(copy);
                    }
                }
            }

            foreach (KeyValuePair<Material, List<UnusedTextureInfo>> pair in copiedMaterials) {
                Material mat = pair.Key;
                foreach (var unused in pair.Value) {
                    report.TexturesStrippedCount++;
                    report.StrippedDetails.Add($"[{mat.name}] Stripped '{unused.propertyName}' ({unused.assignedTexture.name}) - Reason: {unused.reason}");
                }
                report.MaterialsModified++;
                report.CreatedClones.Add(materialMap[mat]);
            }

            LogReport(report, label, settings.LogToConsole);
            return report;
        }

        /// <summary>
        /// Finds unused texture slots on every Mochie material under the given roots without modifying anything.
        /// </summary>
        public static List<MaterialScanResult> FindUnusedTextures(IEnumerable<GameObject> roots, HashSet<string> animatedProperties) {
            bool allowValueRules = BuildTextureStripperSettings.instance.StripZeroValueFeatures;
            var results = new List<MaterialScanResult>();
            var seenMaterials = new HashSet<Material>();

            foreach (GameObject root in roots) {
                if (root == null) continue;
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true)) {
                    foreach (Material mat in renderer.sharedMaterials) {
                        if (mat == null || !seenMaterials.Add(mat)) continue;
                        if (!MochieShaderStripRules.IsMochieMaterial(mat)) continue;

                        List<UnusedTextureInfo> unused = MochieShaderStripRules.GetUnusedTextures(mat, animatedProperties, allowValueRules);
                        if (unused.Count > 0) {
                            results.Add(new MaterialScanResult { material = mat, unusedTextures = unused });
                        }
                    }
                }
            }
            return results;
        }

        public static HashSet<string> GetAnimatedPropertyNames(GameObject root) {
            return GetAnimatedPropertyNames(new[] { root });
        }

        /// <summary>
        /// Collects the material property names animated by any AnimationClip reachable from the given hierarchies.
        /// </summary>
        public static HashSet<string> GetAnimatedPropertyNames(IEnumerable<GameObject> roots) {
            var animatedProps = new HashSet<string>(StringComparer.Ordinal);
            var components = new List<Object>();
            var clips = new HashSet<AnimationClip>();

            foreach (GameObject root in roots) {
                if (root == null) continue;
                foreach (Component component in root.GetComponentsInChildren<Component>(true)) {
                    if (component == null) continue;
                    components.Add(component);
                    if (component is Animator animator && animator.runtimeAnimatorController != null) {
                        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips) {
                            if (clip != null) clips.Add(clip);
                        }
                    }
                }
            }

            // CollectDependencies follows every serialized reference, so this also finds clips used by Timeline assets,
            // legacy Animation components, VRChat playable layers and Modular Avatar / VRCFury components.
            foreach (Object dependency in EditorUtility.CollectDependencies(components.ToArray())) {
                if (dependency is AnimationClip clip) clips.Add(clip);
            }

            foreach (AnimationClip clip in clips) {
                ExtractClipProperties(clip, animatedProps);
            }
            return animatedProps;
        }

        /// <summary>
        /// Swaps the materials on every renderer under the given roots, and in the scene descriptor's Dynamic Materials list:
        /// before a world build starts, the VRChat SDK copies the materials of every active renderer into it, which would
        /// keep the originals (and all their textures) in the build.
        /// </summary>
        static void RemapMaterials(List<GameObject> roots, Dictionary<Material, Material> materialMap) {
            foreach (GameObject root in roots) {
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true)) {
                    Material[] sharedMaterials = renderer.sharedMaterials;
                    bool rendererModified = false;
                    for (int i = 0; i < sharedMaterials.Length; i++) {
                        Material mat = sharedMaterials[i];
                        if (mat != null && materialMap.TryGetValue(mat, out Material target) && target != mat) {
                            sharedMaterials[i] = target;
                            rendererModified = true;
                        }
                    }
                    if (rendererModified) {
                        renderer.sharedMaterials = sharedMaterials;
                    }
                }
#if VRC_SDK_VRCSDK3
                foreach (VRC.SDKBase.VRC_SceneDescriptor descriptor in root.GetComponentsInChildren<VRC.SDKBase.VRC_SceneDescriptor>(true)) {
                    List<Material> dynamicMaterials = descriptor.DynamicMaterials;
                    if (dynamicMaterials == null) continue;
                    for (int i = 0; i < dynamicMaterials.Count; i++) {
                        Material mat = dynamicMaterials[i];
                        if (mat != null && materialMap.TryGetValue(mat, out Material target)) {
                            dynamicMaterials[i] = target;
                        }
                    }
                }
#endif
            }
        }

        /// <summary>
        /// Originals that something besides the swapped renderer slots still references (scripts like Udon behaviours,
        /// animation material swaps, prefabs spawned at runtime, etc).
        /// </summary>
        static List<Material> FindStillReferencedOriginals(List<GameObject> roots, Dictionary<Material, Material> materialMap) {
            var originals = new HashSet<Material>(materialMap.Where(pair => pair.Value != pair.Key).Select(pair => pair.Key));
            var components = roots.SelectMany(root => root.GetComponentsInChildren<Component>(true))
                                  .Where(component => component != null)
                                  .Cast<Object>()
                                  .ToArray();
            var stillReferenced = new List<Material>();
            foreach (Object dependency in EditorUtility.CollectDependencies(components)) {
                if (dependency is Material mat && originals.Remove(mat)) {
                    stillReferenced.Add(mat);
                }
            }
            return stillReferenced;
        }

        static void LogReport(TextureStripperReport report, string label, bool logToConsole) {
            if (logToConsole && report.TexturesStrippedCount > 0) {
                Debug.Log($"<color=#66c2ff>[Mochie Texture Stripper]</color> Stripped <b>{report.TexturesStrippedCount}</b> unused texture references across <b>{report.MaterialsModified}</b> Mochie materials on {label}. Original project assets were untouched.\n{string.Join("\n", report.StrippedDetails)}");
            }
            if (logToConsole && report.KeptOriginals.Count > 0) {
                Debug.Log($"<color=#66c2ff>[Mochie Texture Stripper]</color> Kept <b>{report.KeptOriginals.Count}</b> Mochie materials on {label} as they are, since something other than a renderer also uses them (for example a script, an animation material swap or a prefab). They ship either way, and whatever uses them has to see the material the renderer draws.\n{string.Join("\n", report.KeptOriginals.Select(mat => mat.name))}");
            }
        }

        private static void ExtractClipProperties(AnimationClip clip, HashSet<string> propSet) {
            if (clip == null) return;

            // Float / Color curve bindings
            foreach (var binding in AnimationUtility.GetCurveBindings(clip)) {
                ParseBindingPropertyName(binding.propertyName, propSet);
            }

            // Object reference curve bindings
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip)) {
                ParseBindingPropertyName(binding.propertyName, propSet);
            }
        }

        private static void ParseBindingPropertyName(string propertyName, HashSet<string> propSet) {
            if (string.IsNullOrEmpty(propertyName)) return;

            // Unity material animation paths can take several forms:
            // 1. "material._Color.r", "material._EmissionStrength", "material._MainTex_ST.x"
            // 2. "m_Materials.Array.data[0]._Color.r", "m_Materials.Array.data[1]._EmissionMap"
            // 3. "materials.Array.data[0]._MainTex"
            string prop = null;
            if (propertyName.StartsWith("material.", StringComparison.Ordinal)) {
                prop = propertyName.Substring("material.".Length);
            }
            else {
                int dataIdx = propertyName.IndexOf(".Array.data[", StringComparison.Ordinal);
                if (dataIdx >= 0) {
                    int closeBracket = propertyName.IndexOf(']', dataIdx);
                    if (closeBracket >= 0 && closeBracket + 2 < propertyName.Length) {
                        prop = propertyName.Substring(closeBracket + 2);
                    }
                }
            }

            if (string.IsNullOrEmpty(prop)) return;

            // Add the exact raw property
            propSet.Add(prop);

            // Strip RGBA / XYZW component suffixes (e.g., "_EmissionColor.r" -> "_EmissionColor")
            int dotIdx = prop.LastIndexOf('.');
            if (dotIdx > 0) {
                string baseProp = prop.Substring(0, dotIdx);
                propSet.Add(baseProp);
                prop = baseProp;
            }

            // If it's a texture scale/offset animation (e.g. "_MainTex_ST"), also protect the base texture ("_MainTex")
            if (prop.EndsWith("_ST", StringComparison.Ordinal) && prop.Length > 3) {
                propSet.Add(prop.Substring(0, prop.Length - 3));
            }
        }
    }

    #region Scene Build Hook
    /// <summary>
    /// Runs for every scene being built: player builds and scene AssetBundles such as VRChat world uploads.
    /// VRChat avatars are built from a prefab rather than a scene and are handled by BuildTextureStripperAvatarHook.
    /// </summary>
    public class BuildTextureStripperSceneProcess : IProcessSceneWithReport {
        public int callbackOrder => -100;

        public void OnProcessScene(Scene scene, BuildReport report) {
            // Unity also calls this when entering Play mode
            if (EditorApplication.isPlayingOrWillChangePlaymode && !BuildPipeline.isBuildingPlayer) {
                return;
            }

            if (!BuildTextureStripperSettings.instance.StripOnBuild) {
                return;
            }

            try {
                BuildTextureStripper.StripUnusedTextures(scene.GetRootGameObjects(), $"scene '{scene.name}'");
            }
            catch (Exception ex) {
                Debug.LogError($"[Mochie Texture Stripper] Error during build scene preprocessing: {ex}");
            }
        }
    }
    #endregion
}
