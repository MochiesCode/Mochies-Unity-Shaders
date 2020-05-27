//----------------------------
// Color Filtering
//----------------------------

float3 ApplyGeneralFilters(float3 albedo){
    albedo = GetSaturation(albedo, _Saturation);
    albedo = lerp(albedo, GetHDR(albedo), _HDR);
    albedo = GetContrast(albedo, _Contrast);
    albedo += albedo*_Brightness;
	return albedo;
}

float3 ApplyTeamColors(masks m, float3 albedo, float2 uv0){
	float3 baseCol = albedo;
	float4 teamMask = UNITY_SAMPLE_TEX2D_SAMPLER(_TeamColorMask, _MainTex, uv0);

	// Alloy team colors implementation
	float weight = dot(teamMask, float4(1.0h, 1.0h, 1.0h, 1.0h));
	teamMask /= max(1.0h, weight);
	float3 teamColor = _TeamColor0 * teamMask.r 
					+ _TeamColor1 * teamMask.g 
					+ _TeamColor2 * teamMask.b 
					+ _TeamColor3 * teamMask.a 
					+ saturate(1.0h - weight).rrr;
	albedo *= teamColor;

	albedo.rgb = ApplyGeneralFilters(albedo);
	albedo = lerp(baseCol, albedo, m.filterMask*2);
	return albedo;
}

float3 GetHSLFilter(masks m, float3 albedo){
    float3 baseCol = albedo;
    UNITY_BRANCH
    if (_AutoShift == 1)
        _Hue += frac(_Time.y*_AutoShiftSpeed);
    float3 shift = float3(_Hue, 0, _Luminance);
    float3 hsl = RGBtoHSL(albedo);
    float hslRange = step(_HSLMin, hsl) * step(hsl, _HSLMax);
    albedo = HSLtoRGB(hsl + shift * hslRange);
	albedo = ApplyGeneralFilters(albedo);
	albedo = lerp(baseCol, albedo, m.filterMask);
    return albedo;
}

float3 GetHSVFilter(masks m, float3 albedo){
    float3 baseCol = albedo;
    UNITY_BRANCH
    if (_AutoShift == 1)
        _Hue += frac(_Time.y*_AutoShiftSpeed);
    float3 shift = float3(_Hue, 0, _Value);
    float3 hsv = RGBtoHSV(albedo);
    float hsvRange = step(_HSLMin, hsv) * step(hsv, _HSLMax);
    albedo = HSVtoRGB(hsv + shift * hsvRange);
	albedo = ApplyGeneralFilters(albedo);
	albedo = lerp(baseCol, albedo, m.filterMask);
    return albedo;
}

float3 GetRGBFilter(masks m, float3 albedo){
    float3 baseCol = albedo;
    albedo.r *= _RAmt;
    albedo.g *= _GAmt;
    albedo.b *= _BAmt;
	albedo.rgb = ApplyGeneralFilters(albedo);
	albedo = lerp(baseCol, albedo, m.filterMask);
    return albedo;
}

//------------------------------------
// Albedo/Diffuse/Emission/Rim/GIF
//------------------------------------

void ApplyCutout(float alpha){
	#if defined(_ALPHATEST_ON)
        UNITY_BRANCH
        if (_ATM != 1)
            clip(alpha - _Cutoff);
    #endif
}

float2 ScaleUV(float2 uv, float2 pos,  float2 scale, float rot){
	uv -= pos + 0.5;
	uv = Rotate2D(uv, rot) + 0.5;
	uv = (uv - 0.5) / scale + 0.5;
    return uv;
}

bool FrameClip(float2 uv, float2 rowsColumns, float2 fco){
	float2 size = float2(1/rowsColumns.x, 1/rowsColumns.y)-fco;
	bool xClip = uv.x < size.x || uv.x > 1-size.x;
	bool yClip = uv.y < size.y || uv.y > 1-size.y;
	return !(xClip || yClip);
}

float2 GetSpritesheetUV(float2 uv, float2 rowsColumns, float scrubPos, float fps, int manualScrub){
	float2 size = float2(1/rowsColumns.x, 1/rowsColumns.y);
	uint totalFrames = rowsColumns.x * rowsColumns.y;
	uint index = 0;

	UNITY_BRANCH
	if (manualScrub == 1)
		index = scrubPos;
	else
		index = _Time.y*fps;

	uint indexX = index % rowsColumns.x;
	uint indexY = floor((index % totalFrames) / rowsColumns.x);
	float2 offset = float2(size.x*indexX,-size.y*indexY);
	float2 uv1 = uv*size;
	uv1.y = uv1.y + size.y*(rowsColumns.y - 1);
	uv = uv1 + offset;
	return uv;
}

float4 GetSpritesheetCol(g2f i, 
		sampler2D tex, float4 spriteColor,
		float2 pos, float2 scale, float2 rowsColumns, float2 fco, 
		float rot, float scrubPos, float fps, int manualScrub
	) {
	float4 gifCol = 0;
	UNITY_BRANCH
	if (_EnableSpritesheet == 1){
		float2 scaledUV = ScaleUV(i.uv.xy, pos, scale, rot);
		float2 uv = GetSpritesheetUV(scaledUV, rowsColumns, scrubPos, fps, manualScrub);
		gifCol = tex2D(tex, uv) * spriteColor * FrameClip(scaledUV, rowsColumns, fco);
	}
	return gifCol;
}

float4 ApplySpritesheet(g2f i, masks m, float4 col, float4 gifCol, int blendMode){
	
	float interpolator = gifCol.a * m.filterMask;
	UNITY_BRANCH
	if (blendMode == 0){
		#if defined(_ALPHABLEND_ON) || defined(_ALPHATEST_ON) || defined(_ALPHAPREMULTIPLY_ON)
			gifCol.a = AverageRGB(gifCol.rgb) > 0.01;
		#endif
		col += gifCol * interpolator;
	}
	else if (blendMode == 1){
		col.rgb *= lerp(1, gifCol.rgb, interpolator);
		#if defined(_ALPHABLEND_ON) || defined(_ALPHATEST_ON) || defined(_ALPHAPREMULTIPLY_ON)
			col.a = lerp(col.a, gifCol.a, interpolator);
		#endif
	}
	else 
		col = lerp(col, gifCol, interpolator);
	return col;
}

float4 ApplyUnlitSpritesheet(g2f i, masks m, float4 diffuse){
	UNITY_BRANCH
	if (_EnableSpritesheet == 1 && _UnlitSpritesheet == 1){
		float4 spriteCol = GetSpritesheetCol(i, 
			_Spritesheet,
			_SpritesheetCol,
			_SpritesheetPos,
			_SpritesheetScale,
			_RowsColumns,
			_FrameClipOfs,
			_SpritesheetRot,
			_ScrubPos,
			_FPS,
			_ManualScrub
		);
		diffuse = ApplySpritesheet(i, m, diffuse, spriteCol, _SpritesheetBlending);
		UNITY_BRANCH
		if (_EnableSpritesheet1 == 1){
			spriteCol = GetSpritesheetCol(i, 
				_Spritesheet1,
				_SpritesheetCol1,
				_SpritesheetPos1,
				_SpritesheetScale1,
				_RowsColumns1,
				_FrameClipOfs1,
				_SpritesheetRot1,
				_ScrubPos1,
				_FPS1,
				_ManualScrub1
			);
			diffuse = ApplySpritesheet(i, m, diffuse, spriteCol, _SpritesheetBlending1);
		}
		ApplyCutout(diffuse.a);
	}
	else if (_EnableSpritesheet == 0 && _UnlitSpritesheet == 1){
		ApplyCutout(diffuse.a);
	}
	return diffuse;
}

float4 ApplyLitSpritesheet(g2f i, masks m, float4 albedo){
	UNITY_BRANCH
	if (_EnableSpritesheet == 1 && _UnlitSpritesheet == 0){
		float4 spriteCol = GetSpritesheetCol(i, 
			_Spritesheet,
			_SpritesheetCol,
			_SpritesheetPos,
			_SpritesheetScale,
			_RowsColumns,
			_FrameClipOfs,
			_SpritesheetRot,
			_ScrubPos,
			_FPS,
			_ManualScrub
		);
		albedo = ApplySpritesheet(i, m, albedo, spriteCol, _SpritesheetBlending);
		UNITY_BRANCH
		if (_EnableSpritesheet1 == 1){
			spriteCol = GetSpritesheetCol(i, 
				_Spritesheet1,
				_SpritesheetCol1,
				_SpritesheetPos1,
				_SpritesheetScale1,
				_RowsColumns1,
				_FrameClipOfs1,
				_SpritesheetRot1,
				_ScrubPos1,
				_FPS1,
				_ManualScrub1
			);
			albedo = ApplySpritesheet(i, m, albedo, spriteCol, _SpritesheetBlending1);
		}
	}
	return albedo;
}

float3 GetDetailAlbedo(g2f i, lighting l, masks m, float3 col){
    float3 detailAlbedo = UNITY_SAMPLE_TEX2D_SAMPLER(_DetailAlbedoMap, _MainTex, i.uv2.xy).rgb * unity_ColorSpaceDouble;
    float3 albedo = lerp(col, col*detailAlbedo, m.detailMask);
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
		cubeMask = SampleCubeMask(_CubeBlendMask, i.uv.xy, _CubeBlend, _CubeBlendMaskChannel, _IsCubeBlendMask); 
		albedo.rgb = BlendCubemap(albedo0, albedo1, cubeMask, _CubeBlendMode);
	}

    albedo.rgb = GetDetailAlbedo(i, l, m, albedo);

	#if defined(_ALPHABLEND_ON) || defined(_ALPHATEST_ON) || defined(_ALPHAPREMULTIPLY_ON)
		UNITY_BRANCH
		if (_UseAlphaMask == 1)
			albedo.a = SampleMask(_AlphaMask, i.uv.xy, _AlphaMaskChannel, true);
	#endif

	albedo = ApplyLitSpritesheet(i, m, albedo);

	UNITY_BRANCH
	if (_PostFiltering == 0 && _FilterModel > 0){
		UNITY_BRANCH
		if 		(_FilterModel == 1) albedo.rgb = GetRGBFilter(m, albedo.rgb);
		else if (_FilterModel == 2) albedo.rgb = GetHSLFilter(m, albedo.rgb);
		else if (_FilterModel == 3) albedo.rgb = GetHSVFilter(m, albedo.rgb);
		else if (_FilterModel == 4) albedo.rgb = ApplyTeamColors(m, albedo.rgb, i.uv.xy);
	}

	UNITY_BRANCH
	if 		(_BlendMode == 2) albedo.a *= _Color.a;
	else if (_BlendMode == 3) albedo.a = _Color.a;

    return albedo;
}

float4 GetDiffuse(lighting l, float4 albedo, float atten){
    float4 diffuse;
    float3 lightCol = atten * l.directCol + l.indirectCol;
    diffuse.rgb = albedo.rgb;
    diffuse.rgb *= lerp(lightCol, 1, cubeMask*_UnlitCube*_CubeMode == 1);
	diffuse.rgb = lerp(diffuse.rgb, clamp(diffuse.rgb, 0, albedo.rgb), _ColorPreservation);
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
	#if defined(UNITY_PASS_FORWARDBASE)
		UNITY_BRANCH
		if (_EmissionToggle == 1){
			emiss = UNITY_SAMPLE_TEX2D(_EmissionMap, i.uv.zw).rgb * _EmissionColor.rgb;
			emiss *= SampleMask(_EmissMask, i.uv.xy, _EmissMaskChannel, true);
			emiss *= GetPulse(i);
		}
	#endif
	return emiss;
}

float3 ApplyRimLighting(g2f i, lighting l, masks m, float3 diffuse){
	#if defined(UNITY_PASS_FORWARDBASE)
    UNITY_BRANCH
    if (_RenderMode == 1 && _RimLighting == 1){
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

float3 ApplyERimLighting(g2f i, lighting l, masks m, float3 diffuse, float roughness){
	#if defined(UNITY_PASS_FORWARDBASE)
    UNITY_BRANCH
    if (_RenderMode == 1 && _EnvironmentRim == 1){
		float3 reflCol = GetERimReflections(i, l, roughness);
        float VdotL = abs(dot(l.viewDir, l.normal));
        float rim = pow((1-VdotL), (1-_ERimWidth) * 10);
        rim = smootherstep(_ERimEdge, (1-_ERimEdge), rim);
        rim *= m.eRimMask;
        float3 rimCol = reflCol * _ERimTint.rgb;
        float interpolator = rim*_ERimStr;

		[forcecase]
		switch (_ERimBlending){
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
// Toon Workflows
//----------------------------
float3 GetMetallicWorkflow(g2f i, lighting l, masks m, float3 albedo, out float3 specularTint, out float smoothness, out float omr){
	float metallic = _Metallic;
	UNITY_BRANCH
	if (_UseMetallicMap == 1)
		metallic = UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicGlossMap, _MainTex, i.uv.xy);
	smoothness = _Glossiness;
	UNITY_BRANCH
	if (_UseSpecMap == 1)
		smoothness = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, _MainTex, i.uv.xy);
	UNITY_BRANCH
	if (_RoughnessFiltering == 1){
		smoothness = saturate(lerp(0.5, smoothness, _RoughContrast));
		smoothness += saturate(smoothness * _RoughIntensity);
		smoothness = saturate(smoothness + _RoughLightness);
	}
	smoothness = 1-smoothness;
	specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
	omr = unity_ColorSpaceDielectricSpec.a - metallic * unity_ColorSpaceDielectricSpec.a;
	float interpolator = _ReflectionStr*m.reflectionMask;
	return lerp(albedo, albedo*omr, interpolator);
}

float3 GetSpecWorkflow(g2f i, lighting l, masks m, float3 albedo, out float3 specularTint, out float smoothness, out float omr){
	UNITY_BRANCH
	if (_UseSpecMap == 1){
		float4 specMap = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, _MainTex, i.uv.xy);
		specularTint = specMap.rgb;
		UNITY_BRANCH
		if (_PBRWorkflow == 1){
			UNITY_BRANCH
			if (_UseSmoothMap == 1){
				smoothness = UNITY_SAMPLE_TEX2D_SAMPLER(_SmoothnessMap, _MainTex, i.uv.xy).r * _GlossMapScale;
				UNITY_BRANCH
				if (_SmoothnessFiltering == 1){
					smoothness = lerp(smoothness, pow(smoothness, 0.454545), _LinearSmooth);
					smoothness = saturate(lerp(0.5, smoothness, _SmoothContrast));
					smoothness += saturate(smoothness * _SmoothIntensity);
					smoothness = saturate(smoothness + _SmoothLightness);
				}
			}
			else smoothness = _GlossMapScale;
		}
		else {
			smoothness = specMap.a * _GlossMapScale;
			UNITY_BRANCH
			if (_SmoothnessFiltering == 1){
				smoothness = saturate(lerp(0.5, smoothness, _SmoothContrast));
				smoothness += saturate(smoothness * _SmoothIntensity);
				smoothness = saturate(smoothness + _SmoothLightness);
			}
		}
	}
	else {
		specularTint = _SpecCol.rgb;
		smoothness = _GlossMapScale;
	}
	
	omr = 1-max(max(specularTint.r, specularTint.g), specularTint.b);
	albedo = albedo * (float3(1,1,1) - specularTint);
	return albedo;
}

float3 GetPackedWorkflow(g2f i, lighting l, masks m, float3 albedo, out float3 specularTint, out float smoothness, out float omr){
	float4 packedTex = tex2D(_PackedMap, i.uv.xy);
	metallic = ChannelCheck(packedTex, _MetallicChannel);
	roughness = ChannelCheck(packedTex, _RoughnessChannel);
	UNITY_BRANCH
	if (_RoughnessFiltering == 1){
		roughness = saturate(lerp(0.5, roughness, _RoughContrast));
		roughness += saturate(roughness * _RoughIntensity);
		roughness = saturate(roughness + _RoughLightness);
	}
	smoothness = 1-roughness;
	specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
	omr = unity_ColorSpaceDielectricSpec.a - metallic * unity_ColorSpaceDielectricSpec.a;
	float interpolator = _ReflectionStr*m.reflectionMask;
	return lerp(albedo, albedo*omr, interpolator);
}

float3 GetWorkflow(g2f i, lighting l, masks m, float3 albedo, out float3 specularTint, out float smoothness, out float omr){
	float3 diffuse = albedo;
	UNITY_BRANCH
	if (_PBRWorkflow == 0){
		diffuse = GetMetallicWorkflow(i, l, m, albedo, specularTint, smoothness, omr);
	}
	else if (_PBRWorkflow == 3){
		diffuse = GetPackedWorkflow(i, l, m, albedo, specularTint, smoothness, omr);
	}
	else diffuse = GetSpecWorkflow(i, l, m, albedo, specularTint, smoothness, omr);

	return diffuse;
}

// PBR filtering previews
float3 ApplyRoughPreview(g2f i, float3 diffuse){
	UNITY_BRANCH
	if (_UseSpecMap == 1){
		UNITY_BRANCH
		if (_RoughnessFiltering == 1 && _PreviewRough == 1){
			if (_PBRWorkflow != 3)
				diffuse = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, _MainTex, i.uv.xy);
			else {
				float3 packedTex = tex2D(_PackedMap, i.uv.xy);
				diffuse = ChannelCheck(packedTex, _RoughnessChannel);
			}
			diffuse = saturate(lerp(0.5, diffuse, _RoughContrast));
			diffuse += saturate(diffuse * _RoughIntensity);
			diffuse = saturate(diffuse + _RoughLightness);
		}
	}
	else {
		UNITY_BRANCH
		if (_PackedRoughPreview == 1){
			float3 packedTex = tex2D(_PackedMap, i.uv.xy);
			diffuse = ChannelCheck(packedTex, _RoughnessChannel);
			diffuse = saturate(lerp(0.5, diffuse, _RoughContrast));
			diffuse += saturate(diffuse * _RoughIntensity);
			diffuse = saturate(diffuse + _RoughLightness);
		}
	}
	return diffuse;
}

float3 ApplySmoothPreview(float3 diffuse){
	UNITY_BRANCH
	if (_SmoothnessFiltering == 1 && _PreviewSmooth == 1)
		diffuse = smoothness;
	return diffuse;
}

float3 ApplyAOPreview(lighting l, float3 diffuse){
	UNITY_BRANCH
	if (_AOFiltering == 1 && _PreviewAO == 1)
		diffuse = l.ao;
	return diffuse;
}

float3 ApplyHeightPreview(g2f i, float3 diffuse){
	UNITY_BRANCH
	if (_HeightFiltering == 1 && _PreviewHeight == 1){
		diffuse = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap, _MainTex, i.uv.xy);
		diffuse = saturate(lerp(0.5, diffuse, _HeightContrast));
		diffuse += saturate(diffuse * _HeightIntensity);
		diffuse = saturate(diffuse + _HeightLightness);
	}
	return diffuse;
}

//----------------------------
// UV Distortion
//----------------------------
// float2 GetCurlOffset(g2f i){
// 	float xOfs = GetCurl3D(i.uv.xy, _NoiseMinMax, _NoiseScale, _Time.y*_NoiseSpeed) * _DistortUVStr;
// 	float yOfs = GetCurl3D(i.uv.xy, _NoiseMinMax, _NoiseScale, (_Time.y+43.423984)*_NoiseSpeed) * _DistortUVStr;
// 	return float2(xOfs, yOfs);
// }

float2 GetSimplexOffset(g2f i){
	float xOfs = GetSimplex3D(i.uv.xy, _NoiseMinMax, _NoiseScale, _Time.y*_NoiseSpeed, _NoiseOctaves) * _DistortUVStr;
	float yOfs = GetSimplex3D(i.uv.xy, _NoiseMinMax, _NoiseScale, (_Time.y+43.423984)*_NoiseSpeed, _NoiseOctaves) * _DistortUVStr;
	return float2(xOfs, yOfs);
}

float3 GetUVOffset(g2f i){
	_DistortUVStr *= SampleMask(_DistortUVMask, i.uv.xy, _DistortUVMaskChannel, _DistortUVStr > 0);
	float3 ofs = 0;

	// UNITY_BRANCH
	// switch(_DistortionStyle){
	// 	case 0: ofs = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_DistortUVMap, _MainTex, i.uv4.zw), _DistortUVStr); break;
	// 	case 1: ofs.xy = GetSimplexOffset(i); break;
	// 	case 2: ofs.xy = GetCurlOffset(i); break;
	// 	default: break;
	// }

	UNITY_BRANCH
	if (_DistortionStyle == 0)
		ofs = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_DistortUVMap, _MainTex, i.uv4.zw), _DistortUVStr);
	else 
		ofs.xy = GetSimplexOffset(i);

	return ofs * 0.1;
}

void ApplyUVDistortion(inout g2f i, inout float3 uvOffset){
	UNITY_BRANCH
	if (_DistortUVs == 1) {
		uvOffset = GetUVOffset(i);
		i.uv.xy += uvOffset.xy * _DistortMainUV;
		i.uv.zw += uvOffset.xy * _DistortEmissUV;
		i.uv2.xy += uvOffset.xy * _DistortDetailUV;
		i.uv2.zw += uvOffset.xy * _DistortRimUV;
	}
}

float3 ApplyNoisePreview(g2f i, float3 diffuse){
	UNITY_BRANCH
	if (_DistortUVs == 1 && _PreviewNoise == 1){
		float red = saturate(uvOffset.x - uvOffset.y);
		float green = saturate(uvOffset.y - uvOffset.x);
		float blue = (red + green);
		diffuse = float3(red, green, blue)*5;
	}
	return diffuse;
}

//----------------------------
// Parallax Mapping
//----------------------------
float2 GetParallaxOffset(g2f i){
    float2 uvOffset = 0;
	float2 prevUVOffset = 0;
	float stepSize = 1.0/15.0;
	float stepHeight = 1;
	float2 uvDelta = i.tangentViewDir.xy * (stepSize * _Parallax);
	float surfaceHeight = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap, _MainTex, i.uv.xy);
	surfaceHeight = clamp(surfaceHeight, 0, 0.999);
	float prevStepHeight = stepHeight;
	float prevSurfaceHeight = surfaceHeight;
	[unroll(15)]
	for (int j = 1; j < 15 && stepHeight > surfaceHeight; j++){
		prevUVOffset = uvOffset;
		prevStepHeight = stepHeight;
		prevSurfaceHeight = surfaceHeight;
		uvOffset -= uvDelta;
		stepHeight -= stepSize;
		surfaceHeight = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap, _MainTex, i.uv.xy+uvOffset);
		UNITY_BRANCH
		if (_HeightFiltering == 1){
			surfaceHeight = saturate(lerp(0.5, surfaceHeight, _HeightContrast));
			surfaceHeight += saturate(surfaceHeight * _HeightIntensity);
			surfaceHeight = saturate(surfaceHeight + _HeightLightness);
		}
	}
	float prevDifference = prevStepHeight - prevSurfaceHeight;
	float difference = surfaceHeight - stepHeight;
	float t = prevDifference / (prevDifference + difference);
	uvOffset = lerp(prevUVOffset, uvOffset, t);
    return uvOffset;
}

float3 GetTangentViewDir(g2f i){
    i.tangentViewDir = normalize(i.tangentViewDir);
    i.tangentViewDir.xy /= (i.tangentViewDir.z + 0.42);
    return i.tangentViewDir;
}

// Parallax Mapping
void ApplyParallax(inout g2f i){
    UNITY_BRANCH
	if (_RenderMode == 1 && _UseParallaxMap == 1 && _Parallax > 0){
		i.tangentViewDir = GetTangentViewDir(i);
		float2 parallaxOffset = GetParallaxOffset(i);
		i.uv.xy += parallaxOffset;
		i.uv.zw += parallaxOffset;
		i.uv1.xy += parallaxOffset;
		i.uv1.zw += parallaxOffset;
		i.uv2.xy += parallaxOffset;
		i.normal.xy += parallaxOffset;
    }
}

//----------------------------
// Mask Sampling
//----------------------------
masks GetMasks(g2f i){
	masks m = (masks)1;

	m.detailMask = SampleMask(_DetailMask, i.uv.xy, _DetailMaskChannel, true);
	m.spriteMask = SampleMask(_SpritesheetMask, i.uv.xy, _SpritesheetMaskChannel, _EnableSpritesheet);
	m.filterMask = SampleMask(_FilterMask, i.uv.xy, _FilterMaskChannel, _FilterModel);

	UNITY_BRANCH
	if (_MaskingMode != 0){

		// Separate
		if (_MaskingMode == 1){
			#if !defined(OUTLINE)
				m.reflectionMask = SampleMask(_ReflectionMask, i.uv.xy, _ReflectionMaskChannel, _Reflections);
				m.specularMask = SampleMask(_SpecularMask, i.uv.xy, _SpecularMaskChannel, _Specular);
			#endif
			m.shadowMask = SampleMask(_ShadowMask, i.uv.xy, _ShadowMaskChannel, _Shadows);
			m.rimMask = SampleMask(_RimMask, i.uv.xy, _RimMaskChannel, _RimLighting);
			m.eRimMask = SampleMask(_ERimMask, i.uv.xy, _ERimMaskChannel, _EnvironmentRim);
			m.ddMask = SampleMask(_DDMask, i.uv.xy, _DDMaskChannel, _DisneyDiffuse > 0);
			m.anisoMask = 1-SampleMask(_InterpMask, i.uv.xy, _InterpMaskChannel, _SpecularStyle == 2);
			m.smoothMask = SampleMask(_SmoothShadeMask, i.uv.xy, _SmoothShadeMaskChannel, _SHStr > 0);
			m.matcapMask = SampleMask(_MatcapMask, i.uv.xy, _MatcapMaskChannel, _MatcapToggle);
		}

		// Packed RGB
		else if (_MaskingMode == 2){
			float3 mask0 = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMask0, _MainTex, i.uv.xy).rgb;
			float3 mask1 = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMask1, _MainTex, i.uv.xy).rgb;

			m.reflectionMask = mask0.r;
			m.specularMask = mask0.r;
			m.matcapMask = mask0.g;
			m.shadowMask = mask0.b;

			m.rimMask = mask1.r;
			m.eRimMask = mask1.g;
			m.ddMask = mask1.b;
			m.smoothMask = mask1.b;
		}
			
		// Packed RGBA
		else if (_MaskingMode == 3){
			float4 mask0 = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMask0, _MainTex, i.uv.xy);
			float4 mask1 = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMask1, _MainTex, i.uv.xy);

			m.reflectionMask = mask0.r;
			m.specularMask = mask0.g;
			m.matcapMask = mask0.b;
			m.shadowMask = mask0.a;

			m.rimMask = mask1.r;
			m.eRimMask = mask1.g;
			m.ddMask = mask1.b;
			m.smoothMask = mask1.a;
		}
	}
	return m;
}