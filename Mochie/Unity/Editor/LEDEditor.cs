using System.Reflection;
using UnityEditor;
using UnityEngine;
using Mochie;

public class LEDEditor : ShaderGUI {

	public static GUIContent mainTex = new GUIContent("Main Texture", "The main texture, used to drive the emission.");
	public static GUIContent RGBMatrixTex = new GUIContent("RGB Matrix Texture", "The RGB pixel layout pattern.");
	public static GUIContent smoothTex = new GUIContent("Roughness");
	public static GUIContent flipbookTex = new GUIContent("Flipbook");
	string versionLabel = "v1.4.1";
	bool isTransparent = false;
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
	MaterialProperty _Color = null;
	MaterialProperty _ZWrite = null;

    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public override void OnGUI(MaterialEditor me, MaterialProperty[] props){
		Material material = (Material)me.target;
		isTransparent = material.shader.name.Contains("(Transparent)");
        foreach (var property in GetType().GetFields(bindingFlags)){
            if (property.FieldType == typeof(MaterialProperty)){
                property.SetValue(this, FindProperty(property.Name, props));
            }
        }

        EditorGUI.BeginChangeCheck(); {
			MGUI.BoldLabel("Image");
			MGUI.Space2();
			if (_FlipbookMode.floatValue == 0){
				me.TexturePropertySingleLine(mainTex, _MainTex, _FlipbookMode);
				MGUI.TexPropLabel("Flipbook", 105);
				me.ShaderProperty(_Color, "Color");
				if (_MainTex.textureValue){
					MGUI.TextureSOScroll(me, _MainTex, _UVScroll);
					MGUI.Space6();
				}
			}
			else {
				me.TexturePropertySingleLine(flipbookTex, _Flipbook, _FlipbookMode);
				MGUI.TexPropLabel("Flipbook", 105);
				if (_Flipbook.textureValue){
					MGUI.TextureSO(me, _MainTex);
					MGUI.SpaceN2();
					me.ShaderProperty(_FPS, "FPS");
					MGUI.Space6();
				}
			}
			MGUI.SetKeyword(material, "_FLIPBOOK_MODE", material.GetInt("_FlipbookMode") == 1);
			me.TexturePropertySingleLine(smoothTex, _SpecGlossMap, _Glossiness);
			MGUI.TextureSO(me, _SpecGlossMap, _SpecGlossMap.textureValue);
			MGUI.SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
			MGUI.Space4();

			MGUI.SetKeyword(material, "_EMISSION", true);
            me.ShaderProperty(_EmissionIntensity, "Emission Strength");
			me.ShaderProperty(_LightmapEmissionScale, "Lightmap Emission Strength");
			me.ShaderProperty(_BoostAmount, "Boost Multiplier");
			me.ShaderProperty(_BoostThreshold, "Boost Threshold");
			material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
			MGUI.Space4();

			MGUI.BoldLabel("Panel");
			MGUI.Space2();
            me.TexturePropertySingleLine(RGBMatrixTex, _RGBSubPixelTex);
			MGUI.TextureSO(me, _RGBSubPixelTex, _RGBSubPixelTex.textureValue);
			me.ShaderProperty(_Backlight, "Backlit");
			MGUI.Space4();
			MGUI.BoldLabel("Render Settings");
			if (isTransparent){
				me.ShaderProperty(_ZWrite, "ZWrite");
			}
			me.RenderQueueField();
			MGUI.Space8();
        }

		MGUI.DoFooter(versionLabel);
    }
	
	public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
		if (mat.HasProperty("_Glossiness"))
			mat.SetFloat("_Glossiness", 0.035f);
		base.AssignNewShaderToMaterial(mat, oldShader, newShader);
	}
}
