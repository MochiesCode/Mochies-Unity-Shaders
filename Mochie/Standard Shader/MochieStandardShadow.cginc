#ifndef MOCHIE_STANDARD_SHADOW_INCLUDED
#define MOCHIE_STANDARD_SHADOW_INCLUDED

#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityStandardUtils.cginc"
#include "MochieStandardKeyDefines.cginc"
#include "../Common/Utilities.cginc"

#include "../Common/Sampling.cginc"
#include "MochieStandardSampling.cginc"

#if (defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)) && defined(UNITY_USE_DITHER_MASK_FOR_ALPHABLENDED_SHADOWS)
    #define UNITY_STANDARD_USE_DITHER_MASK 1
#endif

// Need to output UVs in shadow caster, since we need to sample texture and do clip/dithering based on it
#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
#define UNITY_STANDARD_USE_SHADOW_UVS 1
#endif

// Has a non-empty shadow caster output struct (it's an error to have empty structs on some platforms...)
#if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
#define UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT 1
#endif

#ifdef UNITY_STEREO_INSTANCING_ENABLED
#define UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT 1
#endif

float		_NaNLmao;
half4       _Color;
half        _Cutoff;
#ifdef UNITY_STANDARD_USE_DITHER_MASK
	sampler3D   _DitherMaskLOD;
#endif

// Handle PremultipliedAlpha from Fade or Transparent shading mode
half4       _SpecColor;
half        _Metallic;
#ifdef _SPECGLOSSMAP
	Texture2D   _SpecGlossMap;
#endif
#ifdef _METALLICGLOSSMAP
	Texture2D   _MetallicGlossMap;
#endif

#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
	Texture2D   _ParallaxMap;
	Texture2D	_ParallaxMask;
	float2		_ParallaxMaskScroll;
	float4		_ParallaxMask_ST;
	half        _Parallax;
	int			_ParallaxSteps;
	float		_ParallaxOffset;
	float2 		uvOffset;
#endif

Texture2D 	_AlphaMask;
SamplerState sampler_AlphaMask;
float4		_AlphaMask_ST;
float		_AlphaMaskOpacity;
int			_AlphaMaskChannel;
half		_UVAlphaMask;
half		_UV4Rotate;
half		_UV0Rotate;
half		_UV1Rotate;
float2		_UV0Scroll;
float2		_UV1Scroll;
float2		_UV4Scroll;

#if WORKFLOW_PACKED
	Texture2D _PackedMap;
	int _RoughnessMult, _MetallicMult, _OcclusionMult, _HeightMult;
	int _RoughnessChannel, _MetallicChannel, _OcclusionChannel, _HeightChannel;
#endif

#if SSR_ENABLED || DECAL_ENABLED
	sampler2D _CameraDepthTexture;
	#define HAS_DEPTH_TEXTURE
#endif

#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
	#include "MochieStandardParallax.cginc"
#endif

#include "../Common/Utilities.cginc"

half ShadowGetOneMinusReflectivity(half2 uv, SampleData sd)
{
    half metallicity = _Metallic;
	#if WORKFLOW_MODULAR
		half4 packedMap = SampleTexture(_PackedMap, uv, sd);
		metallicity = ChannelCheck(packedMap, _MetallicChannel);
	#else
		#if defined(_METALLICGLOSSMAP)
			metallicity = SampleTexture(_MetallicGlossMap, uv, sd).r;
		#endif
	#endif
    return OneMinusReflectivityFromMetallic(metallicity);
}

// SHADOW_ONEMINUSREFLECTIVITY(): workaround to get one minus reflectivity based on UNITY_SETUP_BRDF_INPUT
#define SHADOW_JOIN2(a, b) a##b
#define SHADOW_JOIN(a, b) SHADOW_JOIN2(a,b)
#define SHADOW_ONEMINUSREFLECTIVITY SHADOW_JOIN(UNITY_SETUP_BRDF_INPUT, _ShadowGetOneMinusReflectivity)

struct VertexInput
{
    float4 vertex   : POSITION;
    float3 normal   : NORMAL;
    float2 uv0      : TEXCOORD0;
	float2 uv1      : TEXCOORD1;
	float2 uv2      : TEXCOORD2;
	float2 uv3      : TEXCOORD3;
	float2 uv4      : TEXCOORD4;
    half4 tangent   : TANGENT;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutputShadowCaster
{
    V2F_SHADOW_CASTER_NOPOS
	float2 tex : TEXCOORD1;
	float2 tex1 : TEXCOORD2;
	float2 tex2 : TEXCOORD3;
	float4 localPos : TEXCOORD4;
	float3 normal : TEXCOORD5;
	half3 viewDirForParallax : TEXCOORD6;
	#if DECAL_ENABLED
		float4 screenPos : TEXCOORD7;
		float3 objPos : TEXCOORD8;
		float3 raycast : TEXCOORD9;
	#endif
};

float2 SelectUVSet(VertexInput v, int selection){
	float2 uvs[] = {v.uv0, v.uv1, v.uv2, v.uv3, v.uv4};
	return uvs[selection];
}

float2 GetAlphaMaskUV(VertexInput v){
	#ifdef _ALPHAMASK_ON
		float2 coords = Rotate2D(SelectUVSet(v, _UVAlphaMask), _UV4Rotate);
		coords = TRANSFORM_TEX(coords.xy, _AlphaMask);
		coords += _Time.y * _UV4Scroll;
	#else
		float2 coords = 0;
	#endif
	return coords;
}

#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
struct VertexOutputStereoShadowCaster
{
    UNITY_VERTEX_OUTPUT_STEREO
};
#endif

// We have to do these dances of outputting SV_POSITION separately from the vertex shader,
// and inputting VPOS in the pixel shader, since they both map to "POSITION" semantic on
// some platforms, and then things don't go well.

void vertShadowCaster (VertexInput v
    , out float4 opos : SV_POSITION
    #ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
    , out VertexOutputShadowCaster o
    #endif
    #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
    , out VertexOutputStereoShadowCaster os
    #endif
)
{
	#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
		o = (VertexOutputShadowCaster)0;
	#endif
    UNITY_SETUP_INSTANCE_ID(v);
    #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(os);
    #endif
    TRANSFER_SHADOW_CASTER_NOPOS(o,opos)
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        o.tex = TRANSFORM_TEX(v.uv0, _MainTex);
		o.tex1 = v.uv0;
		o.tex2 = GetAlphaMaskUV(v);
		o.localPos = v.vertex;
		o.normal = UnityObjectToWorldNormal(v.normal);
		#if DECAL_ENABLED
			float4 pos = UnityObjectToClipPos(v.vertex);
			o.screenPos = ComputeGrabScreenPos(pos);
			o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1));
			o.raycast = UnityObjectToViewPos(v.vertex).xyz * float3(-1,-1,1);
		#endif
        #if defined(_PARALLAXMAP)
            TANGENT_SPACE_ROTATION;
			o.viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
        #endif
    #endif
}

#if DECAL_ENABLED
void GetDepthData(VertexOutputShadowCaster i, inout SampleData sd){
	float rawDepth = DecodeFloatRG(tex2Dproj(_CameraDepthTexture, i.screenPos));
	float depth = Linear01Depth(rawDepth);
	i.raycast = i.raycast * (_ProjectionParams.z / i.raycast.z);
	float4 vpos = float4(i.raycast * depth, 1);
	float3 wpos = mul(unity_CameraToWorld, vpos).xyz;
	float3 wposX = ddx(wpos);
	float3 wposY = ddy(wpos);
	sd.depthNormal = abs(normalize(cross(wposY, wposX)));
	sd.worldPixelPos = wpos;
}
#endif

SampleData SampleDataSetup(UNITY_POSITION(vpos)
	#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
		, VertexOutputShadowCaster i
	#endif
	){

	SampleData sd = (SampleData)0;

	#ifndef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
		VertexOutputShadowCaster i = (VertexOutputShadowCaster)0;
		i.localPos = vpos;
		#if DECAL_ENABLED
			float4 pos = UnityObjectToClipPos(vpos);
			i.screenPos = ComputeGrabScreenPos(pos);
			i.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1));
			i.raycast = UnityObjectToViewPos(vpos).xyz * float3(-1,-1,1);
		#endif
	#endif

	sd.localPos = i.localPos;
	sd.normal = i.normal;
	sd.rotation = _UV0Rotate;
	sd.scaleTransform = _MainTex_ST;
	#if DECAL_ENABLED
		GetDepthData(i, sd);
		sd.objPos = i.objPos;
	#endif
	return sd;
}

half4 fragShadowCaster (UNITY_POSITION(vpos)
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
    , VertexOutputShadowCaster i
#endif
) : SV_Target
{
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        #if defined(_PARALLAXMAP) && (SHADER_TARGET >= 30)
            half3 viewDirForParallax = normalize(i.viewDirForParallax);
			#if WORKFLOW_MODULAR
				half4 packedMap = SampleTexture(_PackedMap, i.tex.xy);
				half h = ChannelCheck(packedMap, _HeightChannel) + _ParallaxOffset;
				h = clamp(h, 0, 0.999);
				float2 maskUV = TRANSFORM_TEX(i.tex1.xy, _ParallaxMask) + (_Time.y*_ParallaxMaskScroll);
				half m = _ParallaxMask.Sample(sampler_MainTex, maskUV);
				_Parallax = lerp(0.02, _Parallax, _HeightMult);
				half2 offset = ParallaxOffsetMultiStep(h, _Parallax * m, i.tex.xy, viewDirForParallax);
			#else
				half h = SampleTexture(_ParallaxMap, i.tex.xy).g + _ParallaxOffset;
				h = clamp(h, 0, 0.999);
				float2 maskUV = TRANSFORM_TEX(i.tex1.xy, _ParallaxMask) + (_Time.y*_ParallaxMaskScroll);
				half m = _ParallaxMask.Sample(sampler_MainTex, maskUV);
				half2 offset = ParallaxOffsetMultiStep(h, _Parallax * m, i.tex.xy, viewDirForParallax);
			#endif
           
            i.tex.xy += offset;
        #endif

		SampleData sd = SampleDataSetup(vpos
		#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
		,	i
		#endif
		);
		#ifdef _ALPHAMASK_ON
			half alpha = ChannelCheck(SampleTexture(_AlphaMask, sampler_AlphaMask, i.tex2), _AlphaMaskChannel) * _AlphaMaskOpacity;
		#else
			half alpha = SampleTexture(_MainTex, i.tex.xy, sd);
		#endif
        #if defined(_ALPHATEST_ON)
            clip (alpha - _Cutoff);
        #endif
        #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
            #if defined(_ALPHAPREMULTIPLY_ON)
                half outModifiedAlpha;
                PreMultiplyAlpha(half3(0, 0, 0), alpha, ShadowGetOneMinusReflectivity(i.tex, sd), outModifiedAlpha);
                alpha = outModifiedAlpha;
            #endif
            #if defined(UNITY_STANDARD_USE_DITHER_MASK)
                // Use dither mask for alpha blended shadows, based on pixel position xy
                // and alpha level. Our dither texture is 4x4x16.
                #ifdef LOD_FADE_CROSSFADE
                    #define _LOD_FADE_ON_ALPHA
                    alpha *= unity_LODFade.y;
                #endif
                half alphaRef = tex3D(_DitherMaskLOD, float3(vpos.xy*0.25,alpha*0.9375)).a;
                clip (alphaRef - 0.01);
            #else
                clip (alpha - _Cutoff);
            #endif
        #endif
    #endif // #if defined(UNITY_STANDARD_USE_SHADOW_UVS)

    #ifdef LOD_FADE_CROSSFADE
        #ifdef _LOD_FADE_ON_ALPHA
            #undef _LOD_FADE_ON_ALPHA
        #else
            UnityApplyDitherCrossFade(vpos.xy);
        #endif
    #endif

    SHADOW_CASTER_FRAGMENT(i)
}

#endif // UNITY_STANDARD_SHADOW_INCLUDED