#ifndef STANDARD_THIRDPARTY_DEFINED
#define STANDARD_THIRDPARTY_DEFINED

//--------------
// AUDIOLINK
//--------------

Texture2D _AudioTexture;
SamplerState sampler_AudioTexture;
int _AudioLinkEmission;
int _AudioLinkEmissionMeta;
float _AudioLinkEmissionStrength;
float _AudioLinkMin;
float _AudioLinkMax;

struct audioLinkData {
    bool textureExists;
    float bass;
    float lowMid;
    float upperMid;
    float treble;
};

float GetAudioLinkBand(audioLinkData al, int band){
    float4 bands = float4(al.bass, al.lowMid, al.upperMid, al.treble);
    return bands[band-1];
}

void GrabExists(inout audioLinkData al, inout float versionBand, inout float versionTime){
    float width = 0;
    float height = 0;
    _AudioTexture.GetDimensions(width, height);
    if (width > 64){
        versionBand = 0.0625;
        versionTime = 0.25;
    }
    al.textureExists = width > 16;
}

float SampleAudioTexture(float time, float band){
    return MOCHIE_SAMPLE_TEX2D_LOD(_AudioTexture, float2(time,band),0);
}

void InitializeAudioLink(inout audioLinkData al, float time){
    float versionBand = 1;
    float versionTime = 1;
    GrabExists(al, versionBand, versionTime);
    if (al.textureExists){
        time *= versionTime;
        al.bass = SampleAudioTexture(time, 0.125 * versionBand);
        al.lowMid = SampleAudioTexture(time, 0.375 * versionBand);
        al.upperMid = SampleAudioTexture(time, 0.625 * versionBand);
        al.treble = SampleAudioTexture(time, 0.875 * versionBand);
    }
}

//--------------
// LTCGI
//--------------

float4 _LTCGI_DiffuseColor;
float4 _LTCGI_SpecularColor;
float _LTCGIStrength;
float _LTCGIRoughness;
float _LTCGISpecularOcclusion;

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

void CalculateLTCGI(v2f i, InputData id, inout LightingData ld){
    if (_LTCGIStrength > 0){
        accumulator_struct acc = (accumulator_struct)0;
        float2 lmuv = (i.lightmapUV.xy - unity_LightmapST.zw) / unity_LightmapST.xy;
        LTCGI_Contribution(acc, i.worldPos, id.normal, ld.viewDir, id.roughness * _LTCGIRoughness, lmuv);
        ld.ltcgiSpecularity = acc.specular * _LTCGIStrength;
        ld.ltcgiDiffuse = acc.diffuse * id.baseColor.rgb * id.occlusion * ld.omr * _LTCGIStrength;
    }
}

#endif

//--------------
// AREALIT
//--------------

MOCHIE_DECLARE_TEX2D(_AreaLitOcclusion);
float4 _AreaLitOcclusion_ST;
int _AreaLitOcclusionUVSet;
int _AreaLitToggle;
float _AreaLitSpecularOcclusion;
float _AreaLitStrength;
float _AreaLitRoughnessMultiplier;
float _OpaqueLights;

#if AREALIT_ENABLED

#include "../../AreaLit/Shader/Lighting.hlsl"

void CalculateAreaLit(v2f i, InputData id, inout LightingData ld){
    #if AREALIT_ENABLED
        if (_AreaLitStrength > 0){
            float4 alOcclusion = MOCHIE_SAMPLE_TEX2D(_AreaLitOcclusion, i.uv4.xy);
            AreaLightFragInput ai;
            ai.pos = i.worldPos;
            ai.normal = id.normal;
            ai.view = -ld.viewDir;
            ai.roughness = id.roughness * _AreaLitRoughnessMultiplier;
            ai.occlusion = float4(id.occlusion.xxx, 1) * alOcclusion;
            ai.screenPos = i.pos.xy;
            float4 diffTerm, specTerm;
            ShadeAreaLights(ai, diffTerm, specTerm, true, !IsSpecularOff(), IsStereo());
            ld.areaLitSpecularity = specTerm * ld.specularTint * _AreaLitStrength;
            ld.areaLitDiffuse = diffTerm * id.baseColor * ld.omr * _AreaLitStrength;
        }
    #endif
}

#endif

#endif