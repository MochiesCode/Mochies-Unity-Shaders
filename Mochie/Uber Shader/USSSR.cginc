//-----------------------------------------------------------------------------------
// SCREEN SPACE REFLECTIONS
// 
// Made by error.mdl, Toocanzs, and Xiexe.
// Edits by Mochie
//-----------------------------------------------------------------------------------

#if REFLECTIONS_ENABLED && SSR_ENABLED

float3 GetBlurredGP(const sampler2D ssrg, const float2 texelSize, const float2 uvs, const float dim){
	float2 pixSize = 2/texelSize;
	float center = floor(dim*0.5);
	float3 refTotal = float3(0,0,0);
	[loop]
	for (int i = 0; i < floor(dim); i++){
		[loop]
		for (int j = 0; j < floor(dim); j++){
			float4 refl = tex2Dlod(ssrg, float4(uvs.x + pixSize.x*(i-center), uvs.y + pixSize.y*(j-center),0,0));
			refTotal += refl.rgb;
		}
	}
	return refTotal/(floor(dim)*floor(dim));
}

float4 ReflectRay(float3 reflectedRay, float3 rayDir, float _LRad, float _SRad, float _Step, float noise, const int maxIterations){
	
	#if UNITY_SINGLE_PASS_STEREO
		half x_min = 0.5*unity_StereoEyeIndex;
		half x_max = 0.5 + 0.5*unity_StereoEyeIndex;
	#else
		half x_min = 0.0;
		half x_max = 1.0;
	#endif
	
	static const float4x4 worldToDepth = mul(UNITY_MATRIX_MV, unity_WorldToObject);
	reflectedRay = mul(worldToDepth, float4(reflectedRay, 1));
	rayDir = mul(worldToDepth, float4(rayDir, 0));
	int totalIterations = 0;
	int direction = 1;
	float3 finalPos = 0;
	float step = _Step;
	float lRad = _LRad;
	float sRad = _SRad;

	[loop]
	for (int i = 0; i < maxIterations; i++){
		totalIterations = i;
		float4 spos = ComputeGrabScreenPos(mul(UNITY_MATRIX_P, float4(reflectedRay, 1)));
		float2 uvDepth = spos.xy / spos.w;
		UNITY_BRANCH
		if (uvDepth.x > x_max || uvDepth.x < x_min || uvDepth.y > 1 || uvDepth.y < 0){
			break;
		}

		float rawDepth = DecodeFloatRG(tex2Dlod(_CameraDepthTexture,float4(uvDepth,0,0)));
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

float4 GetSSRColor(
	const float4 wPos, const float3 viewDir, float3 rayDir, const half3 faceNormal, float smoothness, float4 albedo, float metallic, float mask, float2 screenUVs, float4 screenPos
){
	
	if (mask < 0.01)
		return 0;
	else {

		float FdotR = dot(faceNormal, rayDir.xyz);

		// Changed dithering to skip 4x4 blocks and moved it up to avoid unnecessary noise texture samples - Mochie
		float2 ditherUV = floor((_SSRGrab_TexelSize.zw*screenUVs.xy) * 0.5) * 0.5;
		float dither = frac(ditherUV.x + ditherUV.y);
		dither *= _Dith;

		if (dither != 0) {
			return 0;
		}
		else {
			float4 noiseUvs = screenPos;
			noiseUvs.xy = (noiseUvs.xy * _SSRGrab_TexelSize.zw) / (_NoiseTexSSR_TexelSize.zw * noiseUvs.w);	
			float4 noiseRGBA = tex2Dlod(_NoiseTexSSR, float4(noiseUvs.xy,0,0));
			float noise = noiseRGBA.r;
			
			float3 reflectedRay = wPos.xyz + (_LRad*_Step/FdotR + noise*_Step)*rayDir;
			
			// scatter rays based on roughness. WARNING. CAN BE EXTREMELY EXPENSIVE. RANDOM AND SPREAD OUT TEXTURE SAMPLES ARE VERY INEFFCIENT
			// YOU MAY WANT TO REMOVE THIS.
			float scatterMult = 0.2;
			float4 scatter = float4(0.5 - noiseRGBA.rgb,0);
			rayDir = normalize(rayDir + scatterMult*scatter*(1-smoothness)*sqrt(FdotR));

			if (FdotR < 0){
				return 0;
			}
			else {
				
				float4 finalPos = ReflectRay(reflectedRay, rayDir, _LRad, _SRad, _Step, noise, _MaxSteps);
				float totalSteps = finalPos.w;
				finalPos.w = 1;
				if (finalPos.x == 0 && finalPos.y == 0 && finalPos.z == 0){
					return 0;
				}
				
				float4 uvs = UNITY_PROJ_COORD(ComputeGrabScreenPos(mul(UNITY_MATRIX_P, finalPos)));
				uvs.xy = uvs.xy / uvs.w;

				#if UNITY_SINGLE_PASS_STEREO
					float xfade = 1;
				#else
					float xfade = smoothstep(0, _EdgeFade, uvs.x)*smoothstep(1, 1-_EdgeFade, uvs.x); //Fade x uvs out towards the edges
				#endif
				float yfade = smoothstep(0, _EdgeFade, uvs.y)*smoothstep(1, 1-_EdgeFade, uvs.y); //Same for y
				float lengthFade = smoothstep(1, 0, 2*(totalSteps / _MaxSteps)-1);
			
				float fade = xfade * yfade * lengthFade;
			
				// Get the color of the grabpass at the ray's screen uv location, applying
				// an (expensive) _Blur effect to partially simulate roughness
				// Second input for GetBlurredGP is some math to make it so the max _Blurring
				// occurs at 0.5 smoothness.
				float blurFac = max(1,min(12, 12 * (-2)*(smoothness-1)));
				float4 reflection = float4(GetBlurredGP(_SSRGrab, _SSRGrab_TexelSize.zw, uvs.xy, blurFac),1);
				
				// If you're alpha-blending the reflection, then multiplying the alpha by the reflection
				// strength and fade is enough. If you're adding the reflection, then you'll need to
				// also multiply the color by those terms.
				reflection.rgb = lerp(reflection.rgb, reflection.rgb*albedo.rgb, metallic);

				float RdotV = dot(rayDir, viewDir);
				reflection.a = FdotR*fade*smoothness*_Alpha;
				return max(0,reflection);
			}
		}
	}	
}

#endif

// REFLECT RAY FUNCTION

/** @brief March a ray from a given position in a given direction
*         until it intersects the depth buffer.
*
*  Given a starting location and direction march a ray in fixed steps. Each
*  step convert the ray's position to screenspace coordinates and depth, and
*  compare to the the depth texture's value at that locaion. If the ray is
*  within _LRad of the depth buffer, reduce the fixed step size to 1/10
*  of the original value. If the depth in the depth texture is also smaller
*  than the rays current depth, reverse the direction. Repeat until the ray
*  is within _SRad of the depth texture or the maximum number of
*  iterations is exceeded. Additionally, the loop will be cut short if the
*  ray passes out of the camera's view.
*  
*  @param reflectedRay Starting position of the ray, in world space
*  @param rayDir Direction the ray is going, in world space
*  @param _LRad Distance above/below the depth texture the ray must be
*         within before it will slow down and possibly reverse direction.
*         Expressed in world-space units
*  @param _SRad Distance above/below the depth texture the ray must be
*         before it can be considered to have successfully intersected the
*         depth texture. World-space units.
*  @param _Step Initial (large) size of the steps the ray moves each
*         iteration before it gets within _LRad of the depth texture.
*         In world space coordinates/scale
*  @param noise Random noise added to offset the ray's starting position.
*         This dramatically helps to hide repeating artifacts from the ray-
*         marching process.
*  @param maxIterations The maximum number of times we can step the ray
*         before we give up.
*  @return The final xyz position of the ray, with the number of iterations
*          it took stored in the w component. If the function ran out of
*          iterations or the ray went off screen, the xyz will be (0,0,0).
*/

// GET SSR FUNCTION

/** @brief Gets the reflected color for a pixel
*  
*  Same as getSSRColor, but it takes the reflected rays direction instead of internally
*  calculating the reflection direction. If you're getting the cubemap reflection, you'll
*  have already calculated the same reflection direction elsewhere so it makes no sense to
*  calculate it again.
*
*	@param wPos World position of the fragment
*  @param viewDir World-space view direction of the fragment
*  @param rayDir Reflected ray's world-space direction
*  @param faceNormal Raw mesh normal direction
*  @param _LRad Large intersection radius for the ray (see ReflectRay())
*  @param _SRad Small intersection radius for the ray (see ReflectRay())
*  @param _Step initial step size for the ray (see ReflectRay())
*  @param _Blur Square root of the max number of texture samples that can be taken to _Blur the grabpass
*  @param _MaxSteps Max number of steps the ray can go
*  @param _Dith Only do SSR on 1 out of every 2x2 pixel block if 1, otherwise do on every pixel if 0
*  @param smoothness Smoothness, determines how _Blurred the grabpass is, how scattered the rays are, and how strong the reflection is
*  @param _EdgeFade How far off the edges of the screen the reflection gets faded out
*  @param _SSRGrab_TexelSize.zw width, height of screen in pixels (It is wise to use the zw components of the texel size of the grabpass for this,
*		   unity's screen params give the wrong width for single pass stereo cameras)
*  @param _SSRGrab Grabpass sampler
*  @param _NoiseTexSSR Noise texture sampler
*  @param _NoiseTexSSR_TexelSize.zw width/height of the noise texture
*  @param albedo Albedo color of the pixel
*  @param metallic How strongly the reflection color is influenced by the albedo color
*  @param _RTint Override for how metallic the surface is, not necessary, I should remove this.
*  @param mask Mask for how strong the SSR is. Useful for making the SSR only affect certain parts of a material without making them less smooth
*  @param _Alpha Multiplier for how intense the SSR should be
*/