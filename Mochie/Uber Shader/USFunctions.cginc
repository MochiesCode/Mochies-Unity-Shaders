#ifndef US_FUNCTIONS_INCLUDED
#define US_FUNCTIONS_INCLUDED

//----------------------------
// Color Filtering
//----------------------------

void ApplyGeneralFilters(inout float3 albedo){
    albedo = GetSaturation(albedo, _Saturation);
    albedo = lerp(albedo, GetHDR(albedo), _HDR);
	albedo = lerp(albedo, ACES(albedo), _ACES);
    albedo = GetContrast(albedo, _Contrast);
    albedo *= _Brightness;
}

void ApplyHSVFilter(inout float3 albedo){
	_Hue += lerp(0, frac(_Time.y*_AutoShiftSpeed), _AutoShift);
	if (_Hue > 0 && _Hue < 1)
		albedo = HSVShift(albedo, _Hue, 0, 0);
}

void ApplyTeamColors(masks m, inout float3 albedo){;

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
}

void ApplyFiltering(g2f i, masks m, inout float3 albedo){
	float3 albedoOut = albedo;
	ApplyTeamColors(m, albedoOut);
	ApplyHSVFilter(albedoOut);
	albedoOut *= _RGB;
	ApplyGeneralFilters(albedoOut);
	albedo = lerp(albedo, albedoOut, m.filterMask);
}

//------------------------------------
// Albedo
//------------------------------------

void ApplyCutout(g2f i, float2 screenUV, inout float4 albedo){
	if (_BlendMode == 1){
		albedo.a = (albedo.a - _Cutoff) / max(fwidth(albedo.a), 0.0001) + 0.5;
	}
	else if (_BlendMode == 2){
		clip(Dither(screenUV, albedo.a));
	}
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

float3 GetSpritesheetUV(float2 uv, float2 rowsColumns, float2 scroll, float scrubPos, float fps, int manualScrub){
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
	return float3(uv,0);
}

float3 GetFlipbookUV(Texture2DArray flipbook, SamplerState ss, float2 uv, float2 scroll, float scrubPos, float fps, int manualScrub){
	float width, height, elements;
	flipbook.GetDimensions(width, height, elements);
	uint arrayIndex = frac(_Time.y*fps*(1/elements))*elements;
	uint index = lerp(arrayIndex, scrubPos, manualScrub);
	return float3(uv, index);
}

float4 GetSpritesheetColor(g2f i, 
		Texture2D tex, Texture2DArray flipbook, SamplerState ss, float4 spriteColor,
		float2 pos, float2 scale, float2 rowsColumns, float2 fco, float2 scroll,
		float rot, float scrubPos, float fps, float brightness, int manualScrub, int mode, int clipEdge
	) {

	float2 scrolledUV = frac(i.rawUV.xy + (_Time.y * scroll));
	float2 scaledUV = ScaleUV(scrolledUV, pos, scale, rot);
	float3 uv = 0;
	float4 col = 0;

	UNITY_BRANCH
	if (mode == 0){
		uv = GetFlipbookUV(flipbook, ss, scaledUV, scroll, scrubPos, fps, manualScrub);
	}
	else uv = GetSpritesheetUV(scaledUV, rowsColumns, scroll, scrubPos, fps, manualScrub);
		
	UNITY_BRANCH
	if (mode == 1)
		col = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_MainTex, uv.xy) * spriteColor * brightness * FrameClip(scaledUV, rowsColumns, fco);
	else
		col = MOCHIE_SAMPLE_TEX2DARRAY_SAMPLER(flipbook, ss, uv) * spriteColor * brightness * lerp(1, FrameClip(scrolledUV, 1, 0.99), clipEdge);

	return col;
}

void ApplySpritesheetBlending(g2f i, inout float4 col, float4 gifCol, int blendMode){
	col.rgb = BlendColors(col.rgb, gifCol.rgb, blendMode, gifCol.a);
	if (_UseSpritesheetAlpha == 1){
		col.a = gifCol.a;
		#if ALPHA_TEST
			if (_BlendMode != 2)
				clip(col.a - _Cutoff);
		#endif
	}
}

void ApplySpritesheet0(g2f i, inout float4 col){
	float4 spriteCol = GetSpritesheetColor(i, 
		_Spritesheet, _Flipbook0, sampler_Flipbook0,
		_SpritesheetCol, _SpritesheetPos, _SpritesheetScale,
		_RowsColumns, _FrameClipOfs, _Flipbook0Scroll, _SpritesheetRot, _ScrubPos, _FPS,
		_SpritesheetBrightness, _ManualScrub, _SpritesheetMode0, _Flipbook0ClipEdge
	);
	ApplySpritesheetBlending(i, col, spriteCol, _SpritesheetBlending);
}

void ApplySpritesheet1(g2f i, inout float4 col){
	float4 spriteCol = GetSpritesheetColor(i, 
		_Spritesheet1, _Flipbook1, sampler_Flipbook1, 
		_SpritesheetCol1, _SpritesheetPos1, _SpritesheetScale1,
		_RowsColumns1, _FrameClipOfs1, _Flipbook1Scroll, _SpritesheetRot1, _ScrubPos1, _FPS1,
		_SpritesheetBrightness1, _ManualScrub1, _SpritesheetMode1, _Flipbook1ClipEdge
	);
	ApplySpritesheetBlending(i, col, spriteCol, _SpritesheetBlending1);
}

void ApplyBCDissolve(g2f i, audioLinkData al, inout float4 albedo, out float3 bcRimColor){
	bcRimColor = 0;
	#if BCDISSOLVE_ENABLED
		float2 texUV = TRANSFORM_TEX(i.rawUV, _MainTex2);
		float2 noiseUV = TRANSFORM_TEX(i.rawUV, _BCNoiseTex);
		float4 albedo2 = MOCHIE_SAMPLE_TEX2D_SAMPLER(_MainTex2, sampler_MainTex, texUV) * _BCColor;
		float noise = MOCHIE_SAMPLE_TEX2D_SAMPLER(_BCNoiseTex, sampler_MainTex, noiseUV);
		#if AUDIOLINK_ENABLED
			float bcDissolveValueAL = GetAudioLinkBand(al, _AudioLinkBCDissolveBand, _AudioLinkRemapBCDissolveMin, _AudioLinkRemapBCDissolveMax);
			_BCDissolveStr *= lerp(1, bcDissolveValueAL, _AudioLinkBCDissolveMultiplier*_AudioLinkStrength);
		#endif
		float dissolveStr = noise - _BCDissolveStr;
		float rimInner = step(dissolveStr, _BCRimWidth*0.035);
		float rimOuter = step(dissolveStr+_BCRimWidth*0.035, _BCRimWidth*0.035);
		float3 rim = (rimInner - rimOuter) * _BCRimCol;
		albedo = lerp(albedo2, albedo, ceil(dissolveStr));
		bcRimColor = rim * _BCRimCol.a;
	#endif
}

float3 GetDetailAlbedo(g2f i, float3 albedo){
	float3 detailAlbedo = MOCHIE_SAMPLE_TEX2D_SAMPLER(_DetailAlbedoMap, sampler_MainTex, i.uv2.xy);
	return BlendColors(albedo, detailAlbedo, _DetailAlbedoBlending);
}

float4 GetAlbedo(g2f i, lighting l, masks m, audioLinkData al){
	float4 mainTex = UNITY_SAMPLE_TEX2D(_MainTex, i.uv.xy);
	
	float4 albedo = 1;
	cubeMask = 1;
	
	#if !CUBEMAP_ENABLED && !COMBINED_CUBEMAP_ENABLED
		albedo = mainTex;
		float4 mirrorTex = MOCHIE_SAMPLE_TEX2D_SAMPLER(_MirrorTex, sampler_MainTex, i.uv.xy);
		if (i.isReflection && _MirrorBehavior == 2)
			albedo = mirrorTex;
		albedo *= _Color;

	#elif CUBEMAP_ENABLED
		albedo = mainTex * 2;
		_CubeRotate0 = lerp(_CubeRotate0, _CubeRotate0 *_Time.y, _AutoRotate0);
		float3 vDir = Rotate3D(l.viewDir, _CubeRotate0);
		albedo = texCUBE(_MainTexCube0, vDir);
		albedo *= _CubeColor0;

	#elif COMBINED_CUBEMAP_ENABLED
		_CubeRotate0 = lerp(_CubeRotate0, _CubeRotate0 *_Time.y, _AutoRotate0);
		float3 vDir = Rotate3D(l.viewDir, _CubeRotate0);
		float4 albedo0 = mainTex;
		float4 mirrorTex = MOCHIE_SAMPLE_TEX2D_SAMPLER(_MirrorTex, sampler_MainTex, i.uv.xy);
		if (i.isReflection && _MirrorBehavior == 2)
			albedo0 = mirrorTex;
		float4 albedo1 = texCUBE(_MainTexCube0, vDir);
		albedo0 *= _Color;
		albedo1 *= _CubeColor0;
		cubeMask = lerp(_CubeBlend, MOCHIE_SAMPLE_TEX2D_SAMPLER(_CubeBlendMask, sampler_MainTex, i.rawUV.xy).r, _IsCubeBlendMask); 
		albedo.rgb = BlendColors(albedo0, albedo1, _CubeBlendMode, cubeMask);
	#endif

	ApplyBCDissolve(i, al, albedo, bcRimColor);

	#if SHADING_ENABLED
		float detailInterp = _DetailAlbedoStrength * m.detailMask * _UsingDetailAlbedo;
		albedo.rgb = lerp(albedo.rgb, GetDetailAlbedo(i, albedo.rgb), detailInterp);
	#endif

	#if NON_OPAQUE_RENDERING
		if (_UseAlphaMask == 1){
			float2 alphaMaskUV = TRANSFORM_TEX(i.rawUV, _AlphaMask);
			float4 alphaMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_AlphaMask, sampler_MainTex, alphaMaskUV) * _Color.a;
			albedo.a = ChannelCheck(alphaMask, _AlphaMaskChannel);
		}
	#endif

	#if SPRITESHEETS_ENABLED
		if (_EnableSpritesheet == 1 && _UnlitSpritesheet == 0)
			ApplySpritesheet0(i, albedo);
		if (_EnableSpritesheet1 == 1 && _UnlitSpritesheet1 == 0)
			ApplySpritesheet1(i, albedo);
	#endif

	#if FILTERING_ENABLED
		albedo.rgb = lerp(albedo.rgb, smootherstep(1,0,albedo.rgb), _Invert);
		#if !POST_FILTERING_ENABLED
			ApplyFiltering(i, m, albedo.rgb);
		#endif
	#endif

	albedo *= lerp(1, i.color, _VertexColor);
	
    return albedo;
}

//----------------------------
// Emission/Rim
//----------------------------
float GetPulse(g2f i){
	float pulse = 1;
	[flatten]
	switch (_PulseWaveform){
		case 0: pulse = 0.5*(sin(_Time.y * _PulseSpeed)+1); break; 			// Sin
		case 1: pulse = round((sin(_Time.y * _PulseSpeed)+1)*0.5); break; 	// Square
		case 2: pulse = abs((_Time.y * (_PulseSpeed * 0.333)%2)-1); break; 	// Triangle
		case 3: pulse = frac(_Time.y * (_PulseSpeed * 0.2)); break; 		// Saw
		case 4: pulse = 1-frac(_Time.y * (_PulseSpeed * 0.2)); break; 		// Reverse Saw
		default: break;
	}
	float mask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_PulseMask, sampler_MainTex, i.uv.xy);
	pulse = lerp(1, pulse, _PulseStr*mask);
	return pulse;
}

float3 GetEmission(g2f i, masks m, audioLinkData al){
	float3 emiss = MOCHIE_SAMPLE_TEX2D(_EmissionMap, i.uv.zw).rgb * _EmissionColor.rgb * _EmissIntensity;
	float2 emiss2uv = ScaleOffsetScrollUV(i.rawUV, _EmissionMap2_ST.xy, _EmissionMap2_ST.zw, _EmissScroll2);
	float3 emiss2 = MOCHIE_SAMPLE_TEX2D(_EmissionMap2, emiss2uv).rgb * _EmissionColor2.rgb * _EmissIntensity2;
	emiss += emiss2;
	emiss *= m.emissMask;
	#if !OUTLINE_PASS
		#if PULSE_ENABLED
			emiss *= GetPulse(i);
		#endif
		#if AUDIOLINK_ENABLED
			float emissValueAL = GetAudioLinkBand(al, _AudioLinkEmissionBand, _AudioLinkRemapEmissionMin, _AudioLinkRemapEmissionMax);
			emiss *= lerp(1, emissValueAL, _AudioLinkEmissionMultiplier*_AudioLinkStrength);
		#endif
	#endif
	return emiss;
}

#if SHADING_ENABLED
float GetRim(lighting l, float width){
	float rim = GetFresnel(l.VdotL, width, _RimEdge);
	#if AUDIOLINK_ENABLED
		audioLinkData ral = (audioLinkData)0;
		float VVRdotL = abs(dot(l.viewDirVR, l.normal));
		InitializeAudioLink(ral, 1-VVRdotL);
		float pulseValueAL = 1-GetAudioLinkBand(ral, _AudioLinkRimBand, _AudioLinkRemapRimMin, _AudioLinkRemapRimMax);
		float pulseRim = pow((1-pulseValueAL), (1-_AudioLinkRimPulseWidth) * 10);
		pulseRim = smoothstep(_AudioLinkRimPulseSharp, 1-_AudioLinkRimPulseSharp, pulseRim);
		rim += (pulseRim * _AudioLinkRimPulse * _AudioLinkStrength);
	#endif
	return rim;
}

void ApplyRimLighting(g2f i, lighting l, masks m, audioLinkData al, inout float3 diffuse){
	#if AUDIOLINK_ENABLED
		float rimValueAL = GetAudioLinkBand(al, _AudioLinkRimBand, _AudioLinkRemapRimMin, _AudioLinkRemapRimMax);
		_RimWidth *= lerp(1, rimValueAL, _AudioLinkRimWidth);
	#endif
	float rim = GetRim(l, _RimWidth);
	rim *= m.rimMask;
	float3 rimCol = MOCHIE_SAMPLE_TEX2D_SAMPLER(_RimTex, sampler_MainTex, i.uv2.zw).rgb * _RimCol.rgb;
	float interpolator = rim*_RimStr*lerp(l.worldBrightness, 1, _UnlitRim);
	#if AUDIOLINK_ENABLED
		interpolator *= lerp(1, rimValueAL, _AudioLinkRimMultiplier*_AudioLinkStrength);
	#endif
	diffuse = BlendColors(diffuse, rimCol, _RimBlending, interpolator);
}
#endif

//----------------------------
// Workflows
//----------------------------


float3 GetMetallicWorkflow(g2f i, lighting l, masks m, float3 albedo){
	metallic = lerp(_Metallic, MOCHIE_SAMPLE_TEX2D_SAMPLER(_MetallicGlossMap, sampler_MainTex, i.uv.xy), _UseMetallicMap);
	metallic = lerp(metallic, Remap(metallic, 0, 1, _MetallicRemapMin, _MetallicRemapMax), _MetallicFiltering);
	ApplyPBRFiltering(metallic, _MetallicContrast, _MetallicIntensity, _MetallicLightness, _MetallicFiltering, prevMetal);
	roughness = lerp(_Glossiness, MOCHIE_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, sampler_MainTex, i.uv.xy), _UseSpecMap);
	roughness = lerp(roughness, GetDetailRough(i, roughness), _DetailRoughStrength * m.detailMask * _UsingDetailRough);
	roughness = lerp(roughness, Remap(roughness, 0, 1, _RoughRemapMin, _RoughRemapMax), _RoughnessFiltering);
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
		float4 specMap = MOCHIE_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, sampler_MainTex, i.uv.xy);
		specularTint = specMap.rgb;
		if (_PBRWorkflow == 1){
			if (_UseSmoothMap == 1){
				smoothness = MOCHIE_SAMPLE_TEX2D_SAMPLER(_SmoothnessMap, sampler_MainTex, i.uv.xy).r * _GlossMapScale;
				smoothness = lerp(smoothness, Remap(smoothness, 0, 1, _SmoothRemapMin, _SmoothRemapMax), _SmoothnessFiltering);
				ApplyPBRFiltering(smoothness, _SmoothContrast, _SmoothIntensity, _SmoothLightness, _SmoothnessFiltering, prevSmooth);
			}
			else smoothness = _GlossMapScale;
		}
		else {
			smoothness = specMap.a * _GlossMapScale;
			smoothness = lerp(smoothness, Remap(smoothness, 0, 1, _SmoothRemapMin, _SmoothRemapMax), _SmoothnessFiltering);
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
	metallic = lerp(metallic, Remap(metallic, 0, 1, _MetallicRemapMin, _MetallicRemapMax), _MetallicFiltering);
	ApplyPBRFiltering(metallic, _MetallicContrast, _MetallicIntensity, _MetallicLightness, _MetallicFiltering, prevMetal);

	roughness = lerp(roughness, GetDetailRough(i, roughness), _DetailRoughStrength * m.detailMask * _UsingDetailRough);
	roughness = lerp(roughness, Remap(roughness, 0, 1, _RoughRemapMin, _RoughRemapMax), _RoughnessFiltering);
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

void InitializeModularChannels(){
	metallic = ChannelCheck(packedTex, _MetallicChannel);
	roughness = ChannelCheck(packedTex, _RoughnessChannel);
}

float3 GetWorkflow(g2f i, lighting l, masks m, float3 albedo){
	float3 diffuse = albedo;
	#if DEFAULT_WORKFLOW
		diffuse = GetMetallicWorkflow(i, l, m, albedo);
	#elif SPECULAR_WORKFLOW
		diffuse = GetSpecWorkflow(i, l, m, albedo);
	#elif PACKED_WORKFLOW
		InitializeModularChannels();
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

void ApplyMetallicPreview(inout float3 diffuse){
	diffuse = lerp(diffuse, prevMetal, _MetallicFiltering * _PreviewMetallic);
}

//----------------------------
// UV Distortion
//----------------------------
float2 GetTorusOffset(float2 uv) {
    // translated to hlsl and modified from https://www.shadertoy.com/view/Md3Bz7
    // http://web.cs.ucdavis.edu/~amenta/s12/findnorm.pdf
    float phi = UNITY_TWO_PI*uv.x;
    float theta = UNITY_TWO_PI*uv.y;
    float3 c = cos(float3(phi, phi + UNITY_HALF_PI, theta));
    float2 result = float2(c.x*c.z,-c.y*c.z);
    return result * 0.5 + 0.5;  
}

float2 GetSimplexOffset(g2f i){
	float xOfs = GetSimplex3D(i.rawUV.xy, _NoiseScale, _Time.y*_NoiseSpeed, _NoiseOctaves) * _DistortUVStr;
	float yOfs = GetSimplex3D(i.rawUV.xy, _NoiseScale, (_Time.y+43.423984)*_NoiseSpeed, _NoiseOctaves) * _DistortUVStr;
	return float2(xOfs, yOfs);
}

float3 GetUVOffset(g2f i){

	_DistortUVStr *= MOCHIE_SAMPLE_TEX2D_SAMPLER(_DistortUVMask, sampler_MainTex, i.uv.xy);
	float3 ofs = 0;

	#if UV_DISTORTION_NORMALMAP
		ofs = UnpackScaleNormal(MOCHIE_SAMPLE_TEX2D_SAMPLER(_DistortUVMap, sampler_MainTex, i.uv3.xy), _DistortUVStr);
	#else
		ofs.xy = GetSimplexOffset(i);
	#endif

	ofs *= 0.1;
	uvOffsetOut = ofs;
	
	return ofs;
}

void ApplyUVDistortion(inout g2f i, audioLinkData al, inout float3 uvOffset){
	uvOffset = GetUVOffset(i);
	#if AUDIOLINK_ENABLED
		float alValue = GetAudioLinkBand(al, _AudioLinkUVDistortionBand, _AudioLinkRemapUVDistortionMin, _AudioLinkRemapUVDistortionMax);
		uvOffset *= lerp(1, alValue, _AudioLinkUVDistortionMultiplier * _AudioLinkStrength);
	#endif
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
	float stepSize = 1.0/_ParallaxSteps;
	float stepHeight = 1;
	float2 uvDelta = i.tangentViewDir.xy * (stepSize * _Parallax);
	float surfaceHeight = 0;

	#if PACKED_WORKFLOW
		packedTex = MOCHIE_SAMPLE_TEX2D_SAMPLER(_PackedMap, sampler_MainTex, i.uv.xy);
		surfaceHeight = ChannelCheck(packedTex, _HeightChannel)+_ParallaxOffset;
		surfaceHeight = clamp(surfaceHeight, 0, 0.999);
		float prevStepHeight = stepHeight;
		float prevSurfaceHeight = surfaceHeight;

		[unroll(50)]
		for (int j = 1; j <= _ParallaxSteps && stepHeight > surfaceHeight; j++){
			prevUVOffset = uvOffset;
			prevStepHeight = stepHeight;
			prevSurfaceHeight = surfaceHeight;
			uvOffset -= uvDelta;
			stepHeight -= stepSize;
			surfaceHeight = ChannelCheck(MOCHIE_SAMPLE_TEX2D_SAMPLER(_PackedMap, sampler_MainTex, i.uv.xy+uvOffset), _HeightChannel)+_ParallaxOffset;
			surfaceHeight = lerp(surfaceHeight, Remap(surfaceHeight, 0, 1, _HeightRemapMin, _HeightRemapMax), _HeightFiltering);
			ApplyPBRFiltering(surfaceHeight, _HeightContrast, _HeightIntensity, _HeightLightness, _HeightFiltering, prevHeight);
		}

		[unroll(3)]
		for (int k = 0; k < 3; k++) {
			uvDelta *= 0.5;
			stepSize *= 0.5;

			if (stepHeight < surfaceHeight) {
				uvOffset += uvDelta;
				stepHeight += stepSize;
			}
			else {
				uvOffset -= uvDelta;
				stepHeight -= stepSize;
			}
			surfaceHeight = ChannelCheck(MOCHIE_SAMPLE_TEX2D_SAMPLER(_PackedMap, sampler_MainTex, i.uv.xy+uvOffset), _HeightChannel)+_ParallaxOffset;
			surfaceHeight = lerp(surfaceHeight, Remap(surfaceHeight, 0, 1, _HeightRemapMin, _HeightRemapMax), _HeightFiltering);
			ApplyPBRFiltering(surfaceHeight, _HeightContrast, _HeightIntensity, _HeightLightness, _HeightFiltering, prevHeight);
		}
	#else
		surfaceHeight = MOCHIE_SAMPLE_TEX2D_SAMPLER(_ParallaxMap, sampler_MainTex, i.uv.xy+uvOffset)+_ParallaxOffset;
		surfaceHeight = clamp(surfaceHeight, 0, 0.999);
		float prevStepHeight = stepHeight;
		float prevSurfaceHeight = surfaceHeight;

		[unroll(50)]
		for (int j = 1; j <= _ParallaxSteps && stepHeight > surfaceHeight; j++){
			prevUVOffset = uvOffset;
			prevStepHeight = stepHeight;
			prevSurfaceHeight = surfaceHeight;
			uvOffset -= uvDelta;
			stepHeight -= stepSize;
			surfaceHeight = MOCHIE_SAMPLE_TEX2D_SAMPLER(_ParallaxMap, sampler_MainTex, i.uv.xy+uvOffset)+_ParallaxOffset;
			surfaceHeight = lerp(surfaceHeight, Remap(surfaceHeight, 0, 1, _HeightRemapMin, _HeightRemapMax), _HeightFiltering);
			ApplyPBRFiltering(surfaceHeight, _HeightContrast, _HeightIntensity, _HeightLightness, _HeightFiltering, prevHeight);
		}
		
		[unroll(3)]
		for (int k = 0; k < 3; k++) {
			uvDelta *= 0.5;
			stepSize *= 0.5;

			if (stepHeight < surfaceHeight) {
				uvOffset += uvDelta;
				stepHeight += stepSize;
			}
			else {
				uvOffset -= uvDelta;
				stepHeight -= stepSize;
			}
			surfaceHeight = MOCHIE_SAMPLE_TEX2D_SAMPLER(_ParallaxMap, sampler_MainTex, i.uv.xy+uvOffset)+_ParallaxOffset;
			surfaceHeight = lerp(surfaceHeight, Remap(surfaceHeight, 0, 1, _HeightRemapMin, _HeightRemapMax), _HeightFiltering);
			ApplyPBRFiltering(surfaceHeight, _HeightContrast, _HeightIntensity, _HeightLightness, _HeightFiltering, prevHeight);
		}
	#endif
	
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
    }
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
		metallic = lerp(_Metallic, MOCHIE_SAMPLE_TEX2D_SAMPLER(_MetallicGlossMap, sampler_MainTex, i.uv.xy), _UseMetallicMap);
		omr = unity_ColorSpaceDielectricSpec.a - metallic * unity_ColorSpaceDielectricSpec.a;
	#elif SPECULAR_WORKFLOW
		float3 specularTint = MOCHIE_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, sampler_MainTex, i.uv.xy).rgb;
		omr = 1-max(max(specularTint.r, specularTint.g), specularTint.b);
	#elif PACKED_WORKFLOW
		float4 packedTex = MOCHIE_SAMPLE_TEX2D_SAMPLER(_PackedMap, sampler_MainTex, i.uv.xy);
		metallic = ChannelCheck(packedTex, _MetallicChannel);
		omr = unity_ColorSpaceDielectricSpec.a - metallic * unity_ColorSpaceDielectricSpec.a;
	#endif
	return omr;
}

float ShadowPremultiplyAlpha(g2f i, float alpha){
	float omr = GetOneMinusReflectivity(i);
	alpha = 1-omr + alpha*omr;
	return alpha;
}

void NearClip(g2f i){
    if (_NearClipToggle == 1 && !i.isReflection){
        #if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
            float camDist = distance(i.worldPos, (unity_StereoWorldSpaceCameraPos[0].xyz + unity_StereoWorldSpaceCameraPos[1].xyz)*0.5);
        #else
            float camDist = distance(i.worldPos, _WorldSpaceCameraPos.xyz);
        #endif
        float ncMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_NearClipMask, sampler_MainTex, i.rawUV);
        if (camDist < _NearClip && ncMask > 0.5){
            discard;
        }
    }
}

void MirrorClip(g2f i){
	if ((i.isReflection && _MirrorBehavior == 3) ||  (!i.isReflection && _MirrorBehavior == 1))
		discard;
}

//----------------------------
// Mask Sampling
//----------------------------
masks GetMasks(g2f i){
	masks m = (masks)1;
	m.anisoMask = 0;
	float2 refractDissUV = i.uv.xy;

	// Separate
	#if SEPARATE_MASKING
		float2 reflUV, specUV, refractUV, erimUV, matcapUV, matcapBlendUV, anisoUV, subsurfUV, rimUV;
		float2 detailUV, shadowUV, diffuseUV, filterUV, teamUV, emissUV, emissPulseUV;
		reflUV = refractUV = specUV = erimUV = matcapUV = matcapBlendUV = anisoUV = subsurfUV = rimUV = i.uv.xy;
		detailUV = shadowUV = diffuseUV = filterUV = teamUV = emissUV = emissPulseUV = i.uv.xy;
		#if SHADING_ENABLED
			#if !OUTLINE_PASS
				#if MASK_SOS_ENABLED
					reflUV = TRANSFORM_TEX(i.rawUV, _ReflectionMask) + (_Time.y*_ReflectionMaskScroll);
					specUV = TRANSFORM_TEX(i.rawUV, _SpecularMask) + (_Time.y*_SpecularMaskScroll);
					erimUV = TRANSFORM_TEX(i.rawUV, _ERimMask) + (_Time.y*_ERimMaskScroll);
					matcapUV = TRANSFORM_TEX(i.rawUV, _MatcapMask) + (_Time.y*_MatcapMaskScroll);
					matcapBlendUV = TRANSFORM_TEX(i.rawUV, _MatcapBlendMask) + (_Time.y*_MatcapBlendMaskScroll);
					anisoUV = TRANSFORM_TEX(i.rawUV, _InterpMask) + (_Time.y*_InterpMaskScroll);
					subsurfUV = TRANSFORM_TEX(i.rawUV, _SubsurfaceMask) + (_Time.y*_SubsurfaceMaskScroll);
					rimUV = TRANSFORM_TEX(i.rawUV, _RimMask) + (_Time.y*_RimMaskScroll);
					refractUV = TRANSFORM_TEX(i.rawUV, _RefractionMask) + (_Time.y*_RefractionMaskScroll);
				#endif
				#if REFLECTIONS_ENABLED
					m.reflectionMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_ReflectionMask, sampler_MainTex, reflUV);
				#endif
				#if SPECULAR_ENABLED
					m.specularMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_SpecularMask, sampler_MainTex, specUV);
				#endif
				#if ENVIRONMENT_RIM_ENABLED
					m.eRimMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_ERimMask, sampler_MainTex, erimUV);
				#endif
				#if MATCAP_ENABLED
					m.matcapPrimMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_MatcapMask, sampler_MainTex, matcapUV);
					m.matcapSecMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_MatcapBlendMask, sampler_MainTex, matcapBlendUV);
				#endif
				#if COMBINED_SPECULAR
					m.anisoMask = 1-MOCHIE_SAMPLE_TEX2D_SAMPLER(_InterpMask, sampler_MainTex, anisoUV);
				#endif
				#if REFRACTION_ENABLED
					refractDissUV = TRANSFORM_TEX(i.rawUV, _RefractionDissolveMask) + (_Time.y*_RefractionDissolveMaskScroll);
					m.refractMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_RefractionMask, sampler_MainTex, refractUV);
					m.refractDissolveMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_RefractionDissolveMask, sampler_MainTex, refractDissUV);
				#endif
				#if SUBSURFACE_ENABLED
					m.subsurfMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_SubsurfaceMask, sampler_MainTex, subsurfUV);
				#endif
				m.rimMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_RimMask, sampler_MainTex, rimUV);
			#endif
			#if MASK_SOS_ENABLED
				detailUV = TRANSFORM_TEX(i.rawUV, _DetailMask) + (_Time.y*_DetailMaskScroll);
				shadowUV = TRANSFORM_TEX(i.rawUV, _ShadowMask) + (_Time.y*_ShadowMaskScroll);
				diffuseUV = TRANSFORM_TEX(i.rawUV, _DiffuseMask) + (_Time.y*_DiffuseMaskScroll);
				filterUV = TRANSFORM_TEX(i.rawUV, _FilterMask) + (_Time.y*_FilterMaskScroll);
				teamUV = TRANSFORM_TEX(i.rawUV, _TeamColorMask) + (_Time.y*_TeamColorMaskScroll);
				emissUV = TRANSFORM_TEX(i.rawUV, _EmissMask) + (_Time.y*_EmissMaskScroll);
				emissPulseUV = TRANSFORM_TEX(i.rawUV, _EmissPulseMask) + (_Time.y*_EmissPulseMaskScroll);
			#endif
			m.detailMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_DetailMask, sampler_MainTex, detailUV);
			m.shadowMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_ShadowMask, sampler_MainTex, shadowUV);
			m.diffuseMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_DiffuseMask, sampler_MainTex, diffuseUV);
		#endif
		#if FILTERING_ENABLED
			#if OUTLINE_PASS
				m.filterMask = lerp(MOCHIE_SAMPLE_TEX2D_SAMPLER(_FilterMask, sampler_MainTex, filterUV), 1, _IgnoreFilterMask);
			#else
				m.filterMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_FilterMask, sampler_MainTex, filterUV);
			#endif
			m.teamMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_TeamColorMask, sampler_MainTex, teamUV);
		#endif
		#if EMISSION_ENABLED
			m.emissMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_EmissMask, sampler_MainTex, emissUV);
			m.emissPulseMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_PulseMask, sampler_MainTex, emissPulseUV);
		#endif

	// Packed
	#elif PACKED_MASKING
		#if SHADING_ENABLED
			#if !OUTLINE_PASS
				float4 mask0 = MOCHIE_SAMPLE_TEX2D_SAMPLER(_PackedMask0, sampler_MainTex, i.uv.xy);
				m.reflectionMask = mask0.r;
				m.specularMask = mask0.g;
				m.matcapPrimMask = mask0.b;
				m.matcapSecMask = mask0.a;
				float4 mask2 = MOCHIE_SAMPLE_TEX2D_SAMPLER(_PackedMask2, sampler_MainTex, i.uv.xy);
				m.rimMask = mask2.r;
				m.eRimMask = mask2.g;
				m.refractMask = mask2.b;
				m.anisoMask = mask2.a;
			#endif
			float4 mask1 = MOCHIE_SAMPLE_TEX2D_SAMPLER(_PackedMask1, sampler_MainTex, i.uv.xy);
			m.shadowMask = mask1.r;
			m.diffuseMask = mask1.g;
			m.subsurfMask = mask1.b;
			m.detailMask = mask1.a;
		#endif
		float4 mask3 = MOCHIE_SAMPLE_TEX2D_SAMPLER(_PackedMask3, sampler_MainTex, i.uv.xy);
		m.emissMask = mask3.r;
		m.emissPulseMask = mask3.g;
		#if OUTLINE_PASS
			m.filterMask = lerp(mask3.b, 1, _IgnoreFilterMask);
		#else
			m.filterMask = mask3.b;
		#endif
		#if FILTERING_ENABLED
			m.teamMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_TeamColorMask, sampler_MainTex, i.uv.xy);
		#endif
	#elif FILTERING_ENABLED
		m.teamMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_TeamColorMask, sampler_MainTex, i.uv.xy);
	#endif

	#if REFRACTION_ENABLED
		refractDissUV = TRANSFORM_TEX(i.rawUV, _RefractionDissolveMask) + (_Time.y*_RefractionDissolveMaskScroll);
		m.refractDissolveMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_RefractionDissolveMask, sampler_MainTex, refractDissUV);
	#endif

	#if SHADING_ENABLED
		if (_Iridescence == 1){
			m.iridescenceMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_IridescenceMask, sampler_MainTex, i.rawUV);
		}
	#endif

	return m;
}

#endif // US_FUNCTIONS_INCLUDED