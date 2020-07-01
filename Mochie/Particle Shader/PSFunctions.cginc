#ifndef P_FUNCS_INCLUDED
#define P_FUNCS_INCLUDED

void Softening(v2f i, inout float fade){
    fade = 1;
	#if defined(_FADING_ON)
        float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
        float partZ = i.projPos.z;
        fade = saturate((1-_SoftenStr) * (sceneZ-partZ));
    #endif
}

float4 GetTexture(v2f i){ 

	#if defined(PSX) && defined(EFFECT_BUMP)
		float4 texCol = tex2D(_MainTex, i.uv0.xy);
		ApplyDistortion(i, texCol.a);
		if (_DistortMainTex == 1)
			texCol = tex2D(_MainTex, i.uv0.xy);
		float4 grabCol = float4(tex2Dproj(_PSGrab, i.uv1).rgb, texCol.a);
		texCol = lerp(texCol, grabCol*lerp(1,texCol.a,_BlendMode == 1), _DistortionBlend);
	#else
		float4 texCol = tex2D(_MainTex, i.uv0.xy) * _Color;
	#endif

	if (_FlipbookBlending == 1){
		float4 blendedTex = tex2D(_MainTex, i.uv0.xy) * _Color;
		texCol = lerp(texCol, blendedTex, i.uv0.z);
	}
    if (_IsCutout == 1)
        clip(texCol.a - _Cutout);
	
	#if defined(PSX)
		texCol = ApplyLayeredTex(i, texCol);
	#endif
    return texCol;
}

float4 GetProjPos(float4 vertex0, float4 vertex1){
    float4 projPos = 0;
    #if defined(_FADING_ON)
        projPos = ComputeScreenPos(vertex1);
		projPos.z = -UnityObjectToViewPos(vertex0).z;
    #endif
    return projPos;
}

float GetFalloff(float4 vertex){
    float falloff = 1;
    if (_Falloff == 1){
        float dist = distance(GetCameraPos(), mul(unity_ObjectToWorld, vertex));
        falloff = smoothstep(_MaxRange, clamp(_MinRange, 0, _MinRange-0.001), dist);
        falloff *= smoothstep(clamp(_NearMinRange, 0, _NearMaxRange-0.001), _NearMaxRange, dist);
    }
    return falloff;
}

float GetPulse(){
	float pulse = 1;
	if (_Pulse == 1){
		UNITY_BRANCH
		switch (_Waveform){
			case 0: pulse = 0.5*(sin(_Time.y * _PulseSpeed)+1); break;
			case 1: pulse = round((sin(_Time.y * _PulseSpeed)+1)*0.5); break;
			case 2: pulse = abs((_Time.y * (_PulseSpeed * 0.333)%2)-1); break;
			case 3: pulse = frac(_Time.y * (_PulseSpeed * 0.2)); break;
			case 4: pulse = 1-frac(_Time.y * (_PulseSpeed * 0.2)); break;
			default: break;
		}
	}
	return lerp(1, pulse, _PulseStr);
}

float4 GetColor(v2f i){
	Softening(i, fade);
    i.color.a *= fade;
    float4 col = 1;

    [forcecase]
	switch (_BlendMode){

		// Alpha Blended
		case 0:
			col = i.color * GetTexture(i);
			col.a = saturate(col.a);
			col.a *= i.falloff;
			col *= _Color;
			col *= i.pulse;
			#if defined(PSX)
				col.rgb = GetHSVFilter(col);
			#endif
			break;

		// Premultiplied
		case 1: 
			col = i.color * GetTexture(i) * i.color.a;
			col *= i.falloff;
			col *= _Color;
			col *= i.pulse;
			#if defined(PSX)
				col.rgb = GetHSVFilter(col);
			#endif
			break;

		// Additive
		case 2:
			col = 2.0 * i.color * GetTexture(i);
			col.a = saturate(col.a);
			col.a *= i.falloff;
			col *= _Color;
			#if defined(PSX)
				col.rgb = GetHSVFilter(col);
			#endif
			break;
		
		// Soft Additive
		case 3: 
			col = i.color * GetTexture(i);
			col.rgb *= col.a;
			col *= i.falloff;
			col *= _Color;
			col.rgb *= i.pulse;
			#if defined(PSX)
				col.rgb = GetHSVFilter(col);
			#endif
			break;
		
		// Multiply
		case 4:
			float4 prev = i.color * GetTexture(i);
			col = lerp(float4(1,1,1,1), prev, prev.a*i.falloff);
			col *= _Color;
			#if defined(PSX)
				col.rgb = GetHSVFilter(col);
			#endif
			break;
		
		// Multiply x2
		case 5:
			float4 tex = GetTexture(i);
			col.rgb = tex.rgb * i.color.rgb * 2;
			col.a = i.color.a * tex.a;
			col = lerp(float4(0.5,0.5,0.5,0.5), col, col.a*i.falloff);
			col *= _Color;
			#if defined(PSX)
				col.rgb = GetHSVFilter(col);
			#endif
			break;
		
		default: break;
	}
    return col;
}

#endif