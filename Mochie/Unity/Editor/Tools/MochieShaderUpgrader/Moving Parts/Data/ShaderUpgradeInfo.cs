using System;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public class ShaderUpgradeInfo
    {
        public string ShaderName { get; private set; }
        public string OldShaderGuid { get; private set; }
        public string NewShaderGuid { get; private set; }
        
        private ShaderUpgradeInfo() {}

        public ShaderUpgradeInfo(string shaderName, string oldShaderGuid, string newShaderGuid)
        {
            ShaderName = shaderName;
            OldShaderGuid = oldShaderGuid;
            NewShaderGuid = newShaderGuid;
        }
    }
}