using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Mochie;

namespace Mochie {

    public class ParticleEditor : ShaderGUI {

        GUIContent texLabel = new GUIContent("Base Color");
        GUIContent tex2Label = new GUIContent("Secondary Color");
        GUIContent normalLabel = new GUIContent("Normal Map");
        GUIContent applyStreamsText = new GUIContent("Fix Vertex Streams", "Apply the vertex stream layout to all Particle Systems using this mat");

        static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
        Toggles toggles = new Toggles(new string[] {
                "Base", 
                "Lighting",
                "Filtering", 
                "Distortion", 
                "Pulse", 
                "Falloff",
                "Audio Link",
                "Filtering ",
                "Distortion ",
                "Dissolve",
                "Opacity",
                "Cutout",
                "Random Hue",
                "Outlines",
                "Special Effects",
                "Render Settings"
        }, 0);

        string versionLabel = "v3.0";

        // Render Settings
        MaterialProperty _BlendMode = null;
        MaterialProperty _AlphaSource = null;
        MaterialProperty _DstBlend = null;
        MaterialProperty _Culling = null;
        MaterialProperty _ZTest = null;
        MaterialProperty _Falloff = null;
        MaterialProperty _IsCutout = null;
        MaterialProperty _Cutoff = null;
        MaterialProperty _FlipbookBlending = null;
        MaterialProperty _LightingToggle = null;

        // Color
        MaterialProperty _MainTex = null;
        MaterialProperty _MainTexUVMode = null;
        MaterialProperty _MainTexSpeed = null;
        MaterialProperty _MainTexPolarRotation = null;
        MaterialProperty _MainTexPolarSpeed = null;
        MaterialProperty _MainTexPolarRadius = null;
        MaterialProperty _AlphaMask = null;
        MaterialProperty _AlphaMaskUVMode = null;
        MaterialProperty _AlphaMaskSpeed = null;
        MaterialProperty _AlphaMaskPolarRotation = null;
        MaterialProperty _AlphaMaskPolarSpeed = null;
        MaterialProperty _AlphaMaskPolarRadius = null;
        MaterialProperty _AlphaMaskChannel = null;
        MaterialProperty _SecondTex = null;
        MaterialProperty _SecondTexUVMode = null;
        MaterialProperty _SecondTexSpeed = null;
        MaterialProperty _SecondTexPolarRotation = null;
        MaterialProperty _SecondTexPolarSpeed = null;
        MaterialProperty _SecondTexPolarRadius = null;
        MaterialProperty _TexBlendMode = null;
        MaterialProperty _Color = null;
        MaterialProperty _SecondColor = null;
        MaterialProperty _Softening = null;
        MaterialProperty _SoftenStr = null;
        MaterialProperty _Opacity = null;
        MaterialProperty _CutoutRim = null;
        MaterialProperty _CutoutRimWidth = null;
        MaterialProperty _CutoutRimColor = null;
        MaterialProperty _CutoutRimBlend = null;

        // Lighting
        MaterialProperty _NormalMapLighting = null;
        MaterialProperty _NormalMapLightingUVMode = null;
        MaterialProperty _NormalMapLightingSpeed = null;
        MaterialProperty _NormalMapLightingPolarRotation = null;
        MaterialProperty _NormalMapLightingPolarSpeed = null;
        MaterialProperty _NormalMapLightingPolarRadius = null;
        MaterialProperty _NormalMapLightingScale = null;
        MaterialProperty _Metallic = null;
        MaterialProperty _Roughness = null;
        MaterialProperty _MetallicMap = null;
        MaterialProperty _MetallicMapUVMode = null;
        MaterialProperty _MetallicMapSpeed = null;
        MaterialProperty _MetallicMapPolarRotation = null;
        MaterialProperty _MetallicMapPolarSpeed = null;
        MaterialProperty _MetallicMapPolarRadius = null;
        MaterialProperty _RoughnessMap = null;
        MaterialProperty _RoughnessMapUVMode = null;
        MaterialProperty _RoughnessMapSpeed = null;
        MaterialProperty _RoughnessMapPolarRotation = null;
        MaterialProperty _RoughnessMapPolarSpeed = null;
        MaterialProperty _RoughnessMapPolarRadius = null;
        MaterialProperty _ReflectionsToggle = null;
        MaterialProperty _SpecularHighlightsToggle = null;
        MaterialProperty _ReflectionStrength = null;
        MaterialProperty _SpecularHighlightStrength = null;
        MaterialProperty _LightVolumes = null;
        MaterialProperty _LightVolumeSpecularity = null;
        MaterialProperty _LightVolumeSpecularityStrength = null;
        MaterialProperty _LightVolumeStrength = null;
        MaterialProperty _Emission = null;
        MaterialProperty _EmissionColor = null;
        MaterialProperty _EmissionMap = null;
        MaterialProperty _EmissionMapUVMode = null;
        MaterialProperty _EmissionMapSpeed = null;
        MaterialProperty _EmissionMapPolarRotation = null;
        MaterialProperty _EmissionMapPolarSpeed = null;
        MaterialProperty _EmissionMapPolarRadius = null;
        MaterialProperty _EmissionLightReactivity = null;
        MaterialProperty _EmissionLightReactivityMin = null;
        MaterialProperty _EmissionLightReactivityMax = null;

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
        MaterialProperty _NormalMapUVMode = null;
        MaterialProperty _NormalMapSpeed = null;
        MaterialProperty _NormalMapPolarRotation = null;
        MaterialProperty _NormalMapPolarSpeed = null;
        MaterialProperty _NormalMapPolarRadius = null;
        MaterialProperty _DistortionStr = null;
        MaterialProperty _DistortionBlend = null;
        MaterialProperty _DistortionSpeed = null;
        MaterialProperty _DistortMainTex = null;

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

        //Dissolve
        MaterialProperty _Dissolve = null;
        MaterialProperty _DissolveMode = null;
        MaterialProperty _DissolveNoise = null;
        MaterialProperty _DissolveNoiseUVMode = null;
        MaterialProperty _DissolveNoiseSpeed = null;
        MaterialProperty _DissolveNoisePolarRotation = null;
        MaterialProperty _DissolveNoisePolarSpeed = null;
        MaterialProperty _DissolveNoisePolarRadius = null;
        MaterialProperty _DissolveAgeThreshold = null;
        MaterialProperty _DissolveAgeThresholdMin = null;
        MaterialProperty _DissolveAgeThresholdMax = null;
        MaterialProperty _DissolveAmount = null;
        MaterialProperty _DissolveRandomOffset = null;
        MaterialProperty _DissolveRimColor = null;
        MaterialProperty _DissolveRimWidth = null;
        MaterialProperty _DissolveRimBlend = null;

        // Random Hue
        MaterialProperty _RandomHue = null;
        MaterialProperty _RandomHueMode = null;
        MaterialProperty _RandomHueMonoTint = null;
        MaterialProperty _RandomHueMax = null;
        MaterialProperty _RandomHueMin = null;
        MaterialProperty _RandomSatMax = null;
        MaterialProperty _RandomSatMin = null;

        // Outlines
        MaterialProperty _Outlines = null;
        MaterialProperty _OutlineThickness = null;
        MaterialProperty _OutlineColor = null;
        MaterialProperty _OutlineStencilPass = null;
        MaterialProperty _OutlineStencilCompare = null;
        MaterialProperty _OutlineStencilToggle = null;
        
        // Audio Link
        MaterialProperty _AudioLink = null;
        MaterialProperty _AudioLinkStrength = null;
        MaterialProperty _AudioLinkRemapMin = null;
        MaterialProperty _AudioLinkRemapMax = null;

        MaterialProperty _AudioLinkFilterBand = null;
        MaterialProperty _AudioLinkFilterStrength = null;
        MaterialProperty _AudioLinkRemapFilterMin = null;
        MaterialProperty _AudioLinkRemapFilterMax = null;

        MaterialProperty _AudioLinkDistortionBand = null;
        MaterialProperty _AudioLinkDistortionStrength = null;
        MaterialProperty _AudioLinkRemapDistortionMin = null;
        MaterialProperty _AudioLinkRemapDistortionMax = null;

        MaterialProperty _AudioLinkOpacityBand = null;
        MaterialProperty _AudioLinkOpacityStrength = null;
        MaterialProperty _AudioLinkRemapOpacityMin = null;
        MaterialProperty _AudioLinkRemapOpacityMax = null;

        MaterialProperty _AudioLinkCutoutBand = null;
        MaterialProperty _AudioLinkCutoutStrength = null;
        MaterialProperty _AudioLinkRemapCutoutMin = null;
        MaterialProperty _AudioLinkRemapCutoutMax = null;

        MaterialProperty _AudioLinkEmissionBand = null;
        MaterialProperty _AudioLinkEmissionStrength = null;
        MaterialProperty _AudioLinkRemapEmissionMin = null;
        MaterialProperty _AudioLinkRemapEmissionMax = null;

        MaterialProperty _AudioLinkOutlineBand = null;
        MaterialProperty _AudioLinkOutlineStrength = null;
        MaterialProperty _AudioLinkRemapOutlineMin = null;
        MaterialProperty _AudioLinkRemapOutlineMax = null;

        // Stencil
        MaterialProperty _StencilRef = null;
        MaterialProperty _StencilPass = null;
        MaterialProperty _StencilFail = null;
        MaterialProperty _StencilZFail = null;
        MaterialProperty _StencilCompare = null;

        // Render Settings
        MaterialProperty _QueueOffset = null;


        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        List<ParticleSystemRenderer> m_RenderersUsingThisMaterial = new List<ParticleSystemRenderer>();
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

            if (mat.GetInt("_MaterialResetCheck") == 0){
                mat.SetInt("_MaterialResetCheck", 1);
                ApplyMaterialSettings(mat);
            }
            
            bool isXVersion = MGUI.IsXVersion(mat);

            List<ParticleSystemVertexStream> streams = new List<ParticleSystemVertexStream>();
            streams.Add(ParticleSystemVertexStream.Position);
            streams.Add(ParticleSystemVertexStream.Normal);
            streams.Add(ParticleSystemVertexStream.Tangent);
            streams.Add(ParticleSystemVertexStream.Color);
            streams.Add(ParticleSystemVertexStream.UV);
            streams.Add(ParticleSystemVertexStream.UV2);
            streams.Add(ParticleSystemVertexStream.AnimBlend);
            streams.Add(ParticleSystemVertexStream.AnimFrame);
            streams.Add(ParticleSystemVertexStream.Center);
            streams.Add(ParticleSystemVertexStream.AgePercent);
            streams.Add(ParticleSystemVertexStream.StableRandomXYZW);
            
            string warnings = "";
            List<ParticleSystemVertexStream> rendererStreams = new List<ParticleSystemVertexStream>();
            foreach (ParticleSystemRenderer renderer in m_RenderersUsingThisMaterial){
                if (renderer != null){
                    renderer.GetActiveVertexStreams(rendererStreams);
                    bool streamsValid = rendererStreams.SequenceEqual(streams);
                    if (!streamsValid) warnings += "  " + renderer.name + "\n";
                }
            }

            if (isXVersion)
                MGUI.DoHeader("PARTICLES X");
            else
                MGUI.DoHeader("PARTICLES");

            EditorGUI.BeginChangeCheck(); {

                foreach (var obj in _BlendMode.targets)
                    ApplyMaterialSettings((Material)obj);

                if (!foldouts.ContainsKey(mat))
                    foldouts.Add(mat, toggles);
                
                bool notOpaque = false;
                if (_BlendMode.floatValue != 6)
                    notOpaque = true;

                // Vertex Stream Handler
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
                    MGUI.Space4();
                }

                // Base
                bool baseToggle = Foldouts.DoFoldout(foldouts, mat, "Base", 1, Foldouts.Style.StandardButton);
                if (Foldouts.DoFoldoutButton(MGUI.collapseLabel, 11)) Toggles.CollapseFoldouts(mat, foldouts, 1);
                if (baseToggle) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.PropertyGroup(() =>{
                            me.ShaderProperty(_BlendMode, "Blending Mode");
                            if (notOpaque || _IsCutout.floatValue == 1){
                                me.ShaderProperty(_AlphaSource, "Alpha Source");
                            }
                            if (notOpaque)
                                me.ShaderProperty(_Opacity, "Opacity");
                            MGUI.ToggleSlider(me, "Cutout", _IsCutout, _Cutoff);
                            if (notOpaque)
                                MGUI.ToggleSlider(me, Tips.softening, _Softening, _SoftenStr);
                            me.ShaderProperty(_FlipbookBlending, Tips.flipbookBlending);
                            if (_IsCutout.floatValue == 1 && isXVersion)
                                me.ShaderProperty(_CutoutRim, "Cutout Rim");
                        });
                        if (_IsCutout.floatValue == 1 && _CutoutRim.floatValue == 1 && isXVersion){
                            MGUI.PropertyGroup(() =>{
                                me.ShaderProperty(_CutoutRimWidth, "Rim Width");
                                me.ShaderProperty(_CutoutRimColor, "Rim Color");
                                me.ShaderProperty(_CutoutRimBlend, "Rim Blend");
                            });
                        }
                        MGUI.PropertyGroup( () => {
                            me.TexturePropertySingleLine(texLabel, _MainTex, _Color);
                            DrawUVBlock(mat, me, _MainTex, _MainTexUVMode, _MainTexSpeed, _MainTexPolarRadius, _MainTexPolarRotation, _MainTexPolarSpeed);
                            me.TexturePropertySingleLine(tex2Label, _SecondTex, _SecondColor, _TexBlendMode);
                            MGUI.TexPropLabel("Blending", 113, true);
                            DrawUVBlock(mat, me, _SecondTex, _SecondTexUVMode, _SecondTexSpeed, _SecondTexPolarRadius, _SecondTexPolarRotation, _SecondTexPolarSpeed);
                            if (_AlphaSource.floatValue == 1 && (notOpaque || _IsCutout.floatValue == 1)){
                                me.TexturePropertySingleLine(Tips.alphaMaskText, _AlphaMask, _AlphaMaskChannel);
                                DrawUVBlock(mat, me, _AlphaMask, _AlphaMaskUVMode, _AlphaMaskSpeed, _AlphaMaskPolarRadius, _AlphaMaskPolarRotation, _AlphaMaskPolarSpeed);
                            }
                        });
                    });
                }

                // Lighting
                if (Foldouts.DoFoldout(foldouts, mat, me, _LightingToggle, "Lighting", Foldouts.Style.StandardToggle)){
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_LightingToggle.floatValue == 0);
                        MGUI.PropertyGroup(()=>{
                            me.TexturePropertySingleLine(Tips.normalMapText, _NormalMapLighting, _NormalMapLighting.textureValue ? _NormalMapLightingScale : null);
                            DrawUVBlock(mat, me, _NormalMapLighting, _NormalMapLightingUVMode, _NormalMapLightingSpeed, _NormalMapLightingPolarRadius, _NormalMapLightingPolarRotation, _NormalMapLightingPolarSpeed);
                            me.TexturePropertySingleLine(Tips.metallicText, _MetallicMap, _Metallic);
                            DrawUVBlock(mat, me, _MetallicMap, _MetallicMapUVMode, _MetallicMapSpeed, _MetallicMapPolarRadius, _MetallicMapPolarRotation, _MetallicMapPolarSpeed);
                            me.TexturePropertySingleLine(Tips.roughnessText, _RoughnessMap, _Roughness);
                            DrawUVBlock(mat, me, _RoughnessMap, _RoughnessMapUVMode, _RoughnessMapSpeed, _RoughnessMapPolarRadius, _RoughnessMapPolarRotation, _RoughnessMapPolarSpeed);
                            MGUI.ToggleFloat(me, Tips.cubemapReflectionsText, _ReflectionsToggle, _ReflectionStrength);
                            MGUI.ToggleFloat(me, Tips.specularHighlightsText, _SpecularHighlightsToggle, _SpecularHighlightStrength);
                            MGUI.ToggleFloat(me, "Light Volume Lighting", _LightVolumes, _LightVolumeStrength);
                            if (_LightVolumes.floatValue != 0)
                                MGUI.ToggleFloat(me, Tips.lightVolumeSpecText, _LightVolumeSpecularity, _LightVolumeSpecularityStrength);
                        });
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_Emission, "Emission");
                            MGUI.ToggleGroup(_Emission.floatValue == 0);
                            if (isXVersion){
                                me.ShaderProperty(_EmissionLightReactivity, "Light Reactivity");
                                if (_EmissionLightReactivity.floatValue == 1)
                                    MGUI.SliderMinMax01(_EmissionLightReactivityMin, _EmissionLightReactivityMax, "Lighting Threshold", 0);
                            }
                            me.TexturePropertySingleLine(Tips.emissionText, _EmissionMap, _EmissionColor);
                            DrawUVBlock(mat, me, _EmissionMap, _EmissionMapUVMode, _EmissionMapSpeed, _EmissionMapPolarRadius, _EmissionMapPolarRotation, _EmissionMapPolarSpeed);
                            MGUI.ToggleGroupEnd();
                        });
                        MGUI.ToggleGroupEnd();
                        MGUI.DisplayInfo("Please note that for full lighting support you need to enable the various lighting options at the bottom of the particle system 'Renderer' tab.");   
                    });
                }

                // Filtering
                if (Foldouts.DoFoldout(foldouts, mat, me, _Filtering, "Filtering", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_Filtering.floatValue == 0);
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_AutoShift, Tips.autoShift);
                            if (_AutoShift.floatValue == 1)
                                me.ShaderProperty(_AutoShiftSpeed, "Speed");
                            else
                                me.ShaderProperty(_Hue, "Hue");
                            me.ShaderProperty(_Saturation, "Saturation");
                            me.ShaderProperty(_Brightness, "Brightness");
                            me.ShaderProperty(_Contrast, "Contrast");
                            me.ShaderProperty(_HDR, "HDR");
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Distortion
                if (Foldouts.DoFoldout(foldouts, mat, me, _Distortion, "Distortion", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_Distortion.floatValue == 0);
                        MGUI.PropertyGroup(()=>{
                            me.TexturePropertySingleLine(normalLabel, _NormalMap, _DistortMainTex);
                            MGUI.TexPropLabel("Distort UVs", 124, false);
                            DrawUVBlock(mat, me, _NormalMap, _NormalMapUVMode, _NormalMapSpeed, _NormalMapPolarRadius, _NormalMapPolarRotation, _NormalMapPolarSpeed);
                            me.ShaderProperty(_DistortionStr, "Strength");
                            if (_BlendMode.floatValue != 6)
                                me.ShaderProperty(_DistortionBlend, "Blend");
                        });
                        if (_BlendMode.floatValue == 6)
                            MGUI.DisplayInfo("Please note that when the material is opaque only UV distortion is functional.");  
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Special Effects
                if (isXVersion){

                    // Dissolve
                    if (Foldouts.DoFoldout(foldouts, mat, me, _Dissolve, "Dissolve", Foldouts.Style.StandardToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_Dissolve.floatValue == 0);
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_DissolveMode, "Mode");
                                MGUI.Space2();
                                me.TexturePropertySingleLine(new GUIContent("Noise"), _DissolveNoise, _DissolveRandomOffset);
                                MGUI.TexPropLabel(Tips.randomOffsetText, 100, false);
                                DrawUVBlock(mat, me, _DissolveNoise, _DissolveNoiseUVMode, _DissolveNoiseSpeed, _DissolveNoisePolarRadius, _DissolveNoisePolarRotation, _DissolveNoisePolarSpeed);
                                if (_DissolveMode.floatValue == 1)
                                    me.ShaderProperty(_DissolveAmount, "Dissolve Amount");
                                else
                                    MGUI.SliderMinMax01(_DissolveAgeThresholdMin, _DissolveAgeThresholdMax, "Age Threshold", 0); // me.ShaderProperty(_DissolveAgeThreshold, "Age Threshold");
                            });
                            MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_DissolveRimBlend, "Rim Blending");
                            me.ShaderProperty(_DissolveRimColor, "Rim Color");
                            me.ShaderProperty(_DissolveRimWidth, "Rim Width"); 
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    // Randomn Hue
                    if (Foldouts.DoFoldout(foldouts, mat, me, _RandomHue, "Random Hue", Foldouts.Style.StandardToggle)){
                            MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_RandomHue.floatValue == 0);
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_RandomHueMode, "Mode");
                                me.ShaderProperty(_RandomHueMonoTint, Tips.monoTintText);
                                MGUI.SliderMinMax01(_RandomHueMin, _RandomHueMax, "Hue Range", 0);
                                MGUI.SliderMinMax01(_RandomSatMin, _RandomSatMax, "Saturation Range", 0);
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }
                    
                    // Outlines
                    if (Foldouts.DoFoldout(foldouts, mat, me, _Outlines, "Outlines", Foldouts.Style.StandardToggle)){
                        if (_BlendMode.floatValue != 6){
                            MGUI.Space6();
                            MGUI.DisplayError("Outlines are only available to opaque materials.");
                            MGUI.SpaceN6();
                        } 
                        MGUI.ToggleGroup(_BlendMode.floatValue != 6);
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_Outlines.floatValue == 0);
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_OutlineColor, "Color");
                                me.ShaderProperty(_OutlineThickness, "Thickness");
                                me.ShaderProperty(_OutlineStencilToggle, Tips.stencilMode);
                                if (EditorGUI.EndChangeCheck()){
                                    if (_OutlineStencilToggle.floatValue == 0)
                                        DoStencilReset();
                                    else
                                        ApplyOutlineStencilConfig(mat);
                                }
                            });
                            MGUI.ToggleGroupEnd();
                        });
                        MGUI.ToggleGroupEnd();
                    }
                }

                // Pulse
                if (Foldouts.DoFoldout(foldouts, mat, me, _Pulse, "Pulse", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_Pulse.floatValue == 0);
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_Waveform, "Waveform");
                            me.ShaderProperty(_PulseStr, "Strength");
                            me.ShaderProperty(_PulseSpeed, "Speed");
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Falloff
                if (notOpaque){
                    if (Foldouts.DoFoldout(foldouts, mat, me, _Falloff, "Falloff", Foldouts.Style.StandardToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_Falloff.floatValue == 0);
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_MinRange, "Far Min Range");
                                me.ShaderProperty(_MaxRange, "Far Max Range");
                                MGUI.Space4();
                                me.ShaderProperty(_NearMinRange, "Near Min Range");
                                me.ShaderProperty(_NearMaxRange, "Near Max Range");
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }
                }

                // Audio Link
                if (Foldouts.DoFoldout(foldouts, mat, me, _AudioLink, "Audio Link", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_AudioLink.floatValue == 0);
                        me.ShaderProperty(_AudioLinkStrength, "Global Strength");
                        MGUI.SliderMinMax01(_AudioLinkRemapMin, _AudioLinkRemapMax, "Global Remap", 1);
                        MGUI.Space4();
                        MGUI.BoldLabel("Filtering");
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_AudioLinkFilterBand, "Band");
                            me.ShaderProperty(_AudioLinkFilterStrength, "Strength");
                            MGUI.SliderMinMax01(_AudioLinkRemapFilterMin, _AudioLinkRemapFilterMax, "Remap", 1);
                        });
                        MGUI.BoldLabel("Distortion");
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_AudioLinkDistortionBand, "Band");
                            me.ShaderProperty(_AudioLinkDistortionStrength, "Strength");
                            MGUI.SliderMinMax01(_AudioLinkRemapDistortionMin, _AudioLinkRemapDistortionMax, "Remap", 1);
                        });
                        if (notOpaque){
                            MGUI.BoldLabel("Opacity");
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_AudioLinkOpacityBand, "Band");
                                me.ShaderProperty(_AudioLinkOpacityStrength, "Strength");
                                MGUI.SliderMinMax01(_AudioLinkRemapOpacityMin, _AudioLinkRemapOpacityMax, "Remap", 1);
                            });
                        }
                        MGUI.BoldLabel("Cutout");
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_AudioLinkCutoutBand, "Band");
                            me.ShaderProperty(_AudioLinkCutoutStrength, "Strength");
                            MGUI.SliderMinMax01(_AudioLinkRemapCutoutMin, _AudioLinkRemapCutoutMax, "Remap", 1);
                        });
                        if (isXVersion && !notOpaque){
                            MGUI.BoldLabel("Outlines");
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_AudioLinkOutlineBand, "Band");
                                me.ShaderProperty(_AudioLinkOutlineStrength, "Strength");
                                MGUI.SliderMinMax01(_AudioLinkRemapOutlineMin, _AudioLinkRemapOutlineMax, "Remap", 1);
                            });
                        }
                        MGUI.BoldLabel("Emission");
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_AudioLinkEmissionBand, "Band");
                            me.ShaderProperty(_AudioLinkEmissionStrength, "Strength");
                            MGUI.SliderMinMax01(_AudioLinkRemapEmissionMin, _AudioLinkRemapEmissionMax, "Remap", 1);
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Rendering
                if (Foldouts.DoFoldout(foldouts, mat, "Render Settings", Foldouts.Style.Standard)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.PropertyGroup(()=>{
                            _QueueOffset.floatValue = (int)_QueueOffset.floatValue;
                            me.ShaderProperty(_QueueOffset, Tips.queueOffset);
                            MGUI.SpaceN1();
                            MGUI.DummyProperty("Render Queue:", mat.renderQueue.ToString());
                            me.ShaderProperty(_Culling, "Culling");
                            me.ShaderProperty(_ZTest, "ZTest");
                        });
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_StencilRef, "Reference Value");
                            me.ShaderProperty(_StencilPass, "Pass Operation");
                            me.ShaderProperty(_StencilFail, "Fail Operation");
                            me.ShaderProperty(_StencilZFail, "Z Fail Operation");
                            me.ShaderProperty(_StencilCompare, "Compare Function");
                        });
                    });
                }
            }
            
            MGUI.DoFooter(versionLabel);
        }

        // Set blending mode
        public static void SetBlendMode(Material mat) {
            switch (mat.GetInt("_BlendMode")) {
                case 0:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.DisableKeyword("_ALPHA_ADD_ON");
                    mat.DisableKeyword("_ALPHA_ADD_SOFT_ON");
                    mat.DisableKeyword("_ALPHA_MUL_ON");
                    mat.DisableKeyword("_ALPHA_MULX2_ON");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+mat.GetInt("_QueueOffset");
                    break;
                case 1:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.DisableKeyword("_ALPHA_ADD_ON");
                    mat.DisableKeyword("_ALPHA_ADD_SOFT_ON");
                    mat.DisableKeyword("_ALPHA_MUL_ON");
                    mat.DisableKeyword("_ALPHA_MULX2_ON");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+mat.GetInt("_QueueOffset");
                    break;
                case 2:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.EnableKeyword("_ALPHA_ADD_ON");
                    mat.DisableKeyword("_ALPHA_ADD_SOFT_ON");
                    mat.DisableKeyword("_ALPHA_MUL_ON");
                    mat.DisableKeyword("_ALPHA_MULX2_ON");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_ZWrite", 0);
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+mat.GetInt("_QueueOffset");
                    break;
                case 3:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.DisableKeyword("_ALPHA_ADD_ON");
                    mat.EnableKeyword("_ALPHA_ADD_SOFT_ON");
                    mat.DisableKeyword("_ALPHA_MUL_ON");
                    mat.DisableKeyword("_ALPHA_MULX2_ON");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
                    mat.SetInt("_ZWrite", 0);
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+mat.GetInt("_QueueOffset");
                    break;
                case 4:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.DisableKeyword("_ALPHA_ADD_ON");
                    mat.DisableKeyword("_ALPHA_ADD_SOFT_ON");
                    mat.EnableKeyword("_ALPHA_MUL_ON");
                    mat.DisableKeyword("_ALPHA_MULX2_ON");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
                    mat.SetInt("_ZWrite", 0);
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+mat.GetInt("_QueueOffset");
                    break;
                case 5:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.DisableKeyword("_ALPHA_ADD_ON");
                    mat.DisableKeyword("_ALPHA_ADD_SOFT_ON");
                    mat.DisableKeyword("_ALPHA_MUL_ON");
                    mat.EnableKeyword("_ALPHA_MULX2_ON");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
                    mat.SetInt("_ZWrite", 0);
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+mat.GetInt("_QueueOffset");
                    break;
                case 6:
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.DisableKeyword("_ALPHA_ADD_ON");
                    mat.DisableKeyword("_ALPHA_ADD_SOFT_ON");
                    mat.DisableKeyword("_ALPHA_MUL_ON");
                    mat.DisableKeyword("_ALPHA_MULX2_ON");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    if (mat.GetInt("_IsCutout") == 1 || mat.GetInt("_Dissolve") == 1){
                        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest+mat.GetInt("_QueueOffset");
                        mat.SetOverrideTag("RenderType", "TransparentCutout");
                    }
                    else {
                        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry+mat.GetInt("_QueueOffset");
                        mat.SetOverrideTag("RenderType", "Opaque");
                    }
                    break;
            }
        }

        void ApplyMaterialSettings(Material mat){
            int blendMode = mat.GetInt("_BlendMode");
            bool softening = mat.GetInt("_Softening") == 1 && blendMode != 6;
            bool distortion = mat.GetInt("_Distortion") == 1;
            bool distortUV = mat.GetInt("_DistortMainTex") == 1 && distortion;
            bool layering = mat.GetTexture("_SecondTex");
            bool pulse = mat.GetInt("_Pulse") == 1;
            bool falloff = mat.GetInt("_Falloff") == 1 && blendMode != 6;
            bool flipbook = mat.GetInt("_FlipbookBlending") == 1;
            bool cutout = mat.GetInt("_IsCutout") == 1;
            bool alphaMask = mat.GetTexture("_AlphaMask") && (blendMode != 6 || cutout) && mat.GetInt("_AlphaSource") == 1;
            bool filtering = mat.GetInt("_Filtering") == 1;
            bool audiolink = mat.GetInt("_AudioLink") == 1;
            bool lighting = mat.GetInt("_LightingToggle") == 1;
            bool normalMap = mat.GetTexture("_NormalMapLighting") && lighting;
            bool reflections = mat.GetInt("_ReflectionsToggle") == 1 && lighting;
            bool specHighlight = mat.GetInt("_SpecularHighlightsToggle") == 1 && lighting;
            bool metallicMap = mat.GetTexture("_MetallicMap") && lighting;
            bool roughnessMap = mat.GetTexture("_RoughnessMap") && lighting;
            bool dissolve = mat.GetInt("_Dissolve") == 1;
            bool randomHue = mat.GetInt("_RandomHue") == 1;
            bool outlines = mat.GetInt("_Outlines") == 1 && blendMode == 6;
            bool outlineStencil = mat.GetInt("_OutlineStencilToggle") == 1;
            bool emission = mat.GetInt("_Emission") == 1 && lighting;

            MGUI.SetKeyword(mat, "_ALPHATEST_ON", cutout);
            MGUI.SetKeyword(mat, "_FADING_ON", softening);
            MGUI.SetKeyword(mat, "_DISTORTION_ON", distortion);
            MGUI.SetKeyword(mat, "_DISTORTION_UV_ON", distortUV);
            MGUI.SetKeyword(mat, "_LAYERED_TEX_ON", layering);
            MGUI.SetKeyword(mat, "_PULSE_ON", pulse);
            MGUI.SetKeyword(mat, "_FALLOFF_ON", falloff);
            MGUI.SetKeyword(mat, "_FLIPBOOK_BLENDING", flipbook);
            MGUI.SetKeyword(mat, "_FILTERING_ON", filtering);
            MGUI.SetKeyword(mat, "_AUDIOLINK_ON", audiolink);
            MGUI.SetKeyword(mat, "_LIGHTING_ON", lighting);
            MGUI.SetKeyword(mat, "_NORMALMAP_ON", normalMap);
            MGUI.SetKeyword(mat, "_REFLECTIONS_ON", reflections);
            MGUI.SetKeyword(mat, "_SPECULAR_HIGHLIGHTS_ON", specHighlight);
            MGUI.SetKeyword(mat, "_METALLIC_MAP_ON", metallicMap);
            MGUI.SetKeyword(mat, "_ROUGHNESS_MAP_ON", roughnessMap);
            MGUI.SetKeyword(mat, "_DISSOLVE_ON", dissolve);
            MGUI.SetKeyword(mat, "_ALPHA_MASK_ON", alphaMask);
            MGUI.SetKeyword(mat, "_RANDOM_HUE_ON", randomHue);
            MGUI.SetKeyword(mat, "_EMISSION_ON", emission);

            mat.SetShaderPassEnabled("Always", outlines);
            mat.SetShaderPassEnabled("GrabPass", distortion);

            if (outlineStencil)
                mat.SetInt("_OutlineCulling", 0);
            else
                mat.SetInt("_OutlineCulling", 1);

            if (!outlineStencil){
                mat.SetFloat("_OutlineStencilPass", mat.GetFloat("_StencilPass"));
                mat.SetFloat("_OutlineStencilCompare", mat.GetFloat("_StencilCompare"));
            }

            SetBlendMode(mat);
        }

        void ApplyOutlineStencilConfig(Material mat){
            mat.SetFloat("_StencilPass", (float)UnityEngine.Rendering.StencilOp.Replace);
            mat.SetFloat("_StencilFail", (float)UnityEngine.Rendering.StencilOp.Keep);
            mat.SetFloat("_StencilZFail", (float)UnityEngine.Rendering.StencilOp.Keep);
            mat.SetFloat("_StencilCompare", (float)UnityEngine.Rendering.CompareFunction.Always);

            mat.SetFloat("_OutlineStencilPass", (float)UnityEngine.Rendering.StencilOp.Keep);
            mat.SetFloat("_OutlineStencilCompare", (float)UnityEngine.Rendering.CompareFunction.NotEqual);
        }

        void DoStencilReset(){
            _StencilRef.floatValue = 1f;
            _StencilPass.floatValue = (float)UnityEngine.Rendering.StencilOp.Keep;
            _StencilFail.floatValue = (float)UnityEngine.Rendering.StencilOp.Keep;
            _StencilZFail.floatValue = (float)UnityEngine.Rendering.StencilOp.Keep;
            _StencilCompare.floatValue = (float)UnityEngine.Rendering.CompareFunction.Always;
            _OutlineStencilPass.floatValue = (float)UnityEngine.Rendering.StencilOp.Keep;
            _OutlineStencilCompare.floatValue = (float)UnityEngine.Rendering.CompareFunction.Always;
        }

        public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
            base.AssignNewShaderToMaterial(mat, oldShader, newShader);
            MGUI.ClearKeywords(mat);
            ApplyMaterialSettings(mat);
        }

        void DrawUVBlock(
            Material mat,
            MaterialEditor me,
            MaterialProperty texture,
            MaterialProperty uvMode,
            MaterialProperty speed,
            MaterialProperty polarRadius,
            MaterialProperty polarRotation,
            MaterialProperty polarSpeed
        ){
            if (texture.textureValue){
                if (MGUI.IsXVersion(mat)){
                    me.ShaderProperty(uvMode, "UV Mode");
                    if (uvMode.floatValue != 1){
                        MGUI.TextureSOScroll(me, texture, speed);
                    }
                    else {
                        me.ShaderProperty(polarRadius, "Radius");
                        me.ShaderProperty(polarRotation, "Rotation");
                        me.ShaderProperty(polarSpeed, "Rotation Speed");
                    }
                }
                else {
                    MGUI.TextureSOScroll(me, texture, speed);
                }
                MGUI.Space6();
            }
        }

        void CacheRenderersUsingThisMaterial(Material mat){
            m_RenderersUsingThisMaterial.Clear();

            ParticleSystemRenderer[] renderers = UnityEngine.Object.FindObjectsOfType(typeof(ParticleSystemRenderer)) as ParticleSystemRenderer[];
            foreach (ParticleSystemRenderer renderer in renderers)
            {
                if (renderer.sharedMaterial == mat)
                    m_RenderersUsingThisMaterial.Add(renderer);
            }
        }
    }
}