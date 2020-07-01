#ifndef US_DEFINES_INCLUDED
#define US_DEFINES_INCLUDED

#include "UnityPBSLighting.cginc"
#include "../Common/Color.cginc"
#include "../Common/Utilities.cginc"
#include "../Common/Noise.cginc"
#include "Autolight.cginc"

#if defined(SHADOWS_DEPTH) && !defined(SPOT)
	#define SHADOW_COORDS(idx1) unityShadowCoord2 _ShadowCoord : TEXCOORD##idx1;
#endif

UNITY_DECLARE_TEX2D(_MainTex); float4 _MainTex_ST, _MainTex_TexelSize;
UNITY_DECLARE_TEX2D_NOSAMPLER(_MirrorTex);
UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MetallicGlossMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecGlossMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_OcclusionMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_ParallaxMap);
UNITY_DECLARE_TEX2D(_EmissionMap); float4 _EmissionMap_ST, _EmissionMap_TexelSize;
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailAlbedoMap); float4 _DetailAlbedoMap_ST, _DetailAlbedoMap_TexelSize;
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMap); float4 _DetailNormalMap_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissMask); float4 _EmissMask_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_OutlineTex); float4 _OutlineTex_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_RimTex); float4 _RimTex_ST, _RimTex_TexelSize;
UNITY_DECLARE_TEX2D_NOSAMPLER(_DistortUVMap); float4 _DistortUVMap_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_ReflTex); float4 _ReflTex_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecTex); float4 _SpecTex_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_AOTintTex);
UNITY_DECLARE_TEX2D_NOSAMPLER(_SmoothnessMap);

UNITY_DECLARE_TEX2D_NOSAMPLER(_Matcap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_ShadowMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_ReflectionMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_InterpMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_RimMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DDMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_SmoothShadeMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_FilterMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_TeamColorMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_OutlineMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_CubeBlendMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DistortUVMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_PackedMask0);
UNITY_DECLARE_TEX2D_NOSAMPLER(_PackedMask1);
UNITY_DECLARE_TEX2D_NOSAMPLER(_PulseMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_SubsurfaceMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_TranslucencyMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_SubsurfaceTex);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MatcapMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_SpritesheetMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_AlphaMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_ERimMask);

samplerCUBE _MainTexCube0, _MainTexCube1, _ReflCube;
sampler3D _DitherMaskLOD;
sampler2D _ShadowRamp; float4 _ShadowRamp_TexelSize;
sampler2D _NoiseTexSSR; float4 _NoiseTexSSR_TexelSize;
sampler2D _PackedMap;
sampler2D _Spritesheet;
sampler2D _Spritesheet1;

int _Dith;
float _Alpha;
float _Blur;
float _EdgeFade;
half _RTint;
half _LRad;
half _SRad;
float _Step;
int _MaxSteps;
int _SSR;
int _IsCubeBlendMask;

sampler2D_float _CameraDepthTexture;
float4 _CameraDepthTexture_TexelSize;
sampler2D _SSRGrab;
float4 _SSRGrab_TexelSize;

int _MaskingToggle;
int _MaskingMode;
int _DetailMaskChannel;
int _ShadowMaskChannel;
int _ReflectionMaskChannel;
int _SpecularMaskChannel;
int _InterpMaskChannel;
int _RimMaskChannel;
int _DDMaskChannel;
int _SmoothShadeMaskChannel;
int _FilterMaskChannel;
int _TeamColorMaskChannel;
int _OutlineMaskChannel;
int _CubeBlendMaskChannel;
int _DistortUVMaskChannel;
int _EmissMaskChannel;
int _PulseMaskChannel;
int _SubsurfaceMaskChannel;
int _MatcapMaskChannel;
int _SpritesheetMaskChannel;
int _AlphaMaskChannel;
int _DissolveMaskChannel;
int _ERimMaskChannel;

int _RenderMode, _BlendMode, _ZWrite, _ATM, _ColorPreservation, _UseAlphaMask;
int _CubeMode, _CubeBlendMode, _UnlitCube, _AutoRotate0, _AutoRotate1;
int _StaticLightDirToggle, _NonlinearSHToggle, _ClampAdditive;
int _Shadows, _ShadowMode, _RTSelfShadow, _AttenSmoothing, _ShadowDithering;
int _Reflections, _UseReflCube;
int _Specular, _SharpSpecular, _SpecularStyle, _AnisoLerp;
int _MatcapToggle, _MatcapBlending, _UnlitMatcap;
int _RimLighting, _RimBlending, _UnlitRim;
int _RoughnessFiltering, _MirrorBehavior;
int _ClearCoat, _HardenNormals;
int _PBRWorkflow, _SourceAlpha;
int _MetallicChannel, _RoughnessChannel, _OcclusionChannel, _HeightChannel;
int _PulseToggle, _PulseWaveform, _ReactToggle, _CrossMode;
int _FilterModel, _AutoShift, _TeamColorsToggle;
int _DistortMainUV, _DistortDetailUV, _DistortEmissUV, _DistortRimUV;
int _Outline, _ApplyOutlineLighting, _ApplyOutlineEmiss;
int _Subsurface, _PostFiltering;
int _ManualScrub, _ScrubPos, _EnableSpritesheet, _UnlitSpritesheet, _SpritesheetBlending;
int _ManualScrub1, _ScrubPos1, _EnableSpritesheet1, _SpritesheetBlending1;
int _AOFiltering, _HeightFiltering;
int _UseSpecTex, _UseReflTex, _EnvironmentRim, _ERimBlending, _ERimUseRough;
int _SRampTintAO, _PreviewActive, _UseAOTintTex, _UseSmoothMap, _LinearSmooth, _PreviewSmooth;
int _SmoothnessFiltering, _PackedRoughPreview, _ShadowConditions, _DirectAO, _UseRimTex, _DistortUVs;
int _DistortionStyle, _PreviewNoise, _IndirectAO, _NoiseOctaves, _UseMetallicMap, _UseSpecMap;
int _UseDetailNormal, _UseParallaxMap, _EmissionToggle;
int _MatcapUseRough, _ReflUseRough, _SpecUseRough, _UseNormalMap;
int _UseMirrorAlbedo, _DissolveStyle;

float4 _Color; 
float4 _CubeColor0, _CubeColor1;
float4 _SpecCol, _ReflCol, _MatcapColor;
float4 _RimCol, _ERimTint, _EmissionColor, _OutlineCol;
float4 _TeamColor0, _TeamColor1, _TeamColor2, _TeamColor3;
float4 _SColor, _ShadowTint;
float4 _SpritesheetCol, _SpritesheetCol1;

float3 _CubeRotate0, _CubeRotate1;
float3 _StaticLightDir;
float3 _AOTint;
float3 _DissolveNoiseScale;

float2 _MainTexScroll;
float2 _EmissScroll;
float2 _DetailScroll;
float2 _RimScroll;
float2 _OutlineScroll;
float2 _DistortUVScroll;
float2 _RowsColumns;
float2 _FrameClipOfs;
float2 _SpritesheetScale, _SpritesheetPos;
float2 _RowsColumns1;
float2 _FrameClipOfs1;
float2 _SpritesheetScale1, _SpritesheetPos1;
float2 _NoiseScale;

float _Opacity, _Cutoff, _OutlineThicc, _OutlineRange;
float _DistanceFadeMin, _DistanceFadeMax, _ClipRimStr, _ClipRimWidth;
float _CubeBlend;
float _BumpScale, _DetailNormalMapScale;
float _Metallic, _Glossiness, _GlossMapScale, _OcclusionStrength, _Parallax;
float _DirectCont, _IndirectCont, _DirectContSTD, _IndirectContSTD, _RTDirectCont, _RTIndirectCont, _VLightCont, _AdditiveMax;
float _ShadowStr, _RampWidth0, _RampWidth1, _RampWeight, _ShadowDitherStr;
float _ReflectionStr;
float _SpecStr, _AnisoLayerStr, _AnisoAngleX, _AnisoAngleY, _AnisoLayerX, _AnisoLayerY;
float _MatcapStr;
float _RimStr, _RimWidth, _RimEdge;
float _RoughLightness, _RoughIntensity, _RoughContrast;
float _SHStr, _DisneyDiffuse;
float _PulseSpeed, _PulseStr, _Crossfade, _ReactThresh;
float _Saturation, _Brightness, _RAmt, _GAmt, _BAmt;
float _AutoShiftSpeed, _Hue, _Luminance, _HSLMin, _HSLMax;
float _Contrast, _HDR, _Noise;
float _DistortUVStr;
float _SPen, _SStr, _SSharp, _SAtten;
float _SpritesheetRot, _FPS;
float _SpritesheetRot1, _FPS1;
float _AOLightness, _AOIntensity, _AOContrast;
float _HeightLightness, _HeightIntensity, _HeightContrast;
float _NaNLmao;
float _ERimWidth, _ERimStr, _ERimEdge, _ERimRoughness;
float _SmoothLightness, _SmoothIntensity, _SmoothContrast;
float _Value;
float _NoiseSpeed, _RampPos, _SharpSpecStr;
float _MatcapRough, _SpecRough, _ReflRough;

// int _DebugIntRange, _DebugToggle, _DebugEnum;
// float _DebugFloat, _DebugRange;
// float4 _DebugVector, _DebugColor, _DebugHDRColor;

// Outputs
float4 spec;
float3 specularTint;
float3 uvOffset;
float omr;
float metallic;
float roughness;
float smoothness;
float occlusion;
float height; 
float cubeMask;

int _PreviewRough, _PreviewAO, _PreviewHeight;

struct lighting {
    float NdotL;
	float NdotV;
	float NdotH;
	float TdotH;
	float BdotH;
	float LdotH;
    float3 ao;
	float3 sRamp;
    float worldBrightness;
    float3 directCol;
    float3 indirectCol;
	float3 vLightCol;
    float3 lightDir; 
    float3 viewDir; 
    float3 halfVector; 
    float3 normalDir;
    float3 reflectionDir;
	float3 normal;
	float3 binormal;
	float4 tangent;
	float4 toLightX;
	float4 toLightY;
	float4 toLightZ;
	float4 lengthSq;
	float4 vLightWeight;
	float4 vLightAtten;
	float2 screenUVs;
	bool lightEnv;
	bool lightEnvFull;
};

struct masks {
    float reflectionMask;
    float specularMask;
	float eRimMask;
	float detailMask;
	float shadowMask;
	float rimMask;
	float matcapMask;
	float dissolveMask;
	float ddMask;
	float anisoMask;
	float smoothMask;
	float spriteMask;
	float filterMask;
};

struct appdata {
    float4 vertex : POSITION;
    float4 uv : TEXCOORD0;
	float4 uv1 : TEXCOORD1;
    float4 tangent : TANGENT;
    float3 normal : NORMAL;
};


// Define this here so the Uberx features sees it
#include "USSampling.cginc"

#if defined(UBERX)

UNITY_DECLARE_TEX2D_NOSAMPLER(_DissolveMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DissolveTex); float4 _DissolveTex_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_DissolveRimTex); float4 _DissolveRimTex_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_DissolveFlow);
int _GeomFXToggle, _DisguiseMain;
int _WireframeToggle, _WFMode;
int _GlitchToggle;
int _ShatterToggle;
int _ClonePattern, _ClonePosition, _SaturateEP;
int _Screenspace;
int _DistanceFadeToggle;
int _ShowInMirror, _ShowBase, _Connected;
int _DissolveToggle, _DissolveChannel, _DissolveWave, _DissolveBlending;
float4 _Clone1, _Clone2, _Clone3, _Clone4, _Clone5, _Clone6, _Clone7, _Clone8;
float4 _DissolveRimCol;
float4 _WFColor;
float4 _ClipRimColor;
float3 _EntryPos;
float3 _BaseOffset, _BaseRotation;
float3 _ReflOffset, _ReflRotation;
float3 _Position, _Rotation;
float2 _DissolveScroll0; 
float2 _DissolveScroll1;
float _Range;
float _DissolveAmount, _DissolveRimWidth, _DissolveBlendSpeed;
float _WFFill, _WFVisibility;
float _ShatterMax, _ShatterMin, _ShatterSpread, _ShatterCull; 
float _Instability, _GlitchFrequency, _GlitchIntensity, _PosPrecision, _PatternMult; 
float _Visibility, _CloneSpacing, _CloneSize;

struct v2g {
	float4 pos : POSITION;
	centroid float4 uv : TEXCOORD0;
	centroid float4 uv1 : TEXCOORD1;
	centroid float4 uv2 : TEXCOORD2;
	centroid float4 uv3 : TEXCOORD3;
	centroid float4 uv4 : TEXCOORD4;
	float4 worldPos : TEXCOORD5;
	float3 binormal : TEXCOORD6; 
	float3 tangentViewDir : TEXCOORD7;
	float3 cameraPos : TEXCOORD8;
	float3 objPos : TEXCOORD9;
	float4 screenPos : TEXCOORD10;
	bool isReflection : TEXCOORD11;
	float3 localPos : TEXCOORD12;

	float4 tangent : TANGENT;
	float3 normal : NORMAL;

	UNITY_SHADOW_COORDS(15)
	UNITY_FOG_COORDS(16)
};

struct g2f {
	float4 pos : SV_POSITION;
	centroid float4 uv : TEXCOORD0;
	centroid float4 uv1 : TEXCOORD1;
	centroid float4 uv2 : TEXCOORD2;
	centroid float4 uv3 : TEXCOORD3;
	centroid float4 uv4 : TEXCOORD4;
	float4 worldPos : TEXCOORD5;
	float3 binormal : TEXCOORD6; 
	float3 tangentViewDir : TEXCOORD7;
	float3 cameraPos : TEXCOORD8;
	float3 objPos : TEXCOORD9;
	float3 bCoords : TEXCOORD10;
	float WFStr : TEXCOORD11;
	uint instID : TEXCOORD12;
	float4 screenPos : TEXCOORD13;
	bool isReflection : TEXCOORD14;
	float3 localPos : TEXCOORD15;

	float4 tangent : TANGENT;
	float3 normal : NORMAL;

	UNITY_SHADOW_COORDS(18)
	UNITY_FOG_COORDS(19)
};

#include "USXFeatures.cginc"

#else

#define v2g v2f
#define g2f v2f

struct v2f {
	float4 pos : SV_POSITION;
	centroid float4 uv : TEXCOORD0;
	centroid float4 uv1 : TEXCOORD1;
	centroid float4 uv2 : TEXCOORD2;
	centroid float4 uv3 : TEXCOORD3;
	centroid float4 uv4 : TEXCOORD4;
	float4 worldPos : TEXCOORD5;
	float3 binormal : TEXCOORD6; 
	float3 tangentViewDir : TEXCOORD7;
	float3 cameraPos : TEXCOORD8;
	float3 objPos : TEXCOORD9;
	float4 screenPos : TEXCOORD10;
	bool isReflection : TEXCOORD11;
	float3 localPos : TEXCOORD12;

	float4 tangent : TANGENT;
	float3 normal : NORMAL;

	UNITY_SHADOW_COORDS(15)
	UNITY_FOG_COORDS(16)
};
#endif

#include "USSSR.cginc"
#include "USLighting.cginc"
#include "USFunctions.cginc"
#include "USPass.cginc"

#endif