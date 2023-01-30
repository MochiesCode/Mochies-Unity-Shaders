#ifndef USBRDF_INCLUDED
#define USBRDF_INCLUDED

float GetDetailRough(g2f i, float roughIn){
	float detailRough = MOCHIE_SAMPLE_TEX2D_SAMPLER(_DetailRoughnessMap, sampler_MainTex, i.uv2.xy);
	return BlendScalars(roughIn, detailRough, _DetailRoughBlending);
}

float3 BlurSample(float2 uv, float2 strength){
	float3 blurCol = 0;
	float2 uvb = uv;
	strength *= 0.015;
	#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		strength.y *= 0.5555555;
	#else
		strength.x *= 0.5625;
	#endif
	[unroll(16)]
	for (int j = 0; j < 16; j++){
		uvb = uv + (kernel[j] * strength);
		blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MUSGrab, uvb);
	}
	return blurCol / 16;
}

float GetRoughness(g2f i, masks m){
	float rough = 0;
	float dummy;
	#if DEFAULT_WORKFLOW
		rough = lerp(_Glossiness, MOCHIE_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, sampler_MainTex, i.uv.xy), _UseSpecMap);
		rough = lerp(rough, GetDetailRough(i, rough), _DetailRoughStrength * m.detailMask * _UsingDetailRough);
		rough = lerp(rough, Remap(rough, 0, 1, _RoughRemapMin, _RoughRemapMax), _RoughnessFiltering);
		ApplyPBRFiltering(rough, _RoughContrast, _RoughIntensity, _RoughLightness, _RoughnessFiltering, dummy);
	#elif SPECULAR_WORKFLOW
		float smooth = 0;
		float4 specMap = MOCHIE_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, sampler_MainTex, i.uv.xy);
		if (_PBRWorkflow == 1){
			if (_UseSmoothMap == 1){
				smooth = MOCHIE_SAMPLE_TEX2D_SAMPLER(_SmoothnessMap, sampler_MainTex, i.uv.xy).r * _GlossMapScale;
				smooth = lerp(smooth, Remap(smooth, 0, 1, _SmoothRemapMin, _SmoothRemapMax), _SmoothnessFiltering);
				ApplyPBRFiltering(smooth, _SmoothContrast, _SmoothIntensity, _SmoothLightness, _SmoothnessFiltering, dummy);
			}
			else smooth = _GlossMapScale;
		}
		else {
			smooth = specMap.a * _GlossMapScale;
			smooth = lerp(smooth, Remap(smooth, 0, 1, _SmoothRemapMin, _SmoothRemapMax), _SmoothnessFiltering);
			ApplyPBRFiltering(smooth, _SmoothContrast, _SmoothIntensity, _SmoothLightness, _SmoothnessFiltering, dummy);
		}
		rough = 1-smooth;
	#elif PACKED_WORKFLOW
		rough = ChannelCheck(packedTex, _RoughnessChannel);
		rough = lerp(rough, GetDetailRough(i, rough), _DetailRoughStrength * m.detailMask * _UsingDetailRough);
		rough = lerp(rough, Remap(rough, 0, 1, _RoughRemapMin, _RoughRemapMax), _RoughnessFiltering);
		ApplyPBRFiltering(rough, _RoughContrast, _RoughIntensity, _RoughLightness, _RoughnessFiltering, dummy);
	#endif

	return rough;
}

void ApplyRefraction(g2f i, lighting l, masks m, inout float3 albedo){
	#if !ADDITIVE_PASS
		float2 screenUV = GetGrabPos(i.grabPos);
		float2 IOR = (_RefractionIOR-1) * mul(UNITY_MATRIX_V, float4(l.normal, 0));
		float2 offset = ((1/(i.grabPos.z + 1) * IOR)) * (1-dot(l.normal, l.viewDir));
		offset = float2(offset.x, -(offset.y * _ProjectionParams.x));

		float2 refractUV = screenUV + offset;
		#if REFRACTION_CA_ENABLED
			float2 uvG = screenUV + (offset * (1 + _RefractionCAStr));
			float2 uvB = screenUV + (offset * (1 - _RefractionCAStr));
			float chromR = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MUSGrab, refractUV).r;
			float chromG = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MUSGrab, uvG).g;
			float chromB = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MUSGrab, uvB).b;
			float3 refractionCol = float3(chromR, chromG, chromB);
		#else
			float3 refractionCol = 0;
			UNITY_BRANCH
			if (_RefractionBlur == 1){
				float blurStrength = _RefractionBlurStrength;
				if (_RefractionBlurRough == 1){
					blurStrength *= GetRoughness(i, m);
				}
				refractionCol = BlurSample(refractUV, blurStrength);
			}
			else {
				refractionCol = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MUSGrab, refractUV);
			}
		#endif
		float alpha = step(m.refractDissolveMask, _RefractionDissolveMaskStr);
		refractionCol = lerp(albedo, refractionCol * _RefractionTint, alpha * m.refractMask);
		albedo = lerp(refractionCol, albedo, _RefractionOpac);
	#endif
}

#if SHADING_ENABLED

float GetFresnel(float VdotL, float width, float edge){
	float rim = pow((1-VdotL), (1-width) * 10);
	return smoothstep(edge, 1-edge, rim);
}

void ApplyIridescence(g2f i, lighting l, masks m, inout float3 col, float multiplier){
	float fresnel = GetFresnel(l.VdotL, _IridescenceWidth, _IridescenceEdge);
	float shiftMultiplier = multiplier*m.iridescenceMask*_IridescenceStrength;
	float hue = (fresnel + (_IridescenceHue * fresnel))*shiftMultiplier;
	col = HSVShift(col, frac(hue),0,0);
}

float GSAARoughness(float3 normal, float roughness){
	float3 normalDDX = ddx_fine(normal);
	float3 normalDDY = ddy_fine(normal); 
	float dotX = dot(normalDDX, normalDDX);
	float dotY = dot(normalDDY, normalDDY);
	float base = saturate(max(dotX, dotY));
	return max(roughness, pow(base, 0.333));
}

float GetRoughness(float smoothness){
	float rough = 1-smoothness;
    rough *= 1.7-0.7*rough;
    return rough;
}

bool SceneHasReflections(){
	float width, height;
	unity_SpecCube0.GetDimensions(width, height);
	return !(width * height < 2);
}

float3 BoxProjection(float3 dir, float3 pos, float4 cubePos, float3 boxMin, float3 boxMax){
    #if UNITY_SPECCUBE_BOX_PROJECTION
        UNITY_BRANCH
        if (cubePos.w > 0){
            float3 factors = ((dir > 0 ? boxMax : boxMin) - pos) / dir;
            float scalar = min(min(factors.x, factors.y), factors.z);
            dir = dir * scalar + (pos - cubePos);
        }
    #endif
    return dir;
}

float3 GetWorldReflections(float3 reflDir, float3 worldPos, float roughness){
    float3 baseReflDir = reflDir;
    reflDir = BoxProjection(reflDir, worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
    float4 envSample0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
    float3 p0 = DecodeHDR(envSample0, unity_SpecCube0_HDR);
    float interpolator = unity_SpecCube0_BoxMin.w;
    UNITY_BRANCH
    if (interpolator < 0.99999){
        float3 refDirBlend = BoxProjection(baseReflDir, worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
        float4 envSample1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, refDirBlend, roughness * UNITY_SPECCUBE_LOD_STEPS);
        float3 p1 = DecodeHDR(envSample1, unity_SpecCube1_HDR);
        p0 = lerp(p1, p0, interpolator);
    }
    return p0;
}

float3 GetReflections(g2f i, lighting l, masks m, float roughness){
    float3 reflections = 0;
	if (m.reflectionMask > 0){
		#if !CUBEMAP_REFLECTIONS
			#if REFLCUBE_EXISTS
				UNITY_BRANCH
				if (SceneHasReflections()){
					reflections = GetWorldReflections(l.reflectionDir, i.worldPos.xyz, roughness);
				}
				else {
					reflections = texCUBElod(_ReflCube, float4(l.reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
					reflections = DecodeHDR(float4(reflections,1), _ReflCube_HDR);
				}
			#else
				reflections = GetWorldReflections(l.reflectionDir, i.worldPos.xyz, roughness);
			#endif
		#else
			#if REFLCUBE_EXISTS
				reflections = texCUBElod(_ReflCube, float4(l.reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
				reflections = DecodeHDR(float4(reflections,1), _ReflCube_HDR);
			#endif
		#endif
		reflections *= l.ao;
		if (_ReflStepping == 1){
			roughness = saturate(roughness*2);
			float3 steppedCol = round(reflections * _ReflSteps)/_ReflSteps;
			reflections = lerp(steppedCol, reflections, roughness);
		}
	}
    return reflections;
}

float3 GetERimReflections(g2f i, lighting l, float roughness){
	float3 reflections = GetWorldReflections(l.reflectionDir, i.worldPos.xyz, lerp(roughness, _ERimRoughness, _ERimUseRough));
	reflections *= _ERimTint.rgb;
	reflections *= l.ao;
	return reflections;
}

void ApplyERimLighting(g2f i, lighting l, masks m, inout float3 diffuse, float roughness){
	if (m.eRimMask > 0){
		float3 reflCol = GetERimReflections(i, l, roughness);
		float rim = GetFresnel(l.VdotL, _ERimWidth, _ERimEdge) * m.eRimMask;
		float3 rimCol = reflCol * _ERimTint.rgb;
		float interpolator = rim*_ERimStr;
		diffuse = BlendColors(diffuse, rimCol, _ERimBlending, interpolator);
	}
}

float3 GetMatcapNormal(g2f i, lighting l, Texture2D tex, float strength, float2 scroll, float4 scaleOffset, int useNormalMap, int mixNormals){
	float3 matcapNormal = l.normal;
	if (useNormalMap){
		float2 uv = ApplyScaleOffset(i.rawUV, scaleOffset);
		uv += _Time.y*scroll;
		float3 normalMap = UnpackScaleNormal(MOCHIE_SAMPLE_TEX2D_SAMPLER(_MatcapNormal0, sampler_MainTex, uv), strength); 
		matcapNormal = normalize(normalMap.x * l.tangent + normalMap.y * l.binormal + normalMap.z * i.normal);
		if (mixNormals){
			matcapNormal = BlendNormals(matcapNormal, l.normal);
		}
	}
	return matcapNormal;
}

float2 GetMatcapUV(float3 viewDir, float3 normal){
	float3 worldViewUp = normalize(float3(0,1,0) - viewDir * dot(viewDir, float3(0,1,0)));
	float3 worldViewRight = normalize(cross(viewDir, worldViewUp));
	return float2(dot(worldViewRight, normal), dot(worldViewUp, normal)) * 0.5 + 0.5;
}

void ApplyMatcap(g2f i, lighting l, masks m, inout float3 environment, float roughness){

	float3 vd0 = lerp(l.viewDir, l.viewDirVR, _MatcapCenter);
	float3 matcapNormal0 = GetMatcapNormal(i, l, _MatcapNormal0, _MatcapNormal0Str, _MatcapNormal0Scroll, _MatcapNormal0_ST, _MatcapNormal0Toggle, _MatcapNormal0Mix);
	float2 matcapUV0 = GetMatcapUV(vd0, matcapNormal0);
	float isUnlit0 = lerp(l.worldBrightness, 1, _UnlitMatcap);
	float lod0 = lerp(roughness, _MatcapRough, _MatcapUseRough) * UNITY_SPECCUBE_LOD_STEPS;
	float2 uv0 = (matcapUV0 * _Matcap_ST.xy) + (uvOffsetOut.xy*20*_DistortMatcap0);
	float4 matcap = MOCHIE_SAMPLE_TEX2D_SAMPLER_LOD(_Matcap, sampler_MainTex, uv0, lod0) * _MatcapColor;
	matcap.rgb *= _MatcapStr * m.matcapPrimMask * isUnlit0;

	if (_UseMatcap1 == 1){
		float3 vd1 = lerp(l.viewDir, l.viewDirVR, _MatcapCenter1);
		float3 matcapNormal1 = GetMatcapNormal(i, l, _MatcapNormal1, _MatcapNormal1Str, _MatcapNormal1Scroll, _MatcapNormal1_ST, _MatcapNormal1Toggle, _MatcapNormal1Mix);
		float2 matcapUV1 = GetMatcapUV(vd1, matcapNormal1);
		float isUnlit1 = lerp(l.worldBrightness, 1, _UnlitMatcap1);
		float lod1 = lerp(roughness, _MatcapRough1, _MatcapUseRough1) * UNITY_SPECCUBE_LOD_STEPS;
		float2 uv1 = (matcapUV1 * _Matcap1_ST.xy) + (uvOffsetOut.xy*20*_DistortMatcap1);
		float4 matcap1 = MOCHIE_SAMPLE_TEX2D_SAMPLER_LOD(_Matcap1, sampler_MainTex, uv1, lod1) * _MatcapColor1;
		matcap1.rgb *= _MatcapStr1 * m.matcapSecMask * isUnlit1;
		environment = lerp(environment + matcap1.rgb, environment + (matcap1.rgb*matcap1.a), _MatcapBlending1);
	}

	environment = lerp(environment + matcap.rgb, environment + (matcap.rgb*matcap.a), _MatcapBlending);
}

void ApplyDithering(g2f i, inout float3 ramp){
	if (_DitheredShadows == 1){
		float slice = Average(ramp) * 0.9375;
		float3 dither = tex3D(_DitherMaskLOD, float3(i.pos.xy*0.25, slice)).a - 0.01;
		ramp = lerp(dither, 1, ramp);
	}
}

float3 GetForwardRamp(g2f i, lighting l, masks m, float atten){
	float3 ramp = 1;
	UNITY_BRANCH
	if (_ShadowMode == 1){
		l.NdotL *= MOCHIE_SAMPLE_TEX2D_SAMPLER(_DetailShadowMap, sampler_MainTex, i.rawUV);
		if (!l.lightEnv || _RTSelfShadow == 1){
			atten = lerp(atten, smootherstep(0,1,atten), _AttenSmoothing);
			l.NdotL *= atten;
		}
		_RampPos = lerp(_RampPos, saturate(_RampPos), l.lightEnv);
		float3 ramp0 = linearstep(0, _RampWidth0, l.NdotL-_RampPos);
		float3 ramp1 = linearstep(0, _RampWidth1, l.NdotL-_RampPos);
		ramp = lerp(ramp0, ramp1, _RampWeight);
		ramp = lerp(1, ramp, _ShadowStr*m.shadowMask); 
		ramp = lerp(_ShadowTint.rgb, 1, ramp);
		ApplyDithering(i, ramp);
		ramp = lerp3(ramp, lerp(1, ramp, l.lightEnv), lerp(ramp,1,l.lightEnv), _ShadowConditions);
	}
	else if (_ShadowMode == 2){
		l.NdotL *= MOCHIE_SAMPLE_TEX2D_SAMPLER(_DetailShadowMap, sampler_MainTex, i.rawUV);
		if (!l.lightEnv || _RTSelfShadow == 1){
			atten = lerp(atten, smootherstep(0,1,atten), _AttenSmoothing);
			l.NdotL *= atten;
		}
		float rampUV = l.NdotL * 0.5 + 0.5;
		ramp = MOCHIE_SAMPLE_TEX2D(_ShadowRamp, rampUV.xx).rgb;
		ramp = lerp(1, ramp, _ShadowStr*m.shadowMask);
		ApplyDithering(i, ramp);
		ramp = lerp3(ramp, lerp(1, ramp, l.lightEnv), lerp(1, ramp, !l.lightEnv), _ShadowConditions);
	}
	return ramp;
}

float3 GetAddRamp(g2f i, lighting l, masks m, float shadows, float atten){
	float3 ramp = atten;
	if (_ShadowConditions != 2){
		l.NdotL *= MOCHIE_SAMPLE_TEX2D_SAMPLER(_DetailShadowMap, sampler_MainTex, i.rawUV);
		if (_ShadowMode == 1){
			float3 ramp0 = linearstep(0, _RampWidth0, l.NdotL-_RampPos);
			float3 ramp1 = linearstep(0, _RampWidth1, l.NdotL-_RampPos);
			ramp = lerp(ramp0, ramp1, _RampWeight);
			ramp = lerp(_ShadowTint.rgb, 1, ramp * atten * shadows);
			ApplyDithering(i, ramp);
		}
		else if (_ShadowMode == 2){
			float rampUV = l.NdotL * 0.5 + 0.5;
			ramp = MOCHIE_SAMPLE_TEX2D(_ShadowRamp, rampUV.xx).rgb;
			ramp = lerp(1, ramp, _ShadowStr*m.shadowMask);
			ramp *= shadows * atten;
			ApplyDithering(i, ramp);
		}
	}
	return lerp(atten, ramp, _ShadowStr);
}

float3 GetSubsurfaceLight(g2f i, lighting l, masks m, float3 albedo, float3 atten){
	float3 sss = 0;
	#if SUBSURFACE_ENABLED
		if (m.subsurfMask > 0){
			float3 vLTLight = l.lightDir + l.normal * _ScatterDist; // Distortion
			float3 fLTDot = pow(saturate(dot(l.viewDir, -vLTLight)), _ScatterPow) * _ScatterIntensity * 1.0/UNITY_PI; 
			float3 subsurfaceColor = MOCHIE_SAMPLE_TEX2D_SAMPLER(_ScatterTex, sampler_MainTex, i.uv.xy) * _ScatterCol;
			float thickness = MOCHIE_SAMPLE_TEX2D_SAMPLER(_ThicknessMap, sampler_MainTex, i.uv.xy);
			subsurfaceColor *= lerp(1, albedo, _ScatterBaseColorTint);
			thickness = pow(1-thickness, _ThicknessMapPower);
			sss = (lerp(1, atten, float(any(_WorldSpaceLightPos0.xyz)))
						* (fLTDot + _ScatterAmbient) * thickness
						* (l.directCol + l.indirectCol) * subsurfaceColor) * m.subsurfMask;
		}
	#endif
    return sss; 
}

float3 FresnelLerp(float3 specCol, float3 grazingTerm, float NdotV){
    float t = Pow5(1 - NdotV);
    return lerp(specCol, grazingTerm, t);
}


float3 FresnelTerm(float3 specCol, float LdotH){
    float t = Pow5(1 - LdotH);
    return specCol + (1-specCol) * t;
}

// Implementation from Google Filament
// https://google.github.io/filament/Filament.md.html#lighting/imagebasedlights/anisotropy
float3 GetAnisoReflDir(lighting l){
	float anisotropy = 1;
	float3 anisotropicDirection = anisotropy >= 0.0 ? l.binormal : l.tangent;
	float3 anisotropicTangent = cross(anisotropicDirection, l.viewDir);
	float3 anisotropicNormal = cross(anisotropicTangent, anisotropicDirection);
	float3 bentNormal = normalize(lerp(l.normal, anisotropicNormal, anisotropy));
	float3 r = reflect(-l.viewDir, bentNormal);
	return r;
}

// 	Implementation from Google Filament
// 	https://google.github.io/filament/Filament.md.html#materialsystem/anisotropicmodel/anisotropicspecularbrdf
float GetAnisoTerm(g2f i, lighting l, float roughness){
	float anisotropy = 1;
	float ToV = dot(l.tangent, l.viewDir);
	float BoV = dot(l.binormal, l.viewDir);
	float ToL = dot(l.tangent, l.lightDir);
	float BoL = dot(l.binormal, l.lightDir);
	float NoV = dot(l.normal, l.viewDir);
	float NoL = dot(l.normal, l.lightDir);
	float at = max(roughness * (1.0 + anisotropy), 0.001);
	float ab = max(roughness * (1.0 - anisotropy), 0.001);
	float lambdaV = NoL * length(float3(at * ToV, ab * BoV, NoV));
	float lambdaL = NoV * length(float3(at * ToL, ab * BoL, NoL));
	float visibilityTerm = 0.5 / (lambdaV + lambdaL);
	return visibilityTerm;
}

float GetAnisoTerm(g2f i, lighting l, masks m){
	_RippleAmplitude = abs(1-_RippleAmplitude)+1.01;
	float rippleValue = i.rawUV.z*_RippleFrequency*120;
	float ripple = Average(float4(sin(rippleValue), sin(rippleValue*5.213), sin(rippleValue*8.7622), sin(rippleValue*12.9)));
	float continuous = (0.01/((ripple+_RippleAmplitude)/(2*_RippleAmplitude)))*50;
	float segmented = ripple * _RippleAmplitude;
	ripple = lerp(segmented, continuous, _RippleContinuity);
	ripple = lerp(1, ripple, _RippleStrength);
	_AnisoAngleY *= ripple * 0.0025;
	_AnisoLayerY *= ripple * 0.05;

	float f0 = l.TdotH * l.TdotH / 1 + l.BdotH * l.BdotH / (_AnisoAngleY * _AnisoAngleY) + l.NdotH * l.NdotH;
	float f1 = l.TdotH * l.TdotH / 1 + l.BdotH * l.BdotH / (_AnisoAngleY * _AnisoLayerY) + l.NdotH * l.NdotH;
	float layer0 = saturate(1.0 / (_AnisoAngleY * f0 * f0));
	float layer1 = saturate(1.0 / (_AnisoAngleY * f1 * f1));
	float visibilityTerm = 1;

	if (_AnisoLerp == 1)
		visibilityTerm = lerp(layer1*_AnisoLayerStr, layer0, layer0);
	else
		visibilityTerm = saturate(layer0 + (layer1*_AnisoLayerStr));

	return visibilityTerm;
}

float GetGGXTerm(lighting l, float roughness){
	float visibilityTerm = 0;
	if (l.NdotL > 0){
		float rough = roughness;
		float rough2 = roughness * roughness;

		// Originally used this because it's "mathematically correct" according to unity standard
		// but it actually creates NaNs due to how I'm handling NdotL
		// float lambdaV = l.NdotL * sqrt((-l.NdotV * rough2 + l.NdotV) * l.NdotV + rough2);
		// float lambdaL = l.NdotV * sqrt((-l.NdotL * rough2 + l.NdotL) * l.NdotL + rough2);

		float lambdaV = l.NdotL * (l.NdotV * (1 - rough) + rough);
		float lambdaL = l.NdotV * (l.NdotL * (1 - rough) + rough);

		visibilityTerm = 0.5f / (lambdaV + lambdaL + 1e-5f);
		float d = (l.NdotH * rough2 - l.NdotH) * l.NdotH + 1.0f;
		float dotTerm = UNITY_INV_PI * rough2 / (d * d + 1e-7f);

		visibilityTerm *= dotTerm * UNITY_PI;
	}
	return visibilityTerm;
}

void GetSpecFresTerm(g2f i, lighting l, masks m, inout float3 specularTerm, inout float3 fresnelTerm, float3 specCol, float roughness, inout int steps){

	// GGX
	#if GGX_SPECULAR
		specularTerm = GetGGXTerm(l, roughness) * _SpecStr; 
		fresnelTerm = FresnelTerm(specCol, l.LdotH);
		steps = _SharpSpecStr;

	// Anisotropic
	#elif ANISO_SPECULAR
		specularTerm = GetAnisoTerm(i, l, m) * _AnisoStr;
		fresnelTerm = FresnelTerm(specCol, l.LdotH);
		fresnelTerm = lerp(1, fresnelTerm, metallic);
		steps = _AnisoSteps;

	// Combined
	#elif COMBINED_SPECULAR
		float ggx = GetGGXTerm(l, roughness);
		float aniso = GetAnisoTerm(i, l, m);
		specularTerm = lerp(aniso * _AnisoStr, ggx * _SpecStr, 1-m.anisoMask);
		fresnelTerm = FresnelTerm(specCol, l.LdotH);
		fresnelTerm = lerp(lerp(1, fresnelTerm, metallic), fresnelTerm, 1-m.anisoMask);
		steps = lerp(_AnisoSteps, _SharpSpecStr, 1-m.anisoMask);
	#endif

	specularTerm = max(0, specularTerm * max(0.00001, l.NdotL));
}

float DisneyDiffuse(lighting l, masks m, float percepRough) {
	float dd = 1;
	#if !OUTLINE_PASS
		float fd90 = 0.5 + 2 * l.LdotH * l.LdotH * percepRough;
		float viewScatter = (1 + (fd90 - 1) * Pow5(1 - l.NdotV));
		dd = lerp(1, viewScatter, _DisneyDiffuse * m.diffuseMask);
	#endif
	return dd;
}

float3 GetMochieBRDF(g2f i, lighting l, masks m, float4 diffuse, float4 albedo, float3 specCol, float3 reflCol, float omr, float smoothness, float3 atten){
	float percepRough = 1-smoothness;
	float brdfRoughness = percepRough * percepRough;
	brdfRoughness = max(brdfRoughness, 0.002);
	float3 subsurfCol = GetSubsurfaceLight(i, l, m, atten, albedo.rgb);

	l.directCol *= atten;
	l.directCol += l.vLightCol;

	float diffuseTerm = DisneyDiffuse(l, m, percepRough);
	float3 lighting = l.indirectCol + l.directCol * diffuseTerm;
	float3 specular = 0;
	float3 reflections = 0;
	float3 environment = 0;

	// Specular
	#if SPECULAR_ENABLED
		#if !ADDITIVE_PASS
		if (!(!l.lightEnv && _RealtimeSpec == 1)){
		#endif
		if (m.specularMask > 0){
			float3 fresnelTerm = 1;
			float3 specularTerm = 1;
			float3 specBiasCol = lerp(specCol, albedo, _SpecBiasOverride*_SpecBiasOverrideToggle);
			float specRough = lerp(brdfRoughness, _SpecRough, _SpecUseRough);
			GetSpecFresTerm(i, l, m, specularTerm, fresnelTerm, specBiasCol, specRough, _SharpSpecStr);
			specular = lerp(lighting, 1, _ManualSpecBright) * specularTerm * fresnelTerm * m.specularMask * _SpecCol * l.ao * atten;
			if (_SharpSpecular == 1){
				float specInterp = smoothstep(0.5, 1, saturate(specRough*8));
				float sharpSpec = floor(specular * _SharpSpecStr) / _SharpSpecStr;
				specular = lerp(sharpSpec, specular, 0);
			}
		}
		#if !ADDITIVE_PASS
		}
		#endif
	#endif

	// Reflections
	// Lighting based IOR from Retro's standard mod
	#if REFLECTIONS_ENABLED && FORWARD_PASS			
		if (m.reflectionMask > 0){										
			float surfaceReduction = (1.0 / (brdfRoughness*brdfRoughness + 1.0)) * saturate(lerp(1, (l.NdotL + 0.3), _LightingBasedIOR));
			float grazingTerm = saturate(smoothness + (1-omr));
			reflections = surfaceReduction * reflCol * FresnelLerp(specCol, grazingTerm, lerp(1, l.NdotV, _FresnelStrength*_FresnelToggle));
			#if SSR_ENABLED
				float4 SSRColor = GetSSRColor(i.worldPos, l.viewDir, l.reflectionDir, normalize(i.normal), smoothness, albedo, metallic, m.reflectionMask, l.screenUVs, i.grabPos);
				reflections = lerp(reflections, SSRColor.rgb, SSRColor.a);
			#endif
			reflections *= m.reflectionMask * _ReflectionStr;
		}
	#endif


	environment = specular + reflections;
	if (_Iridescence == 1){
		ApplyIridescence(i, l, m, environment, metallic);
	}
	environment += subsurfCol;

	#if MATCAP_ENABLED
		ApplyMatcap(i, l, m, environment, GetRoughness(smoothness));
	#endif

	// #if REFRACTION_ENABLED
	// 	float3 col = diffuse.rgb * lerp(lighting, 1, step(m.refractDissolveMask, _RefractionDissolveMaskStr) * (1-_RefractionOpac) * m.refractMask);
	// #else
		float3 col = diffuse.rgb * lighting;
	// #endif

	// Prevents being washed out by intense lighting
	float3 maxCol = (diffuse.rgb + environment) * diffuseTerm;
	col = lerp(col, clamp(col, 0, maxCol), _ColorPreservation);

	#if REFRACTION_ENABLED
		if (_UnlitRefraction == 0)
			ApplyRefraction(i, l, m, col);
	#endif

    return col + environment;
}
#else

float4 GetDiffuse(lighting l, float4 albedo, float atten){
    float4 diffuse;
    float3 lightCol = atten * l.directCol + l.indirectCol + l.vLightCol;
    diffuse.rgb = albedo.rgb;
    diffuse.rgb *= lightCol;
	diffuse.rgb = clamp(diffuse.rgb, 0, albedo.rgb);
    diffuse.a = albedo.a;
    return diffuse;
}

#endif // SHADING_ENABLED

#endif // BRDF_INCLUDED