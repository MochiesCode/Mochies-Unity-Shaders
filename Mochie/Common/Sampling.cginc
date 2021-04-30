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

#endif // SAMPLING_INCLUDED