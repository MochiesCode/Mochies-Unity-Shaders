#ifndef PARTICLE_BRDF_INCLUDED
#define PARTICLE_BRDF_INCLUDED

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

float3 GetEnvironmentReflections(float3 reflDir, float3 worldPos, float roughness){
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

void CalculateBRDF(v2f i, InputData id, inout LightingData ld){

    float roughSq = max(id.roughness * id.roughness, 0.003);
    float NdotV = abs(dot(id.normal, ld.viewDir));
    specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, id.albedo, id.metallic);

    #if defined(_REFLECTIONS_ON)
        float3 reflDir = reflect(-ld.viewDir, id.normal);
        float surfaceReduction = 1.0 / (roughSq*roughSq + 1.0);
        float grazingTerm = saturate((1-id.roughness) + (1-ld.omr));
        float3 fresnel = FresnelLerp(specularTint, grazingTerm, NdotV);
        float horizon = min(1 + dot(reflDir, id.normal), 1);
        float3 reflAdjust = fresnel * surfaceReduction * horizon * horizon;
        float3 reflCol = GetEnvironmentReflections(reflDir, i.worldPos, id.roughness) * _ReflectionStrength;
        ld.reflectionCol += (reflCol * reflAdjust);
    #endif

    #if defined(_SPECULAR_HIGHLIGHTS_ON)
        if (ld.isRealtime){
            float3 halfVector = Unity_SafeNormalize(ld.lightDir + ld.viewDir);
            float LdotH = saturate(dot(ld.lightDir, halfVector));
            float NdotH = saturate(dot(id.normal, halfVector));
            float3 fresnelTerm = FresnelTerm(specularTint, LdotH);
            float V = SmithJointGGXVisibilityTerm(ld.NdotL, NdotV, roughSq);
            float D = GGXTerm(NdotH, roughSq);
            float specularTerm = V * D * UNITY_PI;
            ld.specHighlightCol = ld.directCol * fresnelTerm * specularTerm * _SpecularHighlightStrength;
        }
    #endif

    #if defined(UNITY_PASS_FORWARDBASE)
        [branch]
        if (_UdonLightVolumeEnabled == 1 && _LightVolumeSpecularity == 1 && _LightVolumeSpecularityStrength > 0){
            ld.lightVolumeSpecularity = LightVolumeSpecularDominant(id.albedo, 1-id.roughness, id.metallic, id.normal, i.worldPos, lightVolumeL0, lightVolumeL1r, lightVolumeL1g, lightVolumeL1b) * _LightVolumeSpecularityStrength;
        }
    #endif
}

#endif