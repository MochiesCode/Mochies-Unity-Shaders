using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Mochie;

public class GlassEditor : ShaderGUI {

	string versionLabel = "v1.6";

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
	MaterialProperty BlurQuality = null;

	// Rain
	MaterialProperty _RainToggle = null;
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
	MaterialProperty _RainBias = null;
	MaterialProperty _RainThreshold = null;
	MaterialProperty _RainThresholdSize = null;

	// Render Settings
	MaterialProperty _ReflectionsToggle = null;
	MaterialProperty _SpecularToggle = null;
	MaterialProperty _Culling = null;
	MaterialProperty _SamplingMode = null;
	MaterialProperty _BlendMode = null;
	MaterialProperty _LitBaseColor = null;
	MaterialProperty _QueueOffset = null;
	MaterialProperty _TexCoordSpace = null;
	MaterialProperty _TexCoordSpaceSwizzle = null;
	MaterialProperty _GlobalTexCoordScale = null;

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

        EditorGUI.BeginChangeCheck(); {
			
			MGUI.BoldLabel("SURFACE");
			MGUI.PropertyGroup(()=>{
				MGUI.PropertyGroup(()=>{
					if (_BlendMode.floatValue == 0)
						me.ShaderProperty(_GrabpassTint, "Grabpass Tint");
					me.ShaderProperty(_SpecularityTint, "Specularity Tint");
					me.ShaderProperty(_BaseColorTint, "Base Color Tint");
				});
				MGUI.PropertyGroup(()=>{
					me.TexturePropertySingleLine(Tips.baseColorLabel, _BaseColor);
					MGUI.TextureSO(me, _BaseColor, _BaseColor.textureValue);
					me.TexturePropertySingleLine(Tips.metallicMapText, _MetallicMap, _Metallic);
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
						me.ShaderProperty(BlurQuality, "Blur Quality");
						me.ShaderProperty(_Blur, "Blur Strength");
						me.ShaderProperty(_Refraction, "Refraction");
					});
				}
			});
			MGUI.Space10();

			MGUI.BoldLabel("RAIN");
			me.ShaderProperty(_RainToggle, "Enable");
			MGUI.ToggleGroup(_RainToggle.floatValue == 0f);
			MGUI.PropertyGroup(()=>{
				me.ShaderProperty(_RainMode, "Mode");
				if (_RainMode.floatValue == 0){
					RainDroplets(me);
				}
				else if (_RainMode.floatValue == 1){
					RainRipples(me);
				}
				else if (_RainMode.floatValue == 2){
					MGUI.BoldLabel("Droplets");
					RainDroplets(me);
					MGUI.BoldLabel("Ripples");
					RainRipples(me);
					MGUI.BoldLabel("Both");
					RainBoth(me);
				}
				MGUI.SpaceN2();
			});
			MGUI.ToggleGroupEnd();
			MGUI.Space10();

			MGUI.BoldLabel("RENDER SETTINGS");
			MGUI.PropertyGroup(()=>{
				MGUI.PropertyGroup(()=>{
					me.ShaderProperty(_ReflectionsToggle, "Reflections");
					me.ShaderProperty(_SpecularToggle, "Specular Highlights");
					me.ShaderProperty(_LitBaseColor, "Lit Base Color");
				});
				MGUI.PropertyGroup(()=>{
					MGUI.SpaceN1();
					_QueueOffset.floatValue = (int)_QueueOffset.floatValue;
					MGUI.DummyProperty("Render Queue:", mat.renderQueue.ToString());
					me.ShaderProperty(_QueueOffset, Tips.queueOffset);
					me.ShaderProperty(_Culling, "Culling Mode");
					me.ShaderProperty(_SamplingMode, "Sampling Mode");
					/// EditorGUI.BeginChangeCheck();
					me.ShaderProperty(_BlendMode, "Transparency");
					// if (EditorGUI.EndChangeCheck())
					// 	SetBlendMode(mat);
				});
				MGUI.PropertyGroup(()=>{
					me.ShaderProperty(_TexCoordSpace, "Texture Coordinate Space");
					if (_TexCoordSpace.floatValue == 1f){
						me.ShaderProperty(_TexCoordSpaceSwizzle, "Swizzle");
					}
					me.ShaderProperty(_GlobalTexCoordScale, "Texture Coordinate Scale");
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
		
		MGUI.PropertyGroup(()=>{
			if (_RainMode.floatValue != 2){
				me.TexturePropertySingleLine(Tips.maskText, _RainMask, _RainMaskChannel);
				MGUI.TextureSO(me, _RainMask, _RainMask.textureValue);
			}
			me.ShaderProperty(_Strength, "Strength");
			me.ShaderProperty(_Speed, "Speed");
			me.ShaderProperty(_XScale, "X Scale");
			me.ShaderProperty(_YScale, "Y Scale");
			me.ShaderProperty(_RainBias, "Mip Bias");
			me.ShaderProperty(_DynamicDroplets, "Dynamic Droplets");
		});
	}

	void RainRipples(MaterialEditor me){
		MGUI.PropertyGroup(()=>{
			if (_RainMode.floatValue != 2){
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
		MGUI.SetKeyword(mat, "_STOCHASTIC_SAMPLING_ON", mat.GetInt("_SamplingMode") == 1);
		MGUI.SetKeyword(mat, "_NORMALMAP_ON", mat.GetTexture("_NormalMap"));
		MGUI.SetKeyword(mat, "_RAINMODE_RIPPLE", mat.GetInt("_RainMode") == 1);
		MGUI.SetKeyword(mat, "_RAINMODE_AUTO", mat.GetInt("_RainMode") == 2);
	}

	void SetBlendMode(Material mat){
		int blendMode = mat.GetInt("_BlendMode");
		int rainToggle = mat.GetInt("_RainToggle");
		float roughness = mat.GetFloat("_Roughness");
		bool hasNormal = mat.GetTexture("_NormalMap") && mat.GetFloat("_NormalStrength") > 0;
		bool canGrab = roughness > 0 || rainToggle == 1 || hasNormal;
		if (blendMode == 0) blendMode = canGrab ? blendMode : 1;

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
		SetBlendMode(mat);
		MGUI.ClearKeywords(mat);
	}
}