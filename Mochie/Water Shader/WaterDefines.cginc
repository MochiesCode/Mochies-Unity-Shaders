#ifndef WATER_DEFINES_INCLUDED
#define WATER_DEFINES_INCLUDED

float _VRChatMirrorMode;

#include "../Common/Sampling.cginc"

MOCHIE_DECLARE_TEX2D_SCREENSPACE(_CameraDepthTexture);
float4 _CameraDepthTexture_TexelSize;
#define HAS_DEPTH_TEXTURE
#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"
#include "../Common/Color.cginc"
#include "../Common/Utilities.cginc"
#include "../Common/Noise.cginc"

#define BASE_PASS 						defined(UNITY_PASS_FORWARDBASE)
#define ADD_PASS 						defined(UNITY_PASS_FORWARDADD)
#define NORMALMAP1_ENABLED 				defined(_NORMALMAP_1_ON)
#define REFLECTIONS_ENABLED 			defined(_REFLECTIONS_ON)
#define REFLECTIONS_MANUAL_ENABLED 		defined(_REFLECTIONS_MANUAL_ON)
#define REFLECTIONS_MIRROR_ENABLED		defined(_REFLECTIONS_MIRROR_ON)
#define SPECULAR_ENABLED 				defined(_SPECULAR_ON)
#define PBR_ENABLED 					defined(_REFLECTIONS_ON) || defined(_SPECULAR_ON)
#define FLOW_ENABLED 					defined(_FLOW_ON)
#define NOISE_TEXTURE_ENABLED			defined(_NOISE_TEXTURE_ON)
#define GERSTNER_ENABLED 				defined(_GERSTNER_WAVES_ON)
#define VORONOI_ENABLED					defined(_VORONOI_ON)
#define VERT_FLIPBOOK_ENABLED			defined(_VERT_FLIPBOOK_ON)
#define VERT_OFFSET_ENABLED				defined(_NOISE_TEXTURE_ON) || defined(_GERSTNER_WAVES_ON) || defined(_VORONOI_ON) || defined(_VERT_FLIPBOOK_ON)
#define DEPTHFOG_ENABLED 				defined(_DEPTHFOG_ON)
#define FOAM_ENABLED 					defined(_FOAM_ON)
#define EDGEFADE_ENABLED 				defined(_EDGEFADE_ON)
#define SSR_ENABLED 					defined(_SCREENSPACE_REFLECTIONS_ON)
#define STOCHASTIC0_ENABLED 			defined(_NORMALMAP_0_STOCHASTIC_ON)
#define STOCHASTIC1_ENABLED 			defined(_NORMALMAP_1_STOCHASTIC_ON)
#define FOAM_STOCHASTIC_ENABLED 		defined(_FOAM_STOCHASTIC_ON)
#define BASECOLOR_STOCHASTIC_ENABLED 	defined(_BASECOLOR_STOCHASTIC_ON)
#define RAIN_ENABLED 					defined(_RAIN_ON)
#define FOAM_NORMALS_ENABLED			defined(_FOAM_NORMALS_ON)
#define DEPTH_EFFECTS_ENABLED			defined(_DEPTH_EFFECTS_ON)
#define EMISSION_ENABLED				defined(_EMISSION_ON)
#define EMISS_STOCHASTIC_ENABLED		defined(_EMISSIONMAP_STOCHASTIC_ON)
#define AREALIT_ENABLED					defined(_AREALIT_ON)
#define DETAIL_NORMAL_ENABLED			defined(_DETAIL_NORMAL_ON)
#define DETAIL_BASECOLOR_ENABLED		defined(_DETAIL_BASECOLOR_ON)
#define TRANSPARENCY_PREMUL				defined(_PREMUL_MODE_ON)
#define TRANSPARENCY_OPAQUE				defined(_OPAQUE_MODE_ON)
#define TRANSPARENCY_GRABPASS			!defined(_PREMUL_MODE_ON) && !defined(_OPAQUE_MODE_ON)
#define CAUSTICS_VORONOI				defined(_CAUSTICS_VORONOI_ON)
#define CAUSTICS_TEXTURE				defined(_CAUSTICS_TEXTURE_ON)
#define CAUSTICS_FLIPBOOK				defined(_CAUSTICS_FLIPBOOK_ON)
#define CAUSTICS_ENABLED				defined(_CAUSTICS_VORONOI_ON) || defined(_CAUSTICS_TEXTURE_ON) || defined(_CAUSTICS_FLIPBOOK_ON)
#define NORMALMAP_FLIPBOOK_MODE			defined(_NORMALMAP_FLIPBOOK_ON)
#define NORMALMAP_FLIPBOOK_STOCH		defined(_NORMALMAP_FLIPBOOK_STOCHASTIC_ON)
#define BICUBIC_LIGHTMAPPING_ENABLED	defined(_BICUBIC_LIGHTMAPPING_ON)
#define AUDIOLINK_ENABLED				defined(_AUDIOLINK_ON)

MOCHIE_DECLARE_TEX2D_SCREENSPACE(_MWGrab);
MOCHIE_DECLARE_TEX2D(_FlowMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_MainTex);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_NormalMap0);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_NormalMap1);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_FoamTex);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_FoamNoiseTex);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_CausticsTex);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_BlendNoise);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_RoughnessMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_MetallicMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_EmissionMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_AreaLitMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailBaseColor);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailNormal);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_OpacityMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_CausticsDistortionTex);

MOCHIE_DECLARE_TEX2DARRAY(_VertOffsetFlipbook);
MOCHIE_DECLARE_TEX2DARRAY_NOSAMPLER(_NormalMapFlipbook);
MOCHIE_DECLARE_TEX2DARRAY_NOSAMPLER(_CausticsTexArray);

sampler2D _NoiseTex;
sampler2D _VertexOffsetMask;
samplerCUBE _ReflCube;

sampler2D _ReflectionTex0;
sampler2D _ReflectionTex1;

float4 _VertexOffsetMask_ST;
float4 _CausticsTex_TexelSize;
float4 _OpacityMask_ST;
float2 _OpacityMaskScroll;
float _AreaLitStrength;
float _AreaLitRoughnessMult;
float4 _AreaLitMask_ST;
float2 _NormalMapFlipbookScale;
float _NormalMapFlipbookStrength;
float _NormalMapFlipbookSpeed;
float2 _VertOffsetFlipbookScale;
float _VertOffsetFlipbookStrength;
float _VertOffsetFlipbookSpeed;
float _CausticsFlipbookSpeed;
float4 _FogTint, _Color, _FoamColor, _ReflTint, _SpecTint, _NonGrabColor;
float4 _ReflCube_HDR;
float4 _MainTex_ST;
float3 _Offset;
float3 _LightDir;
float3 _ReflCubeRotation;
float2 _NormalMapScale0, _NormalMapScale1;
float2 _NormalMapScroll0, _NormalMapScroll1;
float2 _FlowMapScale;
float4 _RoughnessMap_ST;
float4 _MetallicMap_ST;
float4 _DetailBaseColor_ST;
float4 _DetailNormal_ST;
float4 _DetailBaseColorTint;
float2 _DetailScroll;
float4 _EmissionMap_ST;
float3 _EmissionColor;
float2 _EmissionMapScroll;
float _EmissionDistortionStrength;
float2 _VoronoiScale;
float2 _VoronoiScroll;
float _VoronoiSpeed;
float _VoronoiWaveHeight;
float3 _VoronoiOffset;
float4 _BackfaceTint;
float2 _NoiseTexScale;
float2 _NoiseTexScroll;
float2 _FoamTexScale;
float2 _MainTexScroll;
float2 _FoamTexScroll;
float2 _FoamNoiseTexScroll;
float2 _FoamNoiseTexScale;
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
float _FoamEdgeStrength;
float _NoiseTexBlur;
float _SpecStrength;
float _ReflStrength;
float _SSRStrength;
float _EdgeFadeSSR;
float _NormalMapOffset0;
float _NormalMapOffset1;
float _BaseColorOffset;
float _FoamOffset;
float _WaveSpeedGlobal, _WaveSpeed0, _WaveSpeed1, _WaveSpeed2;
float _WaveScaleGlobal, _WaveScale0, _WaveScale1, _WaveScale2;
float _WaveStrengthGlobal, _WaveStrength0, _WaveStrength1, _WaveStrength2;
float _Turbulence, _TurbulenceScale, _TurbulenceSpeed;
float _FoamCrestStrength, _FoamCrestThreshold;
float _FoamNoiseTexCrestStrength, _FoamNoiseTexStrength;
float _BaseColorDistortionStrength;
float _FoamDistortionStrength;
float _VertRemapMin, _VertRemapMax;
float _WaveDirection0, _WaveDirection1, _WaveDirection2;
float _Specular;
float _RippleStr;
float _RippleScale;
float _RippleSpeed;
float _RippleSize;
float _RippleDensity;
float _FoamNormalStrength;
float _CausticsDisp;
float _CausticsDistortion;
float _CausticsDistortionScale;
float2 _CausticsDistortionSpeed;
float3 _CausticsRotation;
float _CausticsSurfaceFade;
float3 _CausticsColor;
float4 _AngleTint;
float _TessMin;
float _TessMax;
float _TessDistMin;
float _TessDistMax;
float _TessellationOffsetMask;
float _DetailNormalStrength;
float2 _BlendNoiseScale;
float _ShadowStrength;
float4 _NonGrabBackfaceTint;
float _FogBrightness;
float _VertexOffsetMaskStrength;
float4 _SubsurfaceTint;
float _SubsurfaceThreshold;
float _SubsurfaceBrightness;
float _SubsurfaceStrength;
float4 _FogTint2;
float _FogPower2;
float _FogBrightness2;
int _VertexOffsetMaskChannel;
int _BlendNoiseSource;
int _FlowMapUV;
int _BackfaceReflections;
int _DetailBaseColorBlend;
int _DetailNormalBlend;
int _DetailTextureMode;
int _RecalculateNormals;
int _TransparencyMode;
int _TexCoordSpace;
int _TexCoordSpaceSwizzle;
int _MirrorNormalOffsetSwizzle;
int _InvertNormals;
float _GlobalTexCoordScaleUV;
float _GlobalTexCoordScaleWorld;
int _VisualizeFlowmap;
int _AudioLink;
int _AudioLinkBand;
float _AudioLinkStrength;

float _Test;
float _ZeroProp;
const static float2 jump = float2(0.1, 0.25);

#ifdef TESSELLATION_VARIANT
struct TessellationControlPoint {
	float4 vertex : INTERNALTESSPOS;
	float4 uv : TEXCOORD0;
	float4 uv1 : TEXCOORD1;
	float4 uv2 : TEXCOORD2;
	float4 uv3 : TEXCOORD3;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};
#endif

struct appdata {
	float4 vertex : POSITION;
	float4 uv : TEXCOORD0;
	float4 uv1 : TEXCOORD1;
	float4 uv2 : TEXCOORD2;
	float4 uv3 : TEXCOORD3;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	UNITY_VERTEX_INPUT_INSTANCE_ID
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
	float3 wave : TEXCOORD10;
	float3 tangentViewDir : TEXCOORD11;
	bool isInVRMirror : TEXCOORD12;
	float2 uvFlow : TEXCOORD13;
	float4 reflUV : TEXCOORD14;
	float2 lightmapUV : TEXCOORD15;
	#ifdef TESSELLATION_VARIANT
		float offsetMask : TEXCOORD16;
	#endif
	UNITY_FOG_COORDS(17)
	UNITY_SHADOW_COORDS(18)
	UNITY_VERTEX_INPUT_INSTANCE_ID 
	UNITY_VERTEX_OUTPUT_STEREO
};

#include "WaterSSR.cginc"
#include "WaterFunctions.cginc"
#include "WaterAudioLink.cginc"
#if AREALIT_ENABLED
	#include "../../AreaLit/Shader/Lighting.hlsl"
#endif

#endif // WATER_DEFINES_INCLUDED