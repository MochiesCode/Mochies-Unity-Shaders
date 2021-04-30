#ifndef MOCHIE_STANDARD_INPUT_INCLUDED
#define MOCHIE_STANDARD_INPUT_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityPBSLighting.cginc" // TBD: remove
#include "UnityStandardUtils.cginc"

//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || _PARALLAXMAP)
    #define _TANGENT_TO_WORLD 1
#endif

#if (_DETAIL_MULX2 || _DETAIL_MUL || _DETAIL_ADD || _DETAIL_LERP)
    #define _DETAIL 1
#endif


//---------------------------------------
half4       _Color;
half        _Cutoff;

sampler2D   _MainTex;
float4      _MainTex_ST;
float		_Saturation;

sampler2D   _DetailAlbedoMap;
float4      _DetailAlbedoMap_ST;

sampler2D   _BumpMap;
half        _BumpScale;

sampler2D   _DetailMask;
sampler2D   _DetailNormalMap;
half        _DetailNormalMapScale;

sampler2D   _SpecGlossMap;
sampler2D   _MetallicGlossMap;
half        _Metallic;
float       _Glossiness;
float       _GlossMapScale;

sampler2D   _OcclusionMap;
half        _OcclusionStrength;

sampler2D   _ParallaxMap;
sampler2D	_ParallaxMask;
float2		_UV2Scroll;
float4		_ParallaxMask_ST;
half        _Parallax;
int			_ParallaxSteps;
float		_ParallaxOffset;
float2 		uvOffset;
float2		lightmapOffset;
half        _UVSec;
half		_UV0Rotate;
half		_UV1Rotate;
float2		_UV0Scroll;
float2		_UV1Scroll;

half4       _EmissionColor;
sampler2D   _EmissionMap;
sampler2D   _EmissionMask;
float		_EmissionIntensity;
float2		_UV3Scroll;
float4		_EmissionMask_ST;

float _ReflectionStrength, _SpecularStrength;
float _TSSBias;

sampler2D _PackedMap;
UNITY_DECLARE_TEXCUBE(_ReflCube);
UNITY_DECLARE_TEXCUBE(_ReflCubeOverride);
float4 _ReflCube_HDR, _ReflCubeOverride_HDR;
float _CubeThreshold;
int _RoughnessMult, _MetallicMult, _OcclusionMult, _HeightMult;
int _RoughnessChannel, _MetallicChannel, _OcclusionChannel, _HeightChannel;

#define REFLECTION_FALLBACK defined(_MAPPING_6_FRAMES_LAYOUT)
#define REFLECTION_OVERRIDE defined(_COLOROVERLAY_ON)
#define GSAA_ENABLED defined(FXAA)
#define TSS_ENABLED defined(BLOOM)
#define SSR_ENABLED defined(GRAIN)
#define STOCHASTIC_ENABLED defined(EFFECT_HUE_VARIATION)
#define WORKFLOW_PACKED defined(BLOOM_LENS_DIRT)
#define WORKFLOW_MODULAR defined(_FADING_ON)

#if SSR_ENABLED
	sampler2D _GrabTexture; 
	sampler2D _NoiseTexSSR;
	sampler2D _CameraDepthTexture;
	float4 _GrabTexture_TexelSize;
	float4 _NoiseTexSSR_TexelSize;
	float _EdgeFade;
	float _SSRStrength;
#endif

#if SHADER_TARGET < 50
	#define ddx_fine ddx
	#define ddy_fine ddy
#endif

//-------------------------------------------------------------------------------------
// Input functions

float ChannelCheck(float4 rgba, int channel){
	float selection = 0;
	switch (channel){
		case 0: selection = rgba.r; break;
		case 1: selection = rgba.g; break;
		case 2: selection = rgba.b; break;
		case 3: selection = rgba.a; break;
		default: break;
	}
	return selection;
}

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

float2 Rotate2D(float2 coords, float rot){
	rot *= (UNITY_PI/180.0);
	float sinVal = sin(rot);
	float cosX = cos(rot);
	float2x2 mat = float2x2(cosX, -sinVal, sinVal, cosX);
	mat = ((mat*0.5)+0.5)*2-1;
	coords -= 0.5;
	return mul(coords, mat) + 0.5;
}

float RoundTo(float value, uint fraction){
	return round(value * fraction) / fraction;
}

float2 RoundTo(float2 value, uint fraction0, uint fraction1){
	float x = round(value.x * fraction0) / fraction0;
	float y = round(value.y * fraction0) / fraction1;
	return float2(x,y);
}

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

#define TSS_ENABLED defined(BLOOM)
#define STOCHASTIC_ENABLED defined(EFFECT_HUE_VARIATION)

#include "../Common/Sampling.cginc"

#if STOCHASTIC_ENABLED
	#define tex2D tex2Dstoch
#elif TSS_ENABLED
	#define tex2D tex2Dsuper
#endif

half DetailMask(float2 uv)
{
    return tex2D(_DetailMask, uv).a;
}

half3 Albedo(float4 texcoords)
{
    half3 albedo = _Color.rgb * tex2D(_MainTex, texcoords.xy).rgb;
	albedo = lerp(dot(albedo, float3(0.3,0.59,0.11)), albedo, _Saturation);
#if _DETAIL
    half mask = DetailMask(texcoords.xy);
    half3 detailAlbedo = tex2D(_DetailAlbedoMap, texcoords.zw).rgb;
    #if _DETAIL_MULX2
        albedo *= LerpWhiteTo (detailAlbedo * unity_ColorSpaceDouble.rgb, mask);
    #elif _DETAIL_MUL
        albedo *= LerpWhiteTo (detailAlbedo, mask);
    #elif _DETAIL_ADD
        albedo += detailAlbedo * mask;
    #elif _DETAIL_LERP
        albedo = lerp (albedo, detailAlbedo, mask);
    #endif
#endif
    return albedo;
}

half Alpha(float2 uv)
{
#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    return _Color.a;
#else
    return tex2D(_MainTex, uv).a * _Color.a;
#endif
}


half Occlusion(float2 uv)
{
#if WORKFLOW_PACKED
	half occ = tex2D(_PackedMap, uv).r;
	return LerpOneTo(occ, lerp(1, _OcclusionStrength, _OcclusionMult));
#elif WORKFLOW_MODULAR
	half4 map = tex2D(_PackedMap, uv);
	half occ = ChannelCheck(map, _OcclusionChannel);
	return LerpOneTo(occ, lerp(1, _OcclusionStrength, _OcclusionMult));
#else
	half occ = tex2D(_OcclusionMap, uv).g;
	return LerpOneTo (occ, _OcclusionStrength);
#endif
}

half2 MetallicRough(float2 uv)
{
	half2 mg;
	#if WORKFLOW_PACKED
		float3 packedMap = tex2D(_PackedMap, uv);
		packedMap.g *= lerp(1, _Glossiness, _RoughnessMult);
		packedMap.b *= lerp(1, _Metallic, _MetallicMult);
		mg.r = packedMap.b;
		mg.g = 1-packedMap.g;
		return mg;
	#elif WORKFLOW_MODULAR
		float4 packedMap = tex2D(_PackedMap, uv);
		float rough = ChannelCheck(packedMap, _RoughnessChannel);
		float metal = ChannelCheck(packedMap, _MetallicChannel);
		rough *= lerp(1, _Glossiness, _RoughnessMult);
		metal *= lerp(1, _Metallic, _MetallicMult);
		mg.r = metal;
		mg.g = 1-rough;
		return mg;
	#else
		#ifdef _METALLICGLOSSMAP
			mg.r = tex2D(_MetallicGlossMap, uv).r * _Metallic;
		#else
			mg.r = _Metallic;
		#endif

		#ifdef _SPECGLOSSMAP
			mg.g = 1.0f - (tex2D(_SpecGlossMap, uv).r * _Glossiness);
		#else
			mg.g = 1.0f - _Glossiness;
		#endif
		return mg;
	#endif
}

half3 Emission(float2 uv, float2 uvMask)
{
#ifndef _EMISSION
    return 0;
#else
	float3 emissTex = tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb * _EmissionIntensity;
	float emissMask = tex2D(_EmissionMask, uvMask).r;
    return emissTex * emissMask;
#endif
}

#ifdef _NORMALMAP
half3 NormalInTangentSpace(float4 texcoords)
{
	half3 normalTangent = UnpackScaleNormal(tex2D(_BumpMap, texcoords.xy), _BumpScale);

#if _DETAIL && defined(UNITY_ENABLE_DETAIL_NORMALMAP)
    half mask = DetailMask(texcoords.xy);
    half3 detailNormalTangent = UnpackScaleNormal(tex2D(_DetailNormalMap, texcoords.zw), _DetailNormalMapScale);
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

	#if WORKFLOW_PACKED || WORKFLOW_MODULAR
		#if WORKFLOW_PACKED
			half h = tex2D(_PackedMap, texcoords.xy).a + _ParallaxOffset;
		#else
			half4 packedMap = tex2D(_PackedMap, texcoords.xy);
			half h = ChannelCheck(packedMap, _HeightChannel) + _ParallaxOffset;
		#endif
		h = clamp(h, 0, 0.999);
		float2 maskUV = TRANSFORM_TEX(texcoords, _ParallaxMask) + (_Time.y*_UV2Scroll);
		half m = tex2D(_ParallaxMask, maskUV);
		_Parallax = lerp(0.02, _Parallax, _HeightMult);
		offset = ParallaxOffsetMultiStep(h, _Parallax * m, texcoords.xy, viewDir);
		return float4(texcoords.xy + offset, texcoords.zw + offset);
	#else
		half h = tex2D(_ParallaxMap, texcoords.xy).g + _ParallaxOffset;
		h = clamp(h, 0, 0.999);
		float2 maskUV = TRANSFORM_TEX(texcoords, _ParallaxMask) + (_Time.y*_UV2Scroll);
		half m = tex2D(_ParallaxMask, maskUV);
		offset = ParallaxOffsetMultiStep(h, _Parallax * m, texcoords.xy, viewDir);
		return float4(texcoords.xy + offset, texcoords.zw + offset);
	#endif
	return texcoords;
}

#if STOCHASTIC_ENABLED
	#undef tex2D
#elif TSS_ENABLED
	#undef tex2D
#endif

#endif // UNITY_STANDARD_INPUT_INCLUDED
