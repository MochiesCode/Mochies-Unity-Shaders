#include "UnityCG.cginc"

// Global
sampler2D _MSFXGrab; float4 _MSFXGrab_TexelSize, _MSFXGrab_ST;
sampler2D _CameraDepthTexture; float4 _CameraDepthTexture_TexelSize;
int _BlendMode;
float _MinRange, _MaxRange;
float _Opacity;

// Color Filtering
int _FilterModel, _AutoShift, _ColorUseGlobal, _RoundingToggle;
float4 _Color;
float _ColorMinRange, _ColorMaxRange, _Rounding, _RoundingOpacity;
float _RAmt, _GAmt, _BAmt;
float _Exposure, _Contrast, _HDR;
float _Invert, _InvertR, _InvertG, _InvertB, _Noise, _SaturationRGB;
float _Hue, _AutoShiftSpeed, _SaturationHSL, _Luminance, _HSLMin, _HSLMax;

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
int _FocusPlayer, _BlurUseGlobal, _BlurY, _BlurSamples;
int _BlurModel, _RGBSplit, _Flicker, _DoF;
float _BlurMinRange, _BlurMaxRange;
float _BlurOpacity, _BlurStr;
float _DoFP2O, _DoFRadius, _DoFFade;
float _FlickerSpeedX, _FlickerSpeedY;
float _PixelationStr, _RippleGridStr;
float _BlurRadius;
float _CrushContrast;

// outputs
float3 wPos;
float3 wNorm;
float depth;

#if defined(SFXX)
	#include "SFXXDefines.cginc"
#endif

struct appdata {
    float4 vertex : POSITION;
    float4 uv : TEXCOORD0;
	float4 tangent : TANGENT;
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
};


static const float divisor[19] = {
	2.0,
	2.666666,
	3.0,
	3.175,
	3.333333,
	3.45,
	3.5,
	3.575,
	3.6,
	3.666666,
	3.7,
	3.725,
	3.75,
	3.775,
	3.8,
	3.8,
	3.8,
	3.8,
	3.8
};

#include "SFXUtilities.cginc"
#include "SFXFunctions.cginc"
#include "SFXPass.cginc"