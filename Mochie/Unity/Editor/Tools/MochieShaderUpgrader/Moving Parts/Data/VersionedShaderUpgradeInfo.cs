using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    /// <summary>
    /// In-place upgrades for a shader family that shares its properties (e.g. Standard, Standard Lite and Standard Mobile).
    /// The current version is the default of <see cref="MaterialVersionStamp.PropertyName"/> in the shaders (the package version, bumped every release),
    /// and a material's version is the stamp in its .mat file. Steps between the two run in order.
    /// v1.78 is the baseline, so steps start at "1.79". Materials without a stamp predate v1.78 and get every step.
    /// </summary>
    public class VersionedShaderUpgradeInfo : ShaderUpgradeInfo
    {
        public string[] ShaderNames { get; private set; }
        public VersionStep[] Steps { get; private set; }

        // Materials upgraded in memory that haven't been written yet. Their disk stamp is still old, so the save guard
        // would otherwise run the same steps on them a second time.
        static readonly HashSet<string> UpgradedPendingSave = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public VersionedShaderUpgradeInfo(string[] shaderNames, params VersionStep[] steps)
        {
            ShaderNames = shaderNames ?? Array.Empty<string>();
            Steps = (steps ?? Array.Empty<VersionStep>()).OrderBy(step => step.ToVersion).ToArray();
        }

        /// <summary>
        /// Removes <paramref name="assetPath"/> from the upgraded-but-unsaved list
        /// </summary>
        /// <returns>True if it was upgraded in memory and is waiting to be saved</returns>
        public static bool ConsumeUpgradedPendingSave(string assetPath) => UpgradedPendingSave.Remove(assetPath);

        bool TryGetPendingSteps(Material material, out VersionStep[] pendingSteps, out int currentVersion)
        {
            pendingSteps = null;
            currentVersion = 0;

            if(material == null || material.shader == null || Steps.Length == 0 || !ShaderNames.Contains(material.shader.name))
                return false;

            if(!MaterialVersionStamp.TryGetCurrentVersion(material.shader, out currentVersion))
                return false;

            // Skip the disk read when every step is past what this shader version knows about
            int current = currentVersion;
            if(!Steps.Any(step => step.ToVersion <= current))
                return false;

            if(UpgradedPendingSave.Contains(AssetDatabase.GetAssetPath(material)))
                return false;

            if(MaterialVersionStamp.ReadFromDisk(material, out int materialVersion) == MaterialVersionStamp.ReadResult.Unreadable)
                return false;

            pendingSteps = Steps.Where(step => step.ToVersion > materialVersion && step.ToVersion <= current).ToArray();
            return pendingSteps.Length > 0;
        }

        protected override bool IsValidUpgrade(Material material) => TryGetPendingSteps(material, out _, out _);

        public override bool RunUpgrade(Material material)
        {
            if(!TryGetPendingSteps(material, out var pendingSteps, out int currentVersion))
                return false;

            Undo.RecordObject(material, "Upgrade Mochie Material");

            // Unity bug workaround: on a freshly loaded material, the first Set of a property its file doesn't have yet (e.g. the target of a rename)
            // reads back fine but is reset to the shader default on save. Any read beforehand avoids it.
            material.GetFloat(MaterialVersionStamp.PropertyName);

            foreach(var step in pendingSteps)
            {
                // Fresh context per step so each one sees the values the previous step wrote
                step.Upgrade.RunUpgrade(material, new MaterialContext(material));
            }

            material.SetFloat(MaterialVersionStamp.PropertyName, currentVersion);

            string assetPath = AssetDatabase.GetAssetPath(material);
            if(!string.IsNullOrEmpty(assetPath))
                UpgradedPendingSave.Add(assetPath);

            return true;
        }
    }
}
