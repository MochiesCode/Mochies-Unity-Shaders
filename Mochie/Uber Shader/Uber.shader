// BY MOCHIE
// Version 1.7.1

Shader "Mochie/Uber Shader" {
    Properties {
		
		//----------------------------
		// BASE
		//----------------------------
		[Enum(Off,0, On,1)]_RenderMode("", Int) = 1
		[HideInInspector]_BlendMode("__mode", Float) = 0.0
		[HideInInspector]_SrcBlend("__src", Float) = 1.0
		[HideInInspector]_DstBlend ("__dst", Float) = 0.0
		[Enum(Off,0, On,1)]_ATM("", Int) = 0
		[Enum(Off,0, On,1)]_ZWrite("", Int) = 1
		[Enum(Off,0, On,2)]_CullingMode("", Int) = 2
		[Enum(2D,0, Cubemap,1, Combined,2)]_CubeMode("", Int) = 0
		[Toggle(_)]_UnlitCube("tog", Int) = 0
		[Toggle(_)]_UseAlphaMask("", Int) = 0
		_Cutoff("", Range(0,1)) = 0.5

		_MainTex("tex", 2D) = "white" {} // MainTex
		_Color("col", Color) = (1,1,1,1)
		[Toggle(_)]_ColorPreservation("", Int) = 1
		_AlphaMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_AlphaMaskChannel("", Int) = 0
		_MainTexScroll("", Vector) = (0,0,0,0)

		_CubeBlendMask("tex", 2D) = "white" {} // MainTex (Cubemap)
		_CubeBlend("ra", Range(0,1)) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_CubeBlendMaskChannel("", Int) = 0
		_MainTexCube0("tex", CUBE) = "white" {}
		_CubeColor0("col", Color) = (1,1,1,1)
		[Enum(Lerp,0, Add,1, Sub,2, Mult,3)]_CubeBlendMode("en04", Int) = 0
		_CubeRotate0("vec", Vector) = (180,0,0,0)
		[Toggle(_)]_AutoRotate0("tog", Int) = 0



		//----------------------------
		// TEXTURE MAPPING
		//----------------------------
		//PRIMARY MAPS
		[Enum(Metallic,0, Specular (RGB),1, Specular (RGBA),2, Packed,3)]_PBRWorkflow("", Int) = 0
		_MetallicGlossMap("tex", 2D) = "white" {}
		_Metallic("ra", Range(0,1)) = 0
		_SpecGlossMap("tex", 2D) = "white" {}
		_Glossiness("ra", Range(0,1)) = 0.5
		_SmoothnessMap("", 2D) = "white" {}
		_GlossMapScale("ra", Range(0,1)) = 1
		_OcclusionMap("tex", 2D) = "white" {}
		_OcclusionStrength("", Range(0,1)) = 1
		_BumpMap("tex", 2D) = "bump" {}
		_BumpScale("", Range(-2,2)) = 1
		_ParallaxMap("tex", 2D) = "white" {}
		_Parallax("", Range(0,0.1)) = 0.01
		_DetailMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DetailMaskChannel("", Int) = 0
		_PackedMap("tex", 2D) = "white" {} // Packed workflow
		[Enum(Red,0, Green,1, Blue,2)]_MetallicChannel("", Int) = 0 
		[Enum(Red,0, Green,1, Blue,2)]_RoughnessChannel("", Int) = 1
		[Enum(Red,0, Green,1, Blue,2)]_OcclusionChannel("", Int) = 2

		// DETAIL MAPS
		_DetailAlbedoMap("tex", 2D) = "gray" {}
		_DetailNormalMap("tex", 2D) = "bump" {}
		_DetailNormalMapScale("", Range(-2,2)) = 1
		_DetailScroll("", Vector) = (0,0,0,0)

		// PBR FILTERING
		[Toggle(_)]_RoughnessFiltering("", Int) = 0 // Roughness
		[Toggle(_)]_PreviewRough("", Int) = 0
		_RoughLightness("", Range(-1,1)) = 0
		_RoughIntensity("", Range(0,1)) = 0
		_RoughContrast("", Range(-1,2)) = 1
		[Toggle(_)]_SmoothnessFiltering("", Int) = 0 // Smoothness
		[Toggle(_)]_PreviewSmooth("", Int) = 0
		[Toggle(_)]_LinearSmooth("", Int) = 0
		_SmoothLightness("", Range(-1,1)) = 0
		_SmoothIntensity("", Range(0,1)) = 0
		_SmoothContrast("", Range(-1,2)) = 1
		[Toggle(_)]_AOFiltering("", Int) = 0 // AO
		[Toggle(_)]_PreviewAO("", Int) = 0
		[Toggle(_)]_DirectAO("", Int) = 1
		[Toggle(_)]_IndirectAO("", Int) = 0
		_AOTintTex("tex", 2D) = "white" {}
		_AOTint("col", Color) = (0,0,0,1)
		_AOLightness("", Range(-1,1)) = 0
		_AOIntensity("", Range(0,1)) = 0
		_AOContrast("", Range(-1,2)) = 1
		[Toggle(_)]_HeightFiltering("", Int) = 0 // Height
		[Toggle(_)]_PreviewHeight("", Int) = 0
		_HeightLightness("", Range(-1,1)) = 0
		_HeightIntensity("", Range(0,1)) = 0
		_HeightContrast("", Range(-1,2)) = 1


		
		//----------------------------
		// SHADING
		//----------------------------
		// LIGHTING
		[Toggle(_)]_StaticLightDirToggle("", Int) = 0
		_StaticLightDir("vec", Vector) = (0,0.75,1,0)
		_DisneyDiffuse("ra", Range(0,1)) = 0 // Diffuse Shading
		_SHStr("ra", Range(0,1)) = 0.1
		[Toggle(_)]_NonlinearSHToggle("tog", Int) = 1
		_RTDirectCont("", Range(0,1)) = 1 // Realtime Lighting
		_RTIndirectCont("", Range(0,1)) = 1
		_VLightCont("", Range(0,1)) = 1
		[Toggle(_)]_ClampAdditive("", Int) = 1
		_AdditiveMax("", Range(0,2)) = 1
		_DirectCont("", Range(0,1)) = 0.6 // Baked Lighting
		_IndirectCont("", Range(0,1)) = 0.5

		// MASKING
		[Enum(Off,0, Separate,1, Packed (RGB),2, Packed (RGBA),3)]_MaskingMode("", Int) = 0
		_ReflectionMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_ReflectionMaskChannel("", Int) = 0	
		_SpecularMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_SpecularMaskChannel("", Int) = 0
		_MatcapMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_MatcapMaskChannel("", Int) = 0
		_ShadowMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_ShadowMaskChannel("", Int) = 0
		_RimMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_RimMaskChannel("", Int) = 0
		_ERimMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_ERimMaskChannel("", Int) = 0
		_DDMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DDMaskChannel("", Int) = 0
		_SmoothShadeMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_SmoothShadeMaskChannel("", Int) = 0
		_PackedMask0("tex", 2D) = "white" {}
		_PackedMask1("tex", 2D) = "white" {}

		// REFLECTIONS
		[Toggle(_)]_Reflections("", Int) = 0
		_ReflCube("tex", CUBE) = "white" {}
		[Toggle(_)]_ReflCubeFallback("", Int) = 0
		_ReflTex("tex", 2D) = "white" {}
		_ReflCol("col", Color) = (1,1,1,1)
		_ReflectionStr("fl", Float) = 1
		[Toggle(_)]_ReflUseRough("", Int) = 1
		_ReflRough("ra", Range(0,2)) = 0.5
		[Toggle(_)]_SSR("", Int) = 0 // SSR
		[Toggle(_)]_Dith("", Int) = 0
		_Alpha("", Range(0.0, 1.0)) = 1
		[IntRange]_MaxSteps ("", Range(1,50)) = 50
		_Step("", Float) = 0.09 
		_LRad("", Float) = 0.2
		_SRad("", Float) = 0.02
		_EdgeFade("", Range(0,1)) = 0.1
		[HideInInspector]_NoiseTexSSR("SSRNoise", 2D) = "black" {}

		// SPECULAR
		[Toggle(_)]_Specular("", Int) = 0
		[Enum(GGX,0, Anisotropic,1, Combined,2)]_SpecularStyle("", Int) = 0
		_SpecTex("tex", 2D) = "white" {}
		_SpecCol("col", Color) = (1,1,1,1)
		_SpecStr("fl", Float) = 1
		[Toggle(_)]_SharpSpecular("tog", Int) = 0
		[IntRange]_SharpSpecStr("ra", Range(1,10)) = 1
		_AnisoAngleX("ra", Range(0,1)) = 1
        _AnisoAngleY("ra", Range(0,1)) = 0.05
		_AnisoLayerX("fl", Float) = 2
		_AnisoLayerY("fl", Float) = 10
		_AnisoLayerStr("ra", Range(0,1)) = 0.1
		[Toggle(_)]_AnisoLerp("tog", Int) = 0
		_InterpMask("tex", 2D) = "gray" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_InterpMaskChannel("", Int) = 0
		[Toggle(_)]_SpecUseRough("", Int) = 1
		_SpecRough("ra", Range(0,2)) = 0.5

		// MATCAP
		[Toggle(_)]_MatcapToggle("", Int) = 0
		_Matcap("tex", 2D) = "black" {}
		_MatcapColor("col", Color) = (1,1,1,1)
		[Enum(Add,0, Mult,1, Alpha,2)]_MatcapBlending("en02", Int) = 0
		_MatcapStr("fl", Float) = 1
		[Toggle(_)]_UnlitMatcap("tog", Int) = 0
		[Toggle(_)]_MatcapUseRough("", Int) = 1
		_MatcapRough("ra", Range(0,2)) = 0.5

		// SHADOWS
		[Toggle(_)]_Shadows("", Int) = 1
		[Enum(Manual Blend,0, Ramp,1)]_ShadowMode("", Int) = 0
		[Enum(Always,0, Realtime Lighting Only,1, Baked Lighting Only,2)]_ShadowConditions("", Int) = 0
		[Toggle(_)]_MainTexTint("", Int) = 0
		_ShadowTint("col", Color) = (0,0,0,1)
		_RampPos("", Range(0,1)) = 0
		_RampWidth0("ra", Range(0.005,1)) = 0.005
		_RampWidth1("ra", Range(0.005,1)) = 0.5
		_RampWeight("ra", Range(0,1)) = 0
		_ShadowRamp("ShadowRamp", 2D) = "white" {}
		_ShadowStr("ra", Range(0,1)) = 1
		[Toggle(_)]_ShadowDithering("", Int) = 0
		_ShadowDitherStr("ra", Range(0,1)) = 0.3
		[Toggle(_)]_RTSelfShadow("", Int) = 1
		[Toggle(_)]_AttenSmoothing("", Int) = 1
		
		// SUBSURFACE SCATTERING
		[Toggle(_)]_Subsurface("", Int) = 0
		_TranslucencyMap("tex", 2D) = "black" {}
		_SubsurfaceMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_SubsurfaceMaskChannel("", Int) = 0
		_SubsurfaceTex("tex", 2D) = "white" {}
		_SColor("col", Color) = (1,1,1,1)
		_SStr("ra", Range(0,1)) = 1
		_SPen("ra", Range(0,1)) = 0.5
		_SSharp("ra", Range(0,1)) = 0.5
		_SAtten("ra", Range(0,1)) = 0.8

		// BASIC RIM
		[Toggle(_)]_RimLighting("", Int) = 0
		_RimTex("tex", 2D) = "white" {}
		[HDR]_RimCol("col", Color) = (1,1,1,1)
		[Enum(Lerp,0, Add,1, Sub,2, Mult,3)]_RimBlending("en03", Int) = 0
		_RimScroll("", Vector) = (0,0,0,0)
		_RimStr("ra", Range(0,1)) = 1
		_RimWidth("", Range (0,1)) = 0.5
		_RimEdge("ra", Range(0,0.5)) = 0
		
		// ENVIRONMENT RIM
		[Toggle(_)]_EnvironmentRim("", Int) = 0
		_ERimTex("tex", 2D) = "white" {}
		[HDR]_ERimTint("col", Color) = (1,1,1,1)
		[Enum(Lerp,0, Add,1, Sub,2, Mult,3)]_ERimBlending("en03", Int) = 1
		_ERimScroll("", Vector) = (0,0,0,0)
		_ERimStr("ra", Range(0,1)) = 1
		_ERimWidth("ra", Range (0,1)) = 0.7
		_ERimEdge("ra", Range(0,0.5)) = 0
		_ERimRoughness("ra", Range(0,2)) = 0.5
		[Toggle(_)]_ERimUseRough("tog", Int) = 0

		// NORMALS
		[Toggle(_)]_HardenNormals("", Int) = 0
		[Toggle(_)]_InvertNormalY0("", Int) = 0
		[Toggle(_)]_InvertNormalY1("", Int) = 0
		[Toggle(_)]_ClearCoat("tog", Int) = 0



		//----------------------------
		// EMISSION
		//----------------------------
		[Enum(Off,0, Emission Map,1, Albedo Alpha,2)]_EmissionToggle("", Int) = 0
		_EmissionMap("tex", 2D) = "white" {}
		[HDR]_EmissionColor("col", Color) = (0,0,0,1)
		_EmissScroll("", Vector) = (0,0,0,0)
		_EmissMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_EmissMaskChannel("", Int) = 0

		// LIGHT REACTIVITY
		[Toggle(_)]_ReactToggle("tog", Int) = 0
		[Toggle(_)]_CrossMode("tog", Int) = 0
		_Crossfade("ra", Range(0,0.2)) = 0.1
		_ReactThresh("ra", Range(0,1)) = 0.5

		// PULSE
		[Toggle(_)]_PulseToggle("tog", Int) = 0
		[Enum(Sine,0, Square,1, Triangle,2, Saw,3, Reverse Saw,4)]_PulseWaveform("en04", Int) = 0
		_PulseMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_PulseMaskChannel("", Int) = 0
		_PulseStr("ra", Range(0,1)) = 0.5
		_PulseSpeed("fl", Float) = 1
		


		//----------------------------
		// FILTERS
		//----------------------------
		[Enum(Off,0, RGB,1, HSL,2, HSV,3, Team Colors,4)]_FilterModel("", Int) = 0
		[Toggle(_)]_PostFiltering("", Int) = 0
		_FilterMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_FilterMaskChannel("", Int) = 0

		_RAmt("ra", Float) = 1 // RGB, HSL, HSV
		_GAmt("ra", Float) = 1
		_BAmt("ra", Float) = 1
		[Toggle(_)]_AutoShift("tog", Int) = 0
		_AutoShiftSpeed("ra", Range(0,1)) = 0.25
		_Hue("ra", Range(0,1)) = 0
		_Luminance ("ra", Range(-1,1)) = 0
		_Value("fl", Float) = 0
		_HSLMin("ra", Range(0,1)) = 0
		_HSLMax("ra", Range(0,1)) = 1

		_TeamColorMask("tex", 2D) = "white" {} // Team Colors
		_TeamColor0("col", Color) = (1,1,1,1)
		_TeamColor1("col", Color) = (1,1,1,1)
		_TeamColor2("col", Color) = (1,1,1,1)
		_TeamColor3("col", Color) = (1,1,1,1)

		_Saturation("fl", Float) = 1 // Other settings
		_Brightness("fl", Float) = 0
		_Contrast("ra", Range(0,2)) = 1
		_HDR("ra", Range(0,1)) = 0
		// _Noise("ra", Range(0,1)) = 0
		


		//----------------------------
		// SPRITE SHEETS
		//----------------------------
		[Toggle(_)]_UnlitSpritesheet("tog", Int) = 0
		_SpritesheetMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_SpritesheetMaskChannel("", Int) = 0

		// SHEET 1
		[Toggle(_)]_EnableSpritesheet("", Int) = 0
		_Spritesheet("tex", 2D) = "white" {}
		[HDR]_SpritesheetCol("col", Color) = (1,1,1,1)
		[Enum(Add,0, Mult,1, Alpha,2)]_SpritesheetBlending("en02", Int) = 2
		_RowsColumns("", Vector) = (1,1,0,0)
		_FrameClipOfs("", Vector) = (0,0,0,0)
		_SpritesheetPos("vec", Vector) = (0,0,0,0)
		_SpritesheetScale("vec", Vector) = (1,1,0,0)
		_SpritesheetRot("ra", Range(0,360)) = 0
		_FPS("", Range(1,120)) = 30
		[Toggle(_)]_ManualScrub("", Int) = 0
		_ScrubPos("", Int) = 1

		// SHEET 2
		[Toggle(_)]_EnableSpritesheet1("", Int) = 0
		_Spritesheet1("tex", 2D) = "white" {}
		[HDR]_SpritesheetCol1("col", Color) = (1,1,1,1)
		[Enum(Add,0, Mult,1, Alpha,2)]_SpritesheetBlending1("en02", Int) = 2
		_RowsColumns1("", Vector) = (1,1,0,0)
		_FrameClipOfs1("", Vector) = (0,0,0,0)
		_SpritesheetPos1("vec", Vector) = (0,0,0,0)
		_SpritesheetScale1("vec", Vector) = (1,1,0,0)
		_SpritesheetRot1("ra", Range(0,360)) = 0
		_FPS1("", Range(1,120)) = 30
		[Toggle(_)]_ManualScrub1("", Int) = 0
		_ScrubPos1("", Int) = 1



		//----------------------------
		// UV DISTORTION
		//----------------------------
		[Enum(Normal Map,0, Simplex,1)]_DistortionStyle("", Int) = 0
		[Toggle(_)]_PreviewNoise("", Int) = 0
		[IntRange]_NoiseOctaves("", Range(1,5)) = 1
		_NoiseScale("vec", Vector) = (1,1,0,0)
		_NoiseSpeed("fl", Float) = 0.5
		_NoiseMinMax("", Vector) = (-1,1,0,0)
		_DistortUVMap("tex", 2D) = "bump" {}
		_DistortUVStr("fl", Float) = 1
		_DistortUVScroll("", Vector) = (0,0,0,0)
		_DistortUVMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DistortUVMaskChannel("", Int) = 0
		[Toggle(_)]_DistortMainUV("", Int) = 0
		[Toggle(_)]_DistortDetailUV("", Int) = 0
		[Toggle(_)]_DistortEmissUV("", Int) = 0
		[Toggle(_)]_DistortRimUV("", Int) = 0



		//----------------------------
		// OUTLINE
		//----------------------------
		[Enum(Off,0, Solid Color,1, Tinted,2, Texture,3)]_Outline("", Int) = 0
		[Toggle(_)]_ApplyOutlineLighting("tog", Int) = 0
		[Toggle(_)]_ApplyOutlineEmiss("tog", Int) = 0
		_OutlineTex("tex", 2D) = "white" {}
		[HDR]_OutlineCol("col", Color) = (0.75,0.75,0.75,1)
		_OutlineScroll("", Vector) = (0,0,0,0)
		_OutlineMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_OutlineMaskChannel("", Int) = 0
		_OutlineThicc("x", Float) = 0.1
		_OutlineRange("x", Range(0,1)) = 0



		//----------------------------
		// SPECIAL FEATURES
		//----------------------------
		[Toggle(_)]_GeomFXToggle("tog", Int) = 0
		[Toggle(_)]_DisguiseMain("", Int) = 0

		// DISTANCE FADE
		[Enum(Off,0, Clip,1, Noise,2)]_DistanceFadeToggle("", Int) = 0
		_DistanceFadeMin("", Range(0,20)) = 2
		_DistanceFadeMax("", Range(0,20)) = 4
		[HDR]_ClipRimColor("col", Color) = (1,1,1,1)
		_ClipRimStr("ra", Range(1,4)) = 1
		_ClipRimWidth("fl", Float) = 1

		// DISSOLVE
		[Toggle(_)]_DissolveToggle("", Int) = 0
		_DissolveAmount("ra", Range(0,1)) = 0
		[Toggle(_)]_DissolveBlending("tog", Int) = 0
		_DissolveBlendSpeed("ra", Range(0,1)) = 0.2
		_DissolveMask("tex", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DissolveMaskChannel("", Int) = 0
		_DissolveTex("Dissolve Map", 2D) = "white" {}
		_DissolveScroll0("", Vector) = (0,0,0,0)
		_DissolveRimTex("Dissolve Rim", 2D) = "white" {}
		[HDR]_DissolveRimCol("col", Color) = (1,1,1,1)
		_DissolveScroll1("", Vector) = (0,0,0,0)
		_DissolveRimWidth("fl", Float) = 0.5
		[HideInInspector]_DissolveFlow("Dissolve Flowmap", 2D) = "black" {}

		// SCREENSPACE
		[Toggle(_)]_Screenspace("", Int) = 0
		_Range("ra", Range(0,50)) = 10
		_Position("", Vector) = (0,0,0.25,0)
		_Rotation("", Vector) = (0,0,0,0)

		// MESH MANIPULATION
		[Toggle(_)]_ShowBase("", Int) = 1
		_BaseOffset("", Vector) = (0,0,0,0)
		_BaseRotation("", Vector) = (0,0,0,0)
		[Toggle(_)]_ShowInMirror("", Int) = 1
		[Toggle(_)]_Connected("", Int) = 1
		_ReflOffset("", Vector) = (0,0,0,0)
		_ReflRotation("", Vector) = (0,0,0,0)
		
		// CLONES
		[Enum(Manual,0, Diamond,1, Pyramid,2, Stack,3, Arrow,4, Wall,5)]_ClonePattern("en04", Int) = 0
		_Visibility("", Range(0,1)) = 0
		_EntryPos("vec", Vector) = (0,1,0,0)
		[Toggle(_)]_SaturateEP("", Int) = 1
		_CloneSpacing("fl", Float) = 0
		_Clone1("vec", Vector) = (1,0,0,1)
		_Clone2("vec", Vector) = (-1,0,0,1)
		_Clone3("vec", Vector) = (0,0, 1,1)
		_Clone4("vec", Vector) = (0,0,-1,1)
		_Clone5("vec", Vector) = (0.5,0,0.5,1)
		_Clone6("vec", Vector) = (-0.5,0,0.5,1)
		_Clone7("vec", Vector) = (0.5,0,-0.5,1)
		_Clone8("vec", Vector) = (-0.5,0,-0.5,1)

		// GLITCH
		[Toggle(_)]_GlitchToggle("", Int) = 0
		_Instability("ra", Range(0,0.01)) = 0
		_GlitchIntensity("ra", Range(0,0.1)) = 0
		_GlitchFrequency("ra", Range(0,0.01)) = 0

		// SHATTER CULLING
		[Toggle(_)]_ShatterToggle("", Int) = 0
		_ShatterSpread("fl", Float) = 0.347
		_ShatterMin("fl", Float) = 0.25
		_ShatterMax("fl", Float) = 0.65
		_ShatterCull("fl", Float) = 0.535

		// WIREFRAME
		[Toggle(_)]_WireframeToggle("tog", Int) = 0
		[Enum(Normal,0, Tread,1, Quad,2, Rect,3, Zigzag,4)]_WFMode("en04", Int) = 0
		[HDR]_WFColor("col", Color) = (0,0,0,1)
		_WFVisibility("ra", Range(0,1)) = 1
		_WFFill("ra", Range(0,1)) = 0
		_PatternMult("", Float) = 2.5

		// TOUCH ANYTHING BELOW HERE AND YOUR SHADER WILL BREAK
		[HideInInspector]_UseDetailNormal("", Int) = 0
		[HideInInspector]_UseParallaxMap("", Int) = 0
		[HideInInspector]_UseMetallicMap("", Int) = 0
		[HideInInspector]_UseSpecMap("", Int) = 0
		[HideInInspector]_UseReflCube("", Int) = 0
		[HideInInspector]_UseSpecTex("", Int) = 0
		[HideInInspector]_UseReflTex("", Int) = 0
		[HideInInspector]_UseERimTex("", Int) = 0
		[HideInInspector]_UseSmoothMap("", Int) = 0
		[HideInInspector]_PackedRoughPreview("", Int) = 0
		[HideInInspector]_UseAOTintTex("", Int) = 0
		[HideInInspector]_PreviewActive("", Int) = 0
		[HideInInspector]_IsCubeBlendMask("", Int) = 0
		[HideInInspector]_UseRimTex("", Int) = 0
		[HideInInspector]_DistortUVs("", Int) = 0
		[HideInInspector]_NaNxddddd("", Float) = 0.0

		[Enum(Zero,0, One,1, Two,2, Three,3)]_DebugEnum("", Int) = 0
		[HDR]_DebugHDRColor("", Color) = (1,1,1,1)
		_DebugColor("", Color) = (1,1,1,1)
		_DebugVector("", Vector) = (0,0,0,0)
		_DebugFloat("", Float) = 1
		_DebugRange("", Range(0,10)) = 1
		[IntRange]_DebugIntRange("", Range(0,10)) = 1
		[Toggle(_)]_DebugToggle("", Int) = 0

    }

    SubShader {
        Tags {
			"RenderType"="Opaque" 
			"Queue"="Geometry"
		}
		GrabPass {
			Tags {"LightMode"="Always"}
			"_SSRGrab"
		}
        Cull [_CullingMode]
		AlphaToMask [_ATM]
        Pass {
            Name "ForwardBase"
            Tags {"LightMode"="ForwardBase"}
			Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma multi_compile _ _FOG_EXP2
            #pragma multi_compile_fwdbase
			#pragma skip_variants DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE 
			#pragma skip_variants DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING 
			#pragma skip_variants UNITY_HDR_ON
            #pragma target 5.0
			#pragma warning (disable : 3033)
			#if !(UNITY_VERSION >= 201840)
				#define UNITY_PASS_FORWARDBASE
			#endif
            #include "USDefines.cginc"
            ENDCG
        }

        Pass {
            Name "ForwardAdd"
            Tags {"LightMode"="ForwardAdd"}
            Blend [_SrcBlend] One
			Fog {Color (0,0,0,0)}
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma multi_compile _ _FOG_EXP2
            #pragma multi_compile_fwdadd_fullshadows
            #pragma target 5.0
			#pragma warning (disable : 3033)
			#if !(UNITY_VERSION >= 201840)
				#define UNITY_PASS_FORWARDADD
			#endif
            #include "USDefines.cginc"
            ENDCG
        }

        Pass {
            Name "ShadowCaster"
            Tags {"RenderType"="Transparent" "Queue"="Transparent" "LightMode"="ShadowCaster"}
			AlphaToMask [_ATM]
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma multi_compile_shadowcaster
			#pragma target 5.0
			#pragma warning (disable : 3033)	
			#if !(UNITY_VERSION >= 201840)
				#define UNITY_PASS_SHADOWCASTER
			#endif
            #include "USDefines.cginc"
            ENDCG
        }

        Pass {
            Name "Outline"
            Tags {"LightMode"="ForwardBase"}
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma multi_compile _ _FOG_EXP2
            #pragma multi_compile_fwdbase
			#pragma skip_variants DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE 
			#pragma skip_variants DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING 
			#pragma skip_variants UNITY_HDR_ON
			#pragma target 5.0
			#pragma warning (disable : 3033)
            #define OUTLINE
			#if !(UNITY_VERSION >= 201840)
				#define UNITY_PASS_FORWARDBASE
			#endif
            #include "USDefines.cginc"
            ENDCG
        }
    }
    CustomEditor "USEditor"
}