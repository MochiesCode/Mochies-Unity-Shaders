#ifndef STANDARD_PARALLAX_DEFINED
#define STANDARD_PARALLAX_DEFINED

// Amplify POM - ugly ah
float2 GetParallaxOffset(Texture2D heightMap, float2 uvs, float3 normalWorld, float3 viewWorld, float3 viewDirTan, float strength, int minSamples, int maxSamples, int channel){
    float3 result = 0;
    int stepIndex = 0;
    int numSteps = _HeightSteps; // (int)lerp((float)maxSamples, (float)minSamples, saturate(dot(normalWorld, viewWorld)));
    float layerHeight = 1.0 / numSteps;
    float2 plane = strength * (viewDirTan.xy / viewDirTan.z);
    float2 deltaTex = -plane * layerHeight;
    float2 prevTexOffset = 0;
    float prevRayZ = 1.0f;
    float prevHeight = 0.0f;
    float2 currTexOffset = deltaTex;
    float currRayZ = 1.0f - layerHeight;
    float currHeight = 0.0f;
    float intersection = 0;
    float2 finalTexOffset = 0;

    [unroll(16)]
    while (stepIndex < numSteps + 1){
         currHeight = SampleTexture(heightMap, uvs + currTexOffset)[channel] + _HeightOffset;
         if ( currHeight > currRayZ ){
              stepIndex = numSteps + 1;
         }
         else{
              stepIndex++;
              prevTexOffset = currTexOffset;
              prevRayZ = currRayZ;
              prevHeight = currHeight;
              currTexOffset += deltaTex;
              currRayZ -= layerHeight;
         }
    }

    int sectionSteps = 3;
    int sectionIndex = 0;
    float newZ = 0;
    float newHeight = 0;

    [unroll(3)]
    while (sectionIndex < sectionSteps){
         intersection = (prevHeight - prevRayZ) / (prevHeight - currHeight + currRayZ - prevRayZ);
         finalTexOffset = prevTexOffset + intersection * deltaTex;
         newZ = prevRayZ - intersection * layerHeight;
         newHeight = SampleTexture(heightMap, uvs + finalTexOffset)[channel] + _HeightOffset;
         if (newHeight > newZ){
              currTexOffset = finalTexOffset;
              currHeight = newHeight;
              currRayZ = newZ;
              deltaTex = intersection * deltaTex;
              layerHeight = intersection * layerHeight;
         }
         else {
              prevTexOffset = finalTexOffset;
              prevHeight = newHeight;
              prevRayZ = newZ;
              deltaTex = (1-intersection) * deltaTex;
              layerHeight = (1-intersection) * layerHeight;
         }
         sectionIndex++;
    }

    return finalTexOffset;
}

// Catlike Coding POM
float2 GetParallaxOffset(v2f i, Texture2D heightMap, float2 uv, float3 tangentViewDir, float strength, int channel){
    float2 uvOffset = 0;
    float2 prevUVOffset = 0;
    float stepSize = 1.0/_HeightSteps;
    float stepHeight = 1;
    tangentViewDir.xy = Rotate2DCentered(tangentViewDir.xy, _UVMainRotation);
    float2 uvDelta = tangentViewDir.xy * stepSize * strength;
    float prevStepHeight = stepHeight;
    float surfaceHeight = SampleTexture(heightMap, uv)[channel];
    surfaceHeight = clamp(surfaceHeight, 0, 0.999);
    float prevSurfaceHeight = surfaceHeight;

    [unroll(16)]
    for (int j = 1; j <= _HeightSteps && stepHeight > surfaceHeight; j++){
        prevUVOffset = uvOffset;
        prevStepHeight = stepHeight;
        prevSurfaceHeight = surfaceHeight;
        uvOffset -= uvDelta;
        stepHeight -= stepSize;
        surfaceHeight = SampleTexture(heightMap, uv+uvOffset)[channel] + _HeightOffset;
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
        surfaceHeight = SampleTexture(heightMap, uv+uvOffset)[channel] + _HeightOffset;
    }

    return uvOffset;
}

void ApplyParallaxHeight(inout v2f i, float3 viewDir, float3 tangentViewDir, float3 vertexNormal, bool isFrontFace){
    #if defined(_PARALLAX_ON)
        if (isFrontFace){
            float mask = _HeightMask.Sample(sampler_DefaultSampler, i.uv1.zw)[_HeightMaskChannel];
            float strength = _HeightStrength * mask;

            // Catlike Coding POM
            #if defined(_WORKFLOW_PACKED_ON)
                parallaxOffset = GetParallaxOffset(i, _PackedMap, i.uv0.xy, tangentViewDir, strength, _HeightChannel);
            #else
                parallaxOffset = GetParallaxOffset(i, _HeightMap, i.uv0.xy, tangentViewDir, strength, 1);
            #endif

            // Amplify POM
            // #if defined(_WORKFLOW_PACKED_ON)
            // 	parallaxOffset = GetParallaxOffset(_PackedMap, i.uv0.xy, vertexNormal, viewDir, tangentViewDir, strength, 8, 16, _HeightChannel);
            // #else
            // 	parallaxOffset = GetParallaxOffset(_HeightMap, i.uv0.xy, vertexNormal, viewDir, tangentViewDir, strength, 8, 16, 1);
            // #endif

            i.uv0.xy += parallaxOffset;
            i.uv0.zw += parallaxOffset * (_DetailMainTex_ST.xy / _MainTex_ST.xy);
            i.uv1.xy += parallaxOffset * (_DetailMask_ST.xy / _MainTex_ST.xy);
            i.uv2.xy += parallaxOffset * (_RainScale / _MainTex_ST.xy);
            i.uv2.zw += parallaxOffset * (_RainMask_ST.xy / _MainTex_ST.xy);
            i.uv3.xy += parallaxOffset * (_EmissionMask_ST.xy / _MainTex_ST.xy);
            i.uv3.zw += parallaxOffset * (_RippleScale / _MainTex_ST.xy);
            i.uv4.xy += parallaxOffset * (_AreaLitOcclusion_ST.xy / _MainTex_ST.xy);
            i.uv4.zw += parallaxOffset * (_AlphaMask_ST.xy / _MainTex_ST.xy);
            if (_ApplyHeightOffset == 1){
                i.lightmapUV.xy += parallaxOffset * (unity_LightmapST.xy / _MainTex_ST.xy) * 0.5;
                i.lightmapUV.zw += parallaxOffset * (unity_DynamicLightmapST.xy / _MainTex_ST.xy) * 0.5;
            }
        }
    #endif
}

#endif