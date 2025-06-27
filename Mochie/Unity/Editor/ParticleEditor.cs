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
        GUIContent applyStreamsText = new GUIContent("Fix Vertex Streams", "Apply the vertex stream layout to all Particle Systems using this material");

        static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
        Toggles toggles = new Toggles(new string[] {
                "Base", 
                "Filtering", 
                "Distortion", 
                "Pulse", 
                "Falloff",
                "Audio Link",
                "Filtering ",
                "Distortion ",
                "Opacity",
                "Cutout",
                "RENDER SETTINGS"
        }, 0);

        string versionLabel = "v2.2.1";

        // Render Settings
        MaterialProperty _BlendMode = null;
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
        MaterialProperty _NormalMapLighting = null;
        MaterialProperty _NormalMapLightingScale = null;
        MaterialProperty _SecondTex = null;
        MaterialProperty _SecondTexScroll = null;
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
            
            List<ParticleSystemVertexStream> streams = new List<ParticleSystemVertexStream>();
            streams.Add(ParticleSystemVertexStream.Position);
            streams.Add(ParticleSystemVertexStream.UV);
            streams.Add(ParticleSystemVertexStream.UV2);
            streams.Add(ParticleSystemVertexStream.AnimBlend);
            streams.Add(ParticleSystemVertexStream.Custom1XYZ);
            streams.Add(ParticleSystemVertexStream.Center);
            streams.Add(ParticleSystemVertexStream.Color);
            streams.Add(ParticleSystemVertexStream.Tangent);
            streams.Add(ParticleSystemVertexStream.Normal);

            string warnings = "";
            List<ParticleSystemVertexStream> rendererStreams = new List<ParticleSystemVertexStream>();
            foreach (ParticleSystemRenderer renderer in m_RenderersUsingThisMaterial){
                if (renderer != null){
                    renderer.GetActiveVertexStreams(rendererStreams);
                    bool streamsValid = rendererStreams.SequenceEqual(streams);
                    if (!streamsValid) warnings += "  " + renderer.name + "\n";
                }
            }

            MGUI.DoHeader("PARTICLES");

            EditorGUI.BeginChangeCheck(); {

                foreach (var obj in _BlendMode.targets)
                    ApplyMaterialSettings((Material)obj);

                if (!foldouts.ContainsKey(mat))
                    foldouts.Add(mat, toggles);


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
                            me.ShaderProperty(_Opacity, "Opacity");
                            MGUI.ToggleSlider(me, "Cutout", _IsCutout, _Cutoff);
                            MGUI.ToggleSlider(me, Tips.softening, _Softening, _SoftenStr);
                            me.ShaderProperty(_FlipbookBlending, Tips.flipbookBlending);
                            me.ShaderProperty(_LightingToggle, "Lighting");
                        });
                        MGUI.PropertyGroup( () => {
                            me.TexturePropertySingleLine(texLabel, _MainTex, _Color);
                            me.TexturePropertySingleLine(Tips.normalMapText, _NormalMapLighting, _NormalMapLighting.textureValue ? _NormalMapLightingScale : null);
                            me.TexturePropertySingleLine(tex2Label, _SecondTex, _SecondColor, _TexBlendMode);
                            MGUI.TexPropLabel("Blending", 113, true);
                            if (_SecondTex.textureValue){
                                MGUI.TextureSOScroll(me, _SecondTex, _SecondTexScroll);
                            }
                        });
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
                            MGUI.Vector2Field(_NormalMapScale, "Scale");
                            MGUI.SpaceN3();
                            MGUI.Vector2Field(_DistortionSpeed, "Scrolling");
                            me.ShaderProperty(_DistortionStr, "Strength");
                            me.ShaderProperty(_DistortionBlend, "Blend");
                            MGUI.ToggleGroupEnd();
                        });
                        MGUI.DisplayInfo("Please note that when utilizing texture sheet animation the normal map used for distortion must also be a texture sheet and be matching the dimensions/tiles set for the base color in the particle system settings.");
                    });
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
                if (Foldouts.DoFoldout(foldouts, mat, me, _Falloff, "Falloff", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_Falloff.floatValue == 0);
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_FalloffMode, Tips.falloffMode);
                            MGUI.Space4();
                            me.ShaderProperty(_MinRange, "Far Min Range");
                            me.ShaderProperty(_MaxRange, "Far Max Range");
                            MGUI.Space4();
                            me.ShaderProperty(_NearMinRange, "Near Min Range");
                            me.ShaderProperty(_NearMaxRange, "Near Max Range");
                        });
                        MGUI.ToggleGroupEnd();
                    });
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
                        MGUI.BoldLabel("Opacity");
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_AudioLinkOpacityBand, "Band");
                            me.ShaderProperty(_AudioLinkOpacityStrength, "Strength");
                            MGUI.SliderMinMax01(_AudioLinkRemapOpacityMin, _AudioLinkRemapOpacityMax, "Remap", 1);
                        });
                        MGUI.BoldLabel("Cutout");
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_AudioLinkCutoutBand, "Band");
                            me.ShaderProperty(_AudioLinkCutoutStrength, "Strength");
                            MGUI.SliderMinMax01(_AudioLinkRemapCutoutMin, _AudioLinkRemapCutoutMax, "Remap", 1);
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Rendering
                if (Foldouts.DoFoldout(foldouts, mat, "Render Settings", Foldouts.Style.Standard)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.PropertyGroup(()=>{
                            me.RenderQueueField();
                            me.ShaderProperty(_Culling, "Culling");
                            me.ShaderProperty(_ZTest, "ZTest");
                        });
                    });
                }
            }

            MGUI.DoFooter(versionLabel);
        }

        // Set blending mode
        public static void SetBlendMode(Material material) {
            switch (material.GetInt("_BlendMode")) {
                case 0:
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHA_ADD_ON");
                    material.DisableKeyword("_ALPHA_ADD_SOFT_ON");
                    material.DisableKeyword("_ALPHA_MUL_ON");
                    material.DisableKeyword("_ALPHA_MULX2_ON");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    break;
                case 1:
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHA_ADD_ON");
                    material.DisableKeyword("_ALPHA_ADD_SOFT_ON");
                    material.DisableKeyword("_ALPHA_MUL_ON");
                    material.DisableKeyword("_ALPHA_MULX2_ON");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    break;
                case 2:
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.EnableKeyword("_ALPHA_ADD_ON");
                    material.DisableKeyword("_ALPHA_ADD_SOFT_ON");
                    material.DisableKeyword("_ALPHA_MUL_ON");
                    material.DisableKeyword("_ALPHA_MULX2_ON");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    break;
                case 3:
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHA_ADD_ON");
                    material.EnableKeyword("_ALPHA_ADD_SOFT_ON");
                    material.DisableKeyword("_ALPHA_MUL_ON");
                    material.DisableKeyword("_ALPHA_MULX2_ON");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
                    break;
                case 4:
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHA_ADD_ON");
                    material.DisableKeyword("_ALPHA_ADD_SOFT_ON");
                    material.EnableKeyword("_ALPHA_MUL_ON");
                    material.DisableKeyword("_ALPHA_MULX2_ON");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
                    break;
                case 5:
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHA_ADD_ON");
                    material.DisableKeyword("_ALPHA_ADD_SOFT_ON");
                    material.DisableKeyword("_ALPHA_MUL_ON");
                    material.EnableKeyword("_ALPHA_MULX2_ON");
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
            bool audiolink = mat.GetInt("_AudioLink") == 1;
            bool lighting = mat.GetInt("_LightingToggle") == 1;
            bool normalMap = mat.GetTexture("_NormalMapLighting") && lighting;

            MGUI.SetKeyword(mat, "_ALPHATEST_ON", cutout);
            MGUI.SetKeyword(mat, "_FADING_ON", softening);
            MGUI.SetKeyword(mat, "_DISTORTION_ON", distortion);
            MGUI.SetKeyword(mat, "_DISTORTION_UV_ON", distortUV);
            MGUI.SetKeyword(mat, "_LAYERED_TEX_ON", layering);
            MGUI.SetKeyword(mat, "_PULSE_ON", pulse);
            MGUI.SetKeyword(mat, "_FALLOFF_ON", falloff);
            MGUI.SetKeyword(mat, "_FLIPBOOK_BLENDING_ON", flipbook);
            MGUI.SetKeyword(mat, "_FILTERING_ON", filtering);
            MGUI.SetKeyword(mat, "_AUDIOLINK_ON", audiolink);
            MGUI.SetKeyword(mat, "_LIGHTING_ON", lighting);
            MGUI.SetKeyword(mat, "_NORMALMAP_ON", normalMap);
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
    }
}