// ---------------------------
// Color Filters
// ---------------------------
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
    filteredCol = GetContrast(filteredCol);
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
    filteredCol = GetContrast(filteredCol);
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
        case 1: case 2: col *= i.globalF; col.a *= _Opacity; break;
        case 3: case 4: col *= i.globalF; break;
        case 5: col = lerp(1, col, i.globalF); break;
        case 6: col = lerp(0.5, col, i.globalF); break;
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
	col = GetGrayscale(col);
	return col.r;
}

// ---------------------------
// Shake
// ---------------------------
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
	_BlurStr *= i.blurF;
	_BlurStr *= 8;
	#if UNITY_SINGLE_PASS_STEREO
		_BlurStr *= 0.75;
	#endif
    return GetPixelBlurColor(i.uv);
}

float3 GetCrushBlur(v2f i){
	_BlurStr = GetDoF(i, _BlurStr);
	_BlurStr *= i.blurF;
	_BlurStr *= 8;
	#if UNITY_SINGLE_PASS_STEREO
		_BlurStr *= 0.75;
	#endif
    return GetCrushBlurColor(i.uv);
}

float2 DoDitherBlur(v2f i){
	_BlurStr = GetDoF(i, _BlurStr);
	_BlurStr *= i.blurF;
    UNITY_BRANCH
    if (_BlurStr > 0 && _BlurModel == 2 && _RGBSplit == 0){
        _BlurStr *= 0.01;
        float2 noise = GetNoiseRGB(i.uv, _BlurStr).rg;
        UNITY_BRANCH
        if (_Flicker == 1){
            noise.r *= sin(_Time.y * _FlickerSpeedX*100);
            noise.g *= sin(_Time.y * _FlickerSpeedY*100);
        }
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
        UNITY_BRANCH
        if (_Flicker == 1){
            noise.r *= sin(_Time.y * _FlickerSpeedX*100);
            noise.g *= sin(_Time.y * _FlickerSpeedY*100);
            noise.b *= sin(_Time.y * ((_FlickerSpeedX+_FlickerSpeedY)/2)*100);
        }
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
		UNITY_BRANCH
		switch (_BlurModel){
			case 1: blurCol = GetPixelBlur(i); break;
			case 2: blurCol = GetRGBDitherBlur(i, blurCol); break;
			case 3: blurCol = GetRadialBlur(i); break;
			case 4: blurCol = GetCrushBlur(i); break;
			default: blurCol = col; break;
		}
		col = lerp(col, blurCol, _BlurOpacity);
	}
    return col;
}
#if defined(SFXX)
	#include "SFXXFunctions.cginc"
#endif