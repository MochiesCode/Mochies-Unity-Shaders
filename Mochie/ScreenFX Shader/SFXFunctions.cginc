#ifndef SFX_FUNCS_INCLUDED
#define SFX_FUNCS_INCLUDED

// ---------------------------
// Triplanar/Depth
// ---------------------------
float2 AlignWithGrabTexel(float2 uv){
	return (floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) * abs(_CameraDepthTexture_TexelSize.xy);
}

void GetDepth(v2f i, out float3 wPos, out float3 wNorm, out float depth){
    depth = Linear01Depth(DecodeFloatRG(tex2Dproj(_CameraDepthTexture, i.uv)));
    float4 vPos = float4(i.raycast * depth, 1);
    wPos = mul(unity_CameraToWorld, vPos).xyz;
	wNorm = normalize(cross(ddy_fine(wPos), ddx_fine(wPos)));
}

float GetRadius(v2f i, float3 pos, float range, float falloff){
    GetDepth(i, wPos, wNorm, depth);
    float dist = distance(wPos, pos);
    dist = smoothstep(range, range - falloff, dist);
    return dist;
}

float GetTriplanarRadius(v2f i, float3 pos, float range, float falloff){
	GetDepth(i, wPos, wNorm, depth);
	float dist = distance(wPos, pos);
	#if TRIPLANAR_ENABLED
		if (_Triplanar == 1)
			dist = smoothstep(range, range - falloff, dist);
		else if (_Triplanar == 2)
			_TPScanFade = lerp(_TPThickness-0.001, -3, _TPScanFade);
			dist = smoothstep(range+_TPThickness, range+_TPScanFade, dist) * smoothstep(range-_TPThickness, range-_TPScanFade, dist);
	#endif
	return dist;
}

float4 GetTriplanar(v2f i, sampler2D tex, sampler2D nTex, float2 _ST0, float2 _ST1, float radius) {
    #if MAIN_PASS
        wPos += _Time.y*_DistortionSpeed;
    #else
        _ST0 *= 0.5;
    #endif
    wNorm = abs(wNorm);
    wNorm /= (wNorm.x + wNorm.y + wNorm.z);
    #if MAIN_PASS
        float2 uvX = wPos.yz * _ST0;
        float2 uvY = wPos.xz * _ST0;
        float2 uvZ = wPos.xy * _ST0;
        float4 diffX = float4(UnpackNormal(tex2D(tex, uvX)), 1);
        float4 diffY = float4(UnpackNormal(tex2D(tex, uvY)), 1);
        float4 diffZ = float4(UnpackNormal(tex2D(tex, uvZ)), 1);
        return float4((diffX * wNorm.x) + (diffY * wNorm.y) + (diffZ * wNorm.z));
    #elif TRIPLANAR_PASS
		#if TRIPLANAR_ENABLED
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
			if (_Triplanar == 2)
				radius = saturate(radius-GetNoise(i.uv.xy, _TPNoise));
			col.a *= radius*i.globalF*i.pulseSpeed;
			col.a *= lerp(noise, 1, col.a);
			return col;
		#endif
    #endif
	return 0;
}

// ---------------------------
// Color Filters
// ---------------------------
float3 GetInversion(float3 col){
    col.rgb = abs(_Invert - col.rgb);
    col.r = abs(_InvertR - col.r);
    col.g = abs(_InvertG - col.g);
    col.b = abs(_InvertB - col.b);
    return col;
}
// dot(col, float3(0.3,0.59,0.11))
void ApplySaturation(inout float3 col){
	float3 satCol = GetSaturation(col, _Saturation);
	float satR = GetSaturation(satCol, _SaturationR).r;
	float satG = GetSaturation(satCol, _SaturationG).g;
	float satB = GetSaturation(satCol, _SaturationB).b;
	col = float3(satR, satG, satB);
}

void ApplyGeneralFilters(inout float3 col){
    ApplySaturation(col);
    col = lerp(col, GetHDR(col), _HDR);
    col = GetContrast(col, _Contrast);
    col *= _Brightness;
}

void ApplyNoise(v2f i, inout float3 col){
	float3 noiseCol = col;
	noiseCol += GetScanNoise(i.uv, _ScanLine, _ScanLineThick, _ScanLineSpeed);
	noiseCol += GetNoiseRGB(i.uv, _NoiseRGB);
	noiseCol += GetNoiseSFX(i.uv, _Noise);
	col = lerp(col, noiseCol, _NoiseStrength * i.noiseF * i.pulseSpeed);
}

void ApplyColor(v2f i, inout float3 col){
	float3 rgb = GetInversion(col) * _Color;
	_Hue += lerp(0, frac(_Time.y*_AutoShiftSpeed), _AutoShift);
	float3 hsv = HSVShift(rgb, _Hue, 0, 0);
	hsv *= _RGB;
	ApplyGeneralFilters(hsv);
	col = lerp(col, hsv, _FilterStrength * i.colorF * i.pulseSpeed);
}

void ApplyTransparency(v2f i, inout float4 col){
    switch (_BlendMode){
        case 1: col *= i.globalF; col.a *= _Opacity; break;
		case 2: col *= i.globalF * _Opacity; break;
        case 3: case 4: col.rgb *= i.globalF; col.rgb *= _Opacity; break;
        case 5: col = lerp(1, col, i.globalF*_Opacity); break;
        case 6: col = lerp(0.5, col, i.globalF*_Opacity); break;
        default: break;
    }
}

float4 DoZoomTransparency(v2f i, float4 col){
    switch (_BlendMode){
        case 1: col *= i.globalF; col.a *= _Opacity; break;
		case 2: col *= i.globalF; break;
        case 3: case 4: col.rgb *= i.globalF; col.rgb *= _Opacity; break;
        case 5: col = lerp(1, col, i.globalF*_Opacity); break;
        case 6: col = lerp(0.5, col, i.globalF*_Opacity); break;
        default: break;
    }
    return col;
}

float SampleDepthTex(float2 uv){
	return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
}

float GrayscaleSample(float2 uv){
	float3 col = tex2D(_MSFXGrab, uv);
	col = Desaturate(col);
	return col.r;
}

// ---------------------------
// Shake
// ---------------------------
float2 GetShakeTime(){
    float2 shakeTime;
    if (_ShakeModel != 3){
        shakeTime.x = sin(_Time.y * _ShakeSpeedX * 0.2);
        shakeTime.y = sin(_Time.y * _ShakeSpeedY * 0.2);
        if (_ShakeModel == 2)
            shakeTime = round(shakeTime);
    }
    else {
        shakeTime = _Time.x * _ShakeSpeedXY;
    }
    return shakeTime;
}

void ApplyNoiseShake(inout v2f i){
	_Amplitude *= 0.1;
	float2 offset = GetShakeTime();
	offset = tex2Dlod(_ShakeNoiseTex, float4(offset.xy,0,0)).rg-0.27;
	i.uv.xy += (offset * i.pulseSpeed * i.shakeF * _Amplitude);
}

void ApplyShake(inout v2f i){
	if (_ShakeModel != 3){
		float2 uv0 = i.uv.xy;
		float2 shakeTime = GetShakeTime();
		_Amplitude *= 50;
		_Amplitude *= i.shakeF;
		uv0.x += shakeTime.x * (_MSFXGrab_TexelSize.x * _Amplitude);
		uv0.y += shakeTime.y * (_MSFXGrab_TexelSize.y * _Amplitude);
		i.uv.xy = uv0;
	}
}

// ---------------------------
// Distortion
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

void ApplyMapDistortion(inout v2f i){
    float3 uv0 = i.uv.xyz;
	float2 uv1 = i.uvd;
	uv1.x *= 1.4;
	_DistortionStr *= 128;
	_DistortionStr *= i.distortionF;
	float2 distMap = UnpackNormal(tex2D(_NormalMap, uv1)).rg;
	float2 offset = distMap * _DistortionStr * _MSFXGrab_TexelSize.xy;
	uv0.xy = (offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(uv0.z)) + uv0.xy;
	i.uv.xy = uv0.xy;
}

void ApplyWGDistortion(inout v2f i){
    float4 uv0 = i.uv;
	_DistortionStr *= 128;
	_DistortionStr *= i.distortionF;
	float3 distortPos = lerp(i.cameraPos, i.objPos, _DistortionP2O);
	float radius = GetRadius(i, distortPos, _DistortionRadius, _DistortionFade);
	float2 distMap = GetTriplanar(i, _NormalMap, _NormalMap, _NormalMap_ST.xy, 0, 0).rg;	
	float2 offset = distMap * _DistortionStr * _MSFXGrab_TexelSize.xy;
	uv0.xy = lerp(uv0.xy, (offset * (UNITY_Z_0_FAR_FROM_CLIPSPACE(uv0.z)) + uv0.xy), radius);
	i.uv.xy = uv0.xy;
}

// ---------------------------
// Blur
// ---------------------------
void EncodeHDR(inout float3 rgb) {
	rgb *= 1.0 / 8;
	float m = max(max(rgb.r, rgb.g), max(rgb.b, 1e-6));
	m = ceil(m * 255) / 255;
	rgb /= m;
}

void ApplyCrush(inout float3 col, float strength){
	float3 crushCol = GammaToLinearSpace(col);
	EncodeHDR(crushCol);
	col = lerp(col, crushCol, strength);
}

float GetDoF(v2f i){
	float3 focusPos = lerp(i.cameraPos, i.objPos, _DoFP2O);
	return saturate(GetRadius(i, focusPos, _DoFRadius, _DoFFade));
}

void ApplyRipplePixelate(inout v2f i){
	if (_RippleGridStr > 0){
		_RippleGridStr *= i.blurF;
		i.uv.xy += sin(i.pos) * _RippleGridStr/1000;
	}
	if (_PixelationStr > 0){
		_PixelationStr *= i.blurF;
		_PixelationStr = lerp(1e-08, 0.01, _PixelationStr);
		i.uv.x = (int)(i.uv.x / _PixelationStr) * _PixelationStr;
		i.uv.y = (int)(i.uv.y / _PixelationStr) * _PixelationStr;
	}
}

void ApplyPixelBlur(v2f i, inout float3 blurCol){
	_BlurStr *= 16;
	#if BLUR_Y_ENABLED
		#if CHROM_ABB_ENABLED
			ApplyChromaticAbberationY(_MSFXGrab, _MSFXGrab_TexelSize.xy, i.uv, _PixelBlurSamples, _BlurStr, blurCol);
		#else
			ApplyStandardBlurY(_MSFXGrab, _MSFXGrab_TexelSize.xy, i.uv, _PixelBlurSamples, _BlurStr, blurCol);
		#endif	
	#else
		#if CHROM_ABB_ENABLED
			ApplyChromaticAbberation(_MSFXGrab, _MSFXGrab_TexelSize.xy, i.uv, _PixelBlurSamples, _BlurStr, blurCol);
		#else
			ApplyStandardBlur(_MSFXGrab, _MSFXGrab_TexelSize.xy, i.uv, _PixelBlurSamples, _BlurStr, blurCol);
		#endif
	#endif
	ApplyCrush(blurCol, _CrushBlur);
}

void ApplyDitherBlur(inout v2f i){
    #if BLUR_DITHER_ENABLED && !CHROM_ABB_ENABLED
        _BlurStr *= 0.01;
        float2 noise = GetNoiseRGB(i.uv, _BlurStr).rg;
		i.uv.y += noise.g;
		#if !BLUR_Y_ENABLED
			i.uv.x += noise.r;
		#endif
    #endif
}

void ApplyRGBDitherBlur(v2f i, inout float3 col){
	_BlurStr *= 0.01;
	float3 noise = GetNoiseRGB(i.uv, _BlurStr);
	#if BLUR_Y_ENABLED
		float4 redUV = float4(i.uv.x, i.uv.y+(noise.r*0.3333), i.uv.zw);
		float4 greenUV = float4(i.uv.x, i.uv.y+(noise.g*0.6666), i.uv.zw);
		float4 blueUV = float4(i.uv.x, i.uv.y+noise.b, i.uv.zw);
	#else
		float4 redUV = float4(i.uv.x+noise.r, i.uv.yzw);
		float4 greenUV = float4(i.uv.x, i.uv.y+noise.g, i.uv.zw);
		float4 blueUV = float4(i.uv.x+noise.b, i.uv.y+noise.b, i.uv.zw);
	#endif
	float red = tex2Dproj(_MSFXGrab, redUV).r;
	float green = tex2Dproj(_MSFXGrab, greenUV).g;
	float blue = tex2Dproj(_MSFXGrab, blueUV).b;
	col = float3(red, green, blue);
}

void ApplyBlur(v2f i, inout float3 col, float3 blurCol){
	_BlurStr *= i.blurF;
	#if BLUR_PIXEL_ENABLED
		ApplyPixelBlur(i, blurCol);
	#elif BLUR_DITHER_ENABLED && CHROM_ABB_ENABLED
		ApplyRGBDitherBlur(i, blurCol);
	#elif BLUR_RADIAL_ENABLED
		ApplyRadialBlur(i, _MSFXGrab, i.uv, _BlurSamples, _BlurRadius, _BlurStr, blurCol);
	#endif
	col = lerp(col, blurCol, _BlurOpacity);
}

#endif