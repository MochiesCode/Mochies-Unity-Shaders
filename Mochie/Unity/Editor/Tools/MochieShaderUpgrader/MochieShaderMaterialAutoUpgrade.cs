using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public static class MochieShaderMaterialAutoUpgrade
    {
        const string ErrorShaderName = "Hidden/InternalErrorShader";
        
        public static readonly ShaderUpgradeInfo[] ShaderUpgradeInfos =
        {
            new ShaderUpgradeInfo("Mochie/Standard", "cddaa3a02eb956746b502b80b76e92bc", "61dfaeeda7ef44742b40324ecd77f87c"),
            new ShaderUpgradeInfo("Mochie/Standard Lite", "f927f4173320600459911ac97d99d0a2", "cc6f91d2b31e3e4408db75c7c6a5f034"),
        };

        public static readonly MochieMaterialUpgradeBase[] Upgrades =
        {
            new MochieMaterialUpgrade_V0_To_V1()
        };

        public static string[] ShaderNames
        {
            get
            {
                if(_shaderNames == null)
                    _shaderNames = ShaderUpgradeInfos.Select(info => info.ShaderName).ToArray();
                return _shaderNames;
            }
        }
        static string[] _shaderNames;
        
        static Regex YamlGuidExtractor
        {
            get
            {
                if(_yamlGuidExtractor == null)
                    _yamlGuidExtractor = new Regex(@"m_Shader: {fileID: \d+, guid: ([\w\d]+), type: \d+}");
                return _yamlGuidExtractor;
            }
        }

        static Regex _yamlGuidExtractor;
        
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
                if(material.parent != null)
                {
                    #if MOCHIE_DEV
                    Debug.Log($"Skipping upgrade of material <b>{material.name}</b> because it's a variant of <b>{material.parent.name}</b>");
                    #endif
                    continue;
                }
                
                AssetDatabase.SaveAssetIfDirty(material);
                
                if(material.shader == null || material.shader.name == ErrorShaderName)
                    ProcessErrorMaterialAndUpgrade(material);
                else if(ShaderNames.Contains(material.shader.name))
                    ProcessValidMaterialAndUpgrade(material);
            }
        }
        
        /// <summary>
        /// Process a material with a valid shader, try to switch it to a matching shader and upgrade it 
        /// </summary>
        /// <param name="material">Material with a valid non error shader</param>
        static void ProcessValidMaterialAndUpgrade(Material material)
        {
            var shader = material.shader;
            if(!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(shader, out string shaderGuid, out long _))
            {
                Debug.LogError($"Couldn't process Material {material.name}'s Shader {material.shader.name} with GUID {shaderGuid}");
                return;
            }

            // Check if this is a mochie material using an old shader that's still in the project
            ShaderUpgradeInfo upgradeInfo = ShaderUpgradeInfos.FirstOrDefault(x => x.OldShaderGuid == shaderGuid); 
            if(upgradeInfo == null)
                return;

            Shader newShader = Shader.Find(upgradeInfo.ShaderName);
            if(newShader == null)
                return;
            
            // Set shader here so we can use material.SetProperty() api (ex: material.SetFloat())
            material.shader = newShader;
            
            foreach(var upgrade in Upgrades)
                upgrade.RunUpgrade(material);
            
            var editor = Editor.CreateEditor(material) as MaterialEditor;
            editor.SetShader(newShader); // Set shader here again so keywords get reset and applied
        }

        /// <summary>
        /// Processes a missing shader material and tries to upgrade it if conditions match
        /// </summary>
        /// <param name="material">Material with missing shader</param>
        /// <param name="materialPath">Optional Material path so we don't have to ask the asset database again if we already have it</param>
        static void ProcessErrorMaterialAndUpgrade(Material material, string materialPath = null)
        {
            if(materialPath == null)
                materialPath = AssetDatabase.GetAssetPath(material);
            
            string fullMaterialPath = FileUtil.GetPhysicalPath(materialPath);
            
            // Open material as text and extract expected shader guid from it
            string extractedGuid = null;
            using(var reader = new StreamReader(fullMaterialPath))
            {
                string line;
                while((line = reader.ReadLine()) != null)
                {
                    var match = YamlGuidExtractor.Match(line);
                    if(match.Success)
                    {
                        extractedGuid = match.Groups[1].Value;
                        break;
                    }
                }
            }

            if(string.IsNullOrWhiteSpace(extractedGuid))
            {
                Debug.LogError($"Couldn't extract guid from material at {materialPath}");
                return;
            }

            // Get new shader based on the old shader's guid
            var newInfo = ShaderUpgradeInfos.FirstOrDefault(info => info.OldShaderGuid == extractedGuid);
            if(newInfo == null)
                return;
            
            Shader newShader = AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(newInfo.NewShaderGuid));
            if(newShader == null)
                return;
            
            Shader oldShader = material.shader;
            material.shader = newShader;
            
            foreach(var upgrade in Upgrades)
                upgrade.RunUpgrade(material);
            
            var editor = Editor.CreateEditor(material) as MaterialEditor;
            editor.customShaderGUI.AssignNewShaderToMaterial(material, oldShader, newShader);
        }
    }
}
