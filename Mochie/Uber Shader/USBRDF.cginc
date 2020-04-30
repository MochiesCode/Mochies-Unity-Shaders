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

float3 GetStaticReflCol(float3 reflDir, float roughness, float brightness){
    return texCUBElod(_ReflCube, float4(reflDir, roughness * UNITY_SPECCUBE_LOD_STEPS))*brightness;
}

float3 GetStaticReflections(float3 reflDir, float3 worldPos, float brightness, float roughness){
    float3 p0 = 0;
    UNITY_BRANCH
    if (_ReflCubeFallback == 0){
		p0 = GetStaticReflCol(reflDir, roughness, brightness);
    }
    else {
		float3 wr = GetWorldReflections(reflDir, worldPos, roughness);
        if (!any(wr)) 
            p0 = GetStaticReflCol(reflDir, roughness, brightness);
        else p0 = wr;
	}
    return p0;
}

float3 GetReflections(g2f i, lighting l, float roughness){
    float3 reflections = 0;
	#if defined(UNITY_PASS_FORWARDBASE)
		#if !defined(_GLOSSYREFLECTIONS_OFF)
			UNITY_BRANCH
			if (_UseReflCube == 0)
				reflections = GetWorldReflections(l.reflectionDir, i.worldPos.xyz, roughness);
			else
				reflections = GetStaticReflections(l.reflectionDir, i.worldPos.xyz, l.worldBrightness, roughness);
			reflections *= l.ao;
		#endif
	#endif
    return reflections;
}

void ApplyMatcap(g2f i, lighting l, masks m, inout float3 environment){
	#if defined(UNITY_PASS_FORWARDBASE) && !defined(OUTLINE)
		UNITY_BRANCH
		if (_MatcapToggle == 1){
			float3 worldViewUp = normalize(float3(0,1,0) - l.viewDir * dot(l.viewDir, float3(0,1,0)));
			float3 worldViewRight = normalize(cross(l.viewDir, worldViewUp));
			float2 matcapUV = float2(dot(worldViewRight, l.normal), dot(worldViewUp, l.normal)) * 0.5 + 0.5;
			float4 matcap = UNITY_SAMPLE_TEX2D_SAMPLER(_Matcap, _MainTex, matcapUV) * _MatcapColor;
			matcap.rgb *= _MatcapStr * m.matcapMask * lerp(l.worldBrightness, 1, _UnlitMatcap);
			UNITY_BRANCH
			if (_MatcapBlending == 0){
				environment += matcap.rgb;
			} 
			else if (_MatcapBlending == 1){
				environment *= matcap.rgb; 
			}
			else if (_MatcapBlending == 2){
				environment += matcap.rgb * matcap.a;
			}
		}
	#endif
}

// float DotHalftone(lighting l, float scalar){
// 	bool inMirror = IsInMirror();
// 	float2 uv = l.screenUVs;
// 	#if UNITY_SINGLE_PASS_STEREO
// 		uv.x *= 2;
// 	#endif
    
//     float2 nearest = 2 * frac(100 * uv) - 1;
//     float dist = length(nearest);
// 	float dotSize = 5 * scalar;
//     float dotMask = step(dotSize, dist);

// 	return dotMask;
// }

float GetRamp(g2f i, lighting l, masks m, float atten){
	float ramp = 1;
	float dithering = 1;
	UNITY_BRANCH
	if (_ShadowDithering == 1){
		float2 ditherUV = floor((l.screenUVs * _ScreenParams.xy) * _ShadowDitherStr) * 0.5;
		dithering = 1-frac(ditherUV.x + ditherUV.y);
		l.NdotL *= dithering;
	}
	#if defined(UNITY_PASS_FORWARDBASE)
		UNITY_BRANCH
		if (_EnableShadowRamp == 1){
			float rampUV = l.NdotL * 0.5 + 0.5;
			UNITY_BRANCH
			if (_LinearIntRamp == 1){
				float pixelSize = max(_ShadowRamp_TexelSize.z, _ShadowRamp_TexelSize.w);
				rampUV *= pixelSize;
				float2 duv = float2(ddx(rampUV), ddy(rampUV));
				float rf = rsqrt(dot(duv, duv));
				float x = floor(rampUV);
				x = x - max(0, 0.5 - rf * (rampUV - floor(rampUV)));
				x = x + max(0, 0.5 - rf * (ceil(rampUV) - rampUV));
				x = (x + 0.5) / pixelSize;
				rampUV = x;
			}
			ramp = tex2D(_ShadowRamp, rampUV.xx).rgb;
			ramp = lerp(1, ramp, _ShadowStr*m.shadowMask*_Shadows);
		}
		else {
			UNITY_BRANCH
			if (!l.lightEnv || _RTSelfShadow == 1){
				atten = lerp(atten, smootherstep(0,1,atten), _AttenSmoothing);
				l.NdotL *= atten;
			}
			float ramp0 = smoothstep(0, _RampWidth0, l.NdotL);
			float ramp1 = smoothstep(0, _RampWidth1, l.NdotL);
			ramp = lerp(ramp0, ramp1, _RampWeight);
			// UNITY_BRANCH
			// if (_ShadowDithering == 1)
			// 	dithering = 1-DotHalftone(l, 1-ramp);
			// ramp *= 1-dithering;
		}
		ramp = lerp(1, ramp, _ShadowStr*m.shadowMask*_Shadows); 
	#else
		float ramp0 = smoothstep(0, _RampWidth0, l.NdotL);
		float ramp1 = smoothstep(0, _RampWidth1, l.NdotL);
		ramp = lerp(ramp0, ramp1, _RampWeight) * atten;
	#endif
	return ramp;
}

float3 GetSSS(g2f i, lighting l, float3 albedo, float atten){
    float3 sss = 0;
    UNITY_BRANCH
    if (_Subsurface == 1){
        _SPen = 1-_SPen;
		float thickness = 1 - UNITY_SAMPLE_TEX2D_SAMPLER(_TranslucencyMap, _MainTex, i.uv.xy);
		float3 subCol = UNITY_SAMPLE_TEX2D_SAMPLER(_SubsurfaceTex, _MainTex, i.uv.xy) * _SColor;
        float ndl = smoothstep(_SPen-_SSharp, _SPen+_SSharp, l.NdotL);
        atten = lerp(1, saturate(ndl * atten), _SAtten);
        float3 vLTLight = l.directCol * l.normalDir;
        float fLTDot = DotClamped(l.viewDir, -l.halfVector);
		float mask = SampleMask(_SubsurfaceMask, i.uv.xy, _SubsurfaceMaskChannel, true);
        float3 fLT = (l.indirectCol + fLTDot) * thickness * _SStr * subCol * atten * mask;
        sss = l.directCol * fLT * albedo;
    }
    return sss;
}

float3 FresnelLerp(float3 F0, float3 F90, float cosA){
    float t = Pow5(1 - cosA);
    return lerp(F0, F90, t);
}

float3 FresnelTerm(float3 F0, float cosA){
    float t = Pow5(1 - cosA);
    return F0 + (1-F0) * t;
}

float GetGGXTerm(lighting l, float roughness){
	float rough2 = roughness * roughness;
	float lambdaV = l.NdotL * sqrt((-l.NdotV * rough2 + l.NdotV) * l.NdotV + rough2);
    float lambdaL = l.NdotV * sqrt((-l.NdotL * rough2 + l.NdotL) * l.NdotL + rough2);

	float visibilityTerm = 0.5f / (lambdaV + lambdaL + 1e-5f);

    float d = (l.NdotH * rough2 - l.NdotH) * l.NdotH + 1.0f;
	float dotTerm = UNITY_INV_PI * rough2 / (d * d + 1e-7f);

	return visibilityTerm * dotTerm * UNITY_PI;
}

float GetAnisoTerm(lighting l){
	_AnisoAngleX *= 0.1;
	_AnisoAngleY *= 0.1;
	float f0 = l.TdotH * l.TdotH / (_AnisoAngleX * _AnisoAngleX) + l.BdotH * l.BdotH / (_AnisoAngleY * _AnisoAngleY) + l.NdotH * l.NdotH;
	float f1 = l.TdotH * l.TdotH / (_AnisoAngleX * _AnisoAngleX * _AnisoLayerX) + l.BdotH * l.BdotH / (_AnisoAngleY * _AnisoAngleY * _AnisoLayerY) + l.NdotH * l.NdotH;
	float layer0 = saturate(1.0 / (_AnisoAngleX * _AnisoAngleY * f0 * f0));
	float layer1 = saturate(1.0 / (_AnisoAngleX * _AnisoAngleY * f1 * f1));
	float addTerm = saturate(layer0 + (layer1*_AnisoLayerStr)); 
	float lerpTerm = lerp(layer1*_AnisoLayerStr, layer0, layer0);
	return lerp(addTerm, lerpTerm, _AnisoLerp);
}

void GetSpecFresTerm(lighting l, masks m, inout float specularTerm, inout float3 fresnelTerm, float3 specCol, float roughness){
	UNITY_BRANCH
	switch (_SpecularStyle){
		case 0: 
			specularTerm = GetGGXTerm(l, roughness); 
			fresnelTerm = FresnelTerm(specCol, l.LdotH);
			break;
		case 1: 
			specularTerm = GetAnisoTerm(l);
			break;
		case 2: 
			float ggx = GetGGXTerm(l, roughness);
			float aniso = GetAnisoTerm(l);
			specularTerm = lerp(aniso, ggx, m.anisoMask);
			fresnelTerm = FresnelTerm(specCol, l.LdotH);
			fresnelTerm = lerp(1, fresnelTerm, m.anisoMask);
			break;
		default: break;
	}

	specularTerm = max(0, specularTerm * l.NdotL);

	UNITY_BRANCH
	if (_SharpSpecular == 1){
		roughness = saturate(roughness*2);
		float sharpTerm = round(specularTerm);
		specularTerm = lerp(sharpTerm, specularTerm, roughness);
	}
}

float DisneyDiffuse(lighting l, masks m, float atten, float percepRough) {
	float fd90 = 0.5 + 2 * l.LdotH * l.LdotH * percepRough;
	float lightScatter = (1 + (fd90 - 1) * Pow5(1 - atten));
	float viewScatter = (1 + (fd90 - 1) * Pow5(1 - l.NdotV));
	float term = lightScatter * viewScatter;
	return lerp(1, term, _DisneyDiffuse * m.ddMask);
}

float3 GetMochieBRDF(g2f i, lighting l, masks m, float4 diffuse, float4 albedo, float3 specCol, float3 reflCol, float oneMinusReflectivity, float smoothness, float atten) {
	float percepRough = 1-smoothness;
	float roughness = percepRough * percepRough;
	roughness = max(roughness, 0.002);
	float3 subsurfCol = GetSSS(i, l, diffuse.rgb, atten);

	l.directCol += l.vLightCol;
	l.directCol *= atten;
	l.directCol += subsurfCol;

	float diffuseTerm = DisneyDiffuse(l, m, atten, percepRough);
	float3 lighting = l.indirectCol + l.directCol * diffuseTerm;
	float3 specular = 0;
	float3 reflections = 0;

	// Specular
	#if !defined(_SPECULARHIGHLIGHTS_OFF)
		float3 fresnelTerm = 1;
		float specularTerm = 1;
		GetSpecFresTerm(l, m, specularTerm, fresnelTerm, specCol, roughness);
		specular = lighting * specularTerm * fresnelTerm * m.specularMask * _SpecStr * _SpecCol;
	#endif

	// Reflections
	#if !defined(_GLOSSYREFLECTIONS_OFF)
		float surfaceReduction = 1.0 / (roughness*roughness + 1.0);
		float grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
		reflections = surfaceReduction * reflCol * FresnelLerp(specCol, grazingTerm, l.NdotV);
		#if defined(UNITY_PASS_FORWARDBASE) && !defined(OUTLINE)
			UNITY_BRANCH
			if (_SSR == 1){
				float4 SSRColor = GetSSRColor2(
					i.worldPos,
					l.viewDir,
					l.reflectionDir,
					normalize(i.normal),
					smoothness,
					albedo,
					metallic,
					m.reflectionMask,
					l.screenUVs,
					i.screenPos
				);
				//reflections = SSRColor.a; 
				reflections = lerp(reflections, SSRColor.rgb, SSRColor.a);
			}
		#endif
		reflections *= m.reflectionMask * _ReflectionStr;
	#endif

	// Add it all up
	float3 environment = specular + reflections;
	ApplyMatcap(i, l, m, environment);
	lighting = lerp(lighting, 1, cubeMask*_UnlitCube*_CubeMode > 0);
	float3 col = diffuse.rgb * lighting;

	// Prevent color from being washed out by intense lighting
	#if defined(UNITY_PASS_FORWARDBASE)
		UNITY_BRANCH
		if (_ColorPreservation == 1){
			float3 maxCol = (diffuse.rgb + environment + subsurfCol) * diffuseTerm;
			col = clamp(col, 0, maxCol);
		}
	#endif

    return col + environment;
}