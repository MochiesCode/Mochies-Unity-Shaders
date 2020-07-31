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
float SampleCubeMask(texture2D tex, float2 uv, float str, int isBlendMask){
	float mask = 1;
	if (isBlendMask == 1){
		mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).r;
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