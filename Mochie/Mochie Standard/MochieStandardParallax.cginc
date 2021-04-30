#ifndef MOCHIE_STANDARD_PARALLAX_INCLUDED
#define MOCHIE_STANDARD_PARALLAX_INCLUDED

float2 ParallaxOffsetMultiStep(float surfaceHeight, float strength, float2 uv, float3 tangentViewDir){
    float2 uvOffset = 0;
	float2 prevUVOffset = 0;
	float stepSize = 1.0/_ParallaxSteps;
	float stepHeight = 1;
	tangentViewDir.xy = Rotate2D(tangentViewDir.xy, _UV0Rotate);
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
			surfaceHeight = tex2D(_PackedMap, uv+uvOffset).a + _ParallaxOffset;
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
			surfaceHeight = tex2D(_PackedMap, uv+uvOffset).a + _ParallaxOffset;
		}
	#elif WORKFLOW_MODULAR
		[unroll(50)]
		for (int j = 1; j <= _ParallaxSteps && stepHeight > surfaceHeight; j++){
			prevUVOffset = uvOffset;
			prevStepHeight = stepHeight;
			prevSurfaceHeight = surfaceHeight;
			uvOffset -= uvDelta;
			stepHeight -= stepSize;
			float4 heightSample = tex2D(_PackedMap, uv+uvOffset);
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
			float4 heightSample = tex2D(_PackedMap, uv+uvOffset);
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
			surfaceHeight = tex2D(_ParallaxMap, uv+uvOffset) + _ParallaxOffset;
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
			surfaceHeight = tex2D(_ParallaxMap, uv+uvOffset) + _ParallaxOffset;
		}
	#endif

    return uvOffset;
}

#endif // MOCHIE_STANDARD_PARALLAX_INCLUDED