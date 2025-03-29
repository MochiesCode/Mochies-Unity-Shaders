#ifndef STANDARD_FRAG_INCLUDED
#define STANDARD_FRAG_INCLUDED

float4 frag (v2f i, bool isFrontFace : SV_IsFrontFace) : SV_Target {
    
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

    InitializeDefaultSampler(defaultSampler);

    float3x3 tangentToWorld;
    float3 viewDir, tangentViewDir;
    CalculateViewDirection(i, viewDir, tangentViewDir, tangentToWorld);
    ApplyParallaxHeight(i, viewDir, tangentViewDir, i.normal, isFrontFace);

    // For triplanar
    // Easier to just make these global instead of making a struct and passing them around
    worldVertexPos = i.worldPos;
    worldVertexNormal = i.normal;
    localVertexPos = i.localPos;
    localVertexNormal = i.localNorm;

    InputData id = (InputData)0;
    InitializeInputData(i, id, tangentToWorld, isFrontFace);

    UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
    atten = FadeShadows(i.worldPos, i.lightmapUV, atten);

    LightingData ld = (LightingData)0;
    InitializeLightingData(i, id, ld, viewDir, tangentViewDir, atten);

    #if LTCGI_ENABLED
        CalculateLTCGI(i, id, ld);
    #endif
    #if AREALIT_ENABLED
        CalculateAreaLit(i, id, ld);
    #endif
    CalculateBRDF(i, id, ld);

    ApplyLighting(id, ld);

    float4 diffuse = id.diffuse;
    diffuse.a = id.alpha;
    diffuse.r += defaultSampler.r; // Stopping sampler from getting optimized out (this value is imperceptibly small)

    if (_UnityFogToggle == 1){
        UNITY_APPLY_FOG(i.fogCoord, diffuse);
    }

    DebugView(i, id, ld, diffuse);
    
    return diffuse;
}

#endif