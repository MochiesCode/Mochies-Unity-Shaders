#ifndef STANDARD_RAIN_INCLUDED
#define STANDARD_RAIN_INCLUDED

float2 GetFlipbookUV(float2 uv, float width, float height, float speed, float2 invertAxis){
    float tile = fmod(trunc(_Time.y * speed), width*height);
    float2 tileCount = float2(1.0, 1.0) / float2(width, height);
    float tileY = abs(invertAxis.y * height - (floor(tile * tileCount.x) + invertAxis.y * 1));
    float tileX = abs(invertAxis.x * width - ((tile - width * floor(tile * tileCount.x)) + invertAxis.x * 1));
    return (uv + float2(tileX, tileY)) * tileCount;
}

float3 GetFlipbookNormals(float2 uv, inout float flipbookBase, float mask){
    float2 baseUV = uv.xy;
    uv.xy = frac(uv);
    float2 flipUV = GetFlipbookUV(uv, _RainColumns, _RainRows, _RainSpeed, float2(0,1));

    float2 origUV = baseUV.xy / float2(_RainColumns, _RainRows);
    float4 uvdd = float4(ddx(origUV), ddy(origUV));

    flipbookBase = tex2Dgrad(_RainSheet, flipUV, uvdd.xw, uvdd.zw).g * mask;
    rainStrength = _RainStrength * mask;
    return tex2DnormalSmooth(_RainSheet, flipUV, uvdd, rainStrength);
}

// based on https://www.toadstorm.com/blog/?p=742
void ApplyExtraDroplets(float2 uv, inout float3 rainNormal, inout float flipbookBase, float mask){
    if (_DynamicDroplets > 0){
        float2 dropletMaskUV = uv.xy;
        float4 dropletMask = tex2Dbias(_DropletMask, float4(dropletMaskUV,0,-1));
        float3 dropletMaskNormal = UnpackScaleNormal(float4(dropletMask.rg,1,1), _RainStrength*2*mask);
        float droplets = Remap(dropletMask.b, 0, 1, -1, 1);
        droplets += (_Time.y*(_RainSpeed/200.0));
        droplets = frac(droplets);
        droplets = dropletMask.a - droplets;
        droplets = Remap(droplets, 1-_DynamicDroplets, 1, 0, 1);
        float dropletRough = smoothstep(0, 0.1, droplets);
        droplets = smoothstep(0, 0.4, droplets);
        // flipbookBase = smoothstep(0, 0.1, flipbookBase);
        flipbookBase = saturate(flipbookBase + dropletRough);
        rainNormal = lerp(rainNormal, dropletMaskNormal, droplets);
    }
}

void ApplyRainNormal(v2f i, inout InputData id){
    #if RAIN_ENABLED
        float rainMask = _RainMask.Sample(sampler_DefaultSampler, i.uv2.zw)[_RainMaskChannel];
        #if defined(_RAIN_DROPLETS_ON)
            float3 rainNormal = GetFlipbookNormals(i.uv2.xy, id.rainFlipbook, rainMask);
            ApplyExtraDroplets(i.uv2.xy, rainNormal, id.rainFlipbook, rainMask);
        #elif defined(_RAIN_RIPPLES_ON)
            float3 rainNormal = GetRipplesNormal(i.uv3.zw, 1, _RippleStrength*rainMask, _RippleSpeed, _RippleSize, _RippleDensity);
        #elif defined(_RAIN_AUTO_ON)
            float facingAngle = 1-abs(dot(id.vNormal, float3(0,1,0)));
            float threshAngle = _RainThresholdSize * 0.5;
            rainThreshold = smoothstep(_RainThreshold - threshAngle, _RainThreshold + threshAngle, facingAngle);
            float3 rainNormal0 = GetRipplesNormal(i.uv3.zw, 1, _RippleStrength*rainMask, _RippleSpeed, _RippleSize, _RippleDensity);
            float3 rainNormal1 = GetFlipbookNormals(i.uv2.xy, id.rainFlipbook, rainMask);
            ApplyExtraDroplets(i.uv2.xy, rainNormal1, id.rainFlipbook, rainMask);
            float3 rainNormal = lerp(rainNormal0, rainNormal1, rainThreshold);
        #endif
        #if defined(_NORMALMAP_ON) || defined(_DETAIL_NORMAL_ON)
            id.tsNormal = BlendNormals(id.tsNormal, rainNormal);
        #else
            id.tsNormal = rainNormal;
        #endif
    #endif
}

#endif