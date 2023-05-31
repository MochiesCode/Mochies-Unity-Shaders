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
		"Rim Light",
		"Subsurface Scattering",
		"Filtering",
		"Render Settings",
		"Reflections & Specular Highlights",
		"Rain",
		"AreaLit",
		"LTCGI"
	}, 1);

	string versionLabel = "v1.22.3";
	public static string receiverText = "AreaLit Maps";
	public static string emitterText = "AreaLit Light";
	public static string projectorText = "AreaLit Projector";
	// β

	MaterialProperty blendMode = null;
	MaterialProperty workflow = null;
	MaterialProperty albedoMap = null;
	MaterialProperty albedoColor = null;
	MaterialProperty detailColor = null;
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
	MaterialProperty detailMetallicMap = null;
	MaterialProperty detailMetallicBlend = null;
	MaterialProperty detailMetallicChannel = null;
	MaterialProperty detailOcclusionChannel = null;
	MaterialProperty detailRoughnessChannel = null;
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
	MaterialProperty audioLinkEmissionMeta = null;

	MaterialProperty huePost = null;
	MaterialProperty saturationPost = null;
	MaterialProperty contrastPost = null;
	MaterialProperty brightnessPost = null;
	MaterialProperty acesFilter = null;

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
	MaterialProperty uv4Rot = null;
	MaterialProperty uv5Rot = null;
	MaterialProperty uv0Scroll = null;
	MaterialProperty uv1Scroll = null;
	MaterialProperty uv2Scroll = null;
	MaterialProperty uv3Scroll = null;
	MaterialProperty uv4Scroll = null;
	MaterialProperty uv5Scroll = null;
	MaterialProperty reflCube = null;
	MaterialProperty cubeThreshold = null;
	MaterialProperty heightMult = null;
	MaterialProperty roughMult = null;
	MaterialProperty metalMult = null;
	MaterialProperty occMult = null;

	MaterialProperty detailWorkflow = null;
	MaterialProperty detailSamplingMode = null;
	MaterialProperty detailTriplanarFalloff = null;
	MaterialProperty detailPackedMap = null;
	MaterialProperty detailRoughMult = null;
	MaterialProperty detailMetalMult = null;
	MaterialProperty detailOccMult = null;
	// MaterialProperty detailUseSmoothness = null;
	MaterialProperty detailRoughnessStrength = null;
	MaterialProperty detailMetallicStrength = null;
	MaterialProperty detailOcclusionStrength = null;

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
	MaterialProperty useSmoothness = null;
	MaterialProperty detailMaskChannel = null;
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
	MaterialProperty tintReflShad = null;
	
	MaterialProperty audioLinkEmission = null;
	MaterialProperty audioLinkEmissionStrength = null;

	MaterialProperty uvPri = null;
	MaterialProperty uvEmissMask = null;
	MaterialProperty uvHeightMask = null;

	MaterialProperty alphaMask = null;
	MaterialProperty uvAlphaMask = null;
	MaterialProperty useAlphaMask = null;
	MaterialProperty alphaMaskChannel = null;
	MaterialProperty alphaMaskOpacity = null;
	MaterialProperty useFresnel = null;
	MaterialProperty fresnelStrength = null;

	MaterialProperty rainMask = null;
	MaterialProperty uvRainMask = null;
	MaterialProperty rainToggle = null;
	MaterialProperty rippleScale = null;
	MaterialProperty rippleSpeed = null;
	MaterialProperty rippleStr = null;

	MaterialProperty rimTog = null;
	MaterialProperty rimStr = null;
	MaterialProperty rimBlend = null;
	MaterialProperty rimCol = null;
	MaterialProperty rimWidth = null;
	MaterialProperty rimEdge = null;
	MaterialProperty rimMask = null;
	MaterialProperty uvRimMask = null;
	MaterialProperty uvRimMaskScroll = null;
	MaterialProperty uvRimMaskRot = null;

	MaterialProperty uvDetailMask = null;
	MaterialProperty detailScroll = null;
	MaterialProperty detailRotate = null;

	MaterialProperty filtering = null;
	MaterialProperty bicubicLightmap = null;
	MaterialProperty ltcgi = null;
	MaterialProperty ltcgi_diffuse_off = null;
	MaterialProperty ltcgi_spec_off = null;
	MaterialProperty ltcgiStrength = null;
	MaterialProperty _BakeryMode = null;
	MaterialProperty _BAKERY_LMSPEC = null;
	MaterialProperty _BAKERY_SHNONLINEAR = null;
	// MaterialProperty _RNM0 = null;
	// MaterialProperty _RNM1 = null;
	// MaterialProperty _RNM2 = null;

	MaterialProperty areaLitToggle = null;
	MaterialProperty areaLitStrength = null;
	MaterialProperty areaLitRoughnessMult = null;
	MaterialProperty lightMesh = null;
	MaterialProperty lightTex0 = null;
	MaterialProperty lightTex1 = null;
	MaterialProperty lightTex2 = null;
	MaterialProperty lightTex3 = null;
	MaterialProperty opaqueLights = null;

	MaterialProperty mirrorToggle = null;

	MaterialProperty occlusionUVSet = null;
	MaterialProperty areaLitOcclusion = null;

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
		culling = FindProperty("_Cull", props);
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
		_BAKERY_LMSPEC = FindProperty("_BAKERY_LMSPEC", props);
		_BAKERY_SHNONLINEAR = FindProperty("_BAKERY_SHNONLINEAR", props);
		filtering = FindProperty("_Filtering", props);
		tintReflShad = FindProperty("_TintReflShad", props);
		ltcgiStrength = FindProperty("_LTCGIStrength", props);
		acesFilter = FindProperty("_ACES", props);
		rainToggle = FindProperty("_RainToggle", props);
		rippleScale = FindProperty("_RippleScale", props);
		rippleSpeed = FindProperty("_RippleSpeed", props);
		rippleStr = FindProperty("_RippleStr", props);
		rainMask = FindProperty("_RainMask", props);
		uv5Rot = FindProperty("_UV5Rotate", props);
		uv5Scroll = FindProperty("_UV5Scroll", props);
		uvRainMask = FindProperty("_UVRainMask", props);
		mirrorToggle = FindProperty("_MirrorToggle", props);
		areaLitToggle = FindProperty("_AreaLitToggle", props);
		lightMesh = FindProperty("_LightMesh", props);
		lightTex0 = FindProperty("_LightTex0", props);
		lightTex1 = FindProperty("_LightTex1", props);
		lightTex2 = FindProperty("_LightTex2", props);
		lightTex3 = FindProperty("_LightTex3", props);
		opaqueLights = FindProperty("_OpaqueLights", props);
		detailColor = FindProperty("_DetailColor", props);
		detailMetallicMap = FindProperty("_DetailMetallicMap", props);
		detailWorkflow = FindProperty("_DetailWorkflow", props);
		detailSamplingMode = FindProperty("_DetailSamplingMode", props);
		detailTriplanarFalloff = FindProperty("_DetailTriplanarFalloff", props);
		detailPackedMap = FindProperty("_DetailPackedMap", props);
		detailRoughMult = FindProperty("_DetailRoughnessMult", props);
		detailMetalMult = FindProperty("_DetailMetallicMult", props);
		detailOccMult = FindProperty("_DetailOcclusionMult", props);
		detailMetallicBlend = FindProperty("_DetailMetallicBlend", props);
		detailMetallicChannel = FindProperty("_DetailMetallicChannel", props);
		detailOcclusionChannel = FindProperty("_DetailOcclusionChannel", props);
		detailRoughnessChannel = FindProperty("_DetailRoughnessChannel", props);
		detailRoughnessStrength = FindProperty("_DetailRoughnessStrength", props);
		detailMetallicStrength = FindProperty("_DetailMetallicStrength", props);
		detailOcclusionStrength = FindProperty("_DetailOcclusionStrength", props);
		areaLitStrength = FindProperty("_AreaLitStrength", props);
		areaLitRoughnessMult = FindProperty("_AreaLitRoughnessMult", props);
		rimMask = FindProperty("_RimMask", props);
		uvRimMask = FindProperty("_UVRimMask", props);
		uvRimMaskScroll = FindProperty("_UVRimMaskScroll", props);
		uvRimMaskRot = FindProperty("_UVRimMaskRotate", props);
		audioLinkEmissionMeta = FindProperty("_AudioLinkEmissionMeta", props);
		occlusionUVSet = FindProperty("_OcclusionUVSet", props);
		areaLitOcclusion = FindProperty("_AreaLitOcclusion", props);
		uvDetailMask = FindProperty("_UVDetailMask", props);
		detailScroll = FindProperty("_DetailScroll", props);
		detailRotate = FindProperty("_DetailRotate", props);
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

		bool isLite = MGUI.IsLiteVersion(material);

		// Detect any changes to the material
		EditorGUI.BeginChangeCheck();{
			
			// Core Shader Variant
			MGUI.BoldLabel("Shader Variant");
			DoVariantArea(material);
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
			
			// Reflections & Specular Highlights
			bool reflSpecFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "Reflections & Specular Highlights");
			if (reflSpecFoldout){
				DoReflSpecArea(isLite);
			}

			if (!isLite){
				// Rim
				bool rimFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "Rim Light");
				if (rimFoldout){
					DoRimArea();
				}

				// Subsurface
				bool subsurfaceArea = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "Subsurface Scattering");
				if (subsurfaceArea){
					DoSubsurfaceArea();
				}

				// Rain
				bool rainArea = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "Rain");
				if (rainArea){
					DoRainArea();
				}

				// Filtering
				bool filteringFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "Filtering");
				if (filteringFoldout){
					DoFilteringArea();
				}
			}

			// UVs
			bool uvFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "UVs");
			if (uvFoldout){
				DoUVArea();
			}

			// Rendering options
			bool renderFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "Render Settings");
			if (renderFoldout){
				DoRenderingArea(material, isLite);
			}

			// AreaLit
			if (Shader.Find("AreaLit/Standard") != null){
				bool arealitFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "AreaLit");
				if (arealitFoldout){
					DoAreaLitArea();
				}
			}
			else {
				areaLitToggle.floatValue = 0f;
				material.SetInt("_AreaLitToggle", 0);
				material.DisableKeyword("_AREALIT_ON");
			}

			// LTCGI
			if (Shader.Find("LTCGI/Blur Prefilter") != null){
				bool ltcgiFoldout = Foldouts.DoSmallFoldoutBold(foldouts, material, me, "LTCGI");
				if (ltcgiFoldout){
					DoLTCGIArea();
				}
			}
			else {
				ltcgi.floatValue = 0;
				ltcgi_diffuse_off.floatValue = 0;
				ltcgi_spec_off.floatValue = 0;
				material.SetInt("_LTCGI", 0);
				material.SetInt("_LTCGI_DIFFUSE_OFF", 0);
				material.SetInt("_LTCGI_SPECULAR_OFF", 0);
				material.DisableKeyword("LTCGI");
				material.DisableKeyword("LTCGI_DIFFUSE_OFF");
				material.DisableKeyword("LTCGI_SPECULAR_OFF");
			}

			// Watermark and version display
			MGUI.DoFooter(versionLabel);
		}

		// Ensure settings are applied correctly if anything changed
		if (EditorGUI.EndChangeCheck()){
			foreach (var obj in blendMode.targets)
				MaterialChanged((Material)obj);
		}

		MGUI.Space8();
	}

	void DoVariantArea(Material mat){	
		MGUI.PropertyGroup(()=>{
			me.ShaderProperty(blendMode, Tips.standBlendMode);
			me.ShaderProperty(useSmoothness, Tips.useSmoothness);
			if (blendMode.floatValue > 0){
				me.ShaderProperty(useAlphaMask, Tips.separateAlpha);
			}
			if (blendMode.floatValue == 1)
				me.ShaderProperty(alphaCutoff, Tips.alphaCutoffText);
			// MGUI.SpaceN2();
			// if (MGUI.PropertyButton("Unity Standard Packing Format")){
			// 	ApplyStandardPackingFormat(mat);
			// }
		});
	}

	void DoPrimaryArea(Material material){
		MGUI.PropertyGroup(()=>{
			me.ShaderProperty(workflow, Tips.standWorkflow);
			me.ShaderProperty(samplingMode, Tips.samplingMode);
			if (workflow.floatValue > 0 && samplingMode.floatValue < 3)
				me.ShaderProperty(useHeight, Tips.useHeight);
			if (samplingMode.floatValue == 3){
				me.ShaderProperty(triplanarFalloff, Tips.triplanarFalloff);
			}
		});
		MGUI.PropertyGroup(()=>{
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
			DoEmissionArea(material);
		});
	}

	void DoDetailArea(){
		MGUI.BoldLabel("Detail Textures");
		MGUI.PropertyGroup(()=>{
			me.ShaderProperty(detailWorkflow, Tips.standWorkflow);
			me.ShaderProperty(detailSamplingMode, Tips.samplingMode);
			// me.ShaderProperty(detailUseSmoothness, Tips.useSmoothness);
			if (detailSamplingMode.floatValue == 3){
				me.ShaderProperty(detailTriplanarFalloff, Tips.triplanarFalloff);
			}
			// MGUI.SpaceN2();
			// if (MGUI.PropertyButton("Unity Standard Packing Format")){
			// 	ApplyStandardPackingFormat(material);
			// }
		});
		MGUI.PropertyGroup(() => {
			GUIContent roughLabel = Tips.roughnessText;
			GUIContent roughBlendLabel = new GUIContent("Balls");
			GUIContent roughStrLabel = Tips.roughnessPackedText;
			if (useSmoothness.floatValue == 1){
				roughLabel.text = "Smoothness";
				roughBlendLabel.text = "Smoothness Blend";
				roughStrLabel.text = "Smoothness Strength";
			}
			else {
				roughLabel.text = "Roughness";
				roughBlendLabel.text = "Roughness Blend";
				roughStrLabel.text = "Roughness Strength";

			}
			bool hasDetailAlbedo = detailAlbedoMap.textureValue;
			me.TexturePropertySingleLine(Tips.albedoText, detailAlbedoMap, hasDetailAlbedo ? detailColor : null, hasDetailAlbedo ? detailAlbedoBlend : null);
			if (detailWorkflow.floatValue == 1){
				me.TexturePropertySingleLine(Tips.packedMapText, detailPackedMap);
				MGUI.sRGBWarning(detailPackedMap);
				if (detailPackedMap.textureValue){
					MGUI.PropertyGroupLayer(()=>{
						MGUI.SpaceN3();
						me.ShaderProperty(detailMetallicChannel, "Metallic");
						me.ShaderProperty(detailRoughnessChannel, roughLabel.text);
						me.ShaderProperty(detailOcclusionChannel, "Occlusion");
						MGUI.SpaceN3();
					});
					MGUI.PropertyGroupLayer(()=>{
						MGUI.SpaceN2();
						me.ShaderProperty(detailMetallicBlend, "Metallic Blend");
						me.ShaderProperty(detailRoughBlend, roughBlendLabel);
						me.ShaderProperty(detailAOBlend, "Occlusion Blend");
						MGUI.SpaceN2();
					});
					MGUI.PropertyGroupLayer(()=>{
						MGUI.SpaceN2();
						MGUI.ToggleSlider(me, Tips.metallicPackedText, detailMetalMult, detailMetallicStrength);
						MGUI.ToggleSlider(me, roughStrLabel, detailRoughMult, detailRoughnessStrength);
						MGUI.ToggleSlider(me, Tips.occlusionPackedText, detailOccMult, detailOcclusionStrength);
						MGUI.SpaceN2();
					});
				}
			}
			else {
				bool hasMetallic = detailMetallicMap.textureValue;
				bool hasRoughness = detailRoughnessMap.textureValue;
				bool hasAO = detailAOMap.textureValue;
				me.TexturePropertySingleLine(Tips.metallicMapText, detailMetallicMap, hasMetallic ? detailMetallicStrength : null, hasMetallic ? detailMetallicBlend : null);
				MGUI.sRGBWarning(detailMetallicMap);
				me.TexturePropertySingleLine(Tips.roughnessText, detailRoughnessMap, hasRoughness ? detailRoughnessStrength : null, hasRoughness ? detailRoughBlend : null);
				MGUI.sRGBWarning(detailRoughnessMap);
				me.TexturePropertySingleLine(Tips.occlusionText, detailAOMap, hasAO ? detailOcclusionStrength : null, hasAO ? detailAOBlend : null);
				me.TexturePropertySingleLine(Tips.normalMapText, detailNormalMap, detailNormalMap.textureValue ? detailNormalMapScale : null);
				me.TexturePropertySingleLine(Tips.detailMaskText, detailMask, detailMask.textureValue ? detailMaskChannel : null);
				MGUI.sRGBWarning(detailAOMap);
			}
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
				if (audioLinkEmission.floatValue > 0){
					me.ShaderProperty(audioLinkEmissionMeta, Tips.audioLinkEmissionMeta);
				}
				MGUI.Space2();
				
				me.TexturePropertySingleLine(Tips.emissionText, emissionMap, emissionColorForRendering, emissIntensity);
				// me.TexturePropertyWithHDRColor(Tips.emissionText, emissionMap, emissionColorForRendering, false);
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

	void DoReflSpecArea(bool isLite){;
		MGUI.PropertyGroup(() => {
			MGUI.PropertyGroupLayer(()=>{
				MGUI.SpaceN2();
				MGUI.ToggleFloat(me, Tips.highlightsText, highlights, specularStrength);
				MGUI.ToggleFloat(me, Tips.reflectionsText, reflections, reflectionStrength);
				if (!isLite){
					MGUI.ToggleFloat(me, Tips.ssrText, ssr, ssrStrength);
					if (ssr.floatValue == 1){
						me.ShaderProperty(edgeFade, Tips.edgeFadeText);
					}
					MGUI.ToggleFloat(me, Tips.reflVertexColor, reflVertexColor, reflVertexColorStrength);
				}
				MGUI.ToggleFloat(me, Tips.reflShadows, reflShadows, reflShadowStrength);
				if (reflShadows.floatValue == 1){
					MGUI.PropertyGroupLayer(() =>{
						MGUI.SpaceN2();
						me.ShaderProperty(tintReflShad, "Lightmap Tint");
						me.ShaderProperty(brightnessReflShad, "Brightness");
						me.ShaderProperty(contrastReflShad, "Contrast");
						me.ShaderProperty(hdrReflShad, "HDR");
						MGUI.SpaceN2();
					});
				}
				MGUI.ToggleFloat(me, Tips.gsaa, gsaa, gsaaStrength);
				MGUI.ToggleFloat(me, Tips.useFresnel, useFresnel, fresnelStrength);
				MGUI.SpaceN2();
			});
			if (mirrorToggle.floatValue == 0 && !isLite){
				MGUI.PropertyGroupLayer(()=>{
					MGUI.SpaceN4();
					me.TexturePropertySingleLine(Tips.reflOverrideText, reflOverride);
					me.TexturePropertySingleLine(Tips.reflCubeText, reflCube, reflCube.textureValue ? cubeThreshold : null);
					MGUI.SpaceN4();
				});
			}
		});
	}

	void DoRimArea(){
		MGUI.PropertyGroup(()=>{
			me.ShaderProperty(rimTog, "Enable");
			MGUI.ToggleGroup(rimTog.floatValue == 0);
			MGUI.PropertyGroupLayer(() => {
				MGUI.SpaceN2();
				me.TexturePropertySingleLine(Tips.maskLabel, rimMask);
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

	void DoRainArea(){
		MGUI.PropertyGroup(()=>{
			me.ShaderProperty(rainToggle, "Enable");
			MGUI.ToggleGroup(rainToggle.floatValue == 0);
			MGUI.PropertyGroupLayer(()=>{
				MGUI.SpaceN2();
				me.TexturePropertySingleLine(Tips.maskText, rainMask);
				me.ShaderProperty(rippleStr, "Strength");
				me.ShaderProperty(rippleScale, "Scale");
				me.ShaderProperty(rippleSpeed, "Speed");
				MGUI.SpaceN2();
			});
			MGUI.ToggleGroupEnd();
		});
	}

	void DoUVArea(){
		bool needsHeightMaskUV = (((workflow.floatValue > 0 && useHeight.floatValue == 1) || (workflow.floatValue == 0 && heightMap.textureValue)) && parallaxMask.textureValue) && samplingMode.floatValue < 3;
		bool needsEmissMaskUV = emissionEnabled && emissionMask.textureValue;
		bool needsAlphaMaskUV = blendMode.floatValue > 0 && useAlphaMask.floatValue > 0;
		bool needsRainMaskUV = rainToggle.floatValue == 1 && rainMask.textureValue;
		bool needsRimMaskUV = rimTog.floatValue == 1 && rimMask.textureValue;
		bool needsDetailMaskUV = detailMask.textureValue;

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
				if (detailSamplingMode.floatValue < 3){
					me.ShaderProperty(uvSetSecondary, Tips.uvSetLabel.text);
					MGUI.TextureSOScroll(me, detailAlbedoMap, uv1Scroll);
				}
				else {
					MGUI.TextureSO(me, detailAlbedoMap);
				}
				me.ShaderProperty(uv1Rot, "Rotation");
				MGUI.SpaceN4();
			});
			if (needsDetailMaskUV){
				MGUI.Space4();
				MGUI.BoldLabel("Detail Mask");
				MGUI.PropertyGroupLayer(()=>{
					MGUI.SpaceN2();
					me.ShaderProperty(uvDetailMask, Tips.uvSetLabel.text);
					MGUI.TextureSOScroll(me, detailMask, detailScroll);
					me.ShaderProperty(detailRotate, "Rotation");
					MGUI.SpaceN2();
				});
			}
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
			if (needsRainMaskUV){
				MGUI.Space4();
				MGUI.BoldLabel("Rain Mask");
				MGUI.PropertyGroupLayer(()=>{
					MGUI.SpaceN2();
					me.ShaderProperty(uvRainMask, Tips.uvSetLabel.text);
					MGUI.TextureSOScroll(me, rainMask, uv5Scroll);
					me.ShaderProperty(uv5Rot, "Rotation");
					MGUI.SpaceN2();
				});
			}
			if (needsRimMaskUV){
				MGUI.Space4();
				MGUI.BoldLabel("Rim Mask");
				MGUI.PropertyGroupLayer(()=>{
					MGUI.SpaceN2();
					me.ShaderProperty(uvRimMask, Tips.uvSetLabel.text);
					MGUI.TextureSOScroll(me, rimMask, uvRimMaskScroll);
					me.ShaderProperty(uvRimMaskRot, "Rotation");
					MGUI.SpaceN2();
				});
			}
		});
	}

	void DoRenderingArea(Material mat, bool isLite){
		MGUI.PropertyGroup(()=>{
			MGUI.PropertyGroupLayer(() => {
				MGUI.SpaceN2();
				me.ShaderProperty(culling, Tips.culling);
				queueOffset.floatValue = (int)queueOffset.floatValue;
				me.ShaderProperty(queueOffset, Tips.queueOffset);
				MGUI.SpaceN1();
				MGUI.DummyProperty("Render Queue:", mat.renderQueue.ToString());
				if (!isLite)
					me.ShaderProperty(mirrorToggle, Tips.mirrorMode);
				MGUI.SpaceN4();
			});

			MGUI.Space1();
			MGUI.PropertyGroupLayer(() => {
				MGUI.SpaceN3();
				me.ShaderProperty(_BakeryMode, Tips.bakeryMode);
				me.ShaderProperty(_BAKERY_SHNONLINEAR, "Bakery Non-Linear SH");
				me.ShaderProperty(_BAKERY_LMSPEC, "Bakery Lightmap Specular");
				me.ShaderProperty(bicubicLightmap, Tips.bicubicLightmap);
				me.EnableInstancingField();
				MGUI.SpaceN2();
				me.DoubleSidedGIField();
				MGUI.SpaceN3();
			});

			if (ssr.floatValue == 1 && !isLite){
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
			MGUI.ToggleGroup(filtering.floatValue == 0);
			MGUI.BoldLabel("Global Color");
			MGUI.PropertyGroupLayer(()=>{
				MGUI.SpaceN1();
				me.ShaderProperty(huePost, "Hue");
				me.ShaderProperty(saturationPost, "Saturation");
				me.ShaderProperty(brightnessPost, "Brightness");
				me.ShaderProperty(contrastPost, "Contrast");
				me.ShaderProperty(acesFilter, Tips.aces);
				MGUI.SpaceN2();
			});
			MGUI.Space4();
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
			MGUI.ToggleGroupEnd();
		});
	}

	void CheckTrilinear(Texture tex) {
		if(!tex)
			return;
		if(tex.mipmapCount <= 1) {
			me.HelpBoxWithButton(
				EditorGUIUtility.TrTextContent("Mip maps are required, please enable them in the texture import settings."),
				EditorGUIUtility.TrTextContent("OK"));
			return;
		}
		if(tex.filterMode != FilterMode.Trilinear) {
			if(me.HelpBoxWithButton(
				EditorGUIUtility.TrTextContent("Trilinear filtering is required, and aniso is recommended."),
				EditorGUIUtility.TrTextContent("Fix Now"))) {
				tex.filterMode = FilterMode.Trilinear;
				tex.anisoLevel = 1;
				EditorUtility.SetDirty(tex);
			}
			return;
		}
	}

	void DoAreaLitArea(){
		MGUI.PropertyGroup(()=>{
			me.ShaderProperty(areaLitToggle, "Enable");
			MGUI.ToggleGroup(areaLitToggle.floatValue == 0);
			MGUI.PropertyGroupLayer(()=>{
				MGUI.SpaceN2();
				me.ShaderProperty(areaLitStrength, "Strength");
				me.ShaderProperty(areaLitRoughnessMult, "Roughness Multiplier");
				me.ShaderProperty(opaqueLights, Tips.opaqueLightsText);
				MGUI.SpaceN2();
			});
			MGUI.PropertyGroupLayer(()=>{
				MGUI.SpaceN2();
				var lightMeshText = !lightMesh.textureValue ? Tips.lightMeshText : new GUIContent(
					Tips.lightMeshText.text + $" (max: {lightMesh.textureValue.height})", Tips.lightMeshText.tooltip
				);
				me.TexturePropertySingleLine(lightMeshText, lightMesh);
				me.TexturePropertySingleLine(Tips.lightTex0Text, lightTex0);
				CheckTrilinear(lightTex0.textureValue);
				me.TexturePropertySingleLine(Tips.lightTex1Text, lightTex1);
				CheckTrilinear(lightTex1.textureValue);
				me.TexturePropertySingleLine(Tips.lightTex2Text, lightTex2);
				CheckTrilinear(lightTex2.textureValue);
				me.TexturePropertySingleLine(Tips.lightTex3Text, lightTex3);
				CheckTrilinear(lightTex3.textureValue);
				me.TexturePropertySingleLine(new GUIContent("Occlusion"), areaLitOcclusion);
				if (areaLitOcclusion.textureValue){
					me.ShaderProperty(occlusionUVSet, "UV Set");
				}
				MGUI.TextureSO(me, areaLitOcclusion, areaLitOcclusion.textureValue);
				MGUI.SpaceN2();
			});
			MGUI.ToggleGroupEnd();
			MGUI.DisplayInfo("Note that the AreaLit package files MUST be inside a folder named AreaLit (case sensitive) directly in the Assets folder (Assets/AreaLit)");
		});
	}

	void DoLTCGIArea(){
		MGUI.PropertyGroup(()=>{
			me.ShaderProperty(ltcgi, "Enable");
			MGUI.ToggleGroup(ltcgi.floatValue == 0);
			MGUI.PropertyGroupLayer(()=>{
				MGUI.SpaceN2();
				me.ShaderProperty(ltcgiStrength, "Strength");
				me.ShaderProperty(ltcgi_spec_off, "Disable Specular");
				me.ShaderProperty(ltcgi_diffuse_off, "Disable Diffuse");
				MGUI.SpaceN2();
			});
			MGUI.ToggleGroupEnd();
		});
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
		int ssrToggle = material.GetInt("_SSR");
		bool isLite = MGUI.IsLiteVersion(material);

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
				if (ssrToggle == 1 && !isLite)
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
				if (ssrToggle == 1 && !isLite)
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
		int detailWorkflow = material.GetInt("_DetailWorkflow");
		int samplingMode = material.GetInt("_SamplingMode");
		int blendModeEnum = material.GetInt("_BlendMode");
		int ltcgiToggle = material.GetInt("_LTCGI");
		int detailSamplingMode = material.GetInt("_DetailSamplingMode");
		bool isLite = MGUI.IsLiteVersion(material);

		material.SetInt("_ReflCubeToggle", material.GetTexture("_ReflCube") ? 1 : 0);
		material.SetInt("_ReflCubeOverrideToggle", material.GetTexture("_ReflCubeOverride") ? 1 : 0);
		MGUI.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap") || material.GetTexture("_DetailNormalMap"));
		MGUI.SetKeyword(material, "_WORKFLOW_PACKED_ON", workflow == 1);
		MGUI.SetKeyword(material, "_DETAIL_WORKFLOW_PACKED_ON", detailWorkflow == 1);
		MGUI.SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
		MGUI.SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));
		MGUI.SetKeyword(material, "_DETAIL_MULX2", material.GetTexture("_DetailAlbedoMap"));
		MGUI.SetKeyword(material, "_SCREENSPACE_REFLECTIONS_ON", material.GetInt("_SSR") == 1 && !isLite);
		MGUI.SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF", material.GetInt("_SpecularHighlights") == 0);
		MGUI.SetKeyword(material, "_GLOSSYREFLECTIONS_OFF", material.GetInt("_GlossyReflections") == 0);
		MGUI.SetKeyword(material, "_STOCHASTIC_ON",  samplingMode == 1);
		MGUI.SetKeyword(material, "_TSS_ON", samplingMode == 2);
		MGUI.SetKeyword(material, "_TRIPLANAR_ON", samplingMode == 3);
		MGUI.SetKeyword(material, "_DETAIL_STOCHASTIC_ON", detailSamplingMode == 1);
		MGUI.SetKeyword(material, "_DETAIL_TSS_ON", detailSamplingMode == 2);
		MGUI.SetKeyword(material, "_DETAIL_TRIPLANAR_ON", detailSamplingMode == 3);
		MGUI.SetKeyword(material, "_DETAIL_ROUGH_ON", material.GetTexture("_DetailRoughnessMap"));
		MGUI.SetKeyword(material, "_DETAIL_AO_ON", material.GetTexture("_DetailAOMap"));
		MGUI.SetKeyword(material, "_DETAIL_METALLIC_ON", material.GetTexture("_DetailMetallicMap"));
		MGUI.SetKeyword(material, "_AUDIOLINK_ON", material.GetInt("_AudioLinkEmission") > 0);
		MGUI.SetKeyword(material, "_AUDIOLINK_META_ON", material.GetInt("_AudioLinkEmission") > 0 && material.GetInt("_AudioLinkEmissionMeta") > 0);
		MGUI.SetKeyword(material, "_ALPHAMASK_ON", blendModeEnum > 0 && material.GetInt("_UseAlphaMask") == 1);
		MGUI.SetKeyword(material, "_BICUBIC_SAMPLING_ON", material.GetInt("_BicubicLightmap") == 1);
		MGUI.SetKeyword(material, "BAKERY_SH", material.GetInt("_BakeryMode") == 1);
		MGUI.SetKeyword(material, "BAKERY_RNM", material.GetInt("_BakeryMode") == 2);
		MGUI.SetKeyword(material, "BAKERY_MONOSH", material.GetInt("_BakeryMode") == 3);
		MGUI.SetKeyword(material, "_AREALIT_ON", material.GetInt("_AreaLitToggle") == 1);
		MGUI.SetKeyword(material, "LTCGI", ltcgiToggle == 1);
		MGUI.SetKeyword(material, "LTCGI_DIFFUSE_OFF", material.GetInt("_LTCGI_DIFFUSE_OFF") == 1 && ltcgiToggle == 1);
		MGUI.SetKeyword(material, "LTCGI_SPECULAR_OFF", material.GetInt("_LTCGI_SPECULAR_OFF") == 1 && ltcgiToggle == 1);
		MGUI.SetKeyword(material, "_MIRROR_ON", material.GetInt("_MirrorToggle") == 1);

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

		material.SetShaderPassEnabled("Always", material.GetInt("_SSR") == 1 && !isLite);
	}

	static void MaterialChanged(Material material){
		SetupMaterialWithBlendMode(material);
		SetMaterialKeywords(material);
	}

	static void ApplyStandardPackingFormat(Material mat){
		mat.SetInt("_Workflow", 1);
		mat.SetInt("_UseSmoothness", 1);
		mat.SetInt("_MetallicChannel", 0);
		mat.SetInt("_RoughnessChannel", 3);
		mat.SetInt("_OcclusionChannel", 1);
		mat.SetInt("_HeightChannel", 2);
		MaterialChanged(mat);
	}
}