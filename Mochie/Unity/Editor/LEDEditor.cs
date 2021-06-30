using System.Reflection;
using UnityEditor;
using UnityEngine;
using Mochie;

public class LEDEditor : ShaderGUI {

	public static GUIContent mainTex = new GUIContent("Main Texture", "The main texture, used to drive the emission.");
	public static GUIContent RGBMatrixTex = new GUIContent("RGB Matrix Texture", "The RGB pixel layout pattern.");
	public static GUIContent smoothTex = new GUIContent("Roughness");

	MaterialProperty _MainTex = null;
	MaterialProperty _RGBSubPixelTex = null;
    MaterialProperty _EmissionIntensity = null; 
    MaterialProperty _Glossiness = null; 
    MaterialProperty _LightmapEmissionScale = null; 
    MaterialProperty _ApplyGamma = null;
    MaterialProperty _Backlight = null;
	MaterialProperty _SpecGlossMap = null;
	MaterialProperty _UVScroll = null;

    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public override void OnGUI(MaterialEditor m_MaterialEditor, MaterialProperty[] props){
		Material material = (Material)m_MaterialEditor.target;

        foreach (var property in GetType().GetFields(bindingFlags)){
            if (property.FieldType == typeof(MaterialProperty)){
                property.SetValue(this, FindProperty(property.Name, props));
            }
        }

        EditorGUI.BeginChangeCheck(); {
			MGUI.BoldLabel("Image");
			MGUI.Space2();
        	m_MaterialEditor.TexturePropertySingleLine(mainTex, _MainTex);
			if (_MainTex.textureValue){
				MGUI.TextureSOScroll(m_MaterialEditor, _MainTex, _UVScroll);
				MGUI.Space6();
			}
			m_MaterialEditor.TexturePropertySingleLine(smoothTex, _SpecGlossMap, _Glossiness);
			MGUI.TextureSO(m_MaterialEditor, _SpecGlossMap, _SpecGlossMap.textureValue);
			MGUI.SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
			MGUI.Space4();

			MGUI.SetKeyword(material, "_EMISSION", true);
            m_MaterialEditor.ShaderProperty(_EmissionIntensity, "Emission Strength");
			m_MaterialEditor.ShaderProperty(_LightmapEmissionScale, "Lightmap Emission Strength");
			material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
			MGUI.Space4();

			MGUI.BoldLabel("Panel");
			MGUI.Space2();
            m_MaterialEditor.TexturePropertySingleLine(RGBMatrixTex, _RGBSubPixelTex);
			MGUI.TextureSO(m_MaterialEditor, _RGBSubPixelTex, _RGBSubPixelTex.textureValue);
			MGUI.Space2();

			m_MaterialEditor.ShaderProperty(_Backlight, "Backlit");
			m_MaterialEditor.ShaderProperty(_ApplyGamma, "Gamma Correction");
			MGUI.Space8();
        }
    }
	
	public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
		if (mat.HasProperty("_Glossiness"))
			mat.SetFloat("_Glossiness", 0.035f);
		base.AssignNewShaderToMaterial(mat, oldShader, newShader);
	}
}
