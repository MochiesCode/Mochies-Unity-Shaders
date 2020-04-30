float AverageRGB(float3 rgb){
    return (rgb.r + rgb.g + rgb.b)/3;
}

void SamplePackedMask(g2f i, inout masks m){
	float4 mask0 = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMask0, _MainTex, i.uv.xy);
	float4 mask1 = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMask1, _MainTex, i.uv.xy);
	
	// Mask 0
	m.reflectionMask = mask0.r;
	m.specularMask = mask0.g;
	m.detailMask = mask0.b;
	m.shadowMask = mask0.a;
	
	// Mask 1
	m.rimMask = mask1.r;
	m.matcapMask = mask1.g;
	m.ddMask = mask1.b;
	m.smoothMask = mask1.a;
}

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
	switch (channel){
		case 0: mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).r; break;
		case 1: mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).g; break;
		case 2: mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).b; break;
		case 3: mask = UNITY_SAMPLE_TEX2D_SAMPLER(tex, _MainTex, uv).a; break;
		default: break;
	}
	mask *= str;
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

float linearstep(float j, float k, float x) {
	x = clamp((x - j) / (k - j), 0.0, 1.0); 
	return x;
}

float4 lerp34(float4 a, float4 b, float4 c, float t){
	if (t <= 1)
		return lerp(a, b, t);
	else
		return lerp(b, c, t*0.5);
}

float3 lerp33(float3 a, float3 b, float3 c, float t){
	if (t <= 1)
		return lerp(a, b, t);
	else
		return lerp(b, c, t*0.5);
}

float2 lerp32(float2 a, float2 b, float2 c, float t){
	if (t <= 1)
		return lerp(a, b, t);
	else
		return lerp(b, c, t*0.5);
}

float2 lerp3(float a, float b, float c, float t){
	if (t <= 1)
		return lerp(a, b, t);
	else
		return lerp(b, c, t*0.5);
}

float smootherstep(float edge0, float edge1, float x) {
    x = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
    return x * x * x * (x * (x * 6 - 15) + 10);    
}

float2x2 GetRotationMatrix(float axis){
	float c, s, ang;
	ang = (axis+90) * (UNITY_PI/180.0);
	sincos(ang, c, s);
	float2x2 mat = float2x2(c,-s,s,c);
	mat = ((mat*0.5)+0.5)*2-1;
	return mat;
}

float3 Rotate(float3 coords, float3 axis){
	coords.xy = mul(GetRotationMatrix(axis.x), coords.xy);
	coords.xz = mul(GetRotationMatrix(axis.y), coords.xz);
	coords.yz = mul(GetRotationMatrix(axis.z), coords.yz);
    return coords;
}

float3 GetNoiseRGB(float2 p, float strength) {
    float3 rgb = 0;
    UNITY_BRANCH
    if (strength > 0){
        float3 p3 = frac(float3(p.xyx) * (float3(443.8975, 397.2973, 491.1871)+frac(_Time.y)));
        p3 += dot(p3, p3.yxz + 19.19);
        rgb = frac(float3((p3.x + p3.y)*p3.z, (p3.x + p3.z)*p3.y, (p3.y + p3.z)*p3.x));
        rgb = (rgb-0.5)*2*strength;
    }
	return rgb;
}

float GetNoise(float2 p){
	float n = frac(sin(p.x*100+p.y*6574)*5647);
	n = linearstep(-1,1,n);
	return n;
}

bool IsInMirror() {
    return unity_CameraProjection[2][0] != 0 || unity_CameraProjection[2][1] != 0;
}

float3 FlowUV (float2 uv, float time, float phase) {
	float progress = frac(time + phase);
	float waveform = 1-abs(1-2 * progress);
	uv += (time - progress) * float2(0.24, 0.2083333);
	float3 uvw = float3(uv, waveform);
	return uvw;
}

float oetf_sRGB_scalar(float L) {
	float V = 1.055 * (pow(L, 1.0 / 2.4)) - 0.055;
	if (L <= 0.0031308)
		V = L * 12.92;
	return V;
}

float3 oetf_sRGB(float3 L) {
	return float3(oetf_sRGB_scalar(L.r), oetf_sRGB_scalar(L.g), oetf_sRGB_scalar(L.b));
}

float eotf_sRGB_scalar(float V) {
	float L = pow((V + 0.055) / 1.055, 2.4);
	if (V <= oetf_sRGB_scalar(0.0031308))
		L = V / 12.92;
	return L;
}

float3 GetHDR(float3 rgb) {
	return float3(eotf_sRGB_scalar(rgb.r), eotf_sRGB_scalar(rgb.g), eotf_sRGB_scalar(rgb.b));
}

float3 GetContrast(float3 col){
    return clamp((lerp(float3(0.5,0.5,0.5), col, _Contrast)), 0, 10);
}

float3 GetSaturation(float3 col, float interpolator){
    return lerp(dot(col, float3(0.3,0.59,0.11)), col, interpolator);
}

const static float EPS = 1e-10;
float3 RGBtoHCV(in float3 rgb) {
    float4 P = lerp(float4(rgb.bg, -1.0, 2.0/3.0), float4(rgb.gb, 0.0, -1.0/3.0), step(rgb.b, rgb.g));
    float4 Q = lerp(float4(P.xyw, rgb.r), float4(rgb.r, P.yzx), step(P.x, rgb.r));
    float C = Q.x - min(Q.w, Q.y);
    float H = abs((Q.w - Q.y) / (6 * C + EPS) + Q.z);
    return float3(H, C, Q.x);
}

float3 RGBtoHSL(in float3 rgb) {
    float3 HCV = RGBtoHCV(rgb);
    float L = HCV.z - HCV.y * 0.5;
    float S = HCV.y / (1 - abs(L * 2 - 1) + EPS);
    return float3(HCV.x, S, L);
}

float3 HSLtoRGB(float3 c) {
    c = float3(frac(c.x), clamp(c.yz, 0.0, 1.0));
    float3 rgb = clamp(abs(fmod(c.x * 6.0 + float3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return c.z + c.y * (rgb - 0.5) * (1.0 - abs(2.0 * c.z - 1.0));
}