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

float3 GetLTCGISpecularity(v2f i, float3 normal, float3 viewDir, float roughness){
    if (_LTCGIStrength > 0){
        accumulator_struct acc = (accumulator_struct)0;
        LTCGI_Contribution(acc, i.worldPos, normal, viewDir, roughness * _LTCGIRoughness, 0);
        return acc.specular * _LTCGIStrength;
    }
    return 0;
}

#endif

#endif