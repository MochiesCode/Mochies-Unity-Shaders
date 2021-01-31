#ifndef USBRDF_INCLUDED
#define USBRDF_INCLUDED

#if SHADING_ENABLED

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

float3 GetReflections(g2f i, lighting l, float roughness){
    float3 reflections = 0;
	#if !CUBEMAP_REFLECTIONS
		#if REFLCUBE_EXISTS
			UNITY_BRANCH
			if (SceneHasReflections()){
				reflections = GetWorldReflections(l.reflectionDir, i.worldPos.xyz, roughness);
			}
			else {
				reflections = texCUBElod(_ReflCube, float4(l.reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS))*l.worldBrightness;
				reflections = DecodeHDR(float4(reflections,1), _ReflCube_HDR);
			}
		#else
			reflections = GetWorldReflections(l.reflectionDir, i.worldPos.xyz, roughness);
		#endif
	#else
		#if REFLCUBE_EXISTS
			reflections = texCUBElod(_ReflCube, float4(l.reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS))*l.worldBrightness;
			reflections = DecodeHDR(float4(reflections,1), _ReflCube_HDR);
		#endif
	#endif
	reflections *= l.ao;
	if (_ReflStepping == 1){
		roughness = saturate(roughness*2);
		float3 steppedCol = round(reflections * _ReflSteps)/_ReflSteps;
		reflections = lerp(steppedCol, reflections, roughness);
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
	float3 reflCol = GetERimReflections(i, l, roughness);
	float VdotL = abs(dot(l.viewDir, l.normal));
	float rim = pow((1-VdotL), (1-_ERimWidth) * 10);
	rim = smoothstep(_ERimEdge, (1-_ERimEdge), rim);
	rim *= m.eRimMask;
	float3 rimCol = reflCol * _ERimTint.rgb;
	float interpolator = rim*_ERimStr;

	[flatten]
	switch (_ERimBlending){
		case 0: diffuse = lerp(diffuse, rimCol, interpolator); break;
		case 1: diffuse += rimCol*interpolator; break;
		case 2: diffuse -= rimCol*interpolator; break;
		case 3: diffuse *= lerp(1, rimCol, interpolator); break;
	}
}

void ApplyMatcap(g2f i, lighting l, masks m, inout float3 environment, float roughness){
	float3 worldViewUp = normalize(float3(0,1,0) - l.viewDir * dot(l.viewDir, float3(0,1,0)));
	float3 worldViewRight = normalize(cross(l.viewDir, worldViewUp));
	float2 matcapUV = float2(dot(worldViewRight, l.normal), dot(worldViewUp, l.normal)) * 0.5 + 0.5;

	float isUnlit0 = lerp(l.worldBrightness, 1, _UnlitMatcap);
	float lod0 = lerp(roughness, _MatcapRough, _MatcapUseRough) * UNITY_SPECCUBE_LOD_STEPS;
	float2 uv0 = (matcapUV * _Matcap_ST.xy) + (uvOffsetOut.xy*20*_DistortMatcap0);
	float4 matcap = UNITY_SAMPLE_TEX2D_LOD_SAMPLER(_Matcap, _MainTex, float4(uv0,0,lod0)) * _MatcapColor;
	matcap.rgb *= _MatcapStr * m.matcapMask * isUnlit0;

	float isUnlit1 = lerp(l.worldBrightness, 1, _UnlitMatcap1);
	float lod1 = lerp(roughness, _MatcapRough1, _MatcapUseRough1) * UNITY_SPECCUBE_LOD_STEPS;
	float2 uv1 = (matcapUV * _Matcap1_ST.xy) + (uvOffsetOut.xy*20*_DistortMatcap1);
	float4 matcap1 = UNITY_SAMPLE_TEX2D_LOD_SAMPLER(_Matcap1, _MainTex, float4(uv1,0,lod1)) * _MatcapColor1;
	matcap1.rgb *= _MatcapStr1 * m.matcapMask * isUnlit1;

	if (_UseMatcap1 == 1){
		float blendMask = 1-m.matcapBlendMask;
		float3 blend10 = matcap1.rgb * blendMask;
		float3 blend11 = matcap1.rgb * matcap1.a * blendMask;
		environment = lerp(environment + blend10, environment + blend11, _MatcapBlending1);
	}

	float3 blend00 = matcap.rgb * m.matcapBlendMask;
	float3 blend01 = matcap.rgb * matcap.a * m.matcapBlendMask;
	environment = lerp(environment + blend00, environment + blend01, _MatcapBlending);
}

void ApplyDithering(g2f i, inout float3 ramp){
	if (_DitheredShadows == 1){
		float slice = Average(ramp) * 0.9375;
		float3 dither = tex3D(_DitherMaskLOD, float3(i.pos.xy*0.25, slice)).a - 0.01;
		ramp = lerp(dither, 1, ramp);
	}
}

float3 GetForwardRamp(g2f i, lighting l, masks m, float3 albedo, float3 atten){
	float3 ramp = 1;
	if (_ShadowMode == 1){
		float3 tint = _ShadowTint.rgb;
		if (!l.lightEnv || _RTSelfShadow == 1){
			atten = lerp(atten, smootherstep(0,1,atten), _AttenSmoothing);
			l.NdotL *= atten;
		}
		float3 ramp0 = linearstep(0, _RampWidth0, l.NdotL-_RampPos);
		float3 ramp1 = linearstep(0, _RampWidth1, l.NdotL-_RampPos);
		ramp = lerp(ramp0, ramp1, _RampWeight);
		
		ramp = lerp(1, ramp, _ShadowStr*m.shadowMask); 
		ramp = lerp(tint, 1, ramp);
		ApplyDithering(i, ramp);
		ramp = lerp3(ramp, lerp(1, ramp, l.lightEnv), lerp(ramp,1,l.lightEnv), _ShadowConditions);
	}
	else if (_ShadowMode == 2){
		if (!l.lightEnv || _RTSelfShadow == 1){
			atten = lerp(atten, smootherstep(0,1,atten), _AttenSmoothing);
			l.NdotL *= atten;
		}
		float rampUV = l.NdotL * 0.5 + 0.5;
		ramp = tex2D(_ShadowRamp, rampUV.xx).rgb;
		ramp = lerp(1, ramp, _ShadowStr*m.shadowMask);
		ApplyDithering(i, ramp);
		ramp = lerp3(ramp, lerp(1, ramp, l.lightEnv), lerp(1, ramp, !l.lightEnv), _ShadowConditions);
	}
	return ramp;
}

float3 GetAddRamp(g2f i, lighting l, masks m, float3 albedo, float3 atten){
	float3 ramp = 1;
	if (_ShadowMode == 0){
		ramp = smoothstep(0, 0.005, l.NdotL) * atten;
	}
	else if (_ShadowMode == 1){
		float3 tint = _ShadowTint.rgb;
		float3 ramp0 = linearstep(0, _RampWidth0, l.NdotL-_RampPos);
		float3 ramp1 = linearstep(0, _RampWidth1, l.NdotL-_RampPos);
		ramp = lerp(ramp0, ramp1, _RampWeight) * atten;
		ramp = lerp(tint*atten, 1, ramp);
		ApplyDithering(i, ramp);
	}
	else {
		float rampUV = l.NdotL * 0.5 + 0.5;
		ramp = tex2D(_ShadowRamp, rampUV.xx).rgb;
		ramp = lerp(1, ramp, _ShadowStr*m.shadowMask) * atten;
		ApplyDithering(i, ramp);
	}
	return ramp;
}

float3 GetRamp(g2f i, lighting l, masks m, float3 albedo, float3 atten){
	float3 ramp = 1;
	#if FORWARD_PASS
		ramp = GetForwardRamp(i, l, m, albedo, atten);
	#else
		ramp = GetAddRamp(i, l, m, albedo, atten);
	#endif
	return ramp;
}

float3 GetSSS(g2f i, lighting l, masks m, float3 albedo, float3 atten){
    float3 sss = 0;
	#if !OUTLINE_PASS
		if (_Subsurface == 1){
			_SPen = 1-_SPen;
			float thickness = 1 - UNITY_SAMPLE_TEX2D_SAMPLER(_TranslucencyMap, _MainTex, i.uv.xy);
			float3 subCol = UNITY_SAMPLE_TEX2D_SAMPLER(_SubsurfaceTex, _MainTex, i.uv.xy) * _SColor;
			float ndl = smoothstep(_SPen-_SSharp, _SPen+_SSharp, l.NdotL);
			atten = lerp(1, saturate(ndl * atten), _SAtten);
			float3 vLTLight = l.directCol * l.normalDir;
			float fLTDot = DotClamped(l.viewDir, -l.halfVector);
			float3 fLT = (l.indirectCol + fLTDot) * thickness * _SStr * subCol * atten * m.subsurfMask;
			sss = l.directCol * fLT * albedo;
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

float GetGGXTerm(lighting l, float roughness){
	float rough2 = roughness * roughness;
	float lambdaV = l.NdotL * sqrt((-l.NdotV * rough2 + l.NdotV) * l.NdotV + rough2);
    float lambdaL = l.NdotV * sqrt((-l.NdotL * rough2 + l.NdotL) * l.NdotL + rough2);

	float visibilityTerm = 0.5f / (lambdaV + lambdaL + 1e-5f);
    float d = (l.NdotH * rough2 - l.NdotH) * l.NdotH + 1.0f;
	float dotTerm = UNITY_INV_PI * rough2 / (d * d + 1e-7f);

	visibilityTerm *= dotTerm * UNITY_PI;

	return visibilityTerm;
}

float GetAnisoTerm(g2f i, lighting l, masks m){
	_RippleAmplitude = abs(_RippleAmplitude)+1.01;
	float rippleValue = i.rawUV.z*_RippleFrequency*100;
	float ripple = (sin(rippleValue) + sin(rippleValue*_RippleSeeds.x) + sin(rippleValue*_RippleSeeds.y) + sin(rippleValue*_RippleSeeds.z))/3.0;
	ripple = (ripple+_RippleAmplitude)/(2.0*_RippleAmplitude);
	ripple = lerp(ripple, (0.01/ripple)*50, _RippleInvert);
	_AnisoAngleY *= ripple;
	_AnisoLayerY *= ripple;
	_AnisoAngleY *=  0.005;
	_AnisoLayerY *= 0.025;

	float f0 = l.TdotH * l.TdotH / (_AnisoAngleX * _AnisoAngleX) + l.BdotH * l.BdotH / (_AnisoAngleY * _AnisoAngleY) + l.NdotH * l.NdotH;
	float f1 = l.TdotH * l.TdotH / (_AnisoAngleX * _AnisoLayerX) + l.BdotH * l.BdotH / (_AnisoAngleY * _AnisoLayerY) + l.NdotH * l.NdotH;
	float layer0 = saturate(1.0 / (_AnisoAngleX * _AnisoAngleY * f0 * f0));
	float layer1 = saturate(1.0 / (_AnisoAngleX * _AnisoAngleY * f1 * f1));
	float visibilityTerm = 1;

	if (_AnisoLerp == 1)
		visibilityTerm = lerp(layer1*_AnisoLayerStr, layer0, layer0);
	else
		visibilityTerm = saturate(layer0 + (layer1*_AnisoLayerStr));

	return visibilityTerm;
}

void GetSpecFresTerm(g2f i, lighting l, masks m, inout float3 specularTerm, inout float3 fresnelTerm, float3 specCol, float roughness){

	// GGX
	#if GGX_SPECULAR
		specularTerm = GetGGXTerm(l, roughness) * _SpecStr; 
		fresnelTerm = FresnelTerm(specCol, l.LdotH);

	// Anisotropic
	#elif ANISO_SPECULAR
		specularTerm = GetAnisoTerm(i,l,m) * _AnisoStr;
		fresnelTerm = FresnelTerm(specCol, l.LdotH);
		fresnelTerm = lerp(1, fresnelTerm, metallic);

	// Combined
	#elif COMBINED_SPECULAR
		float ggx = GetGGXTerm(l, roughness);
		float aniso = GetAnisoTerm(i,l,m);
		specularTerm = lerp(aniso * _AnisoStr, ggx * _SpecStr, 1-m.anisoMask);
		fresnelTerm = FresnelTerm(specCol, l.LdotH);
		fresnelTerm = lerp(lerp(1, fresnelTerm, metallic), fresnelTerm, 1-m.anisoMask);
	#endif

	specularTerm = max(0, specularTerm * l.NdotL);
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
	float3 subsurfCol = GetSSS(i, l, m, diffuse.rgb, atten);

	l.directCol *= atten;
	l.directCol += l.vLightCol;

	float diffuseTerm = DisneyDiffuse(l, m, percepRough);
	float3 lighting = l.indirectCol + l.directCol * diffuseTerm;
	float3 specular = 0;
	float3 reflections = 0;
	float3 environment = 0;

	// Specular
	#if !OUTLINE_PASS
		#if SPECULAR_ENABLED
			#if !ADDITIVE_PASS
			if (!(!l.lightEnv && _RealtimeSpec == 1)){
			#endif
				float3 fresnelTerm = 1;
				float3 specularTerm = 1;
				float3 specBiasCol = lerp(specCol, albedo, _SpecBiasOverride*_SpecBiasOverrideToggle);
				GetSpecFresTerm(i, l, m, specularTerm, fresnelTerm, specBiasCol, lerp(brdfRoughness, _SpecRough, _SpecUseRough));
				specular = lerp(lighting, 1, _ManualSpecBright) * specularTerm * fresnelTerm * m.specularMask * _SpecCol * l.ao;
				if (_SharpSpecular == 1){
					roughness = saturate(roughness*2);
					float sharpTerm = round(_SharpSpecStr*Average(specular))/_SharpSpecStr;
					specular = lerp(specular*sharpTerm, specular, roughness);
				}
			#if !ADDITIVE_PASS
			}
			#endif
		#endif

		// Reflections
		// Lighting based IOR from Retro's standard mod
		#if REFLECTIONS_ENABLED && FORWARD_PASS													
			float surfaceReduction = (1.0 / (brdfRoughness*brdfRoughness + 1.0)) * lerp(1, (l.NdotL + 0.3), _LightingBasedIOR);
			float grazingTerm = saturate(smoothness + (1-omr));
			reflections = surfaceReduction * reflCol * FresnelLerp(specCol, grazingTerm, l.NdotV);
			#if SSR_ENABLED
				float4 SSRColor = GetSSRColor(i.worldPos, l.viewDir, l.reflectionDir, normalize(i.normal), smoothness, albedo, metallic, m.reflectionMask, l.screenUVs, i.grabPos);
				reflections = lerp(reflections, SSRColor.rgb, SSRColor.a);
			#endif
			reflections *= m.reflectionMask * _ReflectionStr;
		#endif

		environment = specular + reflections  + subsurfCol;
		
		#if MATCAP_ENABLED
			ApplyMatcap(i, l, m, environment, GetRoughness(smoothness));
		#endif
	#endif

	#if REFRACTION_ENABLED
		float3 col = diffuse.rgb * lerp(lighting, 1, step(m.refractDissolveMask, _RefractionDissolveMaskStr) * (1-_RefractionOpac) * m.refractMask);
	#else
		float3 col = diffuse.rgb * lighting;
	#endif

	// Prevents being washed out by intense lighting
	float3 maxCol = (diffuse.rgb + environment) * diffuseTerm;
	col = lerp(col, clamp(col, 0, maxCol), _ColorPreservation);

    return col + environment;
}
#else

float4 GetDiffuse(lighting l, float4 albedo, float3 atten){
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