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
// Color Filters
// ---------------------------
float3 GetInversion(float3 col){
    col.rgb = abs(_Invert - col.rgb);
    col.r = abs(_InvertR - col.r);
    col.g = abs(_InvertG - col.g);
    col.b = abs(_InvertB - col.b);
    return col;
}

float3 GetHSLFilter(v2f i, float3 col){
    float pulseSpeed = i.pulseSpeed;
    float2 uv = i.uv.xy;
	#if defined(SFXX)
		if (_PulseColor == 1)
			i.colorF *= pulseSpeed;
	#endif
    float3 filteredCol = col;
    filteredCol = GetInversion(filteredCol);
    UNITY_BRANCH
    if (_AutoShift == 1)
        _Hue += frac(_Time.y*_AutoShiftSpeed);
    float3 shift = float3(_Hue, _SaturationHSL, _Luminance);
    float3 hsl = RGBtoHSL(filteredCol);
    float hslRange = step(_HSLMin, hsl) * step(hsl, _HSLMax);
    filteredCol = HSLtoRGB(hsl + shift * hslRange);
    filteredCol = lerp(filteredCol, GetHDR(filteredCol), _HDR);
    filteredCol = GetContrast(filteredCol, _Contrast);
    filteredCol += filteredCol*_Exposure;
    filteredCol += GetNoiseRGB(uv, _Noise).r;
    col = lerp(col, filteredCol, i.colorF);
    return col;
}

float3 GetRGBFilter(v2f i, float3 col){
    float pulseSpeed = i.pulseSpeed;
    float2 uv = i.uv.xy;
	#if defined(SFXX)
		if (_PulseColor == 1)
			i.colorF *= pulseSpeed;
	#endif
    float3 filteredCol = col;
    filteredCol = GetInversion(filteredCol);
    filteredCol *= _Color.rgb;
    filteredCol = GetSaturation(filteredCol, _SaturationRGB);
    filteredCol = lerp(filteredCol, GetHDR(filteredCol), _HDR);
    filteredCol = GetContrast(filteredCol, _Contrast);
    filteredCol += filteredCol*_Exposure;
    filteredCol += GetNoiseRGB(uv, _Noise).r;
    col = lerp(col, filteredCol, i.colorF);
    return col;
}

float3 DoColor(v2f i, float3 col){
    // UNITY_BRANCH
    // if (_FilterModel > 0 && _Sobel == 1){
    //     float3 n[8];
    //     SobelKernel3(i, _MSFXGrab, n);
    //     col = lerp(col, GetSobel3(n), _SobelStr);
    // }
    UNITY_BRANCH
    switch (_FilterModel){
        case 1: col = GetRGBFilter(i, col); break;
        case 2: col = GetHSLFilter(i, col); break;
        default: break;
    }
    return col;
}

float4 DoTransparency(v2f i, float4 col){
    UNITY_BRANCH
    switch (_BlendMode){
        case 1: col *= i.globalF; col.a *= _Opacity; break;
		case 2: col *= i.globalF * _Opacity; break;
        case 3: case 4: col.rgb *= i.globalF; col.rgb *= _Opacity; break;
        case 5: col = lerp(1, col, i.globalF*_Opacity); break;
        case 6: col = lerp(0.5, col, i.globalF*_Opacity); break;
        default: break;
    }
    return col;
}

float4 DoZoomTransparency(v2f i, float4 col){
    UNITY_BRANCH
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

float3 DoRounding(float3 col){
	UNITY_BRANCH
	if (_RoundingToggle){
		float3 roundedCol = col*(round(col.rgb*_Rounding)/_Rounding);
		col = lerp(col, roundedCol, _RoundingOpacity);
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

float2 DoNoiseShake(float falloff){
    float2 offset = 0;
    UNITY_BRANCH
    if (_ShakeModel == 3){
        _Amplitude *= 0.1;
        _Amplitude *= falloff;
        offset = GetShakeTime();
        offset = tex2Dlod(_ShakeNoiseTex, float4(offset.xy, 0, 0)).rg;
        offset = (offset-0.27) * _Amplitude;
    }
    return offset; 
}

float2 DoShake(v2f i){
    float2 uv0 = i.uv.xy;
    UNITY_BRANCH
    if (_ShakeModel == 1 || _ShakeModel == 2){
        float2 shakeTime = GetShakeTime();
        _Amplitude *= 50;
        _Amplitude *= i.shakeF;
        uv0.x += shakeTime.x * (_MSFXGrab_TexelSize.x * _Amplitude);
        uv0.y += shakeTime.y * (_MSFXGrab_TexelSize.y * _Amplitude);
    }
    return uv0;
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

float2 GetMapDistortion(v2f i){
    float3 uv0 = i.uv.xyz;
    UNITY_BRANCH
    if (_DistortionStr > 0){
        float2 uv1 = i.uvd;
        uv1.x *= 1.4;
        _DistortionStr *= 128;
        _DistortionStr *= i.distortionF;
        float2 distMap = UnpackNormal(tex2D(_NormalMap, uv1)).rg;
        float2 offset = distMap * _DistortionStr * _MSFXGrab_TexelSize.xy;
        uv0.xy = (offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(uv0.z)) + uv0.xy;
    }
    return uv0.xy;
}

float2 GetWGDistortion(v2f i){
    float4 uv0 = i.uv;
    UNITY_BRANCH
    if (_DistortionStr > 0){
        _DistortionStr *= 128;
        _DistortionStr *= i.distortionF;
        float3 distortPos = lerp(i.cameraPos, i.objPos, _DistortionP2O);
        float radius = GetRadius(i, distortPos, _DistortionRadius, _DistortionFade);
        float2 distMap = GetTriplanar(i, _NormalMap, _NormalMap, _NormalMap_ST.xy, 0, 0).rg;	
        float2 offset = distMap * _DistortionStr * _MSFXGrab_TexelSize.xy;
        uv0.xy = lerp(uv0.xy, (offset * (UNITY_Z_0_FAR_FROM_CLIPSPACE(uv0.z)) + uv0.xy), radius);
    }
    return uv0.xy;
}

float2 DoDistortion(v2f i){
    UNITY_BRANCH
    switch (_DistortionModel){
        case 1: i.uv.xy = GetMapDistortion(i); break;
        case 2: i.uv.xy = GetWGDistortion(i); GetDepth(i, wPos, wNorm, depth); break;
        default: break;
    }
    return i.uv.xy;
}

// ---------------------------
// Blur
// ---------------------------
float3 EncodeHDR(float3 rgb) {
	rgb *= 1.0 / 8;
	float m = max(max(rgb.r, rgb.g), max(rgb.b, 1e-6));
	m = ceil(m * 255) / 255;
	return rgb/m;
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

float3 Crush(float3 col){
	col.rgb = GammaToLinearSpace(col);
	col = EncodeHDR(col);
	return col;
}

float2 DoRipplePixelate(v2f i){
    float2 uv0 = i.uv.xy;
    UNITY_BRANCH
    if (_BlurModel > 0){
        UNITY_BRANCH
        if (_RippleGridStr > 0){
            _RippleGridStr = GetDoF(i, _RippleGridStr);
            _RippleGridStr *= i.blurF;
            uv0 += sin(i.pos) * _RippleGridStr/1000;
        }
        UNITY_BRANCH
        if (_PixelationStr > 0){
            _PixelationStr = GetDoF(i, _PixelationStr);
            _PixelationStr *= i.blurF;
            _PixelationStr = lerp(1e-08, 0.01, _PixelationStr);
            uv0.x = (int)(uv0.x / _PixelationStr) * _PixelationStr;
            uv0.y = (int)(uv0.y / _PixelationStr) * _PixelationStr;
        }
    }
    return uv0;
}

float3 GetPixelBlur(v2f i){
	_BlurStr = GetDoF(i, _BlurStr);
	_BlurStr *= 16;
	float3 blurCol = 0;
	UNITY_BRANCH
	if (_BlurY == 0){
		UNITY_BRANCH
		if (_RGBSplit == 0)
			StandardBlur(_MSFXGrab, _MSFXGrab_TexelSize.xy, i.uv, _PixelBlurSamples, _BlurStr, blurCol);
		else 
			ChromaticAbberation(_MSFXGrab, _MSFXGrab_TexelSize.xy, i.uv, _PixelBlurSamples, _BlurStr, blurCol);
	}
	else {
		UNITY_BRANCH
		if (_RGBSplit == 0)
			StandardBlurY(_MSFXGrab, _MSFXGrab_TexelSize.xy, i.uv, _PixelBlurSamples, _BlurStr, blurCol);
		else
			ChromaticAbberationY(_MSFXGrab, _MSFXGrab_TexelSize.xy, i.uv, _PixelBlurSamples, _BlurStr, blurCol);
	}

	UNITY_BRANCH
	if (_CrushBlur == 1)
		blurCol = Crush(blurCol);
	return blurCol;
}

float2 DoDitherBlur(v2f i){
    UNITY_BRANCH
    if (_BlurStr > 0 && _BlurModel == 2 && _RGBSplit == 0){
		_BlurStr = GetDoF(i, _BlurStr);
        _BlurStr *= 0.01;
        float2 noise = GetNoiseRGB(i.uv, _BlurStr).rg;
        i.uv.y += noise.g;
        UNITY_BRANCH
        if (_BlurY != 1)
            i.uv.x += noise.r;
    }
    return i.uv.xy;
}

float3 GetRGBDitherBlur(v2f i, float3 col){
    UNITY_BRANCH
    if (_RGBSplit == 1){
        _BlurStr *= 0.01;
        float3 noise = GetNoiseRGB(i.uv, _BlurStr);
        float4 redUV = float4(i.uv.x+noise.r, i.uv.yzw);
        float4 greenUV = float4(i.uv.x, i.uv.y+noise.g, i.uv.zw);
        float4 blueUV = float4(i.uv.x+noise.b, i.uv.y+noise.b, i.uv.zw);
        float red = tex2Dproj(_MSFXGrab, redUV).r;
        float green = tex2Dproj(_MSFXGrab, greenUV).g;
        float blue = tex2Dproj(_MSFXGrab, blueUV).b;
        col = float3(red, green, blue);
    }
    return col;
}

float3 DoBlur(v2f i, float3 col, float3 blurCol){
	UNITY_BRANCH
	if (_BlurStr != 0){
		_BlurStr *= i.blurF;
		[forcecase]
		switch (_BlurModel){
			case 1: blurCol = GetPixelBlur(i); break;
			case 2: blurCol = GetRGBDitherBlur(i, blurCol); break;
			case 3: RadialBlur(i, _MSFXGrab, i.uv, _BlurSamples, _BlurRadius, _BlurStr, blurCol); break;
			default: blurCol = col; break;
		}
		col = lerp(col, blurCol, _BlurOpacity);
		
	}
    return col;
}

#endif