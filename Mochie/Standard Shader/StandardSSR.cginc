#ifndef STANDARD_SSR_INCLUDED
#define STANDARD_SSR_INCLUDED

//-----------------------------------------------------------------------------------
// SCREEN SPACE REFLECTIONS
// 
// Original made by error.mdl, Toocanzs, and Xiexe.
// Reworked and updated by Mochie
//-----------------------------------------------------------------------------------

float3 GetBlurredGrabPass(const float2 texelSize, const float2 uvs, const float dim){
    float2 pixSize = 2/texelSize;
    float dimFloored = floor(dim);
    float center = floor(dim*0.5);
    float3 refTotal = float3(0,0,0);
    for (int i = 0; i < dimFloored; i++){
        for (int j = 0; j < dimFloored; j++){
            float4 refl = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_GrabTexture, float2(uvs.x + pixSize.x*(i-center), uvs.y + pixSize.y*(j-center)));
            refTotal += refl.rgb;
        }
    }
    return refTotal/(dimFloored*dimFloored);
}

float4 ReflectRay(float3 reflectedRay, float3 rayDir, float _LRad, float _SRad, float _Step, float noise, const int maxIterations){

    #if UNITY_SINGLE_PASS_STEREO
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
        [branch]
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

float4 GetSSR(const float3 wPos, const float3 viewDir, float3 rayDir, const half3 faceNormal, float smoothness, float3 albedo, float metallic, float4 screenPos){
    
    float FdotR = dot(faceNormal, rayDir.xyz);
    float roughness = 1-smoothness;
    
    [branch]
    if (IsInMirror() || FdotR < 0 || roughness > 0.3){
        return 0;
    }
    else {
        float2 noiseUV = (screenPos.xy * _GrabTexture_TexelSize.zw) / (_NoiseTexSSR_TexelSize.zw * screenPos.w);	
        float noise = _NoiseTexSSR.SampleLevel(sampler_NoiseTexSSR, noiseUV.xy, 0).r;
        
        float3 reflectedRay = wPos + (_SSRHeight*_SSRHeight/FdotR + noise*_SSRHeight)*rayDir;
        float4 finalPos = ReflectRay(reflectedRay, rayDir, _SSRHeight, 0.02, _SSRHeight, noise, 50);
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
            float xfade = smoothstep(0, _SSREdgeFade, uvs.x) * smoothstep(1, 1-_SSREdgeFade, uvs.x); //Fade x uvs out towards the edges
        #endif
        float yfade = smoothstep(0, _SSREdgeFade, uvs.y)*smoothstep(1, 1-_SSREdgeFade, uvs.y); //Same for y
        float smoothFade = smoothstep(0.3, 0.1, 1-smoothness);
        float reflectionAlpha = xfade * yfade * smoothFade;

        float4 reflection = 0;
        if (reflectionAlpha > 0){
            float blurFac = max(1,min(12, 12 * (-2)*(smoothness-1)));
            reflection.rgb = GetBlurredGrabPass(_GrabTexture_TexelSize.zw, uvs.xy, blurFac);
            reflection.rgb = lerp(reflection.rgb, reflection.rgb*albedo.rgb,smoothstep(0, 1.75, metallic));
            reflection.a = reflectionAlpha;
        }

        return max(0,reflection);
    }	
}

#endif