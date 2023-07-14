#ifndef UTILITIES_INCLUDED
#define UTILITIES_INCLUDED

float2 Rotate2D(float2 coords, float rot){
	rot *= (UNITY_PI/180.0);
	float sinVal = sin(rot);
	float cosX = cos(rot);
	float2x2 mat = float2x2(cosX, -sinVal, sinVal, cosX);
	mat = ((mat*0.5)+0.5)*2-1;
	return mul(coords, mat);
}

float2 ScaleOffsetScrollUV(float2 uv, float2 scale, float2 offset, float2 scroll){
	return (uv + offset + (scroll * _Time.y * 0.1)) * scale;
}

float2 ScaleOffsetUV(float2 uv, float2 scale, float2 offset){
	return uv * scale + offset;
}

float2 ScaleOffsetRotateUV(float2 uv, float2 scale, float2 offset, float rot){
	uv -= offset + 0.5;
	uv = Rotate2D(uv, rot) + 0.5;
	uv = (uv - 0.5) / scale + 0.5;
    return uv;
}

bool IsInMirror(){
	return unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f;
}

void MirrorCheck(){
    if (IsInMirror()) discard;
}

bool IsLeftEye(){
    return UNITY_MATRIX_P._13 < 0;
}

bool IsRightEye(){
    return UNITY_MATRIX_P._13 > 0;
}

bool IsNotVR(){
    return UNITY_MATRIX_P._13 == 0;
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

#ifndef SHADER_API_METAL
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
#endif

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

float RemapPositive(float x){
	return (x + 1) * 0.5;
}

float2 RemapPositive(float2 x){
	return (x + 1) * 0.5;
}

float3 RemapPositive(float3 x){
	return (x + 1) * 0.5;
}

float4 RemapPositive(float4 x){
	return (x + 1) * 0.5;
}

float RoundTo(float x, float y){
	return ceil(x*y)/y;
}

float SmoothFalloff(float minR, float maxR, float d){
	return smoothstep(maxR, clamp(minR, 0, maxR-0.001), d);
}

float GetFalloff(int ug, float gf, float minR, float maxR, float d){
    return lerp(smoothstep(maxR, clamp(minR, 0, maxR-0.001), d), gf, ug);
}

float Safe_DotClamped(float3 a, float3 b){
	return max(0.00001, dot(a,b));
}

float2 ApplyScaleOffset(float2 uv, float4 scaleOffset){
	return uv.xy * scaleOffset.xy + scaleOffset.zw;
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
	float depth = LinearEyeDepth(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV)) / screenPos.w;
	float3 worldSpacePos = worldDir * depth + _WorldSpaceCameraPos;
	return worldSpacePos;
}

float3 GetWorldSpacePixelPosSP(float4 vertex, float2 scrnPos){
	float4 worldPos = mul(unity_ObjectToWorld, float4(vertex.xyz, 1));
	float4 screenPos = mul(UNITY_MATRIX_VP, worldPos); 
	worldPos = mul(inverse(UNITY_MATRIX_VP), screenPos);
	float3 worldDir = worldPos.xyz - _WorldSpaceCameraPos;
	float depth = LinearEyeDepth(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, scrnPos)) / screenPos.w;
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

	#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)           
		float ipd = length(mul(unity_WorldToObject, 
							float4(unity_StereoWorldSpaceCameraPos[0].xyz - unity_StereoWorldSpaceCameraPos[1].xyz, 0)));
		float4 absPos = vertex + float4(ipd*(0.5-unity_StereoEyeIndex), 0, 0, 0);
	#else
		float ipd = 0.0;
		float4 absPos = vertex;
	#endif
	float4 wPos = mul(unity_CameraToWorld, absPos);
	float4 oPos = mul(unity_WorldToObject, wPos);
	return UnityObjectToClipPos(oPos);
}

float3 GetObjPos(){
    return mul(unity_ObjectToWorld, float4(0,0,0,1));
}

float3 GetCameraPos(){
    float3 cameraPos = _WorldSpaceCameraPos;
    #if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
        cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5;
    #endif
    return cameraPos;
}

float3 GetForwardVector(){
	#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
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

float2 GetPolarCoords(float2 uv, float2 center, float radialScale, float lengthScale) {
    float2 delta = uv - center;
    float radius = length(delta) * 2 * radialScale;
    float angle = atan2(delta.x, delta.y) * 1.0/6.28 * lengthScale;
    return float2(radius, angle);
}

float3 Rotate3D(float3 coords, float3 axis){
	coords.xy = mul(GetRotationMatrix(axis.x), coords.xy);
	coords.xz = mul(GetRotationMatrix(axis.y), coords.xz);
	coords.yz = mul(GetRotationMatrix(axis.z), coords.yz);
    return coords;
}

float3 GetNoiseRGB(float2 p, float3 str) {
    float3 rgb = 0;
	float3 p3 = frac(float3(p.xyx) * (float3(443.8975, 397.2973, 491.1871)+frac(_Time.y)));
	p3 += dot(p3, p3.yxz + 19.19);
	rgb = frac(float3((p3.x + p3.y)*p3.z, (p3.x + p3.z)*p3.y, (p3.y + p3.z)*p3.x));
	rgb = (rgb-0.5)*2*str;
	return rgb;
}

float3 GetNoiseRGB(float2 p) {
    float3 rgb = 0;
	float3 p3 = frac(float3(p.xyx) * (float3(443.8975, 397.2973, 491.1871)+frac(_Time.y)));
	p3 += dot(p3, p3.yxz + 19.19);
	rgb = frac(float3((p3.x + p3.y)*p3.z, (p3.x + p3.z)*p3.y, (p3.y + p3.z)*p3.x));
	rgb = (rgb-0.5)*2;
	return rgb;
}

float GetNoise(float2 p){
	float n = frac(sin(p.x*100+p.y*6574)*5647);
	n = linearstep(-1, 1, n);
	return n;
}

float GetNoise(float2 p, float str){
	float n = frac(sin(p.x*100+p.y*6574)*5647);
	n = linearstep(-1, 1, n)*str;
	return n;
}

float GetNoiseSFX(float2 p, float str){
	float n = GetNoiseRGB(p, str);
	return n;
}

float GetScanNoise(float2 p, float str, float thickness, float speed){
	float2 n = frac(p.yx * float2(443.8975*thickness*smoothstep(-100, 1, GetNoise(p)), 397.2973) + frac(_Time.y*speed));
	float noise = (n + n) * 0.5;
	noise = (noise-0.5)*2*str;
	return noise;
}

float3 FlowUV (float2 uv, float time, float phase) {
	float progress = frac(time + phase);
	float waveform = 1-abs(1-2 * progress);
	uv += (time - progress) * float2(0.24, 0.2083333);
	float3 uvw = float3(uv, waveform);
	return uvw;
}

float2 GetFlipbookUV(float2 uv, float columns, float rows, float speed){
	float2 size = float2(1/columns, 1/rows);
	uint totalFrames = columns * rows;
	uint index = _Time.y * speed;
	uint indexX = index % columns;
	uint indexY = floor((index % totalFrames) / columns);
	float2 offset = float2(size.x*indexX,-size.y*indexY);
	float2 flipbookUV = frac(uv)*size;
	flipbookUV.y = flipbookUV.y + size.y*(rows - 1);
	flipbookUV += offset;
	return flipbookUV;
}

float GetRimValue(float3 viewDir, float3 normal, float rimWidth, float rimEdge){
	float VdotL = abs(dot(viewDir, normal));
	float rim = pow((1-VdotL), (1-rimWidth) * 10);
	rim = smoothstep(rimEdge, 1-rimEdge, rim);
	return rim;
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

void ApplyVertRounding(inout float4 worldPos, inout float4 localPos, float precision, float strength, float mask){
   	worldPos = lerp(worldPos, ceil(worldPos*precision)/precision, strength * mask);
	localPos = mul(unity_WorldToObject, worldPos);
}

float2 GetGrabPos(float4 grabPos){
	#if UNITY_UV_STARTS_AT_TOP
		float scale = -1.0;
	#else
		float scale = 1.0;
	#endif
	float halfPosW = grabPos.w * 0.5;
	grabPos.y = (grabPos.y - halfPosW) * _ProjectionParams.x * scale + halfPosW;
	#if SHADER_API_D3D9 || SHADER_API_D3D11
		grabPos.w += 0.00000000001;
	#endif
	return(grabPos / grabPos.w).xy;
}

float GetWave(int wave, float speed, float amp){
	float pulse = 1;
	switch (wave){
		case 0: break;													// None
		case 1: pulse = 0.5*(sin(_Time.y * speed)+1); break; 			// Sin
		case 2: pulse = round((sin(_Time.y * speed)+1)*0.5); break;		// Square
		case 3: pulse = abs((_Time.y * (speed * 0.333)%2)-1); break;	// Triangle
		case 4: pulse = frac(_Time.y * (speed * 0.2)); break; 			// Saw
		case 5: pulse = 1-frac(_Time.y * (speed * 0.2)); break; 		// Reverse Saw
		default: break;
	}
	return max(0, lerp(1, pulse, amp));
}

float ChannelCheck(float4 rgba, int channel){
	return rgba[channel];
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
float3 Get1RippleNormal(float2 uv, float2 center, float time, float scale, float strength){
	float2 ray = normalize(uv - center);
	float x = scale * length(uv - center) / (0.5*UNITY_PI);
	float dx = 0.01;
	float x1 = x + dx;
	float fx = min(x, 10);
	float falloff = 0.5 * cos(UNITY_PI * fx / 10.0) + 0.5;

	x = clamp(x - time,-1.57079632679, 1.57079632679);
	x1 = clamp(x1 - time,-1.57079632679, 1.57079632679);
	float ripple = falloff*cos(5.0 * x) * cos(x);
	float ripple1 = falloff*cos(5.0 * x1) * cos(x1);
	
	float dy = ripple1 - ripple;
	float3 normal = float3(ray*(-dy), dx/strength);
	//normal = 0.5 + 0.5 * normal;
	normal = normalize(normal);
	return normal;
}

float2 N22(float2 p){
	float3 a = frac(p.xyx * float3(123.34, 234.34, 345.65));
	a += dot(a, a + 34.34);
	return frac(float2(a.x * a.y, a.y * a.z));
}

float2 N11(float2 p){
	float3 a = frac(p.xxx * float3(123.34, 234.34, 345.65));
	a += dot(a, a + 34.34);
	return frac((a.x * a.y * a.z));
}

float3 GetRipplesNormal(float2 uv, float scale, float strength, float speed){
	float3 normals = float3(0,0,1);
	if (strength != 0){
		float2 uv_scaled = uv * scale;
		float2 cell0 = floor(uv_scaled);
		[unroll]
		for (int y = -1; y <= 1; y++) {
			[unroll]
			for (int x = -1; x <= 1; x++){
				float2 cellx = cell0 + float2(x, y);
				float noise = N11(0.713323*cellx.x + 5.139274*cellx.y);
				float2 center = cellx + N22(cellx);
				normals = Get1RippleNormal(uv_scaled, center, 13 * frac(speed * (_Time[0] + 2*noise)) - 4.0, 10.0, strength) + normals;
			}
		}
	}
	return normalize(normals);
}

            // v2f vert (appdata v)
            // {
            //     v2f o;
            //     float4 vert = mul(unity_ObjectToWorld, v.vertex);
            //     float3 objX = mul(unity_ObjectToWorld, float4(1, 0, 0, 0)).xyz;
            //     float3 localZ = normalize(cross(objX, float3(0, 1, 0)));
            //     float3 localX = normalize(cross(float3(0, 1, 0), localZ));

            //     vert.xyz += localX + localZ;
            //     o.vertex = mul(UNITY_MATRIX_VP, vert);
            //     o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            //     UNITY_TRANSFER_FOG(o,o.vertex);
            //     return o;
            // }

#endif