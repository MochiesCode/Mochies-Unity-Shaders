#ifndef STANDARD_RAIN_INCLUDED
#define STANDARD_RAIN_INCLUDED

float horizonAdj;

float GetHorizonAdjustment(float3 worldPos, float3 normal){
    float3 viewDir = normalize(GetCameraPos() - worldPos);
    float vdn = abs(dot(viewDir, normal));
    float rim = saturate(1-pow(1-vdn, 3));
    rim = smoothstep(0, 1-_RippleHorizonAdjustmentDistance, rim * rim);
    return rim;
}

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

float3 GetRippleNormal(v2f i, InputData id, float rainMask, float horizonAdjustment){
    float3 rainNormal = 0;
    [branch]
    if (_PuddleToggle == 1){
        float3 dryRain = GetRipplesNormal(i.uv3.zw, 1, _RippleStrength*rainMask*horizonAdjustment, _RippleSpeed, _RippleSize, _RippleDensity);
        float3 puddleRain = GetRipplesNormal(i.uv3.zw, 1, _PuddleRippleStrength*rainMask*horizonAdjustment, _PuddleRippleSpeed, _PuddleRippleSize, _PuddleRippleDensity);
        rainNormal = lerp(dryRain, puddleRain, id.puddleMask);
    }
    else {
        rainNormal = GetRipplesNormal(i.uv3.zw, 1, _RippleStrength*rainMask*horizonAdjustment, _RippleSpeed, _RippleSize, _RippleDensity);
    }
    return rainNormal;
}

void ApplyRainNormal(v2f i, inout InputData id){
    #if RAIN_ENABLED
        #if defined(_PARALLAX_ON) // Doing this here to facilitate rain ripples being at the same offset as puddle surfaces
            float parallaxStrength =  lerp(1, parallaxOffset * lerp(_PuddleHeightStrength, 1, _PuddleUseHeightMap), id.puddleMask);
            i.uv2.xy += parallaxOffset * (_RainScale / _MainTex_ST.xy) * parallaxStrength;
            i.uv2.zw += parallaxOffset * (_RainMask_ST.xy / _MainTex_ST.xy) * parallaxStrength;
            i.uv3.zw += parallaxOffset * (_RippleScale / _MainTex_ST.xy) * parallaxStrength;
        #endif
        float rainMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_RainMask, sampler_DefaultSampler, i.uv2.zw)[_RainMaskChannel];
        #if defined(_RAIN_DROPLETS_ON)
            float3 rainNormal = GetFlipbookNormals(i.uv2.xy, id.rainFlipbook, rainMask);
            ApplyExtraDroplets(i.uv2.xy, rainNormal, id.rainFlipbook, rainMask);
        #elif defined(_RAIN_RIPPLES_ON)
            float horizonAdjustment = lerp(1, GetHorizonAdjustment(i.worldPos, id.vNormal), _RippleHorizonAdjustment);
            horizonAdj = horizonAdjustment;
            float3 rainNormal = GetRippleNormal(i, id, rainMask, horizonAdjustment);
        #elif defined(_RAIN_AUTO_ON)
            float horizonAdjustment = lerp(1, GetHorizonAdjustment(i.worldPos, id.vNormal), _RippleHorizonAdjustment);
            horizonAdj = horizonAdjustment;
            float threshAngle = _RainThresholdSize * 0.5;
            float rainThreshold = smoothstep(_RainThreshold - threshAngle, _RainThreshold + threshAngle, 1-id.facingAngle);
            float3 rainNormal0 = GetRippleNormal(i, id, rainMask, horizonAdjustment);
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

void CalculatePuddleMask(v2f i, inout InputData id){
    [branch]
    if (_PuddleToggle == 1 && id.facingAngle > 0 && id.isFrontFace){
        float puddleThreshold = smoothstep(0.98 - 0.02, 0.98 + 0.02, id.facingAngle);
        #if defined(_PARALLAX_ON)
            [branch]
            if (_PuddleUseHeightMap == 1){
                #if defined(_WORKFLOW_PACKED_ON)
                    id.puddleMask = (1-MOCHIE_SAMPLE_TEX2D_SAMPLER(_PackedMap, sampler_DefaultSampler, i.uv5.xy)[_HeightChannel]) * _PuddleStrength * puddleThreshold;
                #else
                    id.puddleMask = (1-MOCHIE_SAMPLE_TEX2D_SAMPLER(_HeightMap, sampler_DefaultSampler, i.uv5.xy).g) * _PuddleStrength * puddleThreshold;
                #endif
            }
            else {
                id.puddleMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_PuddleTexture, sampler_DefaultSampler, i.uv5.xy).g * _PuddleStrength * puddleThreshold;
            }
        #else
            id.puddleMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_PuddleTexture, sampler_DefaultSampler, i.uv5.xy).g * _PuddleStrength * puddleThreshold;
        #endif
        id.puddleMask = linearstep(_PuddleThresholdMin, _PuddleThresholdMax, id.puddleMask);
    }
}

void ApplyPuddles(v2f i, inout InputData id){
    if (_PuddleToggle == 1){
        id.roughness = saturate(id.roughness - id.puddleMask);
        id.metallic = lerp(id.metallic, _PuddleMetallic, id.puddleMask);
        id.occlusion = lerp(id.occlusion, 1, id.puddleMask * (1-_PuddleOcclusionStrength));
        #if defined(BASE_PASS)
            float tintStrength = id.puddleMask * _PuddleTint.a;
            #if defined(_PARALLAX_ON)
                if (_PuddleHeightBasedTint == 1)
                    tintStrength *= 1-finalSurfaceHeight;
            #endif
            id.baseColor.rgb = lerp(id.baseColor.rgb, _PuddleTint, tintStrength);
        #endif
    }
}

#endif