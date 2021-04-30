#ifndef MOCHIE_STANDARD_SHADOW_INCLUDED
#define MOCHIE_STANDARD_SHADOW_INCLUDED

#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityStandardUtils.cginc"

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


half4       _Color;
half        _Cutoff;
sampler2D   _MainTex;
float4      _MainTex_ST;
#ifdef UNITY_STANDARD_USE_DITHER_MASK
sampler3D   _DitherMaskLOD;
#endif

// Handle PremultipliedAlpha from Fade or Transparent shading mode
half4       _SpecColor;
half        _Metallic;
#ifdef _SPECGLOSSMAP
sampler2D   _SpecGlossMap;
#endif
#ifdef _METALLICGLOSSMAP
sampler2D   _MetallicGlossMap;
#endif



#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
sampler2D   _ParallaxMap;
sampler2D	_ParallaxMask;
float2		_ParallaxMaskScroll;
float4		_ParallaxMask_ST;
half        _Parallax;
int			_ParallaxSteps;
float		_ParallaxOffset;
float2 		uvOffset;
half		_UV0Rotate;
half		_UV1Rotate;
float2		_UV0Scroll;
float2		_UV1Scroll;
#endif

sampler2D _PackedMap;
float _TSSBias;

int _RoughnessMult, _MetallicMult, _OcclusionMult, _HeightMult;
int _RoughnessChannel, _MetallicChannel, _OcclusionChannel, _HeightChannel;

#if SHADER_TARGET < 50
	#define ddx_fine ddx
	#define ddy_fine ddy
#endif

float2 Rotate2D(float2 coords, float rot){
	rot *= (UNITY_PI/180.0);
	float sinVal = sin(rot);
	float cosX = cos(rot);
	float2x2 mat = float2x2(cosX, -sinVal, sinVal, cosX);
	mat = ((mat*0.5)+0.5)*2-1;
	coords -= 0.5;
	return mul(coords, mat) + 0.5;
}

#define TSS_ENABLED defined(BLOOM)
#define STOCHASTIC_ENABLED defined(EFFECT_HUE_VARIATION)

#include "../Common/Sampling.cginc"

#if STOCHASTIC_ENABLED
	#define tex2D tex2Dstoch
#elif TSS_ENABLED
	#define tex2D tex2Dsuper
#endif

#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
	#include "MochieStandardParallax.cginc"
#endif

half RoughnessSetup_ShadowGetOneMinusReflectivity(half2 uv)
{
    half metallicity = _Metallic;
	#if !defined(BLOOM_LENS_DIRT) && defined(_METALLICGLOSSMAP)
		metallicity = tex2D(_MetallicGlossMap, uv).r;
	#else
		metallicity = tex2D(_PackedMap, uv).b;
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
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
        half4 tangent   : TANGENT;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
struct VertexOutputShadowCaster
{
    V2F_SHADOW_CASTER_NOPOS
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        float2 tex : TEXCOORD1;

        #if defined(_PARALLAXMAP) || defined(BLOOM_LENS_DIRT)
            half3 viewDirForParallax : TEXCOORD2;
        #endif
    #endif
};
#endif

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
    UNITY_SETUP_INSTANCE_ID(v);
    #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(os);
    #endif
    TRANSFER_SHADOW_CASTER_NOPOS(o,opos)
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        o.tex = TRANSFORM_TEX(v.uv0, _MainTex);

        #if defined(_PARALLAXMAP)
            TANGENT_SPACE_ROTATION;
			o.viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
        #endif
    #endif
}

#if STOCHASTIC_ENABLED
	#define tex2D tex2Dstoch
#endif

half4 fragShadowCaster (UNITY_POSITION(vpos)
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
    , VertexOutputShadowCaster i
#endif
) : SV_Target
{
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        #if defined(_PARALLAXMAP) && (SHADER_TARGET >= 30)
            half3 viewDirForParallax = normalize(i.viewDirForParallax);
			#if WORKFLOW_PACKED || WORKFLOW_MODULAR
				#if WORKFLOW_PACKED
					half h = tex2D(_PackedMap, i.tex.xy).a + _ParallaxOffset;
				#else
					half4 packedMap = tex2D(_PackedMap, i.tex.xy);
					half h = ChannelCheck(packedMap, _HeightChannel) + _ParallaxOffset;
				#endif
				h = clamp(h, 0, 0.999);
				float2 maskUV = TRANSFORM_TEX(i.tex1.xy, _ParallaxMask) + (_Time.y*_ParallaxMaskScroll);
				half m = tex2D(_ParallaxMask, maskUV);
				_Parallax = lerp(0.02, _Parallax, _HeightMult);
				half2 offset = ParallaxOffsetMultiStep(h, _Parallax * m, i.tex.xy, viewDirForParallax);
			#else
				half h = tex2D(_ParallaxMap, i.tex.xy).g + _ParallaxOffset;
				h = clamp(h, 0, 0.999);
				float2 maskUV = TRANSFORM_TEX(i.tex1.xy, _ParallaxMask) + (_Time.y*_ParallaxMaskScroll);
				half m = tex2D(_ParallaxMask, maskUV);
				half2 offset = ParallaxOffsetMultiStep(h, _Parallax * m, i.tex.xy, viewDirForParallax);
			#endif
           
            i.tex.xy += offset;
        #endif

        #if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
            half alpha = _Color.a;
        #else
            half alpha = tex2D(_MainTex, i.tex.xy).a * _Color.a;
        #endif
        #if defined(_ALPHATEST_ON)
            clip (alpha - _Cutoff);
        #endif
        #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
            #if defined(_ALPHAPREMULTIPLY_ON)
                half outModifiedAlpha;
                PreMultiplyAlpha(half3(0, 0, 0), alpha, SHADOW_ONEMINUSREFLECTIVITY(i.tex), outModifiedAlpha);
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

#if STOCHASTIC_ENABLED
	#undef tex2D
#elif TSS_ENABLED
	#undef tex2D
#endif

#endif // UNITY_STANDARD_SHADOW_INCLUDED