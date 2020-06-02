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

float linearstep(float j, float k, float x) {
	x = clamp((x - j) / (k - j), 0.0, 1.0); 
	return x;
}

float2 linearstep(float2 j, float2 k, float2 x) {
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

float GetRoughness(float smoothness){
	float roughness = 1-smoothness;
    roughness *= 1.7-0.7*roughness;
    return roughness;
}