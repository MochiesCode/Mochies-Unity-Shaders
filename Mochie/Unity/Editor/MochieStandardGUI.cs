using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Mochie;

internal class MochieStandardGUI : ShaderGUI {

	private static class Styles {
		public static GUIContent uvSetLabel = EditorGUIUtility.TrTextContent("UV Set");
		public static GUIContent albedoText = EditorGUIUtility.TrTextContent("Base Color", "Base Color (RGB) and Transparency (A)");
		public static GUIContent alphaCutoffText = EditorGUIUtility.TrTextContent("Alpha Cutoff", "Threshold for alpha cutoff");
		public static GUIContent metallicMapText = EditorGUIUtility.TrTextContent("Metallic", "Metallic (R)");
		public static GUIContent roughnessText = EditorGUIUtility.TrTextContent("Roughness", "Roughness (R)");
		public static GUIContent highlightsText = EditorGUIUtility.TrTextContent("Specular Highlights", "GGX Specular Highlights");
		public static GUIContent reflectionsText = EditorGUIUtility.TrTextContent("Cubemap Reflections", "Standard cubemap-based reflections");
		public static GUIContent normalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map");
		public static GUIContent heightMapText = EditorGUIUtility.TrTextContent("Height Map", "Height Map (G)");
		public static GUIContent occlusionText = EditorGUIUtility.TrTextContent("Ambient Occlusion", "Ambient Occlusion (G)");
		public static GUIContent emissionText = EditorGUIUtility.TrTextContent("Color", "Emission (RGB)");
		public static GUIContent detailMaskText = EditorGUIUtility.TrTextContent("Detail Mask", "Mask for Secondary Maps (A)");
		public static GUIContent detailAlbedoText = EditorGUIUtility.TrTextContent("Detail Base Color", "Base Color (RGB) multiplied by 2");
		public static GUIContent detailNormalMapText = EditorGUIUtility.TrTextContent("Detail Normal Map", "Normal Map");
		public static GUIContent packedMapText = EditorGUIUtility.TrTextContent("Packed Map (ARMH)", "AO, Roughness, Metallic, Height");
		public static GUIContent packedMapModText = EditorGUIUtility.TrTextContent("Packed Map");
		public static GUIContent reflCubeText = EditorGUIUtility.TrTextContent("Reflection Fallback", "Replace environment reflections below the luminance threshold with this cubemap");
		public static GUIContent reflOverrideText = EditorGUIUtility.TrTextContent("Reflection Override", "Override the primary reflection probe sample with this cubemap");
		public static GUIContent ssrText = EditorGUIUtility.TrTextContent("Screen Space Reflections");
		public static GUIContent edgeFadeText = EditorGUIUtility.TrTextContent("Edge Fade");
		public static GUIContent stepsText = EditorGUIUtility.TrTextContent("Parallax Steps");
		public static GUIContent fallbackAlphaMaskText = EditorGUIUtility.TrTextContent("Fallback Alpha Mask");
		public static GUIContent maskText = EditorGUIUtility.TrTextContent("Mask");
		public static GUIContent heightMaskText = EditorGUIUtility.TrTextContent("Height Mask");
		public static GUIContent parallaxOfsText = EditorGUIUtility.TrTextContent("Parallax Offset");
	}

    public static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
    Toggles toggles = new Toggles(new string[] {
		"Shader Variant",
		"Primary Textures",
		"Detail Textures",
		"UVs",
		"Render Settings"
	});

	string watermark = "Watermark_Pro";
	string patIcon = "Patreon_Icon";
	string versionLabel = "v1.4";

	MaterialProperty blendMode = null;
	MaterialProperty workflow = null;
	MaterialProperty albedoMap = null;
	MaterialProperty albedoColor = null;
	MaterialProperty alphaCutoff = null;
	MaterialProperty metallicMap = null;
	MaterialProperty metallic = null;
	MaterialProperty roughness = null;
	MaterialProperty roughnessMap = null;
	MaterialProperty highlights = null;
	MaterialProperty reflections = null;
	MaterialProperty bumpScale = null;
	MaterialProperty bumpMap = null;
	MaterialProperty occlusionStrength = null;
	MaterialProperty occlusionMap = null;
	MaterialProperty heightMapScale = null;
	MaterialProperty heightMap = null;
	MaterialProperty emissionColorForRendering = null;
	MaterialProperty emissionMap = null;
	MaterialProperty detailMask = null;
	MaterialProperty detailAlbedoMap = null;
	MaterialProperty detailNormalMapScale = null;
	MaterialProperty detailNormalMap = null;
	MaterialProperty uvSetSecondary = null;
	MaterialProperty culling = null;
	MaterialProperty packedMap = null;
	MaterialProperty useHeight = null;
	MaterialProperty saturation = null;

	MaterialProperty uv0Rot = null;
	MaterialProperty uv1Rot = null;
	MaterialProperty uv0Scroll = null;
	MaterialProperty uv1Scroll = null;
	MaterialProperty uv2Scroll = null;
	MaterialProperty uv3Scroll = null;

	MaterialProperty reflCube = null;
	MaterialProperty cubeThreshold = null;
	MaterialProperty heightMult = null;
	MaterialProperty roughMult = null;
	MaterialProperty metalMult = null;
	MaterialProperty occMult = null;
	MaterialProperty gsaa = null;
	MaterialProperty ssr = null;
	MaterialProperty ssrNoise = null;
	MaterialProperty edgeFade = null;
	MaterialProperty steps = null;

	MaterialProperty emissionMask = null;
	MaterialProperty queueOffset = null;
	MaterialProperty reflectionStrength = null;
	MaterialProperty specularStrength = null;
	MaterialProperty parallaxMask = null;
	
	MaterialProperty ssrStrength = null;
	MaterialProperty parallaxOfs = null;
	MaterialProperty emissIntensity = null;
	MaterialProperty reflOverride = null;
	MaterialProperty roughChannel = null;
	MaterialProperty metallicChannel = null;
	MaterialProperty occlusionChannel = null;
	MaterialProperty heightChannel = null;
	MaterialProperty samplingMode = null;
	MaterialProperty triplanarFalloff = null;
	MaterialProperty edgeFadeMin = null;
	MaterialProperty edgeFadeMax = null;

	MaterialEditor m_MaterialEditor;

	bool m_FirstTimeApply = true;
	bool emissionEnabled = false;

	public void FindProperties(MaterialProperty[] props, Material mat){
		blendMode = FindProperty("_BlendMode", props);
		workflow = FindProperty("_Workflow", props);
		albedoMap = FindProperty("_MainTex", props);
		albedoColor = FindProperty("_Color", props);
		alphaCutoff = FindProperty("_Cutoff", props);
		metallicMap = FindProperty("_MetallicGlossMap", props, false);
		metallic = FindProperty("_Metallic", props, false);
		roughness = FindProperty("_Glossiness", props);
		roughnessMap = FindProperty("_SpecGlossMap", props);
		highlights = FindProperty("_SpecularHighlights", props, false);
		reflections = FindProperty("_GlossyReflections", props, false);
		bumpScale = FindProperty("_BumpScale", props);
		bumpMap = FindProperty("_BumpMap", props);
		heightMapScale = FindProperty("_Parallax", props);
		heightMap = FindProperty("_ParallaxMap", props);
		occlusionStrength = FindProperty("_OcclusionStrength", props);
		occlusionMap = FindProperty("_OcclusionMap", props);
		emissionColorForRendering = FindProperty("_EmissionColor", props);
		emissionMap = FindProperty("_EmissionMap", props);
		detailMask = FindProperty("_DetailMask", props);
		detailAlbedoMap = FindProperty("_DetailAlbedoMap", props);
		detailNormalMapScale = FindProperty("_DetailNormalMapScale", props);
		detailNormalMap = FindProperty("_DetailNormalMap", props);
		uvSetSecondary = FindProperty("_UVSec", props);
		packedMap = FindProperty("_PackedMap", props);
		useHeight = FindProperty("_UseHeight", props);
		saturation = FindProperty("_Saturation", props);
		uv0Rot = FindProperty("_UV0Rotate", props);
		uv1Rot = FindProperty("_UV1Rotate", props);
		reflCube = FindProperty("_ReflCube", props);
		cubeThreshold = FindProperty("_CubeThreshold", props);
		culling = FindProperty("_CullingMode", props);
		roughMult = FindProperty("_RoughnessMult", props);
		heightMult = FindProperty("_HeightMult", props);
		metalMult = FindProperty("_MetallicMult", props);
		occMult = FindProperty("_OcclusionMult", props);
		gsaa = FindProperty("_GSAA", props);
		ssr = FindProperty("_SSR", props);
		ssrNoise = FindProperty("_NoiseTexSSR", props);
		edgeFade = FindProperty("_EdgeFade", props);
		steps = FindProperty("_ParallaxSteps", props);
		uv0Scroll = FindProperty("_UV0Scroll", props);
		uv1Scroll = FindProperty("_UV1Scroll", props);
		emissionMask = FindProperty("_EmissionMask", props);
		queueOffset = FindProperty("_QueueOffset", props);
		reflectionStrength = FindProperty("_ReflectionStrength", props);
		specularStrength = FindProperty("_SpecularStrength", props);
		parallaxMask = FindProperty("_ParallaxMask", props);
		uv2Scroll = FindProperty("_UV2Scroll", props);
		ssrStrength = FindProperty("_SSRStrength", props);
		parallaxOfs = FindProperty("_ParallaxOffset", props);
		emissIntensity = FindProperty("_EmissionIntensity", props);
		reflOverride = FindProperty("_ReflCubeOverride", props);
		roughChannel = FindProperty("_RoughnessChannel", props);
		heightChannel = FindProperty("_HeightChannel", props);
		metallicChannel = FindProperty("_MetallicChannel", props);
		occlusionChannel = FindProperty("_OcclusionChannel", props);
		uv3Scroll = FindProperty("_UV3Scroll", props);
		samplingMode = FindProperty("_SamplingMode", props);
		triplanarFalloff = FindProperty("_TriplanarFalloff", props);
		edgeFadeMin = FindProperty("_EdgeFadeMin", props);
		edgeFadeMax = FindProperty("_EdgeFadeMax", props);
	}

	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props){
		m_MaterialEditor = materialEditor;
		Material material = materialEditor.target as Material;

		FindProperties(props, material);

		// Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
		// material to a standard shader.
		// Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
		if (m_FirstTimeApply){
			MaterialChanged(material);
			m_FirstTimeApply = false;
		}

		// Add mat to foldout dictionary if it isn't in there yet
		if (!foldouts.ContainsKey(material))
			foldouts.Add(material, toggles);

		// Use default labelWidth
		EditorGUIUtility.labelWidth = 0f;

		// Detect any changes to the material
		EditorGUI.BeginChangeCheck();{
			
			// Core Shader Variant
			MGUI.Space8();
			bool variantFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, m_MaterialEditor, "Shader Variant");
			if (variantFoldout){
				MGUI.PropertyGroup(() => {
					m_MaterialEditor.ShaderProperty(workflow, "Workflow");
					m_MaterialEditor.ShaderProperty(blendMode, "Blending Mode");
					EditorGUI.BeginChangeCheck();
					m_MaterialEditor.ShaderProperty(samplingMode, "Sampling Mode");
					if (samplingMode.floatValue == 3){
						m_MaterialEditor.ShaderProperty(triplanarFalloff, "Triplanar Falloff");
					}
					if (samplingMode.floatValue == 4){
						m_MaterialEditor.ShaderProperty(edgeFadeMin, "Edge Fade Min");
						m_MaterialEditor.ShaderProperty(edgeFadeMax, "Edge Fade Max");
					}
					if (blendMode.floatValue == 1)
						m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text);
				});
			}

			// Primary properties
			bool texturesFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, m_MaterialEditor, "Primary Textures");
			if (texturesFoldout){
				MGUI.PropertyGroup( () => {
					m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap, albedoColor, albedoMap.textureValue ? saturation : null);
					if (albedoMap.textureValue)
						MGUI.TexPropLabel("Saturation", 118);
					if (workflow.floatValue > 0){
						m_MaterialEditor.TexturePropertySingleLine(workflow.floatValue == 1 ? Styles.packedMapText : Styles.packedMapModText, packedMap);
						MGUI.sRGBWarning(packedMap);
						if (packedMap.textureValue){
							if (workflow.floatValue == 2){
								MGUI.PropertyGroupLayer(() => {
									m_MaterialEditor.ShaderProperty(metallicChannel, "Metallic");
									m_MaterialEditor.ShaderProperty(roughChannel, "Roughness");
									m_MaterialEditor.ShaderProperty(occlusionChannel, "Occlusion");
									if (useHeight.floatValue == 1 && samplingMode.floatValue < 3){
										m_MaterialEditor.ShaderProperty(heightChannel, "Height");
									}
								});
							}
							MGUI.PropertyGroupLayer( () => {
								MGUI.ToggleSlider(m_MaterialEditor, "Metallic Strength", metalMult, metallic);
								MGUI.ToggleSlider(m_MaterialEditor, "Roughness Strength", roughMult, roughness);
								MGUI.ToggleSlider(m_MaterialEditor, "AO Strength", occMult, occlusionStrength);
								if (useHeight.floatValue == 1 && samplingMode.floatValue < 3){
									MGUI.ToggleSlider(m_MaterialEditor, "Height Strength", heightMult, heightMapScale);
									m_MaterialEditor.ShaderProperty(steps, Styles.stepsText);
									m_MaterialEditor.ShaderProperty(parallaxOfs, Styles.parallaxOfsText);
								}
							});
							MGUI.Space8();
						}
						if (useHeight.floatValue == 1 && samplingMode.floatValue < 3){
							m_MaterialEditor.TexturePropertySingleLine(Styles.heightMaskText, parallaxMask);
						}
					}
					else {
						m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap, metallic);
						MGUI.sRGBWarning(metallicMap);
						m_MaterialEditor.TexturePropertySingleLine(Styles.roughnessText, roughnessMap, roughness);
						MGUI.sRGBWarning(roughnessMap);
						m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText, occlusionMap, occlusionMap.textureValue ? occlusionStrength : null);
						MGUI.sRGBWarning(occlusionMap);
						if (samplingMode.floatValue < 3){
							m_MaterialEditor.TexturePropertySingleLine(Styles.heightMapText, heightMap, heightMap.textureValue ? heightMapScale : null);
							MGUI.sRGBWarning(heightMap);
							if (heightMap.textureValue){
								m_MaterialEditor.TexturePropertySingleLine(Styles.heightMaskText, parallaxMask);
								m_MaterialEditor.ShaderProperty(steps, Styles.stepsText, 2);
								m_MaterialEditor.ShaderProperty(parallaxOfs, Styles.parallaxOfsText, 2);
							}
						}
					}
					m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap, bumpMap.textureValue ? bumpScale : null);
					m_MaterialEditor.TexturePropertySingleLine(Styles.reflOverrideText, reflOverride);
					m_MaterialEditor.TexturePropertySingleLine(Styles.reflCubeText, reflCube, reflCube.textureValue ? cubeThreshold : null);

					MGUI.Space2();
					if (workflow.floatValue > 0 && samplingMode.floatValue < 3)
						m_MaterialEditor.ShaderProperty(useHeight, "Packed Height");
					DoEmissionArea(material);
				});
			}
			bool detailFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, m_MaterialEditor, "Detail Textures");
			if (detailFoldout){
				MGUI.PropertyGroup(() => {
					m_MaterialEditor.TexturePropertySingleLine(Styles.detailMaskText, detailMask);
					m_MaterialEditor.TexturePropertySingleLine(Styles.detailAlbedoText, detailAlbedoMap);
					m_MaterialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, detailNormalMap, detailNormalMap.textureValue ? detailNormalMapScale : null);
				});
			}

			// UVs
			bool needsDetailUV = detailAlbedoMap.textureValue || detailNormalMap.textureValue;
			bool needsHeightMaskUV = (((workflow.floatValue > 0 && useHeight.floatValue == 1) || (workflow.floatValue == 0 && heightMap.textureValue)) && parallaxMask.textureValue) && samplingMode.floatValue < 3;
			bool needsEmissMaskUV = emissionEnabled && emissionMask.textureValue;
			bool uvFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, m_MaterialEditor, "UVs");
			if (uvFoldout){
				MGUI.PropertyGroup( () => {
					if (needsDetailUV || needsHeightMaskUV){
						GUILayout.Label("Primary", EditorStyles.boldLabel);
						MGUI.SpaceN2();
					}
					EditorGUI.BeginChangeCheck();
					if (samplingMode.floatValue < 3){
						MGUI.TextureSOScroll(m_MaterialEditor, albedoMap, uv0Scroll);
					}
					else {
						MGUI.TextureSO(m_MaterialEditor, albedoMap);
					}
					if (EditorGUI.EndChangeCheck())
						emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset; // Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake
					m_MaterialEditor.ShaderProperty(uv0Rot, "Rotation");
					if (needsDetailUV || needsHeightMaskUV){
						MGUI.Space2();
					}
					if (needsDetailUV){
						GUILayout.Label("Detail", EditorStyles.boldLabel);
						MGUI.SpaceN2();
						if (samplingMode.floatValue < 3){
							m_MaterialEditor.ShaderProperty(uvSetSecondary, Styles.uvSetLabel.text);
							MGUI.Space2();
						}
						if (samplingMode.floatValue < 3){
							MGUI.TextureSOScroll(m_MaterialEditor, detailAlbedoMap, uv1Scroll);
						}
						else {
							MGUI.TextureSO(m_MaterialEditor, detailAlbedoMap);
						}
						m_MaterialEditor.ShaderProperty(uv1Rot, "Rotation");
					}
					if (needsHeightMaskUV){
						MGUI.Space2();
						GUILayout.Label("Height Mask", EditorStyles.boldLabel);
						MGUI.SpaceN2();
						MGUI.TextureSOScroll(m_MaterialEditor, parallaxMask, uv2Scroll);
					}
					if (needsEmissMaskUV){
						MGUI.Space2();
						GUILayout.Label("Emission Mask", EditorStyles.boldLabel);
						MGUI.SpaceN2();
						MGUI.TextureSOScroll(m_MaterialEditor, emissionMask, uv3Scroll);
					}
				});
			}

			// Rendering options
			bool renderFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, m_MaterialEditor, "Render Settings");
			if (renderFoldout){
				MGUI.PropertyGroup( () => {
					if (samplingMode.floatValue != 4){
						m_MaterialEditor.ShaderProperty(culling, "Culling");
					}
					queueOffset.floatValue = (int)queueOffset.floatValue;
					m_MaterialEditor.ShaderProperty(queueOffset, "Render Queue Offset");
				});
				MGUI.PropertyGroup( () => {
					MGUI.ToggleFloat(m_MaterialEditor, Styles.highlightsText.text, highlights, specularStrength);
					MGUI.ToggleFloat(m_MaterialEditor, Styles.reflectionsText.text, reflections, reflectionStrength);
					MGUI.ToggleFloat(m_MaterialEditor, "Screen Space Reflections", ssr, ssrStrength);
					if (ssr.floatValue == 1){
						m_MaterialEditor.ShaderProperty(edgeFade, Styles.edgeFadeText);
						MGUI.DisplayInfo("\nSSR in VRChat requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs\n\nIt is also is VERY expensive, please use it sparingly!\n");
						MGUI.SpaceN2();
					}
				});
				MGUI.PropertyGroup( () => {
					m_MaterialEditor.ShaderProperty(gsaa, "Specular Antialiasing");
					m_MaterialEditor.EnableInstancingField();
					MGUI.SpaceN2();
					m_MaterialEditor.DoubleSidedGIField();
				});
			}

			// Watermark and version display
			MGUI.Space16();
			if (!EditorGUIUtility.isProSkin){
				watermark = "Watermark";
			}
			Texture2D watermarkTex = (Texture2D)Resources.Load(watermark, typeof(Texture2D));
			Texture2D patIconTex = (Texture2D)Resources.Load(patIcon, typeof(Texture2D));
			float buttonSize = 24.0f;
			float xPos = 53.0f;
			MGUI.CenteredTexture(watermarkTex, 0, 0);
			GUILayout.Space(-buttonSize);
			if (MGUI.LinkButton(patIconTex, buttonSize, buttonSize, xPos)){
				Application.OpenURL("https://www.patreon.com/mochieshaders");
			}
			GUILayout.Space(buttonSize);
			MGUI.VersionLabel(versionLabel, 12,-16,-30);

			SetDecalRendering(material);
		}

		// Ensure settings are applied correctly if anything changed
		if (EditorGUI.EndChangeCheck()){
			foreach (var obj in blendMode.targets)
				MaterialChanged((Material)obj);
		}

		MGUI.Space8();
	}

	public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader){
		// _Emission property is lost after assigning Standard shader to the material
		// thus transfer it before assigning the new shader
		if (material.HasProperty("_Emission")){
			material.SetColor("_EmissionColor", material.GetColor("_Emission"));
		}
		base.AssignNewShaderToMaterial(material, oldShader, newShader);
		MaterialChanged(material);
	}


	void DoEmissionArea(Material material){
		// Emission for GI?
		if (m_MaterialEditor.EmissionEnabledProperty()){
			emissionEnabled = true;
			bool hadEmissionTexture = emissionMap.textureValue != null;
			MGUI.PropertyGroupLayer( () => {
				m_MaterialEditor.LightmapEmissionFlagsProperty(0, true);
				MGUI.Space2();
				m_MaterialEditor.TexturePropertySingleLine(Styles.emissionText, emissionMap, emissionColorForRendering, emissIntensity);
				MGUI.TexPropLabel("Intensity", 111);
				m_MaterialEditor.TexturePropertySingleLine(Styles.maskText, emissionMask);
			});
			float brightness = emissionColorForRendering.colorValue.maxColorComponent;
			if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
				emissionColorForRendering.colorValue = Color.white;
		}
		else {
			emissionEnabled = false;
		}
	}

	public static void SetupMaterialWithBlendMode(Material material){
		int samplingMode = material.GetInt("_SamplingMode");
		switch (material.GetInt("_BlendMode")){
			case 0:
				material.SetOverrideTag("RenderType", "");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				if (samplingMode == 4)
					material.SetInt("_ZWrite", 0);
				else
					material.SetInt("_ZWrite", 1);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry+material.GetInt("_QueueOffset");
				if (material.GetInt("_SSR") == 1)
					material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest+51+material.GetInt("_QueueOffset");
				break;
			case 1:
				material.SetOverrideTag("RenderType", "TransparentCutout");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				if (samplingMode == 4)
					material.SetInt("_ZWrite", 0);
				else
					material.SetInt("_ZWrite", 1);
				material.EnableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest+material.GetInt("_QueueOffset");
				if (material.GetInt("_SSR") == 1)
					material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest+51+material.GetInt("_QueueOffset");
				break;
			case 2:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.EnableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+material.GetInt("_QueueOffset");
				break;
			case 3:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+material.GetInt("_QueueOffset");
				break;
		}
	}

	static void SetMaterialKeywords(Material material){

		// Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
		// (MaterialProperty value might come from renderer material property block)
		int workflow =  material.GetInt("_Workflow");
		int samplingMode = material.GetInt("_SamplingMode");
		MGUI.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap") || material.GetTexture("_DetailNormalMap"));
		MGUI.SetKeyword(material, "BLOOM_LENS_DIRT", workflow == 1);
		MGUI.SetKeyword(material, "_FADING_ON", workflow == 2);
		MGUI.SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
		MGUI.SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));
		MGUI.SetKeyword(material, "_DETAIL_MULX2", material.GetTexture("_DetailAlbedoMap") || material.GetTexture("_DetailNormalMap"));
		MGUI.SetKeyword(material, "_MAPPING_6_FRAMES_LAYOUT", material.GetTexture("_ReflCube"));
		MGUI.SetKeyword(material, "_COLOROVERLAY_ON", material.GetTexture("_ReflCubeOverride"));
		MGUI.SetKeyword(material, "FXAA", material.GetInt("_GSAA") == 1);
		MGUI.SetKeyword(material, "GRAIN", material.GetInt("_SSR") == 1);
		MGUI.SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF", material.GetInt("_SpecularHighlights") == 0);
		MGUI.SetKeyword(material, "_GLOSSYREFLECTIONS_OFF", material.GetInt("_GlossyReflections") == 0);
		MGUI.SetKeyword(material, "EFFECT_HUE_VARIATION",  samplingMode == 1);
		MGUI.SetKeyword(material, "BLOOM", samplingMode == 2);
		MGUI.SetKeyword(material, "_COLORCOLOR_ON", samplingMode == 3);
		MGUI.SetKeyword(material, "EFFECT_BUMP", samplingMode == 4);
		if (samplingMode < 3){
			if (!material.GetTexture("_PackedMap"))
				MGUI.SetKeyword(material, "_PARALLAXMAP", material.GetTexture("_ParallaxMap"));
			else
				MGUI.SetKeyword(material, "_PARALLAXMAP", material.GetInt("_UseHeight") == 1);
		}
		else {
			MGUI.SetKeyword(material, "_PARALLAXMAP", false);
		}

		// A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
		// or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
		// The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
		MaterialEditor.FixupEmissiveFlag(material);
		bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
		MGUI.SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);

		material.SetShaderPassEnabled("Always", material.GetInt("_SSR") == 1);
	}

	static void SetDecalRendering(Material material){
		int userCull = material.GetInt("_CullingMode");
		int realCull = material.GetInt("_Cull");
		int samplingMode = material.GetInt("_SamplingMode");

		if (samplingMode == 4){
			material.SetInt("_Cull", 1);
			material.SetInt("_MetaCull", 1);
			material.SetInt("_ZTest", 5);
		}
		else {
			material.SetInt("_MetaCull", 0);
			material.SetInt("_Cull", userCull);
			material.SetInt("_ZTest", 4);
		}
	}

	static void MaterialChanged(Material material){
		SetupMaterialWithBlendMode(material);
		SetMaterialKeywords(material);
	}
}