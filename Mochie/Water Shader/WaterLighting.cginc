#ifndef WATER_LIGHTING_INCLUDED
#define WATER_LIGHTING_INCLUDED

#ifndef TEXTURE2D_ARGS
#define TEXTURE2D_ARGS(textureName, samplerName) Texture2D textureName, SamplerState samplerName
#define TEXTURE2D_PARAM(textureName, samplerName) textureName, samplerName
#define SAMPLE_TEXTURE2D(textureName, samplerName, coord2) textureName.Sample(samplerName, coord2)
#endif

Texture2D _RNM0, _RNM1, _RNM2;
SamplerState custom_bilinear_clamp_sampler;
float4 _RNM0_TexelSize;

#define GRAYSCALE float3(0.2125, 0.7154, 0.0721)

float SampleBakedOcclusion(v2f i){
    #if defined (SHADOWS_SHADOWMASK)
        #if defined(LIGHTMAP_ON)
            #if defined(_BICUBIC_SAMPLING_ON)
                float4 rawOcclusionMask = SampleShadowMaskBicubic(i.lightmapUV.xy);
            #else
                float4 rawOcclusionMask = UNITY_SAMPLE_TEX2D(unity_ShadowMask, i.lightmapUV.xy);
            #endif
        #else
            float4 rawOcclusionMask = float4(1.0, 1.0, 1.0, 1.0);
            #if UNITY_LIGHT_PROBE_PROXY_VOLUME
                if (unity_ProbeVolumeParams.x == 1.0){
                    rawOcclusionMask = LPPV_SampleProbeOcclusion(i.worldPos);
                }
                else {
                    #if defined(_BICUBIC_SAMPLING_ON)
                        rawOcclusionMask = SampleShadowMaskBicubic(i.lightmapUV.xy);
                    #else
                        rawOcclusionMask = UNITY_SAMPLE_TEX2D(unity_ShadowMask, i.lightmapUV.xy);
                    #endif
                }
            #else
                #if defined(_BICUBIC_SAMPLING_ON)
                    rawOcclusionMask = SampleShadowMaskBicubic(i.lightmapUV.xy);
                #else
                    rawOcclusionMask = UNITY_SAMPLE_TEX2D(unity_ShadowMask, i.lightmapUV.xy);
                #endif
            #endif
        #endif
        return saturate(dot(rawOcclusionMask, unity_OcclusionMaskSelector));

    #else

        // In forward dynamic objects can only get baked occlusion from LPPV, light probe occlusion is done on the CPU by attenuating the light color.
        float atten = 1.0f;
        #if defined(UNITY_INSTANCING_ENABLED) && defined(UNITY_USE_SHCOEFFS_ARRAYS)
            // ...unless we are doing instancing, and the attenuation is packed into SHC array's .w component.
            atten = unity_SHC.w;
        #endif

        #if UNITY_LIGHT_PROBE_PROXY_VOLUME && !defined(LIGHTMAP_ON) && !UNITY_STANDARD_SIMPLE
            float4 rawOcclusionMask = atten.xxxx;
            if (unity_ProbeVolumeParams.x == 1.0)
                rawOcclusionMask = LPPV_SampleProbeOcclusion(i.worldPos);
            return saturate(dot(rawOcclusionMask, unity_OcclusionMaskSelector));
        #endif

        return atten;
    #endif
}

float FadeShadows(v2f i, float atten) {
    #if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
        float bakedAtten = SampleBakedOcclusion(i);
        float zDist = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
        float fadeDist = UnityComputeShadowFadeDistance(i.worldPos, zDist);
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
    float R0 = L0;
    float3 R1 = 0.5f * L1;
    float lenR1 = length(R1);
    float q = dot(normalize(R1), n) * 0.5 + 0.5;
    q = saturate(q);
    float p = 1.0f + 2.0f * lenR1 / R0;
    float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);
    return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
}

void BakeryRNMLightmapAndSpecular(inout half3 lightMap, float2 lightmapUV, inout half3 directSpecular, float3 normalTS, float3 viewDirTS, float3 viewDir, half roughness)
{
    float3 rnm0 = DecodeLightmap(_RNM0.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0));
    float3 rnm1 = DecodeLightmap(_RNM1.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0));
    float3 rnm2 = DecodeLightmap(_RNM2.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0));

    const float3 rnmBasis0 = float3(0.816496580927726f, 0.0f, 0.5773502691896258f);
    const float3 rnmBasis1 = float3(-0.4082482904638631f, 0.7071067811865475f, 0.5773502691896258f);
    const float3 rnmBasis2 = float3(-0.4082482904638631f, -0.7071067811865475f, 0.5773502691896258f);

    lightMap =    saturate(dot(rnmBasis0, normalTS)) * rnm0
                + saturate(dot(rnmBasis1, normalTS)) * rnm1
                + saturate(dot(rnmBasis2, normalTS)) * rnm2;
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

    lightMap = L0 + normalWS.x * L1x + normalWS.y * L1y + normalWS.z * L1z;

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

    sh = L0 + normalWorld.x * L1x + normalWorld.y * L1y + normalWorld.z * L1z;
    diffuseColor = max(sh, 0.0);

    specularColor = 0;
    #ifdef BAKERY_LMSPEC
        dominantDir = nL1;
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

float3 GetSH(v2f i, InputData id, LightingData ld){
    [branch]
    if (_UdonLightVolumeEnabled == 1 && _LightVolumesToggle == 1){
        LightVolumeSHSpecular(i.worldPos, lightVolumeL0, lightVolumeL1r, lightVolumeL1g, lightVolumeL1b, lvSpec, id.baseColor.rgb, 1-id.roughness, id.metallic, id.normal, ld.viewDir, id.vNormal*_LightVolumeBias, 1);
        return lerp(1, LightVolumeEvaluate(id.normal, lightVolumeL0, lightVolumeL1r, lightVolumeL1g, lightVolumeL1b), _LightVolumeStrength);
    }
    else {
        return 1;
    }
}

float3 GetRealtimeIndirectLighting(v2f i, InputData id, LightingData ld){
    float3 indirectCol = 0;
    #if UNITY_LIGHT_PROBE_PROXY_VOLUME
        if (unity_ProbeVolumeParams.x == 1){
            indirectCol = max(0, SHEvalLinearL0L1_SampleProbeVolume(float4(id.normal, 1), i.worldPos));
        }
        else {
            indirectCol = GetSH(i, id, ld);
        }
    #else
        indirectCol = GetSH(i, id, ld);
    #endif
    return indirectCol;
}

void GetIndirectLighting(inout v2f i, InputData id, inout LightingData ld) {
    ld.indirectCol = 1;
    ld.lmSpec = 0;

    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        i.lightmapUV.xy += id.normalTS.xy * _LightmapDistortion * 0.05;
        i.lightmapUV.zw += id.normalTS.xy * _LightmapDistortion * 0.05;

        #if defined(_BICUBIC_SAMPLING_ON)
            ld.indirectCol = DecodeLightmap(SampleLightmapBicubic(i.lightmapUV.xy));
            #if defined(DIRLIGHTMAP_COMBINED) || defined(BAKERY_MONOSH)
                float4 lightmapDir = SampleLightmapDirBicubic(i.lightmapUV.xy);
            #endif
        #else
            ld.indirectCol = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV.xy));
            #if defined(DIRLIGHTMAP_COMBINED) || defined(BAKERY_MONOSH)
                float4 lightmapDir = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, i.lightmapUV.xy);
            #endif
        #endif

        float roughness = _Roughness;
        #if PBR_ENABLED
            roughness = id.roughness;
        #endif
        roughness = max(0.004, roughness * roughness);

        #if defined(BAKERY_MONOSH)
            BakeryMonoSH(ld.indirectCol, ld.lmSpec, lightmapDir, id.normal, ld.viewDir, roughness);
        #elif defined(BAKERY_RNM)
            BakeryRNMLightmapAndSpecular(ld.indirectCol, i.lightmapUV.xy, ld.lmSpec, id.normalTS, i.tangentViewDir, ld.viewDir, roughness);
        #elif defined(BAKERY_SH)
            BakerySHLightmapAndSpecular(ld.indirectCol, i.lightmapUV.xy, ld.lmSpec, id.normal, ld.viewDir, roughness);
        #else
            #if defined(DIRLIGHTMAP_COMBINED)
                ld.indirectCol = DecodeDirectionalLightmap(ld.indirectCol, lightmapDir, id.normal);
            #endif
        #endif

        #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN)
            ld.indirectCol = SubtractMainLightWithRealtimeAttenuationFromLightmap(ld.indirectCol, ld.atten, 0, id.normal);
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
                    ld.indirectCol += DecodeDirectionalLightmap(realtimeColor, realtimeDirTex, id.normal);
                #else
                    ld.indirectCol += realtimeColor;
                #endif
            }
        #endif

        [branch]
        if (_UdonLightVolumeEnabled == 1 && _AdditiveLightVolumesToggle == 1 && _LightVolumesToggle == 1){
            LightVolumeAdditiveSHSpecular(i.worldPos, lightVolumeL0, lightVolumeL1r, lightVolumeL1g, lightVolumeL1b, lvSpec, id.baseColor.rgb, 1-roughness, id.metallic, id.normal, ld.viewDir, id.vNormal*_LightVolumeBias, 1);
            ld.indirectCol += LightVolumeEvaluate(id.normal, lightVolumeL0, lightVolumeL1r, lightVolumeL1g, lightVolumeL1b) * _AdditiveLightVolumeStrength;
        }

        ld.indirectCol = GetSaturation(ld.indirectCol, _IndirectSaturation);
        ld.indirectCol = linearstep(-0.5, 0.5, ld.indirectCol);
        ld.indirectCol = lerp(1, ld.indirectCol, _IndirectStrength);

    #else
        ld.indirectCol = GetRealtimeIndirectLighting(i, id, ld);
    #endif
}

void InitializeLightingData(inout v2f i, inout InputData id, inout LightingData ld, float atten){

    #if BASE_PASS
        float3 lightDir = _Specular == 1 ? UnityWorldSpaceLightDir(i.worldPos) : _LightDir;
        lightDir = normalize(lightDir);
    #else
        float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
    #endif
    float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
    float3 halfVector = Unity_SafeNormalize(lightDir + viewDir);

    ld.lightDir = lightDir;
    ld.viewDir = viewDir;
    ld.halfVector = halfVector;
    ld.reflDir = reflect(-viewDir, id.normal);
    ld.screenUV = id.screenUV;
    ld.isRealtime = any(_WorldSpaceLightPos0.xyz);

    ld.NdotL = saturate(dot(id.normal, lightDir));
    ld.NdotV = abs(dot(id.normal, viewDir));

    ld.omr = unity_ColorSpaceDielectricSpec.a - id.metallic * unity_ColorSpaceDielectricSpec.a;
    float3 specularBaseColor = id.baseColor.rgb;
    #if DETAIL_BASECOLOR_ENABLED
        specularBaseColor = lerp(id.baseColor.rgb, id.detailBC.rgb, id.detailBC.a);
    #endif
    ld.specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, specularBaseColor, id.metallic);

    #if SPECULAR_ENABLED
        if (id.isFrontFace){
            atten *= ld.NdotL;
        }
    #endif
    ld.atten = atten;

    #if TRANSPARENCY_OPAQUE
        id.diffuse.rgb *= lerp(1, atten, _ShadowStrength);
    #endif
    #if defined(SHADOWS_SHADOWMASK)
        float shadowOpacity = id.alpha;
        #if DEPTH_EFFECTS_ENABLED
            #if EDGEFADE_ENABLED
                shadowOpacity *= id.edgeFadeDepth;
            #endif
        #endif
        id.diffuse.rgb *= lerp(1, atten, shadowOpacity * 0.5);
    #endif

    CalculateTangentViewDir(i);
    #if defined(UNITY_PASS_FORWARDBASE)
        GetIndirectLighting(i, id, ld);

        if (_UdonLightVolumeEnabled == 0 || _LightVolumesToggle == 0 || _LightVolumeSpecularity == 0)
            lvSpec = 0;
        ld.lightVolumeSpecularity = lvSpec * _LightVolumeSpecularityStrength;
    #endif
}

void ApplyLighting(inout InputData id, inout LightingData ld){
    #if defined(UNITY_PASS_FORWARDBASE)
        id.diffuse.rgb *= ld.indirectCol * ld.omr;
        id.diffuse.rgb += ld.lmSpec;
        ld.specHighlightCol += ld.lightVolumeSpecularity;
    #endif
    id.diffuse.rgb += ld.specHighlightCol;
    id.diffuse.rgb += ld.reflectionCol;
    id.diffuse.rgb += ld.areaLitColor;
    #if EMISSION_ENABLED
        id.diffuse.rgb += id.emission.rgb;
    #endif
}

float4 CompositeFinalColor(v2f i, InputData id, LightingData ld){
    float4 col = id.diffuse;

    if (_VisualizeFlowmap)
        col = id.flowMap;

    id.flowMap = lerp(0, id.flowMap, _ZeroProp);

    #if TRANSPARENCY_OPAQUE
        col.a = 1;
    #endif

    #if defined(UNITY_PASS_FORWARDADD)
        #if DEPTH_EFFECTS_ENABLED
            #if EDGEFADE_ENABLED
                col = lerp(0, col, id.edgeFadeDepth);
            #endif
        #endif
        col.rgb *= _LightColor0 * ld.atten;
        col = lerp(0, col, id.alpha);
    #else
        #if DEPTH_EFFECTS_ENABLED
            #if EDGEFADE_ENABLED
                col = lerp(id.baseCol, col, id.edgeFadeDepth);
            #endif
        #endif
        col = lerp(id.baseCol, col, id.alpha);
    #endif

    UNITY_APPLY_FOG(i.fogCoord, col);

    return col + id.flowMap;
}

#endif // WATER_LIGHTING_INCLUDED
