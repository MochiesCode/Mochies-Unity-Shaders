#ifndef US_MACROS_INCLUDED
#define US_MACROS_INCLUDED

Texture2D _MainTex;
SamplerState sampler_MainTex;
float4 _MainTex_ST, _MainTex_TexelSize;

// Extra macros for reusing samplers with manual lod, lod by gradient, and bias
#ifndef UNITY_SAMPLE_TEX2D_GRAD_SAMPLER
	#define UNITY_SAMPLE_TEX2D_GRAD_SAMPLER(tex,samplertex,coord,dx,dy) tex.SampleGrad(sampler##samplertex,coord,dx,dy)
#endif
#ifndef UNITY_SAMPLE_TEX2D_LOD_SAMPLER
	#define UNITY_SAMPLE_TEX2D_LOD_SAMPLER(tex,samplertex,coord) tex.SampleLevel(sampler##samplertex,(coord).xy,(coord).w)
#endif
#ifndef UNITY_SAMPLE_TEX2D_BIAS_SAMPLER
	#define UNITY_SAMPLE_TEX2D_BIAS_OFFS_SAMPLER(tex,samplertex,coord,offset) tex.SampleBias(sampler##samplertex,(coord).xy,(coord).w,(offset).xy)
	#define UNITY_SAMPLE_TEX2D_BIAS_SAMPLER(tex,samplertex,coord) tex.SampleBias(sampler##samplertex,(coord).xy,(coord).w)
#endif
#ifndef UNITY_SAMPLE_TEX2D_LOD
	#define UNITY_SAMPLE_TEX2D_LOD(tex,coord) tex.SampleLevel(sampler##tex,(coord).xy,(coord).w)
#endif

#ifdef POINT
	#define POI_LIGHT_ATTENUATION(destName, shadows, input, worldPos) \
		unityShadowCoord3 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1)).xyz; \
		float shadows = UNITY_SHADOW_ATTENUATION(input, worldPos); \
		float destName = tex2D(_LightTexture0, dot(lightCoord, lightCoord).rr).r;
#endif

#ifdef POINT_COOKIE
	#if !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
		#define DECLARE_LIGHT_COORD(input, worldPos) unityShadowCoord3 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1)).xyz
	#else
		#define DECLARE_LIGHT_COORD(input, worldPos) unityShadowCoord3 lightCoord = input._LightCoord
	#endif
	#define POI_LIGHT_ATTENUATION(destName, shadows, input, worldPos) \
        DECLARE_LIGHT_COORD(input, worldPos); \
        float shadows = UNITY_SHADOW_ATTENUATION(input, worldPos); \
        float destName = tex2D(_LightTextureB0, dot(lightCoord, lightCoord).rr).r * texCUBE(_LightTexture0, lightCoord).w;
#endif

#ifdef SPOT
	#if !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
		#define DECLARE_LIGHT_COORD(input, worldPos) unityShadowCoord4 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1))
	#else
		#define DECLARE_LIGHT_COORD(input, worldPos) unityShadowCoord4 lightCoord = input._LightCoord
	#endif
	#define POI_LIGHT_ATTENUATION(destName, shadows, input, worldPos) \
		DECLARE_LIGHT_COORD(input, worldPos); \
		float shadows = UNITY_SHADOW_ATTENUATION(input, worldPos); \
		float destName = (lightCoord.z > 0) * UnitySpotCookie(lightCoord) * UnitySpotAttenuate(lightCoord.xyz);
#endif

float SampleCubeMask(texture2D tex, float2 uv, float str, int isBlendMask){
	float mask = 1;
	if (isBlendMask == 1){
		mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).r;
	}
	else mask = str;
	return mask;
}

float ChannelCheck(float4 packedTex, int channel){
	float map = 0;
	[flatten]
	switch (channel){
		case 0: map = packedTex.r; break;
		case 1: map = packedTex.g; break;
		case 2: map = packedTex.b; break;
		case 3: map = packedTex.a; break;
		default: break;
	}
	return map;
}

float3 BlendCubemap(float3 baseCol, float3 cubeCol, float blend, int blendMode){
	[flatten]
	switch (blendMode){
		case 0: baseCol = lerp(baseCol, cubeCol, blend); break;
		case 1: baseCol += cubeCol * blend; break;
		case 2: baseCol -= cubeCol * blend; break;
		case 3: baseCol *= lerp(1, cubeCol, blend); break;
		default: break;
	}
	return baseCol;
}

void ApplyPBRFiltering(inout float value, float contrast, float intensity, float lightness, int shouldApply, inout float previewValue){
	if (shouldApply == 1){
		value = saturate(lerp(0.5, value, contrast));
		value += saturate(value * intensity);
		value = saturate(value + lightness);
		previewValue = value;
	}
}

void ApplyPBRFiltering(inout float3 value, float contrast, float intensity, float lightness, int shouldApply, inout float3 previewValue){
	if (shouldApply == 1){
		value = saturate(lerp(0.5, value, contrast));
		value += saturate(value * intensity);
		value = saturate(value + lightness);
		previewValue = value;
	}
}

#endif