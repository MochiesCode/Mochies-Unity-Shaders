using System.Reflection;
using UnityEditor;
using UnityEngine;
using Mochie;

public class LEDEditor : ShaderGUI {

	public static GUIContent mainTex = new GUIContent("Main Texture", "The main texture, used to drive the emission.");
	public static GUIContent RGBMatrixTex = new GUIContent("RGB Matrix Texture", "The RGB pixel layout pattern.");
	public static GUIContent smoothTex = new GUIContent("Roughness");
	public static GUIContent flipbookTex = new GUIContent("Flipbook");
	string versionLabel = "v1.3";
	MaterialProperty _MainTex = null;
	MaterialProperty _RGBSubPixelTex = null;
    MaterialProperty _EmissionIntensity = null; 
    MaterialProperty _Glossiness = null; 
    MaterialProperty _LightmapEmissionScale = null; 
    MaterialProperty _Backlight = null;
	MaterialProperty _SpecGlossMap = null;
	MaterialProperty _UVScroll = null;
	MaterialProperty _BoostAmount = null;
	MaterialProperty _BoostThreshold = null;
	MaterialProperty _FlipbookMode = null;
	MaterialProperty _Flipbook = null;
	MaterialProperty _FPS = null;

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
			if (_FlipbookMode.floatValue == 0){
				m_MaterialEditor.TexturePropertySingleLine(mainTex, _MainTex, _FlipbookMode);
				MGUI.TexPropLabel("Flipbook", 105);
				if (_MainTex.textureValue){
					MGUI.TextureSOScroll(m_MaterialEditor, _MainTex, _UVScroll);
					MGUI.Space6();
				}
			}
			else {
				m_MaterialEditor.TexturePropertySingleLine(flipbookTex, _Flipbook, _FlipbookMode);
				MGUI.TexPropLabel("Flipbook", 105);
				if (_Flipbook.textureValue){
					MGUI.TextureSO(m_MaterialEditor, _MainTex);
					MGUI.SpaceN2();
					m_MaterialEditor.ShaderProperty(_FPS, "FPS");
					MGUI.Space6();
				}
			}
			MGUI.SetKeyword(material, "_FLIPBOOK_MODE", material.GetInt("_FlipbookMode") == 1);
			m_MaterialEditor.TexturePropertySingleLine(smoothTex, _SpecGlossMap, _Glossiness);
			MGUI.TextureSO(m_MaterialEditor, _SpecGlossMap, _SpecGlossMap.textureValue);
			MGUI.SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
			MGUI.Space4();

			MGUI.SetKeyword(material, "_EMISSION", true);
            m_MaterialEditor.ShaderProperty(_EmissionIntensity, "Emission Strength");
			m_MaterialEditor.ShaderProperty(_LightmapEmissionScale, "Lightmap Emission Strength");
			m_MaterialEditor.ShaderProperty(_BoostAmount, "Boost Multiplier");
			m_MaterialEditor.ShaderProperty(_BoostThreshold, "Boost Threshold");
			material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
			MGUI.Space4();

			MGUI.BoldLabel("Panel");
			MGUI.Space2();
            m_MaterialEditor.TexturePropertySingleLine(RGBMatrixTex, _RGBSubPixelTex);
			MGUI.TextureSO(m_MaterialEditor, _RGBSubPixelTex, _RGBSubPixelTex.textureValue);
			MGUI.Space2();

			m_MaterialEditor.ShaderProperty(_Backlight, "Backlit");
			MGUI.Space8();
        }

		MGUI.Space20();
		float buttonSize = 35f;
		Rect footerRect = EditorGUILayout.GetControlRect();
		footerRect.x += (MGUI.GetInspectorWidth()/2f)-buttonSize-5f;
		footerRect.width = buttonSize;
		footerRect.height = buttonSize;
		if (GUI.Button(footerRect, MGUI.patIconTex))
			Application.OpenURL("https://www.patreon.com/mochieshaders");
		footerRect.x += buttonSize + 5f;
		footerRect.y += 17f;
		GUIStyle formatting = new GUIStyle();
		formatting.fontSize = 15;
		formatting.fontStyle = FontStyle.Bold;
		if (EditorGUIUtility.isProSkin){
			formatting.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1);
			formatting.hover.textColor = new Color(0.8f, 0.8f, 0.8f, 1);
			GUI.Label(footerRect, versionLabel, formatting);
			footerRect.y += 20f;
			footerRect.x -= 35f;
			footerRect.width = 70f;
			footerRect.height = 70f;
			GUI.Label(footerRect, MGUI.mochieLogoPro);
			GUILayout.Space(90);
		}
		else {
			GUI.Label(footerRect, versionLabel, formatting);
			footerRect.y += 20f;
			footerRect.x -= 35f;
			footerRect.width = 70f;
			footerRect.height = 70f;
			GUI.Label(footerRect, MGUI.mochieLogo);
			GUILayout.Space(90);
		}
    }
	
	public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
		if (mat.HasProperty("_Glossiness"))
			mat.SetFloat("_Glossiness", 0.035f);
		base.AssignNewShaderToMaterial(mat, oldShader, newShader);
	}
}
