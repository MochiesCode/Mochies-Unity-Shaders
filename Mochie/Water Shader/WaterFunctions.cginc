
#ifndef WATER_FUNCTIONS_INCLUDED
#define WATER_FUNCTIONS_INCLUDED

float2 ScaleUV(float2 uv, float2 scale, float2 scroll){
	return (uv + scroll * _Time.y * 0.1) * scale;
}

void ParallaxOffset(v2f i, inout float2 uv, float offset, bool isFrontFace){
	if (isFrontFace)
		uv -= (i.tangentViewDir.xy * offset);
}

void CalculateTangentViewDir(inout v2f i){
	i.tangentViewDir = normalize(i.tangentViewDir);
	i.tangentViewDir.xy /= (i.tangentViewDir.z + 0.42);
}

float3 BoxProjection(float3 dir, float3 pos, float4 cubePos, float3 boxMin, float3 boxMax){
	#if UNITY_SPECCUBE_BOX_PROJECTION
		UNITY_BRANCH
		if (cubePos.w > 0){
			float3 factors = ((dir > 0 ? boxMax : boxMin) - pos) / dir;
			float scalar = min(min(factors.x, factors.y), factors.z);
			dir = dir * scalar + (pos - cubePos);
		}
	#endif
	return dir;
}

float3 GetWorldReflections(float3 reflDir, float3 worldPos, float roughness){
	float3 baseReflDir = reflDir;
	roughness *= 1.7-0.7*roughness;
	reflDir = BoxProjection(reflDir, worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
	float4 envSample0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
	float3 p0 = DecodeHDR(envSample0, unity_SpecCube0_HDR);
	float interpolator = unity_SpecCube0_BoxMin.w;
	UNITY_BRANCH
	if (interpolator < 0.99999){
		float3 refDirBlend = BoxProjection(baseReflDir, worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
		float4 envSample1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, refDirBlend, roughness * UNITY_SPECCUBE_LOD_STEPS);
		float3 p1 = DecodeHDR(envSample1, unity_SpecCube1_HDR);
		p0 = lerp(p1, p0, interpolator);
	}
	return p0;
}

float3 GetManualReflections(float3 reflDir, float roughness){
	roughness *= 1.7-0.7*roughness;
	reflDir = Rotate3D(reflDir, _ReflCubeRotation);
	float4 envSample0 = texCUBElod(_ReflCube, float4(reflDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
	return DecodeHDR(envSample0, _ReflCube_HDR);
}

float SpecularTerm(float NdotL, float NdotV, float NdotH, float roughness){
	float visibilityTerm = 0;
	float rough = roughness;
	float rough2 = roughness * roughness;

	float lambdaV = NdotL * (NdotV * (1 - rough) + rough);
	float lambdaL = NdotV * (NdotL * (1 - rough) + rough);

	visibilityTerm = 0.5f / (lambdaV + lambdaL + 1e-5f);
	float d = (NdotH * rough2 - NdotH) * NdotH + 1.0f;
	float dotTerm = UNITY_INV_PI * rough2 / (d * d + 1e-7f);

	return max(0, visibilityTerm * dotTerm * UNITY_PI * NdotL);
}

float FadeShadows (float3 worldPos, float atten) {
	#if HANDLE_SHADOWS_BLENDING_IN_GI
		float viewZ = dot(_WorldSpaceCameraPos - worldPos, UNITY_MATRIX_V[2].xyz);
		float shadowFadeDistance = UnityComputeShadowFadeDistance(worldPos, viewZ);
		float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
		atten = saturate(atten + shadowFade);
	#endif
	return atten;
}

float GetDepth(v2f i, float2 screenUV){
	#if UNITY_UV_STARTS_AT_TOP
		if (_CameraDepthTexture_TexelSize.y < 0) {
			screenUV.y = 1 - screenUV.y;
		}
	#endif
	screenUV.y = _ProjectionParams.x * .5 + .5 - screenUV.y * _ProjectionParams.x;
	float backgroundDepth = LinearEyeDepth(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV));
	float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(i.uvGrab.z);
	float depthDifference = backgroundDepth - surfaceDepth;
	return depthDifference / 20;
}

float2 AlignWithGrabTexel(float2 uv) {
	#if UNITY_UV_STARTS_AT_TOP
		if (_CameraDepthTexture_TexelSize.y < 0) {
			uv.y = 1 - uv.y;
		}
	#endif
	return (floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) * abs(_CameraDepthTexture_TexelSize.xy);
}

float3 FlowUV (float2 uv, float2 flowVector, float time, float phase) {
	float progress = frac(time + phase);
	float3 uvw;
	uvw.xy = uv - flowVector * progress;
	uvw.xy += phase;
	uvw.xy += (time - progress) * jump;
	uvw.z = 1 - abs(1 - 2 * progress);
	return uvw;
}

float3 GerstnerWave(float4 wave, float3 vertex, float speed, float rotation, inout float3 tangent, inout float3 binormal){
	float k = 2 * UNITY_PI / wave.w;
	float c = sqrt(9.8/k);
	float2 dir = normalize(wave.xy);
	dir = Rotate2D(dir, rotation);
	float f = k * (dot(dir,vertex.xz) - c * _Time.y*0.2*speed);
	float steepness = wave.z;
	float a = steepness / k;

	if (_RecalculateNormals == 1){
		tangent += float3(
			-dir.x * dir.x * (steepness * sin(f)),
			dir.x * (steepness * cos(f)),
			-dir.x * dir.y * (steepness * sin(f))
		);
		binormal += float3(
			-dir.x * dir.y * (steepness * sin(f)),
			dir.y * (steepness * cos(f)),
			-dir.y * dir.y * (steepness * sin(f))
		);
	}
	
	return float3(0, a * sin(f), dir.y * (a*cos(f)));
}

#endif // WATER_FUNCTIONS_INCLUDED