// By Mochie#8794

Shader "Mochie/Particles" {
    Properties {
        
        [Enum(Alpha,0, Premultiplied,1, Additive,2, Soft Additive,3, Multiply,4, Multiply x2,5, Opaque,6)]_BlendMode("", Int) = 1.0
        [HideInInspector]_SrcBlend("__src", Int) = 1
        [HideInInspector]_DstBlend("__dst", Int) = 10
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Int) = 4
        [ToggleUI]_ZWrite("", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)]_Culling("", Int) = 2
        [ToggleUI]_FlipbookBlending("", Int) = 0
        [ToggleUI]_LightingToggle("Lighting", Int) = 0
        [Enum(Base Color,0, Alpha Mask,1)]_AlphaSource("Alpha Source", Int) = 0

        _MainTex("", 2D) = "white" {}
        [Enum(Default,0, Polar,1, Panosphere,2)]_MainTexUVMode("UV Mode", Int) = 0
        _MainTexSpeed("Speed", Vector) = (0,0,0,0)
        _MainTexPolarRotation("Rotation", Float) = 0
        _MainTexPolarSpeed("Speed", Float) = 0
        _MainTexPolarRadius("Radius", Float) = 1
        [ToggleUI]_MainTexTSToggle("Is Texturesheet", Int) = 0
        _AlphaMask("Alpha Mask", 2D) = "white" {}
        [Enum(Default,0, Polar,1, Panosphere,2)]_AlphaMaskUVMode("UV Mode", Int) = 0
        _AlphaMaskSpeed("Speed", Vector) = (0,0,0,0)
        _AlphaMaskPolarRotation("Rotation", Float) = 0
        _AlphaMaskPolarSpeed("Speed", Float) = 0
        _AlphaMaskPolarRadius("Radius", Float) = 1
        [Enum(Red,0, Green,1, Blue,2, Alpha,3)]_AlphaMaskChannel("Alpha Mask Channel", Int) = 0
        [HDR]_Color("", Color) = (1,1,1,1)
        [ToggleUI]_Layering("", Int) = 0
        [Enum(Lerp,0, Add,1, Sub,2, Mult,3)]_TexBlendMode("", Int) = 0
        _SecondTex("", 2D) = "white" {}
        [Enum(Default,0, Polar,1, Panosphere,2)]_SecondTexUVMode("UV Mode", Int) = 0
        _SecondTexSpeed("Speed", Vector) = (0,0,0,0)
        _SecondTexPolarRotation("Rotation", Float) = 0
        _SecondTexPolarSpeed("Speed", Float) = 0
        _SecondTexPolarRadius("Radius", Float) = 1
        [HDR]_SecondColor("", Color) = (1,1,1,1)
        _Brightness("", Float) = 1
        _Opacity("", Range(0,1)) = 1
        [ToggleUI]_CutoutRim("Cutout Rim", Int) = 0
        _CutoutRimWidth("Cutout Rim Width", Float) = 1
        [HDR]_CutoutRimColor("Cutout Rim Color", Color) = (1,1,1,1)
        [Enum(Add,0, Multiply,1)]_CutoutRimBlend("Cutout Rim Blending", Int) = 0

        _NormalMapLighting("Normal Map", 2D) = "bump" {}
        [Enum(Default,0, Polar,1, Panosphere,2)]_NormalMapLightingUVMode("UV Mode", Int) = 0
        _NormalMapLightingSpeed("Speed", Vector) = (0,0,0,0)
        _NormalMapLightingPolarRotation("Rotation", Float) = 0
        _NormalMapLightingPolarSpeed("Speed", Float) = 0
        _NormalMapLightingPolarRadius("Radius", Float) = 1
        _NormalMapLightingScale("Scale", Float) = 1
        [ToggleUI]_NormalMapLightingTSToggle("Is Texturesheet", Int) = 0
        _Metallic("Metallic", Range(0,1)) = 0
        _Roughness("Roughness", Range(0,1)) = 1
        _MetallicMap("Metallic Map", 2D) = "white" {}
        [Enum(Default,0, Polar,1, Panosphere,2)]_MetallicMapUVMode("UV Mode", Int) = 0
        _MetallicMapSpeed("Speed", Vector) = (0,0,0,0)
        _MetallicMapPolarRotation("Rotation", Float) = 0
        _MetallicMapPolarSpeed("Speed", Float) = 0
        _MetallicMapPolarRadius("Radius", Float) = 1
        _RoughnessMap("Roughness Map", 2D) = "white" {}
        [Enum(Default,0, Polar,1, Panosphere,2)]_RoughnessMapUVMode("UV Mode", Int) = 0
        _RoughnessMapSpeed("Speed", Vector) = (0,0,0,0)
        _RoughnessMapPolarRotation("Rotation", Float) = 0
        _RoughnessMapPolarSpeed("Speed", Float) = 0
        _RoughnessMapPolarRadius("Radius", Float) = 1
        [ToggleUI]_ReflectionsToggle("Reflections", Int) = 0
        [ToggleUI]_SpecularHighlightsToggle("Specular Highlights", Int) = 0
        _ReflectionStrength("Reflection Strength", Float) = 1
        _SpecularHighlightStrength("Specular Highlight Strength", Float) = 1
        [ToggleUI]_LightVolumes("Light Volumes", Int) = 1
        _LightVolumeSpecularity("Light Volumes Specularity", Int) = 0
        _LightVolumeSpecularityStrength("Light Volumes Specularity Strength", Float) = 1
        _LightVolumeStrength("Light Volumes Strength", Float) = 1
        [ToggleUI]_Emission("Emission", Int) = 0
        _EmissionMap("Emission Map", 2D) = "white" {}
        [Enum(Default,0, Polar,1, Panosphere,2)]_EmissionMapUVMode("UV Mode", Int) = 0
        _EmissionMapSpeed("Speed", Vector) = (0,0,0,0)
        _EmissionMapPolarRotation("Rotation", Float) = 0
        _EmissionMapPolarSpeed("Speed", Float) = 0
        _EmissionMapPolarRadius("Radius", Float) = 1
        [HDR]_EmissionColor("Emission Color", Color) = (0,0,0,1)
        [ToggleUI]_EmissionLightReactivity("Light Reactivity", Int) = 0
        _EmissionLightReactivityMin("Light Reactivity Min", Float) = 0
        _EmissionLightReactivityMax("Light Reactivity Max", Float) = 1

        _IsCutout("", Int) = 0
        _Cutoff("", Range(0,1)) = 0.5
        [ToggleUI]_Softening("", Int) = 0
        _SoftenStr("", Range(0, 0.999)) = 0

        [ToggleUI]_Filtering("", Int) = 0
        [Enum(HSV,0, Oklab,1)]_HueMode("Hue Mode", Int) = 0
        [ToggleUI]_AutoShift("", Int) = 0
        _AutoShiftSpeed("", Float) = 0.25
        _Hue("", Range(0,1)) = 0
        _Saturation("", Float) = 1
        _HDR("", Float) = 0
        _Contrast("", Float) = 1

        [ToggleUI]_Distortion("", Int) = 0
        _NormalMap("", 2D) = "bump" {}
        [Enum(Default,0, Polar,1, Panosphere,2)]_NormalMapUVMode("UV Mode", Int) = 0
        _NormalMapSpeed("Speed", Vector) = (0,0,0,0)
        _NormalMapPolarRotation("Rotation", Float) = 0
        _NormalMapPolarSpeed("Speed", Float) = 0
        _NormalMapPolarRadius("Radius", Float) = 1
        [ToggleUI]_NormalMapTSToggle("Is Texturesheet", Int) = 0
        [ToggleUI]_DistortMainTex("", Int) = 0
        _DistortionStr("", Float) = 0
        _DistortionBlend("", Range(0,1)) = 0.5
        _DistortionSpeed("", Vector) = (0,0,0,0)

        [ToggleUI]_Pulse("", Int) = 0
        [Enum(Sin,0, Square,1, Triangle,2, Saw,3, Reverse Saw,4)]_Waveform("", Int) = 0
        _PulseStr("", Range(0,1)) = 0.5
        _PulseSpeed("", Float) = 1

        [ToggleUI]_Falloff("", Int) = 0
        _NearMinRange("", Float) = 1
        _NearMaxRange("", Float) = 2
        _MinRange("", Float) = 8
        _MaxRange("", Float) = 15

        [ToggleUI]_AudioLink("", Int) = 0
        _AudioLinkStrength("", Range(0,1)) = 1
        _AudioLinkRemapMin("", Float) = 0
        _AudioLinkRemapMax("", Float) = 1

        [ToggleUI]_Dissolve("", Int) = 0
        [Enum(Particle Lifetime,0, Manual,1)]_DissolveMode("Dissolve Mode", Int) = 0
        _DissolveNoise("Dissolve Noise", 2D) = "white" {}
        [Enum(Default,0, Polar,1, Panosphere,2)]_DissolveNoiseUVMode("UV Mode", Int) = 0
        _DissolveNoiseSpeed("Speed", Vector) = (0,0,0,0)
        _DissolveNoisePolarRotation("Rotation", Float) = 0
        _DissolveNoisePolarSpeed("Speed", Float) = 0
        _DissolveNoisePolarRadius("Radius", Float) = 1
        _DissolveAgeThreshold("Dissolve Age Threshold", Range(0,1)) = 0
        _DissolveAgeThresholdMin("Dissolve Age Threshold Min", Float) = 0
        _DissolveAgeThresholdMax("Dissolve Age Threshold Max", Float) = 1
        _DissolveAmount("Dissolve Amount", Range(0,1)) = 0
        [ToggleUI]_DissolveRandomOffset("Dissolve Random Offset", Int) = 0
        [HDR]_DissolveRimColor("Dissolve Rim Color", Color) = (1,1,1,1)
        _DissolveRimWidth("Dissolve Rim Width", Float) = 0.5
        [Enum(Add,0, Multiply,1)]_DissolveRimBlend("Dissolve Rim Blend", Int) = 0
        
        [ToggleUI]_RandomHue("Random Hue", Int) = 0
        [Enum(HSV,0, Oklab,1)]_RandomHueMode("Random Hue Mode", Int) = 0
        [ToggleUI]_RandomHueMonoTint("Mono Tint", Int) = 0
        _RandomHueMax("Random Hue Max", Float) = 1
        _RandomHueMin("Random Hue Min", Float) = 0
        _RandomSatMax("Random Saturation Max", Float) = 1
        _RandomSatMin("Random Saturation Min", Float) = 0.8

        [ToggleUI]_Outlines("Outlines", Int) = 0
        [ToggleUI]_OutlineStencilToggle("Outline Stencil Toggle", Int) = 0
        _OutlineThickness("Outline Thickness", Float) = 1
        [HDR]_OutlineColor("Color", Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.StencilOp)]_OutlineStencilPass("Outline Stencil Op", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_OutlineStencilCompare("Outline Stencil Comp", Float) = 8
        [HideInInspector]_OutlineCulling("Outline Culling Mode", Int) = 1

        [Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkFilterBand("", Int) = 0
        _AudioLinkFilterStrength("", Range(0,1)) = 0
        _AudioLinkRemapFilterMin("", Float) = 0
        _AudioLinkRemapFilterMax("", Float) = 1

        [Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkDistortionBand("", Int) = 0
        _AudioLinkDistortionStrength("", Range(0,1)) = 0
        _AudioLinkRemapDistortionMin("", Float) = 0
        _AudioLinkRemapDistortionMax("", Float) = 1

        [Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkOpacityBand("", Int) = 0
        _AudioLinkOpacityStrength("", Range(0,1)) = 0
        _AudioLinkRemapOpacityMin("", Float) = 0
        _AudioLinkRemapOpacityMax("", Float) = 1

        [Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkCutoutBand("", Int) = 0
        _AudioLinkCutoutStrength("", Range(0,1)) = 0
        _AudioLinkRemapCutoutMin("", Float) = 0
        _AudioLinkRemapCutoutMax("", Float) = 1

        [Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkOutlineBand("", Int) = 0
        _AudioLinkOutlineStrength("", Range(0,1)) = 0
        _AudioLinkRemapOutlineMin("", Float) = 0
        _AudioLinkRemapOutlineMax("", Float) = 1

        [Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkEmissionBand("", Int) = 0
        _AudioLinkEmissionStrength("", Range(0,1)) = 0
        _AudioLinkRemapEmissionMin("", Float) = 0
        _AudioLinkRemapEmissionMax("", Float) = 1

        [IntRange]_StencilRef("ra", Range(1,255)) = 1
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPass("enx", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFail("emx", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilZFail("enx", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilCompare("enx", Float) = 8

        _QueueOffset("Queue Offset", Int) = 0
        [HideInInspector]_ZWrite("__zw", Float) = 0
        [HideInInspector]_MaterialResetCheck("Reset", Int) = 0
        [HideInInspector]_NaNLmao("", Float) = 0.0

    }

    SubShader {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "IgnoreProjector"="True"
            "PreviewType"="Plane" 
        }
        GrabPass {
            Tags {"LightMode"="GrabPass"}
            "_MPSGrab"
        }
        Blend [_SrcBlend] [_DstBlend]
        Cull [_Culling]
        ZTest [_ZTest]
        ZWrite [_ZWrite]

        Pass {
            Name "FORWARD"
            Tags {"LightMode" = "ForwardBase"}
            Stencil {
                Ref [_StencilRef]
                Comp [_StencilCompare]
                Pass [_StencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }
            CGPROGRAM
            #pragma exclude_renderers gles
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            #pragma shader_feature_local _ _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHA_ADD_ON _ALPHA_ADD_SOFT_ON _ALPHA_MUL_ON _ALPHA_MULX2_ON
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _FILTERING_ON
            #pragma shader_feature_local _DISTORTION_ON
            #pragma shader_feature_local _DISTORTION_UV_ON
            #pragma shader_feature_local _LAYERED_TEX_ON
            #pragma shader_feature_local _PULSE_ON
            #pragma shader_feature_local _FALLOFF_ON
            #pragma shader_feature_local _FLIPBOOK_BLENDING
            #pragma shader_feature_local _FADING_ON
            #pragma shader_feature_local _AUDIOLINK_ON
            #pragma shader_feature_local _LIGHTING_ON
            #pragma shader_feature_local _NORMALMAP_ON
            #pragma shader_feature_local _REFLECTIONS_ON
            #pragma shader_feature_local _SPECULAR_HIGHLIGHTS_ON
            #pragma shader_feature_local _METALLIC_MAP_ON
            #pragma shader_feature_local _ROUGHNESS_MAP_ON
            #pragma shader_feature_local _ALPHA_MASK_ON
            #pragma shader_feature_local _EMISSION_ON
            #pragma multi_compile _ SOFTPARTICLES_ON
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase
            #pragma instancing_options procedural:vertInstancingSetup
            #include "ParticleDefines.cginc"
            #include "ParticleVert.cginc"
            #include "ParticleFrag.cginc"

            ENDCG
        }

        Pass {
            Name "FORWARD_DELTA"
            Tags {"LightMode" = "ForwardAdd"}
            Blend [_SrcBlend] One
            Fog {Color (0,0,0,0)}
            ZWrite Off
            Stencil {
                Ref [_StencilRef]
                Comp [_StencilCompare]
                Pass [_StencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }
            CGPROGRAM
            #pragma exclude_renderers gles
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            #pragma shader_feature_local _ _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHA_ADD_ON _ALPHA_ADD_SOFT_ON _ALPHA_MUL_ON _ALPHA_MULX2_ON
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _FILTERING_ON
            #pragma shader_feature_local _DISTORTION_ON
            #pragma shader_feature_local _DISTORTION_UV_ON
            #pragma shader_feature_local _LAYERED_TEX_ON
            #pragma shader_feature_local _PULSE_ON
            #pragma shader_feature_local _FALLOFF_ON
            #pragma shader_feature_local _FLIPBOOK_BLENDING
            #pragma shader_feature_local _FADING_ON
            #pragma shader_feature_local _AUDIOLINK_ON
            #pragma shader_feature_local _LIGHTING_ON
            #pragma shader_feature_local _NORMALMAP_ON
            #pragma shader_feature_local _SPECULAR_HIGHLIGHTS_ON
            #pragma shader_feature_local _METALLIC_MAP_ON
            #pragma shader_feature_local _ROUGHNESS_MAP_ON
            #pragma shader_feature_local _ALPHA_MASK_ON
            #pragma multi_compile _ SOFTPARTICLES_ON
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdadd_fullshadows 
            #pragma instancing_options procedural:vertInstancingSetup
            #include "ParticleDefines.cginc"
            #include "ParticleVert.cginc"
            #include "ParticleFrag.cginc"

            ENDCG
        }

        Pass {
            Tags {"LightMode" = "ShadowCaster"}
            Stencil {
                Ref [_StencilRef]
                Comp [_StencilCompare]
                Pass [_StencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }
            CGPROGRAM
            #pragma exclude_renderers gles
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            #pragma shader_feature_local _ _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHA_ADD_ON _ALPHA_ADD_SOFT_ON _ALPHA_MUL_ON _ALPHA_MULX2_ON
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _DISTORTION_ON
            #pragma shader_feature_local _DISTORTION_UV_ON
            #pragma shader_feature_local _LAYERED_TEX_ON
            #pragma shader_feature_local _PULSE_ON
            #pragma shader_feature_local _FALLOFF_ON
            #pragma shader_feature_local _FLIPBOOK_BLENDING
            #pragma shader_feature_local _FADING_ON
            #pragma shader_feature_local _AUDIOLINK_ON
            #pragma shader_feature_local _ALPHA_MASK_ON
            #pragma multi_compile _ SOFTPARTICLES_ON
            #pragma multi_compile_instancing
            #pragma multi_compile_shadowcaster
            #pragma instancing_options procedural:vertInstancingSetup
            #include "ParticleDefines.cginc"
            #include "ParticleVert.cginc"
            #include "ParticleFrag.cginc"

            ENDCG
        }
    }
    CustomEditor "Mochie.ParticleEditor"
}