#include "USBRDF.cginc"

float3 ApplyLREmission(lighting l, float3 diffuse, float3 emiss){
	UNITY_BRANCH
	if (_EmissionToggle > 0){
		float interpolator = 0;
		UNITY_BRANCH
		if (_ReactToggle == 1){
			UNITY_BRANCH
			if (_CrossMode == 1){
				float2 threshold = saturate(float2(_ReactThresh-_Crossfade, _ReactThresh+_Crossfade));
				interpolator = smootherstep(threshold.x, threshold.y, l.worldBrightness); 
			}
			else {
				interpolator = l.worldBrightness;
			}
		}
		diffuse = lerp(diffuse+emiss, diffuse, interpolator);
	}
	return diffuse;
}

float FadeShadows (g2f i, float3 atten) {
    #if HANDLE_SHADOWS_BLENDING_IN_GI
        float viewZ = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
        float shadowFadeDistance = UnityComputeShadowFadeDistance(i.worldPos, viewZ);
        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        atten = saturate(atten + shadowFade);
    #endif
	return atten;
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

void GetLengthSq(g2f i, inout lighting l){
	UNITY_BRANCH
	if (i.isVLight){
		l.toLightX = unity_4LightPosX0 - i.worldPos.x;
		l.toLightY = unity_4LightPosY0 - i.worldPos.y;
		l.toLightZ = unity_4LightPosZ0 - i.worldPos.z;
		l.lengthSq += l.toLightX * l.toLightX;
		l.lengthSq += l.toLightY * l.toLightY;
		l.lengthSq += l.toLightZ * l.toLightZ;
	}
}

void GetVertexLightAtten(inout lighting l){
	float4 lightAttenSq = unity_4LightAtten0;
	float4 atten = 1.0 / (1.0 + l.lengthSq * lightAttenSq);
	l.vLightWeight = saturate(1 - (l.lengthSq * lightAttenSq / 25));
	l.vLightAtten = min(atten, l.vLightWeight * l.vLightWeight);	
}

float3 GetVertexLightColor(g2f i, lighting l) {
	float3 lightColor = 0;
	UNITY_BRANCH
	if (i.isVLight){
		
		// NdotL
		float4 NdotL = 0;
		NdotL += l.toLightX * l.normal.x;
		NdotL += l.toLightY * l.normal.y;
		NdotL += l.toLightZ * l.normal.z;

		// Correct NdotL
		float4 corr = rsqrt(l.lengthSq);
		NdotL = max(0, NdotL * corr);

		float4 vlAtten = NdotL * l.vLightAtten;
		UNITY_BRANCH
		if (_RenderMode != 2){
			float4 ramp0 = smoothstep(0, _RampWidth0+0.005, NdotL);
			float4 ramp1 = smoothstep(0, _RampWidth1+0.005, NdotL);
			vlAtten = lerp(ramp0, ramp1, _RampWeight) * l.vLightAtten;
		}

		lightColor.rgb += unity_LightColor[0] * vlAtten.x;
		lightColor.rgb += unity_LightColor[1] * vlAtten.y;
		lightColor.rgb += unity_LightColor[2] * vlAtten.z;
		lightColor.rgb += unity_LightColor[3] * vlAtten.w;
	}
	return lightColor * _VLightCont;
}

void GetLightColor(g2f i, inout lighting l, masks m){
	#if defined(UNITY_PASS_FORWARDBASE)
		float3 probeCol = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
		UNITY_BRANCH
		if (_RenderMode == 1){
			l.indirectCol = lerp(ShadeSH9(l.normal), ShadeSHNL(l.normal), _NonlinearSHToggle);
			l.indirectCol = lerp(probeCol, l.indirectCol, _SHStr*m.smoothMask);
		}
		else l.indirectCol = probeCol;

		UNITY_BRANCH
		if (l.lightEnv){
			l.directCol = _LightColor0;
			UNITY_BRANCH
			if (_RenderMode == 1){
				l.directCol *= _RTDirectCont;
				l.indirectCol *= _RTIndirectCont;
			}
		}
		else {
			UNITY_BRANCH
			if (_RenderMode == 1){
				l.directCol = l.indirectCol * _DirectCont;
				l.indirectCol *= _IndirectCont;
			}
			else {
				l.directCol = l.indirectCol * 0.6;
				l.indirectCol *= 0.5;
			}
		}

		l.worldBrightness = saturate(AverageRGB(l.directCol + l.indirectCol + l.vLightCol));
		l.directCol *= lerp(1, l.ao, _DirectAO);
		l.indirectCol *= lerp(1, l.ao, _IndirectAO);
	#else
		l.directCol = lerp(_LightColor0, clamp(_LightColor0, 0, _AdditiveMax), _ClampAdditive);
	#endif
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
	
	dirX *= length(toLightX);
	dirY *= length(toLightY);
	dirZ *= length(toLightZ);
	dirW *= length(toLightW);
	
	return normalize(dirX + dirY + dirZ + dirW);
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
		if (i.isVLight){
			float weight = smoothstep(0, 0.3, Average(l.vLightWeight) * Average(l.vLightCol));
			lightDir += GetVertexLightDir(i.worldPos) * weight;
			// lightDir += GetVertexLightDir(l, i.worldPos);
		}
	#endif

	return normalize(lightDir);
}

float3 GetViewDir(float3 worldPos){
	return normalize(_WorldSpaceCameraPos.xyz - worldPos);
}

float3 GetHalfVector(float3 lightDir, float3 viewDir){
	return normalize(lightDir + viewDir);
}

float3 GetNormal(g2f i, float3 normalDir){
	return lerp(normalDir, normalize(i.normal), _ClearCoat);
}

float3 GetBinormal(float4 tangent, float3 normalDir){
	return cross(normalDir, tangent.xyz) * (tangent.w * unity_WorldTransformParams.w);
}

float3 GetNormalDir(g2f i, float detailMask){
	float3 normalMap = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, i.uv.xy), _BumpScale);
	normalMap.y = lerp(normalMap.y, 1-normalMap.y, _InvertNormalY0);
	UNITY_BRANCH
	if (_UseDetailNormal == 1){
		float3 detailNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_DetailNormalMap, _MainTex, i.uv2.xy), _DetailNormalMapScale * detailMask);
		detailNormal.y = lerp(detailNormal.y, 1-detailNormal.y, _InvertNormalY1);
		normalMap = BlendNormals(normalMap, detailNormal);
	}
	else normalMap = normalize(normalMap);

	UNITY_BRANCH
	if (_HardenNormals == 1){
		float3 xPos = ddx(i.worldPos);
		float3 yPos = ddy(i.worldPos);
		i.normal = normalize(cross(yPos, xPos));
	}
	float3 normalDir = normalize(normalMap.x * i.tangent + normalMap.y * i.binormal + normalMap.z * i.normal);
	return normalDir;
}

float3 GetAO(g2f i){
	float3 ao = 1;
	UNITY_BRANCH
	if (_PBRWorkflow == 3){
		float4 packedTex = tex2D(_PackedMap, i.uv.xy);
		ao = ChannelCheck(packedTex, _OcclusionChannel);
	}
	else ao = UNITY_SAMPLE_TEX2D_SAMPLER(_OcclusionMap, _MainTex, i.uv.xy).rgb;

	UNITY_BRANCH
	if (_AOFiltering == 1){
		UNITY_BRANCH
		if (_UseAOTintTex == 1)
			_AOTint.rgb *= UNITY_SAMPLE_TEX2D_SAMPLER(_AOTintTex, _MainTex, i.uv.xy).rgb;
		ao = lerp(_AOTint, 1, ao);
		ao = saturate(lerp(0.5, ao, _AOContrast));
		ao += saturate(ao * _AOIntensity);
		ao = saturate(ao + _AOLightness);
	}
	ao = lerp(1, ao, _OcclusionStrength);
	return ao;
}

lighting GetLighting(g2f i, masks m, float3 atten){
    lighting l = (lighting)0;
	l.ao = 1;

	#if defined(UNITY_PASS_FORWARDBASE)
		l.lightEnv = any(_WorldSpaceLightPos0);
	#endif

	l.screenUVs = i.screenPos.xy / (i.screenPos.w+0.0000000001);
	#if UNITY_SINGLE_PASS_STEREO
		l.screenUVs.x *= 2;
	#endif

    UNITY_BRANCH
    if (_RenderMode > 0){
		l.ao = GetAO(i);
		l.viewDir = GetViewDir(i.worldPos);
		l.normalDir = GetNormalDir(i, m.detailMask);
		l.normal = GetNormal(i, l.normalDir);
		l.tangent = i.tangent;
		l.binormal = GetBinormal(l.tangent, l.normal);
		l.reflectionDir = reflect(-l.viewDir, l.normal);
		UNITY_BRANCH
		if (i.isVLight){
			GetLengthSq(i, l);
			GetVertexLightAtten(l);
			l.vLightCol = GetVertexLightColor(i, l);
		}
		l.lightDir = GetLightDir(i, l);
		l.halfVector = GetHalfVector(l.lightDir, l.viewDir);

		l.NdotL = DotClamped(l.normalDir, l.lightDir);
		l.NdotV = abs(dot(l.normal, l.viewDir));
		l.NdotH = DotClamped(l.normal, l.halfVector);
		l.LdotH = DotClamped(l.lightDir, l.halfVector);
		UNITY_BRANCH
		if (_Specular == 1){
			l.TdotH = dot(l.tangent, l.halfVector);
			l.BdotH = dot(l.binormal, l.halfVector);
		}
    }

	GetLightColor(i,l,m);

    return l;
}