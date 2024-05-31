Shader "Mochie/Glass" {
    Properties {

        _GrabpassTint("Grabpass Tint", Color) = (1,1,1,1)
        _SpecularityTint("Specularity Tint", Color) = (1,1,1,1)
		_BaseColorTint("Base Color Tint", Color) = (1,1,1,1)

        _BaseColor("Base Color", 2D) = "black" {}
		_RoughnessMap("Roughness Map", 2D) = "white" {}
        _MetallicMap("Metallic Map", 2D) = "white" {}
        _OcclusionMap("Occlusion Map", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _Roughness("Roughness", Range(0,1)) = 0
        _Metallic("Metallic", Range(0,1)) = 0
		_Occlusion("Occlusion", Range(0,1)) = 1
        _NormalStrength("Normal Strength", Float) = 1
        [KeywordEnum(ULTRA, HIGH, MED, LOW)]BlurQuality("Blur Quality", Int) = 1
		_Blur("Blur Strength", Float) = 1
        _Refraction("Refraction Strength", Float) = 5
        [ToggleUI]_RefractMeshNormals("Refract Mesh Normals", Int) = 0

        [Toggle(_RAIN_ON)]_RainToggle("Enable", Int) = 0
		[HideInInspector]_RainSheet("Texture Sheet", 2D) = "black" {}
		[HideInInspector]_Rows("Rows", Float) = 8
		[HideInInspector]_Columns("Columns", Float) = 8
		_Speed("Speed", Float) = 60
		_XScale("X Scale", Float) = 1.5
        _YScale("Y Scale", Float) = 1.5
		_Strength("Normal Strength", Float) = 0.3
		_RippleScale("Ripple Scale", Float) = 10
		_RippleSpeed("Ripple Speed", Float) = 10
		_RippleStrength("Ripple Strength", Float) = 1
        _RippleSize("Ripple Size", Range(2,10)) = 6
        _RippleDensity("Ripple Density", Float) = 1.57079632679
        _RainThreshold("Threshold", Range(0,1)) = 0.01
        _RainThresholdSize("Threshold Size", Range(0,1)) = 0.01
        [Enum(Droplets,0, Ripples,1, Automatic,2)]_RainMode("Mode", Int) = 0
        _RainMask("Mask", 2D) = "white" {}
        [Enum(Red,0, Green,1, Blue,2, Alpha,0)]_RainMaskChannel("Channel", Int) = 0
        _DropletMask("Rain Droplet Mask", 2D) = "white" {}
        _DynamicDroplets("Droplet Strength", Range(0,1)) = 0.5
        _RainBias("Rain Bias", Float) = -1
        _Test("Test", Float) = 1

        [Toggle(_REFLECTIONS_ON)]_ReflectionsToggle("Reflections", Int) = 1
        [Toggle(_SPECULAR_HIGHLIGHTS_ON)]_SpecularToggle("Specular Highlights", Int) = 1
        [Toggle(_LIT_BASECOLOR_ON)]_LitBaseColor("Lit Base Color", Int) = 1
        [Enum(Default,0, Stochastic,1)]_SamplingMode("Sampling Mode", Int) = 0
		[Enum(UnityEngine.Rendering.CullMode)]_Culling("Culling", Int) = 2
        [Enum(Grabpass,0, Premultiplied,1, Opaque,2)]_BlendMode("Transparency", Int) = 0
        [Enum(UV,0, World,1)]_TexCoordSpace("Texture Coordinate Space", Int) = 0
		[Enum(XY,0, XZ,1, YZ,2)]_TexCoordSpaceSwizzle("Swizzle", Int) = 0
        _GlobalTexCoordScale("Global Scale", Float) = 1
        [HideInInspector]_SrcBlend("Src Blend", Int) = 1
        [HideInInspector]_DstBlend("Dst Blend", Int) = 0
        [HideInInspector]_ZWrite("Z Write", Int) = 0
        [HideInInspector]_MaterialResetCheck("Reset", Int) = 0
        _QueueOffset("Queue Offset", Int) = 0
    }
    SubShader {
        Tags { 
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "ForceNoShadowCaster"="True"
            "IgnoreProjector"="True"
        }
        GrabPass {
            Tags {"LightMode"="Always"}
            "_GlassGrab"
        }
        Cull [_Culling]
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]

        Pass {
            Name "ForwardBase"
            Tags {"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma shader_feature_local _RAIN_ON
            #pragma shader_feature_local _ BLURQUALITY_ULTRA BLURQUALITY_HIGH BLURQUALITY_MED BLURQUALITY_LOW
            #pragma shader_feature_local _REFLECTIONS_ON
            #pragma shader_feature_local _SPECULAR_HIGHLIGHTS_ON
            #pragma shader_feature_local _GRABPASS_ON
            #pragma shader_feature_local _LIT_BASECOLOR_ON
            #pragma shader_feature_local _STOCHASTIC_SAMPLING_ON
            #pragma shader_feature_local _NORMALMAP_ON
            #pragma shader_feature_local _ _RAINMODE_RIPPLE _RAINMODE_AUTO 
            #pragma target 5.0

            #include "GlassDefines.cginc"
            #include "GlassPass.cginc"

            ENDCG
        }

        Pass {
            Name "ForwardAdd"
            Tags {"LightMode"="ForwardAdd"}
            Blend One One
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_fwdadd fullshadows
            #pragma shader_feature_local _RAIN_ON
            #pragma shader_feature_local _SPECULAR_HIGHLIGHTS_ON
            #pragma shader_feature_local _LIT_BASECOLOR_ON
            #pragma shader_feature_local _STOCHASTIC_SAMPLING_ON
            #pragma shader_feature_local _NORMALMAP_ON
            #pragma shader_feature_local _ _RAINMODE_RIPPLE _RAINMODE_AUTO 
            #pragma target 5.0

            #include "GlassDefines.cginc"
            #include "GlassPass.cginc"

            ENDCG
        }
    }
    CustomEditor "GlassEditor"
}