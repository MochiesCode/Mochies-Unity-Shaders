using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    /// <summary>
    /// Saving a material writes the current stamp default into it, which would mark an outdated material as up to date without running
    /// its upgrades. This runs any pending versioned upgrades right before the material is written, while the old stamp is still on disk.
    /// </summary>
    public class MochieMaterialSaveGuard : AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            List<string> upgraded = null;

            foreach(string path in paths)
            {
                if(!path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Already upgraded by UpgradeMaterials, which is saving it now
                if(VersionedShaderUpgradeInfo.ConsumeUpgradedPendingSave(path))
                    continue;

                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material == null)
                    continue;

                try
                {
                    if(MochieShaderMaterialAutoUpgrade.UpgradeMaterial(material, versionedOnly: true))
                    {
                        VersionedShaderUpgradeInfo.ConsumeUpgradedPendingSave(path);
                        if(upgraded == null)
                            upgraded = new List<string>();
                        upgraded.Add(material.name);
                    }
                }
                catch(Exception ex)
                {
                    // Never block a save
                    Debug.LogWarning($"[MochieShaderUpgrader] Couldn't upgrade '{path}' before saving: {ex.Message}");
                }
            }

            if(upgraded != null)
                Debug.Log($"[MochieShaderUpgrader] Upgraded {upgraded.Count} outdated material(s) before saving: {string.Join(", ", upgraded)}");

            return paths;
        }
    }
}
