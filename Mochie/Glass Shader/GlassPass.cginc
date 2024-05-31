#ifndef GLASS_PASS_INCLUDED
#define GLASS_PASS_INCLUDED

v2f vert (appdata v){
	v2f o = (v2f)0;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	
	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = v.uv;
	o.uvGrab = ComputeGrabScreenPos(o.pos);

	o.normal = UnityObjectToWorldNormal(v.normal);
	o.tangent.xyz = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0)).xyz);
	o.tangent.w = v.tangent.w;
	o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	o.cameraPos = GetCameraPos();
	o.localPos = v.vertex;
	
	UNITY_TRANSFER_SHADOW(o, o.pos)
	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}

float4 frag (v2f i, bool isFrontFace : SV_IsFrontFace) : SV_Target {

	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
	atten = FadeShadows(i.worldPos, atten);

	#if defined(_RAIN_ON)
		float2 maskUV = TRANSFORM_TEX(i.uv, _RainMask);
	#endif

	if (_TexCoordSpace == 1){
		float2 worldYZ = Rotate2D(i.worldPos.yz, -90);
		float2 worldCoordSelect[3] = {i.worldPos.xy, -i.worldPos.xz, worldYZ}; 
		float2 worldCoords = worldCoordSelect[_TexCoordSpaceSwizzle];
		i.uv.xy = worldCoords;
	}
	i.uv.xy *= abs(_GlobalTexCoordScale);

	float3 specCol = 0;
	float3 reflCol = 0;
	float flipbookBase = 0;
	float rainThreshold = 0;

	float3 normalDir = normalize(i.normal);
	float3 normalMap = 0;
	#if defined(_NORMALMAP_ON)
		normalMap = UnpackScaleNormal(SampleTexture(_NormalMap, TRANSFORM_TEX(i.uv, _NormalMap)), _NormalStrength);
	#endif
	#if defined(_RAIN_ON)
		float rainMask = tex2D(_RainMask, maskUV);
		float3 rainNormal = normalDir;
		#if defined(_RAINMODE_RIPPLE)
			rainNormal = GetRipplesNormal(i.uv, _RippleScale, _RippleStrength*rainMask, _RippleSpeed, _RippleSize, _RippleDensity);
		#elif defined(_RAINMODE_AUTO)
			float facingAngle = 1-abs(dot(normalDir, float3(0,1,0)));
			float threshAngle = _RainThresholdSize * 0.5;
			rainThreshold = smoothstep(_RainThreshold - threshAngle, _RainThreshold + threshAngle, facingAngle);
			float3 rainNormal0 = GetRipplesNormal(i.uv, _RippleScale, _RippleStrength*rainMask, _RippleSpeed, _RippleSize, _RippleDensity);
			float3 rainNormal1 = GetFlipbookNormals(i, flipbookBase, rainMask);
			ApplyExtraDroplets(i, rainNormal1, flipbookBase, rainMask);
			rainNormal = lerp(rainNormal0, rainNormal1, rainThreshold);
		#else
			rainNormal = GetFlipbookNormals(i, flipbookBase, rainMask);
			ApplyExtraDroplets(i, rainNormal, flipbookBase, rainMask);
		#endif
		#if defined(_NORMALMAP_ON)
			normalMap = BlendNormals(rainNormal, normalMap);
		#else
			normalMap = rainNormal;
		#endif
	#endif
	#if defined(_NORMALMAP_ON) || defined(_RAIN_ON)
		float3 binormal = cross(i.normal, i.tangent.xyz) * (i.tangent.w * unity_WorldTransformParams.w);
		normalDir = normalize(normalMap.x * i.tangent + normalMap.y * binormal + normalMap.z * i.normal);
	#endif
	normalDir = lerp(-normalDir, normalDir, isFrontFace);
	normalMap = lerp(-normalMap, normalMap, isFrontFace);
	
	float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
	float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
	float3 reflDir = reflect(-viewDir, normalDir);

	float roughnessMap = SampleTexture(_RoughnessMap, TRANSFORM_TEX(i.uv, _RoughnessMap)) * _Roughness;
	float flipbookRoughness = flipbookBase;
	#if defined(_RAINMODE_AUTO)
		flipbookBase *= rainThreshold;
	#endif
	float roughness = saturate(roughnessMap-flipbookBase);

	#if defined(_SPECULAR_HIGHLIGHTS_ON) || defined(_REFLECTIONS_ON)
		float roughSq = roughness * roughness;
		float roughBRDF = max(roughSq, 0.003);
		float metallic = SampleTexture(_MetallicMap, TRANSFORM_TEX(i.uv, _MetallicMap)) * _Metallic;
		float omr = unity_ColorSpaceDielectricSpec.a - metallic * unity_ColorSpaceDielectricSpec.a;
		float3 specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, 1, metallic);

		float3 halfVector = normalize(lightDir + viewDir);
		float NdotL = dot(normalDir, lightDir);
		float NdotH = Safe_DotClamped(normalDir, halfVector);
		float LdotH = Safe_DotClamped(lightDir, halfVector);
		float NdotV = abs(dot(normalDir, viewDir));

		#if defined(_SPECULAR_HIGHLIGHTS_ON)
			float3 fresnelTerm = FresnelTerm(specularTint, LdotH);
			float specularTerm = SpecularTerm(NdotL, NdotV, NdotH, roughBRDF);
			specCol = _LightColor0 * fresnelTerm * specularTerm * atten;
		#endif

		#if defined(_REFLECTIONS_ON)
			float surfaceReduction = 1.0 / (roughBRDF*roughBRDF + 1.0);
			float grazingTerm = saturate((1-_Roughness) + (1-omr));
			float fresnel = FresnelLerp(specularTint, grazingTerm, NdotV);
			reflCol = GetWorldReflections(reflDir, i.worldPos, roughness) * fresnel * surfaceReduction;
		#endif
	#endif

	// float2 offset = lerp(normalMap, normalDir, _RefractMeshNormals) * _Refraction * 0.01;
	float2 offset = normalMap * _Refraction * 0.01;
	float2 screenUV = (i.uvGrab.xy / max(EPSILON, i.uvGrab.w)) + offset;
	// float3 wPos = GetWorldSpacePixelPos(i.localPos, screenUV);
	// float dist = distance(wPos, i.cameraPos);
	// _Blur *= 1-min(dist/10, 1);

	float3 grabCol = 0;

	#ifdef _GRABPASS_ON
		float blurStr = _Blur * 0.0125;
		#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
			blurStr *= 0.25;
		#endif
		if (_Roughness > 0 && _Blur > 0)
			grabCol = BlurredGrabpassSample(screenUV, (roughness * blurStr));
		else
			grabCol = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_GlassGrab, screenUV);
		grabCol *= _GrabpassTint;
	#endif

	float4 baseColorTex = SampleTexture(_BaseColor, TRANSFORM_TEX(i.uv, _BaseColor)) * _BaseColorTint;
	float3 baseColor = baseColorTex.rgb * baseColorTex.a;
	#if defined(_LIT_BASECOLOR_ON) || defined(UNITY_PASS_FORWARDADD)
		float3 lightCol = ShadeSH9(normalDir) + _LightColor0;
		baseColor *= saturate(lightCol) * atten;
	#endif
	float occlusion = lerp(1, SampleTexture(_OcclusionMap, TRANSFORM_TEX(i.uv, _OcclusionMap)), _Occlusion);
	float3 specularity = (specCol + reflCol) * _SpecularityTint;

	float3 col = (specularity + grabCol + baseColor) * occlusion;
	#ifdef _GRABPASS_ON
		float4 finalCol = float4(col, 1);
	#else
		float4 finalCol = float4(col, 0);
	#endif

	UNITY_APPLY_FOG(i.fogCoord, finalCol);
	return finalCol; // float4(flipbookBase.xxx, 1);
}

#endif