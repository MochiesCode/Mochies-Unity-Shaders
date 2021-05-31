#ifndef SAMPLING_INCLUDED
#define SAMPLING_INCLUDED

#define UNITY_SAMPLE_TEX2D_GRAD_SAMPLER(tex,samplertex,coord,dx,dy) tex.SampleGrad(sampler##samplertex,coord,dx,dy)

#define UNITY_SAMPLE_TEX2D_LOD_SAMPLER(tex,samplertex,coord) tex.SampleLevel(sampler##samplertex,(coord).xy,(coord).w)

#define UNITY_SAMPLE_TEX2D_BIAS_OFFS_SAMPLER(tex,samplertex,coord,offset) tex.SampleBias(sampler##samplertex,(coord).xy,(coord).w,(offset).xy)

#define UNITY_SAMPLE_TEX2D_BIAS_SAMPLER(tex,samplertex,coord) tex.SampleBias(sampler##samplertex,(coord).xy,(coord).w)

#define UNITY_SAMPLE_TEX2D_LOD(tex,coord) tex.SampleLevel(sampler##tex,(coord).xy,(coord).w)

float3 tex2Dnormal(sampler2D tex, float2 uv, float offset, float strength){
	offset = pow(offset, 3) * 0.1;
	float2 offsetU = float2(uv.x + offset, uv.y);
	float2 offsetV = float2(uv.x, uv.y + offset);
	float normalSample = tex2D(tex, uv);
	float uSample = tex2D(tex, offsetU);
	float vSample = tex2D(tex, offsetV);
	float3 va = float3(1, 0, (uSample - normalSample) * strength);
	float3 vb = float3(0, 1, (vSample - normalSample) * strength);
	return normalize(cross(va, vb));
}

// From https://www.willpodpechan.com/blog/2020/10/16/de-tiled-triplanar-mapping-in-unity
float4 tex2Dstoch(sampler2D tex, float2 uv){
	//skew the uv to create triangular grid
	float2 skewUV = mul(float2x2 (1.0, 0.0, -0.57735027, 1.15470054), uv * 3.464);

	//vertices on the triangular grid
	int2 vertID = int2(floor(skewUV));

	//barycentric coordinates of uv position
	float3 temp = float3(frac(skewUV), 0);
	temp.z = 1.0 - temp.x - temp.y;
	
	//each vertex on the grid gets an according weight value
	int2 vertA, vertB, vertC;
	float weightA, weightB, weightC;

	//determine which triangle we're in
	if (temp.z > 0.0){
		weightA = temp.z;
		weightB = temp.y;
		weightC = temp.x;
		vertA = vertID;
		vertB = vertID + int2(0, 1);
		vertC = vertID + int2(1, 0);
	}
	else {
		weightA = -temp.z;
		weightB = 1.0 - temp.y;
		weightC = 1.0 - temp.x;
		vertA = vertID + int2(1, 1);
		vertB = vertID + int2(1, 0);
		vertC = vertID + int2(0, 1);
	}	

	//get derivatives to avoid triangular artifacts
	float2 dx = ddx_fine(uv);
	float2 dy = ddy_fine(uv);

	//offset uvs using magic numbers
	float2 randomA = uv + frac(sin(fmod(float2(dot(vertA, float2(127.1, 311.7)), dot(vertA, float2(269.5, 183.3))), 3.14159)) * 43758.5453);
	float2 randomB = uv + frac(sin(fmod(float2(dot(vertB, float2(127.1, 311.7)), dot(vertB, float2(269.5, 183.3))), 3.14159)) * 43758.5453);
	float2 randomC = uv + frac(sin(fmod(float2(dot(vertC, float2(127.1, 311.7)), dot(vertC, float2(269.5, 183.3))), 3.14159)) * 43758.5453);
	
	//get texture samples
	float4 sampleA = tex2Dgrad(tex, randomA, dx, dy);
	float4 sampleB = tex2Dgrad(tex, randomB, dx, dy);
	float4 sampleC = tex2Dgrad(tex, randomC, dx, dy);
	
	//blend samples with weights	
	return sampleA * weightA + sampleB * weightB + sampleC * weightC;
}

float4 tex2Dsuper(sampler2D tex, float2 uv){
	float2 dx = ddx_fine(uv);
	float2 dy = ddy_fine(uv);
	float2 uvOffsets = float2(0.125,0.375);
	// float4 offsetUV = float4(0,0,0,_TSSBias);
	float4 offsetUV = float4(0,0,0,-0.25);
	
	half4 col = 0;
	offsetUV.xy = uv + uvOffsets.x * dx + uvOffsets.y * dy;
	col += tex2Dbias(tex, offsetUV);
	offsetUV.xy = uv - uvOffsets.x * dx - uvOffsets.y * dy;
	col += tex2Dbias(tex, offsetUV);
	offsetUV.xy = uv + uvOffsets.y * dx - uvOffsets.x * dy;
	col += tex2Dbias(tex, offsetUV);
	offsetUV.xy = uv - uvOffsets.y * dx + uvOffsets.x * dy;
	col += tex2Dbias(tex, offsetUV);
	col *= 0.25;

	return col;
}

static const float2 kernel[16] = {
	float2(0,0),
	float2(0.54545456,0),
	float2(0.16855472,0.5187581),
	float2(-0.44128203,0.3206101),
	float2(-0.44128197,-0.3206102),
	float2(0.1685548,-0.5187581),
	float2(1,0),
	float2(0.809017,0.58778524),
	float2(0.30901697,0.95105654),
	float2(-0.30901703,0.9510565),
	float2(-0.80901706,0.5877852),
	float2(-1,0),
	float2(-0.80901694,-0.58778536),
	float2(-0.30901664,-0.9510566),
	float2(0.30901712,-0.9510565),
	float2(0.80901694,-0.5877853),
};

// edgeRadius is 0-1 property range (default 0.5)
float tex2Dsobel(sampler2D tex, float2 uv, float edgeRadius) {
	edgeRadius = lerp(0.0001, 0.005, edgeRadius);
	float2 delta = edgeRadius.xx;
	
	float4 hr = 0;
	float4 vt = 0;
	
	hr += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)) *  1.0;
	hr += tex2D(tex, (uv + float2( 0.0, -1.0) * delta)) *  0.0;
	hr += tex2D(tex, (uv + float2( 1.0, -1.0) * delta)) * -1.0;
	hr += tex2D(tex, (uv + float2(-1.0,  0.0) * delta)) *  2.0;
	hr += tex2D(tex, (uv + float2( 0.0,  0.0) * delta)) *  0.0;
	hr += tex2D(tex, (uv + float2( 1.0,  0.0) * delta)) * -2.0;
	hr += tex2D(tex, (uv + float2(-1.0,  1.0) * delta)) *  1.0;
	hr += tex2D(tex, (uv + float2( 0.0,  1.0) * delta)) *  0.0;
	hr += tex2D(tex, (uv + float2( 1.0,  1.0) * delta)) * -1.0;
	
	vt += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)) *  1.0;
	vt += tex2D(tex, (uv + float2( 0.0, -1.0) * delta)) *  2.0;
	vt += tex2D(tex, (uv + float2( 1.0, -1.0) * delta)) *  1.0;
	vt += tex2D(tex, (uv + float2(-1.0,  0.0) * delta)) *  0.0;
	vt += tex2D(tex, (uv + float2( 0.0,  0.0) * delta)) *  0.0;
	vt += tex2D(tex, (uv + float2( 1.0,  0.0) * delta)) *  0.0;
	vt += tex2D(tex, (uv + float2(-1.0,  1.0) * delta)) * -1.0;
	vt += tex2D(tex, (uv + float2( 0.0,  1.0) * delta)) * -2.0;
	vt += tex2D(tex, (uv + float2( 1.0,  1.0) * delta)) * -1.0;
	
	return sqrt(hr * hr + vt * vt);
}

// edgeBlur is 0-1 property range (default 0.1)
float4 tex2Dblur(sampler2D tex, float2 uv, float edgeBlur){
	float4 blurCol = 0;
	float strength = edgeBlur * 0.001;
	UNITY_UNROLL
	for (int i = 0; i < 16; i++){
		blurCol += tex2D(tex, uv + (strength * kernel[i]));
	}	
	return blurCol /= 16;
}

#ifdef MOCHIE_STANDARD // Only supports standard mod for now due to struct requirements
	
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

float4 tex2Ddecal(sampler2D tex, SampleData sd){
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
	float4 col = tex2D(tex, uv);
	col.a *= smoothstep(_EdgeFadeMax,_EdgeFadeMin, distance(wpos, sd.objPos));
	return col;
}

float3 tex2DdecalNormal(sampler2D tex, SampleData sd, float normalScale){
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
	return UnpackScaleNormal(tex2D(tex, uv), normalScale);
}
#endif

// Based on Xiexe's implementation
// https://github.com/Xiexe/XSEnvironmentShaders/blob/bf992e8e292a0562ce4164964f16b3abdc97f078/XSEnvironment/LightingFunctions.cginc#L213

float4 tex2Dtri(sampler2D tex, SampleData sd) {
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
		sampleX = tex2D(tex, uvX);
	if (projectedNormal.y > 0)
		sampleY = tex2D(tex, uvY);
	if (projectedNormal.z > 0)
		sampleZ = tex2D(tex, uvZ);

	return (sampleX * projectedNormal.x) + (sampleY * projectedNormal.y) + (sampleZ * projectedNormal.z);
}

float3 tex2DtriNormal(sampler2D tex, SampleData sd, float normalScale) {
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
		sampleX = tex2D(tex, uvX);
	if (projectedNormal.y > 0)
		sampleY = tex2D(tex, uvY);
	if (projectedNormal.z > 0)
		sampleZ = tex2D(tex, uvZ);

	sampleX.xyz = UnpackScaleNormal(sampleX, normalScale);
	sampleY.xyz = UnpackScaleNormal(sampleY, normalScale);
	sampleZ.xyz = UnpackScaleNormal(sampleZ, normalScale);

	return (sampleX * projectedNormal.x) + (sampleY * projectedNormal.y) + (sampleZ * projectedNormal.z);
}

float4 SampleTexture(sampler2D tex, float2 uv){
	float4 col = 0;
	#if STOCHASTIC_ENABLED
		col = tex2Dstoch(tex, uv);
	#elif TSS_ENABLED
		col = tex2Dsuper(tex, uv);
	#else
		col = tex2D(tex, uv);
	#endif
	return col;
}

float4 SampleTexture(sampler2D tex, float2 uv, SampleData sd){
	float4 col = 0;
	#if STOCHASTIC_ENABLED
		col = tex2Dstoch(tex, uv);
	#elif TSS_ENABLED
		col = tex2Dsuper(tex, uv);
	#elif TRIPLANAR_ENABLED
		col = tex2Dtri(tex, sd);
	#elif DECAL_ENABLED
		col = tex2Ddecal(tex, sd);
	#else
		col = tex2D(tex, uv);
	#endif
	return col;
}

float3 SampleTexture(sampler2D tex, float2 uv, SampleData sd, float normalScale){
	float3 col = 0;
	#if STOCHASTIC_ENABLED
		float4 normalMap = tex2Dstoch(tex, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#elif TSS_ENABLED
		float4 normalMap = tex2Dsuper(tex, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#elif TRIPLANAR_ENABLED
		col = tex2DtriNormal(tex, sd, normalScale);
	#elif DECAL_ENABLED
		col = tex2DdecalNormal(tex, sd, normalScale);
	#else
		float4 normalMap = tex2D(tex, uv);
		col = UnpackScaleNormal(normalMap, normalScale);
	#endif
	return col;
}

#endif // MOCHIE STANDARD

#endif // SAMPLING_INCLUDED