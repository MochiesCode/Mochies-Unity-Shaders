using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mochie {

    class GradientObject : ScriptableObject {
        public Gradient gradient = new Gradient();
    }

    internal class UberEditor : ShaderGUI {

        static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
        Toggles toggles = new Toggles(new string[] {
                "Base", 
                "Textures",
                "Shading", 
                "Masks", 
                "Lighting",
                "Shadows",
                "Reflections",
                "Specular Highlights",
                "Subsurface Scattering",
                "Matcap",
                "Basic Rim",
                "Roughness Filter",
                "Normals",
                "Emission",
                "Pulse",
                "Light Reactivity",
                "Filtering",
                "UV Distortion",
                "Outline",
                "Special Features",
                "Distance Fade",
                "Dissolve",
                "Screenspace",
                "Clones",
                "Clones ",
                "Glitch",
                "Shatter Culling",
                "Wireframe",
                "Flipbook",
                "Occlusion Filter",
                "Height Filter",
                "Primary Layer",
                "Secondary Layer",
                "Environment Rim",
                "Smoothness Filter",
                "Metallic Filter",
                "Render Settings",
                "Curvature Filter",
                "Primary Maps",
                "Detail Maps",
                "Primary Matcap",
                "Secondary Matcap",
                "General",
                "Diffuse Shading",
                "Realtime Lighting",
                "Baked Lighting",
                "Masks ",
                "Refraction",
                "Vertex Manipulation",
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
                "Base Color Dissolve",
                "Audio Link",
                "Emission ",
                "Rim",
                "Dissolve ",
                "Base Color Dissolve ",
                "Vertex Manipulation ",
                "Triangle Offset",
                "Wireframe ",
                "Iridescence",
                "UV Distortion ",
                "Outline ",
                // "Visualizers",
                "Oscilloscope",
                "Primary Matcap",
                "Secondary Matcap",
                "Debug"
        }, 0);

        private GradientObject gradientObj;
        private SerializedProperty colorGradient;
        private SerializedObject serializedGradient;
        private EditorWindow gradientWindow;
        private Texture2D rampTex;

        private GradientObject iridescenceGradientObj;
        private SerializedProperty iridescenceColorGradient;
        private SerializedObject iridescenceSerializedGradient;
        private EditorWindow iridescenceGradientWindow;
        private Texture2D iridescenceRampTex;

        static readonly int blendingLabelPos = 111;

        static readonly string unityFolderPath = "Assets/Mochie/Unity";
        string versionLabel = "v1.34.1";
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
        MaterialProperty _DetailMetallicBlending = null;
        MaterialProperty _DetailMetallic = null;
        MaterialProperty _DetailMetallicStrength = null;
        MaterialProperty _UseShadowsForLREmiss = null;
        MaterialProperty _HueMode = null;
        MaterialProperty _MonoTint = null;
        MaterialProperty _LitCubemap = null;
        MaterialProperty _LightVolumeSpecularity = null;
        MaterialProperty _LightVolumeSpecularityStrength = null;
        MaterialProperty _IridescenceMode = null;
        MaterialProperty _IridescenceRamp = null;

        MaterialProperty _VRCFallback = null;
        MaterialProperty _NaNLmao = null;

        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        MaterialEditor m_me;
        public override void OnGUI(MaterialEditor me, MaterialProperty[] props) {
            
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

            // For iridescence ramp gradient editor
            if (iridescenceGradientObj == null){
                iridescenceGradientObj = GradientObject.CreateInstance<GradientObject>();
                iridescenceSerializedGradient = new SerializedObject(iridescenceGradientObj);
                iridescenceColorGradient = iridescenceSerializedGradient.FindProperty("gradient");
                iridescenceRampTex = new Texture2D(128, 16);
                IridescenceGradientToTexture(ref iridescenceRampTex);
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

            // Add mat to foldout dictionary if it isn't in there yet
            if (!foldouts.ContainsKey(mat))
                foldouts.Add(mat, toggles);
                
            foreach (var obj in _BlendMode.targets)
                ApplyMaterialSettings((Material)obj);

            if (mat.GetInt("_MaterialResetCheck") == 0){
                mat.SetInt("_MaterialResetCheck", 1);
                ApplyMaterialSettings(mat);
                SetBlendMode(mat);
            }

            Texture2D standardIcon = (Texture2D)Resources.Load("StandardIcon", typeof(Texture2D));
            GUIContent standardButtonContent = new GUIContent(standardIcon, "Apply preset property values that visually match Standard shader.");

            string headerText = "UBER";
            if (isUberX && isOutline)
                headerText = "UBER X OUTLINE";
            else if (isUberX && !isOutline)
                headerText = "UBER X";
            else if (!isUberX && isOutline)
                headerText = "UBER OUTLINE";

            MGUI.DoHeader(headerText);

            // -----------------
            // Base Settings
            // -----------------
            bool baseToggle = Foldouts.DoFoldout(foldouts, mat, "Base", 2, Foldouts.Style.StandardButton);
            if (Foldouts.DoFoldoutButton(MGUI.collapseLabel, 11)) Toggles.CollapseFoldouts(mat, foldouts, 1);
            if (Foldouts.DoFoldoutButton(standardButtonContent, -13)) DoStandardLighting(mat);
            if (baseToggle) {
                MGUI.PropertyGroupParent(()=>{
                    MGUI.PropertyGroup(()=>{
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
                    });
                    MGUI.PropertyGroup(()=>{
                        switch((int)cubeMode){

                            // Tex Only
                            case 0: 
                                me.TexturePropertySingleLine(Tips.baseColorLabel, _MainTex, _Color, renderMode == 1 ? _ColorPreservation : null);
                                if (renderMode == 1) MGUI.TexPropLabel(Tips.colorPreservation, 123, true);
                                if (_MirrorBehavior.floatValue == 2)
                                    me.TexturePropertySingleLine(Tips.mirrorTexLabel, _MirrorTex);
                                MGUI.TextureSOScroll(me, _MainTex, _MainTexScroll);
                                me.ShaderProperty(_MainTexRot, "Rotation");
                                break;
                            
                            // Cubemap Only
                            case 1: 
                                me.TexturePropertySingleLine(Tips.reflCubeLabel, _MainTexCube0, _CubeColor0, renderMode == 1 ? _ColorPreservation : null);
                                if (renderMode == 1) 
                                    MGUI.TexPropLabel(Tips.colorPreservation, 123, true);
                                MGUI.Vector3Field(_CubeRotate0, "Rotation", false);
                                me.ShaderProperty(_AutoRotate0, "Auto Rotate");
                                break;
                            
                            // Tex and Cubemap
                            case 2: 
                                me.TexturePropertySingleLine(Tips.baseColorLabel, _MainTex, _Color, renderMode == 1 ? _ColorPreservation : null);
                                if (renderMode == 1) 
                                    MGUI.TexPropLabel(Tips.colorPreservation, 123, true);
                                if (_MirrorBehavior.floatValue == 2)
                                    me.TexturePropertySingleLine(Tips.mirrorTexLabel, _MirrorTex);
                                MGUI.TextureSOScroll(me, _MainTex, _MainTexScroll);
                                me.ShaderProperty(_MainTexRot, "Rotation");

                                me.TexturePropertySingleLine(new GUIContent("Blend"), _CubeBlendMask, _CubeBlendMask.textureValue ? null : _CubeBlend);

                                me.TexturePropertySingleLine(Tips.reflCubeLabel, _MainTexCube0, _CubeColor0, _CubeBlendMode);
                                MGUI.TexPropLabel("Blending", blendingLabelPos, true);
                                MGUI.Vector3Field(_CubeRotate0, "Rotation", false);
                                me.ShaderProperty(_AutoRotate0, "Auto Rotate");
                                break;
                            default: break;
                        }
                        if (_UseAlphaMask.floatValue == 1 && (isCutout || isTransparent)){
                            me.TexturePropertySingleLine(Tips.alphaMaskLabel, _AlphaMask, _AlphaMaskChannel);
                            MGUI.TextureSO(me, _AlphaMask);
                        }
                        me.ShaderProperty(_VertexColor, "Vertex Color");
                    });
                });
            }

            // -----------------
            // Textures
            // -----------------
            if (renderMode == 1){
                if (Foldouts.DoFoldout(foldouts, mat, "Textures", Foldouts.Style.Standard)) {
                    MGUI.PropertyGroupParent(()=>{
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
                                        me.ShaderProperty(_ParallaxOffset, Tips.heightOffsetText, 1);
                                        me.ShaderProperty(_ParallaxSteps, Tips.heightStepsText, 1);
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
                            bool usingDetMetal = _DetailMetallic.textureValue;
                            me.TexturePropertySingleLine(Tips.baseColorLabel, _DetailAlbedoMap, usingDetAlbedo ? _DetailAlbedoStrength : null, usingDetAlbedo ? _DetailAlbedoBlending : null);
                            if (workflow == 0 || workflow >= 3){
                                me.TexturePropertySingleLine(Tips.metallicTexLabel, _DetailMetallic, usingDetMetal ? _DetailMetallicStrength : null, usingDetMetal ? _DetailMetallicBlending : null);
                                me.TexturePropertySingleLine(Tips.roughnessTexLabel, _DetailRoughnessMap, usingDetRough ? _DetailRoughStrength : null, usingDetRough ? _DetailRoughBlending : null);
                            }
                            me.TexturePropertySingleLine(Tips.occlusionTexLabel, _DetailOcclusionMap, usingDetOcc ? _DetailOcclusionStrength : null, usingDetOcc ? _DetailOcclusionBlending : null);
                            me.TexturePropertySingleLine(Tips.normalTexLabel, _DetailNormalMap, usingDetNormal ? _DetailNormalMapScale : null);

                            if (usingDetAlbedo || usingDetNormal || usingDetRough || usingDetOcc || usingDetMetal) {
                                MGUI.TextureSOScroll(me, _DetailAlbedoMap, _DetailScroll);
                                me.ShaderProperty(_DetailRot, "Rotation");
                            }
                        });
                    });

                    // Masking
                    if (Foldouts.DoFoldout(foldouts, mat, me, _MaskingMode, "Masks", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_MaskingMode.floatValue == 0);
                            if (_MaskingMode.floatValue != 2){
                                me.ShaderProperty(_EnableMaskTransform, Tips.enableMaskTransformLabel);
                                MGUI.PropertyGroup(() => {
                                    me.TexturePropertySingleLine(Tips.reflLabel, _ReflectionMask);
                                    MGUI.TextureSOScroll(me, _ReflectionMask, _ReflectionMaskScroll, _ReflectionMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    me.TexturePropertySingleLine(Tips.specLabel, _SpecularMask);
                                    MGUI.TextureSOScroll(me, _SpecularMask, _SpecularMaskScroll, _SpecularMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    me.TexturePropertySingleLine(Tips.anisoBlendLabel, _InterpMask); 
                                    MGUI.TextureSOScroll(me, _InterpMask, _InterpMaskScroll, _InterpMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    me.TexturePropertySingleLine(Tips.matcapPrimaryMask, _MatcapMask);
                                    MGUI.TextureSOScroll(me, _MatcapMask, _MatcapMaskScroll, _MatcapMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    me.TexturePropertySingleLine(Tips.matcapSecondaryMask, _MatcapBlendMask);
                                    MGUI.TextureSOScroll(me, _MatcapBlendMask, _MatcapBlendMaskScroll, _MatcapBlendMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    me.TexturePropertySingleLine(Tips.shadowLabel, _ShadowMask);
                                    MGUI.TextureSOScroll(me, _ShadowMask, _ShadowMaskScroll, _ShadowMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    me.TexturePropertySingleLine(Tips.basicRimLabel, _RimMask);
                                    MGUI.TextureSOScroll(me, _RimMask, _RimMaskScroll, _RimMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    me.TexturePropertySingleLine(Tips.eRimLabel, _ERimMask);
                                    MGUI.TextureSOScroll(me, _ERimMask, _ERimMaskScroll, _ERimMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    me.TexturePropertySingleLine(Tips.diffuseLabel, _DiffuseMask);
                                    MGUI.TextureSOScroll(me, _DiffuseMask, _DiffuseMaskScroll, _DiffuseMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    me.TexturePropertySingleLine(Tips.subsurfLabel, _SubsurfaceMask);
                                    MGUI.TextureSOScroll(me, _SubsurfaceMask, _SubsurfaceMaskScroll, _SubsurfaceMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    me.TexturePropertySingleLine(Tips.detailLabel, _DetailMask);
                                    MGUI.TextureSOScroll(me, _DetailMask, _DetailMaskScroll, _DetailMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    me.TexturePropertySingleLine(Tips.emissLabel, _EmissMask);
                                    MGUI.TextureSOScroll(me, _EmissMask, _EmissMaskScroll, _EmissMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    me.TexturePropertySingleLine(Tips.emissPulseLabel, _PulseMask);
                                    MGUI.TextureSOScroll(me, _PulseMask, _EmissPulseMaskScroll, _PulseMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    me.TexturePropertySingleLine(Tips.filterLabel, _FilterMask);
                                    MGUI.TextureSOScroll(me, _FilterMask, _FilterMaskScroll, _FilterMask.textureValue && _EnableMaskTransform.floatValue == 1);	
                                    me.TexturePropertySingleLine(Tips.refractLabel, _RefractionMask);
                                    MGUI.TextureSOScroll(me, _RefractionMask, _RefractionMaskScroll, _RefractionMask.textureValue && _EnableMaskTransform.floatValue == 1);
                                    if (isOutline){
                                        me.TexturePropertySingleLine(Tips.olThickLabel, _OutlineMask);
                                        MGUI.TextureSOScroll(me, _OutlineMask, _OutlineMaskScroll, _OutlineMask.textureValue && _EnableMaskTransform.floatValue == 1);
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
                            MGUI.ToggleGroupEnd();
                        });
                    }
                    
                    if (workflow == 0 || workflow >= 3){

                        // Metallic Filtering
                        if (Foldouts.DoFoldout(foldouts, mat, me, _MetallicFiltering, "Metallic Filter", Foldouts.Style.ThinToggle)) {
                            MGUI.PropertyGroupParent(() => {
                                MGUI.ToggleGroup(_MetallicFiltering.floatValue == 0);
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_PreviewMetallic, "Preview");
                                    MGUI.SliderMinMax01(_MetallicRemapMin, _MetallicRemapMax, "Remap", 1);
                                    me.ShaderProperty(_MetallicLightness, "Lightness");
                                    me.ShaderProperty(_MetallicIntensity, "Intensity");
                                    me.ShaderProperty(_MetallicContrast, "Contrast");
                                });
                                MGUI.ToggleGroupEnd();
                            });
                        }

                        // Roughness Filtering
                        if (Foldouts.DoFoldout(foldouts, mat, me, _RoughnessFiltering, "Roughness Filter", Foldouts.Style.ThinToggle)) {
                            MGUI.PropertyGroupParent(() => {
                                MGUI.ToggleGroup(_RoughnessFiltering.floatValue == 0);
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_PreviewRough, "Preview");
                                    MGUI.SliderMinMax01(_RoughRemapMin, _RoughRemapMax, "Remap", 1);
                                    me.ShaderProperty(_RoughLightness, "Lightness");
                                    me.ShaderProperty(_RoughIntensity, "Intensity");
                                    me.ShaderProperty(_RoughContrast, "Contrast");
                                });
                                MGUI.ToggleGroupEnd();
                            });
                        }
                    }

                    // Smoothness Filtering (for specular)
                    else {
                        if (Foldouts.DoFoldout(foldouts, mat, me, _SmoothnessFiltering, "Smoothness Filter", Foldouts.Style.ThinToggle)) {
                            MGUI.PropertyGroupParent(() => {
                                MGUI.ToggleGroup(_SmoothnessFiltering.floatValue == 0);
                                MGUI.PropertyGroup(()=>{
                                    me.ShaderProperty(_PreviewSmooth, "Preview");
                                    MGUI.SliderMinMax01(_SmoothRemapMin, _SmoothRemapMax, "Remap", 1);
                                    me.ShaderProperty(_SmoothLightness, "Lightness");
                                    me.ShaderProperty(_SmoothIntensity, "Intensity");
                                    me.ShaderProperty(_SmoothContrast, "Contrast");
                                    
                                });
                                MGUI.ToggleGroupEnd();
                            });
                        }
                    }

                    
                    if (Foldouts.DoFoldout(foldouts, mat, me, _AOFiltering, "Occlusion Filter", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(() => {
                            MGUI.ToggleGroup(_AOFiltering.floatValue == 0);
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_PreviewAO, "Preview");
                                me.TexturePropertySingleLine(Tips.tintLabel, _AOTintTex, _AOTint);
                                MGUI.SliderMinMax01(_AORemapMin, _AORemapMax, "Remap", 1);
                                me.ShaderProperty(_AOLightness, "Lightness");
                                me.ShaderProperty(_AOIntensity, "Intensity");
                                me.ShaderProperty(_AOContrast, "Contrast");
                                
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    // Height Filtering
                    if (Foldouts.DoFoldout(foldouts, mat, me, _HeightFiltering, "Height Filter", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(() => {
                            MGUI.ToggleGroup(_HeightFiltering.floatValue == 0);
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_PreviewHeight, "Preview");
                                MGUI.SliderMinMax01(_HeightRemapMin, _HeightRemapMax, "Remap", 1);
                                me.ShaderProperty(_HeightLightness, "Lightness");
                                me.ShaderProperty(_HeightIntensity, "Intensity");
                                me.ShaderProperty(_HeightContrast, "Contrast");
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }
                    
                    // Base Color Dissolve
                    if (Foldouts.DoFoldout(foldouts, mat, me, _BCDissolveToggle, "Base Color Dissolve", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(() => {
                            MGUI.ToggleGroup(_BCDissolveToggle.floatValue == 0);
                            MGUI.PropertyGroup(()=>{
                                me.TexturePropertySingleLine(Tips.baseColor2Label, _MainTex2, _BCColor);
                                MGUI.TextureSO(me, _MainTex2, _MainTex2.textureValue);
                                me.TexturePropertySingleLine(Tips.dissolveTexLabel, _BCNoiseTex, _BCDissolveStr);
                                MGUI.TextureSO(me, _BCNoiseTex, _BCNoiseTex.textureValue);
                                me.ShaderProperty(_BCRimCol, "Rim Color");
                                me.ShaderProperty(_BCRimWidth, "Rim Width");
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }
                    MGUI.Space6();
                }

                // -----------------
                // Shading
                // -----------------

                if (Foldouts.DoFoldout(foldouts, mat, "Shading", Foldouts.Style.Standard)) {
                    MGUI.Space6();

                    // Lighting
                    if (Foldouts.DoFoldout(foldouts, mat, "Lighting", Foldouts.Style.Thin)) {
                        MGUI.PropertyGroupParent(()=>{
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
                        });
                    }

                    // Shadows
                    if (Foldouts.DoFoldout(foldouts, mat, me, _ShadowMode, "Shadows", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_ShadowMode.floatValue == 0);
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

                                        TextureImporter importer = (TextureImporter) AssetImporter.GetAtPath(rampPath);
                                        importer.wrapMode = TextureWrapMode.Clamp;
                                        importer.SaveAndReimport();

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
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    // Reflections
                    if (Foldouts.DoFoldout(foldouts, mat, me, _Reflections, "Reflections", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(() => {
                            MGUI.PropertyGroup(()=>{
                                GUIContent reflCubeLabel = Tips.reflCubeLabel;
                                if (_Reflections.floatValue == 1)
                                    reflCubeLabel.text = "Fallback Cubemap";
                                else
                                    reflCubeLabel.text = "Cubemap";
                                me.TexturePropertySingleLine(reflCubeLabel, _ReflCube, _LitCubemap);
                                MGUI.TexPropLabel(Tips.litCubemapLabel, 100, false);
                                me.ShaderProperty(_ReflCol, "Tint");
                                me.ShaderProperty(_ReflectionStr, "Strength");
                                MGUI.ToggleFloat(me, "Fresnel", _FresnelToggle, _FresnelStrength);
                                MGUI.ToggleSlider(me, "Manual Roughness", _ReflUseRough, _ReflRough);
                                MGUI.ToggleIntSlider(me, "Stepping", _ReflStepping, _ReflSteps);
                                me.ShaderProperty(_LightingBasedIOR, Tips.lightingBasedIOR);
                                me.ShaderProperty(_SSR, Tips.ssrText);
                            });
                        });
                        if (_SSR.floatValue == 1){
                            MGUI.PropertyGroup(() => {
                                MGUI.DisplayInfo("SSR requires a render queue of 2501 or above to function correctly.");
                                MGUI.DisplayInfo("\nSSR in VRChat requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs\nAnd can only be used on 1 material per scene/avatar.\n\nIt is also is VERY expensive, please use it sparingly!\n");
                                me.ShaderProperty(_Alpha, "Strength");
                                me.ShaderProperty(_MaxSteps, "Max Steps");
                                me.ShaderProperty(_Step, "Step Size");
                                me.ShaderProperty(_LRad, "Intersection (L)");
                                me.ShaderProperty(_SRad, "Intersection (S)");
                                me.ShaderProperty(_EdgeFade, "Edge Fade");
                            });
                        }
                    }

                    // Specular Highlights
                    if (Foldouts.DoFoldout(foldouts, mat, me, _Specular, "Specular Highlights", Foldouts.Style.ThinToggle)) {
                        if (_Specular.floatValue == 3){
                            MGUI.Space6();
                            MGUI.DisplayInfo("Note: Use the Specular Blend mask in the masks tab to interpolate between GGX and Anisotropic");
                            MGUI.Space6();
                        }
                        MGUI.PropertyGroupParent(() => {
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_SpecCol, "Tint");
                                if (_Specular.floatValue == 1){
                                    me.ShaderProperty(_SpecStr, "Strength");
                                    MGUI.ToggleFloat(me, "Light Volume Specularity", _LightVolumeSpecularity, _LightVolumeSpecularityStrength);
                                    MGUI.ToggleSlider(me, "Manual Roughness", _SpecUseRough, _SpecRough);
                                    MGUI.ToggleSlider(me, Tips.specBiasOverride, _SpecBiasOverrideToggle, _SpecBiasOverride);
                                    MGUI.ToggleIntSlider(me, "Stepping", _SharpSpecular, _SharpSpecStr);
                                    MGUI.SpaceN1();
                                    me.ShaderProperty(_RealtimeSpec, Tips.realtimeSpec);
                                }
                                else if (_Specular.floatValue == 2){
                                    me.ShaderProperty(_AnisoStr, "Strength");
                                    MGUI.ToggleFloat(me, "Light Volume Specularity", _LightVolumeSpecularity, _LightVolumeSpecularityStrength);
                                    me.ShaderProperty(_RealtimeSpec, Tips.realtimeSpec);
                                    MGUI.ToggleIntSlider(me, "Stepping", _SharpSpecular, _AnisoSteps);
                                    MGUI.SpaceN1();
                                }
                                else {
                                    me.ShaderProperty(_SpecStr, "GGX Strength");
                                    me.ShaderProperty(_AnisoStr, "Aniso Strength");
                                    MGUI.ToggleFloat(me, "Light Volume Specularity", _LightVolumeSpecularity, _LightVolumeSpecularityStrength);
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
                                    MGUI.PropertyGroup(() => {
                                        MGUI.SpaceN2();
                                        me.ShaderProperty(_AnisoAngleY, "Layer 1 Thickness");
                                        me.ShaderProperty(_AnisoLayerY, "Layer 2 Thickness");
                                        me.ShaderProperty(_AnisoLayerStr, "Layer Blend");
                                        me.ShaderProperty(_AnisoLerp, "Lerp Blend");
                                        MGUI.SpaceN2();
                                    });
                                    MGUI.PropertyGroup(() => {
                                        MGUI.SpaceN2();
                                        me.ShaderProperty(_RippleStrength, "Hair Strength");
                                        me.ShaderProperty(_RippleFrequency, "Hair Density");
                                        me.ShaderProperty(_RippleAmplitude, "Hair Intensity");
                                        me.ShaderProperty(_RippleContinuity, "Hair Continuity");
                                        MGUI.SpaceN2();
                                    });
                                }
                            });
                        });
                    }

                    // Matcap
                    if (Foldouts.DoFoldout(foldouts, mat, me, _MatcapToggle, "Matcap", Foldouts.Style.ThinToggle)) {
                        MGUI.ToggleGroup(_MatcapToggle.floatValue == 0);
                        MGUI.Space2();
                        if (Foldouts.DoFoldout(foldouts, mat, "Primary Matcap", Foldouts.Style.ThinShort)) {
                            MGUI.PropertyGroupParent(() => {
                                MGUI.PropertyGroup(() => {
                                    me.TexturePropertySingleLine(new GUIContent("Matcap"), _Matcap, _MatcapColor, _MatcapBlending);
                                    MGUI.TexPropLabel("Blending", blendingLabelPos, true);
                                    if (_Matcap.textureValue){
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
                                MGUI.PropertyGroup(() => {
                                    MGUI.SpaceN2();
                                    me.ShaderProperty(_MatcapStr, "Strength");
                                    MGUI.ToggleSlider(me, "Manual Roughness", _MatcapUseRough, _MatcapRough);

                                    me.ShaderProperty(_UnlitMatcap, "Unlit");
                                    me.ShaderProperty(_MatcapCenter, "No Depth in VR");
                                    MGUI.SpaceN2();
                                });
                            });
                        }

                        if (Foldouts.DoFoldout(foldouts, mat, "Secondary Matcap", Foldouts.Style.ThinShort)) {
                            MGUI.PropertyGroupParent(()=>{
                                MGUI.PropertyGroup(() => {
                                    me.TexturePropertySingleLine(new GUIContent("Matcap"), _Matcap1, _MatcapColor1, _MatcapBlending1);
                                    MGUI.TexPropLabel("Blending", blendingLabelPos, true);
                                    if (_Matcap1.textureValue){
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
                                MGUI.PropertyGroup(() => {
                                    MGUI.SpaceN2();
                                    me.ShaderProperty(_MatcapStr1, "Strength");
                                    MGUI.ToggleSlider(me, "Manual Roughness", _MatcapUseRough1, _MatcapRough1);
                                    me.ShaderProperty(_UnlitMatcap1, "Unlit");
                                    me.ShaderProperty(_MatcapCenter1, "No Depth in VR");
                                    MGUI.ToggleGroupEnd();
                                    MGUI.SpaceN2();
                                });
                            });
                        }
                        MGUI.ToggleGroupEnd();
                        MGUI.Space2();
                    }

                    // Subsurface Scattering
                    if (Foldouts.DoFoldout(foldouts, mat, me, _Subsurface, "Subsurface Scattering", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(() => {
                            MGUI.ToggleGroup(_Subsurface.floatValue == 0);
                            MGUI.PropertyGroup(()=>{
                                me.TexturePropertySingleLine(Tips.thicknessTexLabel, _ThicknessMap, _ThicknessMapPower);
                                me.TexturePropertySingleLine(Tips.colorLabel, _ScatterTex, _ScatterCol, _ScatterBaseColorTint);
                                MGUI.TexPropLabel("Base Color Tint", 150, true);
                                me.ShaderProperty(_ScatterIntensity, "Direct Strength");
                                me.ShaderProperty(_ScatterAmbient, "Indirect Strength");
                                me.ShaderProperty(_ScatterPow, "Power");
                                me.ShaderProperty(_ScatterDist, "Normal Strength");
                                me.ShaderProperty(_ScatterWrap, "Wrapping Factor");
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    // Rim
                    if (Foldouts.DoFoldout(foldouts, mat, me, _RimLighting, "Basic Rim", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(() => {
                            MGUI.ToggleGroup(_RimLighting.floatValue == 0);
                            MGUI.PropertyGroup(()=>{
                                me.TexturePropertySingleLine(Tips.colorLabel, _RimTex, _RimCol, _RimBlending);
                                MGUI.TexPropLabel("Blending", blendingLabelPos, true);
                                MGUI.TextureSOScroll(me, _RimTex, _RimScroll, _RimTex.textureValue);
                                me.ShaderProperty(_RimStr, "Strength");
                                me.ShaderProperty(_RimWidth, "Width");
                                me.ShaderProperty(_RimEdge, "Sharpness");
                                me.ShaderProperty(_UnlitRim, "Unlit");
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    // Environment Rim
                    if (Foldouts.DoFoldout(foldouts, mat, me, _EnvironmentRim, "Environment Rim", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(() => {
                            MGUI.ToggleGroup(_EnvironmentRim.floatValue == 0);
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_ERimBlending, "Blending");
                                me.ShaderProperty(_ERimTint, "Tint");
                                me.ShaderProperty(_ERimStr, "Strength");
                                me.ShaderProperty(_ERimWidth, "Width");
                                me.ShaderProperty(_ERimEdge, "Sharpness");
                                MGUI.ToggleSlider(me, "Manual Roughness", _ERimUseRough, _ERimRoughness);
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    // Refraction
                    if (Foldouts.DoFoldout(foldouts, mat, me, _Refraction, "Refraction", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_Refraction.floatValue == 0);
                            MGUI.PropertyGroup(() => {
                                MGUI.DisplayInfo("Refraction requires a render queue of 2501 or above to function correctly.");
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
                        });
                    }

                    // Iridescence
                    if (Foldouts.DoFoldout(foldouts, mat, me, _Iridescence, "Iridescence", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_Iridescence.floatValue == 0);
                            MGUI.PropertyGroup( () => {
                                me.ShaderProperty(_IridescenceMode, "Mode");
                                me.TexturePropertySingleLine(Tips.maskLabel, _IridescenceMask);
                                if (_IridescenceMode.floatValue == 1){
                                    me.TexturePropertySingleLine(Tips.shadowRampLabel, _IridescenceRamp);
                                    GUILayout.Space(-19);
                                    EditorGUI.BeginChangeCheck();
                                    EditorGUILayout.PropertyField(iridescenceColorGradient, new GUIContent(" "));
                                    if (EditorGUI.EndChangeCheck()){
                                        iridescenceSerializedGradient.ApplyModifiedProperties();
                                        IridescenceGradientToTexture(ref iridescenceRampTex);
                                    }
                                    if (MGUI.SimpleButton("Apply", MGUI.GetPropertyWidth(), EditorGUIUtility.labelWidth)){
                                        byte[] iridescenceEncodedTex = iridescenceRampTex.EncodeToPNG();
                                        int iridescenceRampID = UnityEngine.Random.Range(0,10000000);
                                        string iridescenceRampPath = unityFolderPath+"/Textures/Ramps/Ramp_"+iridescenceRampID+".png";
                                        MGUI.WriteBytes(iridescenceEncodedTex, iridescenceRampPath);
                                        AssetDatabase.ImportAsset(iridescenceRampPath);
                                        
                                        TextureImporter importer = (TextureImporter) AssetImporter.GetAtPath(iridescenceRampPath);
                                        importer.wrapMode = TextureWrapMode.Clamp;
                                        importer.SaveAndReimport();
                                        
                                        _IridescenceRamp.textureValue = (Texture)EditorGUIUtility.Load(iridescenceRampPath);
                                    }
                                }
                                me.ShaderProperty(_IridescenceStrength, "Strength");
                                if (_IridescenceMode.floatValue == 0){
                                    me.ShaderProperty(_IridescenceWidth, "Width");
                                    me.ShaderProperty(_IridescenceEdge, "Sharpness");
                                }
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    // Normals
                    if (Foldouts.DoFoldout(foldouts, mat, "Normals", Foldouts.Style.Thin)) {
                        MGUI.PropertyGroupParent(() => {
                            MGUI.PropertyGroup(()=>{
                                me.ShaderProperty(_HardenNormals, "Hard Edges");
                                me.ShaderProperty(_ClearCoat, Tips.clearCoat);
                                me.ShaderProperty(_GSAA, Tips.gsaa);
                            });
                        });
                    };
                    MGUI.Space6();
                }
            }
            else {
                if (Foldouts.DoFoldout(foldouts, mat, me, _MaskingMode, "Masks ", Foldouts.Style.StandardToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_MaskingMode.floatValue == 0);
                        if (_MaskingMode.floatValue != 2){
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
                        MGUI.ToggleGroupEnd();
                    });
                }
            }
            
            // -----------------
            // Emission
            // -----------------
            if (Foldouts.DoFoldout(foldouts, mat, me, _EmissionToggle, "Emission", Foldouts.Style.StandardToggle)) {
                MGUI.ToggleGroup(_EmissionToggle.floatValue == 0);
                MGUI.PropertyGroupParent(()=>{
                    MGUI.PropertyGroup(() => {
                        me.TexturePropertySingleLine(Tips.emissTexLabel, _EmissionMap, _EmissionColor, _EmissIntensity);
                        MGUI.TexPropLabel("Intensity", 111, true);
                        MGUI.TextureSOScroll(me, _EmissionMap, _EmissScroll);
                    });
                    MGUI.PropertyGroup(() => {
                        me.TexturePropertySingleLine(Tips.emissTexLabel, _EmissionMap2, _EmissionColor2, _EmissIntensity2);
                        MGUI.TexPropLabel("Intensity", 111, true);
                        MGUI.TextureSOScroll(me, _EmissionMap2, _EmissScroll2);
                    });
                });
                if (Foldouts.DoFoldout(foldouts, mat, me, _ReactToggle, "Light Reactivity", Foldouts.Style.ThinToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_ReactToggle.floatValue == 0);
                        me.ShaderProperty(_UseShadowsForLREmiss, "Include Shadows");
                        me.ShaderProperty(_CrossMode, Tips.crossMode);
                        MGUI.PropertyGroup(() => {
                            MGUI.ToggleGroup(_CrossMode.floatValue == 0);
                            me.ShaderProperty(_ReactThresh, Tips.reactThresh);
                            me.ShaderProperty(_Crossfade, Tips.crossFade);
                            MGUI.ToggleGroupEnd();
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }

                if (Foldouts.DoFoldout(foldouts, mat, me, _PulseToggle, "Pulse", Foldouts.Style.ThinToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_PulseToggle.floatValue == 0);
                        me.ShaderProperty(_PulseWaveform, "Waveform");
                        MGUI.PropertyGroup(() => {
                            me.ShaderProperty(_PulseStr, "Strength");
                            me.ShaderProperty(_PulseSpeed, "Speed");
                        });
                        MGUI.ToggleGroupEnd();
                    });
                };
                MGUI.ToggleGroupEnd();
                MGUI.Space6();
            }
                
            // -----------------
            // Filters
            // -----------------
            if (Foldouts.DoFoldout(foldouts, mat, me, _Filtering, "Filtering", Foldouts.Style.StandardToggle)) {
                MGUI.PropertyGroupParent(()=>{
                    MGUI.ToggleGroup(_Filtering.floatValue == 0);
                    MGUI.PropertyGroup(() => {
                        me.ShaderProperty(_TeamFiltering, "Color Masking");
                        me.ShaderProperty(_PostFiltering, Tips.postFiltering);
                        me.ShaderProperty(_Invert, "Invert");
                        
                    });
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_HueMode, "Hue Mode");
                        me.ShaderProperty(_MonoTint, Tips.monoTintText);
                        me.ShaderProperty(_AutoShift, "Auto Hue Shift");
                        if (_AutoShift.floatValue == 0)
                            me.ShaderProperty(_Hue, "Hue");
                        else
                            me.ShaderProperty(_AutoShiftSpeed, "Shift Speed");
                    });
                    MGUI.PropertyGroup(() => {

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
                });
            }

            // -----------------
            // Flipbook
            // -----------------
            if (Foldouts.DoFoldout(foldouts, mat, "Flipbook", Foldouts.Style.Standard)) {
                MGUI.Space6();
                if (Foldouts.DoFoldout(foldouts, mat, me, _EnableSpritesheet, "Primary Layer", Foldouts.Style.ThinToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_EnableSpritesheet.floatValue == 0);
                        me.ShaderProperty(_SpritesheetMode0, "Mode");
                        MGUI.PropertyGroup(() => {
                            if (_SpritesheetMode0.floatValue == 1)
                                me.TexturePropertySingleLine(new GUIContent("Sprite Sheet"), _Spritesheet, _SpritesheetCol, _SpritesheetBlending);
                            else
                                me.TexturePropertySingleLine(new GUIContent("Flipbook Asset"), _Flipbook0, _SpritesheetCol, _SpritesheetBlending);
                            MGUI.TexPropLabel("Blending", blendingLabelPos, true);
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
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }

                if (Foldouts.DoFoldout(foldouts, mat, me, _EnableSpritesheet1, "Secondary Layer", Foldouts.Style.ThinToggle)) {
                    MGUI.PropertyGroupParent(()=>{
                        MGUI.ToggleGroup(_EnableSpritesheet1.floatValue == 0);
                        me.ShaderProperty(_SpritesheetMode1, "Mode");
                        MGUI.PropertyGroup(() => {
                            if (_SpritesheetMode1.floatValue == 1)
                                me.TexturePropertySingleLine(new GUIContent("Sprite Sheet"), _Spritesheet1, _SpritesheetCol1, _SpritesheetBlending1);
                            else
                                me.TexturePropertySingleLine(new GUIContent("Flipbook Asset"), _Flipbook1, _SpritesheetCol1, _SpritesheetBlending1);
                            MGUI.TexPropLabel("Blending", blendingLabelPos, true);
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
                            
                            
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }
                MGUI.Space6();
            }

            // -----------------
            // Outline
            // -----------------
            if (isOutline){
                if (Foldouts.DoFoldout(foldouts, mat, "Outline", Foldouts.Style.Standard)) {
                    MGUI.PropertyGroupParent(()=>{
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
                                MGUI.TexPropLabel("Use Alpha", 115, true);
                            }
                            MGUI.TextureSOScroll(me, _OutlineTex, _OutlineScroll, _OutlineTex.textureValue);
                            me.ShaderProperty(_OutlineMult, "Thickness");
                            me.ShaderProperty(_OutlineThicc, "Multiplier");
                            me.ShaderProperty(_OutlineRange, Tips.outlineRange);
                        });
                        MGUI.ToggleGroupEnd();
                    });
                }
            }

            // -----------------
            // UV Distortion
            // -----------------
            if (Foldouts.DoFoldout(foldouts, mat, me, _DistortionStyle, "UV Distortion", Foldouts.Style.StandardToggle)) {
                MGUI.PropertyGroupParent(()=>{
                    MGUI.ToggleGroup(_DistortionStyle.floatValue == 0);
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
                    MGUI.ToggleGroupEnd();
                });
            }

            // -----------------
            // Vertex Manip
            // -----------------
            if (Foldouts.DoFoldout(foldouts, mat, me, _VertexManipulationToggle, "Vertex Manipulation", Foldouts.Style.StandardToggle)) {
                MGUI.PropertyGroupParent(()=>{
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
                });
            }

            // -----------------
            // Audio Link
            // -----------------
            if (Foldouts.DoFoldout(foldouts, mat, me, _AudioLinkToggle, "Audio Link", Foldouts.Style.StandardToggle)) {
                MGUI.PropertyGroupParent(()=>{
                    MGUI.PropertyGroup(()=>{
                        me.ShaderProperty(_AudioLinkStrength, "Strength");
                        MGUI.ToggleGroup(_AudioLinkToggle.floatValue == 0);
                        MGUI.SliderMinMax(_AudioLinkRemapMin, _AudioLinkRemapMax, 0f, 2f, "Remap", 0);
                    });
                });

                if (Foldouts.DoFoldout(foldouts, mat, "Oscilloscope", Foldouts.Style.Thin)) {
                    MGUI.PropertyGroupParent(() => {
                        MGUI.PropertyGroup(() => {
                            me.ShaderProperty(_OscilloscopeStrength, "Strength");
                            me.ShaderProperty(_OscilloscopeCol, "Color");
                            MGUI.Vector2Field(_OscilloscopeMarginLR, "Margin Left/Right");
                            MGUI.Vector2Field(_OscilloscopeMarginTB, "Margin Top/Bottom");
                            MGUI.Vector2Field(_OscilloscopeScale, "Scale");
                            MGUI.Vector2Field(_OscilloscopeOffset, "Offset");
                            me.ShaderProperty(_OscilloscopeRot, "Rotation");
                        });
                    });
                }

                if (Foldouts.DoFoldout(foldouts, mat, "Emission ", Foldouts.Style.Thin)) {
                    MGUI.PropertyGroupParent(() => {
                        MGUI.PropertyGroup(() => {
                            me.ShaderProperty(_AudioLinkEmissionBand, "Band");
                            me.ShaderProperty(_AudioLinkEmissionMultiplier, "Strength");
                            MGUI.SliderMinMax(_AudioLinkRemapEmissionMin, _AudioLinkRemapEmissionMax, 0f, 2f, "Remap", 1);
                        });
                    });
                }

                if (Foldouts.DoFoldout(foldouts, mat, "Rim", Foldouts.Style.Thin)) {
                    MGUI.PropertyGroupParent(() => {
                        MGUI.PropertyGroup(() => {
                            me.ShaderProperty(_AudioLinkRimBand, "Band");
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
                    });
                }

                if (Foldouts.DoFoldout(foldouts, mat, "Base Color Dissolve ", Foldouts.Style.Thin)) {
                    MGUI.PropertyGroupParent(() => {
                        MGUI.PropertyGroup(() => {
                            me.ShaderProperty(_AudioLinkBCDissolveBand, "Band");
                            me.ShaderProperty(_AudioLinkBCDissolveMultiplier, "Strength");
                            MGUI.SliderMinMax(_AudioLinkRemapBCDissolveMin, _AudioLinkRemapBCDissolveMax, 0f, 2f, "Remap", 1);
                        });
                    });
                }
                
                if (Foldouts.DoFoldout(foldouts, mat, "UV Distortion ", Foldouts.Style.Thin)) {
                    MGUI.PropertyGroupParent(() => {
                        MGUI.PropertyGroup(() => {
                            me.ShaderProperty(_AudioLinkUVDistortionBand, "Band");
                            me.ShaderProperty(_AudioLinkUVDistortionMultiplier, "Strength");
                            MGUI.SliderMinMax(_AudioLinkRemapUVDistortionMin, _AudioLinkRemapUVDistortionMax, 0f, 2f, "Remap", 1);
                        });
                    });
                }

                if (isOutline){
                    if (Foldouts.DoFoldout(foldouts, mat, "Outline ", Foldouts.Style.Thin)) {
                        MGUI.PropertyGroupParent(() => {
                            MGUI.PropertyGroup(() => {
                                me.ShaderProperty(_AudioLinkOutlineBand, "Band");
                                me.ShaderProperty(_AudioLinkOutlineMultiplier, "Strength");
                                MGUI.SliderMinMax(_AudioLinkRemapOutlineMin, _AudioLinkRemapOutlineMax, 0f, 2f, "Remap", 1);
                            });
                        });
                    }
                }

                if (isUberX){
                    if (Foldouts.DoFoldout(foldouts, mat, "Vertex Manipulation ", Foldouts.Style.Thin)) {
                        MGUI.PropertyGroupParent(() => {
                            MGUI.PropertyGroup(() => {
                                me.ShaderProperty(_AudioLinkVertManipBand, "Band");
                                me.ShaderProperty(_AudioLinkVertManipMultiplier, "Strength");
                                MGUI.SliderMinMax(_AudioLinkRemapVertManipMin, _AudioLinkRemapVertManipMax, 0f, 2f, "Remap", 1);
                            });
                        });
                    }

                    
                    if (Foldouts.DoFoldout(foldouts, mat, "Dissolve ", Foldouts.Style.Thin)) {
                        MGUI.PropertyGroupParent(() => {
                            MGUI.PropertyGroup(() => {
                                me.ShaderProperty(_AudioLinkDissolveBand, "Band");
                                me.ShaderProperty(_AudioLinkDissolveMultiplier, "Strength");
                                MGUI.SliderMinMax(_AudioLinkRemapDissolveMin, _AudioLinkRemapDissolveMax, 0f, 2f, "Remap", 1);
                            });
                        });
                    }

                    if (Foldouts.DoFoldout(foldouts, mat, "Triangle Offset", Foldouts.Style.Thin)) {
                        MGUI.PropertyGroupParent(() => {
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
                        });
                    }

                    if (Foldouts.DoFoldout(foldouts, mat, "Wireframe ", Foldouts.Style.Thin)) {
                        MGUI.PropertyGroupParent(() => {
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
                        });
                    }
                    MGUI.Space6();
                }
                MGUI.ToggleGroupEnd();
            }

            // -----------------
            // X Features
            // -----------------
            if (isUberX){
                
                if (Foldouts.DoFoldout(foldouts, mat, "Special Features", Foldouts.Style.Standard)) {
                    MGUI.Space6();
                    if (Foldouts.DoFoldout(foldouts, mat, me, _DistanceFadeToggle, "Distance Fade", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_DistanceFadeToggle.floatValue == 0);
                            MGUI.DisplayInfo("Requires non-opaque blending mode to function.");
                            MGUI.PropertyGroup(() => {
                                me.ShaderProperty(_DFClones, "Clones Only");
                                if (_DistanceFadeToggle.floatValue != 2){
                                    me.ShaderProperty(_DistanceFadeMin, "Range");
                                    me.ShaderProperty(_ClipRimColor, "Rim Color");
                                    me.ShaderProperty(_ClipRimStr, "Intensity");
                                    me.ShaderProperty(_ClipRimWidth, "Width");
                                }
                                else {
                                    me.ShaderProperty(_DistanceFadeMin, "Min Range"); 
                                    me.ShaderProperty(_DistanceFadeMax, "Max Range");
                                }
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    if (Foldouts.DoFoldout(foldouts, mat, me, _DissolveStyle, "Dissolve", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_DissolveStyle.floatValue == 0);
                            MGUI.DisplayInfo("Texture and simplex dissolve require a non-opaque blending mode to function.");
                            MGUI.PropertyGroup(() => {
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
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }
                    
                    // Screenspace
                    if (Foldouts.DoFoldout(foldouts, mat, me, _Screenspace, "Screenspace", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(()=>{
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
                        });
                    }

                    // Clones
                    if (Foldouts.DoFoldout(foldouts, mat, me, _CloneToggle, "Clones", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_CloneToggle.floatValue == 0);
                            MGUI.PropertyGroup(() => {
                                me.ShaderProperty(_Visibility, "Enable");
                                MGUI.Vector3Field(_EntryPos, "Offset Direction", false);
                                me.ShaderProperty(_SaturateEP, "Clamp Offset Direction");
                                EditorGUI.BeginChangeCheck();
                                me.ShaderProperty(_ClonePattern, "Pattern Preset");
                                if (EditorGUI.EndChangeCheck())
                                    ApplyClonePositions();
                                bool positionsFoldout = Foldouts.DoSmallFoldout(foldouts, mat, me, "Clones ");
                                if (positionsFoldout){
                                    MGUI.PropertyGroup(() => {
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
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    // Glitch
                    if (Foldouts.DoFoldout(foldouts, mat, me, _GlitchToggle, "Glitch", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(()=>{
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
                        });
                    }

                    // Shatter Culling
                    if (Foldouts.DoFoldout(foldouts, mat, me, _ShatterToggle, "Shatter Culling", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_ShatterToggle.floatValue == 0);
                            MGUI.PropertyGroup(() => {
                                MGUI.ToggleGroup(_CloneToggle.floatValue == 0);
                                me.ShaderProperty(_ShatterClones, "Clones Only");
                                MGUI.ToggleGroupEnd();
                                me.ShaderProperty(_ShatterSpread, "Spread");
                                me.ShaderProperty(_ShatterMin, "Min Range");
                                me.ShaderProperty(_ShatterMax, "Max Range");
                                me.ShaderProperty(_ShatterCull, "Culling Range");
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }

                    // Wireframe
                    if (Foldouts.DoFoldout(foldouts, mat, me, _WireframeToggle, "Wireframe", Foldouts.Style.ThinToggle)) {
                        MGUI.PropertyGroupParent(()=>{
                            MGUI.ToggleGroup(_WireframeToggle.floatValue == 0);
                            MGUI.PropertyGroup(() => {
                                MGUI.ToggleGroup(_CloneToggle.floatValue == 0);
                                me.ShaderProperty(_WFClones, "Clones Only");
                                MGUI.ToggleGroupEnd();
                                me.ShaderProperty(_WFMode, "Pattern");
                                me.ShaderProperty(_WFColor, "Color");
                                me.ShaderProperty(_WFVisibility, "Wire Opacity");
                                me.ShaderProperty(_WFFill, "Fill Opacity");
                                if (isTransparent)
                                    me.ShaderProperty(_WireframeTransparency, "Use Alpha");
                            });
                            MGUI.ToggleGroupEnd();
                        });
                    }
                    MGUI.Space6();
                }
            }

            // -----------------
            // Rendering
            // -----------------
            if (Foldouts.DoFoldout(foldouts, mat, "Render Settings", Foldouts.Style.Standard)) {
                MGUI.PropertyGroupParent(()=>{
                    MGUI.BoldLabel("General");
                    MGUI.PropertyGroup(() => {
                        me.RenderQueueField();
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
                });
            }

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

        private void IridescenceGradientToTexture(ref Texture2D tex){
            for (int x = 0; x < tex.width; x++){
                Color col = iridescenceGradientObj.gradient.Evaluate((float)x / tex.width);
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
            bool usingDetailMetallic = mat.GetTexture("_DetailMetallic");

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
            
            if (usingDetailMetallic)
                mat.SetInt("_UsingDetailMetallic", 1);
            else
                mat.SetInt("_UsingDetailMetallic", 0);

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
    }
}