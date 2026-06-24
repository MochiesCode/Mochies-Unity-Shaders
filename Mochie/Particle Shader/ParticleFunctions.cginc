#ifndef PARTICLE_FUNCTIONS_INCLUDED
#define PARTICLE_FUNCTIONS_INCLUDED

#if !defined(X_VERSION)
float2 GetUVs(v2f i, int mode, float4 scaleOffset, float2 speed, float polarRadius, float polarRotation, float polarSpeed){
    return ScaleOffsetScrollUV(i.uv0.xy, scaleOffset.xy, scaleOffset.zw, speed);
}
#endif

float GetAudioLinkBand(audioLinkData al, int band, float remapMin, float remapMax){
    float4 bands = float4(al.bass, al.lowMid, al.upperMid, al.treble);
    return Remap(bands[band], _AudioLinkRemapMin, _AudioLinkRemapMax, remapMin, remapMax);
}

void InitializeAudioLink(inout audioLinkData al){
    al.textureExists = AudioLinkIsAvailable();
    if (al.textureExists){
        al.bass = AudioLinkData(ALPASS_AUDIOBASS);
        al.lowMid = AudioLinkData(ALPASS_AUDIOLOWMIDS);
        al.upperMid = AudioLinkData(ALPASS_AUDIOHIGHMIDS);
        al.treble = AudioLinkData(ALPASS_AUDIOTREBLE);
    }
}

void ApplyDistortionBlend(v2f i, inout float4 baseColor){
    i.uvGrab.xy /= i.uvGrab.w;
    float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
    float3 grabCol = 0;
    [branch]
    if (true){
        grabCol = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_ParticleGrab, i.uvGrab.xy).rgb;
    }
    else {
        grabCol = GetEnvironmentReflections(-viewDir + offset, i.worldPos, 0);
    }
    baseColor.rgb = lerp(baseColor.rgb, grabCol.rgb*lerp(1,baseColor.a,_BlendMode == 1), _DistortionBlend);
}

float2 GetMeshRefraction(v2f i){
    float2 offset = 0;
    if (_MeshRefraction == 1){
        float3 vNormalDir = normalize(i.normal);
        float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
        float2 screenUV = GetGrabPos(UNITY_PROJ_COORD(i.uvGrab));
        float2 IOR = (_RefractionIOR - 1) * mul(UNITY_MATRIX_V, float4(vNormalDir, 0));
        float2 meshOffset = ((1/(i.uvGrab.z + 1) * IOR)) * (1-dot(vNormalDir, viewDir));
        meshOffset = float2(meshOffset.x, -(meshOffset.y * _ProjectionParams.x));
        offset = meshOffset;
    }
    return offset;
}

void ApplyDistortion(inout v2f i, inout float2 uv, float alpha, audioLinkData al, out float3 offset){
    #if defined(_AUDIOLINK_ON)
        if (_AudioLinkDistortionStrength > 0){
            float alDistortion = GetAudioLinkBand(al, _AudioLinkDistortionBand, _AudioLinkRemapDistortionMin, _AudioLinkRemapDistortionMax);
            _DistortionStr *= lerp(1, alDistortion, _AudioLinkDistortionStrength * _AudioLinkStrength);
        }
    #endif
    float2 normalMapUV = GetUVs(i, _NormalMapUVMode, _NormalMap_ST, _NormalMapSpeed, _NormalMapPolarRadius, _NormalMapPolarRotation, _NormalMapPolarSpeed);
    float4 normalMap = tex2D(_NormalMap, normalMapUV);
    #if defined(_FLIPBOOK_BLENDING)
        float4 normalMap2 = tex2D(_NormalMap, i.uv0.zw);
        normalMap = lerp(normalMap, normalMap2, i.animBlend.x);
    #endif
    float3 normal = UnpackNormal(normalMap);
    offset = normal * alpha * _DistortionStr * ((i.color.r + i.color.b + i.color.g)/3.0);
    offset.xy += GetMeshRefraction(i);

    #if defined(_FADING_ON)
        offset *= fade;
    #endif
    #if defined(_DISTORTION_ON)
        i.uvGrab.xy += offset;
    #endif
    #if defined(_DISTORTION_UV_ON)
        i.uv0.xy += offset;
        i.uv0.zw += offset;
        uv += offset;
    #endif
}

void ApplyHSVFilter(inout float4 col, audioLinkData al){
    float3 baseCol = col;
    _Hue += frac(_Time.y*_AutoShiftSpeed) * _AutoShift;
    float3 filteredCol = col;
    if (_HueMode == 0)
        filteredCol = HueShift(col.rgb, _Hue, _MonoTint);
    else
        filteredCol = HueShiftOklab(col.rgb, _Hue, _MonoTint);
    filteredCol = GetSaturation(filteredCol, _Saturation);
    filteredCol = lerp(filteredCol, GetHDR(filteredCol), _HDR);
    filteredCol = GetContrast(filteredCol, _Contrast);
    col.rgb = lerp(col.rgb, filteredCol, col.a);
    col.rgb *= _Brightness;
    #if defined(_AUDIOLINK_ON)
        if (_AudioLinkFilterStrength > 0){
            float alFilter = GetAudioLinkBand(al, _AudioLinkFilterBand, _AudioLinkRemapFilterMin, _AudioLinkRemapFilterMax);
            alFilter = lerp(1, alFilter, _AudioLinkFilterStrength * _AudioLinkStrength);
            col.rgb = lerp(baseCol, col.rgb, alFilter);
        }
    #endif
}

void Softening(v2f i, inout float fade){
    float2 screenUV = i.projPos.xy / i.projPos.w;
    #if UNITY_UV_STARTS_AT_TOP
        if (_CameraDepthTexture_TexelSize.y < 0) {
            screenUV.y = 1 - screenUV.y;
        }
    #endif
    float sceneZ = LinearEyeDepth(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV));
    float partZ = i.projPos.z;
    fade = saturate((1-_SoftenStr) * (sceneZ-partZ));
}

void ApplyEmission(v2f i, LightingData ld, audioLinkData al, inout float3 col){
    float2 emissionMapUV = GetUVs(i, _EmissionMapUVMode, _EmissionMap_ST, _EmissionMapSpeed, _EmissionMapPolarRadius, _EmissionMapPolarRotation, _EmissionMapPolarSpeed);
    float3 emission = tex2D(_EmissionMap, emissionMapUV) * _EmissionColor;
    #if defined(X_VERSION)
        ApplyLightReactivity(ld, emission);
    #endif
    #if defined(_AUDIOLINK_ON)
        if (_AudioLinkEmissionStrength > 0){
            float alEmiss = GetAudioLinkBand(al, _AudioLinkEmissionBand, _AudioLinkRemapEmissionMin, _AudioLinkRemapEmissionMax);
            alEmiss = lerp(1, alEmiss, _AudioLinkEmissionStrength * _AudioLinkStrength);
            emission *= alEmiss;
        }
    #endif
    col += emission * globalAlpha;
}

float4 GetAlpha(v2f i){
    float2 alphaMaskUV = GetUVs(i, _AlphaMaskUVMode, _AlphaMask_ST, _AlphaMaskSpeed, _AlphaMaskPolarRadius, _AlphaMaskPolarRotation, _AlphaMaskPolarSpeed);
    float alpha = tex2D(_AlphaMask, alphaMaskUV)[_AlphaMaskChannel];
    #if defined(_FLIPBOOK_BLENDING)
        float alphaBlend = tex2D(_AlphaMask, i.uv0.zw)[_AlphaMaskChannel];
        alpha = lerp(alpha, alphaBlend, i.animBlend.x);
    #endif
    return alpha;
}

float4 ApplyLayeredTex(v2f i, float4 texCol){
    float alpha = _BlendMode == 1 ? texCol.a : 1;
    float2 secondTexUV = GetUVs(i, _SecondTexUVMode, _SecondTex_ST, _SecondTexSpeed, _SecondTexPolarRadius, _SecondTexPolarRotation, _SecondTexPolarSpeed);
    float4 secondTexCol = tex2D(_SecondTex, secondTexUV) * _SecondColor;
    switch (_TexBlendMode){
        case 0: texCol = lerp(secondTexCol*alpha, texCol, texCol.a); break;
        case 1: texCol.rgb += secondTexCol.rgb*alpha; break;
        case 2: texCol.rgb -= secondTexCol.rgb; break;
        case 3: texCol.rgb *= secondTexCol.rgb; break;
        default: break;
    }
    return texCol;
}

float4 GetBaseColor(inout v2f i, audioLinkData al, float alpha){

    #if defined(_DISTORTION_ON)
        float2 mainTexUV = GetUVs(i, _MainTexUVMode, _MainTex_ST, _MainTexSpeed, _MainTexPolarRadius, _MainTexPolarRotation, _MainTexPolarSpeed);
        float4 baseCol = tex2D(_MainTex, mainTexUV);
        if (_AlphaSource == 1)
            baseCol.a = alpha;
        ApplyDistortion(i, mainTexUV, baseCol.a, al, offset);
        #if defined(_DISTORTION_UV_ON)
            baseCol = tex2D(_MainTex, mainTexUV);
            if (_AlphaSource == 1)
                baseCol.a = alpha;
        #endif
    #else
        float2 mainTexUV = GetUVs(i, _MainTexUVMode, _MainTex_ST, _MainTexSpeed, _MainTexPolarRadius, _MainTexPolarRotation, _MainTexPolarSpeed);
        float4 baseCol = tex2D(_MainTex, mainTexUV);
        if (_AlphaSource == 1)
            baseCol.a = alpha;
    #endif

    #if defined(_FLIPBOOK_BLENDING)
        float4 blendedTex = tex2D(_MainTex, i.uv0.zw);
        baseCol = lerp(baseCol, blendedTex, i.animBlend.x);
        if (_AlphaSource == 1)
            baseCol.a = alpha;
    #endif

    #if defined(_ALPHATEST_ON)
        #if defined(_AUDIOLINK_ON)
            if (_AudioLinkCutoutStrength > 0){
                float alCutout = GetAudioLinkBand(al, _AudioLinkCutoutBand, _AudioLinkRemapCutoutMin, _AudioLinkRemapCutoutMax);
                alCutout = lerp(1, alCutout, _AudioLinkCutoutStrength * _AudioLinkStrength);
                _Cutoff = lerp(1, _Cutoff, alCutout);
            }
        #endif
        if (_Cutoff == 1)
            _Cutoff += 1;
        clip(baseCol.a - _Cutoff);
    #endif

    #if defined(_LAYERED_TEX_ON)
        baseCol = ApplyLayeredTex(i, baseCol);
    #endif

    return baseCol;
}

float4 GetProjPos(float4 vertex0, float4 vertex1){
    float4 projPos = ComputeScreenPos(vertex1);
    projPos.z = -UnityObjectToViewPos(vertex0).z;
    return projPos;
}

float GetFalloff(v2f i){
    float dist = distance(GetCameraPos(), i.center);
    float falloff = smoothstep(_MaxRange, clamp(_MinRange, 0, _MinRange-0.001), dist);
    falloff *= smoothstep(clamp(_NearMinRange, 0, _NearMaxRange-0.001), _NearMaxRange, dist);
    return falloff;
}

float GetPulse(){
    float pulse = 1;
    switch (_Waveform){
        case 0: pulse = 0.5*(sin(_Time.y * _PulseSpeed)+1); break;
        case 1: pulse = round((sin(_Time.y * _PulseSpeed)+1)*0.5); break;
        case 2: pulse = abs((_Time.y * (_PulseSpeed * 0.333)%2)-1); break;
        case 3: pulse = frac(_Time.y * (_PulseSpeed * 0.2)); break;
        case 4: pulse = 1-frac(_Time.y * (_PulseSpeed * 0.2)); break;
        default: break;
    }
    return lerp(1, pulse, _PulseStr);
}

float3x3 ConstructTBNMatrix(v2f i, float3 normal){
    float crossSign = (i.tangent.w > 0.0 ? 1.0 : -1.0) * unity_WorldTransformParams.w;
    float3 binormal = cross(normal, i.tangent.xyz) * crossSign;
    return float3x3(i.tangent.xyz, binormal, normal);
}

float3 GetNormal(v2f i, float3x3 tbn){
    float2 normalMapLightingUV = GetUVs(i, _NormalMapLightingUVMode, _NormalMapLighting_ST, _NormalMapLightingSpeed, _NormalMapLightingPolarRadius, _NormalMapLightingPolarRotation, _NormalMapLightingPolarSpeed);
    float3 normalMap = UnpackScaleNormal(tex2D(_NormalMapLighting, normalMapLightingUV), _NormalMapLightingScale);
    #if defined(_FLIPBOOK_BLENDING)
        float3 blendedTex = UnpackScaleNormal(tex2D(_NormalMapLighting, i.uv0.zw), _NormalMapLightingScale);
        normalMap = lerp(normalMap, blendedTex, i.animBlend.x);
    #endif
    return Unity_SafeNormalize(mul(normalMap, tbn));
}

void InitializeInputData(v2f i, inout InputData id, audioLinkData al, float4 albedo){
    id.normal = normalize(i.normal);
    #if defined(_NORMALMAP_ON)
        float3x3 tbn = ConstructTBNMatrix(i, id.normal);
        id.normal = GetNormal(i, tbn);
    #endif

    #if defined(_WORKFLOW_PACKED_ON)
        float2 packedMapUV = GetUVs(i, _PackedMapUVMode, _PackedMap_ST, _PackedMapSpeed, _PackedMapPolarRadius, _PackedMapPolarRotation, _PackedMapPolarSpeed);
        float4 packedMap = tex2D(_PackedMap, packedMapUV);
        id.metallic = packedMap[_MetallicChannel] * _PackedMetallicStrength;
        id.roughness = packedMap[_RoughnessChannel] * _PackedRoughnessStrength;
        id.occlusion = lerp(1, packedMap[_OcclusionChannel], _PackedOcclusionStrength);
    #else
        id.metallic = _Metallic;
        #if defined(_METALLIC_MAP_ON)
            float2 metallicMapUV = GetUVs(i, _MetallicMapUVMode, _MetallicMap_ST, _MetallicMapSpeed, _MetallicMapPolarRadius, _MetallicMapPolarRotation, _MetallicMapPolarSpeed);
            id.metallic = tex2D(_MetallicMap, metallicMapUV).g * _Metallic;
        #endif
        id.roughness = _Roughness;
        #if defined(_ROUGHNESS_MAP_ON)
            float2 roughnessMapUV = GetUVs(i, _RoughnessMapUVMode, _RoughnessMap_ST, _RoughnessMapSpeed, _RoughnessMapPolarRadius, _RoughnessMapPolarRotation, _RoughnessMapPolarSpeed);
            id.roughness = tex2D(_RoughnessMap, roughnessMapUV).g * _Roughness;
        #endif
        id.occlusion = 1;
        #if defined(_OCCLUSION_MAP_ON)
            float2 occlusionMapUV = GetUVs(i, _OcclusionMapUVMode, _OcclusionMap_ST, _OcclusionMapSpeed, _OcclusionMapPolarRadius, _OcclusionMapPolarRotation, _OcclusionMapPolarSpeed);
            id.occlusion = lerp(1, tex2D(_OcclusionMap, occlusionMapUV).g, _Occlusion);
        #endif
    #endif
    id.albedo = albedo * _Color * i.color;
}

float4 GetColor(v2f i, float omr, float4 tex){
    float4 col = tex;
    #if defined(_ALPHABLEND_ON)
        col = i.color * tex * _Color;
        col.a *= globalAlpha;
    #elif defined(_ALPHAPREMULTIPLY_ON)
        tex *= _Color;
        col.rgb = i.color * tex * i.color.a * tex.a;
        col.a = 1-omr + tex.a*omr;
        col *= globalAlpha;
    #elif defined(_ALPHA_ADD_ON)
        col = i.color * tex * _Color;
        col *= globalAlpha;
    #elif defined(_ALPHA_ADD_SOFT_ON)
        col = i.color * tex * _Color;
        col.rgb *= col.a;
        col *= globalAlpha;
    #elif defined(_ALPHA_MUL_ON)
        float4 prev = i.color * tex * _Color;
        col = lerp(float4(1,1,1,1), prev, prev.a * globalAlpha);
    #elif defined(_ALPHA_MULX2_ON)
        col.rgb = i.color.rgb * tex.rgb * _Color * 2;
        col.a = i.color.a * tex.a;
        col = lerp(float4(0.5,0.5,0.5,0.5), col, col.a * globalAlpha);
    #else
        col.rgb = i.color.rgb * tex.rgb * _Color * globalAlpha;
        col.a = tex.a;
    #endif
    return col;
} 
#endif // PARTICLE_FUNCTIONS_INCLUDED