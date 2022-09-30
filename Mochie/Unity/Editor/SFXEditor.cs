using UnityEditor;
using UnityEngine;
using System;
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

    Toggles toggles = new Toggles(new string[] {
			"GENERAL",
			"FILTERING",
			"Filtering 1",
			"SHAKE",
			"Shake 1",
			"DISTORTION",
			"Distortion 1",
			"BLUR",
			"Blur 1",
			"ZOOM",
			"Zoom 1",
			"IMAGE OVERLAY",
			"FOG",
			"Fog 1",
			"TRIPLANAR",
			"Triplanar 1",
			"OUTLINE",
			"Outline 1",
			"MISC",
			"Misc 1",
			"Letterbox",
			"Deep Fry",
			"Pulse",
			"UV Manipulation",
			"Rounding",
			"Normal Map",
			"Depth Buffer",
			"Safe Zone",
			"NOISE",
			"Noise 1",
			"AUDIO LINK",
			"Image Overlay 1"
	}, 0);

    // Texture file names
	string header = "SFXHeader_Pro";
	string versionLabel = "v1.14";
	
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

	// Audio Link
	MaterialProperty _AudioLinkToggle = null;
	MaterialProperty _AudioLinkStrength = null;
	MaterialProperty _AudioLinkMin = null;
	MaterialProperty _AudioLinkMax = null;
	MaterialProperty _AudioLinkFilteringStrength = null;
	MaterialProperty _AudioLinkFilteringBand = null;
	MaterialProperty _AudioLinkFilteringMin = null;
	MaterialProperty _AudioLinkFilteringMax = null;
	MaterialProperty _AudioLinkShakeStrength = null;
	MaterialProperty _AudioLinkShakeBand = null;
	MaterialProperty _AudioLinkShakeMin = null;
	MaterialProperty _AudioLinkShakeMax = null;
	MaterialProperty _AudioLinkDistortionStrength = null;
	MaterialProperty _AudioLinkDistortionBand = null;
	MaterialProperty _AudioLinkDistortionMin = null;
	MaterialProperty _AudioLinkDistortionMax = null;
	MaterialProperty _AudioLinkBlurStrength = null;
	MaterialProperty _AudioLinkBlurBand = null;
	MaterialProperty _AudioLinkBlurMin = null;
	MaterialProperty _AudioLinkBlurMax = null;
	MaterialProperty _AudioLinkNoiseStrength = null;
	MaterialProperty _AudioLinkNoiseBand = null;
	MaterialProperty _AudioLinkNoiseMin = null;
	MaterialProperty _AudioLinkNoiseMax = null;
	MaterialProperty _AudioLinkZoomStrength = null;
	MaterialProperty _AudioLinkZoomBand = null;
	MaterialProperty _AudioLinkZoomMin = null;
	MaterialProperty _AudioLinkZoomMax = null;
	MaterialProperty _AudioLinkSSTStrength = null;
	MaterialProperty _AudioLinkSSTBand = null;
	MaterialProperty _AudioLinkSSTMin = null;
	MaterialProperty _AudioLinkSSTMax = null;
	MaterialProperty _AudioLinkFogOpacity = null;
	MaterialProperty _AudioLinkFogRadius = null;
	MaterialProperty _AudioLinkFogBand = null;
	MaterialProperty _AudioLinkFogMin = null;
	MaterialProperty _AudioLinkFogMax = null;
	MaterialProperty _AudioLinkTriplanarOpacity = null;
	MaterialProperty _AudioLinkTriplanarBand = null;
	MaterialProperty _AudioLinkTriplanarMin = null;
	MaterialProperty _AudioLinkTriplanarMax = null;
	MaterialProperty _AudioLinkTriplanarRadius = null;
	MaterialProperty _AudioLinkOutlineStrength = null;
	MaterialProperty _AudioLinkOutlineBand = null;
	MaterialProperty _AudioLinkOutlineMin = null;
	MaterialProperty _AudioLinkOutlineMax = null;
	MaterialProperty _AudioLinkMiscStrength = null;
	MaterialProperty _AudioLinkMiscBand = null;
	MaterialProperty _AudioLinkMiscMin = null;
	MaterialProperty _AudioLinkMiscMax = null;

    // Extras
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
	MaterialProperty _SobelClearInner = null;
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
			}
		}
		else {
			header = "SFXHeader_Pro";
			if (!EditorGUIUtility.isProSkin){
				header = "SFXHeader";
			}
		}

		// Add mat to foldout dictionary if it isn't in there yet
		
		if (!foldouts.ContainsKey(mat))
			foldouts.Add(mat, toggles);

		ApplyMaterialSettings(mat);
		
        Texture2D headerTex = (Texture2D)Resources.Load(header, typeof(Texture2D));

        GUILayout.Label(headerTex);
		MGUI.Space4();

        EditorGUI.BeginChangeCheck(); {

            // Global
			Dictionary<Action, GUIContent> generalTabButtons = new Dictionary<Action, GUIContent>();
			generalTabButtons.Add(()=>{Toggles.CollapseFoldouts(mat, foldouts, 1);}, MGUI.collapseLabel);
			generalTabButtons.Add(()=>{ResetRendering();}, MGUI.resetLabel);
			Action generalTabAction = ()=>{
				me.ShaderProperty(_DisplayGlobalGizmo, "Display Range Gizmos");
				me.RenderQueueField();
                EditorGUI.BeginChangeCheck();
				me.ShaderProperty(_BlendMode, "Blending Mode");
				MGUI.Space4();
				if (EditorGUI.EndChangeCheck())
					SetBlendMode(mat);
                if (_BlendMode.floatValue > 0)
                    me.ShaderProperty(_Opacity, "Opacity");
				
				GUILayout.Label("Global Falloff", EditorStyles.boldLabel);
				MGUI.SpaceN2();
                me.ShaderProperty(_MinRange, minLabel);
                me.ShaderProperty(_MaxRange, maxLabel);
            };
			Foldouts.Foldout("GENERAL", foldouts, generalTabButtons, mat, me, generalTabAction);

            // Filtering
			Dictionary<Action, GUIContent> filterTabButtons = new Dictionary<Action, GUIContent>();
			filterTabButtons.Add(()=>{ResetColor();}, MGUI.resetLabel);
			Action filterTabAction = ()=>{
				me.ShaderProperty(_FilterModel, modeLabel);
				if (_FilterModel.floatValue > 0){
					me.ShaderProperty(_FilterStrength, "Opacity");
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
			};
			Foldouts.Foldout("FILTERING", foldouts, filterTabButtons, mat, me, filterTabAction);

            // Shake
			Dictionary<Action, GUIContent> shakeTabButtons = new Dictionary<Action, GUIContent>();
			shakeTabButtons.Add(()=>{ResetShake();}, MGUI.resetLabel);
			Action shakeTabAction = ()=>{
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
            };
			Foldouts.Foldout("SHAKE", foldouts, shakeTabButtons, mat, me, shakeTabAction);

            // Distortion
			Dictionary<Action, GUIContent> distTabButtons = new Dictionary<Action, GUIContent>();
			distTabButtons.Add(()=>{ResetDistortion();}, MGUI.resetLabel);
			Action distTabAction = ()=>{
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
            };
			Foldouts.Foldout("DISTORTION", foldouts, distTabButtons, mat, me, distTabAction);

			// Blur
			Dictionary<Action, GUIContent> blurTabButtons = new Dictionary<Action, GUIContent>();
			blurTabButtons.Add(()=>{ResetBlur();}, MGUI.resetLabel);
			Action blurTabAction = ()=>{
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
			};
			Foldouts.Foldout("BLUR", foldouts, blurTabButtons, mat, me, blurTabAction);

			// Noise
			Dictionary<Action, GUIContent> noiseTabButtons = new Dictionary<Action, GUIContent>();
			noiseTabButtons.Add(()=>{ResetNoise();}, MGUI.resetLabel);
			Action noiseTabAction = ()=>{
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
			};
			Foldouts.Foldout("NOISE", foldouts, noiseTabButtons, mat, me, noiseTabAction);

			if (isSFXX){
				Dictionary<Action, GUIContent> zoomTabButtons = new Dictionary<Action, GUIContent>();
				zoomTabButtons.Add(()=>{ResetZoom();}, MGUI.resetLabel);
				Action zoomTabAction = ()=>{
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
				};
				Foldouts.Foldout("ZOOM", foldouts, zoomTabButtons, mat, me, zoomTabAction);
				
				// Screenspace Texture Overlay
				Dictionary<Action, GUIContent> sstTabButtons = new Dictionary<Action, GUIContent>();
				sstTabButtons.Add(()=>{ResetSST();}, MGUI.resetLabel);
				Action sstTabAction = ()=>{
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
				};
				Foldouts.Foldout("IMAGE OVERLAY", foldouts, sstTabButtons, mat, me, sstTabAction);

				// Fog
				Dictionary<Action, GUIContent> fogTabButtons = new Dictionary<Action, GUIContent>();
				fogTabButtons.Add(()=>{ResetFog();}, MGUI.resetLabel);
				Action fogTabAction = ()=>{
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
				};
				Foldouts.Foldout("FOG", foldouts, fogTabButtons, mat, me, fogTabAction);

				// Triplanar Mapping
				Dictionary<Action, GUIContent> triplanarTabButtons = new Dictionary<Action, GUIContent>();
				triplanarTabButtons.Add(()=>{ResetTriplanar();}, MGUI.resetLabel);
				Action triplanarTabAction = ()=>{
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
								MGUI.SpaceN5();
								MGUI.Vector3Field(_TPScroll, "Scroll Speed", false);
								MGUI.Space4();
							}
							me.TexturePropertySingleLine(tpNoiseTexLabel, _TPNoiseTex);
							if (_TPNoiseTex.textureValue){          
								MGUI.TextureSO(me, _TPNoiseTex);
								MGUI.SpaceN5();
								MGUI.Vector3Field(_TPNoiseScroll, "Scroll Speed", false);
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
				};
				Foldouts.Foldout("TRIPLANAR", foldouts, triplanarTabButtons, mat, me, triplanarTabAction);

				// Outline
				Dictionary<Action, GUIContent> outlineTabButtons = new Dictionary<Action, GUIContent>();
				outlineTabButtons.Add(()=>{ResetOutline();}, MGUI.resetLabel);
				Action outlineTabAction = ()=>{
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
								me.ShaderProperty(_SobelClearInner, "Remove Inner Shading");
							}
							else if (_OutlineType.floatValue == 2){
								me.ShaderProperty(_AuraStr, "Thickness");
								me.ShaderProperty(_AuraFade, "Fade");
							}
						});
					}
				};
				Foldouts.Foldout("OUTLINE", foldouts, outlineTabButtons, mat, me, outlineTabAction);

				// Extras
				Dictionary<Action, GUIContent> miscTabButtons = new Dictionary<Action, GUIContent>();
				miscTabButtons.Add(()=>{ResetExtras();}, MGUI.resetLabel);
				Action miscTabAction = ()=>{
					Action letterboxTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_Letterbox.floatValue == 0);
							me.ShaderProperty(_UseZoomFalloff, "Use Zoom Falloff");
							me.ShaderProperty(_LetterboxStr, "Bar Width");
							MGUI.ToggleGroupEnd();
						});
					};
					Foldouts.SubFoldout("Letterbox", foldouts, null, mat, me, letterboxTabAction, _Letterbox);

					Action pulseTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_Pulse.floatValue == 0);
							me.ShaderProperty(_WaveForm, "Waveform");
							MGUI.ToggleGroup(_WaveForm.floatValue == 0);
							me.ShaderProperty(_PulseColor, "Include Filtering");
							me.ShaderProperty(_PulseSpeed, speedLabel);
							MGUI.ToggleGroupEnd();
							MGUI.ToggleGroupEnd();
						});
					};
					Foldouts.SubFoldout("Pulse", foldouts, null, mat, me, pulseTabAction, _Pulse);

					Action uvManipTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_Shift.floatValue == 0);
							me.ShaderProperty(_InvertX, "Invert X");
							me.ShaderProperty(_InvertY, "Invert Y");
							me.ShaderProperty(_ShiftX, "Shift X");
							me.ShaderProperty(_ShiftY, "Shift Y");
							MGUI.ToggleGroupEnd();
						});
					};
					Foldouts.SubFoldout("UV Manipulation", foldouts, null, mat, me, uvManipTabAction, _Shift);

					Action roundingTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_RoundingToggle.floatValue == 0);
							me.ShaderProperty(_RoundingOpacity, "Opacity");
							me.ShaderProperty(_Rounding, "Precision");
							MGUI.ToggleGroupEnd();
						});
					};
					Foldouts.SubFoldout("Rounding", foldouts, null, mat, me, roundingTabAction, _RoundingToggle);

					Action nmTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_NMFToggle.floatValue == 0);
							me.ShaderProperty(_NMFOpacity, "Opacity");
							me.ShaderProperty(_NormalMapFilter, "Strength");
							MGUI.ToggleGroupEnd();
						});
					};
					Foldouts.SubFoldout("Normal Map", foldouts, null, mat, me, nmTabAction, _NMFToggle);

					Action depthTabAction = ()=>{					
						if (_DepthBufferToggle.floatValue == 1){
							MGUI.DisplayInfo("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
						}
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_DepthBufferToggle.floatValue == 0);
							me.ShaderProperty(_DBOpacity, "Opacity");
							me.ShaderProperty(_DBColor, "Tint");
							MGUI.ToggleGroupEnd();
						});
					};
					Foldouts.SubFoldout("Depth Buffer", foldouts, null, mat, me, depthTabAction, _DepthBufferToggle);
				};
				Foldouts.Foldout("MISC", foldouts, miscTabButtons, mat, me, miscTabAction);
			}

			// Audio Link
			Dictionary<Action, GUIContent> audioLinkTabButtons = new Dictionary<Action, GUIContent>();
			audioLinkTabButtons.Add(()=>{ResetAudioLink();}, MGUI.resetLabel);
			Action audioLinkTabAction = ()=>{
				MGUI.ToggleSlider(me, Tips.audioLinkEmission, _AudioLinkToggle, _AudioLinkStrength);
				MGUI.ToggleGroup(_AudioLinkToggle.floatValue == 0);
				MGUI.SliderMinMax(_AudioLinkMin, _AudioLinkMax, 0f, 2f, "Remap", 1);
				MGUI.Space6();

				Dictionary<Action, GUIContent> alFilteringTabButtons = new Dictionary<Action, GUIContent>();
				alFilteringTabButtons.Add(()=>{ResetALFiltering();}, MGUI.resetLabel);
				Action alFilteringTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_AudioLinkFilteringBand, "Band");
						me.ShaderProperty(_AudioLinkFilteringStrength, "Strength");
						MGUI.SliderMinMax(_AudioLinkFilteringMin, _AudioLinkFilteringMax, 0f, 2f, "Remap", 1);
					});
				};
				Foldouts.SubFoldout("Filtering 1", foldouts, alFilteringTabButtons, mat, me, alFilteringTabAction);

				Dictionary<Action, GUIContent> alShakeTabButtons = new Dictionary<Action, GUIContent>();
				alShakeTabButtons.Add(()=>{ResetALShake();}, MGUI.resetLabel);
				Action alShakeTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_AudioLinkShakeBand, "Band");
						me.ShaderProperty(_AudioLinkShakeStrength, "Strength");
						MGUI.SliderMinMax(_AudioLinkShakeMin, _AudioLinkShakeMax, 0f, 2f, "Remap", 1);
					});
				};
				Foldouts.SubFoldout("Shake 1", foldouts, alShakeTabButtons, mat, me, alShakeTabAction);

				Dictionary<Action, GUIContent> alDistortionTabButtons = new Dictionary<Action, GUIContent>();
				alDistortionTabButtons.Add(()=>{ResetALDistortion();}, MGUI.resetLabel);
				Action alDistortionTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_AudioLinkDistortionBand, "Band");
						me.ShaderProperty(_AudioLinkDistortionStrength, "Strength");
						MGUI.SliderMinMax(_AudioLinkDistortionMin, _AudioLinkDistortionMax, 0f, 2f, "Remap", 1);
					});
				};
				Foldouts.SubFoldout("Distortion 1", foldouts, alDistortionTabButtons, mat, me, alDistortionTabAction);

				Dictionary<Action, GUIContent> alBlurTabButtons = new Dictionary<Action, GUIContent>();
				alBlurTabButtons.Add(()=>{ResetALBlur();}, MGUI.resetLabel);
				Action alBlurTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_AudioLinkBlurBand, "Band");
						me.ShaderProperty(_AudioLinkBlurStrength, "Strength");
						MGUI.SliderMinMax(_AudioLinkBlurMin, _AudioLinkBlurMax, 0f, 2f, "Remap", 1);
					});
				};
				Foldouts.SubFoldout("Blur 1", foldouts, alBlurTabButtons, mat, me, alBlurTabAction);

				Dictionary<Action, GUIContent> alNoiseTabButtons = new Dictionary<Action, GUIContent>();
				alNoiseTabButtons.Add(()=>{ResetALNoise();}, MGUI.resetLabel);
				Action alNoiseTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_AudioLinkNoiseBand, "Band");
						me.ShaderProperty(_AudioLinkNoiseStrength, "Strength");
						MGUI.SliderMinMax(_AudioLinkNoiseMin, _AudioLinkNoiseMax, 0f, 2f, "Remap", 1);
					});
				};
				Foldouts.SubFoldout("Noise 1", foldouts, alNoiseTabButtons, mat, me, alNoiseTabAction);

				if (isSFXX){
					Dictionary<Action, GUIContent> alZoomTabButtons = new Dictionary<Action, GUIContent>();
					alZoomTabButtons.Add(()=>{ResetALZoom();}, MGUI.resetLabel);
					Action alZoomTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_AudioLinkZoomBand, "Band");
							me.ShaderProperty(_AudioLinkZoomStrength, "Strength");
							MGUI.SliderMinMax(_AudioLinkZoomMin, _AudioLinkZoomMax, 0f, 2f, "Remap", 1);
						});
					};
					Foldouts.SubFoldout("Zoom 1", foldouts, alZoomTabButtons, mat, me, alZoomTabAction);

					Dictionary<Action, GUIContent> alSSTTabButtons = new Dictionary<Action, GUIContent>();
					alSSTTabButtons.Add(()=>{ResetALSST();}, MGUI.resetLabel);
					Action alSSTTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_AudioLinkSSTBand, "Band");
							me.ShaderProperty(_AudioLinkSSTStrength, "Strength");
							MGUI.SliderMinMax(_AudioLinkSSTMin, _AudioLinkSSTMax, 0f, 2f, "Remap", 1);
						});
					};
					Foldouts.SubFoldout("Image Overlay 1", foldouts, alSSTTabButtons, mat, me, alSSTTabAction);

					Dictionary<Action, GUIContent> alFogTabButtons = new Dictionary<Action, GUIContent>();
					alFogTabButtons.Add(()=>{ResetALFog();}, MGUI.resetLabel);
					Action alFogTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_AudioLinkFogBand, "Band");
							me.ShaderProperty(_AudioLinkFogOpacity, "Opacity");
							me.ShaderProperty(_AudioLinkFogRadius, "Radius");
							MGUI.SliderMinMax(_AudioLinkFogMin, _AudioLinkFogMax, 0f, 2f, "Remap", 1);
						});
					};
					Foldouts.SubFoldout("Fog 1", foldouts, alFogTabButtons, mat, me, alFogTabAction);

					Dictionary<Action, GUIContent> alTriplanarTabButtons = new Dictionary<Action, GUIContent>();
					alTriplanarTabButtons.Add(()=>{ResetALTriplanar();}, MGUI.resetLabel);
					Action alTriplanarTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_AudioLinkTriplanarBand, "Band");
							me.ShaderProperty(_AudioLinkTriplanarOpacity, "Opacity");
							me.ShaderProperty(_AudioLinkTriplanarRadius, "Radius");
							MGUI.SliderMinMax(_AudioLinkTriplanarMin, _AudioLinkTriplanarMax, 0f, 2f, "Remap", 1);
						});
					};
					Foldouts.SubFoldout("Triplanar 1", foldouts, alTriplanarTabButtons, mat, me, alTriplanarTabAction);

					Dictionary<Action, GUIContent> alOutlineTabButtons = new Dictionary<Action, GUIContent>();
					alOutlineTabButtons.Add(()=>{ResetALOutline();}, MGUI.resetLabel);
					Action alOutlineTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_AudioLinkOutlineBand, "Band");
							me.ShaderProperty(_AudioLinkOutlineStrength, "Strength");
							MGUI.SliderMinMax(_AudioLinkOutlineMin, _AudioLinkOutlineMax, 0f, 2f, "Remap", 1);
						});
					};
					Foldouts.SubFoldout("Outline 1", foldouts, alOutlineTabButtons, mat, me, alOutlineTabAction);

					Dictionary<Action, GUIContent> alMiscTabButtons = new Dictionary<Action, GUIContent>();
					alMiscTabButtons.Add(()=>{ResetALMisc();}, MGUI.resetLabel);
					Action alMiscTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_AudioLinkMiscBand, "Band");
							me.ShaderProperty(_AudioLinkMiscStrength, "Strength");
							MGUI.SliderMinMax(_AudioLinkMiscMin, _AudioLinkMiscMax, 0f, 2f, "Remap", 1);
						});
					};
					Foldouts.SubFoldout("Misc 1", foldouts, alMiscTabButtons, mat, me, alMiscTabAction);
				}
				MGUI.ToggleGroupEnd();
			};
			Foldouts.Foldout("AUDIO LINK", foldouts, audioLinkTabButtons, mat, me, audioLinkTabAction);
		}
		MGUI.DoFooter(versionLabel);
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
		int audioLink = mat.GetInt("_AudioLinkToggle");

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
		bool audioLinkEnabled = audioLink == 1;
		
		SetKeyword(mat, "_COLOR_ON", filteringEnabled);
		SetKeyword(mat, "_SHAKE_ON", shakeEnabled);
		SetKeyword(mat, "_DISTORTION_ON", distortionEnabled);
		SetKeyword(mat, "_DISTORTION_WORLD_ON", distortionTriEnabled);
		SetKeyword(mat, "_BLUR_PIXEL_ON", blurPixelEnabled);
		SetKeyword(mat, "_BLUR_DITHER_ON", blurDitherEnabled);
		SetKeyword(mat, "_BLUR_RADIAL_ON", blurRadEnabled);
		SetKeyword(mat, "_BLUR_Y_ON", blurYEnabled);
		SetKeyword(mat, "_CHROMATIC_ABBERATION_ON", blurChromEnabled);
		SetKeyword(mat, "_DOF_ON", dofEnabled);
		SetKeyword(mat, "_ZOOM_ON", zoomEnabled);
		SetKeyword(mat, "_ZOOM_RGB_ON", zoomRGBEnabled);
		SetKeyword(mat, "_IMAGE_OVERLAY_ON", sstEnabled);
		SetKeyword(mat, "_IMAGE_OVERLAY_DISTORTION_ON", sstDistEnabled);
		SetKeyword(mat, "_FOG_ON", fogEnabled);
		SetKeyword(mat, "_TRIPLANAR_ON", tpEnabled);
		SetKeyword(mat, "_OUTLINE_ON", outlineEnabled);
		SetKeyword(mat, "_NOISE_ON", noiseEnabled);
		SetKeyword(mat, "_AUDIOLINK_ON", audioLinkEnabled);

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
		_SSTScale.floatValue = 2f;
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
		_SobelClearInner.floatValue = 1f;
	}

	void ResetAudioLink(){
		_AudioLinkToggle.floatValue = 0f;
		_AudioLinkStrength.floatValue = 0f;
		_AudioLinkMin.floatValue = 0f;
		_AudioLinkMax.floatValue = 1f;
		ResetALFiltering();
		ResetALShake();
		ResetALDistortion();
		ResetALBlur();
		ResetALNoise();
		ResetALZoom();
		ResetALSST();
		ResetALFog();
		ResetALTriplanar();
		ResetALOutline();
		ResetALMisc();
	}

	void ResetALFiltering(){
		_AudioLinkFilteringStrength.floatValue = 0f;
		_AudioLinkFilteringBand.floatValue = 0f;
		_AudioLinkFilteringMin.floatValue = 0f;
		_AudioLinkFilteringMax.floatValue = 1f;
	}

	void ResetALShake(){
		_AudioLinkShakeStrength.floatValue = 0f;
		_AudioLinkShakeBand.floatValue = 0f;
		_AudioLinkShakeMin.floatValue = 0f;
		_AudioLinkShakeMax.floatValue = 1f;
	}

	void ResetALDistortion(){
		_AudioLinkDistortionStrength.floatValue = 0f;
		_AudioLinkDistortionBand.floatValue = 0f;
		_AudioLinkDistortionMin.floatValue = 0f;
		_AudioLinkDistortionMax.floatValue = 1f;
	}

	void ResetALBlur(){
		_AudioLinkBlurStrength.floatValue = 0f;
		_AudioLinkBlurBand.floatValue = 0f;
		_AudioLinkBlurMin.floatValue = 0f;
		_AudioLinkBlurMax.floatValue = 1f;
	}

	void ResetALNoise(){
		_AudioLinkNoiseStrength.floatValue = 0f;
		_AudioLinkNoiseBand.floatValue = 0f;
		_AudioLinkNoiseMin.floatValue = 0f;
		_AudioLinkNoiseMax.floatValue = 1f;
	}

	void ResetALZoom(){
		_AudioLinkZoomStrength.floatValue = 0f;
		_AudioLinkZoomBand.floatValue = 0f;
		_AudioLinkZoomMin.floatValue = 0f;
		_AudioLinkZoomMax.floatValue = 1f;
	}

	void ResetALSST(){
		_AudioLinkSSTStrength.floatValue = 0f;
		_AudioLinkSSTBand.floatValue = 0f;
		_AudioLinkSSTMin.floatValue = 0f;
		_AudioLinkSSTMax.floatValue = 1f;
	}

	void ResetALFog(){
		_AudioLinkFogOpacity.floatValue = 0f;
		_AudioLinkFogRadius.floatValue = 0f;
		_AudioLinkFogBand.floatValue = 0f;
		_AudioLinkFogMin.floatValue = 0f;
		_AudioLinkFogMax.floatValue = 1f;
	}

	void ResetALTriplanar(){
		_AudioLinkTriplanarOpacity.floatValue = 0f;
		_AudioLinkTriplanarBand.floatValue = 0f;
		_AudioLinkTriplanarMin.floatValue = 0f;
		_AudioLinkTriplanarMax.floatValue = 1f;
		_AudioLinkTriplanarRadius.floatValue = 0f;
	}

	void ResetALOutline(){
		_AudioLinkOutlineStrength.floatValue = 0f;
		_AudioLinkOutlineBand.floatValue = 0f;
		_AudioLinkOutlineMin.floatValue = 0f;
		_AudioLinkOutlineMax.floatValue = 1f;
	}

	void ResetALMisc(){
		_AudioLinkMiscStrength.floatValue = 0f;
		_AudioLinkMiscBand.floatValue = 0f;
		_AudioLinkMiscMin.floatValue = 0f;
		_AudioLinkMiscMax.floatValue = 1f;
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