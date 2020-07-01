// Extra macros for reusing samplers with manual lod, lod by gradient, and bias
#if !defined(UNITY_SAMPLE_TEX2D_GRAD_SAMPLER)
	#define UNITY_SAMPLE_TEX2D_GRAD_SAMPLER(tex,samplertex,coord,dx,dy) tex.SampleGrad(sampler##samplertex,coord,dx,dy)
#endif
#if !defined(UNITY_SAMPLE_TEX2D_LOD_SAMPLER)
	#define UNITY_SAMPLE_TEX2D_LOD_SAMPLER(tex,samplertex,coord) tex.SampleLevel(sampler##samplertex,(coord).xy,(coord).w)
#endif
#if !defined(UNITY_SAMPLE_TEX2D_BIAS_SAMPLER)
	#define UNITY_SAMPLE_TEX2D_BIAS_OFFS_SAMPLER(tex,samplertex,coord,offset) tex.SampleBias(sampler##samplertex,(coord).xy,(coord).w,(offset).xy)
	#define UNITY_SAMPLE_TEX2D_BIAS_SAMPLER(tex,samplertex,coord) tex.SampleBias(sampler##samplertex,(coord).xy,(coord).w)
#endif

float4 tex2DBoolWhite(sampler2D tex, float2 uv, bool shouldSample){
	float4 col = 1;
	if (shouldSample)
		col = tex2D(tex, uv);
	return col;
}

float4 tex2DBoolBlack(sampler2D tex, float2 uv, bool shouldSample){
	float4 col = 0;
	if (shouldSample)
		col = tex2D(tex, uv);
	return col;
}

float4 tex2DBoolWhiteSampler(Texture2D tex, float2 uv, bool shouldSample){
	float4 col = 1;
	if (shouldSample)
		col = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv);
	return col;
}

float4 tex2DBoolBlackSampler(Texture2D tex, float2 uv, bool shouldSample){
	float4 col = 0;
	if (shouldSample)
		col = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv);
	return col;
}

float SampleMask(texture2D tex, float2 uv, int channel, bool isOn){
	float mask = 1;
	if (isOn){
		float4 maskTex = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv);
		[flatten]
		switch (channel){
			case 0: mask = maskTex.r; break;
			case 1: mask = maskTex.g; break;
			case 2: mask = maskTex.b; break;
			case 3: mask = maskTex.a; break;
			default: break;
		}
	}
	return mask;
}

float SampleTex2DMask(sampler2D tex, float2 uv, int channel){
	float mask = 1;
	float4 maskTex = tex2D(tex, uv);
	[flatten]
	switch (channel){
		case 0: mask = maskTex.r; break;
		case 1: mask = maskTex.g; break;
		case 2: mask = maskTex.b; break;
		case 3: mask = maskTex.a; break;
		default: break;
	}
	return mask;
}

float SampleCubeMask(texture2D tex, float2 uv, float str, int channel, int isBlendMask){
	float mask = 1;
	float4 maskTex = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv);
	if (isBlendMask == 1){
		[flatten]
		switch (channel){
			case 0: mask = maskTex.r; break;
			case 1: mask = maskTex.g; break;
			case 2: mask = maskTex.b; break;
			case 3: mask = maskTex.a; break;
			default: break;
		}
	}
	else mask = str;
	return mask;
}

float ChannelCheck(float3 packedTex, int channel){
	float map = 0;
	[flatten]
	switch (channel){
		case 0: map = packedTex.r; break;
		case 1: map = packedTex.g; break;
		case 2: map = packedTex.b; break;
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