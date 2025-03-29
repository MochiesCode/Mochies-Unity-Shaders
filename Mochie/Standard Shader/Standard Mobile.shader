// By Mochie
// https://github.com/MochiesCode/Mochies-Unity-Shaders

Shader "Mochie/Standard Mobile" {
    Properties {
        
        // Variant
        [Enum(Opaque,0, Cutout,1, Fade,2, Transparent,3)]_BlendMode("Blending Mode", Int) = 0
        [Enum(Base Color Alpha,0, Alpha Mask,1)]_AlphaSource("Alpha Source", Int) = 0
        [Enum(Off,0, On,1)]_MipMapRescaling("Mip Map Rescaling", Int) = 1
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        _MipMapScale("Mip Map Scale", Float) = 0.25
        [Enum(Off,0, On,1)]_SmoothnessToggle("Use Smoothness", Int) = 0
        [Enum(Local,0, World,1)]_TriplanarCoordSpace("Triplanar Coordinate Space", Int) = 0

        // Primary Textures
        [Enum(Separate,0, Packed,1)]_PrimaryWorkflow("Primary Workflow", Int) = 0
        [Enum(Default,0, Stochastic,1, Supersampling,2, Triplanar,3)]_PrimarySampleMode("Primary Sampling Mode", Int) = 0
        _MainTex("Base Color", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0,1)) = 1
        _SampleMetallic("Sample Metallic Map", Int) = 0
        _MetallicMap("Metallic Map", 2D) = "white" {}
        _MetallicStrength("Metallic Strength", Range(0,1)) = 0
        _SampleRoughness("Sample Roughness Map", Int) = 0
        _RoughnessMap("Roughness Map", 2D) = "white" {}
        _RoughnessStrength("Roughness Strength", Range(0,1)) = 1
        _SampleOcclusion("Sample Occlusion Map", Int) = 0
        _OcclusionMap("Occlusion Map", 2D) = "white" {}
        _OcclusionStrength("Occlusion Strength", Range(0,1)) = 1
        _HeightMap("Height Map", 2D) = "black" {}
        _HeightStrength("Height Strength", Range(0,0.2)) = 0.02
        _HeightOffset("Height Offset", Range(-1, 1)) = 0
        [IntRange]_HeightSteps("Height Steps", Range(1,16)) = 8

        _PackedMap("Packed Map", 2D) = "white" {}
        [Enum(Off,0, On,1)]_PackedHeight("Packed Height", Int) = 0
        _PackedRoughnessStrength("Packed Roughness Strength", Range(0,1)) = 1
        _PackedMetallicStrength("Packed Metallic Strength", Range(0,1)) = 1
        _PackedOcclusionStrength("Packed Occlusion Strength", Range(0,1)) = 1
        [Enum(Red,0, Green,1, Blue,2, Alpha,3)]_RoughnessChannel("Roughness Channel", Int) = 1
        [Enum(Red,0, Green,1, Blue,2, Alpha,3)]_MetallicChannel("Metallic Channel", Int) = 2
        [Enum(Red,0, Green,1, Blue,2, Alpha,3)]_OcclusionChannel("Occlusion Channel", Int) = 0
        [Enum(Red,0, Green,1, Blue,2, Alpha,3)]_HeightChannel("Height Channel", Int) = 3

        [Enum(UV0,0, UV1,1, UV2,2, UV3,3, UV4,4, World,5, Local,6)]_UVMainSet("UV Set for primary textures", Int) = 0
        [Enum(XY,0, XZ,1, YZ,2)]_UVMainSwizzle("Swizzle", Int) = 0
        _UVMainScroll("Scroll Speed", Vector) = (0,0,0,0)
        _UVMainRotation("Rotation", Float) = 0

        // Emission/Audiolink
        _EmissionMap("Emission Map", 2D) = "white" {}
        _EmissionStrength("Emission Strength", Float) = 1
        [HDR]_EmissionColor("Emission Color", Color) = (0,0,0)
        [Enum(Off,0, Sin,1, Square,2, Triangle,3, Saw,4, Reverse Saw,5)]_EmissionPulseWave("Pulse", Int) = 0
        _EmissionPulseSpeed("Pulse Speed", Float) = 1
        _EmissionPulseStrength("Pulse Strength", Float) = 1
        [Enum(Off,0, Bass,1, Low Mids,2, Upper Mids,3, Highs,4)]_AudioLinkEmission("Emission Band", Int) = 0
        _AudioLinkEmissionStrength("Emission Strength", Float) = 1
        [ToggleUI]_AudioLinkEmissionMeta("Audio Link Meta Emission", Int) = 0
        _AudioLinkMin("Audio Link Min", Float) = 0
        _AudioLinkMax("Audio Link Max", Float) = 1

        // Detail Textures
        [Enum(Separate,0, Packed,1)]_DetailWorkflow("Detail Workflow", Int) = 0
        [Enum(Default,0, Stochastic,1, Supersampling,2, Triplanar,3)]_DetailSampleMode("Detail Sampling Mode", Int) = 0
        _DetailMainTex("Base Color", 2D) = "white" {}
        _DetailColor("Color", Color) = (1,1,1,1)
        _DetailMainTexStrength("Detail Base Color Strength", Range(0,1)) = 1
        [Enum(Add,0, Alpha,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_DetailMainTexBlend("Detail Base Color Blend", Int) = 2
        _DetailNormalMap("Normal Map", 2D) = "bump" {}
        _DetailNormalStrength("Normal Strength", Float) = 1
        _DetailMetallicMap("Metallic Map", 2D) = "white" {}
        _DetailMetallicStrength("Metallic Strength", Range(0,1)) = 0
        [Enum(Add,0, Alpha,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_DetailMetallicBlend("Detail Metallic Blend", Int) = 2
        _DetailRoughnessMap("Roughness Map", 2D) = "white" {}
        _DetailRoughnessStrength("Roughness Strength", Range(0,1)) = 1
        [Enum(Add,0, Alpha,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_DetailRoughnessBlend("Detail Roughness Blend", Int) = 2
        _DetailOcclusionMap("Occlusion Map", 2D) = "white" {}
        _DetailOcclusionStrength("Occlusion Strength", Range(0,1)) = 1
        [Enum(Add,0, Alpha,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_DetailOcclusionBlend("Detail Occlusion Blend", Int) = 2

        _DetailPackedMap("Packed Map", 2D) = "white" {}
        [ToggleUI]_DetailRoughnessMultiplier("Roughness Multiplier", Int) = 0
        [ToggleUI]_DetailMetallicMultiplier("Metallic Multiplier", Int) = 0
        [ToggleUI]_DetailOcclusionMultiplier("Occlusion Multiplier", Int) = 0
        [Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DetailRoughnessChannel("Roughness Channel", Int) = 1
        [Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DetailMetallicChannel("Metallic Channel", Int) = 2
        [Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DetailOcclusionChannel("Occlusion Channel", Int) = 0

        [Enum(UV0,0, UV1,1, UV2,2, UV3,3, UV4,4, World,5, Local,6)]_UVDetailSet("UV Set for detail textures", Int) = 0
        [Enum(XY,0, XZ,1, YZ,2)]_UVDetailSwizzle("Swizzle", Int) = 0
        _UVDetailScroll("Scroll Speed", Vector) = (0,0,0,0)
        _UVDetailRotation("Rotation", Float) = 0
        
        // Independant Textures
        _DetailMask("Detail Mask", 2D) = "white" {}
        [Enum(Red,0, Green,1, Blue,2, Alpha,3)]_DetailMaskChannel("Detail Mask Channel", Int) = 0
        [Enum(UV0,0, UV1,1, UV2,2, UV3,3, UV4,4, World,5, Local,6)]_UVDetailMaskSet("UV Set for detail mask", Int) = 0
        [Enum(XY,0, XZ,1, YZ,2)]_UVDetailMaskSwizzle("Swizzle", Int) = 0
        _UVDetailMaskScroll("Scroll Speed", Vector) = (0,0,0,0)
        _UVDetailMaskRotation("Rotation", Float) = 0

        _HeightMask("Height Mask", 2D) = "white" {}
        [Enum(Red,0, Green,1, Blue,2, Alpha,3)]_HeightMaskChannel("Height Mask Channel", Int) = 0
        [Enum(UV0,0, UV1,1, UV2,2, UV3,3, UV4,4, World,5, Local,6)]_UVHeightMaskSet("UV Set for Height mask", Int) = 0
        [Enum(XY,0, XZ,1, YZ,2)]_UVHeightMaskSwizzle("Swizzle", Int) = 0
        _UVHeightMaskScroll("Scroll Speed", Vector) = (0,0,0,0)
        _UVHeightMaskRotation("Rotation", Float) = 0

        _RainMask("Rain Mask", 2D) = "white" {}
        [Enum(Red,0, Green,1, Blue,2, Alpha,3)]_RainMaskChannel("Rain Mask Channel", Int) = 0
        [Enum(UV0,0, UV1,1, UV2,2, UV3,3, UV4,4, World,5, Local,6)]_UVRainMaskSet("UV Set for Rain mask", Int) = 0
        [Enum(XY,0, XZ,1, YZ,2)]_UVRainMaskSwizzle("Swizzle", Int) = 0
        _UVRainMaskScroll("Scroll Speed", Vector) = (0,0,0,0)
        _UVRainMaskRotation("Rotation", Float) = 0

        _EmissionMask("Mask", 2D) = "white" {}
        [Enum(Red,0, Green,1, Blue,2, Alpha,3)]_EmissionMaskChannel("Emission Mask Channel", Int) = 0
        [Enum(UV0,0, UV1,1, UV2,2, UV3,3, UV4,4, World,5, Local,6)]_UVEmissionMaskSet("UV Set for Emission mask", Int) = 0
        [Enum(XY,0, XZ,1, YZ,2)]_UVEmissionMaskSwizzle("Swizzle", Int) = 0
        _UVEmissionMaskScroll("Scroll Speed", Vector) = (0,0,0,0)
        _UVEmissionMaskRotation("Rotation", Float) = 0

        _AlphaMask("Alpha Mask", 2D) = "white" {}
        [Enum(Red,0, Green,1, Blue,2, Alpha,3)]_AlphaMaskChannel("Alpha Mask Channel", Int) = 0
        [Enum(UV0,0, UV1,1, UV2,2, UV3,3, UV4,4, World,5, Local,6)]_UVAlphaMaskSet("UV Set for Alpha mask", Int) = 0
        [Enum(XY,0, XZ,1, YZ,2)]_UVAlphaMaskSwizzle("Swizzle", Int) = 0
        _UVAlphaMaskScroll("Scroll Speed", Vector) = (0,0,0,0)
        _UVAlphaMaskRotation("Rotation", Float) = 0
        
        // Specularity
        [Enum(Unity Standard,0, Google Filament,1)]_ShadingModel("Specular Model", Int) = 0
        [ToggleUI]_ReflectionsToggle("Reflections", Int) = 1
        [ToggleUI]_SpecularHighlightsToggle("Specular Highlights", Int) = 1
        _ReflectionStrength("Reflection Strength", Float) = 1
        _SpecularHighlightStrength("Specular Strength", Float) = 1
        [ToggleUI]_FresnelToggle("Fresnel", Int) = 1
        _FresnelStrength("Fresnel Strength", Float) = 1
        [ToggleUI]_SpecularOcclusionToggle("Specular Occlusion", Int) = 0
        _SpecularOcclusionStrength("Specular Occlusion Strength", Float) = 1
        _SpecularOcclusionContrast("Contrast", Float) = 1
        _SpecularOcclusionBrightness("Brightness", Float) = 20
        _SpecularOcclusionHDR("HDR", Float) = 1
        _SpecularOcclusionTint("Tint", Color) = (1,1,1,1)
        [ToggleUI]_SSRToggle("Screenspace Reflections", Int) = 0
        [ToggleUI]_VRSSR("Enable in VR", Int) = 1
        _SSRStrength("SSR Strength", Float) = 1
        _SSREdgeFade("Edge Fade", Range(0,1)) = 0.1
        _SSRHeight("SSR Height", Float) = 0.2
        _ContactHardening("Contact Hardening", Range(0,1)) = 0
        [ToggleUI]_GSAAToggle("GSAA", Int) = 0
        _GSAAStrength("GSAA Strength", Float) = 1
        _IndirectSpecularOcclusionStrength("Baked Spec Occlusion Strength", Range(0,1)) = 0.2
        _RealtimeSpecularOcclusionStrength("Realtime Spec Occlusion Strength", Range(0,1)) = 0

        // Subsurface Scattering
        [ToggleUI]_Subsurface("Subsurface Scattering", Int) = 0
        [ToggleUI]_ScatterBaseColorTint("Scatter Base Color Tint", Int) = 0
        _ThicknessMap("Thickness Map", 2D) = "black" {}
        _ThicknessMapPower("Thickness Map Power", Float) = 1
        _ScatterCol("Subsurface Color", Color) = (1,1,1,1)
        _ScatterIntensity("Intensity", Range(0,10)) = 1
        _ScatterPow("Power", Range(0.01,10)) = 1
        _ScatterDist("Distance", Range(0,10)) = 1
        _ScatterAmbient("Ambient Intensity", Range(0,0.5)) = 0
        _ScatterShadow("Shadow Power", Range(0,1)) = 1
        _WrappingFactor("Wrapping Factor", Range(0.001, 1)) = 0.01

        // Rain
        [Enum(Off,0, Droplets,1, Ripples,2, Automatic,3)]_RainMode("Mode", Int) = 0
        [Enum(UV0,0, UV1,1, UV2,2, UV3,3, UV4,4, World,5, Local,6)]_UVRainSet("Rain UVs", Float) = 0
        [Enum(XY,0, XZ,1, YZ,2)]_UVRainSwizzle("Swizzle", Int) = 0
        _UVRainRotation("Rotation", Float) = 0
        [Enum(UV0,0, UV1,1, UV2,2, UV3,3, UV4,4, World,5, Local,6)]_UVRippleSet("Ripple UVs", Float) = 0
        [Enum(XY,0, XZ,1, YZ,2)]_UVRippleSwizzle("Swizzle", Int) = 0
        _UVRippleRotation("Rotation", Float) = 0
        [HideInInspector]_RainSheet("Rain Texture Sheet", 2D) = "black" {}
        [HideInInspector]_RainRows("Rows", Float) = 8
        [HideInInspector]_RainColumns("Columns", Float) = 8
        _RainSpeed("Speed", Float) = 60
        _RainScale("Rain Scale", Vector) = (1,1,0,0)
        _RainStrength("Normal Strength", Float) = 0.3
        _DropletMask("Rain Droplet Mask", 2D) = "white" {}
        _DynamicDroplets("Droplet Strength", Range(0,1)) = 0.5
        _RainThreshold("Threshold", Range(0,1)) = 0.01
        _RainThresholdSize("Threshold Size", Range(0,1)) = 0.01
        _RippleScale("Ripple Scale", Vector) = (40,40,0,0)
        _RippleSpeed("Ripple Speed", Float) = 18
        _RippleStrength("Ripple Strength", Float) = 1
        _RippleDensity("Ripple Density", Float) = 0.3
        _RippleSize("Ripple Size", Float) = 2


        // Filtering
        [ToggleUI]_Filtering("Filtering", Int) = 0
        [Enum(HSV,0, Oklab,1)]_HueMode("Hue Mode", Int) = 0
        [ToggleUI]_MonoTint("Mono Tint", Int) = 0
        _Hue("Hue", Range(0,1)) = 0
        _Contrast("Contrast", Float) = 1
        _Saturation("Saturation", Float) = 1
        _Brightness("Brightness", Float) = 1

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
        _ACES("ACES", Float) = 0
        _ColorGradingLUT("Color Grading LUT", 2D) = "white" {}
        _ColorGradingLUTStrength("LUT Strength", Range(0,2)) = 1 

        // Lightmapping stuff
        [ToggleUI] _BAKERY_LMSPEC ("Lightmap Specular", Float) = 0
        _BakeryLMSpecStrength("Lightmap Specular Strength", Float) = 1
        [ToggleUI] _BAKERY_SHNONLINEAR ("Non-Linear SH", Float) = 0
        [ToggleUI]_BicubicSampling("Bicubic Sampling", Int) = 1
        [ToggleUI]_ApplyHeightOffset("Apply Height Offset", Int) = 0
        [Enum(None, 0, SH, 1, RNM, 2, MONOSH, 3)] _BakeryMode ("Bakery Mode", Int) = 0
        [ToggleUI]_IgnoreRealtimeGI("Ignore Realtime GI", Int) = 0
        _RNM0("RNM0", 2D) = "black" {}
        _RNM1("RNM1", 2D) = "black" {}
        _RNM2("RNM2", 2D) = "black" {}

        // AreaLit
        [Toggle(_AREALIT_ON)]_AreaLitToggle("Enable", Int) = 0
        _AreaLitStrength("Strength", Float) = 1
        _AreaLitRoughnessMultiplier("Roughness Multiplier", Float) = 1
        _AreaLitSpecularOcclusion("Apply Specular Occlusion", Float) = 0
        [NoScaleOffset]_LightMesh("Light Mesh", 2D) = "black" {}
        [NoScaleOffset]_LightTex0("Light Texture 0", 2D) = "white" {}
        [NoScaleOffset]_LightTex1("Light Texture 1", 2D) = "black" {}
        [NoScaleOffset]_LightTex2("Light Texture 2", 2D) = "black" {}
        [NoScaleOffset]_LightTex3("Light Texture 3", 2DArray) = "black" {}
        [ToggleOff]_OpaqueLights("Opaque Lights", Float) = 1.0
        _AreaLitOcclusion("Occlusion", 2D) = "white" {}
        [Enum(UV0,0,UV1,1, UV2,2, UV3,3, UV4,4, Lightmap UV,5)]_AreaLitOcclusionUVSet("UV Set for occlusion map", Float) = 0
        
        // LTCGI
        [Toggle(LTCGI)]_LTCGI("LTCGI", Int) = 0
        _LTCGI_mat("LTC Mat", 2D) = "black" {}
        _LTCGI_amp("LTC Amp", 2D) = "black" {}
        _LTCGIStrength("LTCGI Strength", Float) = 1
        _LTCGIRoughness("LTCGI Roughness", Float) = 1
        _LTCGISpecularOcclusion("Apply Specular Occlusion", Float) = 0
        _LTCGI_DiffuseColor ("LTCGI Diffuse Color", Color) = (1,1,1,1)
        _LTCGI_SpecularColor ("LTCGI Specular Color", Color) = (1,1,1,1)
        
        // Render Settings
        [Enum(UnityEngine.Rendering.CullMode)]_Culling("Cull", Int) = 2
        _QueueOffset("Queue Offset", Int) = 0
        [ToggleUI]_UnityFogToggle("Unity Fog", Int) = 1
        [ToggleUI]_VertexBaseColor("Vertex Base Color", Int) = 0
        
        // Debug
        [ToggleUI]_DebugEnable("Enable Debug View", Int) = 0
        [ToggleUI]_DebugVertexColors("Vertex Colors", Int) = 0
        [ToggleUI]_DebugBaseColor("Base Color", Int) = 1
        [ToggleUI]_DebugNormals("Normals", Int) = 0
        [ToggleUI]_DebugRoughness("Roughness", Int) = 0
        [ToggleUI]_DebugMetallic("Metallic", Int) = 0
        [ToggleUI]_DebugOcclusion("Occlusion", Int) = 0
        [ToggleUI]_DebugHeight("Height", Int) = 0
        [ToggleUI]_DebugAtten("Attenuation", Int) = 0
        [ToggleUI]_DebugReflections("Reflections", Int) = 0
        [ToggleUI]_DebugSpecular("Specular Highlights", Int) = 0
        [ToggleUI]_DebugAlpha("Alpha", Int) = 0
        [ToggleUI]_DebugLighting("Alpha", Int) = 0

        // NO TOUCHY
        [HideInInspector]_SrcBlend("__src", Float) = 1.0
        [HideInInspector]_DstBlend("__dst", Float) = 0.0
        [HideInInspector]_ZWrite("__zw", Float) = 1.0
        [HideInInspector]_ZTest("__zt", Float) = 4.0
        [HideInInspector]_MaterialResetCheck("Reset Check", Int) = 0
        [HideInInspector]_NoiseTexSSR("SSR Noise Tex", 2D) = "black"
        [HideInInspector]_DefaultSampler("DO NOT REMOVE THIS", 2D) = "white" {}
        [HideInInspector]_DFG("LUT for Filament SM", 2D) = "white" {}
        [HideInInspector]_AlphaToMask("Alpha Coverage", Int) = 0
        [HideInInspector]_SampleCustomLUT("Sample Custom LUT?", Int) = 0
        [ToggleUI]_MaterialDebugMode("Debug Mode", Int) = 0
    }

    CGINCLUDE
        #define STANDARD_MOBILE
    ENDCG

    SubShader {
        Tags {
            "RenderType"="Opaque"
            "RenderQueue"="Geometry"
            "PerformanceChecks"="False"
            "LTCGI"="_LTCGI"
        }
        Cull [_Culling]
        ZTest [_ZTest]
        AlphaToMask [_AlphaToMask]
        
        Pass {
            Name "FORWARD"
            Tags {"LightMode" = "ForwardBase"}
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _REFLECTIONS_ON
            #pragma shader_feature_local _SPECULAR_HIGHLIGHTS_ON
            #pragma shader_feature_local _WORKFLOW_PACKED_ON
            #pragma shader_feature_local _EMISSION_ON
            #pragma shader_feature_local _NORMALMAP_ON
            #pragma shader_feature_local _DETAIL_MAINTEX_ON
            #pragma shader_feature_local _DETAIL_NORMAL_ON
            #pragma shader_feature_local _AREALIT_ON
            #pragma shader_feature_local _OPAQUELIGHTS_OFF
            #pragma shader_feature_local _AUDIOLINK_ON
            #pragma shader_feature_local _BICUBIC_SAMPLING_ON
            #pragma shader_feature_local LTCGI
            #pragma shader_feature_local BAKERY_LMSPEC
            #pragma shader_feature_local BAKERY_SHNONLINEAR
            #pragma shader_feature_local _ BAKERY_SH BAKERY_RNM BAKERY_MONOSH
            // #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing
            #define BASE_PASS
            #include "StandardDefines.cginc"

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
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _SPECULAR_HIGHLIGHTS_ON
            #pragma shader_feature_local _WORKFLOW_PACKED_ON
            #pragma shader_feature_local _NORMALMAP_ON
            #pragma shader_feature_local _DETAIL_MAINTEX_ON
            #pragma shader_feature_local _DETAIL_NORMAL_ON
            // #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fog
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_instancing
            #define ADD_PASS
            #include "StandardDefines.cginc"

            ENDCG
        }
        
        Pass {
            Tags {"LightMode" = "ShadowCaster"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _WORKFLOW_PACKED_ON
            // #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #pragma multi_compile_shadowcaster
            #define SHADOWCASTER_PASS

            #include "StandardDefines.cginc"

            ENDCG
        }

        Pass {
            Name "META"
            Tags {"LightMode"="Meta"}
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _WORKFLOW_PACKED_ON
            #pragma shader_feature_local _EMISSION_ON
            #pragma shader_feature_local _NORMALMAP_ON
            #pragma shader_feature_local _DETAIL_MAINTEX_ON
            #pragma shader_feature_local _DETAIL_NORMAL_ON
            #pragma shader_feature_local _AUDIOLINK_ON
            #pragma shader_feature_local _AUDIOLINK_META_ON
            #pragma shader_feature EDITOR_VISUALIZATION
            #define META_PASS
            
            #include "StandardDefines.cginc"
            ENDCG
        }
    }
    CustomEditor "Mochie.StandardEditor"
}
