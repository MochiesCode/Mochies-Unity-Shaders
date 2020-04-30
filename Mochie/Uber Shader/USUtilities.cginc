#include "../Common/Color.cginc"
#include "../Common/Utilities.cginc"

float SampleMask(texture2D tex, float2 uv, int channel, bool isOn){
	float mask = 1;
	UNITY_BRANCH
	if (isOn){
		UNITY_BRANCH
		switch (channel){
			case 0: mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).r; break;
			case 1: mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).g; break;
			case 2: mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).b; break;
			case 3: mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).a; break;
			default: break;
		}
	}
	return mask;
}

float SampleTex2DMask(sampler2D tex, float2 uv, int channel){
	float mask = 1;
	UNITY_BRANCH
	switch (channel){
		case 0: mask = tex2D(tex, uv).r; break;
		case 1: mask = tex2D(tex, uv).g; break;
		case 2: mask = tex2D(tex, uv).b; break;
		case 3: mask = tex2D(tex, uv).a; break;
		default: break;
	}
	return mask;
}

float SampleCubeMask(texture2D tex, float2 uv, float str, int channel){
	float mask = 1;
	UNITY_BRANCH
	if (_IsCubeBlendMask == 1){
		UNITY_BRANCH
		switch (channel){
			case 0: mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).r; break;
			case 1: mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).g; break;
			case 2: mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).b; break;
			case 3: mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).a; break;
			default: break;
		}
	}
	else mask = str;
	return mask;
}

float ChannelCheck(float4 packedTex, float map, int channel){
	UNITY_BRANCH
	switch (channel){
		case 0: map = packedTex.r; break;
		case 1: map = packedTex.g; break;
		case 2: map = packedTex.b; break;
		case 3: map = packedTex.a; break;
		default: break;
	}
	return map;
}

float3 BlendCubemap(float3 baseCol, float3 cubeCol, float blend){
	UNITY_BRANCH
	switch (_CubeBlendMode){
		case 0: baseCol = lerp(baseCol, cubeCol, blend); break;
		case 1: baseCol += cubeCol * blend; break;
		case 2: baseCol -= cubeCol * blend; break;
		case 3: baseCol *= lerp(1, cubeCol, blend); break;
		default: break;
	}
	return baseCol;
}

float3 FlowUV (float2 uv, float time, float phase) {
	float progress = frac(time + phase);
	float waveform = 1-abs(1-2 * progress);
	uv += (time - progress) * float2(0.24, 0.2083333);
	float3 uvw = float3(uv, waveform);
	return uvw;
}

float3 GetContrast(float3 col){
    return clamp((lerp(float3(0.5,0.5,0.5), col, _Contrast)), 0, 10);
}

float3 GetSaturation(float3 col, float interpolator){
    return lerp(dot(col, float3(0.3,0.59,0.11)), col, interpolator);
}