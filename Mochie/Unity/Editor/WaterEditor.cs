using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Mochie;

namespace Mochie {
    
    public class WaterEditor : ShaderGUI {

        GUIContent texLabel = new GUIContent("Base Color");
        GUIContent normalLabel = new GUIContent("Normal Map");
        GUIContent flowLabel = new GUIContent("Flow Map");
        GUIContent noiseLabel = new GUIContent("Noise Texture");
        GUIContent foamLabel = new GUIContent("Foam Texture");
        GUIContent cubeLabel = new GUIContent("Cubemap");
        GUIContent emissLabel = new GUIContent("Emission Map");

        static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
        Toggles toggles = new Toggles(new string[] {
                "Base", 
                "Normal Maps", 
                "Specularity", 
                "Flow Mapping", 
                "Vertex Offset",
                "Caustics",
                "Depth Fog",
                "Foam",
                "Edge Fade",
                "Rain",
                "Tessellation",
                "Emission",
                "LTCGI",
                "AreaLit",
                "Lightmap Settings",
                "Render Settings"
        }, 0);

        string versionLabel = "v1.26.1";

        MaterialProperty _Color = null;
        MaterialProperty _NonGrabColor = null;
        MaterialProperty _AngleTint = null;
        MaterialProperty _BackfaceTint = null;
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
        MaterialProperty _WaveSpeedGlobal = null;
        MaterialProperty _WaveStrengthGlobal = null;
        MaterialProperty _WaveScaleGlobal = null;
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
        // MaterialProperty _Turbulence = null;
        // MaterialProperty _TurbulenceSpeed = null;
        // MaterialProperty _TurbulenceScale = null;
        MaterialProperty _VoronoiScale = null;
        MaterialProperty _VoronoiScroll = null;
        MaterialProperty _VoronoiWaveHeight = null;
        MaterialProperty _VoronoiOffset = null;
        MaterialProperty _VoronoiSpeed = null;
        MaterialProperty _CausticsTex = null;
        MaterialProperty _CausticsToggle = null;
        MaterialProperty _CausticsOpacity = null;
        MaterialProperty _CausticsScale = null;
        MaterialProperty _CausticsSpeed = null;
        MaterialProperty _CausticsFade = null;
        MaterialProperty _CausticsDisp = null;
        MaterialProperty _CausticsShadow = null;
        MaterialProperty _CausticsShadowStrength = null;
        MaterialProperty _CausticsDistortion = null;
        MaterialProperty _CausticsDistortionTex = null;
        MaterialProperty _CausticsDistortionScale = null;
        MaterialProperty _CausticsDistortionSpeed = null;
        MaterialProperty _CausticsRotation = null;
        MaterialProperty _CausticsColor = null;
        MaterialProperty _CausticsPower = null;
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
        MaterialProperty _FoamEdgeStrength = null;
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
        // MaterialProperty _NormalMapOffset1 = null;
        // MaterialProperty _FoamOffset = null;
        // MaterialProperty _NormalMapOffset0 = null;
        // MaterialProperty _BaseColorOffset = null;
        MaterialProperty _FoamDistortionStrength = null;
        MaterialProperty _VertRemapMin = null;
        MaterialProperty _VertRemapMax = null;
        MaterialProperty _LightDir = null;
        MaterialProperty _SpecTint = null;
        MaterialProperty _ReflCube = null;
        MaterialProperty _ReflTint = null;
        MaterialProperty _ReflCubeRotation = null;
        MaterialProperty _RainToggle = null;
        MaterialProperty _RippleScale = null;
        MaterialProperty _RippleSpeed = null;
        MaterialProperty _RippleStr = null;
        MaterialProperty _FoamNormalToggle = null;
        MaterialProperty _FoamNormalStrength = null;
        MaterialProperty _DepthEffects = null;
        MaterialProperty _TessMin = null;
        MaterialProperty _TessMax = null;
        MaterialProperty _TessDistMin = null;
        MaterialProperty _TessDistMax = null;
        MaterialProperty _TessellationOffsetMask = null;
        MaterialProperty _BlendNoise = null;
        MaterialProperty _BlendNoiseScale = null;
        MaterialProperty _BlendNoiseSource = null;
        MaterialProperty _FlowMapUV = null;
        MaterialProperty _BackfaceReflections = null;
        MaterialProperty _RoughnessMap = null;
        MaterialProperty _MetallicMap = null;
        MaterialProperty _EmissionMap = null;
        MaterialProperty _EmissionMapStochasticToggle = null;
        MaterialProperty _EmissionColor = null;
        MaterialProperty _EmissionMapScroll = null;
        MaterialProperty _DetailBaseColor = null;
        MaterialProperty _DetailBaseColorTint = null;
        MaterialProperty _AreaLitToggle = null;
        MaterialProperty _AreaLitMask = null;
        MaterialProperty _AreaLitStrength = null;
        MaterialProperty _AreaLitRoughnessMult = null;
        MaterialProperty _LightMesh = null;
        MaterialProperty _LightTex0 = null;
        MaterialProperty _LightTex1 = null;
        MaterialProperty _LightTex2 = null;
        MaterialProperty _LightTex3 = null;
        MaterialProperty _OpaqueLights = null;
        MaterialProperty _TransparencyMode = null;
        MaterialProperty _DetailNormal = null;
        MaterialProperty _DetailNormalStrength = null;
        MaterialProperty _DetailTextureMode = null;
        MaterialProperty _DetailScroll = null;
        MaterialProperty _StencilRef = null;
        MaterialProperty _RecalculateNormals = null;
        MaterialProperty _ShadowStrength = null;
        MaterialProperty _SSRHeight = null;
        MaterialProperty _CausticsTexArray = null;
        MaterialProperty _NormalMapFlipbook = null;
        MaterialProperty _NormalMapFlipbookSpeed = null;
        MaterialProperty _NormalMapFlipbookStrength = null;
        MaterialProperty _NormalMapMode = null;
        MaterialProperty _VertOffsetFlipbook = null;
        MaterialProperty _VertOffsetFlipbookStrength = null;
        MaterialProperty _VertOffsetFlipbookSpeed = null;
        MaterialProperty _NormalMapFlipbookScale = null;
        MaterialProperty _CausticsFlipbookSpeed = null;
        MaterialProperty _RippleSize = null;
        MaterialProperty _RippleDensity = null;
        MaterialProperty _OpacityMask = null;
        MaterialProperty _OpacityMaskScroll = null;
        MaterialProperty _NonGrabBackfaceTint = null;
        MaterialProperty _EmissionDistortionStrength = null;
        MaterialProperty _FogBrightness = null;
        MaterialProperty _VertexOffsetMask = null;
        MaterialProperty _VertexOffsetMaskStrength = null;
        MaterialProperty _VertexOffsetMaskChannel = null;
        MaterialProperty _SubsurfaceTint = null;
        MaterialProperty _SubsurfaceThreshold = null;
        MaterialProperty _SubsurfaceBrightness = null;
        MaterialProperty _SubsurfaceStrength = null;
        MaterialProperty _TexCoordSpace = null;
        MaterialProperty _TexCoordSpaceSwizzle = null;
        MaterialProperty _GlobalTexCoordScaleUV = null;
        MaterialProperty _GlobalTexCoordScaleWorld = null;
        MaterialProperty _MirrorNormalOffsetSwizzle = null;
        MaterialProperty _InvertNormals = null;
        MaterialProperty _VisualizeFlowmap = null;
        MaterialProperty _AudioLink = null;
        MaterialProperty _AudioLinkStrength = null;
        MaterialProperty _AudioLinkBand = null;
        MaterialProperty _CausticsFlipbookDisp = null;
        MaterialProperty _RippleMask = null;
        MaterialProperty _QueueOffset = null;
        MaterialProperty _HorizonAdjustment = null;
        MaterialProperty _HorizonAdjustmentDistance = null;
        MaterialProperty _EmissionFlowToggle = null;
        MaterialProperty _FogContribution = null;
        MaterialProperty _LightmapDistortion = null;
        MaterialProperty _IndirectStrength = null;
        MaterialProperty _IndirectSaturation = null;
        MaterialProperty _VRSSR = null;
        MaterialProperty _CausticsFlipbookBlend = null;
        MaterialProperty _HorizonTint = null;
        MaterialProperty _HorizonTintDistance = null;
        MaterialProperty _HorizonTintStrength = null;
        // MaterialProperty _WireframeVisualization = null;
        // MaterialProperty _WireframeColor = null;

        // Lightmapping Settings
        MaterialProperty _BAKERY_LMSPEC = null;
        MaterialProperty _BakeryLMSpecStrength = null;
       //  MaterialProperty _BAKERY_SHNONLINEAR = null;
        MaterialProperty _BakeryMode = null;
        MaterialProperty _BicubicSampling = null;
        MaterialProperty _IgnoreRealtimeGI = null;

        // LTCGI
        MaterialProperty _LTCGI = null;
        MaterialProperty _LTCGIStrength = null;
        MaterialProperty _LTCGIRoughness = null;
        MaterialProperty _LTCGI_SpecularColor = null;

        // MaterialProperty _FoamCrestPower = null;
        // MaterialProperty _FogTint2 = null;
        // MaterialProperty _FogPower2 = null;
        // MaterialProperty _FogBrightness2 = null;

        // MaterialProperty _NormalFlipbookStochasticToggle = null;

        // MaterialProperty _Test1 = null;
        // MaterialProperty _Test2 = null;
        
        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        MaterialEditor me;

        public override void OnGUI(MaterialEditor matEditor, MaterialProperty[] props) {

            me = matEditor;

            if (!me.isVisible)
                return;

            foreach (var property in GetType().GetFields(bindingFlags)){
                if (property.FieldType == typeof(MaterialProperty))
                    property.SetValue(this, FindProperty(property.Name, props));
            }
            Material mat = (Material)me.target;

            if (!foldouts.ContainsKey(mat))
                foldouts.Add(mat, toggles);

            if (mat.GetInt("_MaterialResetCheck") == 0){
                mat.SetInt("_MaterialResetCheck", 1);
                SetKeywords(mat);
                SetBlendMode(mat);
            }

            bool isTessellated = MGUI.IsTessellated(mat);

            MGUI.DoHeader(isTessellated ? "TESSELLATED WATER" : "WATER");

            EditorGUI.BeginChangeCheck(); {
                
                int transMode = mat.GetInt("_TransparencyMode");

                // Base
                bool baseToggle = Foldouts.DoFoldout(foldouts, mat, "Base", 1, Foldouts.Style.StandardButton);
                if (Foldouts.DoFoldoutButton(MGUI.collapseLabel, 11)) Toggles.CollapseFoldouts(mat, foldouts, 1);
                if (baseToggle) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.PropertyGroup(()=>{
                            me.TexturePropertySingleLine(texLabel, _MainTex, _BaseColorStochasticToggle);
                            MGUI.TexPropLabel(Tips.stochasticLabel, 117, false);
                            if (_MainTex.textureValue){
                                MGUI.TextureSOScroll(me, _MainTex, _MainTexScroll);
                                // me.ShaderProperty(_BaseColorOffset, Tips.parallaxOffsetLabel);
                                me.ShaderProperty(_BaseColorDistortionStrength, "Distortion Strength");
                                MGUI.Space4();
                            }
                            if (transMode > 0){
                                me.TexturePropertySingleLine(new GUIContent("Opacity"), _OpacityMask, _Opacity);
                                MGUI.TextureSOScroll(me, _OpacityMask, _OpacityMaskScroll, _OpacityMask.textureValue);
                            }
                            else {
                                me.ShaderProperty(_ShadowStrength, "Shadow Strength");
                            }
                        });
                        MGUI.PropertyGroup(()=>{
                            bool hasDetailBC = _DetailBaseColor.textureValue;
                            bool hasDetailN = _DetailNormal.textureValue;
                            me.TexturePropertySingleLine(new GUIContent("Decal Base Color"), _DetailBaseColor, hasDetailBC ? _DetailBaseColorTint : null);
                            me.TexturePropertySingleLine(new GUIContent("Decal Normal Map"), _DetailNormal, hasDetailN ? _DetailNormalStrength : null);
                            MGUI.TextureSOScroll(me, _DetailBaseColor, _DetailScroll, hasDetailBC || hasDetailN);
                        });
                        MGUI.PropertyGroup(() => {
                            if (transMode == 2)
                                me.ShaderProperty(_Color, "Surface Tint");
                            else
                                me.ShaderProperty(_NonGrabColor, "Surface Tint");
                            me.ShaderProperty(_AngleTint, "Glancing Tint");
                            if (transMode == 2)
                                me.ShaderProperty(_BackfaceTint, "Backface Tint");
                            else
                                me.ShaderProperty(_NonGrabBackfaceTint, "Backface Tint");
                            me.ShaderProperty(_HorizonTint, "Horizon Tint");
                            me.ShaderProperty(_HorizonTintDistance, "Horizon Tint Distance");
                            me.ShaderProperty(_HorizonTintStrength, "Horizon Tint Strength");
                        });
                    });
                }

                // Normal Maps
                if (Foldouts.DoFoldout(foldouts, mat, "Normal Maps", Foldouts.Style.Standard)) {
                    MGUI.PropertyGroupParent(()=>{
                        me.ShaderProperty(_NormalMapMode, "Mode");
                        me.ShaderProperty(_DistortionStrength, "Refraction Strength");
                        me.ShaderProperty(_HorizonAdjustment, Tips.horizonAdjustmentText);
                        me.ShaderProperty(_HorizonAdjustmentDistance, "Horizon Adjustment Distance");
                        me.ShaderProperty(_InvertNormals, "Invert");
                        MGUI.Space4();
                        if (_NormalMapMode.floatValue == 0){
                            MGUI.BoldLabel("Primary");
                            MGUI.PropertyGroup(() => {
                                me.TexturePropertySingleLine(Tips.normalMapText, _NormalMap0, _Normal0StochasticToggle);
                                MGUI.TexPropLabel(Tips.stochasticLabel, 117, false);
                                me.ShaderProperty(_NormalStr0, "Strength");
                                MGUI.Vector2Field(_NormalMapScale0, "Scale");
                                MGUI.Vector2Field(_NormalMapScroll0, "Scrolling");
                                me.ShaderProperty(_Rotation0, "Rotation");
                            });
                            MGUI.BoldLabel("Secondary");
                            MGUI.SpaceN18();
                            me.ShaderProperty(_Normal1Toggle, " ");
                            MGUI.PropertyGroup(() => {
                                MGUI.ToggleGroup(_Normal1Toggle.floatValue == 0);
                                me.TexturePropertySingleLine(Tips.normalMapText, _NormalMap1, _Normal1StochasticToggle);
                                MGUI.TexPropLabel(Tips.stochasticLabel, 117, false);
                                me.ShaderProperty(_NormalStr1, "Strength");
                                MGUI.Vector2Field(_NormalMapScale1, "Scale");
                                MGUI.Vector2Field(_NormalMapScroll1, "Scrolling");
                                me.ShaderProperty(_Rotation1, "Rotation");
                                MGUI.ToggleGroupEnd();
                            });
                        }
                        else {
                            MGUI.PropertyGroup(()=>{
                                me.TexturePropertySingleLine(new GUIContent("Flipbook"), _NormalMapFlipbook);
                                MGUI.Vector2Field(_NormalMapFlipbookScale, "Scale");
                                me.ShaderProperty(_NormalMapFlipbookStrength, "Strength");
                                me.ShaderProperty(_NormalMapFlipbookSpeed, "Speed");
                            });
                        }
                    });
                }

                // Specularity
                if (Foldouts.DoFoldout(foldouts, mat, "Specularity", Foldouts.Style.Standard)) {
                    MGUI.PropertyGroupParent(()=>{
                        me.TexturePropertySingleLine(Tips.roughnessText, _RoughnessMap, _Roughness);
                        MGUI.TextureSO(me, _RoughnessMap, _RoughnessMap.textureValue && _DetailTextureMode.floatValue != 1);
                        me.TexturePropertySingleLine(Tips.metallicText, _MetallicMap, _Metallic);
                        MGUI.TextureSO(me, _MetallicMap, _MetallicMap.textureValue && _DetailTextureMode.floatValue != 1);
                        me.ShaderProperty(_DetailTextureMode, Tips.detailMode);
                        MGUI.Space8();
                        me.ShaderProperty(_Reflections, "Reflections");
                        MGUI.PropertyGroup(()=>{
                            MGUI.ToggleGroup(_Reflections.floatValue == 0);
                            if (_Reflections.floatValue == 2){
                                me.TexturePropertySingleLine(cubeLabel, _ReflCube, _ReflTint);
                                MGUI.Vector3Field(_ReflCubeRotation, "Rotation", false);
                            }
                            else {
                                if (_Reflections.floatValue == 3)
                                    me.ShaderProperty(_MirrorNormalOffsetSwizzle, Tips.mirrorNormalSwizzleText);
                                me.ShaderProperty(_ReflTint, "Tint");
                            }
                            me.ShaderProperty(_ReflStrength, "Strength");
                            if (_DepthEffects.floatValue == 1 && _Reflections.floatValue != 3){
                                MGUI.ToggleFloat(me, "Screenspace Reflections", _SSR, _SSRStrength);
                                if (_SSR.floatValue > 0){
                                    me.ShaderProperty(_EdgeFadeSSR, Tips.ssrEdgeFadeText);
                                    me.ShaderProperty(_SSRHeight, Tips.ssrDepthText);
                                    me.ShaderProperty(_VRSSR, "Enable SSR in VR");
                                }
                            }
                            me.ShaderProperty(_BackfaceReflections, "Apply to Backfaces");
                            MGUI.ToggleGroupEnd();
                        });
                        if (_Reflections.floatValue == 3){
                            MGUI.DisplayWarning("Mirror mode requires a VRChat mirror component with this shader selected in the custom shader field. It also requires the mesh be facing forwards on the local Z axis (see default unity quad for example). Lastly, this incurs the same performance cost as any other VRChat mirror, use it very sparingly.");
                        }
                        MGUI.Space8();
                        me.ShaderProperty(_Specular, "Specular Highlights");
                        MGUI.PropertyGroup( ()=>{
                            MGUI.ToggleGroup(_Specular.floatValue == 0);
                            me.ShaderProperty(_SpecTint, "Tint");
                            me.ShaderProperty(_SpecStrength, "Strength");
                            if (_Specular.floatValue == 2){
                                MGUI.Vector3Field(_LightDir, "Light Direction", false);
                            }
                            MGUI.ToggleGroupEnd();
                        });
                    });
                }

                // Emission
                if (Foldouts.DoFoldout(foldouts, mat, "Emission", Foldouts.Style.Standard)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(!me.EmissionEnabledProperty());
                        me.LightmapEmissionFlagsProperty(0, true);
                        MGUI.Space4();
                        MGUI.PropertyGroup(()=>{
                            me.TexturePropertySingleLine(emissLabel, _EmissionMap, _EmissionMapStochasticToggle);
                            MGUI.TexPropLabel(Tips.stochasticLabel, 117, false);
                            me.ShaderProperty(_EmissionColor, "Tint");
                            MGUI.TextureSOScroll(me, _EmissionMap, _EmissionMapScroll);
                            me.ShaderProperty(_EmissionDistortionStrength, "Distortion Strength");
                            me.ShaderProperty(_EmissionFlowToggle, "Apply Flow Mapping");
                            me.ShaderProperty(_AudioLink, "Audio Link");
                            if (_AudioLink.floatValue == 1){
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_AudioLinkBand, "Band");
                                    me.ShaderProperty(_AudioLinkStrength, "Strength");
                                });
                            }
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Flow Mapping
                if (Foldouts.DoFoldout(foldouts, mat, me, _FlowToggle, "Flow Mapping", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.PropertyGroup(()=>{
                            MGUI.ToggleGroup(_FlowToggle.floatValue == 0);
                            me.TexturePropertySingleLine(flowLabel, _FlowMap, _FlowMapUV);
                            MGUI.sRGBWarning(_FlowMap);
                            if (_BlendNoiseSource.floatValue == 1){
                                me.TexturePropertySingleLine(Tips.blendNoise, _BlendNoise);
                                MGUI.sRGBWarning(_BlendNoise);
                            }
                            MGUI.Vector2Field(_FlowMapScale, "Flow Map Scale");
                            if (_BlendNoiseSource.floatValue == 1)
                                MGUI.Vector2Field(_BlendNoiseScale, "Blend Noise Scale");
                            me.ShaderProperty(_FlowSpeed, "Speed");
                            me.ShaderProperty(_FlowStrength, "Strength");
                            me.ShaderProperty(_BlendNoiseSource, "Blend Noise Source");
                            me.ShaderProperty(_VisualizeFlowmap, "Visualize");
                            MGUI.ToggleGroupEnd();
                        });
                    });
                }

                // Vertex Offset
                if (Foldouts.DoFoldout(foldouts, mat, me, _VertOffsetMode, "Vertex Offset", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_VertOffsetMode.floatValue == 0);
                        if (_VertOffsetMode.floatValue == 1 || _VertOffsetMode.floatValue == 0){
                            MGUI.PropertyGroup(()=>{
                                me.TexturePropertySingleLine(noiseLabel, _NoiseTex);
                                me.ShaderProperty(_NoiseTexBlur, "Precision");
                                MGUI.Vector2Field(_NoiseTexScale, "Scale");
                                MGUI.Vector2Field(_NoiseTexScroll, "Scrolling");
                            });
                            MGUI.PropertyGroup(()=>{
                                MGUI.Vector3Field(_Offset, "Strength", false);
                                me.ShaderProperty(_WaveHeight, "Strength Multiplier");
                                MGUI.SliderMinMax(_VertRemapMin, _VertRemapMax, -1f, 1f, "Remap", 1);
                            });
                        }
                        else if (_VertOffsetMode.floatValue == 2){
                            MGUI.DisplayInfo("For best results ensure your base mesh is a plane of approximately 10m x 10m (before scaling in the scene). Also don't be afraid to use extremely large or small values, these settings vary significantly depending on the scale you're working at.");
                            MGUI.BoldLabel("Global");
                            MGUI.PropertyGroup(() => {
                                me.ShaderProperty(_WaveStrengthGlobal, "Strength");
                                me.ShaderProperty(_WaveScaleGlobal, "Scale");
                                me.ShaderProperty(_WaveSpeedGlobal, "Speed");
                                me.ShaderProperty(_RecalculateNormals, "Recalculate Normals");
                            });
                            MGUI.PropertyGroup(()=>{
                                me.TexturePropertySingleLine(Tips.maskText, _VertexOffsetMask, _VertexOffsetMaskStrength, _VertexOffsetMaskChannel);
                                MGUI.TextureSO(me, _VertexOffsetMask);
                            });
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
                            // MGUI.BoldLabel("Turbulence");
                            // MGUI.PropertyGroup(() => {
                            // 	me.ShaderProperty(_Turbulence, Tips.turbulence);
                            // 	me.ShaderProperty(_TurbulenceSpeed, "Speed");
                            // 	me.ShaderProperty(_TurbulenceScale, "Scale");
                            // });
                        }
                        else if (_VertOffsetMode.floatValue == 3){
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_VoronoiSpeed, "Speed");
                                MGUI.Vector2Field(_VoronoiScale, "Scale");
                                MGUI.Vector2Field(_VoronoiScroll, "Scrolling");
                                MGUI.Vector3Field(_VoronoiOffset, "Strength", false);
                                me.ShaderProperty(_VoronoiWaveHeight, "Strength Multiplier");
                            });
                        }
                        else if (_VertOffsetMode.floatValue == 4){
                            MGUI.PropertyGroup(()=>{
                                me.TexturePropertySingleLine(new GUIContent("Vertex Offset Flipbook"), _VertOffsetFlipbook);
                                MGUI.Vector2Field(_NormalMapFlipbookScale, "Scale");
                                me.ShaderProperty(_VertOffsetFlipbookStrength, "Strength");
                                me.ShaderProperty(_VertOffsetFlipbookSpeed, "Speed");
                            });
                        }
                        if (_VertOffsetMode.floatValue > 0 && _VertOffsetMode.floatValue != 2){
                            MGUI.PropertyGroup(()=>{
                                me.TexturePropertySingleLine(Tips.maskText, _VertexOffsetMask, _VertexOffsetMaskStrength, _VertexOffsetMaskChannel);
                                MGUI.TextureSO(me, _VertexOffsetMask);
                            });
                        }
                        if (_VertOffsetMode.floatValue > 0){
                            MGUI.BoldLabel("Subsurface Scattering");
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_SubsurfaceTint, "Tint");
                                me.ShaderProperty(_SubsurfaceStrength, "Strength");
                                me.ShaderProperty(_SubsurfaceBrightness, "Brightness");
                                me.ShaderProperty(_SubsurfaceThreshold, "Threshold");
                            });
                        }
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Caustics
                if (_DepthEffects.floatValue == 1 && transMode == 2){
                    if (Foldouts.DoFoldout(foldouts, mat, me, _CausticsToggle, "Caustics", Foldouts.Style.StandardToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_CausticsToggle.floatValue == 0);
                            MGUI.PropertyGroup(()=>{
                                if (_CausticsToggle.floatValue == 2){
                                    me.TexturePropertySingleLine(new GUIContent("Caustics Texture"), _CausticsTex);
                                    me.TexturePropertySingleLine(new GUIContent("Shadow Texture"), _CausticsShadow, _CausticsShadowStrength);
                                }
                                else if (_CausticsToggle.floatValue == 3){
                                    me.TexturePropertySingleLine(new GUIContent("Caustics Flipbook"), _CausticsTexArray);
                                    me.ShaderProperty(_CausticsFlipbookBlend, "Blending");
                                }
                                me.ShaderProperty(_CausticsColor, "Color");
                                me.ShaderProperty(_CausticsOpacity, "Strength");
                                
                                if (_CausticsToggle.floatValue == 1){
                                    me.ShaderProperty(_CausticsPower, "Power");
                                    me.ShaderProperty(_CausticsDisp, "Dispersion");
                                }
                                if (_CausticsToggle.floatValue != 3)
                                    me.ShaderProperty(_CausticsSpeed, "Speed");
                                else {
                                    me.ShaderProperty(_CausticsFlipbookSpeed, "Speed");
                                    me.ShaderProperty(_CausticsFlipbookDisp, "Dispersion");
                                }
                                me.ShaderProperty(_CausticsScale, "Scale");
                                me.ShaderProperty(_CausticsFade, Tips.causticsFade);
                                // me.ShaderProperty(_CausticsSurfaceFade, Tips.causticsSurfaceFade);
                                MGUI.Vector3Field(_CausticsRotation, "Rotation", false);
                            });
                            if (_CausticsToggle.floatValue != 3){
                                MGUI.PropertyGroup( ()=>{
                                    me.TexturePropertySingleLine(new GUIContent("Distortion Texture"), _CausticsDistortionTex);
                                    me.ShaderProperty(_CausticsDistortion, "Distortion Strength");
                                    me.ShaderProperty(_CausticsDistortionScale, "Distortion Scale");
                                    MGUI.Vector2Field(_CausticsDistortionSpeed, "Distortion Speed");
                                });
                            }
                            MGUI.ToggleGroupEnd();
                        });
                    }
                }

                // Foam
                if (Foldouts.DoFoldout(foldouts, mat, me, _FoamToggle, "Foam", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_FoamToggle.floatValue == 0);
                        MGUI.PropertyGroup(()=>{
                            me.TexturePropertySingleLine(foamLabel, _FoamTex, _FoamColor, _FoamStochasticToggle);
                            MGUI.TexPropLabel(Tips.stochasticLabel, 117, true);
                            MGUI.Space2();
                            MGUI.Vector2Field(_FoamTexScale, "Scale");
                            MGUI.Vector2Field(_FoamTexScroll, "Scrolling");
                            // me.ShaderProperty(_FoamOffset, Tips.parallaxOffsetLabel);
                            me.ShaderProperty(_FoamDistortionStrength, "Distortion Strength");
                            MGUI.ToggleFloat(me, Tips.foamNormal, _FoamNormalToggle, _FoamNormalStrength);
                        });
                        MGUI.PropertyGroup(()=>{
                            me.TexturePropertySingleLine(noiseLabel, _FoamNoiseTex);
                            MGUI.Vector2Field(_FoamNoiseTexScale, "Scale");
                            MGUI.Vector2Field(_FoamNoiseTexScroll, "Scrolling");
                            me.ShaderProperty(_FoamNoiseTexStrength, Tips.foamNoiseTexStrength);
                            me.ShaderProperty(_FoamNoiseTexCrestStrength, Tips.foamNoiseTexCrestStrength);
                        });
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_FoamRoughness, Tips.foamRoughness);
                            me.ShaderProperty(_FoamPower, Tips.foamPower);
                            me.ShaderProperty(_FoamEdgeStrength, Tips.foamEdgeStrength);
                            me.ShaderProperty(_FoamCrestStrength, Tips.foamCrestStrength);
                            // me.ShaderProperty(_FoamCrestPower, Tips.foamCrestPower);
                            me.ShaderProperty(_FoamCrestThreshold, Tips.foamCrestThreshold);
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Depth Fog
                if (_DepthEffects.floatValue == 1 && transMode == 2){
                    if (Foldouts.DoFoldout(foldouts, mat, me, _FogToggle, "Depth Fog", Foldouts.Style.StandardToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_FogToggle.floatValue == 0);
                            // MGUI.BoldLabel("Layer 1");
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_FogTint, "Color");
                                me.ShaderProperty(_FogBrightness, "Brightness");
                                me.ShaderProperty(_FogPower, "Power");
                            });
                            // MGUI.BoldLabel("Layer 2");
                            // MGUI.PropertyGroup(()=>{
                            // 	me.ShaderProperty(_FogTint2, "Color");
                            // 	me.ShaderProperty(_FogBrightness2, "Brightness");
                            // 	me.ShaderProperty(_FogPower2, "Power");
                            // });
                            MGUI.ToggleGroupEnd();
                        });
                    }
                }

                // Edge Fade
                if (_DepthEffects.floatValue == 1 && transMode == 2){
                    if (Foldouts.DoFoldout(foldouts, mat, me, _EdgeFadeToggle, "Edge Fade", Foldouts.Style.StandardToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.PropertyGroup(()=>{
                                MGUI.ToggleGroup(_EdgeFadeToggle.floatValue == 0);
                                me.ShaderProperty(_EdgeFadePower, "Power");
                                me.ShaderProperty(_EdgeFadeOffset, "Offset");
                                MGUI.ToggleGroupEnd();
                            });
                        });
                    }
                }

                // Rain
                if (Foldouts.DoFoldout(foldouts, mat, me, _RainToggle, "Rain", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_RainToggle.floatValue == 0);
                        MGUI.PropertyGroup(()=>{
                            me.TexturePropertySingleLine(Tips.maskText, _RippleMask);
                            MGUI.TextureSO(me, _RippleMask, _RippleMask.textureValue);
                            me.ShaderProperty(_RippleStr, "Strength");
                            me.ShaderProperty(_RippleSpeed, "Speed");
                            me.ShaderProperty(_RippleScale, "Scale");
                        });
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_RippleDensity, "Ripple Density");
                            me.ShaderProperty(_RippleSize, "Ripple Size");
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }

                // LTCGI
                if (Shader.Find("LTCGI/Blur Prefilter") != null){
                    if (Foldouts.DoFoldout(foldouts, mat, me, _LTCGI, "LTCGI", Foldouts.Style.StandardToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_LTCGI.floatValue == 0);
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_LTCGIStrength, "Strength");
                                me.ShaderProperty(_LTCGIRoughness, "Roughness Multiplier");
                                me.ShaderProperty(_LTCGI_SpecularColor, "Tint");
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }
                }
                else {
                    _LTCGI.floatValue = 0;
                    mat.SetInt("_LTCGI", 0);
                    mat.DisableKeyword("LTCGI");
                }

                // AreaLit
                if (Shader.Find("AreaLit/Standard") != null){
                    if (Foldouts.DoFoldout(foldouts, mat, me, _AreaLitToggle, "AreaLit", Foldouts.Style.StandardToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            bool reflDisabled = _AreaLitToggle.floatValue == 1 && _Reflections.floatValue == 0;
                            bool cantInteract = _AreaLitToggle.floatValue == 0 || reflDisabled;
                            if (reflDisabled){
                                MGUI.DisplayError("Reflections are disabled, please enable them to use AreaLit.");
                            }
                            MGUI.ToggleGroup(cantInteract);
                            MGUI.PropertyGroup(()=>{
                                me.TexturePropertySingleLine(Tips.maskText, _AreaLitMask);
                                MGUI.TextureSO(me, _AreaLitMask, _AreaLitMask.textureValue);
                                me.ShaderProperty(_AreaLitStrength, "Strength");
                                me.ShaderProperty(_AreaLitRoughnessMult, "Roughness Multiplier");
                                me.ShaderProperty(_OpaqueLights, Tips.opaqueLightsText);
                            });
                            MGUI.PropertyGroup(()=>{
                                var lightMeshText = !_LightMesh.textureValue ? Tips.lightMeshText : new GUIContent(
                                    Tips.lightMeshText.text + $" (max: {_LightMesh.textureValue.height})", Tips.lightMeshText.tooltip
                                );
                                me.TexturePropertySingleLine(lightMeshText, _LightMesh);
                                me.TexturePropertySingleLine(Tips.lightTex0Text, _LightTex0);
                                MGUI.CheckTrilinear(_LightTex0.textureValue, me);
                                me.TexturePropertySingleLine(Tips.lightTex1Text, _LightTex1);
                                MGUI.CheckTrilinear(_LightTex1.textureValue, me);
                                me.TexturePropertySingleLine(Tips.lightTex2Text, _LightTex2);
                                MGUI.CheckTrilinear(_LightTex2.textureValue, me);
                                me.TexturePropertySingleLine(Tips.lightTex3Text, _LightTex3);
                                MGUI.CheckTrilinear(_LightTex3.textureValue, me);
                            });
                            MGUI.ToggleGroupEnd();
                            MGUI.DisplayInfo("Note that the AreaLit package files MUST be inside a folder named AreaLit (case sensitive) directly in the Assets folder (Assets/AreaLit)");
                            MGUI.Space2();
                        });
                    }
                }
                else {
                    _AreaLitToggle.floatValue = 0f;
                    mat.SetInt("_AreaLitToggle", 0);
                    mat.DisableKeyword("_AREALIT_ON");
                }

                // Tessellation
                if (isTessellated){
                    if (Foldouts.DoFoldout(foldouts, mat, "Tessellation", Foldouts.Style.Standard)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_TessellationOffsetMask, "Vertex Offset Mask");
                                // me.ShaderProperty(_WireframeVisualization, "Debug Wireframe");
                                // if (_WireframeVisualization.floatValue == 1)
                                //     me.ShaderProperty(_WireframeColor, "Wireframe Color");
                                me.ShaderProperty(_TessMin, "Min Factor");
                                me.ShaderProperty(_TessMax, "Max Factor");
                                me.ShaderProperty(_TessDistMin, "Min Distance");
                                me.ShaderProperty(_TessDistMax, "Max Distance");
                            });
                            MGUI.DisplayWarning("WARNING: Tessellation is known to cause issues on some hardware, and can be extremely expensive if you turn up the settings too far. Experimentation will likely be required as factors need to be set based on the base triangle count of the mesh.");
                            MGUI.DisplayInfo("Use the 'Wireframe' scene view for easy visualization.");
                        });
                    }
                }

                // Lightmap Settings
                if (Foldouts.DoFoldout(foldouts, mat, "Lightmap Settings", Foldouts.Style.Standard)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_BakeryMode, Tips.bakeryMode);
                            MGUI.ToggleFloat(me, "Bakery Specular Highlights", _BAKERY_LMSPEC, _BakeryLMSpecStrength);
                        });
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_LightmapDistortion, "Lightmap UV Distortion");
                            me.ShaderProperty(_IndirectStrength, "Lightmap Strength");
                            me.ShaderProperty(_IndirectSaturation, "Lightmap Saturation");
                            me.ShaderProperty(_FogContribution, "Depth Fog Contribution");
                        });
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_BicubicSampling, Tips.bicubicLightmap);
                            me.ShaderProperty(_IgnoreRealtimeGI, Tips.ignoreRealtimeGIText);
                        });
                    });
                }

                // Render Settings
                if (Foldouts.DoFoldout(foldouts, mat, "Render Settings", Foldouts.Style.Standard)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.PropertyGroup(()=>{
                            _QueueOffset.floatValue = (int)_QueueOffset.floatValue;
                            EditorGUI.BeginChangeCheck();
                            me.ShaderProperty(_QueueOffset, Tips.queueOffset);
                            if (EditorGUI.EndChangeCheck()){
                                SetBlendMode(mat);
                            }
                            MGUI.SpaceN1();
                            MGUI.DummyProperty("Render Queue:", mat.renderQueue.ToString());
                            me.ShaderProperty(_StencilRef, "Stencil Reference");
                        });
                        MGUI.PropertyGroup(()=>{
                            EditorGUI.BeginChangeCheck();
                            me.ShaderProperty(_TransparencyMode, "Transparency Mode");
                            if (EditorGUI.EndChangeCheck()){
                                SetBlendMode(mat);
                            }
                            me.ShaderProperty(_CullMode, "Culling Mode");
                            me.ShaderProperty(_ZWrite, "ZWrite");
                            if (transMode == 2){
                                me.ShaderProperty(_DepthEffects, "Depth Effects");
                            }
                        });
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_TexCoordSpace, "Texture Coordinate Space");
                            if (_TexCoordSpace.floatValue == 1f){
                                me.ShaderProperty(_TexCoordSpaceSwizzle, "Swizzle");
                                me.ShaderProperty(_GlobalTexCoordScaleWorld, "Texture Coordinate Scale");
                            }
                            else {
                                me.ShaderProperty(_GlobalTexCoordScaleUV, "Texture Coordinate Scale");
                            }
                        });
                        if (_DepthEffects.floatValue == 1 && transMode == 2){
                            MGUI.DisplayInfo("Depth effects require a \"Depth Light\" prefab be present in the scene.\n(Found in: Assets/Mochie/Unity/Prefabs)");
                            MGUI.Space2();
                        }
                    });
                }

            }
            if (EditorGUI.EndChangeCheck()){
                SetKeywords(mat);
            }
            
            MGUI.DoFooter(versionLabel);
        }

        public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
            base.AssignNewShaderToMaterial(mat, oldShader, newShader);
            MGUI.ClearKeywords(mat);
            SetKeywords(mat);
            SetBlendMode(mat);
        }

        void SetBlendMode(Material mat){
            int transMode = mat.GetInt("_TransparencyMode");
            switch (transMode){
                
                // Opaque
                case 0:
                    mat.SetOverrideTag("RenderType", "Opaque");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.SetShaderPassEnabled("Always", false);
                    mat.EnableKeyword("_OPAQUE_MODE_ON");
                    mat.DisableKeyword("_PREMUL_MODE_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry+mat.GetInt("_QueueOffset");
                break;

                // Premultiplied
                case 1:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.SetShaderPassEnabled("Always", false);
                    mat.DisableKeyword("_OPAQUE_MODE_ON");
                    mat.EnableKeyword("_PREMUL_MODE_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+mat.GetInt("_QueueOffset");
                break;

                // Grabpass
                case 2:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 0);
                    mat.SetShaderPassEnabled("Always", true);
                    mat.DisableKeyword("_OPAQUE_MODE_ON");
                    mat.DisableKeyword("_PREMUL_MODE_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+mat.GetInt("_QueueOffset");
                break;

                default: break;
            }
        }

        void SetKeywords(Material mat){
            MaterialEditor.FixupEmissiveFlag(mat);
            bool isEmissive = (mat.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            int transMode = mat.GetInt("_TransparencyMode");
            int depthFXToggle = mat.GetInt("_DepthEffects");
            int vertMode = mat.GetInt("_VertOffsetMode");
            int reflMode = mat.GetInt("_Reflections");
            int specMode = mat.GetInt("_Specular");
            int causticsMode = mat.GetInt("_CausticsToggle");
            int normalMapMode = mat.GetInt("_NormalMapMode");
            int bicubic = mat.GetInt("_BicubicSampling");
            int ssrToggle = mat.GetInt("_SSR");
            bool ssrEnabled = ssrToggle == 1 && depthFXToggle == 1 && transMode == 2 && (reflMode == 1 || reflMode == 2);

            MGUI.SetKeyword(mat, "_EMISSION_ON", isEmissive);
            MGUI.SetKeyword(mat, "_REFLECTIONS_ON", reflMode > 0);
            MGUI.SetKeyword(mat, "_REFLECTIONS_MANUAL_ON", reflMode == 2);
            MGUI.SetKeyword(mat, "_REFLECTIONS_MIRROR_ON", reflMode == 3);
            MGUI.SetKeyword(mat, "_SPECULAR_ON", specMode > 0);
            MGUI.SetKeyword(mat, "_NOISE_TEXTURE_ON", vertMode == 1);
            MGUI.SetKeyword(mat, "_GERSTNER_WAVES_ON", vertMode == 2);
            MGUI.SetKeyword(mat, "_VORONOI_ON", vertMode == 3);
            MGUI.SetKeyword(mat, "_VERT_FLIPBOOK_ON", vertMode == 4);
            MGUI.SetKeyword(mat, "_DEPTH_EFFECTS_ON", depthFXToggle == 1 && transMode == 2);
            MGUI.SetKeyword(mat, "_DETAIL_BASECOLOR_ON", mat.GetTexture("_DetailBaseColor"));
            MGUI.SetKeyword(mat, "_DETAIL_NORMAL_ON", mat.GetTexture("_DetailNormal"));
            MGUI.SetKeyword(mat, "_CAUSTICS_VORONOI_ON", causticsMode == 1 && depthFXToggle == 1);
            MGUI.SetKeyword(mat, "_CAUSTICS_TEXTURE_ON", causticsMode == 2 && depthFXToggle == 1);
            MGUI.SetKeyword(mat, "_CAUSTICS_FLIPBOOK_ON", causticsMode == 3 && depthFXToggle == 1);
            MGUI.SetKeyword(mat, "_NORMALMAP_FLIPBOOK_ON", normalMapMode == 1);
            MGUI.SetKeyword(mat, "_SCREENSPACE_REFLECTIONS_ON", ssrEnabled);
            MGUI.SetKeyword(mat, "_BICUBIC_LIGHTMAPPING_ON", bicubic == 1);
            MGUI.SetKeyword(mat, "_FOAM_NORMALS_ON", mat.GetInt("_FoamNormalToggle") == 1);
            MGUI.SetKeyword(mat, "BAKERY_SH", mat.GetInt("_BakeryMode") == 1);
            MGUI.SetKeyword(mat, "BAKERY_RNM", mat.GetInt("_BakeryMode") == 2);
            MGUI.SetKeyword(mat, "BAKERY_MONOSH", mat.GetInt("_BakeryMode") == 3);
            MGUI.SetKeyword(mat, "BAKERY_LMSPEC", mat.GetInt("_BAKERY_LMSPEC") == 1);
            MGUI.SetKeyword(mat, "_EMISSION_FLOW_ON", mat.GetInt("_EmissionFlowToggle") == 1);
            MGUI.SetKeyword(mat, "LTCGI", mat.GetInt("_LTCGI") == 1);
        }
    }
}