using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

public class SFXEditor : ShaderGUI {

    public enum BlendingModes {OPAQUE, ALPHA, PREMULTIPLIED, ADDITIVE, SOFT_ADDITIVE, MULTIPLY, MULTIPLY_2x}

    // GUIContent instead of string for TexturePropertySingleLine
    GUIContent screenTexLabel = new GUIContent("Texture");
    GUIContent shakeNoiseTexLabel = new GUIContent("Noise Texture");
    GUIContent normalMapLabel = new GUIContent("Normal Map");
    GUIContent tpTexLabel = new GUIContent("Texture");
    GUIContent tpNoiseTexLabel = new GUIContent("Noise Texture");
    
	public static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
	public static Dictionary<string, float> presetDict = new Dictionary<string, float>();
	public static List<string> presetsList = new List<string>();
	public static string[] presets;
	
	int popupIndex = -1;
	string presetText = "";
	string dirPath = "Assets/Mochie/Unity/Presets/ScreenFX/";
	
    Toggles toggles = new Toggles(
		new string[] {
			"GENERAL",
			"COLOR",
			"SHAKE",
			"DISTORTION",
			"BLUR",
			"ZOOM",
			"IMAGE OVERLAY",
			"FOG",
			"TRIPLANAR",
			"OUTLINE",
			"MISC",
			"Framebuffer",
			"Letterbox",
			"Deep Fry",
			"Pulse",
			"UV Manipulation",
			"Rounding",
			"Normal Map",
			"Depth Buffer",
			"Safe Zone",
			"PRESETS"
		}
	);

    // Texture file names
	string header = "SFXHeader_Pro";
	string watermark = "Watermark_Pro";
	string patIcon = "Patreon_Icon";

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
    MaterialProperty _ColorUseGlobal = null;
    MaterialProperty _ColorMinRange = null;
    MaterialProperty _ColorMaxRange = null;
    MaterialProperty _Color = null;
    MaterialProperty _SaturationRGB = null;
    MaterialProperty _Exposure = null;
    MaterialProperty _Contrast = null;
    MaterialProperty _HDR = null;
    MaterialProperty _Invert = null;
    MaterialProperty _InvertR = null;
    MaterialProperty _InvertG = null;
    MaterialProperty _InvertB = null;
    MaterialProperty _Noise = null;
    MaterialProperty _AutoShift = null;
    MaterialProperty _AutoShiftSpeed = null;
    MaterialProperty _Hue = null;
    MaterialProperty _SaturationHSL = null;
    MaterialProperty _Luminance = null;
    MaterialProperty _HSLMin = null;
    MaterialProperty _HSLMax = null;
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
//    MaterialProperty _OutlineThiccN = null;
	MaterialProperty _GhostingToggle = null;
	MaterialProperty _GhostingStr = null;
	MaterialProperty _FreezeFrame = null;
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

    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    MaterialEditor m_MaterialEditor;
    public override void OnGUI(MaterialEditor me, MaterialProperty[] props) {
		Material mat = (Material)me.target;

        if (!me.isVisible)
            return;
        foreach (var property in GetType().GetFields(bindingFlags)){
            if (property.FieldType == typeof(MaterialProperty))
                property.SetValue(this, FindProperty(property.Name, props));
        }

		// Generate preset popup items (and folders if necessary)
		if (!AssetDatabase.IsValidFolder(MGUI.parentPath))
			AssetDatabase.CreateFolder(MGUI.presetPath, "Presets");
		if (!AssetDatabase.IsValidFolder(MGUI.parentPath+"/ScreenFX"))
			AssetDatabase.CreateFolder(MGUI.parentPath, "ScreenFX");
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


		bool isSFXX = MGUI.IsXVersion(mat);

		if (isSFXX){
			header = "SFXHeaderX_Pro";
			if (!EditorGUIUtility.isProSkin){
				header = "SFXHeaderX";
				watermark = "Watermark";
			}
		}
		else {
			header = "SFXHeader_Pro";
			if (!EditorGUIUtility.isProSkin){
				header = "SFXHeader";
				watermark = "Watermark";
			}
		}

		// Add mat to foldout dictionary if it isn't in there yet
		
		if (!foldouts.ContainsKey(mat))
			foldouts.Add(mat, toggles);

        Texture2D headerTex = (Texture2D)Resources.Load(header, typeof(Texture2D));
		Texture2D watermarkTex = (Texture2D)Resources.Load(watermark, typeof(Texture2D));
		Texture2D patIconTex = (Texture2D)Resources.Load(patIcon, typeof(Texture2D));
		Texture2D resetIconTex = (Texture2D)Resources.Load("ResetIcon", typeof(Texture2D));
		Texture2D collapseIconTex = (Texture2D)Resources.Load("CollapseIcon", typeof(Texture2D));
		Texture2D expandIconTex = (Texture2D)Resources.Load("ExpandIcon", typeof(Texture2D));
		Texture2D collapseIcon = (Texture2D)Resources.Load("CollapseIcon", typeof(Texture2D));

        MGUI.CenteredTexture(headerTex, 0, 0);
		MGUI.Space4();
        EditorGUI.BeginChangeCheck(); {
			
			mat.SetShaderPassEnabled("Always", _GhostingToggle.floatValue == 1 || _FreezeFrame.floatValue == 1);

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
                if (_BlendMode.floatValue > 0)
                    me.ShaderProperty(_Opacity, "Opacity");
				
				GUILayout.Label("Global Falloff", EditorStyles.boldLabel);
				MGUI.SpaceN2();
                me.ShaderProperty(_MinRange, minLabel);
                me.ShaderProperty(_MaxRange, maxLabel);
                MGUI.Space8();
            }

            // Color Filtering
			bool colorTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "COLOR");
			if (MGUI.TabButton(resetIconTex, 26f))
				ResetColor();
			MGUI.Space8();
			if (colorTab){
				MGUI.Space4();
				me.ShaderProperty(_FilterModel, modeLabel);
				if (_FilterModel.floatValue > 0){
					MGUI.Space6();
					me.ShaderProperty(_ColorUseGlobal, ugfLabel);
					if (_ColorUseGlobal.floatValue == 0){
						me.ShaderProperty(_ColorMinRange, minLabel, 1);
						me.ShaderProperty(_ColorMaxRange, maxLabel, 1);
					}
					MGUI.Space6();
				}
				switch((int)_FilterModel.floatValue){
					case 1: 
						me.ShaderProperty(_Color, colorLabel);
						break;
					case 2: 
						me.ShaderProperty(_AutoShift, "Auto Shift");
						if (_AutoShift.floatValue ==1)
							me.ShaderProperty(_AutoShiftSpeed, speedLabel);
						else
							me.ShaderProperty(_Hue, "Hue");
						me.ShaderProperty(_SaturationHSL, "Saturation");
						me.ShaderProperty(_Luminance, "Luminance");
						me.ShaderProperty(_HSLMin, "Min Threshold");
						me.ShaderProperty(_HSLMax, "Max Threshold");
						break;
					default: break;
				}
				if (_FilterModel.floatValue > 0){
					MGUI.Space8();
					//ToggleSlider(me, "Sobel", _Sobel, _SobelStr);
					me.ShaderProperty(_HDR, "HDR");
					me.ShaderProperty(_Contrast, "Contrast");
					me.ShaderProperty(_Exposure, "Exposure");
					if (_FilterModel.floatValue != 3)
						me.ShaderProperty(_SaturationRGB, "Saturation");
					me.ShaderProperty(_Noise, "Noise");
					me.ShaderProperty(_Invert, "Invert");
					me.ShaderProperty(_InvertR, "Red Inversion", 1);
					me.ShaderProperty(_InvertG, "Green Inversion", 1);
					me.ShaderProperty(_InvertB, "Blue Inversion", 1);
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
                        me.ShaderProperty(_ShakeMinRange, minLabel, 1);
                        me.ShaderProperty(_ShakeMaxRange, maxLabel, 1);
                    }
					MGUI.Space6();
                }
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
                        me.ShaderProperty(_DistortionMinRange, minLabel, 1);
                        me.ShaderProperty(_DistortionMaxRange, maxLabel, 1);
                    }
					MGUI.Space6();
                    if (_DistortionModel.floatValue == 1 || _DistortionModel.floatValue == 2){
                        me.TexturePropertySingleLine(normalMapLabel, _NormalMap);
                        me.TextureScaleOffsetProperty(_NormalMap);
                    }
                    me.ShaderProperty(_DistortionStr, strengthLabel);
                    me.ShaderProperty(_DistortionSpeed, speedLabel);
                    if (_DistortionModel.floatValue == 2){
                        GUILayout.Label("Triplanar", EditorStyles.boldLabel);
						MGUI.DisplayWarning("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
                        me.ShaderProperty(_DistortionRadius, radiusLabel);
                        me.ShaderProperty(_DistortionFade, fadeLabel);
                        me.ShaderProperty(_DistortionP2O, p2oLabel);
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
						me.ShaderProperty(_BlurMinRange, minLabel, 1);
						me.ShaderProperty(_BlurMaxRange, maxLabel, 1);
					}
					MGUI.Space6();
					me.ShaderProperty(_BlurOpacity, "Opacity");
					me.ShaderProperty(_BlurStr, strengthLabel);
					if (_BlurModel.floatValue == 3){
						me.ShaderProperty(_BlurSamples, "Sample Count");
						me.ShaderProperty(_BlurRadius, radiusLabel);
					}
					me.ShaderProperty(_PixelationStr, "Pixelation");
					me.ShaderProperty(_RippleGridStr, "Ripple Grid");
					if (_BlurModel.floatValue != 3){
						me.ShaderProperty(_DoF, "Depth of Field");
						if (_DoF.floatValue == 1){
							MGUI.DisplayWarning("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
							me.ShaderProperty(_DoFRadius, radiusLabel, 1);
							me.ShaderProperty(_DoFFade, fadeLabel, 1);
							me.ShaderProperty(_DoFP2O, p2oLabel, 1);
							MGUI.Space4();
						}
						me.ShaderProperty(_BlurY, "Y Axis Only");
						me.ShaderProperty(_RGBSplit, "Chrom. Abberation");
					}
					if (_BlurModel.floatValue == 1)
						me.ShaderProperty(_CrushBlur, "Crush");
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
							me.ShaderProperty(_ZoomMinRange, minLabel, 1);
							me.ShaderProperty(_ZoomMaxRange, maxLabel, 1);
						}
						MGUI.Space6();
						if (_Zoom.floatValue == 2){
							me.ShaderProperty(_ZoomStrR, "Red");
							me.ShaderProperty(_ZoomStrG, "Green");
							me.ShaderProperty(_ZoomStrB, "Blue");
						}
						else me.ShaderProperty(_ZoomStr, strengthLabel);
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
							me.ShaderProperty(_SSTMinRange, minLabel, 1);
							me.ShaderProperty(_SSTMaxRange, maxLabel, 1);
						}
						MGUI.Space6();
						if (_SST.floatValue != 3){
							me.TexturePropertySingleLine(screenTexLabel, _ScreenTex, _SSTColor, _SSTBlend);
						}
						else
							me.TexturePropertySingleLine(normalMapLabel, _ScreenTex, _SSTAnimatedDist);
						if (_SST.floatValue > 1){
							me.ShaderProperty(_SSTColumnsX, "Columns (X)");
							me.ShaderProperty(_SSTRowsY, "Rows (Y)");
							MGUI.CustomToggleSlider("Frame", _ManualScrub, _ScrubPos, 0f, (_SSTColumnsX.floatValue*_SSTRowsY.floatValue)-1);
							MGUI.ToggleGroup(_ManualScrub.floatValue == 1);
							me.ShaderProperty(_SSTAnimationSpeed, "FPS");
							MGUI.ToggleGroupEnd();
						}
						GUILayout.Label(new GUIContent("UVs"), EditorStyles.boldLabel);
						me.ShaderProperty(_SSTScale, "Scale");
						me.ShaderProperty(_SSTWidth, "Width");
						me.ShaderProperty(_SSTHeight, "Height");
						me.ShaderProperty(_SSTLR, "Left / Right");
						me.ShaderProperty(_SSTUD, "Down / Up");
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
						MGUI.DisplayWarning("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
						MGUI.Space6();
						me.ShaderProperty(_FogUseGlobal, ugfLabel);
						if (_FogUseGlobal.floatValue == 0){
							me.ShaderProperty(_FogMinRange, minLabel, 1);
							me.ShaderProperty(_FogMaxRange, maxLabel, 1);
						}
						MGUI.Space6();
						me.ShaderProperty(_FogColor, colorLabel);
						me.ShaderProperty(_FogRadius, radiusLabel);
						me.ShaderProperty(_FogFade, fadeLabel);
						me.ShaderProperty(_FogP2O, p2oLabel);
						
						if (Foldouts.DoMediumFoldout(foldouts, mat, me, _FogSafeZone, 1, "Safe Zone")){
							MGUI.Space6();
							MGUI.ToggleGroup(_FogSafeZone.floatValue == 0);
							me.ShaderProperty(_FogSafeRadius, "Vision Radius");
							me.ShaderProperty(_FogSafeMaxRange, "Outer Perimeter");
							me.ShaderProperty(_FogSafeOpacity, "Opacity");
							MGUI.ToggleGroupEnd();
						}
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
						MGUI.DisplayWarning("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
						MGUI.Space6();
						me.ShaderProperty(_TPUseGlobal, ugfLabel);
						if (_TPUseGlobal.floatValue == 0){
							me.ShaderProperty(_TPMinRange, minLabel, 1);
							me.ShaderProperty(_TPMaxRange, maxLabel, 1);
						}
						MGUI.Space6();
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
						MGUI.Space8();
						me.ShaderProperty(_TPRadius, radiusLabel);
						if (_Triplanar.floatValue == 2){
							me.ShaderProperty(_TPScanFade, fadeLabel);
							me.ShaderProperty(_TPThickness, "Thickness");
							me.ShaderProperty(_TPNoise, "Noise");
						}
						else me.ShaderProperty(_TPFade, fadeLabel);
						me.ShaderProperty(_TPP2O, p2oLabel);
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
						MGUI.DisplayWarning("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
						MGUI.Space6();
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
					if (Foldouts.DoMediumFoldout(foldouts, mat, me, 0, "Framebuffer")){
						// if (_FreezeFrame.floatValue == 1 && _GhostingToggle.floatValue == 1){
						// 	_FreezeFrame.floatValue = 0;
						// 	_GhostingToggle.floatValue = 0;
						// }
						// me.ShaderProperty(_FreezeFrame, "Freeze Frame");
						// MGUI.ToggleSlider(me, "Ghosting", _GhostingToggle, _GhostingStr);
						MGUI.FramebufferSection(me, new MaterialProperty[] {_FreezeFrame, _GhostingToggle}, _GhostingStr);
						MGUI.Space6();
					}
					else MGUI.SpaceN2();

					if (Foldouts.DoMediumFoldout(foldouts, mat, me, _Letterbox, 0, "Letterbox")){
						MGUI.Space6();
						MGUI.ToggleGroup(_Letterbox.floatValue == 0);
						me.ShaderProperty(_UseZoomFalloff, "Use Zoom Falloff");
						me.ShaderProperty(_LetterboxStr, "Bar Width");
						MGUI.ToggleGroupEnd();
						MGUI.Space6();
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
						MGUI.Space6();
						MGUI.ToggleGroup(_Pulse.floatValue == 0);
						me.ShaderProperty(_WaveForm, "Waveform");
						MGUI.ToggleGroup(_WaveForm.floatValue == 0);
						me.ShaderProperty(_PulseColor, "Include Color");
						me.ShaderProperty(_PulseSpeed, speedLabel);
						MGUI.ToggleGroupEnd();
						MGUI.ToggleGroupEnd();
						MGUI.Space6();
					}
					else MGUI.SpaceN4();

					if (Foldouts.DoMediumFoldout(foldouts, mat, me, _Shift, 0, "UV Manipulation")){
						MGUI.Space6();
						MGUI.ToggleGroup(_Shift.floatValue == 0);
						me.ShaderProperty(_InvertX, "Invert X");
						me.ShaderProperty(_InvertY, "Invert Y");
						me.ShaderProperty(_ShiftX, "Shift X");
						me.ShaderProperty(_ShiftY, "Shift Y");
						MGUI.ToggleGroupEnd();
						MGUI.Space6();
					}
					else MGUI.SpaceN4();

					if (Foldouts.DoMediumFoldout(foldouts, mat, me, _RoundingToggle, 0, "Rounding")){
						MGUI.Space6();
						MGUI.ToggleGroup(_RoundingToggle.floatValue == 0);
						me.ShaderProperty(_RoundingOpacity, "Opacity");
						me.ShaderProperty(_Rounding, "Precision");
						MGUI.ToggleGroupEnd();
						MGUI.Space6();
					}
					else MGUI.SpaceN4();

					if (Foldouts.DoMediumFoldout(foldouts, mat, me, _NMFToggle, 0, "Normal Map")){
						MGUI.Space6();
						MGUI.ToggleGroup(_NMFToggle.floatValue == 0);
						me.ShaderProperty(_NMFOpacity, "Opacity");
						me.ShaderProperty(_NormalMapFilter, "Strength");
						MGUI.ToggleGroupEnd();
						MGUI.Space6();
					}
					else MGUI.SpaceN4();
					
					if (Foldouts.DoMediumFoldout(foldouts, mat, me, _DepthBufferToggle, 0, "Depth Buffer")){
						MGUI.Space6();
						if (_DepthBufferToggle.floatValue == 1){
							MGUI.DisplayWarning("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
							MGUI.Space6();
						}
						MGUI.ToggleGroup(_DepthBufferToggle.floatValue == 0);
						me.ShaderProperty(_DBOpacity, "Opacity");
						me.ShaderProperty(_DBColor, "Tint");
						MGUI.ToggleGroupEnd();
						MGUI.Space6();
					}
					else MGUI.SpaceN2();
					MGUI.Space6();
				}
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

    // Set blending mode
    public static void SetBlendMode(Material material, BlendingModes mode) {
        EditorGUI.BeginChangeCheck();
        switch (mode) {
             case BlendingModes.OPAQUE:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                break;
             case BlendingModes.ALPHA:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                break;
            case BlendingModes.PREMULTIPLIED:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                break;
            case BlendingModes.ADDITIVE:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                break;
            case BlendingModes.SOFT_ADDITIVE:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                break;
            case BlendingModes.MULTIPLY:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                break;
            case BlendingModes.MULTIPLY_2x:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
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
		_SaturationRGB.floatValue = 1f;
		_AutoShiftSpeed.floatValue = 0.25f;
		_Hue.floatValue = 0f;
		_SaturationHSL.floatValue = 0f;
		_Luminance.floatValue = 0f;
		_HSLMin.floatValue = 0f;
		_HSLMax.floatValue = 1f;
		_HDR.floatValue = 0f;
		_Exposure.floatValue = 0f;
		_Contrast.floatValue = 1f;
		_Invert.floatValue = 0f;
		_InvertR.floatValue = 0f;
		_InvertG.floatValue = 0f;
		_InvertB.floatValue = 0f;
		_Noise.floatValue = 0f;
	}

	void ResetShake(){
		_ShakeModel.floatValue = 0f;
		_ShakeUseGlobal.floatValue = 0f;
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
		_GhostingToggle.floatValue = 0f;
		_GhostingStr.floatValue = 0.7f;
		_FreezeFrame.floatValue = 0f;
		_RoundingToggle.floatValue = 0f;
		_Rounding.floatValue = 1f;
		_RoundingOpacity.floatValue = 1f;
		_NormalMapFilter.floatValue = 0.74f;
		_NMFToggle.floatValue = 0f;
		_NMFOpacity.floatValue = 1f;
		_DBColor.colorValue = Color.white;
	}
}