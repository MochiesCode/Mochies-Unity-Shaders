
#ifndef WATER_FUNCTIONS_INCLUDED
#define WATER_FUNCTIONS_INCLUDED

float2 ScaleUV(float2 uv, float2 scale, float2 scroll){
    return (uv + scroll * _Time.y * 0.1) * scale;
}

void ParallaxOffset(v2f i, inout float2 uv, float offset, bool isFrontFace){
    if (isFrontFace)
        uv -= (i.tangentViewDir.xy * offset);
}

void CalculateTangentViewDir(inout v2f i){
    i.tangentViewDir = normalize(i.tangentViewDir);
    i.tangentViewDir.xy /= (i.tangentViewDir.z + 0.42);
}

float3 BoxProjection(float3 dir, float3 pos, float4 cubePos, float3 boxMin, float3 boxMax){
    #if UNITY_SPECCUBE_BOX_PROJECTION
        UNITY_BRANCH
        if (cubePos.w > 0){
            float3 factors = ((dir > 0 ? boxMax : boxMin) - pos) / dir;
            float scalar = min(min(factors.x, factors.y), factors.z);
            dir = dir * scalar + (pos - cubePos);
        }
    #endif
    return dir;
}

float3 GetMirrorReflections(float4 reflUV, float3 normal, float roughness){
    float perceptualRoughness = roughness;
    perceptualRoughness = perceptualRoughness*(1.7 - 0.7*perceptualRoughness);
    float mip = perceptualRoughnessToMipmapLevel(perceptualRoughness);
    float2 normalSwizzle[3] = {normal.xy, normal.xz, normal.yz}; 
    reflUV.xy -= normalSwizzle[_MirrorNormalOffsetSwizzle];
    float2 uv = reflUV.xy / (reflUV.w + 0.00000001);
    float4 uvMip = float4(uv, 0, mip * 6);
    float3 refl = unity_StereoEyeIndex == 0 ? tex2Dlod(_ReflectionTex0, uvMip) : tex2Dlod(_ReflectionTex1, uvMip);
    return refl;
}

float3 GetWorldReflections(float3 reflDir, float3 worldPos, float roughness){
    float3 baseReflDir = reflDir;
    roughness *= 1.7-0.7*roughness;
    reflDir = BoxProjection(reflDir, worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
    float4 envSample0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
    float3 p0 = DecodeHDR(envSample0, unity_SpecCube0_HDR);
    float interpolator = unity_SpecCube0_BoxMin.w;
    UNITY_BRANCH
    if (interpolator < 0.99999){
        float3 refDirBlend = BoxProjection(baseReflDir, worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
        float4 envSample1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, refDirBlend, roughness * UNITY_SPECCUBE_LOD_STEPS);
        float3 p1 = DecodeHDR(envSample1, unity_SpecCube1_HDR);
        p0 = lerp(p1, p0, interpolator);
    }
    return p0;
}

float3 GetManualReflections(float3 reflDir, float roughness){
    roughness *= 1.7-0.7*roughness;
    reflDir = Rotate3D(reflDir, _ReflCubeRotation);
    float4 envSample0 = texCUBElod(_ReflCube, float4(reflDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
    return DecodeHDR(envSample0, _ReflCube_HDR);
}

// Unused
float SampleDepthCorrected(float2 screenUV){
    float2 texSize = _CameraDepthTexture_TexelSize.xy;
    float d0 = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV);
    float d1 = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV + float2(1.0, 0.0) * texSize);
    float d2 = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV + float2(-1.0, 0.0) * texSize);
    float d3 = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV + float2(0.0, 1.0) * texSize);
    float d4 = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV + float2(0.0, -1.0) * texSize);
    return min(d0, min(d1, min(d2, min(d3, d4))));
}

float GetDepth(v2f i, float2 screenUV){
    #if UNITY_UV_STARTS_AT_TOP
        if (_CameraDepthTexture_TexelSize.y < 0) {
            screenUV.y = 1 - screenUV.y;
        }
    #endif
    screenUV.y = _ProjectionParams.x * .5 + .5 - screenUV.y * _ProjectionParams.x;
    float backgroundDepth = LinearEyeDepth(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV));
    float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(i.uvGrab.z);
    float depthDifference = backgroundDepth - surfaceDepth;
    return depthDifference / 20;
}

float2 AlignWithGrabTexel(float2 uv) {
    #if UNITY_UV_STARTS_AT_TOP
        if (_CameraDepthTexture_TexelSize.y < 0) {
            uv.y = 1 - uv.y;
        }
    #endif
    return (floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) * abs(_CameraDepthTexture_TexelSize.xy);
}

float3 FlowUV (float2 uv, float2 flowVector, float time, float phase) {
    float progress = frac(time + phase);
    float3 uvw;
    uvw.xy = uv - flowVector * progress;
    uvw.xy += phase;
    uvw.xy += (time - progress) * jump;
    uvw.z = 1 - abs(1 - 2 * progress);
    return uvw;
}

float3 GerstnerWave(float4 wave, float3 vertex, float speed, float rotation, inout float3 tangent, inout float3 binormal, float offsetMask){
    float k = 2 * UNITY_PI / wave.w;
    float c = sqrt(9.8/k);
    float2 dir = normalize(wave.xy);
    dir = Rotate2D(dir, rotation);
    float f = k * (dot(dir,vertex.xz) - c * _Time.y*0.2*speed);
    float steepness = wave.z;
    float a = steepness / k;

    if (_RecalculateNormals == 1){
        tangent += float3(
            -dir.x * dir.x * (steepness * sin(f)),
            dir.x * (steepness * cos(f)),
            -dir.x * dir.y * (steepness * sin(f))
        ) * offsetMask;
        binormal += float3(
            -dir.x * dir.y * (steepness * sin(f)),
            dir.y * (steepness * cos(f)),
            -dir.y * dir.y * (steepness * sin(f))
        ) * offsetMask;
    }
    
    return float3(dir.x * (a*cos(f)), a * sin(f), dir.y * (a*cos(f)));
}

// [ToggleUI]_RimToggle("Enable", Int) = 0
// [HDR]_RimCol("Rim Color", Color) = (1,1,1,1)
// [Enum(Add,0, Sub,1, Mul,2, Mulx2,3, Overlay,4, Screen,5, Lerp,6)]_RimBlending("Rim Blending", Int) = 0
// _RimStr("Rim Strength", Float) = 1
// _RimWidth("Rim Width", Range (0,1)) = 0.5
// _RimEdge("Rim Edge", Range(0,0.5)) = 0
// _RimMask("Rim Mask", 2D) = "white" {}
// _UVRimMaskScroll("Scrolling", Vector) = (0,0,0,0)
// _UVRimMaskRotate("Rotation", Float) = 0

float GetHorizonAdjustment(float3 worldPos, float3 normal, float3 cameraPos){
    float3 viewDir = normalize(cameraPos - worldPos);
    float vdn = abs(dot(viewDir, normal));
    float rim = saturate(1-pow(1-vdn, 5));
    rim = smoothstep(0, 1-_HorizonAdjustmentDistance, rim);
    return rim;
}

#endif // WATER_FUNCTIONS_INCLUDED