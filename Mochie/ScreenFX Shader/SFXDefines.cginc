#ifndef SFX_DEFINES_INCLUDED
#define SFX_DEFINES_INCLUDED

#include "UnityCG.cginc"
#include "../Common/Sampling.cginc"
#include "SFXKeyDefines.cginc"

// Global
MOCHIE_DECLARE_TEX2D_SCREENSPACE(_MSFXGrab); float4 _MSFXGrab_TexelSize, _MSFXGrab_ST;
MOCHIE_DECLARE_TEX2D_SCREENSPACE(_CameraDepthTexture); float4 _CameraDepthTexture_TexelSize;

float SampleDepthTex(float2 uv){
	uv.y = _ProjectionParams.x * 0.5 + 0.5 - uv.y * _ProjectionParams.x;
    return MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, uv);
}

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

// Audio Link
MOCHIE_DECLARE_TEX2D(_AudioTexture);
float _AudioLinkStrength;
float _AudioLinkMin, _AudioLinkMax;

int _AudioLinkFilteringBand;
float _AudioLinkFilteringStrength;
float _AudioLinkFilteringMin, _AudioLinkFilteringMax;

int _AudioLinkShakeBand;
float _AudioLinkShakeStrength;
float _AudioLinkShakeMin, _AudioLinkShakeMax;

int _AudioLinkDistortionBand;
float _AudioLinkDistortionStrength;
float _AudioLinkDistortionMin, _AudioLinkDistortionMax;

int _AudioLinkBlurBand;
float _AudioLinkBlurStrength;
float _AudioLinkBlurMin, _AudioLinkBlurMax;

int _AudioLinkNoiseBand;
float _AudioLinkNoiseStrength;
float _AudioLinkNoiseMin, _AudioLinkNoiseMax;

// outputs
float3 wPos;
float3 wNorm;
float depth;

#if X_FEATURES
	// Audio Link
	int _AudioLinkZoomBand;
	float _AudioLinkZoomStrength;
	float _AudioLinkZoomMin, _AudioLinkZoomMax;

	int _AudioLinkFogBand;
	float _AudioLinkFogOpacity, _AudioLinkFogRadius;
	float _AudioLinkFogMin, _AudioLinkFogMax;

	int _AudioLinkTriplanarBand;
	float _AudioLinkTriplanarOpacity;
	float _AudioLinkTriplanarRadius;
	float _AudioLinkTriplanarMin, _AudioLinkTriplanarMax;

	int _AudioLinkOutlineBand;
	float _AudioLinkOutlineStrength;
	float _AudioLinkOutlineMin, _AudioLinkOutlineMax;

	int _AudioLinkMiscBand;
	float _AudioLinkMiscStrength;
	float _AudioLinkMiscMin, _AudioLinkMiscMax;

	int _AudioLinkSSTBand;
	float _AudioLinkSSTStrength;
	float _AudioLinkSSTMin, _AudioLinkSSTMax;

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
	float4 _ScreenTex_ST;
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
	MOCHIE_DECLARE_TEX2D_SCREENSPACE(_ZoomGrab);
	int _Zoom, _ZoomUseGlobal, _NeedsZoomPass;
	float _ZoomMinRange, _ZoomMaxRange;
	float _ZoomStr, _ZoomStrR, _ZoomStrG, _ZoomStrB;

	// Extras
	int _OLUseGlobal, _OutlineType, _Shift, _InvertX, _InvertY, _Sobel, _DepthBufferToggle, _SobelClearInner;
	int _AuraSampleCount;
	float _OLMinRange, _OLMaxRange, _AuraFade, _AuraStr;
	float4 _OutlineCol, _BackgroundCol;
	float3 _DBColor;
	float _OutlineThiccS, _OutlineThiccN, _OutlineThresh, _SobelStr;
	float _ShiftX, _ShiftY, _Rotate;
	int _Pulse, _WaveForm, _PulseColor;
	float _PulseSpeed, _NormalMapFilter, _NMFToggle, _NMFOpacity, _DBOpacity;
#endif

struct audioLinkData {
	bool textureExists;
	float bass;
	float lowMid;
	float upperMid;
	float treble;
};

struct appdata {
    float4 vertex : POSITION;
    float4 uv : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
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
	UNITY_VERTEX_INPUT_INSTANCE_ID 
	UNITY_VERTEX_OUTPUT_STEREO
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