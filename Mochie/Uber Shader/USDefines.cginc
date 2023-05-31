#ifndef US_DEFINES_INCLUDED
#define US_DEFINES_INCLUDED

#include "USKeyDefines.cginc"
#include "UnityPBSLighting.cginc"
#include "../Common/Sampling.cginc"
#include "../Common/Color.cginc"
#include "../Common/Utilities.cginc"
#include "../Common/Noise.cginc"
#include "AutoLight.cginc"

MOCHIE_DECLARE_TEX2D(_MainTex);
float4 _MainTex_ST, _MainTex_TexelSize;

#if defined(SHADOWS_DEPTH) && !defined(SPOT)
	#define SHADOW_COORDS(idx1) unityShadowCoord2 _ShadowCoord : TEXCOORD##idx1;
#endif

#ifdef POINT
	#define POI_LIGHT_ATTENUATION(destName, shadows, input, worldPos) \
		unityShadowCoord3 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1)).xyz; \
		float shadows = UNITY_SHADOW_ATTENUATION(input, worldPos); \
		float destName = tex2D(_LightTexture0, dot(lightCoord, lightCoord).rr).r;
#endif

#ifdef POINT_COOKIE
	#if !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
		#define DECLARE_LIGHT_COORD(input, worldPos) unityShadowCoord3 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1)).xyz
	#else
		#define DECLARE_LIGHT_COORD(input, worldPos) unityShadowCoord3 lightCoord = input._LightCoord
	#endif
	#define POI_LIGHT_ATTENUATION(destName, shadows, input, worldPos) \
        DECLARE_LIGHT_COORD(input, worldPos); \
        float shadows = UNITY_SHADOW_ATTENUATION(input, worldPos); \
        float destName = tex2D(_LightTextureB0, dot(lightCoord, lightCoord).rr).r * texCUBE(_LightTexture0, lightCoord).w;
#endif

#ifdef SPOT
	#if !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
		#define DECLARE_LIGHT_COORD(input, worldPos) unityShadowCoord4 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1))
	#else
		#define DECLARE_LIGHT_COORD(input, worldPos) unityShadowCoord4 lightCoord = input._LightCoord
	#endif
	#define POI_LIGHT_ATTENUATION(destName, shadows, input, worldPos) \
		DECLARE_LIGHT_COORD(input, worldPos); \
		float shadows = UNITY_SHADOW_ATTENUATION(input, worldPos); \
		float destName = (lightCoord.z > 0) * UnitySpotCookie(lightCoord) * UnitySpotAttenuate(lightCoord.xyz);
#endif

// PBR Shading
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_MainTex2);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_MirrorTex);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_PackedMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_MetallicGlossMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_SpecGlossMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_BumpMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_OcclusionMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_SmoothnessMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_ParallaxMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_ThicknessMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailAlbedoMap); 
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailRoughnessMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailOcclusionMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_ScatterTex);

// NPR Shading
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_RimTex); 
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_AOTintTex);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_Matcap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_Matcap1);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_MatcapNormal0);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_MatcapNormal1);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailShadowMap);

// FX
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_BCNoiseTex);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_OutlineTex);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DissolveTex); 
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DissolveFlow);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DistortUVMap);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_Spritesheet);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_Spritesheet1);

// Masks
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_MatcapBlendMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_EmissMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DetailMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_ShadowMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_RimMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DiffuseMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_FilterMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_TeamColorMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_CubeBlendMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DistortUVMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_PulseMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_SubsurfaceMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_MatcapMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_AlphaMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_ERimMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_RefractionMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_RefractionDissolveMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_SpecularMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_InterpMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_ReflectionMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_DissolveMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_OutlineMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_NearClipMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_VertexExpansionMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_VertexRoundingMask);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_PackedMask0);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_PackedMask1);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_PackedMask2);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_PackedMask3);
MOCHIE_DECLARE_TEX2D_NOSAMPLER(_IridescenceMask);

// Requires dedicated samplers
MOCHIE_DECLARE_TEX2D(_EmissionMap);
MOCHIE_DECLARE_TEX2D(_EmissionMap2);
MOCHIE_DECLARE_TEX2D(_ShadowRamp);
MOCHIE_DECLARE_TEX2D(_NoiseTexSSR);
MOCHIE_DECLARE_TEX2D(_AudioLinkTriOffsetMask);
MOCHIE_DECLARE_TEX2D(_AudioLinkWireframeMask);
MOCHIE_DECLARE_TEX2DARRAY(_Flipbook0);
MOCHIE_DECLARE_TEX2DARRAY(_Flipbook1);
MOCHIE_DECLARE_TEX2D_SCREENSPACE(_CameraDepthTexture);
MOCHIE_DECLARE_TEX2D_SCREENSPACE(_MUSGrab);

samplerCUBE _MainTexCube0, _MainTexCube1;
samplerCUBE _ReflCube;
sampler2D _DitherMaskLOD2D;
sampler3D _DitherMaskLOD;

float4 _RimTex_ST;
float4 _DetailAlbedoMap_ST;
float4 _DistortUVMap_ST;
float4 _Matcap_ST;
float4 _Matcap1_ST;
float4 _RefractionMask_ST;
float4 _AlphaMask_ST;
float4 _ReflectionMask_ST;
float4 _SpecularMask_ST;
float4 _ERimMask_ST;
float4 _MatcapMask_ST;
float4 _MatcapBlendMask_ST;
float4 _InterpMask_ST;
float4 _SubsurfaceMask_ST;
float4 _RimMask_ST;
float4 _DetailMask_ST;
float4 _ShadowMask_ST;
float4 _DiffuseMask_ST;
float4 _FilterMask_ST;
float4 _TeamColorMask_ST;
float4 _EmissMask_ST;
float4 _EmissPulseMask_ST;
float4 _EmissionMap2_ST;
float4 _RefractionDissolveMask_ST;
float4 _OutlineMask_ST;
float4 _BCNoiseTex_ST;
float4 _MainTex2_ST;
float4 _OutlineTex_ST;
float4 _ReflTex_ST;
float4 _EmissionMap_ST;
float4 _SpecTex_ST;
float4 _DissolveTex_ST;
float4 _MUSGrab_TexelSize;
float4 _AudioLinkTriOffsetMask_ST;
float4 _AudioLinkWireframeMask_ST;
float4 _MatcapNormal0_ST;
float4 _MatcapNormal1_ST;

int _Hide;
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
int _DissolveStyle;
int _DistortMatcap0, _DistortMatcap1, _MatcapUseRough1, _Invert;
int _TeamFiltering, _UVSec, _DetailRoughBlending;
int _SpecBiasOverrideToggle, _IgnoreFilterMask;
int _SpritesheetMode0, _SpritesheetMode1;
int _DitheredShadows;
int _UsingDetailRough, _UsingDetailOcclusion;
int _DetailOcclusionBlending;
int _UsingDetailAlbedo, _DetailAlbedoBlending;
int _NearClipToggle;
int _BCDissolveToggle;
int _AlphaMaskChannel;
int _AudioLinkEmissionBand;
int _AudioLinkRimBand;
int _AudioLinkDissolveBand;
int _AudioLinkBCDissolveBand;
int _AudioLinkVertManipBand;
int _AudioLinkToggle;
int _AudioLinkTriOffsetCoords;
int _AudioLinkTriOffsetCoordInvert;
int _AudioLinkWireframeBand;
int _AudioLinkWireframeCoords;
int _AudioLinkWireframeCoordInvert;
int _AudioLinkTriOffsetMode;
int _AudioLinkWireframeMode;
int _MatcapCenter, _MatcapCenter1;
int _Iridescence;
int _RefractionBlur;
int _RefractionBlurRough;
int _AudioLinkOutlineBand;
int _UseOutlineTexAlpha;
int _MatcapNormal0Toggle;
int _MatcapNormal1Toggle;
int _MatcapNormal0Mix;
int _MatcapNormal1Mix;

float _RefractionBlurStrength;
float _IridescenceStrength;
float _IridescenceHue;
float _IridescenceWidth;
float _IridescenceEdge;
float _IridescenceCurl;
float _IridescenceCurlScale;

float4 _Color; 
float4 _CubeColor0, _CubeColor1;
float4 _MatcapColor, _MatcapColor1;
float4 _RimCol, _ERimTint;
float4 _TeamColor0, _TeamColor1, _TeamColor2, _TeamColor3;
float4 _SColor, _ShadowTint;
float4 _BCRimCol, _BCColor;
float4 _AudioLinkWireframeColor;

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
float2 _RefractionMaskScroll;
float2 _ReflectionMaskScroll;
float2 _SpecularMaskScroll;
float2 _ERimMaskScroll;
float2 _MatcapMaskScroll;
float2 _MatcapBlendMaskScroll;
float2 _InterpMaskScroll;
float2 _SubsurfaceMaskScroll;
float2 _RimMaskScroll;
float2 _DetailMaskScroll;
float2 _ShadowMaskScroll;
float2 _DiffuseMaskScroll;
float2 _FilterMaskScroll;
float2 _TeamColorMaskScroll;
float2 _EmissMaskScroll;
float2 _EmissPulseMaskScroll; 
float2 _RefractionDissolveMaskScroll;
float2 _OutlineMaskScroll;
float2 _EmissScroll;
float2 _OutlineScroll;
float2 _DissolveScroll0; 
float2 _DissolveScroll1;
float2 _AudioLinkTriOffsetMaskScroll;
float2 _AudioLinkWireframeMaskScroll;
float2 _MatcapNormal0Scroll;
float2 _MatcapNormal1Scroll;

float2 _FrameClipOfs;
float2 _SpritesheetScale;
float2 _SpritesheetPos;
float2 _RowsColumns1;
float2 _FrameClipOfs1;
float2 _SpritesheetScale1;
float2 _SpritesheetPos1;
float2 _RowsColumns;
float2 _Flipbook0Scroll;
float2 _Flipbook1Scroll;
int _Flipbook0ClipEdge;
int _Flipbook1ClipEdge;

float _Opacity, _Cutoff;
float _RefractionDissolveMaskStr;
float _DistanceFadeMin, _DistanceFadeMax, _ClipRimStr, _ClipRimWidth;
float _CubeBlend;
float _BumpScale, _DetailNormalMapScale, _DetailRoughStrength;
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
float _ERimWidth, _ERimStr, _ERimEdge, _ERimRoughness;
float _Value;
float _NoiseSpeed, _RampPos;
float _MatcapRough, _MatcapRough1;
float _SpecBiasOverride;
float _DetailOcclusionStrength, _DetailAlbedoStrength;
float _NearClip;
float _BCRimWidth, _BCDissolveStr;
float _AudioLinkVertManipMultiplier;
float _AudioLinkRimWidth;
float _AudioLinkRimPulse;
float _AudioLinkRimPulseWidth;
float _AudioLinkTriOffsetSize;
float _AudioLinkTriOffsetStartPos;
float _AudioLinkWireframeStartPos;
float _AudioLinkTriOffsetEndPos;
float _AudioLinkWireframeEndPos;
float _AudioLinkWireframeSize;
float _AudioLinkWireframeStrength;
float _AudioLinkWireframeFalloff;
float _AudioLinkTriOffsetFalloff;
float _AudioLinkStrength;
float _AudioLinkOutlineMultiplier;
float _AudioLinkRemapOutlineMax, _AudioLinkRemapOutlineMin;
float _MatcapNormal0Str;
float _MatcapNormal1Str;
float _ACES;

float3 _VertexRotation;
float3 _VertexPosition;

int _PreviewRough, _PreviewAO, _PreviewHeight;
int _AOFiltering, _HeightFiltering, _RoughnessFiltering, _SmoothnessFiltering;
float _AOLightness, _AOIntensity, _AOContrast;
float _AORemapMin, _AORemapMax;
float _RoughLightness, _RoughIntensity, _RoughContrast;
float _RoughRemapMin, _RoughRemapMax;
float _SmoothLightness, _SmoothIntensity, _SmoothContrast;
float _SmoothRemapMin, _SmoothRemapMax;
float _HeightRemapMin, _HeightRemapMax;

int _UnlitSpritesheet, _UnlitSpritesheet1, _UseSpritesheetAlpha;
int _ManualScrub, _ScrubPos, _EnableSpritesheet, _SpritesheetBlending;
int _ManualScrub1, _ScrubPos1, _EnableSpritesheet1, _SpritesheetBlending1;
float4 _SpritesheetCol, _SpritesheetCol1;

float _SpritesheetRot, _FPS;
float _SpritesheetRot1, _FPS1;
float _SpritesheetBrightness, _SpritesheetBrightness1;

int _OutlineToggle, _ApplyOutlineLighting, _ApplyOutlineEmiss, _ApplyAlbedoTint, _UseVertexColor;
float4 _OutlineCol;
float _OutlineThicc, _OutlineRange, _OutlineMult;
float4 _EmissionColor;
int _PulseToggle, _PulseWaveform, _ReactToggle, _CrossMode;
float _PulseSpeed, _PulseStr, _Crossfade, _ReactThresh;

float4 _EmissionColor2;
float _EmissIntensity2;
float2 _EmissScroll2;
float _EmissIntensity;
float _DetailNormalmapScale;
float _Parallax;
float _ParallaxOffset;
float _ParallaxSteps;
float _HeightLightness, _HeightIntensity, _HeightContrast;
float _AudioLinkEmissionMultiplier;
float _AudioLinkDissolveMultiplier;
float _AudioLinkBCDissolveMultiplier;

float4 _ReflCol, _ReflCube_HDR, _NoiseTexSSR_TexelSize;
float _ReflectionStr, _ReflRough;
int _Reflections, _ReflUseRough, _ReflStepping, _ReflSteps;
int _Dith, _MaxSteps, _SSR, _LightingBasedIOR;
float4 _CameraDepthTexture_TexelSize;
float _Alpha, _Blur, _EdgeFade, _RTint, _LRad, _SRad, _Step;
float _AudioLinkRimMultiplier;
float _AudioLinkRimPulseSharp;

int _Specular, _SharpSpecular, _SharpSpecStr, _SpecTermStep, _UVAniso, _RealtimeSpec;
int _AnisoSteps, _AnisoLerp, _SpecUseRough, _ManualSpecBright;
float4 _SpecCol;
float _AnisoAngleX, _AnisoAngleY, _AnisoLayerX, _AnisoLayerY;
float _SpecStr, _AnisoStr, _AnisoLayerStr, _RippleFrequency, _RippleAmplitude, _SpecRough, _RippleStrength;
float _RippleContinuity;

int _Refraction, _UnlitRefraction, _RefractionCA;
float3 _RefractionTint;
float _RefractionOpac, _RefractionIOR, _RefractionCAStr;
int _GSAA;
int _VertexExpansionClamp;
float3 _VertexExpansion;
float _VertexRounding, _VertexRoundingPrecision;

int _ScatterBaseColorTint;
float4 _ScatterCol;
float _ThicknessMapPower;
float _ScatterIntensity;
float _ScatterPow;
float _ScatterDist;
float _ScatterAmbient;
float _ScatterWrap;

int _FresnelToggle;
float _FresnelStrength;

float _VertexColor;

int _AudioLinkUVDistortionBand;
float _AudioLinkUVDistortionMultiplier;
float _AudioLinkRemapMin;
float _AudioLinkRemapMax;
float _AudioLinkRemapEmissionMin;
float _AudioLinkRemapEmissionMax;
float _AudioLinkRemapRimMin;
float _AudioLinkRemapRimMax;
float _AudioLinkRemapDissolveMin;
float _AudioLinkRemapDissolveMax;
float _AudioLinkRemapBCDissolveMin;
float _AudioLinkRemapBCDissolveMax;
float _AudioLinkRemapVertManipMin;
float _AudioLinkRemapVertManipMax;
float _AudioLinkRemapTriOffsetMin;
float _AudioLinkRemapTriOffsetMax;
float _AudioLinkRemapWireframeMin;
float _AudioLinkRemapWireframeMax;
float _AudioLinkRemapUVDistortionMin;
float _AudioLinkRemapUVDistortionMax;
float _AudioLinkRemapRimPulseMin;
float _AudioLinkRemapRimPulseMax;

float _OscilloscopeStrength;
float4 _OscilloscopeCol;
float2 _OscilloscopeScale, _OscilloscopeOffset;
float _OscilloscopeRot;
float2 _OscilloscopeMarginLR, _OscilloscopeMarginTB;

float _MainTexRot, _DetailRot;
float _AlphaStrength;

int _MetallicFiltering, _PreviewMetallic;
float _MetallicIntensity, _MetallicContrast, _MetallicLightness;
float _MetallicRemapMin, _MetallicRemapMax;

// Outputs
float4 packedTex;
float4 spec;
float3 specularTint;
float3 uvOffset;
float omr;
float metallic;
float roughness;
float smoothness;
float occlusion;
float height; 
float curvature;
float cubeMask;
float _NaNLmao;
float prevRough;
float prevSmooth;
float prevHeight;
float prevMetal;
float prevCurve;
float audioLink;
float3 bcRimColor;
float3 prevAO;
float3 uvOffsetOut;

struct lighting {
    float NdotL;
	float NdotV;
	float NdotH;
	float TdotH;
	float BdotH;
	float LdotH;
	float VdotL;
	float VVRdotL;
    float3 ao;
	float3 sRamp;
    float worldBrightness;
    float3 directCol;
    float3 indirectCol;
	float3 vLightCol;
    float3 lightDir;
	float3 vLightDir; 
    float3 viewDir;
	float3 viewDirVR;
    float3 halfVector; 
    float3 normalDir;
    float3 reflectionDir;
	float3 normal;
	float3 normalMesh;
	float3 binormal;
	float4 tangent;
	float2 screenUVs;
	bool lightEnv;
	bool lightEnvFull;
};

struct masks {
    float reflectionMask;
    float specularMask;
	float matcapPrimMask;
	float shadowMask;
	float subsurfMask;
	float diffuseMask;
	float matcapSecMask;
	float rimMask;
	float eRimMask;
	float detailMask;
	float dissolveMask;
	float anisoMask;
	float filterMask;
	float emissMask;
	float emissPulseMask;
	float refractMask;
	float refractDissolveMask;
	float iridescenceMask;
	float4 teamMask;
};

struct audioLinkData {
	bool textureExists;
	float bass;
	float lowMid;
	float upperMid;
	float treble;
};

struct appdata {
    float4 vertex : POSITION;
    float4 uv : TEXCOORD0;
	float4 uv1 : TEXCOORD1;
	float4 uv2 : TEXCOORD2;
    float4 tangent : TANGENT;
    float3 normal : NORMAL;
	float4 color : COLOR_Centroid;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

#if X_FEATURES

int _GeomFXToggle;
int _ShatterClones, _DissolveClones, _GlitchClones, _WFClones, _DFClones;
int _WireframeToggle, _WFMode, _WireframeTransparency;
int _GlitchToggle;
int _ShatterToggle;
int _CloneToggle, _ClonePattern, _ClonePosition, _SaturateEP;
int _Screenspace;
int _DistanceFadeToggle;
int _ShowInMirror, _ShowBase, _Connected;
int _DissolveChannel, _DissolveWave, _DissolveBlending;
int _GeomDissolveAxis, _GeomDissolveAxisFlip, _GeomDissolveWireframe;
int _GeomDissolveFilter, _GeomDissolveClamp;
int _AudioLinkTriOffsetBand;

float4 _Clone1, _Clone2, _Clone3, _Clone4, _Clone5, _Clone6, _Clone7, _Clone8;
float4 _DissolveRimCol;
float4 _WFColor;
float4 _ClipRimColor;
float3 _EntryPos;
float3 _BaseOffset, _BaseRotation;
float3 _ReflOffset, _ReflRotation;
float3 _Position, _Rotation;
float3 _GeomDissolveSpread, _DissolvePoint0, _DissolvePoint1;

float _Range;
float _DissolveAmount, _DissolveRimWidth, _DissolveBlendSpeed;
float _GeomDissolveAmount, _GeomDissolveWidth, _GeomDissolveClip;
float _WFFill, _WFVisibility;
float _ShatterMax, _ShatterMin, _ShatterSpread, _ShatterCull; 
float _Instability, _GlitchFrequency, _GlitchIntensity, _PosPrecision, _PatternMult; 
float _Visibility, _CloneSize;
float _AudioLinkTriOffsetStrength;

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
	float4 grabPos : TEXCOORD10;
	bool isReflection : TEXCOORD11;
	float4 localPos : TEXCOORD12;
	float roundingMask : TEXCOORD13;
	float4 screenPos : TEXCOORD14;
	float thicknessMask : TEXCOORD15;

	float4 color : COLOR_Centroid;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;

	UNITY_SHADOW_COORDS(19)
	UNITY_FOG_COORDS(20)

	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
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
	float4 grabPos : TEXCOORD13;
	bool isReflection : TEXCOORD14;
	float4 localPos : TEXCOORD15;
	float wfOpac : TEXCOORD16;
	float4 screenPos : TEXCOORD17;
	float wfOpacAL : TEXCOORD18;
	float wfStrAL : TEXCOORD19;
	float thicknessMask : TEXCOORD20;

	float4 color : COLOR_Centroid;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;

	UNITY_SHADOW_COORDS(24)
	UNITY_FOG_COORDS(25)

	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

#include "USAudioLink.cginc"
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
	float3 tangentViewDir : TEXCOORD7;
	float3 cameraPos : TEXCOORD8;
	float3 objPos : TEXCOORD9;
	float4 grabPos : TEXCOORD10;
	bool isReflection : TEXCOORD11;
	float4 localPos : TEXCOORD12;
	float4 screenPos : TEXCOORD13;
	float thicknessMask : TEXCOORD14;

	float4 color : COLOR_Centroid;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;

	UNITY_SHADOW_COORDS(18)
	UNITY_FOG_COORDS(19)
	
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};
#endif

#include "USAudioLink.cginc"
#include "USSSR.cginc"
#include "USBRDF.cginc"
#include "USLighting.cginc"
#include "USFunctions.cginc"
#include "USPass.cginc"

#endif // US_DEFINES_INCLUDED