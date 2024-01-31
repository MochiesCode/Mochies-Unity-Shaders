#ifndef WATER_FRAG_INCLUDED
#define WATER_FRAG_INCLUDED

float bla[2]; // Literally the stupidest thing I've ever had to do (part 1)

float4 frag(v2f i, bool isFrontFace: SV_IsFrontFace) : SV_Target {

	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	#if defined(UNITY_PASS_SHADOWCASTER)
		return 0;
	#endif

	UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
	atten = FadeShadows(i.worldPos, atten);

	if (_TexCoordSpace == 1){
		_GlobalTexCoordScaleWorld = abs(_GlobalTexCoordScaleWorld);
		float2 worldCoordSelect[3] = {-i.worldPos.xy, -i.worldPos.xz, -i.worldPos.yz}; 
		float2 worldCoords = worldCoordSelect[_TexCoordSpaceSwizzle] * _GlobalTexCoordScaleWorld;
		i.uv.xy = worldCoords;
		i.uvFlow.xy = worldCoords;
	}
	else {
		_GlobalTexCoordScaleUV = abs(_GlobalTexCoordScaleUV);
		i.uv.xy *= _GlobalTexCoordScaleUV;
		i.uvFlow.xy *= _GlobalTexCoordScaleUV;
	}
	
	#if GERSTNER_ENABLED
		if (_RecalculateNormals == 1){
			_NormalStr0 = -_NormalStr0;
			_NormalStr1 = -_NormalStr1;
			_DetailNormalStrength = -_DetailNormalStrength;
			_FoamNormalStrength = -_FoamNormalStrength;
			_NormalMapFlipbookStrength = -_NormalMapFlipbookStrength;
		}
	#endif

	if (_InvertNormals == 1){
		_NormalStr0 = -_NormalStr0;
		_NormalStr1 = -_NormalStr1;
		_DetailNormalStrength = -_DetailNormalStrength;
		_FoamNormalStrength = -_FoamNormalStrength;
		_NormalMapFlipbookStrength = -_NormalMapFlipbookStrength;
	}

	#if VERT_OFFSET_ENABLED
		i.wave.y = saturate(i.wave.y);
	#endif

	float4 detailBC = 0;
	float2 detailUV = i.uv;
	#if DETAIL_BASECOLOR_ENABLED
		detailUV = TRANSFORM_TEX(i.uv, _DetailBaseColor) + (_Time.y * _DetailScroll);
	#elif DETAIL_NORMAL_ENABLED
		detailUV = TRANSFORM_TEX(i.uv, _DetailNormal) + (_Time.y * _DetailScroll);
	#endif
	
	#if DETAIL_BASECOLOR_ENABLED
		detailBC = MOCHIE_SAMPLE_TEX2D_SAMPLER(_DetailBaseColor, sampler_FlowMap, detailUV) * _DetailBaseColorTint;
	#endif

	float3 normalMap;
	float3 detailNormal;
	float2 uvNormal0 = ScaleUV(i.uv, _NormalMapScale0, _NormalMapScroll0);
	float2 uvNormal1 = ScaleUV(i.uv, _NormalMapScale1, _NormalMapScroll1);
	float2 baseUV0 = Rotate2D(uvNormal0, _Rotation0);

	float2 baseUV1 = Rotate2D(uvNormal1, _Rotation1);
	float3 uv00 = float3(baseUV0, 1);
	float3 uv10 = float3(baseUV1, 1);
	#if FOAM_ENABLED
		float2 uvFoam = ScaleUV(i.uv, _FoamTexScale, _FoamTexScroll * 0.1);
		float3 uvF0 = float3(uvFoam, 1);
		float3 uvF1 = uvF0;
	#endif
	#if NORMALMAP_FLIPBOOK_MODE
		float2 normalFlipbookUV = i.uv * _NormalMapFlipbookScale;
		float3 uvNF0 = float3(normalFlipbookUV, 1);
		float3 uvNF1 = uvNF0;
	#endif
	#if EMISSION_ENABLED
		float2 emissionUV = TRANSFORM_TEX(i.uv, _EmissionMap) + (_Time.y * _EmissionMapScroll);
		float3 uvE0 = float3(emissionUV, 1);
		float3 uvE1 = uvE0;
	#endif

	float2 uvFlow = ScaleUV(i.uvFlow, _FlowMapScale, 0);
	float4 flowMap = MOCHIE_SAMPLE_TEX2D(_FlowMap, uvFlow);

	// Literally the stupidest thing I've ever had to do (part 2)
	float dummy = bla[unity_StereoEyeIndex];
    if (_ScreenParams.x == 0)
        flowMap.x += dummy;

	#if FLOW_ENABLED
		float blendNoise = flowMap.a;
		if (_BlendNoiseSource == 1){
			float2 uvBlend = ScaleUV(i.uv, _BlendNoiseScale, 0);
			blendNoise = MOCHIE_SAMPLE_TEX2D_SAMPLER_LOD(_BlendNoise, sampler_FlowMap, uvBlend, 0);
		}
		float2 flow = (flowMap.rg * 2 - 1) * _FlowStrength * 0.1;
		float time = _Time.y * _FlowSpeed + blendNoise;
		uv00 = FlowUV(baseUV0, flow, time, 0);
		float3 uv01 = FlowUV(baseUV0, flow, time, 0.5);
		uv10 = FlowUV(baseUV1, flow, time, 0);
		float3 uv11 = FlowUV(baseUV1, flow, time, 0.5);
		#if FOAM_ENABLED
			uvF0 = FlowUV(uvFoam, flow, time, 0);
			uvF1 = FlowUV(uvFoam, flow, time, 0.5);
		#endif
		#if NORMALMAP_FLIPBOOK_MODE
			uvNF0 = FlowUV(normalFlipbookUV, flow, time, 0);
			uvNF1 = FlowUV(normalFlipbookUV, flow, time, 0.5);
		#endif
		#if EMISSION_ENABLED
			uvE0 = FlowUV(emissionUV, flow, time, 0);
			uvE1 = FlowUV(emissionUV, flow, time, 0.5);
		#endif
	#endif

	#if NORMALMAP_FLIPBOOK_MODE
		#if FLOW_ENABLED
			float4 normalFlipbookSample0 = tex2DflipbookSmooth(_NormalMapFlipbook, sampler_FlowMap, uvNF0.xy, _NormalMapFlipbookSpeed);
			float4 normalFlipbookSample1 = tex2DflipbookSmooth(_NormalMapFlipbook, sampler_FlowMap, uvNF1.xy, _NormalMapFlipbookSpeed);
			float3 normalMap0 = UnpackScaleNormal(normalFlipbookSample0, _NormalMapFlipbookStrength) * uvNF0.z;
			float3 normalMap1 = UnpackScaleNormal(normalFlipbookSample1, _NormalMapFlipbookStrength) * uvNF1.z;
			normalMap = normalize(normalMap0 + normalMap1);
		#else
			float4 normalFlipbook = tex2DflipbookSmooth(_NormalMapFlipbook, sampler_FlowMap, normalFlipbookUV, _NormalMapFlipbookSpeed);
			normalMap = normalize(UnpackScaleNormal(normalFlipbook, _NormalMapFlipbookStrength));
		#endif
	#else
		#if NORMALMAP1_ENABLED
			#if STOCHASTIC0_ENABLED
				_NormalStr0 *= 1.5;
				float4 normalMap0Sample = tex2Dstoch(_NormalMap0, sampler_FlowMap, uv00.xy);
			#else
				float4 normalMap0Sample = MOCHIE_SAMPLE_TEX2D_SAMPLER(_NormalMap0, sampler_FlowMap, uv00.xy);
			#endif
			float3 normalMap0 = UnpackScaleNormal(normalMap0Sample, _NormalStr0) * uv00.z;

			#if FLOW_ENABLED
				#if STOCHASTIC0_ENABLED
					float4 normalMap1Sample = tex2Dstoch(_NormalMap0, sampler_FlowMap, uv01.xy);
				#else
					float4 normalMap1Sample = MOCHIE_SAMPLE_TEX2D_SAMPLER(_NormalMap0, sampler_FlowMap, uv01.xy);
				#endif
				float3 normalMap1 = UnpackScaleNormal(normalMap1Sample, _NormalStr0) * uv01.z;
			#endif
			
			#if STOCHASTIC1_ENABLED
				_NormalStr1 *= 1.5;
				float4 detailNormal0Sample = tex2Dstoch(_NormalMap1, sampler_FlowMap, uv10.xy);
			#else
				float4 detailNormal0Sample = MOCHIE_SAMPLE_TEX2D_SAMPLER(_NormalMap1, sampler_FlowMap, uv10.xy);
			#endif
			float3 detailNormal0 = UnpackScaleNormal(detailNormal0Sample, _NormalStr1) * uv10.z;

			#if FLOW_ENABLED
				#if STOCHASTIC1_ENABLED
					float4 detailNormal1Sample = tex2Dstoch(_NormalMap1, sampler_FlowMap, uv11.xy);
				#else
					float4 detailNormal1Sample = MOCHIE_SAMPLE_TEX2D_SAMPLER(_NormalMap1, sampler_FlowMap, uv11.xy);
				#endif
				float3 detailNormal1 = UnpackScaleNormal(detailNormal1Sample, _NormalStr1) * uv11.z;
				normalMap0 = normalize(normalMap0 + normalMap1);
				detailNormal0 = normalize(detailNormal0 + detailNormal1);
			#endif
			normalMap = BlendNormals(normalMap0, detailNormal0);
		#else
			#if STOCHASTIC0_ENABLED
				_NormalStr0 *= 1.5;
				float4 normalMap0Sample = tex2Dstoch(_NormalMap0, sampler_FlowMap, uv00.xy);
			#else
				float4 normalMap0Sample = MOCHIE_SAMPLE_TEX2D_SAMPLER(_NormalMap0, sampler_FlowMap, uv00.xy);
			#endif
			float3 normalMap0 = UnpackScaleNormal(normalMap0Sample, _NormalStr0) * uv00.z;

			#if FLOW_ENABLED
				#if STOCHASTIC0_ENABLED
					float4 normalMap1Sample = tex2Dstoch(_NormalMap0, sampler_FlowMap, uv01.xy);
				#else
					float4 normalMap1Sample = MOCHIE_SAMPLE_TEX2D_SAMPLER(_NormalMap0, sampler_FlowMap, uv01.xy);
				#endif
				float3 normalMap1 = UnpackScaleNormal(normalMap1Sample, _NormalStr0) * uv01.z;
				normalMap = normalize(normalMap0 + normalMap1);
			#else
				normalMap = normalize(normalMap0);
			#endif
		#endif
	#endif

	#if RAIN_ENABLED
		float3 rainNormal = GetRipplesNormal(i.uv, _RippleScale, _RippleStr, _RippleSpeed, _RippleSize, _RippleDensity);
		normalMap = BlendNormals(normalMap, rainNormal);
	#endif

	#if DETAIL_NORMAL_ENABLED
		float3 detNorm = UnpackScaleNormal(MOCHIE_SAMPLE_TEX2D_SAMPLER(_DetailNormal, sampler_FlowMap, detailUV), _DetailNormalStrength);
		#if DETAIL_BASECOLOR_ENABLED
			normalMap = lerp(normalMap, detNorm, detailBC.a);
		#else
			normalMap = BlendNormals(detNorm, normalMap);
		#endif
	#endif

	i.normal = normalize(dot(i.normal, i.normal) >= 1.01 ? i.cNormal : i.normal);
	float3 earlyNormal = normalize(normalMap.x * i.tangent + normalMap.y * i.binormal + normalMap.z * i.normal);
	float2 refractionDir = normalMap.xy;
	#if GERSTNER_ENABLED
		if (_RecalculateNormals == 1){
			refractionDir = earlyNormal.xz;
		}
	#endif
	float2 uvOffset = refractionDir * _DistortionStrength;
	#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		uvOffset.x *= 0.5;
		uvOffset.xy *= 0.5;
	#endif
	float proj = i.uvGrab.w + 0.00001;
	float2 screenUV = 0;
	#if DEPTH_EFFECTS_ENABLED
		if (!IsInMirror()){
			screenUV = AlignWithGrabTexel((i.uvGrab.xy + uvOffset) / proj);
			if (GetDepth(i, screenUV) < 0){
				screenUV = AlignWithGrabTexel(i.uvGrab.xy / proj);
			}
		}
		else screenUV = (i.uvGrab.xy + uvOffset) / proj;
	#else
		screenUV = (i.uvGrab.xy + uvOffset) / proj;
	#endif

	float2 baseUV = i.uvGrab.xy/proj;
	float2 mainTexUV = TRANSFORM_TEX(i.uv, _MainTex) + _Time.y * 0.1 * _MainTexScroll;
	mainTexUV += normalMap.xy * _BaseColorDistortionStrength;
	float4 surfaceTint = 0;
	#if TRANSPARENCY_OPAQUE || TRANSPARENCY_PREMUL
		surfaceTint = _NonGrabColor;
		if (!isFrontFace)
			surfaceTint = _NonGrabBackfaceTint;
	#else
		surfaceTint = _Color;
		if (!isFrontFace)
			surfaceTint = _BackfaceTint;
	#endif
	float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
	if (isFrontFace){
		float NdotVert = abs(dot(earlyNormal, viewDir));
		surfaceTint *= lerp(_AngleTint, 1, NdotVert);
	}

	#if VERT_OFFSET_ENABLED
		float3 waveTint = _SubsurfaceTint.rgb * _SubsurfaceBrightness;
		float subsurfaceThreshold = smoothstep(_SubsurfaceThreshold, 1, i.wave.y);
		surfaceTint.rgb = lerp(surfaceTint.rgb, waveTint, subsurfaceThreshold * _SubsurfaceStrength);
	#endif

	#if BASECOLOR_STOCHASTIC_ENABLED
		float4 mainTex = tex2Dstoch(_MainTex, sampler_FlowMap, mainTexUV) * surfaceTint;
	#else
		float4 mainTex = MOCHIE_SAMPLE_TEX2D_SAMPLER(_MainTex, sampler_FlowMap, mainTexUV) * surfaceTint;
	#endif

	#if TRANSPARENCY_GRABPASS
		float4 baseCol = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MWGrab, baseUV);
		float4 col = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MWGrab, screenUV) * mainTex;
		float depth = saturate(1-GetDepth(i, screenUV));
		float rawDepth = saturate(1-GetDepth(i, baseUV));
	#else
		float4 baseCol = mainTex;
		float4 col = mainTex;
		float depth = 1;
		float rawDepth = 1;
	#endif

	#if DEPTH_EFFECTS_ENABLED
		#if CAUSTICS_ENABLED
			if (isFrontFace && !i.isInVRMirror){
				float caustDepth = depth;
				float caustFade = saturate(pow(caustDepth, _CausticsFade));
				if (caustFade > 0){
					float3 wPos = GetWorldSpacePixelPosSP(i.localPos, screenUV);
					float2 depthUV = Rotate3D(wPos, _CausticsRotation).xz;
					float3 caustics = 0;
					#if CAUSTICS_VORONOI
						_CausticsDistortion *= 0.5;
						float3 causticsOffset = UnpackNormal(tex2Dstoch(_CausticsDistortionTex, sampler_FlowMap, (depthUV*_CausticsDistortionScale*0.1)+_Time.y*_CausticsDistortionSpeed*0.05));
						float2 causticsUV = (depthUV + uvOffset + (causticsOffset.xy * _CausticsDistortion)) * _CausticsScale;
						causticsUV *= 7.5;
						_CausticsSpeed *= 3;
						float voronoi0 = Voronoi2D(causticsUV, _Time.y*_CausticsSpeed);
						float voronoi1 = Voronoi2D(causticsUV, (_Time.y*_CausticsSpeed)+_CausticsDisp);
						float voronoi2 = Voronoi2D(causticsUV, (_Time.y*_CausticsSpeed)+_CausticsDisp*2.0);
						caustics = float3(voronoi0, voronoi1, voronoi2);
						caustics = pow(caustics, _CausticsPower*1.5);
						caustics = smootherstep(0, 1, caustics);
					#elif CAUSTICS_TEXTURE
						float3 causticsOffset = UnpackNormal(tex2Dstoch(_CausticsDistortionTex, sampler_FlowMap, (depthUV*_CausticsDistortionScale*0.1)+_Time.y*_CausticsDistortionSpeed*0.05));
						float2 causticsUV = (depthUV + uvOffset + (causticsOffset.xy * _CausticsDistortion)) * _CausticsScale;
						causticsUV *= 0.2;
						_CausticsSpeed *= 0.05;
						_CausticsOpacity *= 10;
						float2 uvTex0 = causticsUV +_Time.y * _CausticsSpeed;
						float2 uvTex1 = causticsUV * -1 + _Time.y * _CausticsSpeed * 0.5;
						float3 tex0 = MOCHIE_SAMPLE_TEX2D_SAMPLER(_CausticsTex, sampler_FlowMap, uvTex0);
						float3 tex1 = MOCHIE_SAMPLE_TEX2D_SAMPLER(_CausticsTex, sampler_FlowMap, uvTex1);
						caustics = min(tex0, tex1);
						caustics = clamp(caustics, 0, 0.1);
					#elif CAUSTICS_FLIPBOOK
						float2 causticsUV = (depthUV + uvOffset) * _CausticsScale;
						caustics = tex2DflipbookSmooth(_CausticsTexArray, sampler_FlowMap, causticsUV * 0.35, _CausticsFlipbookSpeed);
						caustics = smootherstep(0.15, 1, caustics);
					#endif
					// topFade = 1-saturate(pow(caustDepth, _CausticsSurfaceFade) * 2);
					col.rgb += saturate(caustics * _CausticsColor * caustFade * _CausticsOpacity * surfaceTint);
				}
			}
		#endif
		#if DEPTHFOG_ENABLED
			if (isFrontFace && !i.isInVRMirror){
				float fogDepth = depth;
				fogDepth = saturate(pow(fogDepth, _FogPower));
				float fogDepth0 = saturate(pow(fogDepth, _FogPower*0.5));
				_FogTint.rgb *= _FogBrightness;
				_FogTint.rgb *= surfaceTint.rgb;
				col.rgb = lerp(col.rgb, lerp(_FogTint.rgb, col.rgb, fogDepth), _FogTint.a);
				col.rgb = lerp(col.rgb, lerp(_FogTint.rgb*0.5, col.rgb, fogDepth0), _FogTint.a);

				// float fogDepth0 = saturate(pow(depth, _FogPower));
				// float fogDepth1 = saturate(pow(depth, _FogPower2));
				// _FogTint.rgb *= _FogBrightness;
				// _FogTint2.rgb *= _FogBrightness2;
				// _FogTint.rgb *= surfaceTint.rgb;
				// _FogTint2.rgb *= surfaceTint.rgb;
				// col.rgb = lerp(col.rgb, lerp(_FogTint.rgb, col.rgb, fogDepth0), _FogTint.a);
				// col.rgb = lerp(col.rgb, lerp(_FogTint2.rgb, col.rgb, fogDepth1), _FogTint2.a);
			}
		#endif
	#endif


	#if FOAM_ENABLED
		float2 uvFoamOffset = normalMap.xy * _FoamDistortionStrength * 0.1;
		#if FLOW_ENABLED
			uvF0.xy += uvFoamOffset;
			uvF1.xy += uvFoamOffset;
			#if FOAM_STOCHASTIC_ENABLED
				float4 foamTex0 = tex2Dstoch(_FoamTex, sampler_FlowMap, uvF0.xy) * uvF0.z;
				float4 foamTex1 = tex2Dstoch(_FoamTex, sampler_FlowMap, uvF1.xy) * uvF1.z;
			#else
				float4 foamTex0 = MOCHIE_SAMPLE_TEX2D_SAMPLER(_FoamTex, sampler_FlowMap, uvF0.xy) * uvF0.z;
				float4 foamTex1 = MOCHIE_SAMPLE_TEX2D_SAMPLER(_FoamTex, sampler_FlowMap, uvF1.xy) * uvF1.z;
			#endif
			float4 foamTex = (foamTex0 + foamTex1) * _FoamColor;
		#else
			uvFoam += uvFoamOffset;
			#if FOAM_STOCHASTIC_ENABLED
				float4 foamTex = tex2Dstoch(_FoamTex, sampler_FlowMap, uvFoam) * _FoamColor;
			#else
				float4 foamTex = MOCHIE_SAMPLE_TEX2D_SAMPLER(_FoamTex, sampler_FlowMap, uvFoam) * _FoamColor;
			#endif
		#endif

		float2 foamNoiseUV = ScaleUV(i.uv.xy, _FoamNoiseTexScale, _FoamNoiseTexScroll);
		#if FOAM_STOCHASTIC_ENABLED
			float3 foamNoiseSample = tex2Dstoch(_FoamNoiseTex, sampler_FlowMap, foamNoiseUV);
		#else
			float3 foamNoiseSample = MOCHIE_SAMPLE_TEX2D_SAMPLER(_FoamNoiseTex, sampler_FlowMap, foamNoiseUV);
		#endif
		float foamNoise = Average(foamNoiseSample);
		float foamTexNoise = lerp(1, foamNoise, _FoamNoiseTexStrength);
		float foamCrestNoise = lerp(1, foamNoise, _FoamNoiseTexCrestStrength);
		float foam = 0;
		float foamDepth = 1;
		#if DEPTH_EFFECTS_ENABLED
			if (!i.isInVRMirror){
				foamDepth = saturate(pow(rawDepth,_FoamPower));
				#if EDGEFADE_ENABLED
					foamDepth = 1-saturate(Remap(1-foamDepth, 0, 1, -_EdgeFadeOffset, 1));
				#endif
				foam = saturate(foamTex.a * foamDepth * _FoamEdgeStrength * foamTexNoise * Average(foamTex.rgb));
				col.rgb = lerp(col.rgb, foamTex.rgb, foam);
			}
		#endif
		float crestFoam = 0;
		#if VERT_OFFSET_ENABLED
			float crestThreshold = smoothstep(_FoamCrestThreshold, 1, i.wave.y);
			crestFoam = saturate(foamTex.a * _FoamCrestStrength * crestThreshold * foamCrestNoise * 10);
			col.rgb = lerp(col.rgb, foamTex.rgb, crestFoam);
			crestFoam *= 1.5; // Increase roughness strength of crest foam
		#endif
		#if FOAM_NORMALS_ENABLED
			float foamNormalStr = lerp(0,_FoamNormalStrength,foam+crestFoam);
			#if FLOW_ENABLED
				float3 foamNormalTex0 = tex2Dnormal(_FoamTex, sampler_FlowMap, uvF0.xy, 0.15, foamNormalStr) * uvF0.z;
				float3 foamNormalTex1 = tex2Dnormal(_FoamTex, sampler_FlowMap, uvF1.xy, 0.15, foamNormalStr) * uvF1.z;
				float3 foamNormal = (foamNormalTex0 + foamNormalTex1);
			#else
				float3 foamNormal = tex2Dnormal(_FoamTex, sampler_FlowMap, uvFoam, 0.15,foamNormalStr);
			#endif
			normalMap = BlendNormals(foamNormal, normalMap);
		#endif
	#endif

	#if DETAIL_BASECOLOR_ENABLED
		col = lerp(col, detailBC, detailBC.a);
	#endif

	float3 normalDir = i.normal;

	#if PBR_ENABLED
		normalDir = normalize(normalMap.x * i.tangent + normalMap.y * i.binormal + normalMap.z * i.normal);
		if (!isFrontFace)
			normalDir = -normalDir;
		float NdotV = abs(dot(normalDir, viewDir));
		float2 roughnessMapUV = detailUV;
		if (_DetailTextureMode != 1)
			roughnessMapUV = TRANSFORM_TEX(i.uv, _RoughnessMap);
		float roughnessMap = MOCHIE_SAMPLE_TEX2D_SAMPLER(_RoughnessMap, sampler_FlowMap, roughnessMapUV);
		#if DETAIL_BASECOLOR_ENABLED
			if (_DetailTextureMode == 1)
				roughnessMap *= detailBC.a;
		#endif
		float rough = roughnessMap * _Roughness;
		#if FOAM_ENABLED
			float foamLerp = (foam + crestFoam);
			rough = lerp(rough, _FoamRoughness, foamLerp*2);
		#endif
		float roughSq = rough * rough;
		float roughBRDF = max(roughSq, 0.003);
		
		float2 metallicMapUV = detailUV;
		if (_DetailTextureMode != 1)
			metallicMapUV = TRANSFORM_TEX(i.uv, _MetallicMap);
		float metallicMap = MOCHIE_SAMPLE_TEX2D_SAMPLER(_MetallicMap, sampler_FlowMap, metallicMapUV);
		#if DETAIL_BASECOLOR_ENABLED
			if (_DetailTextureMode == 1)
				metallicMap *= detailBC.a;
		#endif
		float metallic = metallicMap * _Metallic;

		float omr = unity_ColorSpaceDielectricSpec.a - metallic * unity_ColorSpaceDielectricSpec.a;
		float3 specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, 1, metallic);

		#if BASE_PASS
			float3 lightDir = _Specular == 1 ? UnityWorldSpaceLightDir(i.worldPos) : _LightDir;
			lightDir = normalize(lightDir);
		#else
			float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
		#endif
		float3 halfVector = Unity_SafeNormalize(lightDir + viewDir);
		float NdotL = saturate(dot(normalDir, lightDir));
		// atten *= NdotL;

		float3 specCol = 0;
		float3 reflCol = 0;
		#if SPECULAR_ENABLED
			if (isFrontFace){
				float roughInterp = smoothstep(0.001, 0.003, roughSq);
				float NdotH = saturate(dot(normalDir, halfVector));
				float LdotH = saturate(dot(lightDir, halfVector));
				atten *= NdotL;
				float3 fresnelTerm = FresnelTerm(specularTint, LdotH);
				float specularTerm = SpecularTerm(NdotL, NdotV, NdotH, roughBRDF);
				float3 specLightCol = _Specular == 1 ? _LightColor0 : 1;
				specCol = specLightCol * fresnelTerm * specularTerm;
				specCol = lerp(smootherstep(0, 0.9, specCol), specCol, roughInterp) * _SpecStrength * _SpecTint;
				#if defined(UNITY_PASS_FORWARDBASE)
					specCol *= _ShadowStrength > 0 ? atten : 1;
				#else
					specCol *= atten;
				#endif
			}
		#endif

		#if REFLECTIONS_ENABLED
			if ((_BackfaceReflections == 0 && isFrontFace) || (_BackfaceReflections == 1)){			
				float3 reflDir = reflect(-viewDir, normalDir);
				float surfaceReduction = 1.0 / (roughBRDF*roughBRDF + 1.0);
				float grazingTerm = saturate((1-rough) + (1-omr));
				float fresnel = FresnelLerp(specularTint, grazingTerm, NdotV);
				#if defined(_REFLECTIONS_MANUAL_ON)
					reflCol = GetManualReflections(reflDir, rough);
				#elif defined(_REFLECTIONS_MIRROR_ON)
					reflCol = GetMirrorReflections(i.reflUV, normalDir, rough);
				#else
					reflCol = GetWorldReflections(reflDir, i.worldPos, rough);
				#endif
				reflCol *= _ReflStrength;
				#if DEPTH_EFFECTS_ENABLED
					#if SSR_ENABLED
						half4 ssrCol = GetSSR(i.worldPos, viewDir, reflDir, normalDir, 1-rough, col.rgb, _Metallic, screenUV, i.uvGrab);
						ssrCol.rgb *= _SSRStrength;
						#if FOAM_ENABLED
							foamLerp = 1-foamLerp;
							foamLerp = smoothstep(0.7, 1, foamLerp);
							ssrCol.a *= foamLerp;
						#endif
						if (_EdgeFadeSSR == 0)
							ssrCol.a = ssrCol.a > 0 ? 1 : 0;
						reflCol = lerp(reflCol, ssrCol.rgb, ssrCol.a * saturate(_SSRStrength));
						#if SPECULAR_ENABLED
							specCol *= 1-ssrCol.a;
						#endif
					#endif
				#endif
				reflCol = reflCol * fresnel * surfaceReduction * _ReflTint;
			}
		#endif
		col.rgb += specCol;
		col.rgb += reflCol;

		#if AREALIT_ENABLED && (REFLECTIONS_ENABLED)
			AreaLightFragInput ai;
			ai.pos = i.worldPos;
			ai.normal = normalDir;
			ai.view = -viewDir;
			ai.roughness = roughBRDF * _AreaLitRoughnessMult;
			ai.occlusion = 1; // float4(occlusion, 1);
			ai.screenPos = i.pos.xy;
			half4 diffTerm, specTerm;
			if (_AreaLitStrength > 0){
				ShadeAreaLights(ai, diffTerm, specTerm, true, !IsSpecularOff(), IsStereo());
			}
			else {
				diffTerm = 0;
				specTerm = 0;
			}
			float3 areaLitColor = col.rgb * diffTerm + specularTint * specTerm;
			col.rgb += areaLitColor * _AreaLitStrength * MOCHIE_SAMPLE_TEX2D_SAMPLER(_AreaLitMask, sampler_FlowMap, TRANSFORM_TEX(i.uv, _AreaLitMask)).r;
		#endif
	#endif
	
	#if DEPTH_EFFECTS_ENABLED
		#if EDGEFADE_ENABLED
			float edgeFadeDepth = 1;
			if (!i.isInVRMirror){
				edgeFadeDepth = rawDepth;
				edgeFadeDepth = 1-saturate(pow(edgeFadeDepth, _EdgeFadePower));
				edgeFadeDepth = saturate(Remap(edgeFadeDepth, 0, 1, -_EdgeFadeOffset, 1));
			}
		#endif
	#endif

	#if !(TRANSPARENCY_OPAQUE)
		float2 opacityUV = ScaleOffsetScrollUV(i.uv, _OpacityMask_ST.xy, _OpacityMask_ST.zw, _OpacityMaskScroll);
		_Opacity *= MOCHIE_SAMPLE_TEX2D_SAMPLER(_OpacityMask, sampler_FlowMap, opacityUV);
	#endif

	#if defined(UNITY_PASS_FORWARDADD)
		#if DEPTH_EFFECTS_ENABLED
			#if EDGEFADE_ENABLED
				col = lerp(0, col, edgeFadeDepth);
			#endif
		#endif
		col.rgb *= _LightColor0 * atten;
		col = lerp(0, col, _Opacity);
	#else
		#if DEPTH_EFFECTS_ENABLED
			#if EDGEFADE_ENABLED
				col = lerp(baseCol, col, edgeFadeDepth);
			#endif
		#endif
		col = lerp(baseCol, col, _Opacity);
	#endif

	#if TRANSPARENCY_OPAQUE
		col.rgb *= lerp(1, atten, _ShadowStrength);
	#endif

	#if EMISSION_ENABLED
		float2 emissUVOffset = normalMap.xy * _EmissionDistortionStrength;
		uvE0.xy += emissUVOffset;
		uvE1.xy += emissUVOffset;
		emissionUV += emissUVOffset;
		#if EMISS_STOCHASTIC_ENABLED
			#if FLOW_ENABLED
				float3 emissCol0 = tex2Dstoch(_EmissionMap, sampler_FlowMap, uvE0.xy) * uvE0.z;
				float3 emissCol1 = tex2Dstoch(_EmissionMap, sampler_FlowMap, uvE1.xy) * uvE1.z;
				float3 emissCol = emissCol0 + emissCol1;
			#else
				float3 emissCol = tex2Dstoch(_EmissionMap, sampler_FlowMap, emissionUV);
			#endif
		#else
			#if FLOW_ENABLED
				float3 emissCol0 = MOCHIE_SAMPLE_TEX2D_SAMPLER(_EmissionMap, sampler_FlowMap, uvE0.xy) * uvE0.z;
				float3 emissCol1 = MOCHIE_SAMPLE_TEX2D_SAMPLER(_EmissionMap, sampler_FlowMap, uvE1.xy) * uvE1.z;
				float3 emissCol = emissCol0 + emissCol1;
			#else	
				float3 emissCol = MOCHIE_SAMPLE_TEX2D_SAMPLER(_EmissionMap, sampler_FlowMap, emissionUV);
			#endif
		#endif
		emissCol *= _EmissionColor;
		#if AUDIOLINK_ENABLED
			audioLinkData al = (audioLinkData)0;
			InitializeAudioLink(al);
			float audiolink = GetAudioLinkBand(al, _AudioLinkBand);
			emissCol *= audiolink;
		#endif
		col.rgb += (emissCol * _EmissionColor);
	#endif

	ApplyIndirectLighting(i.lightmapUV, i.normal, normalDir, col.rgb);

	if (_VisualizeFlowmap)
		col = flowMap;

	flowMap = lerp(0, flowMap, _ZeroProp);

	#if TRANSPARENCY_OPAQUE
		col.a = 1;
	#endif
	
	UNITY_APPLY_FOG(i.fogCoord, col);
	
	return col + flowMap;
	// return float4(i.wave.yyy, 1);
	// return float4(subsurfaceThreshold.xxx, 1);
}

#include "WaterTess.cginc"

#endif // WATER_FRAG_INCLUDED