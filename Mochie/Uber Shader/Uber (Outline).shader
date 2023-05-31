// By Mochie#8794

Shader "Mochie/Uber (Outline)" {
    Properties {
		//----------------------------
		// BASE
		//----------------------------
		[ToggleUI]_Hide("tog", Int) = 0
		[Enum(Off,0, On,1)]_RenderMode("en2", Int) = 1
		[Enum(Opaque,0, Cutout,1, Dithered,2, Fade,4, Transparent,5)]_BlendMode("en5", Int) = 0
		[HideInInspector]_SrcBlend("inte", Int) = 1
		[HideInInspector]_DstBlend ("inte", Int) = 0
		[Enum(Off,0, On,1)]_ATM("en2", Int) = 0
		[Enum(Off,0, On,1)]_ZWrite("en2", Int) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("enx", Float) = 4.0
		[Enum(UnityEngine.Rendering.CullMode)]_CullingMode("enx", Int) = 2
		[Enum(Default,0, Only Reflection,1, Textured Reflection,2, No Reflection,3)]_MirrorBehavior("en4", Int) = 0
		[Enum(2D,0, Cubemap,1, Combined,2)]_CubeMode("en3", Int) = 0
		[Enum(Base Color Alpha,0, Alpha Mask,1)]_UseAlphaMask("en2", Int) = 0
		_Cutoff("ra", Range(0,1)) = 0.5
		_AlphaStrength("ra", Range(0,1)) = 1
		_NearClip("fl", Float) = 0
		_NearClipMask("tex", 2D) = "white" {}
		[ToggleUI]_NearClipToggle("tog", Int) = 0
		
		_MainTex("tex", 2D) = "white" {} // MainTex
		_MirrorTex("tex", 2D) = "white" {}
		_Color("col", Color) = (1,1,1,1)
		_VertexColor("ra", Range(0,1)) = 0
		[ToggleUI]_ColorPreservation("tog", Int) = 1
		_AlphaMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_AlphaMaskChannel("en03", Int) = 0
		_MainTexScroll("vec", Vector) = (0,0,0,0)
		_MainTexRot("fl", Float) = 0
		_DetailRot("fl", Float) = 0
		
		_CubeBlendMask("tex", 2D) = "white" {} // MainTex (Cubemap)
		_CubeBlend("ra", Range(0,1)) = 0
		_MainTexCube0("tex", CUBE) = "white" {}
		_CubeColor0("col", Color) = (1,1,1,1)
		[Enum(Add,0, Sub,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_CubeBlendMode("en6", Int) = 6
		_CubeRotate0("vec", Vector) = (180,0,0,0)
		[ToggleUI]_AutoRotate0("tog", Int) = 0

		//----------------------------
		// TEXTURE MAPPING
		//----------------------------
		//PRIMARY MAPS
		[Enum(Metallic,0, Specular,1, Specular (Alpha Smoothness),2, Packed,3)]_PBRWorkflow("en4", Int) = 0
		_MetallicGlossMap("tex", 2D) = "white" {}
		_Metallic("ra", Range(0,1)) = 0
		_SpecGlossMap("tex", 2D) = "white" {}
		_Glossiness("ra", Range(0,1)) = 0.5
		_SmoothnessMap("tex", 2D) = "white" {}
		_GlossMapScale("ra", Range(0,1)) = 1
		_OcclusionMap("tex", 2D) = "white" {}
		_OcclusionStrength("ra", Range(0,1)) = 1
		_BumpMap("tex", 2D) = "bump" {}
		_BumpScale("ra", Range(-2,2)) = 1
		_ParallaxMap("tex", 2D) = "white" {}
		_Parallax("ra", Range(0,0.1)) = 0.01
		_ParallaxOffset("ra", Range(-1,1)) = 0
		[IntRange]_ParallaxSteps("ra", Range(1,50)) = 25
		_DetailMask("tex", 2D) = "white" {}
		_PackedMap("tex", 2D) = "white" {} // Packed workflow
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_MetallicChannel("en4", Int) = 0 
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_RoughnessChannel("en4", Int) = 1
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_OcclusionChannel("en4", Int) = 2
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_HeightChannel("en4", Int) = 3
		[ToggleUI]_EnablePackedHeight("tog", Int) = 0

		// DETAIL MAPS
		_DetailAlbedoMap("tex", 2D) = "gray" {}
		_DetailAlbedoStrength("ra", Range(0,1)) = 1
		[Enum(Add,0, Sub,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_DetailAlbedoBlending("en6", Int) = 3
		_UsingDetailAlbedo("tog", Int) = 0
		_DetailRoughnessMap("tex", 2D) = "white" {}
		_DetailRoughStrength("ra", Range(0,1)) = 1
		[Enum(Add,0, Sub,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_DetailRoughBlending("en6", Int) = 6
		_UsingDetailRough("tog", Int) = 0
		_DetailOcclusionMap("tex", 2D) = "white" {}
		_DetailOcclusionStrength("ra", Range(0,1)) = 1
		[Enum(Add,0, Sub,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_DetailOcclusionBlending("en6", Int) = 2
		_UsingDetailOcclusion("tog", Int) = 0
		_DetailNormalMap("tex", 2D) = "bump" {}
		_DetailNormalMapScale("ra", Range(-2,2)) = 1
		_DetailScroll("vec", Vector) = (0,0,0,0)
		[Enum(UV0,0, UV1,1, UV2,2)]_UVSec("en3", Int) = 0

		// MASKING
		[Enum(Off,0, Separate,1, Packed,2)]_MaskingMode("en3", Int) = 0
		[ToggleUI]_EnableMaskTransform("tog", Int) = 0
		_ReflectionMask("tex", 2D) = "white" {}	
		_SpecularMask("tex", 2D) = "white" {}
		_MatcapMask("tex", 2D) = "white" {}
		_ShadowMask("tex", 2D) = "white" {}
		_RimMask("tex", 2D) = "white" {}
		_ERimMask("tex", 2D) = "white" {}
		_DiffuseMask("tex", 2D) = "white" {}
		_SubsurfaceMask("tex", 2D) = "white" {}
		_InterpMask("tex", 2D) = "white" {}
		_DistortUVMask("tex", 2D) = "white" {}
		_MatcapBlendMask("tex", 2D) = "white" {}
		_EmissMask("tex", 2D) = "white" {}
		_FilterMask("tex", 2D) = "white" {}
		_RefractionMask("tex", 2D) = "white" {}
		_OutlineMask("tex", 2D) = "white" {}
		_PackedMask0("tex", 2D) = "white" {}
		_PackedMask1("tex", 2D) = "white" {}
		_PackedMask2("tex", 2D) = "white" {}
		_PackedMask3("tex", 2D) = "white" {}
		
		_ReflectionMaskScroll("vec", Vector) = (0,0,0,0)
		_SpecularMaskScroll("vec", Vector) = (0,0,0,0)
		_ERimMaskScroll("vec", Vector) = (0,0,0,0)
		_MatcapMaskScroll("vec", Vector) = (0,0,0,0)
		_MatcapBlendMaskScroll("vec", Vector) = (0,0,0,0)
		_InterpMaskScroll("vec", Vector) = (0,0,0,0)
		_SubsurfaceMaskScroll("vec", Vector) = (0,0,0,0)
		_RimMaskScroll("vec", Vector) = (0,0,0,0)
		_DetailMaskScroll("vec", Vector) = (0,0,0,0)
		_ShadowMaskScroll("vec", Vector) = (0,0,0,0)
		_DiffuseMaskScroll("vec", Vector) = (0,0,0,0)
		_FilterMaskScroll("vec", Vector) = (0,0,0,0)
		_EmissMaskScroll("vec", Vector) = (0,0,0,0)
		_EmissPulseMaskScroll("vec", Vector) = (0,0,0,0)
		_OutlineMaskScroll("vec", Vector) = (0,0,0,0)

		// PBR FILTERING
		[ToggleUI]_RoughnessFiltering("tog", Int) = 0 // Roughness
		[ToggleUI]_PreviewRough("tog", Int) = 0
		_RoughLightness("ra", Range(-1,1)) = 0
		_RoughIntensity("ra", Range(0,1)) = 0
		_RoughContrast("ra", Range(-1,2)) = 1
		_RoughRemapMin("fl", Float) = 0
		_RoughRemapMax("fl", Float) = 1
		[ToggleUI]_SmoothnessFiltering("tog", Int) = 0 // Smoothness
		[ToggleUI]_PreviewSmooth("tog", Int) = 0
		_SmoothLightness("ra", Range(-1,1)) = 0
		_SmoothIntensity("ra", Range(0,1)) = 0
		_SmoothContrast("ra", Range(-1,2)) = 1
		_SmoothRemapMin("fl", Float) = 0
		_SmoothRemapMax("fl", Float) = 1
		[ToggleUI]_AOFiltering("tog", Int) = 0 // AO
		[ToggleUI]_PreviewAO("tog", Int) = 0
		[ToggleUI]_DirectAO("tog", Int) = 1
		[ToggleUI]_IndirectAO("tog", Int) = 0
		_AOTintTex("tex", 2D) = "white" {}
		_AOTint("col", Color) = (0,0,0,1)
		_AOLightness("ra", Range(-1,1)) = 0
		_AOIntensity("ra", Range(0,1)) = 0
		_AOContrast("ra", Range(-1,2)) = 1
		_AORemapMin("fl", Float) = 0
		_AORemapMax("fl", Float) = 1
		[ToggleUI]_HeightFiltering("tog", Int) = 0 // Height
		[ToggleUI]_PreviewHeight("tog", Int) = 0
		_HeightLightness("ra", Range(-1,1)) = 0
		_HeightIntensity("ra", Range(0,1)) = 0
		_HeightContrast("ra", Range(-1,2)) = 1
		_HeightRemapMin("fl", Float) = 0
		_HeightRemapMax("fl", Float) = 1
		[ToggleUI]_MetallicFiltering("tog", Int) = 0
		[ToggleUI]_PreviewMetallic("tog", Int) = 0
		_MetallicLightness("ra", Range(-1,1)) = 0
		_MetallicIntensity("ra", Range(0,1)) = 0
		_MetallicContrast("ra", Range(-1,2)) = 1
		_MetallicRemapMin("fl", Float) = 0
		_MetallicRemapMax("fl", Float) = 1
		
		// Base Color Dissolve
		[ToggleUI]_BCDissolveToggle("tog", Int) = 0
		_MainTex2("tex", 2D) = "white" {}
		_BCColor("col", Color) = (1,1,1,1)
		_BCNoiseTex("tex", 2D) = "white" {}
		_BCDissolveStr("ra", Range(0,1)) = 0
		_BCRimWidth("ra", Float) = 0.5
		[HDR]_BCRimCol("col", Color) = (1,1,1,1)

		//----------------------------
		// SHADING
		//----------------------------
		// LIGHTING
		[ToggleUI]_StaticLightDirToggle("tog", Int) = 0
		_StaticLightDir("vec", Vector) = (0,0.75,1,0)
		_DisneyDiffuse("ra", Range(0,1)) = 0 // Diffuse Shading
		_SHStr("ra", Range(0,1)) = 0
		[ToggleUI]_NonlinearSHToggle("tog", Int) = 1
		_RTDirectCont("fl", Float) = 1 // Realtime Lighting
		_RTIndirectCont("fl", Float) = 1
		_VLightCont("fl", Float) = 1
		_AddCont("fl", Float) = 1
		[ToggleUI]_ClampAdditive("tog", Int) = 1
		_DirectCont("fl", Float) = 0.6 // Baked Lighting
		_IndirectCont("fl", Float) = 0.5

		// REFLECTIONS
		[Enum(Off,0, Environment,1, Cubemap,2)]_Reflections("en3", Int) = 0
		_ReflCube("tex", CUBE) = "white" {}
		[ToggleUI]_ReflCubeFallback("tog", Int) = 0
		_ReflCol("col", Color) = (1,1,1,1)
		_ReflectionStr("fl", Float) = 1
		[ToggleUI]_ReflUseRough("tog", Int) = 0
		_ReflRough("ra", Range(0,2)) = 0.5
		[ToggleUI]_ReflStepping("tog", Int) = 0
		[IntRange]_ReflSteps("ra", Range(1,15)) = 7
		[ToggleUI]_LightingBasedIOR("tog", Int) = 0
		[ToggleUI]_SSR("tog", Int) = 0
		[ToggleUI]_Dith("tog", Int) = 0
		_Alpha("ra", Range(0.0, 1.0)) = 1
		[IntRange]_MaxSteps ("ra", Range(1,50)) = 50
		_Step("fl", Float) = 0.09 
		_LRad("fl", Float) = 0.2
		_SRad("fl", Float) = 0.02
		_EdgeFade("ra", Range(0,1)) = 0.1
		[HideInInspector]_NoiseTexSSR("SSRNoise", 2D) = "black" {}
		[ToggleUI]_FresnelToggle("tog", Int) = 1
		_FresnelStrength("fl", Float) = 1

		// SPECULAR
		[Enum(Off,0, GGX,1, Anisotropic,2, Combined,3)]_Specular("en4", Int) = 0
		[Enum(UV0,0, UV1,1, UV2,2)]_UVAniso("en3", Int) = 0
		_SpecCol("col", Color) = (1,1,1,1)
		_SpecStr("fl", Float) = 1
		_AnisoStr("fl", Float) = 1
		[ToggleUI]_ManualSpecBright("tog", Int) = 0
		[ToggleUI]_SharpSpecular("tog", Int) = 0
		[ToggleUI]_RealtimeSpec("tog", Int) = 0
		[IntRange]_SharpSpecStr("ra", Range(1,15)) = 1
		[IntRange]_AnisoSteps("ra", Range(1,15)) = 2
        _AnisoAngleY("ra", Float) = 1
		_AnisoLayerY("fl", Float) = 1
		_AnisoLayerStr("ra", Range(0,1)) = 0.1
		[ToggleUI]_AnisoLerp("tog", Int) = 0
		_RippleFrequency("fl", Float) = 1
		_RippleAmplitude("fl", Float) = 0.5
		_RippleStrength("ra", Range(0,1)) = 0
		_RippleContinuity("ra", Range(0,1)) = 1
		[ToggleUI]_SpecUseRough("tog", Int) = 0
		_SpecRough("ra", Range(0,2)) = 0.5
		[ToggleUI]_SpecBiasOverrideToggle("tog", Int) = 0
		_SpecBiasOverride("ra",  Range(0,1)) = 0.5

		// MATCAP
		[ToggleUI]_MatcapToggle("tog", Int) = 0
		_Matcap("tex", 2D) = "black" {}
		_Matcap1("tex", 2D) = "black" {}
		_MatcapColor("col", Color) = (1,1,1,1)
		_MatcapColor1("col", Color) = (1,1,1,1)
		[Enum(Add,0, Alpha,1)]_MatcapBlending("en2", Int) = 0
		[Enum(Add,0, Alpha,1)]_MatcapBlending1("en2", Int) = 0
		_MatcapStr("fl", Float) = 1
		_MatcapStr1("fl", Float) = 1
		[ToggleUI]_UnlitMatcap("tog", Int) = 0
		[ToggleUI]_UnlitMatcap1("tog", Int) = 0
		[ToggleUI]_MatcapUseRough("tog", Int) = 0
		_MatcapRough("ra", Range(0,2)) = 0.5
		[ToggleUI]_MatcapUseRough1("tog", Int) = 0
		_MatcapRough1("ra", Range(0,2)) = 0.5
		[ToggleUI]_MatcapCenter("tog", Int) = 0
		[ToggleUI]_MatcapCenter1("tog", Int) = 0
		_MatcapNormal0("tex", 2D) = "bump" {}
		_MatcapNormal0Str("fl", Float) = 1
		_MatcapNormal0Scroll("vec", Vector) = (0,0,0,0)
		[ToggleUI]_MatcapNormal0Toggle("tog", Int) = 0
		_MatcapNormal1("tex", 2D) = "bump" {}
		_MatcapNormal1Str("fl", Float) = 1
		_MatcapNormal1Scroll("vec", Vector) = (0,0,0,0)
		[ToggleUI]_MatcapNormal1Toggle("tog", Int) = 0
		[ToggleUI]_MatcapNormal0Mix("tog", Int) = 0
		[ToggleUI]_MatcapNormal1Mix("tog", Int) = 0

		// SHADOWS
		[Enum(Off,0, Manual Blend,1, Ramp,2)]_ShadowMode("en3", Int) = 0
		[Enum(Always,0, Realtime Lighting Only,1, Baked Lighting Only,2)]_ShadowConditions("en3", Int) = 0
		[ToggleUI]_MainTexTint("tog", Int) = 0
		_ShadowTint("col", Color) = (0,0,0,1)
		_RampPos("ra", Range(-1,1)) = 0
		_RampWidth0("ra", Range(0.005,1)) = 0.005
		_RampWidth1("ra", Range(0.005,1)) = 0.5
		_RampWeight("ra", Range(0,1)) = 0
		_ShadowRamp("ShadowRamp", 2D) = "white" {}
		_DetailShadowMap("tex", 2D) = "white" {}
		_ShadowStr("ra", Range(0,1)) = 0.5
		[ToggleUI]_ShadowDithering("tog", Int) = 0
		[ToggleUI]_RTSelfShadow("tog", Int) = 1
		[ToggleUI]_AttenSmoothing("tog", Int) = 1
		[ToggleUI]_DitheredShadows("tog", Int) = 0
		
		// SUBSURFACE SCATTERING
		[ToggleUI]_Subsurface("tog", Int) = 0
		[ToggleUI]_ScatterBaseColorTint("tog", Int) = 0
		_ThicknessMap("tex", 2D) = "black" {}
		_ThicknessMapPower("fl", Float) = 1
		_ScatterTex("tex", 2D) = "white" {}
		_ScatterCol("col", Color) = (1,1,1,1)
		_ScatterIntensity("fl", Float) = 1
		_ScatterPow("fl", Float) = 1
		_ScatterDist("fl", Float) = 1
		_ScatterAmbient("fl", Float) = 0
		_ScatterWrap("ra", Range(0.001,1)) = 0.01

		// BASIC RIM
		[ToggleUI]_RimLighting("tog", Int) = 0
		_RimTex("tex", 2D) = "white" {}
		[HDR]_RimCol("col", Color) = (1,1,1,1)
		[Enum(Add,0, Sub,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_RimBlending("en4", Int) = 0
		_RimScroll("vec", Vector) = (0,0,0,0)
		_RimStr("ra", Range(0,1)) = 1
		_RimWidth("ra", Range (0,1)) = 0.5
		_RimEdge("ra", Range(0,0.5)) = 0
		[ToggleUI]_UnlitRim("tog", Int) = 1
		
		// ENVIRONMENT RIM
		[ToggleUI]_EnvironmentRim("tog", Int) = 0
		_ERimTex("tex", 2D) = "white" {}
		[HDR]_ERimTint("col", Color) = (1,1,1,1)
		[Enum(Add,0, Sub,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_ERimBlending("en4", Int) = 0
		_ERimScroll("vec", Vector) = (0,0,0,0)
		_ERimStr("ra", Range(0,1)) = 1
		_ERimWidth("ra", Range (0,1)) = 0.7
		_ERimEdge("ra", Range(0,0.5)) = 0
		_ERimRoughness("ra", Range(0,2)) = 0.5
		[ToggleUI]_ERimUseRough("tog", Int) = 0

		// REFRACTION
		[ToggleUI]_Refraction("tog", Int) = 0
		_RefractionTint("col", Color) = (1,1,1)
		_RefractionMaskScroll("vec", Vector) = (0,0,0,0)
		_RefractionMaskStr("ra", Range(0,1)) = 1
		_RefractionOpac("ra", Range(0,1)) = 0
		_RefractionIOR("fl", Float) = 1.3
		[ToggleUI]_UnlitRefraction("tog", Int) = 0
		[ToggleUI]_RefractionCA("tog", Int) = 0
		_RefractionCAStr("ra", Range(0,1)) = 0.1
		_RefractionDissolveMask("tex", 2D) = "white" {}
		_RefractionDissolveMaskScroll("vec", Vector) = (0,0,0,0)
		_RefractionDissolveMaskStr("ra", Range(0,1)) = 1
		[ToggleUI]_RefractionBlur("tog", Int) = 0
		_RefractionBlurStrength("ra", Range(0,1)) = 0
		[ToggleUI]_RefractionBlurRough("tog", Int) = 0
		
		// IRIDESCENCE
		[ToggleUI]_Iridescence("tog", Int) = 0
		_IridescenceStrength("ra", Range(0,1)) = 1
		_IridescenceHue("ra", Range(0,1)) = 0
		_IridescenceWidth("ra", Range(0,1)) = 0.7
		_IridescenceEdge("ra", Range(0,0.5)) = 0
		_IridescenceMask("tex", 2D) = "white" {}
		_IridescenceCurl("ra", Range(0,1)) = 1
		_IridescenceCurlScale("fl", Float) = 1

		// NORMALS
		[ToggleUI]_HardenNormals("tog", Int) = 0
		[ToggleUI]_ClearCoat("tog", Int) = 0
		[ToggleUI]_GSAA("tog", Int) = 0

		//----------------------------
		// EMISSION
		//----------------------------
		[ToggleUI]_EmissionToggle("tog", Int) = 0
		_EmissionMap("tex", 2D) = "white" {}
		[HDR]_EmissionColor("col", Color) = (0,0,0)
		_EmissScroll("vec", Vector) = (0,0,0,0)
		_EmissIntensity("ra", Float) = 1

		_EmissionMap2("tex", 2D) = "white" {}
		[HDR]_EmissionColor2("col", Color) = (0,0,0)
		_EmissScroll2("vec", Vector) = (0,0,0,0)
		_EmissIntensity2("ra", Float) = 1

		// LIGHT REACTIVITY
		[ToggleUI]_ReactToggle("tog", Int) = 0
		[ToggleUI]_CrossMode("tog", Int) = 0
		_Crossfade("ra", Range(0,0.2)) = 0.1
		_ReactThresh("ra", Range(0,1)) = 0.5

		// PULSE
		[ToggleUI]_PulseToggle("tog", Int) = 0
		[Enum(Sine,0, Square,1, Triangle,2, Saw,3, Reverse Saw,4)]_PulseWaveform("en5", Int) = 0
		_PulseMask("tex", 2D) = "white" {}
		_PulseStr("ra", Range(0,1)) = 0.5
		_PulseSpeed("fl", Float) = 1

		//----------------------------
		// FILTERS
		//----------------------------
		[ToggleUI]_Filtering("tog", Int) = 0
		[ToggleUI]_TeamFiltering("tog", Int) = 0
		[ToggleUI]_PostFiltering("tog", Int) = 0
		[ToggleUI]_Invert("tog", Int) = 0
		
		_RGB("vec", Vector) = (1,1,1,0)
		[ToggleUI]_AutoShift("tog", Int) = 0
		_AutoShiftSpeed("ra", Range(0,1)) = 0.25
		_Hue("ra", Range(0,1)) = 0
		_Value("fl", Float) = 0

		_TeamColorMask("tex", 2D) = "white" {} // Team Colors
		_TeamColor0("col", Color) = (1,1,1,1)
		_TeamColor1("col", Color) = (1,1,1,1)
		_TeamColor2("col", Color) = (1,1,1,1)
		_TeamColor3("col", Color) = (1,1,1,1)

		_Saturation("fl", Float) = 1 // Other settings
		_Brightness("fl", Float) = 1
		_Contrast("fl", Float) = 1
		_HDR("fl", Float) = 0
		_ACES("fl", Float) = 0
		
		//----------------------------
		// FLIPBOOK
		//----------------------------
		// SHEET 1
		[ToggleUI]_EnableSpritesheet("tog", Int) = 0
		[Enum(Flipbook,0, Sprite Sheet,1)]_SpritesheetMode0("en2", Int) = 0
		[ToggleUI]_UnlitSpritesheet("tog", Int) = 0
		_Spritesheet("tex", 2D) = "white" {}
		_Flipbook0("tex", 2DArray) = "white" {}
		_SpritesheetCol("col", Color) = (1,1,1,1)
		_SpritesheetBrightness("fl", Float) = 1
		[Enum(Add,0, Sub,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_SpritesheetBlending("en3", Int) = 6
		_RowsColumns("vec", Vector) = (8,8,0,0)
		_FrameClipOfs("vec", Vector) = (0,0,0,0)
		_SpritesheetPos("vec", Vector) = (0,0,0,0)
		_SpritesheetScale("vec", Vector) = (1,1,0,0)
		_SpritesheetRot("ra", Range(0,360)) = 0
		_FPS("ra", Range(1,120)) = 30
		[ToggleUI]_ManualScrub("tog", Int) = 0
		_ScrubPos("tog", Int) = 1
		[ToggleUI]_UseSpritesheetAlpha("tog", Int) = 0
		_Flipbook0Scroll("vec", Vector) = (0,0,0,0)
		[ToggleUI]_Flipbook0ClipEdge("tog", Int) = 0

		// SHEET 2
		[ToggleUI]_EnableSpritesheet1("tog", Int) = 0
		[Enum(Flipbook,0, Sprite Sheet,1)]_SpritesheetMode1("en2", Int) = 0
		[ToggleUI]_UnlitSpritesheet1("tog", Int) = 0
		_Spritesheet1("tex", 2D) = "white" {}
		_Flipbook1("tex", 2DArray) = "white" {}
		_SpritesheetCol1("col", Color) = (1,1,1,1)
		_SpritesheetBrightness1("fl", Float) = 1
		[Enum(Add,0, Sub,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_SpritesheetBlending1("en3", Int) = 6
		_RowsColumns1("vec", Vector) = (8,8,0,0)
		_FrameClipOfs1("vec", Vector) = (0,0,0,0)
		_SpritesheetPos1("vec", Vector) = (0,0,0,0)
		_SpritesheetScale1("vec", Vector) = (1,1,0,0)
		_SpritesheetRot1("ra", Range(0,360)) = 0
		_FPS1("ra", Range(1,120)) = 30
		[ToggleUI]_ManualScrub1("tog", Int) = 0
		_ScrubPos1("tog", Int) = 1
		_Flipbook1Scroll("vec", Vector) = (0,0,0,0)
		[ToggleUI]_Flipbook1ClipEdge("tog", Int) = 0

		//----------------------------
		// OUTLINE
		//----------------------------
		[ToggleUI]_OutlineToggle("tog", Int) = 0
		[ToggleUI]_StencilToggle("tog", Int) = 0
		[ToggleUI]_ApplyOutlineLighting("tog", Int) = 0
		[ToggleUI]_ApplyOutlineEmiss("tog", Int) = 0
		[ToggleUI]_ApplyAlbedoTint("tog", Int) = 0
		[ToggleUI]_IgnoreFilterMask("tog", Int) = 0
		[ToggleUI]_UseVertexColor("tog", Int) = 0
		[ToggleUI]_UseOutlineTexAlpha("tog", Int) = 0
		_OutlineTex("tex", 2D) = "white" {}
		[HDR]_OutlineCol("col", Color) = (0.75,0.75,0.75,1)
		_OutlineScroll("vec", Vector) = (0,0,0,0)
		_OutlineThicc("x", Float) = 0.1
		_OutlineMult("ra", Range(0,1)) = 1
		_OutlineRange("fl", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_OutlineStencilPass("enx", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_OutlineStencilCompare("enx", Float) = 8

		//----------------------------
		// UV DISTORTION
		//----------------------------
		[Enum(Off,0, Normal Map,1, Simplex,2)]_DistortionStyle("en3", Int) = 0
		[IntRange]_NoiseOctaves("ra", Range(1,3)) = 1
		_NoiseScale("vec", Vector) = (1,1,0,0)
		_NoiseSpeed("fl", Float) = 0.5
		_DistortUVMap("tex", 2D) = "bump" {}
		_DistortUVStr("fl", Float) = 1
		_DistortUVScroll("vec", Vector) = (0,0,0,0)
		[ToggleUI]_DistortMainUV("tog", Int) = 0
		[ToggleUI]_DistortDetailUV("tog", Int) = 0
		[ToggleUI]_DistortEmissUV("tog", Int) = 0
		[ToggleUI]_DistortRimUV("tog", Int) = 0
		[ToggleUI]_DistortMatcap0("tog", Int) = 0
		[ToggleUI]_DistortMatcap1("tog", Int) = 0

		//----------------------------
		// VERTEX MANIPULATION
		//----------------------------
		[ToggleUI]_VertexManipulationToggle("tog", Int) = 0
		_VertexExpansion("vec", Vector) = (0,0,0,0)
		[ToggleUI]_VertexExpansionClamp("tog", Int) = 0
		_VertexRounding("ra", Range(0,0.99)) = 0
		_VertexRoundingPrecision("fl", Float) = 100
		_VertexExpansionMask("tex", 2D) = "white" {}
		_VertexRoundingMask("tex", 2D) = "white" {}
		_VertexRotation("vec", Vector) = (0,0,0,0)
		_VertexPosition("vec", Vector) = (0,0,0,0)

		//----------------------------
		// AUDIO LINK
		//----------------------------
		[ToggleUI]_AudioLinkToggle("tog", Int) = 0
		_AudioLinkStrength("ra", Range(0,1)) = 1
		_AudioLinkRemapMin("fl", Float) = 0
		_AudioLinkRemapMax("fl", Float) = 1

		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkEmissionBand("en03", Int) = 0
		_AudioLinkEmissionMultiplier("ra", Range(0,1)) = 0
		_AudioLinkRemapEmissionMin("fl", Float) = 0
		_AudioLinkRemapEmissionMax("fl", Float) = 1

		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkRimBand("en03", Int) = 0
		_AudioLinkRimMultiplier("ra", Range(0,1)) = 0
		_AudioLinkRimWidth("ra", Range(0,1)) = 0
		_AudioLinkRimPulse("ra", Range(0,1)) = 0
		_AudioLinkRimPulseWidth("ra", Range(0,1)) = 0.5
		_AudioLinkRimPulseSharp("ra", Range(0, 0.5)) = 0.3
		_AudioLinkRemapRimMin("fl", Float) = 0
		_AudioLinkRemapRimMax("fl", Float) = 1
		_AudioLinkRemapRimPulseMin("fl", Float) = 0
		_AudioLinkRemapRimPulseMax("fl", Float) = 1

		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkDissolveBand("en03", Int) = 0
		_AudioLinkDissolveMultiplier("ra", Range(0,1)) = 0
		_AudioLinkRemapDissolveMin("fl", Float) = 0
		_AudioLinkRemapDissolveMax("fl", Float) = 1

		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkBCDissolveBand("en03", Int) = 0
		_AudioLinkBCDissolveMultiplier("ra", Range(0,1)) = 0
		_AudioLinkRemapBCDissolveMin("fl", Float) = 0
		_AudioLinkRemapBCDissolveMax("fl", Float) = 1

		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkVertManipBand("en03", Int) = 0
		_AudioLinkVertManipMultiplier("ra", Range(0,1)) = 0
		_AudioLinkRemapVertManipMin("fl", Float) = 0
		_AudioLinkRemapVertManipMax("fl", Float) = 1

		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkUVDistortionBand("en03", Int) = 0
		_AudioLinkUVDistortionMultiplier("ra", Range(0,1)) = 0
		_AudioLinkRemapUVDistortionMin("fl", Float) = 0
		_AudioLinkRemapUVDistortionMax("fl", Float) = 1

		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkTriOffsetBand("en03", Int) = 0
		[Enum(X,0, Y,1, Z,2)]_AudioLinkTriOffsetCoords("en02", Int) = 1
		[Enum(Pulse,0, Levels,1)]_AudioLinkTriOffsetMode("en01", Int) = 0
		_AudioLinkTriOffsetStartPos("fl", Float) = -0.5
		_AudioLinkTriOffsetEndPos("fl", Float) = 0.5
		_AudioLinkTriOffsetFalloff("fl",Float) = 0.05
		_AudioLinkTriOffsetSize("fl", Float) = 0.1
		_AudioLinkTriOffsetMask("tex", 2D) = "white" {}
		_AudioLinkTriOffsetStrength("ra", Range(0,1)) = 0
		_AudioLinkTriOffsetMaskScroll("vec", Vector) = (0,0,0,0)
		_AudioLinkRemapTriOffsetMin("fl", Float) = 0
		_AudioLinkRemapTriOffsetMax("fl", Float) = 1

		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkWireframeBand("en03", Int) = 0
		[Enum(X,0, Y,1, Z,2)]_AudioLinkWireframeCoords("en02", Int) = 1
		[Enum(Pulse,0, Levels,1)]_AudioLinkWireframeMode("en01", Int) = 0
		[HDR]_AudioLinkWireframeColor("col", Color) = (1,1,1,1)
		_AudioLinkWireframeStartPos("fl", Float) = -0.5
		_AudioLinkWireframeEndPos("fl", Float) = 0.5
		_AudioLinkWireframeFalloff("fl",Float) = 0.05
		_AudioLinkWireframeSize("fl", Float) = 0.1
		_AudioLinkWireframeMask("tex", 2D) = "white" {}
		_AudioLinkWireframeStrength("ra", Range(0,1)) = 0
		_AudioLinkWireframeMaskScroll("vec", Vector) = (0,0,0,0)
		_AudioLinkRemapWireframeMin("fl", Float) = 0
		_AudioLinkRemapWireframeMax("fl", Float) = 1

		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkOutlineBand("en03", Int) = 0
		_AudioLinkOutlineMultiplier("fl", Float) = 0
		_AudioLinkRemapOutlineMin("fl", Float) = 0
		_AudioLinkRemapOutlineMax("fl", Float) = 1
		
		_OscilloscopeStrength("ra", Range(0,1)) = 0
		[HDR]_OscilloscopeCol("col", Color) = (1,1,1,1)
		_OscilloscopeScale("vec", Vector) = (1,1,0,0)
		_OscilloscopeOffset("vec", Vector) = (0,0,0,0)
		_OscilloscopeRot("ra", Range(0,360)) = 0
		_OscilloscopeMarginLR("vec", Vector) = (0,1,0,0)
		_OscilloscopeMarginTB("vec", Vector) = (1,0,0,0)

		//----------------------------
		// SPECIAL FEATURES
		//----------------------------
		// DISTANCE FADE
		[Enum(Off,0, Clip,1, Noise,2)]_DistanceFadeToggle("en3", Int) = 0
		[ToggleUI]_DFClones("tog", Int) = 0
		_DistanceFadeMin("ra", Range(0,20)) = 2
		_DistanceFadeMax("ra", Range(0,20)) = 4
		[HDR]_ClipRimColor("col", Color) = (1,1,1,1)
		_ClipRimStr("ra", Range(1,4)) = 1
		_ClipRimWidth("fl", Float) = 1

		// DISSOLVE
		[Enum(Off,0, Texture,1, Simplex,2, Geometry,3)]_DissolveStyle("en4", Int) = 0
		[Enum(X,0, Y,1, Z,2, Point to Point,3)]_GeomDissolveAxis("en4", Int) = 1
		[ToggleUI]_GeomDissolveAxisFlip("tog", Int) = 0
		[ToggleUI]_GeomDissolveWireframe("tog", Int) = 0
		[ToggleUI]_DissolveClones("tog", Int) = 0
		[ToggleUI]_GeomDissolveClamp("tog", Int) = 0
		[IntRange]_GeomDissolveFilter("ra", Range(1,50)) = 1
		_GeomDissolveAmount("fl", Float) = -2
		_GeomDissolveWidth("fl", Float) = 0.25
		_GeomDissolveClip("fl", Float) = 0
		_GeomDissolveSpread("vec", Vector) = (0.1,0.1,0.1,1)
		_DissolvePoint0("vec", Vector) = (0,0,0,0)
		_DissolvePoint1("vec", Vector) = (1,1,1,1)
		_DissolveNoiseScale("vec", Vector) = (3,3,3,0)
		_DissolveAmount("ra", Range(0,1)) = 0
		[ToggleUI]_DissolveBlending("tog", Int) = 0
		_DissolveBlendSpeed("ra", Range(0,1)) = 0.2
		_DissolveMask("tex", 2D) = "white" {}
		_DissolveTex("Dissolve Map", 2D) = "white" {}
		_DissolveScroll0("vec", Vector) = (0,0,0,0)
		[HDR]_DissolveRimCol("col", Color) = (1,1,1,1)
		_DissolveScroll1("vec", Vector) = (0,0,0,0)
		_DissolveRimWidth("fl", Float) = 0.5
		[HideInInspector]_DissolveFlow("Dissolve Flowmap", 2D) = "black" {}

		// SCREENSPACE
		[ToggleUI]_Screenspace("tog", Int) = 0
		_Range("ra", Range(0,50)) = 10
		_Position("vec", Vector) = (0,0,0.25,0)
		_Rotation("vec", Vector) = (0,0,0,0)
		
		// CLONES
		[ToggleUI]_CloneToggle("tog", Int) = 0
		[Enum(Diamond,0, Pyramid,1, Stack,2)]_ClonePattern("en3", Int) = 0
		_Visibility("ra", Range(0,1)) = 0
		_EntryPos("vec", Vector) = (0,1,0,0)
		[ToggleUI]_SaturateEP("tog", Int) = 1
		_Clone1("vec", Vector) = (1,0,0,1)
		_Clone2("vec", Vector) = (-1,0,0,1)
		_Clone3("vec", Vector) = (0,0, 1,1)
		_Clone4("vec", Vector) = (0,0,-1,1)
		_Clone5("vec", Vector) = (0.5,0,0.5,1)
		_Clone6("vec", Vector) = (-0.5,0,0.5,1)
		_Clone7("vec", Vector) = (0.5,0,-0.5,1)
		_Clone8("vec", Vector) = (-0.5,0,-0.5,1)

		// GLITCH
		[ToggleUI]_GlitchToggle("tog", Int) = 0
		[ToggleUI]_GlitchClones("tog", Int) = 0
		_Instability("ra", Range(0,0.01)) = 0
		_GlitchIntensity("ra", Range(0,0.1)) = 0
		_GlitchFrequency("ra", Range(0,0.01)) = 0

		// SHATTER CULLING
		[ToggleUI]_ShatterToggle("tog", Int) = 0
		[ToggleUI]_ShatterClones("tog", Int) = 0
		_ShatterSpread("fl", Float) = 0.347
		_ShatterMin("fl", Float) = 0.25
		_ShatterMax("fl", Float) = 0.65
		_ShatterCull("fl", Float) = 0.535

		// WIREFRAME
		[ToggleUI]_WireframeToggle("tog", Int) = 0
		[ToggleUI]_WFClones("tog", Int) = 0
		[Enum(Triangle,0, Quad,1)]_WFMode("en3", Int) = 0
		[HDR]_WFColor("col", Color) = (0,0,0,1)
		_WFVisibility("ra", Range(0,1)) = 1
		_WFFill("ra", Range(0,1)) = 0
		[ToggleUI]_WireframeTransparency("tog", Int) = 0 
		_PatternMult("fl", Float) = 2.5

		// TOUCH ANYTHING BELOW HERE AND YOUR SHADER WILL BREAK
		[HideInInspector]_UseMetallicMap("tog", Int) = 0
		[HideInInspector]_UseSpecMap("tog", Int) = 0
		[HideInInspector]_UseSmoothMap("tog", Int) = 0
		[HideInInspector]_PackedRoughPreview("tog", Int) = 0
		[HideInInspector]_IsCubeBlendMask("tog", Int) = 0
		[HideInInspector]_DistortUVs("tog", Int) = 0
		[HideInInspector]_NaNLmao("fl", Float) = 0.0
		[HideInInspector]_OutlineCulling("tog", Int) = 1
		[HideInInspector]_UseMatcap1("tog", Int) = 0

		[IntRange]_StencilRef("ra", Range(1,255)) = 1
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPass("enx", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFail("emx", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilZFail("enx", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilCompare("enx", Float) = 8

		[Enum(Unlit,0, Toon,1, Particle,2, Matcap,3, Standard,4, DoubleSided,5, Hidden,6)]_VRCFallback("en06", Int) = 1

    }

    SubShader {
        Tags {
			"RenderType"="Opaque" 
			"Queue"="Geometry"
			"DisableBatching"="True"
			"VRCFallback"="Toon"
		}
		GrabPass {
			Tags {"LightMode"="Always"}
			"_MUSGrab"
		}
		ZTest [_ZTest]
		Cull [_CullingMode]
		AlphaToMask [_ATM]
        Pass {
            Name "ForwardBase"
            Tags {"LightMode"="ForwardBase"}
			Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
			Stencil {
                Ref [_StencilRef]
                Comp [_StencilCompare]
                Pass [_StencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma shader_feature_local _SHADING_ON
			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_local _ _CUBEMAP_ON _COMBINED_CUBEMAP_ON
			#pragma shader_feature_local _ _PACKED_WORKFLOW_ON _SPECULAR_WORKFLOW_ON
			#pragma shader_feature_local _ _SPECULAR_ANISO_ON _SPECULAR_COMBINED_ON
			#pragma shader_feature_local _CUBEMAP_REFLECTIONS_ON
			#pragma shader_feature_local _REFLECTIONS_ON
			#pragma shader_feature_local _SPECULAR_ON
			#pragma shader_feature_local _PACKED_MASKING_ON
			#pragma shader_feature_local _SEPARATE_MASKING_ON
			#pragma shader_feature_local _FILTERING_ON
			#pragma shader_feature_local _POST_FILTERING_ON
			#pragma shader_feature_local _EMISSION_ON
			#pragma shader_feature_local _PARALLAXMAP_ON
			#pragma shader_feature_local _NORMALMAP_ON
			#pragma shader_feature_local _DETAIL_NORMALMAP_ON
			#pragma shader_feature_local _MATCAP_ON
			#pragma shader_feature_local _PBR_PREVIEW_ON
			#pragma shader_feature_local _UV_DISTORTION_ON
			#pragma shader_feature_local _UV_DISTORTION_NORMALMAP_ON
			#pragma shader_feature_local _SCREENSPACE_REFLECTIONS_ON
			#pragma shader_feature_local _PULSE_ON
			#pragma shader_feature_local _ENVIRONMENT_RIM_ON
			#pragma shader_feature_local _FLIPBOOK_ON
			#pragma shader_feature_local _REFRACTION_ON
			#pragma shader_feature_local _CHROMATIC_ABBERATION_ON
			#pragma shader_feature_local _VERTEX_MANIP_ON
			#pragma shader_feature_local _MASK_TRANSFORMS_ON
			#pragma shader_feature_local _REFLECTION_FALLBACK_ON
			#pragma shader_feature_local _BASECOLOR_DISSOLVE_ON
			#pragma shader_feature_local _AUDIOLINK_ON
			#pragma shader_feature_local _RIM_ON
			#pragma shader_feature_local _SUBSURFACE_ON

			#pragma multi_compile _ VERTEXLIGHT_ON
			#pragma multi_compile_fog
			#pragma multi_compile_fwdbase
			// #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#pragma skip_variants DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE 
			#pragma skip_variants DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING
			#ifndef UNITY_PASS_FORWARDBASE
				#define UNITY_PASS_FORWARDBASE
			#endif
			#define OUTLINE_VARIANT
			#pragma target 5.0
			#pragma warning (disable : 3033)
            #include "USDefines.cginc"
            ENDCG
        }

        Pass {
            Name "ForwardAdd"
            Tags {"LightMode"="ForwardAdd"}
			Blend [_SrcBlend] One
			Fog {Color (0,0,0,0)}
            ZWrite Off
			Stencil {
                Ref [_StencilRef]
                Comp [_StencilCompare]
                Pass [_StencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma shader_feature_local _SHADING_ON
			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_local _ _CUBEMAP_ON _COMBINED_CUBEMAP_ON
			#pragma shader_feature_local _ _PACKED_WORKFLOW_ON _SPECULAR_WORKFLOW_ON
			#pragma shader_feature_local _ _SPECULAR_ANISO_ON _SPECULAR_COMBINED_ON
			#pragma shader_feature_local _REFLECTIONS_ON
			#pragma shader_feature_local _SPECULAR_ON
			#pragma shader_feature_local _PACKED_MASKING_ON
			#pragma shader_feature_local _SEPARATE_MASKING_ON
			#pragma shader_feature_local _FILTERING_ON
			#pragma shader_feature_local _POST_FILTERING_ON
			#pragma shader_feature_local _PARALLAXMAP_ON
			#pragma shader_feature_local _NORMALMAP_ON
			#pragma shader_feature_local _DETAIL_NORMALMAP_ON
			#pragma shader_feature_local _UV_DISTORTION_ON
			#pragma shader_feature_local _UV_DISTORTION_NORMALMAP_ON
			#pragma shader_feature_local _FLIPBOOK_ON
			#pragma shader_feature_local _REFRACTION_ON
			#pragma shader_feature_local _CHROMATIC_ABBERATION_ON
			#pragma shader_feature_local _VERTEX_MANIP_ON
			#pragma shader_feature_local _MASK_TRANSFORMS_ON
			#pragma shader_feature_local _BASECOLOR_DISSOLVE_ON
			#pragma shader_feature_local _AUDIOLINK_ON
			#pragma shader_feature_local _SUBSURFACE_ON

			#pragma multi_compile_fog
			#pragma multi_compile_fwdadd_fullshadows
			// #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#pragma skip_variants DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE 
			#pragma skip_variants DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING 
			#ifndef UNITY_PASS_FORWARDADD
				#define UNITY_PASS_FORWARDADD
			#endif
			#define OUTLINE_VARIANT
			#pragma target 5.0
			#pragma warning (disable : 3033)
            #include "USDefines.cginc"
            ENDCG
        }

        Pass {
            Name "Outline"
            Tags {"LightMode"="ForwardBase"}
			Cull [_OutlineCulling]
			Stencil {
                Ref [_StencilRef]
                Comp [_OutlineStencilCompare]
                Pass [_OutlineStencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma shader_feature_local _SHADING_ON
			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_local _ _CUBEMAP_ON _COMBINED_CUBEMAP_ON
			#pragma shader_feature_local _ _PACKED_WORKFLOW_ON _SPECULAR_WORKFLOW_ON
			#pragma shader_feature_local _ _SPECULAR_ANISO_ON _SPECULAR_COMBINED_ON
			#pragma shader_feature_local _CUBEMAP_REFLECTIONS_ON
			#pragma shader_feature_local _PACKED_MASKING_ON
			#pragma shader_feature_local _SEPARATE_MASKING_ON
			#pragma shader_feature_local _FILTERING_ON
			#pragma shader_feature_local _POST_FILTERING_ON
			#pragma shader_feature_local _EMISSION_ON
			#pragma shader_feature_local _PBR_PREVIEW_ON
			#pragma shader_feature_local _PULSE_ON
			#pragma shader_feature_local _FLIPBOOK_ON
			#pragma shader_feature_local _VERTEX_MANIP_ON
			#pragma shader_feature_local _MASK_TRANSFORMS_ON
			#pragma shader_feature_local _BASECOLOR_DISSOLVE_ON
			#pragma shader_feature_local _AUDIOLINK_ON

			#pragma multi_compile _ VERTEXLIGHT_ON
			#pragma multi_compile_fog
			#pragma multi_compile_fwdbase
			// #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#pragma skip_variants DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE 
			#pragma skip_variants DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING 
			#define OUTLINE
			#ifndef UNITY_PASS_FORWARDBASE
				#define UNITY_PASS_FORWARDBASE
			#endif
			#define OUTLINE_VARIANT
			#pragma target 5.0
			#pragma warning (disable : 3033)
            #include "USDefines.cginc"
            ENDCG
        }

        Pass {
            Name "ShadowCaster"
            Tags {"LightMode"="ShadowCaster"}
			ZWrite On ZTest LEqual
			AlphaToMask Off
			Stencil {
                Ref [_StencilRef]
                Comp [_StencilCompare]
                Pass [_StencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma shader_feature_local _SHADING_ON
			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_local _ _PACKED_WORKFLOW_ON _SPECULAR_WORKFLOW_ON
			#pragma shader_feature_local _PACKED_MASKING_ON
			#pragma shader_feature_local _SEPARATE_MASKING_ON
			#pragma shader_feature_local _PBR_PREVIEW_ON
			#pragma shader_feature_local _REFRACTION_ON
			#pragma shader_feature_local _FLIPBOOK_ON
			#pragma shader_feature_local _VERTEX_MANIP_ON
			#pragma shader_feature_local _MASK_TRANSFORMS_ON
			#pragma shader_feature_local _BASECOLOR_DISSOLVE_ON
			#pragma shader_feature_local _AUDIOLINK_ON

			#pragma multi_compile_shadowcaster
			// #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#pragma skip_variants DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE 
			#pragma skip_variants DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING 
			#ifndef UNITY_PASS_SHADOWCASTER
				#define UNITY_PASS_SHADOWCASTER
			#endif
			#define OUTLINE_VARIANT
			#pragma target 5.0
			#pragma warning (disable : 3033)
            #include "USDefines.cginc"
            ENDCG
        }
    }
    CustomEditor "USEditor"
}