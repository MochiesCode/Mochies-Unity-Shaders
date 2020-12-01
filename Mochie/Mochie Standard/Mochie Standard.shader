// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Mochie/Mochie Standard"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
		_Saturation("Saturation", Float) = 1
        _MainTex("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_PackedMap("Packed Texture (RAM)", 2D) = "white" {}
		[ToggleUI]_RoughnessMult("", Int) = 0
		[ToggleUI]_MetallicMult("", Int) = 0
		[ToggleUI]_OcclusionMult("", Int) = 0
		[ToggleUI]_HeightMult("", Int) = 0

        _Glossiness("Roughness", Range(0.0, 2.0)) = 1.0
        _SpecGlossMap("Roughness Map", 2D) = "white" {}

        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax ("Height Scale", Range (0, 0.2)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}
		[IntRange]_ParallaxSteps("Parallax Steps", Range(1,50)) = 25

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}
		
        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
		_UV0Rotate("UV0 Rotation", Float) = 0

        _DetailMask("Detail Mask", 2D) = "white" {}
        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        _DetailNormalMap("Normal Map", 2D) = "bump" {}
		_UV1Rotate("UV1 Rotation", Float) = 0
        [Enum(UV0,0,UV1,1)]_UVSec("UV Set for secondary textures", Float) = 0

		_ReflCube("Reflection Cubemap", CUBE) = "" {}
		_ReflCubeMask("Fallback Alpha Mask", CUBE) = "" {}
		_CubeThreshold("Threshold", Range(0,1)) = 0.45
		_EdgeFade("SSR Edge Fade", Range(0,1)) = 0.1
		
		
		[Enum(UnityEngine.Rendering.CullMode)]_CullingMode("", Int) = 2
		[ToggleOff]_SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff]_GlossyReflections("Glossy Reflections", Float) = 1.0
		[ToggleUI]_SSR("Screenspace Reflections", Int) = 0
		[ToggleUI]_Dith("SSR Dithering", Int) = 0
		[ToggleUI]_UseHeight("Use Heightmap", Int) = 0
		[ToggleUI]_GSAA("GSAA", Int) = 0

        [HideInInspector]_Mode("__mode", Float) = 0.0
		[HideInInspector]_WorkMode("__workmode", Float) = 0.0
        [HideInInspector]_SrcBlend("__src", Float) = 1.0
        [HideInInspector]_DstBlend("__dst", Float) = 0.0
        [HideInInspector]_ZWrite("__zw", Float) = 1.0
		[HideInInspector]_NoiseTexSSR("SSR Noise Texture", 2D) = "black" {}
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT RoughnessSetup
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300
		
		Cull [_CullingMode]

		GrabPass {
			Tags {"LightMode"="Always"}
			"_MSSRGrab"
		}
        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 5.0

            // -------------------------------------
			#pragma shader_feature BLOOM_LENS_DIRT

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
			#pragma shader_feature CHROMATIC_ABBERATION_LOW
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma vertex vertBase
            #pragma fragment fragBase
            #include "MochieStandardCoreForward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 5.0

            // -------------------------------------
			#pragma shader_feature BLOOM_LENS_DIRT
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SPECGLOSSMAP
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP
			#pragma shader_feature _MAPPING_6_FRAMES_LAYOUT
			#pragma shader_feature FXAA
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog

            #pragma vertex vertAdd
            #pragma fragment fragAdd
            #include "MochieStandardCoreForward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 3.5

            // -------------------------------------
			#pragma shader_feature BLOOM_LENS_DIRT
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "MochieStandardShadow.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Deferred pass
        Pass
        {
            Name "DEFERRED"
            Tags { "LightMode" = "Deferred" }

            CGPROGRAM
            #pragma target 3.0
            #pragma exclude_renderers nomrt


            // -------------------------------------
			#pragma shader_feature BLOOM_LENS_DIRT
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SPECGLOSSMAP
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing

            #pragma vertex vertDeferred
            #pragma fragment fragDeferred

            #include "MochieStandardCore.cginc"

            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta
			#pragma shader_feature BLOOM_LENS_DIRT
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SPECGLOSSMAP
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 150

        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 2.0
			#pragma shader_feature BLOOM_LENS_DIRT
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SPECGLOSSMAP
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
			#pragma shader_feature CHROMATIC_ABBERATION_LOW
            // SM2.0: NOT SUPPORTED shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP

            #pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #pragma vertex vertBase
            #pragma fragment fragBase
            #include "MochieStandardCoreForward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 2.0
			#pragma shader_feature BLOOM_LENS_DIRT
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SPECGLOSSMAP
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            // SM2.0: NOT SUPPORTED #pragma shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP
            #pragma skip_variants SHADOWS_SOFT

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog

            #pragma vertex vertAdd
            #pragma fragment fragAdd
            #include "MochieStandardCoreForward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 2.0
			#pragma shader_feature BLOOM_LENS_DIRT
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SPECGLOSSMAP
            #pragma skip_variants SHADOWS_SOFT
            #pragma multi_compile_shadowcaster

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "MochieStandardShadow.cginc"

            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta
			#pragma shader_feature BLOOM_LENS_DIRT
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SPECGLOSSMAP
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }
    FallBack "VertexLit"
    CustomEditor "MochieStandardGUI"
}
