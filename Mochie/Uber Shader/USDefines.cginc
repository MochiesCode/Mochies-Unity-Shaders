#ifndef US_DEFINES_INCLUDED
#define US_DEFINES_INCLUDED

#include "USKeyDefines.cginc"
#include "UnityPBSLighting.cginc"
#include "../Common/Color.cginc"
#include "../Common/Utilities.cginc"
#include "../Common/Noise.cginc"
#include "Autolight.cginc"

#if defined(SHADOWS_DEPTH) && !defined(SPOT)
	#define SHADOW_COORDS(idx1) unityShadowCoord2 _ShadowCoord : TEXCOORD##idx1;
#endif

UNITY_DECLARE_TEX2D(_MainTex); float4 _MainTex_ST, _MainTex_TexelSize;
UNITY_DECLARE_TEX2D_NOSAMPLER(_MetallicGlossMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecGlossMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_OcclusionMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailAlbedoMap); float4 _DetailAlbedoMap_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_RimTex); float4 _RimTex_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_DistortUVMap); float4 _DistortUVMap_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_AOTintTex);
UNITY_DECLARE_TEX2D_NOSAMPLER(_SmoothnessMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_Matcap); float4 _Matcap_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_Matcap1); float4 _Matcap1_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_MatcapBlendMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_ShadowMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_RimMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DiffuseMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_FilterMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_TeamColorMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_CubeBlendMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DistortUVMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_PulseMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_SubsurfaceMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_TranslucencyMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_SubsurfaceTex);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MatcapMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_AlphaMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_ERimMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_PackedMask0);
UNITY_DECLARE_TEX2D_NOSAMPLER(_PackedMask1);
UNITY_DECLARE_TEX2D_NOSAMPLER(_PackedMask2);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MirrorTex);

sampler2D _PackedMap;
sampler2D _ShadowRamp;
samplerCUBE _MainTexCube0, _MainTexCube1;

int _IsCubeBlendMask;
int _MaskingMode;
int _RenderMode, _BlendMode, _ZWrite, _ATM, _ColorPreservation, _UseAlphaMask;
int _CubeMode, _CubeBlendMode, _AutoRotate0, _AutoRotate1;
int _StaticLightDirToggle, _NonlinearSHToggle, _ClampAdditive;
int _ShadowMode, _RTSelfShadow, _AttenSmoothing, _ShadowDithering;
int _MatcapToggle, _MatcapBlending, _MatcapBlending1, _UnlitMatcap;
int _RimLighting, _RimBlending, _UnlitRim;
int _MirrorBehavior;
int _ClearCoat, _HardenNormals;
int _PBRWorkflow, _SourceAlpha;
int _MetallicChannel, _RoughnessChannel, _OcclusionChannel, _HeightChannel;
int _FilterModel, _AutoShift;
int _DistortMainUV, _DistortDetailUV, _DistortEmissUV, _DistortRimUV;
int _Subsurface, _PostFiltering;
int _EnvironmentRim, _ERimBlending, _ERimUseRough;
int _SRampTintAO, _UseSmoothMap, _PreviewSmooth;
int _PackedRoughPreview, _ShadowConditions, _DirectAO, _DistortUVs;
int _DistortionStyle, _IndirectAO, _NoiseOctaves, _UseMetallicMap, _UseSpecMap;
int _MatcapUseRough, _UseMatcap1, _UnlitMatcap1;
int _UseMirrorAlbedo, _DissolveStyle;
int _DistortMatcap0, _DistortMatcap1, _MatcapUseRough1, _Invert;
int _TeamFiltering, _GeomDissolveAxis, _GeomDissolveAxisFlip, _GeomDissolveWireframe;

float4 _Color; 
float4 _CubeColor0, _CubeColor1;
float4 _MatcapColor, _MatcapColor1;
float4 _RimCol, _ERimTint;
float4 _TeamColor0, _TeamColor1, _TeamColor2, _TeamColor3;
float4 _SColor, _ShadowTint;

float3 _CubeRotate0, _CubeRotate1;
float3 _StaticLightDir;
float3 _AOTint;
float3 _DissolveNoiseScale;
float3 _RGB;

float2 _MainTexScroll;
float2 _DetailScroll;
float2 _RimScroll;
float2 _DistortUVScroll;
float2 _NoiseScale;

float _Opacity, _Cutoff;
float _DistanceFadeMin, _DistanceFadeMax, _ClipRimStr, _ClipRimWidth;
float _CubeBlend;
float _BumpScale, _DetailNormalMapScale;
float _Metallic, _Glossiness, _GlossMapScale, _OcclusionStrength;
float _DirectCont, _IndirectCont, _DirectContSTD, _IndirectContSTD, _RTDirectCont, _RTIndirectCont, _VLightCont, _AdditiveMax;
float _ShadowStr, _RampWidth0, _RampWidth1, _RampWeight;
float _MatcapStr, _MatcapStr1;
float _RimStr, _RimWidth, _RimEdge;
float _SHStr, _DisneyDiffuse;
float _Saturation, _Brightness;
float _AutoShiftSpeed, _Hue;
float _Contrast, _HDR, _Noise;
float _DistortUVStr;
float _SPen, _SStr, _SSharp, _SAtten;
float _ERimWidth, _ERimStr, _ERimEdge, _ERimRoughness;
float _Value;
float _NoiseSpeed, _RampPos;
float _MatcapRough, _MatcapRough1;
float _GeomDissolveAmount, _GeomDissolveWidth, _GeomDissolveClip, _GeomDissolveSpread;

int _PreviewRough, _PreviewAO, _PreviewHeight;
int _AOFiltering, _HeightFiltering, _RoughnessFiltering, _SmoothnessFiltering;
float _AOLightness, _AOIntensity, _AOContrast;
float _RoughLightness, _RoughIntensity, _RoughContrast;
float _SmoothLightness, _SmoothIntensity, _SmoothContrast;

sampler3D _DitherMaskLOD;
sampler2D _DitherMaskLOD2D;

sampler2D _Spritesheet;
sampler2D _Spritesheet1;
int _UnlitSpritesheet, _UnlitSpritesheet1;
int _ManualScrub, _ScrubPos, _EnableSpritesheet, _SpritesheetBlending;
int _ManualScrub1, _ScrubPos1, _EnableSpritesheet1, _SpritesheetBlending1;
float4 _SpritesheetCol, _SpritesheetCol1;
float2 _RowsColumns;
float2 _FrameClipOfs;
float2 _SpritesheetScale, _SpritesheetPos;
float2 _RowsColumns1;
float2 _FrameClipOfs1;
float2 _SpritesheetScale1, _SpritesheetPos1;
float _SpritesheetRot, _FPS;
float _SpritesheetRot1, _FPS1;


UNITY_DECLARE_TEX2D_NOSAMPLER(_OutlineTex); float4 _OutlineTex_ST;
sampler2D _OutlineMask;
int _OutlineToggle, _ApplyOutlineLighting, _ApplyOutlineEmiss, _ApplyAlbedoTint, _UseVertexColor;
float4 _OutlineCol;
float2 _OutlineScroll;
float _OutlineThicc, _OutlineRange, _OutlineMult;


UNITY_DECLARE_TEX2D(_EmissionMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissMask);
float4 _EmissionMap_ST, _EmissionColor;
int _PulseToggle, _PulseWaveform, _ReactToggle, _CrossMode;
float _PulseSpeed, _PulseStr, _Crossfade, _ReactThresh;
float2 _EmissScroll;


UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMap); 
float _DetailNormalmapScale;


UNITY_DECLARE_TEX2D_NOSAMPLER(_ParallaxMap);
float _Parallax;
float _HeightLightness, _HeightIntensity, _HeightContrast;


UNITY_DECLARE_TEX2D_NOSAMPLER(_ReflectionMask);
sampler2D_float _CameraDepthTexture;
sampler2D _SSRGrab;
samplerCUBE _ReflCube;
sampler2D _NoiseTexSSR;
float4 _ReflCol, _ReflTex_ST, _NoiseTexSSR_TexelSize;
float _ReflectionStr, _ReflRough;
int _Reflections, _ReflUseRough, _ReflStepping, _ReflSteps;
int _Dith, _MaxSteps, _SSR, _LightingBasedIOR;
float4 _SSRGrab_TexelSize;
float4 _CameraDepthTexture_TexelSize;
float _Alpha, _Blur, _EdgeFade, _RTint, _LRad, _SRad, _Step;


UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_InterpMask);
int _Specular, _SharpSpecular, _SharpSpecStr, _SpecTermStep;
int _AnisoSteps, _AnisoLerp, _RippleInvert, _SpecUseRough, _ManualSpecBright;
float4 _SpecCol, _SpecTex_ST;
float3 _RippleSeeds;
float _AnisoAngleX, _AnisoAngleY, _AnisoLayerX, _AnisoLayerY;
float _SpecStr, _AnisoStr, _AnisoLayerStr, _RippleFrequency, _RippleAmplitude, _SpecRough;

int _DebugIntRange, _DebugToggle, _DebugEnum;
float _DebugFloat, _DebugRange;
float4 _DebugVector, _DebugColor, _DebugHDRColor;

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
float _NaNLmao;
float prevRough;
float prevSmooth;
float prevHeight;
float3 prevAO;
float3 uvOffsetOut;

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
	float3 vLightDir; 
    float3 viewDir;
    float3 halfVector; 
    float3 normalDir;
    float3 reflectionDir;
	float3 normal;
	float3 binormal;
	float4 tangent;
	float2 screenUVs;
	bool lightEnv;
	bool lightEnvFull;
};

struct masks {
    float reflectionMask;
    float specularMask;
	float matcapMask;
	float shadowMask;
	float subsurfMask;
	float diffuseMask;
	float matcapBlendMask;
	float rimMask;
	float eRimMask;
	float detailMask;
	float dissolveMask;
	float anisoMask;
	float filterMask;
	float emissMask;
	float4 teamMask;
};

struct appdata {
    float4 vertex : POSITION;
    float4 uv : TEXCOORD0;
	float4 uv1 : TEXCOORD1;
    float4 tangent : TANGENT;
    float3 normal : NORMAL;
	float4 color : COLOR;
};

// Define this here so the Uberx features sees it
#include "USSampling.cginc"

#if X_FEATURES

UNITY_DECLARE_TEX2D_NOSAMPLER(_DissolveMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DissolveTex); float4 _DissolveTex_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_DissolveRimTex); float4 _DissolveRimTex_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_DissolveFlow);
int _GeomFXToggle;
int _ShatterClones, _DissolveClones, _GlitchClones, _WFClones, _DFClones;
int _WireframeToggle, _WFMode;
int _GlitchToggle;
int _ShatterToggle;
int _CloneToggle, _ClonePattern, _ClonePosition, _SaturateEP;
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
float _Visibility, _CloneSize;

struct v2g {
	float4 pos : POSITION;
	centroid float4 uv : TEXCOORD0;
	centroid float4 uv1 : TEXCOORD1;
	centroid float4 uv2 : TEXCOORD2;
	centroid float4 uv3 : TEXCOORD3;
	centroid float4 rawUV : TEXCOORD4;
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
	centroid float4 rawUV : TEXCOORD4;
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
	float wfOpac : TEXCOORD16;

	float4 tangent : TANGENT;
	float3 normal : NORMAL;

	UNITY_SHADOW_COORDS(19)
	UNITY_FOG_COORDS(20)
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
	centroid float4 rawUV : TEXCOORD4;
	float4 worldPos : TEXCOORD5;
	float3 binormal : TEXCOORD6; 
	float3 tangentViewDir : TEXCOOR78;
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
#include "USBRDF.cginc"
#include "USLighting.cginc"
#include "USFunctions.cginc"
#include "USPass.cginc"

#endif