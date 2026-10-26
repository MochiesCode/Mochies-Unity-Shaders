
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

float4 GetScreenPosition(float4 positionCS){
    float4 ndc = positionCS * 0.5f;
    ndc.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    ndc.zw = positionCS.zw;

    ndc.xy = TransformStereoScreenSpaceTex(ndc.xy, ndc.w);
    return ndc;
}

float GetDepth(float2 uv){
    return MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, uv).r;
}

float GetCorrectionDepth(v2f i, float2 screenUV){
    #if UNITY_UV_STARTS_AT_TOP
        if (_CameraDepthTexture_TexelSize.y < 0) {
            screenUV.y = 1 - screenUV.y;
        }
    #endif
    #if !(defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3))
        screenUV.y = _ProjectionParams.x * .5 + .5 - screenUV.y * _ProjectionParams.x;
    #endif
    float backgroundDepth = LinearEyeDepth(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV));
    float surfaceDepth = LinearEyeDepth(i.uvGrab.z / i.uvGrab.w);
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

float3 GerstnerWave(float4 wave, float3 vertex, float speed, float rotation, float phase, inout float3 tangent, inout float3 binormal, float offsetMask){
    float k = 2 * UNITY_PI / wave.w;
    float c = sqrt(9.8/k);
    float2 dir = normalize(wave.xy);
    dir = Rotate2D(dir, rotation);
    float f = k * (dot(dir,vertex.xz) - c * _Time.y*0.2*speed) + phase;
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

// Frame for world space texture coordinates, rows are (u axis, up axis, v axis)
// XZ is the identity so vertex offsets behave the same as object space on an unrotated/unscaled mesh
// World coords are negated positions, so texture coordinates increase along -u and -v
float3x3 GetWorldTexCoordFrame(int swizzle){
    float3x3 frames[3] = {
        float3x3(1,0,0, 0,0,1, 0,1,0), // XY
        float3x3(1,0,0, 0,1,0, 0,0,1), // XZ
        float3x3(0,1,0, 1,0,0, 0,0,1)  // YZ
    };
    return frames[swizzle];
}

// Aligns the tangent frame with world space texture coordinates so normal maps don't rotate with the mesh
void GetWorldTexCoordTangents(float3 normal, out float3 tangent, out float3 binormal){
    float3x3 frame = GetWorldTexCoordFrame(_TexCoordSpaceSwizzle);
    tangent = -frame[0];
    binormal = -frame[2];
    tangent = Unity_SafeNormalize(tangent - normal * dot(normal, tangent));
    binormal = Unity_SafeNormalize(binormal - normal * dot(normal, binormal));
}

// Converts a world space offset to object space, cancelling out object scale/rotation
float3 WorldOffsetToObject(float3 offset){
    return mul((float3x3)unity_WorldToObject, offset);
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

#if !defined(META_PASS)
float GetHorizonAdjustment(v2f i, float3 normal, float distance){
    float3 viewDir = normalize(i.cameraPos - i.worldPos);
    float vdn = abs(dot(viewDir, normal));
    float rim = saturate(1-pow(1-vdn, 5));
    rim = smoothstep(0, 1-distance, rim);
    return rim;
}

float GetHorizonAdjustment(v2f i, float distance){
    return GetHorizonAdjustment(i, i.normal, distance);
}
#endif

#endif // WATER_FUNCTIONS_INCLUDED