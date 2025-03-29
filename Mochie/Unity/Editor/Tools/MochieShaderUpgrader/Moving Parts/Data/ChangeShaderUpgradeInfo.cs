using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public class ChangeShaderUpgradeInfo : ShaderUpgradeInfo
    {
        const string ErrorShaderName = "Hidden/InternalErrorShader";
        
        public string OldShaderGuid { get; private set; }
        public string NewShaderGuid { get; private set; }

        public ChangeShaderUpgradeInfo(string shaderName, string oldShaderGuid, string newShaderGuid, params MochieMaterialUpgradeBase[] upgrades) : base(shaderName, upgrades)
        {
            OldShaderGuid = oldShaderGuid;
            NewShaderGuid = newShaderGuid;
        }

        protected override bool IsValidUpgrade(Material material)
        {
            string shaderGuid;
            if(material.shader.name == ErrorShaderName)
            {
                shaderGuid = GetErrorShaderGuid(material);
            }
            else
            {
                string shaderPath = AssetDatabase.GetAssetPath(material.shader);
                shaderGuid = AssetDatabase.AssetPathToGUID(shaderPath);
            }

            return shaderGuid == OldShaderGuid;
        }
        
        public override void RunUpgrade(Material material)
        {
            if(!IsValidUpgrade(material))
                return;
            
            Shader newShader = AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(NewShaderGuid));
            if(newShader == null)
            {
                Debug.LogError($"Couldn't upgrade material: Shader <b>{ShaderName}</b> with Guid <b>{NewShaderGuid}</b> not found.");
                return;
            }
            
            material.shader = newShader;
            foreach(var upgrade in Upgrades)
                upgrade.RunUpgrade(material);
        }
        
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

        static string GetErrorShaderGuid(Material material)
        {
            string materialPath = AssetDatabase.GetAssetPath(material);
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

            return extractedGuid;
        }
    }
}