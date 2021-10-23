// By Mochie#8794

Shader "Mochie/Water" {
    Properties {
		
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Base Color", 2D) = "white" {}
		_MainTexScroll("Scrolling", Vector) = (0,0.1,0,0)
		_BaseColorOffset("Parallax Offset", Float) = 0
		[Toggle(_BASECOLOR_STOCHASTIC_ON)]_BaseColorStochasticToggle("Stochastic Sampling", Int) = 0
		_BaseColorDistortionStrength("Distortion Strength", Float) = 0.1
		_DistortionStrength("Distortion Strength", Float) = 0.1
		_Roughness("Roughness", Range(0,1)) = 0
		_Metallic("Metallic", Range(0,1)) = 0
		_Opacity("Opacity", Range(0,1)) = 1
		[ToggleUI]_Reflections("Probe Reflections", Int) = 1
		_ReflStrength("Reflection Strength", Float) = 1
		[ToggleUI]_SSR("Screenspace Reflections", Int) = 0
		_SSRStrength("SSR Strength", Float) = 1
		_EdgeFadeSSR("Edge Fade", Float) = 0.1
		[ToggleUI]_Specular("Specular", Int) = 1
		_SpecStrength("Specular Strength", Float) = 1
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull", Int) = 2
		[Enum(Off,0, On,1)]_ZWrite("ZWrite", Int) = 0
		
		[NoScaleOffset]_NormalMap0 ("", 2D) = "bump" {}
		_NormalStr0("Strength", Float) = 0.2
		_NormalMapScale0("Scale", Vector) = (3,3,0,0)
		_Rotation0("Rotation", Float) = 0
		_NormalMapScroll0("Scrolling", Vector) = (0.1,0.1,0,0)
		_NormalMapOffset0("Parallax Offset", Float) = 0
		[Toggle(_NORMALMAP_0_STOCHASTIC_ON)]_Normal0StochasticToggle("Stochastic Sampling", Int) = 0

		[Toggle(_NORMALMAP_1_ON)]_Normal1Toggle("Enable", Int) = 1
		[NoScaleOffset]_NormalMap1("", 2D) = "bump" {}
		_NormalStr1("Strength", Float) = 0.3
		_NormalMapScale1("Scale", Vector) = (4,4,0,0)
		_Rotation1("Rotation", Float) = 0
		_NormalMapScroll1("Scrolling", Vector) = (-0.1,0.1,0,0)
		_NormalMapOffset1("Parallax Offset", Float) = 0
		[Toggle(_NORMALMAP_1_STOCHASTIC_ON)]_Normal1StochasticToggle("Stochastic Sampling", Int) = 0

		[Toggle(_FLOW_ON)]_FlowToggle("Enable", Int) = 1
		[NoScaleOffset]_FlowMap("Flow Map", 2D) = "black" {}
		_FlowSpeed("Speed", Float) = 0.25
		_FlowStrength("Strength", Float) = 0.1
		_FlowMapScale("Scale", Vector) = (2,2,0,0)

		[Enum(Off,0, Noise Texture,1, Gerstner Waves,2)]_VertOffsetMode("Mode", Int) = 0
		[NoScaleOffset]_NoiseTex("Noise Texture", 2D) = "black" {}
		_NoiseTexScale("Scale", Vector) = (1,1,0,0)
		_NoiseTexScroll("Scrolling", Vector) = (0.3,0.06,0,0)
		_NoiseTexBlur("Blur", Range(0,1)) = 0.8
		_WaveHeight("Strength", Float) = 0.1
		_Offset("Offset", Vector) = (0,1,0,0)
		_WaveSpeed0("Wave 1 Speed", Float) = 1
		_WaveSpeed1("Wave 2 Speed", Float) = 1.1
		_WaveSpeed2("Wave 3 Speed", Float) = 1.2
		_WaveScale0("Wave 1 Scale", Float) = 4
		_WaveScale1("Wave 2 Scale", Float) = 2
		_WaveScale2("Wave 3 Scale", Float) = 1
		_WaveStrength0("Wave 1 Strength", Range(0,1)) = 0.1
		_WaveStrength1("Wave 2 Strength", Range(0,1)) = 0.1
		_WaveStrength2("Wave 3 Strength", Range(0,1)) = 0.1
		_Turbulence("Turbulence", Float) = 1
		_TurbulenceScale("Turbulence Scale", Float) = 3
		_TurbulenceSpeed("Turbulence Speed", Float) = 0.3
		_VertRemapMin("Remap Min", Float) = -1
		_VertRemapMax("Remap Max", Float) = 1
		
		[Toggle(_CAUSTICS_ON)]_CausticsToggle("Enable", Int) = 0
		_CausticsOpacity("Opacity", Range(0,1)) = 1
		_CausticsPower("Power", Float) = 5
		_CausticsScale("Scale", Float) = 5
		_CausticsSpeed("Speed", Float) = 1
		_CausticsFade("Depth Fade", Float) = 10
		
		[Toggle(_DEPTHFOG_ON)]_FogToggle("Enable", Int) = 1
		[HDR]_FogTint("Color", Color) = (1,1,1,0.3)
		_FogPower("Power", Float) = 1

		[Toggle(_FOAM_ON)]_FoamToggle("Enable", Int) = 1
		[NoScaleOffset]_FoamTex("Foam Texture", 2D) = "white" {}
		_FoamNoiseTex("Noise Texture", 2D) = "white" {}
		_FoamNoiseTexScale("Scale", Vector) = (2,2,0,0)
		_FoamNoiseTexScroll("Scroll", Vector) = (0,0.1,0,0)
		_FoamNoiseTexStrength("Strength", Float) = 0
		_FoamNoiseTexCrestStrength("Crest Strength", Float) = 1.1
		_FoamTexScale("Scale", Vector) = (3,3,0,0)
		_FoamTexScroll("Scroll", Vector) = (0.1,-0.1,0,0)
		_FoamRoughness("Roughness", Range(0,1)) = 0.6
		_FoamColor("Color", Color) = (1,1,1,1)
		_FoamPower("Power", Float) = 200
		_FoamOpacity("Opacity", Float) = 3
		_FoamOffset("Parallax Offset", Float) = 0
		_FoamCrestThreshold("Crest Threshold", Float) = 0.5
		_FoamCrestStrength("Crest Strength", Float) = 1
		[Toggle(_FOAM_STOCHASTIC_ON)]_FoamStochasticToggle("Stochastic Sampling", Int) = 0
		_FoamDistortionStrength("Distortion Strength", Float) = 0.1

		[Toggle(_EDGEFADE_ON)]_EdgeFadeToggle("Enable", Int) = 1
		_EdgeFadePower("Power", Float) = 200
		_EdgeFadeOffset("Offset", Float) = 0.5

		[HideInInspector]_NoiseTexSSR("SSR Noise Tex", 2D) = "black"
    }

    SubShader {
        Tags {
			"Queue"="Transparent" 
			"RenderType"="Transparent"
			"DisableBatching"="True"
			"IgnoreProjector"="True"
			"PreviewType"="Plane"
			"ForceNoShadowCasting"="True"
		}
		GrabPass {"_MWGrab"}
		ZWrite [_ZWrite]
		Cull [_CullMode]
        Pass {
			Tags {"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma shader_feature_local _REFLECTIONS_ON
			#pragma shader_feature_local _SCREENSPACE_REFLECTIONS_ON
			#pragma shader_feature_local _SPECULAR_ON
			#pragma shader_feature_local _NORMALMAP_1_ON
			#pragma shader_feature_local _FLOW_ON
			#pragma shader_feature_local _VERTEX_OFFSET_ON
			#pragma shader_feature_local _DEPTHFOG_ON
			#pragma shader_feature_local _FOAM_ON
			#pragma shader_feature_local _CAUSTICS_ON
			#pragma shader_feature_local _EDGEFADE_ON
			#pragma shader_feature_local _NORMALMAP_0_STOCHASTIC_ON
			#pragma shader_feature_local _NORMALMAP_1_STOCHASTIC_ON
			#pragma shader_feature_local _FOAM_STOCHASTIC_ON
			#pragma shader_feature_local _BASECOLOR_STOCHASTIC_ON
			#pragma shader_feature_local _GERSTNER_WAVES_ON

            #pragma target 5.0

			#include "WaterDefines.cginc"
			#include "WaterPass.cginc"

            ENDCG
        }

		Pass {
			Tags {"LightMode"="ForwardAdd"}
			Blend SrcAlpha One
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd fullshadows
			#pragma multi_compile_fog
			#pragma shader_feature_local _NORMALMAP_1_ON
			#pragma shader_feature_local _SPECULAR_ON
			#pragma shader_feature_local _FLOW_ON
			#pragma shader_feature_local _VERTEX_OFFSET_ON
			#pragma shader_feature_local _DEPTHFOG_ON
			#pragma shader_feature_local _FOAM_ON
			#pragma shader_feature_local _CAUSTICS_ON
			#pragma shader_feature_local _EDGEFADE_ON
			#pragma shader_feature_local _NORMALMAP_0_STOCHASTIC_ON
			#pragma shader_feature_local _NORMALMAP_1_STOCHASTIC_ON
			#pragma shader_feature_local _FOAM_STOCHASTIC_ON
			#pragma shader_feature_local _BASECOLOR_STOCHASTIC_ON
			#pragma shader_feature_local _GERSTNER_WAVES_ON

            #pragma target 5.0

			#include "WaterDefines.cginc"
			#include "WaterPass.cginc"

            ENDCG
        }
    }
	CustomEditor "WaterEditor"
}