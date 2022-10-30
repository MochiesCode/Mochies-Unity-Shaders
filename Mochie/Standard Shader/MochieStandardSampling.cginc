#ifndef MOCHIE_STANDARD_SAMPLING_INCLUDED
#define MOCHIE_STANDARD_SAMPLING_INCLUDED

Texture2D		_MainTex;
SamplerState 	sampler_MainTex;
Texture2D		_DetailAlbedoMap;
SamplerState 	sampler_DetailAlbedoMap;
Texture2D		_DetailRoughnessMap;
SamplerState	sampler_DetailRoughnessMap;
Texture2D		_DetailAOMap;
SamplerState	sampler_DetailAOMap;
Texture2D 		_DetailMetallicMap;
SamplerState 	sampler_DetailMetallicMap;
#if defined(UNITY_ENABLE_DETAIL_NORMALMAP)
	Texture2D		_DetailNormalMap;
	SamplerState	sampler_DetailNormalMap;
#endif

float4      	_MainTex_ST;
float4      	_DetailAlbedoMap_ST;

#include "../Standard Shader/MochieStandardKeyDefines.cginc"

struct SampleData {
	float4 localPos;
	float3 objPos;
	float3 depthNormal;
	float3 worldPixelPos;
	float3 normal;
	float4 scaleTransform;
};

float _DetailTriplanarFalloff;
float _TriplanarFalloff;

// Based on Xiexe's implementation
// https://github.com/Xiexe/XSEnvironmentShaders/blob/bf992e8e292a0562ce4164964f16b3abdc97f078/XSEnvironment/LightingFunctions.cginc#L213

float4 tex2Dtri(Texture2D tex, SampleData sd, float falloff) {
	float3 surfaceNormal = sd.normal;
	float3 pos = sd.localPos;
    surfaceNormal = abs(surfaceNormal);
	float3 projectedNormal = pow(abs(surfaceNormal), falloff);
    projectedNormal /= (surfaceNormal.x + surfaceNormal.y + surfaceNormal.z);
	float3 normalSign = sign(surfaceNormal);

	float2 uvX = sd.scaleTransform.xy * (pos.zy * float2(normalSign.x, 1)) + sd.scaleTransform.zw;
	float2 uvY = sd.scaleTransform.xy * (pos.xz * float2(normalSign.y, 1)) + sd.scaleTransform.zw;
	float2 uvZ = sd.scaleTransform.xy * (pos.xy * float2(-normalSign.z, 1)) + sd.scaleTransform.zw;

	float4 sampleX, sampleY, sampleZ;
	sampleX = sampleY = sampleZ = 0;

	if (projectedNormal.x > 0)
		sampleX = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_MainTex, uvX);
	if (projectedNormal.y > 0)
		sampleY =  MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_MainTex, uvY);
	if (projectedNormal.z > 0)
		sampleZ =  MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_MainTex, uvZ);

	return (sampleX * projectedNormal.x) + (sampleY * projectedNormal.y) + (sampleZ * projectedNormal.z);
}

float4 tex2Dtri(Texture2D tex, SamplerState ss, SampleData sd, float falloff) {
	float3 surfaceNormal = sd.normal;
	float3 pos = sd.localPos;
    surfaceNormal = abs(surfaceNormal);
	float3 projectedNormal = pow(abs(surfaceNormal), falloff);
    projectedNormal /= (surfaceNormal.x + surfaceNormal.y + surfaceNormal.z);
	float3 normalSign = sign(surfaceNormal);

	float2 uvX = sd.scaleTransform.xy * (pos.zy * float2(normalSign.x, 1)) + sd.scaleTransform.zw;
	float2 uvY = sd.scaleTransform.xy * (pos.xz * float2(normalSign.y, 1)) + sd.scaleTransform.zw;
	float2 uvZ = sd.scaleTransform.xy * (pos.xy * float2(-normalSign.z, 1)) + sd.scaleTransform.zw;

	float4 sampleX, sampleY, sampleZ;
	sampleX = sampleY = sampleZ = 0;

	if (projectedNormal.x > 0)
		sampleX = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, uvX);
	if (projectedNormal.y > 0)
		sampleY =  MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, uvY);
	if (projectedNormal.z > 0)
		sampleZ =  MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, uvZ);

	return (sampleX * projectedNormal.x) + (sampleY * projectedNormal.y) + (sampleZ * projectedNormal.z);
}

float3 tex2DtriNormal(Texture2D tex, SampleData sd, float normalScale, float falloff) {
	float3 surfaceNormal = sd.normal;
	float3 pos = sd.localPos;
    surfaceNormal = abs(surfaceNormal);
	float3 projectedNormal = pow(abs(surfaceNormal), falloff);
    projectedNormal /= (surfaceNormal.x + surfaceNormal.y + surfaceNormal.z);
	float3 normalSign = sign(surfaceNormal);

	float2 uvX = sd.scaleTransform.xy * (pos.zy * float2(normalSign.x, 1)) + sd.scaleTransform.zw;
	float2 uvY = sd.scaleTransform.xy * (pos.xz * float2(normalSign.y, 1)) + sd.scaleTransform.zw;
	float2 uvZ = sd.scaleTransform.xy * (pos.xy * float2(-normalSign.z, 1)) + sd.scaleTransform.zw;

	float4 sampleX, sampleY, sampleZ;
	sampleX = sampleY = sampleZ = 0;

	if (projectedNormal.x > 0)
		sampleX = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_MainTex, uvX);
	if (projectedNormal.y > 0)
		sampleY = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_MainTex, uvY);
	if (projectedNormal.z > 0)
		sampleZ = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_MainTex, uvZ);

	sampleX.xyz = UnpackScaleNormal(sampleX, normalScale);
	sampleY.xyz = UnpackScaleNormal(sampleY, normalScale);
	sampleZ.xyz = UnpackScaleNormal(sampleZ, normalScale);

	return (sampleX * projectedNormal.x) + (sampleY * projectedNormal.y) + (sampleZ * projectedNormal.z);
}

float3 tex2DtriNormal(Texture2D tex, SamplerState ss, SampleData sd, float normalScale, float falloff) {
	float3 surfaceNormal = sd.normal;
	float3 pos = sd.localPos;
    surfaceNormal = abs(surfaceNormal);
	float3 projectedNormal = pow(abs(surfaceNormal), falloff);
    projectedNormal /= (surfaceNormal.x + surfaceNormal.y + surfaceNormal.z);
	float3 normalSign = sign(surfaceNormal);

	float2 uvX = sd.scaleTransform.xy * (pos.zy * float2(normalSign.x, 1)) + sd.scaleTransform.zw;
	float2 uvY = sd.scaleTransform.xy * (pos.xz * float2(normalSign.y, 1)) + sd.scaleTransform.zw;
	float2 uvZ = sd.scaleTransform.xy * (pos.xy * float2(-normalSign.z, 1)) + sd.scaleTransform.zw;

	float4 sampleX, sampleY, sampleZ;
	sampleX = sampleY = sampleZ = 0;

	if (projectedNormal.x > 0)
		sampleX = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, uvX);
	if (projectedNormal.y > 0)
		sampleY = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, uvY);
	if (projectedNormal.z > 0)
		sampleZ = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, uvZ);

	sampleX.xyz = UnpackScaleNormal(sampleX, normalScale);
	sampleY.xyz = UnpackScaleNormal(sampleY, normalScale);
	sampleZ.xyz = UnpackScaleNormal(sampleZ, normalScale);

	return (sampleX * projectedNormal.x) + (sampleY * projectedNormal.y) + (sampleZ * projectedNormal.z);
}

float4 SampleTexture(Texture2D tex, float2 uv){
	float4 col = 0;
	#if STOCHASTIC_ENABLED
		col = tex2Dstoch(tex, sampler_MainTex, uv);
	#elif TSS_ENABLED
		col = tex2Dsuper(tex, sampler_MainTex, uv);
	#else
		col = tex.Sample(sampler_MainTex, uv);
	#endif
	return col;
}

float4 SampleTexture(Texture2D tex, SamplerState ss, float2 uv){
	float4 col = 0;
	#if STOCHASTIC_ENABLED
		col = tex2Dstoch(tex, ss, uv);
	#elif TSS_ENABLED
		col = tex2Dsuper(tex, ss, uv);
	#else
		col = tex.Sample(ss, uv);
	#endif
	return col;
}

float4 SampleTexture(Texture2D tex, float2 uv, SampleData sd){
	float4 col = 0;
	#if STOCHASTIC_ENABLED
		col = tex2Dstoch(tex, sampler_MainTex, uv);
	#elif TSS_ENABLED
		col = tex2Dsuper(tex, sampler_MainTex, uv);
	#elif TRIPLANAR_ENABLED
		col = tex2Dtri(tex, sd, _TriplanarFalloff);
	#else
		col = tex.Sample(sampler_MainTex, uv);
	#endif
	return col;
}

float4 SampleTexture(Texture2D tex, SamplerState ss, float2 uv, SampleData sd){
	float4 col = 0;
	#if STOCHASTIC_ENABLED
		col = tex2Dstoch(tex, ss, uv);
	#elif TSS_ENABLED
		col = tex2Dsuper(tex, ss, uv);
	#elif TRIPLANAR_ENABLED
		col = tex2Dtri(tex, ss, sd, _TriplanarFalloff);
	#else
		col = tex.Sample(ss, uv);
	#endif
	return col;
}

float3 SampleTexture(Texture2D tex, float2 uv, SampleData sd, float normalScale){
	float3 col = 0;
	#if STOCHASTIC_ENABLED
		float4 normalMap = tex2Dstoch(tex, sampler_MainTex, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#elif TSS_ENABLED
		float4 normalMap = tex2Dsuper(tex, sampler_MainTex, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#elif TRIPLANAR_ENABLED
		col = tex2DtriNormal(tex, sd, normalScale, _TriplanarFalloff);
	#else
		float4 normalMap = tex.Sample(sampler_MainTex, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#endif
	return col;
}

float3 SampleTexture(Texture2D tex, SamplerState ss, float2 uv, SampleData sd, float normalScale){
	float3 col = 0;
	#if STOCHASTIC_ENABLED
		float4 normalMap = tex2Dstoch(tex, ss, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#elif TSS_ENABLED
		float4 normalMap = tex2Dsuper(tex, ss, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#elif TRIPLANAR_ENABLED
		col = tex2DtriNormal(tex, ss, sd, normalScale, _TriplanarFalloff);
	#else
		float4 normalMap = tex.Sample(ss, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#endif
	return col;
}

float4 SampleDetailTexture(Texture2D tex, float2 uv){
	float4 col = 0;
	#if DETAIL_STOCHASTIC_ENABLED
		col = tex2Dstoch(tex, sampler_MainTex, uv);
	#elif DETAIL_TSS_ENABLED
		col = tex2Dsuper(tex, sampler_MainTex, uv);
	#else
		col = tex.Sample(sampler_MainTex, uv);
	#endif
	return col;
}

float4 SampleDetailTexture(Texture2D tex, SamplerState ss, float2 uv){
	float4 col = 0;
	#if DETAIL_STOCHASTIC_ENABLED
		col = tex2Dstoch(tex, ss, uv);
	#elif DETAIL_TSS_ENABLED
		col = tex2Dsuper(tex, ss, uv);
	#else
		col = tex.Sample(ss, uv);
	#endif
	return col;
}

float4 SampleDetailTexture(Texture2D tex, float2 uv, SampleData sd){
	float4 col = 0;
	#if DETAIL_STOCHASTIC_ENABLED
		col = tex2Dstoch(tex, sampler_MainTex, uv);
	#elif DETAIL_TSS_ENABLED
		col = tex2Dsuper(tex, sampler_MainTex, uv);
	#elif DETAIL_TRIPLANAR_ENABLED
		col = tex2Dtri(tex, sd, _DetailTriplanarFalloff);
	#else
		col = tex.Sample(sampler_MainTex, uv);
	#endif
	return col;
}

float4 SampleDetailTexture(Texture2D tex, SamplerState ss, float2 uv, SampleData sd){
	float4 col = 0;
	#if DETAIL_STOCHASTIC_ENABLED
		col = tex2Dstoch(tex, ss, uv);
	#elif DETAIL_TSS_ENABLED
		col = tex2Dsuper(tex, ss, uv);
	#elif DETAIL_TRIPLANAR_ENABLED
		col = tex2Dtri(tex, ss, sd, _DetailTriplanarFalloff);
	#else
		col = tex.Sample(ss, uv);
	#endif
	return col;
}

float3 SampleDetailTexture(Texture2D tex, float2 uv, SampleData sd, float normalScale){
	float3 col = 0;
	#if DETAIL_STOCHASTIC_ENABLED
		float4 normalMap = tex2Dstoch(tex, sampler_MainTex, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#elif DETAIL_TSS_ENABLED
		float4 normalMap = tex2Dsuper(tex, sampler_MainTex, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#elif DETAIL_TRIPLANAR_ENABLED
		col = tex2DtriNormal(tex, sd, normalScale, _DetailTriplanarFalloff);
	#else
		float4 normalMap = tex.Sample(sampler_MainTex, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#endif
	return col;
}

float3 SampleDetailTexture(Texture2D tex, SamplerState ss, float2 uv, SampleData sd, float normalScale){
	float3 col = 0;
	#if DETAIL_STOCHASTIC_ENABLED
		float4 normalMap = tex2Dstoch(tex, ss, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#elif DETAIL_TSS_ENABLED
		float4 normalMap = tex2Dsuper(tex, ss, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#elif DETAIL_TRIPLANAR_ENABLED
		col = tex2DtriNormal(tex, ss, sd, normalScale, _DetailTriplanarFalloff);
	#else
		float4 normalMap = tex.Sample(ss, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#endif
	return col;
}

#endif // MOCHIE_STANDARD_SAMPLING_INCLUDED