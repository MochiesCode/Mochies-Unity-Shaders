// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Standard shader modification by Mochie (Mochie#8794)
// https://www.patreon.com/mochieshaders
// https://github.com/MochiesCode/Mochies-Unity-Shaders

// Does not support deferred rendering
// Does not meet the same instruction count restrictions as normal standard, intended for modern PC use only

Shader "Mochie/Standard" {
    Properties {

		[Enum(Opaque,0, Cutout,1, Fade,2, Transparent,3)]_BlendMode("Blending Mode", Int) = 0
        _Color("Color", Color) = (1,1,1,1)
		_DetailColor("Detail Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}
		_AlphaMask("Alpha", 2D) = "white" {}
		[Enum(Off,0, On,1)]_UseAlphaMask("Use Alpha Mask", Int) = 0
		_AlphaMaskOpacity("Alpha Mask Opacity", Range(0,1)) = 1
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		[Enum(Standard,0, Packed,1)]_Workflow("Workflow", Int) = 0
		[Enum(Default,0, Stochastic,1, Supersampled,2, Triplanar,3)]_SamplingMode("Sampling Mode", Int) = 0
		_TriplanarFalloff("Triplanar Falloff", Float) = 1
		_PackedMap("Packed Texture", 2D) = "white" {}
		[ToggleUI]_RoughnessMult("Roughness Multiplier", Int) = 0
		[ToggleUI]_MetallicMult("Metallic Multiplier", Int) = 0
		[ToggleUI]_OcclusionMult("Occlusion Multiplier", Int) = 0
		[ToggleUI]_HeightMult("Height Multiplier", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_RoughnessChannel("Roughness Channel", Int) = 1
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_MetallicChannel("Metallic Channel", Int) = 2
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_OcclusionChannel("Occlusion Channel", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_HeightChannel("Height Channel", Int) = 3

		[Enum(Standard,0, Packed,1)]_DetailWorkflow("Workflow", Int) = 0
		[Enum(Default,0, Stochastic,1, Supersampled,2, Triplanar,3)]_DetailSamplingMode("Sampling Mode", Int) = 0
		_DetailTriplanarFalloff("Triplanar Falloff", Float) = 1
		_DetailPackedMap("Packed Texture", 2D) = "white" {}
		_DetailRoughnessStrength("Detail Roughness Strength", Range(0,1)) = 1
		_DetailMetallicStrength("Detail Metallic Strength", Range(0,1)) = 1
		_DetailOcclusionStrength("Detail Occlusion Strength", Range(0,1)) = 1
		[ToggleUI]_DetailRoughnessMult("Roughness Multiplier", Int) = 0
		[ToggleUI]_DetailMetallicMult("Metallic Multiplier", Int) = 0
		[ToggleUI]_DetailOcclusionMult("Occlusion Multiplier", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DetailRoughnessChannel("Roughness Channel", Int) = 1
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DetailMetallicChannel("Metallic Channel", Int) = 2
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DetailOcclusionChannel("Occlusion Channel", Int) = 0

        _Glossiness("Roughness", Range(0.0, 1.0)) = 0.5
        _SpecGlossMap("Roughness Map", 2D) = "white" {}
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}
        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}
        _Parallax("Height Scale", Range (0, 0.2)) = 0.02
        _ParallaxMap("Height Map", 2D) = "black" {}
		_ParallaxMask("Height Mask", 2D) = "white" {}
		[IntRange]_ParallaxSteps("Parallax Steps", Range(1,50)) = 25
		_ParallaxOffset("Parallax Offset", Range(-1, 1)) = 0
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        [HDR]_EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
		_EmissionMask("Mask", 2D) = "white" {}
		_EmissionIntensity("Intensity", Float) = 1
		[Enum(Off,0, Sin,1, Square,2, Triangle,3, Saw,4, Reverse Saw,5)]_EmissPulseWave("Pulse", Int) = 0
		_EmissPulseSpeed("Pulse Speed", Float) = 1
		_EmissPulseStrength("Pulse Strength", Float) = 1
		[Enum(Off,0, Bass,1, Low Mids,2, Upper Mids,3, Highs,4)]_AudioLinkEmission("Emission Band", Int) = 0
		_AudioLinkEmissionStrength("Emission Strength", Float) = 1
		[ToggleUI]_AudioLinkEmissionMeta("Audio Link Meta Emission", Int) = 0

		_UV0Rotate("UV0 Rotation", Float) = 0
		_UV0Scroll("UV0 Scrolling", Vector) = (0,0,0,0)
		_UV1Rotate("UV1 Rotation", Float) = 0
		_UV1Scroll("UV1 Scrolling", Vector) = (0,0,0,0)
		_UV2Scroll("Mask Scrolling", Vector) = (0,0,0,0)
		_UV3Rotate("UV3 Rotation", Float) = 0
		_UV3Scroll("Mask Scrolling", Vector) = (0,0,0,0)
		_UV4Rotate("UV4 Rotation", Float) = 0
		_UV4Scroll("Mask Scrolling", Vector) = (0,0,0,0)
		_UV5Rotate("UV5 Rotation", Float) = 0
		_UV5Scroll("Mask Scrolling", Vector) = (0,0,0,0)
		_DetailRotate("Detail Mask Rotate", Float) = 0
		_DetailScroll("Detail Mask Scroll", Vector) = (0,0,0,0)

        _DetailMask("Detail Mask", 2D) = "white" {}
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DetailMaskChannel("Detail Mask Channel", Int) = 3
        _DetailAlbedoMap("Detail Base Color", 2D) = "gray" {}
		[Enum(Add,0, Alpha,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_DetailAlbedoBlend("Detail Base Color Blend", Int) = 2
		_DetailNormalMap("Detail Normal Map", 2D) = "bump" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        _DetailRoughnessMap("Detail Roughness Map", 2D) = "white" {}
		[Enum(Add,0, Alpha,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_DetailRoughBlend("Detail Roughness Blend", Int) = 2
		_DetailAOMap("Detail AO Map", 2D) = "white" {}
		[Enum(Add,0, Alpha,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_DetailAOBlend("Detail AO Blend", Int) = 2
		_DetailMetallicMap("Detail Metallic Map", 2D) = "white" {}
		[Enum(Add,0, Alpha,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_DetailMetallicBlend("Detail Metallic Blend", Int) = 2

		[Enum(UV0,0,UV1,1, UV2,2, UV3,3, UV4,4)]_UVPri("UV Set for primary textures", Float) = 0
        [Enum(UV0,0,UV1,1, UV2,2, UV3,3, UV4,4)]_UVSec("UV Set for secondary textures", Float) = 0
		[Enum(UV0,0,UV1,1, UV2,2, UV3,3, UV4,4)]_UVEmissMask("UV Set for Emission Mask", Float) = 0
		[Enum(UV0,0,UV1,1, UV2,2, UV3,3, UV4,4)]_UVHeightMask("UV Set for height mask", Float) = 0
		[Enum(UV0,0,UV1,1, UV2,2, UV3,3, UV4,4)]_UVAlphaMask("UV Set for alpha mask", Float) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_AlphaMaskChannel("Alpha Mask Channel", Int) = 3
		[Enum(UV0,0,UV1,1, UV2,2, UV3,3, UV4,4)]_UVRainMask("UV Set for rain mask", Float) = 0
		[Enum(UV0,0,UV1,1, UV2,2, UV3,3, UV4,4)]_UVRimMask("UV Set for rim mask", Float) = 0
		[Enum(UV0,0,UV1,1, UV2,2, UV3,3, UV4,4)]_UVDetailMask("UV Set for detail mask", Float) = 0

		[ToggleUI]_Filtering("Filtering", Int) = 0
		_Hue("Hue", Range(0,1)) = 0
		_Contrast("Contrast", Float) = 1
		_Saturation("Saturation", Float) = 1
		_Brightness("Brightness", Float) = 1
		_ACES("ACES", Float) = 0

		_HueDet("Hue", Range(0,1)) = 0
		_ContrastDet("Contrast", Float) = 1
		_BrightnessDet("Brightness", Float) = 1
		_SaturationDet("Saturation", Float) = 1

		_HueEmiss("Hue", Range(0,1)) = 0
		_ContrastEmiss("Contrast", Float) = 1
		_SaturationEmiss("Saturation", Float) = 1
		_BrightnessEmiss("Brightness", Float) = 1

		_HuePost("Hue", Range(0,1)) = 0
		_ContrastPost("Contrast", Float) = 1
		_SaturationPost("Saturation", Float) = 1
		_BrightnessPost("Brightness", Float) = 1

		[ToggleUI]_ReflCubeToggle("", Int) = 0
		[ToggleUI]_ReflCubeOverrideToggle("", Int) = 0
		_ReflCube("Reflection Fallback", CUBE) = "" {}
		_ReflCubeOverride("Reflection Override", CUBE) = "" {}
		_CubeThreshold("Threshold", Range(0.0001,1)) = 0.45
		_EdgeFade("SSR Edge Fade", Range(0,1)) = 0.1
		
		[ToggleUI]_Subsurface("Subsurface Scattering", Int) = 0
		[ToggleUI]_ScatterAlbedoTint("Scatter Albedo Tint", Int) = 0
		_ThicknessMap("Thickness Map", 2D) = "black" {}
		_ThicknessMapPower("Thickness Map Power", Float) = 1
		_ScatterCol("Subsurface Color", Color) = (1,1,1,1)
		_ScatterIntensity("Intensity", Range(0,10)) = 1
		_ScatterPow("Power", Range(0.01,10)) = 1
		_ScatterDist("Distance", Range(0,10)) = 1
		_ScatterAmbient("Ambient Intensity", Range(0,0.5)) = 0
		_ScatterShadow("Shadow Power", Range(0,1)) = 1
		_WrappingFactor("Wrapping Factor", Range(0.001, 1)) = 0.01

		[ToggleUI]_RimToggle("Enable", Int) = 0
		[HDR]_RimCol("Rim Color", Color) = (1,1,1,1)
		[Enum(Add,0, Sub,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_RimBlending("Rim Blending", Int) = 0
		_RimStr("Rim Strength", Float) = 1
		_RimWidth("Rim Width", Range (0,1)) = 0.5
		_RimEdge("Rim Edge", Range(0,0.5)) = 0
		_RimMask("Rim Mask", 2D) = "white" {}
		_UVRimMaskScroll("Scrolling", Vector) = (0,0,0,0)
		_UVRimMaskRotate("Rotation", Float) = 0

		_MetaCull("", Int) = 0
		[Enum(UnityEngine.Rendering.CullMode)]_Cull("", Int) = 2
		[ToggleOff]_SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff]_GlossyReflections("Glossy Reflections", Float) = 1.0
		[ToggleUI]_SSR("Screenspace Reflections", Int) = 0
		[Enum(Off,0, On,1)]_UseHeight("Use Heightmap", Int) = 0
		[ToggleUI]_ReflShadows("Shadowed Reflections", Int) = 0
		[ToggleUI]_ReflVertexColor("Vertex Color Reflections", Int) = 0
		[ToggleUI]_GSAA("GSAA", Int) = 0
		[Enum(Off,0, On,1)]_UseSmoothness("Use Smoothness", Int) = 0
		[Enum(Off,0, On,1)]_DetailUseSmoothness("Detail Use Smoothness", Int) = 0
		[ToggleUI]_UseFresnel("Use Fresnel", Int) = 1
		[ToggleUI]_BicubicLightmap("Bicubic Lightmap", Int) = 1
        [ToggleUI]_LTCGI("LTCGI", Int) = 0
    	[ToggleUI]_LTCGI_DIFFUSE_OFF("LTCGI Disable Diffuse", Int) = 0
		[ToggleUI]_LTCGI_SPECULAR_OFF("LTCGI Disable Specular", Int) = 0
		_LTCGI_mat("LTC Mat", 2D) = "black" {}
        _LTCGI_amp("LTC Amp", 2D) = "black" {}
		_LTCGIStrength("LTCGI Strength", Float) = 1
		_FresnelStrength("Fresnel Strength", Float) = 1
		_SSRStrength("SSR Strength", Float) = 1
		_ReflectionStrength("Relfection Strength", Float) = 1
		_SpecularStrength("Specular Strength", Float) = 1
		_QueueOffset("Queue Offset", Int) = 0
		_GSAAStrength("GSAA Strength", Float) = 1
		_ReflShadowStrength("Shadowed Reflection Strength", Float) = 1
		_ReflVertexColorStrength("Vertex Color Reflection Strength", Float) = 1

		_ContrastReflShad("Contrast", Float) = 1
		_BrightnessReflShad("Brightness", Float) = 100
		_HDRReflShad("HDR", Float) = 1
		_TintReflShad("Tint", Color) = (1,1,1,1)

		[ToggleUI]_RainToggle("Enable", Int) = 0
		_RippleScale("Ripple Scale", float) = 40
		_RippleSpeed("Ripple Speed", float) = 10
		_RippleStr("Ripple Strength", float) = 1
		_RainMask("Rain Mask", 2D) = "white" {}

		[Toggle(BAKERY_LMSPEC)] _BAKERY_LMSPEC ("Enable Lightmap Specular", Float) = 0
		[Toggle(BAKERY_SHNONLINEAR)] _BAKERY_SHNONLINEAR ("Non-Linear SH", Float) = 0
		[Enum(None, 0, SH, 1, RNM, 2, MONOSH, 3)] _BakeryMode ("Bakery Mode", Int) = 0
		_RNM0("RNM0", 2D) = "black" {}
		_RNM1("RNM1", 2D) = "black" {}
		_RNM2("RNM2", 2D) = "black" {}

		[ToggleUI]_AreaLitToggle("Enable", Int) = 0
		_AreaLitMask("Mask", 2D) = "white" {}
		_AreaLitStrength("Strength", Float) = 1
		_AreaLitRoughnessMult("Roughness Multiplier", Float) = 1
		[NoScaleOffset]_LightMesh("Light Mesh", 2D) = "black" {}
		[NoScaleOffset]_LightTex0("Light Texture 0", 2D) = "white" {}
		[NoScaleOffset]_LightTex1("Light Texture 1", 2D) = "black" {}
		[NoScaleOffset]_LightTex2("Light Texture 2", 2D) = "black" {}
		[NoScaleOffset]_LightTex3("Light Texture 3", 2DArray) = "black" {}
		[ToggleOff]_OpaqueLights("Opaque Lights", Float) = 1.0

		_AreaLitOcclusion("Occlusion", 2D) = "white" {}
		[Enum(UV0,0,UV1,1, UV2,2, UV3,3, UV4,4)]_OcclusionUVSet("UV Set for occlusion map", Float) = 0

        [HideInInspector]_SrcBlend("__src", Float) = 1.0
        [HideInInspector]_DstBlend("__dst", Float) = 0.0
        [HideInInspector]_ZWrite("__zw", Float) = 1.0
		[HideInInspector]_ZTest("__zt", Float) = 4.0
		[HideInInspector]_NoiseTexSSR("SSR Noise Texture", 2D) = "black" {}
		[HideInInspector]_NaNLmao("lol", Float) = 0

		[ToggleUI]_MirrorToggle("Mirror Mode", Int) = 0
		[HideInInspector] _ReflectionTex0("", 2D) = "white" {}
        [HideInInspector] _ReflectionTex1("", 2D) = "white" {}

		// [HideInInspector] BAKERY_META_ALPHA_ENABLE ("Enable Bakery alpha meta pass", Float) = 1.0
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT RoughnessSetup
		#define MOCHIE_BRDF BRDF1_Mochie_PBS
    ENDCG

    SubShader
    {
        Tags {
			"RenderType"="Opaque" 
			"PerformanceChecks"="False"
			"LTCGI"="_LTCGI"
		}
        LOD 300
		
		Cull [_Cull]
		ZTest [_ZTest]

		GrabPass {
			Tags {"LightMode"="Always"}
			"_GrabTexture"
		}

        Pass {
            Name "FORWARD"
            Tags {"LightMode" = "ForwardBase"}
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 5.0
			#pragma vertex vertBase
            #pragma fragment fragBase
			#define MOCHIE_STANDARD
			#pragma shader_feature_local _WORKFLOW_PACKED_ON
			#pragma shader_feature_local _DETAIL_WORKFLOW_PACKED_ON
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _SPECGLOSSMAP
            #pragma shader_feature_local ___ _DETAIL_MULX2
            #pragma shader_feature_local _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local _ _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature_local _PARALLAXMAP
			#pragma shader_feature_local _SCREENSPACE_REFLECTIONS_ON
			#pragma shader_feature_local _ _STOCHASTIC_ON _TSS_ON _TRIPLANAR_ON
			#pragma shader_feature_local _ _DETAIL_STOCHASTIC_ON _DETAIL_TSS_ON _DETAIL_TRIPLANAR_ON
			#pragma shader_feature_local _DETAIL_ROUGH_ON
			#pragma shader_feature_local _DETAIL_AO_ON
			#pragma shader_feature_local _DETAIL_METALLIC_ON
			#pragma shader_feature_local _AUDIOLINK_ON
			#pragma shader_feature_local _DETAIL_SAMPLEMODE_ON
			#pragma shader_feature_local _ALPHAMASK_ON
			#pragma shader_feature_local _BICUBIC_SAMPLING_ON
			#pragma shader_feature_local LTCGI
			#pragma shader_feature_local LTCGI_DIFFUSE_OFF
			#pragma shader_feature_local LTCGI_SPECULAR_OFF
			#pragma shader_feature_local _ BAKERY_SH BAKERY_RNM BAKERY_MONOSH
			#pragma shader_feature_local BAKERY_LMSPEC
			#pragma shader_feature_local BAKERY_SHNONLINEAR
			#pragma shader_feature_local _OPAQUELIGHTS_OFF
			#pragma shader_feature_local _AREALIT_ON
			#pragma shader_feature_local _MIRROR_ON
			#pragma shader_feature_local LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing
            #include "MochieStandardCoreForward.cginc"
            ENDCG
        }

        Pass {
            Name "FORWARD_DELTA"
            Tags {"LightMode" = "ForwardAdd"}
            Blend [_SrcBlend] One
            Fog {Color (0,0,0,0)}
            ZWrite Off

            CGPROGRAM
            #pragma target 5.0
			#pragma vertex vertAdd
            #pragma fragment fragAdd
			#define MOCHIE_STANDARD
			#pragma shader_feature_local _WORKFLOW_PACKED_ON
			#pragma shader_feature_local _DETAIL_WORKFLOW_PACKED_ON
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _SPECGLOSSMAP
            #pragma shader_feature_local _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local ___ _DETAIL_MULX2
            #pragma shader_feature_local _PARALLAXMAP
			#pragma shader_feature_local _ _STOCHASTIC_ON _TSS_ON _TRIPLANAR_ON
			#pragma shader_feature_local _ _DETAIL_STOCHASTIC_ON _DETAIL_TSS_ON _DETAIL_TRIPLANAR_ON
			#pragma shader_feature_local _DETAIL_ROUGH_ON
			#pragma shader_feature_local _DETAIL_AO_ON
			#pragma shader_feature_local _DETAIL_METALLIC_ON
			#pragma shader_feature_local _DETAIL_SAMPLEMODE_ON
			#pragma shader_feature_local _ALPHAMASK_ON
			#pragma shader_feature_local LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
            #pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_instancing
            #include "MochieStandardCoreForward.cginc"
            ENDCG
        }

        Pass {
            Name "ShadowCaster"
            Tags {"LightMode" = "ShadowCaster"}
            ZWrite On

            CGPROGRAM
            #pragma target 3.5
			#pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster
			#define MOCHIE_STANDARD
			#pragma shader_feature_local _WORKFLOW_PACKED_ON
			#pragma shader_feature_local _DETAIL_WORKFLOW_PACKED_ON
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
			#pragma shader_feature_local _DETAIL_METALLIC_ON
            #pragma shader_feature_local _PARALLAXMAP
			#pragma shader_feature_local _ _STOCHASTIC_ON _TSS_ON _TRIPLANAR_ON
			#pragma shader_feature_local _ _DETAIL_STOCHASTIC_ON _DETAIL_TSS_ON _DETAIL_TRIPLANAR_ON
			#pragma shader_feature_local _ALPHAMASK_ON
			#pragma shader_feature_local LOD_FADE_CROSSFADE
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
            #include "MochieStandardShadow.cginc"
            ENDCG
        }

        Pass {
            Name "META"
            Tags {"LightMode"="Meta"}
            Cull [_MetaCull]

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta
			#define MOCHIE_STANDARD
			#pragma shader_feature_local _WORKFLOW_PACKED_ON
			#pragma shader_feature_local _DETAIL_WORKFLOW_PACKED_ON
            #pragma shader_feature_local _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _SPECGLOSSMAP
            #pragma shader_feature_local ___ _DETAIL_MULX2
			#pragma shader_feature_local _ _STOCHASTIC_ON _TSS_ON _TRIPLANAR_ON
			#pragma shader_feature_local _ _DETAIL_STOCHASTIC_ON _DETAIL_TSS_ON _DETAIL_TRIPLANAR_ON
			#pragma shader_feature_local _DETAIL_ROUGH_ON
			#pragma shader_feature_local _DETAIL_AO_ON
			#pragma shader_feature_local _DETAIL_METALLIC_ON
			#pragma shader_feature_local _ALPHAMASK_ON
			#pragma shader_feature_local _OPAQUELIGHTS_OFF
			#pragma shader_feature_local _AREALIT_ON
			#pragma shader_feature_local _DETAIL_SAMPLEMODE_ON
			#pragma shader_feature_local _AUDIOLINK_ON
			#pragma shader_feature_local _AUDIOLINK_META_ON
            #pragma shader_feature EDITOR_VISUALIZATION
            #include "MochieStandardMeta.cginc"
            ENDCG
        }
		
        // Pass {
        //     Name "META_BAKERY"
        //     Tags {"LightMode"="Meta"}
        //     Cull [_MetaCull]

        //     CGPROGRAM
        //     #pragma vertex vert_meta
        //     #pragma fragment frag_meta
		// 	#define MOCHIE_STANDARD
		// 	#define BAKERY_META
		// 	#pragma shader_feature_local _WORKFLOW_PACKED_ON
		// 	#pragma shader_feature_local _DETAIL_WORKFLOW_PACKED_ON
        //     #pragma shader_feature_local _EMISSION
        //     #pragma shader_feature_local _METALLICGLOSSMAP
        //     #pragma shader_feature_local _SPECGLOSSMAP
        //     #pragma shader_feature_local ___ _DETAIL_MULX2
		// 	#pragma shader_feature_local _ _STOCHASTIC_ON _TSS_ON _TRIPLANAR_ON
		// 	#pragma shader_feature_local _ _DETAIL_STOCHASTIC_ON _DETAIL_TSS_ON _DETAIL_TRIPLANAR_ON
		// 	#pragma shader_feature_local _DETAIL_ROUGH_ON
		// 	#pragma shader_feature_local _DETAIL_AO_ON
		// 	#pragma shader_feature_local _DETAIL_METALLIC_ON
		// 	#pragma shader_feature_local _ALPHAMASK_ON
		// 	#pragma shader_feature_local _OPAQUELIGHTS_OFF
		// 	#pragma shader_feature_local _AREALIT_ON
		// 	#pragma shader_feature_local _DETAIL_SAMPLEMODE_ON
		// 	#pragma shader_feature_local _AUDIOLINK_ON
		// 	#pragma shader_feature_local _AUDIOLINK_META_ON
        //     #pragma shader_feature EDITOR_VISUALIZATION
        //     #include "MochieStandardMeta.cginc"
        //     ENDCG
        // }
    }
    FallBack "VertexLit"
    CustomEditor "MochieStandardGUI"
}