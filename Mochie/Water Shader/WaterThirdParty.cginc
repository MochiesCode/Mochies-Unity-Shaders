#ifndef WATER_THIRDPARTY_DEFINED
#define WATER_THIRDPARTY_DEFINED

float4 _LTCGI_DiffuseColor;
float4 _LTCGI_SpecularColor;
float _LTCGIStrength;
float _LTCGIRoughness;

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

#if !defined(META_PASS)
float3 GetLTCGISpecularity(v2f i, InputData id, LightingData ld){
    if (_LTCGIStrength > 0){
        accumulator_struct acc = (accumulator_struct)0;
        LTCGI_Contribution(acc, i.worldPos, id.normal, ld.viewDir, id.roughness * _LTCGIRoughness, 0);
        return acc.specular * _LTCGIStrength;
    }
    return 0;
}

void CalculateLTCGI(v2f i, InputData id, inout LightingData ld){
    ld.ltcgiSpecularity = GetLTCGISpecularity(i, id, ld);
    ld.reflectionCol += ld.ltcgiSpecularity * ld.reflAdjust;
}
#endif

#endif

#if AREALIT_ENABLED
#include "../../AreaLit/Shader/Lighting.hlsl"

#if !defined(META_PASS)
void CalculateAreaLit(v2f i, InputData id, inout LightingData ld){
    AreaLightFragInput ai;
    ai.pos = i.worldPos;
    ai.normal = id.normal;
    ai.view = -ld.viewDir;
    ai.roughness = id.roughBRDF * _AreaLitRoughnessMult;
    ai.occlusion = 1;
    ai.screenPos = i.pos.xy;
    half4 diffTerm, specTerm;
    if (_AreaLitStrength > 0){
        ShadeAreaLights(ai, diffTerm, specTerm, true, !IsSpecularOff(), IsStereo());
    }
    else {
        diffTerm = 0;
        specTerm = 0;
    }
    ld.areaLitColor = id.diffuse.rgb * diffTerm + ld.specularTint * specTerm;
    ld.areaLitColor *= _AreaLitStrength * MOCHIE_SAMPLE_TEX2D_SAMPLER(_AreaLitMask, sampler_FlowMap, TRANSFORM_TEX(i.uv, _AreaLitMask)).r;
}
#endif
#endif

#include "../Common/AudioLink.cginc"

struct audioLinkData {
    bool textureExists;
    float bass;
    float lowMid;
    float upperMid;
    float treble;
};

float GetAudioLinkBand(audioLinkData al, int band){
    float4 bands = float4(al.bass, al.lowMid, al.upperMid, al.treble);
    return bands[band];
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

#endif