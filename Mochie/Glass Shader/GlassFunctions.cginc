
#include "../Common/Utilities.cginc"

void ApplyGSAA(float3 normal, inout float roughness){
    if (_GSAAToggle == 1){
        float3 normalDDX = ddx(normal);
        float3 normalDDY = ddy(normal); 
        float dotX = dot(normalDDX, normalDDX);
        float dotY = dot(normalDDY, normalDDY);
        float base = saturate(max(dotX, dotY));
        roughness = max(roughness, pow(base, 0.333)*_GSAAStrength);
    }
}

float4 SampleTexture(sampler2D tex, float2 uv){
    #if defined(_STOCHASTIC_SAMPLING_ON)
        return tex2Dstoch(tex, uv);
    #else
        return tex2D(tex, uv);
    #endif
    return 0;
}

float FadeShadows (float3 worldPos, float atten) {
    #if HANDLE_SHADOWS_BLENDING_IN_GI
        float viewZ = dot(_WorldSpaceCameraPos - worldPos, UNITY_MATRIX_V[2].xyz);
        float shadowFadeDistance = UnityComputeShadowFadeDistance(worldPos, viewZ);
        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        atten = saturate(atten + shadowFade);
    #endif
    return atten;
}

float3 ShadeSH9(float3 normal){
    return max(0, ShadeSH9(float4(normal,1)));
}

float2 GetFlipbookUV(float2 uv, float width, float height, float speed, float2 invertAxis){
    float tile = fmod(trunc(_Time.y * speed), width*height);
    float2 tileCount = float2(1.0, 1.0) / float2(width, height);
    float tileY = abs(invertAxis.y * height - (floor(tile * tileCount.x) + invertAxis.y * 1));
    float tileX = abs(invertAxis.x * width - ((tile - width * floor(tile * tileCount.x)) + invertAxis.x * 1));
    return (uv + float2(tileX, tileY)) * tileCount;
}

float3 GetFlipbookNormals(v2f i, inout float flipbookBase, float mask){
    float2 uv = frac(ScaleOffsetUV(i.uv, float2(_XScale, _YScale), 0));
    float2 flipUV = GetFlipbookUV(uv, _Columns, _Rows, _Speed, float2(0,1));

    float2 origUV = i.uv / float2(_Columns, _Rows) * float2(_XScale, _YScale);
    float4 uvdd = float4(ddx(origUV), ddy(origUV));

    flipbookBase = tex2Dgrad(_RainSheet, flipUV, uvdd.xw, uvdd.zw).g * mask;
    rainStrength = _Strength * mask;
    return tex2DnormalSmooth(_RainSheet, flipUV, uvdd, rainStrength);
}

// based on https://www.toadstorm.com/blog/?p=742
void ApplyExtraDroplets(v2f i, inout float3 rainNormal, inout float flipbookBase, float mask){
    if (_DynamicDroplets > 0){
        float2 dropletMaskUV = ScaleOffsetUV(i.uv, float2(_XScale, _YScale), 0);
        float4 dropletMask = tex2Dbias(_DropletMask, float4(dropletMaskUV,0,-1));
        float3 dropletMaskNormal = UnpackScaleNormal(float4(dropletMask.rg,1,1), _Strength*2*mask);
        float droplets = Remap(dropletMask.b, 0, 1, -1, 1);
        droplets += (_Time.y*(_Speed/200.0));
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

#include "GlassKernels.cginc"

float3 BlurredGrabpassSample(float2 uv, float str){
    float3 blurCol = 0;
    float2 blurStr = str;
    blurStr.x *= 0.5625;
    float2 uvBlur = uv;
    
    #if defined(_BLURQUALITY_ULTRA)
        [unroll(71)]
        for (uint index = 0; index < 71; ++index){
            uvBlur.xy = uv.xy + (kernel71[index] * blurStr);
            blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_GlassGrab, uvBlur);
        }
        blurCol /= 71;
    #elif defined(_BLURQUALITY_HIGH)
        [unroll(43)]
        for (uint index = 0; index < 43; ++index){
            uvBlur.xy = uv.xy + (kernel43[index] * blurStr);
            blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_GlassGrab, uvBlur);
        }
        blurCol /= 43;
    #elif defined(_BLURQUALITY_MED)
        [unroll(22)]
        for (uint index = 0; index < 22; ++index){
            uvBlur.xy = uv.xy + (kernel22[index] * blurStr);
            blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_GlassGrab, uvBlur);
        }
        blurCol /= 22;
    #elif defined(_BLURQUALITY_LOW)
        [unroll(16)]
        for (uint index = 0; index < 16; ++index){
            uvBlur.xy = uv.xy + (kernel16[index] * blurStr);
            blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_GlassGrab, uvBlur);
        }
        blurCol /= 16;
    #endif

    return blurCol;
}

float3 BoxProjection(float3 dir, float3 pos, float4 cubePos, float3 boxMin, float3 boxMax){
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
        UNITY_BRANCH
        if (cubePos.w > 0){
            float3 factors = ((dir > 0 ? boxMax : boxMin) - pos) / dir;
            float scalar = min(min(factors.x, factors.y), factors.z);
            dir = dir * scalar + (pos - cubePos);
        }
    #endif
    return dir;
}

float3 GetWorldReflections(float3 reflDir, float3 worldPos, float roughness){
    float3 baseReflDir = reflDir;
    roughness *= 1.7-0.7*roughness;
    reflDir = BoxProjection(reflDir, worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
    float4 envSample0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
    float3 p0 = DecodeHDR(envSample0, unity_SpecCube0_HDR);
    UNITY_BRANCH
    if (unity_SpecCube0_BoxMin.w < 0.99999){
        float3 refDirBlend = BoxProjection(baseReflDir, worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
        float4 envSample1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, refDirBlend, roughness * UNITY_SPECCUBE_LOD_STEPS);
        float3 p1 = DecodeHDR(envSample1, unity_SpecCube1_HDR);
        p0 = lerp(p1, p0, unity_SpecCube0_BoxMin.w);
    }
    return p0;
}

float SpecularTerm(float NdotL, float NdotV, float NdotH, float roughness){
    float visibilityTerm = 0;
    float rough = roughness;
    float rough2 = roughness * roughness;

    float lambdaV = NdotL * (NdotV * (1 - rough) + rough);
    float lambdaL = NdotV * (NdotL * (1 - rough) + rough);

    visibilityTerm = 0.5f / (lambdaV + lambdaL + 1e-5f);
    float d = (NdotH * rough2 - NdotH) * NdotH + 1.0f;
    float dotTerm = UNITY_INV_PI * rough2 / (d * d + 1e-7f);

    return max(0, visibilityTerm * dotTerm * UNITY_PI * NdotL);
}

#if LTCGI_ENABLED

#include "Packages/at.pimaker.ltcgi/Shaders/LTCGI_structs.cginc"

struct accumulator_struct {
    float3 diffuse;
    float3 specular;
};

void callback_diffuse(inout accumulator_struct acc, in ltcgi_output output);
void callback_specular(inout accumulator_struct acc, in ltcgi_output output);

#define LTCGI_V2_CUSTOM_INPUT accumulator_struct
#define LTCGI_V2_DIFFUSE_CALLBACK callback_diffuse
#define LTCGI_V2_SPECULAR_CALLBACK callback_specular

#include "Packages/at.pimaker.ltcgi/Shaders/LTCGI.cginc"

// now we declare LTCGI APIv2 functions for real
void callback_diffuse(inout accumulator_struct acc, in ltcgi_output output) {
    acc.diffuse += output.intensity * output.color * _LTCGI_DiffuseColor;
}
void callback_specular(inout accumulator_struct acc, in ltcgi_output output) {
    acc.specular += output.intensity * output.color * _LTCGI_SpecularColor;
}

float3 GetLTCGISpecularity(v2f i, float3 normal, float3 viewDir, float roughness){
    if (_LTCGIStrength > 0){
        accumulator_struct acc = (accumulator_struct)0;
        LTCGI_Contribution(acc, i.worldPos, normal, viewDir, roughness * _LTCGIRoughness, 0);
        return acc.specular * _LTCGIStrength;
    }
    return 0;
}

#endif
