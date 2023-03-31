// By Mochie#8794

Shader "Mochie/Water" {
    Properties {
		
		_Color("Color", Color) = (1,1,1,1)
		_AngleTint("Angle Tint", Color) = (1,1,1,1)
		_BackfaceTint("Backface Tint", Color) = (1,1,1,1)
		_MainTex("Base Color", 2D) = "white" {}
		_MainTexScroll("Scrolling", Vector) = (0,0.1,0,0)
		_BaseColorOffset("Parallax Offset", Float) = 0
		[Toggle(_BASECOLOR_STOCHASTIC_ON)]_BaseColorStochasticToggle("Stochastic Sampling", Int) = 0
		_BaseColorDistortionStrength("Distortion Strength", Float) = 0.1
		_DetailBaseColor("Detail Base Color", 2D) = "white" {}
		_DetailBaseColorTint("Detail Base Color Tint", Color) = (1,1,1,1)
		_DetailNormal("Detail Normal", 2D) = "bump" {}
		_DetailNormalStrength("Detail Normal Strength", Float) = 1
		_DistortionStrength("Distortion Strength", Float) = 0.5
		[ToggleUI]_DetailTextureMode("Detail Textures", Int) = 0
		_DetailScroll("Detail Scroll", Vector) = (0,0,0,0)
		_Roughness("Roughness", Range(0,1)) = 0
		_RoughnessMap("Roughness Map", 2D) = "white" {}
		_Metallic("Metallic", Range(0,1)) = 0
		_MetallicMap("Metallic Map", 2D) = "white" {}
		_Opacity("Opacity", Range(0,1)) = 1
		_ShadowStrength("Shadow Strength", Range(0,1)) = 0

		[Toggle(_EMISSION_ON)]_EmissionToggle("Emission Toggle", Int) = 0
		[Toggle(_EMISSIONMAP_STOCHASTIC_ON)]_EmissionMapStochasticToggle("Stochastic Sampling", Int) = 0
		_EmissionMap("Emission Map", 2D) = "white" {}
		[HDR]_EmissionColor("Emission Color", Color) = (1,1,1,1)
		_EmissionMapScroll("Emission Map Scroll", Vector) = (0,0,0,0)
		
		[NoScaleOffset]_NormalMap0 ("", 2D) = "bump" {}
		_NormalStr0("Strength", Float) = 0.1
		_NormalMapScale0("Scale", Vector) = (3,3,0,0)
		_Rotation0("Rotation", Float) = 0
		_NormalMapScroll0("Scrolling", Vector) = (0.1,0.1,0,0)
		_NormalMapOffset0("Parallax Offset", Float) = 0
		[Toggle(_NORMALMAP_0_STOCHASTIC_ON)]_Normal0StochasticToggle("Stochastic Sampling", Int) = 0

		[Toggle(_NORMALMAP_1_ON)]_Normal1Toggle("Enable", Int) = 1
		[NoScaleOffset]_NormalMap1("", 2D) = "bump" {}
		_NormalStr1("Strength", Float) = 0.2
		_NormalMapScale1("Scale", Vector) = (4,4,0,0)
		_Rotation1("Rotation", Float) = 0
		_NormalMapScroll1("Scrolling", Vector) = (-0.1,0.1,0,0)
		_NormalMapOffset1("Parallax Offset", Float) = 0
		[Toggle(_NORMALMAP_1_STOCHASTIC_ON)]_Normal1StochasticToggle("Stochastic Sampling", Int) = 0

		[Enum(Off,0, Environment,1, Manual,2)]_Reflections("Probe Reflections", Int) = 1
		_ReflStrength("Reflection Strength", Float) = 1
		_ReflTint("Reflection Tint", Color) = (1,1,1,1)
		[ToggleUI]_BackfaceReflections("Backface Reflections", Int) = 1
		[ToggleUI]_SSR("Screenspace Reflections", Int) = 0
		_SSRStrength("SSR Strength", Float) = 1
		_EdgeFadeSSR("Edge Fade", Float) = 0.1
		_ReflCube("Cubemap", CUBE) = "gray" {}
		_ReflCubeRotation("Rotation", Vector) = (0,0,0,0)
		[Enum(Off,0, Directional Light,1, Manual,2)]_Specular("Specular", Int) = 1
		_SpecStrength("Specular Strength", Float) = 1
		_SpecTint("Specular Tint", Color) = (1,1,1,1)
		_LightDir("LightDir", Vector) = (0,0.75,1,0)

		[Toggle(_FLOW_ON)]_FlowToggle("Enable", Int) = 1
		[NoScaleOffset]_FlowMap("Flow Map", 2D) = "black" {}
		[Enum(UV0,0, UV1,1, UV2,2, UV3,3)]_FlowMapUV("Flow Map UV Set", Int) = 0
		_FlowSpeed("Speed", Float) = 0.25
		_FlowStrength("Strength", Float) = 0.1
		_FlowMapScale("Scale", Vector) = (2,2,0,0)
		_BlendNoise("Blend Noise", 2D) = "white" {}
		_BlendNoiseScale("Blend Noise Scale", Vector) = (2,2,0,0)
		[Enum(Flowmap Alpha,0, Separate Texture,1)]_BlendNoiseSource("Blend Noise Source", Int) = 0

		[Enum(Off,0, Noise Texture,1, Gerstner Waves,2, Voronoi,3)]_VertOffsetMode("Mode", Int) = 0
		[NoScaleOffset]_NoiseTex("Noise Texture", 2D) = "black" {}
		_NoiseTexScale("Scale", Vector) = (3,3,0,0)
		_NoiseTexScroll("Scrolling", Vector) = (0,0.1,0,0)
		_NoiseTexBlur("Blur", Range(0,1)) = 0.8
		_WaveHeight("Strength", Float) = 1
		_Offset("Offset", Vector) = (0,1,0,0)
		_VoronoiScale("Scale", Vector) = (2,2,0,0)
		_VoronoiScroll("Scrolling", Vector) = (0,-0.25,0,0)
		_VoronoiWaveHeight("Strength", Float) = 1
		_VoronoiOffset("Offset", Vector) = (0,1,0,0)
		_VoronoiSpeed("Speed", Float) = 1.5
		_WaveSpeedGlobal("Wave Global Speed", Float) = 2
		_WaveScaleGlobal("Wave Global Scale", Float) = 1
		_WaveStrengthGlobal("Wave Global Strength", Float) = 1.5
		_WaveSpeed0("Wave 1 Speed", Float) = 1
		_WaveSpeed1("Wave 2 Speed", Float) = 1.1
		_WaveSpeed2("Wave 3 Speed", Float) = 1.2
		_WaveScale0("Wave 1 Scale", Float) = 4
		_WaveScale1("Wave 2 Scale", Float) = 2
		_WaveScale2("Wave 3 Scale", Float) = 1
		_WaveStrength0("Wave 1 Strength", Float) = 0.1
		_WaveStrength1("Wave 2 Strength", Float) = 0.1
		_WaveStrength2("Wave 3 Strength", Float) = 0.1
		_WaveDirection0("Wave 1 Direction", Range(0,360)) = 0
		_WaveDirection1("Wave 2 Direction", Range(0,360)) = 335
		_WaveDirection2("Wave 3 Direction", Range(0,360)) = 13
		_Turbulence("Turbulence", Float) = 1
		_TurbulenceScale("Turbulence Scale", Float) = 3
		_TurbulenceSpeed("Turbulence Speed", Float) = 0.3
		_VertRemapMin("Remap Min", Float) = -1
		_VertRemapMax("Remap Max", Float) = 1
		[ToggleUI]_RecalculateNormals("Recalculate Normals", Int) = 1
		
		[Toggle(_CAUSTICS_ON)]_CausticsToggle("Enable", Int) = 1
		_CausticsTex("Texture", 2D) = "black" {}
		_CausticsDisp("Dispersion", Float) = 0.25
		_CausticsDistortion("Distortion", Float) = 0.1
		_CausticsDistortionScale("Distortion Scale", Float) = 0.2
		_CausticsDistortionSpeed("Distortion Speed", Vector) = (0.1,0.1,0,0)
		_CausticsColor("Color", Color) = (1,1,1,1)
		_CausticsOpacity("Opacity", Float) = 1
		_CausticsPower("Power", Float) = 1
		_CausticsThreshold("Threshold", Float) = 0
		_CausticsScale("Scale", Float) = 7.5
		_CausticsSpeed("Speed", Float) = 3
		_CausticsFade("Depth Fade", Float) = 5
		_CausticsRotation("Rotation", Vector) = (-20,0,20,0)
		_CausticsSurfaceFade("Surface Fade", Float) = 100
		
		[Toggle(_DEPTHFOG_ON)]_FogToggle("Enable", Int) = 1
		_FogTint("Color", Color) = (0.11,0.26,0.26,1)
		_FogPower("Power", Float) = 12

		[Toggle(_FOAM_ON)]_FoamToggle("Enable", Int) = 1
		[NoScaleOffset]_FoamTex("Foam Texture", 2D) = "white" {}
		_FoamNoiseTex("Noise Texture", 2D) = "white" {}
		_FoamNoiseTexScale("Scale", Vector) = (3,3,0,0)
		_FoamNoiseTexScroll("Scroll", Vector) = (0,0.1,0,0)
		_FoamNoiseTexStrength("Strength", Float) = 0
		_FoamNoiseTexCrestStrength("Crest Strength", Float) = 1.1
		_FoamTexScale("Scale", Vector) = (5,5,0,0)
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
		[ToggleUI]_FoamNormalToggle("Foam Normals", Int) = 1
		_FoamNormalStrength("Foam Normal Strength", Float) = 4

		[Toggle(_EDGEFADE_ON)]_EdgeFadeToggle("Enable", Int) = 1
		_EdgeFadePower("Power", Float) = 200
		_EdgeFadeOffset("Offset", Float) = 0.5

		[Toggle(_RAIN_ON)]_RainToggle("Enable", Int) = 0
		_RippleScale("Ripple Scale", float) = 40
		_RippleSpeed("Ripple Speed", float) = 10
		_RippleStr("Ripple Strength", float) = 1
		
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

		// Unused in this variant
		[ToggleUI]_TessellationOffsetMask("Vertex Offset Mask", Int) = 1
		_TessMin("Min Tessellation Factor", Float) = 1
		_TessMax("Max Tessellation Factor", Float) = 9
		_TessDistMin("Min Tessellation Distance", Float) = 25
		_TessDistMax("Max Tessellation Distance", Float) = 50

		[IntRange]_StencilRef("Stencil Reference", Range(1,255)) = 65
		[Enum(Opaque,0, Premultiplied,1, Grabpass,2)]_TransparencyMode("Transparency Mode", Int) = 2
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull", Int) = 2
		[Enum(Off,0, On,1)]_ZWrite("ZWrite", Int) = 0
		[Enum(Off,0, On,1)]_DepthEffects("Depth Effects", Int) = 1
		
		[HideInInspector]_SrcBlend("__src", Float) = 1.0
        [HideInInspector]_DstBlend("__dst", Float) = 0.0
		[HideInInspector]_NoiseTexSSR("SSR Noise Tex", 2D) = "black"
    }

    SubShader {
        Tags {
			"Queue"="Transparent" 
			"RenderType"="Transparent"
			"DisableBatching"="True"
			"PreviewType"="Plane"
			// "ForceNoShadowCasting"="True"
			"IgnoreProjector"="True"
		}
		Stencil {
			Ref [_StencilRef]
			Comp Always
			Pass Replace
			Fail Keep
			ZFail Keep
		}
		GrabPass {
			Tags {"LightMode"="Always"}
			"_MWGrab"
		}
		ZWrite [_ZWrite]
		Cull [_CullMode]
        Pass {
			Tags {"LightMode"="ForwardBase"}
			Blend [_SrcBlend] [_DstBlend]
            CGPROGRAM
			#pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma shader_feature_local _SPECULAR_ON
			#pragma shader_feature_local _SPECULAR_MANUAL_ON
			#pragma shader_feature_local _REFLECTIONS_ON
			#pragma shader_feature_local _REFLECTIONS_MANUAL_ON
			#pragma shader_feature_local _SCREENSPACE_REFLECTIONS_ON
			#pragma shader_feature_local _NORMALMAP_1_ON
			#pragma shader_feature_local _DETAIL_NORMAL_ON
			#pragma shader_feature_local _DETAIL_BASECOLOR_ON
			#pragma shader_feature_local _FLOW_ON
			#pragma shader_feature_local _ _NOISE_TEXTURE_ON _GERSTNER_WAVES_ON _VORONOI_ON
			#pragma shader_feature_local _DEPTHFOG_ON
			#pragma shader_feature_local _FOAM_ON
			#pragma shader_feature_local _CAUSTICS_ON
			#pragma shader_feature_local _EDGEFADE_ON
			#pragma shader_feature_local _NORMALMAP_0_STOCHASTIC_ON
			#pragma shader_feature_local _NORMALMAP_1_STOCHASTIC_ON
			#pragma shader_feature_local _FOAM_STOCHASTIC_ON
			#pragma shader_feature_local _BASECOLOR_STOCHASTIC_ON
			#pragma shader_feature_local _RAIN_ON
			#pragma shader_feature_local _FOAM_NORMALS_ON
			#pragma shader_feature_local _DEPTH_EFFECTS_ON
			#pragma shader_feature_local _EMISSION_ON
			#pragma shader_feature_local _OPAQUELIGHTS_OFF
			#pragma shader_feature_local _AREALIT_ON
			#pragma shader_feature_local _ _OPAQUE_MODE_ON _PREMUL_MODE_ON
			#pragma multi_compile_instancing
            #pragma target 5.0

			#include "WaterDefines.cginc"
			#include "WaterVert.cginc"
			#include "WaterFrag.cginc"

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
			#pragma shader_feature_local _SPECULAR_ON 
			#pragma shader_feature_local _SPECULAR_MANUAL_ON
			#pragma shader_feature_local _NORMALMAP_1_ON
			#pragma shader_feature_local _DETAIL_NORMAL_ON
			#pragma shader_feature_local _DETAIL_BASECOLOR_ON
			#pragma shader_feature_local _FLOW_ON
			#pragma shader_feature_local _ _NOISE_TEXTURE_ON _GERSTNER_WAVES_ON _VORONOI_ON
			#pragma shader_feature_local _DEPTHFOG_ON
			#pragma shader_feature_local _FOAM_ON
			#pragma shader_feature_local _CAUSTICS_ON
			#pragma shader_feature_local _EDGEFADE_ON
			#pragma shader_feature_local _NORMALMAP_0_STOCHASTIC_ON
			#pragma shader_feature_local _NORMALMAP_1_STOCHASTIC_ON
			#pragma shader_feature_local _FOAM_STOCHASTIC_ON
			#pragma shader_feature_local _BASECOLOR_STOCHASTIC_ON
			#pragma shader_feature_local _RAIN_ON
			#pragma shader_feature_local _FOAM_NORMALS_ON
			#pragma shader_feature_local _DEPTH_EFFECTS_ON
			#pragma shader_feature_local _EMISSION_ON
			#pragma shader_feature_local _ _OPAQUE_MODE_ON _PREMUL_MODE_ON
			#pragma multi_compile_instancing
            #pragma target 5.0

			#include "WaterDefines.cginc"
			#include "WaterVert.cginc"
			#include "WaterFrag.cginc"

            ENDCG
        }

		Pass {
            Tags {"LightMode" = "ShadowCaster"}
            CGPROGRAM
			#pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
			#pragma shader_feature_local _ _NOISE_TEXTURE_ON _GERSTNER_WAVES_ON _VORONOI_ON
			#pragma shader_feature_local _ _OPAQUE_MODE_ON _PREMUL_MODE_ON
			#pragma multi_compile_instancing
            #pragma target 5.0

			#include "WaterDefines.cginc"
			#include "WaterVert.cginc"
			#include "WaterFrag.cginc"

            ENDCG
        }
    }
	CustomEditor "WaterEditor"
}