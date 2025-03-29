#ifndef STANDARD_BRDF_INCLUDED
#define STANDARD_BRDF_INCLUDED

float ComputeDistanceBaseRoughness(float distanceIntersectionToShadedPoint, float distanceIntersectionToProbeCenter, float perceptualRoughness){
    float newPerceptualRoughness = clamp(distanceIntersectionToShadedPoint / distanceIntersectionToProbeCenter * perceptualRoughness, 0, perceptualRoughness);
    return lerp(newPerceptualRoughness, perceptualRoughness, perceptualRoughness);
}

//SOURCE - https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl#L78
//This simplified version assume that we care about the result only when we are inside the box
//NOTE: Untouched from HDRP
float IntersectRayAABBSimple(float3 start, float3 dir, float3 boxMin, float3 boxMax){
    float3 invDir = rcp(dir);

    // Find the ray intersection with box plane
    float3 rbmin = (boxMin - start) * invDir;
    float3 rbmax = (boxMax - start) * invDir;

    float3 rbminmax = float3((dir.x > 0.0) ? rbmax.x : rbmin.x, (dir.y > 0.0) ? rbmax.y : rbmin.y, (dir.z > 0.0) ? rbmax.z : rbmin.z);

    return min(min(rbminmax.x, rbminmax.y), rbminmax.z);
}

//SOURCE - https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightEvaluation.hlsl  
//return projectionDistance, can be used in ComputeDistanceBaseRoughness formula
//return in R the unormalized corrected direction which is used to fetch cubemap but also its length represent the distance of the capture point to the intersection
//Length R can be reuse as a parameter of ComputeDistanceBaseRoughness for distIntersectionToProbeCenter
//NOTE: Modified to be much simpler, and to work with the Built-In Render Pipeline (BIRP)
float EvaluateLight_EnvIntersection(float3 worldSpacePosition, inout float3 R, float3 boxMin, float3 boxMax, float3 probePos){
    float projectionDistance = IntersectRayAABBSimple(worldSpacePosition, R, boxMin, boxMax);

    R = (worldSpacePosition + projectionDistance * R) - probePos;

    return projectionDistance;
}

float3 GetEnvironmentReflections(float3 reflDir, float3 worldPos, float roughness){
    float3 baseReflDir = reflDir;
    float roughness0 = roughness;
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
        [branch]
        if (unity_SpecCube0_ProbePosition.w > 0){
            float projectionDistance0 = EvaluateLight_EnvIntersection(worldPos, baseReflDir, unity_SpecCube0_BoxMin.xyz, unity_SpecCube0_BoxMax.xyz, unity_SpecCube0_ProbePosition.xyz);
            float distanceBasedRoughness0 = ComputeDistanceBaseRoughness(projectionDistance0, length(baseReflDir), roughness0);
            roughness0 = lerp(roughness, distanceBasedRoughness0, _ContactHardening);
        }
    #endif
    roughness0 *= 1.7-0.7*roughness0;
    float4 envSample0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, baseReflDir, roughness0 * UNITY_SPECCUBE_LOD_STEPS);
    float3 p0 = DecodeHDR(envSample0, unity_SpecCube0_HDR);
    [branch]
    if (unity_SpecCube0_BoxMin.w < 0.99999){
        float3 blendReflDir = reflDir;
        float roughness1 = roughness;
        #ifdef UNITY_SPECCUBE_BOX_PROJECTION
            [branch]
            if (unity_SpecCube1_ProbePosition.w > 0){
                float projectionDistance1 = EvaluateLight_EnvIntersection(worldPos, blendReflDir, unity_SpecCube1_BoxMin.xyz, unity_SpecCube1_BoxMax.xyz, unity_SpecCube1_ProbePosition.xyz);
                float distanceBasedRoughness1 = ComputeDistanceBaseRoughness(projectionDistance1, length(blendReflDir), roughness1);
                roughness1 = lerp(roughness, distanceBasedRoughness1, _ContactHardening);
            }
        #endif
        roughness1 *= 1.7-0.7*roughness1;
        float4 envSample1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, blendReflDir, roughness1 * UNITY_SPECCUBE_LOD_STEPS);
        float3 p1 = DecodeHDR(envSample1, unity_SpecCube1_HDR);
        p0 = lerp(p1, p0, unity_SpecCube0_BoxMin.w);
    }
    return p0;
}

void ApplyDiffuseTerm(inout LightingData ld, InputData id, float diffuseTerm){
    if (_Subsurface == 1){
        float3 wrappedDiffuse = saturate((diffuseTerm + _WrappingFactor) / (1.0f + _WrappingFactor)) * 2 / (2 * (1 + _WrappingFactor));
        ld.directCol *= lerp(diffuseTerm, wrappedDiffuse, ld.thickness);
    }
    else {
        ld.directCol *= diffuseTerm;
    }
}

float3 GetSpecularOcclusion(LightingData ld){
    float3 specularOcclusion = 1;
    #if defined(BASE_PASS)
        #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
            if (_SpecularOcclusionToggle == 1){
                float3 lightmap = Desaturate(ld.indirectCol);
                lightmap = GetContrast(lightmap, _SpecularOcclusionContrast);
                lightmap = lerp(lightmap, GetHDR(lightmap), _SpecularOcclusionHDR);
                lightmap *= _SpecularOcclusionBrightness;
                lightmap *= _SpecularOcclusionTint;
                specularOcclusion = saturate(lerp(1, lightmap, _SpecularOcclusionStrength));
            }
        #else
            if (_SpecularOcclusionToggle == 1){
                specularOcclusion = lerp(1, ld.atten * saturate((ld.VNdotL * ld.VNdotL) + ld.VNdotL), 0.9);
                specularOcclusion = saturate(lerp(1, specularOcclusion, _SpecularOcclusionToggle*_SpecularOcclusionStrength));
            }
        #endif
    #endif
    return specularOcclusion;
}

float3 GetFilamentEnergyConservation(float NdotV, float perceptualRoughness, float3 f0, out float2 dfg){
    float2 dfguv = float2(NdotV, perceptualRoughness);
    dfg = MOCHIE_SAMPLE_TEX2D(_DFG, dfguv).xy;
    return 1.0 + f0 * (1.0 / dfg.y - 1.0);
}

float ComputeSpecularAO(float NdotV, float ao, float roughness){
    return saturate(pow(NdotV + ao, exp2(-16.0 * roughness - 1.0)) - 1.0 + ao);
}

float3 gtaoMultiBounce(float visibility, float3 baseColor) {
    // Jimenez et al. 2016, "Practical Realtime Strategies for Accurate Indirect Occlusion"
    half3 a =  2.0404 * baseColor - 0.3324;
    half3 b = -4.7951 * baseColor + 0.6417;
    half3 c =  2.7552 * baseColor + 0.6903;

    return max((visibility), ((visibility * a + b) * visibility + c) * visibility);
}

void CalculateFilamentModel(InputData id, inout LightingData ld, float2 dfg, float NdotV, float roughSq, float3 f0, float horizon){
    ld.reflAdjust = lerp(dfg.xxx, dfg.yyy, f0) * horizon * horizon;
    float indirectSpecularOcclusion = saturate(length(ld.indirectCol) * (1.0 / _IndirectSpecularOcclusionStrength));
    indirectSpecularOcclusion *= lerp(1, lerp(1, ld.atten * ld.NdotL, ld.isRealtime), _RealtimeSpecularOcclusionStrength);
    float computedSpecularOcclusion = ComputeSpecularAO(NdotV, id.occlusion * indirectSpecularOcclusion, roughSq);
    // computedSpecularOcclusion *= energyConservation; - this explodes when metallic and roughness = 1. Dunno why.
    ld.specularOcclusion = gtaoMultiBounce(computedSpecularOcclusion, f0);
}

void CalculateBRDF(v2f i, InputData id, inout LightingData ld){

    float roughSq = max(id.roughness * id.roughness, 0.003);
    float3 halfVector = Unity_SafeNormalize(ld.lightDir + ld.viewDir);
    float NdotV = abs(dot(id.normal, ld.viewDir));
    float NdotH = saturate(dot(id.normal, halfVector));
    float LdotH = saturate(dot(ld.lightDir, halfVector));
    float3 reflDir = reflect(-ld.viewDir, id.normal);
    float horizon = min(1 + dot(reflDir, id.normal), 1);
    float3 diffuseTerm = 1;

    [branch]
    if (_ShadingModel == 1){
        float2 dfg;
        float reflectance = 0.5;
        float3 f0 = 0.16 * reflectance * reflectance * ld.omr + id.baseColor * id.metallic;
        diffuseTerm = GetFilamentEnergyConservation(NdotV, id.roughness, f0, dfg);
        CalculateFilamentModel(id, ld, dfg, NdotV, roughSq, f0, horizon);
        #if defined(_REFLECTIONS_ON)
            ld.indirectCol *= 1-ld.reflAdjust;
        #endif
    }
    else {
        diffuseTerm = DisneyDiffuse(NdotV, ld.NdotL, LdotH, id.roughness);
        float surfaceReduction = 1.0 / (roughSq*roughSq + 1.0);
        float grazingTerm = saturate((1-id.roughness) + (1-ld.omr));
        float3 fresnel = FresnelLerp(ld.specularTint, grazingTerm, lerp(1, NdotV, _FresnelStrength*_FresnelToggle));
        ld.reflAdjust = fresnel * surfaceReduction * horizon * horizon;
        ld.specularOcclusion = GetSpecularOcclusion(ld);
    }

    ApplyDiffuseTerm(ld, id, diffuseTerm);
    ld.lightCol = (ld.indirectCol * id.occlusion + ld.directCol) * ld.omr;

    #if defined(_REFLECTIONS_ON)
        float3 environmentReflections = GetEnvironmentReflections(reflDir, i.worldPos, id.roughness);
        ld.reflectionCol += environmentReflections * ld.reflAdjust * ld.specularOcclusion * id.occlusion * _ReflectionStrength;
    #endif
    #if LTCGI_ENABLED
        ld.reflectionCol += ld.ltcgiSpecularity * ld.reflAdjust * id.occlusion * saturate(lerp(1, ld.specularOcclusion, _LTCGISpecularOcclusion));
    #endif
    #if AREALIT_ENABLED
        ld.reflectionCol += ld.areaLitSpecularity * saturate(lerp(1, ld.specularOcclusion, _AreaLitSpecularOcclusion));
    #endif
    #if SSR_ENABLED
        float4 ssr = 0;
        [branch]
        if (((_VRSSR == 0 && IsNotVR()) || _VRSSR == 1) && _SSRStrength > 0){
            ssr = GetSSR(i.worldPos, ld.viewDir, reflDir, id.normal, 1-id.roughness, id.baseColor, id.metallic, i.grabUV);
            if (_SSREdgeFade == 0)
                ssr.a = ssr.a > 0 ? 1 : 0;
            ssr.rgb *= ld.reflAdjust * ld.specularOcclusion * id.occlusion;
            ld.reflectionCol = lerp(ld.reflectionCol, ssr.rgb, ssr.a * saturate(_SSRStrength));
        }
    #endif
    
    #if defined(_SPECULAR_HIGHLIGHTS_ON)
        #if defined(BASE_PASS)
        [branch]
        if (ld.isRealtime){
        #endif
            float3 fresnelTerm = FresnelTerm(ld.specularTint, LdotH) * lerp(1, diffuseTerm, _ShadingModel);
            float V = SmithJointGGXVisibilityTerm(ld.NdotL, NdotV, roughSq);
            float D = GGXTerm(NdotH, roughSq);
            float specularTerm = V * D * UNITY_PI;
            ld.specHighlightCol = max(0, (ld.directCol - ld.vLightCol)) * fresnelTerm * specularTerm * _SpecularHighlightStrength;
            #if SSR_ENABLED
                ld.specHighlightCol *= (1-ssr.a);
            #endif
        #if defined(BASE_PASS)
        }
        #endif
    #endif
    
    ld.lmSpec *= ld.reflAdjust * UNITY_PI * ld.specularOcclusion * ld.specularTint * _BakeryLMSpecStrength;
}

#endif