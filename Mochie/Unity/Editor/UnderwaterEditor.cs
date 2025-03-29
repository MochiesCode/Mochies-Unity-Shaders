using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Mochie;

namespace Mochie {

    public class UnderwaterEditor : ShaderGUI {

        string versionLabel = "v1.2";

        // Base
        // MaterialProperty _RenderMode = null;
        MaterialProperty _Color = null;
        MaterialProperty _StencilRef = null;

        // Depth of Field
        MaterialProperty _DoFToggle = null;
        MaterialProperty _DepthToggle = null;
        MaterialProperty _HQBlur = null;
        MaterialProperty _BlurStr = null;
        MaterialProperty _Radius = null;
        MaterialProperty _Fade = null;

        // Caustics
        MaterialProperty _CausticsTex = null;
        MaterialProperty _CausticsToggle = null;
        MaterialProperty _CausticsMode = null;
        MaterialProperty _CausticsOpacity = null;
        MaterialProperty _CausticsScale = null;
        MaterialProperty _CausticsSpeed = null;
        MaterialProperty _CausticsFade = null;
        MaterialProperty _CausticsDisp = null;
        MaterialProperty _CausticsDistortion = null;
        MaterialProperty _CausticsDistortionTex = null;
        MaterialProperty _CausticsDistortionScale = null;
        MaterialProperty _CausticsDistortionSpeed = null;
        MaterialProperty _CausticsRotation = null;
        MaterialProperty _CausticsColor = null;
        MaterialProperty _CausticsPower = null;
        MaterialProperty _CausticsTexArray = null;
        MaterialProperty _CausticsFlipbookSpeed = null;
        MaterialProperty _CausticsFlipbookDisp = null;

        // Fog
        MaterialProperty _FogToggle = null;
        MaterialProperty _FogTint = null;
        MaterialProperty _FogOpacity = null;
        MaterialProperty _FogRadius = null;
        MaterialProperty _FogFade = null;

        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        bool m_FirstTimeApply = true;

        public override void OnGUI(MaterialEditor me, MaterialProperty[] props) {
            if (!me.isVisible)
                return;

            foreach (var property in GetType().GetFields(bindingFlags)){
                if (property.FieldType == typeof(MaterialProperty))
                    property.SetValue(this, FindProperty(property.Name, props));
            }
            Material mat = (Material)me.target;
            if (m_FirstTimeApply){
                m_FirstTimeApply = false;
            }

            if (mat.GetInt("_MaterialResetCheck") == 0){
                mat.SetInt("_MaterialResetCheck", 1);
                ApplyMaterialSettings(mat);
            }

            EditorGUI.BeginChangeCheck(); {

                mat.SetShaderPassEnabled("Always", mat.GetInt("_DoFToggle") == 1);
                
                MGUI.BoldLabel("Base");
                MGUI.PropertyGroup(()=>{
                    // me.ShaderProperty(_RenderMode, "Mesh Rendering Mode");
                    me.ShaderProperty(_Color, "Color");
                    me.ShaderProperty(_StencilRef, "Stencil Reference");
                });
                MGUI.Space10();

                MGUI.BoldLabel("Blur");
                me.ShaderProperty(_DoFToggle, "Enable");
                MGUI.ToggleGroup(_DoFToggle.floatValue == 0f);
                MGUI.PropertyGroup(()=>{
                    me.ShaderProperty(_HQBlur, "High Quality");
                    me.ShaderProperty(_DepthToggle, "Depth of Field");
                    me.ShaderProperty(_BlurStr, "Strength");
                    if (_DepthToggle.floatValue == 1){
                        me.ShaderProperty(_Radius, "Radius");
                        me.ShaderProperty(_Fade, "Fade");
                    }
                });
                MGUI.ToggleGroupEnd();
                MGUI.Space10();

                MGUI.BoldLabel("Caustics");
                me.ShaderProperty(_CausticsToggle, "Enable");
                MGUI.ToggleGroup(_CausticsToggle.floatValue == 0f);
                MGUI.PropertyGroup(()=>{
                    me.ShaderProperty(_CausticsMode, "Style");
                    if (_CausticsMode.floatValue == 1){
                        me.TexturePropertySingleLine(new GUIContent("Caustics Texture"), _CausticsTex);
                    }
                    else if (_CausticsMode.floatValue == 2){
                        me.TexturePropertySingleLine(new GUIContent("Caustics Flipbook"), _CausticsTexArray);
                    }
                    me.ShaderProperty(_CausticsColor, "Color");
                    me.ShaderProperty(_CausticsOpacity, "Strength");
                    
                    if (_CausticsMode.floatValue == 0){
                        me.ShaderProperty(_CausticsPower, "Power");
                        me.ShaderProperty(_CausticsDisp, "Dispersion");
                    }
                    if (_CausticsMode.floatValue != 2)
                        me.ShaderProperty(_CausticsSpeed, "Speed");
                    else {
                        me.ShaderProperty(_CausticsFlipbookSpeed, "Speed");
                        me.ShaderProperty(_CausticsFlipbookDisp, "Dispersion");
                    }
                    me.ShaderProperty(_CausticsScale, "Scale");
                    me.ShaderProperty(_CausticsFade, Tips.causticsFade);
                    // me.ShaderProperty(_CausticsSurfaceFade, Tips.causticsSurfaceFade);
                    MGUI.Vector3Field(_CausticsRotation, "Rotation", false);
                    if (_CausticsMode.floatValue != 2){
                        me.ShaderProperty(_CausticsDistortion, "Distortion Strength");
                        me.ShaderProperty(_CausticsDistortionScale, "Distortion Scale");
                        MGUI.Vector2Field(_CausticsDistortionSpeed, "Distortion Speed");
                        me.TexturePropertySingleLine(new GUIContent("Distortion Texture"), _CausticsDistortionTex);
                    }
                });
                MGUI.ToggleGroupEnd();
                MGUI.Space10();

                MGUI.BoldLabel("Fog");
                me.ShaderProperty(_FogToggle, "Enable");
                MGUI.ToggleGroup(_FogToggle.floatValue == 0f);
                MGUI.PropertyGroup(()=>{
                    me.ShaderProperty(_FogTint, "Color");
                    me.ShaderProperty(_FogOpacity, "Opacity");
                    me.ShaderProperty(_FogRadius, "Radius");
                    me.ShaderProperty(_FogFade, "Fade");
                });
                MGUI.ToggleGroupEnd();
            }
            ApplyMaterialSettings(mat);
            MGUI.Space10();
            MGUI.DisplayInfo("   This shader requires a \"Depth Light\" prefab be present in the scene.\n   (Found in: Assets/Mochie/Unity/Prefabs)");
            MGUI.DoFooter(versionLabel);
        }

        public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
            base.AssignNewShaderToMaterial(mat, oldShader, newShader);
            MGUI.ClearKeywords(mat);
        }

        void ApplyMaterialSettings(Material mat){
            int causticsMode = mat.GetInt("_CausticsMode");
            MGUI.SetKeyword(mat, "_CAUSTICS_VORONOI_ON", causticsMode == 0);
            MGUI.SetKeyword(mat, "_CAUSTICS_TEXTURE_ON", causticsMode == 1);
            MGUI.SetKeyword(mat, "_CAUSTICS_FLIPBOOK_ON", causticsMode == 2);
        }
    }
}