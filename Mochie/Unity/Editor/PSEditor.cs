using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Mochie;

public class PSEditor : ShaderGUI {

    GUIContent texLabel = new GUIContent("Base Color");
   	GUIContent tex2Label = new GUIContent("Secondary Color");
    GUIContent normalLabel = new GUIContent("Normal Map");
	GUIContent applyStreamsText = new GUIContent("Fix Vertex Streams", "Apply the vertex stream layout to all Particle Systems using this material");

    static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
    Toggles toggles = new Toggles(
		new string[] {
			"BASE", 
			"FILTERING", 
			"DISTORTION", 
			"PULSE", 
			"FALLOFF"
		}
	);

	float buttonSize = 24.0f;
    string header = "ParticleHeader_Pro";
	string watermark = "Watermark_Pro";
	string patIcon = "Patreon_Icon";
	string keyTex = "KeyIcon_Pro";
	string versionLabel = "v2.0";

    // Render Settings
    MaterialProperty _BlendMode = null;
    MaterialProperty _SrcBlend = null;
    MaterialProperty _DstBlend = null;
    MaterialProperty _Culling = null;
    MaterialProperty _ZTest = null;
    MaterialProperty _Falloff = null;
    MaterialProperty _IsCutout = null;
    MaterialProperty _Cutoff = null;
	MaterialProperty _FlipbookBlending = null;

    // Color
    MaterialProperty _MainTex = null;
    MaterialProperty _SecondTex = null;
    MaterialProperty _TexBlendMode = null;
    MaterialProperty _Color = null;
    MaterialProperty _SecondColor = null;
    MaterialProperty _Softening = null;
    MaterialProperty _SoftenStr = null;
	MaterialProperty _Opacity = null;

    // Filtering
    MaterialProperty _Filtering = null;
    MaterialProperty _AutoShift = null;
    MaterialProperty _AutoShiftSpeed = null;
    MaterialProperty _Hue = null;
    MaterialProperty _Saturation = null;
    MaterialProperty _Contrast = null;
    MaterialProperty _HDR = null;
	MaterialProperty _Brightness = null;

    // Distortion
    MaterialProperty _Distortion = null;
    MaterialProperty _NormalMap = null;
    MaterialProperty _DistortionStr = null;
    MaterialProperty _DistortionBlend = null;
    MaterialProperty _DistortionSpeed = null;
	MaterialProperty _DistortMainTex = null;
	MaterialProperty _NormalMapScale = null;

	// Pulse
	MaterialProperty _Pulse = null;
	MaterialProperty _Waveform = null;
	MaterialProperty _PulseStr = null;
	MaterialProperty _PulseSpeed = null;

	// Falloff
	MaterialProperty _FalloffMode = null;
    MaterialProperty _MinRange = null;
    MaterialProperty _MaxRange = null;
    MaterialProperty _NearMinRange = null;
    MaterialProperty _NearMaxRange = null;

    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	List<ParticleSystemRenderer> m_RenderersUsingThisMaterial = new List<ParticleSystemRenderer>();
    MaterialEditor m_MaterialEditor;
	bool m_FirstTimeApply = true;

	bool displayKeywords = false;
	List<string> keywordsList = new List<string>();

    public override void OnGUI(MaterialEditor me, MaterialProperty[] props) {
        if (!me.isVisible)
            return;
        foreach (var property in GetType().GetFields(bindingFlags)){
            if (property.FieldType == typeof(MaterialProperty))
                property.SetValue(this, FindProperty(property.Name, props));
        }
        if (_DstBlend.floatValue == 0) _DstBlend.floatValue = 10;
        Material mat = (Material)me.target;
        if (m_FirstTimeApply){
			CacheRenderersUsingThisMaterial(mat);
			m_FirstTimeApply = false;
        }

		header = "ParticleHeader_Pro";
		if (!EditorGUIUtility.isProSkin){
			header = "ParticleHeader";
			watermark = "Watermark";
			keyTex = "KeyIcon";
		}

        Texture2D headerTex = (Texture2D)Resources.Load(header, typeof(Texture2D));
		Texture2D watermarkTex = (Texture2D)Resources.Load(watermark, typeof(Texture2D));
		Texture2D patIconTex = (Texture2D)Resources.Load(patIcon, typeof(Texture2D));
		Texture2D resetIconTex = (Texture2D)Resources.Load("ResetIcon", typeof(Texture2D));
		Texture2D keyIcon = (Texture2D)Resources.Load(keyTex, typeof(Texture2D));
		GUIContent keyLabel = new GUIContent(keyIcon, "Toggle material keywords list.");

        MGUI.CenteredTexture(headerTex, 0, 0);
        GUILayout.Space(-34);
		ListKeywords(mat, keyLabel, buttonSize);

        EditorGUI.BeginChangeCheck(); {

			foreach (var obj in _BlendMode.targets)
				ApplyMaterialSettings((Material)obj);

            if (!foldouts.ContainsKey(mat))
                foldouts.Add(mat, toggles);

            // -----------------
            // Render Settings
            // -----------------
			bool baseTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "BASE");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetBase();
			MGUI.Space8();
            if (baseTab){

				// Vertex Stream Helper
				List<ParticleSystemVertexStream> streams = new List<ParticleSystemVertexStream>();
				streams.Add(ParticleSystemVertexStream.Position);
				streams.Add(ParticleSystemVertexStream.UV);
				streams.Add(ParticleSystemVertexStream.AnimBlend);
				streams.Add(ParticleSystemVertexStream.Speed);
				streams.Add(ParticleSystemVertexStream.Center);
				streams.Add(ParticleSystemVertexStream.Color);

				string warnings = "";
				List<ParticleSystemVertexStream> rendererStreams = new List<ParticleSystemVertexStream>();
				foreach (ParticleSystemRenderer renderer in m_RenderersUsingThisMaterial){
					if (renderer != null){
						renderer.GetActiveVertexStreams(rendererStreams);
						bool streamsValid = rendererStreams.SequenceEqual(streams);
						if (!streamsValid) warnings += "  " + renderer.name + "\n";
					}
				}
				if (warnings != ""){
					EditorGUILayout.HelpBox("Incorrect or missing vertex streams detected:\n" + warnings, MessageType.Warning, true);
					if (GUILayout.Button(applyStreamsText, EditorStyles.miniButton)){
						foreach (ParticleSystemRenderer renderer in m_RenderersUsingThisMaterial){
							if (renderer != null){
								if (renderer != null)
									renderer.SetActiveVertexStreams(streams);
							}
						}
					}
				}

                // Blending mode dropdown
                MGUI.Space4();
				MGUI.PropertyGroup( () => {
					me.RenderQueueField();
					me.ShaderProperty(_BlendMode, "Blending Mode");
					me.ShaderProperty(_Culling, "Culling");
					me.ShaderProperty(_ZTest, "ZTest");
				});
				MGUI.PropertyGroup( () => {
					me.ShaderProperty(_Opacity, "Opacity");
					MGUI.ToggleSlider(me, "Cutout", _IsCutout, _Cutoff);
					MGUI.ToggleSlider(me, "Softening", _Softening, _SoftenStr);
					me.ShaderProperty(_FlipbookBlending, "Flipbook Blending");
				});
				MGUI.PropertyGroup( () => {
					me.TexturePropertySingleLine(texLabel, _MainTex, _Color);
					me.TexturePropertySingleLine(tex2Label, _SecondTex, _SecondColor, _SecondTex.textureValue ? _TexBlendMode : null);
					if (_SecondTex.textureValue){
						MGUI.TexPropLabel("Blending", 113);
					}
				});
				MGUI.Space4();
			}
				
			// Filtering
			bool filteringTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "FILTERING");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetFiltering();
			MGUI.Space8();
			if (filteringTab){
				MGUI.Space4();
				me.ShaderProperty(_Filtering, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					MGUI.ToggleGroup(_Filtering.floatValue == 0);
					me.ShaderProperty(_AutoShift, "Auto Shift");
					if (_AutoShift.floatValue ==1)
						me.ShaderProperty(_AutoShiftSpeed, "Speed");
					else
						me.ShaderProperty(_Hue, "Hue");
					me.ShaderProperty(_Saturation, "Saturation");
					me.ShaderProperty(_Brightness, "Brightness");
					me.ShaderProperty(_Contrast, "Contrast");
					me.ShaderProperty(_HDR, "HDR");
					MGUI.ToggleGroupEnd();
				});
				MGUI.Space8();
			}

			// Distortion
			bool distortionTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "DISTORTION");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetDistortion();
			MGUI.Space8();
			if (distortionTab){
				MGUI.Space4();
				me.ShaderProperty(_Distortion, "Enable");
				MGUI.Space4();
				MGUI.ToggleGroup(_Distortion.floatValue == 0);
				MGUI.PropertyGroup( () => {
					me.TexturePropertySingleLine(normalLabel, _NormalMap, _DistortMainTex);
					MGUI.TexPropLabel("Distort UVs", 127);
					MGUI.Vector2Field(_NormalMapScale, "Scale");
					MGUI.Vector2Field(_DistortionSpeed, "Scrolling");
					me.ShaderProperty(_DistortionStr, "Strength");
					me.ShaderProperty(_DistortionBlend, "Blend");
					MGUI.ToggleGroupEnd();
				});
				MGUI.Space8();
			}

			// Pulse
			bool pulseTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "PULSE");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetPulse();
			MGUI.Space8();
			if (pulseTab){
				MGUI.Space4();
				me.ShaderProperty(_Pulse, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					MGUI.ToggleGroup(_Pulse.floatValue == 0);
					me.ShaderProperty(_Waveform, "Waveform");
					me.ShaderProperty(_PulseStr, "Strength");
					me.ShaderProperty(_PulseSpeed, "Speed");
					MGUI.ToggleGroupEnd();
				});
				MGUI.Space8();
			}

			// Falloff
			bool falloffTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "FALLOFF");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetFalloff();
			MGUI.Space8();
			if (falloffTab){
				MGUI.Space4();
				me.ShaderProperty(_Falloff, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					MGUI.ToggleGroup(_Falloff.floatValue == 0);
					me.ShaderProperty(_FalloffMode, "Mode");
					MGUI.Space4();
					me.ShaderProperty(_MinRange, "Far Min Range");
					me.ShaderProperty(_MaxRange, "Far Max Range");
					MGUI.Space4();
					me.ShaderProperty(_NearMinRange, "Near Min Range");
					me.ShaderProperty(_NearMaxRange, "Near Max Range");
					MGUI.ToggleGroupEnd();
				});
				MGUI.Space8();
			}
			GUILayout.Space(15);
            MGUI.CenteredTexture(watermarkTex, 0, 0);
			float buttonSize = 24.0f;
			float xPos = 53.0f;
			GUILayout.Space(-buttonSize);
			if (MGUI.LinkButton(patIconTex, buttonSize, buttonSize, xPos)){
				Application.OpenURL("https://www.patreon.com/mochieshaders");
			}
			GUILayout.Space(buttonSize);
			MGUI.VersionLabel(versionLabel, 12,-16,-20);
        }
    }

    // Set blending mode
    public static void SetBlendMode(Material material) {
        switch (material.GetInt("_BlendMode")) {
            case 0:
				material.EnableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.DisableKeyword("_COLORCOLOR_ON");
				material.DisableKeyword("_SPECGLOSSMAP");
				material.DisableKeyword("_METALLICGLOSSMAP");
				material.DisableKeyword("_PARALLAXMAP");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                break;
            case 1:
				material.DisableKeyword("_ALPHABLEND_ON");
				material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				material.DisableKeyword("_COLORCOLOR_ON");
				material.DisableKeyword("_SPECGLOSSMAP");
				material.DisableKeyword("_METALLICGLOSSMAP");
				material.DisableKeyword("_PARALLAXMAP");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                break;
            case 2:
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.EnableKeyword("_COLORCOLOR_ON");
				material.DisableKeyword("_SPECGLOSSMAP");
				material.DisableKeyword("_METALLICGLOSSMAP");
				material.DisableKeyword("_PARALLAXMAP");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                break;
            case 3:
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.DisableKeyword("_COLORCOLOR_ON");
				material.EnableKeyword("_SPECGLOSSMAP");
				material.DisableKeyword("_METALLICGLOSSMAP");
				material.DisableKeyword("_PARALLAXMAP");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
                break;
            case 4:
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.DisableKeyword("_COLORCOLOR_ON");
				material.DisableKeyword("_SPECGLOSSMAP");
				material.EnableKeyword("_METALLICGLOSSMAP");
				material.DisableKeyword("_PARALLAXMAP");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
                break;
            case 5:
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.DisableKeyword("_COLORCOLOR_ON");
				material.DisableKeyword("_SPECGLOSSMAP");
				material.DisableKeyword("_METALLICGLOSSMAP");
				material.EnableKeyword("_PARALLAXMAP");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
                break;
        }
    }

	void ApplyMaterialSettings(Material mat){
		bool softening = mat.GetInt("_Softening") == 1;
		bool distortion = mat.GetInt("_Distortion") == 1;
		bool distortUV = mat.GetInt("_DistortMainTex") == 1 && distortion;
		bool layering = mat.GetTexture("_SecondTex");
		bool pulse = mat.GetInt("_Pulse") == 1;
		bool falloff = mat.GetInt("_Falloff") == 1;
		bool flipbook = mat.GetInt("_FlipbookBlending") == 1;
		bool cutout = mat.GetInt("_IsCutout") == 1;
		bool filtering = mat.GetInt("_Filtering") == 1;

		MGUI.SetKeyword(mat, "_ALPHATEST_ON", cutout);
		MGUI.SetKeyword(mat, "_FADING_ON", softening);
		MGUI.SetKeyword(mat, "EFFECT_BUMP", distortion);
		MGUI.SetKeyword(mat, "_NORMALMAP", distortUV);
		MGUI.SetKeyword(mat, "_DETAIL_MULX2", layering);
		MGUI.SetKeyword(mat, "_ALPHAMODULATE_ON", pulse);
		MGUI.SetKeyword(mat, "DEPTH_OF_FIELD", falloff);
		MGUI.SetKeyword(mat, "_REQUIRE_UV2", flipbook);
		MGUI.SetKeyword(mat, "_COLOROVERLAY_ON", filtering);
		mat.SetShaderPassEnabled("Always", distortion);
		SetBlendMode(mat);
	}

	public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
		base.AssignNewShaderToMaterial(mat, oldShader, newShader);
		MGUI.ClearKeywords(mat);
	}

	void CacheRenderersUsingThisMaterial(Material material){
		m_RenderersUsingThisMaterial.Clear();

		ParticleSystemRenderer[] renderers = UnityEngine.Object.FindObjectsOfType(typeof(ParticleSystemRenderer)) as ParticleSystemRenderer[];
		foreach (ParticleSystemRenderer renderer in renderers)
		{
			if (renderer.sharedMaterial == material)
				m_RenderersUsingThisMaterial.Add(renderer);
		}
	}

	void ListKeywords(Material mat, GUIContent label, float buttonSize){
		keywordsList.Clear();
		foreach (string s in mat.shaderKeywords)
			keywordsList.Add(s);
		
		if (MGUI.LinkButton(label, buttonSize, buttonSize, (MGUI.GetInspectorWidth()/2.0f)-11.0f))
			displayKeywords = !displayKeywords;
		
		if (displayKeywords){
			MGUI.Space8();
			string infoString = "";
			if (keywordsList.Capacity == 0){
				infoString = "NO KEYWORDS FOUND";
			}
			else {
				infoString = "\nKeywords Used:\n\n";
				foreach (string s in keywordsList){
					infoString += " " + s + "\n";
				}
			}
			MGUI.DisplayText(infoString);
			MGUI.SpaceN8();
		}
		GUILayout.Space(buttonSize-10);
	}

	void ResetBase(){
		_BlendMode.floatValue = 1f;
		_Culling.floatValue = 2f;
		_FlipbookBlending.floatValue = 0f;
		_ZTest.floatValue = 4f;
		_SrcBlend.floatValue = 1f;
		_DstBlend.floatValue = 10f;
		_Color.colorValue = Color.white;
		_TexBlendMode.floatValue = 0f;
		_SecondTex.textureValue = null;
		_SecondColor.colorValue = Color.white;
		_Opacity.floatValue = 1f;
		_Cutoff.floatValue = 0f;
		_IsCutout.floatValue = 0f;
		_Softening.floatValue = 0f;
		_SoftenStr.floatValue = 0f;
	}

	void ResetFiltering(){
		_AutoShift.floatValue = 0f;
		_AutoShiftSpeed.floatValue = 0.25f;
		_Hue.floatValue = 0f;
		_Saturation.floatValue = 1f;
		_HDR.floatValue = 0f;
		_Contrast.floatValue = 1f;
		_Brightness.floatValue = 1f;
	}

	void ResetDistortion(){
		_DistortMainTex.floatValue = 0f;
		_DistortionStr.floatValue = 0f;
		_DistortionBlend.floatValue = 0.5f;
		_DistortionSpeed.vectorValue = new Vector4(0,0,0,0);
	}

	void ResetPulse(){
		_Waveform.floatValue = 0f;
		_PulseStr.floatValue = 0.5f;
		_PulseSpeed.floatValue = 1f;
	}

	void ResetFalloff(){
		_MinRange.floatValue = 8f;
		_MaxRange.floatValue = 15f;
		_NearMinRange.floatValue = 1f;
		_NearMaxRange.floatValue = 5f;
	}
}