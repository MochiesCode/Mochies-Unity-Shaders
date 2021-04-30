#ifndef PS_FUNCTIONS_INCLUDED
#define PS_FUNCTIONS_INCLUDED

float4 ApplyLayeredTex(v2f i, float4 texCol){
	float alpha = _BlendMode == 1 ? texCol.a : 1;
	float4 secondTexCol = tex2D(_SecondTex, i.uv0.xy) * _SecondColor;
	switch (_TexBlendMode){
		case 0: texCol = lerp(secondTexCol*alpha, texCol, texCol.a); break;
		case 1: texCol.rgb += secondTexCol.rgb*alpha; break;
		case 2: texCol.rgb -= secondTexCol.rgb; break;
		case 3: texCol.rgb *= secondTexCol.rgb; break;
		default: break;
	}
    return texCol;
}

void ApplyDistortion(inout v2f i, float alpha){
	float2 duv = i.uv0.xy - (_Time.y*_DistortionSpeed);
	duv *= _NormalMapScale;
	float2 normal = UnpackNormal(tex2D(_NormalMap, duv)).rg;
	float2 offset = normal * alpha * _DistortionStr * ((i.color.r + i.color.b + i.color.g)/3.0);
	#if FADING_ENABLED
		offset *= fade;
	#endif
	i.uv1.xy += offset;
	#if DISTORTION_UV_ENABLED
		i.uv0.xy += offset;
	#endif
}


float3 GetHSVFilter(float4 col){
	_Hue += lerp(0, frac(_Time.y*_AutoShiftSpeed), _AutoShift);
	float3 filteredCol = HSVShift(col.rgb, _Hue, 0, 0);
	filteredCol = GetSaturation(filteredCol, _Saturation);
	filteredCol = lerp(filteredCol, GetHDR(filteredCol), _HDR);
	filteredCol = GetContrast(filteredCol, _Contrast);
	col.rgb = lerp(col.rgb, filteredCol, col.a);
	col.rgb *= _Brightness;
    return col;
}

void Softening(v2f i, inout float fade){
	float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
	float partZ = i.projPos.z;
	fade = saturate((1-_SoftenStr) * (sceneZ-partZ));
}

float4 GetTexture(v2f i){ 

	#if DISTORTION_ENABLED
		float4 texCol = tex2D(_MainTex, i.uv0.xy);
		ApplyDistortion(i, texCol.a);
		#if DISTORTION_UV_ENABLED
			texCol = tex2D(_MainTex, i.uv0.xy);
		#endif
		float4 grabCol = float4(tex2Dproj(_GrabTexture, i.uv1).rgb, texCol.a);
		texCol = lerp(texCol, grabCol*lerp(1,texCol.a,_BlendMode == 1), _DistortionBlend);
	#else
		float4 texCol = tex2D(_MainTex, i.uv0.xy);
	#endif

	#if FLIPBOOK_BLEND_ENABLED
		float4 blendedTex = tex2D(_MainTex, i.uv0.xy);
		texCol = lerp(texCol, blendedTex, i.uv0.z);
	#endif

    #if ALPHA_TEST_ENABLED
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
	if (_FalloffMode == 0)
		pos = mul(unity_ObjectToWorld, i.center);
	else
		pos = mul(unity_ObjectToWorld, i.vertex);
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

	i.color.a *= _Opacity;
    float4 col = 1;
	float4 tex = GetTexture(i);

	#if ALPHA_BLEND
		col = i.color * tex * _Color;
		col.a *= i.falloff;
		col.a *= i.pulse;
	#elif ALPHA_PREMULTIPLY
		col = i.color * tex * i.color.a * _Color;
		col *= i.falloff;
		col *= i.pulse;
	#elif ALPHA_ADD
		col = i.color * tex * _Color;
		col *= i.falloff;
		col *= i.pulse;
	#elif ALPHA_ADD_SOFT
		col = i.color * tex * _Color;
		col.rgb *= col.a;
		col *= i.falloff;
		col *= i.pulse;
	#elif ALPHA_MULTIPLY
		float4 prev = i.color * tex * _Color;
		col = lerp(float4(1,1,1,1), prev, prev.a * i.falloff * i.pulse);
	#elif ALPHA_MULTIPLYX2
		col.rgb = i.color.rgb * tex.rgb * _Color * 2;
		col.a = i.color.a * tex.a;
		col = lerp(float4(0.5,0.5,0.5,0.5), col, col.a * i.falloff * i.pulse);
	#endif

	#if FILTERING_ENABLED
		col.rgb = GetHSVFilter(col);
	#endif

    return col;
}

#endif // PS_FUNCTIONS_INCLUDED