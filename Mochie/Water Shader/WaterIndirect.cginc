#ifndef WATER_INDIRECT_INCLUDED
#define WATER_INDIRECT_INCLUDED

#ifndef TEXTURE2D_ARGS
#define TEXTURE2D_ARGS(textureName, samplerName) Texture2D textureName, SamplerState samplerName
#define TEXTURE2D_PARAM(textureName, samplerName) textureName, samplerName
#define SAMPLE_TEXTURE2D(textureName, samplerName, coord2) textureName.Sample(samplerName, coord2)
#endif

Texture2D _RNM0, _RNM1, _RNM2;
SamplerState custom_bilinear_clamp_sampler;
float4 _RNM0_TexelSize;

#define GRAYSCALE float3(0.2125, 0.7154, 0.0721)

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

float FadeShadows (float3 worldPos, float2 lmuv, float atten) {
    #if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
        float bakedAtten = SampleBakedOcclusion(lmuv, worldPos);
        float zDist = dot(_WorldSpaceCameraPos - worldPos, UNITY_MATRIX_V[2].xyz);
        float fadeDist = UnityComputeShadowFadeDistance(worldPos, zDist);
        atten = UnityMixRealtimeAndBakedShadows(atten, bakedAtten, UnityComputeShadowFade(fadeDist));
    #endif
    return atten;
}

float4 SampleTexture2DBicubicFilter(TEXTURE2D_ARGS(tex, smp), float2 coord, float4 texSize){
    coord = coord * texSize.xy - 0.5;
    float fx = frac(coord.x);
    float fy = frac(coord.y);
    coord.x -= fx;
    coord.y -= fy;

    float4 xcubic = cubic(fx);
    float4 ycubic = cubic(fy);

    float4 c = float4(coord.x - 0.5, coord.x + 1.5, coord.y - 0.5, coord.y + 1.5);
    float4 s = float4(xcubic.x + xcubic.y, xcubic.z + xcubic.w, ycubic.x + ycubic.y, ycubic.z + ycubic.w);
    float4 offset = c + float4(xcubic.y, xcubic.w, ycubic.y, ycubic.w) / s;

    float4 sample0 = SAMPLE_TEXTURE2D(tex, smp, float2(offset.x, offset.z) * texSize.zw);
    float4 sample1 = SAMPLE_TEXTURE2D(tex, smp, float2(offset.y, offset.z) * texSize.zw);
    float4 sample2 = SAMPLE_TEXTURE2D(tex, smp, float2(offset.x, offset.w) * texSize.zw);
    float4 sample3 = SAMPLE_TEXTURE2D(tex, smp, float2(offset.y, offset.w) * texSize.zw);

    float sx = s.x / (s.x + s.y);
    float sy = s.z / (s.z + s.w);

    return lerp(
        lerp(sample3, sample2, sx),
        lerp(sample1, sample0, sx), sy);
}

float4 SampleShadowMaskBicubic(float2 uv){
    #ifdef SHADER_API_D3D11
        float width, height;
        unity_ShadowMask.GetDimensions(width, height);

        float4 unity_ShadowMask_TexelSize = float4(width, height, 1.0/width, 1.0/height);

        return SampleTexture2DBicubicFilter(TEXTURE2D_PARAM(unity_ShadowMask, samplerunity_ShadowMask),
            uv, unity_ShadowMask_TexelSize);
    #else
        return SAMPLE_TEXTURE2D(unity_ShadowMask, samplerunity_ShadowMask, uv);
    #endif
}

float4 SampleLightmapBicubic(float2 uv){
    #ifdef SHADER_API_D3D11
        float width, height;
        unity_Lightmap.GetDimensions(width, height);

        float4 unity_Lightmap_TexelSize = float4(width, height, 1.0/width, 1.0/height);

        return SampleTexture2DBicubicFilter(TEXTURE2D_PARAM(unity_Lightmap, samplerunity_Lightmap),
            uv, unity_Lightmap_TexelSize);
    #else
        return SAMPLE_TEXTURE2D(unity_Lightmap, samplerunity_Lightmap, uv);
    #endif
}

float4 SampleLightmapDirBicubic(float2 uv){
    #ifdef SHADER_API_D3D11
        float width, height;
        unity_LightmapInd.GetDimensions(width, height);

        float4 unity_LightmapInd_TexelSize = float4(width, height, 1.0/width, 1.0/height);

        return SampleTexture2DBicubicFilter(TEXTURE2D_PARAM(unity_LightmapInd, samplerunity_Lightmap),
            uv, unity_LightmapInd_TexelSize);
    #else
        return SAMPLE_TEXTURE2D(unity_LightmapInd, samplerunity_Lightmap, uv);
    #endif
}

float4 SampleDynamicLightmapBicubic(float2 uv){
    #ifdef SHADER_API_D3D11
        float width, height;
        unity_DynamicLightmap.GetDimensions(width, height);

        float4 unity_DynamicLightmap_TexelSize = float4(width, height, 1.0/width, 1.0/height);

        return SampleTexture2DBicubicFilter(TEXTURE2D_PARAM(unity_DynamicLightmap, samplerunity_DynamicLightmap),
            uv, unity_DynamicLightmap_TexelSize);
    #else
        return SAMPLE_TEXTURE2D(unity_DynamicLightmap, samplerunity_DynamicLightmap, uv);
    #endif
}

float4 SampleDynamicLightmapDirBicubic(float2 uv){
    #ifdef SHADER_API_D3D11
        float width, height;
        unity_DynamicDirectionality.GetDimensions(width, height);

        float4 unity_DynamicDirectionality_TexelSize = float4(width, height, 1.0/width, 1.0/height);

        return SampleTexture2DBicubicFilter(TEXTURE2D_PARAM(unity_DynamicDirectionality, samplerunity_DynamicLightmap),
            uv, unity_DynamicDirectionality_TexelSize);
    #else
        return SAMPLE_TEXTURE2D(unity_DynamicDirectionality, samplerunity_DynamicLightmap, uv);
    #endif
}

float shEvaluateDiffuseL1Geomerics(float L0, float3 L1, float3 n)
{
    // average energy
    float R0 = L0;
    
    // avg direction of incoming light
    float3 R1 = 0.5f * L1;
    
    // directional brightness
    float lenR1 = length(R1);
    
    // linear angle between normal and direction 0-1
    //float q = 0.5f * (1.0f + dot(R1 / lenR1, n));
    //float q = dot(R1 / lenR1, n) * 0.5 + 0.5;
    float q = dot(normalize(R1), n) * 0.5 + 0.5;
    q = saturate(q); // Thanks to ScruffyRuffles for the bug identity.
    
    // power for q
    // lerps from 1 (linear) to 3 (cubic) based on directionality
    float p = 1.0f + 2.0f * lenR1 / R0;
    
    // dynamic range constant
    // should vary between 4 (highly directional) and 0 (ambient)
    float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);
    
    return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
}

void BakeryRNMLightmapAndSpecular(inout half3 lightMap, float2 lightmapUV, inout half3 directSpecular, float3 normalTS, float3 viewDirTS, float3 viewDir, half roughness)
{
    // normalTS.g *= -1; // might not be needed anymore
    float3 rnm0 = DecodeLightmap(_RNM0.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0));
    float3 rnm1 = DecodeLightmap(_RNM1.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0));
    float3 rnm2 = DecodeLightmap(_RNM2.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0));

    const float3 rnmBasis0 = float3(0.816496580927726f, 0.0f, 0.5773502691896258f);
    const float3 rnmBasis1 = float3(-0.4082482904638631f, 0.7071067811865475f, 0.5773502691896258f);
    const float3 rnmBasis2 = float3(-0.4082482904638631f, -0.7071067811865475f, 0.5773502691896258f);

    lightMap =    saturate(dot(rnmBasis0, normalTS)) * rnm0
                + saturate(dot(rnmBasis1, normalTS)) * rnm1
                + saturate(dot(rnmBasis2, normalTS)) * rnm2;

    // [branch]
    // if (_BAKERY_SHNONLINEAR == 1){
    //     float3 viewDirT = -normalize(viewDirTS);
    //     float3 dominantDirT = rnmBasis0 * dot(rnm0, GRAYSCALE) +
    //                             rnmBasis1 * dot(rnm1, GRAYSCALE) +
    //                             rnmBasis2 * dot(rnm2, GRAYSCALE);

    //     float3 dominantDirTN = normalize(dominantDirT);
    //     half3 specColor = saturate(dot(rnmBasis0, dominantDirTN)) * rnm0 +
    //                         saturate(dot(rnmBasis1, dominantDirTN)) * rnm1 +
    //                         saturate(dot(rnmBasis2, dominantDirTN)) * rnm2;

    //     half3 halfDir = Unity_SafeNormalize(dominantDirTN - viewDirT);
    //     half NoH = saturate(dot(normalTS, halfDir));
    //     half spec = GGXTerm(NoH, roughness);
    //     directSpecular += spec * specColor;
    // }
}

void BakerySHLightmapAndSpecular(inout half3 lightMap, float2 lightmapUV, inout half3 directSpecular, float3 normalWS, float3 viewDir, half roughness)
{
    half3 L0 = lightMap;
    float3 nL1x = _RNM0.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0) * 2.0 - 1.0;
    float3 nL1y = _RNM1.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0) * 2.0 - 1.0;
    float3 nL1z = _RNM2.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0) * 2.0 - 1.0;
    float3 L1x = nL1x * L0 * 2.0;
    float3 L1y = nL1y * L0 * 2.0;
    float3 L1z = nL1z * L0 * 2.0;

    // [branch]
    // if (_BAKERY_SHNONLINEAR == 1){
    //     float lumaL0 = dot(L0, float(1));
    //     float lumaL1x = dot(L1x, float(1));
    //     float lumaL1y = dot(L1y, float(1));
    //     float lumaL1z = dot(L1z, float(1));
    //     float lumaSH = shEvaluateDiffuseL1Geomerics(lumaL0, float3(lumaL1x, lumaL1y, lumaL1z), normalWS);

    //     lightMap = L0 + normalWS.x * L1x + normalWS.y * L1y + normalWS.z * L1z;
    //     float regularLumaSH = dot(lightMap, 1.0);
    //     lightMap *= lerp(1.0, lumaSH / regularLumaSH, saturate(regularLumaSH * 16.0));
    // }
    // else {
        lightMap = L0 + normalWS.x * L1x + normalWS.y * L1y + normalWS.z * L1z;
    // }

    #ifdef BAKERY_LMSPEC
        float3 dominantDir = float3(dot(nL1x, GRAYSCALE), dot(nL1y, GRAYSCALE), dot(nL1z, GRAYSCALE));
        float3 halfDir = Unity_SafeNormalize(normalize(dominantDir) + viewDir);
        half NoH = saturate(dot(normalWS, halfDir));
        half spec = GGXTerm(NoH, roughness);
        half3 sh = L0 + dominantDir.x * L1x + dominantDir.y * L1y + dominantDir.z * L1z;
        dominantDir = normalize(dominantDir);

        directSpecular += max(spec * sh, 0.0);
    #endif
}

void BakeryMonoSH(inout half3 diffuseColor, inout half3 specularColor, float3 dominantDir, float3 normalWorld, float3 viewDir, half roughness)
{
    half3 L0 = diffuseColor;

    float3 nL1 = dominantDir * 2 - 1;
    float3 L1x = nL1.x * L0 * 2;
    float3 L1y = nL1.y * L0 * 2;
    float3 L1z = nL1.z * L0 * 2;
    half3 sh;

    // [branch]
    // if (_BAKERY_SHNONLINEAR == 1){
    //     float lumaL0 = dot(L0, 1);
    //     float lumaL1x = dot(L1x, 1);
    //     float lumaL1y = dot(L1y, 1);
    //     float lumaL1z = dot(L1z, 1);
    //     float lumaSH = shEvaluateDiffuseL1Geomerics(lumaL0, float3(lumaL1x, lumaL1y, lumaL1z), normalWorld);

    //     sh = L0 + normalWorld.x * L1x + normalWorld.y * L1y + normalWorld.z * L1z;
    //     float regularLumaSH = dot(sh, 1);
    //     //sh *= regularLumaSH < 0.001 ? 1 : (lumaSH / regularLumaSH);
    //     sh *= lerp(1, lumaSH / regularLumaSH, saturate(regularLumaSH*16));

    //     //sh.r = shEvaluateDiffuseL1Geomerics(L0.r, float3(L1x.r, L1y.r, L1z.r), normalWorld);
    //     //sh.g = shEvaluateDiffuseL1Geomerics(L0.g, float3(L1x.g, L1y.g, L1z.g), normalWorld);
    //     //sh.b = shEvaluateDiffuseL1Geomerics(L0.b, float3(L1x.b, L1y.b, L1z.b), normalWorld);
    // }
    // else {
        sh = L0 + normalWorld.x * L1x + normalWorld.y * L1y + normalWorld.z * L1z;
    // }

    diffuseColor = max(sh, 0.0);

    specularColor = 0;
    #ifdef BAKERY_LMSPEC
        dominantDir = nL1;
        float focus = saturate(length(dominantDir));
        half3 halfDir = Unity_SafeNormalize(normalize(dominantDir) + viewDir);
        half nh = saturate(dot(normalWorld, halfDir));
        half spec = GGXTerm(nh, roughness);

        sh = L0 + dominantDir.x * L1x + dominantDir.y * L1y + dominantDir.z * L1z;
        
        specularColor = max(spec * sh, 0.0);
    #endif
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

void GetIndirectLighting(out float3 indirectCol, out float3 lmSpec, float4 lightmapUV, float3 normal, float3 normalts, float3 worldPos, float3 viewDir, float3 tangentViewDir, float roughness, float atten) {
    indirectCol = 1;
    lmSpec = 0;

    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        #if defined(_BICUBIC_SAMPLING_ON)
            indirectCol = DecodeLightmap(SampleLightmapBicubic(lightmapUV));
            #if defined(DIRLIGHTMAP_COMBINED) || defined(BAKERY_MONOSH)
                float4 lightmapDir = SampleLightmapDirBicubic(lightmapUV);
            #endif
        #else
            indirectCol = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV));
            #if defined(DIRLIGHTMAP_COMBINED) || defined(BAKERY_MONOSH)
                float4 lightmapDir = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, lightmapUV);
            #endif
        #endif

        roughness = max(0.004, roughness * roughness);
        #if defined(BAKERY_MONOSH)
            BakeryMonoSH(indirectCol, lmSpec, lightmapDir, normal, viewDir, roughness);
        #elif defined(BAKERY_RNM)
            BakeryRNMLightmapAndSpecular(indirectCol, lightmapUV, lmSpec, normalts, tangentViewDir, viewDir, roughness);
        #elif defined(BAKERY_SH)
            BakerySHLightmapAndSpecular(indirectCol, lightmapUV, lmSpec, normal, viewDir, roughness);
        #else
            #if defined(DIRLIGHTMAP_COMBINED)
                indirectCol = DecodeDirectionalLightmap(indirectCol, lightmapDir, normal);
            #endif
        #endif

        #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN)
            indirectCol = SubtractMainLightWithRealtimeAttenuationFromLightmap(indirectCol, atten, 0, normal);
        #endif

        #if defined(DYNAMICLIGHTMAP_ON)
            if (_IgnoreRealtimeGI != 1){
                #if defined(_BICUBIC_SAMPLING_ON)
                    float4 realtimeColorTex = SampleDynamicLightmapBicubic(lightmapUV.zw);
                #else
                    float4 realtimeColorTex = UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, lightmapUV.zw);
                #endif
                float3 realtimeColor = DecodeRealtimeLightmap(realtimeColorTex);
                #if defined(DIRLIGHTMAP_COMBINED)
                    #if defined(_BICUBIC_SAMPLING_ON)
                        float4 realtimeDirTex = SampleDynamicLightmapDirBicubic(lightmapUV.zw);
                    #else
                        float4 realtimeDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicDirectionality, unity_DynamicLightmap, lightmapUV.zw);
                    #endif
                    indirectCol += DecodeDirectionalLightmap(realtimeColor, realtimeDirTex, normal);
                #else
                    indirectCol += realtimeColor;
                #endif
            }
        #endif
    #else
        // SH too hard to work in, might figure this out later
        // #if UNITY_LIGHT_PROBE_PROXY_VOLUME
        //     if (unity_ProbeVolumeParams.x == 1) {
        //         indirectCol = SHEvalLinearL0L1_SampleProbeVolume(float4(normal, 1), worldPos);
        //         indirectCol = max(0, indirectCol);
        //     }
        //     else {
        //         [branch]
        //         if (_BAKERY_SHNONLINEAR == 1)
        //             indirectCol = max(0, ShadeSHNL(normal));
        //         else
        //             indirectCol = max(0, ShadeSH9(float4(normal, 1)));
        //     }
        // #else
        //     [branch]
        //     if (_BAKERY_SHNONLINEAR == 1)
        //         indirectCol = max(0, ShadeSHNL(normal));
        //     else
        //         indirectCol = max(0, ShadeSH9(float4(normal, 1)));
        // #endif
    #endif
}

#endif // WATER_INDIRECT_INCLUDED