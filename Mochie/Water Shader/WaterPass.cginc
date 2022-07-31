#ifndef WATER_PASS_INCLUDED
#define WATER_PASS_INCLUDED

v2f vert (appdata v) {
	v2f o = (v2f)0;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	#if VERTEX_OFFSET_ENABLED
		float2 noiseUV = ScaleUV(v.uv, _NoiseTexScale, _NoiseTexScroll*10);
		float noiseWaveTex = tex2Dlod(_NoiseTex, float4(noiseUV,0,lerp(0,8,_NoiseTexBlur)));
		float noiseWave = Remap(noiseWaveTex, 0, 1, _VertRemapMin, _VertRemapMax);
		float offsetWave = noiseWave * _WaveHeight;
		o.wave = _Offset * offsetWave;
		v.vertex.xyz += o.wave;
		o.wave.y = (o.wave.y + 1) * 0.5;
	#elif GERSTNER_ENABLED
		float turb = 0;
		float3 wave0 = 0;
		float3 wave1 = 0;
		float3 wave2 = 0;
		if (_Turbulence > 0){
			_Turbulence *= _WaveStrengthGlobal;
			_TurbulenceSpeed *= _WaveSpeedGlobal;
			_TurbulenceScale *= _WaveScaleGlobal;
			turb = Perlin3D(float3(v.uv.xy*_TurbulenceScale, _Time.y*_TurbulenceSpeed))+1;
			turb *= _Turbulence*0.1;
		}
		if (_WaveStrength0 > 0){
			_WaveStrength0 *= _WaveStrengthGlobal;
			_WaveSpeed0 *= _WaveSpeedGlobal;
			_WaveScale0 *= _WaveScaleGlobal;
			float4 waveProperties0 = float4(0,1, _WaveStrength0 + turb, _WaveScale0);
			wave0 = GerstnerWave(waveProperties0, v.vertex.xyz, _WaveSpeed0, _WaveDirection0);
		}
		if (_WaveStrength1 > 0){
			_WaveStrength1 *= _WaveStrengthGlobal;
			_WaveSpeed1 *= _WaveSpeedGlobal;
			_WaveScale1 *= _WaveScaleGlobal;
			float4 waveProperties1 = float4(0,1, _WaveStrength1 + turb, _WaveScale1);
			wave1 = GerstnerWave(waveProperties1, v.vertex.xyz, _WaveSpeed1, _WaveDirection1);
		}
		if (_WaveStrength2 > 0){
			_WaveStrength2 *= _WaveStrengthGlobal;
			_WaveSpeed2 *= _WaveSpeedGlobal;
			_WaveScale2 *= _WaveScaleGlobal;
			float4 waveProperties2 = float4(0,1, _WaveStrength2 + turb, _WaveScale2);
			wave2 = GerstnerWave(waveProperties2, v.vertex.xyz, _WaveSpeed2, _WaveDirection2);
		}
		o.wave = wave0 + wave1 + wave2;
		v.vertex.xyz += o.wave;
		o.wave.y = (o.wave.y + 1) * 0.5;
	#endif

	o.pos = UnityObjectToClipPos(v.vertex);
	o.normal = UnityObjectToWorldNormal(v.normal);
	o.cNormal = UnityObjectToWorldNormal(v.normal);
	o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0)).xyz);
	o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	o.uvGrab = ComputeGrabScreenPos(o.pos);
	o.localPos = v.vertex;
	o.uv = v.uv;

	o.isInVRMirror = false;
	#if UNITY_SINGLE_PASS_STEREO
		if (IsInMirror())
			o.isInVRMirror = true;
	#endif
	v.tangent.xyz = normalize(v.tangent.xyz);
	v.normal = normalize(v.normal);
	float3x3 objectToTangent = float3x3(v.tangent.xyz, (cross(v.normal, v.tangent.xyz) * v.tangent.w), v.normal);
	o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));

	UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

float4 frag(v2f i, bool isFrontFace: SV_IsFrontFace) : SV_Target {

	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	#if defined(UNITY_PASS_FORWARDADD)
		UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
		atten = FadeShadows(i.worldPos, atten);
	#endif

	CalculateTangentViewDir(i);

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
		ParallaxOffset(i, uvFoam, _FoamOffset);
		float3 uvF0 = float3(uvFoam, 1);
		float3 uvF1 = uvF0;
	#endif

	#if FLOW_ENABLED
		float2 uvFlow = ScaleUV(i.uv, _FlowMapScale, 0);
		float4 flowMap = tex2D(_FlowMap, uvFlow);
		float2 flow = (flowMap.rg * 2 - 1) * _FlowStrength * 0.1;
		float time = _Time.y * _FlowSpeed + flowMap.a;
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
		ParallaxOffset(i, uv00.xy, _NormalMapOffset0);
		float3 normalMap0 = UnpackScaleNormal(tex2D(_NormalMap0, uv00.xy), _NormalStr0) * uv00.z;
		#if FLOW_ENABLED
			ParallaxOffset(i, uv01.xy, _NormalMapOffset0);
			float3 normalMap1 = UnpackScaleNormal(tex2D(_NormalMap0, uv01.xy), _NormalStr0) * uv01.z;
		#endif
		#undef tex2D

		#if STOCHASTIC1_ENABLED
			#define tex2D tex2Dstoch
			_NormalStr1 *= 1.5;
		#endif
		ParallaxOffset(i, uv10.xy, _NormalMapOffset1);
		float3 detailNormal0 = UnpackScaleNormal(tex2D(_NormalMap1, uv10.xy), _NormalStr1) * uv10.z;
		#if FLOW_ENABLED
			ParallaxOffset(i, uv11.xy, _NormalMapOffset1);
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
		ParallaxOffset(i, uv00.xy, _NormalMapOffset0);
		float3 normalMap0 = UnpackScaleNormal(tex2D(_NormalMap0, uv00.xy), _NormalStr0) * uv00.z;
		#if FLOW_ENABLED
			ParallaxOffset(i, uv01.xy, _NormalMapOffset0);
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

	float2 uvOffset = normalMap.xy * _DistortionStrength;
	#if UNITY_SINGLE_PASS_STEREO
		uvOffset.x *= 0.5;
		uvOffset.xy *= 0.5;
	#endif
	float proj = i.uvGrab.w + 0.00001;
	float2 screenUV = 0;
	if (!IsInMirror()){
		screenUV = AlignWithGrabTexel((i.uvGrab.xy + uvOffset) / proj);
		if (GetDepth(i, screenUV) < 0){
			screenUV = AlignWithGrabTexel(i.uvGrab.xy / proj);
		}
	}
	else screenUV = (i.uvGrab.xy + uvOffset) / proj;

	float2 baseUV = i.uvGrab.xy/proj;
	float2 mainTexUV = TRANSFORM_TEX(i.uv, _MainTex) + _Time.y * 0.1 * _MainTexScroll;
	mainTexUV += normalMap.xy * _BaseColorDistortionStrength;
	ParallaxOffset(i, mainTexUV, _BaseColorOffset);
	#if BASECOLOR_STOCHASTIC_ENABLED
		float4 mainTex = tex2Dstoch(_MainTex, mainTexUV) * _Color;
	#else
		float4 mainTex = tex2D(_MainTex, mainTexUV) * _Color;
	#endif
	float4 baseCol = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MWGrab, baseUV);
	float4 col = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MWGrab, screenUV) * mainTex;
	float depth = saturate(1-GetDepth(i, screenUV));
	
	if (isFrontFace && !i.isInVRMirror){
		#if CAUSTICS_ENABLED
			float caustDepth = depth;
			float caustFade = saturate(pow(caustDepth, _CausticsFade));
			if (caustFade > 0){
				float3 wPos = GetWorldSpacePixelPosSP(i.localPos, screenUV);
				float2 depthUV = Rotate3D(wPos, _CausticsRotation).xz;
				float3 causticsOffset = UnpackNormal(tex2D(_NormalMap1, (depthUV*_CausticsDistortionScale*0.1)+_Time.y*_CausticsDistortionSpeed*0.05));
				float2 causticsUV = (depthUV + uvOffset + (causticsOffset.xy * _CausticsDistortion)) * _CausticsScale;
				float voronoi0 = Voronoi2D(causticsUV, _Time.y*_CausticsSpeed);
				float voronoi1 = Voronoi2D(causticsUV, (_Time.y*_CausticsSpeed)+_CausticsDisp);
				float voronoi2 = Voronoi2D(causticsUV, (_Time.y*_CausticsSpeed)+_CausticsDisp*2.0);
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
			col.rgb = lerp(col.rgb, lerp(_FogTint.rgb, col.rgb, fogDepth), _FogTint.a);
		#endif
	}

	#if FOAM_ENABLED
		float foamDepth = saturate(pow(depth,_FoamPower));
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
		if (!i.isInVRMirror)
			foam = saturate(foamTex.a * foamDepth * _FoamOpacity * foamTexNoise * Average(foamTex.rgb));
		col.rgb = lerp(col.rgb, foamTex.rgb, foam);
		
		float crestThreshold = smoothstep(_FoamCrestThreshold, 1, i.wave.y);
		float crestFoam = saturate(foamTex.a * _FoamOpacity * _FoamCrestStrength * crestThreshold * foamCrestNoise * 10);
		col.rgb = lerp(col.rgb, foamTex.rgb, crestFoam);

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

	#if PBR_ENABLED
		i.normal = normalize(dot(i.normal, i.normal) >= 1.01 ? i.cNormal : i.normal);
		float3 normalDir = normalize(normalMap.x * i.tangent + normalMap.y * i.binormal + normalMap.z * i.normal);
		if (!isFrontFace)
			normalDir = -normalDir;
		float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
		float NdotV = abs(dot(normalDir, viewDir));
		col *= lerp(_AngleTint, 1, NdotV);
		#if FOAM_ENABLED
			float foamLerp = (foam + crestFoam);
			_Roughness = lerp(_Roughness, _FoamRoughness, foamLerp);
		#endif
		float roughSq = _Roughness * _Roughness;
		float roughBRDF = max(roughSq, 0.003);
			
		float omr = unity_ColorSpaceDielectricSpec.a - _Metallic * unity_ColorSpaceDielectricSpec.a;
		float3 specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, 1, _Metallic);
		float3 specCol = 0;
		float3 reflCol = 0;
		#if SPECULAR_ENABLED
			if (isFrontFace){
				float roughInterp = smoothstep(0.001, 0.003, roughSq);
				#if BASE_PASS
					float3 lightDir = _Specular == 1 ? UnityWorldSpaceLightDir(i.worldPos) : _LightDir;
					lightDir = normalize(lightDir);
				#else
					float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
				#endif
				float3 halfVector = Unity_SafeNormalize(lightDir + viewDir);
				float NdotL = saturate(dot(normalDir, lightDir));
				float NdotH = saturate(dot(normalDir, halfVector));
				float LdotH = saturate(dot(lightDir, halfVector));
				float3 fresnelTerm = FresnelTerm(specularTint, LdotH);
				float specularTerm = SpecularTerm(NdotL, NdotV, NdotH, roughBRDF);
				float3 specLightCol = _Specular == 1 ? _LightColor0 : 1;
				specCol = specLightCol * fresnelTerm * specularTerm;
				specCol = lerp(smootherstep(0, 0.9, specCol), specCol, roughInterp) * _SpecStrength * _SpecTint;
				#if defined(UNITY_PASS_FORWARDADD)
					specCol *= atten;
				#endif
			}
		#endif

		#if REFLECTIONS_ENABLED
			if (isFrontFace){			
				float3 reflDir = reflect(-viewDir, normalDir);
				float surfaceReduction = 1.0 / (roughBRDF*roughBRDF + 1.0);
				float grazingTerm = saturate((1-_Roughness) + (1-omr));
				float fresnel = FresnelLerp(specularTint, grazingTerm, NdotV);
				#if REFLECTIONS_MANUAL_ENABLED
					reflCol = GetManualReflections(reflDir, _Roughness);
				#else
					reflCol = GetWorldReflections(reflDir, i.worldPos, _Roughness);
				#endif
				#if SSR_ENABLED
					half4 ssrCol = GetSSR(i.worldPos, viewDir, reflDir, normalDir, 1-_Roughness, col.rgb, _Metallic, screenUV, i.uvGrab);
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
				reflCol = reflCol * fresnel * surfaceReduction * _ReflStrength * _ReflTint;
			}
		#endif
		col.rgb += specCol;
		col.rgb += reflCol;
	#endif
	
	#if EDGEFADE_ENABLED
		float edgeFadeDepth = 1;
		if (!i.isInVRMirror){
			edgeFadeDepth = saturate(1-GetDepth(i, baseUV));
			edgeFadeDepth = (1-saturate(pow(edgeFadeDepth, _EdgeFadePower)));
			edgeFadeDepth = saturate(Remap(edgeFadeDepth, 0, 1, -_EdgeFadeOffset, 1));
		}
	#endif

	#if defined(UNITY_PASS_FORWARDADD)
		#if EDGEFADE_ENABLED
			col = lerp(0, col, edgeFadeDepth);
		#endif
		col.rgb *= _LightColor0 * atten;
		col = lerp(0, col, _Opacity);
	#else
		#if EDGEFADE_ENABLED
			col = lerp(baseCol, col, edgeFadeDepth);
		#endif
		col = lerp(baseCol, col, _Opacity);
	#endif
	UNITY_APPLY_FOG(i.fogCoord, col);
	return col;
}

#endif // WATER_PASS_INCLUDED