#ifndef PS_DEFINES_INCLUDED
#define PS_DEFINES_INCLUDED

#include "../Common/Sampling.cginc"
#include "PSKeyDefines.cginc"
#include "UnityCG.cginc"

MOCHIE_DECLARE_TEX2D_SCREENSPACE(_CameraDepthTexture);
float4 _CameraDepthTexture_TexelSize;
sampler2D _MainTex;
float4 _Color, _SecondColor;
int _Falloff, _IsCutout, _BlendMode, _Softening, _Pulse, _Waveform, _FlipbookBlending;
int _FalloffMode;
float _MinRange, _MaxRange, _NearMinRange, _NearMaxRange, _Cutoff;
float _SoftenStr, _PulseStr, _PulseSpeed, fade;
float _Brightness, _Opacity;
float _NaNLmao;

MOCHIE_DECLARE_TEX2D_SCREENSPACE(_MPSGrab); float4 _MPSGrab_TexelSize;
sampler2D _SecondTex;
sampler2D _NormalMap;
float4 _SecondTex_ST;
float2 _NormalMapScale, _DistortionSpeed, _SecondTexScroll;
int _Distortion, _DistortMainTex, _TexBlendMode, _Filtering, _AutoShift, _Layering;
float _DistortionStr, _DistortionBlend;
float _Hue, _Saturation, _Contrast, _HDR, _AutoShiftSpeed;

Texture2D _AudioTexture;
SamplerState sampler_AudioTexture;

float _AudioLink;
float _AudioLinkStrength;
float _AudioLinkRemapMin;
float _AudioLinkRemapMax;
float _AudioLinkFilterBand;
float _AudioLinkFilterStrength;
float _AudioLinkRemapFilterMin;
float _AudioLinkRemapFilterMax;
float _AudioLinkDistortionBand;
float _AudioLinkDistortionStrength;
float _AudioLinkRemapDistortionMin;
float _AudioLinkRemapDistortionMax;
float _AudioLinkOpacityBand;
float _AudioLinkOpacityStrength;
float _AudioLinkRemapOpacityMin;
float _AudioLinkRemapOpacityMax;
float _AudioLinkCutoutBand;
float _AudioLinkCutoutStrength;
float _AudioLinkRemapCutoutMin;
float _AudioLinkRemapCutoutMax;

struct appdata {
    float3 vertex : POSITION;
    float4 uv0 : TEXCOORD0;
	float3 center : TEXCOORD1;
	float4 color : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {
    float4 pos : SV_POSITION;
    float4 uv0 : TEXCOORD0;
	#if DISTORTION_ENABLED
		float4 uv1 : TEXCOORD1;
	#endif
	#if FALLOFF_ENABLED
    	float falloff : TEXCOORD2;
		float3 center : TEXCOORD3;
		float3 vertex : TEXCOORD4;
	#endif
	#if FADING_ENABLED
    	float4 projPos : TEXCOORD5;
	#endif
	#if PULSE_ENABLED
		float pulse : TEXCOORD6;
	#endif
	float4 color : COLOR;
    UNITY_FOG_COORDS(8)
	UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

struct audioLinkData {
	bool textureExists;
	float bass;
	float lowMid;
	float upperMid;
	float treble;
};

#include "../Common/Utilities.cginc"
#include "../Common/Color.cginc"
#include "PSFunctions.cginc"

#endif // PS_DEFINES_INCLUDED