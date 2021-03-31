#define TSS_ENABLED defined(BLOOM)
#define STOCHASTIC_ENABLED defined(EFFECT_HUE_VARIATION)

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

#if STOCHASTIC_ENABLED
	#define tex2D tex2Dstoch
#elif TSS_ENABLED
	#define tex2D tex2Dsuper
#endif