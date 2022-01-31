
#ifndef MOCHIE_STANDARD_SAMPLING_INCLUDED
#define MOCHIE_STANDARD_SAMPLING_INCLUDED

Texture2D		_MainTex;
SamplerState 	sampler_MainTex;
#if DETAIL_BASECOLOR
	Texture2D		_DetailAlbedoMap;
	SamplerState 	sampler_DetailAlbedoMap;
#endif
#if DETAIL_ROUGH
	Texture2D		_DetailRoughnessMap;
	SamplerState	sampler_DetailRoughnessMap;
#endif
#if DETAIL_AO
	Texture2D		_DetailAOMap;
	SamplerState	sampler_DetailAOMap;
#endif
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
	float rotation;
};

float _TriplanarFalloff;
float _EdgeFadeMin;
float _EdgeFadeMax;

#if DECAL_ENABLED

float2 RotateDecalUV(float2 coords, float rot){
	rot *= (UNITY_PI/180.0);
	float sinVal = sin(rot);
	float cosX = cos(rot);
	float2x2 mat = float2x2(cosX, -sinVal, sinVal, cosX);
	mat = ((mat*0.5)+0.5)*2-1;
	return mul(coords, mat);;
}

float4 tex2Ddecal(Texture2D tex, SampleData sd){
	float3 normal = sd.depthNormal;
	float3 wpos = sd.worldPixelPos;
	float stepXY = step(normal.x, normal.y);
	float stepYX = step(normal.y, normal.x);
	float stepXZ = step(normal.x, normal.z);
	float stepYZ = step(normal.y, normal.z);
	float stepZX = step(normal.z, normal.x);
	float stepZY = step(normal.z, normal.y);
	float2 uv = stepYX*stepZX*((wpos.zy - sd.objPos.zy)*sd.scaleTransform.xy + float2(0.5, 0.5))+
				stepXY*stepZY*((wpos.xz - sd.objPos.xz)*sd.scaleTransform.xy + float2(0.5, 0.5))+
				stepXZ*stepYZ*((wpos.xy - sd.objPos.xy)*sd.scaleTransform.xy + float2(0.5, 0.5));
	uv = RotateDecalUV(uv, sd.rotation);
	float4 col = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_MainTex, uv); // tex.Sample(sampler_MainTex, uv);
	col.a *= smoothstep(_EdgeFadeMax,_EdgeFadeMin, distance(wpos, sd.objPos));
	return col;
}

float3 tex2DdecalNormal(Texture2D tex, SampleData sd, float normalScale){
	float3 normal = sd.depthNormal;
	float3 wpos = sd.worldPixelPos;
	float stepXY = step(normal.x, normal.y);
	float stepYX = step(normal.y, normal.x);
	float stepXZ = step(normal.x, normal.z);
	float stepYZ = step(normal.y, normal.z);
	float stepZX = step(normal.z, normal.x);
	float stepZY = step(normal.z, normal.y);
	float2 uv = stepYX*stepZX*((wpos.zy - sd.objPos.zy)*sd.scaleTransform.xy + float2(0.5, 0.5))+
				stepXY*stepZY*((wpos.xz - sd.objPos.xz)*sd.scaleTransform.xy + float2(0.5, 0.5))+
				stepXZ*stepYZ*((wpos.xy - sd.objPos.xy)*sd.scaleTransform.xy + float2(0.5, 0.5));
	uv = RotateDecalUV(uv, sd.rotation);
	return UnpackScaleNormal(MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_MainTex, uv), normalScale);
}
#endif

// Based on Xiexe's implementation
// https://github.com/Xiexe/XSEnvironmentShaders/blob/bf992e8e292a0562ce4164964f16b3abdc97f078/XSEnvironment/LightingFunctions.cginc#L213

float4 tex2Dtri(Texture2D tex, SampleData sd) {
	float3 surfaceNormal = sd.normal;
	float3 pos = sd.localPos;
    surfaceNormal = abs(surfaceNormal);
	float3 projectedNormal = pow(abs(surfaceNormal), _TriplanarFalloff);
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

float4 tex2Dtri(Texture2D tex, SamplerState ss, SampleData sd) {
	float3 surfaceNormal = sd.normal;
	float3 pos = sd.localPos;
    surfaceNormal = abs(surfaceNormal);
	float3 projectedNormal = pow(abs(surfaceNormal), _TriplanarFalloff);
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

float3 tex2DtriNormal(Texture2D tex, SampleData sd, float normalScale) {
	float3 surfaceNormal = sd.normal;
	float3 pos = sd.localPos;
    surfaceNormal = abs(surfaceNormal);
	float3 projectedNormal = pow(abs(surfaceNormal), _TriplanarFalloff);
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

float3 tex2DtriNormal(Texture2D tex, SamplerState ss, SampleData sd, float normalScale) {
	float3 surfaceNormal = sd.normal;
	float3 pos = sd.localPos;
    surfaceNormal = abs(surfaceNormal);
	float3 projectedNormal = pow(abs(surfaceNormal), _TriplanarFalloff);
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
		col = tex2Dtri(tex, sd);
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
		col = tex2Dtri(tex, ss, sd);
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
		col = tex2DtriNormal(tex, sd, normalScale);
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
		col = tex2DtriNormal(tex, ss, sd, normalScale);
	#else
		float4 normalMap = tex.Sample(ss, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#endif
	return col;
}

#endif // MOCHIE_STANDARD_SAMPLING_INCLUDED