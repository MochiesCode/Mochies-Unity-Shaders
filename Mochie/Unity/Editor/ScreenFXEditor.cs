using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using Mochie;

namespace Mochie {

    public class ScreenFXEditor : ShaderGUI {

        public enum BlendingModes {Opaque, Alpha, Premultiplied, Additive, Soft_Additive, Multiply, Multiply_2x}

        GUIContent screenTexLabel = new GUIContent("Texture");
        GUIContent shakeNoiseTexLabel = new GUIContent("Noise Texture");
        GUIContent normalMapLabel = new GUIContent("Normal Map");
        GUIContent tpTexLabel = new GUIContent("Texture");
        GUIContent tpNoiseTexLabel = new GUIContent("Noise Texture");
        
        public static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();

        Toggles toggles = new Toggles(new string[] {
                "General",
                "Filtering",
                "Filtering ",
                "Shake",
                "Shake ",
                "Distortion",
                "Distortion ",
                "Blur",
                "Blur ",
                "Zoom",
                "Zoom ",
                "Image Overlay",
                "Fog",
                "Fog ",
                "Triplanar",
                "Triplanar ",
                "Outline",
                "Outline ",
                "Misc",
                "Misc ",
                "Letterbox",
                "Deep Fry",
                "Pulse",
                "UV Manipulation",
                "Rounding",
                "Normal Map Filter",
                "Depth Buffer",
                "Safe Zone",
                "Noise",
                "Noise ",
                "Audio Link",
                "Image Overlay ",
                "Sobel Filter",
        }, 0);

        string versionLabel = "v1.21";
        
        // Commonly used strings
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
        // MaterialProperty _MirrorRenderMode = null;
        MaterialProperty _CameraRenderMode = null;
        MaterialProperty _BlendMode = null;
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
        MaterialProperty _SaturationR = null;
        MaterialProperty _SaturationB = null;
        MaterialProperty _SaturationG = null;
        MaterialProperty _ClampToggle = null;
        MaterialProperty _ClampMax = null;
        MaterialProperty _HueMode = null;
        MaterialProperty _MonoTint = null;

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
        // MaterialProperty _FogHeightMin = null;
        // MaterialProperty _FogHeightMax = null;
        // MaterialProperty _HeightFalloff = null;

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
        MaterialProperty _AudioLinkOutlineColBand = null;
        MaterialProperty _AudioLinkOutlineColStrength = null;
        MaterialProperty _AudioLinkOutlineColMin = null;
        MaterialProperty _AudioLinkOutlineColMax = null;
        MaterialProperty _AudioLinkOutlineMixBand = null;
        MaterialProperty _AudioLinkOutlineMixStrength = null;
        MaterialProperty _AudioLinkOutlineMixMin = null;
        MaterialProperty _AudioLinkOutlineMixMax = null;
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
        MaterialProperty _OutlineTex = null;
        MaterialProperty _OutlineTexAlt = null;
        MaterialProperty _OutlineTexCoord = null;
        MaterialProperty _OutlineTexCoordAlt = null;
        MaterialProperty _OutlineThresh = null;
        MaterialProperty _OutlineAltEnable = null;
        MaterialProperty _OutlineThreshAlt = null;
        MaterialProperty _OutlineColAlt = null;
        MaterialProperty _BackgroundCol = null;
        MaterialProperty _OutlineType = null;
        MaterialProperty _OutlineThiccS = null;
        MaterialProperty _OLUseGlobal = null;
        MaterialProperty _OLMinRange = null;
        MaterialProperty _OLMaxRange = null;
        MaterialProperty _OutlineCube = null;
        MaterialProperty _OutlineCubeAlt = null;
        MaterialProperty _OutlineBackgroundCube = null;
        MaterialProperty _OutlineCubeToggle = null;
        MaterialProperty _OutlineHueShift = null;
        MaterialProperty _OutlineHueShiftAuto = null;
        MaterialProperty _OutlineHueShiftSpeed = null;
        MaterialProperty _CubeRotate = null;
        MaterialProperty _CubeRotateAlt = null;
        MaterialProperty _CubeRotateBG = null;
        MaterialProperty _AutoRotate = null;
        MaterialProperty _AutoRotateAlt = null;
        MaterialProperty _AutoRotateBG = null;
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
        MaterialProperty _SobelFilterToggle = null;
        MaterialProperty _SobelFilterColor = null;
        MaterialProperty _SobelFilterBackgroundColor = null;
        MaterialProperty _SobelFilterOpacity = null;

        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawGizmo(MeshRenderer meshRenderer, GizmoType gizmoType){
            if (meshRenderer.sharedMaterial != null){
                Material material = meshRenderer.sharedMaterial;
                if (!material.shader.name.Contains("Mochie/Screen FX")) return;
                if (!foldouts.ContainsKey(material)) return;
                if (material.GetFloat("_DisplayGlobalGizmo") > 0){
                    Vector3 position = meshRenderer.transform.position;
                    Toggles toggles = foldouts[material];

                    if (toggles.GetState("General")){
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireSphere(position, material.GetFloat("_MinRange"));
                        Gizmos.color = new Color(0.9f, 0.9f, 0.3f, 1f);
                        Gizmos.DrawWireSphere(position, material.GetFloat("_MaxRange"));
                    }
                    if (toggles.GetState("Filtering")){
                        if (material.GetFloat("_FilterModel") > 0 && material.GetFloat("_ColorUseGlobal") == 0) {
                            Gizmos.color = Color.white;
                            Gizmos.DrawWireSphere(position, material.GetFloat("_ColorMinRange"));
                            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            Gizmos.DrawWireSphere(position, material.GetFloat("_ColorMaxRange"));
                        }
                    }
                    if (toggles.GetState("Shake")){
                        if (material.GetFloat("_ShakeModel") > 0 && material.GetFloat("_ShakeUseGlobal") == 0) {
                            Gizmos.color = Color.white;
                            Gizmos.DrawWireSphere(position, material.GetFloat("_ShakeMinRange"));
                            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            Gizmos.DrawWireSphere(position, material.GetFloat("_ShakeMaxRange"));
                        }
                    }
                    if (toggles.GetState("Distortion")){
                        if (material.GetFloat("_DistortionModel") > 0 && material.GetFloat("_DistortionUseGlobal") == 0) {
                            Gizmos.color = Color.white;
                            Gizmos.DrawWireSphere(position, material.GetFloat("_DistortionMinRange"));
                            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            Gizmos.DrawWireSphere(position, material.GetFloat("_DistortionMaxRange"));
                        }
                    }
                    if (toggles.GetState("Blur")){
                        if (material.GetFloat("_BlurModel") > 0 && material.GetFloat("_BlurUseGlobal") == 0) {
                            Gizmos.color = Color.white;
                            Gizmos.DrawWireSphere(position, material.GetFloat("_BlurMinRange"));
                            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            Gizmos.DrawWireSphere(position, material.GetFloat("_BlurMaxRange"));
                        }
                    }
                    if (toggles.GetState("Noise")){
                        if (material.GetFloat("_NoiseMode") > 0 && material.GetFloat("_NoiseUseGlobal") == 0) {
                            Gizmos.color = Color.white;
                            Gizmos.DrawWireSphere(position, material.GetFloat("_NoiseMinRange"));
                            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            Gizmos.DrawWireSphere(position, material.GetFloat("_NoiseMaxRange"));
                        }
                    }
                    if (toggles.GetState("Zoom")){
                        if (material.GetFloat("_Zoom") > 0 && material.GetFloat("_ZoomUseGlobal") == 0) {
                            Gizmos.color = Color.white;
                            Gizmos.DrawWireSphere(position, material.GetFloat("_ZoomMinRange"));
                            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            Gizmos.DrawWireSphere(position, material.GetFloat("_ZoomMaxRange"));
                        }
                    }
                    if (toggles.GetState("Image Overlay")){
                        if (material.GetFloat("_SST") > 0 && material.GetFloat("_SSTUseGlobal") == 0) {
                            Gizmos.color = Color.white;
                            Gizmos.DrawWireSphere(position, material.GetFloat("_SSTMinRange"));
                            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            Gizmos.DrawWireSphere(position, material.GetFloat("_SSTMaxRange"));
                        }
                    }
                    if (toggles.GetState("Fog")){
                        if (material.GetFloat("_Fog") > 0 && material.GetFloat("_FogUseGlobal") == 0) {
                            Gizmos.color = Color.white;
                            Gizmos.DrawWireSphere(position, material.GetFloat("_FogMinRange"));
                            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            Gizmos.DrawWireSphere(position, material.GetFloat("_FogMaxRange"));
                        }
                    }
                    if (toggles.GetState("Triplanar")){
                        if (material.GetFloat("_Triplanar") > 0 && material.GetFloat("_TPUseGlobal") == 0) {
                            Gizmos.color = Color.white;
                            Gizmos.DrawWireSphere(position, material.GetFloat("_TPMinRange"));
                            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            Gizmos.DrawWireSphere(position, material.GetFloat("_TPMaxRange"));
                        }
                    }
                    if (toggles.GetState("Outline")){
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

            // Add mat to foldout dictionary if it isn't in there yet
            if (!foldouts.ContainsKey(mat))
                foldouts.Add(mat, toggles);

            ApplyMaterialSettings(mat);
            SetBlendMode(mat);

            string headerText = "SCREEN FX";
            if (isSFXX) headerText += " X";
            MGUI.DoHeader(headerText);

            EditorGUI.BeginChangeCheck(); {

                // Global
                bool generalToggle = Foldouts.DoFoldout(foldouts, mat, "General", 1, Foldouts.Style.StandardButton);
                if (Foldouts.DoFoldoutButton(MGUI.collapseLabel, 11)) Toggles.CollapseFoldouts(mat, foldouts, 1);
                if (generalToggle) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_DisplayGlobalGizmo, "Display Range Gizmos");
                        me.RenderQueueField();
                        // me.ShaderProperty(_MirrorRenderMode, "VRC Mirror");
                        me.ShaderProperty(_CameraRenderMode, "VRC Handheld Camera");
                        EditorGUI.BeginChangeCheck();
                        me.ShaderProperty(_BlendMode, "Blending Mode");
                        MGUI.Space4();
                        if (EditorGUI.EndChangeCheck())
                            SetBlendMode(mat);
                        if (_BlendMode.floatValue > 0)
                            me.ShaderProperty(_Opacity, "Opacity");
                        if (_BlendMode.floatValue > 1)
                            MGUI.DisplayInfo("Blend modes other than opaque and alpha use the global falloff to fade out the entire material. This is because these blend modes change screen values by default and need to falloff based on something, even when no effects are enabled.");
                        GUILayout.Label("Global Falloff", EditorStyles.boldLabel);
                        MGUI.SpaceN2();
                        me.ShaderProperty(_MinRange, minLabel);
                        me.ShaderProperty(_MaxRange, maxLabel);
                        });
                    });
                }

                // Filtering
                if (Foldouts.DoFoldout(foldouts, mat, me, _FilterModel, "Filtering", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_FilterModel.floatValue == 0);
                        me.ShaderProperty(_FilterStrength, "Opacity");
                        me.ShaderProperty(_ColorUseGlobal, ugfLabel);
                        if (_ColorUseGlobal.floatValue == 0){
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_ColorMinRange, minLabel);
                                me.ShaderProperty(_ColorMaxRange, maxLabel);
                            });
                        }
                        else MGUI.Space6();
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_Color, "Tint");
                            if (_AutoShift.floatValue == 0)
                                me.ShaderProperty(_Hue, "Hue");
                            else
                                me.ShaderProperty(_AutoShiftSpeed, "Shift Speed");
                            me.ShaderProperty(_HueMode, "Hue Mode");
                            me.ShaderProperty(_MonoTint, Tips.monoTintText);
                            me.ShaderProperty(_AutoShift, "Auto Hue Shift");
                        });
                        MGUI.PropertyGroup(()=>{
                            MGUI.Vector3FieldRGB(_RGB, "RGB Multiplier");
                            me.ShaderProperty(_Brightness, "Brightness");
                            me.ShaderProperty(_Contrast, "Contrast");
                            me.ShaderProperty(_HDR, "HDR");
                            MGUI.ToggleFloat(me, "Image Clamp", _ClampToggle, _ClampMax);
                        });
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_Saturation, "Saturation");
                            me.ShaderProperty(_SaturationR, "Red Saturation");
                            me.ShaderProperty(_SaturationG, "Green Saturation");
                            me.ShaderProperty(_SaturationB, "Blue Saturation");
                        });
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_Invert, "Invert");
                            me.ShaderProperty(_InvertR, "Red Inversion");
                            me.ShaderProperty(_InvertG, "Green Inversion");
                            me.ShaderProperty(_InvertB, "Blue Inversion");
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Shake
                if (Foldouts.DoFoldout(foldouts, mat, me, _ShakeModel, "Shake", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_ShakeModel.floatValue == 0);
                        me.ShaderProperty(_ShakeUseGlobal, ugfLabel);
                        if (_ShakeUseGlobal.floatValue == 0){
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_ShakeMinRange, minLabel);
                                me.ShaderProperty(_ShakeMaxRange, maxLabel);
                            });
                        }
                        MGUI.PropertyGroup(()=>{
                            if (_ShakeModel.floatValue < 3){
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
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Distortion
                if (Foldouts.DoFoldout(foldouts, mat, me, _DistortionModel, "Distortion", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_DistortionModel.floatValue == 0);
                        me.ShaderProperty(_DistortionUseGlobal, ugfLabel);
                        if (_DistortionUseGlobal.floatValue == 0){
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_DistortionMinRange, minLabel);
                                me.ShaderProperty(_DistortionMaxRange, maxLabel);
                            });
                        }
                        MGUI.PropertyGroup(()=>{
                            me.TexturePropertySingleLine(normalMapLabel, _NormalMap);
                            me.TextureScaleOffsetProperty(_NormalMap);
                            me.ShaderProperty(_DistortionStr, strengthLabel);
                            me.ShaderProperty(_DistortionSpeed, speedLabel);
                        });
                        if (_DistortionModel.floatValue == 2){
                            MGUI.PropertyGroup(()=>{
                                MGUI.DisplayInfo("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
                                me.ShaderProperty(_DistortionRadius, radiusLabel);
                                me.ShaderProperty(_DistortionFade, fadeLabel);
                                me.ShaderProperty(_DistortionP2O, p2oLabel);
                            });
                        }
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Blur
                if (Foldouts.DoFoldout(foldouts, mat, me, _BlurModel, "Blur", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_BlurModel.floatValue == 0);
                        if (_BlurModel.floatValue == 1){
                            me.ShaderProperty(_PixelBlurSamples, "Sample Count");
                            if (_PixelBlurSamples.floatValue > 43){
                                MGUI.Space6();
                                MGUI.DisplayWarning("High sample counts can be very laggy! If your strength value is low please consider staying at or below 43 samples.");
                            }
                        }
                        me.ShaderProperty(_BlurUseGlobal, ugfLabel);
                        if (_BlurUseGlobal.floatValue == 0){
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_BlurMinRange, minLabel);
                                me.ShaderProperty(_BlurMaxRange, maxLabel);
                            });
                        }
                        MGUI.PropertyGroup(()=>{
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
                                    MGUI.PropertyGroup(()=>{
                                        MGUI.DisplayInfo("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
                                        me.ShaderProperty(_DoFRadius, radiusLabel);
                                        me.ShaderProperty(_DoFFade, fadeLabel);
                                        me.ShaderProperty(_DoFP2O, p2oLabel);
                                    });
                                }
                            }
                        });
                        MGUI.PropertyGroup(_BlurModel.floatValue != 3, ()=>{
                            if (_BlurModel.floatValue != 3){
                                me.ShaderProperty(_DoF, "Depth of Field");
                                if (_DoF.floatValue == 1){
                                    MGUI.PropertyGroup(()=>{
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
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Noise
                if (Foldouts.DoFoldout(foldouts, mat, me, _NoiseMode, "Noise", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_NoiseMode.floatValue == 0);
                        me.ShaderProperty(_NoiseStrength, "Opacity");
                        MGUI.Space6();
                        me.ShaderProperty(_NoiseUseGlobal, ugfLabel);
                        if (_NoiseUseGlobal.floatValue == 0){
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_NoiseMinRange, minLabel);
                                me.ShaderProperty(_NoiseMaxRange, maxLabel);
                            });
                        }
                        else MGUI.Space6();
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_Noise, "Noise (Grayscale)");
                            MGUI.Vector3FieldRGB(_NoiseRGB, "Noise (RGB)");
                            me.ShaderProperty(_ScanLine, "Scan Lines");
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_ScanLineThick, "Bar Thickness");
                                me.ShaderProperty(_ScanLineSpeed, "Scroll Speed");
                            });
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }

                if (isSFXX){
                    if (Foldouts.DoFoldout(foldouts, mat, me, _Zoom, "Zoom", Foldouts.Style.StandardToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_Zoom.floatValue == 0);
                            me.ShaderProperty(_ZoomUseGlobal, ugfLabel);
                            if (_ZoomUseGlobal.floatValue == 0){
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_ZoomMinRange, minLabel);
                                    me.ShaderProperty(_ZoomMaxRange, maxLabel);
                                });
                            }
                            MGUI.PropertyGroup(()=>{
                                if (_Zoom.floatValue == 2){
                                    me.ShaderProperty(_ZoomStrR, "Red");
                                    me.ShaderProperty(_ZoomStrG, "Green");
                                    me.ShaderProperty(_ZoomStrB, "Blue");
                                }
                                else me.ShaderProperty(_ZoomStr, strengthLabel);
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }
                    
                    // Screenspace Texture Overlay
                    if (Foldouts.DoFoldout(foldouts, mat, me, _SST, "Image Overlay", Foldouts.Style.StandardToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_SST.floatValue == 0);
                            me.ShaderProperty(_SSTUseGlobal, ugfLabel);
                            if (_SSTUseGlobal.floatValue == 0){
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_SSTMinRange, minLabel);
                                    me.ShaderProperty(_SSTMaxRange, maxLabel);
                                });
                            }
                            else MGUI.Space6();
                            MGUI.PropertyGroup(()=>{
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
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_SSTScale, "Scale");
                                me.ShaderProperty(_SSTWidth, "Width");
                                me.ShaderProperty(_SSTHeight, "Height");
                                me.ShaderProperty(_SSTLR, "Left / Right");
                                me.ShaderProperty(_SSTUD, "Down / Up");
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    // Triplanar Mapping
                    if (Foldouts.DoFoldout(foldouts, mat, me, _Triplanar, "Triplanar", Foldouts.Style.StandardToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_Triplanar.floatValue == 0);
                            me.ShaderProperty(_TPUseGlobal, ugfLabel);
                            if (_TPUseGlobal.floatValue == 0){
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_TPMinRange, minLabel);
                                    me.ShaderProperty(_TPMaxRange, maxLabel);
                                });
                            }
                            MGUI.PropertyGroup(()=>{
                                MGUI.DisplayInfo("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
                                me.TexturePropertySingleLine(tpTexLabel, _TPTexture, _TPColor);
                                if (_TPTexture.textureValue){
                                    MGUI.TextureSO(me, _TPTexture);
                                    MGUI.SpaceN2();
                                    MGUI.Vector3Field(_TPScroll, "Scroll Speed", false);
                                    MGUI.Space4();
                                }
                                me.TexturePropertySingleLine(tpNoiseTexLabel, _TPNoiseTex);
                                if (_TPNoiseTex.textureValue){          
                                    MGUI.TextureSO(me, _TPNoiseTex);
                                    MGUI.SpaceN2();
                                    MGUI.Vector3Field(_TPNoiseScroll, "Scroll Speed", false);
                                }
                            });
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_TPRadius, radiusLabel);
                                if (_Triplanar.floatValue == 2){
                                    me.ShaderProperty(_TPScanFade, fadeLabel);
                                    me.ShaderProperty(_TPThickness, "Thickness");
                                    me.ShaderProperty(_TPNoise, "Noise");
                                }
                                else me.ShaderProperty(_TPFade, fadeLabel);
                                me.ShaderProperty(_TPP2O, p2oLabel);
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    // Outline
                    if (Foldouts.DoFoldout(foldouts, mat, me, _OutlineType, "Outline", Foldouts.Style.StandardToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_OutlineType.floatValue == 0);
                                me.ShaderProperty(_OutlineCubeToggle, "Color Source");
                                me.ShaderProperty(_OLUseGlobal, ugfLabel);
                                if (_OLUseGlobal.floatValue == 0){
                                    MGUI.PropertyGroup(()=>{
                                        me.ShaderProperty(_OLMinRange, minLabel);
                                        me.ShaderProperty(_OLMaxRange, maxLabel);
                                    });
                                }
                                MGUI.DisplayInfo("This feature requires a \"Depth Light\" prefab be present in the scene.\n(Found in: Assets/Mochie/Unity/Prefabs)");
                                if (_OutlineType.floatValue == 1){
                                    MGUI.PropertyGroup(()=>{
                                        me.ShaderProperty(_OutlineAltEnable, "Enable Mix Tint");
                                        me.ShaderProperty(_SobelClearInner, "Remove Inner Shading");
                                    });
                                }
                                MGUI.PropertyGroup(()=>{
                                if (_OutlineType.floatValue == 2)
                                    me.ShaderProperty(_AuraSampleCount, "Sample Count");
                                me.ShaderProperty(_OutlineHueShiftAuto, "Auto Hue Shift");
                                if (_OutlineHueShiftAuto.floatValue == 1){
                                    me.ShaderProperty(_OutlineHueShiftSpeed, "Shift Speed");
                                }
                                else {
                                    me.ShaderProperty(_OutlineHueShift, "Hue");
                                }
                                if (_OutlineCubeToggle.floatValue == 0){
                                    me.TexturePropertySingleLine(new GUIContent("Base Line Tint"), _OutlineTex, _OutlineCol);
                                    if (_OutlineTex.textureValue)
                                        MGUI.Vector2Field(_OutlineTexCoord, "Coordinates");
                                }
                                else {
                                    me.TexturePropertySingleLine(new GUIContent("Base Cubemap"), _OutlineCube, _OutlineCol);
                                    if (_OutlineCube.textureValue){
                                        MGUI.PropertyGroup(()=>{
                                            MGUI.SpaceN2();
                                            MGUI.Vector3Field(_CubeRotate, _AutoRotate.floatValue == 1 ? "Speed" : "Rotation", false);
                                            me.ShaderProperty(_AutoRotate, "Auto Rotate");
                                            MGUI.SpaceN2();
                                        });
                                        MGUI.Space6();
                                    }
                                }
                                if (_OutlineType.floatValue == 1 && _OutlineAltEnable.floatValue == 1){
                                    if (_OutlineCubeToggle.floatValue == 0){
                                        me.TexturePropertySingleLine(new GUIContent("Mix Line Tint"), _OutlineTexAlt, _OutlineColAlt);
                                        if (_OutlineTexAlt.textureValue)
                                            MGUI.Vector2Field(_OutlineTexCoordAlt, "Coordinates");
                                    }
                                    else {
                                        me.TexturePropertySingleLine(new GUIContent("Mix Cubemap"), _OutlineCubeAlt, _OutlineColAlt);
                                        if (_OutlineCubeAlt.textureValue){
                                            MGUI.PropertyGroup(()=>{
                                                MGUI.SpaceN2();
                                                MGUI.Vector3Field(_CubeRotateAlt, _AutoRotateAlt.floatValue == 1 ? "Speed" : "Rotation", false);
                                                me.ShaderProperty(_AutoRotateAlt, "Auto Rotate");
                                                MGUI.SpaceN2();
                                            });
                                            MGUI.Space6();
                                        }
                                    }
                                }
                                if (_OutlineCubeToggle.floatValue == 0){
                                    me.ShaderProperty(_BackgroundCol, "Background Tint");
                                }
                                else {
                                    me.TexturePropertySingleLine(new GUIContent("Background Cubemap"), _OutlineBackgroundCube, _BackgroundCol);
                                    if (_OutlineBackgroundCube.textureValue){
                                        MGUI.PropertyGroup(()=>{
                                            MGUI.SpaceN2();
                                            MGUI.Vector3Field(_CubeRotateBG, _AutoRotateBG.floatValue == 1 ? "Speed" : "Rotation", false);
                                            me.ShaderProperty(_AutoRotateBG, "Auto Rotate");
                                            MGUI.SpaceN2();
                                        });
                                        MGUI.Space6();
                                    }
                                }
                                if (_OutlineType.floatValue == 1){
                                    me.ShaderProperty(_OutlineThiccS, "Thickness");
                                    me.ShaderProperty(_OutlineThresh, "Base Strength");
                                    if (_OutlineAltEnable.floatValue == 1){
                                        me.ShaderProperty(_OutlineThreshAlt, "Mix Strength");
                                    }
                                }
                                else if (_OutlineType.floatValue == 2){
                                    me.ShaderProperty(_AuraStr, "Thickness");
                                    me.ShaderProperty(_AuraFade, "Fade");
                                }
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    // Fog
                    if (Foldouts.DoFoldout(foldouts, mat, me, _Fog, "Fog", Foldouts.Style.StandardToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_Fog.floatValue == 0);
                            me.ShaderProperty(_FogUseGlobal, ugfLabel);
                            if (_FogUseGlobal.floatValue == 0){
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_FogMinRange, minLabel);
                                    me.ShaderProperty(_FogMaxRange, maxLabel);
                                });
                            }
                            MGUI.PropertyGroup(()=>{
                                MGUI.DisplayInfo("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
                                me.ShaderProperty(_FogColor, colorLabel);
                                me.ShaderProperty(_FogRadius, radiusLabel);
                                me.ShaderProperty(_FogFade, fadeLabel);
                                me.ShaderProperty(_FogP2O, p2oLabel);
                                me.ShaderProperty(_FogSafeZone, "Safe Zone");
                                if (_FogSafeZone.floatValue == 1){
                                    MGUI.PropertyGroup(()=>{	
                                        me.ShaderProperty(_FogSafeRadius, "Vision Radius");
                                        me.ShaderProperty(_FogSafeMaxRange, "Outer Perimeter");
                                        me.ShaderProperty(_FogSafeOpacity, "Opacity");
                                    });
                                }
                                // me.ShaderProperty(_HeightFalloff, "Height Falloff");
                                // if (_HeightFalloff.floatValue == 1){
                                // 	MGUI.PropertyGroup(()=>{
                                // 		me.ShaderProperty(_FogHeightMin, "Min Height");
                                // 		me.ShaderProperty(_FogHeightMax, "Max Height");
                                // 	});
                                // }
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    // Misc
                    if (Foldouts.DoFoldout(foldouts, mat, "Misc", Foldouts.Style.Standard)) {
                        MGUI.Space6();
                        if (Foldouts.DoFoldout(foldouts, mat, me, _Letterbox, "Letterbox", Foldouts.Style.ThinToggle)){
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.ToggleGroup(_Letterbox.floatValue == 0);
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_UseZoomFalloff, "Use Zoom Falloff");
                                    me.ShaderProperty(_LetterboxStr, "Bar Width");
                                });
                                MGUI.ToggleGroupEnd();
                            });
                        }

                        if (Foldouts.DoFoldout(foldouts, mat, me, _Pulse, "Pulse", Foldouts.Style.ThinToggle)){
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.ToggleGroup(_Pulse.floatValue == 0);
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_WaveForm, "Waveform");
                                    MGUI.ToggleGroup(_WaveForm.floatValue == 0);
                                    me.ShaderProperty(_PulseColor, "Include Filtering");
                                    me.ShaderProperty(_PulseSpeed, speedLabel);
                                    MGUI.ToggleGroupEnd();
                                });
                                MGUI.ToggleGroupEnd();
                            });
                        }

                        if (Foldouts.DoFoldout(foldouts, mat, me, _Shift, "UV Manipulation", Foldouts.Style.ThinToggle)){
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.ToggleGroup(_Shift.floatValue == 0);
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_InvertX, "Invert X");
                                    me.ShaderProperty(_InvertY, "Invert Y");
                                    me.ShaderProperty(_ShiftX, "Shift X");
                                    me.ShaderProperty(_ShiftY, "Shift Y");
                                });
                                MGUI.ToggleGroupEnd();
                            });
                        }

                        if (Foldouts.DoFoldout(foldouts, mat, me, _RoundingToggle, "Rounding", Foldouts.Style.ThinToggle)){
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.ToggleGroup(_RoundingToggle.floatValue == 0);
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_RoundingOpacity, "Opacity");
                                    me.ShaderProperty(_Rounding, "Precision");
                                });
                                MGUI.ToggleGroupEnd();
                            });
                        }

                        if (Foldouts.DoFoldout(foldouts, mat, me, _NMFToggle, "Normal Map Filter", Foldouts.Style.ThinToggle)){
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.ToggleGroup(_NMFToggle.floatValue == 0);
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_NMFOpacity, "Opacity");
                                    me.ShaderProperty(_NormalMapFilter, "Strength");
                                });
                                MGUI.ToggleGroupEnd();
                            });
                        }

                        if (Foldouts.DoFoldout(foldouts, mat, me, _DepthBufferToggle, "Depth Buffer", Foldouts.Style.ThinToggle)){
                            if (_DepthBufferToggle.floatValue == 1){
                                MGUI.DisplayInfo("This feature requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs");
                            }
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.ToggleGroup(_DepthBufferToggle.floatValue == 0);
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_DBOpacity, "Opacity");
                                    me.ShaderProperty(_DBColor, "Tint");
                                });
                                MGUI.ToggleGroupEnd();
                            });
                        }

                        if (Foldouts.DoFoldout(foldouts, mat, me, _SobelFilterToggle, "Sobel Filter", Foldouts.Style.ThinToggle)){
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.ToggleGroup(_SobelFilterToggle.floatValue == 0);
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_SobelFilterColor, "Line Color");
                                    me.ShaderProperty(_SobelFilterBackgroundColor, "Background Color");
                                    me.ShaderProperty(_SobelFilterOpacity, "Opacity");
                                });
                                MGUI.ToggleGroupEnd();
                            });
                        }
                        MGUI.Space6();
                    }
                }
    
                // Audio Link
                if (Foldouts.DoFoldout(foldouts, mat, me, _AudioLinkToggle, "Audio Link", Foldouts.Style.StandardToggle)) {
                    MGUI.ToggleGroup(_AudioLinkToggle.floatValue == 0);
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_AudioLinkStrength, "Strength");
                            MGUI.SliderMinMax(_AudioLinkMin, _AudioLinkMax, 0f, 2f, "Remap", 1);
                        });
                    });

                    if (Foldouts.DoFoldout(foldouts, mat, "Filtering ", Foldouts.Style.Thin)){
                        MGUI.PropertyGroupParent(()=>{
                            me.ShaderProperty(_AudioLinkFilteringBand, "Band");
                            me.ShaderProperty(_AudioLinkFilteringStrength, "Strength");
                            MGUI.SliderMinMax(_AudioLinkFilteringMin, _AudioLinkFilteringMax, 0f, 2f, "Remap", 1);
                        });
                    }

                    if (Foldouts.DoFoldout(foldouts, mat, "Shake ", Foldouts.Style.Thin)){
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_AudioLinkShakeBand, "Band");
                                me.ShaderProperty(_AudioLinkShakeStrength, "Strength");
                                MGUI.SliderMinMax(_AudioLinkShakeMin, _AudioLinkShakeMax, 0f, 2f, "Remap", 1);
                            });
                        });
                    }

                    if (Foldouts.DoFoldout(foldouts, mat, "Distortion ", Foldouts.Style.Thin)){
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_AudioLinkDistortionBand, "Band");
                                me.ShaderProperty(_AudioLinkDistortionStrength, "Strength");
                                MGUI.SliderMinMax(_AudioLinkDistortionMin, _AudioLinkDistortionMax, 0f, 2f, "Remap", 1);
                            });
                        });
                    }

                    if (Foldouts.DoFoldout(foldouts, mat, "Blur ", Foldouts.Style.Thin)){
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_AudioLinkBlurBand, "Band");
                                me.ShaderProperty(_AudioLinkBlurStrength, "Strength");
                                MGUI.SliderMinMax(_AudioLinkBlurMin, _AudioLinkBlurMax, 0f, 2f, "Remap", 1);
                            });
                        });
                    }

                    if (Foldouts.DoFoldout(foldouts, mat, "Noise ", Foldouts.Style.Thin)){
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_AudioLinkNoiseBand, "Band");
                                me.ShaderProperty(_AudioLinkNoiseStrength, "Strength");
                                MGUI.SliderMinMax(_AudioLinkNoiseMin, _AudioLinkNoiseMax, 0f, 2f, "Remap", 1);
                            });
                        });
                    }

                    if (isSFXX){
                        if (Foldouts.DoFoldout(foldouts, mat, "Zoom ", Foldouts.Style.Thin)){
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_AudioLinkZoomBand, "Band");
                                    me.ShaderProperty(_AudioLinkZoomStrength, "Strength");
                                    MGUI.SliderMinMax(_AudioLinkZoomMin, _AudioLinkZoomMax, 0f, 2f, "Remap", 1);
                                });
                            });
                        }

                        if (Foldouts.DoFoldout(foldouts, mat, "Image Overlay ", Foldouts.Style.Thin)){
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_AudioLinkSSTBand, "Band");
                                    me.ShaderProperty(_AudioLinkSSTStrength, "Strength");
                                    MGUI.SliderMinMax(_AudioLinkSSTMin, _AudioLinkSSTMax, 0f, 2f, "Remap", 1);
                                });
                            });
                        }

                        if (Foldouts.DoFoldout(foldouts, mat, "Fog ", Foldouts.Style.Thin)){
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_AudioLinkFogBand, "Band");
                                    me.ShaderProperty(_AudioLinkFogOpacity, "Opacity");
                                    me.ShaderProperty(_AudioLinkFogRadius, "Radius");
                                    MGUI.SliderMinMax(_AudioLinkFogMin, _AudioLinkFogMax, 0f, 2f, "Remap", 1);
                                });
                            });
                        }

                        if (Foldouts.DoFoldout(foldouts, mat, "Triplanar ", Foldouts.Style.Thin)){
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_AudioLinkTriplanarBand, "Band");
                                    me.ShaderProperty(_AudioLinkTriplanarOpacity, "Opacity");
                                    me.ShaderProperty(_AudioLinkTriplanarRadius, "Radius");
                                    MGUI.SliderMinMax(_AudioLinkTriplanarMin, _AudioLinkTriplanarMax, 0f, 2f, "Remap", 1);
                                });
                            });
                        }

                        if (Foldouts.DoFoldout(foldouts, mat, "Outline ", Foldouts.Style.Thin)){
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.BoldLabel("Thickness");
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_AudioLinkOutlineBand, "Band");
                                    me.ShaderProperty(_AudioLinkOutlineStrength, "Strength");
                                    MGUI.SliderMinMax(_AudioLinkOutlineMin, _AudioLinkOutlineMax, 0f, 2f, "Remap", 1);
                                });
                                MGUI.BoldLabel("Base Line Opacity");
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_AudioLinkOutlineColBand, "Band");
                                    me.ShaderProperty(_AudioLinkOutlineColStrength, "Strength");
                                    MGUI.SliderMinMax(_AudioLinkOutlineColMin, _AudioLinkOutlineColMax, 0f, 2f, "Remap", 1);
                                });
                                MGUI.BoldLabel("Mix Line Opacity");
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_AudioLinkOutlineMixBand, "Band");
                                    me.ShaderProperty(_AudioLinkOutlineMixStrength, "Strength");
                                    MGUI.SliderMinMax(_AudioLinkOutlineMixMin, _AudioLinkOutlineMixMax, 0f, 2f, "Remap", 1);
                                });
                            });
                        }

                        if (Foldouts.DoFoldout(foldouts, mat, "Misc ", Foldouts.Style.Thin)){
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_AudioLinkMiscBand, "Band");
                                    me.ShaderProperty(_AudioLinkMiscStrength, "Strength");
                                    MGUI.SliderMinMax(_AudioLinkMiscMin, _AudioLinkMiscMax, 0f, 2f, "Remap", 1);
                                });
                            });
                        }
                    }
                    MGUI.ToggleGroupEnd();
                }
            }
            MGUI.DoFooter(versionLabel);
        }

        void ApplyMaterialSettings(Material mat){
            int zoomMode = mat.GetInt("_Zoom");
            int sstMode = mat.GetInt("_SST");
            int letterboxMode = mat.GetInt("_Letterbox");

            bool isXVersion = MGUI.IsXVersion(mat);
            bool zoomEnabled = zoomMode == 1 && isXVersion;
            bool zoomRGBEnabled = zoomMode == 2 && isXVersion;
            bool sstEnabled = sstMode < 3 && sstMode > 0 && isXVersion;
            bool sstDistEnabled = sstMode == 3 && isXVersion;
            bool letterboxEnabled = letterboxMode > 0 && isXVersion;
            
            MGUI.SetKeyword(mat, "_COLOR_ON", mat.GetInt("_FilterModel") > 0);
            MGUI.SetKeyword(mat, "_SHAKE_ON", mat.GetInt("_ShakeModel") > 0);
            MGUI.SetKeyword(mat, "_DISTORTION_ON", mat.GetInt("_DistortionModel") == 1);
            MGUI.SetKeyword(mat, "_DISTORTION_WORLD_ON", mat.GetInt("_DistortionModel") == 2);
            MGUI.SetKeyword(mat, "_BLUR_PIXEL_ON", mat.GetInt("_BlurModel") == 1);
            MGUI.SetKeyword(mat, "_BLUR_DITHER_ON", mat.GetInt("_BlurModel") == 2);
            MGUI.SetKeyword(mat, "_BLUR_RADIAL_ON", mat.GetInt("_BlurModel") == 3);
            MGUI.SetKeyword(mat, "_BLUR_Y_ON", mat.GetInt("_BlurY") == 1);
            MGUI.SetKeyword(mat, "_CHROMATIC_ABBERATION_ON", mat.GetInt("_RGBSplit") == 1);
            MGUI.SetKeyword(mat, "_DOF_ON", mat.GetInt("_DoF") == 1);
            MGUI.SetKeyword(mat, "_ZOOM_ON", zoomMode == 1 && isXVersion);
            MGUI.SetKeyword(mat, "_ZOOM_RGB_ON", zoomMode == 2 && isXVersion);
            MGUI.SetKeyword(mat, "_IMAGE_OVERLAY_ON", sstMode < 3 && sstMode > 0 && isXVersion);
            MGUI.SetKeyword(mat, "_IMAGE_OVERLAY_DISTORTION_ON", sstMode == 3 && isXVersion);
            MGUI.SetKeyword(mat, "_FOG_ON", mat.GetInt("_Fog") == 1 && isXVersion);
            MGUI.SetKeyword(mat, "_TRIPLANAR_ON", mat.GetInt("_Triplanar") > 0 && isXVersion);
            MGUI.SetKeyword(mat, "_OUTLINE_ON", mat.GetInt("_OutlineType") > 0 && isXVersion);
            MGUI.SetKeyword(mat, "_NOISE_ON", mat.GetInt("_NoiseMode") == 1);
            MGUI.SetKeyword(mat, "_AUDIOLINK_ON", mat.GetInt("_AudioLinkToggle") == 1);
            MGUI.SetKeyword(mat, "_SOBEL_FILTER_ON", mat.GetInt("_SobelFilterToggle") == 1);

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
    }
}