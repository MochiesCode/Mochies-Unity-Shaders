#ifndef STANDARD_DEFINES_INCLUDED
#define STANDARD_DEFINES_INCLUDED

#include "UnityCG.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"
#include "../Common/Utilities.cginc"
#include "../Common/Color.cginc"
#include "../Common/Sampling.cginc"

#define LTCGI_ENABLED defined(LTCGI)
#define AREALIT_ENABLED defined(_AREALIT_ON)
#define IS_OPAQUE !defined(_ALPHATEST_ON) && !defined(_ALPHABLEND_ON) && !defined(_ALPHAPREMULTIPLY_ON)
#define IS_TRANSPARENT defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
#define DETAIL_MASK_NEEDED defined(_DETAIL_MAINTEX_ON) || defined(_DETAIL_NORMAL_ON) || defined(_DETAIL_METALLIC_ON) || defined(_DETAIL_ROUGHNESS_ON) || defined(_DETAIL_OCCLUSION_ON) || defined(_WORKFLOW_DETAIL_PACKED_ON)
#define RAIN_ENABLED defined(_RAIN_DROPLETS_ON) || defined(_RAIN_RIPPLES_ON) || defined(_RAIN_AUTO_ON)
#define NEEDS_LIGHTMAP_UV defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON) || LTCGI_ENABLED || AREALIT_ENABLED
#define SSR_ENABLED defined(_SSR_ON) && !defined(SHADER_API_MOBILE)

#if defined(SHADOWS_DEPTH) && !defined(SPOT)
    #define SHADOW_COORDS(idx1) unityShadowCoord2 _ShadowCoord : TEXCOORD##idx1;
#endif

#define EPSILON 1.192092896e-07
#define GRAYSCALE float3(0.2125, 0.7154, 0.0721)

// Global stuff
Texture2D _DefaultSampler;
SamplerState sampler_DefaultSampler;
Texture2D _DFG;
SamplerState sampler_DFG;
int _BlendMode;
int _AlphaSource;
int _MipMapRescaling;
int _SmoothnessToggle;
int _TriplanarCoordSpace;
int _ApplyHeightOffset;
float _Cutoff;
float _MipMapScale;

// Base Texture Set
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_MainTex);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_NormalMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_PackedMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_MetallicMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_RoughnessMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_OcclusionMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_HeightMap);

float4 _MainTex_ST;
float4 _MainTex_TexelSize;
float2 _UVMainScroll;
float _UVMainRotation;
int _UVMainSet;
int _UVMainSwizzle;

float4 _Color;
float _MetallicStrength;
float _PackedMetallicStrength;
float _RoughnessStrength;
float _PackedRoughnessStrength;
float _OcclusionStrength;
float _PackedOcclusionStrength;
float _NormalStrength;
float _HeightStrength;
float _HeightOffset;
float _HeightSteps;

int _SampleMetallic;
int _SampleOcclusion;
int _SampleRoughness;
int _MetallicChannel;
int _RoughnessChannel;
int _OcclusionChannel;
int _HeightChannel;
int _MetallicMultiplier;
int _RoughnessMultiplier;
int _OcclusionMultiplier;
int _HeightMultiplier;

// Emission
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_EmissionMap);
int _EmissionPulseWave;
float4 _EmissionColor;
float _EmissionStrength;
float _EmissionPulseSpeed;
float _EmissionPulseStrength;

// Detail Texture Set
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailMainTex);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailPackedMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailMetallicMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailRoughnessMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailOcclusionMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMap);

float4 _DetailMainTex_ST;
float2 _UVDetailScroll;
float _UVDetailRotation;
int _UVDetailSet;
int _UVDetailSwizzle;

float4 _DetailColor;
float _DetailMainTexStrength;
float _DetailMetallicStrength;
float _DetailRoughnessStrength;
float _DetailOcclusionStrength;
float _DetailNormalStrength;

int _DetailMetallicChannel;
int _DetailRoughnessChannel;
int _DetailOcclusionChannel;
int _DetailMetallicMultiplier;
int _DetailRoughnessMultiplier;
int _DetailOcclusionMultiplier;
int _DetailMainTexBlend;
int _DetailMetallicBlend;
int _DetailRoughnessBlend;
int _DetailOcclusionBlend;

// Independant Textures
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailMask);
float4 _DetailMask_ST;
float2 _UVDetailMaskScroll;
float _UVDetailMaskRotation;
int _UVDetailMaskSet;
int _UVDetailMaskSwizzle;
int _DetailMaskChannel;

MOCHIE_DECLARE_TEX2D_NOSAMPLER(_HeightMask);
float4 _HeightMask_ST;
float2 _UVHeightMaskScroll;
float _UVHeightMaskRotation;
int _UVHeightMaskSet;
int _UVHeightMaskSwizzle;
int _HeightMaskChannel;

MOCHIE_DECLARE_TEX2D_NOSAMPLER(_RainMask);
float4 _RainMask_ST;
float2 _UVRainMaskScroll;
float _UVRainMaskRotation;
int _UVRainMaskSet;
int _UVRainMaskSwizzle;
int _RainMaskChannel;

MOCHIE_DECLARE_TEX2D_NOSAMPLER(_EmissionMask);
float4 _EmissionMask_ST;
float2 _UVEmissionMaskScroll;
float _UVEmissionMaskRotation;
int _UVEmissionMaskSet;
int _UVEmissionMaskSwizzle;
int _EmissionMaskChannel;

MOCHIE_DECLARE_TEX2D_NOSAMPLER(_AlphaMask);
float4 _AlphaMask_TexelSize;
float4 _AlphaMask_ST;
float2 _UVAlphaMaskScroll;
float _UVAlphaMaskRotation;
int _UVAlphaMaskSet;
int _UVAlphaMaskSwizzle;
int _AlphaMaskChannel;

// Specularity
float3 _SpecularOcclusionTint;
float _ReflectionStrength;
float _SpecularHighlightStrength;
float _FresnelStrength;
float _IndirectSpecularOcclusionStrength;
float _RealtimeSpecularOcclusionStrength;
float _SpecularOcclusionStrength;
float _SpecularOcclusionContrast;
float _SpecularOcclusionBrightness;
float _SpecularOcclusionHDR;
float _ContactHardening;
float _GSAAStrength;
int _SpecularOcclusionToggle;
int _FresnelToggle;
int _IgnoreRealtimeGI;
int _GSAAToggle;
int _ShadingModel;

int _SSRToggle;
int _VRSSR;
MOCHIE_DECLARE_TEX2D_SCREENSPACE(_CameraDepthTexture);
MOCHIE_DECLARE_TEX2D_SCREENSPACE(_GrabTexture);
MOCHIE_DECLARE_TEX2D(_NoiseTexSSR);
float4 _CameraDepthTexture_TexelSize;
float4 _GrabTexture_TexelSize;
float4 _NoiseTexSSR_TexelSize;
float _SSRStrength;
float _SSRHeight;
float _SSREdgeFade;

// Subsurface Scattering
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_ThicknessMap);
float4 _ThicknessMap_ST;
float3 _ScatterCol;
float _ThicknessMapPower;
float _ScatterAmbient;
float _ScatterIntensity;
float _ScatterPow;
float _ScatterDist; 
float _WrappingFactor;
int _ScatterBaseColorTint;
int _Subsurface;

// Filtering
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_ColorGradingLUT);
float4 _ColorGradingLUT_TexelSize;
int _SampleCustomLUT;
int _Filtering;
int _HueMode;
int _MonoTint;
float _Saturation;
float _Hue;
float _Contrast;
float _Brightness;
float _ACES;
float _SaturationDet;
float _HueDet;
float _ContrastDet;
float _BrightnessDet;
float _SaturationEmiss;
float _HueEmiss;
float _ContrastEmiss;
float _BrightnessEmiss;
float _HuePost;
float _SaturationPost;
float _BrightnessPost;
float _ContrastPost;
float _ColorGradingLUTStrength;

// Rain
sampler2D _RainSheet;
sampler2D _DropletMask;
int _UVRainSet;
int _UVRippleSet;
float _UVRainRotation;
int _UVRainSwizzle;
float _UVRippleRotation;
int _UVRippleSwizzle;
float _RippleStrength;
float2 _RippleScale;
float _RippleSpeed;
float2 _RainScale;
float _RainColumns;
float _RainRows;
float _RainSpeed;
float _DynamicDroplets;
float _RainBias;
float _RippleSize;
float _RippleDensity;
float _RainThreshold;
float _RainThresholdSize;
float _RainStrength;

// Render Settings
int _UnityFogToggle;
int _VertexBaseColor;
int _BAKERY_SHNONLINEAR;
float _BakeryLMSpecStrength;

// Debug Toggles
int _MaterialDebugMode;
int _DebugEnable;
int _DebugBaseColor;
int _DebugNormals;
int _DebugRoughness;
int _DebugMetallic;
int _DebugOcclusion;
int _DebugHeight;
int _DebugVertexColors;
int _DebugAtten;
int _DebugLighting;
int _DebugAlpha;
int _DebugReflections;
int _DebugSpecular;

// Outputs
float4 defaultSampler;
float2 parallaxOffset;
float flipbookBase;
float rainThreshold;
float rainStrength;

// Outputs for triplanar stuff
float3 worldVertexPos;
float3 localVertexPos;
float3 worldVertexNormal;
float3 localVertexNormal;

struct appdata {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float4 color : COLOR;
    float2 uv0 : TEXCOORD0;
    float2 uv1 : TEXCOORD1;
    float2 uv2 : TEXCOORD2;
    float2 uv3 : TEXCOORD3;
    float2 uv4 : TEXCOORD4;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {
    #if defined(SHADOWCASTER_PASS)
        V2F_SHADOW_CASTER;
    #else
        float4 pos : SV_POSITION;
    #endif
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float4 color : COLOR;
    float4 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 uv2 : TEXCOORD2;
    float4 uv3 : TEXCOORD3;
    float4 uv4 : TEXCOORD4;
    float4 lightmapUV : TEXCOORD5;
    float3 worldPos : TEXCOORD6;
    float3 localPos : TEXCOORD7;
    float3 localNorm : TEXCOORD8;
    #if defined(META_PASS)
        #if defined(EDITOR_VISUALIZATION)
            float2 vizUV    : TEXCOORD9;
            float4 lightCoord   : TEXCOORD10;
        #endif
    #else
        float4 grabUV : TEXCOORD11;
        bool vertexLightOn : TEXCOORD12;
        UNITY_SHADOW_COORDS(13)
        UNITY_FOG_COORDS(14)
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

struct LightingData {
    float3 reflectionCol;
    float3 specHighlightCol;
    float3 directCol;
    float3 indirectCol;
    float3 subsurfaceCol;
    float3 vLightCol;
    float3 lightCol;
    float3 lightDir;
    float3 specularTint;
    float3 reflAdjust;
    float3 viewDir;
    float2 screenUV;
    float atten;
    float NdotL;
    float VNdotL;
    float omr;
    float thickness;
    float3 lmSpec;
    float3 specularOcclusion;
    float3 areaLitSpecularity;
    float3 areaLitDiffuse;
    float3 ltcgiSpecularity;
    float3 ltcgiDiffuse;
    bool isRealtime;
};

struct InputData {
    float4 baseColor;
    float4 diffuse;
    float3 normal;
    float3 tsNormal;
    float3 vNormal;
    float4 roughness;
    float4 metallic;
    float4 occlusion;
    float4 height;
    float4 emission;
    float4 packedMap;
    float rainFlipbook;
    float alpha;
};

#include "StandardRain.cginc"
#include "StandardThirdParty.cginc"
#include "StandardInput.cginc"
#include "StandardParallax.cginc"
#include "StandardBakery.cginc"
#include "StandardLighting.cginc"
#include "StandardSSR.cginc"
#include "StandardBRDF.cginc"
#include "StandardVert.cginc"
#if defined(META_PASS)
    #include "StandardMeta.cginc"
#elif defined(SHADOWCASTER_PASS)
    #include "StandardShadow.cginc"
#else
    #include "StandardFrag.cginc"
#endif

#endif