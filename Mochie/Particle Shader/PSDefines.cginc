#ifndef PS_DEFINES_INCLUDED
#define PS_DEFINES_INCLUDED

#include "PSKeyDefines.cginc"
#include "UnityCG.cginc"

sampler2D _CameraDepthTexture;
sampler2D _MainTex;
float4 _Color, _SecondColor;
int _Falloff, _IsCutout, _BlendMode, _Softening, _Pulse, _Waveform, _FlipbookBlending;
int _FalloffMode;
float _MinRange, _MaxRange, _NearMinRange, _NearMaxRange, _Cutoff;
float _SoftenStr, _PulseStr, _PulseSpeed, fade;
float _Brightness, _Opacity;
float _NaNLmao;

sampler2D _GrabTexture; float4 _GrabTexture_TexelSize;
sampler2D _SecondTex;
sampler2D _NormalMap;
float2 _NormalMapScale, _DistortionSpeed;
int _Distortion, _DistortMainTex, _TexBlendMode, _Filtering, _AutoShift, _Layering;
float _DistortionStr, _DistortionBlend;
float _Hue, _Saturation, _Contrast, _HDR, _AutoShiftSpeed;

struct appdata {
    float3 vertex : POSITION;
    float4 uv0 : TEXCOORD0;
	float4 center : TEXCOORD1;
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
	float3 center : TEXCOORD5;
	float3 vertex : TEXCOORD6;
	float4 color : COLOR;
    UNITY_FOG_COORDS(7)
	UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};


#include "../Common/Utilities.cginc"
#include "../Common/Color.cginc"
#include "PSFunctions.cginc"

#endif // PS_DEFINES_INCLUDED