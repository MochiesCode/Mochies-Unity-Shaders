using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public static class MochieShaderMaterialAutoUpgrade
    {
        static readonly ShaderUpgradeInfo[] ShaderUpgrades =
        {
            new ChangeShaderUpgradeInfo("Mochie/Standard", "cddaa3a02eb956746b502b80b76e92bc", "61dfaeeda7ef44742b40324ecd77f87c", new MochieMaterialUpgrade_V0_To_V1()),
            new ChangeShaderUpgradeInfo("Mochie/Standard Lite", "f927f4173320600459911ac97d99d0a2", "cc6f91d2b31e3e4408db75c7c6a5f034",new MochieMaterialUpgrade_V0_To_V1()),
            
            //new ShaderUpgradeInfo("Mochie/Standard", new MochieMaterialUpgrade_RunUpgradeCallback()),
            //new ShaderUpgradeInfo("Mochie/Standard Lite", new MochieMaterialUpgrade_RunUpgradeCallback()),
        };
        
        public static void AutoUpgradeAllMaterials()
        {
            var allMaterials = AssetDatabase.FindAssets("t:Material")
                .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
            
            UpgradeMaterials(allMaterials);
        }

        public static void UpgradeMaterials(IEnumerable<Material> materials)
        {
            foreach(var material in materials)
            {
                if(material == null)
                    continue;
                
                #if UNITY_2022_1_OR_NEWER
                if(material.parent != null)
                {
                    #if MOCHIE_DEV
                    Debug.Log($"Skipping upgrade of material <b>{material.name}</b> because it's a variant of <b>{material.parent.name}</b>");
                    #endif
                    continue;
                }
                #endif
                
                AssetDatabase.SaveAssetIfDirty(material);

                foreach(var upgrade in ShaderUpgrades)
                    upgrade.RunUpgrade(material);
                
                var editor = Editor.CreateEditor(material) as MaterialEditor;
                if(!editor || editor.customShaderGUI == null || !typeof(IPostMaterialUpgradeCallback).IsAssignableFrom(editor.customShaderGUI.GetType()))
                    continue;
                    
                #if MOCHIE_DEV
                Debug.Log($"Running IPostMaterialUpgradeCallback on material '{material.name}' with shader '{material.shader.name}'");
                #endif
                ((IPostMaterialUpgradeCallback)editor.customShaderGUI).OnAfterMaterialUpgraded(material);
            }
        }
    }
}
