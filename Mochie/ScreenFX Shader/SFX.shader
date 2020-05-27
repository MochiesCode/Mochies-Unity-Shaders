// BY MOCHIE
// Version 1.7

Shader "Mochie/Screen FX" {
    Properties {

		//Global Settings
		[HideInInspector]_BlendMode("__mode", Float) = 0.0
		[HideInInspector]_SrcBlend("__src", Float) = 1.0
		[HideInInspector]_DstBlend("__dst", Float) = 0.0
		_Opacity("", Range(-1,1)) = 1
		_MinRange("", Range(0,49.99)) = 8
		_MaxRange("", Range (0.01,50)) = 15

		// Color
		[Enum(OFF,0, RGB,1, HSL,2)]_FilterModel("", Int) = 0
		[Toggle(_)]_ColorUseGlobal("", Int) = 1
		_ColorMinRange("", Range(0,49.99)) = 8
		_ColorMaxRange("", Range (0.01,50)) = 15
		[Toggle(_)]_AutoShift("", Int) = 0
		[HDR]_Color("", Color) = (1,1,1,0)
		_SaturationRGB("", Range(0,2)) = 1
		_AutoShiftSpeed("", Range(0,1)) = 0.25
		_Hue("", Range(0,1)) = 0
		_SaturationHSL("", Range(0,1)) = 0
		_Luminance ("", Range(0,0.5)) = 0
		_HSLMin("", Range(0,1)) = 0
		_HSLMax("", Range(0,1)) = 1
		_HDR("", Range(0,1)) = 0
		_Exposure("", Range(0,3)) = 0
		_Contrast("", Range(0,2)) = 1
		_Invert("", Range (0,1)) = 0
		_InvertR("", Range(0,1)) = 0
		_InvertG("", Range(0,1)) = 0
		_InvertB("", Range(0,1)) = 0
		_Noise("", Range(0,1)) = 0
		
		// Shake
		[Enum(OFF,0, SMOOTH,1, COARSE,2, NOISE,3)]_ShakeModel("", Int) = 0
		[Toggle(_)]_ShakeUseGlobal("", Int) = 1
		_ShakeMinRange("", Range(0,49.99)) = 8
		_ShakeMaxRange("", Range (0.01,50)) = 15
		_Amplitude("",Range(0,1)) = 0
		_ShakeSpeedX("", Range(0,1)) = 0.5234375
		_ShakeSpeedY("", Range(0,1)) =  0.78125
		_ShakeNoiseTex("", 2D) = "black" {}
		_ShakeSpeedXY("", Range(0,10)) = 5

		// Distortion
		[Enum(OFF,0, SCREENSPACE,1, TRIPLANAR,2)]_DistortionModel("", Int) = 0
		[Toggle(_)]_DistortionUseGlobal("", Int) = 1
		_DistortionMinRange("", Range(0,49.99)) = 8
		_DistortionMaxRange("", Range (0.01,50)) = 15
		_NormalMap("", 2D) = "bump" {}
		_DistortionStr("", Range(0,1)) = 0.5
		_DistortionSpeed("", Range(0,1)) = 0
		_DistortionRadius("", Float) = 2
		_DistortionFade("", Float) = 1
		_DistortionP2O("", Range(0,1)) = 1
		
		// Blur
		[Enum(OFF,0, PIXEL,1, DITHER,2, RADIAL,3)]_BlurModel("", Int) = 0
		[Enum(Sample16,16, Sample22,22, Sample43,43, Sample71,71, Sample136,136)]_PixelBlurSamples("", Int) = 43
		[Toggle(_)]_BlurUseGlobal("", Int) = 1
		_BlurMinRange("", Range(0,49.99)) = 8
		_BlurMaxRange("", Range (0.01,50)) = 15
		[Toggle(_)]_BlurY("", Int) = 0
		[Toggle(_)]_RGBSplit("", Int) = 0
		[Toggle(_)]_DoF("", Int) = 0
		_DoFRadius("", Float) = 2
		_DoFFade("", Float) = 1
		_DoFP2O("", Range(0,1)) = 1
		_BlurOpacity("", Range(0,1)) = 1
		_BlurStr("", Range(0,1)) = 0
		_BlurRadius("", Range(1,3)) = 1
		_PixelationStr("", Range(0,1)) = 0
		_RippleGridStr("", Range(0,2)) = 0
		[IntRange]_BlurSamples("", Range(2,40)) = 10
		[Toggle(_)]_CrushBlur("", Int) = 0

		// Fog
		[Enum(OFF,0, ON,1)]_Fog("", Int) = 0
		[Toggle(_)]_FogUseGlobal("", Int) = 0
		_FogMinRange("", Range(0,49.99)) = 15
		_FogMaxRange("", Range (0.01,50)) = 20
		_FogColor("", Color) = (0.75,0.75,0.75,1)
		_FogRadius("", Float) = 2
		_FogFade("", Float) = 1
		_FogP2O("", Range(0,1)) = 0
		[Toggle(_)]_FogSafeZone("", Int) = 0
		_FogSafeRadius("", Range(0,20)) = 4
		_FogSafeMaxRange("", Range(0.001,20)) = 6
		_FogSafeOpacity("", Range(0,1)) = 1

		// Zoom
		[Enum(OFF,0, BASIC,1, RGB,2)]_Zoom("", Int) = 0
		_ZoomStr("", Range(0,1)) = 0
		_ZoomStrR("", Range(0,1)) = 0
		_ZoomStrG("", Range(0,1)) = 0
		_ZoomStrB("", Range(0,1)) = 0
		[Toggle(_)]_ZoomUseGlobal("", Int) = 0
		_ZoomMinRange("", Range(0,49.99)) = 3
		_ZoomMaxRange("", Range (0.01,50)) = 4.5

		// Screenspace Texture
		[Enum(OFF,0, STATIC,1, ANIMATED,2, DISTORTION,3)]_SST("", Int) = 0
		[Enum(Alpha,0, Add,1, Multiply,2)]_SSTBlend("", Int) = 0
		[Toggle(_)]_SSTUseGlobal("", Int) = 1
		_SSTMinRange("", Range(0,49.99)) = 8
		_SSTMaxRange("", Range (0.01,50)) = 15
		_ScreenTex("", 2D) = "white" {}
		_SSTColor("", Color) = (1,1,1,1)
		_SSTScale("", Range(0.001,10)) = 1
		_SSTWidth("", Range(0.001,2)) = 1
		_SSTHeight("", Range(0.001,2)) = 1
		_SSTLR("", Range(-1,1)) = 0
		_SSTUD("", Range(-1,1)) = 0
		_SSTAnimatedDist("", Range(0,128)) = 0
		_SSTColumnsX("", Int) = 2
		_SSTRowsY("", Int) = 2
		_SSTAnimationSpeed("", Range(1,120)) = 60
		_SSTFrameSizeXP("", Range(0,1)) = 1
		_SSTFrameSizeYP("", Range(0,1)) = 1
		_SSTFrameSizeXN("", Range(0,1)) = 0
		_SSTFrameSizeYN("", Range(0,1)) = 0
		_ScrubPos("", Int) = 0
		[Toggle(_)]_ManualScrub("", Int) = 0

		// Triplanar
		[Enum(OFF,0, BASIC,1, SCANNER,2)]_Triplanar("", Int) = 0
		[Toggle(_)]_TPUseGlobal("", Int) = 1
		_TPColor("", Color) = (1,1,1,1)
		_TPTexture("", 2D) = "white" {}
		_TPNoiseTex("", 2D) = "white" {}
		_TPMinRange("", Range(0,49.99)) = 8
		_TPMaxRange("", Range(0.01,50)) = 15
		_TPRadius("", Float) = 2
		_TPFade("", Float) = 0.5
		_TPP2O("", Range(0,1)) = 1
		_TPScroll("", Vector) = (0,0,0,0)
		_TPNoiseScroll("", Vector) = (0,0,0,0)
		_TPThickness("", Range(0.001,3)) = 0.4
		_TPNoise("", Range(0,3)) = 0
		_TPScanFade("", Range(0,1)) = 0.1

		// Outline
		[Enum(OFF,0, SOBEL,1, AURA,2)]_OutlineType("", Int) = 0
		[Enum(Sample16,16, Sample22,22, Sample43,43)]_AuraSampleCount("", Int) = 43
		[Toggle(_)]_OLUseGlobal("", Int) = 1
		_OLMinRange("", Range(0,49.99)) = 8
		_OLMaxRange("", Range(0.01,50)) = 15
		[HDR]_OutlineCol("", Color) = (0,0,0,1)
		_BackgroundCol("", Color) = (1,1,1,0)
		_OutlineThresh("", Float) = 1000
		_OutlineThiccS("", Float) = 0.49
		_OutlineThiccN("", Float) = 0.5
		_AuraFade("", Range(0,1)) = 0.5
		_AuraStr("", Range(0,1)) = 0.25

		// Extras
		[Toggle(_)]_Letterbox("", Int) = 0
		[Toggle(_)]_UseZoomFalloff("", Int) = 0
		_LetterboxStr("", Range(0,0.25)) = 0
		[Toggle(_)]_DeepFry("", Int ) = 0
		_Flavor("", Range(0,1)) = 0
		_Heat("", Range(0,1)) = 0
		_Sizzle("", Range(0,1)) = 0
		[Toggle(_)]_Pulse("", Int) = 0
		[Toggle(_)]_PulseColor("", Int) = 0
		_PulseSpeed("", Range(0.5,8)) = 1
		[Enum(NONE,0, SIN,1, SAW,2, REVSAW,3, SQUARE,4, TRIANGLE,5)]_WaveForm("", Int) = 0
		[Toggle(_)]_Shift("", Int) = 0
		[Toggle(_)]_InvertX("", Int) = 0
		[Toggle(_)]_InvertY("", Int) = 0
		_ShiftX("", Float) = 0
		_ShiftY("", Float) = 0
		[Toggle(_)]_GhostingToggle("Ghosting", Int) = 0
		_GhostingStr("", Range(0,0.999)) = 0.7
		[Toggle(_)]_FreezeFrame("Freeze Frame", Int) = 0
		[Toggle(_)]_RoundingToggle("", Int) = 0
		_Rounding("", Range(1,10)) = 1
		_RoundingOpacity("", Range(0,1)) = 1
		_NormalMapFilter("", Range(0,0.99)) = 0.75
		[Toggle(_)]_NMFToggle("", Int) = 0
		_NMFOpacity("", Range(0,1)) = 1
		[Toggle(_)]_DepthBufferToggle("", Int) = 0
		_DBOpacity("", Range(0,1)) = 1
		_DBColor("", Color) = (1,1,1,1)
    }
    SubShader {
        Tags {
			"RenderType"="Overlay" 
			"Queue"="Overlay-2" 
			"ForceNoShadowCasting"="True" 
			"IgnoreProjector"="True"
			"LightMode"="ForwardBase"
		}
		Blend [_SrcBlend] [_DstBlend]
		Cull Front
        ZWrite Off
        ZTest Always

        GrabPass{
			Tags {"LightMode"="ForwardBase"}
			"_MSFXGrab"
		}
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#define FULL_PASS
			#include "SFXDefines.cginc"
			#pragma target 5.0
            ENDCG
        }
    }
	Fallback "Transparent/Diffuse"
	CustomEditor "SFXEditor"
}