#ifndef MOCHIE_STANDARD_INPUT_INCLUDED
#define MOCHIE_STANDARD_INPUT_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityPBSLighting.cginc" // TBD: remove
#include "UnityStandardUtils.cginc"
#include "MochieStandardKeyDefines.cginc"
#include "../Common/Utilities.cginc"
#include "../Common/Color.cginc"

#if SSR_ENABLED || DECAL_ENABLED
	sampler2D _CameraDepthTexture;
#endif

#include "../Common/Sampling.cginc"
#include "MochieStandardSampling.cginc"

//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || _PARALLAXMAP)
    #define _TANGENT_TO_WORLD 1
#endif

#if (_DETAIL_MULX2 || _DETAIL_MUL || _DETAIL_ADD || _DETAIL_LERP)
    #define _DETAIL 1
#endif
//---------------------------------------

half4       	_Color;
half        	_Cutoff;
float			_Saturation;

Texture2D   	_BumpMap;
half        	_BumpScale;

Texture2D   	_DetailMask;
Texture2D   	_DetailAlbedoMap;
float4      	_DetailAlbedoMap_ST;
int				_DetailAlbedoBlend;
Texture2D   	_DetailNormalMap;
half        	_DetailNormalMapScale;
Texture2D		_DetailRoughnessMap;
int				_DetailRoughBlend;
Texture2D		_DetailAOMap;
int				_DetailAOBlend;

Texture2D   	_SpecGlossMap;
Texture2D   	_MetallicGlossMap;
half        	_Metallic;
float       	_Glossiness;
float       	_GlossMapScale;

Texture2D   	_OcclusionMap;
half        	_OcclusionStrength;

Texture2D   	_ParallaxMap;
Texture2D		_ParallaxMask;
float2			_UV2Scroll;
float4			_ParallaxMask_ST;
half        	_Parallax;
int				_ParallaxSteps;
float			_ParallaxOffset;
float2 			uvOffset;
float2			lightmapOffset;
half        	_UVSec;
half			_UV0Rotate;
half			_UV1Rotate;
float2			_UV0Scroll;
float2			_UV1Scroll;

half4       	_EmissionColor;
Texture2D   	_EmissionMap;
Texture2D   	_EmissionMask;
float			_EmissionIntensity;
float2			_UV3Scroll;
float4			_EmissionMask_ST;

Texture2D 		_ThicknessMap;
float 			_ThicknessMapPower;
float3 			_ScatterCol;
float 			_ScatterAmbient;
float 			_ScatterIntensity;
float 			_ScatterPow;
float 			_ScatterDist; 
float			_WrappingFactor;
int 			_ScatterAlbedoTint;
int 			_Subsurface;

float _ReflectionStrength, _SpecularStrength;

Texture2D _PackedMap;
UNITY_DECLARE_TEXCUBE(_ReflCube);
UNITY_DECLARE_TEXCUBE(_ReflCubeOverride);
float4 _ReflCube_HDR, _ReflCubeOverride_HDR;
float _CubeThreshold;
int _RoughnessMult, _MetallicMult, _OcclusionMult, _HeightMult;
int _RoughnessChannel, _MetallicChannel, _OcclusionChannel, _HeightChannel;

#if SSR_ENABLED
	sampler2D _GrabTexture; 
	sampler2D _NoiseTexSSR;
	float4 _GrabTexture_TexelSize;
	float4 _NoiseTexSSR_TexelSize;
	float _EdgeFade;
	float _SSRStrength;
#endif

//-------------------------------------------------------------------------------------
// Input functions

struct VertexInput
{
    float4 vertex   : POSITION;
    half3 normal    : NORMAL;
    float2 uv0      : TEXCOORD0;
    float2 uv1      : TEXCOORD1;
	#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
		float2 uv2      : TEXCOORD2;
	#endif
	#ifdef _TANGENT_TO_WORLD
		half4 tangent   : TANGENT;
	#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

void TexCoords(VertexInput v, inout float4 texcoord, inout float4 texcoord1)
{
	texcoord.xy = Rotate2D(v.uv0, _UV0Rotate);
	texcoord.xy = TRANSFORM_TEX(texcoord.xy, _MainTex);
	texcoord.xy += _Time.y * _UV0Scroll;

	texcoord.zw = Rotate2D((_UVSec == 0 ? v.uv0 : v.uv1), _UV1Rotate);
	texcoord.zw = TRANSFORM_TEX(texcoord.zw, _DetailAlbedoMap);
	texcoord.zw += _Time.y * _UV1Scroll;

	#ifdef _PARALLAXMAP
		texcoord1.xy = TRANSFORM_TEX(v.uv0, _ParallaxMask);
		texcoord1.xy += _Time.y * _UV2Scroll;
	#endif

	#ifdef _EMISSION
		texcoord1.zw = TRANSFORM_TEX(v.uv0, _EmissionMask);
		texcoord1.zw += _Time.y * _UV3Scroll;
	#endif
}

half DetailMask(float2 uv)
{
    return _DetailMask.Sample(sampler_MainTex, uv).a;
}

half3 Albedo(float4 texcoords, SampleData sd)
{
	half3 albedo = _Color.rgb * SampleTexture(_MainTex, texcoords.xy, sd).rgb;
	albedo = lerp(dot(albedo, float3(0.3,0.59,0.11)), albedo, _Saturation);
	#if _DETAIL
		half mask = DetailMask(texcoords.xy);
		sd.scaleTransform = _DetailAlbedoMap_ST;
		sd.rotation = _UV1Rotate;
		half3 detailAlbedo = SampleTexture(_DetailAlbedoMap, texcoords.zw, sd).rgb;
		albedo = BlendColors(albedo, detailAlbedo, _DetailAlbedoBlend);
	#endif
    return albedo;
}

half Alpha(float2 uv, SampleData sd)
{
	#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
		return _Color.a;
	#else
		return SampleTexture(_MainTex, uv, sd).a * _Color.a;
	#endif
}


half Occlusion(float4 uv, SampleData sd)
{
	#if WORKFLOW_PACKED
		half4 map = SampleTexture(_PackedMap, uv.xy, sd);
		half occ = ChannelCheck(map, _OcclusionChannel);
		#if DETAIL_AO
			half occDetail = SampleTexture(_DetailAOMap, uv.zw, sd);
			occ = BlendScalars(occ, occDetail, _DetailAOBlend);
		#endif
		return LerpOneTo(occ, lerp(1, _OcclusionStrength, _OcclusionMult));
	#else
		half occ = SampleTexture(_OcclusionMap, uv.xy, sd).g;
		#if DETAIL_AO
			half occDetail = SampleTexture(_DetailAOMap, uv.zw, sd).g;
			occ = BlendScalars(occ, occDetail, _DetailAOBlend);
		#endif
		return LerpOneTo (occ, _OcclusionStrength);
	#endif
}

half2 MetallicRough(float4 uv, SampleData sd)
{
	half2 mg;
	#if WORKFLOW_PACKED
		half4 packedMap = SampleTexture(_PackedMap, uv.xy, sd);
		float rough = ChannelCheck(packedMap, _RoughnessChannel);
		float metal = ChannelCheck(packedMap, _MetallicChannel);
		#if TRIPLANAR_ENABLED
			metal = smoothstep(0,0.1, metal);
		#endif
		#if DETAIL_ROUGH
			float detailRough = SampleTexture(_DetailRoughnessMap, uv.zw, sd).r;
			rough = BlendScalars(rough, detailRough, _DetailRoughBlend);
		#endif
		rough *= lerp(1, _Glossiness, _RoughnessMult);
		metal *= lerp(1, _Metallic, _MetallicMult);
		mg.r = metal;
		mg.g = 1-rough;
		return mg;
	#else
		#ifdef _METALLICGLOSSMAP
			mg.r = SampleTexture(_MetallicGlossMap, uv.xy, sd).r * _Metallic;
			#if TRIPLANAR_ENABLED
				mg.r = smoothstep(0,0.1,mg.r);
			#endif
		#else
			mg.r = _Metallic;
		#endif

		#ifdef _SPECGLOSSMAP
			mg.g = SampleTexture(_SpecGlossMap, uv.xy, sd).r * _Glossiness;
		#else
			mg.g = _Glossiness;
		#endif

		#if DETAIL_ROUGH
			float detailRough = SampleTexture(_DetailRoughnessMap, uv.zw, sd).r;
			mg.g = BlendScalars(mg.g, detailRough, _DetailRoughBlend);
		#endif

		mg.g = 1-mg.g;
		return mg;
	#endif
}

half3 Emission(float2 uv, float2 uvMask, SampleData sd)
{
	#ifdef _EMISSION
		float3 emissTex = SampleTexture(_EmissionMap, uv, sd);
		float emissMask = _EmissionMask.Sample(sampler_MainTex, uvMask).r;
		emissTex *= _EmissionColor.rgb * _EmissionIntensity * emissMask;
		return emissTex;
	#else
		return 0;
	#endif
}

#ifdef _NORMALMAP
half3 NormalInTangentSpace(float4 texcoords, SampleData sd)
{
	half3 normalTangent = SampleTexture(_BumpMap, texcoords.xy, sd, _BumpScale);
	#if _DETAIL && defined(UNITY_ENABLE_DETAIL_NORMALMAP)
		sd.scaleTransform = _DetailAlbedoMap_ST;
		sd.rotation = _UV1Rotate;
		half mask = DetailMask(texcoords.xy);
		half3 detailNormalTangent = SampleTexture(_DetailNormalMap, texcoords.zw, sd, _DetailNormalMapScale);
		#if _DETAIL_LERP
			normalTangent = lerp(
				normalTangent,
				detailNormalTangent,
				mask);
		#else
			normalTangent = lerp(
				normalTangent,
				BlendNormals(normalTangent, detailNormalTangent),
				mask);
		#endif
	#endif

    return normalTangent;
}
#endif

//----------------------------
// Parallax Mapping
//----------------------------

#include "MochieStandardParallax.cginc"

float4 Parallax (float4 texcoords, half3 viewDir, out float2 offset)
{
	offset = 0;

	#ifndef _PARALLAXMAP
		return texcoords;
	#endif

	#if WORKFLOW_PACKED
		half4 packedMap = SampleTexture(_PackedMap, texcoords.xy);
		half h = ChannelCheck(packedMap, _HeightChannel) + _ParallaxOffset;
		h = clamp(h, 0, 0.999);
		float2 maskUV = TRANSFORM_TEX(texcoords, _ParallaxMask) + (_Time.y*_UV2Scroll);
		half m = _ParallaxMask.Sample(sampler_MainTex, maskUV);
		_Parallax = lerp(0.02, _Parallax, _HeightMult);
		offset = ParallaxOffsetMultiStep(h, _Parallax * m, texcoords.xy, viewDir);
		return float4(texcoords.xy + offset, texcoords.zw + offset);
	#else
		half h = SampleTexture(_ParallaxMap, texcoords.xy).g + _ParallaxOffset;
		h = clamp(h, 0, 0.999);
		float2 maskUV = TRANSFORM_TEX(texcoords, _ParallaxMask) + (_Time.y*_UV2Scroll);
		half m = _ParallaxMask.Sample(sampler_MainTex, maskUV);
		offset = ParallaxOffsetMultiStep(h, _Parallax * m, texcoords.xy, viewDir);
		return float4(texcoords.xy + offset, texcoords.zw + offset);
	#endif
	return texcoords;
}

#endif // UNITY_STANDARD_INPUT_INCLUDED
