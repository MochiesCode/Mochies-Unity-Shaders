#ifndef STANDARD_SHADOW_INCLUDED
#define STANDARD_SHADOW_INCLUDED

sampler3D _DitherMaskLOD;

float ShadowGetOneMinusReflectivity(v2f i){

    float metallicity = _MetallicStrength;
    #if defined(_WORKFLOW_PACKED_ON)
        metallicity = SamplePackedMap(i.uv0.xy)[_MetallicChannel] * _PackedMetallicStrength;
    #elif defined(_WORKFLOW_SPECULAR_ON)
        float4 spec = SampleSpecularMap(i.uv0.xy);
        return 1.0 - max(max(spec.r, spec.g), spec.b);
    #else
        metallicity = SampleMetallicMap(i.uv0.xy);
    #endif

    #if defined(_DETAIL_METALLIC_ON) || defined(_WORKFLOW_DETAIL_PACKED_ON)
        float detailMask = detailMask = _DetailMask.Sample(sampler_DefaultSampler, i.uv1.xy)[_DetailMaskChannel];
        // Same precedence and blending as InitializeInputData: the packed detail workflow wins over separate maps
        #if defined(_WORKFLOW_DETAIL_PACKED_ON)
            float detailMetallic = SampleDetailPackedMap(i.uv0.zw)[_DetailMetallicChannel];
            float blendedMetallic = _DetailMaskMode == 1 ? detailMetallic : BlendScalars(metallicity, detailMetallic, _DetailMetallicBlend);
            metallicity = lerp(metallicity, saturate(blendedMetallic), _DetailMetallicStrength * detailMask);
        #elif defined(_DETAIL_METALLIC_ON)
            float4 detailMetallic = SampleDetailMetallicMap(i.uv0.zw);
            metallicity = lerp(metallicity, saturate(BlendScalarsAlpha(metallicity, detailMetallic, _DetailMetallicBlend, detailMetallic.a)), _DetailMetallicStrength * detailMask);
        #endif
    #endif

    return OneMinusReflectivityFromMetallic(metallicity);
}

// Not using LOD_FADE_CROSSFADE option as it's a multi_compile and severely bloats variant count
float4 frag (v2f i, UNITY_POSITION(vpos), bool isFrontFace : SV_IsFrontFace) : SV_Target {

    if (_MaterialDebugMode == 1 && _DebugFlags != 0){
        discard;
    }

    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    InitializeDefaultSampler(defaultSampler, defaultDetailSampler);

    worldVertexPos = i.worldPos;
    worldVertexNormal = i.normal;
    localVertexPos = i.localPos;
    localVertexNormal = i.localNorm;

    float alpha = SampleBaseColor(i.uv0.xy, i.uv4.zw, i.color.a).a;
    #if defined(_ALPHATEST_ON)
        clip(alpha - _Cutoff);
    #endif
    #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
        #if defined(_ALPHAPREMULTIPLY_ON)
            float omr = ShadowGetOneMinusReflectivity(i);
            alpha = 1-omr + alpha*omr;
        #endif
        // Use dither mask for alpha blended shadows, based on pixel position xy
        // and alpha level. Our dither texture is 4x4x16.
        #if defined(LOD_FADE_CROSSFADE)
            #define _LOD_FADE_ON_ALPHA
            alpha *= unity_LODFade.y;
        #endif
        float alphaRef = tex3D(_DitherMaskLOD, float3(vpos.xy*0.25,alpha*0.9375)).a;
        clip (alphaRef - 0.01);
    #endif

    #if defined(LOD_FADE_CROSSFADE)
        #if defined(_LOD_FADE_ON_ALPHA)
            #undef _LOD_FADE_ON_ALPHA
        #else
            UnityApplyDitherCrossFade(vpos.xy);
        #endif
    #endif

    #if defined(SHADOWS_CUBE) && !defined(SHADOWS_CUBE_IN_DEPTH_TEX)
        return UnityEncodeCubeShadowDepth((length(i.vec) + unity_LightShadowBias.x) * _LightPositionRange.w) + defaultSampler + defaultDetailSampler;
    #else
        return defaultSampler + defaultDetailSampler;
    #endif
}

#endif