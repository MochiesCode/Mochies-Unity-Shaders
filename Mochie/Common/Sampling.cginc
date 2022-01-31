#ifndef SAMPLING_INCLUDED
#define SAMPLING_INCLUDED

// Replacement sampling macros to ensure everything always stays in dx11 territory
// Default unity macros will translate some things to dx9 which kills SamplerState syntax
#define MOCHIE_DECLARE_TEX2D(tex)														Texture2D tex; SamplerState sampler##tex
#define MOCHIE_DECLARE_TEX2D_NOSAMPLER(tex)												Texture2D tex
#define MOCHIE_DECLARE_TEX2DARRAY(tex)													Texture2DArray tex; SamplerState sampler##tex
#define MOCHIE_DECLARE_TEX2DARRAY_NOSAMPLER(tex)										Texture2DArray tex

#define MOCHIE_SAMPLE_TEX2D(tex,coord)													tex.Sample(sampler##tex,coord)
#define MOCHIE_SAMPLE_TEX2D_LOD(tex,coord,lod)											tex.SampleLevel(sampler##tex,coord,lod)
#define MOCHIE_SAMPLE_TEX2D_BIAS(tex,coord,bias)										tex.SampleBias(sampler##tex,coord,bias)
#define MOCHIE_SAMPLE_TEX2D_BIAS_OFFS(tex,coord,bias,offset)							tex.SampleBias(sampler##tex,coord,bias,offset.xy)
#define MOCHIE_SAMPLE_TEX2D_GRAD(tex,coord,dx,dy)										tex.SampleGrad(sampler##tex,coord,dx,dy)

#define MOCHIE_SAMPLE_TEX2D_SAMPLER(tex,texSampler,coord)								tex.Sample(texSampler,coord)
#define MOCHIE_SAMPLE_TEX2D_SAMPLER_LOD(tex,texSampler,coord,lod)						tex.SampleLevel(texSampler,coord,lod)
#define MOCHIE_SAMPLE_TEX2D_SAMPLER_BIAS(tex,texSampler,coord,bias)						tex.SampleBias(texSampler,coord,bias)
#define MOCHIE_SAMPLE_TEX2D_SAMPLER_BIAS_OFFS(tex,texSampler,coord,bias,offset)			tex.SampleBias(texSampler,coord,bias,offset.xy)
#define MOCHIE_SAMPLE_TEX2D_SAMPLER_GRAD(tex,texSampler,coord,dx,dy)					tex.SampleGrad(texSampler,coord,dx,dy)

#define MOCHIE_SAMPLE_TEX2DARRAY(tex,coord)												tex.Sample(sampler##tex,coord)
#define MOCHIE_SAMPLE_TEX2DARRAY_LOD(tex,coord,lod)										tex.SampleLevel(sampler##tex,coord,lod)
#define MOCHIE_SAMPLE_TEX2DARRAY_BIAS(tex,coord,bias)									tex.SampleBias(sampler##tex,coord,bias)
#define MOCHIE_SAMPLE_TEX2DARRAY_BIAS_OFFS(tex,coord,bias,offset)						tex.SampleBias(sampler##tex,coord,bias,offset.xy)
#define MOCHIE_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy)									tex.SampleGrad(sampler##tex,coord,dx,dy)

#define MOCHIE_SAMPLE_TEX2DARRAY_SAMPLER(tex,texSampler,coord)							tex.Sample(texSampler,coord)
#define MOCHIE_SAMPLE_TEX2DARRAY_SAMPLER_LOD(tex,texSampler,coord,lod)					tex.SampleLevel(texSampler,coord,lod)
#define MOCHIE_SAMPLE_TEX2DARRAY_SAMPLER_BIAS(tex,texSampler,coord,bias)				tex.SampleBias(texSampler,coord,bias)
#define MOCHIE_SAMPLE_TEX2DARRAY_SAMPLER_BIAS_OFFS(tex,texSampler,coord,bias,offset)	tex.SampleBias(texSampler,coord,bias,offset.xy)
#define MOCHIE_SAMPLE_TEX2DARRAY_SAMPLER_GRAD(tex,texSampler,coord,dx,dy)				tex.SampleGrad(texSampler,coord,dx,dy)

#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)

#define MOCHIE_DECLARE_TEX2D_SCREENSPACE(tex)											MOCHIE_DECLARE_TEX2DARRAY(tex)
#define MOCHIE_DECLARE_TEX2D_SCREENSPACE_NOSAMPLER(tex)									MOCHIE_DECLARE_TEX2DARRAY_NOSAMPLER(tex)
#define MOCHIE_SAMPLE_TEX2D_SCREENSPACE(tex,coord)										MOCHIE_SAMPLE_TEX2DARRAY_LOD(tex,float3(coord,(float)unity_StereoEyeIndex),0)
#define MOCHIE_SAMPLE_TEX2D_SCREENSPACE_SAMPLER(tex,texSampler,coord)					MOCHIE_SAMPLE_TEX2DARRAY_SAMPLER_LOD(tex,texSampler,float3(coord,(float)unity_StereoEyeIndex),0)

#else

#define MOCHIE_DECLARE_TEX2D_SCREENSPACE(tex)											MOCHIE_DECLARE_TEX2D(tex)
#define MOCHIE_DECLARE_TEX2D_SCREENSPACE_NOSAMPLER(tex)									MOCHIE_DECLARE_TEX2D_NOSAMPLER(tex)
#define MOCHIE_SAMPLE_TEX2D_SCREENSPACE(tex,coord)										MOCHIE_SAMPLE_TEX2D_LOD(tex,coord,0)
#define MOCHIE_SAMPLE_TEX2D_SCREENSPACE_SAMPLER(tex,texSampler,coord)					MOCHIE_SAMPLE_TEX2D_SAMPLER_LOD(tex,texSampler,coord,0)

#endif // defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)

// From shadergraph
float3 tex2Dnormal(Texture2D tex, SamplerState ss, float2 uv, float offset, float strength){
	offset = pow(offset, 3) * 0.1;
	float2 offsetU = float2(uv.x + offset, uv.y);
	float2 offsetV = float2(uv.x, uv.y + offset);
	float normalSample = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, uv);
	float uSample = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, offsetU);
	float vSample = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, offsetV);
	float3 va = float3(1, 0, (uSample - normalSample) * strength);
	float3 vb = float3(0, 1, (vSample - normalSample) * strength);
	return normalize(cross(va, vb));
}

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
float4 tex2Dstoch(Texture2D tex, SamplerState ss, float2 uv){
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
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);

	//offset uvs using magic numbers
	float2 randomA = uv + frac(sin(fmod(float2(dot(vertA, float2(127.1, 311.7)), dot(vertA, float2(269.5, 183.3))), 3.14159)) * 43758.5453);
	float2 randomB = uv + frac(sin(fmod(float2(dot(vertB, float2(127.1, 311.7)), dot(vertB, float2(269.5, 183.3))), 3.14159)) * 43758.5453);
	float2 randomC = uv + frac(sin(fmod(float2(dot(vertC, float2(127.1, 311.7)), dot(vertC, float2(269.5, 183.3))), 3.14159)) * 43758.5453);
	
	//get texture samples
	float4 sampleA = MOCHIE_SAMPLE_TEX2D_SAMPLER_GRAD(tex, ss, randomA, dx, dy);
	float4 sampleB = MOCHIE_SAMPLE_TEX2D_SAMPLER_GRAD(tex, ss, randomB, dx, dy);
	float4 sampleC = MOCHIE_SAMPLE_TEX2D_SAMPLER_GRAD(tex, ss, randomC, dx, dy);
	
	//blend samples with weights	
	return sampleA * weightA + sampleB * weightB + sampleC * weightC;
}

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
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);

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

// From https://bgolus.medium.com/sharper-mipmapping-using-shader-based-supersampling-ed7aadb47bec
float4 tex2Dsuper(Texture2D tex, SamplerState ss, float2 uv){
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);
	float2 uvOffsets = float2(0.125,0.375);
	float4 offsetUV = float4(0,0,0,-0.25);
	
	half4 col = 0;
	offsetUV.xy = uv + uvOffsets.x * dx + uvOffsets.y * dy;
	col += MOCHIE_SAMPLE_TEX2D_SAMPLER_BIAS(tex, ss, offsetUV.xy, offsetUV.w);
	offsetUV.xy = uv - uvOffsets.x * dx - uvOffsets.y * dy;
	col += MOCHIE_SAMPLE_TEX2D_SAMPLER_BIAS(tex, ss, offsetUV.xy, offsetUV.w);
	offsetUV.xy = uv + uvOffsets.y * dx - uvOffsets.x * dy;
	col += MOCHIE_SAMPLE_TEX2D_SAMPLER_BIAS(tex, ss, offsetUV.xy, offsetUV.w);
	offsetUV.xy = uv - uvOffsets.y * dx + uvOffsets.x * dy;
	col += MOCHIE_SAMPLE_TEX2D_SAMPLER_BIAS(tex, ss, offsetUV.xy, offsetUV.w);
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
float tex2Dsobel(Texture2D tex, SamplerState ss, float2 uv, float edgeRadius) {
	edgeRadius = lerp(0.0001, 0.005, edgeRadius);
	float2 delta = edgeRadius.xx;
	
	float4 hr = 0;
	float4 vt = 0;
	
	hr += MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, (uv + float2(-1.0, -1.0) * delta));
	hr += MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, (uv + float2( 1.0, -1.0) * delta)) * -1.0;
	hr += MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, (uv + float2(-1.0,  0.0) * delta)) *  2.0;
	hr += MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, (uv + float2( 1.0,  0.0) * delta)) * -2.0;
	hr += MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, (uv + float2(-1.0,  1.0) * delta));
	hr += MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, (uv + float2( 1.0,  1.0) * delta)) * -1.0;
	
	vt += MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, (uv + float2(-1.0, -1.0) * delta));
	vt += MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, (uv + float2( 0.0, -1.0) * delta)) *  2.0;
	vt += MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, (uv + float2( 1.0, -1.0) * delta));
	vt += MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, (uv + float2(-1.0,  1.0) * delta)) * -1.0;
	vt += MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, (uv + float2( 0.0,  1.0) * delta)) * -2.0;
	vt += MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, (uv + float2( 1.0,  1.0) * delta)) * -1.0;
	
	return sqrt(hr * hr + vt * vt);
}

// edgeBlur is 0-1 property range (default 0.1)
float4 tex2Dblur(Texture2D tex, SamplerState ss, float2 uv, float edgeBlur){
	float4 blurCol = 0;
	float strength = edgeBlur * 0.001;
	[unroll(16)]
	for (int i = 0; i < 16; i++){
		blurCol += MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, ss, uv + (strength * kernel[i]));
	}	
	return blurCol /= 16;
}

float4 tex2Dblur(sampler2D tex, float2 uv, float edgeBlur){
	float4 blurCol = 0;
	float strength = edgeBlur * 0.001;
	[unroll(16)]
	for (int i = 0; i < 16; i++){
		blurCol += tex2D(tex, uv + (strength * kernel[i]));
	}	
	return blurCol /= 16;
}

#endif // SAMPLING_INCLUDED