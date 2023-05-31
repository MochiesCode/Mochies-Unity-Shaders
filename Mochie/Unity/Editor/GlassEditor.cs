using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Mochie;

public class GlassEditor : ShaderGUI {

	string versionLabel = "v1.1.3";

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
	MaterialProperty _RefractMeshNormals = null;

	// Rain
	MaterialProperty _RainToggle = null;
	MaterialProperty _Speed = null;
	MaterialProperty _XScale = null;
	MaterialProperty _YScale = null;
	MaterialProperty _Strength = null;

	// Render Settings
	MaterialProperty _ReflectionsToggle = null;
	MaterialProperty _SpecularToggle = null;
	MaterialProperty _Culling = null;
	MaterialProperty _BlendMode = null;
	MaterialProperty _LitBaseColor = null;

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
					// me.ShaderProperty(_RefractMeshNormals, "Refract Mesh Normals");
				});
			}
			MGUI.Space10();

			MGUI.BoldLabel("RAIN");
			me.ShaderProperty(_RainToggle, "Enable");
			MGUI.ToggleGroup(_RainToggle.floatValue == 0f);
			MGUI.PropertyGroup(()=>{
				me.ShaderProperty(_Speed, "Speed");
				me.ShaderProperty(_XScale, "X Scale");
				me.ShaderProperty(_YScale, "Y Scale");
				me.ShaderProperty(_Strength, "Strength");
			});
			MGUI.ToggleGroupEnd();
			MGUI.Space10();

			MGUI.BoldLabel("RENDER SETTINGS");
			MGUI.PropertyGroup(()=>{
				me.ShaderProperty(_ReflectionsToggle, "Reflections");
				me.ShaderProperty(_SpecularToggle, "Specular Highlights");
				me.ShaderProperty(_LitBaseColor, "Lit Base Color");
				me.RenderQueueField();
				me.ShaderProperty(_Culling, "Culling Mode");
				EditorGUI.BeginChangeCheck();
				me.ShaderProperty(_BlendMode, "Transparency");
				if (EditorGUI.EndChangeCheck())
					SetBlendMode(mat);
			});
			MGUI.ToggleGroupEnd();
		}
		if (EditorGUI.EndChangeCheck()){
			SetBlendMode(mat);
		}
		MGUI.Space10();
		MGUI.DoFooter(versionLabel);
    }

	void SetBlendMode(Material mat){
		int blendMode = mat.GetInt("_BlendMode");
		switch (blendMode){
			case 0: // Grabpass
				mat.SetOverrideTag("RenderType", "Opaque");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				mat.SetShaderPassEnabled("Always", true);
				MGUI.SetKeyword(mat, "_GRABPASS_ON", true);
				break;
			case 1: // Premultiplied
				mat.SetOverrideTag("RenderType", "Transparent");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				mat.SetShaderPassEnabled("Always", false);
				MGUI.SetKeyword(mat, "_GRABPASS_ON", false);
				break;
			case 2: // Opaque
				mat.SetOverrideTag("RenderType", "Opaque");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
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