#ifndef WATER_FRAG_INCLUDED
#define WATER_FRAG_INCLUDED

#if defined(UNITY_PASS_SHADOWCASTER)
float4 frag(v2f i) : SV_Target {
    return 0;
}
#else
float4 frag(v2f i, bool isFrontFace: SV_IsFrontFace) : SV_Target {

    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
    atten = FadeShadows(i, atten);

    InputData id = (InputData)0;
    InitializeInputData(i, id, isFrontFace);

    LightingData ld = (LightingData)0;
    InitializeLightingData(i, id, ld, atten);

    CalculateBRDF(i, id, ld);

    #if LTCGI_ENABLED
        CalculateLTCGI(i, id, ld);
    #endif
    #if AREALIT_ENABLED
        CalculateAreaLit(i, id, ld);
    #endif

    ApplyLighting(id, ld);

    return CompositeFinalColor(i, id, ld);
}
#endif

#include "WaterTess.cginc"

#endif // WATER_FRAG_INCLUDED