#ifndef STANDARD_PICKING_INCLUDED
#define STANDARD_PICKING_INCLUDED

#include "StandardDefines.cginc"
#include "StandardVert.cginc"

float4 _SelectionID;

float4 picking(v2f i) : SV_Target {

    #if IS_TRANSPARENT
        InitializeDefaultSampler(defaultSampler, defaultDetailSampler);
        float4 baseColor = SampleBaseColor(i.uv0.xy, i.uv4.zw);
        #if defined(_ALPHATEST_ON)
            clip(baseColor.a);
        #else
            clip(baseColor.a - 0.1);
        #endif
    #else
        defaultSampler = 0;
    #endif
    return _SelectionID + lerp(0, defaultSampler, _ThisValueIsZero);
}

#endif