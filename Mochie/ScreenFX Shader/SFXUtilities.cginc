// ---------------------------
// Basic Utilities
// ---------------------------
float smootherstep(float edge0, float edge1, float x){
  x = saturate((x - edge0) / (edge1 - edge0));
  return x * x * x * (x * (x * 6 - 15) + 10);
}

float expstep( float x, float k, float n ){
    return exp( -k*pow(x,n) );
}

float smoothlerp(float x, float y, float z){
    return lerp(x, y, smoothstep(0, 1, z));
}

float linearstep(float j, float k, float x) {
	x = clamp((x - j) / (k - j), 0.0, 1.0); 
	return x;
}

float lerp3(float4 x, float4 y, float4 z, float w){
	if (w <= 1)
		return lerp(x, y, w);
	else
		return lerp(y, z, w*0.5);
}

float Average(float3 xyz){
    return saturate((xyz.x+xyz.y+xyz.z)/3.0);
}

bool IsInMirror(){
    return unity_CameraProjection[2][0] != 0.0f || unity_CameraProjection[2][1] != 0.0f;
}

void MirrorCheck(){
    if (IsInMirror()) discard;
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

float4 GetVertexPos(float4 vertex){
	float4 wPos = mul(unity_CameraToWorld, vertex);
	float4 oPos = mul(unity_WorldToObject, wPos);
	return UnityObjectToClipPos(oPos);
}

float GetFalloff(int ug, float gf, float minR, float maxR, float d){
    UNITY_BRANCH
    if (ug == 0)
        return smoothstep(maxR, clamp(minR, 0, maxR-0.001), d);
    else
        return gf;
}

float3 GetNoiseRGB(float2 p, float strength){
    float3 p3 = frac(float3(p.xyx) * (float3(443.8975, 397.2973, 491.1871)+_Time.y));
    p3 += dot(p3, p3.yxz + 19.19);
    float3 rgb = frac(float3((p3.x + p3.y)*p3.z, (p3.x + p3.z)*p3.y, (p3.y + p3.z)*p3.x));
	rgb = (rgb-0.5)*2*strength;
	return rgb;
}

float GetNoise(float2 p, float str){
	float n = saturate(frac(sin(p.x*100+p.y*6574)*5647));
	n = linearstep(-1,1,n)*str;
	return n;
}

// ---------------------------
// Depth stuff
// ---------------------------
float2 AlignWithGrabTexel(float2 uv){
	return (floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) * abs(_CameraDepthTexture_TexelSize.xy);
}

void GetDepth(v2f i, out float3 wPos, out float3 wNorm, out float depth){
    depth = Linear01Depth(DecodeFloatRG(tex2Dproj(_CameraDepthTexture, i.uv)));
    float4 vPos = float4(i.raycast * depth, 1);
    wPos = mul(unity_CameraToWorld, vPos).xyz;
    #if defined(FULL_PASS) && defined(SFXX)
        UNITY_BRANCH
        if (_DistortionModel == 2 || _OutlineType > 0)
            wNorm = normalize(cross(ddy_fine(wPos), ddx_fine(wPos)));
        else
            wNorm = 0;
    #elif defined(FULL_PASS) && !defined(SFXX)
	    UNITY_BRANCH
        if (_DistortionModel == 2)
            wNorm = normalize(cross(ddy_fine(wPos), ddx_fine(wPos)));
        else
            wNorm = 0;
	#else
        wNorm = normalize(cross(ddy_fine(wPos), ddx_fine(wPos)));
    #endif
}

float GetRadius(v2f i, float3 pos, float range, float falloff){
    GetDepth(i, wPos, wNorm, depth);
    float dist = distance(wPos, pos);
    #if defined(FULL_PASS)
        dist = smoothstep(range, range - falloff, dist);
    #else
        UNITY_BRANCH
        switch(_Triplanar){
            case 1: 
                dist = smoothstep(range, range - falloff, dist);
                break;
            case 2:
                _TPScanFade = lerp(_TPThickness-0.001, -3, _TPScanFade);
                dist = smoothstep(range+_TPThickness, range+_TPScanFade, dist) * smoothstep(range-_TPThickness, range-_TPScanFade, dist);
                break;
            default: break;
        }
    #endif
    return dist;
}

float4 GetTriplanar(v2f i, sampler2D tex, sampler2D nTex, float2 _ST0, float2 _ST1, float radius) {
    #if defined(FULL_PASS)
        wPos += _Time.y*_DistortionSpeed;
    #else
        _ST0 *= 0.5;
    #endif
    wNorm = abs(wNorm);
    wNorm /= (wNorm.x + wNorm.y + wNorm.z);
    #if defined(FULL_PASS)
        float2 uvX = wPos.yz * _ST0;
        float2 uvY = wPos.xz * _ST0;
        float2 uvZ = wPos.xy * _ST0;
        float4 diffX = float4(UnpackNormal(tex2D(tex, uvX)), 1);
        float4 diffY = float4(UnpackNormal(tex2D(tex, uvY)), 1);
        float4 diffZ = float4(UnpackNormal(tex2D(tex, uvZ)), 1);
        return float4((diffX * wNorm.x) + (diffY * wNorm.y) + (diffZ * wNorm.z));
    #else
        float3 scrollDiff = _TPScroll*_Time.y;
        float3 scrollNoise = _TPNoiseScroll*_Time.y;
        float2 uvX0 = (wPos.yz*_ST0) - scrollDiff.yz;
        float2 uvY0 = (wPos.xz*_ST0) - scrollDiff.xz;
        float2 uvZ0 = (wPos.xy*_ST0) - scrollDiff.xy;
        float2 uvX1 = (wPos.yz*_ST1) - scrollNoise.yz;
        float2 uvY1 = (wPos.xz*_ST1) - scrollNoise.xz;
        float2 uvZ1 = (wPos.xy*_ST1) - scrollNoise.xy;
        float4 diffX = tex2D(tex, uvX0);
        float4 diffY = tex2D(tex, uvY0);
        float4 diffZ = tex2D(tex, uvZ0);
        float noiseX = tex2D(nTex, uvX1);
        float noiseY = tex2D(nTex, uvY1);
        float noiseZ = tex2D(nTex, uvZ1);
        float4 col = float4((diffX * wNorm.x) + (diffY * wNorm.y) + (diffZ * wNorm.z));
        float noise = (noiseX * wNorm.x) + (noiseY * wNorm.y) + (noiseZ * wNorm.z);
        UNITY_BRANCH
        if (_Triplanar == 2)
            radius = saturate(radius-GetNoise(i.uv.xy, _TPNoise));
        col.a *= radius*i.globalF*i.pulseSpeed;
        col.a *= lerp(noise, 1, col.a);
        return col;
    #endif
}

// ---------------------------
// Color filtering stuff
// ---------------------------
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

float3 GetInversion(float3 col){
    col.rgb = abs(_Invert - col.rgb);
    col.r = abs(_InvertR - col.r);
    col.g = abs(_InvertG - col.g);
    col.b = abs(_InvertB - col.b);
    return col;
}

float3 GetContrast(float3 col){
    return clamp((lerp(float3(0.5,0.5,0.5), col, _Contrast)), 0, 10);
}

float3 GetSaturation(float3 col, float interpolator){
    return lerp(dot(col, float3(0.3,0.59,0.11)), col, interpolator);
}

float3 GetGrayscale(float3 col){
    return dot(col, float3(0.3,0.59,0.11));
}

float oetf_sRGB_scalar(float L) {
	float V = 1.055 * (pow(L, 1.0 / 2.4)) - 0.055;
	if (L <= 0.0031308)
		V = L * 12.92;
	return V;
}

float eotf_sRGB_scalar(float V) {
	float L = pow((V + 0.055) / 1.055, 2.4);
	if (V <= oetf_sRGB_scalar(0.0031308))
		L = V / 12.92;
	return L;
}

// Accurate
float3 GetHDR(float3 rgb) {
	return float3(eotf_sRGB_scalar(rgb.r), eotf_sRGB_scalar(rgb.g), eotf_sRGB_scalar(rgb.b));
}

// Approximated
// float3 GetHDR(float3 rgb){
// 	float3 gammaCol = rgb * (rgb * (rgb * 0.305306011h + 0.682171111h) + 0.012522878h);
// 	return lerp(rgb, gammaCol, _HDR);
// }

// ---------------------------
// Shake stuff
// ---------------------------
float2 GetShakeTime(){
    float2 shakeTime;
    UNITY_BRANCH
    if (_ShakeModel != 3){
        _ShakeSpeedX *= 128;
        _ShakeSpeedY *= 128;
        shakeTime.x = sin(_Time.y * _ShakeSpeedX);
        shakeTime.y = sin(_Time.y * _ShakeSpeedY);
        UNITY_BRANCH
        if (_ShakeModel == 2)
            shakeTime = round(shakeTime);
    }
    else {
        shakeTime = _Time.x * _ShakeSpeedXY;
    }
    return shakeTime;
}

// ---------------------------
// Distortion stuff
// ---------------------------
float3 GetDistortionUV(float3 uv0, float scalar){
    float3 scaledUV = uv0/scalar;
    float3 distortionUV = float3(scaledUV.xy, scaledUV.z+_Time.y*_DistortionSpeed);
    return distortionUV;
}

float3 GetDistortionOffset(float3 uv0, float value){
    float3 offset = value * (_DistortionStr * 2731 * _MSFXGrab_TexelSize.xyz);
    offset = (offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(uv0.z)) + uv0;
    return offset;
}

// ---------------------------
// Blur stuff
// ---------------------------
float GaussianOpacity(float x) { 
	return 0.1*exp(-0.5*x*x / 121.0); 
}

float Brightness(float3 c) { 
	return max(max(c.r, c.g), c.b); 
}

float3 EncodeHDR(float3 rgb) {
	rgb *= 1.0 / 8;
	float m = max(max(rgb.r, rgb.g), max(rgb.b, 1e-6));
	m = ceil(m * 255) / 255;
	return rgb/m;
}

float3 SafeHDR(float3 c) { 
	return min(c, 65000); 
}

float GetDoF(v2f i, float strength){
    UNITY_BRANCH
    if (_DoF == 1){
        float3 focusPos = lerp(i.cameraPos, i.objPos, _DoFP2O);
        float combined = saturate(GetRadius(i, focusPos, _DoFRadius, _DoFFade));
        strength = lerp(strength, 0, combined);
    }
    return strength;
}

float3 GetRadialBlur(v2f i){
    float3 col = 0;
    float2 distance = i.uv.xy/i.uv.w;
    float2 offset = 0.5;
    #if UNITY_SINGLE_PASS_STEREO
        if (unity_StereoEyeIndex == 0)
            offset.x = 0.25;
        if (unity_StereoEyeIndex == 1)
            offset.x = 0.75; 
    #endif
    distance -= offset;
    _BlurStr *= 0.4*i.blurF;
	[unroll(20)]
    for (int j = 0; j < _BlurSamples; j++) {
        float scale = 1-_BlurStr*(j/(float)_BlurSamples)*(length(distance)/_BlurRadius);
        float2 uv = (distance*scale)+offset;
        float3 rgb = tex2Dlod(_MSFXGrab, float4(uv,0,0));
        col += rgb;
    }
    return col/_BlurSamples;
}

float3 BlurX(float2 uv0, float weight, float phase){
	float offset = _MSFXGrab_TexelSize.x * phase * _BlurStr;
	float4 uv1 = float4(uv0.x+offset, uv0.y, 0,0);
	return tex2Dlod(_MSFXGrab, uv1).rgb * weight;
}

float3 BlurY(float2 uv0, float weight, float phase){
	float offset = _MSFXGrab_TexelSize.y * phase * _BlurStr;
	float4 uv1 = float4(uv0.x, uv0.y+offset, 0,0);
	return tex2Dlod(_MSFXGrab, uv1).rgb * weight;
}

float3 BlurXX(float2 uv0, float weight, float phase){
	float offset = _MSFXGrab_TexelSize.x * phase * _BlurStr;
	float4 uv1 = float4(uv0.x+offset, uv0.y, 0,0);
	float4 uv2 = float4(uv0.x-offset, uv0.y, 0,0);
	float3 col1 = tex2Dlod(_MSFXGrab, uv1).rgb;
	float3 col2 = tex2Dlod(_MSFXGrab, uv2).rgb;
	return (col1 + col2) * weight;
}

float3 BlurYY(float2 uv0, float weight, float phase){
	float offset = _MSFXGrab_TexelSize.y * phase * _BlurStr;
	float4 uv1 = float4(uv0.x, uv0.y+offset, 0,0);
	float4 uv2 = float4(uv0.x, uv0.y-offset, 0,0);
	float3 col1 = tex2Dlod(_MSFXGrab, uv1).rgb;
	float3 col2 = tex2Dlod(_MSFXGrab, uv2).rgb;
	return (col1 + col2) * weight;
}

float3 BlurXXYY(float2 uv0, float weight, float phase){
	float2 offset = _MSFXGrab_TexelSize.xy * (phase*0.75) * _BlurStr;
	float4 uv1 = float4(uv0+offset, 0,0);
	float4 uv2 = float4(uv0+float2(-offset.x, offset.y), 0,0);
	float4 uv3 = float4(uv0+float2(offset.x, -offset.y), 0,0);
	float4 uv4 = float4(uv0-offset, 0,0);
	float3 col1 = tex2Dlod(_MSFXGrab, uv1).rgb;
	float3 col2 = tex2Dlod(_MSFXGrab, uv2).rgb;
	float3 col3 = tex2Dlod(_MSFXGrab, uv3).rgb;
	float3 col4 = tex2Dlod(_MSFXGrab, uv4).rgb;
	return (col1 + col2 + col3 + col4) * weight;
}

float3 EqualBlur(float2 uv, float weight, float phase){
	float3 x = BlurXX(uv, weight, phase);
	float3 y = BlurYY(uv, weight, phase);
	float3 xy = BlurXXYY(uv, weight, phase);
	return (x + y + xy)/divisor[_BlurSamples-2];
}

float3 YBlur(float2 uv, float weight, float phase){
	return BlurYY(uv, weight, phase);
}

float3 RGBBlur(float2 uv, float weight, float phase){
	float r = BlurX(uv, weight, phase).r;
	float g0 = BlurY(uv, weight, -phase).g*0.5;
	float g1 = BlurY(uv, weight, phase).g*0.5;
	float b = BlurX(uv, weight, -phase).b;
	return float3(r,g0+g1,b)*2.0;
}

float3 GetPixelBlurColor(float4 uv0){
	float2 uv = uv0.xy/uv0.w;
    float3 blurCol = 0;

	float phase = 0;
	float weight = 1;
	float incPhase = 5.0/_BlurSamples;
	float incWeight = 1.0/_BlurSamples;
	
	// Regular
	UNITY_BRANCH
	if (_RGBSplit == 0){
		UNITY_BRANCH // All Axis
		if (_BlurY == 0){
			[unroll(20)]
			for (int i = 0; i < _BlurSamples; i++){
				phase += incPhase;
				weight -= incWeight;
				blurCol += EqualBlur(uv, weight, phase);
			}
		}
		else { // Y Only
			blurCol = tex2D(_MSFXGrab, uv).rgb;
			[unroll(20)]
			for (int i = 0; i < _BlurSamples; i++){
				phase += incPhase;
				weight -= incWeight;
				blurCol += YBlur(uv, weight, phase);
			}
		}
	}
	else { // RGB Split
		blurCol = tex2D(_MSFXGrab, uv).rgb;
		[unroll(20)]
		for (int i = 0; i < _BlurSamples; i++){
			phase += incPhase;
			weight -= incWeight;
			blurCol += RGBBlur(uv, weight, phase);
		}
	}
	return blurCol/_BlurSamples;
}

float3 Crush(float3 col){
	col.rgb = GammaToLinearSpace(col);
	float br = Brightness(col.rgb);
	col *= max(0, br-1) / max(br, 0.00001);
	col = EncodeHDR(col);
	return col;
}
float3 EqualBlurCrush(float2 uv, float weight, float phase){
	float3 x = BlurXX(uv, weight, phase);
	float3 y = BlurYY(uv, weight, phase);
	float3 xy = BlurXXYY(uv, weight, phase);
	float3 col = (x + y + xy)/divisor[_BlurSamples-2];
	return Crush(col);
}

float3 YBlurCrush(float2 uv, float weight, float phase){
	float3 col = BlurYY(uv, weight, phase);
	return Crush(col);
}

float3 RGBBlurCrush(float2 uv, float weight, float phase){
	float r = BlurX(uv, weight, phase).r;
	float g0 = BlurY(uv, weight, -phase).g*0.5;
	float g1 = BlurY(uv, weight, phase).g*0.5;
	float b = BlurX(uv, weight, -phase).b;
	float3 col = float3(r,g0+g1,b)*2.0;
	return Crush(col);
}

float3 GetCrushBlurColor(float4 uv0){
	float2 uv = uv0.xy/uv0.w;
    float3 blurCol = 0;

	float phase = 0;
	float weight = 1;
	float incPhase = 5.0/_BlurSamples;
	float incWeight = 1.0/_BlurSamples;
	
	// Regular
	UNITY_BRANCH
	if (_RGBSplit == 0){
		UNITY_BRANCH // All Axis
		if (_BlurY == 0){
			[unroll(20)]
			for (int i = 0; i < _BlurSamples; i++){
				phase += incPhase;
				weight -= incWeight;
				blurCol += EqualBlurCrush(uv, weight, phase);
			}
		}
		else { // Y Only
			[unroll(20)]
			for (int i = 0; i < _BlurSamples; i++){
				phase += incPhase;
				weight -= incWeight;
				blurCol += YBlurCrush(uv, weight, phase);
			}
		}
	}
	else { // RGB Split
		[unroll(20)]
		for (int i = 0; i < _BlurSamples; i++){
			phase += incPhase;
			weight -= incWeight;
			blurCol += RGBBlurCrush(uv, weight, phase);
		}
	}
	return blurCol/_BlurSamples*2;
}



// bool FrameClip(float2 uv0){
//     if (uv0.x > _SSTFrameSizeXP || uv0.y > _SSTFrameSizeYP || uv0.x < _SSTFrameSizeXN || uv0.y < _SSTFrameSizeYN)
//         return true;
//     else
//         return false;
// }