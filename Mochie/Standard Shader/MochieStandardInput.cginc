#ifndef MOCHIE_STANDARD_INPUT_INCLUDED
#define MOCHIE_STANDARD_INPUT_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "MochieStandardPBSLighting.cginc"
#include "UnityStandardUtils.cginc"
#include "MochieStandardKeyDefines.cginc"
#include "../Common/Utilities.cginc"
#include "../Common/Color.cginc"

#if SSR_ENABLED
	UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
#endif

#include "../Common/Sampling.cginc"
#include "MochieStandardSampling.cginc"

#if AREALIT_ENABLED
	#include "../../AreaLit/Shader/Lighting.hlsl"
#endif

#ifdef LTCGI
	#include "Assets/_pi_/_LTCGI/Shaders/LTCGI.cginc"
#endif

//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || _PARALLAXMAP) || defined(BAKERY_LMSPEC) && defined(BAKERY_RNM)
    #define _TANGENT_TO_WORLD 1
#endif
//---------------------------------------

Texture2D		_AlphaMask;
float			_AlphaMaskOpacity;
half4       	_Color;
half4			_DetailColor;
half        	_Cutoff;
int				_UseAlphaMask;
int 			_AlphaMaskChannel;
float			_Saturation;
float			_Hue;
float			_Contrast;
float			_Brightness;
float			_ACES;
float			_SaturationDet;
float			_HueDet;
float			_ContrastDet;
float			_BrightnessDet;
float			_SaturationEmiss;
float			_HueEmiss;
float			_ContrastEmiss;
float			_BrightnessEmiss;
float			_HuePost;
float			_SaturationPost;
float			_BrightnessPost;
float			_ContrastPost;
float 			_FresnelStrength;
int				_UseFresnel;

Texture2D   	_BumpMap;
half        	_BumpScale;

Texture2D   	_DetailMask;
float4			_DetailMask_ST;
int				_DetailMaskChannel;
int				_DetailAlbedoBlend;
half        	_DetailNormalMapScale;
int				_DetailRoughBlend;
int				_DetailMetallicBlend;
int				_DetailAOBlend;
float			_DetailMetallicStrength;
float			_DetailOcclusionStrength;
float			_DetailRoughnessStrength;
int 			_UVDetailMask;
float2 			_DetailScroll;
float 			_DetailRotate;


Texture2D   	_SpecGlossMap;
Texture2D   	_MetallicGlossMap;
half        	_Metallic;
float       	_Glossiness;
float       	_GlossMapScale;

Texture2D   	_OcclusionMap;
half        	_OcclusionStrength;

Texture2D   	_ParallaxMap;
Texture2D		_ParallaxMask;
float4			_ParallaxMask_ST;
half        	_Parallax;
int				_ParallaxSteps;
float			_ParallaxOffset;

float4			_AlphaMask_ST;
float2 			uvOffset;
float2			lightmapOffset;
half        	_UVSec;
half			_UVPri;
half			_UVEmissMask;
half			_UVHeightMask;
half			_UVAlphaMask;
half			_UV0Rotate;
half			_UV1Rotate;
half			_UV3Rotate;
half			_UV4Rotate;
float2			_UV0Scroll;
float2			_UV1Scroll;
float2			_UV2Scroll;
float2			_UV3Scroll;
float2			_UV4Scroll;

half4       	_EmissionColor;
Texture2D   	_EmissionMap;
Texture2D   	_EmissionMask;
float			_EmissionIntensity;

float4			_EmissionMask_ST;
int				_EmissPulseWave;
float			_EmissPulseStrength;
float			_EmissPulseSpeed;

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

float			_BrightnessReflShad;
float			_ContrastReflShad;
float			_HDRReflShad;
float3			_TintReflShad;

Texture2D		_RainMask;
float4			_RainMask_ST;
float 			_RippleStr;
float			_RippleScale;
float			_RippleSpeed;
float			_UV5Rotate;
float2			_UV5Scroll;
int				_UVRainMask;
int				_RainToggle;

int _Filtering;

float _ReflectionStrength, _SpecularStrength;
float _ReflShadowStrength;
float _GSAAStrength;
float _ReflVertexColorStrength;
int _ReflShadows;
int _UseSmoothness;
int _DetailUseSmoothness;
int _ReflVertexColor;
int _GSAA;

float _LTCGIStrength;
float _AreaLitStrength;
float _AreaLitRoughnessMult;
sampler2D _AreaLitOcclusion;
float4 _AreaLitOcclusion_ST;
int _OcclusionUVSet;

sampler2D _RimMask;
float4 _RimMask_ST;
float3 _RimCol;
float2 _UVRimMaskScroll;
float _UVRimMaskRotate;
float _RimWidth;
float _RimEdge;
float _RimStr;
int _RimBlending;
int _RimToggle;
int _UVRimMask;

Texture2D _RNM0, _RNM1, _RNM2;
SamplerState sampler_RNM0, sampler_RNM1, sampler_RNM2;
float4 _RNM0_TexelSize;

Texture2D _PackedMap;
Texture2D _DetailPackedMap;
int _ReflCubeToggle;
int _ReflCubeOverrideToggle;
UNITY_DECLARE_TEXCUBE(_ReflCube);
UNITY_DECLARE_TEXCUBE(_ReflCubeOverride);
float4 _ReflCube_HDR, _ReflCubeOverride_HDR;
float _CubeThreshold;
int _RoughnessMult, _MetallicMult, _OcclusionMult, _HeightMult;
int _RoughnessChannel, _MetallicChannel, _OcclusionChannel, _HeightChannel;
int _DetailRoughnessMult, _DetailMetallicMult, _DetailOcclusionMult;
int _DetailRoughnessChannel, _DetailMetallicChannel, _DetailOcclusionChannel;

float3 shadowedReflections;

#if MIRROR_ENABLED
	sampler2D _ReflectionTex0;
	sampler2D _ReflectionTex1;
#endif

#if SSR_ENABLED
	sampler2D _GrabTexture; 
	sampler2D _NoiseTexSSR;
	float4 _GrabTexture_TexelSize;
	float4 _NoiseTexSSR_TexelSize;
	float _EdgeFade;
	float _SSRStrength;
#endif

#if AUDIOLINK_ENABLED
	#ifndef LTCGI
		Texture2D	_AudioTexture;
	#endif
	SamplerState	sampler_AudioTexture;
	int				_AudioLinkEmission;
	float			_AudioLinkEmissionStrength;
#endif

float GSAARoughness(float3 normal, float roughness){
	float3 normalDDX = ddx(normal);
	float3 normalDDY = ddy(normal); 
	float dotX = dot(normalDDX, normalDDX);
	float dotY = dot(normalDDY, normalDDY);
	float base = saturate(max(dotX, dotY));
	return max(roughness, pow(base, 0.333)*_GSAAStrength);
}

//-------------------------------------------------------------------------------------
// Input functions

struct VertexInput
{
    float4 vertex   : POSITION;
    half3 normal    : NORMAL;
	float4 color	: COLOR_Centroid;
    float2 uv0      : TEXCOORD0;
    float2 uv1      : TEXCOORD1;
	float2 uv2      : TEXCOORD2;
	float2 uv3		: TEXCOORD3;
	float2 uv4		: TEXCOORD4;
	#ifdef _TANGENT_TO_WORLD
		half4 tangent   : TANGENT;
	#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

#if AUDIOLINK_ENABLED
struct audioLinkData {
	bool textureExists;
	float bass;
	float lowMid;
	float upperMid;
	float treble;
};

float GetAudioLinkBand(audioLinkData al, int band){
	float4 bands = float4(al.bass, al.lowMid, al.upperMid, al.treble);
	return bands[band-1];
}

void GrabExists(inout audioLinkData al, inout float versionBand, inout float versionTime){
	float width = 0;
	float height = 0;
	_AudioTexture.GetDimensions(width, height);
	if (width > 64){
		versionBand = 0.0625;
		versionTime = 0.25;
	}
	al.textureExists = width > 16;
}

float SampleAudioTexture(float time, float band){
	return MOCHIE_SAMPLE_TEX2D_LOD(_AudioTexture, float2(time,band),0);
}

void InitializeAudioLink(inout audioLinkData al, float time){
	float versionBand = 1;
	float versionTime = 1;
	GrabExists(al, versionBand, versionTime);
	if (al.textureExists){
		time *= versionTime;
		al.bass = SampleAudioTexture(time, 0.125 * versionBand);
		al.lowMid = SampleAudioTexture(time, 0.375 * versionBand);
		al.upperMid = SampleAudioTexture(time, 0.625 * versionBand);
		al.treble = SampleAudioTexture(time, 0.875 * versionBand);
	}
}
#endif

float2 SelectUVSet(VertexInput v, int selection){
	float2 uvs[] = {v.uv0, v.uv1, v.uv2, v.uv3, v.uv4};
	return uvs[selection];
}

void TexCoords(VertexInput v, inout float4 texcoord, inout float4 texcoord1, inout float4 texcoord2, inout float4 texcoord3, inout float4 texcoord4)
{
	texcoord.xy = Rotate2D(SelectUVSet(v, _UVPri), _UV0Rotate);
	texcoord.xy = TRANSFORM_TEX(texcoord.xy, _MainTex);
	texcoord.xy += _Time.y * _UV0Scroll;

	texcoord.zw = Rotate2D((SelectUVSet(v, _UVSec)), _UV1Rotate);
	texcoord.zw = TRANSFORM_TEX(texcoord.zw, _DetailAlbedoMap);
	texcoord.zw += _Time.y * _UV1Scroll;

	texcoord4.zw = Rotate2D((SelectUVSet(v, _UVDetailMask)), _DetailRotate);
	texcoord4.zw = TRANSFORM_TEX(texcoord4.zw, _DetailMask);
	texcoord4.zw += _Time.y * _DetailScroll;

	texcoord1 = 0;
	texcoord2 = 0;
	texcoord3 = 0;
	texcoord4 = float4(0,0,texcoord4.zw);

	#ifdef _PARALLAXMAP
		texcoord1.xy = TRANSFORM_TEX(SelectUVSet(v, _UVHeightMask), _ParallaxMask);
		texcoord1.xy += _Time.y * _UV2Scroll;
	#endif

	#ifdef _EMISSION
		texcoord1.zw = Rotate2D(SelectUVSet(v, _UVEmissMask), _UV3Rotate);
		texcoord1.zw = TRANSFORM_TEX(texcoord1.zw, _EmissionMask);
		texcoord1.zw += _Time.y * _UV3Scroll;
	#endif

	#ifdef _ALPHAMASK_ON
		texcoord2.xy = Rotate2D(SelectUVSet(v, _UVAlphaMask), _UV4Rotate);
		texcoord2.xy = TRANSFORM_TEX(texcoord2.xy, _AlphaMask);
		texcoord2.xy += _Time.y * _UV4Scroll;
	#endif

	if (_RimToggle == 1){
		texcoord2.zw = Rotate2D(SelectUVSet(v, _UVRimMask), _UVRimMaskRotate);
		texcoord2.zw = TRANSFORM_TEX(texcoord2.zw, _RimMask);
		texcoord2.zw += _Time.y * _UVRimMaskScroll;
	}

	if (_RainToggle == 1){
		texcoord3.xy = v.uv0;
		texcoord3.zw = Rotate2D(SelectUVSet(v, _UVRainMask), _UV5Rotate);
		texcoord3.zw = TRANSFORM_TEX(texcoord3.zw, _RainMask);
		texcoord3.zw += _Time.y * _UV5Scroll;
	}

	#if AREALIT_ENABLED
		texcoord4.xy = SelectUVSet(v, _OcclusionUVSet);
		texcoord4.xy = TRANSFORM_TEX(texcoord4.xy, _AreaLitOcclusion);
	#endif
}

half3 Filtering(float3 col, float hue, float saturation, float brightness, float contrast, float aces){
	if (_Filtering == 1){
		if (hue > 0 && hue < 1)
			col = HSVShift(col, hue, 0, 0);
		col = lerp(dot(col, float3(0.3,0.59,0.11)), col, saturation);
		col = GetContrast(col, contrast);
		col = lerp(col, ACES(col), aces);
		col *= brightness;
	}
	return col;
}

half DetailMask(float2 uv)
{
	float4 detailMask = _DetailMask.Sample(sampler_MainTex, uv);
    return detailMask[_DetailMaskChannel];
}

half3 Albedo(float4 texcoords, float2 detailTexCoords, SampleData sd)
{
	half3 albedo = _Color.rgb * SampleTexture(_MainTex, texcoords.xy, sd).rgb;
	albedo = Filtering(albedo, _Hue, _Saturation, _Brightness, _Contrast, 0);
	#if DETAIL_BASECOLOR
		half mask = DetailMask(detailTexCoords);
		sd.scaleTransform = _DetailAlbedoMap_ST;
		half4 detailAlbedo = SampleDetailTexture(_DetailAlbedoMap, sampler_DetailAlbedoMap, texcoords.zw, sd);
		detailAlbedo.rgb = _DetailColor.rgb * Filtering(detailAlbedo.rgb, _HueDet, _SaturationDet, _BrightnessDet, _ContrastDet, 0);
		albedo = BlendColorsAlpha(albedo, detailAlbedo.rgb, _DetailAlbedoBlend, mask, detailAlbedo.a);
	#endif
    return albedo;
}

half Alpha(float2 uv, SampleData sd)
{
	uv += uvOffset;
	#ifdef _ALPHAMASK_ON
		return ChannelCheck(SampleTexture(_AlphaMask, uv), _AlphaMaskChannel) * _AlphaMaskOpacity;
	#else
		return SampleTexture(_MainTex, uv, sd).a * _Color.a;
	#endif
}


half Occlusion(float4 uv, SampleData sd)
{
	#if WORKFLOW_PACKED
		half4 map = SampleTexture(_PackedMap, uv.xy, sd);
		half occ = ChannelCheck(map, _OcclusionChannel);
		occ = LerpOneTo(occ, lerp(1, _OcclusionStrength, _OcclusionMult));
	#else
		half occ = SampleTexture(_OcclusionMap, uv.xy, sd).g;
		occ = LerpOneTo(occ, _OcclusionStrength);
	#endif

	#if DETAIL_WORKFLOW_PACKED
		half4 detMap = SampleDetailTexture(_DetailPackedMap, uv.zw, sd);
		half detOcc = ChannelCheck(detMap, _DetailOcclusionChannel);
		half blendedOcc = BlendScalars(occ, detOcc, _DetailAOBlend, DetailMask(uv.xy));
		occ = lerp(occ, blendedOcc, lerp(1, _DetailOcclusionStrength, _DetailOcclusionMult));	
	#elif DETAIL_AO
		half4 detOcc = SampleDetailTexture(_DetailAOMap, sampler_DetailAOMap, uv.zw, sd);
		half blendedOcc = BlendScalarsAlpha(occ, detOcc.g, _DetailAOBlend, DetailMask(uv.xy), detOcc.a);
		occ = lerp(occ, blendedOcc, _DetailOcclusionStrength);
	#endif

	return occ;
}

half2 MetallicRough(float4 uv, SampleData sd)
{
	half2 metGloss;
	float metal = _Metallic;
	float gloss = _Glossiness; // Gloss = 1
	
	#if WORKFLOW_PACKED
		half4 packedMap = SampleTexture(_PackedMap, uv.xy, sd);
		gloss = ChannelCheck(packedMap, _RoughnessChannel);
		metal = ChannelCheck(packedMap, _MetallicChannel);
		// gloss = lerp(1-gloss, gloss, _UseSmoothness);
		gloss *= lerp(1, _Glossiness, _RoughnessMult);
		metal *= lerp(1, _Metallic, _MetallicMult);
	#else
		#ifdef _METALLICGLOSSMAP
			metal = SampleTexture(_MetallicGlossMap, uv.xy, sd) * _Metallic;
		#endif
		#ifdef _SPECGLOSSMAP
			gloss = SampleTexture(_SpecGlossMap, uv.xy, sd) * _Glossiness;
		#endif
		// gloss = lerp(1-gloss, gloss, _UseSmoothness);
	#endif

	#if DETAIL_WORKFLOW_PACKED
		float detMask = DetailMask(uv.xy);
		float4 detPackedMap = SampleDetailTexture(_DetailPackedMap, uv.zw, sd);
		float detRough = ChannelCheck(detPackedMap, _DetailRoughnessChannel);
		float detMetal = ChannelCheck(detPackedMap, _DetailMetallicChannel);
		// detRough = lerp(1-detRough, detRough, _DetailUseSmoothness);
		float blendedRough = BlendScalars(gloss, detRough, _DetailRoughBlend, detMask);
		float blendedMetal = BlendScalars(metal, detMetal, _DetailMetallicBlend, detMask);
		gloss = lerp(gloss, blendedRough, lerp(1, _DetailRoughnessStrength, _DetailRoughnessMult));
		metal = lerp(metal, blendedMetal, lerp(1, _DetailMetallicStrength, _DetailMetallicMult));
	#else
		#if DETAIL_ROUGH
			float4 detRough = SampleDetailTexture(_DetailRoughnessMap, sampler_DetailRoughnessMap, uv.zw, sd);
			// detRough.r = lerp(1-detRough.r, detRough.r, _DetailUseSmoothness);
			float blendedRough = BlendScalarsAlpha(gloss, detRough, _DetailRoughBlend, DetailMask(uv.xy), detRough.a);
			gloss = lerp(gloss, blendedRough, _DetailRoughnessStrength);
		#endif
		#if DETAIL_METALLIC
			float4 detMetal = SampleDetailTexture(_DetailMetallicMap, sampler_DetailMetallicMap, uv.zw, sd);
			float blendedMetal = BlendScalarsAlpha(metal, detMetal, _DetailMetallicBlend, DetailMask(uv.xy), detMetal.a);
			metal = lerp(metal, blendedMetal, _DetailMetallicStrength);
		#endif
	#endif

	#if WORKFLOW_PACKED
		#if TRIPLANAR_ENABLED
			metal = smoothstep(0,0.1, metal);
		#endif
	#else
		#ifdef _METALLICGLOSSMAP
			#if TRIPLANAR_ENABLED
				metal = smoothstep(0,0.1,metal);
			#endif
		#endif
	#endif

	metGloss.r = metal;
	metGloss.g = lerp(1-gloss, gloss, _UseSmoothness);
	return metGloss;
}

half3 Emission(float2 uv, float2 uvMask, SampleData sd)
{
	#ifdef _EMISSION
		float3 emissTex = SampleTexture(_EmissionMap, uv, sd);
		float emissMask = _EmissionMask.Sample(sampler_MainTex, uvMask+uvOffset).r;
		_EmissionColor.rgb = pow(_EmissionColor.rgb, 2.2);
		emissTex *= _EmissionColor.rgb * _EmissionIntensity * emissMask * GetWave(_EmissPulseWave, _EmissPulseSpeed, _EmissPulseStrength);
		emissTex = Filtering(emissTex, _HueEmiss, _SaturationEmiss, _BrightnessEmiss, _ContrastEmiss, 0);
		#ifdef THIS_IS_A_META_PASS
			#if defined(_AUDIOLINK_ON) && defined(_AUDIOLINK_META_ON)
				audioLinkData al = (audioLinkData)0;
				InitializeAudioLink(al, 0);
				emissTex *= lerp(1, GetAudioLinkBand(al, _AudioLinkEmission), _AudioLinkEmissionStrength * al.textureExists);
			#endif
		#else
			#if AUDIOLINK_ENABLED
				audioLinkData al = (audioLinkData)0;
				InitializeAudioLink(al, 0);
				emissTex *= lerp(1, GetAudioLinkBand(al, _AudioLinkEmission), _AudioLinkEmissionStrength * al.textureExists);
			#endif
		#endif
		return emissTex;
	#else
		return 0;
	#endif
}

void Rim(float3 worldPos, float3 normal, inout float3 col, float2 uv){
	if (_RimToggle == 1){
		float rimMask = tex2D(_RimMask, uv);
		float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
		float vdn = abs(dot(viewDir, normal));
		float rim = pow((1-vdn), (1-_RimWidth) * 10);
		rim = smoothstep(_RimEdge, 1-_RimEdge, rim);
		col = BlendColors(col, _RimCol, _RimBlending, rim*_RimStr*rimMask);
	}
}


#ifdef _NORMALMAP
half3 NormalInTangentSpace(float4 texcoords, float4 raincoords, SampleData sd)
{
	half3 normalTangent = SampleTexture(_BumpMap, texcoords.xy, sd, _BumpScale);
	#if defined(UNITY_ENABLE_DETAIL_NORMALMAP)
		sd.scaleTransform = _DetailAlbedoMap_ST;
		half mask = DetailMask(texcoords.xy);
		half3 detailNormalTangent = SampleDetailTexture(_DetailNormalMap, sampler_DetailNormalMap, texcoords.zw, sd, _DetailNormalMapScale);
		normalTangent = lerp(normalTangent, BlendNormals(normalTangent, detailNormalTangent), mask);
	#endif
	if (_RainToggle == 1){
		float rainMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_RainMask, sampler_MainTex, raincoords.zw);
		float3 rippleNormal = GetRipplesNormal(raincoords.xy, _RippleScale, _RippleStr*rainMask, _RippleSpeed);
		normalTangent = BlendNormals(normalTangent, rippleNormal);
	}
    return normalTangent;
}
#endif

//----------------------------
// Parallax Mapping
//----------------------------

#include "MochieStandardParallax.cginc"

float4 Parallax (float4 texcoords, half3 viewDir, out float2 offset, bool isFrontFace)
{
	offset = 0;

	#ifndef _PARALLAXMAP
		return texcoords;
	#endif

	if (!isFrontFace)
		return texcoords;

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
