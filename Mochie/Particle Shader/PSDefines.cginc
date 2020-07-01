#ifndef P_DEFINES_INCLUDED
#define P_DEFINES_INCLUDED

#include "UnityCG.cginc"

sampler2D _CameraDepthTexture;
sampler2D _MainTex;
float4 _Color, _SecondColor;
int _Falloff, _IsCutout, _BlendMode, _Softening, _Pulse, _Waveform, _FlipbookBlending;
float _MinRange, _MaxRange, _NearMinRange, _NearMaxRange, _Cutout;
float _SoftenStr, _PulseStr, _PulseSpeed, fade;
float _Brightness, _Opacity;
float _NaNLmao;

#if defined(PSX)
	sampler2D _PSGrab; float4 _PSGrab_TexelSize;
	sampler2D _SecondTex;
	sampler2D _NormalMap;
	float2 _NormalMapScale, _DistortionSpeed;
	int _Distortion, _DistortMainTex, _TexBlendMode, _Filtering, _AutoShift, _Layering;
	float _DistortionStr, _DistortionBlend;
	float _Hue, _Saturation, _Value, _Contrast, _HDR, _AutoShiftSpeed;
#endif

struct appdata {
    float3 vertex : POSITION;
    float4 uv0 : TEXCOORD0;
	float4 color : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {
    float4 pos : SV_POSITION;
    float4 uv0 : TEXCOORD0;
	float4 uv1 : TEXCOORD1;
    float falloff : TEXCOORD2;
    float4 projPos : TEXCOORD3;
	float pulse : TEXCOORD4;
	float4 color : COLOR;
    UNITY_FOG_COORDS(6)
	UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};


#include "../Common/Utilities.cginc"
#include "../Common/Color.cginc"
#if defined(PSX)
	#include "PSXFeatures.cginc"
#endif
#include "PSFunctions.cginc"

#endif