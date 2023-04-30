#ifndef WATER_SSR_INCLUDED
#define WATER_SSR_INCLUDED

//-----------------------------------------------------------------------------------
// SCREEN SPACE REFLECTIONS
// 
// Original made by error.mdl, Toocanzs, and Xiexe.
// Reworked and updated by Mochie
//-----------------------------------------------------------------------------------

#if SSR_ENABLED

float4 _MWGrab_TexelSize;
float4 _NoiseTexSSR_TexelSize;
sampler2D _NoiseTexSSR;

float3 GetBlurredGP(const float2 texelSize, const float2 uvs, const float dim){
	float2 pixSize = 2/texelSize;
	float center = floor(dim*0.5);
	float3 refTotal = float3(0,0,0);
	for (int i = 0; i < floor(dim); i++){
		for (int j = 0; j < floor(dim); j++){
			float4 refl = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MWGrab, float2(uvs.x + pixSize.x*(i-center), uvs.y + pixSize.y*(j-center)));
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

		float rawDepth = DecodeFloatRG(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, uvDepth));
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

float4 GetSSR(const float3 wPos, const float3 viewDir, float3 rayDir, const half3 faceNormal, float smoothness, float3 albedo, float metallic, float2 screenUVs, float4 screenPos){
	
	float FdotR = dot(faceNormal, rayDir.xyz);

	UNITY_BRANCH
	if (IsInMirror() || FdotR < 0){
		return 0;
	}
	else {
		float4 noiseUvs = screenPos;
		noiseUvs.xy = (noiseUvs.xy * _MWGrab_TexelSize.zw) / (_NoiseTexSSR_TexelSize.zw * noiseUvs.w);	
		float4 noiseRGBA = tex2Dlod(_NoiseTexSSR, float4(noiseUvs.xy,0,0));
		float noise = noiseRGBA.r;
		
		float3 reflectedRay = wPos + (0.2*0.09/FdotR + noise*0.09)*rayDir;
		float4 finalPos = ReflectRay(reflectedRay, rayDir, 0.2, 0.02, 0.09, noise, 50);
		float totalSteps = finalPos.w;
		finalPos.w = 1;
		
		if (!any(finalPos.xyz)){
			return 0;
		}
		
		float4 uvs = UNITY_PROJ_COORD(ComputeGrabScreenPos(mul(UNITY_MATRIX_P, finalPos)));
		uvs.xy = uvs.xy / uvs.w;

		#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
			float xfade = 1;
		#else
			float xfade = smoothstep(0, _EdgeFadeSSR, uvs.x) * smoothstep(1, 1-_EdgeFadeSSR, uvs.x); //Fade x uvs out towards the edges
		#endif
		float yfade = smoothstep(0, _EdgeFadeSSR, uvs.y)*smoothstep(1, 1-_EdgeFadeSSR, uvs.y); //Same for y
		float lengthFade = smoothstep(1, 0, 2*(totalSteps / 50)-1);

		float blurFac = max(1,min(12, 12 * (-2)*(smoothness-1)));
		float4 reflection = float4(GetBlurredGP(_MWGrab_TexelSize.zw, uvs.xy, blurFac*1.5),1);
		// float4 reflection = float4(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MWGrab, uvs.xy).rgb,1);

		reflection.rgb = lerp(reflection.rgb, reflection.rgb*albedo.rgb,smoothstep(0, 1.75, metallic));

		reflection.a = FdotR * xfade * yfade * lengthFade;
		return max(0,reflection);
	}	
}

#endif

#endif // WATER_SSR_INCLUDED