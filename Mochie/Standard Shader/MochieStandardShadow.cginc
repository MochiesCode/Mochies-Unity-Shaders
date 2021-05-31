#ifndef MOCHIE_STANDARD_SHADOW_INCLUDED
#define MOCHIE_STANDARD_SHADOW_INCLUDED

#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityStandardUtils.cginc"
#include "MochieStandardKeyDefines.cginc"

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

#if SHADER_TARGET < 50
	#define ddx_fine ddx
	#define ddy_fine ddy
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
#endif

half		_UV0Rotate;
half		_UV1Rotate;
float2		_UV0Scroll;
float2		_UV1Scroll;

#if WORKFLOW_PACKED || WORKFLOW_MODULAR
	sampler2D _PackedMap;
	int _RoughnessMult, _MetallicMult, _OcclusionMult, _HeightMult;
	int _RoughnessChannel, _MetallicChannel, _OcclusionChannel, _HeightChannel;
#endif

#if SSR_ENABLED || DECAL_ENABLED
	sampler2D _CameraDepthTexture;
	#define HAS_DEPTH_TEXTURE
#endif

#include "../Common/Sampling.cginc"

#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
	#include "MochieStandardParallax.cginc"
#endif

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

float2 Rotate2D(float2 coords, float rot){
	rot *= (UNITY_PI/180.0);
	float sinVal = sin(rot);
	float cosX = cos(rot);
	float2x2 mat = float2x2(cosX, -sinVal, sinVal, cosX);
	mat = ((mat*0.5)+0.5)*2-1;
	return mul(coords, mat);
}

half RoughnessSetup_ShadowGetOneMinusReflectivity(half2 uv, SampleData sd)
{
    half metallicity = _Metallic;
	#if WORKFLOW_PACKED
		metallicity = SampleTexture(_PackedMap, uv, sd).b;
	#elif WORKFLOW_MODULAR
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
    half4 tangent   : TANGENT;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutputShadowCaster
{
    V2F_SHADOW_CASTER_NOPOS
	float2 tex : TEXCOORD1;
	float2 tex1 : TEXCOORD2;
	float4 localPos : TEXCOORD3;
	float3 normal : TEXCOORD4;
	half3 viewDirForParallax : TEXCOORD5;
	#if DECAL_ENABLED
		float4 screenPos : TEXCOORD6;
		float3 objPos : TEXCOORD7;
		float3 raycast : TEXCOORD8;
	#endif
};

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
	float3 wposX = ddx_fine(wpos);
	float3 wposY = ddy_fine(wpos);
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
			#if WORKFLOW_PACKED || WORKFLOW_MODULAR
				#if WORKFLOW_PACKED
					half h = SampleTexture(_PackedMap, i.tex.xy).a + _ParallaxOffset;
				#else
					half4 packedMap = SampleTexture(_PackedMap, i.tex.xy);
					half h = ChannelCheck(packedMap, _HeightChannel) + _ParallaxOffset;
				#endif
				h = clamp(h, 0, 0.999);
				float2 maskUV = TRANSFORM_TEX(i.tex1.xy, _ParallaxMask) + (_Time.y*_ParallaxMaskScroll);
				half m = tex2D(_ParallaxMask, maskUV);
				_Parallax = lerp(0.02, _Parallax, _HeightMult);
				half2 offset = ParallaxOffsetMultiStep(h, _Parallax * m, i.tex.xy, viewDirForParallax);
			#else
				half h = SampleTexture(_ParallaxMap, i.tex.xy).g + _ParallaxOffset;
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
			SampleData sd = SampleDataSetup(vpos
			#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
			,	i
			#endif
			);
			half alpha = SampleTexture(_MainTex, i.tex.xy, sd).a * _Color.a;
        #endif
        #if defined(_ALPHATEST_ON)
            clip (alpha - _Cutoff);
        #endif
        #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
            #if defined(_ALPHAPREMULTIPLY_ON)
                half outModifiedAlpha;
                PreMultiplyAlpha(half3(0, 0, 0), alpha, SHADOW_ONEMINUSREFLECTIVITY(i.tex, i.localPos, i.normal), outModifiedAlpha);
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