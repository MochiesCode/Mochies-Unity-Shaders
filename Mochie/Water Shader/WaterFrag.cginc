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

	#if TRANSPARENCY_OPAQUE
		_Opacity = 1;
	#endif

	#if GERSTNER_ENABLED
		if (_RecalculateNormals == 1){
			_NormalStr0 = -_NormalStr0;
			_NormalStr1 = -_NormalStr1;
			_DetailNormalStrength = -_DetailNormalStrength;
			_FoamNormalStrength = -_FoamNormalStrength;
		}
	#endif

	float4 detailBC = 0;
	float2 detailUV = i.uv;
	#if DETAIL_BASECOLOR_ENABLED
		detailUV = TRANSFORM_TEX(i.uv, _DetailBaseColor) + (_Time.y * _DetailScroll);
	#elif DETAIL_NORMAL_ENABLED
		detailUV = TRANSFORM_TEX(i.uv, _DetailNormal) + (_Time.y * _DetailScroll);
	#endif
	
	#if DETAIL_BASECOLOR_ENABLED
		detailBC = tex2D(_DetailBaseColor, detailUV) * _DetailBaseColorTint;
	#endif

	// CalculateTangentViewDir(i);

	float3 normalMap;
	float3 detailNormal;
	float2 uvNormal0 = ScaleUV(i.uv, _NormalMapScale0, _NormalMapScroll0);
	float2 uvNormal1 = ScaleUV(i.uv, _NormalMapScale1, _NormalMapScroll1);
	float2 baseUV0 = Rotate2D(uvNormal0, _Rotation0);

	// Literally the stupidest thing I've ever had to do (part 2)
	float dummy = bla[unity_StereoEyeIndex];
    if (_ScreenParams.x == 0)
        baseUV0.x += dummy;

	float2 baseUV1 = Rotate2D(uvNormal1, _Rotation1);
	float3 uv00 = float3(baseUV0, 1);
	float3 uv10 = float3(baseUV1, 1);
	#if FOAM_ENABLED
		float2 uvFoam = ScaleUV(i.uv, _FoamTexScale, _FoamTexScroll * 0.1);
		// ParallaxOffset(i, uvFoam, _FoamOffset, isFrontFace);
		float3 uvF0 = float3(uvFoam, 1);
		float3 uvF1 = uvF0;
	#endif

	#if FLOW_ENABLED
		float2 uvFlow = ScaleUV(i.uvFlow, _FlowMapScale, 0);
		float4 flowMap = tex2D(_FlowMap, uvFlow);
		float blendNoise = flowMap.a;
		if (_BlendNoiseSource == 1){
			float2 uvBlend = ScaleUV(i.uv, _BlendNoiseScale, 0);
			blendNoise = tex2D(_BlendNoise, uvBlend);
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
	#endif

	#if NORMALMAP1_ENABLED
		#if STOCHASTIC0_ENABLED
			#define tex2D tex2Dstoch
			_NormalStr0 *= 1.5;
		#endif
		// ParallaxOffset(i, uv00.xy, _NormalMapOffset0, isFrontFace);
		float3 normalMap0 = UnpackScaleNormal(tex2D(_NormalMap0, uv00.xy), _NormalStr0) * uv00.z;
		#if FLOW_ENABLED
			// ParallaxOffset(i, uv01.xy, _NormalMapOffset0, isFrontFace);
			float3 normalMap1 = UnpackScaleNormal(tex2D(_NormalMap0, uv01.xy), _NormalStr0) * uv01.z;
		#endif
		#undef tex2D

		#if STOCHASTIC1_ENABLED
			#define tex2D tex2Dstoch
			_NormalStr1 *= 1.5;
		#endif
		// ParallaxOffset(i, uv10.xy, _NormalMapOffset1, isFrontFace);
		float3 detailNormal0 = UnpackScaleNormal(tex2D(_NormalMap1, uv10.xy), _NormalStr1) * uv10.z;
		#if FLOW_ENABLED
			// ParallaxOffset(i, uv11.xy, _NormalMapOffset1, isFrontFace);
			float3 detailNormal1 = UnpackScaleNormal(tex2D(_NormalMap1, uv11.xy), _NormalStr1) * uv11.z;
			normalMap0 = normalize(normalMap0 + normalMap1);
			detailNormal0 = normalize(detailNormal0 + detailNormal1);
		#endif
		#undef tex2D
		normalMap = BlendNormals(normalMap0, detailNormal0);
	#else
		#if STOCHASTIC0_ENABLED
			#define tex2D tex2Dstoch
			_NormalStr0 *= 1.5;
		#endif
		// ParallaxOffset(i, uv00.xy, _NormalMapOffset0, isFrontFace);
		float3 normalMap0 = UnpackScaleNormal(tex2D(_NormalMap0, uv00.xy), _NormalStr0) * uv00.z;
		#if FLOW_ENABLED
			// ParallaxOffset(i, uv01.xy, _NormalMapOffset0, isFrontFace);
			float3 normalMap1 = UnpackScaleNormal(tex2D(_NormalMap0, uv01.xy), _NormalStr0) * uv01.z;
			normalMap = normalize(normalMap0 + normalMap1);
		#else
			normalMap = normalize(normalMap0);
		#endif
		#undef tex2D
	#endif

	#if RAIN_ENABLED
		float3 rainNormal = GetRipplesNormal(i.uv, _RippleScale, _RippleStr, _RippleSpeed);
		normalMap = BlendNormals(normalMap, rainNormal);
	#endif

	#if DETAIL_NORMAL_ENABLED
		float3 detNorm = UnpackScaleNormal(tex2D(_DetailNormal, detailUV), _DetailNormalStrength);
		normalMap = lerp(normalMap, detNorm, detailBC.a);
	#endif

	float2 refractionDir = normalMap.xy;
	#if GERSTNER_ENABLED
		if (_RecalculateNormals == 1){
			i.normal = normalize(dot(i.normal, i.normal) >= 1.01 ? i.cNormal : i.normal);
			refractionDir = normalize(normalMap.x * i.tangent + normalMap.y * i.binormal + normalMap.z * i.normal).xz;
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
	// ParallaxOffset(i, mainTexUV, _BaseColorOffset, isFrontFace);
	float4 surfaceTint = _Color;
	if (!isFrontFace)
		surfaceTint = _BackfaceTint;

	#if BASECOLOR_STOCHASTIC_ENABLED
		float4 mainTex = tex2Dstoch(_MainTex, mainTexUV) * surfaceTint;
	#else
		float4 mainTex = tex2D(_MainTex, mainTexUV) * surfaceTint;
	#endif

	#if TRANSPARENCY_GRABPASS
		float4 baseCol = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MWGrab, baseUV);
		float4 col = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MWGrab, screenUV) * mainTex;
		float depth = saturate(1-GetDepth(i, screenUV));
	#else
		float4 baseCol = mainTex;
		float4 col = mainTex;
		float depth = 1;
	#endif

	#if DEPTH_EFFECTS_ENABLED
		if (isFrontFace && !i.isInVRMirror){
			#if CAUSTICS_ENABLED
				float caustDepth = depth;
				float caustFade = saturate(pow(caustDepth, _CausticsFade));
				if (caustFade > 0){
					float3 wPos = GetWorldSpacePixelPosSP(i.localPos, screenUV);
					float2 depthUV = Rotate3D(wPos, _CausticsRotation).xz;
					float3 causticsOffset = UnpackNormal(tex2D(_NormalMap0, (depthUV*_CausticsDistortionScale*0.1)+_Time.y*_CausticsDistortionSpeed*0.05));
					float2 causticsUV = (depthUV + uvOffset + (causticsOffset.xy * _CausticsDistortion)) * _CausticsScale;
					float voronoi0 = Voronoi2D(causticsUV, _Time.y*_CausticsSpeed);
					float voronoi1 = Voronoi2D(causticsUV, (_Time.y*_CausticsSpeed)+_CausticsDisp);
					float voronoi2 = Voronoi2D(causticsUV, (_Time.y*_CausticsSpeed)+_CausticsDisp*2.0);
					// float3 tex0 = tex2D(_CausticsTex, causticsUV + _Time.y*0.05);
					// float3 tex1 = tex2D(_CausticsTex, causticsUV - _Time.y*0.025);
					// float3 voronoi = min(tex0, tex1);
					// float3 voronoi = min(voronoi2, min(voronoi1, voronoi0));
					float3 voronoi = float3(voronoi0, voronoi1, voronoi2);
					voronoi = pow(voronoi, _CausticsPower);
					// topFade = 1-saturate(pow(caustDepth, _CausticsSurfaceFade) * 2);
					float3 caustics = smootherstep(0, 1, voronoi) * _CausticsOpacity * caustFade * _CausticsColor;
					col.rgb += caustics;
				}
			#endif

			#if DEPTHFOG_ENABLED
				float fogDepth = depth;
				fogDepth = saturate(pow(fogDepth, _FogPower));
				float fogDepth0 = saturate(pow(fogDepth, _FogPower/2));
				col.rgb = lerp(col.rgb, lerp(_FogTint.rgb, col.rgb, fogDepth), _FogTint.a);
				col.rgb = lerp(col.rgb, lerp(_FogTint.rgb*0.5, col.rgb, fogDepth0), _FogTint.a);
			#endif
		}
	#endif


	#if FOAM_ENABLED
		float2 uvFoamOffset = normalMap.xy * _FoamDistortionStrength * 0.1;
		#if FOAM_STOCHASTIC_ENABLED
			#define tex2D tex2Dstoch
		#endif
		#if FLOW_ENABLED
			uvF0.xy += uvFoamOffset;
			uvF1.xy += uvFoamOffset;
			float4 foamTex0 = tex2D(_FoamTex, uvF0.xy) * uvF0.z;
			float4 foamTex1 = tex2D(_FoamTex, uvF1.xy) * uvF1.z;
			float4 foamTex = (foamTex0 + foamTex1) * _FoamColor;
		#else
			uvFoam += uvFoamOffset;
			float4 foamTex = tex2D(_FoamTex, uvFoam) * _FoamColor;
		#endif

		float2 foamNoiseUV = ScaleUV(i.uv.xy, _FoamNoiseTexScale, _FoamNoiseTexScroll);
		float foamNoise = Average(tex2D(_FoamNoiseTex, foamNoiseUV).rgb);
		float foamTexNoise = lerp(1, foamNoise, _FoamNoiseTexStrength);
		float foamCrestNoise = lerp(1, foamNoise, _FoamNoiseTexCrestStrength);
		#undef tex2D
		float foam = 0;
		#if DEPTH_EFFECTS_ENABLED
			if (!i.isInVRMirror){
				float foamDepth = saturate(pow(depth,_FoamPower));
				foam = saturate(foamTex.a * foamDepth * _FoamOpacity * foamTexNoise * Average(foamTex.rgb));
				col.rgb = lerp(col.rgb, foamTex.rgb, foam);
			}
		#endif
		float crestThreshold = smoothstep(_FoamCrestThreshold, 1, i.wave.y);
		float crestFoam = saturate(foamTex.a * _FoamOpacity * _FoamCrestStrength * crestThreshold * foamCrestNoise * 10);
		col.rgb = lerp(col.rgb, foamTex.rgb, crestFoam);
		crestFoam *= 1.5; // Increase roughness strength of crest foam
		#if FOAM_NORMALS_ENABLED
			float foamNormalStr = lerp(0,_FoamNormalStrength,foam+crestFoam);
			#if FLOW_ENABLED
				float3 foamNormalTex0 = tex2Dnormal(_FoamTex, uvF0.xy, 0.15, foamNormalStr) * uvF0.z;
				float3 foamNormalTex1 = tex2Dnormal(_FoamTex, uvF1.xy, 0.15, foamNormalStr) * uvF1.z;
				float3 foamNormal = (foamNormalTex0 + foamNormalTex1);
			#else
				float3 foamNormal = tex2Dnormal(_FoamTex, uvFoam, 0.15,foamNormalStr);
			#endif
			// normalMap = lerp(normalMap, foamNormal, saturate((foam+crestFoam)*10)); 
			normalMap = BlendNormals(foamNormal, normalMap);
		#endif
	#endif

	#if DETAIL_BASECOLOR_ENABLED
		col = lerp(col, detailBC, detailBC.a);
	#endif

	#if PBR_ENABLED
		float3 normalDir = normalize(normalMap.x * i.tangent + normalMap.y * i.binormal + normalMap.z * i.normal);
		if (!isFrontFace)
			normalDir = -normalDir;
		float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
		float NdotV = abs(dot(normalDir, viewDir));
		if (!isFrontFace)
			col *= lerp(_AngleTint, 1, NdotV);

		float2 roughnessMapUV = detailUV;
		if (_DetailTextureMode != 1)
			roughnessMapUV = TRANSFORM_TEX(i.uv, _RoughnessMap);
		float roughnessMap = tex2D(_RoughnessMap, roughnessMapUV);
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
		float metallicMap = tex2D(_MetallicMap, metallicMapUV);
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
				#if REFLECTIONS_MANUAL_ENABLED
					reflCol = GetManualReflections(reflDir, rough);
				#else
					reflCol = GetWorldReflections(reflDir, i.worldPos, rough);
				#endif
				#if DEPTH_EFFECTS_ENABLED
					#if SSR_ENABLED
						half4 ssrCol = GetSSR(i.worldPos, viewDir, reflDir, normalDir, 1-rough, col.rgb, _Metallic, screenUV, i.uvGrab);
						ssrCol.rgb *= _SSRStrength * lerp(10, 7, linearstep(0,1,_Metallic));
						#if FOAM_ENABLED
							foamLerp = 1-foamLerp;
							foamLerp = smoothstep(0.7, 1, foamLerp);
							ssrCol.a *= foamLerp;
						#endif
						reflCol = lerp(reflCol, ssrCol.rgb, ssrCol.a);
						#if SPECULAR_ENABLED
							specCol *= (1-smoothstep(0, 0.1, ssrCol.a));
						#endif
					#endif
				#endif
				reflCol = reflCol * fresnel * surfaceReduction * _ReflStrength * _ReflTint;
			}
		#endif
		col.rgb += specCol;
		col.rgb += reflCol;

		#if AREALIT_ENABLED && (REFLECTIONS_ENABLED || REFLECTIONS_MANUAL_ENABLED) 
			AreaLightFragInput ai;
			ai.pos = i.worldPos;
			ai.normal = normalDir;
			ai.view = -viewDir;
			ai.roughness = roughBRDF * _AreaLitRoughnessMult;
			ai.occlusion = 1; // float4(occlusion, 1);
			ai.screenPos = i.pos.xy;
			half4 diffTerm, specTerm;
			ShadeAreaLights(ai, diffTerm, specTerm, true, !IsSpecularOff(), IsStereo());

			float3 areaLitColor = col.rgb * diffTerm + specularTint * specTerm;
			col.rgb += areaLitColor * _AreaLitStrength * tex2D(_AreaLitMask, TRANSFORM_TEX(i.uv, _AreaLitMask)).r;
		#endif
	#endif
	
	#if DEPTH_EFFECTS_ENABLED
		#if EDGEFADE_ENABLED
			float edgeFadeDepth = 1;
			if (!i.isInVRMirror){
				edgeFadeDepth = saturate(1-GetDepth(i, baseUV));
				edgeFadeDepth = (1-saturate(pow(edgeFadeDepth, _EdgeFadePower)));
				edgeFadeDepth = saturate(Remap(edgeFadeDepth, 0, 1, -_EdgeFadeOffset, 1));
			}
		#endif
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
		float2 emissUV = TRANSFORM_TEX(i.uv, _EmissionMap) + (_Time.y * _EmissionMapScroll);
		#if EMISS_STOCHASTIC_ENABLED
			float3 emissCol = tex2Dstoch(_EmissionMap, emissUV);
		#else
			float3 emissCol = tex2D(_EmissionMap, emissUV);
		#endif
		col.rgb += (emissCol * _EmissionColor);
	#endif
	UNITY_APPLY_FOG(i.fogCoord, col);
	
	return col;
}

#include "WaterTess.cginc"

#endif // WATER_FRAG_INCLUDED