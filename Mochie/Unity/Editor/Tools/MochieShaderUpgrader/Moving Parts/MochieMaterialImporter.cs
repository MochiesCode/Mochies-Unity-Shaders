using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public class MochieMaterialImporter : AssetPostprocessor
    {
        [RunAfterClass(typeof(MochieShaderMaterialAutoUpgrade))]
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if(importedAssets == null || importedAssets.Length == 0)
                return;
            
            // If the auto upgrade script is part of the imported assets, try to upgrade all materials in the project
            if(importedAssets.Any(path => path.EndsWith($"{nameof(MochieShaderMaterialAutoUpgrade)}.cs")))
            {
                MochieShaderMaterialAutoUpgrade.AutoUpgradeAllMaterials();
                return;
            }
            
            // Otherwise try to upgrade only the imported materials
            var importedMaterials = importedAssets
                .Where(path => path.EndsWith(".mat", StringComparison.CurrentCultureIgnoreCase))
                .Select(AssetDatabase.LoadAssetAtPath<Material>)
                .ToArray();

            if(importedMaterials.Length > 0)
                MochieShaderMaterialAutoUpgrade.UpgradeMaterials(importedMaterials);
        }
    }
}