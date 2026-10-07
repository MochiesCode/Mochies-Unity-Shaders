using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Mochie;

namespace Mochie {

    public class UnderwaterEditor : ShaderGUI {

        GUIContent causticsTexLabel = new GUIContent("Caustics Texture");
        GUIContent causticsFlipbookLabel = new GUIContent("Caustics Flipbook");
        GUIContent distortionTexLabel = new GUIContent("Distortion Texture");

        public static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
        Toggles toggles = new Toggles(new string[] {
            "Base",
            "Blur",
            "Caustics",
            "Fog"
        }, 0);

        string versionLabel = "v1.3";

        // Base
        MaterialProperty _StencilRef = null;

        // Depth of Field
        MaterialProperty _DoFToggle = null;
        MaterialProperty _DepthToggle = null;
        MaterialProperty _HQBlur = null;
        MaterialProperty _BlurStr = null;
        MaterialProperty _Radius = null;
        MaterialProperty _Fade = null;
        MaterialProperty _Color = null;
        MaterialProperty _AutoShift = null;
        MaterialProperty _AutoShiftSpeed = null;
        MaterialProperty _Hue = null;
        MaterialProperty _HueMode = null;
        MaterialProperty _MonoTint = null;

        // Caustics
        MaterialProperty _CausticsTex = null;
        MaterialProperty _CausticsToggle = null;
        MaterialProperty _CausticsMode = null;
        MaterialProperty _CausticsOpacity = null;
        MaterialProperty _CausticsScale = null;
        MaterialProperty _CausticsSpeed = null;
        MaterialProperty _CausticsDisp = null;
        MaterialProperty _CausticsDistortion = null;
        MaterialProperty _CausticsDistortionTex = null;
        MaterialProperty _CausticsDistortionScale = null;
        MaterialProperty _CausticsDistortionSpeed = null;
        MaterialProperty _CausticsRotateWithLight = null;
        MaterialProperty _CausticsRotation = null;
        MaterialProperty _CausticsColor = null;
        MaterialProperty _CausticsPower = null;
        MaterialProperty _CausticsTexArray = null;
        MaterialProperty _CausticsFlipbookSpeed = null;
        MaterialProperty _CausticsFlipbookDisp = null;
        MaterialProperty _CausticsRange = null;
        MaterialProperty _CausticsFade = null;

        // Fog
        MaterialProperty _FogToggle = null;
        MaterialProperty _FogTint = null;
        MaterialProperty _FogOpacity = null;
        MaterialProperty _FogRadius = null;
        MaterialProperty _FogFade = null;

        // Falloff
        MaterialProperty _Falloff = null;
        MaterialProperty _BoxSize = null;
        MaterialProperty _BoxOffset = null;

        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public override void OnGUI(MaterialEditor me, MaterialProperty[] props) {
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
                ApplyMaterialSettings(mat);
            }

            MGUI.DoHeader("UNDERWATER");

            EditorGUI.BeginChangeCheck(); {

                // Base
                bool baseToggle = Foldouts.DoFoldout(foldouts, mat, "Base", 1, Foldouts.Style.StandardButton);
                if (Foldouts.DoFoldoutButton(MGUI.collapseLabel, 11)) Toggles.CollapseFoldouts(mat, foldouts, 1);
                if (baseToggle) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.PropertyGroup(()=>{
                            me.RenderQueueField();
                            me.ShaderProperty(_StencilRef, "Stencil Reference");
                            me.ShaderProperty(_Falloff, "Falloff");
                            if (_Falloff.floatValue == 1f){
                                MGUI.Vector3Field(_BoxSize, "Size", false);
                                MGUI.Vector3Field(_BoxOffset, "Offset", false);
                            }
                        });
                        MGUI.DisplayInfo("   This shader requires a \"Depth Light\" prefab be present in the scene.\n   (Found in: Assets/Mochie/Unity/Prefabs)");
                    });
                }

                // Blur
                if (Foldouts.DoFoldout(foldouts, mat, me, _DoFToggle, "Blur", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_DoFToggle.floatValue == 0);
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_BlurStr, "Strength");
                            me.ShaderProperty(_HQBlur, "High Quality");
                            me.ShaderProperty(_DepthToggle, "Depth of Field");
                            if (_DepthToggle.floatValue == 1){
                                me.ShaderProperty(_Radius, "Radius");
                                me.ShaderProperty(_Fade, "Fade");
                            }
                        });
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_Color, "Screen Tint");
                            if (_AutoShift.floatValue == 0)
                                me.ShaderProperty(_Hue, "Hue");
                            else
                                me.ShaderProperty(_AutoShiftSpeed, "Shift Speed");
                            me.ShaderProperty(_HueMode, Tips.hueModeText);
                            me.ShaderProperty(_MonoTint, Tips.monoTintText);
                            me.ShaderProperty(_AutoShift, "Auto Hue Shift");
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Caustics
                if (Foldouts.DoFoldout(foldouts, mat, me, _CausticsToggle, "Caustics", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_CausticsToggle.floatValue == 0);
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_CausticsMode, "Style");
                            if (_CausticsMode.floatValue == 1){
                                me.TexturePropertySingleLine(causticsTexLabel, _CausticsTex);
                            }
                            else if (_CausticsMode.floatValue == 2){
                                me.TexturePropertySingleLine(causticsFlipbookLabel, _CausticsTexArray);
                            }
                            me.ShaderProperty(_CausticsColor, "Color");
                            me.ShaderProperty(_CausticsOpacity, "Strength");
                            me.ShaderProperty(_CausticsRange, "Range");
                            me.ShaderProperty(_CausticsFade, "Fade");
                            
                            if (_CausticsMode.floatValue == 0){
                                me.ShaderProperty(_CausticsPower, "Power");
                                me.ShaderProperty(_CausticsDisp, "Dispersion");
                            }
                            if (_CausticsMode.floatValue != 2){
                                me.ShaderProperty(_CausticsSpeed, "Speed");
                                me.ShaderProperty(_CausticsScale, "Scale");
                            }
                            else {
                                me.ShaderProperty(_CausticsFlipbookSpeed, "Speed");
                                me.ShaderProperty(_CausticsScale, "Scale");
                                me.ShaderProperty(_CausticsFlipbookDisp, "Dispersion");
                            }
                            
                            MGUI.ToggleGroup(_CausticsRotateWithLight.floatValue == 1);
                            MGUI.Vector3Field(_CausticsRotation, Tips.causticsRotation, false);
                            MGUI.ToggleGroupEnd();
                            me.ShaderProperty(_CausticsRotateWithLight, Tips.causticsRotateWithLight);
                        });
                        if (_CausticsMode.floatValue != 2){
                            MGUI.PropertyGroup(()=>{
                                me.TexturePropertySingleLine(distortionTexLabel, _CausticsDistortionTex);
                                me.ShaderProperty(_CausticsDistortion, "Distortion Strength");
                                me.ShaderProperty(_CausticsDistortionScale, "Distortion Scale");
                                MGUI.Vector2Field(_CausticsDistortionSpeed, "Distortion Speed");
                            });
                        }
                        MGUI.ToggleGroupEnd();
                    });
                }

                // Fog
                if (Foldouts.DoFoldout(foldouts, mat, me, _FogToggle, "Fog", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_FogToggle.floatValue == 0);
                        MGUI.PropertyGroup(()=>{
                            me.ShaderProperty(_FogTint, "Color");
                            me.ShaderProperty(_FogOpacity, "Opacity");
                            me.ShaderProperty(_FogRadius, "Radius");
                            me.ShaderProperty(_FogFade, "Fade");
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }

            }
            ApplyMaterialSettings(mat);
            MGUI.DoFooter(versionLabel);
        }

        public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
            base.AssignNewShaderToMaterial(mat, oldShader, newShader);
            MGUI.ClearKeywords(mat);
            ApplyMaterialSettings(mat);
        }

        void ApplyMaterialSettings(Material mat){
            mat.SetShaderPassEnabled("Always", mat.GetInt("_DoFToggle") == 1);
            int causticsMode = mat.GetInt("_CausticsMode");
            MGUI.SetKeyword(mat, "_CAUSTICS_VORONOI_ON", causticsMode == 0);
            MGUI.SetKeyword(mat, "_CAUSTICS_TEXTURE_ON", causticsMode == 1);
            MGUI.SetKeyword(mat, "_CAUSTICS_FLIPBOOK_ON", causticsMode == 2);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawGizmo(MeshRenderer meshRenderer, GizmoType gizmoType){
            if (meshRenderer.sharedMaterial != null){
                Material material = meshRenderer.sharedMaterial;
                if (!material.shader.name.Contains("Mochie/Underwater Visuals")) return;
                if (material.GetInt("_Falloff") == 1){
                    Vector3 position = meshRenderer.transform.position;
                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Transform t = meshRenderer.transform;
                    Vector3 boxOffset = material.GetVector("_BoxOffset");
                    Vector3 boxSize = material.GetVector("_BoxSize");
                    Vector3 center = position + t.rotation * boxOffset;
                    Gizmos.matrix = Matrix4x4.TRS(center, t.rotation, Vector3.one);

                    Gizmos.color = new Color(0.2f, 0.6f, 1f, 1f);
                    Gizmos.DrawWireCube(Vector3.zero, boxSize);

                    Gizmos.matrix = oldMatrix;
                }
            }
        }
    }
}