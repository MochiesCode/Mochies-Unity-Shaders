using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Mochie;

namespace Mochie {
        
    public class GlassEditor : ShaderGUI {

        string versionLabel = "v1.9";

        // Surface
        MaterialProperty _GrabpassTint = null;
        MaterialProperty _SpecularityTint = null;
        MaterialProperty _BaseColorTint = null;
        MaterialProperty _BaseColor = null;
        MaterialProperty _RoughnessMap = null;
        MaterialProperty _MetallicMap = null;
        MaterialProperty _OcclusionMap = null;
        MaterialProperty _NormalMap = null;
        MaterialProperty _Roughness = null;
        MaterialProperty _Metallic = null;
        MaterialProperty _Occlusion = null;
        MaterialProperty _NormalStrength = null;
        MaterialProperty _Refraction = null;
        MaterialProperty _Blur = null;
        MaterialProperty _BlurQuality = null;
        MaterialProperty _RefractionIOR = null;
        MaterialProperty _RefractVertexNormal = null;

        // Rain
        // MaterialProperty _RainToggle = null;
        MaterialProperty _Speed = null;
        MaterialProperty _XScale = null;
        MaterialProperty _YScale = null;
        MaterialProperty _Strength = null;
        MaterialProperty _RainMode = null;
        MaterialProperty _RainMask = null;
        MaterialProperty _RainMaskChannel = null;
        MaterialProperty _RippleScale = null;
        MaterialProperty _RippleSpeed = null;
        MaterialProperty _RippleStrength = null;
        MaterialProperty _RippleSize = null;
        MaterialProperty _RippleDensity = null;
        MaterialProperty _DynamicDroplets = null;
        // MaterialProperty _RainBias = null;
        MaterialProperty _RainThreshold = null;
        MaterialProperty _RainThresholdSize = null;

        // Render Settings
        MaterialProperty _ReflectionsToggle = null;
        MaterialProperty _ReflectionStrength = null;
        MaterialProperty _SpecularToggle = null;
        MaterialProperty _SpecularStrength = null;
        MaterialProperty _GSAAToggle = null;
        MaterialProperty _GSAAStrength = null;
        MaterialProperty _Culling = null;
        MaterialProperty _SamplingMode = null;
        MaterialProperty _BlendMode = null;
        MaterialProperty _LitBaseColor = null;
        MaterialProperty _QueueOffset = null;
        MaterialProperty _TexCoordSpace = null;
        MaterialProperty _TexCoordSpaceSwizzle = null;
        MaterialProperty _GlobalTexCoordScale = null;

        // Area Lit
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

        // LTCGI
        MaterialProperty _LTCGI = null;
        MaterialProperty _LTCGIStrength = null;
        MaterialProperty _LTCGIRoughness = null;
        MaterialProperty _LTCGI_SpecularColor = null;

        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public override void OnGUI(MaterialEditor me, MaterialProperty[] props) {
            if (!me.isVisible)
                return;

            foreach (var property in GetType().GetFields(bindingFlags)){
                if (property.FieldType == typeof(MaterialProperty))
                    property.SetValue(this, FindProperty(property.Name, props));
            }
            Material mat = (Material)me.target;
            bool isTwoPass = mat.shader.name.Contains("(Two Pass)"); 

            if (mat.GetInt("_MaterialResetCheck") == 0){
                mat.SetInt("_MaterialResetCheck", 1);
                SetKeywords(mat);
                SetBlendMode(mat);
            }

            EditorGUI.BeginChangeCheck(); {
                
                // Surface
                MGUI.BoldLabel("Surface");
                MGUI.PropertyGroup(()=>{
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_BlendMode, "Transparency");
                        if (_BlendMode.floatValue == 0)
                            me.ShaderProperty(_GrabpassTint, "Grabpass Tint");
                        me.ShaderProperty(_SpecularityTint, "Specularity Tint");
                        me.ShaderProperty(_BaseColorTint, "Base Color Tint");
                    });
                    MGUI.PropertyGroup(()=>{
                        me.TexturePropertySingleLine(Tips.baseColorLabel, _BaseColor, _LitBaseColor);
                        MGUI.TexPropLabel(Tips.litBaseColorText, 100, false);
                        MGUI.TextureSO(me, _BaseColor, _BaseColor.textureValue);
                        me.TexturePropertySingleLine(Tips.metallicText, _MetallicMap, _Metallic);
                        MGUI.TextureSO(me, _MetallicMap, _MetallicMap.textureValue);
                        me.TexturePropertySingleLine(Tips.roughnessTexLabel, _RoughnessMap, _Roughness);
                        MGUI.TextureSO(me, _RoughnessMap, _RoughnessMap.textureValue);
                        me.TexturePropertySingleLine(Tips.occlusionTexLabel, _OcclusionMap, _OcclusionMap.textureValue ? _Occlusion : null);
                        MGUI.TextureSO(me, _OcclusionMap, _OcclusionMap.textureValue);
                        me.TexturePropertySingleLine(Tips.normalMapText, _NormalMap, _NormalMap.textureValue ? _NormalStrength : null);
                        MGUI.TextureSO(me, _NormalMap, _NormalMap.textureValue);
                    });
                    if (_BlendMode.floatValue == 0){
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_BlurQuality, "Blur Quality");
                            me.ShaderProperty(_Blur, "Blur Strength");
                            me.ShaderProperty(_Refraction, Tips.normalRefractionText);
                            if (!isTwoPass)
                                MGUI.ToggleFloat(me, Tips.meshRefractionText, _RefractVertexNormal, _RefractionIOR);
                        });
                    }
                    MGUI.SpaceN2();
                });
                MGUI.Space10();

                // Rain
                MGUI.BoldLabel("Rain");
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
                    MGUI.SpaceN2();
                });
                MGUI.Space10();

                // LTCGI
                if (Shader.Find("LTCGI/Blur Prefilter") != null){
                    MGUI.ShaderPropertyBold(me, _LTCGI, "LTCGI");
                    MGUI.PropertyGroup(()=>{
                        MGUI.ToggleGroup(_LTCGI.floatValue == 0);
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_LTCGIStrength, "Strength");
                            me.ShaderProperty(_LTCGIRoughness, "Roughness Multiplier");
                            me.ShaderProperty(_LTCGI_SpecularColor, "Tint");
                        });
                        MGUI.ToggleGroupEnd();
                        MGUI.SpaceN2();
                    });
                    MGUI.Space10();
                }
                else {
                    _LTCGI.floatValue = 0;
                    mat.SetInt("_LTCGI", 0);
                    mat.DisableKeyword("LTCGI");
                }

                // AreaLit
                if (Shader.Find("AreaLit/Standard") != null){
                    MGUI.ShaderPropertyBold(me, _AreaLitToggle, "AreaLit");
                    MGUI.PropertyGroup(()=>{
                        MGUI.ToggleGroup(_AreaLitToggle.floatValue == 0);
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
                        MGUI.DisplayInfo("Note that the AreaLit package files MUST be inside a folder named AreaLit (case sensitive) directly in the Assets folder (Assets/AreaLit)");
                        MGUI.ToggleGroupEnd();
                    });
                }
                else {
                    _AreaLitToggle.floatValue = 0f;
                    mat.SetInt("_AreaLitToggle", 0);
                    mat.DisableKeyword("_AREALIT_ON");
                }
                MGUI.Space10();

                // Render Settings
                MGUI.BoldLabel("Render Settings");
                MGUI.PropertyGroup(()=>{
                    MGUI.PropertyGroup(()=>{
                        MGUI.ToggleFloat(me, Tips.cubemapReflectionsText, _ReflectionsToggle, _ReflectionStrength);
                        MGUI.ToggleFloat(me, Tips.specularHighlightsText, _SpecularToggle, _SpecularStrength);
                        MGUI.ToggleFloat(me, Tips.gsaa, _GSAAToggle, _GSAAStrength);
                    });
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_SamplingMode, Tips.samplingModeText);
                        me.ShaderProperty(_TexCoordSpace, Tips.texCoordSpaceText);
                        if (_TexCoordSpace.floatValue == 1f){
                            me.ShaderProperty(_TexCoordSpaceSwizzle, Tips.swizzleText);
                        }
                        me.ShaderProperty(_GlobalTexCoordScale, "Texture Coordinate Scale");
                    });
                    MGUI.PropertyGroup(()=>{
                        MGUI.SpaceN1();
                        _QueueOffset.floatValue = (int)_QueueOffset.floatValue;
                        MGUI.DummyProperty("Render Queue:", mat.renderQueue.ToString());
                        me.ShaderProperty(_QueueOffset, Tips.queueOffset);
                        if (!isTwoPass)
                            me.ShaderProperty(_Culling, "Culling Mode");
                    });
                    MGUI.SpaceN2();
                });
                MGUI.ToggleGroupEnd();
            }
            if (EditorGUI.EndChangeCheck()){
                SetKeywords(mat);
            }
            SetBlendMode(mat);
            MGUI.Space10();
            MGUI.DoFooter(versionLabel);
        }

        void RainDroplets(MaterialEditor me){
            MGUI.ToggleGroup(_RainMode.floatValue == 0);
            MGUI.PropertyGroup(()=>{
                if (_RainMode.floatValue != 3){
                    me.TexturePropertySingleLine(Tips.maskText, _RainMask, _RainMaskChannel);
                    MGUI.TextureSO(me, _RainMask, _RainMask.textureValue);
                }
                me.ShaderProperty(_Strength, "Strength");
                me.ShaderProperty(_Speed, "Speed");
                me.ShaderProperty(_XScale, "X Scale");
                me.ShaderProperty(_YScale, "Y Scale");
                me.ShaderProperty(_DynamicDroplets, "Dynamic Droplets");
            });
            MGUI.ToggleGroupEnd();
        }

        void RainRipples(MaterialEditor me){
            MGUI.PropertyGroup(()=>{
                if (_RainMode.floatValue != 3){
                    me.TexturePropertySingleLine(Tips.maskText, _RainMask, _RainMaskChannel);
                    MGUI.TextureSO(me, _RainMask, _RainMask.textureValue);
                }
                me.ShaderProperty(_RippleStrength, "Strength");
                me.ShaderProperty(_RippleSpeed, "Speed");
                me.ShaderProperty(_RippleScale, "Scale");
                me.ShaderProperty(_RippleDensity, "Density");
                me.ShaderProperty(_RippleSize, "Size");
            });
        }

        void RainBoth(MaterialEditor me){
            MGUI.PropertyGroup(()=>{
                me.TexturePropertySingleLine(Tips.maskText, _RainMask, _RainMaskChannel);
                MGUI.TextureSO(me, _RainMask, _RainMask.textureValue);
                me.ShaderProperty(_RainThreshold, "Angle Threshold");
                me.ShaderProperty(_RainThresholdSize, "Threshold Blend");
            });
        }

        void SetKeywords(Material mat){
            int rainMode = mat.GetInt("_RainMode");
            int blurMode = mat.GetInt("_BlurQuality");
            MGUI.SetKeyword(mat, "_STOCHASTIC_SAMPLING_ON", mat.GetInt("_SamplingMode") == 1);
            MGUI.SetKeyword(mat, "_NORMALMAP_ON", mat.GetTexture("_NormalMap"));
            MGUI.SetKeyword(mat, "_RAIN_ON", rainMode > 0);
            MGUI.SetKeyword(mat, "_RAINMODE_RIPPLE", mat.GetInt("_RainMode") == 2);
            MGUI.SetKeyword(mat, "_RAINMODE_AUTO", mat.GetInt("_RainMode") == 3);
            MGUI.SetKeyword(mat, "_BLURQUALITY_LOW", blurMode == 0);
            MGUI.SetKeyword(mat, "_BLURQUALITY_MED", blurMode == 1);
            MGUI.SetKeyword(mat, "_BLURQUALITY_HIGH", blurMode == 2);
            MGUI.SetKeyword(mat, "_BLURQUALITY_ULTRA", blurMode == 3);
            MGUI.SetKeyword(mat, "_AREALIT_ON", mat.GetInt("_AreaLitToggle") == 1);
            MGUI.SetKeyword(mat, "LTCGI", mat.GetInt("_LTCGI") == 1);
            MGUI.SetKeyword(mat, "_REFLECTIONS_ON", mat.GetInt("_ReflectionsToggle") == 1);
            MGUI.SetKeyword(mat, "_SPECULAR_HIGHLIGHTS_ON", mat.GetInt("_SpecularToggle") == 1);
        }

        void SetBlendMode(Material mat){
            int blendMode = mat.GetInt("_BlendMode");
            // float roughness = mat.GetFloat("_Roughness");
            // bool rainToggle = mat.GetInt("_RainMode") > 0;
            // bool hasNormal = mat.GetTexture("_NormalMap") && mat.GetFloat("_NormalStrength") > 0;
            // bool canGrab = roughness > 0 || rainToggle || hasNormal;
            // if (blendMode == 0) blendMode = canGrab ? blendMode : 1;
            #if UNITY_ANDROID
                if (blendMode == 0){
                    mat.SetInt("_BlendMode", 1);
                    blendMode = 1;
                }
            #endif

            switch (blendMode){
                case 0: // Grabpass
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 0);
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+mat.GetInt("_QueueOffset");
                    mat.SetShaderPassEnabled("Always", true);
                    MGUI.SetKeyword(mat, "_GRABPASS_ON", true);
                    break;
                case 1: // Premultiplied
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+mat.GetInt("_QueueOffset");
                    mat.SetShaderPassEnabled("Always", false);
                    MGUI.SetKeyword(mat, "_GRABPASS_ON", false);
                    break;
                case 2: // Opaque
                    mat.SetOverrideTag("RenderType", "Opaque");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry+mat.GetInt("_QueueOffset");
                    mat.SetShaderPassEnabled("Always", false);
                    MGUI.SetKeyword(mat, "_GRABPASS_ON", false);
                    break;
                default: break;
            }
        }

        public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
            base.AssignNewShaderToMaterial(mat, oldShader, newShader);
            MGUI.ClearKeywords(mat);
            SetBlendMode(mat);
            SetKeywords(mat);
        }
    }
}