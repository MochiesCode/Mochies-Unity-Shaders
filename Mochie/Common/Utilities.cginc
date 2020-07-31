bool IsInMirror(){
	return unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f;
}

void MirrorCheck(){
	UNITY_BRANCH
    if (IsInMirror()) discard;
}

float Average(float3 col){
    return (col.r + col.g + col.b)/3.0;
}

float Average(float4 value){
	return (value.x + value.y + value.z + value.w)/4.0;
}

float AverageRGB(float3 col){
    return (col.r + col.g + col.b)/3.0;
}

float maxRGB(float3 col){
	return max(max(col.r, col.g), col.b);
}

float max2(float2 p){
	return max(p.x, p.y);
}

float max3(float3 p){
	return max(max(p.x, p.y), p.z);
}

float max4(float4 p){
	return max(max(max(p.x, p.y), p.z), p.w);
}

float Pow5 (float x){
    return x*x * x*x * x;
}

float2 Pow5 (float2 x){
    return x*x * x*x * x;
}

float3 Pow5 (float3 x){
    return x*x * x*x * x;
}

float4 Pow5 (float4 x){
    return x*x * x*x * x;
}

// ---------------------------
// Remapping/Interpolation
// ---------------------------
float4 lerp3(float4 a, float4 b, float4 c, float t){
	if (t <= 1)
		return lerp(a, b, t);
	else
		return lerp(b, c, t*0.5);
}

float3 lerp3(float3 a, float3 b, float3 c, float t){
	if (t <= 1)
		return lerp(a, b, t);
	else
		return lerp(b, c, t*0.5);
}

float2 lerp3(float2 a, float2 b, float2 c, float t){
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

float linearstep(float j, float k, float x) {
	x = clamp((x - j) / (k - j), 0.0, 1.0); 
	return x;
}

float2 linearstep(float2 j, float2 k, float2 x) {
	x = clamp((x - j) / (k - j), 0.0, 1.0); 
	return x;
}

float3 linearstep(float3 j, float3 k, float3 x) {
	x = clamp((x - j) / (k - j), 0.0, 1.0); 
	return x;
}

float4 linearstep(float4 j, float4 k, float4 x) {
	x = clamp((x - j) / (k - j), 0.0, 1.0); 
	return x;
}

float cubicstep(float x){
	float curve = linearstep(-1,1,1-pow(UNITY_PI * x / 2.0, 0.5));
	return pow(curve, 5);
}

float expstep(float x, float k, float n){
    return exp( -k*pow(x,n) );
}

float smoothlerp(float x, float y, float z){
    return lerp(x, y, smoothstep(0, 1, z));
}

float smootherstep(float edge0, float edge1, float x) {
    x = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
    return x * x * x * (x * (x * 6 - 15) + 10);    
}
float2 smootherstep(float2 edge0, float2 edge1, float2 x) {
    x = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
    return x * x * x * (x * (x * 6 - 15) + 10);    
}
float3 smootherstep(float3 edge0, float3 edge1, float3 x) {
    x = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
    return x * x * x * (x * (x * 6 - 15) + 10);    
}
float4 smootherstep(float4 edge0, float4 edge1, float4 x) {
    x = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
    return x * x * x * (x * (x * 6 - 15) + 10);    
}

// From poi's helper functions
float Remap(float x, float minO, float maxO, float minN, float maxN){
	return minN + (x - minO) * (maxN - minN) / (maxO - minO);
}

float2 Remap(float2 x, float2 minO, float2 maxO, float2 minN, float2 maxN){
	return minN + (x - minO) * (maxN - minN) / (maxO - minO);
}

float3 Remap(float3 x, float3 minO, float3 maxO, float3 minN, float3 maxN){
	return minN + (x - minO) * (maxN - minN) / (maxO - minO);
}

float4 Remap(float4 x, float4 minO, float4 maxO, float4 minN, float4 maxN){
	return minN + (x - minO) * (maxN - minN) / (maxO - minO);
}

float RemapClamped(float x, float minO, float maxO, float minN, float maxN){
	return clamp(minN + (x - minO) * (maxN - minN) / (maxO - minO), minN, maxN);
}

float2 RemapClamped(float2 x, float2 minO, float2 maxO, float2 minN, float2 maxN){
	return clamp(minN + (x - minO) * (maxN - minN) / (maxO - minO), minN, maxN);
}

float3 RemapClamped(float3 x, float3 minO, float3 maxO, float3 minN, float3 maxN){
	return clamp(minN + (x - minO) * (maxN - minN) / (maxO - minO), minN, maxN);
}

float4 RemapClamped(float4 x, float4 minO, float4 maxO, float4 minN, float4 maxN){
	return clamp(minN + (x - minO) * (maxN - minN) / (maxO - minO), minN, maxN);
}

float SmoothFalloff(float minR, float maxR, float dist){
	return smoothstep(maxR, clamp(minR, 0, maxR-0.001), dist);
}

float GetFalloff(int ug, float gf, float minR, float maxR, float d){
    UNITY_BRANCH
    if (ug == 0)
        return SmoothFalloff(minR, maxR, d);
    else
        return gf;
}

#if defined(HAS_DEPTH_TEXTURE)

// Clean world normals by Neitri - https://github.com/netri/Neitri-Unity-Shaders/blob/master/Wireframe%20Overlay.shader
float4x4 inverse(float4x4 input){
	#define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))
	float4x4 cofactors = float4x4(
		minor(_22_23_24, _32_33_34, _42_43_44), 
		-minor(_21_23_24, _31_33_34, _41_43_44),
		minor(_21_22_24, _31_32_34, _41_42_44),
		-minor(_21_22_23, _31_32_33, _41_42_43),

		-minor(_12_13_14, _32_33_34, _42_43_44),
		minor(_11_13_14, _31_33_34, _41_43_44),
		-minor(_11_12_14, _31_32_34, _41_42_44),
		minor(_11_12_13, _31_32_33, _41_42_43),

		minor(_12_13_14, _22_23_24, _42_43_44),
		-minor(_11_13_14, _21_23_24, _41_43_44),
		minor(_11_12_14, _21_22_24, _41_42_44),
		-minor(_11_12_13, _21_22_23, _41_42_43),

		-minor(_12_13_14, _22_23_24, _32_33_34),
		minor(_11_13_14, _21_23_24, _31_33_34),
		-minor(_11_12_14, _21_22_24, _31_32_34),
		minor(_11_12_13, _21_22_23, _31_32_33)
	);
	#undef minor
	return transpose(cofactors) / determinant(input);
}

float3 GetWorldSpacePixelPos(float4 vertex, float2 screenOffset){
	float4 worldPos = mul(unity_ObjectToWorld, float4(vertex.xyz, 1));
	float4 screenPos = mul(UNITY_MATRIX_VP, worldPos); 
	screenPos.xy += screenOffset * screenPos.w;
	worldPos = mul(inverse(UNITY_MATRIX_VP), screenPos);
	float3 worldDir = worldPos.xyz - _WorldSpaceCameraPos;
	float2 screenUV = screenPos.xy / screenPos.w;
	screenUV.y *= _ProjectionParams.x;
	screenUV = screenUV * 0.5f + 0.5f;
	screenUV = UnityStereoTransformScreenSpaceTex(screenUV);
	float depth = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, screenUV))) / screenPos.w;
	float3 worldSpacePos = worldDir * depth + _WorldSpaceCameraPos;
	return worldSpacePos;
}

void GetWorldNormals(float4 localPos, out float3 worldNormal, out float3 worldPos){
	float2 offset = 1.01 / _ScreenParams.xy; 
	worldPos = GetWorldSpacePixelPos(localPos, float2(0,0));
	float3 worldPos1 = GetWorldSpacePixelPos(localPos, float2(0, offset.y));
	float3 worldPos2 = GetWorldSpacePixelPos(localPos, float2(-offset.x, 0));
	worldNormal = normalize(cross(worldPos1 - worldPos, worldPos2 - worldPos));
}

#endif

float4 GetScreenspaceVertexPos(float4 vertex){
	float4 wPos = mul(unity_CameraToWorld, vertex);
	float4 oPos = mul(unity_WorldToObject, wPos);
	return UnityObjectToClipPos(oPos);
}

float3 GetObjPos(){
    return mul(unity_ObjectToWorld, float4(0,0,0,1));
}

float3 GetCameraPos(){
    float3 cameraPos = _WorldSpaceCameraPos;
    #if UNITY_SINGLE_PASS_STEREO
        cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5;
    #endif
    return cameraPos;
}

float3 GetForwardVector(){
	#if UNITY_SINGLE_PASS_STEREO
		float3 p1 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 1, 1));
		float3 p2 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 0, 1));
	#else
		float3 p1 = mul(unity_CameraToWorld, float4(0, 0, 1, 1));
		float3 p2 = mul(unity_CameraToWorld, float4(0, 0, 0, 1));
	#endif
	return normalize(p2 - p1);
}

float2x2 GetRotationMatrix(float axis){
	float c, s, ang;
	ang = (axis+90) * (UNITY_PI/180.0);
	sincos(ang, c, s);
	float2x2 mat = float2x2(c,-s,s,c);
	mat = ((mat*0.5)+0.5)*2-1;
	return mat;
}

float2 Rotate2D(float2 coords, float rot){
	rot *= (UNITY_PI/180.0);
	float sinVal = sin(rot);
	float cosX = cos(rot);
	float2x2 mat = float2x2(cosX, -sinVal, sinVal, cosX);
	mat = ((mat*0.5)+0.5)*2-1;
	return mul(coords, mat);
}

float3 Rotate(float3 coords, float3 axis){
	coords.xy = mul(GetRotationMatrix(axis.x), coords.xy);
	coords.xz = mul(GetRotationMatrix(axis.y), coords.xz);
	coords.yz = mul(GetRotationMatrix(axis.z), coords.yz);
    return coords;
}

float3 GetNoiseRGB(float2 p, float str) {
    float3 rgb = 0;
	float3 p3 = frac(float3(p.xyx) * (float3(443.8975, 397.2973, 491.1871)+frac(_Time.y)));
	p3 += dot(p3, p3.yxz + 19.19);
	rgb = frac(float3((p3.x + p3.y)*p3.z, (p3.x + p3.z)*p3.y, (p3.y + p3.z)*p3.x));
	rgb = (rgb-0.5)*2*str;
	return rgb;
}

float GetNoise(float2 p){
	float n = frac(sin(p.x*100+p.y*6574)*5647);
	n = linearstep(-1,1,n);
	return n;
}

float GetNoise(float2 p, float str){
	float n = frac(sin(p.x*100+p.y*6574)*5647);
	n = linearstep(-1,1,n);
	return n*str;
}

float3 FlowUV (float2 uv, float time, float phase) {
	float progress = frac(time + phase);
	float waveform = 1-abs(1-2 * progress);
	uv += (time - progress) * float2(0.24, 0.2083333);
	float3 uvw = float3(uv, waveform);
	return uvw;
}

float Dither8x8Bayer(int x, int y){
    const float dither[ 64 ] = {
		1, 49, 13, 61,  4, 52, 16, 64,
		33, 17, 45, 29, 36, 20, 48, 32,
		9, 57,  5, 53, 12, 60,  8, 56,
		41, 25, 37, 21, 44, 28, 40, 24,
		3, 51, 15, 63,  2, 50, 14, 62,
		35, 19, 47, 31, 34, 18, 46, 30,
		11, 59,  7, 55, 10, 58,  6, 54,
		43, 27, 39, 23, 42, 26, 38, 22
	};
    return dither[y * 8 + x] / 64;
}

float Dither(float2 pos, float alpha) {
	pos *= _ScreenParams.xy;
	return alpha - Dither8x8Bayer(fmod(pos.x, 8), fmod(pos.y, 8));
}

void ApplyPBRFiltering(inout float value, float contrast, float intensity, float lightness, int shouldApply, inout float previewValue){
	if (shouldApply == 1){
		value = saturate(lerp(0.5, value, contrast));
		value += saturate(value * intensity);
		value = saturate(value + lightness);
		previewValue = value;
	}
}

void ApplyPBRFiltering(inout float3 value, float contrast, float intensity, float lightness, int shouldApply, inout float3 previewValue){
	if (shouldApply == 1){
		value = saturate(lerp(0.5, value, contrast));
		value += saturate(value * intensity);
		value = saturate(value + lightness);
		previewValue = value;
	}
}