float4 GetVertexLightAtten(float4 lengthSq){
	float4 invRangeSqr = unity_4LightAtten0 / 25.0;
	float4 ratio2 = lengthSq * invRangeSqr;
	float4 num = saturate(1.0 - (ratio2 * ratio2));
	float4 atten = (num * num) / (lengthSq + 1.0);	
	return atten;
}

float3 GetVertexLightColor(g2f i, lighting l) {

	float3 lightColor = 0;
	UNITY_BRANCH
	if (i.isVLight){
		float4 lengthSq = 0;
		lengthSq += l.toLightX * l.toLightX;
		lengthSq += l.toLightY * l.toLightY;
		lengthSq += l.toLightZ * l.toLightZ;
		
		// NdotL
		float4 NdotL = 0;
		NdotL += l.toLightX * l.normal.x;
		NdotL += l.toLightY * l.normal.y;
		NdotL += l.toLightZ * l.normal.z;

		// Correct NdotL
		float4 corr = rsqrt(lengthSq);
		NdotL = max(0, NdotL * corr);

		float4 atten = GetVertexLightAtten(lengthSq);
		float4 diff = NdotL * atten;
		UNITY_BRANCH
		if (_RenderMode != 2){
			float4 ramp0 = smoothstep(0, _RampWidth0, NdotL);
			float4 ramp1 = smoothstep(0, _RampWidth1, NdotL);
			diff = lerp(ramp0, ramp1, _RampWeight) * atten;
		}

		lightColor.rgb += unity_LightColor[0] * diff.x;
		lightColor.rgb += unity_LightColor[1] * diff.y;
		lightColor.rgb += unity_LightColor[2] * diff.z;
		lightColor.rgb += unity_LightColor[3] * diff.w;
	}
    return lightColor * _VLightCont;
}

float3 GetVertexLightDir(float3 worldPos) {
    float3 toLightX = float3(unity_4LightPosX0.x, unity_4LightPosY0.x, unity_4LightPosZ0.x);
    float3 toLightY = float3(unity_4LightPosX0.y, unity_4LightPosY0.y, unity_4LightPosZ0.y);
    float3 toLightZ = float3(unity_4LightPosX0.z, unity_4LightPosY0.z, unity_4LightPosZ0.z);
	float3 toLightW = float3(unity_4LightPosX0.w, unity_4LightPosY0.w, unity_4LightPosZ0.w);

    float3 dirX = toLightX - worldPos;
    float3 dirY = toLightY - worldPos;
    float3 dirZ = toLightZ - worldPos;
	float3 dirW = toLightW - worldPos;

	float distX = length(toLightX);
	float distY = length(toLightY);
	float distZ = length(toLightZ);
	float distW = length(toLightW);

	// float3 lengthSq = 0;
	// lengthSq += toLightX * toLightX;
	// lengthSq += toLightY * toLightY;
	// lengthSq += toLightZ * toLightZ;
	// float4 atten = GetVertexLightAtten(float4(lengthSq,1));
	
	dirX *= distX;
	dirY *= distY;
	dirZ *= distZ;
	dirW *= distW;
	
	float3 dir = normalize(dirX + dirY + dirZ + dirW);
    return lerp(0, dir, _VLightCont);
}

void GetLightColor(inout float3 directCol, inout float3 indirectCol, bool lightEnv){
	UNITY_BRANCH
    if (lightEnv){
		directCol = _LightColor0;
		UNITY_BRANCH
		if (_RenderMode > 0){
			directCol *= _RTDirectCont;
			indirectCol *= _RTIndirectCont;
		}
    }
    else {
		UNITY_BRANCH
		if (_RenderMode > 0){
			directCol = indirectCol * _DirectCont;
			indirectCol *= _IndirectCont;
		}
		else {
			directCol = indirectCol * 0.6;
			indirectCol *= 0.6;
		}
    }
}

float3 GetLightDir(g2f i, lighting l) {
	float3 lightDir = _StaticLightDir.xyz;

	#if defined(UNITY_PASS_FORWARDADD)
		lightDir = UnityWorldSpaceLightDir(i.worldPos);
	#else
	UNITY_BRANCH
	if (_StaticLightDirToggle == 0){
		lightDir = UnityWorldSpaceLightDir(i.worldPos) * l.lightEnv;
		lightDir += unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz;
	}
	UNITY_BRANCH
	if (i.isVLight) 
		lightDir += GetVertexLightDir(i.worldPos);
	#endif

	return normalize(lightDir);
}

float3 GetViewDir(float3 worldPos){
	return normalize(_WorldSpaceCameraPos.xyz - worldPos);
}

float3 GetHalfVector(float3 lightDir, float3 viewDir){
	return normalize(lightDir + viewDir);
}

float3 GetNormalDir(g2f i, float detailMask){
	float3 normalMap = UnpackScaleNormal(tex2D(_BumpMap, i.uv.xy), _BumpScale);
	normalMap.y = lerp(normalMap.y, 1-normalMap.y, _InvertNormalY0);
	#if defined(_DETAIL_MULX2)
		float3 detailNormal = UnpackScaleNormal(tex2D(_DetailNormalMap, i.uv2.xy), _DetailNormalMapScale * detailMask);
		detailNormal.y = lerp(detailNormal.y, 1-detailNormal.y, _InvertNormalY1);
		normalMap = BlendNormals(normalMap, detailNormal);
	#else
		normalMap = normalize(normalMap);
	#endif
	UNITY_BRANCH
	if (_HardenNormals == 1){
		float3 dpdx = ddx(i.worldPos);
		float3 dpdy = ddy(i.worldPos);
		i.normal = normalize(cross(dpdy, dpdx));
	}
	float3 normalDir = normalize(normalMap.x * i.tangent + normalMap.y * i.binormal + normalMap.z * i.normal);
	return normalDir;
}

float3 GetNormal(g2f i, float3 normalDir){
	return lerp(normalDir, normalize(i.normal), _ClearCoat);
}

float3 GetBinormal(float4 tangent, float3 normalDir){
	return cross(tangent, normalDir);
}

float GetAO(g2f i){
	float ao = 1;
	UNITY_BRANCH
	if (_RenderMode == 2 && _PBRWorkflow == 2){
		float4 packedTex = tex2D(_PackedMap, i.uv.xy);
		ao = ChannelCheck(packedTex, ao, _OcclusionChannel);
	}
	else ao = tex2D(_OcclusionMap, i.uv.xy).g;
	return lerp(1, ao, _OcclusionStrength);
}

masks GetMasks(g2f i){
	masks m = (masks)1;
	UNITY_BRANCH
	if (_MaskingToggle == 1){
		UNITY_BRANCH
		if (_MaskingMode == 0){
			#if !defined(_GLOSSYREFLECTIONS_OFF)
				m.reflectionMask = SampleMask(_ReflectionMask, i.uv.xy, _ReflectionMaskChannel, true);
			#endif
			#if !defined(_SPECULARHIGHLIGHTS_OFF)
				m.specularMask = SampleMask(_SpecularMask, i.uv.xy, _SpecularMaskChannel, true);
			#endif
			m.detailMask = SampleMask(_DetailMask, i.uv.xy, _DetailMaskChannel, true);
			m.shadowMask = SampleMask(_ShadowMask, i.uv.xy, _ShadowMaskChannel, _Shadows == 1);
			m.rimMask = SampleMask(_RimMask, i.uv.xy, _RimMaskChannel, _RimLighting == 1);
			m.ddMask = SampleMask(_DDMask, i.uv.xy, _DDMaskChannel, _DisneyDiffuse > 0);
			m.anisoMask = 1-SampleMask(_InterpMask, i.uv.xy, _InterpMaskChannel, _SpecularStyle == 2);
			m.matcapMask = SampleMask(_MatcapMask, i.uv.xy, _MatcapMaskChannel, _MatcapToggle == 1);
			m.smoothMask = SampleMask(_SmoothShadeMask, i.uv.xy, _SmoothShadeMaskChannel, _SmoothShading > 0);
		}
		else {
			SamplePackedMask(i, m);
		}
	}
	else {
		m.detailMask = SampleMask(_DetailMask, i.uv.xy, _DetailMaskChannel, true);
	}
	return m;
}

float3 GetOutlineLightColor(float3 indirectCol){
	float3 lightCol = _LightColor0;
	if (!any(_WorldSpaceLightPos0.xyz)){
		lightCol = indirectCol;
	}
	UNITY_BRANCH
	if (_Outline == 1)
		lightCol = saturate(AverageRGB(lightCol));
	return lightCol;
}

float FadeShadows (g2f i, float atten) {
    #if HANDLE_SHADOWS_BLENDING_IN_GI
        float viewZ = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
        float shadowFadeDistance = UnityComputeShadowFadeDistance(i.worldPos, viewZ);
        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        atten = saturate(atten + shadowFade);
    #endif
	return atten;
}

float3 ApplyLREmission(lighting l, float3 diffuse, float atten, float3 emiss){
	#if defined(UNITY_PASS_FORWARDBASE) && defined(_EMISSION)
		UNITY_BRANCH
		if (_RenderMode == 2)
			atten = saturate(l.NdotL*1000);
		float brightness = l.worldBrightness;
		float interpolator = 0;
		UNITY_BRANCH
		if (_CrossMode == 1 && _ReactToggle == 1){
			float2 threshold = saturate(float2(_ReactThresh-_Crossfade, _ReactThresh+_Crossfade));
			interpolator = smootherstep(threshold.x, threshold.y, brightness); 
		}
		UNITY_BRANCH
		if (_CrossMode != 1 && _ReactToggle == 1)
			interpolator = brightness*_ReactToggle;

		return lerp(diffuse+emiss, diffuse, interpolator);
	#else
		return diffuse;
	#endif
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

float3 ApplyMatcap(g2f i, lighting l, masks m, float3 diffuse){

	#if defined(UNITY_PASS_FORWARDBASE)
		UNITY_BRANCH
		if (_MatcapToggle == 1){
			float3 worldViewUp = normalize(float3(0,1,0) - l.viewDir * dot(l.viewDir, float3(0,1,0)));
			float3 worldViewRight = normalize(cross(l.viewDir, worldViewUp));
			float2 matcapUV = float2(dot(worldViewRight, l.normal), dot(worldViewUp, l.normal)) * 0.5 + 0.5;
			float4 matcap = UNITY_SAMPLE_TEX2D_SAMPLER(_Matcap, _MainTex, matcapUV) * _MatcapColor;
			matcap *= _MatcapStr * l.worldBrightness;

			UNITY_BRANCH
			if (_MatcapBlending == 0){
				diffuse += matcap.rgb*m.matcapMask;
			} 
			else if (_MatcapBlending == 1){
				matcap.rgb = lerp(1, matcap.rgb, m.matcapMask);
				diffuse *= matcap.rgb; 
			}
			else if (_MatcapBlending == 2){
				diffuse = lerp(diffuse, matcap, m.matcapMask);
			}
		}
	#endif

	return diffuse;
}

float GetRamp(g2f i, lighting l, masks m, float atten){
	float ramp = 1;
	#if defined(UNITY_PASS_FORWARDBASE)
		UNITY_BRANCH
		if (_EnableShadowRamp == 1){
			float rampUV = l.NdotL * 0.5 + 0.5;
			ramp = tex2D(_ShadowRamp, float2(rampUV, rampUV)).rgb;
			ramp = lerp(1, ramp, _ShadowStr*m.shadowMask*_Shadows);
		}
		else {
			float interp = l.NdotL;
			float upperBound0 = _RampWidth0;
			float upperBound1 = _RampWidth1;
			UNITY_BRANCH
			if (!l.lightEnv || _RTSelfShadow == 1){
				upperBound0 += 0.005;
				upperBound1 += 0.005;
				interp *= atten;
			}
			float ramp0 = smoothstep(0, upperBound0, interp);
			float ramp1 = smoothstep(0, upperBound1, interp);
			ramp = lerp(ramp0, ramp1, _RampWeight);
		}
		ramp = lerp(1, ramp, _ShadowStr*m.shadowMask*_Shadows); 
	#else
		float ramp0 = smoothstep(0, _RampWidth0, l.NdotL);
		float ramp1 = smoothstep(0, _RampWidth1, l.NdotL);
		ramp = lerp(ramp0, ramp1, _RampWeight) * atten;
	#endif

	return ramp;
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
	
	
	// Smith Joint Visibility Term
	// float lambdaV = l.NdotL * (l.NdotV * (1 - roughness) + roughness);
    // float lambdaL = l.NdotV * (l.NdotL * (1 - roughness) + roughness);

	// More expensive but accurate implementation
	float rough2 = roughness * roughness;
	float lambdaV = l.NdotL * sqrt((-l.NdotV * rough2 + l.NdotV) * l.NdotV + rough2);
    float lambdaL = l.NdotV * sqrt((-l.NdotL * rough2 + l.NdotL) * l.NdotL + rough2);

	float visibilityTerm = 0.5f / (lambdaV + lambdaL + 1e-5f);

	// Specular Term
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
	[forcecase]
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

// Modified version of BRDF1 from standard shader.
float3 GetToonBRDF(g2f i, lighting l, masks m, float3 diffuse, float3 specCol, float3 reflCol, float oneMinusReflectivity, float smoothness, float atten) {
	float percepRough = 1-smoothness;
	float roughness = percepRough * percepRough;
	roughness = max(roughness, 0.002);

	l.lightCol += l.vLightCol;
	l.lightCol *= atten;

	float diffuseTerm = DisneyDiffuse(l, m, atten, percepRough);
	float3 specular = 0;
	float3 reflections = 0;

	// Specular
	#if !defined(_SPECULARHIGHLIGHTS_OFF)
		float3 fresnelTerm = 1;
		float specularTerm = 1;
		GetSpecFresTerm(l, m, specularTerm, fresnelTerm, specCol, roughness);
		float3 lightCol = l.lightCol;
		#if defined(UNITY_PASS_FORWARDBASE)
			lightCol = lerp(saturate(l.lightCol*_ProbeContrib), l.lightCol, l.lightEnv);
		#endif
		specular = lightCol * specularTerm * fresnelTerm * m.specularMask * _SpecStr * _SpecCol;
	#endif

	// Reflections
	#if !defined(_GLOSSYREFLECTIONS_OFF)
		float surfaceReduction = 1.0 / (roughness*roughness + 1.0);
		float grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
		reflections = surfaceReduction * reflCol * FresnelLerp(specCol, grazingTerm, l.NdotV);
		reflections *= m.reflectionMask * _ReflectionStr;
	#endif

	// Add it all up
	float3 environment = specular + reflections;
	environment = ApplyMatcap(i, l, m, environment);
	float3 lighting = l.indirectCol + l.lightCol * diffuseTerm;
	cubeMask = _CubeMode == 3 ? 1 : cubeMask;
	lighting = lerp(lighting, 1, cubeMask*_UnlitCube*_CubeMode > 0);
	float3 col = diffuse * lighting;

	// Prevent color from being washed out by intense lighting
	#if defined(UNITY_PASS_FORWARDBASE)
		UNITY_BRANCH
		if (_ColorPreservation == 1){
			float3 maxCol = (diffuse + environment) * diffuseTerm;
			col = clamp(col, 0, maxCol);
		}
	#endif

	col += environment;

    return col;
}

lighting GetLighting(g2f i, masks m, float atten){
    lighting o;
	UNITY_INITIALIZE_OUTPUT(lighting, o);

	#if defined(UNITY_PASS_FORWARDBASE)
		o.lightEnv = any(_WorldSpaceLightPos0);
	#endif

    UNITY_BRANCH
    if (_RenderMode > 0){
		o.ao = GetAO(i);
		o.toLightX = unity_4LightPosX0 - i.worldPos.x;
		o.toLightY = unity_4LightPosY0 - i.worldPos.y;
		o.toLightZ = unity_4LightPosZ0 - i.worldPos.z;

		o.lightDir = GetLightDir(i, o);
		o.viewDir = GetViewDir(i.worldPos);
		o.halfVector = GetHalfVector(o.lightDir, o.viewDir);
		o.normalDir = GetNormalDir(i, m.detailMask);
		o.normal = GetNormal(i, o.normalDir);
		o.tangent = i.tangent;
		o.binormal = GetBinormal(o.tangent, o.normal);
		#if !defined(_GLOSSYREFLECTIONS_OFF)
			o.reflectionDir = reflect(-o.viewDir, o.normal);
		#endif

        o.NdotL = DotClamped(o.normalDir, o.lightDir);
		o.NdotV = abs(dot(o.normal, o.viewDir));
		o.NdotH = DotClamped(o.normal, o.halfVector);
		o.LdotH = DotClamped(o.lightDir, o.halfVector);
		#if !defined(_SPECULARHIGHLIGHTS_OFF)
			UNITY_BRANCH
			if (_SpecularStyle > 0){
				o.TdotH = dot(o.tangent, o.halfVector);
				o.BdotH = dot(o.binormal, o.halfVector);
			}
		#endif
    }

	#if defined(UNITY_PASS_FORWARDBASE)
		float3 probeCol = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
		o.vLightCol = GetVertexLightColor(i, o);
		o.sh9Col = max(0, ShadeSH9(float4(o.normal,1))) * o.ao;
		o.indirectCol = lerp(probeCol, o.sh9Col, _SmoothShading*m.smoothMask);
		GetLightColor(o.lightCol, o.indirectCol, o.lightEnv);
		o.worldBrightness = saturate(AverageRGB(o.lightCol + o.vLightCol));
		o.lightCol *= lerp(1, o.ao, _RenderMode != 0);
	#else
		o.lightCol = lerp(_LightColor0, clamp(_LightColor0, 0, _AdditiveMax), _ClampAdditive);
	#endif
    return o;
}

UnityLight GetDirectLight(lighting l, float atten){
    UnityLight o;
    o.color = l.lightCol * atten;
    o.dir = l.lightDir;
    return o;
}

UnityIndirect GetIndirectLight(g2f i, lighting l, float roughness){
    UnityIndirect o;
    o.diffuse = l.sh9Col + l.vLightCol;
    o.specular = 0;
    #if defined(UNITY_PASS_FORWARDBASE)
        o.specular = GetReflections(i, l, roughness);
    #endif
    return o;
}

// Subsurface Scattering, never really seemed to look right imo, will remake later
// float3 GetSSS(lighting l, float3 albedo, float atten, float2 uv){
//     float3 sss = 0;
//     UNITY_BRANCH
//     if (_Subsurface == 1){
//         _SPen = 1-_SPen;
//         l.NdotL = smootherstep(_SPen-_SSharp, _SPen+_SSharp, l.NdotL);
//         atten = saturate(l.NdotL * atten);
//         float3 vLTLight = l.lightCol * l.normalDir;
//         float fLTDot = saturate(dot(l.viewDir, -l.halfVector));
//         float3 fLT = (fLTDot + l.indirectCol) * UNITY_SAMPLE_TEX2D_SAMPLER(_TranslucencyMap, _MainTex, uv) * _SStrength * atten * _SColor;
//         sss = l.lightCol * fLT * albedo;
//     }
//     return sss;
// }