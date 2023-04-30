#ifndef PS_FUNCTIONS_INCLUDED
#define PS_FUNCTIONS_INCLUDED

float GetAudioLinkBand(audioLinkData al, int band, float remapMin, float remapMax){
	float4 bands = float4(al.bass, al.lowMid, al.upperMid, al.treble);
	return Remap(bands[band], _AudioLinkRemapMin, _AudioLinkRemapMax, remapMin, remapMax);
}

void GrabExists(inout audioLinkData al, inout float versionBand, inout float versionTime){
	float width = 0;
	float height = 0;
	_AudioTexture.GetDimensions(width, height);
	if (width > 64){
		versionBand = 0.0625;
		versionTime = 0.25;
	}
	al.textureExists = width > 16;
}

float SampleAudioTexture(float time, float band){
	return MOCHIE_SAMPLE_TEX2D_LOD(_AudioTexture, float2(time,band),0);
}

void InitializeAudioLink(inout audioLinkData al, float time){
	float versionBand = 1;
	float versionTime = 1;
	GrabExists(al, versionBand, versionTime);
	if (al.textureExists){
		time *= versionTime;
		al.bass = SampleAudioTexture(time, 0.125 * versionBand);
		al.lowMid = SampleAudioTexture(time, 0.375 * versionBand);
		al.upperMid = SampleAudioTexture(time, 0.625 * versionBand);
		al.treble = SampleAudioTexture(time, 0.875 * versionBand);
	}
}

float4 ApplyLayeredTex(v2f i, float4 texCol){
	float alpha = _BlendMode == 1 ? texCol.a : 1;
	float2 secondTexUV = TRANSFORM_TEX(i.uv0, _SecondTex) + (_Time.y * _SecondTexScroll);
	float4 secondTexCol = tex2D(_SecondTex, secondTexUV) * _SecondColor;
	switch (_TexBlendMode){
		case 0: texCol = lerp(secondTexCol*alpha, texCol, texCol.a); break;
		case 1: texCol.rgb += secondTexCol.rgb*alpha; break;
		case 2: texCol.rgb -= secondTexCol.rgb; break;
		case 3: texCol.rgb *= secondTexCol.rgb; break;
		default: break;
	}
    return texCol;
}

void ApplyDistortion(inout v2f i, float alpha, audioLinkData al){
	#if AUDIOLINK_ENABLED
		if (_AudioLinkDistortionStrength > 0){
			float alDistortion = GetAudioLinkBand(al, _AudioLinkDistortionBand, _AudioLinkRemapDistortionMin, _AudioLinkRemapDistortionMax);
			_DistortionStr *= lerp(1, alDistortion, _AudioLinkDistortionStrength * _AudioLinkStrength);
		}
	#endif
	float2 duv = i.uv0.xy - (_Time.y*_DistortionSpeed);
	duv *= _NormalMapScale;
	float2 normal = UnpackNormal(tex2D(_NormalMap, duv)).rg;
	float2 offset = normal * alpha * _DistortionStr * ((i.color.r + i.color.b + i.color.g)/3.0);
	#if FADING_ENABLED
		offset *= fade;
	#endif
	#if DISTORTION_ENABLED
		i.uv1.xy += offset;
	#endif
	#if DISTORTION_UV_ENABLED
		i.uv0.xy += offset;
	#endif
}


float3 GetHSVFilter(float4 col, audioLinkData al){
	float3 baseCol = col;
	_Hue += lerp(0, frac(_Time.y*_AutoShiftSpeed), _AutoShift);
	float3 filteredCol = HSVShift(col.rgb, _Hue, 0, 0);
	filteredCol = GetSaturation(filteredCol, _Saturation);
	filteredCol = lerp(filteredCol, GetHDR(filteredCol), _HDR);
	filteredCol = GetContrast(filteredCol, _Contrast);
	col.rgb = lerp(col.rgb, filteredCol, col.a);
	col.rgb *= _Brightness;
	#if AUDIOLINK_ENABLED
		if (_AudioLinkFilterStrength > 0){
			float alFilter = GetAudioLinkBand(al, _AudioLinkFilterBand, _AudioLinkRemapFilterMin, _AudioLinkRemapFilterMax);
			alFilter = lerp(1, alFilter, _AudioLinkFilterStrength * _AudioLinkStrength);
			col.rgb = lerp(baseCol, col.rgb, alFilter);
		}
	#endif
    return col;
}

void Softening(v2f i, inout float fade){
	#if FADING_ENABLED
		float2 screenUV = i.projPos.xy / i.projPos.w;
		#if UNITY_UV_STARTS_AT_TOP
			if (_CameraDepthTexture_TexelSize.y < 0) {
				screenUV.y = 1 - screenUV.y;
			}
		#endif
		float sceneZ = LinearEyeDepth(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV));
		float partZ = i.projPos.z;
		fade = saturate((1-_SoftenStr) * (sceneZ-partZ));
	#endif
}

float4 GetTexture(v2f i, audioLinkData al){ 

	#if DISTORTION_ENABLED
		float4 texCol = tex2D(_MainTex, i.uv0.xy);
		ApplyDistortion(i, texCol.a, al);
		#if DISTORTION_UV_ENABLED
			texCol = tex2D(_MainTex, i.uv0.xy);
		#endif
		i.uv1.xy /= i.uv1.w;
		float4 grabCol = float4(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MPSGrab, i.uv1.xy).rgb, texCol.a);
		texCol = lerp(texCol, grabCol*lerp(1,texCol.a,_BlendMode == 1), _DistortionBlend);
	#else
		float4 texCol = tex2D(_MainTex, i.uv0.xy);
	#endif

	#if FLIPBOOK_BLEND_ENABLED
		float4 blendedTex = tex2D(_MainTex, i.uv0.xy);
		texCol = lerp(texCol, blendedTex, i.uv0.z);
	#endif

    #if ALPHA_TEST_ENABLED
		#if AUDIOLINK_ENABLED
			if (_AudioLinkCutoutStrength > 0){
				float alCutout = GetAudioLinkBand(al, _AudioLinkCutoutBand, _AudioLinkRemapCutoutMin, _AudioLinkRemapCutoutMax);
				alCutout = lerp(1, alCutout, _AudioLinkCutoutStrength * _AudioLinkStrength);
				_Cutoff = lerp(1, _Cutoff, alCutout);
			}
		#endif
        clip(texCol.a - _Cutoff);
	#endif

	#if LAYERED_TEX_ENABLED
		texCol = ApplyLayeredTex(i, texCol);
	#endif

    return texCol;
}

float4 GetProjPos(float4 vertex0, float4 vertex1){
	float4 projPos = ComputeScreenPos(vertex1);
	projPos.z = -UnityObjectToViewPos(vertex0).z;
    return projPos;
}

float4 GetFalloffPosition(v2f i){
	float4 pos = 0;
	#if FALLOFF_ENABLED
		if (_FalloffMode == 0)
			pos = mul(unity_ObjectToWorld, i.center);
		else
			pos = mul(unity_ObjectToWorld, i.vertex);
	#endif
	return pos;
}

float GetFalloff(v2f i){
	float dist = distance(GetCameraPos(), GetFalloffPosition(i));
	float falloff = smoothstep(_MaxRange, clamp(_MinRange, 0, _MinRange-0.001), dist);
	falloff *= smoothstep(clamp(_NearMinRange, 0, _NearMaxRange-0.001), _NearMaxRange, dist);
    return falloff;
}

float GetPulse(){
	float pulse = 1;
	UNITY_BRANCH
	switch (_Waveform){
		case 0: pulse = 0.5*(sin(_Time.y * _PulseSpeed)+1); break;
		case 1: pulse = round((sin(_Time.y * _PulseSpeed)+1)*0.5); break;
		case 2: pulse = abs((_Time.y * (_PulseSpeed * 0.333)%2)-1); break;
		case 3: pulse = frac(_Time.y * (_PulseSpeed * 0.2)); break;
		case 4: pulse = 1-frac(_Time.y * (_PulseSpeed * 0.2)); break;
		default: break;
	}
	return lerp(1, pulse, _PulseStr);
}

float4 GetColor(v2f i){

	#if FADING_ENABLED
		Softening(i, fade);
    	i.color.a *= fade;
	#endif

	audioLinkData al = (audioLinkData)0;
	#if AUDIOLINK_ENABLED
		InitializeAudioLink(al, 0);
		if (_AudioLinkOpacityStrength > 0){
			float alOpacity = GetAudioLinkBand(al, _AudioLinkOpacityBand, _AudioLinkRemapOpacityMin, _AudioLinkRemapOpacityMax);
			_Opacity *= lerp(1, alOpacity, _AudioLinkOpacityStrength * _AudioLinkStrength);
		}
	#endif

	i.color.a *= _Opacity;
    float4 col = 1;
	float4 tex = GetTexture(i, al);
	float falloff = 1;
	float pulse = 1;
	#if FALLOFF_ENABLED
		falloff = i.falloff;
	#endif
	#if PULSE_ENABLED
		pulse = i.pulse;
	#endif

	#if ALPHA_BLEND
		col = i.color * tex * _Color;
		col.a *= falloff;
		col.a *= pulse;
	#elif ALPHA_PREMULTIPLY
		col = i.color * tex * i.color.a * _Color;
		col *= falloff;
		col *= pulse;
	#elif ALPHA_ADD
		col = i.color * tex * _Color;
		col *= falloff;
		col *= pulse;
	#elif ALPHA_ADD_SOFT
		col = i.color * tex * _Color;
		col.rgb *= col.a;
		col *= falloff;
		col *= pulse;
	#elif ALPHA_MULTIPLY
		float4 prev = i.color * tex * _Color;
		col = lerp(float4(1,1,1,1), prev, prev.a * falloff * pulse);
	#elif ALPHA_MULTIPLYX2
		col.rgb = i.color.rgb * tex.rgb * _Color * 2;
		col.a = i.color.a * tex.a;
		col = lerp(float4(0.5,0.5,0.5,0.5), col, col.a * falloff * pulse);
	#endif

	#if FILTERING_ENABLED
		col.rgb = GetHSVFilter(col, al);
	#endif

    return col;
}

#endif // PS_FUNCTIONS_INCLUDED