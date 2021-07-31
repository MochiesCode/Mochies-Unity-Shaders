#ifndef MOCHIE_STANDARD_PARALLAX_INCLUDED
#define MOCHIE_STANDARD_PARALLAX_INCLUDED

float2 Rotate(float2 coords, float rot){
	rot *= (UNITY_PI/180.0);
	float sinVal = sin(rot);
	float cosX = cos(rot);
	float2x2 mat = float2x2(cosX, -sinVal, sinVal, cosX);
	mat = ((mat*0.5)+0.5)*2-1;
	return mul(coords, mat);
}

float2 ParallaxOffsetMultiStep(float surfaceHeight, float strength, float2 uv, float3 tangentViewDir){
    float2 uvOffset = 0;
	float2 prevUVOffset = 0;
	float stepSize = 1.0/_ParallaxSteps;
	float stepHeight = 1;
	tangentViewDir.xy = Rotate(tangentViewDir.xy, _UV0Rotate);
	float2 uvDelta = tangentViewDir.xy * (stepSize * strength);
	float prevStepHeight = stepHeight;
	float prevSurfaceHeight = surfaceHeight;

	#if WORKFLOW_PACKED
		[unroll(50)]
		for (int j = 1; j <= _ParallaxSteps && stepHeight > surfaceHeight; j++){
			prevUVOffset = uvOffset;
			prevStepHeight = stepHeight;
			prevSurfaceHeight = surfaceHeight;
			uvOffset -= uvDelta;
			stepHeight -= stepSize;
			float4 heightSample = SampleTexture(_PackedMap, uv+uvOffset);
			surfaceHeight = ChannelCheck(heightSample, _HeightChannel) + _ParallaxOffset;
		}
		[unroll(3)]
		for (int k = 0; k < 3; k++) {
			uvDelta *= 0.5;
			stepSize *= 0.5;

			if (stepHeight < surfaceHeight) {
				uvOffset += uvDelta;
				stepHeight += stepSize;
			}
			else {
				uvOffset -= uvDelta;
				stepHeight -= stepSize;
			}
			float4 heightSample = SampleTexture(_PackedMap, uv+uvOffset);
			surfaceHeight = ChannelCheck(heightSample, _HeightChannel) + _ParallaxOffset;
		}
	#else
		[unroll(50)]
		for (int j = 1; j <= _ParallaxSteps && stepHeight > surfaceHeight; j++){
			prevUVOffset = uvOffset;
			prevStepHeight = stepHeight;
			prevSurfaceHeight = surfaceHeight;
			uvOffset -= uvDelta;
			stepHeight -= stepSize;
			surfaceHeight = SampleTexture(_ParallaxMap, uv+uvOffset) + _ParallaxOffset;
		}
		[unroll(3)]
		for (int k = 0; k < 3; k++) {
			uvDelta *= 0.5;
			stepSize *= 0.5;

			if (stepHeight < surfaceHeight) {
				uvOffset += uvDelta;
				stepHeight += stepSize;
			}
			else {
				uvOffset -= uvDelta;
				stepHeight -= stepSize;
			}
			surfaceHeight = SampleTexture(_ParallaxMap, uv+uvOffset) + _ParallaxOffset;
		}
	#endif

    return uvOffset;
}

#endif // MOCHIE_STANDARD_PARALLAX_INCLUDED