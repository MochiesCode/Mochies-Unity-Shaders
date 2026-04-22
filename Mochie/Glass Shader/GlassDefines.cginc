#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"
#include "../Common/Sampling.cginc"
#include "../Common/Color.cginc"

#define AREALIT_ENABLED defined(_AREALIT_ON)
#define LTCGI_ENABLED defined(LTCGI)
#if AREALIT_ENABLED
    #include "../../AreaLit/Shader/Lighting.hlsl"
#endif
#define EPSILON 1.192092896e-07
#define GRABPASS_ENABLED defined(_GRABPASS_ON) && !defined(SHADER_API_MOBILE)
#define IS_OPAQUE !GRABPASS_ENABLED && !defined(_PREMULTIPLIED_ON)

MOCHIE_DECLARE_TEX2D_SCREENSPACE(_GlassGrab);
float4 _GlassGrab_TexelSize;
sampler2D _RainSheet;
sampler2D _MainTex;
sampler2D _RoughnessMap;
sampler2D _OcclusionMap;
sampler2D _MetallicMap;
sampler2D _NormalMap;
sampler2D _RainMask;
sampler2D _DropletMask;
sampler2D _EmissionMap;
float4 _EmissionMap_ST;
float4 _RainPackedMap_ST;
float4 _RainMask_ST;
float4 _RoughnessMap_ST;
float4 _OcclusionMap_ST;
float4 _MetallicMap_ST;
float4 _NormalMap_ST;
float4 _RainSheet_ST;
float4 _MainTex_ST;
float4 _BaseColorTint;
float4 _SpecularityTint;
float4 _GrabpassTint;
float4 _EmissionColor;
float4 _EmissionBlurStrength;
float _EmissionStrength;
float _EmissionParallaxDepth;
float _NormalStrength;
float _Roughness;
float _Metallic;
float _Occlusion;
float _Rows, _Columns;
float _XScale, _YScale;
float _Strength, _Speed;
float _Refraction;
float _Blur;
float _RainMaskChannel;
float _RippleStrength;
float _RippleScale;
float _RippleSpeed;
float _DynamicDroplets;
float _RainBias;
float _RippleSize;
float _RippleDensity;
float _RainThreshold;
float _RainThresholdSize;
float _TexCoordSpace;
float _TexCoordSpaceSwizzle;
float _GlobalTexCoordScale;
float _RefractionIOR;
float _RefractVertexNormal;
float _Test;
float _GSAAToggle;
float _GSAAStrength;
float _SpecularStrength;
float _ReflectionStrength;
float _SSREdgeFade;
float _SSRHeight;
float _SSRStrength;

int _LightmappedSpecularity;
int _BAKERY_SHNONLINEAR;
int _IgnoreRealtimeGI;
float _BakeryLMSpecStrength;
float _LightmapDistortion;
float _IndirectStrength;
float _IndirectSaturation;

float4 _LTCGI_DiffuseColor;
float4 _LTCGI_SpecularColor;
float _LTCGIStrength;
float _LTCGIRoughness;

float _AreaLitStrength;
float _AreaLitRoughnessMult;
float4 _AreaLitMask_ST;

float rainStrength;

#if defined(_SSR_ON)
    MOCHIE_DECLARE_TEX2D_SCREENSPACE(_CameraDepthTexture);
    MOCHIE_DECLARE_TEX2D(_NoiseTexSSR);
    float4 _CameraDepthTexture_TexelSize;
    float4 _NoiseTexSSR_TexelSize;
#endif

struct appdata {
    float4 vertex : POSITION;
    float4 uv : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 uv2 : TEXCOORD2;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {
    float4 pos : SV_POSITION;
    float4 uv : TEXCOORD0;
    float4 uvGrab : TEXCOORD1;
    float3 worldPos : TEXCOORD2;
    float3 binormal : TEXCOORD3;
    float4 localPos : TEXCOORD4;
    float3 cameraPos : TEXCOORD5;
    float4 lightmapUV : TEXCOORD6;
    float3 tangentViewDir : TEXCOORD7;
    float3 normal : NORMAL;
    float4 tangent: TANGENT;
    #if defined(EDITOR_VISUALIZATION)
        float2 vizUV    : TEXCOORD18;
        float4 lightCoord   : TEXCOORD19;
    #endif
    UNITY_FOG_COORDS(10)
    UNITY_SHADOW_COORDS(11)
    UNITY_VERTEX_INPUT_INSTANCE_ID 
    UNITY_VERTEX_OUTPUT_STEREO
};

#include "../Common/Sampling.cginc"
#include "../Common/Utilities.cginc"
#include "../Common/LightVolumes.cginc"
#if defined(_SSR_ON)
    #include "GlassSSR.cginc"
#endif
#include "GlassIndirect.cginc"
#include "GlassFunctions.cginc"