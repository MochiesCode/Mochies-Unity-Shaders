using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public class MochieMaterialImporter : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if(importedAssets == null || importedAssets.Length == 0)
                return;
            
            // If the auto upgrade script is part of the imported assets, prompt to upgrade any outdated materials in the project
            if(importedAssets.Any(path => path.EndsWith($"{nameof(MochieShaderMaterialAutoUpgrade)}.cs", StringComparison.OrdinalIgnoreCase)))
            {
                var outdatedMaterials = MochieShaderMaterialAutoUpgrade.FindMaterialsNeedingUpgrade();
                if(outdatedMaterials.Count > 0 && EditorUtility.DisplayDialog("Mochie Shader Upgrader",
                    $"Found {outdatedMaterials.Count} material(s) in the project using an older version of Mochie's shaders.\n\nWould you like to upgrade them now?",
                    "Upgrade", "Skip"))
                {
                    MochieShaderMaterialAutoUpgrade.UpgradeMaterials(outdatedMaterials);
                }
                return;
            }
            
            // Otherwise try to upgrade only the imported materials
            var importedMaterials = importedAssets
                .Where(path => path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                .Select(AssetDatabase.LoadAssetAtPath<Material>)
                .Where(m => m != null)
                .ToArray();

            if(importedMaterials.Length > 0)
                MochieShaderMaterialAutoUpgrade.UpgradeMaterials(importedMaterials);
        }
    }
}