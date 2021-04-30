using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Mochie;

public class SFXEditor : ShaderGUI {

    public enum BlendingModes {Opaque, Alpha, Premultiplied, Additive, Soft_Additive, Multiply, Multiply_2x}

    GUIContent screenTexLabel = new GUIContent("Texture");
    GUIContent shakeNoiseTexLabel = new GUIContent("Noise Texture");
    GUIContent normalMapLabel = new GUIContent("Normal Map");
    GUIContent tpTexLabel = new GUIContent("Texture");
    GUIContent tpNoiseTexLabel = new GUIContent("Noise Texture");
    
	public static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();

	bool displayKeywords = false;
	List<string> keywordsList = new List<string>();
	float buttonSize = 24.0f;

    Toggles toggles = new Toggles(
		new string[] {
			"GENERAL",
			"FILTERING",
			"SHAKE",
			"DISTORTION",
			"BLUR",
			"ZOOM",
			"IMAGE OVERLAY",
			"FOG",
			"TRIPLANAR",
			"OUTLINE",
			"MISC",
			"Letterbox",
			"Deep Fry",
			"Pulse",
			"UV Manipulation",
			"Rounding",
			"Normal Map",
			"Depth Buffer",
			"Safe Zone",
			"NOISE"
		}
	);

    // Texture file names
	string header = "SFXHeader_Pro";
	string watermark = "Watermark_Pro";
	string patIcon = "Patreon_Icon";
	string versionLabel = "v1.12";
	string keyTex = "KeyIcon_Pro";
	
    // Commonly used strings
    string modeLabel = "Mode";
    string strengthLabel = "Strength";
    string fadeLabel = "Fade";
    string radiusLabel = "Radius";
    string minLabel = "Min Range";
    string maxLabel = "Max Range";
    string p2oLabel = "Player to Object";
    string ugfLabel = "Use Global Falloff";
    string colorLabel = "Color";
    string speedXLabel = "Speed X";
    string speedYLabel = "Speed Y";
    string speedLabel = "Speed";

    // General
    MaterialProperty _BlendMode = null;
    MaterialProperty _SrcBlend = null;
    MaterialProperty _DstBlend = null;
    MaterialProperty _MinRange = null;
    MaterialProperty _MaxRange = null;
    MaterialProperty _Opacity = null;

    // Color Filtering
    MaterialProperty _FilterModel = null;
	MaterialProperty _FilterStrength = null;
    MaterialProperty _ColorUseGlobal = null;
    MaterialProperty _ColorMinRange = null;
    MaterialProperty _ColorMaxRange = null;
    MaterialProperty _Color = null;
	MaterialProperty _RGB = null;
    MaterialProperty _Contrast = null;
    MaterialProperty _HDR = null;
    MaterialProperty _Invert = null;
    MaterialProperty _InvertR = null;
    MaterialProperty _InvertG = null;
    MaterialProperty _InvertB = null;
    MaterialProperty _AutoShift = null;
    MaterialProperty _AutoShiftSpeed = null;
    MaterialProperty _Hue = null;
    MaterialProperty _Saturation = null;
    MaterialProperty _Brightness = null;
	MaterialProperty _Value = null;
	MaterialProperty _SaturationR = null;
	MaterialProperty _SaturationB = null;
	MaterialProperty _SaturationG = null;

    //MaterialProperty _Sobel = null;
    //MaterialProperty _SobelStr = null;

    // Shake
    MaterialProperty _ShakeModel = null;
    MaterialProperty _ShakeUseGlobal = null;
    MaterialProperty _ShakeMinRange = null;
    MaterialProperty _ShakeMaxRange = null;
    MaterialProperty _ShakeNoiseTex = null;
    MaterialProperty _Amplitude = null;
    MaterialProperty _ShakeSpeedX = null;
    MaterialProperty _ShakeSpeedY = null;
    MaterialProperty _ShakeSpeedXY = null;

    // Distortion
    MaterialProperty _DistortionModel = null;
    MaterialProperty _DistortionUseGlobal = null;
    MaterialProperty _DistortionMinRange = null;
    MaterialProperty _DistortionMaxRange = null;
    MaterialProperty _NormalMap = null;
    MaterialProperty _DistortionStr = null;
    MaterialProperty _DistortionSpeed = null;
    MaterialProperty _DistortionRadius = null;
    MaterialProperty _DistortionFade = null;
    MaterialProperty _DistortionP2O = null;

    // Blur
    MaterialProperty _BlurModel = null;
    MaterialProperty _BlurUseGlobal = null;
    MaterialProperty _BlurMinRange = null;
    MaterialProperty _BlurMaxRange = null;
    MaterialProperty _RGBSplit = null;
    MaterialProperty _DoF = null;
    MaterialProperty _BlurOpacity = null;
    MaterialProperty _BlurStr = null;
    MaterialProperty _DoFP2O = null;
    MaterialProperty _DoFFade = null;
    MaterialProperty _DoFRadius = null;
    MaterialProperty _PixelationStr = null;
    MaterialProperty _RippleGridStr = null;
    MaterialProperty _BlurRadius = null;
    MaterialProperty _BlurY = null;
// 	MaterialProperty _BloomThreshold = null;
	MaterialProperty _BlurSamples = null;
	MaterialProperty _PixelBlurSamples = null;
	MaterialProperty _CrushBlur = null;

	// Noise
	MaterialProperty _ScanLine = null;
	MaterialProperty _ScanLineThick = null;
	MaterialProperty _ScanLineSpeed = null;
	MaterialProperty _NoiseMode = null;
	MaterialProperty _Noise = null;
	MaterialProperty _NoiseRGB = null;
	MaterialProperty _NoiseStrength = null;

    // Fog
    MaterialProperty _Fog = null;
    MaterialProperty _FogUseGlobal = null;
    MaterialProperty _FogMinRange = null;
    MaterialProperty _FogMaxRange = null;
    MaterialProperty _FogColor = null;
    MaterialProperty _FogRadius = null;
    MaterialProperty _FogSafeZone = null;
    MaterialProperty _FogSafeRadius = null;
    MaterialProperty _FogSafeMaxRange = null;
    MaterialProperty _FogP2O = null;
    MaterialProperty _FogFade = null; 
    MaterialProperty _FogSafeOpacity = null;

    // Zoom
    MaterialProperty _Zoom = null;
    MaterialProperty _ZoomUseGlobal = null;
    MaterialProperty _ZoomMinRange = null;
    MaterialProperty _ZoomMaxRange = null;
    MaterialProperty _ZoomStr = null;
    MaterialProperty _ZoomStrR = null;
    MaterialProperty _ZoomStrG = null;
    MaterialProperty _ZoomStrB = null;

    // SST
    MaterialProperty _SST = null;
    MaterialProperty _SSTUseGlobal = null;
    MaterialProperty _SSTMinRange = null;
    MaterialProperty _SSTMaxRange = null;
    MaterialProperty _SSTBlend = null;
    MaterialProperty _ScreenTex = null;
    MaterialProperty _SSTColor = null;
    MaterialProperty _SSTWidth = null;
    MaterialProperty _SSTHeight = null;
    MaterialProperty _SSTScale = null;
    MaterialProperty _SSTLR = null;
    MaterialProperty _SSTUD = null;
    MaterialProperty _SSTColumnsX = null;
    MaterialProperty _SSTRowsY = null;
    MaterialProperty _SSTAnimationSpeed = null;
    MaterialProperty _SSTAnimatedDist = null;
	MaterialProperty _ScrubPos = null;
	MaterialProperty _ManualScrub = null;

    // Triplanar
    MaterialProperty _Triplanar = null;
    MaterialProperty _TPUseGlobal = null;
    MaterialProperty _TPMinRange = null;
    MaterialProperty _TPMaxRange = null;
    MaterialProperty _TPRadius = null;
    MaterialProperty _TPFade = null;
    MaterialProperty _TPP2O = null;
    MaterialProperty _TPColor = null;
    MaterialProperty _TPTexture = null;
    MaterialProperty _TPScroll = null;
    MaterialProperty _TPNoiseTex = null;
    MaterialProperty _TPNoiseScroll = null;
    MaterialProperty _TPThickness = null;
    MaterialProperty _TPNoise = null;
    MaterialProperty _TPScanFade = null;

    // Extras
    // MaterialProperty _DeepFry = null;
    // MaterialProperty _Flavor = null;
    // MaterialProperty _Sizzle = null;
    // MaterialProperty _Heat = null;
    MaterialProperty _Pulse = null;
    MaterialProperty _PulseColor = null;
    MaterialProperty _PulseSpeed = null;
    MaterialProperty _WaveForm = null;
    MaterialProperty _Letterbox = null;
    MaterialProperty _LetterboxStr = null;
    MaterialProperty _UseZoomFalloff = null;
    MaterialProperty _Shift = null;
    MaterialProperty _InvertX = null;
    MaterialProperty _InvertY = null;
    MaterialProperty _ShiftX = null;
    MaterialProperty _ShiftY = null;
    MaterialProperty _OutlineCol = null;
    MaterialProperty _OutlineThresh = null;
    MaterialProperty _BackgroundCol = null;
    MaterialProperty _OutlineType = null;
    MaterialProperty _OutlineThiccS = null;
	MaterialProperty _OLUseGlobal = null;
	MaterialProperty _OLMinRange = null;
	MaterialProperty _OLMaxRange = null;
//    MaterialProperty _OutlineThiccN = null;
	MaterialProperty _RoundingToggle = null;
	MaterialProperty _Rounding = null;
	MaterialProperty _RoundingOpacity = null;
	MaterialProperty _NormalMapFilter = null;
	MaterialProperty _NMFOpacity = null;
	MaterialProperty _NMFToggle = null;
	MaterialProperty _DepthBufferToggle = null;
	MaterialProperty _DBOpacity = null;
	MaterialProperty _DBColor = null;
	MaterialProperty _AuraFade = null;
	MaterialProperty _AuraSampleCount = null;
	MaterialProperty _AuraStr = null;
	MaterialProperty _NoiseUseGlobal = null;
	MaterialProperty _NoiseMinRange = null;
	MaterialProperty _NoiseMaxRange = null;
	MaterialProperty _DisplayGlobalGizmo = null;

    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    MaterialEditor m_MaterialEditor;

	[DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    static void DrawGizmo(MeshRenderer meshRenderer, GizmoType gizmoType){
		if (meshRenderer.sharedMaterial != null){
			Material material = meshRenderer.sharedMaterial;
			if (!material.shader.name.Contains("Mochie/Screen FX")) return;
			if (!foldouts.ContainsKey(material)) return;
			if (material.GetFloat("_DisplayGlobalGizmo") > 0){
				Vector3 position = meshRenderer.transform.position;
				Toggles toggles = foldouts[material];

				if (toggles.GetState("GENERAL")){
					Gizmos.color = Color.yellow;
					Gizmos.DrawWireSphere(position, material.GetFloat("_MinRange"));
					Gizmos.color = new Color(0.9f, 0.9f, 0.3f, 1f);
					Gizmos.DrawWireSphere(position, material.GetFloat("_MaxRange"));
				}
				if (toggles.GetState("FILTERING")){
					if (material.GetFloat("_FilterModel") > 0 && material.GetFloat("_ColorUseGlobal") == 0) {
						Gizmos.color = Color.white;
						Gizmos.DrawWireSphere(position, material.GetFloat("_ColorMinRange"));
						Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
						Gizmos.DrawWireSphere(position, material.GetFloat("_ColorMaxRange"));
					}
				}
				if (toggles.GetState("SHAKE")){
					if (material.GetFloat("_ShakeModel") > 0 && material.GetFloat("_ShakeUseGlobal") == 0) {
						Gizmos.color = Color.white;
						Gizmos.DrawWireSphere(position, material.GetFloat("_ShakeMinRange"));
						Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
						Gizmos.DrawWireSphere(position, material.GetFloat("_ShakeMaxRange"));
					}
				}
				if (toggles.GetState("DISTORTION")){
					if (material.GetFloat("_DistortionModel") > 0 && material.GetFloat("_DistortionUseGlobal") == 0) {
						Gizmos.color = Color.white;
						Gizmos.DrawWireSphere(position, material.GetFloat("_DistortionMinRange"));
						Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
						Gizmos.DrawWireSphere(position, material.GetFloat("_DistortionMaxRange"));
					}
				}
				if (toggles.GetState("BLUR")){
					if (material.GetFloat("_BlurModel") > 0 && material.GetFloat("_BlurUseGlobal") == 0) {
						Gizmos.color = Color.white;
						Gizmos.DrawWireSphere(position, material.GetFloat("_BlurMinRange"));
						Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
						Gizmos.DrawWireSphere(position, material.GetFloat("_BlurMaxRange"));
					}
				}
				if (toggles.GetState("NOISE")){
					if (material.GetFloat("_NoiseMode") > 0 && material.GetFloat("_NoiseUseGlobal") == 0) {
						Gizmos.color = Color.white;
						Gizmos.DrawWireSphere(position, material.GetFloat("_NoiseMinRange"));
						Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
						Gizmos.DrawWireSphere(position, material.GetFloat("_NoiseMaxRange"));
					}
				}
				if (toggles.GetState("ZOOM")){
					if (material.GetFloat("_Zoom") > 0 && material.GetFloat("_ZoomUseGlobal") == 0) {
						Gizmos.color = Color.white;
						Gizmos.DrawWireSphere(position, material.GetFloat("_ZoomMinRange"));
						Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
						Gizmos.DrawWireSphere(position, material.GetFloat("_ZoomMaxRange"));
					}
				}
				if (toggles.GetState("IMAGE OVERLAY")){
					if (material.GetFloat("_SST") > 0 && material.GetFloat("_SSTUseGlobal") == 0) {
						Gizmos.color = Color.white;
						Gizmos.DrawWireSphere(position, material.GetFloat("_SSTMinRange"));
						Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
						Gizmos.DrawWireSphere(position, material.GetFloat("_SSTMaxRange"));
					}
				}
				if (toggles.GetState("FOG")){
					if (material.GetFloat("_Fog") > 0 && material.GetFloat("_FogUseGlobal") == 0) {
						Gizmos.color = Color.white;
						Gizmos.DrawWireSphere(position, material.GetFloat("_FogMinRange"));
						Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
						Gizmos.DrawWireSphere(position, material.GetFloat("_FogMaxRange"));
					}
				}
				if (toggles.GetState("TRIPLANAR")){
					if (material.GetFloat("_Triplanar") > 0 && material.GetFloat("_TPUseGlobal") == 0) {
						Gizmos.color = Color.white;
						Gizmos.DrawWireSphere(position, material.GetFloat("_TPMinRange"));
						Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
						Gizmos.DrawWireSphere(position, material.GetFloat("_TPMaxRange"));
					}
				}
				if (toggles.GetState("OUTLINE")){
					if (material.GetFloat("_OutlineType") > 0 && material.GetFloat("_OLUseGlobal") == 0) {
						Gizmos.color = new Color(1f, 0.647f, 0f, 1f);
						Gizmos.DrawWireSphere(position, material.GetFloat("_OLMinRange"));
						Gizmos.color = new Color(1f, 0.847f, 0.2f, 1f);
						Gizmos.DrawWireSphere(position, material.GetFloat("_OLMaxRange"));
					}
				}
			}
		}
    }

    public override void OnGUI(MaterialEditor me, MaterialProperty[] props) {
		Material mat = (Material)me.target;

        if (!me.isVisible)
            return;
        foreach (var property in GetType().GetFields(bindingFlags)){
            if (property.FieldType == typeof(MaterialProperty))
                property.SetValue(this, FindProperty(property.Name, props));
        }

		bool isSFXX = MGUI.IsXVersion(mat);

		if (isSFXX){
			header = "SFXHeaderX_Pro";
			if (!EditorGUIUtility.isProSkin){
				header = "SFXHeaderX";
				watermark = "Watermark";
				keyTex = "KeyIcon";
			}
		}
		else {
			header = "SFXHeader_Pro";
			if (!EditorGUIUtility.isProSkin){
				header = "SFXHeader";
				watermark = "Watermark";
				keyTex = "KeyIcon";
			}
		}

		// Add mat to foldout dictionary if it isn't in there yet
		
		if (!foldouts.ContainsKey(mat))
			foldouts.Add(mat, toggles);

		ApplyMaterialSettings(mat);
		
        Texture2D headerTex = (Texture2D)Resources.Load(header, typeof(Texture2D));
		Texture2D watermarkTex = (Texture2D)Resources.Load(watermark, typeof(Texture2D));
		Texture2D patIconTex = (Texture2D)Resources.Load(patIcon, typeof(Texture2D));
		Texture2D resetIconTex = (Texture2D)Resources.Load("ResetIcon", typeof(Texture2D));
		Texture2D collapseIconTex = (Texture2D)Resources.Load("CollapseIcon", typeof(Texture2D));
		Texture2D expandIconTex = (Texture2D)Resources.Load("ExpandIcon", typeof(Texture2D));
		Texture2D collapseIcon = (Texture2D)Resources.Load("CollapseIcon", typeof(Texture2D));
		Texture2D keyIcon = (Texture2D)Resources.Load(keyTex, typeof(Texture2D));

		GUIContent keyLabel = new GUIContent(keyIcon, "Toggle material keywords list.");

        MGUI.CenteredTexture(headerTex, 0, 0);
		GUILayout.Space(-34);
		ListKeywords(mat, keyLabel, this.buttonSize);

        EditorGUI.BeginChangeCheck(); {

            // Global
			bool generalTab = Foldouts.DoFoldout(foldouts, mat, me, 2, "GENERAL");
			if (MGUI.TabButton(collapseIcon, 26f)){
				for (int i = 1; i <= foldouts[mat].GetToggles().Length-1; i++)
					foldouts[mat].SetState(i, false);
			}
			MGUI.Space8();
			if (MGUI.TabButton(resetIconTex, 54f))
				ResetRendering();
			MGUI.Space8();
            if (generalTab){
				MGUI.Space4();
				me.ShaderProperty(_DisplayGlobalGizmo, "Display Range Gizmos");
				me.RenderQueueField();
                EditorGUI.BeginChangeCheck();
				me.ShaderProperty(_BlendMode, "Blending Mode");
				if (EditorGUI.EndChangeCheck())
					SetBlendMode(mat);
                if (_BlendMode.floatValue > 0)
                    me.ShaderProperty(_Opacity, "Opacity");
				
				GUILayout.Label("Global Falloff", EditorStyles.boldLabel);
				MGUI.SpaceN2();
                me.ShaderProperty(_MinRange, minLabel);
                me.ShaderProperty(_MaxRange, maxLabel);
                MGUI.Space8();
            }

            // Filtering
			bool colorTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "FILTERING");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetColor();
			MGUI.Space8();
			if (colorTab){
				MGUI.Space4();
				me.ShaderProperty(_FilterModel, modeLabel);
				if (_FilterModel.floatValue > 0){
					me.ShaderProperty(_FilterStrength, "Opacity");
					MGUI.Space6();
					me.ShaderProperty(_ColorUseGlobal, ugfLabel);
					if (_ColorUseGlobal.floatValue == 0){
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_ColorMinRange, minLabel);
							me.ShaderProperty(_ColorMaxRange, maxLabel);
						});
					}
					else MGUI.Space6();
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_Color, "Tint");
						if (_AutoShift.floatValue == 0)
							me.ShaderProperty(_Hue, "Hue");
						else
							me.ShaderProperty(_AutoShiftSpeed, "Shift Speed");
						me.ShaderProperty(_AutoShift, "Auto Shift");
					});
					MGUI.PropertyGroup(() => {
						MGUI.Vector3FieldRGB(_RGB, "RGB Multiplier");
						me.ShaderProperty(_Brightness, "Brightness");
						me.ShaderProperty(_Contrast, "Contrast");
						me.ShaderProperty(_HDR, "HDR");
					});
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_Saturation, "Saturation");
						MGUI.PropertyGroupLayer(() => {
							me.ShaderProperty(_SaturationR, "Red Saturation");
							me.ShaderProperty(_SaturationG, "Green Saturation");
							me.ShaderProperty(_SaturationB, "Blue Saturation");
						});
					});
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_Invert, "Invert");
						MGUI.PropertyGroupLayer(() => {
							me.ShaderProperty(_InvertR, "Red Inversion");
							me.ShaderProperty(_InvertG, "Green Inversion");
							me.ShaderProperty(_InvertB, "Blue Inversion");
						});
					});
				}
				MGUI.Space8();
			}

            // Shake
			bool shakeTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "SHAKE");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetShake();
			MGUI.Space8();
            if (shakeTab){
                MGUI.Space4();
                me.ShaderProperty(_ShakeModel, modeLabel);
				if (_ShakeModel.floatValue > 0){
					MGUI.Space6();
                    me.ShaderProperty(_ShakeUseGlobal, ugfLabel);
                    if (_ShakeUseGlobal.floatValue == 0){
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_ShakeMinRange, minLabel);
							me.ShaderProperty(_ShakeMaxRange, maxLabel);
						});
                    }
					else MGUI.Space6();
					MGUI.PropertyGroup(() => {
						if (_ShakeModel.floatValue == 1 || _ShakeModel.floatValue == 2){
							me.ShaderProperty(_Amplitude, "Amplitude");
							me.ShaderProperty(_ShakeSpeedX, speedXLabel);
							me.ShaderProperty(_ShakeSpeedY, speedYLabel);
						}
						if (_ShakeModel.floatValue == 3){
							me.TexturePropertySingleLine(shakeNoiseTexLabel, _ShakeNoiseTex);
							me.ShaderProperty(_Amplitude, "Amplitude");
							me.ShaderProperty(_ShakeSpeedXY, speedLabel);
						}
					});
                }
                MGUI.Space8();
            }

            // Distortion
			bool distTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "DISTORTION");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetDistortion();
			MGUI.Space8();
            if (distTab){
                MGUI.Space4();
                me.ShaderProperty(_DistortionModel, modeLabel);
                if (_DistortionModel.floatValue > 0){
					MGUI.Space6();
                    me.ShaderProperty(_DistortionUseGlobal, ugfLabel);
                    if (_DistortionUseGlobal.floatValue == 0){
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_DistortionMinRange, minLabel);
							me.ShaderProperty(_DistortionMaxRange, maxLabel);
						});
                    }
					else MGUI.Space6();
					MGUI.PropertyGroup(() => {
						me.TexturePropertySingleLine(normalMapLabel, _NormalMap);
						me.TextureScaleOffsetProperty(_NormalMap);
						me.ShaderProperty(_DistortionStr, strengthLabel);
						me.ShaderProperty(_DistortionSpeed, speedLabel);
					});
                    if (_DistortionModel.floatValue == 2){
						MGUI.PropertyGroup(() => {
							MGUI.DisplayInfo("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
							me.ShaderProperty(_DistortionRadius, radiusLabel);
							me.ShaderProperty(_DistortionFade, fadeLabel);
							me.ShaderProperty(_DistortionP2O, p2oLabel);
						});
                    }
                }
                MGUI.Space8();
            }

			// Blur
			bool blurTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "BLUR");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetBlur();
			MGUI.Space8();
			if (blurTab){
				MGUI.Space4();
				me.ShaderProperty(_BlurModel, modeLabel);
				if (_BlurModel.floatValue > 0){
					if (_BlurModel.floatValue == 1){
						me.ShaderProperty(_PixelBlurSamples, "Sample Count");
						if (_PixelBlurSamples.floatValue > 43){
							MGUI.Space6();
							MGUI.DisplayWarning("High sample counts can be very laggy! If your strength value is low please consider staying at or below 43 samples.");
						}
					}
					MGUI.Space6();
					me.ShaderProperty(_BlurUseGlobal, ugfLabel);
					if (_BlurUseGlobal.floatValue == 0){
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_BlurMinRange, minLabel);
							me.ShaderProperty(_BlurMaxRange, maxLabel);
						});
					}
					else MGUI.Space6();
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_BlurOpacity, "Opacity");
						me.ShaderProperty(_BlurStr, strengthLabel);
						if (_BlurModel.floatValue == 3){
							me.ShaderProperty(_BlurSamples, "Sample Count");
							me.ShaderProperty(_BlurRadius, radiusLabel);
						}
						me.ShaderProperty(_PixelationStr, "Pixelation");
						me.ShaderProperty(_RippleGridStr, "Ripple Grid");
						if (_BlurModel.floatValue == 3){
							me.ShaderProperty(_DoF, "Depth of Field");
							if (_DoF.floatValue == 1){
								MGUI.PropertyGroupLayer(() => {
									MGUI.DisplayInfo("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
									me.ShaderProperty(_DoFRadius, radiusLabel);
									me.ShaderProperty(_DoFFade, fadeLabel);
									me.ShaderProperty(_DoFP2O, p2oLabel);
								});
							}
						}
					});
					MGUI.PropertyGroup(_BlurModel.floatValue != 3, () => {
						if (_BlurModel.floatValue != 3){
							me.ShaderProperty(_DoF, "Depth of Field");
							if (_DoF.floatValue == 1){
								MGUI.PropertyGroupLayer(() => {
									MGUI.DisplayInfo("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
									me.ShaderProperty(_DoFRadius, radiusLabel);
									me.ShaderProperty(_DoFFade, fadeLabel);
									me.ShaderProperty(_DoFP2O, p2oLabel);
								});
							}
							me.ShaderProperty(_BlurY, "Y Axis Only");
							me.ShaderProperty(_RGBSplit, "Chrom. Abberation");
						}
						if (_BlurModel.floatValue == 1)
							me.ShaderProperty(_CrushBlur, "Crush");
					});
				}
				MGUI.Space8();  
			}

			// Noise
            bool noiseTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "NOISE");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetNoise();
			MGUI.Space8();
            if (noiseTab){
				MGUI.Space4();
				me.ShaderProperty(_NoiseMode, "Mode");
				if (_NoiseMode.floatValue > 0){
					me.ShaderProperty(_NoiseStrength, "Opacity");
					MGUI.Space6();
					me.ShaderProperty(_NoiseUseGlobal, ugfLabel);
					if (_NoiseUseGlobal.floatValue == 0){
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_NoiseMinRange, minLabel);
							me.ShaderProperty(_NoiseMaxRange, maxLabel);
						});
					}
					else MGUI.Space6();
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_Noise, "Noise (Grayscale)");
						MGUI.Vector3FieldRGB(_NoiseRGB, "Noise (RGB)");
						me.ShaderProperty(_ScanLine, "Scan Lines");
						MGUI.PropertyGroupLayer(() => {
							me.ShaderProperty(_ScanLineThick, "Bar Thickness");
							me.ShaderProperty(_ScanLineSpeed, "Scroll Speed");
						});
					});
				}
				MGUI.Space8();
			}
			if (isSFXX){
				bool zoomTab =Foldouts.DoFoldout(foldouts, mat, me, 1, "ZOOM");
				if (MGUI.TabButton(resetIconTex, 26f))
					ResetZoom();
				MGUI.Space8();
				// Zoom
				if (zoomTab){
					MGUI.Space4();
					me.ShaderProperty(_Zoom, modeLabel);
					if (_Zoom.floatValue > 0){
						MGUI.Space6();
						me.ShaderProperty(_ZoomUseGlobal, ugfLabel);
						if (_ZoomUseGlobal.floatValue == 0){
							MGUI.PropertyGroup(() => {
								me.ShaderProperty(_ZoomMinRange, minLabel);
								me.ShaderProperty(_ZoomMaxRange, maxLabel);
							});
						}
						else MGUI.Space6();
						MGUI.PropertyGroup(() => {
							if (_Zoom.floatValue == 2){
								me.ShaderProperty(_ZoomStrR, "Red");
								me.ShaderProperty(_ZoomStrG, "Green");
								me.ShaderProperty(_ZoomStrB, "Blue");
							}
							else me.ShaderProperty(_ZoomStr, strengthLabel);
						});
					}
					MGUI.Space8();
				}
				
				// Screenspace Texture Overlay
				bool sstTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "IMAGE OVERLAY");
				if (MGUI.TabButton(resetIconTex, 26f))
					ResetSST();
				MGUI.Space8();
				if (sstTab){
					MGUI.Space4();
					me.ShaderProperty(_SST, modeLabel);
					if (_SST.floatValue > 0){
						MGUI.Space6();
						me.ShaderProperty(_SSTUseGlobal, ugfLabel);
						if (_SSTUseGlobal.floatValue == 0){
							MGUI.PropertyGroup(() => {
								me.ShaderProperty(_SSTMinRange, minLabel);
								me.ShaderProperty(_SSTMaxRange, maxLabel);
							});
						}
						else MGUI.Space6();
						MGUI.PropertyGroup(() => {
							if (_SST.floatValue != 3){
								me.TexturePropertySingleLine(screenTexLabel, _ScreenTex, _SSTColor, _SSTBlend);
							}
							else {
								me.TexturePropertySingleLine(normalMapLabel, _ScreenTex);
								me.ShaderProperty(_SSTAnimatedDist, "Strength");
							}
							if (_SST.floatValue > 1){
								me.ShaderProperty(_SSTColumnsX, "Columns (X)");
								me.ShaderProperty(_SSTRowsY, "Rows (Y)");
								MGUI.CustomToggleSlider("Frame", _ManualScrub, _ScrubPos, 0f, (_SSTColumnsX.floatValue*_SSTRowsY.floatValue)-1);
								MGUI.ToggleGroup(_ManualScrub.floatValue == 1);
								me.ShaderProperty(_SSTAnimationSpeed, "FPS");
								MGUI.ToggleGroupEnd();
							}
						});
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_SSTScale, "Scale");
							me.ShaderProperty(_SSTWidth, "Width");
							me.ShaderProperty(_SSTHeight, "Height");
							me.ShaderProperty(_SSTLR, "Left / Right");
							me.ShaderProperty(_SSTUD, "Down / Up");
						});
					}
					MGUI.Space8();
				}

				// Fog
				bool fogTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "FOG");
				if (MGUI.TabButton(resetIconTex, 26f))
					ResetFog();
				MGUI.Space8();
				if (fogTab){
					MGUI.Space4();
					me.ShaderProperty(_Fog, modeLabel);
					if (_Fog.floatValue == 1){
						MGUI.Space6();
						me.ShaderProperty(_FogUseGlobal, ugfLabel);
						if (_FogUseGlobal.floatValue == 0){
							MGUI.PropertyGroup(() => {
								me.ShaderProperty(_FogMinRange, minLabel);
								me.ShaderProperty(_FogMaxRange, maxLabel);
							});
						}
						else MGUI.Space6();
						MGUI.PropertyGroup(() => {
							MGUI.DisplayInfo("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
							me.ShaderProperty(_FogColor, colorLabel);
							me.ShaderProperty(_FogRadius, radiusLabel);
							me.ShaderProperty(_FogFade, fadeLabel);
							me.ShaderProperty(_FogP2O, p2oLabel);
							me.ShaderProperty(_FogSafeZone, "Safe Zone");
							if (_FogSafeZone.floatValue == 1){
								MGUI.PropertyGroupLayer(() => {	
									me.ShaderProperty(_FogSafeRadius, "Vision Radius");
									me.ShaderProperty(_FogSafeMaxRange, "Outer Perimeter");
									me.ShaderProperty(_FogSafeOpacity, "Opacity");
								});
							}
						});
					}
					MGUI.Space8();
				}

				// Triplanar Mapping
				bool triplanarTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "TRIPLANAR");
				if (MGUI.TabButton(resetIconTex, 26f))
					ResetTriplanar();
				MGUI.Space8();
				if (triplanarTab){
					MGUI.Space4();
					me.ShaderProperty(_Triplanar, modeLabel);
					if (_Triplanar.floatValue > 0){				
						MGUI.Space6();
						me.ShaderProperty(_TPUseGlobal, ugfLabel);
						if (_TPUseGlobal.floatValue == 0){
							MGUI.PropertyGroup(() => {
								me.ShaderProperty(_TPMinRange, minLabel);
								me.ShaderProperty(_TPMaxRange, maxLabel);
							});
						}
						else MGUI.Space6();
						MGUI.PropertyGroup(() => {
							MGUI.DisplayInfo("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
							me.TexturePropertySingleLine(tpTexLabel, _TPTexture, _TPColor);
							if (_TPTexture.textureValue){
								MGUI.TextureSO(me, _TPTexture);
								MGUI.Vector3Field(_TPScroll, "Scroll Speed");
								MGUI.Space4();
							}
							me.TexturePropertySingleLine(tpNoiseTexLabel, _TPNoiseTex);
							if (_TPNoiseTex.textureValue){          
								MGUI.TextureSO(me, _TPNoiseTex);
								MGUI.Vector3Field(_TPNoiseScroll, "Scroll Speed");
							}
						});
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_TPRadius, radiusLabel);
							if (_Triplanar.floatValue == 2){
								me.ShaderProperty(_TPScanFade, fadeLabel);
								me.ShaderProperty(_TPThickness, "Thickness");
								me.ShaderProperty(_TPNoise, "Noise");
							}
							else me.ShaderProperty(_TPFade, fadeLabel);
							me.ShaderProperty(_TPP2O, p2oLabel);
						});
					}
					MGUI.Space8();
				}

				// Outline
				bool outlineTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "OUTLINE");
				if (MGUI.TabButton(resetIconTex, 26f))
					ResetOutline();
				MGUI.Space8();
				if (outlineTab){
					MGUI.Space4();
					me.ShaderProperty(_OutlineType, modeLabel);
					if (_OutlineType.floatValue > 0){
						MGUI.Space6();
						me.ShaderProperty(_OLUseGlobal, ugfLabel);
						if (_OLUseGlobal.floatValue == 0){
							MGUI.PropertyGroup(() => {
								me.ShaderProperty(_OLMinRange, minLabel);
								me.ShaderProperty(_OLMaxRange, maxLabel);
							});
						}
						else MGUI.Space6();
						MGUI.PropertyGroup(() => {
							MGUI.DisplayInfo("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
							if (_OutlineType.floatValue == 2)
								me.ShaderProperty(_AuraSampleCount, "Sample Count");
							me.ShaderProperty(_OutlineCol, "Line Tint");
							me.ShaderProperty(_BackgroundCol, "Background Tint");
							if (_OutlineType.floatValue == 1){
								me.ShaderProperty(_OutlineThiccS, "Thickness");
								me.ShaderProperty(_OutlineThresh, strengthLabel);
							}
							else if (_OutlineType.floatValue == 2){
								me.ShaderProperty(_AuraStr, "Thickness");
								me.ShaderProperty(_AuraFade, "Fade");
							}
						});
					}
					MGUI.Space8();
				}

				// Extras
				
				bool miscTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "MISC");
				if (MGUI.TabButton(resetIconTex, 26f))
					ResetExtras();
				MGUI.Space8();
				if (miscTab){
					MGUI.Space4();
					// if (Foldouts.DoMediumFoldout(foldouts, mat, me, 0, "Framebuffer")){
					// 	if (_FreezeFrame.floatValue == 1 && _GhostingToggle.floatValue == 1){
					// 		_FreezeFrame.floatValue = 0;
					// 		_GhostingToggle.floatValue = 0;
					// 	}
					// 	me.ShaderProperty(_FreezeFrame, "Freeze Frame");
					// 	MGUI.ToggleSlider(me, "Ghosting", _GhostingToggle, _GhostingStr);
					// 	MGUI.FramebufferSection(me, new MaterialProperty[] {_FreezeFrame, _GhostingToggle}, _GhostingStr);
					// 	MGUI.Space6();
					// }
					// else MGUI.SpaceN2();

					if (Foldouts.DoMediumFoldout(foldouts, mat, me, _Letterbox, 0, "Letterbox")){
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_Letterbox.floatValue == 0);
							me.ShaderProperty(_UseZoomFalloff, "Use Zoom Falloff");
							me.ShaderProperty(_LetterboxStr, "Bar Width");
							MGUI.ToggleGroupEnd();
						});
					}
					else MGUI.SpaceN4();

					// if (Foldouts.DoMediumFoldout(foldouts, mat, me, 4f, _DeepFry, "Deep Fry")){
					// 	MGUI.Space6();
					// 	MGUI.ToggleGroup(_DeepFry.floatValue == 0);
					// 	me.ShaderProperty(_Flavor, "Flavor");
					// 	me.ShaderProperty(_Heat, "Heat");
					// 	me.ShaderProperty(_Sizzle, "Sizzle");
					// 	MGUI.ToggleGroupEnd();
					// 	MGUI.Space6();
					// }
					// else MGUI.SpaceN2();

					if (Foldouts.DoMediumFoldout(foldouts, mat, me, _Pulse, 0, "Pulse")){
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_Pulse.floatValue == 0);
							me.ShaderProperty(_WaveForm, "Waveform");
							MGUI.ToggleGroup(_WaveForm.floatValue == 0);
							me.ShaderProperty(_PulseColor, "Include Color");
							me.ShaderProperty(_PulseSpeed, speedLabel);
							MGUI.ToggleGroupEnd();
							MGUI.ToggleGroupEnd();
						});
					}
					else MGUI.SpaceN4();

					if (Foldouts.DoMediumFoldout(foldouts, mat, me, _Shift, 0, "UV Manipulation")){
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_Shift.floatValue == 0);
							me.ShaderProperty(_InvertX, "Invert X");
							me.ShaderProperty(_InvertY, "Invert Y");
							me.ShaderProperty(_ShiftX, "Shift X");
							me.ShaderProperty(_ShiftY, "Shift Y");
							MGUI.ToggleGroupEnd();
						});
					}
					else MGUI.SpaceN4();

					if (Foldouts.DoMediumFoldout(foldouts, mat, me, _RoundingToggle, 0, "Rounding")){
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_RoundingToggle.floatValue == 0);
							me.ShaderProperty(_RoundingOpacity, "Opacity");
							me.ShaderProperty(_Rounding, "Precision");
							MGUI.ToggleGroupEnd();
						});
					}
					else MGUI.SpaceN4();

					if (Foldouts.DoMediumFoldout(foldouts, mat, me, _NMFToggle, 0, "Normal Map")){
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_NMFToggle.floatValue == 0);
							me.ShaderProperty(_NMFOpacity, "Opacity");
							me.ShaderProperty(_NormalMapFilter, "Strength");
							MGUI.ToggleGroupEnd();
						});
					}
					else MGUI.SpaceN4();
					
					if (Foldouts.DoMediumFoldout(foldouts, mat, me, _DepthBufferToggle, 0, "Depth Buffer")){
						if (_DepthBufferToggle.floatValue == 1){
							MGUI.DisplayInfo("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
						}
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_DepthBufferToggle.floatValue == 0);
							me.ShaderProperty(_DBOpacity, "Opacity");
							me.ShaderProperty(_DBColor, "Tint");
							MGUI.ToggleGroupEnd();
						});
					}
					else MGUI.SpaceN2();
					MGUI.Space6();
				}
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
		MGUI.VersionLabel(versionLabel, 12,-16,-20);
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

	void SetKeyword(Material m, string keyword, bool state) {
		if (state)
			m.EnableKeyword(keyword);
		else
			m.DisableKeyword(keyword);
	}

	void ApplyMaterialSettings(Material mat){
		int filterMode = mat.GetInt("_FilterModel");
		int shakeMode = mat.GetInt("_ShakeModel");
		int distMode = mat.GetInt("_DistortionModel");
		int blurMode = mat.GetInt("_BlurModel");
		int blurY = mat.GetInt("_BlurY");
		int blurChrom = mat.GetInt("_RGBSplit");
		int dof = mat.GetInt("_DoF");
		int fogMode = mat.GetInt("_Fog");
		int zoomMode = mat.GetInt("_Zoom");
		int sstMode = mat.GetInt("_SST");
		int tpMode = mat.GetInt("_Triplanar");
		int outlineMode = mat.GetInt("_OutlineType");
		int noiseMode = mat.GetInt("_NoiseMode");
		int letterboxMode = mat.GetInt("_Letterbox");

		bool isXVersion = MGUI.IsXVersion(mat);
		bool filteringEnabled = filterMode > 0;
		bool shakeEnabled = shakeMode > 0;
		bool distortionEnabled = distMode == 1;
		bool distortionTriEnabled = distMode == 2;
		bool blurPixelEnabled = blurMode == 1;
		bool blurDitherEnabled = blurMode == 2;
		bool blurRadEnabled = blurMode == 3;
		bool blurYEnabled = blurY == 1;
		bool blurChromEnabled = blurChrom == 1;
		bool blurEnabled = blurMode > 0;
		bool noiseEnabled = noiseMode == 1;
		bool dofEnabled = dof == 1;
		bool fogEnabled = fogMode == 1 && isXVersion;
		bool zoomEnabled = zoomMode == 1 && isXVersion;
		bool zoomRGBEnabled = zoomMode == 2 && isXVersion;
		bool sstEnabled = sstMode < 3 && sstMode > 0 && isXVersion;
		bool sstDistEnabled = sstMode == 3 && isXVersion;
		bool tpEnabled = tpMode > 0 && isXVersion;
		bool outlineEnabled = outlineMode > 0 && isXVersion;
		bool letterboxEnabled = letterboxMode > 0 && isXVersion;
		
		SetKeyword(mat, "_COLORCOLOR_ON", filteringEnabled);
		SetKeyword(mat, "FXAA", shakeEnabled);
		SetKeyword(mat, "EFFECT_BUMP", distortionEnabled);
		SetKeyword(mat, "_TERRAIN_NORMAL_MAP", distortionTriEnabled);
		SetKeyword(mat, "BLOOM", blurPixelEnabled);
		SetKeyword(mat, "GRAIN", blurDitherEnabled);
		SetKeyword(mat, "_SUNDISK_SIMPLE", blurRadEnabled);
		SetKeyword(mat, "BLOOM_LENS_DIRT", blurYEnabled);
		SetKeyword(mat, "CHROMATIC_ABBERATION_LOW", blurChromEnabled);
		SetKeyword(mat, "DEPTH_OF_FIELD", dofEnabled);
		SetKeyword(mat, "_DETAIL_MULX2", zoomEnabled);
		SetKeyword(mat, "_MAPPING_6_FRAMES_LAYOUT", zoomRGBEnabled);
		SetKeyword(mat, "_COLOROVERLAY_ON", sstEnabled);
		SetKeyword(mat, "_PARALLAXMAP", sstDistEnabled);
		SetKeyword(mat, "_FADING_ON", fogEnabled);
		SetKeyword(mat, "PIXELSNAP_ON", tpEnabled);
		SetKeyword(mat, "_COLORADDSUBDIFF_ON", outlineEnabled);
		SetKeyword(mat, "_REQUIRE_UV2", noiseEnabled);

		mat.SetShaderPassEnabled("Always", zoomEnabled || zoomRGBEnabled || sstEnabled || sstDistEnabled ||  letterboxEnabled);
	}

    public static void SetBlendMode(Material mat) {
        int mode = mat.GetInt("_BlendMode");
        switch (mode) {
             case 0: // Opaque:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                break;
             case 1: // Alpha:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                break;
            case 2: // Premultiplied:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                break;
            case 3: // Additive:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                break;
            case 4: // Soft_Additive:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                break;
            case 5: // Multiply:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                break;
            case 6: // Multiply_2x:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
                break;
        }
    }

	void ResetRendering(){
		_BlendMode.floatValue = 0f;
		_SrcBlend.floatValue = 1f;
		_DstBlend.floatValue = 0f;
		_Opacity.floatValue = 1f;
		_MinRange.floatValue = 8f;
		_MaxRange.floatValue = 15f;
	}

	void ResetColor(){
		_FilterModel.floatValue = 0f;
		_ColorUseGlobal.floatValue = 1f;
		_ColorMinRange.floatValue = 8f;
		_ColorMaxRange.floatValue = 15f;
		_AutoShift.floatValue = 0f;
		_Color.colorValue = Color.white;
		_AutoShiftSpeed.floatValue = 0.25f;
		_Hue.floatValue = 0f;
		_Saturation.floatValue = 1f;
		_Value.floatValue = 0f;
		_Brightness.floatValue = 1f;
		_HDR.floatValue = 0f;
		_Contrast.floatValue = 1f;
		_Invert.floatValue = 0f;
		_InvertR.floatValue = 0f;
		_InvertG.floatValue = 0f;
		_InvertB.floatValue = 0f;
		_SaturationR.floatValue = 1f;
		_SaturationG.floatValue = 1f;
		_SaturationB.floatValue = 1f;
	}

	void ResetShake(){
		_ShakeModel.floatValue = 0f;
		_ShakeUseGlobal.floatValue = 1f;
		_ShakeMinRange.floatValue = 8f;
		_ShakeMaxRange.floatValue = 15f;
		_Amplitude.floatValue = 0f;
		_ShakeSpeedX.floatValue = 0.5234375f;
		_ShakeSpeedY.floatValue = 0.78125f;
		_ShakeSpeedXY.floatValue = 5f;
	}
	
	void ResetDistortion(){
		_DistortionModel.floatValue = 0f;
		_DistortionUseGlobal.floatValue = 1f;
		_DistortionMinRange.floatValue = 8f;
		_DistortionMaxRange.floatValue = 15f;
		_DistortionStr.floatValue = 0.5f;
		_DistortionSpeed.floatValue = 0f;
		_DistortionRadius.floatValue = 2f;
		_DistortionFade.floatValue = 1f;
		_DistortionP2O.floatValue = 1f;
	}

	void ResetBlur(){
		_BlurModel.floatValue = 0f;
		_PixelBlurSamples.floatValue = 43f;
		_BlurUseGlobal.floatValue = 1f;
		_BlurMinRange.floatValue = 8f;
		_BlurMaxRange.floatValue = 15f;
		_BlurY.floatValue = 0f;
		_RGBSplit.floatValue = 0f;
		_DoF.floatValue = 0f;
		_DoFRadius.floatValue = 2f;
		_DoFFade.floatValue = 1f;
		_DoFP2O.floatValue = 1f;
		_BlurOpacity.floatValue = 1f;
		_BlurStr.floatValue = 0f;
		_BlurRadius.floatValue = 1f;
		_PixelationStr.floatValue = 0f;
		_RippleGridStr.floatValue = 0f;
		_BlurSamples.floatValue = 10f;
		_CrushBlur.floatValue = 0f;
	}

	void ResetNoise(){
		_NoiseMode.floatValue = 0f;
		_Noise.floatValue = 0f;
		_NoiseRGB.vectorValue = Vector4.zero;
		_ScanLine.floatValue = 0f;
		_ScanLineThick.floatValue = 1f;
		_ScanLineSpeed.floatValue = 1f;
		_NoiseUseGlobal.floatValue = 1f;
		_NoiseMinRange.floatValue = 8f;
		_NoiseMaxRange.floatValue = 15f;
	}
	void ResetFog(){
		_Fog.floatValue = 0f;
		_FogUseGlobal.floatValue = 0f;
		_FogMinRange.floatValue = 15f;
		_FogMaxRange.floatValue = 20f;
		_FogColor.colorValue = new Color(0.75f, 0.75f, 0.75f, 1f);
		_FogRadius.floatValue = 2f;
		_FogFade.floatValue = 1f;
		_FogP2O.floatValue = 0f;
		_FogSafeZone.floatValue = 0f;
		_FogSafeRadius.floatValue = 4f;
		_FogSafeMaxRange.floatValue = 6f;
		_FogSafeOpacity.floatValue = 1f;
	}

	void ResetZoom(){
		_Zoom.floatValue = 0f;
		_ZoomStr.floatValue = 0f;
		_ZoomStrR.floatValue = 0f;
		_ZoomStrG.floatValue = 0f;
		_ZoomStrB.floatValue = 0f;
		_ZoomUseGlobal.floatValue = 0f;
		_ZoomMinRange.floatValue = 3f;
		_ZoomMaxRange.floatValue = 4.5f;
	}

	void ResetSST(){
		_SST.floatValue = 0f;
		_SSTBlend.floatValue = 0f;
		_SSTUseGlobal.floatValue = 1f;
		_SSTMinRange.floatValue = 8f;
		_SSTMaxRange.floatValue = 15f;
		_ScreenTex.textureValue = null;
		_SSTColor.colorValue = Color.white;
		_SSTScale.floatValue = 1f;
		_SSTWidth.floatValue = 1f;
		_SSTHeight.floatValue = 1f;
		_SSTLR.floatValue = 0f;
		_SSTUD.floatValue = 0f;
		_SSTColumnsX.floatValue = 2f;
		_SSTRowsY.floatValue = 2f;
		_SSTAnimationSpeed.floatValue = 60f;
		_ScrubPos.floatValue = 0f;
		_ManualScrub.floatValue = 0f;
	}

	void ResetTriplanar(){
		_Triplanar.floatValue = 0f;
		_TPUseGlobal.floatValue = 1f;
		_TPColor.colorValue = Color.white;
		_TPMinRange.floatValue = 8f;
		_TPMaxRange.floatValue = 15f;
		_TPRadius.floatValue = 2f;
		_TPFade.floatValue = 0.5f;
		_TPP2O.floatValue = 1f;
		_TPScroll.vectorValue = new Vector4(0,0,0,0);
		_TPNoiseScroll.vectorValue = new Vector4(0,0,0,0);
		_TPThickness.floatValue = 0.4f;
		_TPNoise.floatValue = 0f;
		_TPScanFade.floatValue = 0.1f;
	}

	void ResetOutline(){
		_OutlineType.floatValue = 0f;
		_AuraSampleCount.floatValue = 43f;
		_OutlineCol.colorValue = new Color(0,0,0,1);
		_BackgroundCol.colorValue = new Color(1,1,1,0);
		_OutlineThresh.floatValue = 1000f;
		_OutlineThiccS.floatValue = 0.49f;
		_AuraFade.floatValue = 0.5f;
		_AuraStr.floatValue = 0.25f;
		_OLUseGlobal.floatValue = 1f;
		_OLMinRange.floatValue = 8f;
		_OLMaxRange.floatValue = 15f;
	}

	void ResetExtras(){
		_Letterbox.floatValue = 0f;
		_UseZoomFalloff.floatValue = 0f;
		_LetterboxStr.floatValue = 0f;
		_Pulse.floatValue = 0f;
		_PulseColor.floatValue = 0f;
		_PulseSpeed.floatValue = 1f;
		_WaveForm.floatValue = 0f;
		_Shift.floatValue = 0f;
		_InvertX.floatValue = 0f;
		_InvertY.floatValue = 0f;
		_ShiftX.floatValue = 0f;
		_ShiftY.floatValue = 0f;
		_RoundingToggle.floatValue = 0f;
		_Rounding.floatValue = 1f;
		_RoundingOpacity.floatValue = 1f;
		_NormalMapFilter.floatValue = 0.74f;
		_NMFToggle.floatValue = 0f;
		_NMFOpacity.floatValue = 1f;
		_DBColor.colorValue = Color.white;
		_DepthBufferToggle.floatValue = 0f;
	}
}