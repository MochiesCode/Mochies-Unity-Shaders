using System;
using UnityEditor;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public class ShaderUpgradeInfo
    {
        public string ShaderName { get; private set; }
        public MochieMaterialUpgradeBase[] Upgrades { get; private set; }

        protected ShaderUpgradeInfo() {}
        
        public ShaderUpgradeInfo(string shaderName, params MochieMaterialUpgradeBase[] upgrades)
        {
            ShaderName = shaderName;
            Upgrades = upgrades ?? Array.Empty<MochieMaterialUpgradeBase>();
        }

        protected virtual bool IsValidUpgrade(Material material)
        {
            if(material == null || material.shader == null)
                return false;
            return material.shader.name == ShaderName;
        }

        public bool NeedsUpgrade(Material material) => IsValidUpgrade(material);

        public virtual bool RunUpgrade(Material material)
        {
            if(!IsValidUpgrade(material))
                return false;

            Undo.RecordObject(material, "Upgrade Mochie Material");
            var context = new MaterialContext(material);
            bool anyUpgraded = false;

            foreach(var upgrade in Upgrades)
            {
                if(upgrade.RunUpgrade(material, context))
                    anyUpgraded = true;
            }

            return anyUpgraded;
        }
    }
}