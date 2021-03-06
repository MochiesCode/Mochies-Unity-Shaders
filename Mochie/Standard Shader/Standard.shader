// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Standard shader modification by Mochie (Mochie#8794)
// https://www.patreon.com/mochieshaders
// https://github.com/MochiesCode/Mochies-Unity-Shaders

// Does not support deferred rendering
// Does not meet the same instruction count restrictions as normal standard...
// ... intended for modern PC use only

Shader "Mochie/Standard" {
    Properties {

		[Enum(Opaque,0, Cutout,1, Fade,2, Transparent,3)]_BlendMode("Blending Mode", Int) = 0
		[Enum(Standard,0, Packed (Baked),1, Packed (Modular),2)]_Workflow("Workflow", Int) = 0
		[Enum(Default,0, Stochastic,1, Supersampled,2, Triplanar,3)]_SamplingMode("Sampling Mode", Int) = 0
		_TriplanarFalloff("Triplanar Falloff", Float) = 1
		_EdgeFadeMin("Edge Fade Min", Float) = 0.25
		_EdgeFadeMax("Edge Fade Max", Float) = 0.5
        _Color("Color", Color) = (1,1,1,1)
		_Saturation("Saturation", Float) = 1
        _MainTex("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_PackedMap("Packed Texture", 2D) = "white" {}
		[ToggleUI]_RoughnessMult("Roughness Multiplier", Int) = 0
		[ToggleUI]_MetallicMult("Metallic Multiplier", Int) = 0
		[ToggleUI]_OcclusionMult("Occlusion Multiplier", Int) = 0
		[ToggleUI]_HeightMult("Height Multiplier", Int) = 0

		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_RoughnessChannel("Roughness Channel", Int) = 1
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_MetallicChannel("Metallic Channel", Int) = 2
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_OcclusionChannel("Occlusion Channel", Int) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)]_HeightChannel("Height Channel", Int) = 3

        _Glossiness("Roughness", Range(0.0, 1.0)) = 0.5
        _SpecGlossMap("Roughness Map", 2D) = "white" {}
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}
        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}
        _Parallax ("Height Scale", Range (0, 0.2)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}
		_ParallaxMask("Height Mask", 2D) = "white" {}
		[IntRange]_ParallaxSteps("Parallax Steps", Range(1,50)) = 25
		_ParallaxOffset("Parallax Offset", Range(-1, 1)) = 0
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}
        [HDR]_EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
		_EmissionMask("Mask", 2D) = "white" {}
		_EmissionIntensity("Intensity", Float) = 1

		_UV0Rotate("UV0 Rotation", Float) = 0
		_UV0Scroll("UV0 Scrolling", Vector) = (0,0,0,0)
		_UV1Rotate("UV1 Rotation", Float) = 0
		_UV1Scroll("UV1 Scrolling", Vector) = (0,0,0,0)
		_UV2Scroll("Mask Scrolling", Vector) = (0,0,0,0)
		_UV3Scroll("Mask Scrolling", Vector) = (0,0,0,0)

        _DetailMask("Detail Mask", 2D) = "white" {}
        _DetailAlbedoMap("Detail Base Color", 2D) = "gray" {}
		[Enum(Add,0, Subtract,1, Multiply,2, Overlay,3, Screen,4, Soft Light,5)]_DetailAlbedoBlend("Detail Base Color Blend", Int) = 2
		_DetailNormalMap("Detail Normal Map", 2D) = "bump" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        _DetailRoughnessMap("Detail Roughness Map", 2D) = "gray" {}
		[Enum(Add,0, Subtract,1, Multiply,2, Overlay,3, Screen,4, Soft Light,5)]_DetailRoughBlend("Detail Roughness Blend", Int) = 2
		_DetailAOMap("Detail AO Map", 2D) = "white" {}
		[Enum(Add,0, Subtract,1, Multiply,2, Overlay,3, Screen,4, Soft Light,5)]_DetailAOBlend("Detail AO Blend", Int) = 2
        [Enum(UV0,0,UV1,1)]_UVSec("UV Set for secondary textures", Float) = 0

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

		_Cull("", Int) = 2
		_MetaCull("", Int) = 0
		[Enum(UnityEngine.Rendering.CullMode)]_CullingMode("", Int) = 2
		[ToggleOff]_SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff]_GlossyReflections("Glossy Reflections", Float) = 1.0
		[ToggleUI]_SSR("Screenspace Reflections", Int) = 0
		[ToggleUI]_UseHeight("Use Heightmap", Int) = 0
		[ToggleUI]_GSAA("GSAA", Int) = 0
		_SSRStrength("SSR Strength", Float) = 1
		_ReflectionStrength("Relfection Strength", Float) = 1
		_SpecularStrength("Specular Strength", Float) = 1
		_QueueOffset("Queue Offset", Int) = 0

        [HideInInspector]_SrcBlend("__src", Float) = 1.0
        [HideInInspector]_DstBlend("__dst", Float) = 0.0
        [HideInInspector]_ZWrite("__zw", Float) = 1.0
		[HideInInspector]_ZTest("__zt", Float) = 4.0
		[HideInInspector]_NoiseTexSSR("SSR Noise Texture", 2D) = "black" {}
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT RoughnessSetup
		#define MOCHIE_BRDF BRDF1_Mochie_PBS
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
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
			#pragma shader_feature _ BLOOM_LENS_DIRT _FADING_ON
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SPECGLOSSMAP
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _PARALLAXMAP
			#pragma shader_feature _MAPPING_6_FRAMES_LAYOUT
			#pragma shader_feature FXAA
			#pragma shader_feature GRAIN
			#pragma shader_feature _ EFFECT_HUE_VARIATION BLOOM _COLORCOLOR_ON EFFECT_BUMP
			#pragma shader_feature _COLOROVERLAY_ON
			#pragma shader_feature GEOM_TYPE_LEAF
			#pragma shader_feature GEOM_TYPE_FROND
			#pragma shader_feature GEOM_TYPE_MESH
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
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
			#pragma shader_feature _ BLOOM_LENS_DIRT _FADING_ON
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SPECGLOSSMAP
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP
			#pragma shader_feature FXAA
			#pragma shader_feature _ EFFECT_HUE_VARIATION BLOOM _COLORCOLOR_ON EFFECT_BUMP
			#pragma shader_feature _COLOROVERLAY_ON
			#pragma shader_feature GEOM_TYPE_LEAF
			#pragma shader_feature GEOM_TYPE_FROND
			#pragma shader_feature GEOM_TYPE_MESH
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
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
			#pragma shader_feature _ BLOOM_LENS_DIRT _FADING_ON
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _PARALLAXMAP
			#pragma shader_feature _ EFFECT_HUE_VARIATION BLOOM _COLORCOLOR_ON EFFECT_BUMP
			#pragma shader_feature GEOM_TYPE_MESH
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
			#pragma shader_feature _ BLOOM_LENS_DIRT _FADING_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SPECGLOSSMAP
            #pragma shader_feature ___ _DETAIL_MULX2
			#pragma shader_feature _ EFFECT_HUE_VARIATION BLOOM _COLORCOLOR_ON EFFECT_BUMP
			#pragma shader_feature _COLOROVERLAY_ON
			#pragma shader_feature GEOM_TYPE_LEAF
			#pragma shader_feature GEOM_TYPE_FROND
			#pragma shader_feature GEOM_TYPE_MESH
            #pragma shader_feature EDITOR_VISUALIZATION
            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }
    FallBack "VertexLit"
    CustomEditor "MochieStandardGUI"
}
