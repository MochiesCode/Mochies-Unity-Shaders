Shader "Mochie/Uber Shader (Transparent)" {
    Properties {

		_MainTex("", 2D) = "white" {}
		_MainTexCube0("", CUBE) = "white" {}
		_MainTexCube1("", CUBE) = "white" {}
		_EmissionMap("", 2D) = "white" {}
		_PackedMap("", 2D) = "white" {}
		_MetallicGlossMap("", 2D) = "white" {}
		_SpecGlossMap("", 2D) = "white" {}
		_BumpMap("", 2D) = "bump" {}
		_OcclusionMap("", 2D) = "white" {}
		_ParallaxMap("", 2D) = "white" {}
		_DetailMask("", 2D) = "white" {}
		_DetailAlbedoMap("", 2D) = "gray" {}
		_DetailNormalMap("", 2D) = "bump" {}
		_RimTex("", 2D) = "white" {}
		_OutlineTex("", 2D) = "white" {}
		_ReflCube("", CUBE) = "white" {}
		_ShadowRamp("", 2D) = "white" {}
		_EmissMask("", 2D) = "white" {}
		_RimMask("", 2D) = "white" {}
		_ReflectionMask("", 2D) = "white" {}
		_SpecularMask("", 2D) = "white" {}
		_ShadowMask("", 2D) = "white" {}
		_DissolveMask("", 2D) = "white" {}
		_DissolveTex("", 2D) = "white" {}
		_DissolveRimTex("", 2D) = "white" {}
		_DissolveFlow("", 2D) = "black" {}
		_OutlineMask("", 2D) = "white" {}
		_FilterMask("", 2D) = "white" {}
		_ImposterTex("", 2D) = "white" {}
		_MatCap("", 2D) = "black" {}
		_MatcapMask("", 2D) = "white" {}
		_DDMask("", 2D) = "white" {}
		_SmoothShadeMask("", 2D) = "white" {}
		_TeamColorMask("", 2D) = "white" {}
		_RimNormal("", 2D) = "bump" {}
		_InterpMask("", 2D) = "gray" {}
		_DistortUVMap("", 2D) = "bump" {}
		_DistortUVMask("", 2D) = "white" {}
		_CubeBlendMask("", 2D) = "white" {}
		_PackedMask0("", 2D) = "white" {}
		_PackedMask1("", 2D) = "white" {}
		_PulseMask("", 2D) = "white" {}

		_Color("", Color) = (1,1,1,1)
		[HDR]_EmissionColor("", Color) = (0,0,0,1)
		[HDR]_OutlineCol("", Color) = (0.75,0.75,0.75,1)
		_SpecCol("", Color) = (1,1,1,1)
		_ReflCol("", Color) = (1,1,1,1)
		[HDR]_WFColor("", Color) = (0,0,0,1)
		[HDR]_RimCol("", Color) = (1,1,1,1)
		[HDR]_DissolveRimCol("", Color) = (1,1,1,1)
		_ImposterColor("", Color) = (1,1,1,1)
		[HDR]_ClipRimColor("", Color) = (1,1,1,1)
		_MatcapColor("", Color) = (1,1,1,1)
		_TeamColor0("", Color) = (1,1,1,1)
		_TeamColor1("", Color) = (1,1,1,1)
		_TeamColor2("", Color) = (1,1,1,1)
		_TeamColor3("", Color) = (1,1,1,1)
		_CubeColor0("", Color) = (1,1,1,1)
		_CubeColor1("", Color) = (1,1,1,1)

		_MainTexScroll("", Vector) = (0,0,0,0)
		_EmissScroll("", Vector) = (0,0,0,0)
		_OutlineScroll("", Vector) = (0,0,0,0)
		_DetailScroll("", Vector) = (0,0,0,0)
		_RimScroll("", Vector) = (0,0,0,0)
		_DissolveScroll0("", Vector) = (0,0,0,0)
		_DissolveScroll1("", Vector) = (0,0,0,0)
		_RimNormalScroll("", Vector) = (0,0,0,0)
		_EntryPos("", Vector) = (0,1,0,0)
		_Clone1("", Vector) = (1,0,0,1)
		_Clone2("", Vector) = (-1,0,0,1)
		_Clone3("", Vector) = (0,0, 1,1)
		_Clone4("", Vector) = (0,0,-1,1)
		_Clone5("", Vector) = (0.5,0,0.5,1)
		_Clone6("", Vector) = (-0.5,0,0.5,1)
		_Clone7("", Vector) = (0.5,0,-0.5,1)
		_Clone8("", Vector) = (-0.5,0,-0.5,1)
		_StaticLightDir("", Vector) = (0,0.75,1,0)
		_BaseOffset("", Vector) = (0,0,0,0)
		_BaseRotation("", Vector) = (0,0,0,0)
		_ReflOffset("", Vector) = (0,0,0,0)
		_ReflRotation("", Vector) = (0,0,0,0)
		_Position("", Vector) = (0,0,0.25,0)
		_Rotation("", Vector) = (0,0,0,0)
		_ImposterSize("", Vector) = (1,1,0,0)
		_DistortUVScroll("", Vector) = (0,0,0,0)
		_CubeRotate0("", Vector) = (180,0,0,0)
		_CubeRotate1("", Vector) = (180,0,0,0)

		[Toggle(_)]_PulseToggle("", Int) = 0
		[Toggle(_)]_ColorPreservation("", Int) = 1
		[Toggle(_)]_MaskingToggle("", Int) = 0
		[Toggle(_)]_DissolveToggle("", Int) = 0
		[Toggle(_)]_GeomFXToggle("", Int) = 0
		[Toggle(_)]_WireframeToggle("", Int) = 0
		[Toggle(_)]_ShatterToggle("", Int) = 0
		[Toggle(_)]_GlitchToggle("", Int) = 0
		[Toggle(_)]_DisguiseMain("", Int) = 0
		[Toggle(_)]_AutoRotate0("", Int) = 0
		[Toggle(_)]_AutoRotate1("", Int) = 0
		[Toggle(_)]_UnlitCube("", Int) = 0
		[Toggle(_)]_ClearCoat("", Int) = 0
		[Toggle(_)]_StaticLightDirToggle("", Int) = 0
		[Toggle(_)]_ReflCubeFallback("", Int) = 0
		[Toggle(_)]_SharpSpecular("", Int) = 0
		[Toggle(_)]_Shadows("", Int) = 1
		[Toggle(_)]_EnableShadowRamp("", Int) = 0
		[Toggle(_)]_ReactToggle("", Int) = 0
		[Toggle(_)]_CrossMode("", Int) = 0
		[Toggle(_)]_UnlitRim("", Int) = 1
		[Toggle(_)]_Connected("", Int) = 1
		[Toggle(_)]_ShowInMirror("", Int) = 1
		[Toggle(_)]_ShowBase("", Int) = 1
		[Toggle(_)]_SaturateEP("", Int) = 1
		[Toggle(_)]_DissolveBlending("", Int) = 0
		[Toggle(_)]_UnlitOutline("", Int) = 1
		[Toggle(_)]_RimLighting("", Int) = 0
		[Toggle(_)]_AutoShift("", Int) = 0
		[Toggle(_)]_Screenspace("", Int) = 0
		[Toggle(_)]_MatcapToggle("", Int) = 0
		[Toggle(_)]_InvertNormalY0("", Int) = 0
		[Toggle(_)]_InvertNormalY1("", Int) = 0
		[Toggle(_)]_RoughnessAdjust("", Int) = 0
		[Toggle(_)]_AnisoLerp("", Int) = 0
		[Toggle(_)]_DistortMainUV("", Int) = 0
		[Toggle(_)]_DistortDetailUV("", Int) = 0
		[Toggle(_)]_DistortEmissUV("", Int) = 0
		[Toggle(_)]_DistortRimUV("", Int) = 0
		[Toggle(_)]_RTSelfShadow("", Int) = 0
		[Toggle(_)]_ClampAdditive("", Int) = 0
		[Toggle(_)]_HardenNormals("", Int) = 0
		[Toggle(_EMISSION)]_EmissionToggle("", Int) = 0
		[ToggleOff(_SPECULARHIGHLIGHTS_OFF)]_Specular("", Int) = 0
		[ToggleOff(_GLOSSYREFLECTIONS_OFF)]_Reflections("", Int) = 0

		[Enum(2D,0, CUBE,1, 2D_CUBE,2, DOUBLE_CUBE,3)]_CubeMode("", Int) = 0
		[Enum(BASIC,0, TOON,1, STANDARD,2)]_RenderMode("", Int) = 0
		[Enum(OFF,0, ON,2)]_CullingMode("", Int) = 2
		[Enum(OFF,0, FRONT,1, BACK,2)]_OutlineCulling("", Int) = 1
		[Enum(OFF,0, COLOR,1, TINTED,2, TEXTURE,3)]_Outline("", Int) = 0
		[Enum(OFF,0, ON,1)]_ZWrite("ZWrite", Int) = 0
		[Enum(OFF,0, ON,1)]_ATM("", Int) = 0
		[Enum(Metallic,0, Specular,1, Packed,2)]_PBRWorkflow("", Int) = 0
		[Enum(GGX,0, Anisotropic,1, Combined,2)]_SpecularStyle("", Int) = 0
		[Enum(Specular,0, Albedo,1)]_SourceAlpha("", Int) = 0
		[Enum(OFF,0, RGB,1, HSL,2, TEAM_COLORS,3)]_FilterModel("", Int) = 0
		[Enum(OFF,0, CLIP,1, NOISE,2)]_DistanceFadeToggle("", Int) = 0
		[Enum(Lerp,0, Add,1, Sub,2, Mult,3)]_RimBlending("", Int) = 1
		[Enum(Add,0, Mult,1, Alpha,2)]_MatcapBlending("", Int) = 0
		[Enum(Lerp,0, Add,1, Sub,2, Mult,3)]_CubeBlendMode("", Int) = 0
		[Enum(Normal,0, Tread,1, Quad,2, Rect,3, Zigzag,4)]_WFMode("", Int) = 0
		[Enum(Manual,0, Diamond,1, Pyramid,2, Stack,3, Arrow,4, Wall,5)]_ClonePattern("", Int) = 0
		[Enum(SIN,0, SQUARE,1, TRIANGLE,2, SAW,3, REVERSE_SAW,4)]_PulseWaveform("", Int) = 0
		[Enum(Separate,0, Packed,1)]_MaskingMode("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_EmissMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DetailMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_ReflectionMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_SpecularMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_ShadowMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_RimMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DissolveMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DissolveChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_OutlineMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_FilterMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_MatcapMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DDMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_SmoothShadeMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_MetallicChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_RoughnessChannel("", Int) = 1
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_OcclusionChannel("", Int) = 2
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_HeightChannel("", Int) = 3
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_InterpMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DistortUVMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_CubeBlendMaskChannel("", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_PulseMaskChannel("", Int) = 0
       
		_Metallic("", Range(0,1)) = 0
		_Glossiness("", Range(0,1)) = 0.5
		_GlossMapScale("", Range(0,1)) = 0.5
		_BumpScale("", Range(-2,2)) = 1
		_DetailNormalMapScale("", Range(-2,2)) = 1
		_OcclusionStrength("", Range(0,1)) = 1
		_Parallax("", Range(0,0.1)) = 0.01

		_AdditiveMax("", Range(0,2)) = 1
		_CubeBlend("", Range(0,1)) = 0
		_DistortUVStr("", Float) = 0
		_ProbeContrib("", Range(0,4)) = 2
		_AnisoAngleX("", Range(0,1)) = 1
        _AnisoAngleY("", Range(0,1)) = 0.05
		_AnisoLayerX("", Float) = 2
		_AnisoLayerY("", Float) = 10
		_AnisoLayerStr("", Range(0,1)) = 0.1
		_RoughLightness("", Range(-1,1)) = 0
		_RoughIntensity("", Range(0,1)) = 0
		_RoughContrast("", Range(0,2)) = 1
		_RimNormalStr("", Range(-2,2)) = 1
		_VLightCont("", Range(0,1)) = 1
		_DirectCont("", Range(0,1)) = 0.6
		_IndirectCont("", Range(0,1)) = 0.6
		_RTDirectCont("", Range(0,1)) = 1
		_RTIndirectCont("", Range(0,1)) = 1
		_DisneyDiffuse("", Range(0,1)) = 0.2
		_SmoothShading("", Range(0,1)) = 0
		_MatcapStr("", Range(0,2)) = 1
		_DistanceFadeMin("", Range(0,20)) = 2
		_DistanceFadeMax("", Range(0,20)) = 4
		_ClipRimStr("", Range(1,4)) = 1
		_ClipRimWidth("", Float) = 1
		_Cutoff("", Range(0,1)) = 0.5
		_HDR("", Range(0,1)) = 0
		_Contrast("", Range(0,2)) = 1
		_SaturationRGB("", Range(0,2)) = 1
		_Brightness("", Range(-1,1)) = 0
		_RAmt("", Range(0,2)) = 1
		_GAmt("", Range(0,2)) = 1
		_BAmt("", Range(0,2)) = 1
		_Noise("", Range(0,1)) = 0
		_AutoShiftSpeed("", Range(0,1)) = 0.25
		_Hue("", Range(0,1)) = 0
		_SaturationHSL("", Range(-1,1)) = 0
		_Luminance ("", Range(0,0.5)) = 0
		_HSLMin("", Range(0,1)) = 0
		_HSLMax("", Range(0,1)) = 1
		_PulseSpeed("", Float) = 1
		_PulseStr("", Range(0,1)) = 0
		_Crossfade("", Range(0,0.2)) = 0.1
		_ReactThresh("", Range(0,1)) = 0.5
		_ShadowStr("", Range(0,1)) = 0.5
		_RampWidth0("", Range(0,1)) = 0
		_RampWidth1("", Range(0,1)) = 1
		_RampWeight("", Range(0,1)) = 0.15
		_ReflectionStr("", Range(0,1)) = 1
		_SpecStr("", Range(0,1)) = 1
		_RimStr("", Range(0,1)) = 0
		_RimWidth("", Range (0,1)) = 0.5
		_RimEdge("", Range(0,0.5)) = 0
		_CloneSpacing("", Float) = 0
		_Visibility("", Range(0,1)) = 0
		_CloneSize("", Float) = 1
		_Instability("", Range(0,0.01)) = 0
		_GlitchFrequency("", Range(0,0.01)) = 0
		_GlitchIntensity("", Range(0,0.1)) = 0
		_ShatterSpread("", Float) = 0.347
		_ShatterMax("", Float) = 0.65
		_ShatterMin("", Float) = 0.25
		_ShatterCull("", Float) = 0.535
		_WFFill("", Range(0,1)) = 0
		_WFVisibility("", Range(0,1)) = 1
		_PatternMult("", Float) = 2.5
		_Range("", Range(0,50)) = 10
		_DissolveAmount("", Range(0,1)) = 0
		_DissolveBlendSpeed("", Range(0,1)) = 0
		_DissolveRimWidth("", Float) = 0.5
		_OutlineThicc("", Float) = 0.1
		_OutlineRange("", Range(0,1)) = 0
		_BlendMode("__mode", Float) = 0.0
		_SrcBlend("__src", Float) = 5.0
		_UseReflCube("", Int) = 0
    }

    SubShader {
		// Render queue options:
		// Background 	- renders first, will show up behind all environment
		// Geometry 	- standard opaque queue for solid objects
		// AlphaTest	- most often used for cutout shading
		// Transparent	- standard queue for transparent objects
		// Overlay		- standard queue for post processing effects and menus
		// Use + or - to go above or below the queue level (ie. Transparent+1)
		// Change the following text seen below: "Queue"="____" to the desired value
        Tags {
			"RenderType"="Transparent" 
			"Queue"="Transparent"
		}
        Blend [_SrcBlend] OneMinusSrcAlpha
        Cull [_CullingMode]
        Pass {
            Name "ForwardBase"
            Tags {"LightMode"="ForwardBase"}
            ZWrite [_ZWrite]
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma shader_feature _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP
            #pragma shader_feature _SPECGLOSSMAP
			#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _GLOSSYREFLECTIONS_OFF
			#pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma target 5.0
            #define TRANSPARENT
			#if !(UNITY_VERSION >= 201840)
				#define UNITY_PASS_FORWARDBASE
			#endif
            #include "USDefines.cginc"
            #include "USUtilities.cginc"
			#include "USLighting.cginc"
            #include "USFunctions.cginc"
            #include "USPass.cginc"
            ENDCG
        }

        Pass {
            Name "ForwardAdd"
            Tags {"LightMode"="ForwardAdd"}
            Blend SrcAlpha One
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma shader_feature _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP
            #pragma shader_feature _SPECGLOSSMAP
			#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _GLOSSYREFLECTIONS_OFF
			#pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma target 5.0
            #define TRANSPARENT
			#if !(UNITY_VERSION >= 201840)
				#define UNITY_PASS_FORWARDADD
			#endif
            #include "USDefines.cginc"
            #include "USUtilities.cginc"
			#include "USLighting.cginc"
            #include "USFunctions.cginc"
            #include "USPass.cginc"
            ENDCG
        }
    }
    Fallback "Transparent/Diffuse"
    CustomEditor "USEditor"
}