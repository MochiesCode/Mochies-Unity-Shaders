Shader "Mochie/Glass (Two Pass)" {
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
        [Enum(Low,0, Medium,1, High,2, Ultra,3)]_BlurQuality("Blur Quality", Int) = 1
        _Blur("Blur Strength", Float) = 1
        _Refraction("Refraction Strength", Float) = 5
        [ToggleUI]_RefractVertexNormal("Refract Mesh Normals", Int) = 0
        _RefractionIOR("IOR", Float) = 1.2

        // [Toggle(_RAIN_ON)]_RainToggle("Enable", Int) = 0
        [Enum(Off,0, Droplets,1, Ripples,2, Automatic,3)]_RainMode("Mode", Int) = 0
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
        _RainMask("Mask", 2D) = "white" {}
        [Enum(Red,0, Green,1, Blue,2, Alpha,0)]_RainMaskChannel("Channel", Int) = 0
        _DropletMask("Rain Droplet Mask", 2D) = "white" {}
        _DynamicDroplets("Droplet Strength", Range(0,1)) = 0.5
        _RainBias("Rain Bias", Float) = -1
        _Test("Test", Float) = 1

        [ToggleUI]_ReflectionsToggle("Reflections", Int) = 1
        _ReflectionStrength("Reflection Strength", Float) = 1
        [ToggleUI]_SpecularToggle("Specular Highlights", Int) = 1
        _SpecularStrength("Specular Strength", Float) = 1
        [ToggleUI]_GSAAToggle("Specular Antialiasing", Int) = 0
        _GSAAStrength("Specular Antialiasing Strength", Float) = 1
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

        [ToggleUI]_LTCGI("LTCGI", Int) = 0
        _LTCGI_mat("LTC Mat", 2D) = "black" {}
        _LTCGI_amp("LTC Amp", 2D) = "black" {}
        _LTCGIStrength("LTCGI Strength", Float) = 1
        _LTCGIRoughness("LTCGI Roughness", Float) = 1
        _LTCGI_DiffuseColor ("LTCGI Diffuse Color", Color) = (1,1,1,1)
        _LTCGI_SpecularColor ("LTCGI Specular Color", Color) = (1,1,1,1)
    }

    CGINCLUDE
        #define TWO_PASS_TRANSPARENCY
    ENDCG

    SubShader {
        Tags { 
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "ForceNoShadowCaster"="True"
            "IgnoreProjector"="True"
            "LTCGI"="_LTCGI"
        }
        Blend [_SrcBlend] [_DstBlend]
        ZWrite Off

        // Front Face Passes
        GrabPass {
            Tags {"LightMode"="Always"}
            "_GlassGrabOne"
        }
        Pass {
            Name "ForwardBase"
            Tags {"LightMode"="ForwardBase"}
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma shader_feature_local _RAIN_ON
            #pragma shader_feature_local _ _BLURQUALITY_ULTRA _BLURQUALITY_HIGH _BLURQUALITY_MED _BLURQUALITY_LOW
            #pragma shader_feature_local _REFLECTIONS_ON
            #pragma shader_feature_local _SPECULAR_HIGHLIGHTS_ON
            #pragma shader_feature_local _GRABPASS_ON
            #pragma shader_feature_local _LIT_BASECOLOR_ON
            #pragma shader_feature_local _STOCHASTIC_SAMPLING_ON
            #pragma shader_feature_local _NORMALMAP_ON
            #pragma shader_feature_local _ _RAINMODE_RIPPLE _RAINMODE_AUTO 
            #pragma shader_feature_local _AREALIT_ON
            #pragma shader_feature_local LTCGI
            #pragma target 5.0
            #define _GlassGrab _GlassGrabOne
            #include "GlassDefines.cginc"
            #include "GlassPass.cginc"

            ENDCG
        }

        Pass {
            Name "ForwardAdd"
            Tags {"LightMode"="ForwardAdd"}
            Blend One One
            Cull Front
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

        // Back face passes
        GrabPass {
            Tags {"LightMode"="Always"}
            "_GlassGrabTwo"
        }
        Pass {
            Name "ForwardBase"
            Tags {"LightMode"="ForwardBase"}
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma shader_feature_local _RAIN_ON
            #pragma shader_feature_local _ _BLURQUALITY_ULTRA _BLURQUALITY_HIGH _BLURQUALITY_MED _BLURQUALITY_LOW
            #pragma shader_feature_local _REFLECTIONS_ON
            #pragma shader_feature_local _SPECULAR_HIGHLIGHTS_ON
            #pragma shader_feature_local _GRABPASS_ON
            #pragma shader_feature_local _LIT_BASECOLOR_ON
            #pragma shader_feature_local _STOCHASTIC_SAMPLING_ON
            #pragma shader_feature_local _NORMALMAP_ON
            #pragma shader_feature_local _ _RAINMODE_RIPPLE _RAINMODE_AUTO 
            #pragma shader_feature_local _AREALIT_ON
            #pragma shader_feature_local LTCGI
            #pragma target 5.0
            
            #define _GlassGrab _GlassGrabTwo
            #include "GlassDefines.cginc"
            #include "GlassPass.cginc"

            ENDCG
        }

        Pass {
            Name "ForwardAdd"
            Tags {"LightMode"="ForwardAdd"}
            Blend One One
            Cull Back
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
            
            #define _GlassGrab _GlassGrabTwo
            #include "GlassDefines.cginc"
            #include "GlassPass.cginc"

            ENDCG
        }
    }
    CustomEditor "Mochie.GlassEditor"
}