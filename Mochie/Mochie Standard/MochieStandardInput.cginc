// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

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
float2		_ParallaxMaskScroll;
float4		_ParallaxMask_ST;
half        _Parallax;
int			_ParallaxSteps;
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
int _SpectrumInput;
float _SpectrumValue, _SpectrumStrength;

int _DoubleBoxMode;
float3 _BoxOffset;
float _ReflectionStrength, _SpecularStrength;

sampler2D _PackedMap;
UNITY_DECLARE_TEXCUBE(_ReflCube);
float _CubeThreshold;
int _RoughnessMult, _MetallicMult, _OcclusionMult, _HeightMult;

#define REFLECTION_FALLBACK defined(_MAPPING_6_FRAMES_LAYOUT)
#define GSAA_ENABLED defined(FXAA)
#define SSR_ENABLED defined(GRAIN)
// #define WORKFLOW_PACKED defined(BLOOM_LENS_DIRT)
#if SSR_ENABLED
	sampler2D _MSSRGrab; 
	sampler2D _NoiseTexSSR;
	sampler2D _CameraDepthTexture;
	float4 _MSSRGrab_TexelSize;
	float4 _NoiseTexSSR_TexelSize;
	float _EdgeFade;
	float _SSRStrength;
#endif
#if REFLECTION_FALLBACK
	UNITY_DECLARE_TEXCUBE(_ReflCubeMask);
#endif
#if SHADER_TARGET < 50
	#define ddx_fine ddx
	#define ddy_fine ddy
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

float2 Rotate2D(float2 coords, float rot){
	rot *= (UNITY_PI/180.0);
	float sinVal = sin(rot);
	float cosX = cos(rot);
	float2x2 mat = float2x2(cosX, -sinVal, sinVal, cosX);
	mat = ((mat*0.5)+0.5)*2-1;
	return mul(coords, mat);
}

float RoundTo(float value, uint fraction){
	return round(value * fraction) / fraction;
}

float2 RoundTo(float2 value, uint fraction0, uint fraction1){
	float x = round(value.x * fraction0) / fraction0;
	float y = round(value.y * fraction0) / fraction1;
	return float2(x,y);
}

float4 TexCoords(VertexInput v)
{
    float4 texcoord;
    texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex); // Always source from uv0
    texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
	texcoord.xy = Rotate2D(texcoord.xy, _UV0Rotate);
	texcoord.zw = Rotate2D(texcoord.zw, _UV1Rotate);
	texcoord.xy += _Time.y * _UV0Scroll;
	texcoord.zw += _Time.y * _UV1Scroll;
    return texcoord;
}

half DetailMask(float2 uv)
{
    return tex2D (_DetailMask, uv).a;
}

half3 Albedo(float4 texcoords)
{
    half3 albedo = _Color.rgb * tex2D (_MainTex, texcoords.xy).rgb;
	albedo = lerp(dot(albedo, float3(0.3,0.59,0.11)), albedo, _Saturation);
#if _DETAIL
    #if (SHADER_TARGET < 30)
        // SM20: instruction count limitation
        // SM20: no detail mask
        half mask = 1;
    #else
        half mask = DetailMask(texcoords.xy);
    #endif
    half3 detailAlbedo = tex2D (_DetailAlbedoMap, texcoords.zw).rgb;
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
#if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
    return _Color.a;
#else
    return tex2D(_MainTex, uv).a * _Color.a;
#endif
}


half Occlusion(float2 uv)
{
#if (SHADER_TARGET < 30)
    // SM20: instruction count limitation
    // SM20: simpler occlusion
	#ifndef BLOOM_LENS_DIRT
    	return tex2D(_OcclusionMap, uv).g;
	#else
		return tex2D(_PackedMap, uv).r;
	#endif
#else
	#ifndef BLOOM_LENS_DIRT
    	half occ = tex2D(_OcclusionMap, uv).g;
		return LerpOneTo (occ, _OcclusionStrength);
	#else
		half occ = tex2D(_PackedMap, uv).r;
		return LerpOneTo(occ, lerp(1, _OcclusionStrength, _OcclusionMult));
	#endif
    
#endif
}

half4 SpecularGloss(float2 uv)
{
    half4 sg;
#ifdef _SPECGLOSSMAP
    #if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
        sg.rgb = tex2D(_SpecGlossMap, uv).rgb;
        sg.a = tex2D(_MainTex, uv).a;
    #else
        sg = tex2D(_SpecGlossMap, uv);
    #endif
    sg.a *= _GlossMapScale;
#else
    sg.rgb = _SpecColor.rgb;
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        sg.a = tex2D(_MainTex, uv).a * _GlossMapScale;
    #else
        sg.a = _Glossiness;
    #endif
#endif
    return sg;
}

half2 MetallicGloss(float2 uv)
{
    half2 mg;

#ifdef _METALLICGLOSSMAP
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        mg.r = tex2D(_MetallicGlossMap, uv).r;
        mg.g = tex2D(_MainTex, uv).a;
    #else
        mg = tex2D(_MetallicGlossMap, uv).ra;
    #endif
    mg.g *= _GlossMapScale;
#else
    mg.r = _Metallic;
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        mg.g = tex2D(_MainTex, uv).a * _GlossMapScale;
    #else
        mg.g = _Glossiness;
    #endif
#endif
    return mg;
}

half2 MetallicRough(float2 uv)
{
	half2 mg;
	#ifndef BLOOM_LENS_DIRT
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
	#else
		float3 packedMap = tex2D(_PackedMap, uv);
		packedMap.g *= lerp(1, _Glossiness, _RoughnessMult);
		packedMap.b *= lerp(1, _Metallic, _MetallicMult);
		mg.r = packedMap.b;
		mg.g = 1-packedMap.g;
		return mg;
	#endif
}

half3 Emission(float2 uv)
{
#ifndef _EMISSION
    return 0;
#else
	float3 emissTex = tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb;
	emissTex = lerp(emissTex, lerp(0, emissTex, _SpectrumValue), _SpectrumStrength * _SpectrumInput);
    return emissTex * tex2D(_EmissionMask, uv).r;
#endif
}

#ifdef _NORMALMAP
half3 NormalInTangentSpace(float4 texcoords)
{
    half3 normalTangent = UnpackScaleNormal(tex2D (_BumpMap, texcoords.xy), _BumpScale);

#if _DETAIL && defined(UNITY_ENABLE_DETAIL_NORMALMAP)
    half mask = DetailMask(texcoords.xy);
    half3 detailNormalTangent = UnpackScaleNormal(tex2D (_DetailNormalMap, texcoords.zw), _DetailNormalMapScale);
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
float2 ParallaxOffsetMultiStep(float surfaceHeight, float strength, float2 uv, float3 tangentViewDir){
    float2 uvOffset = 0;
	float2 prevUVOffset = 0;
	float stepSize = 1.0/_ParallaxSteps;
	float stepHeight = 1;
	tangentViewDir.xy = Rotate2D(tangentViewDir.xy, _UV0Rotate);
	float2 uvDelta = tangentViewDir.xy * (stepSize * strength);

	#ifdef BLOOM_LENS_DIRT
		float prevStepHeight = stepHeight;
		float prevSurfaceHeight = surfaceHeight;
		[unroll(50)]
		for (int j = 1; j <= _ParallaxSteps && stepHeight > surfaceHeight; j++){
			prevUVOffset = uvOffset;
			prevStepHeight = stepHeight;
			prevSurfaceHeight = surfaceHeight;
			uvOffset -= uvDelta;
			stepHeight -= stepSize;
			surfaceHeight = tex2D(_PackedMap, uv+uvOffset).a;
		}
		[unroll(3)]
		for (int k = 0; k < 3; k++) {
			uvDelta *= 0.5;
			stepSize *= 0.5;

			if (stepHeight < surfaceHeight) {
				uvOffset += uvDelta;
				stepHeight += stepSize;
			}
			else {
				uvOffset -= uvDelta;
				stepHeight -= stepSize;
			}
			surfaceHeight = tex2D(_PackedMap, uv+uvOffset).a;
		}
	#else
		float prevStepHeight = stepHeight;
		float prevSurfaceHeight = surfaceHeight;
		[unroll(50)]
		for (int j = 1; j <= _ParallaxSteps && stepHeight > surfaceHeight; j++){
			prevUVOffset = uvOffset;
			prevStepHeight = stepHeight;
			prevSurfaceHeight = surfaceHeight;
			uvOffset -= uvDelta;
			stepHeight -= stepSize;
			surfaceHeight = tex2D(_ParallaxMap, uv+uvOffset);
		}
		[unroll(3)]
		for (int k = 0; k < 3; k++) {
			uvDelta *= 0.5;
			stepSize *= 0.5;

			if (stepHeight < surfaceHeight) {
				uvOffset += uvDelta;
				stepHeight += stepSize;
			}
			else {
				uvOffset -= uvDelta;
				stepHeight -= stepSize;
			}
			surfaceHeight = tex2D(_ParallaxMap, uv+uvOffset);
		}
	#endif

    return uvOffset;
}

float4 Parallax (float4 texcoords, half3 viewDir, out float2 offset)
{
	offset = 0;
	#ifndef BLOOM_LENS_DIRT
		#if !defined(_PARALLAXMAP) || (SHADER_TARGET < 30)
			return texcoords;
		#else
			half h = tex2D(_ParallaxMap, texcoords.xy).g;
			float2 maskUV = TRANSFORM_TEX(texcoords, _ParallaxMask) + (_Time.y*_ParallaxMaskScroll);
			half m = tex2D(_ParallaxMask, maskUV);
			// _Parallax = lerp(_Parallax, lerp(0, _Parallax, _SpectrumValue), _SpectrumStrength * _SpectrumInput);
			h = clamp(h, 0, 0.999);
			offset = ParallaxOffsetMultiStep(h, _Parallax * m, texcoords.xy, viewDir);
			return float4(texcoords.xy + offset, texcoords.zw + offset);
		#endif
	#else
		#ifndef _PARALLAXMAP
			return texcoords;
		#else
			half h = tex2D(_PackedMap, texcoords.xy).a;
			float2 maskUV = TRANSFORM_TEX(texcoords, _ParallaxMask) + (_Time.y*_ParallaxMaskScroll);
			half m = tex2D(_ParallaxMask, maskUV);
			// _Parallax = lerp(_Parallax, lerp(0, _Parallax, _SpectrumValue), _SpectrumStrength * _SpectrumInput);
			h = clamp(h, 0, 0.999);
			offset = ParallaxOffsetMultiStep(h, _Parallax * m, texcoords.xy, viewDir);
			return float4(texcoords.xy + offset, texcoords.zw + offset);
		#endif
	#endif
	return texcoords;
}

#endif // UNITY_STANDARD_INPUT_INCLUDED
