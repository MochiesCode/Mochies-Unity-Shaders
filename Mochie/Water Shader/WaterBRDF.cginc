#ifndef WATER_BRDF_INCLUDED
#define WATER_BRDF_INCLUDED

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

float3 GetManualReflections(InputData id, float3 reflDir){
    float roughness = id.roughness * (1.7 - 0.7 * id.roughness);
    reflDir = Rotate3D(reflDir, _ReflCubeRotation);
    float4 envSample0 = texCUBElod(_ReflCube, float4(reflDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
    return DecodeHDR(envSample0, _ReflCube_HDR);
}

float3 GetWorldReflections(v2f i, InputData id, float3 reflDir){
    float3 baseReflDir = reflDir;
    float roughness = id.roughness * (1.7 - 0.7 * id.roughness);
    reflDir = BoxProjection(reflDir, i.worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
    float4 envSample0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
    float3 p0 = DecodeHDR(envSample0, unity_SpecCube0_HDR);
    float interpolator = unity_SpecCube0_BoxMin.w;
    UNITY_BRANCH
    if (interpolator < 0.99999){
        float3 refDirBlend = BoxProjection(baseReflDir, i.worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
        float4 envSample1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, refDirBlend, roughness * UNITY_SPECCUBE_LOD_STEPS);
        float3 p1 = DecodeHDR(envSample1, unity_SpecCube1_HDR);
        p0 = lerp(p1, p0, interpolator);
    }
    return p0;
}

#if !defined(META_PASS)
float3 GetMirrorReflections(v2f i, InputData id){
    float perceptualRoughness = id.roughness * (1.7 - 0.7 * id.roughness);
    float mip = perceptualRoughnessToMipmapLevel(perceptualRoughness);
    float2 normalSwizzle[3] = {id.normal.xy, id.normal.xz, id.normal.yz}; 
    float4 reflUV = i.reflUV;
    reflUV.xy -= normalSwizzle[_MirrorNormalOffsetSwizzle];
    float2 uv = reflUV.xy / (reflUV.w + 0.00000001);
    float4 uvMip = float4(uv, 0, mip * 6);
    float3 refl = unity_StereoEyeIndex == 0 ? tex2Dlod(_ReflectionTex0, uvMip) : tex2Dlod(_ReflectionTex1, uvMip);
    return refl;
}
#endif

void CalculateBRDF(v2f i, InputData id, inout LightingData ld){

    #if PBR_ENABLED
        float3 reflDir = reflect(-ld.viewDir, id.normal);
        float surfaceReduction = 1.0 / (id.roughBRDF*id.roughBRDF + 1.0);
        float grazingTerm = saturate((1-id.roughness) + (1-ld.omr));
        float3 fresnel = FresnelLerp(ld.specularTint, grazingTerm, ld.NdotV);
        float horizon = min(1 + dot(reflDir, id.normal), 1);
        ld.reflAdjust = fresnel * surfaceReduction * horizon * horizon;

        #if SPECULAR_ENABLED
            if (id.isFrontFace){
                float roughInterp = smoothstep(0.001, 0.003, id.roughness * id.roughness);
                float NdotH = saturate(dot(id.normal, ld.halfVector));
                float LdotH = saturate(dot(ld.lightDir, ld.halfVector));
                float specAtten = ld.atten;
                float3 fresnelTerm = FresnelTerm(ld.specularTint, LdotH);
                float V = SmithJointGGXVisibilityTerm(ld.NdotL, ld.NdotV, id.roughBRDF);
                float D = GGXTerm(NdotH, id.roughBRDF);
                float specularTerm = V * D * UNITY_PI;
                float3 specLightCol = _Specular == 1 ? _LightColor0 : 1;
                float3 specCol = specLightCol * fresnelTerm * specularTerm;
                specCol = lerp(smootherstep(0, 0.9, specCol), specCol, roughInterp) * _SpecStrength * _SpecTint;
                #if defined(UNITY_PASS_FORWARDBASE)
                    specCol *= _ShadowStrength > 0 ? specAtten : 1;
                #else
                    specCol *= specAtten;
                #endif
                ld.specHighlightCol = specCol;
            }
        #endif
        
        #if REFLECTIONS_ENABLED
            if ((_BackfaceReflections == 0 && id.isFrontFace) || (_BackfaceReflections == 1)){			
                float3 reflCol = 0;
                #if defined(_REFLECTIONS_MANUAL_ON)
                    reflCol = GetManualReflections(id, reflDir);
                #elif defined(_REFLECTIONS_MIRROR_ON)
                    reflCol = GetMirrorReflections(i, id);
                #else
                    reflCol = GetWorldReflections(i, id, reflDir);
                #endif
                reflCol *= _ReflStrength;
                #if DEPTH_EFFECTS_ENABLED
                    #if SSR_ENABLED
                        [branch]
                        if (((_VRSSR == 0 && IsNotVR()) || _VRSSR == 1) && _SSRStrength > 0){
                            float4 ssrCol = GetSSR(i, id, ld, reflDir);
                            ssrCol.rgb *= _SSRStrength;
                            #if FOAM_ENABLED
                                float foamLerp = 1-(id.foam + id.crestFoam);
                                foamLerp = smoothstep(0.7, 1, foamLerp);
                                ssrCol.a *= foamLerp;
                            #endif
                            if (_EdgeFadeSSR == 0)
                                ssrCol.a = ssrCol.a > 0 ? 1 : 0;
                            reflCol = lerp(reflCol, ssrCol.rgb, ssrCol.a * saturate(_SSRStrength));
                            #if SPECULAR_ENABLED
                                ld.specHighlightCol *= 1-ssrCol.a;
                            #endif
                        }
                    #endif
                #endif
                ld.reflectionCol = reflCol * fresnel * surfaceReduction * _ReflTint;
            }
        #elif !REFLECTIONS_ENABLED && (LTCGI_ENABLED || defined(BAKERY_LMSPEC))
            float3 reflDir = reflect(-ld.viewDir, id.normal);
            float surfaceReduction = 1.0 / (id.roughBRDF*id.roughBRDF + 1.0);
            float grazingTerm = saturate((1-id.roughness) + (1-ld.omr));
            float3 fresnel = FresnelLerp(ld.specularTint, grazingTerm, ld.NdotV);
            float horizon = min(1 + dot(reflDir, id.normal), 1);
            ld.reflAdjust = fresnel * surfaceReduction * horizon * horizon;
        #endif

        #if defined(UNITY_PASS_FORWARDBASE)
            ld.lmSpec *= ld.reflAdjust * ld.specularTint * _BakeryLMSpecStrength * lerp(20, 0.5, id.metallic);
        #endif

    #elif !REFLECTIONS_ENABLED && (LTCGI_ENABLED || defined(BAKERY_LMSPEC))
        float3 reflDir = reflect(-ld.viewDir, id.normal);
        float surfaceReduction = 1.0 / (id.roughBRDF*id.roughBRDF + 1.0);
        float grazingTerm = saturate((1-id.roughness) + (1-ld.omr));
        float3 fresnel = FresnelLerp(ld.specularTint, grazingTerm, ld.NdotV);
        float horizon = min(1 + dot(reflDir, id.normal), 1);
        ld.reflAdjust = fresnel * surfaceReduction * horizon * horizon;
        #if defined(UNITY_PASS_FORWARDBASE)
            ld.lmSpec = 0;
        #endif
    #else
        #if defined(UNITY_PASS_FORWARDBASE)
            ld.lmSpec = 0;
        #endif
    #endif
}

#endif // WATER_BRDF_INCLUDED
