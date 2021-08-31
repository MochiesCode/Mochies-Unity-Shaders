#ifndef WATER_DEFINES_INCLUDED
#define WATER_DEFINES_INCLUDED

sampler2D_float _CameraDepthTexture;
float4 _CameraDepthTexture_TexelSize;
#define HAS_DEPTH_TEXTURE
#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"
#include "../Common/Color.cginc"
#include "../Common/Utilities.cginc"
#include "../Common/Noise.cginc"

#define NORMALMAP1_ENABLED defined(_NORMALMAP_1_ON)
#define REFLECTIONS_ENABLED defined(_REFLECTIONS_ON)
#define SPECULAR_ENABLED defined(_SPECULAR_ON)
#define PBR_ENABLED defined(_REFLECTIONS_ON) || defined(_SPECULAR_ON)
#define FLOW_ENABLED defined(_FLOW_ON)
#define VERTEX_OFFSET_ENABLED defined(_VERTEX_OFFSET_ON)
#define DEPTHFOG_ENABLED defined(_DEPTHFOG_ON)
#define FOAM_ENABLED defined(_FOAM_ON)
#define CAUSTICS_ENABLED defined(_CAUSTICS_ON)
#define EDGEFADE_ENABLED defined(_EDGEFADE_ON)

sampler2D _MWGrab;
sampler2D _MainTex;
sampler2D _NormalMap0, _NormalMap1;
sampler2D _FlowMap;
sampler2D _NoiseTex;
sampler2D _FoamTex;

float4 _FogTint, _Color, _FoamColor;
float4 _MainTex_ST;
float3 _Offset;
float2 _NormalMapScale0, _NormalMapScale1;
float2 _NormalMapScroll0, _NormalMapScroll1;
float2 _FlowMapScale;
float2 _NoiseTexScale;
float2 _NoiseTexScroll;
float2 _FoamTexScale;
float2 _MainTexScroll;
float _NormalStr0, _NormalStr1;
float _WaveHeight;
float _FlowSpeed, _FlowStrength;
float _Rotation0, _Rotation1;
float _DistortionStrength;
float _Roughness, _Metallic;
float _FoamRoughness, _FogPower;
float _FoamPower, _Opacity;
float _CausticsScale;
float _CausticsSpeed;
float _CausticsPower;
float _CausticsOpacity;
float _CausticsFade;
float _EdgeFadePower, _EdgeFadeOffset;
float _FoamOpacity;
float _NoiseTexBlur;
float _SpecStrength;
float _ReflStrength;

const static float2 jump = float2(0.1, 0.25);

struct appdata {
	float4 vertex : POSITION;
	float4 uv : TEXCOORD0;
	float4 uv1 : TEXCOORD1;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
};

struct v2f {
	float4 pos : SV_POSITION;
	centroid float4 uv : TEXCOORD1;
	float4 uvGrab : TEXCOORD2;
	float3 worldPos : TEXCOORD3;
	float3 normal : TEXCOORD4;
	centroid float3 cNormal : TEXCOORD5;
	float3 tangent : TEXCOORD6;
	float3 binormal : TEXCOORD7;
	float4 localPos : TEXCOORD9;
	#if SPECULAR_ENABLED && defined(UNITY_PASS_FORWARDADD)
		UNITY_SHADOW_COORDS(12)
	#endif
	UNITY_FOG_COORDS(10)
};

#include "WaterFunctions.cginc"

#endif // WATER_DEFINES_INCLUDED