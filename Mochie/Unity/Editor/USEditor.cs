using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mochie;

class GradientObject : ScriptableObject {
	public Gradient gradient = new Gradient();
}

internal class USEditor : ShaderGUI {

    static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
	Dictionary<Action, GUIContent> baseTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> texturesTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> maskingTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> roughnessTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> smoothnessTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> aoTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> heightTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> bcTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> shadingTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> lightingTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> shadowTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> reflTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> specTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> matcapTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> sssTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> rimTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> eRimTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> refracTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> iriTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> normalTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> bigMaskTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> emissTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> lrTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> pulseTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> filterTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> flipbookTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> book0TabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> book1TabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> outlineTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> uvdTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> vertTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> alTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> alEmissTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> alRimTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> albcdTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> aluvdTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> alVertManipTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> alDissolveTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> alTriOffsTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> alWireframeTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> specialTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> dfTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> dissTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> ssTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> cloneTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> glitchTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> shatterTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> wfTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> renderTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> alOutlineTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> alVizTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> metallicTabButtons = new Dictionary<Action, GUIContent>();

	Toggles toggles = new Toggles(new string[] {
			"BASE", 
			"TEXTURES",
			"SHADING", 
			"Masks", 
			"Lighting",
			"Shadows",
			"Reflections",
			"Specular",
			"Subsurface",
			"Matcap",
			"Basic Rim",
			"Roughness Filter",
			"Normals",
			"EMISSION 0",
			"Pulse",
			"Light Reactivity",
			"FILTERING",
			"UV DISTORTION 0",
			"OUTLINE",
			"SPECIAL FEATURES",
			"Distance Fade",
			"Dissolve 0",
			"Screenspace",
			"Clones",
			"Positions",
			"Glitch",
			"Shatter Culling",
			"Wireframe 0",
			"FLIPBOOK",
			"Occlusion Filter",
			"Height Filter",
			"Primary Layer",
			"Secondary Layer",
			"Environment Rim",
			"Smoothness Filter",
			"Metallic Filter",
			"RENDER SETTINGS",
			"Curvature Filter",
			"Primary Maps",
			"Detail Maps",
			"Primary Matcap",
			"Secondary Matcap",
			"General",
			"Diffuse Shading",
			"Realtime Lighting",
			"Baked Lighting",
			"MASKS",
			"Refraction",
			"VERTEX MANIPULATION 0",
			"Refraction Mask",
			"Reflection Mask",
			"Specular Mask",
			"Specular Blend Mask",
			"Env. Rim Mask",
			"Matcap Mask",
			"Matcap Blend Mask",
			"Aniso Blend Mask",
			"Subsurface Mask",
			"Rim Mask",
			"Detail Mask",
			"Shadow Mask",
			"Diffuse Mask",
			"Filter Mask",
			"Color Mask",
			"Emission Mask",
			"Emission Pulse Mask",
			"Outline Thickness Mask",
			"Base Color Dissolve 0",
			"AUDIO LINK",
			"Emission 1",
			"Rim",
			"Dissolve 1",
			"Base Color Dissolve 1",
			"Vertex Manipulation 1",
			"Triangle Offset",
			"Wireframe 1",
			"Iridescence",
			"UV Distortion 1",
			"Outline 1",
			// "Visualizers",
			"Oscilloscope",
			"DEBUG"
	}, 0);

	private GradientObject gradientObj;
	private SerializedProperty colorGradient;
	private SerializedObject serializedGradient;
	private EditorWindow gradientWindow;
	private Texture2D rampTex;

	static readonly int blendingLabelPos = 111;

	static readonly string unityFolderPath = "Assets/Mochie/Unity";
	string header = "Header_Pro";
	string versionLabel = "v1.29";
	// β
	
	MaterialProperty _RenderMode = null; 
    MaterialProperty _CullingMode = null;
    MaterialProperty _BlendMode = null;
    MaterialProperty _Cutoff = null; 
	MaterialProperty _DistanceFadeToggle = null;
	MaterialProperty _DistanceFadeMax = null;
	MaterialProperty _DistanceFadeMin = null;
	MaterialProperty _ClipRimColor = null;
	MaterialProperty _ClipRimWidth = null;
	MaterialProperty _ClipRimStr = null;
	MaterialProperty _Color = null; 
    MaterialProperty _MainTex = null; 
	MaterialProperty _MirrorTex = null;
	MaterialProperty _MainTexScroll = null;
	MaterialProperty _DetailScroll = null;
	MaterialProperty _RimScroll = null;
	MaterialProperty _MirrorBehavior = null;
    MaterialProperty _FilterMask = null;
    MaterialProperty _Saturation = null; 
	MaterialProperty _RealtimeSpec = null;
    MaterialProperty _Contrast = null; 
    MaterialProperty _RGB = null;
    MaterialProperty _Hue = null;
    MaterialProperty _Filtering= null;
	MaterialProperty _TeamFiltering = null;
    MaterialProperty _AutoShift = null;
    MaterialProperty _AutoShiftSpeed = null;
    MaterialProperty _Brightness = null;
	MaterialProperty _PostFiltering = null;
    MaterialProperty _HDR = null;
	MaterialProperty _Invert = null;
	MaterialProperty _DistortUVMap = null;
	MaterialProperty _DistortUVStr = null;
	MaterialProperty _DistortUVScroll = null;
	MaterialProperty _DistortUVMask = null;
	MaterialProperty _DistortMainUV = null;
	MaterialProperty _DistortEmissUV = null;
	MaterialProperty _DistortDetailUV = null;
	MaterialProperty _DistortRimUV = null;
	MaterialProperty _MainTexCube0 = null;
	MaterialProperty _CubeMode = null;
	MaterialProperty _CubeBlend = null;
	MaterialProperty _CubeRotate0 = null;
	MaterialProperty _AutoRotate0 = null;
	MaterialProperty _CubeColor0 = null;
	MaterialProperty _CubeBlendMode = null;
	MaterialProperty _CubeBlendMask = null;
	MaterialProperty _UnlitMatcap = null;
	MaterialProperty _Spritesheet = null;
	MaterialProperty _SpritesheetPos = null;
	MaterialProperty _SpritesheetScale = null;
	MaterialProperty _SpritesheetRot = null;
	MaterialProperty _RowsColumns = null;
	MaterialProperty _ManualScrub = null;
	MaterialProperty _ScrubPos = null;
	MaterialProperty _FPS = null;
	MaterialProperty _EnableSpritesheet = null;
	MaterialProperty _SpritesheetCol = null;
	MaterialProperty _FrameClipOfs = null;
	MaterialProperty _UnlitSpritesheet = null;
	MaterialProperty _UnlitSpritesheet1 = null;
	MaterialProperty _SpritesheetBlending = null;
	MaterialProperty _Spritesheet1 = null;
	MaterialProperty _SpritesheetPos1 = null;
	MaterialProperty _SpritesheetScale1 = null;
	MaterialProperty _SpritesheetRot1 = null;
	MaterialProperty _RowsColumns1 = null;
	MaterialProperty _ManualScrub1 = null;
	MaterialProperty _IndirectAO = null;
	MaterialProperty _ScrubPos1 = null;
	MaterialProperty _FPS1 = null;
	MaterialProperty _EnableSpritesheet1 = null;
	MaterialProperty _SpritesheetCol1 = null;
	MaterialProperty _FrameClipOfs1 = null;
	MaterialProperty _SpritesheetBlending1 = null;
	MaterialProperty _UseAlphaMask = null;
	MaterialProperty _AlphaMask = null;
	MaterialProperty _AOFiltering = null;
	MaterialProperty _AOLightness = null;
	MaterialProperty _AOIntensity = null;
	MaterialProperty _AOContrast = null;
	MaterialProperty _HeightFiltering = null;
	MaterialProperty _HeightLightness = null;
	MaterialProperty _HeightIntensity = null;
	MaterialProperty _HeightContrast = null;
	MaterialProperty _AOTint = null;
	MaterialProperty _AOTintTex = null;
	MaterialProperty _ShadowMode = null;
	MaterialProperty _ShadowTint = null;
	MaterialProperty _SmoothnessMap = null;
	MaterialProperty _PreviewSmooth = null;
	MaterialProperty _SmoothLightness = null;
	MaterialProperty _SmoothIntensity = null;
	MaterialProperty _SmoothContrast = null;
	MaterialProperty _SmoothnessFiltering = null;
	MaterialProperty _SharpSpecStr = null;
	MaterialProperty _ShadowConditions = null;
	MaterialProperty _DirectAO = null;
	MaterialProperty _Value = null;
	MaterialProperty _DistortionStyle = null;
	MaterialProperty _NoiseScale = null;
	MaterialProperty _NoiseSpeed = null;
	MaterialProperty _NoiseOctaves = null;
    MaterialProperty _PBRWorkflow = null;
	MaterialProperty _StaticLightDirToggle = null;
    MaterialProperty _StaticLightDir = null;
    MaterialProperty _BumpScale = null; 
    MaterialProperty _BumpMap = null; 
    MaterialProperty _Metallic = null; 
    MaterialProperty _MetallicGlossMap = null;
    MaterialProperty _Glossiness = null; 
    MaterialProperty _GlossMapScale = null;
    MaterialProperty _SpecCol = null;
    MaterialProperty _SpecGlossMap = null;
    MaterialProperty _OcclusionStrength = null; 
    MaterialProperty _OcclusionMap = null;
    MaterialProperty _Parallax = null;
    MaterialProperty _ParallaxMap = null; 
    MaterialProperty _ReflCube = null;
    MaterialProperty _DetailMask = null;
    MaterialProperty _DetailAlbedoMap = null;
    MaterialProperty _DetailNormalMapScale = null;
    MaterialProperty _DetailNormalMap = null;
	MaterialProperty _Specular = null;
	MaterialProperty _Reflections = null;
	MaterialProperty _MatcapToggle = null;
	MaterialProperty _Matcap = null;
	MaterialProperty _MatcapStr = null;
	MaterialProperty _MatcapColor = null;
	MaterialProperty _MatcapBlending = null;
	MaterialProperty _MatcapBlending1 = null;
	MaterialProperty _Matcap1 = null;
	MaterialProperty _MatcapColor1 = null;
	MaterialProperty _MatcapStr1 = null;
	MaterialProperty _MatcapBlendMask = null;
	MaterialProperty _DiffuseMask = null;
	MaterialProperty _PackedMap = null;
	MaterialProperty _MetallicChannel = null;
	MaterialProperty _RoughnessChannel = null;
	MaterialProperty _OcclusionChannel = null;
	MaterialProperty _RoughnessFiltering = null;
	MaterialProperty _RoughContrast = null;
	MaterialProperty _RoughLightness = null;
	MaterialProperty _RoughIntensity = null;
	MaterialProperty _MetallicFiltering = null;
	MaterialProperty _MetallicContrast = null;
	MaterialProperty _MetallicLightness = null;
	MaterialProperty _MetallicIntensity = null;
	MaterialProperty _MetallicRemapMin = null;
	MaterialProperty _MetallicRemapMax = null;
	MaterialProperty _PreviewMetallic = null;
	MaterialProperty _VLightCont = null;
	MaterialProperty _SubsurfaceMask = null;
	MaterialProperty _UnlitMatcap1 = null;
	MaterialProperty _Alpha = null;
	MaterialProperty _MaxSteps = null;
	MaterialProperty _Step = null;
	MaterialProperty _LRad = null;
	MaterialProperty _SRad = null;
	MaterialProperty _EdgeFade = null;
	MaterialProperty _SSR = null;
	MaterialProperty _AttenSmoothing = null;
	MaterialProperty _ShadowDithering = null;
	MaterialProperty _MatcapMask = null;
	MaterialProperty _PreviewAO = null;
	MaterialProperty _PreviewRough = null;
	MaterialProperty _PreviewHeight = null;
	MaterialProperty _ZWrite = null;
	MaterialProperty _RippleFrequency = null;
	MaterialProperty _RippleAmplitude = null;
	MaterialProperty _RippleStrength = null;
	MaterialProperty _RippleContinuity = null;
	MaterialProperty _ReflStepping = null;
	MaterialProperty _ReflSteps = null;
	MaterialProperty _PackedMask2 = null;
	MaterialProperty _AnisoStr = null;
	MaterialProperty _MatcapUseRough1 = null;
	MaterialProperty _MatcapRough1 = null;
	MaterialProperty _StencilToggle = null;
	MaterialProperty _StencilRef = null;
	MaterialProperty _StencilPass = null;
	MaterialProperty _StencilFail = null;
	MaterialProperty _StencilZFail = null;
	MaterialProperty _StencilCompare = null;
	MaterialProperty _OutlineStencilPass = null;
	MaterialProperty _OutlineStencilCompare = null;
	MaterialProperty _ZTest = null;
	MaterialProperty _NonlinearSHToggle = null;
	MaterialProperty _SHStr = null;
	MaterialProperty _ColorPreservation = null;
	MaterialProperty _ClearCoat = null;
	MaterialProperty _ReflectionMask = null;
	MaterialProperty _ReflectionStr = null;	
	MaterialProperty _DisneyDiffuse = null;
    MaterialProperty _SharpSpecular = null;
    MaterialProperty _SpecularMask = null;
    MaterialProperty _SpecStr = null; 
    MaterialProperty _ShadowRamp = null;
    MaterialProperty _RampWidth0 = null;
	MaterialProperty _RampWidth1 = null;
	MaterialProperty _RampWeight = null;
    MaterialProperty _ShadowMask = null;
    MaterialProperty _ShadowStr = null; 
	MaterialProperty _DirectCont = null;
	MaterialProperty _IndirectCont = null;
	MaterialProperty _RTDirectCont = null;
	MaterialProperty _RTIndirectCont = null;
	MaterialProperty _AnisoAngleY = null;
	MaterialProperty _AnisoLayerY = null;
	MaterialProperty _AnisoLayerStr = null;
	MaterialProperty _ReflCol = null;
	MaterialProperty _InterpMask = null;
	MaterialProperty _AnisoLerp = null;
	MaterialProperty _MaskingMode = null;
	MaterialProperty _PackedMask0 = null;
	MaterialProperty _PackedMask1 = null;
	MaterialProperty _RTSelfShadow = null;
	MaterialProperty _ClampAdditive = null;
	MaterialProperty _HardenNormals = null;
	MaterialProperty _RampPos = null;
	MaterialProperty _AnisoSteps = null;
	MaterialProperty _DistortMatcap0 = null;
	MaterialProperty _DistortMatcap1 = null;
	MaterialProperty _LightingBasedIOR = null;
	MaterialProperty _OutlineMult = null;
	MaterialProperty _DissolveClones = null;
	MaterialProperty _ShatterClones = null;
	MaterialProperty _DFClones = null;
	MaterialProperty _WFClones = null;
	MaterialProperty _GlitchClones = null;
	MaterialProperty _UseVertexColor = null;
	MaterialProperty _EmissionToggle = null;
    MaterialProperty _EmissionColor = null; 
    MaterialProperty _EmissionMap = null; 
    MaterialProperty _EmissMask = null; 
	MaterialProperty _PulseToggle = null;
	MaterialProperty _PulseWaveform = null;
    MaterialProperty _PulseSpeed = null;
    MaterialProperty _PulseStr = null;
    MaterialProperty _EmissScroll = null; 
    MaterialProperty _Crossfade = null; 
    MaterialProperty _ReactToggle = null; 
    MaterialProperty _CrossMode = null; 
    MaterialProperty _ReactThresh = null;
	MaterialProperty _PulseMask = null;
	MaterialProperty _RimLighting = null;
    MaterialProperty _RimBlending = null;
    MaterialProperty _RimMask = null;
    MaterialProperty _RimCol = null; 
    MaterialProperty _RimTex = null;
    MaterialProperty _RimStr = null; 
    MaterialProperty _RimWidth = null; 
    MaterialProperty _RimEdge = null;
	MaterialProperty _UnlitRim = null;
	MaterialProperty _EnvironmentRim = null;
    MaterialProperty _ERimBlending = null;
    MaterialProperty _ERimTint = null; 
    MaterialProperty _ERimStr = null; 
    MaterialProperty _ERimWidth = null; 
    MaterialProperty _ERimEdge = null;
	MaterialProperty _ERimUseRough = null;
	MaterialProperty _ERimRoughness = null;
    MaterialProperty _ApplyOutlineLighting = null;
	MaterialProperty _ApplyOutlineEmiss = null;
	MaterialProperty _ApplyAlbedoTint = null;
    MaterialProperty _OutlineToggle = null; 
    MaterialProperty _OutlineThicc = null; 
    MaterialProperty _OutlineCol = null;
	MaterialProperty _OutlineTex = null;
	MaterialProperty _OutlineScroll = null;
	MaterialProperty _OutlineMask = null;
	MaterialProperty _OutlineRange = null;
	MaterialProperty _TeamColorMask = null;
	MaterialProperty _TeamColor0 = null;
	MaterialProperty _TeamColor1 = null;
	MaterialProperty _TeamColor2 = null;
	MaterialProperty _TeamColor3 = null;
	MaterialProperty _GlitchToggle = null;
	MaterialProperty _ShatterToggle = null;
	MaterialProperty _WireframeToggle = null;
	MaterialProperty _Instability = null;
    MaterialProperty _GlitchFrequency = null;
    MaterialProperty _GlitchIntensity = null;
    MaterialProperty _ShatterSpread = null;
    MaterialProperty _ShatterMax = null;
    MaterialProperty _ShatterMin = null;
    MaterialProperty _ShatterCull = null;
    MaterialProperty _WFColor = null;
    MaterialProperty _WFFill = null;
    MaterialProperty _WFVisibility = null;
    MaterialProperty _WFMode = null;
    MaterialProperty _PatternMult = null;
	MaterialProperty _EntryPos = null;
	MaterialProperty _SaturateEP = null;
	MaterialProperty _Screenspace = null;
	MaterialProperty _Position = null;
	MaterialProperty _Rotation = null;
	MaterialProperty _Range = null;
	MaterialProperty _DissolveAmount = null;
	MaterialProperty _DissolveTex = null;
	MaterialProperty _DissolveRimCol = null;
	MaterialProperty _DissolveRimWidth = null;
	MaterialProperty _DissolveScroll0 = null;
	MaterialProperty _DissolveBlendSpeed = null;
	MaterialProperty _DissolveBlending = null;
	MaterialProperty _DissolveMask = null;
	MaterialProperty _CloneToggle = null;
	MaterialProperty _ClonePattern = null;
    MaterialProperty _Clone1 = null;
    MaterialProperty _Clone2 = null;
    MaterialProperty _Clone3 = null;
    MaterialProperty _Clone4 = null;
    MaterialProperty _Clone5 = null;
    MaterialProperty _Clone6 = null;
    MaterialProperty _Clone7 = null;
    MaterialProperty _Clone8 = null;
    MaterialProperty _Visibility = null;
	MaterialProperty _ERimMask = null;
	MaterialProperty _DissolveStyle = null;
	MaterialProperty _DissolveNoiseScale = null;
	MaterialProperty _GeomDissolveAmount = null;
	MaterialProperty _GeomDissolveAxis = null;
	MaterialProperty _GeomDissolveAxisFlip = null;
	MaterialProperty _GeomDissolveWidth = null;
	MaterialProperty _GeomDissolveClip = null;
	MaterialProperty _GeomDissolveSpread = null;
	MaterialProperty _GeomDissolveWireframe = null;
	MaterialProperty _ManualSpecBright = null;
	MaterialProperty _GeomDissolveClamp = null;
	MaterialProperty _GeomDissolveFilter = null;
	MaterialProperty _ReflUseRough = null;
	MaterialProperty _ReflRough = null;
	MaterialProperty _SpecUseRough = null;
	MaterialProperty _SpecRough = null;
	MaterialProperty _MatcapUseRough = null;
	MaterialProperty _MatcapRough = null;
	MaterialProperty _UVSec = null;
	MaterialProperty _DetailRoughnessMap = null;
	MaterialProperty _DetailRoughBlending = null;
	MaterialProperty _DetailRoughStrength = null;
	MaterialProperty _HeightChannel = null;
	MaterialProperty _EnablePackedHeight = null;
	MaterialProperty _PackedMask3 = null;
	MaterialProperty _SpritesheetBrightness = null;
	MaterialProperty _SpritesheetBrightness1 = null;
	MaterialProperty _Refraction = null;
	MaterialProperty _RefractionOpac = null;
	MaterialProperty _UnlitRefraction = null;
	MaterialProperty _RefractionIOR = null;
	MaterialProperty _RefractionMask = null;
	MaterialProperty _RefractionCA = null;
	MaterialProperty _RefractionCAStr = null;
	MaterialProperty _RefractionTint = null;
	MaterialProperty _GSAA = null;
	MaterialProperty _DissolvePoint0 = null;
	MaterialProperty _DissolvePoint1 = null;
	MaterialProperty _VertexExpansion = null;
	MaterialProperty _VertexExpansionClamp = null;
	MaterialProperty _VertexRounding = null;
	MaterialProperty _VertexRoundingPrecision = null;
	MaterialProperty _VertexExpansionMask = null;
	MaterialProperty _VertexRoundingMask = null;
	MaterialProperty _UseSpritesheetAlpha = null;
	MaterialProperty _VertexManipulationToggle = null;
	MaterialProperty _RefractionMaskScroll = null;
	MaterialProperty _SpecBiasOverrideToggle = null;
	MaterialProperty _SpecBiasOverride = null;
	MaterialProperty _SpritesheetMode0 = null;
	MaterialProperty _SpritesheetMode1 = null;
	MaterialProperty _Flipbook0 = null;
	MaterialProperty _Flipbook1 = null;
	MaterialProperty _Flipbook0Scroll = null;
	MaterialProperty _Flipbook1Scroll = null;
	MaterialProperty _Flipbook0ClipEdge = null;
	MaterialProperty _Flipbook1ClipEdge = null;
	MaterialProperty _EmissIntensity = null;
	MaterialProperty _MatcapCenter = null;
	MaterialProperty _MatcapCenter1 = null;

	MaterialProperty _EnableMaskTransform = null;
	MaterialProperty _ReflectionMaskScroll = null;
	MaterialProperty _SpecularMaskScroll = null;
	MaterialProperty _ERimMaskScroll = null;
	MaterialProperty _MatcapMaskScroll = null;
	MaterialProperty _MatcapBlendMaskScroll = null;
	MaterialProperty _InterpMaskScroll = null;
	MaterialProperty _SubsurfaceMaskScroll = null;
	MaterialProperty _RimMaskScroll = null;
	MaterialProperty _DetailMaskScroll = null;
	MaterialProperty _ShadowMaskScroll = null;
	MaterialProperty _DiffuseMaskScroll = null;
	MaterialProperty _FilterMaskScroll = null;
	MaterialProperty _EmissMaskScroll = null;
	MaterialProperty _EmissPulseMaskScroll = null; 
	MaterialProperty _RefractionDissolveMask = null;
	MaterialProperty _RefractionDissolveMaskScroll = null;
	MaterialProperty _RefractionDissolveMaskStr = null;
	MaterialProperty _OutlineMaskScroll = null;
	MaterialProperty _IgnoreFilterMask = null;
	MaterialProperty _DitheredShadows = null;
	MaterialProperty _DetailOcclusionMap = null;
	MaterialProperty _DetailOcclusionBlending = null;
	MaterialProperty _DetailOcclusionStrength = null;
	MaterialProperty _DetailAlbedoBlending = null;
	MaterialProperty _DetailAlbedoStrength = null;
	MaterialProperty _NearClip = null;
	MaterialProperty _NearClipMask = null;
	MaterialProperty _NearClipToggle = null;
	MaterialProperty _BCDissolveToggle = null;
	MaterialProperty _BCNoiseTex = null;
	MaterialProperty _BCDissolveStr = null;
	MaterialProperty _BCRimWidth = null;
	MaterialProperty _BCRimCol = null;
	MaterialProperty _MainTex2 = null;
	MaterialProperty _BCColor = null;
	MaterialProperty _AlphaMaskChannel = null;
	MaterialProperty _AudioLinkToggle = null;
	MaterialProperty _AudioLinkEmissionMultiplier = null;
	MaterialProperty _AudioLinkRimMultiplier = null;
	MaterialProperty _AudioLinkRimWidth = null;
	MaterialProperty _AudioLinkDissolveMultiplier = null;
	MaterialProperty _AudioLinkBCDissolveMultiplier = null;
	MaterialProperty _AudioLinkVertManipMultiplier = null;
	MaterialProperty _AudioLinkEmissionBand = null;
	MaterialProperty _AudioLinkRimBand = null;
	MaterialProperty _AudioLinkDissolveBand = null;
	MaterialProperty _AudioLinkBCDissolveBand = null;
	MaterialProperty _AudioLinkVertManipBand = null;
	MaterialProperty _AudioLinkRimPulse = null;
	MaterialProperty _AudioLinkRimPulseWidth = null;
	MaterialProperty _AudioLinkRimPulseSharp = null;
	MaterialProperty _AudioLinkTriOffsetMask = null;
	MaterialProperty _AudioLinkTriOffsetBand = null;
	MaterialProperty _AudioLinkTriOffsetStrength = null;
	MaterialProperty _AudioLinkTriOffsetMaskScroll = null;
	MaterialProperty _AudioLinkTriOffsetCoords = null;
	MaterialProperty _AudioLinkTriOffsetSize = null;
	MaterialProperty _AudioLinkWireframeMask = null;
	MaterialProperty _AudioLinkWireframeBand = null;
	MaterialProperty _AudioLinkWireframeStrength = null;
	MaterialProperty _AudioLinkWireframeMaskScroll = null;
	MaterialProperty _AudioLinkWireframeCoords = null;
	MaterialProperty _AudioLinkWireframeSize = null;
	MaterialProperty _AudioLinkWireframeColor = null;
	MaterialProperty _AudioLinkWireframeStartPos = null;
	MaterialProperty _AudioLinkWireframeEndPos = null;
	MaterialProperty _AudioLinkTriOffsetStartPos = null;
	MaterialProperty _AudioLinkTriOffsetEndPos = null;
	MaterialProperty _AudioLinkWireframeMode = null;
	MaterialProperty _AudioLinkTriOffsetMode = null;
	MaterialProperty _AudioLinkStrength = null;
	MaterialProperty _DetailShadowMap = null;
	MaterialProperty _RoughRemapMin = null;
	MaterialProperty _RoughRemapMax = null;
	MaterialProperty _SmoothRemapMin = null;
	MaterialProperty _SmoothRemapMax = null;
	MaterialProperty _AORemapMin = null;
	MaterialProperty _AORemapMax = null;
	MaterialProperty _HeightRemapMin = null;
	MaterialProperty _HeightRemapMax = null;
	MaterialProperty _Subsurface = null;
	MaterialProperty _ScatterBaseColorTint = null;
	MaterialProperty _ThicknessMap = null;
	MaterialProperty _ThicknessMapPower = null;
	MaterialProperty _ScatterTex = null;
	MaterialProperty _ScatterCol = null;
	MaterialProperty _ScatterAmbient = null;
	MaterialProperty _ScatterIntensity = null;
	MaterialProperty _ScatterPow = null;
	MaterialProperty _ScatterDist = null;
	MaterialProperty _ScatterWrap = null;
	MaterialProperty _Hide = null;
	MaterialProperty _AudioLinkRemapMin = null;
	MaterialProperty _AudioLinkRemapMax = null;
	MaterialProperty _AudioLinkRemapEmissionMin = null;
	MaterialProperty _AudioLinkRemapEmissionMax = null;
	MaterialProperty _AudioLinkRemapRimMin = null;
	MaterialProperty _AudioLinkRemapRimMax = null;
	MaterialProperty _AudioLinkRemapDissolveMin = null;
	MaterialProperty _AudioLinkRemapDissolveMax = null;
	MaterialProperty _AudioLinkRemapBCDissolveMin = null;
	MaterialProperty _AudioLinkRemapBCDissolveMax = null;
	MaterialProperty _AudioLinkRemapVertManipMin = null;
	MaterialProperty _AudioLinkRemapVertManipMax = null;
	MaterialProperty _AudioLinkRemapTriOffsetMin = null;
	MaterialProperty _AudioLinkRemapTriOffsetMax = null;
	MaterialProperty _AudioLinkRemapWireframeMin = null;
	MaterialProperty _AudioLinkRemapWireframeMax = null;
	MaterialProperty _AudioLinkUVDistortionBand = null;
	MaterialProperty _AudioLinkUVDistortionMultiplier = null;
	MaterialProperty _AudioLinkRemapUVDistortionMin = null;
	MaterialProperty _AudioLinkRemapUVDistortionMax = null; 
	MaterialProperty _AudioLinkRemapRimPulseMin = null;
	MaterialProperty _AudioLinkRemapRimPulseMax = null;
	MaterialProperty _Iridescence = null;
	MaterialProperty _IridescenceStrength = null;
	MaterialProperty _IridescenceWidth = null;
	MaterialProperty _IridescenceEdge = null;
	MaterialProperty _IridescenceHue = null;
	MaterialProperty _IridescenceMask = null;
	MaterialProperty _RefractionBlur = null;
	MaterialProperty _RefractionBlurStrength = null;
	MaterialProperty _RefractionBlurRough = null;
	MaterialProperty _AddCont = null;
	MaterialProperty _AudioLinkOutlineBand = null;
	MaterialProperty _AudioLinkOutlineMultiplier = null;
	MaterialProperty _AudioLinkRemapOutlineMin = null;
	MaterialProperty _AudioLinkRemapOutlineMax = null;
	MaterialProperty _ParallaxOffset = null;
	MaterialProperty _ParallaxSteps = null;
	MaterialProperty _VertexRotation = null;
	MaterialProperty _VertexPosition = null;
	MaterialProperty _UseOutlineTexAlpha = null;
	MaterialProperty _MatcapNormal0 = null;
	MaterialProperty _MatcapNormal0Str = null;
	MaterialProperty _MatcapNormal0Scroll = null;
	MaterialProperty _MatcapNormal0Mix = null;
	MaterialProperty _MatcapNormal1 = null;
	MaterialProperty _MatcapNormal1Str = null;
	MaterialProperty _MatcapNormal1Scroll = null;
	MaterialProperty _MatcapNormal1Mix = null;
	MaterialProperty _ACES = null;
	MaterialProperty _MainTexRot = null;
	MaterialProperty _DetailRot = null;
	MaterialProperty _VertexColor = null;
	MaterialProperty _FresnelToggle = null;
	MaterialProperty _FresnelStrength = null;
	MaterialProperty _OscilloscopeStrength = null;
	MaterialProperty _OscilloscopeCol = null;
	MaterialProperty _OscilloscopeScale = null;
	MaterialProperty _OscilloscopeOffset = null;
	MaterialProperty _OscilloscopeRot = null;
	MaterialProperty _OscilloscopeMarginLR = null;
	MaterialProperty _OscilloscopeMarginTB = null;
	MaterialProperty _EmissionMap2 = null;
	MaterialProperty _EmissionColor2 = null;
	MaterialProperty _EmissScroll2 = null;
	MaterialProperty _EmissIntensity2 = null;
	MaterialProperty _AlphaStrength = null;
	MaterialProperty _WireframeTransparency = null;

	MaterialProperty _VRCFallback = null;
	MaterialProperty _NaNLmao = null;

    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    MaterialEditor m_me;
    public override void OnGUI(MaterialEditor me, MaterialProperty[] props) {

		ClearDictionaries();
		
		m_me = me;
		Material mat = (Material)me.target;

		// Find properties
        foreach (var property in GetType().GetFields(bindingFlags)){
            if (property.FieldType == typeof(MaterialProperty)){
                property.SetValue(this, FindProperty(property.Name, props));
            }
        }

		// For shadow ramp gradient editor
		if (gradientObj == null){
			gradientObj = GradientObject.CreateInstance<GradientObject>();
			serializedGradient = new SerializedObject(gradientObj);
			colorGradient = serializedGradient.FindProperty("gradient");
			rampTex = new Texture2D(128, 16);
			GradientToTexture(ref rampTex);
		}

		// Using this to mercilessly murder verts when necessary (must be 0)
		_NaNLmao.floatValue = 0.0f;

		// Generate path sensitive folders if necessary
		if (!AssetDatabase.IsValidFolder(unityFolderPath+"/Textures/Ramps"))
			AssetDatabase.CreateFolder(unityFolderPath+"/Textures", "Ramps");

		float blendMode = _BlendMode.floatValue;
		float workflow = _PBRWorkflow.floatValue;
		float renderMode = _RenderMode.floatValue;
		float cubeMode = _CubeMode.floatValue;

		// Check name of shader to determine if certain properties should be displayed
        bool isTransparent = blendMode >= 4;
        bool isCutout = blendMode > 0 && blendMode < 4;
		bool isUberX = MGUI.IsXVersion(mat);
		bool isOutline = MGUI.IsOutline(mat);

		if (isUberX){
			header = "HeaderX_Pro";
			if (!EditorGUIUtility.isProSkin){
				header = "HeaderX";
			}
		}
		else {
			header = "Header_Pro";
			if (!EditorGUIUtility.isProSkin){
				header = "Header";
			}
		}

		// Add mat to foldout dictionary if it isn't in there yet
		if (!foldouts.ContainsKey(mat))
			foldouts.Add(mat, toggles);
			
		foreach (var obj in _BlendMode.targets)
			ApplyMaterialSettings((Material)obj);

		Texture2D headerTex = (Texture2D)Resources.Load(header, typeof(Texture2D));
		Texture2D standardIcon = (Texture2D)Resources.Load("StandardIcon", typeof(Texture2D));
		Texture2D toonIcon = (Texture2D)Resources.Load("ToonIcon", typeof(Texture2D));
		Texture2D copyTo1Icon = (Texture2D)Resources.Load("CopyTo1Icon", typeof(Texture2D));
		Texture2D copyTo2Icon = (Texture2D)Resources.Load("CopyTo2Icon", typeof(Texture2D));

		GUIContent toonLabel = new GUIContent(toonIcon, "Apply preset property values for basic stylized toon shading.");
		GUIContent standardLabel = new GUIContent(standardIcon, "Apply preset property values that visually match Standard shader.");
		GUIContent copyTo1Label = new GUIContent(copyTo1Icon, "Copy settings to primary layer.");
		GUIContent copyTo2Label = new GUIContent(copyTo2Icon, "Copy settings to secondary layer.");

        GUILayout.Label(headerTex);
		MGUI.Space4();

		// -----------------
		// Base Settings
		// -----------------
		baseTabButtons.Add(()=>{Toggles.CollapseFoldouts(mat, foldouts, 1);}, MGUI.collapseLabel);
		baseTabButtons.Add(()=>{DoStandardLighting(mat);}, standardLabel);
		Action baseTabAction = ()=>{
			me.RenderQueueField();
			me.ShaderProperty(_RenderMode, Tips.renderModeLabel);
			EditorGUI.BeginChangeCheck();
			me.ShaderProperty(_BlendMode, Tips.blendModeLabel);
			me.ShaderProperty(_MirrorBehavior, Tips.mirrorBehaviorLabel);
			me.ShaderProperty(_CubeMode, Tips.cubeModeLabel);
			if (EditorGUI.EndChangeCheck())
				SetBlendMode(mat);
			if (isCutout || isTransparent){
				me.ShaderProperty(_UseAlphaMask, Tips.useAlphaMaskLabel);
				me.ShaderProperty(_AlphaStrength, "Alpha Strength");
			}
			if (blendMode == 1)
				me.ShaderProperty(_Cutoff, "Cutout");
			GUILayout.Space(8);
			switch((int)cubeMode){

				// Tex Only
				case 0: 
					me.TexturePropertySingleLine(Tips.baseColorLabel, _MainTex, _Color, renderMode == 1 ? _ColorPreservation : null);
					if (renderMode == 1) MGUI.TexPropLabel(Tips.colorPreservation, 123);
					if (_MirrorBehavior.floatValue == 2)
						me.TexturePropertySingleLine(Tips.mirrorTexLabel, _MirrorTex);
					MGUI.TextureSOScroll(me, _MainTex, _MainTexScroll);
					me.ShaderProperty(_MainTexRot, "Rotation");
					break;
				
				// Cubemap Only
				case 1: 
					me.TexturePropertySingleLine(Tips.reflCubeLabel, _MainTexCube0, _CubeColor0, renderMode == 1 ? _ColorPreservation : null);
					if (renderMode == 1) 
						MGUI.TexPropLabel(Tips.colorPreservation, 123);
					MGUI.Vector3Field(_CubeRotate0, "Rotation", false);
					me.ShaderProperty(_AutoRotate0, "Auto Rotate");
					break;
				
				// Tex and Cubemap
				case 2: 
					me.TexturePropertySingleLine(Tips.baseColorLabel, _MainTex, _Color, renderMode == 1 ? _ColorPreservation : null);
					if (renderMode == 1) 
						MGUI.TexPropLabel(Tips.colorPreservation, 123);
					if (_MirrorBehavior.floatValue == 2)
						me.TexturePropertySingleLine(Tips.mirrorTexLabel, _MirrorTex);
					MGUI.TextureSOScroll(me, _MainTex, _MainTexScroll);
					me.ShaderProperty(_MainTexRot, "Rotation");

					me.TexturePropertySingleLine(new GUIContent("Blend"), _CubeBlendMask, _CubeBlendMask.textureValue ? null : _CubeBlend);

					me.TexturePropertySingleLine(Tips.reflCubeLabel, _MainTexCube0, _CubeColor0, _CubeBlendMode);
					MGUI.TexPropLabel("Blending", blendingLabelPos);
					MGUI.Vector3Field(_CubeRotate0, "Rotation", false);
					me.ShaderProperty(_AutoRotate0, "Auto Rotate");
					break;
				default: break;
			}
			if (_UseAlphaMask.floatValue == 1 && (isCutout || isTransparent)){
				me.TexturePropertySingleLine(Tips.alphaMaskLabel, _AlphaMask, _AlphaMaskChannel);
				MGUI.TexPropLabel("Channel", 109);
				MGUI.TextureSO(me, _AlphaMask);
			}
			me.ShaderProperty(_VertexColor, "Vertex Color");
		};
		Foldouts.Foldout("BASE", foldouts, baseTabButtons, mat, me, baseTabAction);

		// -----------------
		// Textures
		// -----------------
		if (renderMode == 1){
			texturesTabButtons.Add(()=>{DoTextureMapReset();}, MGUI.resetLabel);
			Action texturesTabAction = ()=>{
				// Primary Maps
				MGUI.BoldLabel("Primary Maps");
				me.ShaderProperty(_PBRWorkflow, "Workflow");
				MGUI.PropertyGroup(() => {
					switch ((int)workflow){

						// Metallic
						case 0:
							me.TexturePropertySingleLine(Tips.metallicTexLabel, _MetallicGlossMap, _MetallicGlossMap.textureValue ? null : _Metallic);
							me.TexturePropertySingleLine(Tips.roughnessTexLabel, _SpecGlossMap, _SpecGlossMap.textureValue ? null : _Glossiness);
							MGUI.sRGBWarning(_SpecGlossMap);
							break;

						// Specular (RGB)
						case 1: 
							me.TexturePropertySingleLine(Tips.specularTexLabel, _SpecGlossMap, _SpecCol);
							me.TexturePropertySingleLine(Tips.smoothTexLabel, _SmoothnessMap, _GlossMapScale);
							break;

						// Specular (RGBA)
						case 2: 
							me.TexturePropertySingleLine(Tips.specularTexLabel, _SpecGlossMap, _SpecCol);
							me.ShaderProperty(_GlossMapScale, "Smoothness", 2);
							break;

						// Packed
						case 3:
							me.TexturePropertySingleLine(Tips.packedTexLabel, _PackedMap);
							MGUI.sRGBWarning(_PackedMap);
							me.ShaderProperty(_MetallicChannel, "Metallic");
							me.ShaderProperty(_RoughnessChannel, "Roughness");
							me.ShaderProperty(_OcclusionChannel, "Occlusion");
							me.ShaderProperty(_HeightChannel, "Height");
							me.ShaderProperty(_OcclusionStrength, "Occlusion Strength");
							MGUI.ToggleSlider(me, "Height Strength", _EnablePackedHeight, _Parallax);
							if (_EnablePackedHeight.floatValue > 0){
								me.ShaderProperty(_ParallaxOffset, Tips.parallaxOfsText, 1);
								me.ShaderProperty(_ParallaxSteps, Tips.stepsText, 1);
							}
							break;
							
						default: break;
					}
					if (workflow < 3)
						me.TexturePropertySingleLine(Tips.occlusionTexLabel, _OcclusionMap, _OcclusionMap.textureValue ? _OcclusionStrength : null);
					me.TexturePropertySingleLine(Tips.normalTexLabel, _BumpMap, _BumpMap.textureValue ? _BumpScale : null);
					if (workflow < 3){
						me.TexturePropertySingleLine(Tips.heightTexLabel, _ParallaxMap, _ParallaxMap.textureValue ? _Parallax : null);
						if (_ParallaxMap.textureValue){
							me.ShaderProperty(_ParallaxOffset, Tips.parallaxOfsText, 2);
							me.ShaderProperty(_ParallaxSteps, Tips.stepsText, 2);
						}
					}
				});

				MGUI.BoldLabel("Detail Maps");
				me.ShaderProperty(_UVSec, "UV Set");
				MGUI.PropertyGroup(() => {
					bool usingDetRough = _DetailRoughnessMap.textureValue;
					bool usingDetOcc = _DetailOcclusionMap.textureValue;
					bool usingDetAlbedo = _DetailAlbedoMap.textureValue;
					bool usingDetNormal = _DetailNormalMap.textureValue;
					me.TexturePropertySingleLine(Tips.baseColorLabel, _DetailAlbedoMap, usingDetAlbedo ? _DetailAlbedoStrength : null, usingDetAlbedo ? _DetailAlbedoBlending : null);
					me.TexturePropertySingleLine(Tips.normalTexLabel, _DetailNormalMap, usingDetNormal ? _DetailNormalMapScale : null);
					if (workflow == 0 || workflow >= 3)
						me.TexturePropertySingleLine(Tips.roughnessTexLabel, _DetailRoughnessMap, usingDetRough ? _DetailRoughStrength : null, usingDetRough ? _DetailRoughBlending : null);
					me.TexturePropertySingleLine(Tips.occlusionTexLabel, _DetailOcclusionMap, usingDetOcc ? _DetailOcclusionStrength : null, usingDetOcc ? _DetailOcclusionBlending : null);
					if (usingDetAlbedo || usingDetNormal || usingDetRough || usingDetOcc) {
						MGUI.TextureSOScroll(me, _DetailAlbedoMap, _DetailScroll);
						me.ShaderProperty(_DetailRot, "Rotation");
					}
				});
				MGUI.Space2();

				// Masking
				maskingTabButtons.Add(()=>{DoMaskingReset();}, MGUI.resetLabel);
				Action maskingTabAction = ()=>{
					me.ShaderProperty(_MaskingMode, Tips.maskingModeLabel);
					if (_MaskingMode.floatValue == 1){
						me.ShaderProperty(_EnableMaskTransform, Tips.enableMaskTransformLabel);
						MGUI.PropertyGroup(() => {
							if (_EnableMaskTransform.floatValue == 0){
								me.TexturePropertySingleLine(Tips.reflLabel, _ReflectionMask);
								me.TexturePropertySingleLine(Tips.specLabel, _SpecularMask);
								me.TexturePropertySingleLine(Tips.anisoBlendLabel, _InterpMask); 
								me.TexturePropertySingleLine(Tips.matcapPrimaryMask, _MatcapMask);
								me.TexturePropertySingleLine(Tips.matcapSecondaryMask, _MatcapBlendMask);
								me.TexturePropertySingleLine(Tips.shadowLabel, _ShadowMask);
								me.TexturePropertySingleLine(Tips.basicRimLabel, _RimMask);
								me.TexturePropertySingleLine(Tips.eRimLabel, _ERimMask);
								me.TexturePropertySingleLine(Tips.diffuseLabel, _DiffuseMask);
								me.TexturePropertySingleLine(Tips.subsurfLabel, _SubsurfaceMask);
								me.TexturePropertySingleLine(Tips.detailLabel, _DetailMask);
								me.TexturePropertySingleLine(Tips.emissLabel, _EmissMask);
								me.TexturePropertySingleLine(Tips.emissPulseLabel, _PulseMask);
								me.TexturePropertySingleLine(Tips.filterLabel, _FilterMask);	
								me.TexturePropertySingleLine(Tips.refractLabel, _RefractionMask);
								if (isOutline)
									me.TexturePropertySingleLine(Tips.olThickLabel, _OutlineMask);
							}
							else {
								MGUI.Space4();
								bool reflMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.reflLabel, "Reflection Mask");
								MGUI.MaskProperty(mat, me, reflMask, _ReflectionMask, _ReflectionMaskScroll);
								bool specMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.specLabel, "Specular Mask");
								MGUI.MaskProperty(mat, me, specMask, _SpecularMask, _SpecularMaskScroll);
								bool interpMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.anisoBlendLabel, "Specular Blend Mask");
								MGUI.MaskProperty(mat, me, interpMask, _InterpMask, _InterpMaskScroll);
								bool matcapMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.matcapPrimaryMask, "Primary Matcap Mask");
								MGUI.MaskProperty(mat, me, matcapMask, _MatcapMask, _MatcapMaskScroll);
								bool matcapBlendMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.matcapSecondaryMask, "Secondary Matcap Mask");
								MGUI.MaskProperty(mat, me, matcapBlendMask, _MatcapBlendMask, _MatcapBlendMaskScroll);
								bool shadowMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.shadowLabel, "Shadow Mask");
								MGUI.MaskProperty(mat, me, shadowMask, _ShadowMask, _ShadowMaskScroll);
								bool rimMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.basicRimLabel, "Rim Mask");
								MGUI.MaskProperty(mat, me, rimMask, _RimMask, _RimMaskScroll);
								bool eRimMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.eRimLabel, "Env. Rim Mask");
								MGUI.MaskProperty(mat, me, eRimMask, _ERimMask, _ERimMaskScroll);
								bool diffuseMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.diffuseLabel, "Diffuse Mask");
								MGUI.MaskProperty(mat, me, diffuseMask, _DiffuseMask, _DiffuseMaskScroll);
								bool subsurfMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.subsurfLabel, "Subsurface Mask");
								MGUI.MaskProperty(mat, me, subsurfMask, _SubsurfaceMask, _SubsurfaceMaskScroll);
								bool detailMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.detailLabel, "Detail Mask");
								MGUI.MaskProperty(mat, me, detailMask, _DetailMask, _DetailMaskScroll);
								bool emissMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.emissLabel, "Emission Mask");
								MGUI.MaskProperty(mat, me, emissMask, _EmissMask, _EmissMaskScroll);
								bool emissPulseMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.emissPulseLabel, "Emission Pulse Mask");
								MGUI.MaskProperty(mat, me, emissPulseMask, _PulseMask, _EmissPulseMaskScroll);
								bool filterMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.filterLabel, "Filter Mask");
								MGUI.MaskProperty(mat, me, filterMask, _FilterMask, _FilterMaskScroll);
								bool refractMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.refractLabel, "Refraction Mask");
								MGUI.MaskProperty(mat, me, refractMask, _RefractionMask, _RefractionMaskScroll);
								if (isOutline){
									bool olThickMask = Foldouts.DoMaskFoldout(foldouts, mat, me, Tips.olThickLabel, "Outline Thickness Mask");
									MGUI.MaskProperty(mat, me, olThickMask, _OutlineMask, _OutlineMaskScroll);
								}
								MGUI.SpaceN2();
							}
						});
					}
					else if (_MaskingMode.floatValue == 2){
						MGUI.PropertyGroup(() => {
							me.TexturePropertySingleLine(new GUIContent("Mask 1"), _PackedMask0);
							GUILayout.Label("Red:	Reflections\nGreen:	Specular\nBlue:	Primary Matcap\nAlpha:	Secondary Matcap");
							MGUI.Space8();
							me.TexturePropertySingleLine(new GUIContent("Mask 2"), _PackedMask1);
							GUILayout.Label("Red:	Shadows\nGreen:	Diffuse Shading\nBlue:	Subsurface\nAlpha:	Detail Maps");
							MGUI.Space8();
							me.TexturePropertySingleLine(new GUIContent("Mask 3"), _PackedMask2);
							GUILayout.Label("Red:	Basic Rim\nGreen:	Environment Rim\nBlue:	Refraction\nAlpha:	Specular Blend");
							MGUI.Space8();
							me.TexturePropertySingleLine(new GUIContent("Mask 4"), _PackedMask3);
							GUILayout.Label("Red:	Emission\nGreen:	Emission Pulse\nBlue:	Filtering\nAlpha:	Outline Thickness");
						});
					}
				};
				Foldouts.SubFoldout("Masks", foldouts, maskingTabButtons, mat, me, maskingTabAction);

				
				if (workflow == 0 || workflow >= 3){

					// Metallic Filtering
					metallicTabButtons.Add(()=>{DoMetallicFilterReset();}, MGUI.resetLabel);
					Action metallicTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_MetallicFiltering.floatValue == 0);
							me.ShaderProperty(_PreviewMetallic, "Preview");
							MGUI.SliderMinMax01(_MetallicRemapMin, _MetallicRemapMax, "Remap", 1);
							me.ShaderProperty(_MetallicLightness, "Lightness");
							me.ShaderProperty(_MetallicIntensity, "Intensity");
							me.ShaderProperty(_MetallicContrast, "Contrast");
							MGUI.ToggleGroupEnd();
						});
					};
					MGUI.ToggleGroupEnd();
					Foldouts.SubFoldout("Metallic Filter", foldouts, metallicTabButtons, mat, me, metallicTabAction, _MetallicFiltering);

					// Roughness Filtering
					roughnessTabButtons.Add(()=>{DoRoughFilterReset();}, MGUI.resetLabel);
					Action roughnessTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_RoughnessFiltering.floatValue == 0);
							me.ShaderProperty(_PreviewRough, "Preview");
							MGUI.SliderMinMax01(_RoughRemapMin, _RoughRemapMax, "Remap", 1);
							me.ShaderProperty(_RoughLightness, "Lightness");
							me.ShaderProperty(_RoughIntensity, "Intensity");
							me.ShaderProperty(_RoughContrast, "Contrast");
							MGUI.ToggleGroupEnd();
						});
					};
					MGUI.ToggleGroupEnd();
					Foldouts.SubFoldout("Roughness Filter", foldouts, roughnessTabButtons, mat, me, roughnessTabAction, _RoughnessFiltering);
				}

				// Smoothness Filtering (for specular)
				else {
					if (workflow == 1)
						MGUI.ToggleGroup(!_SmoothnessMap.textureValue);
					else
						MGUI.ToggleGroup(!_SpecGlossMap.textureValue);
					
					smoothnessTabButtons.Add(()=>{DoSmoothFilterReset();}, MGUI.resetLabel);
					Action smoothnessTabAction = ()=>{
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_SmoothnessFiltering.floatValue == 0);
							me.ShaderProperty(_PreviewSmooth, "Preview");
							MGUI.SliderMinMax01(_SmoothRemapMin, _SmoothRemapMax, "Remap", 1);
							me.ShaderProperty(_SmoothLightness, "Lightness");
							me.ShaderProperty(_SmoothIntensity, "Intensity");
							me.ShaderProperty(_SmoothContrast, "Contrast");
							MGUI.ToggleGroupEnd();
						});
					};
					MGUI.ToggleGroupEnd();
					Foldouts.SubFoldout("Smoothness Filter", foldouts, smoothnessTabButtons, mat, me, smoothnessTabAction, _SmoothnessFiltering);
				}

				// AO Filtering
				if (workflow < 3)
					MGUI.ToggleGroup(!_OcclusionMap.textureValue && workflow < 3);
				else
					MGUI.ToggleGroup(!_PackedMap.textureValue);
				
				aoTabButtons.Add(()=>{DoAOFilterReset();}, MGUI.resetLabel);
				Action aoTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_AOFiltering.floatValue == 0);
						me.ShaderProperty(_PreviewAO, "Preview");
						me.TexturePropertySingleLine(Tips.tintLabel, _AOTintTex, _AOTint);
						MGUI.SliderMinMax01(_AORemapMin, _AORemapMax, "Remap", 1);
						me.ShaderProperty(_AOLightness, "Lightness");
						me.ShaderProperty(_AOIntensity, "Intensity");
						me.ShaderProperty(_AOContrast, "Contrast");
						MGUI.ToggleGroupEnd();
					});
				};
				MGUI.ToggleGroupEnd();
				Foldouts.SubFoldout("Occlusion Filter", foldouts, aoTabButtons, mat, me, aoTabAction, _AOFiltering);

				// Height Filtering
				heightTabButtons.Add(()=>{DoHeightFilterReset();}, MGUI.resetLabel);
				Action heightTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_HeightFiltering.floatValue == 0);
						me.ShaderProperty(_PreviewHeight, "Preview");
						MGUI.SliderMinMax01(_HeightRemapMin, _HeightRemapMax, "Remap", 1);
						me.ShaderProperty(_HeightLightness, "Lightness");
						me.ShaderProperty(_HeightIntensity, "Intensity");
						me.ShaderProperty(_HeightContrast, "Contrast");
						MGUI.ToggleGroupEnd();
					});
				};
				MGUI.ToggleGroupEnd();
				Foldouts.SubFoldout("Height Filter", foldouts, heightTabButtons, mat, me, heightTabAction, _HeightFiltering);
				
				// Base Color Dissolve
				bcTabButtons.Add(()=>{DoBCDissolveReset();}, MGUI.resetLabel);
				Action bcTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_BCDissolveToggle.floatValue == 0);
						me.TexturePropertySingleLine(Tips.baseColor2Label, _MainTex2, _BCColor);
						MGUI.TextureSO(me, _MainTex2, _MainTex2.textureValue);
						me.TexturePropertySingleLine(Tips.dissolveTexLabel, _BCNoiseTex, _BCDissolveStr);
						MGUI.TextureSO(me, _BCNoiseTex, _BCNoiseTex.textureValue);
						me.ShaderProperty(_BCRimCol, "Rim Color");
						me.ShaderProperty(_BCRimWidth, "Rim Width");
						MGUI.ToggleGroupEnd();
					});
				};
				Foldouts.SubFoldout("Base Color Dissolve 0", foldouts, bcTabButtons, mat, me, bcTabAction, _BCDissolveToggle);
			};
			Foldouts.Foldout("TEXTURES", foldouts, texturesTabButtons, mat, me, texturesTabAction);

			// -----------------
			// Shading
			// -----------------
			bool queueError = mat.renderQueue < 2501;
			bool reflError = _Reflections.floatValue > 0 && _SSR.floatValue == 1;
			bool refracError = _Refraction.floatValue > 0 && queueError;
			bool[] shadingErrors = {reflError && queueError, refracError};
			
			shadingTabButtons.Add(()=>{DoShadingReset(mat);}, MGUI.resetLabel);
			Action shadingTabAction = ()=>{
				MGUI.SpaceN8();

				// Lighting
				lightingTabButtons.Add(()=>{DoLightingReset();}, MGUI.resetLabel);
				Action lightingTabAction = ()=>{
					MGUI.BoldLabel("General");
					MGUI.PropertyGroup( () => {
						MGUI.Vector3FieldToggle(Tips.staticLightDirToggle, _StaticLightDirToggle, _StaticLightDir);
						MGUI.ToggleGroup(!_OcclusionMap.textureValue && workflow != 3);
						me.ShaderProperty(_DirectAO, Tips.directAO);
						me.ShaderProperty(_IndirectAO, Tips.indirectAO);
						MGUI.ToggleGroupEnd();
						if (!_OcclusionMap.textureValue && workflow < 3){
							GUILayout.Space(-32);
							GUIStyle f = new GUIStyle(EditorStyles.boldLabel);
							f.fontSize = 10;
							Rect r = EditorGUILayout.GetControlRect();
							r.x += EditorGUIUtility.labelWidth+21f;
							GUI.Label(r, "Requires Occlusion Map", f);
							MGUI.Space10();
						}
					});
					MGUI.BoldLabel("Diffuse Shading");
					MGUI.PropertyGroup( () => {
						me.ShaderProperty(_DisneyDiffuse, Tips.disneyDiffuse);
						me.ShaderProperty(_SHStr, Tips.shStr);
						MGUI.ToggleGroup(_SHStr.floatValue == 0);
						me.ShaderProperty(_NonlinearSHToggle, Tips.nonlinearSHToggle);
						MGUI.ToggleGroupEnd();
					});
					MGUI.BoldLabel("Realtime Light");
					MGUI.PropertyGroup( () => {
						me.ShaderProperty(_RTDirectCont, Tips.rtDirectCont);
						me.ShaderProperty(_RTIndirectCont, Tips.rtIndirectCont);
						me.ShaderProperty(_VLightCont, Tips.vLightCont);
						me.ShaderProperty(_AddCont, Tips.addCont);
						me.ShaderProperty(_ClampAdditive, Tips.clampAdditive);
					});
					MGUI.BoldLabel("Baked Light");
					MGUI.PropertyGroup( () => {
						me.ShaderProperty(_DirectCont, Tips.directCont);
						me.ShaderProperty(_IndirectCont, Tips.indirectCont);
					});
				};
				Foldouts.SubFoldout("Lighting", foldouts, lightingTabButtons, mat, me, lightingTabAction);

				// Shadows
				shadowTabButtons.Add(()=>{DoShadowReset();}, MGUI.resetLabel);
				Action shadowTabAction = ()=>{
					me.ShaderProperty(_ShadowMode, "Mode");
					if (_ShadowMode.floatValue > 0){
						me.ShaderProperty(_ShadowConditions, Tips.shadowConditions);
						MGUI.PropertyGroup( () => {
							me.TexturePropertySingleLine(Tips.detailShadowMap, _DetailShadowMap);
						});
						if (_ShadowMode.floatValue == 1){
							MGUI.PropertyGroup( () => {
								me.ShaderProperty(_ShadowTint, "Tint");
								me.ShaderProperty(_RampWidth0, "Ramp 1");
								me.ShaderProperty(_RampWidth1, "Ramp 2");
								me.ShaderProperty(_RampWeight, "Ramp Blend");
								me.ShaderProperty(_RampPos, "Offset");
							});
						}
						else if (_ShadowMode.floatValue == 2){
							MGUI.PropertyGroup(() => {
								me.TexturePropertySingleLine(Tips.shadowRampLabel, _ShadowRamp);
								GUILayout.Space(-19);
								EditorGUI.BeginChangeCheck();
								EditorGUILayout.PropertyField(colorGradient, new GUIContent(" "));
								if (EditorGUI.EndChangeCheck()){
									serializedGradient.ApplyModifiedProperties();
									GradientToTexture(ref rampTex);
								}
								if (MGUI.SimpleButton("Apply", MGUI.GetPropertyWidth(), EditorGUIUtility.labelWidth)){
									byte[] encodedTex = rampTex.EncodeToPNG();
									int rampID = UnityEngine.Random.Range(0,10000000);
									string rampPath = unityFolderPath+"/Textures/Ramps/Ramp_"+rampID+".png";
									MGUI.WriteBytes(encodedTex, rampPath);
									AssetDatabase.ImportAsset(rampPath);
									_ShadowRamp.textureValue = (Texture)EditorGUIUtility.Load(rampPath);
								}
							});
						}
						MGUI.PropertyGroup( () => {
							me.ShaderProperty(_ShadowStr, "Strength");
							me.ShaderProperty(_DitheredShadows, "Dithering");
							me.ShaderProperty(_RTSelfShadow, Tips.rtSelfShadow);
							MGUI.ToggleGroup(_RTSelfShadow.floatValue == 0);
							me.ShaderProperty(_AttenSmoothing, Tips.attenSmoothing);
							MGUI.ToggleGroupEnd();
						});
					}
				};
				Foldouts.SubFoldout("Shadows", foldouts, shadowTabButtons, mat, me, shadowTabAction);

				// Reflections
				reflTabButtons.Add(()=>{DoReflReset();}, MGUI.resetLabel);
				Action reflTabAction = ()=>{
					me.ShaderProperty(_Reflections, "Mode");
					if (_Reflections.floatValue > 0){
						MGUI.PropertyGroup(() => {
							GUIContent reflCubeLabel = Tips.reflCubeLabel;
							if (_Reflections.floatValue == 1)
								reflCubeLabel.text = "Fallback Cubemap";
							else
								reflCubeLabel.text = "Cubemap";
							me.TexturePropertySingleLine(reflCubeLabel, _ReflCube);
							me.ShaderProperty(_ReflCol, "Tint");
							me.ShaderProperty(_ReflectionStr, "Strength");
							MGUI.ToggleFloat(me, "Fresnel", _FresnelToggle, _FresnelStrength);
							MGUI.ToggleSlider(me, "Manual Roughness", _ReflUseRough, _ReflRough);
							MGUI.ToggleIntSlider(me, "Stepping", _ReflStepping, _ReflSteps);
							me.ShaderProperty(_LightingBasedIOR, Tips.lightingBasedIOR);
							me.ShaderProperty(_SSR, "SSR");
							if (reflError){
								MGUI.PropertyGroupLayer(() => {
									if (queueError){
										MGUI.DisplayError("SSR requires a render queue of 2501 or above to function correctly.");
									}
									MGUI.DisplayInfo("\nSSR in VRChat requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs\nAnd can only be used on 1 material per scene/avatar.\n\nIt is also is VERY expensive, please use it sparingly!\n");
									me.ShaderProperty(_Alpha, "Strength");
									me.ShaderProperty(_MaxSteps, "Max Steps");
									me.ShaderProperty(_Step, "Step Size");
									me.ShaderProperty(_LRad, "Intersection (L)");
									me.ShaderProperty(_SRad, "Intersection (S)");
									me.ShaderProperty(_EdgeFade, "Edge Fade");
								});
							}
						});
					}
				};
				Foldouts.SubFoldout("Reflections", foldouts, reflTabButtons, mat, me, reflTabAction);

				// Specular
				specTabButtons.Add(()=>{DoSpecReset();}, MGUI.resetLabel);
				Action specTabAction = ()=>{
					me.ShaderProperty(_Specular, "Mode");
					if (_Specular.floatValue > 0){
						if (_Specular.floatValue == 3){
							MGUI.Space6();
							MGUI.DisplayInfo("Note: Use the Specular Blend mask in the masks tab to interpolate between GGX and Anisotropic");
							MGUI.Space6();
						}
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_SpecCol, "Tint");
							if (_Specular.floatValue == 1){
								me.ShaderProperty(_SpecStr, "Strength");
								MGUI.ToggleSlider(me, "Manual Roughness", _SpecUseRough, _SpecRough);
								MGUI.ToggleSlider(me, Tips.specBiasOverride, _SpecBiasOverrideToggle, _SpecBiasOverride);
								MGUI.ToggleIntSlider(me, "Stepping", _SharpSpecular, _SharpSpecStr);
								MGUI.SpaceN1();
								me.ShaderProperty(_RealtimeSpec, Tips.realtimeSpec);
							}
							else if (_Specular.floatValue == 2){
								me.ShaderProperty(_AnisoStr, "Strength");
								me.ShaderProperty(_RealtimeSpec, Tips.realtimeSpec);
								MGUI.ToggleIntSlider(me, "Stepping", _SharpSpecular, _AnisoSteps);
								MGUI.SpaceN1();
							}
							else {
								me.ShaderProperty(_SpecStr, "GGX Strength");
								me.ShaderProperty(_AnisoStr, "Aniso Strength");
								me.ShaderProperty(_RealtimeSpec, Tips.realtimeSpec);
								me.ShaderProperty(_ManualSpecBright, Tips.manualSpecBright);
								me.ShaderProperty(_SharpSpecular, "Stepping");
								if (_SharpSpecular.floatValue == 1){
									me.ShaderProperty(_SharpSpecStr, "GGX Steps");
									me.ShaderProperty(_AnisoSteps, "Aniso Steps");
								}
								MGUI.ToggleGroupEnd();
							}
							if (_Specular.floatValue != 3){
								me.ShaderProperty(_ManualSpecBright, Tips.manualSpecBright);
							}
							if (_Specular.floatValue == 2 || _Specular.floatValue == 3){
								MGUI.PropertyGroupLayer(() => {
									MGUI.SpaceN2();
									me.ShaderProperty(_AnisoAngleY, "Layer 1 Thickness");
									me.ShaderProperty(_AnisoLayerY, "Layer 2 Thickness");
									me.ShaderProperty(_AnisoLayerStr, "Layer Blend");
									me.ShaderProperty(_AnisoLerp, "Lerp Blend");
									MGUI.SpaceN2();
								});
								MGUI.PropertyGroupLayer(() => {
									MGUI.SpaceN2();
									me.ShaderProperty(_RippleStrength, "Hair Strength");
									me.ShaderProperty(_RippleFrequency, "Hair Density");
									me.ShaderProperty(_RippleAmplitude, "Hair Intensity");
									me.ShaderProperty(_RippleContinuity, "Hair Continuity");
									MGUI.SpaceN2();
								});
							}
						});
					}
				};
				Foldouts.SubFoldout("Specular", foldouts, specTabButtons, mat, me, specTabAction);

				// Matcap
				matcapTabButtons.Add(()=>{DoMatcapReset();}, MGUI.resetLabel);
				Action matcapTabAction = ()=>{
					MGUI.ToggleGroup(_MatcapToggle.floatValue == 0);
					MGUI.PropertyGroup(() => {
						bool matcap1Tab = Foldouts.DoSmallFoldout(foldouts, mat, me, "Primary Matcap");
						if (matcap1Tab){
							MGUI.PropertyGroupLayer(() => {
								me.TexturePropertySingleLine(new GUIContent("Matcap"), _Matcap, _MatcapColor, _Matcap.textureValue ? _MatcapBlending : null);
								if (_Matcap.textureValue){
									MGUI.TexPropLabel("Blending", blendingLabelPos);
									MGUI.TextureSO(me, _Matcap);
								};
								me.TexturePropertySingleLine(Tips.matcapNormal, _MatcapNormal0, _MatcapNormal0Str);
								if (_MatcapNormal0.textureValue){
									MGUI.TextureSOScroll(me, _MatcapNormal0, _MatcapNormal0Scroll);
								};
								MGUI.ToggleGroup(_MatcapNormal0.textureValue == false);
								me.ShaderProperty(_MatcapNormal0Mix, Tips.matcapNormalMix);
								MGUI.ToggleGroupEnd();
								MGUI.SpaceN2();
							});
							MGUI.PropertyGroupLayer(() => {
								MGUI.SpaceN2();
								me.ShaderProperty(_MatcapStr, "Strength");
								MGUI.ToggleSlider(me, "Manual Roughness", _MatcapUseRough, _MatcapRough);

								me.ShaderProperty(_UnlitMatcap, "Unlit");
								me.ShaderProperty(_MatcapCenter, "No Depth in VR");
								MGUI.SpaceN2();
							});
						}

						bool matcap2Tab = Foldouts.DoSmallFoldout(foldouts, mat, me, "Secondary Matcap");
						if (matcap2Tab){
							MGUI.PropertyGroupLayer(() => {
								me.TexturePropertySingleLine(new GUIContent("Matcap"), _Matcap1, _MatcapColor1, _Matcap1.textureValue ? _MatcapBlending1 : null);
								if (_Matcap1.textureValue){
									MGUI.TexPropLabel("Blending", blendingLabelPos);
									MGUI.TextureSO(me, _Matcap1);
								}
								me.TexturePropertySingleLine(Tips.matcapNormal, _MatcapNormal1, _MatcapNormal1Str);
								if (_MatcapNormal1.textureValue){
									MGUI.TextureSOScroll(me, _MatcapNormal1, _MatcapNormal1Scroll);
								};
								MGUI.ToggleGroup(_MatcapNormal1.textureValue == false);
								me.ShaderProperty(_MatcapNormal1Mix, Tips.matcapNormalMix);
								MGUI.ToggleGroupEnd();
								MGUI.SpaceN2();
							});
							MGUI.PropertyGroupLayer(() => {
								MGUI.SpaceN2();
								me.ShaderProperty(_MatcapStr1, "Strength");
								MGUI.ToggleSlider(me, "Manual Roughness", _MatcapUseRough1, _MatcapRough1);
								me.ShaderProperty(_UnlitMatcap1, "Unlit");
								me.ShaderProperty(_MatcapCenter1, "No Depth in VR");
								MGUI.ToggleGroupEnd();
								MGUI.SpaceN2();
							});
						}
					});
					MGUI.ToggleGroupEnd();
				};
				Foldouts.SubFoldout("Matcap", foldouts, matcapTabButtons, mat, me, matcapTabAction, _MatcapToggle);

				// Subsurface Scattering
				sssTabButtons.Add(()=>{DoSubsurfReset();}, MGUI.resetLabel);
				Action sssTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_Subsurface.floatValue == 0);
						me.TexturePropertySingleLine(Tips.thicknessTexLabel, _ThicknessMap, _ThicknessMap.textureValue ? _ThicknessMapPower : null);
						if (_ThicknessMap.textureValue)
							MGUI.TexPropLabel("Power", 96);
						me.TexturePropertySingleLine(Tips.colorLabel, _ScatterTex, _ScatterCol, _ScatterBaseColorTint);
						MGUI.TexPropLabel("Base Color Tint", 150);
						me.ShaderProperty(_ScatterIntensity, "Direct Strength");
						me.ShaderProperty(_ScatterAmbient, "Indirect Strength");
						me.ShaderProperty(_ScatterPow, "Power");
						me.ShaderProperty(_ScatterDist, "Normal Strength");
						me.ShaderProperty(_ScatterWrap, "Wrapping Factor");
						MGUI.ToggleGroupEnd();
					});
				};
				Foldouts.SubFoldout("Subsurface", foldouts, sssTabButtons, mat, me, sssTabAction, _Subsurface);

				// Rim
				rimTabButtons.Add(()=>{DoRimReset();}, MGUI.resetLabel);
				Action rimTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_RimLighting.floatValue == 0);
						me.TexturePropertySingleLine(Tips.colorLabel, _RimTex, _RimCol, _RimBlending);
						MGUI.TexPropLabel("Blending", blendingLabelPos);
						MGUI.TextureSOScroll(me, _RimTex, _RimScroll, _RimTex.textureValue);
						me.ShaderProperty(_RimStr, "Strength");
						me.ShaderProperty(_RimWidth, "Width");
						me.ShaderProperty(_RimEdge, "Sharpness");
						me.ShaderProperty(_UnlitRim, "Unlit");
						MGUI.ToggleGroupEnd();
					});
				};
				Foldouts.SubFoldout("Basic Rim", foldouts, rimTabButtons, mat, me, rimTabAction, _RimLighting);

				// Environment Rim
				eRimTabButtons.Add(()=>{DoERimReset();}, MGUI.resetLabel);
				Action eRimTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_EnvironmentRim.floatValue == 0);
						me.ShaderProperty(_ERimBlending, "Blending");
						me.ShaderProperty(_ERimTint, "Tint");
						me.ShaderProperty(_ERimStr, "Strength");
						me.ShaderProperty(_ERimWidth, "Width");
						me.ShaderProperty(_ERimEdge, "Sharpness");
						MGUI.ToggleSlider(me, "Manual Roughness", _ERimUseRough, _ERimRoughness);
						MGUI.ToggleGroupEnd();
					});
				};
				Foldouts.SubFoldout("Environment Rim", foldouts, eRimTabButtons, mat, me, eRimTabAction, _EnvironmentRim);

				// Refraction
				refracTabButtons.Add(()=>{DoRefracReset();}, MGUI.resetLabel);
				Action refracTabAction = ()=>{
					MGUI.ToggleGroup(_Refraction.floatValue == 0);
					MGUI.PropertyGroup(() => {
						if (refracError){
							MGUI.DisplayError("Refraction requires a render queue of 2501 or above to function correctly.");
							MGUI.Space2();
						}
						me.ShaderProperty(_RefractionTint, "Tint");
						me.ShaderProperty(_RefractionIOR, "IOR");
						me.ShaderProperty(_RefractionOpac, "Opacity");
						MGUI.ToggleSlider(me, "Chromatic Abberation", _RefractionCA, _RefractionCAStr);
						MGUI.ToggleSlider(me, "Blur", _RefractionBlur, _RefractionBlurStrength);
						MGUI.ToggleGroup(_RefractionBlur.floatValue == 0);
						me.ShaderProperty(_RefractionBlurRough, "Use Roughness");
						MGUI.ToggleGroupEnd();
						me.ShaderProperty(_UnlitRefraction, "Unlit");
						
					});
					MGUI.PropertyGroup(() => {
						me.TexturePropertySingleLine(new GUIContent("Dissolve Mask"), _RefractionDissolveMask, _RefractionDissolveMaskStr);
						MGUI.TextureSOScroll(me, _RefractionDissolveMask, _RefractionDissolveMaskScroll);
					});
					MGUI.ToggleGroupEnd();
				};
				Foldouts.SubFoldout("Refraction", foldouts, refracTabButtons, mat, me, refracTabAction, refracError, _Refraction);

				// Iridescence
				iriTabButtons.Add(()=>{DoIridescenceReset();}, MGUI.resetLabel);
				Action iriTabAction = ()=>{
					MGUI.ToggleGroup(_Iridescence.floatValue == 0);
					MGUI.PropertyGroup( () => {
						me.TexturePropertySingleLine(Tips.maskLabel, _IridescenceMask);
						me.ShaderProperty(_IridescenceStrength, "Strength");
						me.ShaderProperty(_IridescenceHue, "Hue");
						me.ShaderProperty(_IridescenceWidth, "Width");
						me.ShaderProperty(_IridescenceEdge, "Sharpness");
					});
					MGUI.ToggleGroupEnd();
				};
				Foldouts.SubFoldout("Iridescence", foldouts, iriTabButtons, mat, me, iriTabAction, _Iridescence);

				// Normals
				normalTabButtons.Add(()=>{DoNormalReset();}, MGUI.resetLabel);
				Action normalTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_HardenNormals, "Hard Edges");
						me.ShaderProperty(_ClearCoat, Tips.clearCoat);
						me.ShaderProperty(_GSAA, Tips.gsaa);
					});
				};
				Foldouts.SubFoldout("Normals", foldouts, normalTabButtons, mat, me, normalTabAction);
			};
			Foldouts.Foldout("SHADING", foldouts, shadingTabButtons, mat, me, shadingTabAction, shadingErrors);
		}
		else {
			bigMaskTabButtons.Add(()=>{DoMaskingReset();}, MGUI.resetLabel);
			Action bigMaskTabAction = ()=>{
				me.ShaderProperty(_MaskingMode, "Mode");
				if (_MaskingMode.floatValue == 1){
					MGUI.PropertyGroup(() => {
						me.TexturePropertySingleLine(new GUIContent("Emission"), _EmissMask);
						me.TexturePropertySingleLine(new GUIContent("Emission Pulse"), _PulseMask);
						me.TexturePropertySingleLine(new GUIContent("Filtering"), _FilterMask);	
						me.TexturePropertySingleLine(new GUIContent("Outline Thickness"), _OutlineMask);
					});
				}
				else if (_MaskingMode.floatValue == 2){
					MGUI.PropertyGroup(() => {
						me.TexturePropertySingleLine(new GUIContent("Packed Mask"), _PackedMask3);
						GUILayout.Label("Red:	Emission\nGreen:	Emission Pulse\nBlue:	Filtering\nAlpha:	Outline Thickness");
					});
				}
			};
			Foldouts.Foldout("MASKS", foldouts, bigMaskTabButtons, mat, me, bigMaskTabAction);
		}
		
		// -----------------
		// Emission
		// -----------------
		emissTabButtons.Add(()=>{DoEmissionReset();}, MGUI.resetLabel);
		Action emissTabAction = ()=>{
			me.ShaderProperty(_EmissionToggle, "Enable");
			MGUI.ToggleGroup(_EmissionToggle.floatValue == 0);
			MGUI.PropertyGroup(() => {
				me.TexturePropertySingleLine(Tips.emissTexLabel, _EmissionMap, _EmissionColor, _EmissIntensity);
				MGUI.TexPropLabel("Intensity", 111);
				MGUI.TextureSOScroll(me, _EmissionMap, _EmissScroll);
			});
			MGUI.PropertyGroup(() => {
				me.TexturePropertySingleLine(Tips.emissTexLabel, _EmissionMap2, _EmissionColor2, _EmissIntensity2);
				MGUI.TexPropLabel("Intensity", 111);
				MGUI.TextureSOScroll(me, _EmissionMap2, _EmissScroll2);
			});
			lrTabButtons.Add(()=>{DoLRReset();}, MGUI.resetLabel);
			Action lrTabAction = ()=>{
				MGUI.ToggleGroup(_ReactToggle.floatValue == 0);
				me.ShaderProperty(_CrossMode, Tips.crossMode);
				MGUI.PropertyGroup(() => {
					MGUI.ToggleGroup(_CrossMode.floatValue == 0);
					me.ShaderProperty(_ReactThresh, Tips.reactThresh);
					me.ShaderProperty(_Crossfade, Tips.crossFade);
					MGUI.ToggleGroupEnd();
				});
				MGUI.ToggleGroupEnd();
			};
			Foldouts.SubFoldout("Light Reactivity", foldouts, lrTabButtons, mat, me, lrTabAction, _ReactToggle);

			pulseTabButtons.Add(()=>{DoPulseReset();}, MGUI.resetLabel);
			Action pulseTabAction = ()=>{
				MGUI.ToggleGroup(_PulseToggle.floatValue == 0);
				me.ShaderProperty(_PulseWaveform, "Waveform");
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_PulseStr, "Strength");
					me.ShaderProperty(_PulseSpeed, "Speed");
				});
				MGUI.ToggleGroupEnd();
			};
			Foldouts.SubFoldout("Pulse", foldouts, pulseTabButtons, mat, me, pulseTabAction, _PulseToggle);
			MGUI.ToggleGroupEnd();
		};
		Foldouts.Foldout("EMISSION 0", foldouts, emissTabButtons, mat, me, emissTabAction);
			
		// -----------------
		// Filters
		// -----------------
		filterTabButtons.Add(()=>{DoFiltersReset();}, MGUI.resetLabel);
		Action filterTabAction = ()=>{
			me.ShaderProperty(_Filtering, "Enable");
			MGUI.ToggleGroup(_Filtering.floatValue == 0);
			MGUI.PropertyGroup(() => {
				me.ShaderProperty(_TeamFiltering, "Color Masking");
				me.ShaderProperty(_PostFiltering, Tips.postFiltering);
				me.ShaderProperty(_Invert, "Invert");
				me.ShaderProperty(_AutoShift, "Auto Hue Shift");
			});
			MGUI.PropertyGroup(() => {
				if (_AutoShift.floatValue == 0)
					me.ShaderProperty(_Hue, "Hue");
				else
					me.ShaderProperty(_AutoShiftSpeed, "Shift Speed");
				MGUI.Vector3FieldRGB(_RGB, "RGB Multiplier");
				me.ShaderProperty(_Saturation, "Saturation");
				me.ShaderProperty(_Brightness, "Brightness");
				me.ShaderProperty(_Contrast, "Contrast");
				me.ShaderProperty(_HDR, "HDR");
				me.ShaderProperty(_ACES, Tips.aces);
			});
			if (_TeamFiltering.floatValue == 1){
				MGUI.PropertyGroup(() => {
					me.TexturePropertySingleLine(new GUIContent("Color Mask"), _TeamColorMask);
					me.ShaderProperty(_TeamColor0, "Red Channel");
					me.ShaderProperty(_TeamColor1, "Green Channel");
					me.ShaderProperty(_TeamColor2, "Blue Channel");
					me.ShaderProperty(_TeamColor3, "Alpha Channel");
				});
			}
			MGUI.ToggleGroupEnd();
		};
		Foldouts.Foldout("FILTERING", foldouts, filterTabButtons, mat, me, filterTabAction);

		// -----------------
		// Flipbook
		// -----------------
		flipbookTabButtons.Add(()=>{DoSpriteReset();}, MGUI.resetLabel);
		Action flipbookTabAction = ()=>{
			book0TabButtons.Add(()=>{DoSheet1Reset();}, MGUI.resetLabel);
			book0TabButtons.Add(()=>{CopyToSheet2();}, copyTo2Label);
			Action book0TabAction = ()=>{
				MGUI.ToggleGroup(_EnableSpritesheet.floatValue == 0);
				me.ShaderProperty(_SpritesheetMode0, "Mode");
				MGUI.PropertyGroup(() => {
					if (_SpritesheetMode0.floatValue == 1)
						me.TexturePropertySingleLine(new GUIContent("Sprite Sheet"), _Spritesheet, _SpritesheetCol, _SpritesheetBlending);
					else
						me.TexturePropertySingleLine(new GUIContent("Flipbook Asset"), _Flipbook0, _SpritesheetCol, _SpritesheetBlending);
					MGUI.TexPropLabel("Blending", blendingLabelPos);
					me.ShaderProperty(_SpritesheetBrightness, "Brightness");
					me.ShaderProperty(_UnlitSpritesheet, "Unlit");
					me.ShaderProperty(_UseSpritesheetAlpha, "Use Alpha");
					if (_SpritesheetMode0.floatValue == 1){
						MGUI.Vector2Field(_RowsColumns, "Columns / Rows");
						MGUI.Vector2Field(_FrameClipOfs, "Frame Size");
					}
					else {
						me.ShaderProperty(_Flipbook0ClipEdge, Tips.clipEdge);
					}
					MGUI.Vector2Field(_SpritesheetPos, "Position");
					MGUI.Vector2Field(_SpritesheetScale, "Scale");
					MGUI.Vector2Field(_Flipbook0Scroll, "Scrolling");
					me.ShaderProperty(_SpritesheetRot, "Rotation");
					MGUI.ToggleGroup(_ManualScrub.floatValue == 1);
					me.ShaderProperty(_FPS, "Speed");
					MGUI.ToggleGroupEnd();
					float frameCount0 = (_RowsColumns.vectorValue.x * _RowsColumns.vectorValue.y)-1;
					if (_SpritesheetMode0.floatValue == 0 && _Flipbook0.textureValue){
						Texture2DArray t2da0 = (Texture2DArray)_Flipbook0.textureValue;
						frameCount0 = t2da0.depth;
					}
					MGUI.CustomToggleSlider("Frame", _ManualScrub, _ScrubPos, 0f, frameCount0);
					if (_ManualScrub.floatValue == 1 && _RowsColumns.vectorValue.x == 0 && _RowsColumns.vectorValue.y == 0 && _SpritesheetMode0.floatValue == 1)
						MGUI.DisplayWarning("Manual frame scrubbing will not behave correctly when rows and columns are both set to 0.");
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.SubFoldout("Primary Layer", foldouts, book0TabButtons, mat, me, book0TabAction, _EnableSpritesheet);

			book1TabButtons.Add(()=>{DoSheet2Reset();}, MGUI.resetLabel);
			book1TabButtons.Add(()=>{CopyToSheet1();}, copyTo1Label);
			Action book1TabAction = ()=>{
				MGUI.ToggleGroup(_EnableSpritesheet1.floatValue == 0);
				me.ShaderProperty(_SpritesheetMode1, "Mode");
				MGUI.PropertyGroup(() => {
					if (_SpritesheetMode1.floatValue == 1)
						me.TexturePropertySingleLine(new GUIContent("Sprite Sheet"), _Spritesheet1, _SpritesheetCol1, _SpritesheetBlending1);
					else
						me.TexturePropertySingleLine(new GUIContent("Flipbook Asset"), _Flipbook1, _SpritesheetCol1, _SpritesheetBlending1);
					MGUI.TexPropLabel("Blending", blendingLabelPos);
					me.ShaderProperty(_SpritesheetBrightness1, "Brightness");
					me.ShaderProperty(_UnlitSpritesheet1, "Unlit");
					me.ShaderProperty(_UseSpritesheetAlpha, "Use Alpha");
					if (_SpritesheetMode1.floatValue == 1){
						MGUI.Vector2Field(_RowsColumns1, "Columns / Rows");
						MGUI.Vector2Field(_FrameClipOfs1, "Frame Size");
					}
					else {
						me.ShaderProperty(_Flipbook1ClipEdge, Tips.clipEdge);
					}
					MGUI.Vector2Field(_SpritesheetPos1, "Position");
					MGUI.Vector2Field(_SpritesheetScale1, "Scale");
					MGUI.Vector2Field(_Flipbook1Scroll, "Scrolling");
					me.ShaderProperty(_SpritesheetRot1, "Rotation");
					MGUI.ToggleGroup(_ManualScrub1.floatValue == 1);
					me.ShaderProperty(_FPS1, "Speed");
					MGUI.ToggleGroupEnd();
					float frameCount1 = (_RowsColumns1.vectorValue.x * _RowsColumns1.vectorValue.y)-1;
					if (_SpritesheetMode0.floatValue == 0 && _Flipbook1.textureValue){
						Texture2DArray t2da1 = (Texture2DArray)_Flipbook1.textureValue;
						frameCount1 = t2da1.depth;
					}
					MGUI.CustomToggleSlider("Frame", _ManualScrub1, _ScrubPos1, 0f, frameCount1);
					if (_ManualScrub1.floatValue == 1 && _RowsColumns1.vectorValue.x == 0 && _RowsColumns1.vectorValue.y == 0 && _SpritesheetMode1.floatValue == 1)
						MGUI.DisplayWarning("Manual frame scrubbing will not behave correctly when rows and columns are both set to 0.");
					
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.SubFoldout("Secondary Layer", foldouts, book1TabButtons, mat, me, book1TabAction, _EnableSpritesheet1);
		};
		Foldouts.Foldout("FLIPBOOK", foldouts, flipbookTabButtons, mat, me, flipbookTabAction);

		// -----------------
		// Outline
		// -----------------
		if (isOutline){
			outlineTabButtons.Add(()=>{DoOutlineReset();}, MGUI.resetLabel);
			Action outlineTabAction = ()=>{
				if (isTransparent || blendMode == 3){
					MGUI.Space2();
					MGUI.DisplayError("Requires Opaque, Cutout, or Dithered blending mode to function.");
					MGUI.Space6();
				}
				MGUI.ToggleGroup(isTransparent || blendMode == 3);
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_ApplyOutlineLighting, Tips.applyOutlineLighting);
					MGUI.ToggleGroup(_EmissionToggle.floatValue == 0);
					me.ShaderProperty(_ApplyOutlineEmiss, Tips.applyOutlineEmiss);
					MGUI.ToggleGroupEnd();
					me.ShaderProperty(_ApplyAlbedoTint, Tips.applyAlbedoTint);
					EditorGUI.BeginChangeCheck();
					me.ShaderProperty(_StencilToggle, Tips.stencilMode);
					if (EditorGUI.EndChangeCheck()){
						if (_StencilToggle.floatValue == 0)
							DoAdvancedReset();
						else
							ApplyOutlineStencilConfig(mat);
					}
					me.ShaderProperty(_IgnoreFilterMask, Tips.ignoreFilterMask);
					me.ShaderProperty(_UseVertexColor, Tips.useVertexColor);
				});
				MGUI.PropertyGroup(() => {
					me.TexturePropertySingleLine(Tips.colorLabel, _OutlineTex, _OutlineCol, (blendMode == 1 || blendMode == 2) && _OutlineTex.textureValue ? _UseOutlineTexAlpha : null);
					if ((blendMode == 1 || blendMode == 2) && _OutlineTex.textureValue){
						MGUI.TexPropLabel("Use Alpha", 115);
					}
					MGUI.TextureSOScroll(me, _OutlineTex, _OutlineScroll, _OutlineTex.textureValue);
					me.ShaderProperty(_OutlineMult, "Thickness");
					me.ShaderProperty(_OutlineThicc, "Multiplier");
					me.ShaderProperty(_OutlineRange, Tips.outlineRange);
				});
				MGUI.ToggleGroupEnd();
			};
			Foldouts.Foldout("OUTLINE", foldouts, outlineTabButtons, mat, me, outlineTabAction);
		}

		// -----------------
		// UV Distortion
		// -----------------
		uvdTabButtons.Add(()=>{DoUVDReset();}, MGUI.resetLabel);
		Action uvdTabAction = ()=>{
			me.ShaderProperty(_DistortionStyle, "Mode");
			if (_DistortionStyle.floatValue > 0){
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_DistortMainUV, "Main");
					me.ShaderProperty(_DistortDetailUV, "Detail");
					me.ShaderProperty(_DistortEmissUV, "Emission");
					me.ShaderProperty(_DistortRimUV, "Rim");
					me.ShaderProperty(_DistortMatcap0, "Primary Matcap");
					me.ShaderProperty(_DistortMatcap1, "Secondary Matcap");
				});
				if (_DistortionStyle.floatValue == 1){
					MGUI.PropertyGroup(() => {
						me.TexturePropertySingleLine(Tips.maskLabel, _DistortUVMask);
						me.TexturePropertySingleLine(Tips.normalTexLabel, _DistortUVMap, _DistortUVMap.textureValue ? _DistortUVStr : null);
						if (_DistortUVMap.textureValue)
							MGUI.TexPropLabel("Strength", 110);
						MGUI.TextureSOScroll(me, _DistortUVMap, _DistortUVScroll);
					});	
				}
				else if (_DistortionStyle.floatValue == 2) {
					MGUI.PropertyGroup(() => {
						me.TexturePropertySingleLine(Tips.maskLabel, _DistortUVMask);
						me.ShaderProperty(_NoiseOctaves, "Octaves");
						me.ShaderProperty(_DistortUVStr, "Strength");
						me.ShaderProperty(_NoiseSpeed, "Speed");
						MGUI.Vector2Field(_NoiseScale, "Scale");
					});
				}
			}
		};
		Foldouts.Foldout("UV DISTORTION 0", foldouts, uvdTabButtons, mat, me, uvdTabAction);

		// -----------------
		// Vertex Manip
		// -----------------
		vertTabButtons.Add(()=>{DoVertexReset();}, MGUI.resetLabel);
		Action vertTabAction = ()=>{
			me.ShaderProperty(_VertexManipulationToggle, "Enable");
			MGUI.ToggleGroup(_VertexManipulationToggle.floatValue == 0);
			MGUI.PropertyGroup(() => {
				me.TexturePropertySingleLine(Tips.maskLabel, _VertexExpansionMask);
				MGUI.Vector3Field(_VertexExpansion, "Expansion", false);
				me.ShaderProperty(_VertexExpansionClamp, "Clamp Direction");
			});
			MGUI.PropertyGroup(() => {
				me.TexturePropertySingleLine(Tips.maskLabel, _VertexRoundingMask);
				me.ShaderProperty(_VertexRounding, "Position Rounding");
				me.ShaderProperty(_VertexRoundingPrecision, "Precision");
			});
			MGUI.PropertyGroup(() => {
				MGUI.Vector3Field(_VertexRotation, "Rotation", false);
				MGUI.Vector3Field(_VertexPosition, "Position", false);
			});
			MGUI.ToggleGroupEnd();
		};
		Foldouts.Foldout("VERTEX MANIPULATION 0", foldouts, vertTabButtons, mat, me, vertTabAction);

		// -----------------
		// Audio Link
		// -----------------
		alTabButtons.Add(()=>{DoAudioLinkReset();}, MGUI.resetLabel);
		Action alTabAction = ()=>{
			MGUI.ToggleSlider(me, Tips.audioLinkEmission, _AudioLinkToggle, _AudioLinkStrength);
			MGUI.ToggleGroup(_AudioLinkToggle.floatValue == 0);
			MGUI.SliderMinMax(_AudioLinkRemapMin, _AudioLinkRemapMax, 0f, 2f, "Remap", 0);
			MGUI.Space6();

			alVizTabButtons.Add(()=>{DoAudioLinkVizReset();}, MGUI.resetLabel);
			Action alVizTabAction = ()=>{
				// MGUI.BoldLabel("Oscilloscope");
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_OscilloscopeStrength, "Strength");
					me.ShaderProperty(_OscilloscopeCol, "Color");
					MGUI.Vector2Field(_OscilloscopeMarginLR, "Margin Left/Right");
					MGUI.Vector2Field(_OscilloscopeMarginTB, "Margin Top/Bottom");
					MGUI.Vector2Field(_OscilloscopeScale, "Scale");
					MGUI.Vector2Field(_OscilloscopeOffset, "Offset");
					me.ShaderProperty(_OscilloscopeRot, "Rotation");
				});	
			};
			Foldouts.SubFoldout("Oscilloscope", foldouts, alVizTabButtons, mat, me, alVizTabAction);

			alEmissTabButtons.Add(()=>{DoAudioLinkEmissionReset();}, MGUI.resetLabel);
			Action alEmissTabAction = ()=>{
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_AudioLinkEmissionBand, "Band");
					me.ShaderProperty(_AudioLinkEmissionMultiplier, "Strength");
					MGUI.SliderMinMax(_AudioLinkRemapEmissionMin, _AudioLinkRemapEmissionMax, 0f, 2f, "Remap", 1);
				});	
			};
			Foldouts.SubFoldout("Emission 1", foldouts, alEmissTabButtons, mat, me, alEmissTabAction);

			alRimTabButtons.Add(()=>{DoAudioLinkRimReset();}, MGUI.resetLabel);
			Action alRimTabAction = ()=>{
				me.ShaderProperty(_AudioLinkRimBand, "Band");
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_AudioLinkRimMultiplier, "Strength");
					MGUI.SliderMinMax(_AudioLinkRemapRimMin, _AudioLinkRemapRimMax, 0f, 2f, "Remap", 1);
					me.ShaderProperty(_AudioLinkRimWidth, "Width");
				});
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_AudioLinkRimPulse, "Pulse Strength");
					MGUI.SliderMinMax(_AudioLinkRemapRimPulseMin, _AudioLinkRemapRimPulseMax, 0f, 2f, "Pulse Remap", 1);
					me.ShaderProperty(_AudioLinkRimPulseWidth, "Pulse Width");
					me.ShaderProperty(_AudioLinkRimPulseSharp, "Pulse Sharpness");
				});
			};
			Foldouts.SubFoldout("Rim", foldouts, alRimTabButtons, mat, me, alRimTabAction);

			albcdTabButtons.Add(()=>{DoAudioLinkBCDissolveReset();}, MGUI.resetLabel);
			Action albcdTabAction = ()=>{
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_AudioLinkBCDissolveBand, "Band");
					me.ShaderProperty(_AudioLinkBCDissolveMultiplier, "Strength");
					MGUI.SliderMinMax(_AudioLinkRemapBCDissolveMin, _AudioLinkRemapBCDissolveMax, 0f, 2f, "Remap", 1);
				});
			};
			Foldouts.SubFoldout("Base Color Dissolve 1", foldouts, albcdTabButtons, mat, me, albcdTabAction);
			
			aluvdTabButtons.Add(()=>{DoAudioLinkUVDistortionReset();}, MGUI.resetLabel);
			Action aluvdTabAction = ()=>{
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_AudioLinkUVDistortionBand, "Band");
					me.ShaderProperty(_AudioLinkUVDistortionMultiplier, "Strength");
					MGUI.SliderMinMax(_AudioLinkRemapUVDistortionMin, _AudioLinkRemapUVDistortionMax, 0f, 2f, "Remap", 1);
				});
			};
			Foldouts.SubFoldout("UV Distortion 1", foldouts, aluvdTabButtons, mat, me, aluvdTabAction);

			if (isOutline){
				alOutlineTabButtons.Add(()=>{DoAudioLinkOutlineReset();}, MGUI.resetLabel);
				Action alOutlineTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_AudioLinkOutlineBand, "Band");
						me.ShaderProperty(_AudioLinkOutlineMultiplier, "Strength");
						MGUI.SliderMinMax(_AudioLinkRemapOutlineMin, _AudioLinkRemapOutlineMax, 0f, 2f, "Remap", 1);
					});
				};
				Foldouts.SubFoldout("Outline 1", foldouts, alOutlineTabButtons, mat, me, alOutlineTabAction);
			}

			if (isUberX){
				alVertManipTabButtons.Add(()=>{DoAudioLinkVertManipReset();}, MGUI.resetLabel);
				Action alVertManipTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_AudioLinkVertManipBand, "Band");
						me.ShaderProperty(_AudioLinkVertManipMultiplier, "Strength");
						MGUI.SliderMinMax(_AudioLinkRemapMin, _AudioLinkRemapMax, 0f, 2f, "Remap", 1);
					});
				};
				Foldouts.SubFoldout("Vertex Manipulation 1", foldouts, alVertManipTabButtons, mat, me, alVertManipTabAction);

				
				alDissolveTabButtons.Add(()=>{DoAudioLinkDissolveReset();}, MGUI.resetLabel);
				Action alDissolveTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_AudioLinkDissolveBand, "Band");
						me.ShaderProperty(_AudioLinkDissolveMultiplier, "Strength");
						MGUI.SliderMinMax(_AudioLinkRemapDissolveMin, _AudioLinkRemapDissolveMax, 0f, 2f, "Remap", 1);
					});
				};
				Foldouts.SubFoldout("Dissolve 1", foldouts, alDissolveTabButtons, mat, me, alDissolveTabAction);

				
				alTriOffsTabButtons.Add(()=>{DoAudioLinkTriOffsetReset();}, MGUI.resetLabel);
				Action alTriOffsTabAction = ()=>{
					MGUI.PropertyGroup( () => {
						me.ShaderProperty(_AudioLinkTriOffsetBand, "Band");
						me.ShaderProperty(_AudioLinkTriOffsetMode, "Style");
						me.ShaderProperty(_AudioLinkTriOffsetCoords, "Axis");
						me.TexturePropertySingleLine(Tips.maskLabel, _AudioLinkTriOffsetMask);
						MGUI.TextureSOScroll(me, _AudioLinkTriOffsetMask, _AudioLinkTriOffsetMaskScroll, _AudioLinkTriOffsetMask.textureValue);
					});
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_AudioLinkTriOffsetStrength, "Strength");
						MGUI.SliderMinMax(_AudioLinkRemapTriOffsetMin, _AudioLinkRemapTriOffsetMax, 0f, 2f, "Remap", 1);
						me.ShaderProperty(_AudioLinkTriOffsetStartPos, "Start Position");
						me.ShaderProperty(_AudioLinkTriOffsetEndPos, "End Position");
						me.ShaderProperty(_AudioLinkTriOffsetSize, "Size");
					});
				};
				Foldouts.SubFoldout("Triangle Offset", foldouts, alTriOffsTabButtons, mat, me, alTriOffsTabAction);

				alWireframeTabButtons.Add(()=>{DoAudioLinkWireframeReset();}, MGUI.resetLabel);
				Action alWireframeTabAction = ()=>{
					MGUI.PropertyGroup( () => {
						me.ShaderProperty(_AudioLinkWireframeBand, "Band");
						me.ShaderProperty(_AudioLinkWireframeMode, "Style");
						me.ShaderProperty(_AudioLinkWireframeCoords, "Axis");
						me.TexturePropertySingleLine(Tips.maskLabel, _AudioLinkWireframeMask);
						MGUI.TextureSOScroll(me, _AudioLinkWireframeMask, _AudioLinkWireframeMaskScroll, _AudioLinkWireframeMask.textureValue);
					});
					MGUI.PropertyGroup( () => {
						me.ShaderProperty(_AudioLinkWireframeStrength, "Strength");
						MGUI.SliderMinMax(_AudioLinkRemapWireframeMin, _AudioLinkRemapWireframeMax, 0f, 2f, "Remap", 1);
						me.ShaderProperty(_AudioLinkWireframeStartPos, "Start Position");
						me.ShaderProperty(_AudioLinkWireframeEndPos, "End Position");
						me.ShaderProperty(_AudioLinkWireframeSize, "Size");
						me.ShaderProperty(_AudioLinkWireframeColor, "Color");
					});
				};
				Foldouts.SubFoldout("Wireframe 1", foldouts, alWireframeTabButtons, mat, me, alWireframeTabAction);
			}
			MGUI.ToggleGroupEnd();
		};
		Foldouts.Foldout("AUDIO LINK", foldouts, alTabButtons, mat, me, alTabAction);

		// -----------------
		// X Features
		// -----------------
		if (isUberX){
			bool dfError = blendMode == 0 && _DistanceFadeToggle.floatValue > 0;
			bool dissError = blendMode == 0 && _DissolveStyle.floatValue > 0 && _DissolveStyle.floatValue < 3;
			bool[] specErrors = {dfError, dissError};
			
			specialTabButtons.Add(()=>{DoSpecialReset();}, MGUI.resetLabel);
			Action specialTabAction = ()=>{
				MGUI.SpaceN6();
				
				dfTabButtons.Add(()=>{DoDFReset();}, MGUI.resetLabel);
				Action dfTabAction = ()=>{
					me.ShaderProperty(_DistanceFadeToggle, "Mode");
					MGUI.Space2();
					if (_DistanceFadeToggle.floatValue > 0){
						if (dfError){
							MGUI.DisplayError("Requires non-opaque blending mode to function.");
						}
						MGUI.Space2();
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_DistanceFadeToggle.floatValue == 0 || blendMode == 0);
							MGUI.ToggleGroup(_CloneToggle.floatValue == 0);
							me.ShaderProperty(_DFClones, "Clones Only");
							MGUI.ToggleGroupEnd();
							switch ((int)_DistanceFadeToggle.floatValue){
								case 1: 
									me.ShaderProperty(_DistanceFadeMin, "Range");
									me.ShaderProperty(_ClipRimColor, "Rim Color");
									me.ShaderProperty(_ClipRimStr, "Intensity");
									me.ShaderProperty(_ClipRimWidth, "Width"); 
									break; 
								case 2: 
									me.ShaderProperty(_DistanceFadeMin, "Min Range"); 
									me.ShaderProperty(_DistanceFadeMax, "Max Range");
									break;
								default: break;
							}
							MGUI.ToggleGroupEnd();
						});
					}
				};
				Foldouts.SubFoldout("Distance Fade", foldouts, dfTabButtons, mat, me, dfTabAction, dfError);

				dissTabButtons.Add(()=>{DoDissolveReset();}, MGUI.resetLabel);
				Action dissTabAction = ()=>{
					me.ShaderProperty(_DissolveStyle, "Mode");
					if (_DissolveStyle.floatValue > 0){
						if (dissError){
							MGUI.DisplayError("Texture and simplex dissolve require a non-opaque blending mode to function.");
						}
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(dissError);
							if (_DissolveStyle.floatValue < 3){
								me.ShaderProperty(_DissolveAmount, "Strength");
							}
							else {
								me.ShaderProperty(_GeomDissolveAxis, "Axis");
								if (_GeomDissolveAxis.floatValue <= 2)
									me.ShaderProperty(_GeomDissolveAxisFlip, "Invert Axis");
								me.ShaderProperty(_GeomDissolveWireframe, "Apply Wireframe");
								me.ShaderProperty(_GeomDissolveClamp, "Clamp Offset Direction");
								MGUI.ToggleGroup(_CloneToggle.floatValue == 0);
								me.ShaderProperty(_DissolveClones, "Clones Only");
								MGUI.ToggleGroupEnd();
								me.ShaderProperty(_GeomDissolveAmount, "Clip Position");
								me.ShaderProperty(_GeomDissolveWidth, "Falloff Size");
								if (_GeomDissolveAxis.floatValue > 2){
									MGUI.Vector3Field(_DissolvePoint0, "Point 1", false);
									MGUI.Vector3Field(_DissolvePoint1, "Point 2", false);
								}
								MGUI.Vector3Field(_GeomDissolveSpread, "Offset Amount", false);
								me.ShaderProperty(_GeomDissolveClip, "Offset Clip");
								me.ShaderProperty(_GeomDissolveFilter, "Offset Filter");
							}

							if (_DissolveStyle.floatValue < 3){
								if (_DissolveStyle.floatValue == 1)
									MGUI.ToggleSlider(me, "Flow", _DissolveBlending, _DissolveBlendSpeed);
								else if (_DissolveStyle.floatValue == 2){
									me.ShaderProperty(_DissolveBlendSpeed, "Generation Speed");
									MGUI.Vector3Field(_DissolveNoiseScale, "Noise Scale", false);
								}
								MGUI.ToggleGroup(_CloneToggle.floatValue == 0);
								me.ShaderProperty(_DissolveClones, "Clones Only");
								MGUI.ToggleGroupEnd();
								me.TexturePropertySingleLine(Tips.maskLabel, _DissolveMask);
								if (_DissolveStyle.floatValue == 1){
									me.TexturePropertySingleLine(Tips.dissolveTexLabel, _DissolveTex);
									MGUI.TextureSOScroll(me, _DissolveTex, _DissolveScroll0, _DissolveTex.textureValue);
								}
								me.ShaderProperty(_DissolveRimCol, "Rim Color");
								me.ShaderProperty(_DissolveRimWidth, "Rim Width");
							}
							MGUI.ToggleGroupEnd();
						});
					}
				};
				Foldouts.SubFoldout("Dissolve 0", foldouts, dissTabButtons, mat, me, dissTabAction, dissError);
				
				// Screenspace
				ssTabButtons.Add(()=>{DoScreenReset();}, MGUI.resetLabel);
				Action ssTabAction = ()=>{
					MGUI.ToggleGroup(_Screenspace.floatValue == 0);
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_Range, "Range");
						MGUI.Vector3Field(_Position, "Position", false);
						Vector3 v = _Position.vectorValue;
						v.z = Mathf.Clamp(v.z, 0, 10000);
						_Position.vectorValue = v;
						MGUI.Vector3Field(_Rotation, "Rotation", false);
						MGUI.DoResetButton(_Position, _Rotation, new Vector4(0,0,0.25f,0), new Vector4(0,0,0,0));
					});
					MGUI.ToggleGroupEnd();
				};
				Foldouts.SubFoldout("Screenspace", foldouts, ssTabButtons, mat, me, ssTabAction, _Screenspace);

				// Clones
				cloneTabButtons.Add(()=>{DoCloneReset();}, MGUI.resetLabel);
				Action cloneTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_CloneToggle.floatValue == 0);
						me.ShaderProperty(_Visibility, "Enable");
						MGUI.Vector3Field(_EntryPos, "Offset Direction", false);
						me.ShaderProperty(_SaturateEP, "Clamp Offset Direction");
						EditorGUI.BeginChangeCheck();
						me.ShaderProperty(_ClonePattern, "Pattern Preset");
						if (EditorGUI.EndChangeCheck())
							ApplyClonePositions();
						bool positionsFoldout = Foldouts.DoSmallFoldout(foldouts, mat, me, "Positions");
						if (positionsFoldout){
							MGUI.PropertyGroupLayer(() => {
								MGUI.Vector3FieldToggleW("Clone 1", (int)_Clone1.vectorValue.w, _Clone1);
								MGUI.Vector3FieldToggleW("Clone 2", (int)_Clone2.vectorValue.w, _Clone2);
								MGUI.Vector3FieldToggleW("Clone 3", (int)_Clone3.vectorValue.w, _Clone3);
								MGUI.Vector3FieldToggleW("Clone 4", (int)_Clone4.vectorValue.w, _Clone4);
								MGUI.Vector3FieldToggleW("Clone 5", (int)_Clone5.vectorValue.w, _Clone5);
								MGUI.Vector3FieldToggleW("Clone 6", (int)_Clone6.vectorValue.w, _Clone6);
								MGUI.Vector3FieldToggleW("Clone 7", (int)_Clone7.vectorValue.w, _Clone7);
								MGUI.Vector3FieldToggleW("Clone 8", (int)_Clone8.vectorValue.w, _Clone8);
								MGUI.SpaceN4();
							});
						}
						MGUI.ToggleGroupEnd();
					});
				};
				Foldouts.SubFoldout("Clones", foldouts, cloneTabButtons, mat, me, cloneTabAction, _CloneToggle);

				// Glitch
				glitchTabButtons.Add(()=>{DoGlitchReset();}, MGUI.resetLabel);
				Action glitchTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_GlitchToggle.floatValue == 0);
						MGUI.ToggleGroup(_CloneToggle.floatValue == 0);
						me.ShaderProperty(_GlitchClones, "Clones Only");
						MGUI.ToggleGroupEnd();
						me.ShaderProperty(_Instability, "Instability");
						me.ShaderProperty(_GlitchIntensity, "Intensity");
						me.ShaderProperty(_GlitchFrequency, "Frequency");
						MGUI.ToggleGroupEnd();
					});
				};
				Foldouts.SubFoldout("Glitch", foldouts, glitchTabButtons, mat, me, glitchTabAction, _GlitchToggle);

				// Shatter Culling
				shatterTabButtons.Add(()=>{DoShatterReset();}, MGUI.resetLabel);
				Action shatterTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_ShatterToggle.floatValue == 0);
						MGUI.ToggleGroup(_CloneToggle.floatValue == 0);
						me.ShaderProperty(_ShatterClones, "Clones Only");
						MGUI.ToggleGroupEnd();
						me.ShaderProperty(_ShatterSpread, "Spread");
						me.ShaderProperty(_ShatterMin, "Min Range");
						me.ShaderProperty(_ShatterMax, "Max Range");
						me.ShaderProperty(_ShatterCull, "Culling Range");
						MGUI.ToggleGroupEnd();
					});
				};
				Foldouts.SubFoldout("Shatter Culling", foldouts, shatterTabButtons, mat, me, shatterTabAction, _ShatterToggle);

				// Wireframe
				wfTabButtons.Add(()=>{DoAudioLinkReset();}, MGUI.resetLabel);
				Action wfTabAction = ()=>{
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_WireframeToggle.floatValue == 0);
						MGUI.ToggleGroup(_CloneToggle.floatValue == 0);
						me.ShaderProperty(_WFClones, "Clones Only");
						MGUI.ToggleGroupEnd();
						me.ShaderProperty(_WFMode, "Pattern");
						me.ShaderProperty(_WFColor, "Color");
						me.ShaderProperty(_WFVisibility, "Wire Opacity");
						me.ShaderProperty(_WFFill, "Fill Opacity");
						if (isTransparent)
							me.ShaderProperty(_WireframeTransparency, "Use Alpha");
						MGUI.ToggleGroupEnd();
					});
				};
				Foldouts.SubFoldout("Wireframe 0", foldouts, wfTabButtons, mat, me, wfTabAction, _WireframeToggle);
			};
			Foldouts.Foldout("SPECIAL FEATURES", foldouts, specialTabButtons, mat, me, specialTabAction, specErrors);
		}

		// -----------------
		// Rendering
		// -----------------
		renderTabButtons.Add(()=>{DoAdvancedReset();}, MGUI.resetLabel);
		Action renderTabAction = ()=>{
			MGUI.BoldLabel("General");
			MGUI.PropertyGroup(() => {
				me.ShaderProperty(_CullingMode, Tips.cullingModeLabel);
				me.ShaderProperty(_ZWrite, "ZWrite");
				me.ShaderProperty(_ZTest, "ZTest");
				EditorGUI.BeginChangeCheck();
				me.ShaderProperty(_VRCFallback, "Fallback Shader");
				if (EditorGUI.EndChangeCheck())
					SetFallback(mat);
				me.ShaderProperty(_Hide, "Invisible");
			});
			MGUI.BoldLabel("Near Clipping");
			MGUI.PropertyGroup(() => {
				me.ShaderProperty(_NearClipToggle, Tips.nearClipLabel);
				MGUI.Space4();
				MGUI.ToggleGroup(_NearClipToggle.floatValue == 0);
				me.TexturePropertySingleLine(Tips.maskLabel, _NearClipMask);
				me.ShaderProperty(_NearClip, "Range");
				MGUI.ToggleGroupEnd();
			});
			GUILayout.Space(23);
			if (MGUI.MedTabButton(MGUI.resetLabel, 23f)){
				DoStencilReset();
			}
			GUILayout.Space(-18);
			MGUI.BoldLabel("Stencil");
			MGUI.PropertyGroup(() => {
				me.ShaderProperty(_StencilRef, "Reference Value");
				me.ShaderProperty(_StencilPass, "Pass Operation");
				me.ShaderProperty(_StencilFail, "Fail Operation");
				me.ShaderProperty(_StencilZFail, "Z Fail Operation");
				me.ShaderProperty(_StencilCompare, "Compare Function");
				if (isOutline){
					MGUI.ToggleGroup(_StencilToggle.floatValue == 0);
					if (MGUI.SimpleButton("Reapply Outline Configuration", MGUI.GetPropertyWidth(), EditorGUIUtility.labelWidth)){
						ApplyOutlineStencilConfig(mat);
					}
					MGUI.ToggleGroupEnd();
				}
			});
		};
		Foldouts.Foldout("RENDER SETTINGS", foldouts, renderTabButtons, mat, me, renderTabAction);

		if (isUberX){
			MGUI.Space10();
			MGUI.DisplayWarning("Please note that the X version of this shader is, by default, more expensive than the non X version. If you are not using anything from the Special Features tab it is HIGHLY recommended to use the normal Uber shader instead, as there is no visual difference between the two.");
		}
		MGUI.DoFooter(versionLabel);
    }

	private void GradientToTexture(ref Texture2D tex){
		for (int x = 0; x < tex.width; x++){
			Color col = gradientObj.gradient.Evaluate((float)x / tex.width);
			for (int y = 0; y < tex.height; y++) tex.SetPixel(x, y, col);
		}
		tex.Apply();
	}

	void SetKeyword(Material m, string keyword, bool state) {
		if (state)
			m.EnableKeyword(keyword);
		else
			m.DisableKeyword(keyword);
	}

	void ApplyMaterialSettings(Material mat){
		int renderMode = mat.GetInt("_RenderMode");
		int blendMode = mat.GetInt("_BlendMode");
		int cullingMode = mat.GetInt("_CullingMode");
		int cubeMode = mat.GetInt("_CubeMode");
		int maskingMode = mat.GetInt("_MaskingMode");
		int distortMode = mat.GetInt("_DistortionStyle");
		int reflToggle = mat.GetInt("_Reflections");
		int specToggle = mat.GetInt("_Specular");
		int filterToggle = mat.GetInt("_Filtering");
		int workflow = mat.GetInt("_PBRWorkflow");
		int ssr = mat.GetInt("_SSR");
		int pulseToggle = mat.GetInt("_PulseToggle");
		int emissToggle = mat.GetInt("_EmissionToggle");
		int dissolveStyle = mat.GetInt("_DissolveStyle");
		int matcapToggle = mat.GetInt("_MatcapToggle");
		int eRimToggle = mat.GetInt("_EnvironmentRim");
		int spriteToggle0 = mat.GetInt("_EnableSpritesheet");
		int spriteToggle1 = mat.GetInt("_EnableSpritesheet1");
		int postFilterToggle = mat.GetInt("_PostFiltering");
		int stencilToggle = mat.GetInt("_StencilToggle");
		int screenspace = mat.GetInt("_Screenspace");
		int cloneToggle = mat.GetInt("_CloneToggle");
		int dissolveWireframe = mat.GetInt("_GeomDissolveWireframe");
		int refracToggle = mat.GetInt("_Refraction");
		int caToggle = mat.GetInt("_RefractionCA");
		int vManipToggle = mat.GetInt("_VertexManipulationToggle");
		int maskTransToggle = mat.GetInt("_EnableMaskTransform");
		int bcDissToggle = mat.GetInt("_BCDissolveToggle");
		int audioLinkToggle = mat.GetInt("_AudioLinkToggle");
		int rimToggle = mat.GetInt("_RimLighting");
		int subsurfToggle = mat.GetInt("_Subsurface");
		int blurToggle = mat.GetInt("_RefractionBlur");
		bool reflFallback = mat.GetTexture("_ReflCube");
		bool matcapNormal0 = mat.GetTexture("_MatcapNormal0");
		bool matcapNormal1 = mat.GetTexture("_MatcapNormal1");
		bool isUberX = MGUI.IsXVersion(mat);
		bool isOutline = MGUI.IsOutline(mat);
		bool usingNormal = mat.GetTexture("_BumpMap");
		bool usingParallax = workflow < 3 ? mat.GetTexture("_ParallaxMap") : (mat.GetTexture("_PackedMap") && mat.GetInt("_EnablePackedHeight") == 1);
		bool usingDetail = mat.GetTexture("_DetailNormalMap");
		bool usingDetailRough = mat.GetTexture("_DetailRoughnessMap");
		bool usingDetailOcc = mat.GetTexture("_DetailOcclusionMap");
		bool usingDetailAlbedo = mat.GetTexture("_DetailAlbedoMap");

		// Setting floats based on render mode/texture presence/etc
		mat.SetInt("_IsCubeBlendMask", mat.GetTexture("_CubeBlendMask") ? 1 : 0);
		mat.SetInt("_UseSmoothMap", mat.GetTexture("_SmoothnessMap") && workflow == 1 ? 1 : 0);
		mat.SetInt("_UseMatcap1", mat.GetTexture("_Matcap1") ? 1 : 0);
		mat.SetInt("_ATM", blendMode == 1 ? 1 : 0);	

		// Sync the outline stencil settings with base pass stencil settings when not using stencil mode
		if (isOutline && stencilToggle == 0){
			mat.SetFloat("_OutlineStencilRef", mat.GetFloat("_StencilRef"));
			mat.SetFloat("_OutlineStencilPass", mat.GetFloat("_StencilPass"));
			mat.SetFloat("_OutlineStencilFail", mat.GetFloat("_StencilFail"));
			mat.SetFloat("_OutlineStencilZFail", mat.GetFloat("_StencilZFail"));
			mat.SetFloat("_OutlineStencilCompare", mat.GetFloat("_StencilCompare"));
		}

		// Force backface culling off if using screenspace mesh
		if (isUberX && screenspace == 1 && cullingMode != 0)
			mat.SetInt("_CullingMode", 0);

		// Outline should have culling disabled for stencils to look good at mask cutoff points
		if (stencilToggle == 1)
			mat.SetInt("_OutlineCulling", 0);
		else
			mat.SetInt("_OutlineCulling", 1);

		if (dissolveWireframe == 1 && isUberX && dissolveStyle == 3)
			mat.SetInt("_WireframeToggle", 1);

		// Use metallic or specular map based on workflow choice
		if (workflow >= 3){
			mat.SetInt("_UseMetallicMap", 1);
			mat.SetInt("_UseSpecMap", 1);
		}
		else {
			mat.SetInt("_UseMetallicMap", mat.GetTexture("_MetallicGlossMap") ? 1 : 0);
			mat.SetInt("_UseSpecMap", mat.GetTexture("_SpecGlossMap") ? 1 : 0);
		}

		// Handling some jank with PBR filter preview options
		if (workflow != 1 && workflow != 2)
			mat.SetInt("_PreviewSmooth", 0);
		else
			mat.SetInt("_PreviewRough", 0);

		if (workflow >= 3 && _RoughnessFiltering.floatValue == 1 && _PreviewRough.floatValue == 1)
			mat.SetInt("_PackedRoughPreview", 1);
		else
			mat.SetInt("_PackedRoughPreview", 0);

		if (workflow >= 3){
			if (!mat.GetTexture("_PackedMap")){
				mat.SetInt("_AOFiltering", 0);
			}
		}
		else {
			if (!mat.GetTexture("_OcclusionMap"))
				mat.SetInt("_AOFiltering", 0);
		}

		// Toggle detail maps so strength params can be 1 by default without messing up lerps
		if (usingDetailAlbedo)
			mat.SetInt("_UsingDetailAlbedo", 1);
		else
			mat.SetInt("_UsingDetailAlbedo", 0);

		if (usingDetailRough)
			mat.SetInt("_UsingDetailRough", 1);
		else
			mat.SetInt("_UsingDetailRough", 0);

		if (usingDetailOcc)
			mat.SetInt("_UsingDetailOcclusion", 1);
		else
			mat.SetInt("_UsingDetailOcclusion", 0);

		// Matcap normal maps
		if (matcapNormal0)
			mat.SetInt("_MatcapNormal0Toggle", 1);
		else
			mat.SetInt("_MatcapNormal0Toggle", 0);

		if (matcapNormal1)
			mat.SetInt("_MatcapNormal1Toggle", 1);
		else
			mat.SetInt("_MatcapNormal1Toggle", 0);

		bool prevAO = mat.GetInt("_AOFiltering") == 1 && mat.GetInt("_PreviewAO") == 1;
		bool prevRough = mat.GetInt("_RoughnessFiltering") == 1 && mat.GetInt("_PreviewRough") == 1;
		prevRough = prevRough && (workflow == 0 || workflow >= 3);
		bool prevSmooth = mat.GetInt("_SmoothnessFiltering") == 1 && mat.GetInt("_PreviewSmooth") == 1;
		prevSmooth = prevSmooth && (workflow == 1 || workflow == 2);
		bool prevHeight = mat.GetInt("_HeightFiltering") == 1 && mat.GetInt("_PreviewHeight") == 1;
		bool prevMetal = mat.GetInt("_MetallicFiltering") == 1 && mat.GetInt("_PreviewMetallic") == 1;
		prevMetal = prevMetal && (workflow == 0 || workflow >= 3); 

		// Begone grabpass
		bool ssrEnabled = ssr == 1 && reflToggle == 1 && renderMode > 0;
		bool refracEnabled = refracToggle == 1 && renderMode > 0;
		mat.SetShaderPassEnabled("Always", ssrEnabled || refracEnabled);

		SetKeyword(mat, "_PACKED_WORKFLOW_ON", workflow >= 3 && renderMode > 0);
		SetKeyword(mat, "_SPECULAR_WORKFLOW_ON", (workflow == 1 || workflow == 2) && renderMode > 0);
		SetKeyword(mat, "_REFLECTIONS_ON", reflToggle > 0 && renderMode > 0);
		SetKeyword(mat, "_SPECULAR_ON", specToggle > 0 && renderMode > 0);
		SetKeyword(mat, "_CUBEMAP_ON", cubeMode == 1);
		SetKeyword(mat, "_CUBEMAP_COMBINED_ON",  cubeMode == 2);
		SetKeyword(mat, "_PACKED_MASKING_ON", maskingMode == 2);
		SetKeyword(mat, "_SEPARATE_MASKING_ON", maskingMode == 1);
		SetKeyword(mat, "_SHADING_ON", renderMode == 1);
		SetKeyword(mat, "_SPECULAR_ANISO_ON", specToggle == 2 && renderMode > 0);
		SetKeyword(mat, "_SPECULAR_COMBINED_ON", specToggle == 3 && renderMode > 0);
		SetKeyword(mat, "_PBR_PREVIEW_ON", (prevAO || prevRough || prevSmooth || prevHeight || prevMetal) && renderMode > 0);
		SetKeyword(mat, "_UV_DISTORTION_ON", distortMode > 0);
		SetKeyword(mat, "_UV_DISTORTION_NORMALMAP_ON", distortMode == 1);
		SetKeyword(mat, "_FILTERING_ON", filterToggle == 1);
		SetKeyword(mat, "_POST_FILTERING_ON", filterToggle == 1 && postFilterToggle == 1);
		SetKeyword(mat, "_DETAIL_NORMALMAP_ON", usingDetail && renderMode > 0);
		SetKeyword(mat, "_PARALLAXMAP_ON",  usingParallax && renderMode > 0);
		SetKeyword(mat, "_NORMALMAP_ON", usingNormal && renderMode > 0);
		SetKeyword(mat, "_EMISSION_ON", emissToggle == 1);
		SetKeyword(mat, "_ALPHATEST_ON", blendMode > 0 && blendMode < 4);
		SetKeyword(mat, "_ALPHABLEND_ON", blendMode == 4);
		SetKeyword(mat, "_ALPHAPREMULTIPLY_ON", blendMode == 5);
		SetKeyword(mat, "_SCREENSPACE_REFLECTIONS_ON", ssrEnabled);
		SetKeyword(mat, "_PULSE_ON", emissToggle == 1 && pulseToggle == 1);
		SetKeyword(mat, "_CLONES_ON", isUberX && cloneToggle == 1);
		SetKeyword(mat, "_DISSOLVE_TEXTURE_ON", dissolveStyle == 1 && isUberX);
		SetKeyword(mat, "_DISSOLVE_SIMPLEX_ON", dissolveStyle == 2 && isUberX);
		SetKeyword(mat, "_DISSOLVE_GEOMETRY_ON", dissolveStyle == 3 && isUberX);
		SetKeyword(mat, "_MATCAP_ON", matcapToggle == 1 && renderMode > 0);
		SetKeyword(mat, "_ENVIRONMENT_RIM_ON", eRimToggle == 1 && renderMode > 0);
		SetKeyword(mat, "_FLIPBOOK_ON", spriteToggle0 == 1 || spriteToggle1 == 1);
		SetKeyword(mat, "_CUBEMAP_REFLECTIONS_ON", reflToggle == 2 && renderMode > 0);
		SetKeyword(mat, "_REFRACTION_ON", refracEnabled);
		SetKeyword(mat, "_CHROMATIC_ABBERATION_ON", refracToggle == 1 && caToggle == 1 && renderMode > 0);
		SetKeyword(mat, "_VERTEX_MANIP_ON", vManipToggle == 1);
		SetKeyword(mat, "_MASK_TRANSFORMS_ON", maskingMode == 1 && maskTransToggle == 1);
		SetKeyword(mat, "_REFLECTION_FALLBACK_ON", reflFallback && reflToggle > 0 && renderMode > 0);
		SetKeyword(mat, "_BASECOLOR_DISSOLVE_ON", bcDissToggle == 1 && renderMode > 0);
		SetKeyword(mat, "_AUDIOLINK_ON", audioLinkToggle == 1);
		SetKeyword(mat, "_RIM_ON", rimToggle == 1 && renderMode > 0);
		SetKeyword(mat, "_SUBSURFACE_ON", subsurfToggle == 1 && renderMode > 0);
	}

	void SetBlendMode(Material mat){
		int blendMode = mat.GetInt("_BlendMode");
		switch (blendMode){
			case 0: // Opaque
				mat.SetOverrideTag("RenderType", "Opaque");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				mat.SetInt("_ZWrite", 1);
				mat.renderQueue = -1;
				break;
			case 1: // Cutout
			case 2:	// Dithered
			case 3:	// Alpha to Coverage
				mat.SetOverrideTag("RenderType", "TransparentCutout");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				mat.SetInt("_ZWrite", 1);
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
				break;
			case 4: // Fade
				mat.SetOverrideTag("RenderType", "Transparent");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
				break;
			case 5: // Transparent
				mat.SetOverrideTag("RenderType", "Transparent");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				mat.SetInt("_ZWrite", 0);
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
				break;
			default: break;
		}
	}

	void SetFallback(Material mat){
		int fallback = mat.GetInt("_VRCFallback");
		int blendMode = mat.GetInt("_BlendMode");
		string tag = "";
		switch (fallback){
			case 0: tag = "Unlit"; break;
			case 1: tag = "Toon"; break;
			case 2: tag = "Particle"; break;
			case 3: tag = "Matcap"; break;
			case 4: tag = "Sprite"; break;
			case 5: tag = "DoubleSided"; break;
			case 6: tag = "Hidden"; break;
			default: break;
		}
		if (fallback != 6){
			switch (blendMode){
				case 1: // Cutout
				case 2:	// Dithered
				case 3:	// Alpha to Coverage
					tag += "Cutout";
					break;
				case 4: tag += "Fade"; break;
				case 5: tag += "Transparent"; break;
				default: break;
			}
		}
		mat.SetOverrideTag("VRCFallback", tag);
	}

	void ApplyOutlineStencilConfig(Material mat){
		mat.SetFloat("_StencilPass", (float)UnityEngine.Rendering.StencilOp.Replace);
		mat.SetFloat("_StencilFail", (float)UnityEngine.Rendering.StencilOp.Keep);
		mat.SetFloat("_StencilZFail", (float)UnityEngine.Rendering.StencilOp.Keep);
		mat.SetFloat("_StencilCompare", (float)UnityEngine.Rendering.CompareFunction.Always);

		mat.SetFloat("_OutlineStencilPass", (float)UnityEngine.Rendering.StencilOp.Keep);
		mat.SetFloat("_OutlineStencilFail", (float)UnityEngine.Rendering.StencilOp.Keep);
		mat.SetFloat("_OutlineStencilZFail", (float)UnityEngine.Rendering.StencilOp.Keep);
		mat.SetFloat("_OutlineStencilCompare", (float)UnityEngine.Rendering.CompareFunction.NotEqual);
	}

	public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
		if (mat.HasProperty("_Emission"))
			mat.SetColor("_EmissionColor", mat.GetColor("_Emission"));
		base.AssignNewShaderToMaterial(mat, oldShader, newShader);
		MGUI.ClearKeywords(mat);
		ApplyMaterialSettings(mat);
		SetBlendMode(mat);
	}

	void ApplyClonePositions(){

		// Diamond
		if (_ClonePattern.floatValue == 0){
			_Clone1.vectorValue = new Vector4(1,0,0,_Clone1.vectorValue.w);
			_Clone2.vectorValue = new Vector4(-1,0,0,_Clone2.vectorValue.w);
			_Clone3.vectorValue = new Vector4(0,0,1,_Clone3.vectorValue.w);
			_Clone4.vectorValue = new Vector4(0,0,-1,_Clone4.vectorValue.w);
			_Clone5.vectorValue = new Vector4(0.5f,0,0.5f,_Clone5.vectorValue.w);
			_Clone6.vectorValue = new Vector4(-0.5f,0,0.5f,_Clone6.vectorValue.w);
			_Clone7.vectorValue = new Vector4(0.5f,0,-0.5f,_Clone7.vectorValue.w);
			_Clone8.vectorValue = new Vector4(-0.5f,0,-0.5f,_Clone8.vectorValue.w);
		}

		// Pyramid
		else if (_ClonePattern.floatValue == 1){
			_Clone1.vectorValue = new Vector4(0,2.5f,0,_Clone1.vectorValue.w);
			_Clone2.vectorValue = new Vector4(0,1.25f,0,_Clone2.vectorValue.w);
			_Clone3.vectorValue = new Vector4(0,0.75f,0.25f,_Clone3.vectorValue.w);
			_Clone4.vectorValue = new Vector4(0,0.75f,-0.25f,_Clone4.vectorValue.w);
			_Clone5.vectorValue = new Vector4(0.25f,0,0.4f,_Clone5.vectorValue.w);
			_Clone6.vectorValue = new Vector4(-0.25f,0,0.4f,_Clone6.vectorValue.w);
			_Clone7.vectorValue = new Vector4(0.25f,0,-0.4f,_Clone7.vectorValue.w);
			_Clone8.vectorValue = new Vector4(-0.25f,0,-0.4f,_Clone8.vectorValue.w);
		}

		// Stack
		else if (_ClonePattern.floatValue == 2){
			_Clone1.vectorValue = new Vector4(0,1,0,_Clone1.vectorValue.w);
			_Clone2.vectorValue = new Vector4(0,2,0,_Clone2.vectorValue.w);
			_Clone3.vectorValue = new Vector4(0,3,0,_Clone3.vectorValue.w);
			_Clone4.vectorValue = new Vector4(0,4,0,_Clone4.vectorValue.w);
			_Clone5.vectorValue = new Vector4(0,5,0,_Clone5.vectorValue.w);
			_Clone6.vectorValue = new Vector4(0,6,0,_Clone6.vectorValue.w);
			_Clone7.vectorValue = new Vector4(0,7,0,_Clone7.vectorValue.w);
			_Clone8.vectorValue = new Vector4(0,8,0,_Clone8.vectorValue.w);
		}
	}

	void ClearBaseMaps(float cubeMode){
		if (cubeMode == 0){
			_MainTexCube0.textureValue = null;
			_CubeBlendMask.textureValue = null;
		}
		else if (cubeMode == 1){
			_MainTex.textureValue = null;
			_CubeBlendMask.textureValue = null;
		}
		if (_UseAlphaMask.floatValue == 0){
			_AlphaMask.textureValue = null;
		}
	}

	void ClearTextureMaps(float renderMode, float workflow){
		if (renderMode == 1){
			if (workflow == 0){
				_PackedMap.textureValue = null;
				_SmoothnessMap.textureValue = null;
			}
			else if (workflow == 1 || workflow == 2){
				_MetallicGlossMap.textureValue = null;
			}
			else if (workflow >= 3){
				ClearPrimaryMaps(false);
			}
			if (_AOFiltering.floatValue == 0)
				_AOTintTex.textureValue = null;

			if (_MaskingMode.floatValue == 0){
				ClearMasksPacked();
				ClearMasksSeparate();
			}
			else if (_MaskingMode.floatValue == 1){
				ClearMasksPacked();
			}
			else if (_MaskingMode.floatValue == 2){
				ClearMasksSeparate();
			}
		}
		else {
			ClearPrimaryMaps(true);
			ClearDetailMaps();
			_AOTintTex.textureValue = null;
		}
	}

	void ClearPrimaryMaps(bool clearNormal){
		_MetallicGlossMap.textureValue = null;
		_SpecGlossMap.textureValue = null;
		_SmoothnessMap.textureValue = null;
		_OcclusionMap.textureValue = null;
		_ParallaxMap.textureValue = null;
		if (clearNormal)
			_BumpMap.textureValue = null;
	}

	void ClearVertMaps(){
		if (_VertexManipulationToggle.floatValue == 0){
			_VertexExpansionMask.textureValue = null;
			_VertexRoundingMask.textureValue = null;
		}
	}

	void ClearDetailMaps(){
		_DetailAlbedoMap.textureValue = null;
		_DetailNormalMap.textureValue = null;
		_DetailRoughnessMap.textureValue = null;
		_DetailOcclusionMap.textureValue = null;
	}

	void ClearMasksPacked(){
		_PackedMask0.textureValue = null;
		_PackedMask1.textureValue = null;
		_PackedMask2.textureValue = null;
		_PackedMask3.textureValue = null;
	}

	void ClearMasksSeparate(){
		_ReflectionMask.textureValue = null;
		_SpecularMask.textureValue = null;
		_MatcapMask.textureValue = null;
		_ShadowMask.textureValue = null;
		_RimMask.textureValue = null;
		_ERimMask.textureValue = null;
		_DiffuseMask.textureValue = null;
		_SubsurfaceMask.textureValue = null;
		_DetailMask.textureValue = null;
		_EmissMask.textureValue = null;
		_PulseMask.textureValue = null;
		_FilterMask.textureValue = null;
		_MatcapBlendMask.textureValue = null;
		_InterpMask.textureValue = null;
		_OutlineMask.textureValue = null;
		_RefractionMask.textureValue = null;
	}
	void ClearBCDissolveMaps(){
		if (_BCDissolveToggle.floatValue != 1){
			_BCNoiseTex.textureValue = null;
			_MainTex2.textureValue = null;
		}
	}

	void ClearShadingMaps(){
		if (_Reflections.floatValue < 2){
			_ReflCube.textureValue = null;
		}
		if (_MatcapToggle.floatValue == 0){
			_Matcap.textureValue = null;
			_Matcap1.textureValue = null;
		}
		if (_Subsurface.floatValue == 0){
			_ThicknessMap.textureValue = null;
			_ScatterTex.textureValue = null;
		}
		if (_RimLighting.floatValue == 0){
			_RimTex.textureValue = null;
		}
	}

	void ClearEmissionMaps(){
		if (_EmissionToggle.floatValue == 0){
			_EmissionMap.textureValue = null;
		}
	}

	void ClearFilterMaps(){
		if (_Filtering.floatValue == 0 || _TeamFiltering.floatValue == 0){
			_TeamColorMask.textureValue = null;
		}
	}

	void ClearSpriteSheets(){
		if (_EnableSpritesheet.floatValue == 0){
			_Spritesheet.textureValue = null;
		}
		if (_EnableSpritesheet1.floatValue == 0){
			_Spritesheet1.textureValue = null;
		}
	}

	void ClearOutlineMaps(){
		if (_OutlineToggle.floatValue == 0){
			_OutlineTex.textureValue = null;
		}
	}

	void ClearUVDMaps(){
		if (_DistortionStyle.floatValue == 0 || _DistortionStyle.floatValue == 2){
			_DistortUVMap.textureValue = null;
		}
	}

	void ClearSpecialMaps(){
		if (_DissolveStyle.floatValue == 0){
			_DissolveMask.textureValue = null;
			_DissolveTex.textureValue = null;
		}
		if (_DissolveStyle.floatValue == 3){
			_DissolveTex.textureValue = null;
		}
	}

	void CopyToSheet1(){
		_Spritesheet.textureValue = _Spritesheet1.textureValue;
		_Spritesheet.colorValue = _Spritesheet1.colorValue;
		_SpritesheetBrightness.floatValue = _SpritesheetBrightness1.floatValue;
		_SpritesheetBlending.floatValue = _SpritesheetBlending1.floatValue;
		_RowsColumns.vectorValue = _RowsColumns1.vectorValue;
		_FrameClipOfs.vectorValue = _FrameClipOfs1.vectorValue;
		_SpritesheetPos.vectorValue = _SpritesheetPos1.vectorValue;
		_SpritesheetScale.vectorValue = _SpritesheetScale1.vectorValue;
		_SpritesheetRot.floatValue = _SpritesheetRot1.floatValue;
		_ManualScrub.floatValue = _ManualScrub1.floatValue;
		_ScrubPos.floatValue = _ScrubPos1.floatValue;
		_FPS.floatValue = _FPS1.floatValue;
	}

	void CopyToSheet2(){
		_Spritesheet1.textureValue = _Spritesheet.textureValue;
		_Spritesheet1.colorValue = _Spritesheet.colorValue;
		_SpritesheetBrightness1.floatValue = _SpritesheetBrightness.floatValue;
		_SpritesheetBlending1.floatValue = _SpritesheetBlending.floatValue;
		_RowsColumns1.vectorValue = _RowsColumns.vectorValue;
		_FrameClipOfs1.vectorValue = _FrameClipOfs.vectorValue;
		_SpritesheetPos1.vectorValue = _SpritesheetPos.vectorValue;
		_SpritesheetScale1.vectorValue = _SpritesheetScale.vectorValue;
		_SpritesheetRot1.floatValue = _SpritesheetRot.floatValue;
		_ManualScrub1.floatValue = _ManualScrub.floatValue;
		_ScrubPos1.floatValue = _ScrubPos.floatValue;
		_FPS1.floatValue = _FPS.floatValue;
	}

	void DoStandardLighting(Material mat){
		_DisneyDiffuse.floatValue = 0f;
		_SHStr.floatValue = 1f;
		_NonlinearSHToggle.floatValue = 0f;
		_DirectCont.floatValue = 0.6f;
		_IndirectCont.floatValue = 0.4f;
		_RTDirectCont.floatValue = 1f;
		_RTIndirectCont.floatValue = 1f;
		_ClampAdditive.floatValue = 0f;
		_Reflections.floatValue = 1f;
		_ReflectionStr.floatValue = 1f;
		_ReflCol.colorValue = Color.white;
		_Specular.floatValue = 1f;
		_SpecStr.floatValue = 1f;
		_SpecCol.colorValue = Color.white;
		_SharpSpecular.floatValue = 0f;
		_ColorPreservation.floatValue = 0f;
		_ShadowStr.floatValue = 1f;
		_ShadowMode.floatValue = 1f;
		_RampWidth0.floatValue = 1f;
		_RampWidth1.floatValue = 1f;
		_RampWeight.floatValue = 0f;
		_RTSelfShadow.floatValue = 1f;
		_AttenSmoothing.floatValue = 0f;
		_ShadowDithering.floatValue = 0f;
		_ShadowConditions.floatValue = 1f;
		_DirectAO.floatValue = 1f;
		_IndirectAO.floatValue = 0f;
		_LightingBasedIOR.floatValue = 0f;
		_RealtimeSpec.floatValue = 1;
	}

	void DoShadingReset(Material mat){
		DoLightingReset();
		DoReflReset();
		DoSpecReset();
		DoMatcapReset();
		DoShadowReset();
		DoSubsurfReset();
		DoRimReset();
		DoERimReset();
		DoNormalReset();
		DoRefracReset();
		DoIridescenceReset();
		_EnvironmentRim.floatValue = 0f;
		_Reflections.floatValue = 0f;
		_Specular.floatValue = 0f;
		_MatcapToggle.floatValue = 0f;
		_Subsurface.floatValue = 0f;
		_RimLighting.floatValue = 0f;
		_Refraction.floatValue = 0f;
		_Iridescence.floatValue = 0f;
	}

	void DoTextureMapReset(){
		DoPrimaryMapsReset();
		DoDetailMapsReset();
		DoMaskingReset();
		DoRoughFilterReset();
		DoSmoothFilterReset();
		DoAOFilterReset();
		DoHeightFilterReset();
		DoMetallicFilterReset();
	}

	void DoPrimaryMapsReset(){
		_Metallic.floatValue = 0f;
		_Glossiness.floatValue = 0.5f;
		_OcclusionStrength.floatValue = 1f;
		_Parallax.floatValue = 0.02f;
		_BumpScale.floatValue = 1f;
		_GlossMapScale.floatValue = 1f;
		ClearPrimaryMaps(true);
	}

	void DoDetailMapsReset(){
		ClearDetailMaps();
		_DetailAlbedoStrength.floatValue = 1f;
		_DetailAlbedoBlending.floatValue = 4f;
		_DetailNormalMapScale.floatValue = 1f;
		_DetailRoughBlending.floatValue = 0f;
		_DetailRoughStrength.floatValue = 1f;
		_DetailOcclusionBlending.floatValue = 3f;
		_DetailOcclusionStrength.floatValue = 1f;
	}

	void DoRoughFilterReset(){
		_PreviewRough.floatValue = 0f;
		_RoughLightness.floatValue = 0f;
		_RoughIntensity.floatValue = 0f;
		_RoughContrast.floatValue = 1f;
		_RoughRemapMin.floatValue = 0f;
		_RoughRemapMax.floatValue = 1f;
	}

	void DoSmoothFilterReset(){
		_PreviewSmooth.floatValue = 0f;
		_SmoothLightness.floatValue = 0f;
		_SmoothIntensity.floatValue = 0f;
		_SmoothContrast.floatValue = 1f;
		_SmoothRemapMin.floatValue = 0f;
		_SmoothRemapMax.floatValue = 1f;
	}

	void DoAOFilterReset(){
		_AOTintTex.textureValue = null;
		_PreviewAO.floatValue = 0f;
		_AOTint.colorValue = new Color(0,0,0,1);
		_AOLightness.floatValue = 0f;
		_AOIntensity.floatValue = 0f;
		_AOContrast.floatValue = 1f;
		_AORemapMin.floatValue = 0f;
		_AORemapMax.floatValue = 1f;
	}

	void DoHeightFilterReset(){
		_PreviewHeight.floatValue = 0f;
		_HeightLightness.floatValue = 0f;
		_HeightIntensity.floatValue = 0f;
		_HeightContrast.floatValue = 1f;
		_HeightRemapMin.floatValue = 0f;
		_HeightRemapMax.floatValue = 1f;
	}

	void DoMetallicFilterReset(){
		_PreviewMetallic.floatValue = 0f;
		_MetallicLightness.floatValue = 0f;
		_MetallicIntensity.floatValue = 0f;
		_MetallicContrast.floatValue = 1f;
		_MetallicRemapMin.floatValue = 0f;
		_MetallicRemapMax.floatValue = 1f;
	}

	void DoBCDissolveReset(){
		_BCNoiseTex.textureValue = null;
		_MainTex2.textureValue = null;
		_BCDissolveStr.floatValue = 0f;
		_BCRimWidth.floatValue = 0.5f;
		_BCRimCol.colorValue = Color.white;
	}

	void DoLightingReset(){
		_StaticLightDirToggle.floatValue = 0f;
		_StaticLightDir.vectorValue = new Vector4(0, 0.75f, 1, 0);
		_DisneyDiffuse.floatValue = 0f;
		_SHStr.floatValue = 0.0f;
		_NonlinearSHToggle.floatValue = 1f;
		_VLightCont.floatValue = 1f;
		_RTDirectCont.floatValue = 1f;
		_RTIndirectCont.floatValue = 1f;
		_DirectCont.floatValue = 0.6f;
		_IndirectCont.floatValue = 0.5f;
		_ClampAdditive.floatValue = 1f;
		_DirectAO.floatValue = 1f;
		_IndirectAO.floatValue = 0f;
	}

	void DoMaskingReset(){
		_MaskingMode.floatValue = 0f;
		ClearMasksPacked();
		ClearMasksSeparate();
	}

	void DoReflReset(){
		_ReflCube.textureValue = null;
		_ReflectionStr.floatValue = 1f;
		_ReflUseRough.floatValue = 0f;
		_ReflRough.floatValue = 0.5f;
		_ReflCol.colorValue = Color.white;
		_SSR.floatValue = 0f;
		_Alpha.floatValue = 1f;
		_MaxSteps.floatValue = 50f;
		_Step.floatValue = 0.09f;
		_LRad.floatValue = 0.2f;
		_SRad.floatValue = 0.02f;
		_EdgeFade.floatValue = 0.1f;
		_LightingBasedIOR.floatValue = 0f;
		_ReflStepping.floatValue = 0f;
		_ReflSteps.floatValue = 1f;
		_FresnelToggle.floatValue = 1f;
		_FresnelStrength.floatValue = 1f;
	}
	
	void DoSpecReset(){
		_SpecStr.floatValue = 1f;
		_SpecCol.colorValue = Color.white;
		_SharpSpecular.floatValue = 0f;
		_AnisoAngleY.floatValue = 1f;
		_AnisoLayerY.floatValue = 1f;
		_AnisoLayerStr.floatValue = 0.5f;
		_AnisoLerp.floatValue = 0f;
		_SharpSpecStr.floatValue = 1f;
		_SpecUseRough.floatValue = 0f;
		_SpecRough.floatValue = 0.5f;
		_RippleStrength.floatValue = 0f;
		_RippleFrequency.floatValue = 1f;
		_RippleAmplitude.floatValue = 0.5f;
		_RippleContinuity.floatValue = 1f;
		_ManualSpecBright.floatValue = 0f;
		_SpecBiasOverrideToggle.floatValue = 0f;
		_SpecBiasOverride.floatValue = 0.5f;
	}

	void DoMatcapReset(){
		_Matcap.textureValue = null;
		_MatcapStr.floatValue = 1f;
		_MatcapColor.colorValue = Color.white;
		_UnlitMatcap.floatValue = 0f;
		_MatcapBlending.floatValue = 0f;
		_MatcapUseRough.floatValue = 0f;
		_MatcapRough.floatValue = 0.5f;
		_Matcap1.textureValue = null;
		_MatcapStr1.floatValue = 1f;
		_MatcapColor1.colorValue = Color.white;
		_MatcapUseRough1.floatValue = 0f;
		_MatcapRough1.floatValue = 0.5f;
		_MatcapBlending1.floatValue = 0f;
		_UnlitMatcap1.floatValue = 0f;
		_MatcapNormal0.textureValue = null;
		_MatcapNormal1.textureValue = null;
		_MatcapNormal0Str.floatValue = 1f;
		_MatcapNormal1Str.floatValue = 1f;
		_MatcapNormal0Scroll.vectorValue = Vector4.zero;
		_MatcapNormal1Scroll.vectorValue = Vector4.zero;
		_MatcapNormal0Mix.floatValue = 0f;
		_MatcapNormal1Mix.floatValue = 0f;
	}

	void DoShadowReset(){
		string rp = unityFolderPath+"/Textures/Ramps/DefaultRamp.png";
		_ShadowRamp.textureValue = File.Exists(rp) ? (Texture)EditorGUIUtility.Load(rp) : null;
		_ShadowTint.colorValue = new Color(0,0,0,1);
		_ShadowStr.floatValue = 1f;
		_RampWidth0.floatValue = 0.005f;
		_RampWidth1.floatValue = 0.5f;
		_RampWeight.floatValue = 0f;
		_ShadowDithering.floatValue = 0f;
		_RTSelfShadow.floatValue = 1f;
		_AttenSmoothing.floatValue = 1f;
		_RampPos.floatValue = 0f;
		_DitheredShadows.floatValue = 0f;
	}

	void DoSubsurfReset(){
		_ThicknessMap.textureValue = null;
		_ScatterBaseColorTint.floatValue = 0f;
		_ScatterTex.textureValue = null;
		_ThicknessMapPower.floatValue = 1f;
		_ScatterCol.colorValue = Color.white;
		_ScatterIntensity.floatValue = 1f;
		_ScatterPow.floatValue = 1f;
		_ScatterDist.floatValue = 1f;
		_ScatterAmbient.floatValue = 0f;
		_ScatterWrap.floatValue = 0.01f;
	}

	void DoRimReset(){
		_RimTex.textureValue = null;
		_RimCol.colorValue = Color.white;
		_RimBlending.floatValue = 0f;
		_RimStr.floatValue = 1f;
		_RimWidth.floatValue = 0.5f;
		_RimEdge.floatValue = 0f;
	}

	void DoERimReset(){
		_ERimTint.colorValue = Color.white;
		_ERimBlending.floatValue = 1f;
		_ERimStr.floatValue = 1f;
		_ERimWidth.floatValue = 0.7f;
		_ERimEdge.floatValue = 0f;
		_ERimUseRough.floatValue = 0f;
		_ERimRoughness.floatValue = 0.5f;
	}

	void DoRefracReset(){
		_RefractionOpac.floatValue = 0f;
		_UnlitRefraction.floatValue = 0f;
		_RefractionIOR.floatValue = 1.3f;
		_RefractionCA.floatValue = 0f;
		_RefractionCAStr.floatValue = 0.1f;
		_RefractionTint.colorValue = Color.white;
		_RefractionDissolveMask.textureValue = null;
		_RefractionDissolveMaskScroll.vectorValue = Vector4.zero;
		_RefractionDissolveMaskStr.floatValue = 1f;
	}

	void DoIridescenceReset(){
		_IridescenceStrength.floatValue = 1f;
		_IridescenceHue.floatValue = 0f;
		_IridescenceWidth.floatValue = 0.7f;
		_IridescenceEdge.floatValue = 0f;
		_IridescenceMask.textureValue = null;
		// _IridescenceCurl.floatValue = 1f;
		// _IridescenceCurlScale.floatValue = 1f;
	}

	void DoNormalReset(){
		_HardenNormals.floatValue = 0f;
		_ClearCoat.floatValue = 0f;
		_GSAA.floatValue = 0f;
	}

	void DoEmissionReset(){
		_EmissionMap.textureValue = null;
		_EmissionColor.colorValue = new Color(0,0,0,1);
		_ReactToggle.floatValue = 0f;
		_PulseToggle.floatValue = 0f;
		DoLRReset();
		DoPulseReset();
	}

	void DoAudioLinkReset(){
		_AudioLinkRemapMin.floatValue = 0f;
		_AudioLinkRemapMax.floatValue = 1f;
		_AudioLinkStrength.floatValue = 1f;
		DoAudioLinkWireframeReset();
		DoAudioLinkTriOffsetReset();
		DoAudioLinkBCDissolveReset();
		DoAudioLinkDissolveReset();
		DoAudioLinkRimReset();
		DoAudioLinkVertManipReset();
		DoAudioLinkEmissionReset();
		DoAudioLinkUVDistortionReset();
		DoAudioLinkOutlineReset();
		DoAudioLinkVizReset();

	}

	void DoAudioLinkUVDistortionReset(){
		_AudioLinkUVDistortionBand.floatValue = 0f;
		_AudioLinkUVDistortionMultiplier.floatValue = 0f;
		_AudioLinkRemapUVDistortionMin.floatValue = 0f;
		_AudioLinkRemapUVDistortionMax.floatValue = 1f;
	}

	void DoAudioLinkWireframeReset(){
		_AudioLinkWireframeBand.floatValue = 0f;
		_AudioLinkWireframeCoords.floatValue = 1f;
		_AudioLinkWireframeMode.floatValue = 0f;
		_AudioLinkWireframeColor.colorValue = Color.white;
		_AudioLinkWireframeStartPos.floatValue = -0.5f;
		_AudioLinkWireframeEndPos.floatValue = 0.5f;
		_AudioLinkWireframeSize.floatValue = 0.1f;
		_AudioLinkWireframeMask.textureValue = null;
		_AudioLinkWireframeStrength.floatValue = 0f;
		_AudioLinkWireframeMaskScroll.vectorValue = Vector4.zero;
		_AudioLinkRemapWireframeMin.floatValue = 0f;
		_AudioLinkRemapWireframeMax.floatValue = 1f;
	}

	void DoAudioLinkTriOffsetReset(){
		_AudioLinkTriOffsetBand.floatValue = 0;
		_AudioLinkTriOffsetCoords.floatValue = 1f;
		_AudioLinkTriOffsetMode.floatValue = 0f;
		_AudioLinkTriOffsetStartPos.floatValue = -0.5f;
		_AudioLinkTriOffsetEndPos.floatValue = 0.5f;
		_AudioLinkTriOffsetSize.floatValue = 0.1f;
		_AudioLinkTriOffsetMask.textureValue = null;
		_AudioLinkTriOffsetStrength.floatValue = 0f;
		_AudioLinkTriOffsetMaskScroll.vectorValue = Vector4.zero;
		_AudioLinkRemapTriOffsetMin.floatValue = 0f;
		_AudioLinkRemapTriOffsetMax.floatValue = 1f;
	}

	void DoAudioLinkBCDissolveReset(){
		_AudioLinkBCDissolveBand.floatValue = 0f;
		_AudioLinkBCDissolveMultiplier.floatValue = 0f;
		_AudioLinkRemapBCDissolveMin.floatValue = 0f;
		_AudioLinkRemapBCDissolveMax.floatValue = 1f;
	}

	void DoAudioLinkDissolveReset(){
		_AudioLinkDissolveBand.floatValue = 0f;
		_AudioLinkDissolveMultiplier.floatValue = 0f;
		_AudioLinkRemapDissolveMin.floatValue = 0f;
		_AudioLinkRemapDissolveMax.floatValue = 1f;
	}

	void DoAudioLinkRimReset(){
		_AudioLinkRimBand.floatValue = 0f;
		_AudioLinkRimMultiplier.floatValue = 0f;
		_AudioLinkRimWidth.floatValue = 0f;
		_AudioLinkRimPulse.floatValue = 0f;
		_AudioLinkRimPulseWidth.floatValue = 0.5f;
		_AudioLinkRimPulseSharp.floatValue = 0.3f;
		_AudioLinkRemapRimMin.floatValue = 0f;
		_AudioLinkRemapRimMax.floatValue = 1f;
	}

	void DoAudioLinkVertManipReset(){
		_AudioLinkVertManipBand.floatValue = 0f;
		_AudioLinkVertManipMultiplier.floatValue = 0f;
		_AudioLinkRemapVertManipMin.floatValue = 0f;
		_AudioLinkRemapVertManipMax.floatValue = 1f;
	}

	void DoAudioLinkEmissionReset(){
		_AudioLinkEmissionBand.floatValue = 0f;
		_AudioLinkEmissionMultiplier.floatValue = 0f;
		_AudioLinkRemapEmissionMin.floatValue = 0f;
		_AudioLinkRemapEmissionMax.floatValue = 1f;
	}

	void DoAudioLinkOutlineReset(){
		_AudioLinkOutlineBand.floatValue = 0f;
		_AudioLinkOutlineMultiplier.floatValue = 0f;
		_AudioLinkRemapOutlineMin.floatValue = 0f;
		_AudioLinkRemapOutlineMax.floatValue = 1f;
	}

	void DoAudioLinkVizReset(){
		_OscilloscopeStrength.floatValue = 0f;
		_OscilloscopeCol.colorValue = Color.white;
		_OscilloscopeScale.vectorValue = Vector4.one;
		_OscilloscopeOffset.vectorValue = Vector4.zero;
		_OscilloscopeRot.floatValue = 0f;
		_OscilloscopeMarginLR.vectorValue = new Vector4(0,1,0,0);
		_OscilloscopeMarginTB.vectorValue = new Vector4(1,0,0,0);
	}

	void DoLRReset(){
		_CrossMode.floatValue = 0f;
		_Crossfade.floatValue = 0.1f;
		_ReactThresh.floatValue = 0.5f;
	}

	void DoPulseReset(){
		_PulseStr.floatValue = 0.5f;
		_PulseSpeed.floatValue = 1f;
		_PulseWaveform.floatValue = 0f;
	}

	void DoFiltersReset(){
		_Filtering.floatValue = 0f;
		_TeamFiltering.floatValue = 0f;
		_TeamColorMask.textureValue = null;
		_RGB.vectorValue = Vector4.one;
		_HDR.floatValue = 0f;
		_Contrast.floatValue = 1f;
		_Saturation.floatValue = 1f;
		_Brightness.floatValue = 1f;
		_AutoShift.floatValue = 0f;
		_AutoShiftSpeed.floatValue = 0.25f;
		_Hue.floatValue = 0f;
		_TeamColor0.colorValue = Color.white;
		_TeamColor1.colorValue = Color.white;
		_TeamColor2.colorValue = Color.white;
		_TeamColor3.colorValue = Color.white;
		_PostFiltering.floatValue = 0f;
		_Value.floatValue = 0f;
		_Invert.floatValue = 0f;
		_ACES.floatValue = 0f;
	}

	void DoSpriteReset(){
		_EnableSpritesheet.floatValue = 0f;
		_EnableSpritesheet1.floatValue = 0f;
		_UnlitSpritesheet.floatValue = 0f;
		_UnlitSpritesheet1.floatValue = 0f;
		_UseSpritesheetAlpha.floatValue = 0f;
		DoSheet1Reset();
		DoSheet2Reset();
	}

	void DoSheet1Reset(){
		_Spritesheet.textureValue = null;
		_SpritesheetMode0.floatValue = 0f;
		_Flipbook0.textureValue = null;
		_SpritesheetBlending.floatValue = 2f;
		_SpritesheetCol.colorValue = Color.white;
		_RowsColumns.vectorValue = new Vector4(8,8,0,0);
		_FrameClipOfs.vectorValue = Vector4.zero;
		_SpritesheetPos.vectorValue = Vector4.zero;
		_SpritesheetScale.vectorValue = new Vector4(1,1,0,0);
		_SpritesheetRot.floatValue = 0f;
		_FPS.floatValue = 30f;
		_ManualScrub.floatValue = 0f;
		_ScrubPos.floatValue = 1f;
		_Flipbook0ClipEdge.floatValue = 0f;
	}

	void DoSheet2Reset(){
		_Spritesheet1.textureValue = null;
		_SpritesheetMode1.floatValue = 0f;
		_Flipbook1.textureValue = null;
		_SpritesheetBlending1.floatValue = 2f;
		_SpritesheetCol1.colorValue = Color.white;
		_RowsColumns1.vectorValue = new Vector4(8,8,0,0);
		_FrameClipOfs1.vectorValue = Vector4.zero;
		_SpritesheetPos1.vectorValue = Vector4.zero;
		_SpritesheetScale1.vectorValue = new Vector4(1,1,0,0);
		_Flipbook1Scroll.vectorValue = Vector4.zero;
		_SpritesheetRot1.floatValue = 0f;
		_FPS1.floatValue = 30f;
		_ManualScrub1.floatValue = 0f;
		_ScrubPos1.floatValue = 1f;
		_Flipbook1ClipEdge.floatValue = 0f;
	}

	void DoUVDReset(){
		_DistortUVMap.textureValue = null;
		_DistortMainUV.floatValue = 0f;
		_DistortDetailUV.floatValue = 0f;
		_DistortEmissUV.floatValue = 0f;
		_DistortRimUV.floatValue = 0f;
		_DistortUVStr.floatValue = 1f;
		_DistortUVMask.textureValue = null;
		_DistortionStyle.floatValue = 0f;
		_NoiseScale.vectorValue = new Vector4(1,1,0,0);
		_NoiseSpeed.floatValue = 0.5f;
		_NoiseOctaves.floatValue = 1f;
		_DistortMatcap0.floatValue = 0f;
		_DistortMatcap1.floatValue = 0f;
	}

	void DoOutlineReset(){
		_OutlineTex.textureValue = null;
		_OutlineToggle.floatValue = 0f;
		_OutlineCol.colorValue = new Color(0.75f, 0.75f, 0.75f, 1);
		_OutlineThicc.floatValue = 0.1f;
		_OutlineRange.floatValue = 0f;
		_ApplyOutlineLighting.floatValue = 0f;
		_ApplyOutlineEmiss.floatValue = 0f;
		_StencilToggle.floatValue = 0f;
		_OutlineMult.floatValue = 1f;
		_UseVertexColor.floatValue = 0f;
		_IgnoreFilterMask.floatValue = 0f;
	}
	
	void DoVertexReset(){
		_VertexExpansion.vectorValue = Vector4.zero;
		_VertexExpansionClamp.floatValue = 0f;
		_VertexRounding.floatValue = 0f;
		_VertexRoundingPrecision.floatValue = 100f;
		_VertexExpansionMask.textureValue = null;
		_VertexRoundingMask.textureValue = null;
		_VertexManipulationToggle.floatValue = 0f;
		_VertexRotation.vectorValue = Vector4.zero;
	}

	void DoSpecialReset(){
		_DistanceFadeToggle.floatValue = 0f;
		_DissolveStyle.floatValue = 0f;
		_Screenspace.floatValue = 0f;
		_GlitchToggle.floatValue = 0f;
		_ShatterToggle.floatValue = 0f;
		_WireframeToggle.floatValue = 0f;
		_CloneToggle.floatValue = 0f;
		DoDFReset();
		DoDissolveReset();
		DoScreenReset();
		DoCloneReset();
		DoGlitchReset();
		DoShatterReset();
		DoWireframeReset();
	}

	void DoDFReset(){
		_DistanceFadeToggle.floatValue = 0f;
		_DistanceFadeMin.floatValue = 2f;
		_DistanceFadeMax.floatValue = 4f;
		_ClipRimStr.floatValue = 1f;
		_ClipRimWidth.floatValue = 1f;
		_ClipRimColor.colorValue = Color.white;
	}

	void DoDissolveReset(){
		_DissolveStyle.floatValue = 0f;
		_DissolveClones.floatValue = 0f;
		_DissolveNoiseScale.vectorValue = new Vector4(3,3,3,0);
		_DissolveAmount.floatValue = 0f;
		_DissolveBlending.floatValue = 0f;
		_DissolveBlendSpeed.floatValue = 0.2f;
		_DissolveMask.textureValue = null;
		_DissolveRimCol.colorValue = Color.white;
		_DissolveRimWidth.floatValue = 0.5f;
		_GeomDissolveAxis.floatValue = 1f;
		_GeomDissolveAmount.floatValue = 1f;
		_GeomDissolveAxisFlip.floatValue = 0f;
		_GeomDissolveWireframe.floatValue = 0f;
		_GeomDissolveWidth.floatValue = 0.25f;
		_GeomDissolveSpread.vectorValue = new Vector4(0.1f,0.1f,0.1f,0);
		_GeomDissolveClip.floatValue = 0f;
		_GeomDissolveFilter.floatValue = 1f;
		_GeomDissolveClamp.floatValue = 0f;
		_DissolvePoint0.vectorValue = new Vector4(0f,1f,0f,0f);
		_DissolvePoint1.vectorValue = Vector4.zero;
	}

	void DoScreenReset(){
		_Range.floatValue = 10f;
		_Position.vectorValue = new Vector4(0,0,0.25f,0);
		_Rotation.vectorValue = Vector4.zero;
	}

	void DoCloneReset(){
		_ClonePattern.floatValue = 0f;
		_Visibility.floatValue = 0f;
		_EntryPos.vectorValue = new Vector4(0,1,0,0);
		_SaturateEP.floatValue = 1f;
		_Clone1.vectorValue = new Vector4(1,0,0,1);
		_Clone2.vectorValue = new Vector4(-1,0,0,1);
		_Clone3.vectorValue = new Vector4(0,0,1,1);
		_Clone4.vectorValue = new Vector4(0,0,-1,1);
		_Clone5.vectorValue = new Vector4(0.5f,0,0.5f,1);
		_Clone6.vectorValue = new Vector4(-0.5f,0,0.5f,1);
		_Clone7.vectorValue = new Vector4(0.5f,0,-0.5f,1);
		_Clone8.vectorValue = new Vector4(-0.5f,0,-0.5f,1);
	}

	void DoGlitchReset(){
		_Instability.floatValue = 0f;
		_GlitchIntensity.floatValue = 0f;
		_GlitchFrequency.floatValue = 0f;
		_GlitchClones.floatValue = 0f;
	}

	void DoShatterReset(){
		_ShatterSpread.floatValue = 0.347f;
		_ShatterMin.floatValue = 0.25f;
		_ShatterMax.floatValue = 0.65f;
		_ShatterCull.floatValue = 0.535f;
		_ShatterClones.floatValue = 0f;
	}

	void DoWireframeReset(){
		_WFMode.floatValue = 0f;
		_WFColor.colorValue = new Color(0,0,0,1);
		_WFVisibility.floatValue = 1f;
		_WFFill.floatValue = 0f;
		_PatternMult.floatValue = 2.5f;
		_WFClones.floatValue = 0f;
	}
	
	void DoAdvancedReset(){
		DoRenderingReset();
		DoStencilReset();
	}

	void DoRenderingReset(){
		if (_BlendMode.floatValue != 5)
			_ZWrite.floatValue = 1f;
		else
			_ZWrite.floatValue = 0f;
		_ZTest.floatValue = 4f;
	}

	void DoStencilReset(){
		_StencilRef.floatValue = 1f;
		_StencilPass.floatValue = (float)UnityEngine.Rendering.StencilOp.Keep;
		_StencilFail.floatValue = (float)UnityEngine.Rendering.StencilOp.Keep;
		_StencilZFail.floatValue = (float)UnityEngine.Rendering.StencilOp.Keep;
		_StencilCompare.floatValue = (float)UnityEngine.Rendering.CompareFunction.Always;
		_OutlineStencilPass.floatValue = (float)UnityEngine.Rendering.StencilOp.Keep;
		_OutlineStencilCompare.floatValue = (float)UnityEngine.Rendering.CompareFunction.Always;
	}

	void ClearDictionaries(){
		baseTabButtons.Clear();
		texturesTabButtons.Clear();
		maskingTabButtons.Clear();
		roughnessTabButtons.Clear();
		smoothnessTabButtons.Clear();
		aoTabButtons.Clear();
		heightTabButtons.Clear();
		bcTabButtons.Clear();
		shadingTabButtons.Clear();
		lightingTabButtons.Clear();
		shadowTabButtons.Clear();
		reflTabButtons.Clear();
		specTabButtons.Clear();
		matcapTabButtons.Clear();
		sssTabButtons.Clear();
		rimTabButtons.Clear();
		eRimTabButtons.Clear();
		refracTabButtons.Clear();
		iriTabButtons.Clear();
		normalTabButtons.Clear();
		bigMaskTabButtons.Clear();
		emissTabButtons.Clear();
		lrTabButtons.Clear();
		pulseTabButtons.Clear();
		filterTabButtons.Clear();
		flipbookTabButtons.Clear();
		book0TabButtons.Clear();
		book1TabButtons.Clear();
		outlineTabButtons.Clear();
		uvdTabButtons.Clear();
		vertTabButtons.Clear();
		alTabButtons.Clear();
		alEmissTabButtons.Clear();
		alRimTabButtons.Clear();
		albcdTabButtons.Clear();
		aluvdTabButtons.Clear();
		alVertManipTabButtons.Clear();
		alDissolveTabButtons.Clear();
		alTriOffsTabButtons.Clear();
		alWireframeTabButtons.Clear();
		specialTabButtons.Clear();
		dfTabButtons.Clear();
		dissTabButtons.Clear();
		ssTabButtons.Clear();
		cloneTabButtons.Clear();
		glitchTabButtons.Clear();
		shatterTabButtons.Clear();
		wfTabButtons.Clear();
		renderTabButtons.Clear();
		alOutlineTabButtons.Clear();
		alVizTabButtons.Clear();
		metallicTabButtons.Clear();
	}
}