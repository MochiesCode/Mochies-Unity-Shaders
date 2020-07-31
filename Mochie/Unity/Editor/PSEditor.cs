using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

public class PSEditor : ShaderGUI {

    public enum BlendingModes {ALPHA, PREMULTIPLIED, ADDITIVE, SOFT_ADDITIVE, MULTIPLY, MULTIPLY_2x}

    GUIContent texLabel = new GUIContent("Main Tex");
   	GUIContent tex2Label = new GUIContent("Secondary Tex");
    GUIContent normalLabel = new GUIContent("Normal Map");
	GUIContent applyStreamsText = new GUIContent("Fix Vertex Streams", "Apply the vertex stream layout to all Particle Systems using this material");

	public static List<string> presetsList = new List<string>();
	public static string[] presets;

	int popupIndex = -1;
	string presetText = "";
	string dirPath = "Assets/Mochie/Unity/Presets/Particle/";

    static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
    Toggles toggles = new Toggles(
		new string[] {
			"RENDERING", 
			"BASE", 
			"FILTERING", 
			"DISTORTION", 
			"PULSE", 
			"FALLOFF", 
			"PRESETS"
		}
	);
    string header = "ParticleHeader_Pro";
	string watermark = "Watermark_Pro";
	string patIcon = "Patreon_Icon";

    // Render Settings
    MaterialProperty _BlendMode = null;
    MaterialProperty _SrcBlend = null;
    MaterialProperty _DstBlend = null;
    MaterialProperty _Culling = null;
    MaterialProperty _ZTest = null;
    MaterialProperty _ZT = null;
    MaterialProperty _Falloff = null;
    MaterialProperty _IsCutout = null;
    MaterialProperty _Cutout = null;
	MaterialProperty _FlipbookBlending = null;

    // Color
    MaterialProperty _MainTex = null;
    MaterialProperty _SecondTex = null;
    MaterialProperty _Layering = null;
    MaterialProperty _TexBlendMode = null;
    MaterialProperty _Color = null;
    MaterialProperty _SecondColor = null;
    MaterialProperty _Softening = null;
    MaterialProperty _SoftenStr = null;
	MaterialProperty _Brightness = null;
	MaterialProperty _Opacity = null;

    // Filtering
    MaterialProperty _Filtering = null;
    MaterialProperty _AutoShift = null;
    MaterialProperty _AutoShiftSpeed = null;
    MaterialProperty _Hue = null;
    MaterialProperty _Saturation = null;
    MaterialProperty _Value = null;
    MaterialProperty _Contrast = null;
    MaterialProperty _HDR = null;

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
    MaterialProperty _MinRange = null;
    MaterialProperty _MaxRange = null;
    MaterialProperty _NearMinRange = null;
    MaterialProperty _NearMaxRange = null;

    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	List<ParticleSystemRenderer> m_RenderersUsingThisMaterial = new List<ParticleSystemRenderer>();
    MaterialEditor m_MaterialEditor;
	bool m_FirstTimeApply = true;

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

		// Generate preset popup items (and folders if necessary)
		if (!AssetDatabase.IsValidFolder(MGUI.parentPath))
			AssetDatabase.CreateFolder(MGUI.presetPath, "Presets");
		if (!AssetDatabase.IsValidFolder(MGUI.parentPath+"/Particle"))
			AssetDatabase.CreateFolder(MGUI.parentPath, "Particle");
		DirectoryInfo dir = new DirectoryInfo(dirPath);
		FileInfo[] info = dir.GetFiles();
		foreach (FileInfo f in info){
			if (!f.Name.Contains(".meta") && f.Name.Contains(".mat")){
				Material candidate = (Material)AssetDatabase.LoadAssetAtPath(dirPath + f.Name, typeof(Material));
				if (candidate.shader.name == mat.shader.name){
					int indOf = f.Name.IndexOf(".");
					presetsList.Add(f.Name.Substring(0, indOf));
				}
			}
		}
		presets = presetsList.ToArray();
		presetsList.Clear();

		bool isParticleX = MGUI.IsXVersion(mat);

		if (isParticleX){
			header = "ParticleHeaderX_Pro";
			if (!EditorGUIUtility.isProSkin){
				header = "ParticleHeaderX";
				watermark = "Watermark";
			}
		}
		else {
			header = "ParticleHeader_Pro";
			if (!EditorGUIUtility.isProSkin){
				header = "ParticleHeader";
				watermark = "Watermark";
			}
		}
        Texture2D headerTex = (Texture2D)Resources.Load(header, typeof(Texture2D));
		Texture2D watermarkTex = (Texture2D)Resources.Load(watermark, typeof(Texture2D));
		Texture2D patIconTex = (Texture2D)Resources.Load(patIcon, typeof(Texture2D));
		Texture2D resetIconTex = (Texture2D)Resources.Load("ResetIcon", typeof(Texture2D));
        MGUI.CenteredTexture(headerTex, 0, 0);
        
        EditorGUI.BeginChangeCheck(); {
            if (!foldouts.ContainsKey(mat))
                foldouts.Add(mat, toggles);
            bool canDistort = true;

            // -----------------
            // Render Settings
            // -----------------
			bool renderingTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "RENDERING");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetRendering();
			MGUI.Space8();
            if (renderingTab){

                // Blending mode dropdown
                MGUI.Space4();
				me.RenderQueueField();
                EditorGUI.showMixedValue = _BlendMode.hasMixedValue;
                var mode = (BlendingModes)_BlendMode.floatValue;
                EditorGUI.BeginChangeCheck();
                mode = (BlendingModes)EditorGUILayout.Popup("Blending Mode", (int)mode, Enum.GetNames(typeof(BlendingModes)));
                if (EditorGUI.EndChangeCheck()) {
                    me.RegisterPropertyChangeUndo("Blending Mode");
                    _BlendMode.floatValue = (float)mode;
                    foreach (var obj in _BlendMode.targets){
                        SetBlendMode((Material)obj, (BlendingModes)mode);
                    }
                    EditorGUI.showMixedValue = false;
                }
				me.ShaderProperty(_Culling, "Culling Mode");
				me.ShaderProperty(_FlipbookBlending, "Flipbook Blending");
				me.ShaderProperty(_ZTest, "ZTest Always");
				if (_ZTest.floatValue == 1) _ZT.floatValue = 6;
				else _ZT.floatValue = 2;
				
				// Vertex Stream Helper
				List<ParticleSystemVertexStream> streams = new List<ParticleSystemVertexStream>();
				streams.Add(ParticleSystemVertexStream.Position);
				streams.Add(ParticleSystemVertexStream.UV);
				streams.Add(ParticleSystemVertexStream.AnimBlend);
				streams.Add(ParticleSystemVertexStream.Speed);
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
				MGUI.Space8();
			}
            
            // -----------------
            // Base Settings
            // -----------------
			bool baseTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "BASE");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetBase();
			MGUI.Space8();
            if (baseTab){
                MGUI.Space4();
				if (isParticleX){
					me.TexturePropertySingleLine(texLabel, _MainTex, _Color, _Layering);
					MGUI.TexPropLabel("Layered", 110);
					if (_Layering.floatValue == 1){
						me.TexturePropertySingleLine(tex2Label, _SecondTex, _SecondColor, _TexBlendMode);
						MGUI.TexPropLabel("Blending", 113);
					}
				}
				else me.TexturePropertySingleLine(texLabel, _MainTex, _Color);
				 
                MGUI.Space4();
				me.ShaderProperty(_Brightness, "Brightness");
				me.ShaderProperty(_Opacity, "Opacity");
                MGUI.ToggleSlider(me, "Cutout", _IsCutout, _Cutout);
                MGUI.ToggleSlider(me, "Softening", _Softening, _SoftenStr);
				MGUI.SetKeyword(mat, "_FADING_ON", _Softening.floatValue == 1.0);
				MGUI.Space8();
			}

			if (isParticleX){
				
				// Filtering
				bool filteringTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "FILTERING");
				if (MGUI.TabButton(resetIconTex, 26f))
					ResetFiltering();
				MGUI.Space8();
				if (filteringTab){
					MGUI.Space4();
					me.ShaderProperty(_Filtering, "Enable");
					MGUI.Space4();
					MGUI.ToggleGroup(_Filtering.floatValue == 0);
					me.ShaderProperty(_AutoShift, "Auto Shift");
					if (_AutoShift.floatValue ==1)
						me.ShaderProperty(_AutoShiftSpeed, "Speed");
					else
						me.ShaderProperty(_Hue, "Hue");
					me.ShaderProperty(_Saturation, "Saturation");
					me.ShaderProperty(_Value, "Value");
					me.ShaderProperty(_HDR, "HDR");
					me.ShaderProperty(_Contrast, "Contrast");
					MGUI.ToggleGroupEnd();
					MGUI.Space8();
				}

				// Distortion
				if (canDistort){
					bool distortionTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "DISTORTION");
					if (MGUI.TabButton(resetIconTex, 26f))
						ResetDistortion();
					MGUI.Space8();
					if (distortionTab){
						MGUI.Space4();
						me.ShaderProperty(_Distortion, "Enable");
						MGUI.Space4();
						MGUI.ToggleGroup(_Distortion.floatValue == 0);
						me.TexturePropertySingleLine(normalLabel, _NormalMap, _DistortMainTex);
						MGUI.TexPropLabel("Distort Main Tex", 155);
						MGUI.Vector2Field(_NormalMapScale, "Scale");
						MGUI.Vector2Field(_DistortionSpeed, "Scrolling");
						me.ShaderProperty(_DistortionStr, "Strength");
						me.ShaderProperty(_DistortionBlend, "Blend");
						MGUI.ToggleGroupEnd();
						MGUI.Space8();
					}
					if (_Distortion.floatValue == 1) mat.EnableKeyword("EFFECT_BUMP");
				}
				else {
					mat.DisableKeyword("EFFECT_BUMP");
				}
				mat.SetShaderPassEnabled("Always", _Distortion.floatValue == 1);
			}
			else mat.DisableKeyword("EFFECT_BUMP");

			// Pulse
			bool pulseTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "PULSE");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetPulse();
			MGUI.Space8();
			if (pulseTab){
				MGUI.Space4();
				me.ShaderProperty(_Pulse, "Enable");
				MGUI.Space4();
				MGUI.ToggleGroup(_Pulse.floatValue == 0);
				me.ShaderProperty(_Waveform, "Waveform");
				me.ShaderProperty(_PulseStr, "Strength");
				me.ShaderProperty(_PulseSpeed, "Speed");
				MGUI.ToggleGroupEnd();
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
				MGUI.ToggleGroup(_Falloff.floatValue == 0);
				me.ShaderProperty(_MinRange, "Far Min Range");
				me.ShaderProperty(_MaxRange, "Far Max Range");
				MGUI.Space4();
				me.ShaderProperty(_NearMinRange, "Near Min Range");
				me.ShaderProperty(_NearMaxRange, "Near Max Range");
				MGUI.ToggleGroupEnd();
				MGUI.Space8();
			}

			// -----------------
			// Presets
			// -----------------
			if (Foldouts.DoFoldout(foldouts, mat, me, 0, "PRESETS")){
				MGUI.Space4();
				float buttonWidth = EditorGUIUtility.labelWidth-5.0f;
				if (MGUI.SimpleButton("Save", buttonWidth, 0)){
					presetText = MGUI.ReplaceInvalidChars(presetText);
					string filePath = dirPath + presetText + ".mat";
					Material newMat = new Material(mat);
					AssetDatabase.CreateAsset(newMat, filePath);
					AssetDatabase.Refresh();
					GUIUtility.keyboardControl = 0;
					GUIUtility.hotControl = 0;
					presetText = "";
					popupIndex = -1;
				}
				GUILayout.Space(-17);

				// Text area
				Rect r = EditorGUILayout.GetControlRect();
				r.x += EditorGUIUtility.labelWidth;
				r.width = MGUI.GetPropertyWidth();
				presetText = EditorGUI.TextArea(r, presetText);
				
				// Locate button
				if (MGUI.SimpleButton("Locate", buttonWidth, 0) && popupIndex != -1){
					string filePath = dirPath + presets[popupIndex]+".mat";
					EditorUtility.FocusProjectWindow();
					Selection.activeObject = AssetDatabase.LoadAssetAtPath(filePath, typeof(Material));
				}
				GUILayout.Space(-17);

				// Popup list
				r = EditorGUILayout.GetControlRect();
				r.x += EditorGUIUtility.labelWidth;
				r.width = MGUI.GetPropertyWidth();
				popupIndex = EditorGUI.Popup(r, popupIndex, presets);

				// Apply button
				GUILayout.Space(-GUILayoutUtility.GetLastRect().height);
				if (MGUI.SimpleButton("Apply", r.width, r.x-14f) && popupIndex != -1){
					string presetPath = dirPath + presets[popupIndex] + ".mat";
					Material selectedMat = (Material)AssetDatabase.LoadAssetAtPath(presetPath, typeof(Material));
					mat.CopyPropertiesFromMaterial(selectedMat);
					popupIndex = -1;
				}
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
        }
    }

    // Set blending mode
    public static void SetBlendMode(Material material, BlendingModes mode) {
        EditorGUI.BeginChangeCheck();
        switch (mode) {
            case BlendingModes.ALPHA:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                break;
            case BlendingModes.PREMULTIPLIED:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                break;
            case BlendingModes.ADDITIVE:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                break;
            case BlendingModes.SOFT_ADDITIVE:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
                break;
            case BlendingModes.MULTIPLY:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
                break;
            case BlendingModes.MULTIPLY_2x:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
                break;
        }
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

	void FullReset(){
		ResetBase();
		ResetFiltering();
		ResetDistortion();
		ResetPulse();
		ResetFalloff();
	}

	void ResetRendering(){
		_BlendMode.floatValue = 1f;
		_Culling.floatValue = 2f;
		_FlipbookBlending.floatValue = 0f;
		_ZTest.floatValue = 0f;
		_ZT.floatValue = 2f;
		_SrcBlend.floatValue = 1f;
		_DstBlend.floatValue = 10f;
	}

	void ResetBase(){
		_Color.colorValue = Color.white;
		_Layering.floatValue = 0f;
		_TexBlendMode.floatValue = 0f;
		_SecondTex.textureValue = null;
		_SecondColor.colorValue = Color.white;
		_Brightness.floatValue = 1f;
		_Opacity.floatValue = 1f;
		_Cutout.floatValue = 0f;
		_IsCutout.floatValue = 0f;
		_Softening.floatValue = 0f;
		_SoftenStr.floatValue = 0f;
	}

	void ResetFiltering(){
		_AutoShift.floatValue = 0f;
		_AutoShiftSpeed.floatValue = 0.25f;
		_Hue.floatValue = 0f;
		_Saturation.floatValue = 1f;
		_Value.floatValue = 0f;
		_HDR.floatValue = 0f;
		_Contrast.floatValue = 0f;
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