using System;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public class ShaderUpgradeInfo
    {
        public string ShaderName { get; private set; }
        public MochieMaterialUpgradeBase[] Upgrades { get; private set; }

        private ShaderUpgradeInfo() {}
        
        public ShaderUpgradeInfo(string shaderName, params MochieMaterialUpgradeBase[] upgrades)
        {
            ShaderName = shaderName;
            Upgrades = upgrades;
        }

        protected virtual bool IsValidUpgrade(Material material)
        {
            return material.shader.name == ShaderName;
        }

        public virtual void RunUpgrade(Material material)
        {
            if(!IsValidUpgrade(material))
                return;
            
            foreach(var upgrade in Upgrades)
                upgrade.RunUpgrade(material);
        }
    }
}