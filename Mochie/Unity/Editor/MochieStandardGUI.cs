using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Mochie;

internal class MochieStandardGUI : ShaderGUI {

    public static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
    Toggles toggles = new Toggles(new string[] {
		"Shader Variant",
		"Primary Textures",
		// "Detail Textures",
		"UVs",
		// "Emission",
		"Rim",
		"Subsurface Scattering",
		"Filtering",
		"Render Settings"
	}, 1);

	string versionLabel = "v1.13";
	// β

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
	MaterialProperty detailAlbedoBlend = null;
	MaterialProperty detailRoughnessMap = null;
	MaterialProperty detailRoughBlend = null;
	MaterialProperty detailAOMap = null;
	MaterialProperty detailAOBlend = null;
	MaterialProperty detailNormalMapScale = null;
	MaterialProperty detailNormalMap = null;
	MaterialProperty uvSetSecondary = null;
	MaterialProperty culling = null;
	MaterialProperty packedMap = null;
	MaterialProperty useHeight = null;
	MaterialProperty saturation = null;
	MaterialProperty contrast = null;
	MaterialProperty brightness = null;
	MaterialProperty hue = null;
	MaterialProperty saturationDet = null;
	MaterialProperty contrastDet = null;
	MaterialProperty brightnessDet = null;
	MaterialProperty hueDet = null;
	MaterialProperty hueEmiss = null;
	MaterialProperty saturationEmiss = null;
	MaterialProperty brightnessEmiss = null;
	MaterialProperty contrastEmiss = null;
	MaterialProperty reflShadows = null;

	MaterialProperty huePost = null;
	MaterialProperty saturationPost = null;
	MaterialProperty contrastPost = null;
	MaterialProperty brightnessPost = null;

	MaterialProperty subsurface = null;
	MaterialProperty thicknessMap = null;
	MaterialProperty thicknessMapPower = null;
	MaterialProperty scatterCol = null;
	MaterialProperty scatterAmbient = null;
	MaterialProperty scatterIntensity = null;
	MaterialProperty scatterPow = null;
	MaterialProperty scatterDist = null;
	MaterialProperty scatterAlbedoTint = null;
	MaterialProperty wrappingFactor = null;
	
	MaterialProperty uv0Rot = null;
	MaterialProperty uv1Rot = null;
	MaterialProperty uv3Rot = null;
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
	MaterialProperty useSmoothness = null;
	MaterialProperty detailMaskChannel = null;
	MaterialProperty detailSamplingMode = null;
	MaterialProperty gsaaStrength = null;
	MaterialProperty reflShadowStrength = null;
	MaterialProperty reflVertexColor = null;
	MaterialProperty reflVertexColorStrength = null;
	MaterialProperty emissPulseWave = null;
	MaterialProperty emissPulseSpeed = null;
	MaterialProperty emissPulseStrength = null;

	MaterialProperty brightnessReflShad = null;
	MaterialProperty contrastReflShad = null;
	MaterialProperty hdrReflShad = null;
	
	MaterialProperty audioLinkEmission = null;
	MaterialProperty audioLinkEmissionStrength = null;

	MaterialProperty uvPri = null;
	MaterialProperty uvEmissMask = null;
	MaterialProperty uvHeightMask = null;

	MaterialProperty uv4Rot = null;
	MaterialProperty uv4Scroll = null;
	MaterialProperty alphaMask = null;
	MaterialProperty uvAlphaMask = null;
	MaterialProperty useAlphaMask = null;
	MaterialProperty alphaMaskChannel = null;
	MaterialProperty alphaMaskOpacity = null;
	MaterialProperty useFresnel = null;
	MaterialProperty fresnelStrength = null;
	
	MaterialProperty rimTog = null;
	MaterialProperty rimStr = null;
	MaterialProperty rimBlend = null;
	MaterialProperty rimCol = null;
	MaterialProperty rimWidth = null;
	MaterialProperty rimEdge = null;

	MaterialProperty filtering = null;

	MaterialProperty bicubicLightmap = null;

	MaterialProperty ltcgi = null;
	MaterialProperty ltcgi_diffuse_off = null;
	MaterialProperty ltcgi_spec_off = null;

	MaterialProperty _BakeryMode = null;
	MaterialProperty _BAKERY_LMSPEC = null;
	MaterialProperty _BAKERY_SHNONLINEAR = null;
	// MaterialProperty _RNM0 = null;
	// MaterialProperty _RNM1 = null;
	// MaterialProperty _RNM2 = null;

	MaterialEditor me;

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
		detailAlbedoBlend = FindProperty("_DetailAlbedoBlend", props);
		detailRoughnessMap = FindProperty("_DetailRoughnessMap", props);
		detailRoughBlend = FindProperty("_DetailRoughBlend", props);
		detailAOMap = FindProperty("_DetailAOMap", props);
		detailAOBlend = FindProperty("_DetailAOBlend", props);
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
		subsurface = FindProperty("_Subsurface", props);
		thicknessMap = FindProperty("_ThicknessMap", props);
		thicknessMapPower = FindProperty("_ThicknessMapPower", props);
		scatterCol = FindProperty("_ScatterCol", props);
		scatterAmbient = FindProperty("_ScatterAmbient", props);
		scatterPow = FindProperty("_ScatterPow", props);
		scatterIntensity = FindProperty("_ScatterIntensity", props);
		scatterDist = FindProperty("_ScatterDist", props);
		scatterAlbedoTint = FindProperty("_ScatterAlbedoTint", props);
		wrappingFactor = FindProperty("_WrappingFactor", props);
		reflShadows = FindProperty("_ReflShadows", props);
		useSmoothness = FindProperty("_UseSmoothness", props);
		detailMaskChannel = FindProperty("_DetailMaskChannel", props);
		hue = FindProperty("_Hue", props);
		hueDet = FindProperty("_HueDet", props);
		hueEmiss = FindProperty("_HueEmiss", props);
		huePost = FindProperty("_HuePost", props);
		saturationDet = FindProperty("_SaturationDet", props);
		saturationEmiss = FindProperty("_SaturationEmiss", props);
		saturationPost = FindProperty("_SaturationPost", props);
		contrast = FindProperty("_Contrast", props);
		contrastDet = FindProperty("_ContrastDet", props);
		contrastEmiss = FindProperty("_ContrastEmiss", props);
		contrastPost = FindProperty("_ContrastPost", props);
		brightness = FindProperty("_Brightness", props);
		brightnessDet = FindProperty("_BrightnessDet", props);
		brightnessEmiss = FindProperty("_BrightnessEmiss", props);
		brightnessPost = FindProperty("_BrightnessPost", props);
		audioLinkEmission = FindProperty("_AudioLinkEmission", props);
		audioLinkEmissionStrength = FindProperty("_AudioLinkEmissionStrength", props);
		detailSamplingMode = FindProperty("_DetailSamplingMode", props);
		reflShadowStrength = FindProperty("_ReflShadowStrength", props);
		gsaaStrength = FindProperty("_GSAAStrength", props);
		reflVertexColor = FindProperty("_ReflVertexColor", props);
		reflVertexColorStrength = FindProperty("_ReflVertexColorStrength", props);
		contrastReflShad = FindProperty("_ContrastReflShad", props);
		brightnessReflShad = FindProperty("_BrightnessReflShad", props);
		hdrReflShad = FindProperty("_HDRReflShad", props);
		uv3Rot = FindProperty("_UV3Rotate", props);
		emissPulseWave = FindProperty("_EmissPulseWave", props);
		emissPulseSpeed = FindProperty("_EmissPulseSpeed", props);
		emissPulseStrength = FindProperty("_EmissPulseStrength", props);
		uvEmissMask = FindProperty("_UVEmissMask", props);
		uvEmissMask = FindProperty("_UVEmissMask", props);
		uvPri = FindProperty("_UVPri", props);
		uvHeightMask = FindProperty("_UVHeightMask", props);
		uv4Rot = FindProperty("_UV4Rotate", props);
		uv4Scroll = FindProperty("_UV4Scroll", props);
		uvAlphaMask = FindProperty("_UVAlphaMask", props);
		alphaMask = FindProperty("_AlphaMask", props);
		useAlphaMask = FindProperty("_UseAlphaMask", props);
		alphaMaskChannel = FindProperty("_AlphaMaskChannel", props);
		alphaMaskOpacity = FindProperty("_AlphaMaskOpacity", props);
		useFresnel = FindProperty("_UseFresnel", props);
		fresnelStrength = FindProperty("_FresnelStrength", props);
		rimTog = FindProperty("_RimToggle", props);
		rimStr = FindProperty("_RimStr", props);
		rimBlend = FindProperty("_RimBlending", props);
		rimCol = FindProperty("_RimCol", props);
		rimWidth = FindProperty("_RimWidth", props);
		rimEdge = FindProperty("_RimEdge", props);
		bicubicLightmap = FindProperty("_BicubicLightmap", props);
		ltcgi = FindProperty("_LTCGI", props);
		ltcgi_diffuse_off = FindProperty("_LTCGI_DIFFUSE_OFF", props);
		ltcgi_spec_off = FindProperty("_LTCGI_SPECULAR_OFF", props);
		_BakeryMode = FindProperty("_BakeryMode", props);
		// _RNM0 = FindProperty("_RNM0", props);
		// _RNM1 = FindProperty("_RNM1", props);
		// _RNM2 = FindProperty("_RNM2", props);
		_BAKERY_LMSPEC = FindProperty("_BAKERY_LMSPEC", props);
		_BAKERY_SHNONLINEAR = FindProperty("_BAKERY_SHNONLINEAR", props);
		filtering = FindProperty("_Filtering", props);
	}

	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props){
		me = materialEditor;
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
			MGUI.BoldLabel("Shader Variant");
			DoVariantArea();
			MGUI.Space2();

			// Primary properties
			MGUI.BoldLabel("Primary Textures");
			DoPrimaryArea(material);
			MGUI.Space2();

			// Detail properties
			DoDetailArea();
			MGUI.Space4();

			// Emission
			// bool emissFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "Emission");
			// if (emissFoldout){
			// 	DoEmissionArea(material);
			// }
			
			// Rim
			bool rimFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "Rim");
			if (rimFoldout){
				DoRimArea();
			}

			// Subsurface
			bool subsurfaceArea = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "Subsurface Scattering");
			if (subsurfaceArea){
				DoSubsurfaceArea();
			}

			// Filtering
			bool filteringFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "Filtering");
			if (filteringFoldout){
				DoFilteringArea();
			}

			// UVs
			bool uvFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "UVs");
			if (uvFoldout){
				DoUVArea();
			}

			// Rendering options
			bool renderFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "Render Settings");
			if (renderFoldout){
				DoRenderingArea(material);
			}

			// Watermark and version display
			DoFooter();

			// Setup stuff for decal mode
			SetDecalRendering(material);
		}

		// Ensure settings are applied correctly if anything changed
		if (EditorGUI.EndChangeCheck()){
			foreach (var obj in blendMode.targets)
				MaterialChanged((Material)obj);
		}

		MGUI.Space8();
	}

	void DoVariantArea(){
		MGUI.PropertyGroup(() => {
			me.ShaderProperty(workflow, Tips.standWorkflow);
			me.ShaderProperty(blendMode, Tips.standBlendMode);
			me.ShaderProperty(samplingMode, Tips.samplingMode);
			if (blendMode.floatValue > 0){
				me.ShaderProperty(useAlphaMask, Tips.separateAlpha);
			}
			if (samplingMode.floatValue == 3){
				me.ShaderProperty(triplanarFalloff, Tips.triplanarFalloff);
			}
			if (samplingMode.floatValue == 4){
				me.ShaderProperty(edgeFadeMin, "Edge Fade Min");
				me.ShaderProperty(edgeFadeMax, "Edge Fade Max");
			}
			if (blendMode.floatValue == 1)
				me.ShaderProperty(alphaCutoff, Tips.alphaCutoffText);
			me.ShaderProperty(useSmoothness, Tips.useSmoothness);
			if (workflow.floatValue > 0 && samplingMode.floatValue < 3)
				me.ShaderProperty(useHeight, Tips.useHeight);
		});
	}

	void DoPrimaryArea(Material material){
		MGUI.PropertyGroup( () => {
			me.TexturePropertySingleLine(Tips.albedoText, albedoMap, albedoColor);
			if (useAlphaMask.floatValue == 1 && blendMode.floatValue > 0){
				me.TexturePropertySingleLine(Tips.alphaMaskText, alphaMask, alphaMaskOpacity, alphaMaskChannel);
			}
			GUIContent roughLabel = Tips.roughnessText;
			GUIContent roughStrLabel = Tips.roughnessPackedText;
			if (useSmoothness.floatValue == 1){
				roughLabel.text = "Smoothness";
				roughStrLabel.text = "Smoothness Strength";
			}
			else {
				roughLabel.text = "Roughness";
				roughStrLabel.text = "Roughness Strength";
			}
			if (workflow.floatValue == 1){
				me.TexturePropertySingleLine(Tips.packedMapText, packedMap);
				MGUI.sRGBWarning(packedMap);
				if (packedMap.textureValue){
					MGUI.PropertyGroupLayer(() => {
						MGUI.SpaceN3();
						me.ShaderProperty(metallicChannel, "Metallic");
						me.ShaderProperty(roughChannel, roughLabel.text);
						me.ShaderProperty(occlusionChannel, "Occlusion");
						if (useHeight.floatValue == 1 && samplingMode.floatValue < 3){
							me.ShaderProperty(heightChannel, "Height");
						}
						MGUI.SpaceN3();
					});
					MGUI.PropertyGroupLayer( () => {
						MGUI.SpaceN2();
						MGUI.ToggleSlider(me, Tips.metallicPackedText, metalMult, metallic);
						MGUI.ToggleSlider(me, roughStrLabel, roughMult, roughness);
						MGUI.ToggleSlider(me, Tips.occlusionPackedText, occMult, occlusionStrength);
						if (useHeight.floatValue == 1 && samplingMode.floatValue < 3){
							MGUI.ToggleSlider(me, Tips.heightMapPackedText, heightMult, heightMapScale);
							me.ShaderProperty(steps, Tips.stepsText);
							me.ShaderProperty(parallaxOfs, Tips.parallaxOfsText);
						}
						MGUI.SpaceN2();
					});
					MGUI.Space8();
				}
				if (useHeight.floatValue == 1 && samplingMode.floatValue < 3){
					me.TexturePropertySingleLine(Tips.heightMaskText, parallaxMask);
				}
			}
			else {
				me.TexturePropertySingleLine(Tips.metallicMapText, metallicMap, metallic);
				MGUI.sRGBWarning(metallicMap);
				me.TexturePropertySingleLine(roughLabel, roughnessMap, roughness);
				MGUI.sRGBWarning(roughnessMap);
				me.TexturePropertySingleLine(Tips.occlusionText, occlusionMap, occlusionMap.textureValue ? occlusionStrength : null);
				MGUI.sRGBWarning(occlusionMap);
				if (samplingMode.floatValue < 3){
					me.TexturePropertySingleLine(Tips.heightMapText, heightMap, heightMap.textureValue ? heightMapScale : null);
					MGUI.sRGBWarning(heightMap);
					if (heightMap.textureValue){
						me.TexturePropertySingleLine(Tips.heightMaskText, parallaxMask);
						me.ShaderProperty(steps, Tips.stepsText, 2);
						me.ShaderProperty(parallaxOfs, Tips.parallaxOfsText, 2);
					}
				}
			}
			me.TexturePropertySingleLine(Tips.normalMapText, bumpMap, bumpMap.textureValue ? bumpScale : null);
			me.TexturePropertySingleLine(Tips.reflOverrideText, reflOverride);
			me.TexturePropertySingleLine(Tips.reflCubeText, reflCube, reflCube.textureValue ? cubeThreshold : null);
			me.TexturePropertySingleLine(Tips.detailMaskText, detailMask, detailMask.textureValue ? detailMaskChannel : null);
			DoEmissionArea(material);
		});
	}

	void DoDetailArea(){
		MGUI.BoldLabel("Detail Textures");
		MGUI.PropertyGroup(() => {
			me.TexturePropertySingleLine(Tips.detailAlbedoText, detailAlbedoMap, detailAlbedoMap.textureValue ? detailAlbedoBlend : null);
			me.TexturePropertySingleLine(Tips.detailNormalMapText, detailNormalMap, detailNormalMap.textureValue ? detailNormalMapScale : null);
			me.TexturePropertySingleLine(Tips.detailRoughnessMapText, detailRoughnessMap, detailRoughnessMap.textureValue ? detailRoughBlend : null);
			MGUI.sRGBWarning(detailRoughnessMap);
			me.TexturePropertySingleLine(Tips.detailAOMapText, detailAOMap, detailAOMap.textureValue ? detailAOBlend : null);
			MGUI.sRGBWarning(detailAOMap);
			MGUI.SpaceN2();
		});
	}

	void DoEmissionArea(Material material){
		if (me.EmissionEnabledProperty()){
			emissionEnabled = true;
			bool hadEmissionTexture = emissionMap.textureValue != null;
			MGUI.ToggleGroup(!emissionEnabled);
			MGUI.PropertyGroupLayer( () => {
				MGUI.SpaceN2();
				me.LightmapEmissionFlagsProperty(0, true);
				me.ShaderProperty(audioLinkEmission, Tips.audioLinkEmission);
				me.ShaderProperty(emissPulseWave, Tips.emissPulseWave);
				if (audioLinkEmission.floatValue > 0){
					me.ShaderProperty(audioLinkEmissionStrength, Tips.audioLinkEmissionStrength);
				}
				if (emissPulseWave.floatValue > 0){
					me.ShaderProperty(emissPulseStrength, Tips.emissPulseStrength);
					me.ShaderProperty(emissPulseSpeed, Tips.emissPulseSpeed);
				}
				MGUI.Space2();
				me.TexturePropertySingleLine(Tips.emissionText, emissionMap, emissionColorForRendering, emissIntensity);
				MGUI.TexPropLabel("Intensity", 105);
				MGUI.SpaceN2();
				me.TexturePropertySingleLine(Tips.maskText, emissionMask);
				MGUI.SpaceN4();
			});
			MGUI.ToggleGroupEnd();
			float brightness = emissionColorForRendering.colorValue.maxColorComponent;
			if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
				emissionColorForRendering.colorValue = Color.white;
			}
		else {
			emissionEnabled = false;
		}
	}

	void DoRimArea(){
		MGUI.PropertyGroup(()=>{
			me.ShaderProperty(rimTog, "Enable");
			MGUI.ToggleGroup(rimTog.floatValue == 0);
			MGUI.PropertyGroupLayer(() => {
				MGUI.SpaceN2();
				me.ShaderProperty(rimBlend, Tips.rimBlend);
				me.ShaderProperty(rimCol, Tips.rimCol);
				me.ShaderProperty(rimStr, Tips.rimStr);
				me.ShaderProperty(rimWidth, Tips.rimWidth);
				me.ShaderProperty(rimEdge, Tips.rimEdge);
				MGUI.SpaceN2();
			});
			MGUI.ToggleGroupEnd();
		});
	}

	void DoSubsurfaceArea(){
		MGUI.PropertyGroup(()=>{
			me.ShaderProperty(subsurface, "Enable");
			MGUI.ToggleGroup(subsurface.floatValue == 0);
			MGUI.PropertyGroupLayer(() => {
				MGUI.SpaceN1();
				me.TexturePropertySingleLine(Tips.thicknessMapText, thicknessMap, thicknessMap.textureValue ? thicknessMapPower : null);
				if (thicknessMap.textureValue)
					MGUI.TexPropLabel("Power", 94);
				me.ShaderProperty(scatterCol, Tips.scatterCol);
				me.ShaderProperty(scatterAlbedoTint, Tips.scatterAlbedoTint);
				MGUI.Space8();
				me.ShaderProperty(scatterIntensity, Tips.scatterIntensity);
				me.ShaderProperty(scatterAmbient, Tips.scatterAmbient);
				me.ShaderProperty(scatterPow, Tips.scatterPow);
				me.ShaderProperty(scatterDist, Tips.scatterDist);
				me.ShaderProperty(wrappingFactor, Tips.wrappingFactor);
				MGUI.SpaceN2();
			});
			MGUI.ToggleGroupEnd();
		});
	}

	void DoUVArea(){
		bool needsHeightMaskUV = (((workflow.floatValue > 0 && useHeight.floatValue == 1) || (workflow.floatValue == 0 && heightMap.textureValue)) && parallaxMask.textureValue) && samplingMode.floatValue < 3;
		bool needsEmissMaskUV = emissionEnabled && emissionMask.textureValue;
		bool needsAlphaMaskUV = blendMode.floatValue > 0 && useAlphaMask.floatValue > 0;
		MGUI.PropertyGroup( () => {
			MGUI.BoldLabel("Primary");
			EditorGUI.BeginChangeCheck();
			MGUI.PropertyGroupLayer(()=>{
				MGUI.SpaceN2();
				if (samplingMode.floatValue < 3){
					me.ShaderProperty(uvPri, Tips.uvSetLabel.text);
					MGUI.TextureSOScroll(me, albedoMap, uv0Scroll);
				}
				else {
					MGUI.TextureSO(me, albedoMap);
				}
				if (EditorGUI.EndChangeCheck())
					emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset; 
				me.ShaderProperty(uv0Rot, "Rotation");
				MGUI.SpaceN2();
			});
			MGUI.Space4();
			MGUI.BoldLabel("Detail");
			MGUI.PropertyGroupLayer(()=>{
				MGUI.SpaceN2();
				if (samplingMode.floatValue < 3){
					me.ShaderProperty(uvSetSecondary, Tips.uvSetLabel.text);
					MGUI.TextureSOScroll(me, detailAlbedoMap, uv1Scroll);
				}
				else {
					MGUI.TextureSO(me, detailAlbedoMap);
				}
				me.ShaderProperty(uv1Rot, "Rotation");
				me.ShaderProperty(detailSamplingMode, Tips.detailSamplingMode);
				MGUI.SpaceN4();
			});
			if (needsHeightMaskUV){
				MGUI.Space4();
				MGUI.BoldLabel("Height Mask");
				MGUI.PropertyGroupLayer(()=>{
					MGUI.SpaceN2();
					me.ShaderProperty(uvHeightMask, Tips.uvSetLabel.text);
					MGUI.TextureSOScroll(me, parallaxMask, uv2Scroll);
					MGUI.SpaceN2();
				});
			}
			if (needsEmissMaskUV){
				MGUI.Space4();
				MGUI.BoldLabel("Emission Mask");
				MGUI.PropertyGroupLayer(()=>{
					MGUI.SpaceN2();
					me.ShaderProperty(uvEmissMask, Tips.uvSetLabel.text);
					MGUI.TextureSOScroll(me, emissionMask, uv3Scroll);
					me.ShaderProperty(uv3Rot, "Rotation");
					MGUI.SpaceN2();
				});
			}
			if (needsAlphaMaskUV){
				MGUI.Space4();
				MGUI.BoldLabel("Alpha Mask");
				MGUI.PropertyGroupLayer(()=>{
					MGUI.SpaceN2();
					me.ShaderProperty(uvAlphaMask, Tips.uvSetLabel.text);
					MGUI.TextureSOScroll(me, alphaMask, uv4Scroll);
					me.ShaderProperty(uv4Rot, "Rotation");
					MGUI.SpaceN2();
				});
			}
		});
	}

	void DoRenderingArea(Material mat){
		MGUI.PropertyGroup(()=>{
			MGUI.PropertyGroupLayer(() => {
				MGUI.SpaceN2();
				if (samplingMode.floatValue != 4){
					me.ShaderProperty(culling, Tips.culling);
				}
				queueOffset.floatValue = (int)queueOffset.floatValue;
				me.ShaderProperty(queueOffset, Tips.queueOffset);
				MGUI.SpaceN1();
				MGUI.DummyProperty("Render Queue:", mat.renderQueue.ToString());
				MGUI.SpaceN4();
			});
			MGUI.Space1();
			MGUI.PropertyGroupLayer(() => {
				MGUI.SpaceN2();
				MGUI.ToggleFloat(me, Tips.useFresnel, useFresnel, fresnelStrength);
				MGUI.ToggleFloat(me, Tips.highlightsText, highlights, specularStrength);
				MGUI.ToggleFloat(me, Tips.reflectionsText, reflections, reflectionStrength);
				MGUI.ToggleFloat(me, Tips.ssrText, ssr, ssrStrength);
				if (ssr.floatValue == 1){
					me.ShaderProperty(edgeFade, Tips.edgeFadeText);
				}
				MGUI.ToggleFloat(me, Tips.reflVertexColor, reflVertexColor, reflVertexColorStrength);
				MGUI.ToggleFloat(me, Tips.reflShadows, reflShadows, reflShadowStrength);
				if (reflShadows.floatValue == 1){
					me.ShaderProperty(brightnessReflShad, "Brightness", 1);
					me.ShaderProperty(contrastReflShad, "Contrast", 1);
					me.ShaderProperty(hdrReflShad, "HDR", 1);
				}
				MGUI.ToggleFloat(me, Tips.gsaa, gsaa, gsaaStrength);
			});
			MGUI.Space1();
			MGUI.PropertyGroupLayer(() => {
				MGUI.SpaceN3();
				me.ShaderProperty(_BakeryMode, Tips.bakeryMode);
				me.ShaderProperty(_BAKERY_SHNONLINEAR, "Bakery Non-Linear SH");
				me.ShaderProperty(_BAKERY_LMSPEC, "Bakery Lightmap Specular");
				me.ShaderProperty(bicubicLightmap, Tips.bicubicLightmap);
				#if LTCGI_INCLUDED
					me.ShaderProperty(ltcgi, "LTCGI");
					if (ltcgi.floatValue == 1){
						me.ShaderProperty(ltcgi_spec_off, "LTCGI Disable Specular");
						me.ShaderProperty(ltcgi_diffuse_off, "LTCGI Disable Diffuse");
					}
				#else
					ltcgi.floatValue = 0;
					mat.DisableKeyword("LTCGI");
				#endif
				me.EnableInstancingField();
				MGUI.SpaceN2();
				me.DoubleSidedGIField();
				MGUI.SpaceN3();
			});

			if (ssr.floatValue == 1){
				MGUI.DisplayInfo("\nScreenspace reflections in VRChat requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs\n\nIt is also is VERY expensive, please use it sparingly!\n");
			}
			MGUI.Space1();
			// me.TexturePropertySingleLine(new GUIContent("RNM0"), _RNM0);
			// me.TexturePropertySingleLine(new GUIContent("RNM1"), _RNM1);
			// me.TexturePropertySingleLine(new GUIContent("RNM2"), _RNM2);
		});
	}

	void DoFilteringArea(){
		MGUI.PropertyGroup( () => {
			me.ShaderProperty(filtering, "Enable");
			MGUI.BoldLabel("Base Color");
			MGUI.PropertyGroupLayer(()=>{
				MGUI.SpaceN1();
				me.ShaderProperty(hue, "Hue");
				me.ShaderProperty(saturation, "Saturation");
				me.ShaderProperty(brightness, "Brightness");
				me.ShaderProperty(contrast, "Contrast");
				MGUI.SpaceN2();
			});
			MGUI.Space4();
			MGUI.BoldLabel("Detail Color");
			MGUI.PropertyGroupLayer(()=>{
				MGUI.SpaceN1();
				me.ShaderProperty(hueDet, "Hue");
				me.ShaderProperty(saturationDet, "Saturation");
				me.ShaderProperty(brightnessDet, "Brightness");
				me.ShaderProperty(contrastDet, "Contrast");
				MGUI.SpaceN2();
			});
			if (emissionEnabled){
				MGUI.Space4();
				MGUI.BoldLabel("Emission Color");
				MGUI.PropertyGroupLayer(()=>{
					MGUI.SpaceN1();
					me.ShaderProperty(hueEmiss, "Hue");
					me.ShaderProperty(saturationEmiss, "Saturation");
					me.ShaderProperty(brightnessEmiss, "Brightness");
					me.ShaderProperty(contrastEmiss, "Contrast");
					MGUI.SpaceN2();
				});
			}
			MGUI.Space4();
			MGUI.BoldLabel("Global Color");
			MGUI.PropertyGroupLayer(()=>{
				MGUI.SpaceN1();
				me.ShaderProperty(huePost, "Hue");
				me.ShaderProperty(saturationPost, "Saturation");
				me.ShaderProperty(brightnessPost, "Brightness");
				me.ShaderProperty(contrastPost, "Contrast");
				MGUI.SpaceN2();
			});
		});
	}

	void DoFooter(){
		GUILayout.Space(20);
		float buttonSize = 35f;
		Rect footerRect = EditorGUILayout.GetControlRect();
		footerRect.x += (MGUI.GetInspectorWidth()/2f)-buttonSize-5f;
		footerRect.width = buttonSize;
		footerRect.height = buttonSize;
		if (GUI.Button(footerRect, MGUI.patIconTex))
			Application.OpenURL("https://www.patreon.com/mochieshaders");
		footerRect.x += buttonSize + 5f;
		footerRect.y += 17f;
		GUIStyle formatting = new GUIStyle();
		formatting.fontSize = 15;
		formatting.fontStyle = FontStyle.Bold;
		if (EditorGUIUtility.isProSkin){
			formatting.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1);
			formatting.hover.textColor = new Color(0.8f, 0.8f, 0.8f, 1);
			GUI.Label(footerRect, versionLabel, formatting);
			footerRect.y += 20f;
			footerRect.x -= 35f;
			footerRect.width = 70f;
			footerRect.height = 70f;
			GUI.Label(footerRect, MGUI.mochieLogoPro);
			GUILayout.Space(90);
		}
		else {
			GUI.Label(footerRect, versionLabel, formatting);
			footerRect.y += 20f;
			footerRect.x -= 35f;
			footerRect.width = 70f;
			footerRect.height = 70f;
			GUI.Label(footerRect, MGUI.mochieLogo);
			GUILayout.Space(90);
		}
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
		int blendModeEnum = material.GetInt("_BlendMode");
		MGUI.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap") || material.GetTexture("_DetailNormalMap"));
		MGUI.SetKeyword(material, "_WORKFLOW_PACKED_ON", workflow == 1);
		MGUI.SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
		MGUI.SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));
		MGUI.SetKeyword(material, "_DETAIL_MULX2", material.GetTexture("_DetailAlbedoMap"));
		MGUI.SetKeyword(material, "_REFLECTION_FALLBACK_ON", material.GetTexture("_ReflCube"));
		MGUI.SetKeyword(material, "_REFLECTION_OVERRIDE_ON", material.GetTexture("_ReflCubeOverride"));
		MGUI.SetKeyword(material, "_SCREENSPACE_REFLECTIONS_ON", material.GetInt("_SSR") == 1);
		MGUI.SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF", material.GetInt("_SpecularHighlights") == 0);
		MGUI.SetKeyword(material, "_GLOSSYREFLECTIONS_OFF", material.GetInt("_GlossyReflections") == 0);
		MGUI.SetKeyword(material, "_STOCHASTIC_ON",  samplingMode == 1);
		MGUI.SetKeyword(material, "_TSS_ON", samplingMode == 2);
		MGUI.SetKeyword(material, "_TRIPLANAR_ON", samplingMode == 3);
		MGUI.SetKeyword(material, "_DECAL_ON", samplingMode == 4);
		MGUI.SetKeyword(material, "_DETAIL_ROUGH_ON", material.GetTexture("_DetailRoughnessMap"));
		MGUI.SetKeyword(material, "_DETAIL_AO_ON", material.GetTexture("_DetailAOMap"));
		MGUI.SetKeyword(material, "_SUBSURFACE_ON", material.GetInt("_Subsurface") == 1);
		MGUI.SetKeyword(material, "_AUDIOLINK_ON", material.GetInt("_AudioLinkEmission") > 0);
		MGUI.SetKeyword(material, "_DETAIL_SAMPLEMODE_ON", material.GetInt("_DetailSamplingMode") == 1);
		MGUI.SetKeyword(material, "_ALPHAMASK_ON", blendModeEnum > 0 && material.GetInt("_UseAlphaMask") == 1);
		MGUI.SetKeyword(material, "_BICUBIC_SAMPLING_ON", material.GetInt("_BicubicLightmap") == 1);
		MGUI.SetKeyword(material, "BAKERY_SH", material.GetInt("_BakeryMode") == 1);
		MGUI.SetKeyword(material, "BAKERY_RNM", material.GetInt("_BakeryMode") == 2);
		MGUI.SetKeyword(material, "_FILTERING_ON", material.GetInt("_Filtering") == 1);

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