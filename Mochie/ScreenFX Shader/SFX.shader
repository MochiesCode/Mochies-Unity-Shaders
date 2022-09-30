// BY MOCHIE

Shader "Mochie/Screen FX" {
    Properties {

		//Global Settings
		[Enum(Opaque,0, Alpha,1, Premultiplied,2, Additive,3, Soft Additive,4, Multiply,5, Multiply 2x,6)]_BlendMode("", Int) = 0
		_SrcBlend("", Int) = 1
		_DstBlend("", Int) = 0
		_Opacity("", Range(-1,1)) = 1
		[ToggleUI]_DisplayGlobalGizmo("", Int) = 0
		_MinRange("", Float) = 8
		_MaxRange("", Float) = 15

		// Filtering
		[Enum(Off,0, On,1)]_FilterModel("", Int) = 0
		[ToggleUI]_ColorUseGlobal("", Int) = 1
		_FilterStrength("", Range(0,1)) = 1
		_ColorMinRange("", Float) = 8
		_ColorMaxRange("", Float) = 15
		[ToggleUI]_AutoShift("", Int) = 0
		[HDR]_Color("", Color) = (1,1,1,1)
		_RGB("", Vector) = (1,1,1,0)
		_AutoShiftSpeed("", Float) = 0.25
		_Hue("", Range(0,1)) = 0
		_Saturation("", Float) = 1
		_SaturationR("", Float) = 1
		_SaturationG("", Float) = 1
		_SaturationB("", Float) = 1
		_Value("", Float) = 0
		_Brightness("", Float) = 1
		_HDR("", Float) = 0
		_Contrast("", Float) = 1
		_Invert("", Range(0,1)) = 0
		_InvertR("", Range(0,1)) = 0
		_InvertG("", Range(0,1)) = 0
		_InvertB("", Range(0,1)) = 0
		
		// Shake
		[Enum(Off,0, Smooth,1, Coarse,2, Noise,3)]_ShakeModel("", Int) = 0
		[ToggleUI]_ShakeUseGlobal("", Int) = 1
		_ShakeMinRange("", Float) = 8
		_ShakeMaxRange("", Float) = 15
		_Amplitude("",Range(0,1)) = 0
		_ShakeSpeedX("", Range(0,1)) = 0.5234375
		_ShakeSpeedY("", Range(0,1)) =  0.78125
		_ShakeNoiseTex("", 2D) = "black" {}
		_ShakeSpeedXY("", Range(0,10)) = 5

		// Distortion
		[Enum(Off,0, Screenspace,1, Triplanar,2)]_DistortionModel("", Int) = 0
		[ToggleUI]_DistortionUseGlobal("", Int) = 1
		_DistortionMinRange("", Float) = 8
		_DistortionMaxRange("", Float) = 15
		_NormalMap("", 2D) = "bump" {}
		_DistortionStr("", Float) = 0.5
		_DistortionSpeed("", Float) = 0
		_DistortionRadius("", Float) = 2
		_DistortionFade("", Float) = 1
		_DistortionP2O("", Range(0,1)) = 1
		
		// Blur
		[Enum(Off,0, Pixel,1, Dither,2, Radial,3)]_BlurModel("", Int) = 0
		[Enum(Sample16,16, Sample22,22, Sample43,43, Sample71,71, Sample136,136)]_PixelBlurSamples("", Int) = 43
		[ToggleUI]_BlurUseGlobal("", Int) = 1
		_BlurMinRange("", Float) = 8
		_BlurMaxRange("", Float) = 15
		[ToggleUI]_BlurY("", Int) = 0
		[ToggleUI]_RGBSplit("", Int) = 0
		[ToggleUI]_DoF("", Int) = 0
		_DoFRadius("", Float) = 2
		_DoFFade("", Float) = 1
		_DoFP2O("", Range(0,1)) = 1
		_BlurOpacity("", Range(0,1)) = 1
		_BlurStr("", Range(0,1)) = 0
		_BlurRadius("", Range(1,3)) = 1
		_PixelationStr("", Range(0,1)) = 0
		_RippleGridStr("", Range(0,2)) = 0
		[IntRange]_BlurSamples("", Range(2,40)) = 10
		[ToggleUI]_CrushBlur("", Int) = 0

		// Noise
		[Enum(Off,0, On,1)]_NoiseMode("", Int) = 0
		[ToggleUI]_NoiseUseGlobal("", Int) = 0
		_NoiseMinRange("", Float) = 8
		_NoiseMaxRange("", Float) = 15
		_NoiseStrength("", Range(0,1)) = 1
		_NoiseRGB("", Vector) = (0,0,0,0)
		_Noise("", Float) = 0
		_ScanLine("", Float) = 0
		_ScanLineThick("", Float) = 1
		_ScanLineSpeed("", Float) = 1

		// Fog
		[Enum(Off,0, On,1)]_Fog("", Int) = 0
		[ToggleUI]_FogUseGlobal("", Int) = 0
		_FogMinRange("", Float) = 15
		_FogMaxRange("", Float) = 20
		_FogColor("", Color) = (0.75,0.75,0.75,1)
		_FogRadius("", Float) = 2
		_FogFade("", Float) = 1
		_FogP2O("", Range(0,1)) = 0
		[ToggleUI]_FogSafeZone("", Int) = 0
		_FogSafeRadius("", Float) = 4
		_FogSafeMaxRange("", Float) = 6
		_FogSafeOpacity("", Range(0,1)) = 1

		// Zoom
		[Enum(Off,0, Basic,1, RGB,2)]_Zoom("", Int) = 0
		_ZoomStr("", Range(0,1)) = 0
		_ZoomStrR("", Range(0,1)) = 0
		_ZoomStrG("", Range(0,1)) = 0
		_ZoomStrB("", Range(0,1)) = 0
		[ToggleUI]_ZoomUseGlobal("", Int) = 0
		_ZoomMinRange("", Float) = 3
		_ZoomMaxRange("", Float) = 4.5

		// Screenspace Texture
		[Enum(Off,0, Static,1, Animated,2, Distortion,3)]_SST("", Int) = 0
		[Enum(Alpha,0, Add,1, Multiply,2)]_SSTBlend("", Int) = 0
		[ToggleUI]_SSTUseGlobal("", Int) = 1
		_SSTMinRange("", Float) = 8
		_SSTMaxRange("", Float) = 15
		_ScreenTex("", 2D) = "white" {}
		_SSTColor("", Color) = (1,1,1,1)
		_SSTScale("", Float) = 2
		_SSTWidth("", Float) = 1
		_SSTHeight("", Float) = 1
		_SSTLR("", Float) = 0
		_SSTUD("", Float) = 0
		_SSTAnimatedDist("", Float) = 0
		_SSTColumnsX("", Int) = 2
		_SSTRowsY("", Int) = 2
		_SSTAnimationSpeed("", Range(1,120)) = 60
		_SSTFrameSizeXP("", Range(0,1)) = 1
		_SSTFrameSizeYP("", Range(0,1)) = 1
		_SSTFrameSizeXN("", Range(0,1)) = 0
		_SSTFrameSizeYN("", Range(0,1)) = 0
		_ScrubPos("", Int) = 0
		[ToggleUI]_ManualScrub("", Int) = 0

		// Triplanar
		[Enum(Off,0, Basic,1, Scanner,2)]_Triplanar("", Int) = 0
		[ToggleUI]_TPUseGlobal("", Int) = 1
		_TPColor("", Color) = (1,1,1,1)
		_TPTexture("", 2D) = "white" {}
		_TPNoiseTex("", 2D) = "white" {}
		_TPMinRange("", Float) = 8
		_TPMaxRange("", Float) = 15
		_TPRadius("", Float) = 2
		_TPFade("", Float) = 0.5
		_TPP2O("", Range(0,1)) = 1
		_TPScroll("", Vector) = (0,0,0,0)
		_TPNoiseScroll("", Vector) = (0,0,0,0)
		_TPThickness("", Range(0.001,3)) = 0.4
		_TPNoise("", Range(0,3)) = 0
		_TPScanFade("", Range(0,1)) = 0.1

		// Outline
		[Enum(Off,0, Sobel,1, Aura,2)]_OutlineType("", Int) = 0
		[Enum(Sample16,16, Sample22,22, Sample43,43)]_AuraSampleCount("", Int) = 43
		[ToggleUI]_OLUseGlobal("", Int) = 1
		_OLMinRange("", Float) = 8
		_OLMaxRange("", Float) = 15
		[HDR]_OutlineCol("", Color) = (0,0,0,1)
		_BackgroundCol("", Color) = (1,1,1,0)
		_OutlineThresh("", Float) = 1000
		_OutlineThiccS("", Float) = 0.49
		_OutlineThiccN("", Float) = 0.5
		_AuraFade("", Range(0,1)) = 0.5
		_AuraStr("", Range(0,1)) = 0.25
		[ToggleUI]_SobelClearInner("", Int) = 1
		
		// Audio Link
		[ToggleUI]_AudioLinkToggle("", Int) = 0
		_AudioLinkStrength("", Range(0,1)) = 1
		_AudioLinkMin("", Float) = 0
		_AudioLinkMax("", Float) = 1
		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Treble,3)]_AudioLinkFilteringBand("", Int) = 0
		_AudioLinkFilteringStrength("", Range(0,1)) = 0
		_AudioLinkFilteringMin("", Float) = 0
		_AudioLinkFilteringMax("", Float) = 1
		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Treble,3)]_AudioLinkShakeBand("", Int) = 0
		_AudioLinkShakeStrength("", Range(0,1)) = 0
		_AudioLinkShakeMin("", Float) = 0
		_AudioLinkShakeMax("", Float) = 1
		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Treble,3)]_AudioLinkDistortionBand("", Int) = 0
		_AudioLinkDistortionStrength("", Range(0,1)) = 0
		_AudioLinkDistortionMin("", Float) = 0
		_AudioLinkDistortionMax("", Float) = 1
		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Treble,3)]_AudioLinkBlurBand("", Int) = 0
		_AudioLinkBlurStrength("", Range(0,1)) = 0
		_AudioLinkBlurMin("", Float) = 0
		_AudioLinkBlurMax("", Float) = 1
		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Treble,3)]_AudioLinkNoiseBand("", Int) = 0
		_AudioLinkNoiseStrength("", Range(0,1)) = 0
		_AudioLinkNoiseMin("", Float) = 0
		_AudioLinkNoiseMax("", Float) = 1
		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Treble,3)]_AudioLinkZoomBand("", Int) = 0
		_AudioLinkZoomStrength("", Range(0,1)) = 0
		_AudioLinkZoomMin("", Float) = 0
		_AudioLinkZoomMax("", Float) = 1
		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Treble,3)]_AudioLinkSSTBand("", Int) = 0
		_AudioLinkSSTStrength("", Range(0,1)) = 0
		_AudioLinkSSTMin("", Float) = 0
		_AudioLinkSSTMax("", Float) = 1
		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Treble,3)]_AudioLinkFogBand("", Int) = 0
		_AudioLinkFogOpacity("", Range(0,1)) = 0
		_AudioLinkFogRadius("", Range(0,1)) = 0
		_AudioLinkFogMin("", Float) = 0
		_AudioLinkFogMax("", Float) = 1
		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Treble,3)]_AudioLinkTriplanarBand("", Int) = 0
		_AudioLinkTriplanarOpacity("", Range(0,1)) = 0
		_AudioLinkTriplanarRadius("", Range(0,1)) = 0
		_AudioLinkTriplanarMin("", Float) = 0
		_AudioLinkTriplanarMax("", Float) = 1
		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Treble,3)]_AudioLinkOutlineBand("", Int) = 0
		_AudioLinkOutlineStrength("", Range(0,1)) = 0
		_AudioLinkOutlineMin("", Float) = 0
		_AudioLinkOutlineMax("", Float) = 1
		[Enum(Bass,0, Low Mids,1, Upper Mids,2, Treble,3)]_AudioLinkMiscBand("", Int) = 0
		_AudioLinkMiscStrength("", Range(0,1)) = 0
		_AudioLinkMiscMin("", Float) = 0
		_AudioLinkMiscMax("", Float) = 1

		// Extras
		[ToggleUI]_Letterbox("", Int) = 0
		[ToggleUI]_UseZoomFalloff("", Int) = 0
		_LetterboxStr("", Range(0,0.25)) = 0
		[ToggleUI]_DeepFry("", Int ) = 0
		_Flavor("", Range(0,1)) = 0
		_Heat("", Range(0,1)) = 0
		_Sizzle("", Range(0,1)) = 0
		[ToggleUI]_Pulse("", Int) = 0
		[ToggleUI]_PulseColor("", Int) = 0
		_PulseSpeed("", Range(0.5,8)) = 1
		[Enum(None,0, Sin,1, Saw,2, Reverse Saw,3, Square,4, Triangle,5)]_WaveForm("", Int) = 0
		[ToggleUI]_Shift("", Int) = 0
		[ToggleUI]_InvertX("", Int) = 0
		[ToggleUI]_InvertY("", Int) = 0
		_ShiftX("", Float) = 0
		_ShiftY("", Float) = 0
		[ToggleUI]_RoundingToggle("", Int) = 0
		_Rounding("", Float) = 1
		_RoundingOpacity("", Range(0,1)) = 1
		_NormalMapFilter("", Range(0,0.99)) = 0.75
		[ToggleUI]_NMFToggle("", Int) = 0
		_NMFOpacity("", Range(0,1)) = 1
		[ToggleUI]_DepthBufferToggle("", Int) = 0
		_DBOpacity("", Range(0,1)) = 1
		_DBColor("", Color) = (1,1,1,1)
    }
    SubShader {
        Tags {
			"RenderType"="Overlay" 
			"Queue"="Overlay-2" 
			"ForceNoShadowCasting"="True" 
			"IgnoreProjector"="True"
			"PreviewType"="Plane"
			"DisableBatching"="True"
		}
		Cull Front
        ZWrite Off
        ZTest Always

        GrabPass{
			Tags {"LightMode"="ForwardBase"}
			"_MSFXGrab"
		}
        Pass {
			Tags {"LightMode"="ForwardBase"}
			Blend [_SrcBlend] [_DstBlend]
            CGPROGRAM
			#pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
			#pragma shader_feature_local _COLOR_ON
			#pragma shader_feature_local _SHAKE_ON
			#pragma shader_feature_local _ _DISTORTION_ON _DISTORITON_WORLD_ON
			#pragma shader_feature_local _ _BLUR_PIXEL_ON _BLUR_DITHER_ON _BLUR_RADIAL_ON
			#pragma shader_feature_local _BLUR_Y_ON
			#pragma shader_feature_local _CHROMATIC_ABBERATION_ON
			#pragma shader_feature_local _DOF_ON
			#pragma shader_feature_local _NOISE_ON
			#pragma shader_feature_local _AUDIOLINK_ON
			#define MAIN
			#include "SFXDefines.cginc"
            ENDCG
        }
    }
	CustomEditor "SFXEditor"
}