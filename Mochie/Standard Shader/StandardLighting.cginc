#ifndef STANDARD_LIGHTING_INCLUDED
#define STANDARD_LIGHTING_INCLUDED

void ApplyLighting(inout InputData id, LightingData ld){
    id.diffuse.rgb *= ld.lightCol;
    id.diffuse.rgb += ld.ltcgiDiffuse;
    id.diffuse.rgb += ld.areaLitDiffuse;
    id.diffuse.rgb += ld.reflectionCol;
    id.diffuse.rgb += ld.specHighlightCol;
    id.diffuse.rgb += ld.lmSpec;
    id.diffuse.rgb += ld.lightVolumeSpecularity;
    id.diffuse.rgb += ld.subsurfaceCol;
    id.diffuse.rgb += id.emission.rgb;
    id.diffuse.rgb = Filtering(id.diffuse.rgb, _HuePost, _SaturationPost, _BrightnessPost, _ContrastPost, _ACES, true);
}

float3x3 ConstructTBNMatrix(inout v2f i, bool isFrontFace){
    i.normal = normalize(i.normal);
    float crossSign = (i.tangent.w > 0.0 ? 1.0 : -1.0) * unity_WorldTransformParams.w;
    float3 binormal = cross(i.normal.xyz, i.tangent.xyz) * crossSign;
    if (!isFrontFace && _FlipBackfaceNormals == 1){
        i.normal = -i.normal;
    }
    return float3x3(i.tangent.xyz, binormal, i.normal.xyz);
}

void CalculateViewDirection(v2f i, float3x3 tangentToWorld, out float3 viewDir, out float3 tangentViewDir){
    viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
    tangentViewDir = normalize(mul(tangentToWorld, viewDir));
    tangentViewDir.xy /= (tangentViewDir.z + 0.42);
}

float SampleBakedOcclusion(float2 lightmapUV, float3 worldPos){
    #if defined (SHADOWS_SHADOWMASK)
        #if defined(LIGHTMAP_ON)
            #if defined(_BICUBIC_SAMPLING_ON)
                float4 rawOcclusionMask = SampleShadowMaskBicubic(lightmapUV.xy);
            #else
                float4 rawOcclusionMask = UNITY_SAMPLE_TEX2D(unity_ShadowMask, lightmapUV.xy);
            #endif
        #else
            float4 rawOcclusionMask = float4(1.0, 1.0, 1.0, 1.0);
            #if UNITY_LIGHT_PROBE_PROXY_VOLUME
                if (unity_ProbeVolumeParams.x == 1.0){
                    rawOcclusionMask = LPPV_SampleProbeOcclusion(worldPos);
                }
                else {
                    #if defined(_BICUBIC_SAMPLING_ON)
                        rawOcclusionMask = SampleShadowMaskBicubic(lightmapUV.xy);
                    #else
                        rawOcclusionMask = UNITY_SAMPLE_TEX2D(unity_ShadowMask, lightmapUV.xy);
                    #endif
                }
            #else
                #if defined(_BICUBIC_SAMPLING_ON)
                    rawOcclusionMask = SampleShadowMaskBicubic(lightmapUV.xy);
                #else
                    rawOcclusionMask = UNITY_SAMPLE_TEX2D(unity_ShadowMask, lightmapUV.xy);
                #endif
            #endif
        #endif
        return saturate(dot(rawOcclusionMask, unity_OcclusionMaskSelector));

    #else

        //In forward dynamic objects can only get baked occlusion from LPPV, light probe occlusion is done on the CPU by attenuating the light color.
        float atten = 1.0f;
        #if defined(UNITY_INSTANCING_ENABLED) && defined(UNITY_USE_SHCOEFFS_ARRAYS)
            // ...unless we are doing instancing, and the attenuation is packed into SHC array's .w component.
            atten = unity_SHC.w;
        #endif

        #if UNITY_LIGHT_PROBE_PROXY_VOLUME && !defined(LIGHTMAP_ON) && !UNITY_STANDARD_SIMPLE
            float4 rawOcclusionMask = atten.xxxx;
            if (unity_ProbeVolumeParams.x == 1.0)
                rawOcclusionMask = LPPV_SampleProbeOcclusion(worldPos);
            return saturate(dot(rawOcclusionMask, unity_OcclusionMaskSelector));
        #endif

        return atten;
    #endif
}

// Modified version of https://github.com/TwoTailsGames/Unity-Built-in-Shaders/blob/master/CGIncludes/UnityCG.cginc#L872
// Called in the shadowcaster when vertex manipulation is enabled because the offsets are calculated in world space, so we're going world --> clip instead of object --> clip
float4 UnityClipSpaceShadowCasterWorldPos(float4 worldPos, float3 normal){
    float4 wPos = worldPos;

    if (unity_LightShadowBias.z != 0.0){
        float3 wNormal = UnityObjectToWorldNormal(normal);
        float3 wLight = normalize(UnityWorldSpaceLightDir(wPos.xyz));

        // apply normal offset bias (inset position along the normal)
        // bias needs to be scaled by sine between normal and light direction
        // (http://the-witness.net/news/2013/09/shadow-mapping-summary-part-1/)
        //
        // unity_LightShadowBias.z contains user-specified normal offset amount
        // scaled by world space texel size.

        float shadowCos = dot(wNormal, wLight);
        float shadowSine = sqrt(1-shadowCos*shadowCos);
        float normalBias = unity_LightShadowBias.z * shadowSine;

        wPos.xyz -= wNormal * normalBias;
    }

    return mul(UNITY_MATRIX_VP, wPos);
}

float FadeShadows (float3 worldPos, float2 lmuv, float atten) {
    #if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
        float bakedAtten = SampleBakedOcclusion(lmuv, worldPos);
        float zDist = dot(_WorldSpaceCameraPos - worldPos, UNITY_MATRIX_V[2].xyz);
        float fadeDist = UnityComputeShadowFadeDistance(worldPos, zDist);
        atten = UnityMixRealtimeAndBakedShadows(atten, bakedAtten, UnityComputeShadowFade(fadeDist));
    #endif
    return atten;
}

float3 GetSubsurfaceLight(v2f i, InputData id, inout LightingData ld, float3 lightCol, float3 lightDir, float3 viewDir, float3 indirectLight, float atten){
    float3 subsurfaceLight = 0;
    #if defined(BASE_PASS) && !defined(STANDARD_MOBILE) && !defined(STANDARD_LITE)
        [branch]
        if (_Subsurface == 1){
            ld.thickness = pow(1-SampleTexture(_ThicknessMap, i.uv0.xy), _ThicknessMapPower);
            float3 subsurfaceColor = _ScatterCol * lerp(1, id.baseColor, _ScatterBaseColorTint);
            float3 vLTLight = lightDir + id.normal * _ScatterDist;
            float3 fLTDot = pow(saturate(dot(viewDir, -vLTLight)), _ScatterPow) * _ScatterIntensity * 1.0/UNITY_PI; 
            subsurfaceLight = lerp(1, atten, float(any(_WorldSpaceLightPos0.xyz))) 
                        * (fLTDot + _ScatterAmbient) * ld.thickness
                        * (lightCol + indirectLight) * subsurfaceColor; 
        }
    #endif
    return subsurfaceLight;   
}

float3 DecodeDirectionalLightmap(float3 color, float4 dirTex, float3 normalWorld, float strength){
    float halfLambert = dot(normalWorld, dirTex.xyz - 0.5) + 0.5;
    halfLambert /= max(1e-4h, dirTex.w);
    return color * lerp(1, halfLambert, strength);
}

float NonlinearSH(float L0, float3 L1, float3 normal) {
    float R0 = L0;
    float3 R1 = 0.5f * L1;
    float lenR1 = length(R1);
    float q = dot(normalize(R1), normal) * 0.5 + 0.5;
    q = max(0, q);
    float p = 1.0f + 2.0f * lenR1 / R0;
    float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);
    return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
}

float3 ShadeSHNL(float3 normal) {
    float3 indirect;
    indirect.r = NonlinearSH(unity_SHAr.w, unity_SHAr.xyz, normal);
    indirect.g = NonlinearSH(unity_SHAg.w, unity_SHAg.xyz, normal);
    indirect.b = NonlinearSH(unity_SHAb.w, unity_SHAb.xyz, normal);
    return indirect;
}

float3 GetSH(v2f i, InputData id){
    [branch]
    if (_UdonLightVolumeEnabled == 1){
        LightVolumeSH(i.worldPos+i.normal*_LightVolumeBias, lightVolumeL0, lightVolumeL1r, lightVolumeL1g, lightVolumeL1b);
        return LightVolumeEvaluate(id.normal, lightVolumeL0, lightVolumeL1r, lightVolumeL1g, lightVolumeL1b);
    }
    else {
        #if defined(BAKERY_SHNONLINEAR)
            return max(0, ShadeSHNL(id.normal));
        #else
            return max(0, ShadeSH9(float4(id.normal, 1)));
        #endif
    }
}

float3 GetRealtimeIndirectLighting(v2f i, InputData id){
    float3 indirectCol = 0;
    #if UNITY_LIGHT_PROBE_PROXY_VOLUME
        if (unity_ProbeVolumeParams.x == 1){
            indirectCol = max(0, SHEvalLinearL0L1_SampleProbeVolume(float4(id.normal, 1), i.worldPos));
        }
        else {
            indirectCol = GetSH(i, id);
        }
    #else
        indirectCol = GetSH(i, id);
    #endif
    return indirectCol;
}

void GetIndirectLighting(v2f i, InputData id, float3 viewDir, inout float3 indirectCol, inout float3 lmSpec, float3 tangentViewDir, float atten) {
    indirectCol = 0;
    lmSpec = 0;

    #if defined(BASE_PASS)
        #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
            #if defined(_BICUBIC_SAMPLING_ON)
                indirectCol = DecodeLightmap(SampleLightmapBicubic(i.lightmapUV));
                #if defined(DIRLIGHTMAP_COMBINED) || defined(BAKERY_MONOSH)
                    float4 lightmapDir = SampleLightmapDirBicubic(i.lightmapUV);
                #endif
            #else
                indirectCol = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV));
                #if defined(DIRLIGHTMAP_COMBINED) || defined(BAKERY_MONOSH)
                    float4 lightmapDir = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, i.lightmapUV);
                #endif
            #endif

            float bakeryLMSpecRough = id.roughness * id.roughness; // max(0.001, id.roughness); - looks terrible lol
            #if defined(BAKERY_MONOSH)
                BakeryMonoSH(indirectCol, lmSpec, lightmapDir, id.normal, viewDir, bakeryLMSpecRough);
            #elif defined(BAKERY_RNM)
                BakeryRNMLightmapAndSpecular(indirectCol, i.lightmapUV, lmSpec, id.tsNormal, tangentViewDir, viewDir, bakeryLMSpecRough);
            #elif defined(BAKERY_SH)
                BakerySHLightmapAndSpecular(indirectCol, i.lightmapUV, lmSpec, id.normal, viewDir, bakeryLMSpecRough);
            #else
                #if defined(DIRLIGHTMAP_COMBINED)
                    indirectCol = DecodeDirectionalLightmap(indirectCol, lightmapDir, id.normal, 1.5);
                #endif
            #endif

            #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN)
                indirectCol = SubtractMainLightWithRealtimeAttenuationFromLightmap(indirectCol, atten, 0, id.normal);
            #endif

            #if defined(DYNAMICLIGHTMAP_ON)
                if (_IgnoreRealtimeGI != 1){
                    #if defined(_BICUBIC_SAMPLING_ON)
                        float4 realtimeColorTex = SampleDynamicLightmapBicubic(i.lightmapUV.zw);
                    #else
                        float4 realtimeColorTex = UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, i.lightmapUV.zw);
                    #endif
                    float3 realtimeColor = DecodeRealtimeLightmap(realtimeColorTex);
                    #if defined(DIRLIGHTMAP_COMBINED)
                        #if defined(_BICUBIC_SAMPLING_ON)
                            float4 realtimeDirTex = SampleDynamicLightmapDirBicubic(i.lightmapUV.zw);
                        #else
                            float4 realtimeDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicDirectionality, unity_DynamicLightmap, i.lightmapUV.zw);
                        #endif
                        indirectCol += DecodeDirectionalLightmap(realtimeColor, realtimeDirTex, id.normal, 1.5);
                    #else
                        indirectCol += realtimeColor;
                    #endif
                }
            #endif
            
            [branch]
            if (_UdonLightVolumeEnabled == 1 && _AdditiveLightVolumesToggle == 1){
                LightVolumeAdditiveSH(i.worldPos, lightVolumeL0, lightVolumeL1r, lightVolumeL1g, lightVolumeL1b);
                indirectCol += LightVolumeEvaluate(id.normal, lightVolumeL0, lightVolumeL1r, lightVolumeL1g, lightVolumeL1b);
            }

        #else
            indirectCol = GetRealtimeIndirectLighting(i, id);
        #endif
    #endif
}

float3 Shade4PointLightsNoPopIn(
    float4 lightPosX, float4 lightPosY, float4 lightPosZ,
    float3 lightColor0, float3 lightColor1, float3 lightColor2, float3 lightColor3,
    float4 lightAttenSq,
    float3 pos, float3 normal)
{
    float4 toLightX = lightPosX - pos.x;
    float4 toLightY = lightPosY - pos.y;
    float4 toLightZ = lightPosZ - pos.z;

    float4 lengthSq = 0;
    lengthSq += toLightX * toLightX;
    lengthSq += toLightY * toLightY;
    lengthSq += toLightZ * toLightZ;

    lengthSq = max(lengthSq, 0.000001);

    float4 ndotl = 0;
    ndotl += toLightX * normal.x;
    ndotl += toLightY * normal.y;
    ndotl += toLightZ * normal.z;

    float4 corr = rsqrt(lengthSq);
    ndotl = max (float4(0,0,0,0), ndotl * corr);

    float4 atten = 1.0 / (1.0 + lengthSq * lightAttenSq);
    atten = linearstep(float4(0.05,0.05,0.05,0.05), float4(1,1,1,1), atten);
    float4 diff = ndotl * atten;

    float3 col = 0;
    col += lightColor0 * diff.x;
    col += lightColor1 * diff.y;
    col += lightColor2 * diff.z;
    col += lightColor3 * diff.w;
    return col;
}

float3 GetVertexLightColor(v2f i, InputData id){
    float3 vLightCol = 0;
    #if defined(BASE_PASS)
        if (i.vertexLightOn){
            vLightCol = Shade4PointLightsNoPopIn(unity_4LightPosX0, unity_4LightPosY0, 
                unity_4LightPosZ0, unity_LightColor[0].rgb, 
                unity_LightColor[1].rgb, unity_LightColor[2].rgb, 
                unity_LightColor[3].rgb, unity_4LightAtten0, 
                i.worldPos, id.normal
            );
        }
    #endif
    return vLightCol;
}

void InitializeLightingData(v2f i, inout InputData id, inout LightingData ld, float3 viewDir, float3 tangentViewDir, float atten){

    float omr = unity_ColorSpaceDielectricSpec.a - id.metallic * unity_ColorSpaceDielectricSpec.a;
    float3 lightDir = Unity_SafeNormalize(UnityWorldSpaceLightDir(i.worldPos));
    float NdotL = saturate(dot(id.normal, lightDir));
    float VNdotL = saturate(dot(id.vNormal, lightDir));
    ld.specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, id.baseColor, id.metallic);
    ld.isRealtime = any(_WorldSpaceLightPos0.xyz);
    if (ld.isRealtime)
        VNdotL = 1;

    float3 vLightCol = GetVertexLightColor(i, id);
    float3 directCol = (_LightColor0 * atten * NdotL) + vLightCol;
    float3 indirectCol = 0;
    
    GetIndirectLighting(i, id, viewDir, indirectCol, ld.lmSpec, tangentViewDir, atten);
    ld.subsurfaceCol = GetSubsurfaceLight(i, id, ld, _LightColor0, lightDir, viewDir, indirectCol, atten);
    
    ld.directCol = directCol;
    ld.indirectCol = indirectCol;
    ld.vLightCol = vLightCol;
    ld.lightDir = lightDir;
    ld.viewDir = viewDir;
    ld.NdotL = NdotL;
    ld.VNdotL = VNdotL;
    ld.atten = atten;
    ld.omr = omr;
    ld.specularOcclusion = 1;
    
    id.diffuse.rgb = PreMultiplyAlpha(id.diffuse.rgb, id.alpha, ld.omr, id.alpha);
    
}

#endif