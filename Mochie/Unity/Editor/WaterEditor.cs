using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Mochie;

public class WaterEditor : ShaderGUI {

    GUIContent texLabel = new GUIContent("Base Color");
    GUIContent normalLabel = new GUIContent("Normal Map");
	GUIContent flowLabel = new GUIContent("Flow Map");
	GUIContent noiseLabel = new GUIContent("Noise Texture");
	GUIContent foamLabel = new GUIContent("Foam Texture");

	Dictionary<Action, GUIContent> surfaceTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> norm0TabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> norm1TabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> flowTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> vertTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> causticsTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> fogTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> foamTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> edgeFadeTabButtons = new Dictionary<Action, GUIContent>();

    static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
    Toggles toggles = new Toggles(new string[] {
			"SURFACE", 
			"PRIMARY NORMAL", 
			"SECONDARY NORMAL", 
			"FLOW MAPPING", 
			"VERTEX OFFSET",
			"CAUSTICS",
			"DEPTH FOG",
			"FOAM",
			"EDGE FADE"
	}, 0);

    string header = "WaterHeader_Pro";
	string versionLabel = "v1.2";

	MaterialProperty _Color = null;
	MaterialProperty _MainTex = null;
	MaterialProperty _MainTexScroll = null;
	MaterialProperty _DistortionStrength = null;
	MaterialProperty _Roughness = null;
	MaterialProperty _Metallic = null;
	MaterialProperty _Opacity = null;
	MaterialProperty _Reflections = null;
	MaterialProperty _ReflStrength = null;
	MaterialProperty _Specular = null;
	MaterialProperty _SpecStrength = null;
	MaterialProperty _CullMode = null;
	MaterialProperty _ZWrite = null;
	MaterialProperty _BaseColorDistortionStrength = null;

	MaterialProperty _NormalMap0 = null;
	MaterialProperty _NormalStr0 = null;
	MaterialProperty _NormalMapScale0 = null;
	MaterialProperty _Rotation0 = null;
	MaterialProperty _NormalMapScroll0 = null;

	MaterialProperty _Normal1Toggle = null;
	MaterialProperty _NormalMap1 = null;
	MaterialProperty _NormalStr1 = null;
	MaterialProperty _NormalMapScale1 = null;
	MaterialProperty _Rotation1 = null;
	MaterialProperty _NormalMapScroll1 = null;

	MaterialProperty _FlowToggle = null;
	MaterialProperty _FlowMap = null;
	MaterialProperty _FlowSpeed = null;
	MaterialProperty _FlowStrength = null;
	MaterialProperty _FlowMapScale = null;

	MaterialProperty _NoiseTex = null;
	MaterialProperty _NoiseTexScale = null;
	MaterialProperty _NoiseTexScroll = null;
	MaterialProperty _NoiseTexBlur = null;
	MaterialProperty _WaveHeight = null;
	MaterialProperty _Offset = null;
	MaterialProperty _VertOffsetMode = null;
	MaterialProperty _WaveSpeed0 = null;
	MaterialProperty _WaveSpeed1 = null;
	MaterialProperty _WaveSpeed2 = null;
	MaterialProperty _WaveScale0 = null;
	MaterialProperty _WaveScale1 = null;
	MaterialProperty _WaveScale2 = null;
	MaterialProperty _WaveStrength0 = null;
	MaterialProperty _WaveStrength1 = null;
	MaterialProperty _WaveStrength2 = null;
	MaterialProperty _WaveDirection0 = null;
	MaterialProperty _WaveDirection1 = null;
	MaterialProperty _WaveDirection2 = null;
	MaterialProperty _Turbulence = null;
	MaterialProperty _TurbulenceSpeed = null;
	MaterialProperty _TurbulenceScale = null;

	MaterialProperty _CausticsToggle = null;
	MaterialProperty _CausticsOpacity = null;
	MaterialProperty _CausticsPower = null;
	MaterialProperty _CausticsScale = null;
	MaterialProperty _CausticsSpeed = null;
	MaterialProperty _CausticsFade = null;

	MaterialProperty _FogToggle = null;
	MaterialProperty _FogTint = null;
	MaterialProperty _FogPower = null;

	MaterialProperty _FoamToggle = null;
	MaterialProperty _FoamTex = null;
	MaterialProperty _FoamNoiseTex = null;
	MaterialProperty _FoamTexScale = null;
	MaterialProperty _FoamRoughness = null;
	MaterialProperty _FoamColor = null;
	MaterialProperty _FoamPower = null;
	MaterialProperty _FoamOpacity = null;
	MaterialProperty _FoamCrestStrength = null;
	MaterialProperty _FoamCrestThreshold = null;
	MaterialProperty _FoamNoiseTexScroll = null;
	MaterialProperty _FoamNoiseTexCrestStrength = null;
	MaterialProperty _FoamNoiseTexStrength = null;
	MaterialProperty _FoamNoiseTexScale = null;

	MaterialProperty _EdgeFadeToggle = null;
	MaterialProperty _EdgeFadePower = null;
	MaterialProperty _EdgeFadeOffset = null;
	MaterialProperty _SSRStrength = null;
	MaterialProperty _SSR = null;
	MaterialProperty _EdgeFadeSSR = null;
	MaterialProperty _Normal0StochasticToggle = null;
	MaterialProperty _Normal1StochasticToggle = null;
	MaterialProperty _FoamStochasticToggle = null;
	MaterialProperty _FoamTexScroll = null;
	MaterialProperty _BaseColorStochasticToggle = null;
	MaterialProperty _NormalMapOffset1 = null;
	MaterialProperty _FoamOffset = null;
	MaterialProperty _NormalMapOffset0 = null;
	MaterialProperty _BaseColorOffset = null;
	MaterialProperty _FoamDistortionStrength = null;
	MaterialProperty _VertRemapMin = null;
	MaterialProperty _VertRemapMax = null;

    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	bool m_FirstTimeApply = true;

    public override void OnGUI(MaterialEditor me, MaterialProperty[] props) {
        if (!me.isVisible)
            return;

		ClearDictionaries();

        foreach (var property in GetType().GetFields(bindingFlags)){
            if (property.FieldType == typeof(MaterialProperty))
                property.SetValue(this, FindProperty(property.Name, props));
        }
        Material mat = (Material)me.target;
        if (m_FirstTimeApply){
			m_FirstTimeApply = false;
        }

		header = "WaterHeader_Pro";
		if (!EditorGUIUtility.isProSkin){
			header = "WaterHeader";
		}

        Texture2D headerTex = (Texture2D)Resources.Load(header, typeof(Texture2D));
		Texture2D collapseIcon = (Texture2D)Resources.Load("CollapseIcon", typeof(Texture2D));

        GUILayout.Label(headerTex);
		MGUI.Space4();

		if (!foldouts.ContainsKey(mat))
			foldouts.Add(mat, toggles);

        EditorGUI.BeginChangeCheck(); {

            // Surface
			surfaceTabButtons.Add(()=>{Toggles.CollapseFoldouts(mat, foldouts, 1);}, MGUI.collapseLabel);
			surfaceTabButtons.Add(()=>{ResetSurface();}, MGUI.resetLabel);
			Action surfaceTabAction = ()=>{
				MGUI.PropertyGroup( () => {
					me.RenderQueueField();
					me.ShaderProperty(_CullMode, "Culling Mode");
					me.ShaderProperty(_ZWrite, "ZWrite");
					MGUI.Space2();
					MGUI.DisplayInfo("   This shader requires a \"Depth Light\" prefab be present in the scene.\n   (Found in: Assets/Mochie/Unity/Prefabs)");
				});
				MGUI.PropertyGroup( () => {
					me.TexturePropertySingleLine(texLabel, _MainTex, _Color, _BaseColorStochasticToggle);
					MGUI.TexPropLabel("Stochastic Sampling", 172);
					MGUI.TextureSOScroll(me, _MainTex, _MainTexScroll);
					me.ShaderProperty(_BaseColorOffset, "Parallax Offset");
					me.ShaderProperty(_BaseColorDistortionStrength, "Distortion Strength");
				});
				MGUI.PropertyGroup( () => {
					me.ShaderProperty(_Roughness, "Roughness");
					me.ShaderProperty(_Metallic, "Metallic");
					me.ShaderProperty(_Opacity, "Opacity");
					MGUI.ToggleFloat(me, "Specular", _Specular, _SpecStrength);
					MGUI.ToggleFloat(me, "Probe Reflectons", _Reflections, _ReflStrength);
					MGUI.ToggleFloat(me, "Screenspace Reflections", _SSR, _SSRStrength);
					if (_SSR.floatValue > 0)
						me.ShaderProperty(_EdgeFadeSSR, "Edge Fade");
					me.ShaderProperty(_DistortionStrength, "Distortion Strength");
				});
			};
			Foldouts.Foldout("SURFACE", foldouts, surfaceTabButtons, mat, me, surfaceTabAction);

			// Primary Normal
			norm0TabButtons.Add(()=>{ResetPrimaryNormal();}, MGUI.resetLabel);
			Action norm0TabAction = ()=>{
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					me.TexturePropertySingleLine(normalLabel, _NormalMap0, _Normal0StochasticToggle);
					MGUI.TexPropLabel("Stochastic Sampling", 172);
					me.ShaderProperty(_NormalStr0, "Strength");
					MGUI.Vector2Field(_NormalMapScale0, "Scale");
					MGUI.Vector2Field(_NormalMapScroll0, "Scrolling");
					me.ShaderProperty(_Rotation0, "Rotation");
					me.ShaderProperty(_NormalMapOffset0, "Parallax Offset");
				});
			};
			Foldouts.Foldout("PRIMARY NORMAL", foldouts, norm0TabButtons, mat, me, norm0TabAction);

			// Secondary Normal
			norm1TabButtons.Add(()=>{ResetSecondaryNormal();}, MGUI.resetLabel);
			Action norm1TabAction = ()=>{
				me.ShaderProperty(_Normal1Toggle, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					MGUI.ToggleGroup(_Normal1Toggle.floatValue == 0);
					me.TexturePropertySingleLine(normalLabel, _NormalMap1, _Normal1StochasticToggle);
					MGUI.TexPropLabel("Stochastic Sampling", 172);
					me.ShaderProperty(_NormalStr1, "Strength");
					MGUI.Vector2Field(_NormalMapScale1, "Scale");
					MGUI.Vector2Field(_NormalMapScroll1, "Scrolling");
					me.ShaderProperty(_Rotation1, "Rotation");
					me.ShaderProperty(_NormalMapOffset1, "Parallax Offset");
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.Foldout("SECONDARY NORMAL", foldouts, norm1TabButtons, mat, me, norm1TabAction);

			// Flow Mapping
			flowTabButtons.Add(()=>{ResetFlowMapping();}, MGUI.resetLabel);
			Action flowTabAction = ()=>{
				me.ShaderProperty(_FlowToggle, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					MGUI.ToggleGroup(_FlowToggle.floatValue == 0);
					me.TexturePropertySingleLine(flowLabel, _FlowMap);
					MGUI.Vector2Field(_FlowMapScale, "Scale");
					me.ShaderProperty(_FlowSpeed, "Speed");
					me.ShaderProperty(_FlowStrength, "Strength");
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.Foldout("FLOW MAPPING", foldouts, flowTabButtons, mat, me, flowTabAction);

			// Vertex Offset
			vertTabButtons.Add(()=>{ResetVertOffset();}, MGUI.resetLabel);
			Action vertTabAction = ()=>{
				me.ShaderProperty(_VertOffsetMode, "Mode");
				MGUI.Space4();
				MGUI.ToggleGroup(_VertOffsetMode.floatValue == 0);
				if (_VertOffsetMode.floatValue == 1){
					MGUI.PropertyGroup( () => {
						me.TexturePropertySingleLine(noiseLabel, _NoiseTex);
						me.ShaderProperty(_NoiseTexBlur, "Blur");
						MGUI.Vector2Field(_NoiseTexScale, "Scale");
						MGUI.Vector2Field(_NoiseTexScroll, "Scrolling");
					});
					MGUI.PropertyGroup( () => {
						MGUI.Vector3Field(_Offset, "Strength", false);
						me.ShaderProperty(_WaveHeight, "Strength Multiplier");
						MGUI.SliderMinMax(_VertRemapMin, _VertRemapMax, -1f, 1f, "Remap", 1);
					});
				}
				else if (_VertOffsetMode.floatValue == 2){
					MGUI.BoldLabel("Wave 1");
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_WaveStrength0, "Strength");
						me.ShaderProperty(_WaveScale0, "Scale");
						me.ShaderProperty(_WaveSpeed0, "Speed");
						me.ShaderProperty(_WaveDirection0, "Direction");
					});
					MGUI.BoldLabel("Wave 2");
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_WaveStrength1, "Strength");
						me.ShaderProperty(_WaveScale1, "Scale");
						me.ShaderProperty(_WaveSpeed1, "Speed");
						me.ShaderProperty(_WaveDirection1, "Direction");
					});
					MGUI.BoldLabel("Wave 3");
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_WaveStrength2, "Strength");
						me.ShaderProperty(_WaveScale2, "Scale");
						me.ShaderProperty(_WaveSpeed2, "Speed");
						me.ShaderProperty(_WaveDirection2, "Direction");
					});
					MGUI.BoldLabel("Turbulence");
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_Turbulence, "Strength");
						me.ShaderProperty(_TurbulenceSpeed, "Speed");
						me.ShaderProperty(_TurbulenceScale, "Scale");
					});
				}
				MGUI.ToggleGroupEnd();
			};
			Foldouts.Foldout("VERTEX OFFSET", foldouts, vertTabButtons, mat, me, vertTabAction);

			// Caustics
			causticsTabButtons.Add(()=>{ResetCaustics();}, MGUI.resetLabel);
			Action causticsTabAction = ()=>{
				me.ShaderProperty(_CausticsToggle, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					MGUI.ToggleGroup(_CausticsToggle.floatValue == 0);
					me.ShaderProperty(_CausticsOpacity, "Opacity");
					me.ShaderProperty(_CausticsPower, "Power");
					me.ShaderProperty(_CausticsScale, "Scale");
					me.ShaderProperty(_CausticsFade, "Fade");
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.Foldout("CAUSTICS", foldouts, causticsTabButtons, mat, me, causticsTabAction);
			
			// Foam
			foamTabButtons.Add(()=>{ResetFoam();}, MGUI.resetLabel);
			Action foamTabAction = ()=>{
				me.ShaderProperty(_FoamToggle, "Enable");
				MGUI.Space4();
				MGUI.ToggleGroup(_FoamToggle.floatValue == 0);
				MGUI.PropertyGroup( () => {
					me.TexturePropertySingleLine(foamLabel, _FoamTex, _FoamColor, _FoamStochasticToggle);
					MGUI.TexPropLabel("Stochastic Sampling", 172);
					MGUI.Space2();
					MGUI.Vector2Field(_FoamTexScale, "Scale");
					MGUI.Vector2Field(_FoamTexScroll, "Scrolling");
					me.ShaderProperty(_FoamOffset, "Parallax Offset");
					me.ShaderProperty(_FoamDistortionStrength, "Distortion Strength");
				});
				MGUI.PropertyGroup( () => {
					me.TexturePropertySingleLine(noiseLabel, _FoamNoiseTex);
					MGUI.Vector2Field(_FoamNoiseTexScale, "Scale");
					MGUI.Vector2Field(_FoamNoiseTexScroll, "Scrolling");
					me.ShaderProperty(_FoamNoiseTexStrength, "Edge Strength");
					me.ShaderProperty(_FoamNoiseTexCrestStrength, "Crest Strength");
				});
				MGUI.PropertyGroup( () => {
					me.ShaderProperty(_FoamRoughness, "Roughness");
					me.ShaderProperty(_FoamPower, "Power");
					me.ShaderProperty(_FoamOpacity, "Opacity");
					me.ShaderProperty(_FoamCrestStrength, "Crest Strength");
					me.ShaderProperty(_FoamCrestThreshold, "Crest Threshold");
				});
				MGUI.ToggleGroupEnd();
			};
			Foldouts.Foldout("FOAM", foldouts, foamTabButtons, mat, me, foamTabAction);

			// Depth Fog
			fogTabButtons.Add(()=>{ResetFog();}, MGUI.resetLabel);
			Action fogTabAction = ()=>{
				me.ShaderProperty(_FogToggle, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					MGUI.ToggleGroup(_FogToggle.floatValue == 0);
					me.ShaderProperty(_FogTint, "Color");
					me.ShaderProperty(_FogPower, "Power");
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.Foldout("DEPTH FOG", foldouts, fogTabButtons, mat, me, fogTabAction);

			// Edge Fade
			edgeFadeTabButtons.Add(()=>{ResetEdgeFade();}, MGUI.resetLabel);
			Action edgeFadeTabAction = ()=>{
				me.ShaderProperty(_EdgeFadeToggle, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					MGUI.ToggleGroup(_EdgeFadeToggle.floatValue == 0);
					me.ShaderProperty(_EdgeFadePower, "Power");
					me.ShaderProperty(_EdgeFadeOffset, "Offset");
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.Foldout("EDGE FADE", foldouts, edgeFadeTabButtons, mat, me, edgeFadeTabAction);
        }
		ApplyMaterialSettings(mat);

		GUILayout.Space(20);
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
		base.AssignNewShaderToMaterial(mat, oldShader, newShader);
		MGUI.ClearKeywords(mat);
	}

	void ApplyMaterialSettings(Material mat){
		bool reflToggle = mat.GetInt("_Reflections") == 1;
		bool specToggle = mat.GetInt("_Specular") == 1;
		bool ssrToggle = mat.GetInt("_SSR") == 1;
		int vertMode = mat.GetInt("_VertOffsetMode");
		MGUI.SetKeyword(mat, "_REFLECTIONS_ON", reflToggle);
		MGUI.SetKeyword(mat, "_SPECULAR_ON", specToggle);
		MGUI.SetKeyword(mat, "_SCREENSPACE_REFLECTIONS_ON", ssrToggle);
		MGUI.SetKeyword(mat, "_VERTEX_OFFSET_ON", vertMode == 1);
		MGUI.SetKeyword(mat, "_GERSTNER_WAVES_ON", vertMode == 2);
	}
	
	void ResetSurface(){
		_Color.colorValue = Color.white;
		_MainTex.textureValue = null;
		_MainTexScroll.vectorValue = new Vector4(0,0.1f,0,0);
		_DistortionStrength.floatValue = 0.1f;
		_Roughness.floatValue = 0f;
		_Metallic.floatValue = 0f;
		_Opacity.floatValue = 1f;
		_Reflections.floatValue = 1f;
		_ReflStrength.floatValue = 1f;
		_Specular.floatValue = 1f;
		_SpecStrength.floatValue = 1f;
		_CullMode.floatValue = 2f;
		_BaseColorStochasticToggle.floatValue = 0f;
		_BaseColorOffset.floatValue = 0f;
		_ZWrite.floatValue = 0f;
		_BaseColorDistortionStrength.floatValue = 0.1f;
		_SSR.floatValue = 0f;
		_SSRStrength.floatValue = 1f;
	}

	void ResetPrimaryNormal(){
		_NormalMapScale0.vectorValue = new Vector4(3f,3f,0,0);
		_NormalStr0.floatValue = 0.2f;
		_Rotation0.floatValue = 0f;
		_NormalMapScroll0.vectorValue = new Vector4(0.1f,0.1f,0,0);
		_Normal0StochasticToggle.floatValue = 0f;
		_NormalMapOffset0.floatValue = 0f;
	}

	void ResetSecondaryNormal(){
		_NormalStr1.floatValue = 0.3f;
		_NormalMapScale1.vectorValue = new Vector4(4f,4f,0,0);
		_NormalMapScroll1.vectorValue = new Vector4(-0.1f, 0.1f, 0,0);
		_Rotation1.floatValue = 0f;
		_Normal1StochasticToggle.floatValue = 0f;
		_NormalMapOffset1.floatValue = 0f;
	}

	void ResetFlowMapping(){
		_FlowSpeed.floatValue = 0.25f;
		_FlowStrength.floatValue = 0.1f;
		_FlowMapScale.vectorValue = new Vector4(2f,2f,0,0);
	}

	void ResetVertOffset(){
		_NoiseTexScale.vectorValue = new Vector4(1,1,0,0);
		_NoiseTexScroll.vectorValue = new Vector4(0.3f,0.06f,0,0);
		_NoiseTexBlur.floatValue = 0.8f;
		_WaveHeight.floatValue = 0.1f;
		_Offset.vectorValue = new Vector4(0,1,0,0);
		_WaveSpeed0.floatValue = 1f;
		_WaveSpeed1.floatValue = 1.1f;
		_WaveSpeed2.floatValue = 1.2f;
		_WaveStrength0.floatValue = 0.1f;
		_WaveStrength1.floatValue = 0.1f;
		_WaveStrength2.floatValue = 0.1f;
		_WaveScale0.floatValue = 4f;
		_WaveScale1.floatValue = 2f;
		_WaveScale2.floatValue = 1f;
		_WaveDirection0.floatValue = 0f;
		_WaveDirection1.floatValue = 0f;
		_WaveDirection2.floatValue = 0f;
		_TurbulenceSpeed.floatValue = 0.3f;
		_Turbulence.floatValue = 1f;
		_TurbulenceScale.floatValue = 3f;
		_VertRemapMin.floatValue = -1f;
		_VertRemapMax.floatValue = 1f;
	}

	void ResetCaustics(){
		_CausticsOpacity.floatValue = 1f;
		_CausticsPower.floatValue = 5f;
		_CausticsScale.floatValue = 5f;
		_CausticsSpeed.floatValue = 1f;
		_CausticsFade.floatValue = 10f;
	}

	void ResetFog(){
		_FogTint.colorValue = Color.white;
		_FogPower.floatValue = 1f;
	}

	void ResetFoam(){
		_FoamTexScale.vectorValue = new Vector4(3,3,0,0);
		_FoamRoughness.floatValue = 0.6f;
		_FoamColor.colorValue = Color.white;
		_FoamPower.floatValue = 200f;
		_FoamOpacity.floatValue = 3f;
		_FoamTexScroll.vectorValue = new Vector4(0.1f,-0.1f,0,0);
		_FoamStochasticToggle.floatValue = 0f;
		_FoamOffset.floatValue = 0f;
		_FoamCrestStrength.floatValue = 1f;
		_FoamCrestThreshold.floatValue = 0.5f;
		_FoamNoiseTexScroll.vectorValue = new Vector4(0f,0.1f,0f,0f);
		_FoamNoiseTexStrength.floatValue = 0f;
		_FoamNoiseTexCrestStrength.floatValue = 1.1f;
		_FoamNoiseTexScale.vectorValue = new Vector4(2f,2f,0,0);
		_FoamDistortionStrength.floatValue = 0.1f;
	}

	void ResetEdgeFade(){
		_EdgeFadePower.floatValue = 200f;
		_EdgeFadeOffset.floatValue = 0.5f;
	}

	void ClearDictionaries(){
		surfaceTabButtons.Clear();
		norm0TabButtons.Clear();
		norm1TabButtons.Clear();
		flowTabButtons.Clear();
		vertTabButtons.Clear();
		causticsTabButtons.Clear();
		fogTabButtons.Clear();
		foamTabButtons.Clear();
		edgeFadeTabButtons.Clear();
	}
}