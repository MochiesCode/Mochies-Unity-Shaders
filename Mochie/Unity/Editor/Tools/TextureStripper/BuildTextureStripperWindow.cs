using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using PackageSource = UnityEditor.PackageManager.PackageSource;

namespace Mochie {
    /// <summary>
    /// Editor window for inspecting, previewing, and configuring the Mochie Build Pipeline Texture Stripper.
    /// </summary>
    public class BuildTextureStripperWindow : EditorWindow {
        private const float FloatingWidth = 560f;
        private static readonly Vector2 DockedMinSize = new Vector2(480f, 500f);
        private static readonly Vector2 DockedMaxSize = new Vector2(4000f, 4000f);

        private GameObject targetAvatarOrObject;
        private Vector2 scrollPos;
        private Vector2 windowScrollPos;
        private bool resizeQueued;
        private List<ScannedMaterialEntry> scannedEntries = new List<ScannedMaterialEntry>();
        private bool hasScanned = false;
        private int totalUnusedSlotsCount = 0;
        private GUIStyle richLabel;

        private class ScannedMaterialEntry {
            public Material material;
            public List<UnusedTextureInfo> unusedTextures;
            public bool isExpanded = true;
        }

        [MenuItem("Tools/Mochie/Build Texture Stripper", priority = 1002)]
        public static void ShowWindow() {
            var window = GetWindow<BuildTextureStripperWindow>("Texture Stripper");
            window.Show();
        }

        [MenuItem("GameObject/Mochie/Analyze Textures for Build", false, 49)]
        public static void AnalyzeGameObjectContext(MenuCommand command) {
            GameObject go = command.context as GameObject;
            if (go != null) {
                var window = GetWindow<BuildTextureStripperWindow>("Texture Stripper");
                window.targetAvatarOrObject = go;
                window.Show();
                window.RunScan();
            }
        }

        private void OnEnable() {
            // A resize queued before a script reload never runs, so don't let it block the next one
            resizeQueued = false;
            if (targetAvatarOrObject == null && Selection.activeGameObject != null) {
                targetAvatarOrObject = Selection.activeGameObject;
            }
        }

        private void OnGUI() {
            if (richLabel == null) {
                richLabel = new GUIStyle(EditorStyles.label) { richText = true };
            }

            Rect body = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            windowScrollPos = EditorGUILayout.BeginScrollView(windowScrollPos);
            Rect content = EditorGUILayout.BeginVertical();

            EditorGUILayout.Space(6);
            DrawHeader();
            EditorGUILayout.Space(6);

            DrawSettingsSection();
            EditorGUILayout.Space(10);

            DrawScanSection();
            EditorGUILayout.Space(10);

            DrawResultsSection();
            EditorGUILayout.Space(6);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // Content rects only have their real sizes during Repaint. The body fills the visible area, so the
            // difference to the content is how much taller the window needs to be, whatever its tab and border take.
            if (docked) {
                FitToContent(0f);
            }
            else if (Event.current.type == EventType.Repaint) {
                FitToContent(position.height + content.height - body.height);
            }
        }

        // A floating window is sized to its contents, with its min and max size set to match so it can't be
        // resized by hand. Taller content than the screen fits scrolls instead. Docked windows are sized by
        // their dock area, so they get the normal limits back.
        private void FitToContent(float windowHeight) {
            Vector2 size;
            if (docked) {
                size = DockedMinSize;
            }
            else {
                float screenHeight = Screen.currentResolution.height / EditorGUIUtility.pixelsPerPoint - 100f;
                size = new Vector2(FloatingWidth, Mathf.Min(Mathf.Ceil(windowHeight), screenHeight));
            }
            // Sizes are compared within a pixel, since Unity can round them at fractional display scaling
            bool fits = docked
                ? minSize == DockedMinSize && maxSize == DockedMaxSize
                : Near(position.size, size) && Near(minSize, size) && Near(maxSize, size);
            if (fits || resizeQueued) return;
            // Resizing during OnGUI would change the layout mid-pass
            resizeQueued = true;
            EditorApplication.delayCall += () => {
                if (this == null) return;
                resizeQueued = false;
                // Changing the size limits can move a floating window, so keep where it was
                Vector2 windowPos = position.position;
                if (docked) {
                    minSize = DockedMinSize;
                    maxSize = DockedMaxSize;
                    return;
                }
                // Unity clamps each limit against the other, so the one that moves away from the current size goes first
                if (size.y > maxSize.y || size.x > maxSize.x) {
                    maxSize = size;
                    minSize = size;
                }
                else {
                    minSize = size;
                    maxSize = size;
                }
                position = new Rect(windowPos, size);
            };
        }

        private static bool Near(Vector2 a, Vector2 b) {
            return Mathf.Abs(a.x - b.x) < 1f && Mathf.Abs(a.y - b.y) < 1f;
        }

        private void DrawHeader() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Mochie Build Pipeline Texture Stripper", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Non-destructively removes unused texture references (such as separate metallic/roughness slots when using packed workflow) from Mochie materials when building VRChat avatars and worlds.",
                EditorStyles.wordWrappedMiniLabel
            );
            EditorGUILayout.EndVertical();
            EditorGUILayout.HelpBox("Work in progress, use at your own risk. This tool is experimental and may not work correctly in every project. Back up your project before using it, and check your builds with the test clone before uploading.", MessageType.Warning);
        }

        private void DrawSettingsSection() {
            var settings = BuildTextureStripperSettings.instance;
            GUILayout.Label("Build Pipeline Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            settings.StripOnBuild = EditorGUILayout.ToggleLeft(" Strip Unused Textures on Build", settings.StripOnBuild, EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(!settings.StripOnBuild);
            {
                settings.CheckAnimations = EditorGUILayout.ToggleLeft(" Animation Safety Check (Protect animated properties)", settings.CheckAnimations);
                settings.StripZeroValueFeatures = EditorGUILayout.ToggleLeft(" Strip Features Disabled by Zero Strength / Black Color", settings.StripZeroValueFeatures);
                if (settings.StripZeroValueFeatures) {
                    EditorGUILayout.HelpBox("Only enable this if no scripts (e.g. Udon) or material property blocks change these strengths or colors at runtime. Animations are still checked.", MessageType.Warning);
                }
                settings.LogToConsole = EditorGUILayout.ToggleLeft(" Log Build Summary in Console", settings.LogToConsole);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        private void DrawScanSection() {
            GUILayout.Label("Analyze & Preview", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            targetAvatarOrObject = (GameObject)EditorGUILayout.ObjectField("Target Avatar / Object", targetAvatarOrObject, typeof(GameObject), true);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Use Selected Object", GUILayout.Height(24))) {
                if (Selection.activeGameObject != null) {
                    targetAvatarOrObject = Selection.activeGameObject;
                }
            }

            EditorGUI.BeginDisabledGroup(targetAvatarOrObject == null);
            if (GUILayout.Button("Scan Unused Textures", GUILayout.Height(24))) {
                RunScan();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void RunScan() {
            HashSet<string> animatedProps = BuildTextureStripperSettings.instance.CheckAnimations && targetAvatarOrObject != null ?
                BuildTextureStripper.GetAnimatedPropertyNames(targetAvatarOrObject) : null;
            RunScan(animatedProps);
        }

        private void RunScan(HashSet<string> animatedProps) {
            scannedEntries.Clear();
            totalUnusedSlotsCount = 0;
            hasScanned = true;

            if (targetAvatarOrObject == null) return;

            foreach (MaterialScanResult result in BuildTextureStripper.FindUnusedTextures(new[] { targetAvatarOrObject }, animatedProps)) {
                scannedEntries.Add(new ScannedMaterialEntry {
                    material = result.material,
                    unusedTextures = result.unusedTextures,
                    isExpanded = true
                });
                totalUnusedSlotsCount += result.unusedTextures.Count;
            }
        }

        private void DrawResultsSection() {
            if (!hasScanned) return;

            GUILayout.Label($"Scan Results ({scannedEntries.Count} materials, {totalUnusedSlotsCount} unused texture slots)", EditorStyles.boldLabel);

            if (scannedEntries.Count == 0) {
                EditorGUILayout.HelpBox("No unused texture slots detected! All Mochie materials on this object are fully optimized.", MessageType.Info);
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MinHeight(300), GUILayout.MaxHeight(500));
            foreach (var entry in scannedEntries) {
                if (entry.material == null) continue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                entry.isExpanded = EditorGUILayout.Foldout(entry.isExpanded, $"{entry.material.name} ({entry.unusedTextures.Count} unused slots)", true, EditorStyles.foldoutHeader);
                if (GUILayout.Button("Ping", GUILayout.Width(45), GUILayout.Height(18))) {
                    EditorGUIUtility.PingObject(entry.material);
                }
                EditorGUILayout.EndHorizontal();

                if (entry.isExpanded) {
                    EditorGUILayout.LabelField($"Shader: {entry.material.shader.name}", EditorStyles.miniLabel);
                    EditorGUI.indentLevel++;
                    foreach (var unused in entry.unusedTextures) {
                        string textureName = unused.assignedTexture != null ? unused.assignedTexture.name : "missing";
                        string buildOnly = unused.buildOnly ? " <i>(build only)</i>" : "";
                        EditorGUILayout.LabelField($"<b>{unused.propertyName}</b> ({textureName}){buildOnly}", richLabel);
                        EditorGUILayout.LabelField($"Reason: {unused.reason}", EditorStyles.wordWrappedMiniLabel);
                        EditorGUILayout.Space(2);
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.LabelField("Slots marked (build only) are shader default textures or features the shader doesn't currently use. They're stripped from builds but kept by Clean In-Place.", EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Create Non-Destructive Test Clone in Scene", GUILayout.Height(28))) {
                CreateTestClone();
            }

            if (GUILayout.Button("Clean In-Place (Modify Project Assets)", GUILayout.Height(28))) {
                CleanInPlace();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void CreateTestClone() {
            if (targetAvatarOrObject == null) return;

            GameObject clone = Instantiate(targetAvatarOrObject);
            clone.name = targetAvatarOrObject.name + "_StrippedTest";
            Undo.RegisterCreatedObjectUndo(clone, "Create Stripped Test Clone");

            var report = BuildTextureStripper.StripUnusedTextures(clone);
            Selection.activeGameObject = clone;

            EditorUtility.DisplayDialog(
                "Test Clone Created",
                $"Created '{clone.name}' in the scene with {report.TexturesStrippedCount} unused textures stripped across {report.MaterialsModified} materials.\n\nYour original project materials remain completely untouched!",
                "OK"
            );
        }

        private void CleanInPlace() {
            if (targetAvatarOrObject == null) return;

            // These are shared project assets, so protect anything animated in the open scenes, not just on the target.
            HashSet<string> animatedProps = null;
            if (BuildTextureStripperSettings.instance.CheckAnimations) {
                var roots = new List<GameObject> { targetAvatarOrObject };
                for (int i = 0; i < SceneManager.sceneCount; i++) {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.isLoaded) roots.AddRange(scene.GetRootGameObjects());
                }
                animatedProps = BuildTextureStripper.GetAnimatedPropertyNames(roots);
            }

            // Rescan so materials changed since the last scan aren't cleaned against stale results
            RunScan(animatedProps);

            var cleanable = new List<KeyValuePair<Material, List<UnusedTextureInfo>>>();
            int skippedReadOnly = 0;
            foreach (var entry in scannedEntries) {
                List<UnusedTextureInfo> slots = entry.unusedTextures.Where(unused => !unused.buildOnly).ToList();
                if (slots.Count == 0) continue;
                if (!IsEditableMaterialAsset(entry.material)) {
                    skippedReadOnly++;
                    continue;
                }
                cleanable.Add(new KeyValuePair<Material, List<UnusedTextureInfo>>(entry.material, slots));
            }

            int slotCount = cleanable.Sum(pair => pair.Value.Count);
            string readOnlyNote = skippedReadOnly > 0 ? $"\n\n{skippedReadOnly} material(s) that aren't editable .mat assets (embedded in a model or scene, or inside a read-only package) will be skipped." : "";
            if (slotCount == 0) {
                EditorUtility.DisplayDialog("Nothing to Clean", "No unused texture slots can be cleaned in place. Slots marked (build only) are only stripped from builds." + readOnlyNote, "OK");
                return;
            }

            bool confirm = EditorUtility.DisplayDialog(
                "Clean Material Assets In-Place?",
                $"This will clear {slotCount} unused texture references across {cleanable.Count} material assets.\n\n" +
                "These materials are shared project assets, so every object using them is affected. Animation checks only cover this object and the open scenes." +
                readOnlyNote +
                "\n\nThis action can be undone with Edit > Undo.",
                "Clean Assets",
                "Cancel"
            );

            if (!confirm) return;

            foreach (var pair in cleanable) {
                Undo.RecordObject(pair.Key, "Clean Unused Material Textures");
                foreach (var unused in pair.Value) {
                    pair.Key.SetTexture(unused.propertyName, null);
                }
                EditorUtility.SetDirty(pair.Key);
            }

            AssetDatabase.SaveAssets();
            RunScan(animatedProps);

            EditorUtility.DisplayDialog(
                "Assets Cleaned",
                $"Cleared {slotCount} unused texture slots on {cleanable.Count} materials.",
                "OK"
            );
        }

        private static bool IsEditableMaterialAsset(Material mat) {
            string path = AssetDatabase.GetAssetPath(mat);
            if (!path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase) || !AssetDatabase.IsOpenForEdit(mat)) {
                return false;
            }
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path);
            return package == null || package.source == PackageSource.Embedded || package.source == PackageSource.Local;
        }
    }
}
