#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"
#include "../Common/Sampling.cginc"

#define EPSILON 1.192092896e-07

// UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
MOCHIE_DECLARE_TEX2D_SCREENSPACE(_GlassGrab);
sampler2D _RainSheet;
sampler2D _BaseColor;
sampler2D _RoughnessMap;
sampler2D _OcclusionMap;
sampler2D _MetallicMap;
sampler2D _NormalMap;
sampler2D _RainMask;
sampler2D _DropletMask;
float4 _RainPackedMap_ST;
float4 _RainMask_ST;
float4 _RoughnessMap_ST;
float4 _OcclusionMap_ST;
float4 _MetallicMap_ST;
float4 _NormalMap_ST;
float4 _RainSheet_ST;
float4 _BaseColor_ST;
float4 _BaseColorTint;
float4 _SpecularityTint;
float4 _GrabpassTint;
float _NormalStrength;
float _Roughness;
float _Metallic;
float _Occlusion;
float _Rows, _Columns;
float _XScale, _YScale;
float _Strength, _Speed;
float _Refraction;
float _Blur;
float _RainMaskChannel;
float _RippleStrength;
float _RippleScale;
float _RippleSpeed;
float _DynamicDroplets;
float _RainBias;
float _RippleSize;
float _RippleDensity;
float _RainThreshold;
float _RainThresholdSize;
float _TexCoordSpace;
float _TexCoordSpaceSwizzle;
float _GlobalTexCoordScale;
float _Test;

struct appdata {
	float4 vertex : POSITION;
	float4 uv : TEXCOORD0;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {
	float4 pos : SV_POSITION;
	float4 uv : TEXCOORD0;
	float4 uvGrab : TEXCOORD1;
	float3 worldPos : TEXCOORD2;
	float3 binormal : TEXCOORD3;
	float4 localPos : TEXCOORD4;
	float3 cameraPos : TEXCOORD5;
	float3 normal : NORMAL;
	float4 tangent: TANGENT;
	
	UNITY_FOG_COORDS(10)
	UNITY_SHADOW_COORDS(11)
	UNITY_VERTEX_INPUT_INSTANCE_ID 
	UNITY_VERTEX_OUTPUT_STEREO
};

#include "../Common/Sampling.cginc"
#include "../Common/Utilities.cginc"
#include "GlassFunctions.cginc"