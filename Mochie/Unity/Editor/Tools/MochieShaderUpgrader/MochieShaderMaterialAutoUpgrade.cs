using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public static class MochieShaderMaterialAutoUpgrade
    {
        // Shader swaps go first so a swapped material can continue through its family's versioned steps in the same pass.
        // Versioned upgrades look like this (see VersionedShaderUpgradeInfo):
        // new VersionedShaderUpgradeInfo(new[] { "Mochie/Standard", "Mochie/Standard Lite", "Mochie/Standard Mobile" },
        //     new VersionStep("1.79", new MochieMaterialUpgrade_Standard_1_79())),
        public static readonly ShaderUpgradeInfo[] ShaderUpgrades =
        {
            new ChangeShaderUpgradeInfo("Mochie/Standard", "cddaa3a02eb956746b502b80b76e92bc", "61dfaeeda7ef44742b40324ecd77f87c", new MochieMaterialUpgrade_V0_To_V1()),
            new ChangeShaderUpgradeInfo("Mochie/Standard Lite", "f927f4173320600459911ac97d99d0a2", "cc6f91d2b31e3e4408db75c7c6a5f034", new MochieMaterialUpgrade_V0_To_V1()),
        };

        [MenuItem("Tools/Mochie/Upgrade All Materials", priority = 1020)]
        public static void AutoUpgradeAllMaterials()
        {
            UpgradeMaterials(FindMaterialsNeedingUpgrade());
        }

        public static List<Material> FindMaterialsNeedingUpgrade()
        {
            var guids = AssetDatabase.FindAssets("t:Material");
            var result = new List<Material>();
            bool showProgress = guids.Length > 10;

            try
            {
                for(int i = 0; i < guids.Length; i++)
                {
                    if(showProgress)
                        EditorUtility.DisplayProgressBar("Mochie Shader Upgrader", $"Scanning materials ({i + 1}/{guids.Length})", (float)i / guids.Length);

                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(NeedsUpgrade(mat))
                        result.Add(mat);
                }
            }
            finally
            {
                if(showProgress)
                    EditorUtility.ClearProgressBar();
            }

            return result;
        }

        public static bool NeedsUpgrade(Material material)
        {
            if(material == null)
                return false;

            #if UNITY_2022_1_OR_NEWER
            if(material.parent != null)
                return false;
            #endif

            return ShaderUpgrades.Any(upgrade => upgrade.NeedsUpgrade(material));
        }

        [MenuItem("Tools/Mochie/Upgrade Selected Materials", priority = 1021)]
        [MenuItem("Assets/Mochie/Upgrade Selected Materials", priority = 30)]
        public static void UpgradeSelectedMaterials()
        {
            var selectedMaterials = Selection.GetFiltered<Material>(SelectionMode.DeepAssets | SelectionMode.Assets);
            if(selectedMaterials != null && selectedMaterials.Length > 0)
            {
                UpgradeMaterials(selectedMaterials);
            }
            else
            {
                EditorUtility.DisplayDialog("Mochie Shader Upgrader", "No materials selected. Please select one or more materials in the Project window.", "OK");
            }
        }

        [MenuItem("Tools/Mochie/Upgrade Selected Materials", true)]
        [MenuItem("Assets/Mochie/Upgrade Selected Materials", true)]
        static bool ValidateUpgradeSelectedMaterials()
        {
            return Selection.GetFiltered<Material>(SelectionMode.DeepAssets | SelectionMode.Assets).Length > 0;
        }

        public static void UpgradeMaterials(IEnumerable<Material> materials)
        {
            if(materials == null)
                return;

            var materialList = materials.Where(m => m != null).ToList();
            if(materialList.Count == 0)
                return;

            int total = materialList.Count;
            int upgradedCount = 0;

            try
            {
                for(int i = 0; i < total; i++)
                {
                    var material = materialList[i];
                    if(material == null)
                        continue;

                    if(total > 10)
                    {
                        if(EditorUtility.DisplayCancelableProgressBar("Upgrading Mochie Materials", $"Processing {material.name} ({i + 1}/{total})", (float)i / total))
                        {
                            Debug.Log("[MochieShaderUpgrader] Material upgrade cancelled by user.");
                            break;
                        }
                    }

                    if(UpgradeMaterial(material))
                        upgradedCount++;
                }
            }
            finally
            {
                if(total > 10)
                    EditorUtility.ClearProgressBar();

                if(upgradedCount > 0)
                {
                    AssetDatabase.SaveAssets();
                    Debug.Log($"[MochieShaderUpgrader] Successfully upgraded {upgradedCount} material(s).");
                }
            }
        }

        /// <summary>
        /// Upgrades a single material in memory and marks it dirty, without saving
        /// </summary>
        /// <param name="material">Material to upgrade</param>
        /// <param name="versionedOnly">Only run <see cref="VersionedShaderUpgradeInfo"/> upgrades</param>
        /// <returns>True if anything was upgraded</returns>
        public static bool UpgradeMaterial(Material material, bool versionedOnly = false)
        {
            if(material == null)
                return false;

            #if UNITY_2022_1_OR_NEWER
            if(material.parent != null)
            {
                #if MOCHIE_DEV
                Debug.Log($"Skipping upgrade of material <b>{material.name}</b> because it's a variant of <b>{material.parent.name}</b>");
                #endif
                return false;
            }
            #endif

            bool didUpgrade = false;
            foreach(var upgrade in ShaderUpgrades)
            {
                if(versionedOnly && !(upgrade is VersionedShaderUpgradeInfo))
                    continue;

                if(upgrade.RunUpgrade(material))
                    didUpgrade = true;
            }

            if(!didUpgrade)
                return false;

            EditorUtility.SetDirty(material);

            MaterialEditor editor = null;
            try
            {
                editor = Editor.CreateEditor(material) as MaterialEditor;
                if(editor && editor.customShaderGUI is IPostMaterialUpgradeCallback callback)
                {
                    #if MOCHIE_DEV
                    Debug.Log($"Running IPostMaterialUpgradeCallback on material '{material.name}' with shader '{material.shader.name}'");
                    #endif
                    callback.OnAfterMaterialUpgraded(material);
                }
            }
#if MOCHIE_DEV
            catch(Exception ex)
            {
                Debug.LogWarning($"Callback failed for '{material.name}': {ex.Message}");
            }
#else
            catch(Exception) {}
#endif
            finally
            {
                if(editor != null)
                    UnityEngine.Object.DestroyImmediate(editor);
            }

            return true;
        }
    }
}
