using System;
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
            if(material == null || material.shader == null)
                return false;

            string shaderGuid = null;
            if(material.shader.name == ErrorShaderName)
            {
                shaderGuid = GetErrorShaderGuid(material);
            }
            else
            {
                string shaderPath = AssetDatabase.GetAssetPath(material.shader);
                shaderGuid = AssetDatabase.AssetPathToGUID(shaderPath);
            }

            // If it already has the new shader, do not upgrade again
            if(!string.IsNullOrEmpty(NewShaderGuid) && shaderGuid == NewShaderGuid)
                return false;

            // Direct match with the old shader GUID
            if(!string.IsNullOrEmpty(OldShaderGuid) && shaderGuid == OldShaderGuid)
                return true;

            return false;
        }
        
        public override bool RunUpgrade(Material material)
        {
            if(!IsValidUpgrade(material))
                return false;
            
            Shader newShader = null;
            if(!string.IsNullOrEmpty(NewShaderGuid))
            {
                string path = AssetDatabase.GUIDToAssetPath(NewShaderGuid);
                if(!string.IsNullOrEmpty(path))
                    newShader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            }

            if(newShader == null)
                newShader = Shader.Find(ShaderName);

            if(newShader == null)
            {
                Debug.LogError($"Couldn't upgrade material: Shader <b>{ShaderName}</b> with Guid <b>{NewShaderGuid}</b> not found.");
                return false;
            }

            Undo.RecordObject(material, "Upgrade Mochie Material");

            // Extract context before switching shaders so legacy properties are fully preserved
            var context = new MaterialContext(material);

            material.shader = newShader;
            foreach(var upgrade in Upgrades)
                upgrade.RunUpgrade(material, context);
            
            MaterialEditor editor = null;
            try
            {
                editor = Editor.CreateEditor(material) as MaterialEditor;
                editor?.SetShader(newShader, false); // Reset and apply keywords
            }
#if MOCHIE_DEV
            catch(Exception ex)
            {
                Debug.LogWarning($"SetShader failed in MaterialEditor for '{material.name}': {ex.Message}");
            }
#else
            catch(Exception) {}
#endif
            finally
            {
                if(editor != null)
                    UnityEngine.Object.DestroyImmediate(editor);
            }

            return true;
        }
        
        static Regex YamlGuidExtractor
        {
            get
            {
                if(_yamlGuidExtractor == null)
                    _yamlGuidExtractor = new Regex(@"m_Shader:\s*\{fileID:\s*\d+,\s*guid:\s*([a-zA-Z0-9]+)");
                return _yamlGuidExtractor;
            }
        }

        static Regex _yamlGuidExtractor;

        static string GetErrorShaderGuid(Material material)
        {
            string materialPath = AssetDatabase.GetAssetPath(material);
            if(string.IsNullOrEmpty(materialPath))
                return null;

            string fullMaterialPath = FileUtil.GetPhysicalPath(materialPath);
            if(string.IsNullOrEmpty(fullMaterialPath) || !File.Exists(fullMaterialPath))
                return null;

            // Open material as text and extract expected shader guid from it
            try
            {
                using(var reader = new StreamReader(fullMaterialPath))
                {
                    string line;
                    while((line = reader.ReadLine()) != null)
                    {
                        var match = YamlGuidExtractor.Match(line);
                        if(match.Success)
                            return match.Groups[1].Value;
                    }
                }
            }
#if MOCHIE_DEV
            catch(Exception ex)
            {
                Debug.LogWarning($"Couldn't read material file '{fullMaterialPath}': {ex.Message}");
            }
#else
            catch(Exception) {}
#endif

            return null;
        }
    }
}