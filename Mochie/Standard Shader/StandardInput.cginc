#ifndef STANDARD_INPUT_INCLUDED
#define STANDARD_INPUT_INCLUDED

void InitializeDefaultSampler(out float4 defaultSampler){
    defaultSampler = MOCHIE_SAMPLE_TEX2D_SAMPLER(_DefaultSampler, sampler_DefaultSampler, 0) * EPSILON;
}

void DebugView(v2f i, InputData id, LightingData ld, inout float4 diffuse){
    if (_MaterialDebugMode == 1 && _DebugEnable == 1){
        diffuse = _DebugBaseColor == 1 ? id.baseColor : float4(0,0,0,1);
        diffuse.rgb = _DebugAlpha == 1 ? diffuse.rgb + id.alpha : diffuse.rgb;
        diffuse.rgb = _DebugNormals == 1 ? diffuse.rgb + id.tsNormal : diffuse;
        diffuse.rgb = _DebugRoughness == 1 ? diffuse.rgb + id.roughness : diffuse;
        diffuse.rgb = _DebugMetallic == 1 ? diffuse.rgb + id.metallic : diffuse;
        diffuse.rgb = _DebugOcclusion == 1 ? diffuse.rgb + id.occlusion : diffuse;
        diffuse.rgb = _DebugHeight == 1 ? diffuse.rgb + id.height : diffuse;
        diffuse.rgb = _DebugLighting == 1 ? diffuse.rgb + ld.lightCol : diffuse.rgb;
        diffuse.rgb = _DebugAtten == 1 ? diffuse.rgb + ld.atten * ld.NdotL : diffuse;
        diffuse.rgb = _DebugReflections == 1 ? diffuse.rgb + ld.reflectionCol : diffuse.rgb;
        diffuse.rgb = _DebugSpecular == 1 ? diffuse.rgb + ld.specHighlightCol : diffuse.rgb;
        diffuse.rgb = _DebugVertexColors == 1 ? diffuse.rgb + i.color : diffuse.rgb;
    }
}

// Unused for now
// Based on https://halisavakis.com/my-take-on-shaders-color-grading-with-look-up-textures-lut/
#define COLORS 32.0
void ApplyCustomLUT(inout float3 col){
    if (_ColorGradingLUTStrength > 0 && _SampleCustomLUT == 1){
        float maxColor = COLORS - 1.0;
        float halfColX = 0.5 / _ColorGradingLUT_TexelSize.z;
        float halfColY = 0.5 / _ColorGradingLUT_TexelSize.w;
        float threshold = maxColor / COLORS;

        float xOffset = halfColX + col.r * threshold / COLORS;
        float yOffset = halfColY + col.g * threshold;
        float cell = floor(col.b * maxColor);

        float2 lutPos = float2(cell / COLORS + xOffset, yOffset);
        float4 gradedCol = MOCHIE_SAMPLE_TEX2D_SAMPLER(_ColorGradingLUT, sampler_DefaultSampler, lutPos);
        
        col = lerp(col, gradedCol, _ColorGradingLUTStrength);
    }
}
#undef COLORS

float3 Filtering(float3 col, float hue, float saturation, float brightness, float contrast, float aces, bool isPost){
    [branch]
    if (_Filtering == 1){
        if ((hue > 0 && hue < 1) || _MonoTint == 1){
            if (_HueMode == 0)
                col = HueShift(col, hue, _MonoTint);
            else
                col = HueShiftOklab(col, hue, _MonoTint);
        }
        col = lerp(dot(col, float3(0.3,0.59,0.11)), col, saturation);
        col = GetContrast(col, contrast);
        if (isPost){
            col = lerp(col, ACES(col), aces);
            // ApplyCustomLUT(col);
        }
        col *= brightness;
    }
    return col;
}

float2 SelectUVSet(float2 uvs[5], int selection, int swizzle, float3 worldPos, float3 localPos){
    if (selection < 5){
        return uvs[selection];
    }
    else if (selection == 5) {
        worldPos *= 0.2;
        worldPos += 0.5;
        float2 worldPositions[] = {-worldPos.xy, -worldPos.xz, -worldPos.yz};
        return worldPositions[swizzle];
    }
    else {
        localPos *= 0.1;
        localPos += 0.5;
        float2 localPositions[] = {-localPos.xy, -localPos.xz, -localPos.yz};
        return localPositions[swizzle];
    }
    return 0;
}

float2 SelectAreaLitOcclusionUVSet(float2 uvs[5], float2 lightmapUV, int selection){
    float2 uvSets[6] = {uvs[0], uvs[1], uvs[2], uvs[3], uvs[4], lightmapUV};
    return uvSets[selection];
}

void InitializeUVs(appdata v, inout v2f o){

    float2 uvs[5] = {v.uv0, v.uv1, v.uv2, v.uv3, v.uv4};
    
    o.uv0.xy = ScaleOffsetRotateScrollUV(SelectUVSet(uvs, _UVMainSet, _UVMainSwizzle, o.worldPos, o.localPos), _MainTex_ST.xy, _MainTex_ST.zw, _UVMainRotation, _UVMainScroll);
   
    #if DETAIL_MASK_NEEDED
        o.uv0.zw = ScaleOffsetRotateScrollUV(SelectUVSet(uvs, _UVDetailSet, _UVDetailSwizzle, o.worldPos, o.localPos), _DetailMainTex_ST.xy, _DetailMainTex_ST.zw, _UVDetailRotation, _UVDetailScroll);
        o.uv1.xy = ScaleOffsetRotateScrollUV(SelectUVSet(uvs, _UVDetailMaskSet, _UVDetailMaskSwizzle, o.worldPos, o.localPos), _DetailMask_ST.xy, _DetailMask_ST.zw, _UVDetailMaskRotation, _UVDetailMaskScroll);
    #endif

    #if defined(_PARALLAX_ON)
        o.uv1.zw = ScaleOffsetRotateScrollUV(SelectUVSet(uvs, _UVHeightMaskSet, _UVHeightMaskSwizzle, o.worldPos, o.localPos), _HeightMask_ST.xy, _HeightMask_ST.zw, _UVHeightMaskRotation, _UVHeightMaskScroll);
    #endif

    #if RAIN_ENABLED
        o.uv2.xy = Rotate2DCentered(SelectUVSet(uvs, _UVRainSet, _UVRainSwizzle, o.worldPos, o.localPos), _UVRainRotation) * _RainScale;
        o.uv2.zw = ScaleOffsetRotateScrollUV(SelectUVSet(uvs, _UVRainMaskSet, _UVRainMaskSwizzle, o.worldPos, o.localPos), _RainMask_ST.xy, _RainMask_ST.zw, _UVRainMaskRotation, _UVRainMaskScroll);
        o.uv3.zw = Rotate2DCentered(SelectUVSet(uvs, _UVRippleSet, _UVRippleSwizzle, o.worldPos, o.localPos), _UVRippleRotation) * _RippleScale;
    #endif

    #if defined(_EMISSION_ON)
        o.uv3.xy = ScaleOffsetRotateScrollUV(SelectUVSet(uvs, _UVEmissionMaskSet, _UVEmissionMaskSwizzle, o.worldPos, o.localPos), _EmissionMask_ST.xy, _EmissionMask_ST.zw, _UVEmissionMaskRotation, _UVEmissionMaskScroll);
    #endif

    #if defined(LIGHTMAP_ON) || LTCGI_ENABLED
        o.lightmapUV.xy = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
    #endif
    #if defined(DYNAMICLIGHTMAP_ON)
        o.lightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif
    
    #if AREALIT_ENABLED
        o.uv4.xy = SelectAreaLitOcclusionUVSet(uvs, v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw, _AreaLitOcclusionUVSet);
        o.uv4.xy = TRANSFORM_TEX(o.uv4.xy, _AreaLitOcclusion);
    #endif

    if (_AlphaSource == 1){
        o.uv4.zw = ScaleOffsetRotateScrollUV(SelectUVSet(uvs, _UVAlphaMaskSet, _UVAlphaMaskSwizzle, o.worldPos, o.localPos), _AlphaMask_ST.xy, _AlphaMask_ST.zw, _UVAlphaMaskRotation, _UVAlphaMaskScroll);
    }

    if (_PuddleToggle == 1){
        o.uv5.xy = ScaleOffsetRotateScrollUV(SelectUVSet(uvs, _UVPuddleSet, _UVPuddleSwizzle, o.worldPos, o.localPos), _PuddleTexture_ST.xy, _PuddleTexture_ST.zw, _UVPuddleRotation, _UVPuddleScroll);
    }
}

float4 tex2Dtri(Texture2D tex, float4 scaleTransform) {
    float3 surfaceNormal = _TriplanarCoordSpace == 1 ? abs(worldVertexNormal) : abs(localVertexNormal);
    float3 pos = _TriplanarCoordSpace == 1 ? worldVertexPos : localVertexPos;
    float3 projectedNormal = surfaceNormal / (surfaceNormal.x + surfaceNormal.y + surfaceNormal.z);

    float3 normalSign = sign(surfaceNormal);
    float2 uvX = scaleTransform.xy * (pos.zy * float2(normalSign.x, 1)) + scaleTransform.zw;
    float2 uvY = scaleTransform.xy * (pos.xz * float2(normalSign.y, 1)) + scaleTransform.zw;
    float2 uvZ = scaleTransform.xy * (pos.xy * float2(-normalSign.z, 1)) + scaleTransform.zw;

    float4 sampleX, sampleY, sampleZ;
    sampleX = sampleY = sampleZ = 0;
    if (projectedNormal.x > 0) sampleX = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_DefaultSampler, uvX);
    if (projectedNormal.y > 0) sampleY = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_DefaultSampler, uvY);
    if (projectedNormal.z > 0) sampleZ = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_DefaultSampler, uvZ);

    return (sampleX * projectedNormal.x) + (sampleY * projectedNormal.y) + (sampleZ * projectedNormal.z);
}

// Unused for now - works but has some quirks
float4 tex2Dbi(Texture2D tex, float k){

    float3 n = _TriplanarCoordSpace == 1 ? abs(worldVertexNormal) : abs(localVertexNormal);
    float3 p = _TriplanarCoordSpace == 1 ? worldVertexPos : localVertexPos;
    float3 dpdx = ddx(p);
    float3 dpdy = ddy(p);

    // determine major axis (in x; yz are following axis)
    int3 ma =  (n.x>n.y && n.x>n.z) ? int3(0,1,2) :
               (n.y>n.z)            ? int3(1,2,0) :
                                      int3(2,0,1) ;
    // determine minor axis (in x; yz are following axis)
    int3 mi =  (n.x<n.y && n.x<n.z) ? int3(0,1,2) :
               (n.y<n.z)            ? int3(1,2,0) :
                                      int3(2,0,1) ;
    // determine median axis (in x;  yz are following axis)
    int3 me = clamp(3 - mi - ma, 0, 2); 
    
    // project+fetch
    float4 x = MOCHIE_SAMPLE_TEX2D_SAMPLER_GRAD(tex, sampler_DefaultSampler, float2(p[ma.y], p[ma.z]), float2(dpdx[ma.y],dpdx[ma.z]), float2(dpdy[ma.y],dpdy[ma.z]) );
    float4 y = MOCHIE_SAMPLE_TEX2D_SAMPLER_GRAD(tex, sampler_DefaultSampler, float2(p[me.y], p[me.z]), float2(dpdx[me.y],dpdx[me.z]), float2(dpdy[me.y],dpdy[me.z]) );
    
    // blending
    float2 w = float2(n[ma.x],n[me.x]);
    w = clamp( (w-0.5773)/(1.0-0.5773), 0.0, 1.0 );
    w = pow( w, 5/8.0 );
    return (x*w.x + y*w.y) / (w.x + w.y);
}

float4 SampleTexture(Texture2D tex, float2 uv){
    float4 texSample = 0;
    #if defined(_STOCHASTIC_ON)
        texSample = tex2Dstoch(tex, sampler_DefaultSampler, uv);
    #elif defined(_TRIPLANAR_ON)
        texSample = tex2Dtri(tex, _MainTex_ST);
    #elif defined(_SUPERSAMPLING_ON)
        texSample = tex2Dsuper(tex, sampler_DefaultSampler, uv);
    #else
        texSample = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_DefaultSampler, uv);
    #endif
    return texSample;
}

float4 SampleDetailTexture(Texture2D tex, float2 uv){
    float4 texSample = 0;
    #if defined(_STOCHASTIC_DETAIL_ON) || (defined(STANDARD_LITE) && defined(_STOCHASTIC_ON))
        texSample = tex2Dstoch(tex, sampler_DefaultSampler, uv);
    #elif defined(_TRIPLANAR_DETAIL_ON) || (defined(STANDARD_LITE) && defined(_TRIPLANAR_ON))
        texSample = tex2Dtri(tex, _DetailMainTex_ST);
    #elif defined(_SUPERSAMPLING_DETAIL_ON) || (defined(STANDARD_LITE) && defined(_SUPERSAMPLING_ON))
        texSample = tex2Dsuper(tex, sampler_DefaultSampler, uv);
    #else
        texSample = MOCHIE_SAMPLE_TEX2D_SAMPLER(tex, sampler_DefaultSampler, uv);
    #endif
    return texSample;
}

float CalcMipLevel(float2 texture_coord){
    float2 dx = ddx(texture_coord);
    float2 dy = ddy(texture_coord);
    float delta_max_sqr = max(dot(dx, dx), dot(dy, dy));
    return max(0.0, 0.5 * log2(delta_max_sqr));
}

float CutoutAlpha(float alpha, float2 uv){
    float cleanAlpha = (alpha - _Cutoff) / max(fwidth(alpha), 0.0001) + 0.5;
    if (_MipMapRescaling == 1)
        cleanAlpha *= 1 + CalcMipLevel(uv * lerp(_MainTex_TexelSize.zw, _AlphaMask_TexelSize.zw, _AlphaSource)) * _MipMapScale;
    return cleanAlpha;
}

float4 SampleBaseColor(float2 uv, float2 alphaUV){
    float4 baseColor = SampleTexture(_MainTex, uv) * _Color;
    #if !defined(STANDARD_MOBILE)
        if (_AlphaSource == 1)
            baseColor.a = MOCHIE_SAMPLE_TEX2D_SAMPLER(_AlphaMask, sampler_DefaultSampler, alphaUV)[_AlphaMaskChannel];
    #endif
    #if defined(_ALPHATEST_ON)
        baseColor.a = CutoutAlpha(baseColor.a, lerp(uv, alphaUV, _AlphaSource));
    #endif
    baseColor.rgb = Filtering(baseColor.rgb, _Hue, _Saturation, _Brightness, _Contrast, 0, false);
    return baseColor;
}

float4 SampleDetailBaseColor(float2 uv){
    float4 detailBaseColor = SampleDetailTexture(_DetailMainTex, uv) * _DetailColor;
    detailBaseColor.rgb = Filtering(detailBaseColor.rgb, _HueDet, _SaturationDet, _BrightnessDet, _ContrastDet, 0, false);
    return detailBaseColor;
}

float3 SampleNormalMap(float2 uv){
    #if defined(_TRIPLANAR_ON)
        _NormalStrength *= 2;
    #endif
    return UnpackScaleNormal(SampleTexture(_NormalMap, uv), _NormalStrength);
}

float3 SampleDetailNormalMap(float2 uv, float mask){
    #if defined(_TRIPLANAR_DETAIL_ON) || (defined(STANDARD_LITE) && defined(_TRIPLANAR_ON))
        _DetailNormalStrength *= 2;
    #endif
    return UnpackScaleNormal(SampleDetailTexture(_DetailNormalMap, uv), _DetailNormalStrength * mask);
}

float4 SamplePackedMap(float2 uv){
    return SampleTexture(_PackedMap, uv);
}

float4 SampleDetailPackedMap(float2 uv){
    return SampleDetailTexture(_DetailPackedMap, uv);
}

float4 SampleMetallicMap(float2 uv){
    float4 metallic = _MetallicStrength;
    [branch]
    if (_SampleMetallic == 1){
        metallic = SampleTexture(_MetallicMap, uv) * _MetallicStrength;
    }
    return metallic;
}

float4 SampleDetailMetallicMap(float2 uv){
    return SampleDetailTexture(_DetailMetallicMap, uv);
}

float4 SampleRoughnessMap(float2 uv){
    float4 roughness = _RoughnessStrength;
    [branch]
    if (_SampleRoughness == 1){
        roughness = SampleTexture(_RoughnessMap, uv) * _RoughnessStrength;
    }
    return roughness;
}

float4 SampleDetailRoughnessMap(float2 uv){
    return SampleDetailTexture(_DetailRoughnessMap, uv);
}

float4 SampleOcclusionMap(float2 uv){
    float4 occlusion = 1;
    [branch]
    if (_SampleOcclusion == 1){
        occlusion = lerp(1, SampleTexture(_OcclusionMap, uv), _OcclusionStrength);
    }
    return occlusion;
}

float4 SampleDetailOcclusionMap(float2 uv){
    return SampleDetailTexture(_DetailOcclusionMap, uv);
}

float4 SampleEmissionMap(float2 uv){
    float4 emissionMap = SampleTexture(_EmissionMap, uv) * pow(_EmissionColor, 2.2) * _EmissionStrength;
    emissionMap.rgb = Filtering(emissionMap.rgb, _HueEmiss, _SaturationEmiss, _BrightnessEmiss, _ContrastEmiss, 0, false);
    return emissionMap;
}

float4 GetEmission(v2f i){
    float4 emissTex = SampleEmissionMap(i.uv0.xy);
    #if !defined(STANDARD_MOBILE)
        emissTex *= GetWave(_EmissionPulseWave, _EmissionPulseSpeed, _EmissionPulseStrength);
        emissTex *= MOCHIE_SAMPLE_TEX2D_SAMPLER(_EmissionMask, sampler_DefaultSampler, i.uv3.xy)[_EmissionMaskChannel];
    #endif

    #if defined(META_PASS)
        #if defined(_AUDIOLINK_ON) && defined(_AUDIOLINK_META_ON)
            audioLinkData al = (audioLinkData)0;
            InitializeAudioLink(al, 0);
            float alMult = GetAudioLinkBand(al, _AudioLinkEmission);
            alMult = Remap(alMult, 0, 1, _AudioLinkMin, _AudioLinkMax);
            emissTex *= lerp(1, alMult, _AudioLinkEmissionStrength * al.textureExists);
        #endif
    #else
        #if defined(_AUDIOLINK_ON)
            audioLinkData al = (audioLinkData)0;
            InitializeAudioLink(al, 0);
            float alMult = GetAudioLinkBand(al, _AudioLinkEmission);
            alMult = Remap(alMult, 0, 1, _AudioLinkMin, _AudioLinkMax);
            emissTex *= lerp(1, alMult, _AudioLinkEmissionStrength * al.textureExists);
        #endif
    #endif

    return emissTex;
}

void CalculateNormals(v2f i, inout InputData id, float3x3 tangentToWorld, float detailMask){

    id.vNormal = i.normal;
    id.tsNormal = id.vNormal;

    _NormalStrength *= 1-id.puddleMask;
    _DetailNormalStrength *= 1-id.puddleMask;

    #if defined(_NORMALMAP_ON) && !defined(_DETAIL_NORMAL_ON)
        id.tsNormal = SampleNormalMap(i.uv0.xy);
    #elif !defined(_NORMALMAP_ON) && defined(_DETAIL_NORMAL_ON)
        id.tsNormal = SampleDetailNormalMap(i.uv0.zw, detailMask);
    #elif defined(_NORMALMAP_ON) && defined(_DETAIL_NORMAL_ON)
        float3 baseNormal = SampleNormalMap(i.uv0.xy);
        float3 detailNormal = SampleDetailNormalMap(i.uv0.zw, detailMask);
        float3 blendedNormal0 = BlendNormals(baseNormal, detailNormal);
        float3 blendedNormal1 = lerp(baseNormal, detailNormal, detailMask);
        id.tsNormal = lerp(blendedNormal0, blendedNormal1, _DetailMaskMode);
    #endif

    ApplyRainNormal(i, id);

    #if defined(_NORMALMAP_ON) || defined(_DETAIL_NORMAL_ON) || RAIN_ENABLED
        id.normal = Unity_SafeNormalize(mul(id.tsNormal, tangentToWorld));
    #else
        id.normal = id.vNormal;
    #endif
}

void ApplyGSAA(float3 normal, inout float4 roughness){
    if (_GSAAToggle == 1){
        float3 normalDDX = ddx(normal);
        float3 normalDDY = ddy(normal); 
        float dotX = dot(normalDDX, normalDDX);
        float dotY = dot(normalDDY, normalDDY);
        float base = saturate(max(dotX, dotY));
        roughness = max(roughness, pow(base, 0.333)*_GSAAStrength);
    }
}

void InitializeInputData(v2f i, inout InputData id, float3x3 tangentToWorld, bool isFrontFace){

    id.facingAngle = abs(dot(i.normal, float3(0,1,0)));
    id.isFrontFace = isFrontFace;

    float detailMask = 1;
    #if DETAIL_MASK_NEEDED
        detailMask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_DetailMask, sampler_DefaultSampler, i.uv1.xy)[_DetailMaskChannel];
    #endif

    id.baseColor = SampleBaseColor(i.uv0.xy, i.uv4.zw);
    #if IS_TRANSPARENT
        id.alpha = saturate(id.baseColor.a);
    #else
        id.alpha = 1;
    #endif
    #if defined(_DETAIL_MAINTEX_ON)
        float4 detailBaseColor = SampleDetailBaseColor(i.uv0.zw);
        float3 blendedBaseColor = BlendColorsAlpha(id.baseColor, detailBaseColor, _DetailMainTexBlend, detailBaseColor.a);
        blendedBaseColor = lerp(blendedBaseColor, detailBaseColor, _DetailMaskMode);
        id.baseColor.rgb = lerp(id.baseColor, blendedBaseColor, _DetailMainTexStrength * detailMask);
    #endif
    if (_VertexBaseColor == 1)
        id.baseColor *= i.color;

    CalculatePuddleMask(i, id);
    
    CalculateNormals(i, id, tangentToWorld, detailMask);
    
    #if defined(_WORKFLOW_PACKED_ON)
        float4 packedMap = SamplePackedMap(i.uv0.xy);
        id.metallic = packedMap[_MetallicChannel] * _PackedMetallicStrength;
        id.roughness = packedMap[_RoughnessChannel] * _PackedRoughnessStrength;
        id.occlusion = lerp(1, packedMap[_OcclusionChannel], _PackedOcclusionStrength);
    #else
        id.metallic = SampleMetallicMap(i.uv0.xy).g;
        id.roughness = SampleRoughnessMap(i.uv0.xy).g;
        id.occlusion = SampleOcclusionMap(i.uv0.xy).g;
    #endif

    float blendedMetallic, blendedRoughness, blendedOcclusion;
    blendedMetallic = blendedRoughness = blendedOcclusion = 0;
    #if defined(_WORKFLOW_DETAIL_PACKED_ON)
        float4 detailPackedMap = SampleDetailPackedMap(i.uv0.zw);
        if (_DetailMaskMode == 1){
            blendedMetallic = detailPackedMap[_DetailMetallicChannel];
            blendedRoughness = detailPackedMap[_DetailRoughnessChannel];
            blendedOcclusion = detailPackedMap[_DetailOcclusionChannel];
        }
        else {
            blendedMetallic = BlendScalars(id.metallic, detailPackedMap[_DetailMetallicChannel], _DetailMetallicBlend);
            blendedRoughness = BlendScalars(id.roughness, detailPackedMap[_DetailRoughnessChannel], _DetailRoughnessBlend);
            blendedOcclusion = BlendScalars(id.occlusion, detailPackedMap[_DetailOcclusionChannel], _DetailOcclusionBlend);
        }
        id.metallic = lerp(id.metallic, saturate(blendedMetallic), _DetailMetallicStrength * detailMask);
        id.roughness = lerp(id.roughness, blendedRoughness, _DetailRoughnessStrength * detailMask);
        id.occlusion = lerp(id.occlusion, saturate(blendedOcclusion), _DetailOcclusionStrength * detailMask);
    #else
        #if defined(_DETAIL_METALLIC_ON)
            float4 detailMetallic = SampleDetailMetallicMap(i.uv0.zw).b;
            blendedMetallic = BlendScalarsAlpha(id.metallic, detailMetallic, _DetailMetallicBlend, detailMetallic.a);
            blendedMetallic = lerp(blendedMetallic, detailMetallic, _DetailMaskMode);
            id.metallic = lerp(id.metallic, saturate(blendedMetallic), _DetailMetallicStrength * detailMask);
        #endif
        #if defined(_DETAIL_ROUGHNESS_ON)
            float4 detailRoughness = SampleDetailRoughnessMap(i.uv0.zw).g;
            blendedRoughness = BlendScalarsAlpha(id.roughness, detailRoughness, _DetailRoughnessBlend, detailRoughness.a);
            blendedRoughness = lerp(blendedRoughness, detailRoughness, _DetailMaskMode);
            id.roughness = lerp(id.roughness, blendedRoughness, _DetailRoughnessStrength * detailMask);
        #endif
        #if defined(_DETAIL_OCCLUSION_ON)
            float4 detailOcclusion = SampleDetailOcclusionMap(i.uv0.zw).r;
            blendedOcclusion = BlendScalarsAlpha(id.occlusion, detailOcclusion, _DetailOcclusionBlend, detailOcclusion.a);
            blendedOcclusion = lerp(blendedOcclusion, detailOcclusion, _DetailMaskMode);
            id.occlusion = lerp(id.occlusion, saturate(blendedOcclusion), _DetailOcclusionStrength * detailMask);
        #endif
    #endif

    id.roughness = abs(_SmoothnessToggle - id.roughness);

    #if !defined(STANDARD_MOBILE)
        ApplyPuddles(i, id);
    #endif

    id.diffuse = id.baseColor;

    #if defined(_RAIN_DROPLETS_ON) || defined(_RAIN_AUTO_ON)
        id.rainFlipbook = smoothstep(0, 0.1, id.rainFlipbook);
        id.rainFlipbook *= smoothstep(0, 0.1, rainStrength);
        #if defined(_RAIN_AUTO_ON)
            id.rainFlipbook *= rainThreshold;
        #endif
        id.roughness = saturate(id.roughness-id.rainFlipbook);
    #endif

    #if !defined(META_PASS)
        ApplyGSAA(id.vNormal, id.roughness);
    #endif

    #if defined(_EMISSION_ON)
        id.emission = GetEmission(i);
    #endif
}

#endif