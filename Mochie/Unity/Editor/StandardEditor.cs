using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace Mochie {
    
    public class StandardEditor : ShaderGUI, IPostMaterialUpgradeCallback {

        public static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
        Toggles toggles = new Toggles(new string[] {
            "Shader Variant",
            "Primary Textures",
            "UVs",
            "Subsurface Scattering",
            "Filtering",
            "Render Settings",
            "Specularity",
            "Rain",
            "AreaLit",
            "LTCGI",
            "Lightmap Settings",
            "Debug"
        }, 1);

        string versionLabel = "v2.1";

        // Variant Settings
        MaterialProperty _BlendMode = null;
        MaterialProperty _SmoothnessToggle = null;
        MaterialProperty _TriplanarCoordSpace = null;
        MaterialProperty _Cutoff = null;
        MaterialProperty _AlphaSource = null;
        MaterialProperty _MipMapRescaling = null;
        MaterialProperty _MipMapScale = null;

        // Primary Textures
        MaterialProperty _PrimaryWorkflow = null;
        MaterialProperty _PrimarySampleMode = null;
        MaterialProperty _PackedHeight = null;
        MaterialProperty _MainTex = null;
        MaterialProperty _Color = null;
        MaterialProperty _NormalMap = null;
        MaterialProperty _NormalStrength = null;
        MaterialProperty _PackedMap = null;
        MaterialProperty _MetallicMap = null;
        MaterialProperty _MetallicStrength = null;
        MaterialProperty _PackedMetallicStrength = null;
        MaterialProperty _MetallicChannel = null;
        MaterialProperty _RoughnessMap = null;
        MaterialProperty _RoughnessStrength = null;
        MaterialProperty _PackedRoughnessStrength = null;
        MaterialProperty _RoughnessChannel = null;
        MaterialProperty _OcclusionMap = null;
        MaterialProperty _OcclusionStrength = null;
        MaterialProperty _PackedOcclusionStrength = null;
        MaterialProperty _OcclusionChannel = null;
        MaterialProperty _HeightMap = null;
        MaterialProperty _HeightStrength = null;
        MaterialProperty _HeightOffset = null;
        MaterialProperty _HeightSteps = null;
        MaterialProperty _HeightChannel = null;
        MaterialProperty _UVMainSet = null;
        MaterialProperty _UVMainSwizzle = null;
        MaterialProperty _UVMainScroll = null;
        MaterialProperty _UVMainRotation = null;

        // Emission/Audiolink
        MaterialProperty _EmissionMap = null;
        MaterialProperty _EmissionStrength = null;
        MaterialProperty _EmissionColor = null;
        MaterialProperty _EmissionPulseWave = null;
        MaterialProperty _EmissionPulseSpeed = null;
        MaterialProperty _EmissionPulseStrength = null;
        MaterialProperty _AudioLinkEmission = null;
        MaterialProperty _AudioLinkEmissionStrength = null;
        MaterialProperty _AudioLinkEmissionMeta = null;
        MaterialProperty _AudioLinkMin = null;
        MaterialProperty _AudioLinkMax = null;

        // Detail Textures
        MaterialProperty _DetailWorkflow = null;
        MaterialProperty _DetailSampleMode = null;
        MaterialProperty _DetailMainTex = null;
        MaterialProperty _DetailMainTexStrength = null;
        MaterialProperty _DetailColor = null;
        MaterialProperty _DetailMainTexBlend = null;
        MaterialProperty _DetailNormalMap = null;
        MaterialProperty _DetailNormalStrength = null;
        MaterialProperty _DetailPackedMap = null;
        MaterialProperty _DetailMetallicMap = null;
        MaterialProperty _DetailMetallicStrength = null;
        MaterialProperty _DetailMetallicBlend = null;
        MaterialProperty _DetailMetallicMultiplier = null;
        MaterialProperty _DetailMetallicChannel = null;
        MaterialProperty _DetailRoughnessMap = null;
        MaterialProperty _DetailRoughnessStrength = null;
        MaterialProperty _DetailRoughnessBlend = null;
        MaterialProperty _DetailRoughnessMultiplier = null;
        MaterialProperty _DetailRoughnessChannel = null;
        MaterialProperty _DetailOcclusionMap = null;
        MaterialProperty _DetailOcclusionStrength = null;
        MaterialProperty _DetailOcclusionBlend = null;
        MaterialProperty _DetailOcclusionMultiplier = null;
        MaterialProperty _DetailOcclusionChannel = null;
        MaterialProperty _UVDetailSet = null;
        MaterialProperty _UVDetailSwizzle = null;
        MaterialProperty _UVDetailScroll = null;
        MaterialProperty _UVDetailRotation = null;

        // Independant Textures
        MaterialProperty _DetailMask = null;
        MaterialProperty _DetailMaskChannel = null;
        MaterialProperty _UVDetailMaskSet = null;
        MaterialProperty _UVDetailMaskSwizzle = null;
        MaterialProperty _UVDetailMaskScroll = null;
        MaterialProperty _UVDetailMaskRotation = null;

        MaterialProperty _HeightMask = null;
        MaterialProperty _HeightMaskChannel = null;
        MaterialProperty _UVHeightMaskSet = null;
        MaterialProperty _UVHeightMaskSwizzle = null;
        MaterialProperty _UVHeightMaskScroll = null;
        MaterialProperty _UVHeightMaskRotation = null;

        MaterialProperty _RainMask = null;
        MaterialProperty _RainMaskChannel = null;
        MaterialProperty _UVRainMaskSet = null;
        MaterialProperty _UVRainMaskSwizzle = null;
        MaterialProperty _UVRainMaskScroll = null;
        MaterialProperty _UVRainMaskRotation = null;

        MaterialProperty _EmissionMask = null;
        MaterialProperty _EmissionMaskChannel = null;
        MaterialProperty _UVEmissionMaskSet = null;
        MaterialProperty _UVEmissionMaskSwizzle = null;
        MaterialProperty _UVEmissionMaskScroll = null;
        MaterialProperty _UVEmissionMaskRotation = null;

        MaterialProperty _AlphaMask = null;
        MaterialProperty _AlphaMaskChannel = null;
        MaterialProperty _UVAlphaMaskSet = null;
        MaterialProperty _UVAlphaMaskSwizzle = null;
        MaterialProperty _UVAlphaMaskScroll = null;
        MaterialProperty _UVAlphaMaskRotation = null;

        // Specularity
        MaterialProperty _ShadingModel = null;
        MaterialProperty _ReflectionsToggle = null;
        MaterialProperty _SpecularHighlightsToggle = null;
        MaterialProperty _SpecularOcclusionToggle = null;
        MaterialProperty _SpecularOcclusionStrength = null;
        MaterialProperty _SpecularOcclusionContrast = null;
        MaterialProperty _SpecularOcclusionBrightness = null;
        MaterialProperty _SpecularOcclusionTint = null;
        MaterialProperty _SpecularOcclusionHDR = null;
        MaterialProperty _SpecularHighlightStrength = null;
        MaterialProperty _ReflectionStrength = null;
        MaterialProperty _FresnelToggle = null;
        MaterialProperty _FresnelStrength = null;
        MaterialProperty _SSRToggle = null;
        MaterialProperty _VRSSR = null;
        MaterialProperty _SSRStrength = null;
        MaterialProperty _SSREdgeFade = null;
        MaterialProperty _SSRHeight = null;
        MaterialProperty _ContactHardening = null;
        MaterialProperty _GSAAToggle = null;
        MaterialProperty _GSAAStrength = null;
        MaterialProperty _IndirectSpecularOcclusionStrength = null;
        MaterialProperty _RealtimeSpecularOcclusionStrength = null;

        // Subsurface Scattering
        MaterialProperty _Subsurface = null;
        MaterialProperty _ThicknessMap = null;
        MaterialProperty _ScatterCol = null;
        MaterialProperty _ThicknessMapPower = null;
        MaterialProperty _ScatterAmbient = null;
        MaterialProperty _ScatterIntensity = null;
        MaterialProperty _ScatterPow = null;
        MaterialProperty _ScatterDist = null; 
        MaterialProperty _WrappingFactor = null;
        MaterialProperty _ScatterBaseColorTint = null;

        // Filtering
        MaterialProperty _Filtering = null;
        MaterialProperty _HueMode = null;
        MaterialProperty _MonoTint = null;
        MaterialProperty _HuePost = null;
        MaterialProperty _SaturationPost = null;
        MaterialProperty _BrightnessPost = null;
        MaterialProperty _ContrastPost = null;
        MaterialProperty _ACES = null;
        // MaterialProperty _ColorGradingLUT = null;
        // MaterialProperty _ColorGradingLUTStrength = null;
        MaterialProperty _Hue = null;
        MaterialProperty _Saturation = null;
        MaterialProperty _Brightness = null;
        MaterialProperty _Contrast = null;
        MaterialProperty _HueDet = null;
        MaterialProperty _SaturationDet = null;
        MaterialProperty _BrightnessDet = null;
        MaterialProperty _ContrastDet = null;
        MaterialProperty _HueEmiss = null;
        MaterialProperty _SaturationEmiss = null;
        MaterialProperty _BrightnessEmiss = null;
        MaterialProperty _ContrastEmiss = null;
        
        // Rain
        MaterialProperty _RainMode = null;
        MaterialProperty _UVRainSet = null;
        MaterialProperty _UVRainSwizzle = null;
        MaterialProperty _UVRainRotation = null;
        MaterialProperty _UVRippleSet = null;
        MaterialProperty _UVRippleSwizzle = null;
        MaterialProperty _UVRippleRotation = null;
        MaterialProperty _RainSheet = null;
        MaterialProperty _RainRows = null;
        MaterialProperty _RainColumns = null;
        MaterialProperty _RainSpeed = null;
        MaterialProperty _RainScale = null;
        MaterialProperty _RainStrength = null;
        MaterialProperty _DropletMask = null;
        MaterialProperty _DynamicDroplets = null;
        MaterialProperty _RainThreshold = null;
        MaterialProperty _RainThresholdSize = null;
        MaterialProperty _RippleScale = null;
        MaterialProperty _RippleSpeed = null;
        MaterialProperty _RippleStrength = null;
        MaterialProperty _RippleDensity = null;
        MaterialProperty _RippleSize = null;

        // Lightmapping Settings
        MaterialProperty _BAKERY_LMSPEC = null;
        MaterialProperty _BakeryLMSpecStrength = null;
        MaterialProperty _BAKERY_SHNONLINEAR = null;
        MaterialProperty _BakeryMode = null;
        MaterialProperty _BicubicSampling = null;
        MaterialProperty _IgnoreRealtimeGI = null;
        MaterialProperty _ApplyHeightOffset = null;

        // AreaLit
        MaterialProperty _AreaLitToggle = null;
        MaterialProperty _AreaLitStrength = null;
        MaterialProperty _AreaLitRoughnessMultiplier = null;
        MaterialProperty _AreaLitSpecularOcclusion = null;
        MaterialProperty _LightMesh = null;
        MaterialProperty _LightTex0 = null;
        MaterialProperty _LightTex1 = null;
        MaterialProperty _LightTex2 = null;
        MaterialProperty _LightTex3 = null;
        MaterialProperty _OpaqueLights = null;
        MaterialProperty _AreaLitOcclusion = null;
        MaterialProperty _AreaLitOcclusionUVSet = null;

        // LTCGI
        MaterialProperty _LTCGI = null;
        MaterialProperty _LTCGIStrength = null;
        MaterialProperty _LTCGIRoughness = null;
        MaterialProperty _LTCGISpecularOcclusion = null;
        MaterialProperty _LTCGI_DiffuseColor = null;
        MaterialProperty _LTCGI_SpecularColor = null;

        // Render Settings
        MaterialProperty _Culling = null;
        MaterialProperty _QueueOffset = null;
        MaterialProperty _UnityFogToggle = null;
        MaterialProperty _VertexBaseColor = null;
        MaterialProperty _MaterialDebugMode = null;

        // Debug
        MaterialProperty _DebugEnable = null;
        MaterialProperty _DebugBaseColor = null;
        MaterialProperty _DebugNormals = null;
        MaterialProperty _DebugRoughness = null;
        MaterialProperty _DebugMetallic = null;
        MaterialProperty _DebugHeight = null;
        MaterialProperty _DebugVertexColors = null;
        MaterialProperty _DebugAtten = null;
        MaterialProperty _DebugReflections = null;
        MaterialProperty _DebugSpecular = null;
        MaterialProperty _DebugOcclusion = null;
        MaterialProperty _DebugAlpha = null;
        MaterialProperty _DebugLighting = null;
        MaterialProperty _NoiseTexSSR = null;
        MaterialProperty _DefaultSampler = null;
        MaterialProperty _SrcBlend = null;
        MaterialProperty _DstBlend = null;
        MaterialProperty _ZWrite = null;
        MaterialProperty _ZTest = null;
        MaterialProperty _DFG = null;

        MaterialEditor me;
        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        bool emissionEnabled = false;
        bool firstTimeApply = true;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props) {
            me = materialEditor;
            
            if (!me.isVisible)
                return;

            foreach (var property in GetType().GetFields(bindingFlags)){
                if (property.FieldType == typeof(MaterialProperty))
                    property.SetValue(this, FindProperty(property.Name, props));
            }
            Material mat = (Material)me.target;

            bool isMobile = MGUI.IsMobileVersion(mat);
            bool isLite = MGUI.IsNewLiteVersion(mat) || isMobile;

            mat.SetShaderPassEnabled("Always", mat.GetInt("_SSRToggle") == 1 && !isLite);

            if (firstTimeApply){
                SetKeywords(mat);
                SetBlendMode(mat);
                firstTimeApply = false;
            }

            if (mat.GetInt("_MaterialResetCheck") == 0){
                mat.SetInt("_MaterialResetCheck", 1);
                SetKeywords(mat);
                SetBlendMode(mat);
            }

            // Add mat to foldout dictionary if it isn't in there yet
            if (!foldouts.ContainsKey(mat))
                foldouts.Add(mat, toggles);

            EditorGUI.BeginChangeCheck(); {
                DoVariant(isLite, isMobile);
                DoPrimaryTextures(mat, isLite, isMobile);
                if (!isLite)
                    DoDetailTextures();
                else
                    DoDetailTexturesLite();
                DoSpecularity(isLite);
                if (!isMobile){
                    DoSubsurface(mat);
                    DoRain(mat);
                }
                DoFiltering(mat);
                DoUVs(mat, isMobile);
                DoAreaLit(mat);
                DoLTCGI(mat);
                DoLightmapSettings(mat);
                DoRenderSettings(mat);
                DoDebug(mat);
            }
            if (EditorGUI.EndChangeCheck()){
                SetKeywords(mat);
                SetBlendMode(mat);
            }

            MGUI.Space10();
            MGUI.DoFooter(versionLabel);
        }

        #region Layouts

        void DoVariant(bool isLite, bool isMobile){
            MGUI.BoldLabel("Shader Variant");
            MGUI.PropertyGroup(()=>{
                MGUI.PropertyGroup(()=>{
                    me.ShaderProperty(_BlendMode, Tips.standBlendMode);
                    if (_BlendMode.floatValue > 0 && !isMobile){
                        me.ShaderProperty(_AlphaSource, Tips.useAlphaMaskLabel);
                    }
                    if (isLite){
                        me.ShaderProperty(_PrimaryWorkflow, Tips.workflowText);
                        if (!isMobile){
                            me.ShaderProperty(_PrimarySampleMode, Tips.samplingMode);
                            if (_PrimaryWorkflow.floatValue == 1){
                                me.ShaderProperty(_PackedHeight, Tips.packedHeightText);
                            }
                        }
                    }
                    me.ShaderProperty(_SmoothnessToggle, Tips.smoothnessModeText);
                    if (_PrimarySampleMode.floatValue == 3 || (_DetailSampleMode.floatValue == 3 && !isLite))
                        me.ShaderProperty(_TriplanarCoordSpace, "Triplanar Coordinates");
                });
                if (_BlendMode.floatValue == 1){
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_MipMapRescaling, Tips.mipRescalingText);
                        if (_MipMapRescaling.floatValue == 1){
                            me.ShaderProperty(_MipMapScale, "Mip Map Scale");
                        }
                        me.ShaderProperty(_Cutoff, Tips.alphaCutoffText);
                    });
                }
                if (_BlendMode.floatValue == 1 && isMobile){
                    MGUI.DisplayWarning("Please note that cutout can be EXTREMELY expensive on mobile platforms, and it is HIGHLY recommended to avoid using it there.");
                    MGUI.Space2();
                }
                MGUI.SpaceN2();
            });
            MGUI.Space6();
        }

        void DoPrimaryTextures(Material mat, bool isLite, bool isMobile){
            MGUI.BoldLabel("Primary Textures");
            MGUI.PropertyGroup(()=>{
                if (!isLite){
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_PrimaryWorkflow, Tips.workflowText);
                        me.ShaderProperty(_PrimarySampleMode, Tips.samplingMode);
                        if (_PrimaryWorkflow.floatValue == 1){
                            me.ShaderProperty(_PackedHeight, Tips.packedHeightText);
                        }
                    });
                }
                MGUI.PropertyGroup(()=>{
                    me.TexturePropertySingleLine(Tips.baseColorText, _MainTex, _Color);
                    if (_BlendMode.floatValue > 0 && _AlphaSource.floatValue == 1 && !isMobile)
                        me.TexturePropertySingleLine(Tips.alphaMaskLabel, _AlphaMask, _AlphaMask.textureValue ? _AlphaMaskChannel : null);
                    me.TexturePropertySingleLine(Tips.normalMapText, _NormalMap, _NormalMap.textureValue ? _NormalStrength : null);
                    MGUI.NormalWarning(_NormalMap);
                    if (_PrimaryWorkflow.floatValue == 0){
                        me.TexturePropertySingleLine(Tips.metallicText, _MetallicMap, _MetallicStrength);
                        MGUI.sRGBWarning(_MetallicMap);
                        me.TexturePropertySingleLine(_SmoothnessToggle.floatValue == 0 ? Tips.roughnessText : Tips.smoothnessText, _RoughnessMap, _RoughnessStrength);
                        MGUI.sRGBWarning(_RoughnessMap);
                        me.TexturePropertySingleLine(Tips.occlusionText, _OcclusionMap, _OcclusionMap.textureValue ? _OcclusionStrength : null);
                        MGUI.sRGBWarning(_OcclusionMap);
                        if (_PrimarySampleMode.floatValue != 3 && !isMobile){
                            me.TexturePropertySingleLine(Tips.heightMapText, _HeightMap, _HeightMap.textureValue ? _HeightStrength : null);
                            MGUI.sRGBWarning(_HeightMap);
                            if (_HeightMap.textureValue){
                                me.TexturePropertySingleLine(Tips.heightMaskText, _HeightMask, _HeightMask.textureValue ? _HeightMaskChannel : null);
                                me.ShaderProperty(_HeightSteps, Tips.heightStepsText);
                                me.ShaderProperty(_HeightOffset, Tips.heightOffsetText);
                            }
                        }
                        // if (MGUI.PropertyButton("Pack Textures")){
                        // 	PackTextures(_MetallicMap, _RoughnessMap, _OcclusionMap, _HeightMap, _PackedMap);
                        // 	_PrimaryWorkflow.floatValue = 1f;
                        // 	mat.SetInt("_PrimaryWorkflow", 1);
                        // 	MGUI.SetKeyword(mat, "_WORKFLOW_PACKED_ON", true);
                        // }
                    }
                    else {
                        me.TexturePropertySingleLine(Tips.packedMapText, _PackedMap);
                        MGUI.sRGBWarning(_PackedMap);
                        if (_PackedHeight.floatValue == 1 && _PrimarySampleMode.floatValue != 3 && !isMobile){
                            me.TexturePropertySingleLine(Tips.heightMaskText, _HeightMask, _HeightMask.textureValue ? _HeightMaskChannel : null);
                        }
                    }
                    DoEmission(isMobile);
                });
                if (_PrimaryWorkflow.floatValue == 1){
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_MetallicChannel, "Metallic Channel");
                        me.ShaderProperty(_RoughnessChannel, _SmoothnessToggle.floatValue == 0 ? "Roughness Channel" : "Smoothness Channel");
                        me.ShaderProperty(_OcclusionChannel, "Occlusion Channel");
                        if (_PackedHeight.floatValue == 1 && _PrimarySampleMode.floatValue != 3 && !isMobile){
                            me.ShaderProperty(_HeightChannel, "Height Channel");
                        }
                    });
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_PackedMetallicStrength, Tips.metallicPackedText);
                        me.ShaderProperty(_PackedRoughnessStrength, _SmoothnessToggle.floatValue == 0 ? Tips.roughnessPackedText : Tips.smoothnessPackedText);
                        me.ShaderProperty(_PackedOcclusionStrength, Tips.occlusionPackedText);
                        if (_PackedHeight.floatValue == 1 && !isMobile){
                            me.ShaderProperty(_HeightStrength, Tips.heightMapPackedText);
                            me.ShaderProperty(_HeightSteps, Tips.heightStepsText);
                            me.ShaderProperty(_HeightOffset, Tips.heightOffsetText);
                        }
                    });
                }
                MGUI.PropertyGroup(()=>{
                    if (_PrimarySampleMode.floatValue != 3){
                        me.ShaderProperty(_UVMainSet, Tips.uvSetLabel);
                        if (_UVMainSet.floatValue >= 5){
                            me.ShaderProperty(_UVMainSwizzle, Tips.swizzleText);
                        }
                        MGUI.TextureSOScroll(me, _MainTex, _UVMainScroll);
                        me.ShaderProperty(_UVMainRotation, "Rotation");
                    }
                    else {
                        MGUI.TextureSO(me, _MainTex);
                    }
                });
                MGUI.SpaceN2();
            });
            MGUI.Space6();
        }
        
        void DoEmission(bool isMobile){
            if (me.EmissionEnabledProperty()){
                emissionEnabled = true;
                bool hadEmissionTexture = _EmissionMap.textureValue != null;
                MGUI.ToggleGroup(!emissionEnabled);
                MGUI.PropertyGroup(()=>{
                    me.LightmapEmissionFlagsProperty(0, true);
                    if (!isMobile){
                        me.ShaderProperty(_EmissionPulseWave, Tips.emissPulseWave);
                        if (_EmissionPulseWave.floatValue > 0){
                            me.ShaderProperty(_EmissionPulseStrength, Tips.emissPulseStrength);
                            me.ShaderProperty(_EmissionPulseSpeed, Tips.emissPulseSpeed);
                        }
                    }
                    me.ShaderProperty(_AudioLinkEmission, Tips.audioLinkEmission);
                    if (_AudioLinkEmission.floatValue > 0){
                        me.ShaderProperty(_AudioLinkEmissionStrength, Tips.audioLinkEmissionStrength);
                        MGUI.SliderMinMax(_AudioLinkMin, _AudioLinkMax, 0f, 2f, "Remap", 0);
                        me.ShaderProperty(_AudioLinkEmissionMeta, Tips.audioLinkEmissionMeta);
                    }
                    MGUI.Space2();
                    
                    me.TexturePropertySingleLine(Tips.emissionText, _EmissionMap, _EmissionColor, _EmissionStrength);
                    MGUI.SpaceN2();
                    if (!isMobile)
                        me.TexturePropertySingleLine(Tips.maskText, _EmissionMask, _EmissionMask.textureValue ? _EmissionMaskChannel : null);
                });
                MGUI.SpaceN2();
                MGUI.ToggleGroupEnd();
                float brightness = _EmissionColor.colorValue.maxColorComponent;
                if (_EmissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
                    _EmissionColor.colorValue = Color.white;
            }
            else {
                emissionEnabled = false;
            }
        }

        void DoDetailTextures(){
            MGUI.BoldLabel("Detail Textures");
            MGUI.PropertyGroup(()=>{
                MGUI.PropertyGroup(()=>{
                    me.ShaderProperty(_DetailWorkflow, Tips.workflowText);
                    me.ShaderProperty(_DetailSampleMode, Tips.samplingMode);
                });
                MGUI.PropertyGroup(()=>{
                    if (_DetailMainTex.textureValue){
                        me.ShaderProperty(_DetailColor, "Detail Tint");
                        MGUI.Space4();
                    }
                    me.TexturePropertySingleLine(Tips.detailAlbedoText, _DetailMainTex, _DetailMainTex.textureValue ? _DetailMainTexStrength : null, _DetailMainTex.textureValue ? _DetailMainTexBlend : null);
                    me.TexturePropertySingleLine(Tips.detailNormalMapText, _DetailNormalMap, _DetailNormalMap.textureValue ? _DetailNormalStrength : null);
                    MGUI.NormalWarning(_DetailNormalMap);
                    if (_DetailWorkflow.floatValue == 0){
                        me.TexturePropertySingleLine(Tips.detailMetallicMapText, _DetailMetallicMap, _DetailMetallicMap.textureValue ? _DetailMetallicStrength : null, _DetailMetallicMap.textureValue ? _DetailMetallicBlend : null);
                        MGUI.sRGBWarning(_DetailMetallicMap);
                        me.TexturePropertySingleLine(_SmoothnessToggle.floatValue == 0 ? Tips.detailRoughnessMapText : Tips.detailSmoothnessMapText, _DetailRoughnessMap, _DetailRoughnessMap.textureValue ? _DetailRoughnessStrength : null, _DetailRoughnessMap.textureValue ? _DetailRoughnessBlend : null);
                        MGUI.sRGBWarning(_DetailRoughnessMap);
                        me.TexturePropertySingleLine(Tips.detailAOMapText, _DetailOcclusionMap, _DetailOcclusionMap.textureValue ? _DetailOcclusionStrength : null, _DetailOcclusionMap.textureValue ? _DetailOcclusionBlend : null);
                        MGUI.sRGBWarning(_DetailOcclusionMap);
                    }
                    else {
                        me.TexturePropertySingleLine(Tips.packedMapText, _DetailPackedMap);
                        MGUI.sRGBWarning(_DetailPackedMap);
                    }
                    me.TexturePropertySingleLine(Tips.detailMaskText, _DetailMask, _DetailMask.textureValue ? _DetailMaskChannel : null);
                });
                if (_DetailWorkflow.floatValue == 1){
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_DetailMetallicChannel, "Metallic Channel");
                        me.ShaderProperty(_DetailRoughnessChannel, _SmoothnessToggle.floatValue == 0 ? "Roughness Channel" : "Smoothness Channel");
                        me.ShaderProperty(_DetailOcclusionChannel, "Occlusion Channel");
                    });
                    MGUI.PropertyGroup(()=>{
                        MGUI.ToggleSlider(me, Tips.metallicPackedText, _DetailMetallicMultiplier, _DetailMetallicStrength);
                        MGUI.ToggleSlider(me, _SmoothnessToggle.floatValue == 0 ? Tips.roughnessPackedText : Tips.smoothnessPackedText, _DetailRoughnessMultiplier, _DetailRoughnessStrength);
                        MGUI.ToggleSlider(me, Tips.occlusionPackedText, _DetailOcclusionMultiplier, _DetailOcclusionStrength);
                    });
                }
                if (_DetailMainTex.textureValue || _DetailNormalMap.textureValue || (_DetailWorkflow.floatValue == 0 && (_DetailMetallicMap.textureValue || _DetailRoughnessMap.textureValue || _DetailOcclusionMap.textureValue)) || (_DetailWorkflow.floatValue == 1 && _DetailPackedMap.textureValue)){
                    MGUI.PropertyGroup(()=>{
                        if (_DetailSampleMode.floatValue != 3){
                            me.ShaderProperty(_UVDetailSet, Tips.uvSetLabel);
                            if (_UVDetailSet.floatValue >= 5){
                                me.ShaderProperty(_UVDetailSwizzle, Tips.swizzleText);
                            }
                            MGUI.TextureSOScroll(me, _DetailMainTex, _UVDetailScroll);
                            me.ShaderProperty(_UVDetailRotation, "Rotation");
                        }
                        else {
                            MGUI.TextureSO(me, _DetailMainTex);
                        }
                    });
                }
                MGUI.SpaceN2();
            });
            MGUI.Space6();
        }

        void DoDetailTexturesLite(){
            MGUI.BoldLabel("Detail Textures");
            MGUI.PropertyGroup(()=>{
                MGUI.PropertyGroup(()=>{
                    if (_DetailMainTex.textureValue){
                        me.ShaderProperty(_DetailColor, "Detail Tint");
                        MGUI.Space4();
                    }
                    me.TexturePropertySingleLine(Tips.detailAlbedoText, _DetailMainTex, _DetailMainTex.textureValue ? _DetailMainTexStrength : null, _DetailMainTex.textureValue ? _DetailMainTexBlend : null);
                    me.TexturePropertySingleLine(Tips.detailNormalMapText, _DetailNormalMap, _DetailNormalMap.textureValue ? _DetailNormalStrength : null);
                    me.TexturePropertySingleLine(Tips.detailMaskText, _DetailMask, _DetailMask.textureValue ? _DetailMaskChannel : null);
                });
                if (_DetailMainTex.textureValue || _DetailNormalMap.textureValue){
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_UVDetailSet, Tips.uvSetLabel);
                        if (_UVDetailSet.floatValue >= 5){
                            me.ShaderProperty(_UVDetailSwizzle, Tips.swizzleText);
                        }
                        MGUI.TextureSOScroll(me, _DetailMainTex, _UVDetailScroll);
                        me.ShaderProperty(_UVDetailRotation, "Rotation");
                    });
                }
                MGUI.SpaceN2();
            });
            MGUI.Space6();
        }

        void DoSpecularity(bool isLite){
            MGUI.ShaderPropertyBold(me, _ShadingModel, "Specularity");
            MGUI.PropertyGroup(()=>{
                MGUI.PropertyGroup(()=>{
                    MGUI.ToggleFloat(me, Tips.specularHighlightsText, _SpecularHighlightsToggle, _SpecularHighlightStrength);
                    MGUI.ToggleFloat(me, Tips.cubemapReflectionsText, _ReflectionsToggle, _ReflectionStrength);
                    if (!isLite){
                        MGUI.ToggleFloat(me, Tips.ssrText, _SSRToggle, _SSRStrength);
                        if (_SSRToggle.floatValue == 1){
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_SSREdgeFade, Tips.ssrEdgeFadeText);
                                me.ShaderProperty(_SSRHeight, Tips.ssrDepthText);
                                me.ShaderProperty(_VRSSR, "Enable in VR");
                            });
                        }
                    }
                    if (_ShadingModel.floatValue == 0)
                        MGUI.ToggleFloat(me, Tips.useFresnel, _FresnelToggle, _FresnelStrength);
                    me.ShaderProperty(_ContactHardening, Tips.contactHardeningText);
                });
                MGUI.PropertyGroup(()=>{
                    MGUI.ToggleFloat(me, Tips.gsaa, _GSAAToggle, _GSAAStrength);
                    if (_ShadingModel.floatValue == 0){
                        MGUI.ToggleFloat(me, Tips.reflShadows, _SpecularOcclusionToggle, _SpecularOcclusionStrength);
                        if (_SpecularOcclusionToggle.floatValue == 1){
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_SpecularOcclusionTint, "Lightmap Tint");
                                me.ShaderProperty(_SpecularOcclusionBrightness, "Lightmap Brightness");
                                me.ShaderProperty(_SpecularOcclusionContrast, "Lightmap Contrast");
                                me.ShaderProperty(_SpecularOcclusionHDR, "Lightmap HDR");
                            });
                            MGUI.SpaceN2();
                        }
                    }
                    else {
                        me.ShaderProperty(_IndirectSpecularOcclusionStrength, Tips.indirectSpecOccText);
                        me.ShaderProperty(_RealtimeSpecularOcclusionStrength, Tips.realtimeSpecOccText);
                    }
                });
                if (_SSRToggle.floatValue == 1 && !isLite){
                    MGUI.DisplayInfo("Screenspace reflections in VRChat require the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs or at least one directional light with shadows enabled in the scene. \n\nThey can also be expensive, please use them sparingly!");
                }
                else {
                    MGUI.SpaceN2();
                }
            });
            MGUI.Space6();
        }

        void DoSubsurface(Material mat){
            if (Foldouts.DoSmallFoldoutBold(foldouts, mat, me, "Subsurface Scattering")){
                MGUI.PropertyGroup(()=>{
                    me.ShaderProperty(_Subsurface, "Enable");
                    MGUI.ToggleGroup(_Subsurface.floatValue == 0);
                    MGUI.PropertyGroup(() => {
                        me.TexturePropertySingleLine(Tips.thicknessMapText, _ThicknessMap, _ThicknessMapPower);
                        me.ShaderProperty(_ScatterCol, Tips.scatterCol);
                        me.ShaderProperty(_ScatterBaseColorTint, Tips.scatterAlbedoTint);
                    });
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_ScatterIntensity, Tips.scatterIntensity);
                        me.ShaderProperty(_ScatterAmbient, Tips.scatterAmbient);
                        me.ShaderProperty(_ScatterPow, Tips.scatterPow);
                        me.ShaderProperty(_ScatterDist, Tips.scatterDist);
                        me.ShaderProperty(_WrappingFactor, Tips.wrappingFactor);
                    });
                    MGUI.ToggleGroupEnd();
                    MGUI.SpaceN2();
                });
                MGUI.Space6();
            }
        }

        void DoRain(Material mat){
            if (Foldouts.DoSmallFoldoutBold(foldouts, mat, me, "Rain")){
                MGUI.PropertyGroup(()=>{
                    me.ShaderProperty(_RainMode, Tips.rainModeText);
                    if (_RainMode.floatValue == 0 || _RainMode.floatValue == 1){
                        RainDroplets(me);
                    }
                    else if (_RainMode.floatValue == 2){
                        RainRipples(me);
                    }
                    else if (_RainMode.floatValue == 3){
                        MGUI.BoldLabel("Droplets");
                        RainDroplets(me);
                        MGUI.BoldLabel("Ripples");
                        RainRipples(me);
                        MGUI.BoldLabel("Both");
                        RainBoth(me);
                    }
                    MGUI.ToggleGroupEnd();
                    MGUI.SpaceN2();
                });
                MGUI.Space6();
            }
        }

        void RainDroplets(MaterialEditor me){
            MGUI.ToggleGroup(_RainMode.floatValue == 0);
            MGUI.PropertyGroup(()=>{
                if (_RainMode.floatValue != 3){
                    me.TexturePropertySingleLine(Tips.maskText, _RainMask, _RainMask.textureValue ? _RainMaskChannel : null);
                }
                me.ShaderProperty(_RainStrength, "Strength");
                me.ShaderProperty(_RainSpeed, "Speed");
                me.ShaderProperty(_DynamicDroplets, "Dynamic Droplets");
            });
            MGUI.PropertyGroup(()=>{
                me.ShaderProperty(_UVRainSet, Tips.uvSetLabel.text);
                if (_UVRainSet.floatValue >= 5)
                    me.ShaderProperty(_UVRainSwizzle, Tips.swizzleText);
                MGUI.Vector2Field(_RainScale, "Scale");
                me.ShaderProperty(_UVRainRotation, "Rotation");
            });
            MGUI.ToggleGroupEnd();
        }

        void RainRipples(MaterialEditor me){
            MGUI.PropertyGroup(()=>{
                if (_RainMode.floatValue != 3){
                    me.TexturePropertySingleLine(Tips.maskText, _RainMask, _RainMask.textureValue ? _RainMaskChannel : null);
                }
                me.ShaderProperty(_RippleStrength, "Strength");
                me.ShaderProperty(_RippleSpeed, "Speed");
                me.ShaderProperty(_RippleDensity, "Density");
                me.ShaderProperty(_RippleSize, "Size");
            });
            MGUI.PropertyGroup(()=>{
                me.ShaderProperty(_UVRippleSet, Tips.uvSetLabel.text);
                if (_UVRippleSet.floatValue >= 5)
                    me.ShaderProperty(_UVRippleSwizzle, Tips.swizzleText);
                MGUI.Vector2Field(_RippleScale, "Scale");
                me.ShaderProperty(_UVRippleRotation, "Rotation");
            });
        }

        void RainBoth(MaterialEditor me){
            MGUI.PropertyGroup(()=>{
                me.TexturePropertySingleLine(Tips.maskText, _RainMask, _RainMask.textureValue ? _RainMaskChannel : null);
                me.ShaderProperty(_RainThreshold, "Angle Threshold");
                me.ShaderProperty(_RainThresholdSize, "Threshold Blend");
            });
        }

        void DoFiltering(Material mat){
            if (Foldouts.DoSmallFoldoutBold(foldouts, mat, me, "Filtering")){
                MGUI.PropertyGroup(()=>{
                    me.ShaderProperty(_Filtering, "Enable");
                    MGUI.ToggleGroup(_Filtering.floatValue == 0);
                    MGUI.Space2();
                    me.ShaderProperty(_HueMode, "Hue Mode");
                    me.ShaderProperty(_MonoTint, Tips.monoTintText);
                    MGUI.Space4();
                    MGUI.BoldLabel("Post Processing");
                    MGUI.PropertyGroup(()=>{
                        // me.TexturePropertySingleLine(Tips.colorGradingLUTText, _ColorGradingLUT, _ColorGradingLUTStrength);
                        me.ShaderProperty(_HuePost, "Hue");
                        me.ShaderProperty(_SaturationPost, "Saturation");
                        me.ShaderProperty(_BrightnessPost, "Brightness");
                        me.ShaderProperty(_ContrastPost, "Contrast");
                        me.ShaderProperty(_ACES, Tips.aces);
                    });
                    MGUI.Space4();
                    MGUI.BoldLabel("Base Color");
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_Hue, "Hue");
                        me.ShaderProperty(_Saturation, "Saturation");
                        me.ShaderProperty(_Brightness, "Brightness");
                        me.ShaderProperty(_Contrast, "Contrast");
                    });
                    MGUI.Space4();
                    MGUI.BoldLabel("Detail Base Color");
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_HueDet, "Hue");
                        me.ShaderProperty(_SaturationDet, "Saturation");
                        me.ShaderProperty(_BrightnessDet, "Brightness");
                        me.ShaderProperty(_ContrastDet, "Contrast");
                    });
                    if (emissionEnabled){
                        MGUI.Space4();
                        MGUI.BoldLabel("Emission");
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_HueEmiss, "Hue");
                            me.ShaderProperty(_SaturationEmiss, "Saturation");
                            me.ShaderProperty(_BrightnessEmiss, "Brightness");
                            me.ShaderProperty(_ContrastEmiss, "Contrast");
                        });
                    }
                    MGUI.ToggleGroupEnd();
                    MGUI.SpaceN2();
                });
                MGUI.Space6();
            }
        }

        void DoUVs(Material mat, bool isMobile){

            bool needsHeightMaskUV = (((_PrimaryWorkflow.floatValue == 1 && _PackedHeight.floatValue == 1) || (_PrimaryWorkflow.floatValue == 0 && _HeightMap.textureValue)) && _HeightMask.textureValue) && !isMobile;
            bool needsDetailMaskUV = _DetailMask.textureValue;
            bool needsRainMaskUV = _RainMode.floatValue > 0 && _RainMask.textureValue;
            bool needsEmissionMaskUV = emissionEnabled && _EmissionMask.textureValue && !isMobile;
            bool needsAlphaMaskUV = _AlphaMask.textureValue && _AlphaSource.floatValue == 1 && _BlendMode.floatValue > 0;

            if (needsHeightMaskUV || needsDetailMaskUV || needsRainMaskUV || needsEmissionMaskUV || needsAlphaMaskUV){
                if (Foldouts.DoSmallFoldoutBold(foldouts, mat, me, "UVs")){
                    MGUI.PropertyGroup(()=>{
                        if (needsDetailMaskUV){
                            MGUI.BoldLabel("Detail Mask");
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_UVDetailMaskSet, Tips.uvSetLabel.text);
                                if (_UVDetailMaskSet.floatValue >= 5)
                                    me.ShaderProperty(_UVDetailMaskSwizzle, Tips.swizzleText);
                                MGUI.TextureSOScroll(me, _UVDetailMaskSet, _UVDetailMaskScroll);
                                me.ShaderProperty(_UVDetailMaskRotation, "Rotation");
                            });
                        }
                        else {
                            MGUI.SpaceN4();
                        }

                        if (needsHeightMaskUV){
                            MGUI.Space4();
                            MGUI.BoldLabel("Height Mask");
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_UVHeightMaskSet, Tips.uvSetLabel.text);
                                if (_UVHeightMaskSet.floatValue >= 5)
                                    me.ShaderProperty(_UVHeightMaskSwizzle, Tips.swizzleText);
                                MGUI.TextureSOScroll(me, _HeightMask, _UVHeightMaskScroll);
                                me.ShaderProperty(_UVHeightMaskRotation, "Rotation");
                            });
                        }

                        if (needsEmissionMaskUV){
                            MGUI.Space4();
                            MGUI.BoldLabel("Emission Mask");
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_UVEmissionMaskSet, Tips.uvSetLabel.text);
                                if (_UVEmissionMaskSet.floatValue >= 5)
                                    me.ShaderProperty(_UVEmissionMaskSwizzle, Tips.swizzleText);
                                MGUI.TextureSOScroll(me, _EmissionMask, _UVEmissionMaskScroll);
                                me.ShaderProperty(_UVEmissionMaskRotation, "Rotation");
                            });
                        }

                        if (needsRainMaskUV){
                            MGUI.Space4();
                            MGUI.BoldLabel("Rain Mask");
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_UVRainMaskSet, Tips.uvSetLabel.text);
                                if (_UVRainMaskSet.floatValue >= 5)
                                    me.ShaderProperty(_UVRainMaskSwizzle, Tips.swizzleText);
                                MGUI.TextureSOScroll(me, _RainMask, _UVRainMaskScroll);
                                me.ShaderProperty(_UVRainMaskRotation, "Rotation");
                            });
                        }

                        if (needsAlphaMaskUV){
                            MGUI.Space4();
                            MGUI.BoldLabel("Alpha Mask");
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_UVAlphaMaskSet, Tips.uvSetLabel.text);
                                if (_UVAlphaMaskSet.floatValue >= 5)
                                    me.ShaderProperty(_UVAlphaMaskSwizzle, Tips.swizzleText);
                                MGUI.TextureSOScroll(me, _AlphaMask, _UVAlphaMaskScroll);
                                me.ShaderProperty(_UVAlphaMaskRotation, "Rotation");
                            });
                        }

                        MGUI.SpaceN2();
                    });
                    MGUI.Space6();
                }
            }
        }

        void DoLTCGI(Material mat){
            if (Shader.Find("LTCGI/Blur Prefilter") != null){
                LTCGILayout(mat);
            }
            else {
                _LTCGI.floatValue = 0;
                mat.SetInt("_LTCGI", 0);
                mat.DisableKeyword("LTCGI");
            }
        }

        void LTCGILayout(Material mat){
            if (Foldouts.DoSmallFoldoutBold(foldouts, mat, me, "LTCGI")){
                MGUI.PropertyGroup(()=>{
                    me.ShaderProperty(_LTCGI, "Enable");
                    MGUI.ToggleGroup(_LTCGI.floatValue == 0);
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_LTCGIStrength, "Strength");
                        me.ShaderProperty(_LTCGIRoughness, "Roughness Multiplier");
                        if (_ShadingModel.floatValue == 0)
                            me.ShaderProperty(_LTCGISpecularOcclusion, "Specular Occlusion");
                    });
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_LTCGI_DiffuseColor, "Diffuse Color");
                        me.ShaderProperty(_LTCGI_SpecularColor, "Specular Color");
                    });
                    MGUI.ToggleGroupEnd();
                    MGUI.SpaceN2();
                });
                MGUI.Space6();
            }
        }

        void DoAreaLit(Material mat){
            if (Shader.Find("AreaLit/Standard") != null){
                AreaLitLayout(mat);
            }
            else {
                _AreaLitToggle.floatValue = 0f;
                mat.SetInt("_AreaLitToggle", 0);
                mat.DisableKeyword("_AREALIT_ON");
            }
        }

        void CheckTrilinear(Texture tex) {
            if(!tex)
                return;
            if(tex.mipmapCount <= 1) {
                me.HelpBoxWithButton(
                    EditorGUIUtility.TrTextContent("Mip maps are required, please enable them in the texture import settings."),
                    EditorGUIUtility.TrTextContent("OK"));
                return;
            }
            if(tex.filterMode != FilterMode.Trilinear) {
                if(me.HelpBoxWithButton(
                    EditorGUIUtility.TrTextContent("Trilinear filtering is required, and aniso is recommended."),
                    EditorGUIUtility.TrTextContent("Fix Now"))) {
                    tex.filterMode = FilterMode.Trilinear;
                    tex.anisoLevel = 1;
                    EditorUtility.SetDirty(tex);
                }
                return;
            }
        }

        void AreaLitLayout(Material mat){
            if (Foldouts.DoSmallFoldoutBold(foldouts, mat, me, "AreaLit")){
                MGUI.PropertyGroup(()=>{
                    me.ShaderProperty(_AreaLitToggle, "Enable");
                    MGUI.ToggleGroup(_AreaLitToggle.floatValue == 0);
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_AreaLitStrength, "Strength");
                        me.ShaderProperty(_AreaLitRoughnessMultiplier, "Roughness Multiplier");
                        if (_ShadingModel.floatValue == 0)
                            me.ShaderProperty(_AreaLitSpecularOcclusion, "Specular Occlusion");
                        me.ShaderProperty(_OpaqueLights, Tips.opaqueLightsText);
                    });
                    MGUI.PropertyGroup(()=>{
                        var lightMeshText = !_LightMesh.textureValue ? Tips.lightMeshText : new GUIContent(
                            Tips.lightMeshText.text + $" (max: {_LightMesh.textureValue.height})", Tips.lightMeshText.tooltip
                        );
                        me.TexturePropertySingleLine(lightMeshText, _LightMesh);
                        me.TexturePropertySingleLine(Tips.lightTex0Text, _LightTex0);
                        CheckTrilinear(_LightTex0.textureValue);
                        me.TexturePropertySingleLine(Tips.lightTex1Text, _LightTex1);
                        CheckTrilinear(_LightTex1.textureValue);
                        me.TexturePropertySingleLine(Tips.lightTex2Text, _LightTex2);
                        CheckTrilinear(_LightTex2.textureValue);
                        me.TexturePropertySingleLine(Tips.lightTex3Text, _LightTex3);
                        CheckTrilinear(_LightTex3.textureValue);
                        me.TexturePropertySingleLine(new GUIContent("Occlusion"), _AreaLitOcclusion);
                        if (_AreaLitOcclusion.textureValue){
                            me.ShaderProperty(_AreaLitOcclusionUVSet, "UV Set");
                        }
                        MGUI.TextureSO(me, _AreaLitOcclusion, _AreaLitOcclusion.textureValue);
                    });
                    MGUI.DisplayInfo("Note that the AreaLit package files MUST be inside a folder named AreaLit (case sensitive) directly in the Assets folder (Assets/AreaLit)");
                    MGUI.ToggleGroupEnd();
                });
                MGUI.Space6();
            }
        }

        void DoLightmapSettings(Material mat){
            if (Foldouts.DoSmallFoldoutBold(foldouts, mat, me, "Lightmap Settings")){
                MGUI.PropertyGroup(()=>{
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_BakeryMode, Tips.bakeryMode);
                        MGUI.ToggleFloat(me, "Bakery Specular Highlights", _BAKERY_LMSPEC, _BakeryLMSpecStrength);
                    });
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_BicubicSampling, Tips.bicubicLightmap);
                        if ((_PrimaryWorkflow.floatValue == 0 && _HeightMap.textureValue) || (_PrimaryWorkflow.floatValue == 1 && _PackedHeight.floatValue == 1))
                            me.ShaderProperty(_ApplyHeightOffset, Tips.heightmapLightmapText);
                        me.ShaderProperty(_BAKERY_SHNONLINEAR, "Non-Linear SH");
                        me.ShaderProperty(_IgnoreRealtimeGI, Tips.ignoreRealtimeGIText);
                        me.DoubleSidedGIField();
                    });
                    MGUI.SpaceN2();
                    if ((_PrimaryWorkflow.floatValue == 0 && _HeightMap.textureValue) || (_PrimaryWorkflow.floatValue == 1 && _PackedHeight.floatValue == 1)){
                        if (_ApplyHeightOffset.floatValue == 1){
                            MGUI.DisplayWarning("Please note that due to lightmaps being atlased, manipulating their uvs often reveals visual artifacts, and is often not recommended. Be sure to check for artifacts if using this option.");
                        }
                    }
                });
                MGUI.Space6();
            }
        }

        void DoRenderSettings(Material mat){
            if (Foldouts.DoSmallFoldoutBold(foldouts, mat, me, "Render Settings")){
                MGUI.PropertyGroup(()=>{
                    MGUI.PropertyGroup(() => {
                        me.ShaderProperty(_Culling, Tips.culling);
                        _QueueOffset.floatValue = (int)_QueueOffset.floatValue;
                        me.ShaderProperty(_QueueOffset, Tips.queueOffset);
                        MGUI.SpaceN1();
                        MGUI.DummyProperty("Render Queue:", mat.renderQueue.ToString());
                    });
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_UnityFogToggle, Tips.unityFogToggleText);
                        me.ShaderProperty(_VertexBaseColor, Tips.vertexBaseColorText);
                        me.EnableInstancingField();
                        me.ShaderProperty(_MaterialDebugMode, "Debug Mode");
                    });
                    MGUI.SpaceN2();
                });
                MGUI.Space6();
            }
        }

        void DoDebug(Material mat){
            if (_MaterialDebugMode.floatValue == 1){
                if (Foldouts.DoSmallFoldoutBold(foldouts, mat, me, "Debug")){
                    MGUI.PropertyGroup(()=>{
                        MGUI.ShaderPropertyBold(me, _DebugEnable, "Debug View");
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_DebugBaseColor, "Base Color");
                            me.ShaderProperty(_DebugAlpha, "Alpha");
                            me.ShaderProperty(_DebugNormals, "Normals");
                            me.ShaderProperty(_DebugRoughness, "Roughness");
                            me.ShaderProperty(_DebugMetallic, "Metallic");
                            me.ShaderProperty(_DebugOcclusion, "Occlusion");
                            me.ShaderProperty(_DebugHeight, "Height");
                            me.ShaderProperty(_DebugLighting, "Lighting");
                            me.ShaderProperty(_DebugAtten, "Realtime Shadows");
                            me.ShaderProperty(_DebugReflections, "Reflections");
                            me.ShaderProperty(_DebugSpecular, "Specular Highlights");
                            me.ShaderProperty(_DebugVertexColors, "Vertex Colors");
                        });		
                        MGUI.BoldLabel("Default Textures");
                        MGUI.PropertyGroup(()=>{
                            me.TexturePropertySingleLine(Tips.defaultSamplerText, _DefaultSampler);
                            me.TexturePropertySingleLine(new GUIContent("LUT for Filament SM"), _DFG);
                            me.TexturePropertySingleLine(new GUIContent("SSR Noise Texture"), _NoiseTexSSR);
                            me.TexturePropertySingleLine(new GUIContent("Droplet Map"), _DropletMask);
                            me.TexturePropertySingleLine(new GUIContent("Rain Texture Sheet"), _RainSheet);
                            me.ShaderProperty(_RainColumns, "Rain Texture Sheet Columns");
                            me.ShaderProperty(_RainRows, "Rain Texture Sheet Rows");
                        });

                        MGUI.BoldLabel("Render Settings");
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_SrcBlend, "Source Blend Op");
                            me.ShaderProperty(_DstBlend, "Destination Blend Op");
                            me.ShaderProperty(_ZWrite, "ZWrite");
                            me.ShaderProperty(_ZTest, "ZTest");
                        });
                        MGUI.SpaceN2();
                    });
                }
            }
        }

        #endregion

        #region Applying Settings

        void SetProperties(Material mat){
            mat.SetInt("_SampleMetallic", mat.GetTexture("_MetallicMap") ? 1 : 0);
            mat.SetInt("_SampleRoughness", mat.GetTexture("_RoughnessMap") ? 1 : 0);
            mat.SetInt("_SampleOcclusion", mat.GetTexture("_OcclusionMap") ? 1 : 0);
        }

        void SetKeywords(Material mat){
            
            MGUI.ClearKeywords(mat);
            MaterialEditor.FixupEmissiveFlag(mat);
            bool isEmissive = (mat.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            bool enableParallax = (mat.GetInt("_PrimaryWorkflow") == 0 ? mat.GetTexture("_HeightMap") : mat.GetInt("_PackedHeight") == 1) && mat.GetInt("_PrimarySampleMode") != 3;

            mat.SetInt("_SampleMetallic", mat.GetTexture("_MetallicMap") ? 1 : 0);
            mat.SetInt("_SampleRoughness", mat.GetTexture("_RoughnessMap") ? 1 : 0);
            mat.SetInt("_SampleOcclusion", mat.GetTexture("_OcclusionMap") ? 1 : 0);
            mat.SetInt("_SampleCustomLUT", mat.GetTexture("_ColorGradingLUT") ? 1 : 0);

            MGUI.SetKeyword(mat, "_EMISSION_ON", isEmissive);
            MGUI.SetKeyword(mat, "_PARALLAX_ON", enableParallax);
            MGUI.SetKeyword(mat, "_REFLECTIONS_ON", mat.GetInt("_ReflectionsToggle") == 1);
            MGUI.SetKeyword(mat, "_SPECULAR_HIGHLIGHTS_ON", mat.GetInt("_SpecularHighlightsToggle") == 1);
            MGUI.SetKeyword(mat, "_WORKFLOW_PACKED_ON", mat.GetInt("_PrimaryWorkflow") == 1);
            MGUI.SetKeyword(mat, "_WORKFLOW_DETAIL_PACKED_ON", mat.GetInt("_DetailWorkflow") == 1);
            MGUI.SetKeyword(mat, "_STOCHASTIC_ON", mat.GetInt("_PrimarySampleMode") == 1);
            MGUI.SetKeyword(mat, "_SUPERSAMPLING_ON", mat.GetInt("_PrimarySampleMode") == 2);
            MGUI.SetKeyword(mat, "_TRIPLANAR_ON", mat.GetInt("_PrimarySampleMode") == 3);
            MGUI.SetKeyword(mat, "_STOCHASTIC_DETAIL_ON", mat.GetInt("_DetailSampleMode") == 1);
            MGUI.SetKeyword(mat, "_SUPERSAMPLING_DETAIL_ON", mat.GetInt("_DetailSampleMode") == 2);
            MGUI.SetKeyword(mat, "_TRIPLANAR_DETAIL_ON", mat.GetInt("_DetailSampleMode") == 3);
            MGUI.SetKeyword(mat, "_NORMALMAP_ON", mat.GetTexture("_NormalMap"));
            MGUI.SetKeyword(mat, "_DETAIL_MAINTEX_ON", mat.GetTexture("_DetailMainTex"));
            MGUI.SetKeyword(mat, "_DETAIL_NORMAL_ON", mat.GetTexture("_DetailNormalMap"));
            MGUI.SetKeyword(mat, "_DETAIL_METALLIC_ON", mat.GetTexture("_DetailMetallicMap"));
            MGUI.SetKeyword(mat, "_DETAIL_ROUGHNESS_ON", mat.GetTexture("_DetailRoughnessMap"));
            MGUI.SetKeyword(mat, "_DETAIL_OCCLUSION_ON", mat.GetTexture("_DetailOcclusionMap"));
            MGUI.SetKeyword(mat, "_SSR_ON", mat.GetInt("_SSRToggle") == 1);
            MGUI.SetKeyword(mat, "_RAIN_DROPLETS_ON", mat.GetInt("_RainMode") == 1);
            MGUI.SetKeyword(mat, "_RAIN_RIPPLES_ON", mat.GetInt("_RainMode") == 2);
            MGUI.SetKeyword(mat, "_RAIN_AUTO_ON", mat.GetInt("_RainMode") == 3);
            MGUI.SetKeyword(mat, "_AUDIOLINK_ON", mat.GetInt("_AudioLinkEmission") > 0 && isEmissive);
            MGUI.SetKeyword(mat, "_AUDIOLINK_META_ON", mat.GetInt("_AudioLinkEmission") > 0 && mat.GetInt("_AudioLinkEmissionMeta") > 0 && isEmissive);
            MGUI.SetKeyword(mat, "BAKERY_SH", mat.GetInt("_BakeryMode") == 1);
            MGUI.SetKeyword(mat, "BAKERY_RNM", mat.GetInt("_BakeryMode") == 2);
            MGUI.SetKeyword(mat, "BAKERY_MONOSH", mat.GetInt("_BakeryMode") == 3);
            MGUI.SetKeyword(mat, "BAKERY_LMSPEC", mat.GetInt("_BAKERY_LMSPEC") == 1);
            MGUI.SetKeyword(mat, "BAKERY_SHNONLINEAR", mat.GetInt("_BAKERY_SHNONLINEAR") == 1);
            MGUI.SetKeyword(mat, "_BICUBIC_SAMPLING_ON", mat.GetInt("_BicubicSampling") == 1);
            MGUI.SetKeyword(mat, "_AREALIT_ON", mat.GetInt("_AreaLitToggle") == 1);
            MGUI.SetKeyword(mat, "LTCGI", mat.GetInt("_LTCGI") == 1);
        }

        public static void SetBlendMode(Material mat){
            bool ssrToggle = mat.GetInt("_SSRToggle") == 1;
            bool isLite = MGUI.IsNewLiteVersion(mat);
            bool isMobile = MGUI.IsMobileVersion(mat);
            int blendMode = mat.GetInt("_BlendMode");
            mat.SetInt("_AlphaToMask", blendMode == 1 ? 1 : 0);	

            switch (blendMode){
                case 0:
                    mat.SetOverrideTag("RenderType", "Opaque");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry+mat.GetInt("_QueueOffset");
                    if (ssrToggle && !isLite && !isMobile)
                        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest+51+mat.GetInt("_QueueOffset");
                    break;
                case 1:
                    mat.SetOverrideTag("RenderType", "TransparentCutout");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.EnableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest+mat.GetInt("_QueueOffset");
                    if (ssrToggle && !isLite && !isMobile)
                        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest+51+mat.GetInt("_QueueOffset");
                    break;
                case 2:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+mat.GetInt("_QueueOffset");
                    break;
                case 3:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+mat.GetInt("_QueueOffset");
                    break;
            }
        }

        void TransferStandardTextures(Material mat, Shader oldShader){
            if (oldShader == Shader.Find("Standard") || oldShader == Shader.Find("Autodesk Interactive") || oldShader == Shader.Find("Standard (Specular setup)")){
                mat.SetTexture("_NormalMap", mat.GetTexture("_BumpMap"));
                mat.SetTexture("_MetallicMap", mat.GetTexture("_MetallicGlossMap"));
                mat.SetTexture("_HeightMap", mat.GetTexture("_ParallaxMap"));
                mat.SetTexture("_DetailMainTex", mat.GetTexture("_DetailAlbedoMap"));
            }
            if (oldShader == Shader.Find("Autodesk Interactive") || oldShader == Shader.Find("Standard (Specular setup)")){
                mat.SetTexture("_RoughnessMap", mat.GetTexture("_SpecGlossMap"));
            }
        }

        public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
            base.AssignNewShaderToMaterial(mat, oldShader, newShader);
            TransferStandardTextures(mat, oldShader);
            SetKeywords(mat);
            SetBlendMode(mat);
        }

        public void OnAfterMaterialUpgraded(Material mat){
            SetProperties(mat);
            // SetKeywords(mat); - this spams errors from properties not found when called from here, idk why
        }

        #endregion 
    }
}