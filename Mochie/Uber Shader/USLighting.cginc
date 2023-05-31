#ifndef US_LIGHTING_INCLUDED
#define US_LIGHTING_INCLUDED

float FadeShadows (g2f i, float atten) {
    #if HANDLE_SHADOWS_BLENDING_IN_GI
        float viewZ = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
        float shadowFadeDistance = UnityComputeShadowFadeDistance(i.worldPos, viewZ);
        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        atten = saturate(atten + shadowFade);
    #endif
	return atten;
}

void ApplyLREmission(lighting l, inout float3 diffuse, float3 emiss){
	float interpolator = 0;
	if (_ReactToggle == 1){
		float2 threshold = saturate(float2(_ReactThresh-_Crossfade, _ReactThresh+_Crossfade));
		float2 interps = float2(l.worldBrightness, smootherstep(threshold.x, threshold.y, l.worldBrightness));
		interpolator = interps[_CrossMode];
	}
	diffuse = lerp(diffuse+emiss, diffuse, interpolator*_ReactToggle);
}

float3 GetDetailAO(g2f i, float3 aoIn){
	float3 detailAO = MOCHIE_SAMPLE_TEX2D_SAMPLER(_DetailOcclusionMap, sampler_MainTex, i.uv2.xy);
	return BlendColors(aoIn, detailAO, _DetailOcclusionBlending);
}

float3 GetAO(g2f i, masks m){
	float3 ao = 1;
	#if PACKED_WORKFLOW
		ao = ChannelCheck(packedTex, _OcclusionChannel);
	#else
		ao = MOCHIE_SAMPLE_TEX2D_SAMPLER(_OcclusionMap, sampler_MainTex, i.uv.xy).g;
	#endif
	ao = lerp(ao, GetDetailAO(i, ao), _DetailOcclusionStrength * m.detailMask * _UsingDetailOcclusion);
	float3 tintTex = MOCHIE_SAMPLE_TEX2D_SAMPLER(_AOTintTex, sampler_MainTex, i.uv.xy).rgb;

	if (_AOFiltering == 1){
		_AOTint.rgb *= tintTex;
		ao = lerp(_AOTint, 1, ao);
		ao = Remap(ao, 0, 1, _AORemapMin, _AORemapMax);
		ApplyPBRFiltering(ao, _AOContrast, _AOIntensity, _AOLightness, _AOFiltering, prevAO);
		
	}
	ao = lerp(1, ao, _OcclusionStrength);
	return ao;
}

float3 GetNormalDir(g2f i, lighting l, masks m){
	#if !OUTLINE_PASS && (NORMALMAP_ENABLED || DETAIL_NORMALMAP_ENABLED)
	
		#if X_FEATURES
			if (_Screenspace == 1)
				return normalize(i.normal);
		#endif

		#if NORMALMAP_ENABLED && DETAIL_NORMALMAP_ENABLED
			float3 normalMap = UnpackScaleNormal(MOCHIE_SAMPLE_TEX2D_SAMPLER(_BumpMap, sampler_MainTex, i.uv.xy), _BumpScale);
			float3 detailNormal = UnpackScaleNormal(MOCHIE_SAMPLE_TEX2D_SAMPLER(_DetailNormalMap, sampler_MainTex, i.uv2.xy), _DetailNormalMapScale * m.detailMask);
			normalMap = BlendNormals(normalMap, detailNormal);
		#endif

		#if NORMALMAP_ENABLED && !DETAIL_NORMALMAP_ENABLED
			float3 normalMap = UnpackScaleNormal(MOCHIE_SAMPLE_TEX2D_SAMPLER(_BumpMap, sampler_MainTex, i.uv.xy), _BumpScale);
		#endif

		#if !NORMALMAP_ENABLED && DETAIL_NORMALMAP_ENABLED		
			float3 normalMap = UnpackScaleNormal(MOCHIE_SAMPLE_TEX2D_SAMPLER(_DetailNormalMap, sampler_MainTex, i.uv2.xy), _DetailNormalMapScale * m.detailMask);
		#endif

		float3 hardNormals = normalize(cross(ddy(i.worldPos), ddx(i.worldPos)));
		i.normal = _HardenNormals == 1 ? hardNormals : normalize(i.normal);
			
		return normalize(normalMap.x * l.tangent + normalMap.y * l.binormal + normalMap.z * i.normal);
	#else
		return normalize(i.normal);
	#endif
}

float NonlinearSH(float L0, float3 L1, float3 normal) {
    float R0 = L0;
    float3 R1 = 0.5f * L1;
    float lenR1 = length(R1);
    float q = dot(normalize(R1), normal) * 0.5 + 0.5;
    q = max(0, q);
    float p = 1.0f + 2.0f * lenR1 / R0;
    float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);
    return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
}

float3 ShadeSHNL(float3 normal) {
    float3 indirect;
    indirect.r = NonlinearSH(unity_SHAr.w, unity_SHAr.xyz, normal);
    indirect.g = NonlinearSH(unity_SHAg.w, unity_SHAg.xyz, normal);
    indirect.b = NonlinearSH(unity_SHAb.w, unity_SHAb.xyz, normal);
    return max(0, indirect);
}

float3 ShadeSH9(float3 normal){
	return max(0, ShadeSH9(float4(normal,1)));
}

void GetVertexLightData(g2f i, inout lighting l){

	// Attenuation
	float4 toLightX = unity_4LightPosX0 - i.worldPos.x;
	float4 toLightY = unity_4LightPosY0 - i.worldPos.y;
	float4 toLightZ = unity_4LightPosZ0 - i.worldPos.z;

	float4 lengthSq = 0;
	lengthSq += toLightX * toLightX;
	lengthSq += toLightY * toLightY;
	lengthSq += toLightZ * toLightZ;

	float4 atten0 = 1.0 / (1.0 + lengthSq * unity_4LightAtten0);
	float4 atten1 = saturate(1 - (lengthSq * unity_4LightAtten0 / 25));
	float4 atten = min(atten0, atten1 * atten1);

	// Shadow ramp
	float4 NdotL = 0;
	NdotL += toLightX * l.normal.x;
	NdotL += toLightY * l.normal.y;
	NdotL += toLightZ * l.normal.z;

	UNITY_BRANCH
	if (_ShadowMode == 1){
		float4 ramp0 = smootherstep(float4(0,0,0,0), _RampWidth0, NdotL-_RampPos);
		float4 ramp1 = smootherstep(float4(0,0,0,0), _RampWidth1, NdotL-_RampPos);
		atten = lerp(ramp0, ramp1, _RampWeight) * atten;
	}
	else if (_ShadowMode == 2){
		float4 rampUV = NdotL * 0.5 + 0.5;
		float ramp0 = MOCHIE_SAMPLE_TEX2D(_ShadowRamp, rampUV.xx);
		float ramp1 = MOCHIE_SAMPLE_TEX2D(_ShadowRamp, rampUV.yy);
		float ramp2 = MOCHIE_SAMPLE_TEX2D(_ShadowRamp, rampUV.zz);
		float ramp3 = MOCHIE_SAMPLE_TEX2D(_ShadowRamp, rampUV.ww);
		atten = float4(ramp0, ramp1, ramp2, ramp3) * atten;
	}

	// Color
	float3 light0 = atten.x * unity_LightColor[0];
	float3 light1 = atten.y * unity_LightColor[1];
	float3 light2 = atten.z * unity_LightColor[2];
	float3 light3 = atten.w * unity_LightColor[3];

	l.vLightCol = (light0 + light1 + light2 + light3) * _VLightCont;

	// Direction
	float3 toLightXD = float3(unity_4LightPosX0.x, unity_4LightPosY0.x, unity_4LightPosZ0.x);
    float3 toLightYD = float3(unity_4LightPosX0.y, unity_4LightPosY0.y, unity_4LightPosZ0.y);
    float3 toLightZD = float3(unity_4LightPosX0.z, unity_4LightPosY0.z, unity_4LightPosZ0.z);
	float3 toLightWD = float3(unity_4LightPosX0.w, unity_4LightPosY0.w, unity_4LightPosZ0.w);

    float3 dirX = toLightXD - i.worldPos;
    float3 dirY = toLightYD - i.worldPos;
    float3 dirZ = toLightZD - i.worldPos;
	float3 dirW = toLightWD - i.worldPos;
	
	dirX *= length(toLightXD) * light0;
	dirY *= length(toLightYD) * light1;
	dirZ *= length(toLightZD) * light2;
	dirW *= length(toLightWD) * light3;
	
	l.vLightDir = dirX + dirY + dirZ + dirW;
}

float3 GetLightDir(g2f i, lighting l) {
	float3 lightDir = UnityWorldSpaceLightDir(i.worldPos);
	#if FORWARD_PASS
		lightDir *= l.lightEnv;
		lightDir += (unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz) * !l.lightEnv;
		#if VERTEX_LIGHT
			lightDir += l.vLightDir;
		#endif
	#endif
	lightDir = lerp(lightDir, _StaticLightDir.xyz, _StaticLightDirToggle);
	return normalize(lightDir);
}

void GetLightColor(g2f i, inout lighting l, masks m){
	#if FORWARD_PASS
		float3 probeCol = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
		#if SHADING_ENABLED
			UNITY_BRANCH
			if (_NonlinearSHToggle == 1)
				l.indirectCol = ShadeSHNL(l.normal);
			else
				l.indirectCol = ShadeSH9(l.normal);
			l.indirectCol = lerp(probeCol, l.indirectCol, _SHStr*m.diffuseMask);

			l.directCol = lerp(
				l.indirectCol * _DirectCont,		// No realtime light
				_LightColor0 * _RTDirectCont,		// Realtime light
				l.lightEnv
			);

			l.indirectCol = lerp(
				l.indirectCol * _IndirectCont,		// No realtime light
				l.indirectCol * _RTIndirectCont,	// Realtime light
				l.lightEnv
			);

		#else
			l.indirectCol = probeCol;
			if (l.lightEnv){
				l.directCol = _LightColor0;
			}
			else {
				l.directCol = l.indirectCol * 0.6;
				l.indirectCol *= 0.5;
			}
		#endif
		l.worldBrightness = saturate(Average(l.directCol + l.indirectCol + l.vLightCol));
		l.directCol *= lerp(1, l.ao, _DirectAO);
		l.indirectCol *= lerp(1, l.ao, _IndirectAO);
	#else
		#if SHADING_ENABLED
			l.directCol = lerp(_LightColor0, saturate(_LightColor0), _ClampAdditive);
		#else
			l.directCol = saturate(_LightColor0);
		#endif
	#endif
}

lighting GetLighting(g2f i, masks m, float3 atten, bool frontFace){
    lighting l = (lighting)0;
	l.ao = 1;

	#if FORWARD_PASS
		l.lightEnv = any(_WorldSpaceLightPos0.xyz);
	#endif

	l.screenUVs = i.grabPos.xy / (i.grabPos.w+0.0000000001);
	#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		l.screenUVs.y *= 0.5555555;
	#else
		l.screenUVs.x *= 0.5625;
	#endif
    #if SHADING_ENABLED
		l.ao = GetAO(i, m);
		l.viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
		l.viewDirVR = normalize(i.cameraPos - i.worldPos);
		l.tangent = lerp(-i.tangent, i.tangent, frontFace);
		l.binormal = cross(i.normal, i.tangent.xyz) * (i.tangent.w * unity_WorldTransformParams.w);
		l.binormal = lerp(-l.binormal, l.binormal, frontFace);
		l.normalDir = GetNormalDir(i,l,m);
		l.normalMesh = normalize(i.normal);
		l.normal = lerp(l.normalDir, l.normalMesh, _ClearCoat);
		l.normal = lerp(-l.normal, l.normal, frontFace);
		l.reflectionDir = reflect(-l.viewDir, l.normal);
		#if VERTEX_LIGHT
			GetVertexLightData(i, l);
		#endif
		l.lightDir = GetLightDir(i, l);
		l.halfVector = normalize(l.lightDir + l.viewDir);

		l.NdotL = clamp(dot(l.normalDir, l.lightDir), -1, 1);
		l.NdotV = abs(dot(l.normal, l.viewDir));
		l.NdotH = Safe_DotClamped(l.normal, l.halfVector);
		l.LdotH = Safe_DotClamped(l.lightDir, l.halfVector);
		l.VdotL = abs(dot(l.viewDir, l.normal));
		l.VVRdotL = abs(dot(l.viewDirVR, l.normal));
		#if SPECULAR_ENABLED && !OUTLINE_PASS
			l.TdotH = dot(l.tangent, l.halfVector);
			l.BdotH = dot(l.binormal, l.halfVector);
		#endif
    #else
		#if VERTEX_LIGHT
			GetVertexLightData(i, l);
		#elif ADDITIVE_PASS
			l.lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
			l.normal = normalize(i.normal);
			l.NdotL = dot(l.normal, l.lightDir);
		#endif
	#endif

	GetLightColor(i,l,m);

    return l;
}

#endif // US_LIGHTING_INCLUDED