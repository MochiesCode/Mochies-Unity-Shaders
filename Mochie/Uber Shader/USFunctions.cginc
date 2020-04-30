//----------------------------
// Color Filtering
//----------------------------

float3 ApplyGeneralFilters(float3 albedo, float2 uv0){
    albedo = GetSaturation(albedo, _SaturationRGB);
    albedo = lerp(albedo, GetHDR(albedo), _HDR);
    albedo = GetContrast(albedo);
    albedo += albedo*_Brightness;
    albedo += GetNoiseRGB(uv0, _Noise);
	return albedo;
}

float3 ApplyTeamColors(float3 albedo, float2 uv0){
	float3 baseCol = albedo;
	float4 teamMask = UNITY_SAMPLE_TEX2D_SAMPLER(_TeamColorMask, _MainTex, uv0);
	float mask = SampleMask(_FilterMask, uv0, _FilterMaskChannel, true);

	// Alloy team colors implementation
	float weight = dot(teamMask, float4(1.0h, 1.0h, 1.0h, 1.0h));
	teamMask /= max(1.0h, weight);
	float3 teamColor = _TeamColor0 * teamMask.r 
					+ _TeamColor1 * teamMask.g 
					+ _TeamColor2 * teamMask.b 
					+ _TeamColor3 * teamMask.a 
					+ saturate(1.0h - weight).rrr;
	albedo *= teamColor;

	albedo.rgb = ApplyGeneralFilters(albedo, uv0);
	albedo = lerp(baseCol, albedo, mask*2);
	return albedo;
}

float3 GetHSLFilter(float3 albedo, float2 uv0){
    float3 baseCol = albedo;
    float mask = SampleMask(_FilterMask, uv0, _FilterMaskChannel, true);
    UNITY_BRANCH
    if (_AutoShift == 1)
        _Hue += frac(_Time.y*_AutoShiftSpeed);
    float3 shift = float3(_Hue, _SaturationHSL, _Luminance);
    float3 hsl = RGBtoHSL(albedo);
    float hslRange = step(_HSLMin, hsl) * step(hsl, _HSLMax);
    albedo = HSLtoRGB(hsl + shift * hslRange);
    albedo = lerp(albedo, GetHDR(albedo), _HDR);
    albedo = GetContrast(albedo);
    albedo += GetNoiseRGB(uv0, _Noise);
	albedo = lerp(baseCol, albedo, mask);
    return albedo;
}

float3 GetRGBFilter(float3 albedo, float2 uv0){
    float3 baseCol = albedo;
    float mask = SampleMask(_FilterMask, uv0, _FilterMaskChannel, true);
    albedo.r *= _RAmt;
    albedo.g *= _GAmt;
    albedo.b *= _BAmt;
	albedo.rgb = ApplyGeneralFilters(albedo, uv0);
	albedo = lerp(baseCol, albedo, mask);
    return albedo;
}

//------------------------------------
// Albedo/Diffuse/Emission/Rim
//------------------------------------
float3 GetDetailAlbedo(g2f i, lighting l, masks m, float3 col){
    float3 detailAlbedo = UNITY_SAMPLE_TEX2D_SAMPLER(_DetailAlbedoMap, _MainTex, i.uv2.xy).rgb * unity_ColorSpaceDouble;
    float3 albedo = lerp(col, col*detailAlbedo, m.detailMask);
    UNITY_BRANCH
    if (_FilterModel == 2){
        albedo = saturate(albedo);
        albedo = GetSaturation(albedo, 1.2);
    }
    return albedo;
}

float4 GetAlbedo(g2f i, lighting l, masks m){
	float4 albedo = 1;
	cubeMask = 1;

	UNITY_BRANCH
	if (_CubeMode == 0){
		albedo = UNITY_SAMPLE_TEX2D(_MainTex, i.uv.xy); 
		albedo.rgb *= _Color.rgb;
	}
	else if (_CubeMode == 1){ 
		UNITY_BRANCH
		if (_AutoRotate0)
			_CubeRotate0 = _Time.y * _CubeRotate0;
		float3 vDir = Rotate(GetViewDir(i.worldPos), _CubeRotate0);
		albedo = texCUBE(_MainTexCube0, vDir);
		albedo.rgb *= _CubeColor0.rgb;
	}
	else if (_CubeMode == 2){
		UNITY_BRANCH
		if (_AutoRotate0)
			_CubeRotate0 = _Time.y * _CubeRotate0;
		float3 vDir = Rotate(GetViewDir(i.worldPos), _CubeRotate0);
		float4 albedo0 = UNITY_SAMPLE_TEX2D(_MainTex, i.uv.xy); 
		float4 albedo1 = texCUBE(_MainTexCube0, vDir);
		albedo0.rgb *= _Color.rgb;
		albedo1.rgb *= _CubeColor0.rgb;
		cubeMask = SampleCubeMask(_CubeBlendMask, i.uv.xy, _CubeBlend, _CubeBlendMaskChannel); 
		albedo.rgb = BlendCubemap(albedo0, albedo1, cubeMask);
	}

    albedo.rgb = GetDetailAlbedo(i, l, m, albedo);

    UNITY_BRANCH
    if 		(_FilterModel == 1) albedo.rgb = GetRGBFilter(albedo.rgb, i.uv.xy);
   	else if (_FilterModel == 2) albedo.rgb = GetHSLFilter(albedo.rgb, i.uv.xy);
	else if (_FilterModel == 3) albedo.rgb = ApplyTeamColors(albedo.rgb, i.uv.xy);

    #if TRANSPARENT_DEFINED
        UNITY_BRANCH
        if 		(_BlendMode == 2) albedo.a *= _Color.a;
        else if (_BlendMode == 3) albedo.a = _Color.a;
    #endif
    return albedo;
}

float4 GetDiffuse(lighting l, float4 albedo, float atten){
    float4 diffuse;
    float3 lightCol = atten * l.directCol + l.indirectCol;
    diffuse.rgb = albedo.rgb;
    diffuse.rgb *= lerp(lightCol, 1, cubeMask*_UnlitCube*_CubeMode > 0);
    diffuse.a = albedo.a;
    return diffuse;
}

float GetPulse(g2f i){
	float pulse = 1;
	UNITY_BRANCH
	if (_PulseToggle == 1){
		UNITY_BRANCH
		switch (_PulseWaveform){
			case 0: pulse = 0.5*(sin(_Time.y * _PulseSpeed)+1); break;
			case 1: pulse = round((sin(_Time.y * _PulseSpeed)+1)*0.5); break;
			case 2: pulse = abs((_Time.y * (_PulseSpeed * 0.333)%2)-1); break;
			case 3: pulse = frac(_Time.y * (_PulseSpeed * 0.2)); break;
			case 4: pulse = 1-frac(_Time.y * (_PulseSpeed * 0.2)); break;
			default: break;
		}
		float mask = SampleMask(_PulseMask, i.uv.xy, _PulseMaskChannel, true);
		pulse = lerp(1, pulse, _PulseStr*mask);
	}
	return pulse;
}

float3 GetEmission(g2f i){
	float3 emiss = 0;
	#if defined(_EMISSION) && (defined(UNITY_PASS_FORWARDBASE) || defined(OUTLINE))
		emiss = UNITY_SAMPLE_TEX2D(_EmissionMap, i.uv.zw).rgb * _EmissionColor.rgb;
		emiss *= SampleMask(_EmissMask, TRANSFORM_TEX(i.uv.xy, _EmissMask), _EmissMaskChannel, true);
		emiss *= GetPulse(i);
	#endif
	return emiss;
}

void ApplyCutout(float alpha){
	#if defined(_ALPHATEST_ON)
        UNITY_BRANCH
        if (_ATM != 1)
            clip(alpha - _Cutoff);
    #endif
}

float3 ApplyRimLighting(g2f i, lighting l, masks m, float3 diffuse, float atten){
	#if defined(UNITY_PASS_FORWARDBASE)
    UNITY_BRANCH
    if (_RenderMode != 0 && _RimLighting == 1){
        float VdotL = abs(dot(l.viewDir, l.normal));
        float rim = pow((1-VdotL), (1-_RimWidth) * 10);
        rim = smootherstep(_RimEdge, (1-_RimEdge), rim);
        rim *= m.rimMask;
        float3 rimCol = UNITY_SAMPLE_TEX2D_SAMPLER(_RimTex, _MainTex, i.uv2.zw).rgb * _RimCol.rgb;
        float interpolator = rim*_RimStr;

		[forcecase]
		switch (_RimBlending){
			case 0: diffuse = lerp(diffuse, rimCol, interpolator); break;
			case 1: diffuse += rimCol*interpolator; break;
			case 2: diffuse -= rimCol*interpolator; break;
			case 3: diffuse *= lerp(1, rimCol, interpolator); break;
		}
    }
	#endif
    return diffuse;
}

float GetRoughness(float roughness){
    roughness *= 1.7-0.7*roughness;
    return roughness;
}

//----------------------------
// Toon Workflow
//----------------------------
float3 GetToonWorkflow(g2f i, lighting l, masks m, float3 albedo, out float3 specularTint, out float smoothness, out float omr){
	float metallic = _Metallic;
	#if defined(_METALLICGLOSSMAP)
		metallic = UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicGlossMap, _MainTex, i.uv.xy);
	#endif
	specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
	smoothness = _Glossiness;
	#if defined(_SPECGLOSSMAP)
		smoothness = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, _MainTex, i.uv.xy);
	#endif
	UNITY_BRANCH
	if (_RoughnessAdjust == 1){
		smoothness = saturate(lerp(0.5, smoothness, _RoughContrast));
		smoothness += saturate(smoothness * _RoughIntensity);
		smoothness = saturate(smoothness + _RoughLightness);
	}
	smoothness = 1-smoothness;
	omr = OneMinusReflectivityFromMetallic(metallic);
	float interpolator = _ReflectionStr*m.reflectionMask;
	return lerp(albedo, albedo*omr, interpolator);
}

//----------------------------
// PBR Workflows
//----------------------------
void GetSpecularWorkflow(g2f i, float albedoAlpha, inout float4 spec, inout float roughness, inout float smoothness){
    #if defined(_SPECGLOSSMAP)
        UNITY_BRANCH
        if (_SourceAlpha == 1){
            spec.rgb = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, _MainTex, i.uv.xy).rgb;
            spec.a = albedoAlpha;
        }
        else 
            spec = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, _MainTex, i.uv.xy);
        spec.a *= _GlossMapScale;
    #else
        spec.rgb = _SpecCol.rgb;
        UNITY_BRANCH
        if (_SourceAlpha == 1)
            spec.a = albedoAlpha * _GlossMapScale;
        else
            spec.a = _Glossiness;
    #endif
    smoothness = spec.a;
	roughness = 1-spec.a;
}

void GetMetallicWorkflow(g2f i, inout float metallic, inout float roughness, inout float smoothness){
	metallic = _Metallic;
	#if defined(_METALLICGLOSSMAP)
		metallic = metallic = UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicGlossMap, _MainTex, i.uv.xy).r;
	#endif
	roughness = _Glossiness;
	#if defined(_SPECGLOSSMAP)
    	roughness = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, _MainTex, i.uv.xy).r;
	#endif
	UNITY_BRANCH
	if (_RoughnessAdjust){
		roughness = saturate(lerp(0.5, roughness, _RoughContrast));
		roughness += saturate(roughness * _RoughIntensity);
		roughness = saturate(roughness + _RoughLightness);
	}
    smoothness = 1-roughness;
    roughness = GetRoughness(roughness);
}

void GetPackedWorkflow(g2f i, inout float metallic, inout float roughness, inout float smoothness){
	float4 packedTex = tex2D(_PackedMap, i.uv.xy);
	metallic = ChannelCheck(packedTex, metallic, _MetallicChannel);
	roughness = ChannelCheck(packedTex, roughness, _RoughnessChannel);
	UNITY_BRANCH
	if (_RoughnessAdjust){
		roughness = saturate(lerp(0.5, roughness, _RoughContrast));
		roughness += saturate(roughness * _RoughIntensity);
		roughness = saturate(roughness + _RoughLightness);
	}
	
	roughness = GetRoughness(roughness);
	smoothness = 1-roughness;
}

//----------------------------
// UV Distortion
//----------------------------
float3 GetUVOffset(g2f i){
	float2 uv = TRANSFORM_TEX(i.uv.xy, _DistortUVMap)+(_Time.y * _DistortUVScroll);
	_DistortUVStr *= SampleMask(_DistortUVMask, i.uv.xy, _DistortUVMaskChannel, _DistortUVStr > 0);
	float3 ofs = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_DistortUVMap, _MainTex, uv), _DistortUVStr);
	ofs *= 0.01;
	return ofs;
}

void ApplyUVDistortion(inout g2f i, inout float3 uvOffset){
	uvOffset = GetUVOffset(i);
	i.uv.xy += uvOffset.xy * _DistortMainUV;
	i.uv.zw += uvOffset.xy * _DistortEmissUV;
	i.uv2.xy += uvOffset.xy * _DistortDetailUV;
	i.uv2.zw += uvOffset.xy * _DistortRimUV;
}

//----------------------------
// Parallax Mapping
//----------------------------
float SampleParallaxMap(g2f i, float2 offset, int channel){
	float4 parallaxMap = tex2D(_PackedMap, i.uv.xy+offset);
	float height = 1;
	UNITY_BRANCH
	switch(channel){
		case 0: height = parallaxMap.r; break;
		case 1: height = parallaxMap.g; break;
		case 2: height = parallaxMap.b; break;
		case 3: height = parallaxMap.a; break;
		default: break;
	}
	return height;
}

#define STEPS 15
float2 GetParallaxOffset(g2f i){
    float2 uvOffset = 0;
	float2 prevUVOffset = 0;
	float stepSize = 1.0/STEPS;
	float stepHeight = 1;
	float2 uvDelta = i.tangentViewDir.xy * (stepSize * _Parallax);
	float surfaceHeight = 0;
	UNITY_BRANCH
	if (_RenderMode == 2 && _PBRWorkflow == 2) 
		surfaceHeight = SampleParallaxMap(i, 0, _HeightChannel);
	else surfaceHeight = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap, _MainTex, i.uv.xy);
	surfaceHeight = clamp(surfaceHeight, 0, 0.999);
	float prevStepHeight = stepHeight;
	float prevSurfaceHeight = surfaceHeight;

	UNITY_BRANCH
	if (_RenderMode == 2 && _PBRWorkflow == 2){
		UNITY_UNROLL
		for (int j = 1; j < STEPS && stepHeight > surfaceHeight; j++){
			prevUVOffset = uvOffset;
			prevStepHeight = stepHeight;
			prevSurfaceHeight = surfaceHeight;
			uvOffset -= uvDelta;
			stepHeight -= stepSize;
			surfaceHeight = SampleParallaxMap(i, uvOffset, _HeightChannel);
		}
	}
	else {
		UNITY_UNROLL
		for (int j = 1; j < STEPS && stepHeight > surfaceHeight; j++){
			prevUVOffset = uvOffset;
			prevStepHeight = stepHeight;
			prevSurfaceHeight = surfaceHeight;
			uvOffset -= uvDelta;
			stepHeight -= stepSize;
			surfaceHeight = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap, _MainTex, i.uv.xy+uvOffset);
		}
	}

	float prevDifference = prevStepHeight - prevSurfaceHeight;
	float difference = surfaceHeight - stepHeight;
	float t = prevDifference / (prevDifference + difference);
	uvOffset = lerp(prevUVOffset, uvOffset, t);
    return uvOffset;
}
#undef STEPS

float3 GetTangentViewDir(g2f i){
    i.tangentViewDir = normalize(i.tangentViewDir);
    i.tangentViewDir.xy /= (i.tangentViewDir.z + 0.42);
    return i.tangentViewDir;
}

// Parallax Mapping
void ApplyParallax(inout g2f i){
    #if defined(_PARALLAXMAP)
		UNITY_BRANCH
		if (_Parallax > 0){
			i.tangentViewDir = GetTangentViewDir(i);
			float2 parallaxOffset = GetParallaxOffset(i);
			i.uv.xy += parallaxOffset;
			i.uv.zw += parallaxOffset;
			i.uv1.xy += parallaxOffset;
			i.uv1.zw += parallaxOffset;
			i.uv2.xy += parallaxOffset;
			i.normal.xy += parallaxOffset;
		}
    #endif
}

//----------------------------
// Toon Mode Masking
//----------------------------
masks GetMasks(g2f i){
	masks m = (masks)1;
	#if !defined(OUTLINE)
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
				m.smoothMask = SampleMask(_SmoothShadeMask, i.uv.xy, _SmoothShadeMaskChannel, _SHStr > 0);
				m.matcapMask = SampleMask(_MatcapMask, i.uv.xy, _MatcapMaskChannel, _MatcapToggle == 1);
			}
			else if (_MaskingMode == 1){
				float3 mask0 = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMask0, _MainTex, i.uv.xy).rgb;
				float3 mask1 = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMask1, _MainTex, i.uv.xy).rgb;

				m.reflectionMask = mask0.r;
				m.specularMask = mask0.r;
				m.matcapMask = mask0.g;
				m.shadowMask = mask0.b;

				m.rimMask = mask1.r;
				m.detailMask = mask1.g;
				m.ddMask = mask1.b;
				m.smoothMask = mask1.b;
			}
			else {
				float4 mask0 = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMask0, _MainTex, i.uv.xy);
				float4 mask1 = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMask1, _MainTex, i.uv.xy);

				m.reflectionMask = mask0.r;
				m.specularMask = mask0.g;
				m.matcapMask = mask0.b;
				m.shadowMask = mask0.a;

				m.rimMask = mask1.r;
				m.detailMask = mask1.g;
				m.ddMask = mask1.b;
				m.smoothMask = mask1.a;
			}
		}
		else {
			m.detailMask = SampleMask(_DetailMask, i.uv.xy, _DetailMaskChannel, true);
		}
	#else
		m.detailMask = SampleMask(_DetailMask, i.uv.xy, _DetailMaskChannel, true);
	#endif
	return m;
}