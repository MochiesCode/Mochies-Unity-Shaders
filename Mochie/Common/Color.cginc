#ifndef COLOR_INCLUDED
#define COLOR_INCLUDED

#ifdef UNITY_COLORSPACE_GAMMA
	#define mochie_ColorSpaceDouble float3(2.0, 2.0, 2.0)
#else
	#define mochie_ColorSpaceDouble float3(4.59479380, 4.59479380, 4.59479380)
#endif

// ---------------------------
// Color Model Conversions
// ---------------------------
const static float EPS = 1e-10;
float3 RGBtoHCV(in float3 rgb) {
    float4 P = lerp(float4(rgb.bg, -1.0, 2.0/3.0), float4(rgb.gb, 0.0, -1.0/3.0), step(rgb.b, rgb.g));
    float4 Q = lerp(float4(P.xyw, rgb.r), float4(rgb.r, P.yzx), step(P.x, rgb.r));
    float C = Q.x - min(Q.w, Q.y);
    float H = abs((Q.w - Q.y) / (6.0 * C + EPS) + Q.z);
    return float3(H, C, Q.x);
}

float3 RGBtoHSL(in float3 rgb) {
    float3 HCV = RGBtoHCV(rgb);
    float L = HCV.z - HCV.y * 0.5;
    float S = HCV.y / (1.0 - abs(L * 2.0 - 1.0) + EPS);
    return float3(HCV.x, S, L);
}

float3 HSLtoRGB(float3 c) {
    c = float3(frac(c.x), clamp(c.yz, 0.0, 1.0));
    float3 rgb = clamp(abs(fmod(c.x * 6.0 + float3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return c.z + c.y * (rgb - 0.5) * (1.0 - abs(2.0 * c.z - 1.0));
}

float3 RGBtoHSV(float3 c){
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
	float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 HSVtoRGB(float3 c){
	float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float3 HSVShift(float3 col, float hue, float saturation, float value){
	float3 hsv = float3(hue, saturation, value);
    float3 hsvCol = RGBtoHSV(col);
    return HSVtoRGB(hsvCol + hsv);
}

// sRGB luminance(Y) values
const float rY = 0.212655;
const float gY = 0.715158;
const float bY = 0.072187;

// Inverse of sRGB "gamma" function. (approx 2.2)
float inv_gam_sRGB(int ic) {
    float c = ic/255.0;
    if ( c <= 0.04045 )
        return c/12.92;
    else 
        return pow(((c+0.055)/(1.055)),2.4);
}

// sRGB "gamma" function (approx 2.2)
int gam_sRGB(float v) {
    if(v<=0.0031308)
        v *= 12.92;
    else 
        v = 1.055*pow(v,1.0/2.4)-0.055;
    return int(v*255+0.5);
}

// GRAY VALUE ("brightness")
int GetLum(int r, int g, int b) {
    return gam_sRGB(
            rY*inv_gam_sRGB(r) +
            gY*inv_gam_sRGB(g) +
            bY*inv_gam_sRGB(b)
    );
}

float3 WhitePointAdjust(float3 XYZ, float3 from, float3 to){
	float3x3 Bradford=float3x3(0.8951,0.2664,-0.1614,
	-0.7502,1.7135,0.0367,
	0.0389,-0.0685,1.0296);

	float3x3 BradfordInv=float3x3(0.9869929,-0.1470543,0.1599627,
	0.4323053,0.5183603,0.0492912,
	-0.0085287,0.0400428,0.9684867);


	float3 BradFrom= mul(Bradford,from);
	float3 BradTo= mul(Bradford,to);

	float3x3 CR=float3x3(BradTo.x/BradFrom.x,0,0,
	0,BradTo.y/BradFrom.y,0,
	0,0,BradTo.z/BradFrom.z);

	float3x3 convBrad= mul(mul(BradfordInv,CR),Bradford);

	float3 outp=mul(convBrad,XYZ);
	return outp;
}

// Approximated ACES
// Based on https://64.github.io/tonemapping/#aces
float3 ACES(float3 v){
    v *= 0.6f;
    float a = 2.51f;
    float b = 0.03f;
    float c = 2.43f;
    float d = 0.59f;
    float e = 0.14f;
    return (v*(a*v+b))/(v*(c*v+d)+e);
}

float3 ACES_Clamped(float3 v){
    v *= 0.6f;
    float a = 2.51f;
    float b = 0.03f;
    float c = 2.43f;
    float d = 0.59f;
    float e = 0.14f;
    return saturate((v*(a*v+b))/(v*(c*v+d)+e));
}

// ---------------------------
// Photoshop Blending Modes
// ---------------------------

// Single channel
float BlendPinLight(float s, float d){ return (2.0*s - 1.0 > d) ? 2.0*s - 1.0 : (s < 0.5 * d) ? 2.0*s : d; }
float BlendVividLight(float s, float d){ return (s < 0.5) ? 1.0 - (1.0 - d) / (2.0 * s) : d / (2.0 * (1.0 - s)); }
float BlendHardLight(float s, float d){ return (s < 0.5) ? 2.0*s*d : 1.0 - 2.0*(1.0 - s)*(1.0 - d); }
float BlendOverlay(float s, float d){ return (d < 0.5) ? 2.0*s*d : 1.0 - 2.0*(1.0 - s)*(1.0 - d); }
float BlendScreen(float s, float d){ return s + d - s * d; }
float BlendLerp(float s, float d){ return lerp(s, d, 0.5); }
float BlendSoftLight(float s, float d){
    return (s < 0.5) ? d - (1.0 - 2.0*s)*d*(1.0 - d) 
                : (d < 0.25) ? d + (2.0*s - 1.0)*d*((16.0*d - 12.0)*d + 3.0) 
                : d + (2.0*s - 1.0) * (sqrt(d) - d);
}

// RGB
float3 BlendColorBurn(float3 s, float3 d){ return 1.0 - (1.0 - d) / s; }
float3 BlendLinearBurn(float3 s, float3 d ){ return s + d - 1.0; }
float3 BlendDarkerColor(float3 s, float3 d){ return (s.x + s.y + s.z < d.x + d.y + d.z) ? s : d; }
float3 BlendScreen(float3 s, float3 d){ return s + d - s * d; }
float3 BlendLerp(float3 s, float3 d){ return lerp(s, d, 0.5); }
float3 BlendColorDodge(float3 s, float3 d){ return d / (1.0 - s); }
float3 BlendLighterColor(float3 s, float3 d){ return (s.x + s.y + s.z > d.x + d.y + d.z) ? s : d; }
float3 BlendHardMix(float3 s, float3 d){ return floor(s+d); }
float3 BlendDifference(float3 s, float3 d){ return abs(d-s); }
float3 BlendExclusion(float3 s, float3 d){ return s + d - 2.0*s*d; }
float3 BlendLinearLight(float3 s, float3 d){ return 2.0*s + d - 1.0; }

float3 BlendOverlay(float3 s, float3 d){
    float3 c;
    c.x = BlendOverlay(s.x, d.x);
    c.y = BlendOverlay(s.y, d.y);
    c.z = BlendOverlay(s.z, d.z);
    return c; 
}

float3 BlendSoftLight(float3 s, float3 d){
    float3 c;
    c.x = BlendSoftLight(s.x, d.x);
    c.y = BlendSoftLight(s.y, d.y);
    c.z = BlendSoftLight(s.z, d.z);
    return c;
}

float3 BlendHardLight(float3 s, float3 d){
    float3 c;
    c.x = BlendHardLight(s.x, d.x);
    c.y = BlendHardLight(s.y, d.y);
    c.z = BlendHardLight(s.z, d.z);
    return c;
}

float3 BlendVividLight(float3 s, float3 d){
    float3 c;
    c.x = BlendVividLight(s.x, d.x);
    c.y = BlendVividLight(s.y, d.y);
    c.z = BlendVividLight(s.z, d.z);
    return c;
}

float3 BlendPinLight(float3 s, float3 d){
    float3 c;
    c.x = BlendPinLight(s.x, d.x);
    c.y = BlendPinLight(s.y, d.y);
    c.z = BlendPinLight(s.z, d.z);
    return c;
}

float3 BlendHue(float3 s, float3 d){
    d = RGBtoHSV(d);
    d.x = RGBtoHSV(s).x;
    return HSVtoRGB(d);
}

float3 BlendColor(float3 s, float3 d){
    s = RGBtoHSV(s);
    s.z = RGBtoHSV(d).z;
    return HSVtoRGB(s);
}

float3 BlendSaturation(float3 s, float3 d){
    d = RGBtoHSV(d);
    d.y = RGBtoHSV(s).y;
    return HSVtoRGB(d);
}

float3 Desaturate(float3 col){
	return dot(col, float3(0.3, 0.59, 0.11));
}

float3 BlendLuminosity(float3 s, float3 d){
    float dLum = Desaturate(d);
    float sLum = Desaturate(s);
    float lum = sLum - dLum;
    float3 c = d + lum;
    float minC = min(min(c.x, c.y), c.z);
    float maxC = max(max(c.x, c.y), c.z);
    if (minC < 0.0) 
		return sLum + ((c - sLum) * sLum) / (sLum - minC);
    else if (maxC > 1.0) 
		return sLum + ((c - sLum) * (1.0 - sLum)) / (maxC - sLum);
    else 
		return c;
}

float3 GetContrast(float3 col, float contrast){
    return lerp(float3(0.5,0.5,0.5), col, contrast);
}

float3 GetSaturation(float3 col, float interpolator){
    return lerp(dot(col, float3(0.3,0.59,0.11)), col, interpolator);
}

// 0 Add, 1 Sub (or alpha), 2 Mul, 3 Mulx2, 4 Overlay, 5 Screen, 6 Lerp
float3 BlendColors(float3 col0, float3 col1, int blendType){
	float3 blends[7] = {
		col0 + col1,
		col0 - col1,
		col0 * col1,
		col0 * col1 * mochie_ColorSpaceDouble,
		BlendOverlay(col0, col1),
		BlendScreen(col0, col1),
		BlendLerp(col0, col1)
	};
	return blends[blendType];
}

float3 BlendColors(float3 col0, float3 col1, int blendType, float interpolator){
	float3 blends[7] = {
		col0 + (col1*interpolator),
		col0 - (col1*interpolator),
		col0 * lerp(1, col1, interpolator),
		col0 * lerp(1, col1 * mochie_ColorSpaceDouble, interpolator),
		BlendOverlay(col0, col1 * interpolator),
		BlendScreen(col0, col1 * interpolator),
		lerp(col0, col1, interpolator)
	};
	return blends[blendType];
}

float3 BlendColorsAlpha(float3 col0, float3 col1, int blendType, float alpha){
	float3 blends[7] = {
		col0 + col1,
		lerp(col0, col1, alpha),
		col0 * col1,
		col0 * col1 * mochie_ColorSpaceDouble,
		BlendOverlay(col0, col1),
		BlendScreen(col0, col1),
		BlendLerp(col0, col1)
	};
	return blends[blendType];
}

float3 BlendColorsAlpha(float3 col0, float3 col1, int blendType, float interpolator, float alpha){
	float3 blends[7] = {
		col0 + (col1*interpolator),
		lerp(col0, col1, interpolator*alpha),
		col0 * lerp(1, col1, interpolator),
		col0 * lerp(1, col1 * mochie_ColorSpaceDouble, interpolator),
		BlendOverlay(col0, col1 * interpolator),
		BlendScreen(col0, col1 * interpolator),
		lerp(col0, col1, interpolator)
	};
	return blends[blendType];
}

float BlendScalars(float scalar0, float scalar1, int blendType){
	float blends[7] = {
		scalar0 + scalar1,
		scalar0 - scalar1,
		scalar0 * scalar1,
		scalar0 * scalar1 * mochie_ColorSpaceDouble.x,
		BlendOverlay(scalar0, scalar1),
		BlendScreen(scalar0, scalar1),
		BlendLerp(scalar0, scalar1)
	};
	return blends[blendType];
}

float BlendScalars(float scalar0, float scalar1, int blendType, float interpolator){
	float blends[7] = {
		scalar0 + (scalar1*interpolator),
		scalar0 - (scalar1*interpolator),
		scalar0 * lerp(1, scalar1, interpolator),
		scalar0 * lerp(1, scalar1 * mochie_ColorSpaceDouble.x, interpolator),
		BlendOverlay(scalar0, scalar1 * interpolator),
		BlendScreen(scalar0, scalar1 * interpolator),
		lerp(scalar0, scalar1, interpolator)
	};
	return blends[blendType];
}

float BlendScalarsAlpha(float scalar0, float scalar1, int blendType, float interpolator, float alpha){
	float blends[7] = {
		scalar0 + (scalar1*interpolator),
		lerp(scalar0, scalar1, interpolator*alpha),
		scalar0 * lerp(1, scalar1, interpolator),
		scalar0 * lerp(1, scalar1 * mochie_ColorSpaceDouble.x, interpolator),
		BlendOverlay(scalar0, scalar1 * interpolator),
		BlendScreen(scalar0, scalar1 * interpolator),
		lerp(scalar0, scalar1, interpolator)
	};
	return blends[blendType];
}

// ---------------------------
// Fake HDR
// ---------------------------
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

#endif // COLOR_INCLUDED