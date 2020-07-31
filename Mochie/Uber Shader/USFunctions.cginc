//----------------------------
// Color Filtering
//----------------------------

void ApplyGeneralFilters(inout float3 albedo){
    albedo = GetSaturation(albedo, _Saturation);
    albedo = lerp(albedo, GetHDR(albedo), _HDR);
    albedo = GetContrast(albedo, _Contrast);
    albedo *= _Brightness;
}

float3 GetTeamColors(masks m, float3 albedo){;

	// Alloy team colors implementation
	if (_TeamFiltering == 1){
		float3 baseCol = albedo;
		float weight = dot(m.teamMask, float4(1.0h, 1.0h, 1.0h, 1.0h));
		m.teamMask /= max(1.0h, weight);
		float3 teamColor = _TeamColor0 * m.teamMask.r 
						+ _TeamColor1 * m.teamMask.g 
						+ _TeamColor2 * m.teamMask.b 
						+ _TeamColor3 * m.teamMask.a 
						+ saturate(1.0h - weight).rrr;
		albedo *= teamColor;
		albedo = baseCol + 2 * (albedo-baseCol);
	}
	return albedo;
}

void ApplyFiltering(g2f i, masks m, inout float3 albedo){
	float3 rgb = GetTeamColors(m, albedo) * _RGB;
	_Hue += lerp(0, frac(_Time.y*_AutoShiftSpeed), _AutoShift);
	float3 hsv = HSVShift(rgb, _Hue, 0, 0);
	ApplyGeneralFilters(hsv);
	albedo = lerp(albedo, hsv, m.filterMask);
}

//------------------------------------
// Albedo
//------------------------------------

void ApplyCutout(float2 screenUV, float alpha){
	if (_BlendMode == 1)
		clip(alpha - _Cutoff);
	else if (_BlendMode != 3)
		clip(Dither(screenUV, alpha));
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

	index = lerp(_Time.y*fps, scrubPos, manualScrub);

	uint indexX = index % rowsColumns.x;
	uint indexY = floor((index % totalFrames) / rowsColumns.x);
	float2 offset = float2(size.x*indexX,-size.y*indexY);
	float2 uv1 = uv*size;
	uv1.y = uv1.y + size.y*(rowsColumns.y - 1);
	uv = uv1 + offset;
	return uv;
}

float4 GetSpritesheetColor(g2f i, 
		sampler2D tex, float4 spriteColor,
		float2 pos, float2 scale, float2 rowsColumns, float2 fco, 
		float rot, float scrubPos, float fps, int manualScrub
	) {
	float2 scaledUV = ScaleUV(i.rawUV, pos, scale, rot);
	float2 uv = GetSpritesheetUV(scaledUV, rowsColumns, scrubPos, fps, manualScrub);
	return tex2D(tex, uv) * spriteColor * FrameClip(scaledUV, rowsColumns, fco);
}

void ApplySpritesheetBlending(g2f i, inout float3 col, float4 gifCol, int blendMode){
	if (blendMode == 0){
		col.rgb += (gifCol.rgb * gifCol.a); 
	}
	else if (blendMode == 1){
		col.rgb *= lerp(1, gifCol.rgb, gifCol.a);
	}
	else col.rgb = lerp(col.rgb, gifCol.rgb, gifCol.a);
}

void ApplySpritesheet0(g2f i, inout float3 albedo){
	float4 spriteCol = GetSpritesheetColor(i, 
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
	ApplySpritesheetBlending(i, albedo, spriteCol, _SpritesheetBlending);
}

void ApplySpritesheet1(g2f i, inout float3 albedo){
	float4 spriteCol = GetSpritesheetColor(i, 
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
	ApplySpritesheetBlending(i, albedo, spriteCol, _SpritesheetBlending1);
}

float4 GetAlbedo(g2f i, lighting l, masks m){
	float4 mainTex =  UNITY_SAMPLE_TEX2D(_MainTex, i.uv.xy);
	float3 detailAlbedo = UNITY_SAMPLE_TEX2D_SAMPLER(_DetailAlbedoMap, _MainTex, i.uv2.xy).rgb * unity_ColorSpaceDouble;
	float4 albedo = 1;
	cubeMask = 1;

	#if !CUBEMAP_ENABLED && !COMBINED_CUBEMAP_ENABLED
		albedo = mainTex;
		float4 mirrorTex = UNITY_SAMPLE_TEX2D_SAMPLER(_MirrorTex, _MainTex, i.uv.xy);
		#if !X_FEATURES
			if (i.isReflection && _MirrorBehavior == 2)
				albedo = mirrorTex;
		#else
			if (i.isReflection && _UseMirrorAlbedo == 1)
				albedo = mirrorTex;
		#endif
		albedo *= _Color;

	#elif CUBEMAP_ENABLED
		albedo = mainTex * 2;
		_CubeRotate0 = lerp(_CubeRotate0, _CubeRotate0 *_Time.y, _AutoRotate0);
		float3 vDir = Rotate(l.viewDir, _CubeRotate0);
		albedo = texCUBE(_MainTexCube0, vDir);

		// prevent compiler from optimizing out mainTex, which would mean we lose the sampler
		albedo += (mainTex*0.000001);

		albedo *= _CubeColor0;

	#elif COMBINED_CUBEMAP_ENABLED
		_CubeRotate0 = lerp(_CubeRotate0, _CubeRotate0 *_Time.y, _AutoRotate0);
		float3 vDir = Rotate(l.viewDir, _CubeRotate0);
		float4 albedo0 = mainTex; 
		float4 albedo1 = texCUBE(_MainTexCube0, vDir);
		albedo0 *= _Color;
		albedo1 *= _CubeColor0;
		cubeMask = SampleCubeMask(_CubeBlendMask, i.uv.xy, _CubeBlend, _IsCubeBlendMask); 
		albedo.rgb = BlendCubemap(albedo0, albedo1, cubeMask, _CubeBlendMode);
	#endif

    
    albedo.rgb = lerp(albedo.rgb, albedo.rgb*detailAlbedo, m.detailMask);

	#if NON_OPAQUE_RENDERING
		float alphaMask = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaMask, _MainTex, i.uv.xy) * _Color.a;
		if (_UseAlphaMask == 1)
			albedo.a = alphaMask;
	#endif

	#if SPRITESHEETS_ENABLED
		if (_EnableSpritesheet == 1 && _UnlitSpritesheet == 0)
			ApplySpritesheet0(i, albedo.rgb);
		if (_EnableSpritesheet1 == 1 && _UnlitSpritesheet1 == 0)
			ApplySpritesheet1(i, albedo.rgb);
	#endif

	#if FILTERING_ENABLED
		albedo.rgb = lerp(albedo.rgb, smootherstep(1,0,albedo.rgb), _Invert);
		#if !POST_FILTERING_ENABLED
			ApplyFiltering(i, m, albedo.rgb);
		#endif
	#endif

    return albedo;
}

//----------------------------
// Emission/Rim
//----------------------------
float GetPulse(g2f i){
	float pulse = 1;
	switch (_PulseWaveform){
		case 0: pulse = 0.5*(sin(_Time.y * _PulseSpeed)+1); break; 			// Sin
		case 1: pulse = round((sin(_Time.y * _PulseSpeed)+1)*0.5); break; 	// Square
		case 2: pulse = abs((_Time.y * (_PulseSpeed * 0.333)%2)-1); break; 	// Triangle
		case 3: pulse = frac(_Time.y * (_PulseSpeed * 0.2)); break; 		// Saw
		case 4: pulse = 1-frac(_Time.y * (_PulseSpeed * 0.2)); break; 	// Reverse Saw
		default: break;
	}
	float mask = UNITY_SAMPLE_TEX2D_SAMPLER(_PulseMask, _MainTex, i.uv.xy);
	pulse = lerp(1, pulse, _PulseStr*mask);
	return pulse;
}

float3 GetEmission(g2f i){
	float3 emiss = UNITY_SAMPLE_TEX2D(_EmissionMap, i.uv.zw).rgb * _EmissionColor.rgb;
	emiss *= UNITY_SAMPLE_TEX2D_SAMPLER(_EmissMask, _MainTex, i.uv.xy);
	#if PULSE_ENABLED && !OUTLINE_PASS
		emiss *= GetPulse(i);
	#endif
	return emiss;
}

void ApplyRimLighting(g2f i, lighting l, masks m, inout float3 diffuse){
	if (_RimLighting == 1){
		float VdotL = abs(dot(l.viewDir, l.normal));
		float rim = pow((1-VdotL), (1-_RimWidth) * 10);
		rim = smoothstep(_RimEdge, (1-_RimEdge), rim);
		rim *= m.rimMask;
		float3 rimCol = UNITY_SAMPLE_TEX2D_SAMPLER(_RimTex, _MainTex, i.uv2.zw).rgb * _RimCol.rgb;
		float interpolator = rim*_RimStr*lerp(l.worldBrightness, 1, _UnlitRim);

		[flatten]
		switch (_RimBlending){
			case 0: diffuse = lerp(diffuse, rimCol, interpolator); break;
			case 1: diffuse += rimCol*interpolator; break;
			case 2: diffuse -= rimCol*interpolator; break;
			case 3: diffuse *= lerp(1, rimCol, interpolator); break;
		}
	}
}

//----------------------------
// Workflows
//----------------------------
float3 GetMetallicWorkflow(g2f i, lighting l, masks m, float3 albedo){

	metallic = lerp(_Metallic, UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicGlossMap, _MainTex, i.uv.xy), _UseMetallicMap);
	roughness = lerp(_Glossiness, UNITY_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, _MainTex, i.uv.xy), _UseSpecMap);

	ApplyPBRFiltering(roughness, _RoughContrast, _RoughIntensity, _RoughLightness, _RoughnessFiltering, prevRough);

	smoothness = 1-roughness;
	specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
	omr = unity_ColorSpaceDielectricSpec.a - metallic * unity_ColorSpaceDielectricSpec.a;
	#if REFLECTIONS_ENABLED
		float reflStr = 1;
		reflStr = _ReflectionStr*m.reflectionMask;
		return lerp(albedo, albedo*omr, reflStr);
	#else
		return albedo;
	#endif
}

float3 GetSpecWorkflow(g2f i, lighting l, masks m, float3 albedo){
	if (_UseSpecMap == 1){
		float4 specMap = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, _MainTex, i.uv.xy);
		specularTint = specMap.rgb;
		if (_PBRWorkflow == 1){
			if (_UseSmoothMap == 1){
				smoothness = UNITY_SAMPLE_TEX2D_SAMPLER(_SmoothnessMap, _MainTex, i.uv.xy).r * _GlossMapScale;
				ApplyPBRFiltering(smoothness, _SmoothContrast, _SmoothIntensity, _SmoothLightness, _SmoothnessFiltering, prevSmooth);
			}
			else smoothness = _GlossMapScale;
		}
		else {
			smoothness = specMap.a * _GlossMapScale;
			ApplyPBRFiltering(smoothness, _SmoothContrast, _SmoothIntensity, _SmoothLightness, _SmoothnessFiltering, prevSmooth);
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

float3 GetPackedWorkflow(g2f i, lighting l, masks m, float3 albedo){
	float4 packedTex = tex2D(_PackedMap, i.uv.xy);
	#if !PACKED_WORKFLOW_BAKED
		metallic = ChannelCheck(packedTex, _MetallicChannel);
		roughness = ChannelCheck(packedTex, _RoughnessChannel);
	#else
		metallic = packedTex.r;
		roughness = packedTex.g;
	#endif
	roughness = pow(roughness, 0.454545);

	ApplyPBRFiltering(roughness, _RoughContrast, _RoughIntensity, _RoughLightness, _RoughnessFiltering, prevRough);

	smoothness = 1-roughness;
	specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
	omr = unity_ColorSpaceDielectricSpec.a - metallic * unity_ColorSpaceDielectricSpec.a;
	
	#if REFLECTIONS_ENABLED
		float reflStr = 1;
		reflStr = _ReflectionStr*m.reflectionMask;
		return lerp(albedo, albedo*omr, reflStr);
	#else
		return albedo;
	#endif
	
}

float3 GetWorkflow(g2f i, lighting l, masks m, float3 albedo){
	float3 diffuse = albedo;
	#if !SPECULAR_WORKFLOW && !PACKED_WORKFLOW && !PACKED_WORKFLOW_BAKED
		diffuse = GetMetallicWorkflow(i, l, m, albedo);
	#elif SPECULAR_WORKFLOW
		diffuse = GetSpecWorkflow(i, l, m, albedo);
	#elif PACKED_WORKFLOW || PACKED_WORKFLOW_BAKED
		diffuse = GetPackedWorkflow(i, l, m, albedo);
	#endif
	return diffuse;
}

// PBR filtering previews
void ApplyRoughPreview(inout float3 diffuse){
	diffuse = lerp(diffuse, prevRough, _RoughnessFiltering * _PreviewRough);
}

void ApplySmoothPreview(inout float3 diffuse){
	diffuse = lerp(diffuse, prevSmooth, _SmoothnessFiltering * _PreviewSmooth);
}

void ApplyAOPreview(inout float3 diffuse){
	diffuse = lerp(diffuse, prevAO, _AOFiltering * _PreviewAO);
}

void ApplyHeightPreview(inout float3 diffuse){
	#if PARALLAX_ENABLED
		diffuse = lerp(diffuse, prevHeight, _HeightFiltering * _PreviewHeight);
	#endif
}

//----------------------------
// UV Distortion
//----------------------------
float2 GetSimplexOffset(g2f i){
	float xOfs = GetSimplex3D(i.rawUV.xy, _NoiseScale, _Time.y*_NoiseSpeed, _NoiseOctaves) * _DistortUVStr;
	float yOfs = GetSimplex3D(i.rawUV.xy, _NoiseScale, (_Time.y+43.423984)*_NoiseSpeed, _NoiseOctaves) * _DistortUVStr;
	return float2(xOfs, yOfs);
}

float3 GetUVOffset(g2f i){

	_DistortUVStr *= UNITY_SAMPLE_TEX2D_SAMPLER(_DistortUVMask, _MainTex, i.uv.xy);
	float3 ofs = 0;

	#if UV_DISTORTION_NORMALMAP
		ofs = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_DistortUVMap, _MainTex, i.uv3.xy), _DistortUVStr);
	#else
		ofs.xy = GetSimplexOffset(i);
	#endif

	ofs *= 0.1;
	uvOffsetOut = ofs;
	
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
		ApplyPBRFiltering(surfaceHeight, _HeightContrast, _HeightIntensity, _HeightLightness, _HeightFiltering, prevHeight);
	}
	prevHeight = surfaceHeight;
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
	if (_RenderMode == 1){
		i.tangentViewDir = GetTangentViewDir(i);
		float2 parallaxOffset = GetParallaxOffset(i);
		i.uv.xy += parallaxOffset;
		i.uv.zw += parallaxOffset;
		i.uv1.xy += parallaxOffset;
		i.uv1.zw += parallaxOffset;
		i.uv2.xy += parallaxOffset;
		i.uv3.xy += parallaxOffset;
		i.uv3.zw += parallaxOffset;
		i.normal.xy += parallaxOffset;
    }
}

//----------------------------
// Mask Sampling
//----------------------------
masks GetMasks(g2f i){
	masks m = (masks)1;

	m.detailMask = UNITY_SAMPLE_TEX2D_SAMPLER(_DetailMask, _MainTex, i.uv.xy);
	m.matcapBlendMask = UNITY_SAMPLE_TEX2D_SAMPLER(_MatcapBlendMask, _MainTex, i.uv.xy);

	#if FILTERING_ENABLED
		m.filterMask = UNITY_SAMPLE_TEX2D_SAMPLER(_FilterMask, _MainTex, i.uv.xy);
		m.teamMask = UNITY_SAMPLE_TEX2D_SAMPLER(_TeamColorMask, _MainTex, i.uv.xy);
	#endif

	#if COMBINED_SPECULAR && !OUTLINE_PASS
		m.anisoMask = 1-UNITY_SAMPLE_TEX2D_SAMPLER(_InterpMask, _MainTex, i.uv.xy);
	#endif

	// Separate
	#if SEPARATE_MASKING
		#if !OUTLINE_PASS
			#if REFLECTIONS_ENABLED
				m.reflectionMask = UNITY_SAMPLE_TEX2D_SAMPLER(_ReflectionMask, _MainTex, i.uv.xy);
			#endif
			#if SPECULAR_ENABLED
				m.specularMask = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularMask, _MainTex, i.uv.xy);
			#endif
			#if ENVIRONMENT_RIM_ENABLED
				m.eRimMask = UNITY_SAMPLE_TEX2D_SAMPLER(_ERimMask, _MainTex, i.uv.xy);
			#endif
			#if MATCAP_ENABLED
				m.matcapMask = UNITY_SAMPLE_TEX2D_SAMPLER(_MatcapMask, _MainTex, i.uv.xy);
			#endif
			m.subsurfMask = UNITY_SAMPLE_TEX2D_SAMPLER(_SubsurfaceMask, _MainTex, i.uv.xy);
			m.rimMask = UNITY_SAMPLE_TEX2D_SAMPLER(_RimMask, _MainTex, i.uv.xy);
		#endif
		m.shadowMask = UNITY_SAMPLE_TEX2D_SAMPLER(_ShadowMask, _MainTex, i.uv.xy);
		m.diffuseMask = UNITY_SAMPLE_TEX2D_SAMPLER(_DiffuseMask, _MainTex, i.uv.xy);

	// Packed
	#elif PACKED_MASKING
		float3 mask0 = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMask0, _MainTex, i.uv.xy);
		float3 mask1 = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMask1, _MainTex, i.uv.xy);
		float3 mask2 = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMask2, _MainTex, i.uv.xy);

		m.reflectionMask = mask0.r;
		m.specularMask = mask0.g;
		m.matcapMask = mask0.b;
		
		m.shadowMask = mask1.r;
		m.diffuseMask = mask1.g;
		m.subsurfMask = mask1.b;

		m.rimMask = mask2.r;
		m.eRimMask = mask2.g;
	#endif

	return m;
}

//----------------------------
// Transparency Stuff
//----------------------------
float4 PremultiplyAlpha(float4 diffuse, float omr){
	float3 diff = diffuse.rgb * diffuse.a;
	float alpha = 1-omr + diffuse.a*omr;
	return float4(diff, alpha);
}

float GetOneMinusReflectivity(g2f i){
	float omr = 0;
	#if DEFAULT_WORKFLOW
		metallic = lerp(_Metallic, UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicGlossMap, _MainTex, i.uv.xy), _UseMetallicMap);
		omr = unity_ColorSpaceDielectricSpec.a - metallic * unity_ColorSpaceDielectricSpec.a;
	#elif SPECULAR_WORKFLOW
		float3 specularTint = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, _MainTex, i.uv.xy).rgb;
		omr = 1-max(max(specularTint.r, specularTint.g), specularTint.b);
	#elif PACKED_WORKFLOW
		float4 packedTex = tex2D(_PackedMap, i.uv.xy);
		metallic = ChannelCheck(packedTex, _MetallicChannel);
		omr = unity_ColorSpaceDielectricSpec.a - metallic * unity_ColorSpaceDielectricSpec.a;
	#elif PACKED_WORKFLOW_BAKED
		metallic = tex2D(_PackedMap, i.uv.xy).r;
		omr = unity_ColorSpaceDielectricSpec.a - metallic * unity_ColorSpaceDielectricSpec.a;
	#endif
	return omr;
}

float ShadowPremultiplyAlpha(g2f i, float alpha){
	float omr = GetOneMinusReflectivity(i);
	alpha = 1-omr + alpha*omr;
	return alpha;
}