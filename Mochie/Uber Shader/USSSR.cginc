#ifndef USSSR_INCLUDED
#define USSSR_INCLUDED

//-----------------------------------------------------------------------------------
// SCREEN SPACE REFLECTIONS
// 
// Original made by error.mdl, Toocanzs, and Xiexe.
// Reworked and updated by Mochie
//-----------------------------------------------------------------------------------

#if REFLECTIONS_ENABLED && SSR_ENABLED

float3 GetBlurredGP(const float2 texelSize, const float2 uvs, const float dim){
	float2 pixSize = 2/texelSize;
	float center = floor(dim*0.5);
	float3 refTotal = float3(0,0,0);
	for (int i = 0; i < floor(dim); i++){
		for (int j = 0; j < floor(dim); j++){
			float4 refl = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MUSGrab, float2(uvs.x + pixSize.x*(i-center), uvs.y + pixSize.y*(j-center)));
			refTotal += refl.rgb;
		}
	}
	return refTotal/(floor(dim)*floor(dim));
}

float4 ReflectRay(float3 reflectedRay, float3 rayDir, float _LRad, float _SRad, float _Step, float noise, const int maxIterations){
	
	#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		half x_min = 0.5*unity_StereoEyeIndex;
		half x_max = 0.5 + 0.5*unity_StereoEyeIndex;
	#else
		half x_min = 0.0;
		half x_max = 1.0;
	#endif
	
	reflectedRay = mul(UNITY_MATRIX_V, float4(reflectedRay, 1));
	rayDir = mul(UNITY_MATRIX_V, float4(rayDir, 0));
	int totalIterations = 0;
	int direction = 1;
	float3 finalPos = 0;
	float step = _Step;
	float lRad = _LRad;
	float sRad = _SRad;

	for (int i = 0; i < maxIterations; i++){
		totalIterations = i;
		float4 spos = ComputeGrabScreenPos(mul(UNITY_MATRIX_P, float4(reflectedRay, 1)));
		float2 uvDepth = spos.xy / spos.w;
		UNITY_BRANCH
		if (uvDepth.x > x_max || uvDepth.x < x_min || uvDepth.y > 1 || uvDepth.y < 0){
			break;
		}

		float rawDepth = DecodeFloatRG(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture,uvDepth));
		float linearDepth = Linear01Depth(rawDepth);
		float sampleDepth = -reflectedRay.z;
		float realDepth = linearDepth * _ProjectionParams.z;
		float depthDifference = abs(sampleDepth - realDepth);

		if (depthDifference < lRad){ 
			if (direction == 1){
				if(sampleDepth > (realDepth - sRad)){
					if(sampleDepth < (realDepth + sRad)){
						finalPos = reflectedRay;
						break;
					}
					direction = -1;
					step = step*0.1;
				}
			}
			else {
				if(sampleDepth < (realDepth + sRad)){
					direction = 1;
					step = step*0.1;
				}
			}
		}
		reflectedRay = reflectedRay + direction*step*rayDir;
		step += step*(0.025 + 0.005*noise);
		lRad += lRad*(0.025 + 0.005*noise);
		sRad += sRad*(0.025 + 0.005*noise);
	}
	return float4(finalPos, totalIterations);
}

float4 GetSSRColor(const float4 wPos, const float3 viewDir, float3 rayDir, const half3 faceNormal, float smoothness, float4 albedo, float metallic, float mask, float2 screenUVs, float4 screenPos){

	float FdotR = dot(faceNormal, rayDir.xyz);

	UNITY_BRANCH
	if (IsInMirror() || FdotR < 0 || mask < 0.001){
		return 0;
	}
	else {

		float4 noiseUvs = screenPos;
		noiseUvs.xy = (noiseUvs.xy * _MUSGrab_TexelSize.zw) / (_NoiseTexSSR_TexelSize.zw * noiseUvs.w);	
		float4 noiseRGBA = MOCHIE_SAMPLE_TEX2D_LOD(_NoiseTexSSR, noiseUvs.xy,0);
		float noise = noiseRGBA.r;
		
		float3 reflectedRay = wPos.xyz + (_LRad*_Step/FdotR + noise*_Step)*rayDir;
		
		float scatterMult = 0.2;
		float4 scatter = float4(0.5 - noiseRGBA.rgb,0);
		rayDir = normalize(rayDir + scatterMult*scatter*(1-smoothness)*sqrt(FdotR));

		float4 finalPos = ReflectRay(reflectedRay, rayDir, _LRad, _SRad, _Step, noise, _MaxSteps);
		float totalSteps = finalPos.w;
		finalPos.w = 1;

		if (!any(finalPos.xyz))
			return 0;
		
		float4 uvs = UNITY_PROJ_COORD(ComputeGrabScreenPos(mul(UNITY_MATRIX_P, finalPos)));
		uvs.xy = uvs.xy / uvs.w;

		#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
			float xfade = 1;
		#else
			float xfade = smoothstep(0, _EdgeFade, uvs.x)*smoothstep(1, 1-_EdgeFade, uvs.x); //Fade x uvs out towards the edges
		#endif
		float yfade = smoothstep(0, _EdgeFade, uvs.y)*smoothstep(1, 1-_EdgeFade, uvs.y); //Same for y
		float lengthFade = smoothstep(1, 0, 2*(totalSteps / _MaxSteps)-1);
	
		float fade = xfade * yfade * lengthFade;

		float blurFac = max(1,min(12, 12 * (-2)*(smoothness-1)));
		float4 reflection = float4(GetBlurredGP(_MUSGrab_TexelSize.zw, uvs.xy, blurFac),1);
		
		reflection.rgb = lerp(reflection.rgb, reflection.rgb*albedo.rgb, metallic);

		float RdotV = dot(rayDir, viewDir);
		reflection.a = FdotR*fade*smoothness*_Alpha;
		return max(0,reflection);
	}
}

#endif

#endif // USSSR_INCLUDED