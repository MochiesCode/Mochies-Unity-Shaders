#ifndef SFX_DEFINES_INCLUDED
#define SFX_DEFINES_INCLUDED

#include "UnityCG.cginc"
#include "SFXKeyDefines.cginc"

// Global
sampler2D _MSFXGrab; float4 _MSFXGrab_TexelSize, _MSFXGrab_ST;
sampler2D _CameraDepthTexture; float4 _CameraDepthTexture_TexelSize;
int _BlendMode;
float _MinRange, _MaxRange;
float _Opacity;

// Color Filtering
int _FilterModel, _AutoShift, _ColorUseGlobal, _NoiseUseGlobal, _RoundingToggle;
float4 _Color;
float3 _RGB, _NoiseRGB;
float _FilterStrength, _Noise, _NoiseStrength;
float _ColorMinRange, _ColorMaxRange, _Rounding, _RoundingOpacity;
float _Contrast, _HDR;
float _Invert, _InvertR, _InvertG, _InvertB, _Saturation;
float _Hue, _AutoShiftSpeed, _Brightness;
float _SaturationR, _SaturationG, _SaturationB;
float _ScanLine, _ScanLineThick, _ScanLineSpeed;
float _NoiseMinRange, _NoiseMaxRange;

// Shake
int _ShakeModel, _ShakeUseGlobal;
sampler2D _ShakeNoiseTex;
float _ShakeMinRange, _ShakeMaxRange;
float _Amplitude, _AmplitudeMult;
float _ShakeSpeedX, _ShakeSpeedY, _ShakeSpeedXY;

// Distortion
int _DistortionModel, _DistortionUseGlobal;
sampler2D _NormalMap; float4 _NormalMap_ST;
float _DistortionMinRange, _DistortionMaxRange;
float _DistortionStr, _DistortionSpeed;
float _DistortionRadius, _DistortionP2O, _DistortionFade;

// Blur
int _FocusPlayer, _BlurUseGlobal, _BlurY, _BlurSamples, _CrushBlur;
int _BlurModel, _RGBSplit, _DoF, _PixelBlurSamples;
float _BlurMinRange, _BlurMaxRange;
float _BlurOpacity, _BlurStr;
float _DoFP2O, _DoFRadius, _DoFFade;
float _PixelationStr, _RippleGridStr;
float _BlurRadius;

// outputs
float3 wPos;
float3 wNorm;
float depth;

#if X_FEATURES
// Fog
	int _Fog, _FogSafeZone, _FogUseGlobal;
	float4 _FogColor;
	float _FogMinRange, _FogMaxRange;
	float _FogRadius, _FogFade;
	float _FogSafeRadius, _FogSafeMaxRange;
	float _FogP2O, _FogSafeOpacity;

	// Screenspace Texture
	int _SST, _SSTBlend, _SSTUseGlobal, _ManualScrub, _ScrubPos;
	sampler2D _ScreenTex;
	float4 _SSTColor;
	float _SSTMinRange, _SSTMaxRange;
	float _SSTWidth, _SSTHeight, _SSTScale;
	float _SSTLR, _SSTUD;
	float _SSTColumnsX, _SSTRowsY, _SSTAnimationSpeed, _SSTAnimatedDist;
	float _SSTFrameSizeXP, _SSTFrameSizeYP, _SSTFrameSizeXN, _SSTFrameSizeYN;

	// Triplanar
	sampler2D _TPTexture, _TPNoiseTex;
	int _Triplanar, _TPUseGlobal, _TPBlend;
	float4 _TPTexture_ST, _TPNoiseTex_ST, _TPColor;
	float3 _TPScroll, _TPNoiseScroll;
	float _TPRadius, _TPFade, _TPMinRange, _TPMaxRange, _TPP2O, _TPThickness, _TPNoise, _TPScanFade;

	// Letterbox
	int _UseZoomFalloff, _Letterbox;
	float _LetterboxStr;

	// Zoom
	sampler2D _ZoomGrab;
	int _Zoom, _ZoomUseGlobal, _NeedsZoomPass;
	float _ZoomMinRange, _ZoomMaxRange;
	float _ZoomStr, _ZoomStrR, _ZoomStrG, _ZoomStrB;

	// Extras
	int _OLUseGlobal, _OutlineType, _Shift, _InvertX, _InvertY, _Sobel, _DepthBufferToggle;
	int _AuraSampleCount;
	float _OLMinRange, _OLMaxRange, _AuraFade, _AuraStr;
	float4 _OutlineCol, _BackgroundCol;
	float3 _DBColor;
	float _OutlineThiccS, _OutlineThiccN, _OutlineThresh, _SobelStr;
	float _ShiftX, _ShiftY, _Rotate;
	int _Pulse, _WaveForm, _PulseColor;
	float _PulseSpeed, _NormalMapFilter, _NMFToggle, _NMFOpacity, _DBOpacity;
#endif

struct appdata {
    float4 vertex : POSITION;
    float4 uv : TEXCOORD0;
};

struct v2f {
    float4 pos : SV_POSITION;
    float4 uv : TEXCOORD0;
    float2 uvd : TEXCOORD1;
    float2 uvs : TEXCOORD2;
    float3 cameraPos : TEXCOORD3;
    float3 objPos : TEXCOORD4;
    float objDist : TEXCOORD5;
    float3 raycast : TEXCOORD6;
    float pulseSpeed : TEXCOORD7;
    float globalF : TEXCOORD8;
    float colorF : TEXCOORD9;
    float shakeF: TEXCOORD10;
    float distortionF : TEXCOORD11;
    float blurF : TEXCOORD12;
    float fogF : TEXCOORD13;
    float sstF : TEXCOORD14;
	float zoom : TEXCOORD15;
	float4 zoomPos : TEXCOORD16;
	float4 uvR : TEXCOORD17;
	float4 uvG : TEXCOORD18;
	float4 uvB : TEXCOORD19;
	float luv : TEXCOORD20;
	float letterbF : TEXCOORD21;
	float olF : TEXCOORD22;
	float noiseF : TEXCOORD23;
};

#include "../Common/Utilities.cginc"
#include "../Common/Color.cginc"
#include "SFXBlur.cginc"
#include "SFXFunctions.cginc"
#if X_FEATURES
	#include "SFXXFeatures.cginc"
#endif
#include "SFXPass.cginc"

#endif