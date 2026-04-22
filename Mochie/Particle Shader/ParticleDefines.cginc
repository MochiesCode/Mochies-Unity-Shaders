#ifndef PARTICLE_DEFINES_INCLUDED
#pragma exclude_renderers gles
#define PARTICLE_DEFINES_INCLUDED

#include "UnityStandardUtils.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#include "../Common/Utilities.cginc"
#include "../Common/Color.cginc"
#include "../Common/Sampling.cginc"
#include "../Common/AudioLink.cginc"
#include "../Common/LightVolumes.cginc"

#if defined(SHADOWS_DEPTH) && !defined(SPOT)
    #define SHADOW_COORDS(idx1) unityShadowCoord2 _ShadowCoord : TEXCOORD##idx1;
#endif

#define IS_OPAQUE !defined(_ALPHABLEND_ON) && !defined(_ALPHAPREMULTIPLY_ON) && !defined(_ALPHA_ADD_ON) && !defined(_ALPHA_ADD_SOFT_ON) && !defined(_ALPHA_MUL_ON) && !defined(_ALPHA_MULX2_ON)
#define PBR_ENABLED defined(_REFLECTIONS_ON) || defined(_SPECULAR_HIGHLIGHTS_ON)

MOCHIE_DECLARE_TEX2D_SCREENSPACE(_CameraDepthTexture);
MOCHIE_DECLARE_TEX2D_SCREENSPACE(_MPSGrab); float4 _MPSGrab_TexelSize;
float4 _CameraDepthTexture_TexelSize;
sampler2D _MainTex;
float4 _MainTex_ST;
int _MainTexUVMode;
float2 _MainTexSpeed;
float _MainTexRotation;
float _MainTexPolarRotation;
float _MainTexPolarSpeed;
float _MainTexPolarRadius;

sampler2D _AlphaMask;
float4 _AlphaMask_ST;
int _AlphaMaskUVMode;
float2 _AlphaMaskSpeed;
float _AlphaMaskRotation;
float _AlphaMaskPolarRotation;
float _AlphaMaskPolarSpeed;
float _AlphaMaskPolarRadius;
int _AlphaMaskChannel;

sampler2D _NormalMapLighting;
float4 _NormalMapLighting_ST;
int _NormalMapLightingUVMode;
float2 _NormalMapLightingSpeed;
float _NormalMapLightingRotation;
float _NormalMapLightingPolarRotation;
float _NormalMapLightingPolarSpeed;
float _NormalMapLightingPolarRadius;
float _NormalMapLightingScale;

sampler2D _SecondTex;
float4 _SecondTex_ST;
int _SecondTexUVMode;
float2 _SecondTexSpeed;
float _SecondTexRotation;
float _SecondTexPolarRotation;
float _SecondTexPolarSpeed;
float _SecondTexPolarRadius;

sampler2D _MetallicMap;
float4 _MetallicMap_ST;
int _MetallicMapUVMode;
float2 _MetallicMapSpeed;
float _MetallicMapRotation;
float _MetallicMapPolarRotation;
float _MetallicMapPolarSpeed;
float _MetallicMapPolarRadius;
float _Metallic;

sampler2D _RoughnessMap;
float4 _RoughnessMap_ST;
int _RoughnessMapUVMode;
float2 _RoughnessMapSpeed;
float _RoughnessMapRotation;
float _RoughnessMapPolarRotation;
float _RoughnessMapPolarSpeed;
float _RoughnessMapPolarRadius;
float _Roughness;

float _ReflectionStrength;
float _SpecularHighlightStrength;
float _LightVolumeStrength;
int _LightVolumes;

sampler2D _NormalMap;
float4 _NormalMap_ST;
int _NormalMapUVMode;
float2 _NormalMapSpeed;
float _NormalMapRotation;
float _NormalMapPolarRotation;
float _NormalMapPolarSpeed;
float _NormalMapPolarRadius;

int _Emission;
float4 _EmissionColor;
sampler2D _EmissionMap;
float4 _EmissionMap_ST;
int _EmissionMapUVMode;
float2 _EmissionMapSpeed;
float _EmissionMapRotation;
float _EmissionMapPolarRotation;
float _EmissionMapPolarSpeed;
float _EmissionMapPolarRadius;
float _EmissionLightReactivityMin;
float _EmissionLightReactivityMax;
int _EmissionLightReactivity;

sampler2D _DissolveNoise;
float4 _DissolveNoise_ST;
int _DissolveNoiseUVMode;
float2 _DissolveNoiseSpeed;
float _DissolveNoiseRotation;
float _DissolveNoisePolarRotation;
float _DissolveNoisePolarSpeed;
float _DissolveNoisePolarRadius;
int _Dissolve;
int _DissolveMode;
int _DissolveRandomOffset;
int _DissolveRimBlend;
float4 _DissolveRimColor;
float _DissolveRimWidth;
float _DissolveAgeThreshold;
float _DissolveAgeThresholdMin;
float _DissolveAgeThresholdMax;
float _DissolveAmount;

// Core surface
float4 _Color;
float4 _SecondColor;

// Rendering
int _BlendMode;
int _IsCutout;
int _Softening;
int _FlipbookBlending;
int _AlphaSource;
int _Layering;
int _TexBlendMode;
int _Filtering;

// Falloff
int _Falloff;
float _NearMinRange;
float _NearMaxRange;
float _MinRange;
float _MaxRange;

// Opacity / cutoff
float _Opacity;
float _Cutoff;
float _SoftenStr;
int _CutoutRim;
int _CutoutRimBlend;
float _CutoutRimWidth;
float4 _CutoutRimColor;


// Pulse
int _Pulse;
int _Waveform;
float _PulseStr;
float _PulseSpeed;

// Distortion
int _Distortion;
int _DistortMainTex;
float _DistortionStr;
float _DistortionBlend;
float2 _DistortionSpeed;

// Color manipulation
float _Hue;
float _Saturation;
float _Contrast;
float _HDR;
float _Brightness;
float _AutoShiftSpeed;
float _HueMode;
int _AutoShift;
int _MonoTint;

// Random hue
int _RandomHue;
int _RandomHueMode;
int _RandomHueMonoTint;
float _RandomHueMax;
float _RandomHueMin;
float _RandomSatMax;
float _RandomSatMin;

// Outlines
int _Outlines;
float4 _OutlineColor;
float _OutlineThickness;

// AudioLink
float _AudioLink;
float _AudioLinkStrength;
float _AudioLinkRemapMin;
float _AudioLinkRemapMax;

float _AudioLinkFilterBand;
float _AudioLinkFilterStrength;
float _AudioLinkRemapFilterMin;
float _AudioLinkRemapFilterMax;

float _AudioLinkDistortionBand;
float _AudioLinkDistortionStrength;
float _AudioLinkRemapDistortionMin;
float _AudioLinkRemapDistortionMax;

float _AudioLinkOpacityBand;
float _AudioLinkOpacityStrength;
float _AudioLinkRemapOpacityMin;
float _AudioLinkRemapOpacityMax;

float _AudioLinkCutoutBand;
float _AudioLinkCutoutStrength;
float _AudioLinkRemapCutoutMin;
float _AudioLinkRemapCutoutMax;

float _AudioLinkOutlineBand;
float _AudioLinkOutlineStrength;
float _AudioLinkRemapOutlineMin;
float _AudioLinkRemapOutlineMax;

float _AudioLinkEmissionBand;
float _AudioLinkEmissionStrength;
float _AudioLinkRemapEmissionMin;
float _AudioLinkRemapEmissionMax;

// Misc
float _NaNLmao;
float3 specularTint;
float fade;
float2 panoUV;
float globalAlpha;
float falloff;
float pulse;

struct appdata {
    float3 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float4 color : COLOR;
    float4 uv : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 uv2 : TEXCOORD2;
    float4 uv3 : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {
    float4 pos : SV_POSITION;
    float4 uv0 : TEXCOORD0;
    float animBlend : TEXCOORD1;
    float4 uvGrab : TEXCOORD2;
    float falloff : TEXCOORD3;
    float3 center : TEXCOORD4;
    float3 vertex : TEXCOORD5;
    float4 projPos : TEXCOORD6;
    float pulse : TEXCOORD7;
    float4 color : COLOR;
    float3 worldPos : TEXCOORD8;
    float3 normal : TEXCOORD9;
    float4 tangent : TEXCOORD10;
    bool vertexLightOn : TEXCOORD11;
    float4 stableRandom : TEXCOORD12;
    float agePercent : TEXCOORD13;
    UNITY_SHADOW_COORDS(14)
    UNITY_FOG_COORDS(15)
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

struct LightingData {
    float3 reflectionCol;
    float3 specHighlightCol;
    float3 directCol;
    float3 indirectCol;
    float3 lightCol;
    float3 lightDir;
    float3 viewDir;
    float atten;
    float NdotL;
    float omr;
    float3 lightVolumeSpecularity;
    bool isRealtime;
};

struct InputData {
    float4 albedo;
    float4 diffuse;
    float3 normal;
    float3 vNormal;
    float roughness;
    float metallic;
};

struct audioLinkData {
    bool textureExists;
    float bass;
    float lowMid;
    float upperMid;
    float treble;
};

#define UNITY_PARTICLE_INSTANCE_DATA InstanceData
struct InstanceData {
    float3x4 transform;
    uint color;
    float animBlend;
    float animFrame;
    float3 center;
    float agePercent;
    float4 stableRandom;
};

#include "ParticleLighting.cginc"
#include "ParticleBRDF.cginc"
#if defined(X_VERSION)
    #include "ParticleXFeatures.cginc"
#endif
#include "ParticleFunctions.cginc"
#include "UnityStandardParticleInstancing.cginc"

#endif // PS_DEFINES_INCLUDED