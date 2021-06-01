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

    public static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
    Toggles toggles = new Toggles(
		new string[] {
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
			"UV DISTORTION",
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
			"Wireframe 1"
		}
	);

	private GradientObject gradientObj;
	private SerializedProperty colorGradient;
	private SerializedObject serializedGradient;
	private EditorWindow gradientWindow;
	private Texture2D rampTex;

	float buttonSize = 24.0f;
	float xPos = 53.0f;
	int blendingLabelPos = 111;

	string header = "Header_Pro";
	string watermark = "Watermark_Pro";
	string patIcon = "Patreon_Icon";
	string keyTex = "KeyIcon_Pro";
	string versionLabel = "v1.18.1";
	// β
	
	GUIContent maskLabel = new GUIContent("Mask");
    GUIContent baseColorLabel = new GUIContent("Base Color");
	GUIContent baseColor2Label = new GUIContent("Base Color 2");
	GUIContent mirrorTexLabel = new GUIContent("Base Color (Mirror)");
    GUIContent emissTexLabel = new GUIContent("Emission Map");
    GUIContent normalTexLabel = new GUIContent("Normal");
    GUIContent metallicTexLabel = new GUIContent("Metallic");
    GUIContent roughnessTexLabel = new GUIContent("Roughness");
    GUIContent occlusionTexLabel = new GUIContent("Occlusion");
    GUIContent heightTexLabel = new GUIContent("Height");
    GUIContent reflCubeLabel = new GUIContent("Cubemap");
    GUIContent shadowRampLabel = new GUIContent("Ramp");
    GUIContent specularTexLabel = new GUIContent("Specular Map");
    GUIContent primaryMapsLabel = new GUIContent("Primary Maps");
    GUIContent detailMapsLabel = new GUIContent("Detail Maps");
	GUIContent dissolveTexLabel = new GUIContent("Dissolve Map");
	GUIContent dissolveRimTexLabel = new GUIContent("Rim Color");
	GUIContent colorLabel = new GUIContent("Color");
	GUIContent packedTexLabel = new GUIContent("Packed Texture");
	GUIContent cubemapLabel = new GUIContent("Cubemap");
	GUIContent translucTexLabel = new GUIContent("Thickness Map");
	GUIContent tintLabel = new GUIContent("Tint");
	GUIContent filteringLabel = new GUIContent("PBR Filtering");
	GUIContent smoothTexLabel = new GUIContent("Smoothness");
	GUIContent alphaMaskLabel = new GUIContent("Alpha Mask");
	GUIContent curveTexLabel = new GUIContent("Curvature");
	GUIContent detailLabel = new GUIContent("Detail Maps");
	GUIContent subsurfLabel = new GUIContent("Subsurface");
	GUIContent diffuseLabel = new GUIContent("Diffuse");
	GUIContent eRimLabel = new GUIContent("Enviro. Rim");
	GUIContent basicRimLabel = new GUIContent("Basic Rim");
	GUIContent shadowLabel = new GUIContent("Shadows");
	GUIContent matcapLabel = new GUIContent("Matcap");
	GUIContent specLabel = new GUIContent("Specular");
	GUIContent reflLabel = new GUIContent("Reflections");
	GUIContent matcapBlendLabel = new GUIContent("Matcap Blend");
	GUIContent anisoBlendLabel = new GUIContent("Specular Blend");
	GUIContent emissLabel = new GUIContent("Emission");
	GUIContent emissPulseLabel = new GUIContent("Emission Pulse");
	GUIContent filterLabel = new GUIContent("Filtering");
	GUIContent olThickLabel = new GUIContent("Outline Thickness");
	GUIContent refractLabel = new GUIContent("Refraction");
	GUIContent nearClipLabel = new GUIContent("Near Clip");

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
	MaterialProperty _VLightCont = null;
	MaterialProperty _Subsurface = null;
	MaterialProperty _SColor = null;
	MaterialProperty _SubsurfaceMask = null;
	MaterialProperty _SStr = null;
	MaterialProperty _UnlitMatcap1 = null;
	MaterialProperty _SPen = null;
	MaterialProperty _SSharp = null;
	MaterialProperty _TranslucencyMap = null;
	MaterialProperty _SubsurfaceTex = null;
	MaterialProperty _SAtten = null;
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
	// MaterialProperty _UVAniso = null;
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
	MaterialProperty _EmissIntensity = null;

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
	// MaterialProperty _AudioLinkWireframeFalloff = null;
	// MaterialProperty _AudioLinkTriOffsetFalloff = null;
	MaterialProperty _NaNLmao = null;

    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

	bool displayKeywords = false;
	List<string> keywordsList = new List<string>();

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

		// Using this to mercilessly murder verts when necessary (must be 0)
		_NaNLmao.floatValue = 0.0f;

		// Generate path sensitive folders if necessary
		if (!AssetDatabase.IsValidFolder(MGUI.presetPath+"/Textures/Ramps"))
			AssetDatabase.CreateFolder(MGUI.presetPath+"/Textures", "Ramps");

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
				watermark = "Watermark";
				keyTex = "KeyIcon";
			}
		}
		else {
			header = "Header_Pro";
			if (!EditorGUIUtility.isProSkin){
				header = "Header";
				watermark = "Watermark";
				keyTex = "KeyIcon";
			}
		}

		// Add mat to foldout dictionary if it isn't in there yet
		if (!foldouts.ContainsKey(mat))
			foldouts.Add(mat, toggles);
			
		foreach (var obj in _BlendMode.targets)
			ApplyMaterialSettings((Material)obj);
		
        Texture2D headerTex = (Texture2D)Resources.Load(header, typeof(Texture2D));
		Texture2D watermarkTex = (Texture2D)Resources.Load(watermark, typeof(Texture2D));
		Texture2D patIconTex = (Texture2D)Resources.Load(patIcon, typeof(Texture2D));
		Texture2D resetIcon = (Texture2D)Resources.Load("ResetIcon", typeof(Texture2D));
		Texture2D resetFullIcon = (Texture2D)Resources.Load("ResetFullIcon", typeof(Texture2D));
		Texture2D clearTexIcon = (Texture2D)Resources.Load("ClearTexIcon", typeof(Texture2D));
		Texture2D collapseIcon = (Texture2D)Resources.Load("CollapseIcon", typeof(Texture2D));
		Texture2D randomizeIcon = (Texture2D)Resources.Load("RandomizeIcon", typeof(Texture2D));
		Texture2D randomizeColIcon = (Texture2D)Resources.Load("RandomizeColIcon", typeof(Texture2D));
		Texture2D standardIcon = (Texture2D)Resources.Load("StandardIcon", typeof(Texture2D));
		Texture2D toonIcon = (Texture2D)Resources.Load("ToonIcon", typeof(Texture2D));
		Texture2D copyTo1Icon = (Texture2D)Resources.Load("CopyTo1Icon", typeof(Texture2D));
		Texture2D copyTo2Icon = (Texture2D)Resources.Load("CopyTo2Icon", typeof(Texture2D));
		Texture2D keyIcon = (Texture2D)Resources.Load(keyTex, typeof(Texture2D));

		GUIContent randomLabel = new GUIContent(randomizeIcon, "Randomize certain property values (excluding colorpickers).");
		GUIContent randomColLabel = new GUIContent(randomizeColIcon, "Randomize colorpicker values.");
		GUIContent clearTexLabel = new GUIContent(clearTexIcon, "Clear any unused textures from the material so they don't get packaged with your avatar on upload (can help considerably with file size).");
		GUIContent toonLabel = new GUIContent(toonIcon, "Apply preset property values for basic stylized toon shading.");
		GUIContent standardLabel = new GUIContent(standardIcon, "Apply preset property values that visually match Standard shader.");
		GUIContent collapseLabel = new GUIContent(collapseIcon, "Collapse all foldout tabs.");
		GUIContent copyTo1Label = new GUIContent(copyTo1Icon, "Copy settings to primary layer.");
		GUIContent copyTo2Label = new GUIContent(copyTo2Icon, "Copy settings to secondary layer.");
		GUIContent keyLabel = new GUIContent(keyIcon, "Toggle material keywords list.");

        MGUI.CenteredTexture(headerTex, 0, 0);
		GUILayout.Space(-34);
		ListKeywords(mat, keyLabel, buttonSize);

		// -----------------
		// Base Settings
		// -----------------
		bool baseTab = Foldouts.DoFoldout(foldouts, mat, me, 3, "BASE");
		if (MGUI.TabButton(collapseLabel, 26f)){
			for (int i = 1; i <= foldouts[mat].GetToggles().Length-1; i++)
				foldouts[mat].SetState(i, false);
		}
		MGUI.Space8();
		if (MGUI.TabButton(clearTexLabel, 54f)){
			ClearUnusedTextures(renderMode, workflow, cubeMode);
		}
		MGUI.Space8();
		if (MGUI.TabButton(standardLabel, 82f)){
			DoStandardLighting(mat);
		}
		MGUI.Space8();
		if (baseTab){
			MGUI.Space6();
			me.RenderQueueField();
			me.ShaderProperty(_RenderMode, "Shading");
			EditorGUI.BeginChangeCheck();
			me.ShaderProperty(_BlendMode, "Blending Mode");
			me.ShaderProperty(_MirrorBehavior, "Mirror Behavior");
			me.ShaderProperty(_CubeMode, "Main Texture Type");
			if (EditorGUI.EndChangeCheck())
				SetBlendMode(mat);
			me.ShaderProperty(_CullingMode, "Culling");
			if (isCutout || isTransparent)
				me.ShaderProperty(_UseAlphaMask, "Alpha Source");
			if (blendMode == 1)
				me.ShaderProperty(_Cutoff, "Cutout");
			GUILayout.Space(8);
			switch((int)cubeMode){

				// Tex Only
				case 0: 
					me.TexturePropertySingleLine(baseColorLabel, _MainTex, _Color, renderMode == 1 ? _ColorPreservation : null);
					if (renderMode == 1) MGUI.TexPropLabel("Tint Clamp", 123);
					if (_MirrorBehavior.floatValue == 2)
						me.TexturePropertySingleLine(mirrorTexLabel, _MirrorTex);
					MGUI.TextureSOScroll(me, _MainTex, _MainTexScroll);
					break;
				
				// Cubemap Only
				case 1: 
					me.TexturePropertySingleLine(reflCubeLabel, _MainTexCube0, _CubeColor0, renderMode == 1 ? _ColorPreservation : null);
					if (renderMode == 1) 
						MGUI.TexPropLabel("Tint Clamp", 123);
					MGUI.Space4();
					MGUI.Vector3Field(_CubeRotate0, "Rotation");
					me.ShaderProperty(_AutoRotate0, "Auto Rotate");
					break;
				
				// Tex and Cubemap
				case 2: 
					me.TexturePropertySingleLine(baseColorLabel, _MainTex, _Color, renderMode == 1 ? _ColorPreservation : null);
					if (renderMode == 1) 
						MGUI.TexPropLabel("Tint Clamp", 123);
					if (_MirrorBehavior.floatValue == 2)
						me.TexturePropertySingleLine(mirrorTexLabel, _MirrorTex);
					MGUI.TextureSOScroll(me, _MainTex, _MainTexScroll);

					GUILayout.Space(12);
					me.TexturePropertySingleLine(new GUIContent("Blend"), _CubeBlendMask, _CubeBlendMask.textureValue ? null : _CubeBlend);
					GUILayout.Space(12);

					me.TexturePropertySingleLine(reflCubeLabel, _MainTexCube0, _CubeColor0, _CubeBlendMode);
					MGUI.TexPropLabel("Blending", blendingLabelPos);
					MGUI.Space4();
					MGUI.Vector3Field(_CubeRotate0, "Rotation");
					me.ShaderProperty(_AutoRotate0, "Auto Rotate");
					break;
				default: break;
			}
			if (_UseAlphaMask.floatValue == 1 && (isCutout || isTransparent)){
				MGUI.Space8();
				me.TexturePropertySingleLine(alphaMaskLabel, _AlphaMask, _AlphaMaskChannel);
				MGUI.TexPropLabel("Channel", 109);
				MGUI.TextureSO(me, _AlphaMask);
			}
			MGUI.Space8();
		}

		// -----------------
		// Textures
		// -----------------
		if (renderMode == 1){
			bool texturesTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "TEXTURES");
			if (MGUI.TabButton(resetIcon, 26f)){
				DoTextureMapReset();
			}
			MGUI.Space8();
			if (texturesTab){
				MGUI.Space6();

				// Primary Maps
				MGUI.BoldLabel("Primary Maps");
				me.ShaderProperty(_PBRWorkflow, "Workflow");
				MGUI.Space2();
				MGUI.PropertyGroup(() => {
					switch ((int)workflow){

						// Metallic
						case 0:
							me.TexturePropertySingleLine(metallicTexLabel, _MetallicGlossMap, _MetallicGlossMap.textureValue ? null : _Metallic);
							me.TexturePropertySingleLine(roughnessTexLabel, _SpecGlossMap, _SpecGlossMap.textureValue ? null : _Glossiness);
							MGUI.sRGBWarning(_SpecGlossMap);
							break;

						// Specular (RGB)
						case 1: 
							me.TexturePropertySingleLine(specularTexLabel, _SpecGlossMap, _SpecCol);
							me.TexturePropertySingleLine(smoothTexLabel, _SmoothnessMap, _GlossMapScale);
							break;

						// Specular (RGBA)
						case 2: 
							me.TexturePropertySingleLine(specularTexLabel, _SpecGlossMap, _SpecCol);
							me.ShaderProperty(_GlossMapScale, "Smoothness", 2);
							MGUI.Space2();
							break;

						// Packed (Modular)
						case 3:
							me.TexturePropertySingleLine(packedTexLabel, _PackedMap);
							MGUI.sRGBWarning(_PackedMap);
							me.ShaderProperty(_MetallicChannel, "Metallic");
							me.ShaderProperty(_RoughnessChannel, "Roughness");
							me.ShaderProperty(_OcclusionChannel, "Occlusion");
							me.ShaderProperty(_HeightChannel, "Height");
							MGUI.Space8();
							me.ShaderProperty(_OcclusionStrength, "Occlusion Strength");
							MGUI.ToggleSlider(me, "Height Strength", _EnablePackedHeight, _Parallax);
							MGUI.Space8();
							break;
						
						// Packed (Baked)
						case 4:
							me.TexturePropertySingleLine(packedTexLabel, _PackedMap);
							MGUI.sRGBWarning(_PackedMap);
							GUILayout.Label("Red:	Metallic\nGreen:	Roughness\nBlue:	Occlusion\nAlpha:	Height");
							me.ShaderProperty(_OcclusionStrength, "Occlusion Strength");
							MGUI.ToggleSlider(me, "Height Strength", _EnablePackedHeight, _Parallax);
							MGUI.Space8();
							break;	
						default: break;
					}
					if (workflow < 3)
						me.TexturePropertySingleLine(occlusionTexLabel, _OcclusionMap, _OcclusionMap.textureValue ? _OcclusionStrength : null);
					me.TexturePropertySingleLine(normalTexLabel, _BumpMap, _BumpMap.textureValue ? _BumpScale : null);
					if (workflow < 3)
						me.TexturePropertySingleLine(heightTexLabel, _ParallaxMap, _ParallaxMap.textureValue ? _Parallax : null);
				});
				MGUI.Space4();

				MGUI.BoldLabel("Detail Maps");
				me.ShaderProperty(_UVSec, "UV Set");
				MGUI.Space2();
				MGUI.PropertyGroup(() => {
					bool usingDetRough = _DetailRoughnessMap.textureValue;
					bool usingDetOcc = _DetailOcclusionMap.textureValue;
					bool usingDetAlbedo = _DetailAlbedoMap.textureValue;
					bool usingDetNormal = _DetailNormalMap.textureValue;
					me.TexturePropertySingleLine(baseColorLabel, _DetailAlbedoMap, usingDetAlbedo ? _DetailAlbedoStrength : null, usingDetAlbedo ? _DetailAlbedoBlending : null);
					me.TexturePropertySingleLine(normalTexLabel, _DetailNormalMap, usingDetNormal ? _DetailNormalMapScale : null);
					if (workflow == 0 || workflow >= 3)
						me.TexturePropertySingleLine(roughnessTexLabel, _DetailRoughnessMap, usingDetRough ? _DetailRoughStrength : null, usingDetRough ? _DetailRoughBlending : null);
					me.TexturePropertySingleLine(occlusionTexLabel, _DetailOcclusionMap, usingDetOcc ? _DetailOcclusionStrength : null, usingDetOcc ? _DetailOcclusionBlending : null);
					if (usingDetAlbedo || usingDetNormal || usingDetRough || usingDetOcc) {
						MGUI.TextureSOScroll(me, _DetailAlbedoMap, _DetailScroll);
					}
				});

				// Masking
				bool maskingTab = Foldouts.DoMediumFoldout(foldouts, mat, me, 1, "Masks");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoMaskingReset();
				GUILayout.Space(5);
				if (maskingTab){
					MGUI.Space4();
					me.ShaderProperty(_MaskingMode, "Mode");
					if (_MaskingMode.floatValue == 1){
						MGUI.Space4();
						me.ShaderProperty(_EnableMaskTransform, "Enable Transforms");
						MGUI.PropertyGroup(() => {
							if (_EnableMaskTransform.floatValue == 0){
								me.TexturePropertySingleLine(reflLabel, _ReflectionMask);
								me.TexturePropertySingleLine(specLabel, _SpecularMask);
								me.TexturePropertySingleLine(anisoBlendLabel, _InterpMask); 
								me.TexturePropertySingleLine(matcapLabel, _MatcapMask);
								me.TexturePropertySingleLine(matcapBlendLabel, _MatcapBlendMask);
								me.TexturePropertySingleLine(shadowLabel, _ShadowMask);
								me.TexturePropertySingleLine(basicRimLabel, _RimMask);
								me.TexturePropertySingleLine(eRimLabel, _ERimMask);
								me.TexturePropertySingleLine(diffuseLabel, _DiffuseMask);
								me.TexturePropertySingleLine(subsurfLabel, _SubsurfaceMask);
								me.TexturePropertySingleLine(detailLabel, _DetailMask);
								me.TexturePropertySingleLine(emissLabel, _EmissMask);
								me.TexturePropertySingleLine(emissPulseLabel, _PulseMask);
								me.TexturePropertySingleLine(filterLabel, _FilterMask);	
								me.TexturePropertySingleLine(refractLabel, _RefractionMask);
								if (isOutline)
									me.TexturePropertySingleLine(olThickLabel, _OutlineMask);
							}
							else {
								bool reflMask = Foldouts.DoMaskFoldout(foldouts, mat, me, reflLabel, "Reflection Mask");
								MGUI.MaskProperty(mat, me, reflMask, _ReflectionMask, _ReflectionMaskScroll);
								bool specMask = Foldouts.DoMaskFoldout(foldouts, mat, me, specLabel, "Specular Mask");
								MGUI.MaskProperty(mat, me, specMask, _SpecularMask, _SpecularMaskScroll);
								bool interpMask = Foldouts.DoMaskFoldout(foldouts, mat, me, anisoBlendLabel, "Specular Blend Mask");
								MGUI.MaskProperty(mat, me, interpMask, _InterpMask, _InterpMaskScroll);
								bool matcapMask = Foldouts.DoMaskFoldout(foldouts, mat, me, matcapLabel, "Matcap Mask");
								MGUI.MaskProperty(mat, me, matcapMask, _MatcapMask, _MatcapMaskScroll);
								bool matcapBlendMask = Foldouts.DoMaskFoldout(foldouts, mat, me, matcapBlendLabel, "Matcap Blend Mask");
								MGUI.MaskProperty(mat, me, matcapBlendMask, _MatcapBlendMask, _MatcapBlendMaskScroll);
								bool shadowMask = Foldouts.DoMaskFoldout(foldouts, mat, me, shadowLabel, "Shadow Mask");
								MGUI.MaskProperty(mat, me, shadowMask, _ShadowMask, _ShadowMaskScroll);
								bool rimMask = Foldouts.DoMaskFoldout(foldouts, mat, me, basicRimLabel, "Rim Mask");
								MGUI.MaskProperty(mat, me, rimMask, _RimMask, _RimMaskScroll);
								bool eRimMask = Foldouts.DoMaskFoldout(foldouts, mat, me, eRimLabel, "Env. Rim Mask");
								MGUI.MaskProperty(mat, me, eRimMask, _ERimMask, _ERimMaskScroll);
								bool diffuseMask = Foldouts.DoMaskFoldout(foldouts, mat, me, diffuseLabel, "Diffuse Mask");
								MGUI.MaskProperty(mat, me, diffuseMask, _DiffuseMask, _DiffuseMaskScroll);
								bool subsurfMask = Foldouts.DoMaskFoldout(foldouts, mat, me, subsurfLabel, "Subsurface Mask");
								MGUI.MaskProperty(mat, me, subsurfMask, _SubsurfaceMask, _SubsurfaceMaskScroll);
								bool detailMask = Foldouts.DoMaskFoldout(foldouts, mat, me, detailLabel, "Detail Mask");
								MGUI.MaskProperty(mat, me, detailMask, _DetailMask, _DetailMaskScroll);
								bool emissMask = Foldouts.DoMaskFoldout(foldouts, mat, me, emissLabel, "Emission Mask");
								MGUI.MaskProperty(mat, me, emissMask, _EmissMask, _EmissMaskScroll);
								bool emissPulseMask = Foldouts.DoMaskFoldout(foldouts, mat, me, emissPulseLabel, "Emission Pulse Mask");
								MGUI.MaskProperty(mat, me, emissPulseMask, _PulseMask, _EmissPulseMaskScroll);
								bool filterMask = Foldouts.DoMaskFoldout(foldouts, mat, me, filterLabel, "Filter Mask");
								MGUI.MaskProperty(mat, me, filterMask, _FilterMask, _FilterMaskScroll);
								bool refractMask = Foldouts.DoMaskFoldout(foldouts, mat, me, refractLabel, "Refraction Mask");
								MGUI.MaskProperty(mat, me, refractMask, _RefractionMask, _RefractionMaskScroll);
								if (isOutline){
									bool olThickMask = Foldouts.DoMaskFoldout(foldouts, mat, me, olThickLabel, "Outline Thickness Mask");
									MGUI.MaskProperty(mat, me, olThickMask, _OutlineMask, _OutlineMaskScroll);
								}
							}
						});
						MGUI.Space2();
					}
					else if (_MaskingMode.floatValue == 2){
						MGUI.Space2();
						MGUI.PropertyGroup(() => {
							me.TexturePropertySingleLine(new GUIContent("Mask 1"), _PackedMask0);
							GUILayout.Label("Red:	Reflections\nGreen:	Specular\nBlue:	Matcap\nAlpha:	Refraction");
							MGUI.Space8();
							me.TexturePropertySingleLine(new GUIContent("Mask 2"), _PackedMask1);
							GUILayout.Label("Red:	Shadows\nGreen:	Diffuse Shading\nBlue:	Subsurface\nAlpha:	Detail Maps");
							MGUI.Space8();
							me.TexturePropertySingleLine(new GUIContent("Mask 3"), _PackedMask2);
							GUILayout.Label("Red:	Basic Rim\nGreen:	Environment Rim\nBlue:	Matcap Blend\nAlpha:	Specular Blend");
							MGUI.Space8();
							me.TexturePropertySingleLine(new GUIContent("Mask 4"), _PackedMask3);
							GUILayout.Label("Red:	Emission\nGreen:	Emission Pulse\nBlue:	Filtering\nAlpha:	Outline Thickness");
						});
						MGUI.Space2();
					}
					else MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// GUILayout.Label(filteringLabel, EditorStyles.boldLabel);

				// Roughness Filtering
				if (workflow == 0 || workflow >= 3){
					if (workflow == 0)
						MGUI.ToggleGroup(!_SpecGlossMap.textureValue);
					else
						MGUI.ToggleGroup(!_PackedMap.textureValue);
					bool roughFilterTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _RoughnessFiltering, 1, "Roughness Filter");
					if (workflow == 0)  
						roughFilterTab = roughFilterTab && _SpecGlossMap.textureValue;
					else
						roughFilterTab = roughFilterTab && _PackedMap.textureValue;
					
					if (MGUI.MedTabButton(resetIcon, 23f))
						DoRoughFilterReset();
					GUILayout.Space(5);
					if (roughFilterTab){
						MGUI.Space2();
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_RoughnessFiltering.floatValue == 0);
							me.ShaderProperty(_PreviewRough, "Preview");
							me.ShaderProperty(_RoughLightness, "Lightness");
							me.ShaderProperty(_RoughIntensity, "Intensity");
							me.ShaderProperty(_RoughContrast, "Contrast");
							MGUI.ToggleGroupEnd();
						});
						MGUI.Space2();
					}
					else MGUI.SpaceN2();
					MGUI.ToggleGroupEnd();
				}

				// Smoothness Filtering (for specular)
				else {
					if (workflow == 1)
						MGUI.ToggleGroup(!_SmoothnessMap.textureValue);
					else
						MGUI.ToggleGroup(!_SpecGlossMap.textureValue);
					bool smoothFilterTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _SmoothnessFiltering, 1, "Smoothness Filter");
					if (MGUI.MedTabButton(resetIcon, 23f))
						DoSmoothFilterReset();
					GUILayout.Space(5);
					if (workflow == 1)
						smoothFilterTab = smoothFilterTab && _SmoothnessMap.textureValue;
					else
						smoothFilterTab = smoothFilterTab && _SpecGlossMap.textureValue;
					if (smoothFilterTab){
						MGUI.Space2();
						MGUI.PropertyGroup(() => {
							MGUI.ToggleGroup(_SmoothnessFiltering.floatValue == 0);
							me.ShaderProperty(_PreviewSmooth, "Preview");
							me.ShaderProperty(_SmoothLightness, "Lightness");
							me.ShaderProperty(_SmoothIntensity, "Intensity");
							me.ShaderProperty(_SmoothContrast, "Contrast");
							MGUI.ToggleGroupEnd();
						});
						MGUI.Space2();
					}
					else MGUI.SpaceN2();
					MGUI.ToggleGroupEnd();
				}

				// AO Filtering
				if (workflow < 3)
					MGUI.ToggleGroup(!_OcclusionMap.textureValue && workflow < 3);
				else
					MGUI.ToggleGroup(!_PackedMap.textureValue);
				bool aoFilterTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _AOFiltering, 1, "Occlusion Filter");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoAOFilterReset();
				GUILayout.Space(5);
				if (workflow < 3)
					aoFilterTab = aoFilterTab && _OcclusionMap.textureValue;
				else
					aoFilterTab = aoFilterTab && _PackedMap.textureValue;
				if (aoFilterTab){
					MGUI.Space2();
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_AOFiltering.floatValue == 0);
						me.ShaderProperty(_PreviewAO, "Preview");
						MGUI.Space2();
						me.TexturePropertySingleLine(tintLabel, _AOTintTex, _AOTint);
						me.ShaderProperty(_AOLightness, "Lightness");
						me.ShaderProperty(_AOIntensity, "Intensity");
						me.ShaderProperty(_AOContrast, "Contrast");
						MGUI.ToggleGroupEnd();
					});
					MGUI.Space2();
				}
				else MGUI.SpaceN2();
				MGUI.ToggleGroupEnd();

				// Height Filtering
				bool heightConditions = workflow < 3 ? _ParallaxMap.textureValue : (_PackedMap.textureValue && _EnablePackedHeight.floatValue == 1);
				MGUI.ToggleGroup(!heightConditions);
				bool heightFilterTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _HeightFiltering, 1, "Height Filter") && heightConditions;
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoHeightFilterReset();
				GUILayout.Space(5);
				if (heightFilterTab){
					MGUI.Space2();
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_HeightFiltering.floatValue == 0);
						me.ShaderProperty(_PreviewHeight, "Preview");
						me.ShaderProperty(_HeightLightness, "Lightness");
						me.ShaderProperty(_HeightIntensity, "Intensity");
						me.ShaderProperty(_HeightContrast, "Contrast");
						MGUI.ToggleGroupEnd();
					});
					MGUI.Space2();
				}
				else MGUI.SpaceN2();
				MGUI.ToggleGroupEnd();
				
				// Base Color Dissolve
				bool bcDissTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _BCDissolveToggle, 1, "Base Color Dissolve 0");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoBCDissolveReset();
				GUILayout.Space(5);
				if (bcDissTab){
					MGUI.Space2();
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_BCDissolveToggle.floatValue ==0);
						me.TexturePropertySingleLine(baseColor2Label, _MainTex2, _BCColor);
						MGUI.TextureSO(me, _MainTex2, _MainTex2.textureValue);
						me.TexturePropertySingleLine(dissolveTexLabel, _BCNoiseTex, _BCDissolveStr);
						MGUI.TextureSO(me, _BCNoiseTex, _BCNoiseTex.textureValue);
						me.ShaderProperty(_BCRimCol, "Rim Color");
						me.ShaderProperty(_BCRimWidth, "Rim Width");
						MGUI.ToggleGroupEnd();
					});
					MGUI.Space2();
				}
				else MGUI.SpaceN2();
				MGUI.Space8();
			}

			// -----------------
			// Shading
			// -----------------
			bool queueError = mat.renderQueue < 2501;
			bool reflError = _Reflections.floatValue > 0 && _SSR.floatValue == 1;
			bool refracError = _Refraction.floatValue > 0 && queueError;
			bool[] shadingErrors = {reflError && queueError, refracError};
			bool shadingTab = Foldouts.DoFoldoutError(foldouts, mat, me, shadingErrors, 1, "SHADING");
			if (MGUI.TabButton(resetIcon, 26f)){
				DoShadingReset(mat);
			}
			MGUI.Space8();
			if (shadingTab){
				MGUI.Space8();

				// Lighting
				bool lightingTab = Foldouts.DoMediumFoldout(foldouts, mat, me, 1, "Lighting");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoLightingReset();
				GUILayout.Space(5);
				if (lightingTab){
					MGUI.Space2();
					MGUI.BoldLabel("General");
					MGUI.SpaceN2();
					MGUI.PropertyGroup( () => {
						MGUI.ToggleVector3("Static Direction", _StaticLightDirToggle, _StaticLightDir);
						MGUI.ToggleGroup(!_OcclusionMap.textureValue && workflow != 3);
						me.ShaderProperty(_DirectAO, "Direct Occlusion");
						me.ShaderProperty(_IndirectAO, "Indirect Occlusion");
						MGUI.ToggleGroupEnd();
						if (!_OcclusionMap.textureValue && workflow < 3){
							GUILayout.Space(-28);
							GUIStyle f = new GUIStyle(EditorStyles.boldLabel);
							f.fontSize = 10;
							Rect r = EditorGUILayout.GetControlRect();
							r.x += EditorGUIUtility.labelWidth+21f;
							GUI.Label(r, "Requires Occlusion Map", f);
							MGUI.Space10();
						}
					});
					MGUI.BoldLabel("Diffuse Shading");
					MGUI.SpaceN2();
					MGUI.PropertyGroup( () => {
						me.ShaderProperty(_DisneyDiffuse, "Disney Term");
						me.ShaderProperty(_SHStr, "Spherical Harmonics");
						MGUI.ToggleGroup(_SHStr.floatValue == 0);
						me.ShaderProperty(_NonlinearSHToggle, "Nonlinear SH");
						MGUI.ToggleGroupEnd();
					});
					MGUI.BoldLabel("Realtime Light");
					MGUI.SpaceN2();
					MGUI.PropertyGroup( () => {
						me.ShaderProperty(_RTDirectCont, "Direct Intensity");
						me.ShaderProperty(_RTIndirectCont, "Indirect Intensity");
						me.ShaderProperty(_VLightCont, "Vertex Intensity");
						me.ShaderProperty(_ClampAdditive, "Clamp Additive");
					});
					MGUI.BoldLabel("Baked Light");
					MGUI.SpaceN2();
					MGUI.PropertyGroup( () => {
						me.ShaderProperty(_DirectCont, "Direct Intensity");
						me.ShaderProperty(_IndirectCont, "Indirect Intensity");
					});
					MGUI.Space2();
				}
				else MGUI.SpaceN2();

				// Shadows
				bool shadowTab = Foldouts.DoMediumFoldout(foldouts, mat, me, 1, "Shadows");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoShadowReset();
				GUILayout.Space(5);

				if (shadowTab){
					MGUI.Space4();
					me.ShaderProperty(_ShadowMode, "Mode");
					if (_ShadowMode.floatValue > 0){
						me.ShaderProperty(_ShadowConditions, "Conditions");
						if (_ShadowMode.floatValue == 1){
							MGUI.Space2();
							MGUI.PropertyGroup( () => {
								me.ShaderProperty(_ShadowTint, "Tint");
								MGUI.Space4();
								me.ShaderProperty(_RampWidth0, "Ramp 1");
								me.ShaderProperty(_RampWidth1, "Ramp 2");
								me.ShaderProperty(_RampWeight, "Ramp Blend");
								me.ShaderProperty(_RampPos, "Offset");
							});
						}
						else if (_ShadowMode.floatValue == 2){
							MGUI.Space2();
							MGUI.PropertyGroup(() => {
								me.TexturePropertySingleLine(shadowRampLabel, _ShadowRamp);
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
									string rampPath = MGUI.presetPath+"/Textures/Ramps/Ramp_"+rampID+".png";
									MGUI.WriteBytes(encodedTex, rampPath);
									AssetDatabase.ImportAsset(rampPath);
									_ShadowRamp.textureValue = (Texture)EditorGUIUtility.Load(rampPath);
								}
							});
						}
						MGUI.PropertyGroup( () => {
							me.ShaderProperty(_ShadowStr, "Strength");
							me.ShaderProperty(_DitheredShadows, "Dithering");
							me.ShaderProperty(_RTSelfShadow, "Directional Light Shadows");
							MGUI.ToggleGroup(_RTSelfShadow.floatValue == 0);
							me.ShaderProperty(_AttenSmoothing, "Smooth Attenuation");
							MGUI.ToggleGroupEnd();
						});
						MGUI.Space2();
					}
					else MGUI.Space8();
				}
				else MGUI.SpaceN2();

				// Reflections
				bool reflTab = Foldouts.DoMediumFoldoutError(foldouts, mat, me, reflError && queueError, 1, "Reflections");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoReflReset();
				GUILayout.Space(5);
				if (reflTab){
					MGUI.Space4();
					me.ShaderProperty(_Reflections, "Mode");
					if (_Reflections.floatValue > 0){
						MGUI.Space2();
						MGUI.PropertyGroup(() => {
							if (_Reflections.floatValue == 1)
								reflCubeLabel.text = "Fallback Cubemap";
							else
								reflCubeLabel.text = "Cubemap";
							me.TexturePropertySingleLine(reflCubeLabel, _ReflCube);
							me.ShaderProperty(_ReflCol, "Tint");
							me.ShaderProperty(_ReflectionStr, "Strength");
							MGUI.ToggleSlider(me, "Manual Roughness", _ReflUseRough, _ReflRough);
							MGUI.ToggleIntSlider(me, "Stepping", _ReflStepping, _ReflSteps);
							me.ShaderProperty(_LightingBasedIOR, "Lighting-based IOR");
							me.ShaderProperty(_SSR, "SSR");
							if (reflError){
								MGUI.PropertyGroupLayer(() => {
									if (queueError)
										MGUI.DisplayError("SSR requires a render queue of 2501 or above to function correctly.");
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
						MGUI.Space2();
					}
					else MGUI.Space8();
				}
				else MGUI.SpaceN2();

				// Specular
				bool specTab = Foldouts.DoMediumFoldout(foldouts, mat, me, 1, "Specular");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoSpecReset();
				GUILayout.Space(5);
				if (specTab){
					MGUI.Space4();
					me.ShaderProperty(_Specular, "Mode");
					if (_Specular.floatValue > 0){
						MGUI.Space2();
						if (_Specular.floatValue == 3){
							MGUI.Space6();
							MGUI.DisplayInfo("Note: Use Specular Blend mask in the masks tab to interpolate between GGX and Anisotropic");
							MGUI.Space6();
						}
						MGUI.PropertyGroup(() => {
							me.ShaderProperty(_SpecCol, "Tint");
							if (_Specular.floatValue == 1){
								me.ShaderProperty(_SpecStr, "Strength");
								MGUI.ToggleSlider(me, "Manual Roughness", _SpecUseRough, _SpecRough);
								MGUI.ToggleSlider(me, "Manual Bias", _SpecBiasOverrideToggle, _SpecBiasOverride);
								MGUI.ToggleIntSlider(me, "Stepping", _SharpSpecular, _SharpSpecStr);
								me.ShaderProperty(_RealtimeSpec, "Realtime Light Only");
							}
							else if (_Specular.floatValue == 2){
								me.ShaderProperty(_AnisoStr, "Strength");
								me.ShaderProperty(_RealtimeSpec, "Realtime Light Only");
								MGUI.ToggleIntSlider(me, "Stepping", _SharpSpecular, _AnisoSteps);
							}
							else {
								me.ShaderProperty(_SpecStr, "GGX Strength");
								me.ShaderProperty(_AnisoStr, "Aniso Strength");
								me.ShaderProperty(_RealtimeSpec, "Realtime Light Only");
								me.ShaderProperty(_ManualSpecBright, "Ignore Environment");
								me.ShaderProperty(_SharpSpecular, "Stepping");
								if (_SharpSpecular.floatValue == 1){
									me.ShaderProperty(_SharpSpecStr, "GGX Steps");
									me.ShaderProperty(_AnisoSteps, "Aniso Steps");
								}
								MGUI.ToggleGroupEnd();
							}
							if (_Specular.floatValue != 3){
								me.ShaderProperty(_ManualSpecBright, "Ignore Environment");
							}
							if (_Specular.floatValue == 2 || _Specular.floatValue == 3){
								MGUI.Space6();
								me.ShaderProperty(_AnisoAngleY, "Layer 1 Thickness");
								me.ShaderProperty(_AnisoLayerY, "Layer 2 Thickness");
								me.ShaderProperty(_AnisoLayerStr, "Layer Blend");
								me.ShaderProperty(_AnisoLerp, "Lerp Blend");
								MGUI.Space8();
								me.ShaderProperty(_RippleStrength, "Hair Strength");
								me.ShaderProperty(_RippleFrequency, "Hair Density");
								me.ShaderProperty(_RippleAmplitude, "Hair Intensity");
								me.ShaderProperty(_RippleContinuity, "Hair Continuity");
							}
						});
						MGUI.Space2();
					}
					else MGUI.Space8();
				}
				else MGUI.SpaceN2();

				// Matcap
				bool matcapTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _MatcapToggle, 1, "Matcap");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoMatcapReset();
				GUILayout.Space(5);
				if (matcapTab){
					MGUI.Space2();
					MGUI.ToggleGroup(_MatcapToggle.floatValue == 0);
					MGUI.PropertyGroup(() => {
						bool matcap1Tab = Foldouts.DoSmallFoldout(foldouts, mat, me, "Primary Matcap");
						if (matcap1Tab){
							MGUI.SpaceN2();
							MGUI.PropertyGroupLayer(() => {
								me.TexturePropertySingleLine(new GUIContent("Matcap"), _Matcap, _MatcapColor, _Matcap.textureValue ? _MatcapBlending : null);
								if (_Matcap.textureValue){
									MGUI.TexPropLabel("Blending", blendingLabelPos);
									MGUI.TextureSO(me, _Matcap);
								};
								me.ShaderProperty(_MatcapStr, "Strength");
								MGUI.ToggleSlider(me, "Manual Roughness", _MatcapUseRough, _MatcapRough);
								me.ShaderProperty(_UnlitMatcap, "Unlit");
							});
							MGUI.Space4();
						}
						else MGUI.SpaceN2();

						bool matcap2Tab = Foldouts.DoSmallFoldout(foldouts, mat, me, "Secondary Matcap");
						if (matcap2Tab){
							MGUI.SpaceN2();
							MGUI.PropertyGroupLayer(() => {
								me.TexturePropertySingleLine(new GUIContent("Matcap"), _Matcap1, _MatcapColor1, _Matcap1.textureValue ? _MatcapBlending1 : null);
								if (_Matcap1.textureValue){
									MGUI.TexPropLabel("Blending", blendingLabelPos);
									MGUI.TextureSO(me, _Matcap1);
								}
								me.ShaderProperty(_MatcapStr1, "Strength");
								MGUI.ToggleSlider(me, "Manual Roughness", _MatcapUseRough1, _MatcapRough1);
								me.ShaderProperty(_UnlitMatcap1, "Unlit");
								MGUI.ToggleGroupEnd();
							});
						}
						else MGUI.SpaceN2();
					});
					MGUI.Space2();
					MGUI.ToggleGroupEnd();
				}
				else MGUI.SpaceN2();

				// Subsurface Scattering
				bool sssTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _Subsurface, 1, "Subsurface");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoSubsurfReset();
				GUILayout.Space(5);
				if (sssTab){
					MGUI.Space2();
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_Subsurface.floatValue == 0);
						me.TexturePropertySingleLine(translucTexLabel, _TranslucencyMap);
						me.TexturePropertySingleLine(colorLabel, _SubsurfaceTex, _SColor);
						me.ShaderProperty(_SStr, "Strength");
						me.ShaderProperty(_SPen, "Penetration");
						me.ShaderProperty(_SSharp, "Smoothness");
						me.ShaderProperty(_SAtten, "Attenuation");
						MGUI.ToggleGroupEnd();
					});
					MGUI.Space2();
				}
				else MGUI.SpaceN2();

				// Rim
				bool basicRimTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _RimLighting, 1, "Basic Rim");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoRimReset();
				GUILayout.Space(5);
				if (basicRimTab){
					MGUI.Space2();
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_RimLighting.floatValue == 0);
						me.TexturePropertySingleLine(colorLabel, _RimTex, _RimCol, _RimBlending);
						MGUI.TexPropLabel("Blending", blendingLabelPos);
						if (_RimTex.textureValue){
							MGUI.TextureSOScroll(me, _RimTex, _RimScroll);
							MGUI.Space8();
						}
						me.ShaderProperty(_RimStr, "Strength");
						me.ShaderProperty(_RimWidth, "Width");
						me.ShaderProperty(_RimEdge, "Sharpness");
						me.ShaderProperty(_UnlitRim, "Unlit");
						MGUI.ToggleGroupEnd();
					});
					MGUI.Space2();
				}
				else MGUI.SpaceN2();

				// Environment Rim
				bool eRimTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _EnvironmentRim, 1, "Environment Rim");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoERimReset();
				GUILayout.Space(5);
				if (eRimTab){
					MGUI.Space2();
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_EnvironmentRim.floatValue == 0);
						me.ShaderProperty(_ERimBlending, "Blending");
						MGUI.Space2();
						me.ShaderProperty(_ERimTint, "Tint");
						MGUI.Space2();
						me.ShaderProperty(_ERimStr, "Strength");
						me.ShaderProperty(_ERimWidth, "Width");
						me.ShaderProperty(_ERimEdge, "Sharpness");
						MGUI.ToggleSlider(me, "Manual Roughness", _ERimUseRough, _ERimRoughness);
						MGUI.ToggleGroupEnd();
					});
					MGUI.Space2();
				}
				else MGUI.SpaceN2();

				// Refraction
				bool refracTab = Foldouts.DoMediumFoldoutError(foldouts, mat, me, _Refraction, refracError && queueError, 1, "Refraction");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoRefracReset();
				GUILayout.Space(5);
				if (refracTab){
					MGUI.Space2();
					MGUI.ToggleGroup(_Refraction.floatValue == 0);
					MGUI.PropertyGroup(() => {
						if (refracError)
							MGUI.DisplayError("Refractions require a render queue of 2501 or above to function correctly.");
						me.ShaderProperty(_RefractionTint, "Tint");
						me.ShaderProperty(_RefractionIOR, "IOR");
						me.ShaderProperty(_RefractionOpac, "Opacity");
						MGUI.ToggleSlider(me, "Chromatic Abberation", _RefractionCA, _RefractionCAStr);
						me.ShaderProperty(_UnlitRefraction, "Unlit");
						
					});
					MGUI.PropertyGroup(() => {
						me.TexturePropertySingleLine(new GUIContent("Dissolve Mask"), _RefractionDissolveMask, _RefractionDissolveMaskStr);
						MGUI.TextureSOScroll(me, _RefractionDissolveMask, _RefractionDissolveMaskScroll);
					});
					MGUI.ToggleGroupEnd();
					MGUI.Space2();
				}
				else MGUI.SpaceN2();

				// Normals
				bool normalTab = Foldouts.DoMediumFoldout(foldouts, mat, me, 1, "Normals");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoNormalReset();
				GUILayout.Space(5);
				if (normalTab){
					MGUI.Space2();
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_HardenNormals, "Hard Edges");
						me.ShaderProperty(_ClearCoat, "Clearcoat Mode");
						me.ShaderProperty(_GSAA, "GSAA");
					});
					MGUI.Space4();
				}
				else MGUI.Space8();
			}
		}
		else {
			bool bigMaskTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "MASKS");
			if (MGUI.TabButton(resetIcon, 26f)){
				DoMaskingReset();
			}
			MGUI.Space8();
			if (bigMaskTab){
				MGUI.Space4();
				me.ShaderProperty(_MaskingMode, "Mode");
				if (_MaskingMode.floatValue == 1){
					MGUI.Space2();
					MGUI.PropertyGroup(() => {
						me.TexturePropertySingleLine(new GUIContent("Emission"), _EmissMask);
						me.TexturePropertySingleLine(new GUIContent("Emission Pulse"), _PulseMask);
						me.TexturePropertySingleLine(new GUIContent("Filtering"), _FilterMask);	
						me.TexturePropertySingleLine(new GUIContent("Outline Thickness"), _OutlineMask);
					});
					MGUI.Space2();
				}
				else if (_MaskingMode.floatValue == 2){
					MGUI.Space2();
					MGUI.PropertyGroup(() => {
						me.TexturePropertySingleLine(new GUIContent("Packed Mask"), _PackedMask3);
						GUILayout.Label("Red:	Emission\nGreen:	Emission Pulse\nBlue:	Filtering\nAlpha:	Outline Thickness");
					});
					MGUI.Space2();
				}
				else MGUI.Space6();
			}
		}

		// -----------------
		// Emission
		// -----------------
		bool emissTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "EMISSION 0");
		if (MGUI.TabButton(resetIcon, 26f)){
			DoEmissionReset();
		}
		MGUI.Space8();
		if (emissTab){
			MGUI.Space4();
			me.ShaderProperty(_EmissionToggle, "Enable");
			MGUI.Space4();
			MGUI.PropertyGroup(() => {
				MGUI.ToggleGroup(_EmissionToggle.floatValue == 0);
				me.TexturePropertySingleLine(emissTexLabel, _EmissionMap, _EmissionColor, _EmissIntensity);
				MGUI.TexPropLabel("Intensity", 111);
				if (_EmissionMap.textureValue){
					MGUI.TextureSOScroll(me, _EmissionMap, _EmissScroll);
				}
			});
			MGUI.Space4();
			bool lrTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _ReactToggle, 1, "Light Reactivity") && _EmissionToggle.floatValue == 1;
			if (MGUI.MedTabButton(resetIcon, 23f))
				DoLRReset();
			GUILayout.Space(5);
			if (lrTab){
				MGUI.Space4();
				MGUI.ToggleGroup(_ReactToggle.floatValue == 0);
				me.ShaderProperty(_CrossMode, "Crossfade Mode");
				MGUI.PropertyGroup(() => {
					MGUI.ToggleGroup(_CrossMode.floatValue == 0);
					me.ShaderProperty(_ReactThresh, "Threshold");
					me.ShaderProperty(_Crossfade, "Strength");
					MGUI.ToggleGroupEnd();
					
				});
				MGUI.ToggleGroupEnd();
				MGUI.Space4();
			}
			else MGUI.SpaceN2();

			bool pulseTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _PulseToggle, 1, "Pulse") && _EmissionToggle.floatValue == 1;
			if (MGUI.MedTabButton(resetIcon, 23f))
				DoPulseReset();
			GUILayout.Space(5);
			if (pulseTab){
				MGUI.Space4();
				MGUI.ToggleGroup(_PulseToggle.floatValue == 0);
				me.ShaderProperty(_PulseWaveform, "Waveform");
				MGUI.Space2();
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_PulseStr, "Strength");
					me.ShaderProperty(_PulseSpeed, "Speed");
				});
				MGUI.ToggleGroupEnd();
				MGUI.SpaceN4();
			}
			MGUI.ToggleGroupEnd();
			MGUI.Space8();
		}
			
		// -----------------
		// Filters
		// -----------------
		bool filterTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "FILTERING");
		if (MGUI.TabButton(resetIcon, 26f)){
			DoFiltersReset();
		}
		MGUI.Space8();
		if (filterTab){
			MGUI.Space4();
			me.ShaderProperty(_Filtering, "Enable");
			MGUI.Space4();
			MGUI.ToggleGroup(_Filtering.floatValue == 0);
			MGUI.PropertyGroup(() => {
				me.ShaderProperty(_TeamFiltering, "Color Masking");
				me.ShaderProperty(_PostFiltering, "Post Filtering");
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
			MGUI.Space8();
			MGUI.ToggleGroupEnd();
		}

		// -----------------
		// Flipbook
		// -----------------
		bool ssTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "FLIPBOOK");
		if (MGUI.TabButton(resetIcon, 26f))
			DoSpriteReset();
		MGUI.Space8();
		if (ssTab){
			MGUI.Space8();
			bool sheet1Tab = Foldouts.DoMediumFoldout(foldouts, mat, me, _EnableSpritesheet, 2, "Primary Layer");
			if (MGUI.MedTabButton(resetIcon, 23f))
				DoSheet1Reset();
			GUILayout.Space(5);
			if (MGUI.MedTabButton(copyTo2Label, 47f))
				CopyToSheet2();
			GUILayout.Space(5);
			if (sheet1Tab){
				MGUI.Space4();
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
					MGUI.Space6();
					if (_SpritesheetMode0.floatValue == 1){
						MGUI.Vector2Field(_RowsColumns, "Columns / Rows");
						MGUI.Vector2Field(_FrameClipOfs, "Frame Size");
					}
					MGUI.Vector2Field(_SpritesheetPos, "Position");
					MGUI.Vector2Field(_SpritesheetScale, "Scale");
					me.ShaderProperty(_SpritesheetRot, "Rotation");
					MGUI.ToggleGroup(_ManualScrub.floatValue == 1);
					me.ShaderProperty(_FPS, "Speed");
					MGUI.ToggleGroupEnd();
					MGUI.SpaceN2();
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
				MGUI.Space4();
			}
			else MGUI.SpaceN2();

			bool sheet2Tab = Foldouts.DoMediumFoldout(foldouts, mat, me, _EnableSpritesheet1, 2, "Secondary Layer");
			if (MGUI.MedTabButton(resetIcon, 23f))
				DoSheet2Reset();
			GUILayout.Space(5);
			if (MGUI.MedTabButton(copyTo1Label, 47f))
				CopyToSheet1();
			GUILayout.Space(5);
			if (sheet2Tab){
				MGUI.Space4();
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
					MGUI.Space6();
					if (_SpritesheetMode1.floatValue == 1){
						MGUI.Vector2Field(_RowsColumns1, "Columns / Rows");
						MGUI.Vector2Field(_FrameClipOfs1, "Frame Size");
					}
					MGUI.Vector2Field(_SpritesheetPos1, "Position");
					MGUI.Vector2Field(_SpritesheetScale1, "Scale");
					me.ShaderProperty(_SpritesheetRot1, "Rotation");
					MGUI.ToggleGroup(_ManualScrub1.floatValue == 1);
					me.ShaderProperty(_FPS1, "Speed");
					MGUI.ToggleGroupEnd();
					MGUI.SpaceN2();
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
				MGUI.SpaceN4();
			}
			else MGUI.SpaceN2();
			MGUI.Space8();
		}

		// -----------------
		// Outline
		// -----------------
		if (isOutline){
			bool outlineTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "OUTLINE");
			if (MGUI.TabButton(resetIcon, 26f))
				DoOutlineReset();
			MGUI.Space8();
			if (outlineTab){
				if (isTransparent || blendMode == 3){
					MGUI.Space6();
					MGUI.DisplayError("Requires Opaque, Cutout, or Dithered blending mode to function.");
				}
				MGUI.ToggleGroup(isTransparent || blendMode == 3);
				MGUI.Space6();
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_ApplyOutlineLighting, "Apply Shading");
					MGUI.ToggleGroup(_EmissionToggle.floatValue == 0);
					me.ShaderProperty(_ApplyOutlineEmiss, "Apply Emission");
					MGUI.ToggleGroupEnd();
					me.ShaderProperty(_ApplyAlbedoTint, "Base Color Tint");
					EditorGUI.BeginChangeCheck();
					me.ShaderProperty(_StencilToggle, "Stencil Mode");
					if (EditorGUI.EndChangeCheck()){
						if (_StencilToggle.floatValue == 0)
							DoAdvancedReset();
						else
							ApplyOutlineStencilConfig(mat);
					}
					me.ShaderProperty(_IgnoreFilterMask, "Ignore Filter Mask");
					me.ShaderProperty(_UseVertexColor, "Vertex Color Thickness");
				});
				MGUI.PropertyGroup(() => {
					
					me.TexturePropertySingleLine(colorLabel, _OutlineTex, _OutlineCol);
					if (_OutlineTex.textureValue){
						MGUI.TextureSOScroll(me, _OutlineTex, _OutlineScroll);
						MGUI.Space4();
					}
					me.ShaderProperty(_OutlineMult, "Thickness");
					MGUI.Space2();
					me.ShaderProperty(_OutlineThicc, "Multiplier");
					me.ShaderProperty(_OutlineRange, "Min Range");
				});
				MGUI.Space6();
				MGUI.ToggleGroupEnd();
			}
		}

		// -----------------
		// UV Distortion
		// -----------------
		bool uvdTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "UV DISTORTION");
		if (MGUI.TabButton(resetIcon, 26f)){
			DoUVDReset();
		}
		MGUI.Space8();
		if (uvdTab){
			MGUI.Space8();
			me.ShaderProperty(_DistortionStyle, "Mode");
			MGUI.Space2();
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
						me.TexturePropertySingleLine(maskLabel, _DistortUVMask);
						me.TexturePropertySingleLine(normalTexLabel, _DistortUVMap, _DistortUVMap.textureValue ? _DistortUVStr : null);
						if (_DistortUVMap.textureValue)
							MGUI.TexPropLabel("Strength", 110);
						MGUI.TextureSOScroll(me, _DistortUVMap, _DistortUVScroll);
					});	
				}
				else if (_DistortionStyle.floatValue == 2) {
					MGUI.PropertyGroup(() => {
						me.TexturePropertySingleLine(new GUIContent("UV Distortion"), _DistortUVMask);
						MGUI.Space8();
						me.ShaderProperty(_NoiseOctaves, "Octaves");
						me.ShaderProperty(_DistortUVStr, "Strength");
						me.ShaderProperty(_NoiseSpeed, "Speed");
						MGUI.Vector2Field(_NoiseScale, "Scale");
					});
				}
			}
			MGUI.Space8();
		}

		// -----------------
		// Vertex Manip
		// -----------------
		bool vertexTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "VERTEX MANIPULATION 0");
		if (MGUI.TabButton(resetIcon, 26f)){
			DoVertexReset();
		}
		MGUI.Space8();
		if (vertexTab){
			MGUI.Space4();
			me.ShaderProperty(_VertexManipulationToggle, "Enable");
			MGUI.Space4();
			MGUI.ToggleGroup(_VertexManipulationToggle.floatValue == 0);
			MGUI.PropertyGroup(() => {
				me.TexturePropertySingleLine(maskLabel, _VertexExpansionMask);
				MGUI.Vector3FieldNoIndent(_VertexExpansion, "Expansion");
				me.ShaderProperty(_VertexExpansionClamp, "Clamp Direction");
			});
			MGUI.PropertyGroup(() => {
				me.TexturePropertySingleLine(maskLabel, _VertexRoundingMask);
				me.ShaderProperty(_VertexRounding, "Position Rounding");
				me.ShaderProperty(_VertexRoundingPrecision, "Precision");
			});
			MGUI.ToggleGroupEnd();
			MGUI.Space8();
		}

		// -----------------
		// Audio Link
		// -----------------
		bool audioLinkTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "AUDIO LINK");
		if (MGUI.TabButton(resetIcon, 26f)){
			DoAudioLinkReset();
		}
		MGUI.Space8();
		if (audioLinkTab){
			MGUI.Space4();
			me.ShaderProperty(_AudioLinkToggle, "Enable");
			MGUI.ToggleGroup(_AudioLinkToggle.floatValue == 0);
			MGUI.Space4();

			bool emissTabAL = Foldouts.DoMediumFoldout(foldouts, mat, me, 1, "Emission 1");
			if (MGUI.MedTabButton(resetIcon, 23f)){
				DoAudioLinkEmissionReset();
			}
			if (emissTabAL){
				MGUI.Space6();
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_AudioLinkEmissionBand, "Band");
					me.ShaderProperty(_AudioLinkEmissionMultiplier, "Strength");
				});
				MGUI.Space2();
			}
			else MGUI.Space3();

			bool rimTab = Foldouts.DoMediumFoldout(foldouts, mat, me, 1, "Rim");
			if (MGUI.MedTabButton(resetIcon, 23f)){
				DoAudioLinkRimReset();
			}
			if (rimTab){
				MGUI.Space6();
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_AudioLinkRimBand, "Band");
					me.ShaderProperty(_AudioLinkRimMultiplier, "Strength");
					me.ShaderProperty(_AudioLinkRimWidth, "Width");
					me.ShaderProperty(_AudioLinkRimPulse, "Pulse Strength");
					me.ShaderProperty(_AudioLinkRimPulseWidth, "Pulse Width");
					me.ShaderProperty(_AudioLinkRimPulseSharp, "Pulse Sharpness");
				});
				MGUI.Space2();
			}
			else MGUI.Space3();

			bool bcDissTabAL = Foldouts.DoMediumFoldout(foldouts, mat, me, 1, "Base Color Dissolve 1");
			if (MGUI.MedTabButton(resetIcon, 23f)){
				DoAudioLinkBCDissolveReset();
			}
			if (bcDissTabAL){
				MGUI.Space6();
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_AudioLinkBCDissolveBand, "Band");
					me.ShaderProperty(_AudioLinkBCDissolveMultiplier, "Strength");
				});
				MGUI.Space2();
			}
			else MGUI.Space3();

			bool vertManipTabAL = Foldouts.DoMediumFoldout(foldouts, mat, me, 1, "Vertex Manipulation 1");
			if (MGUI.MedTabButton(resetIcon, 23f)){
				DoAudioLinkVertManipReset();
			}
			if (vertManipTabAL){
				MGUI.Space6();
				MGUI.PropertyGroup(() => {
					me.ShaderProperty(_AudioLinkVertManipBand, "Band");
					me.ShaderProperty(_AudioLinkVertManipMultiplier, "Strength");
				});
				MGUI.Space2();
			}
			else MGUI.Space3();

			if (isUberX){
				bool dissTabAL = Foldouts.DoMediumFoldout(foldouts, mat, me, 1, "Dissolve 1");
				if (MGUI.MedTabButton(resetIcon, 23f)){
					DoAudioLinkDissolveReset();
				}
				if (dissTabAL){
					MGUI.Space6();
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_AudioLinkDissolveBand, "Band");
						me.ShaderProperty(_AudioLinkDissolveMultiplier, "Strength");
					});
					MGUI.Space2();
				}
				else MGUI.Space3();

				bool triOfsTabAL = Foldouts.DoMediumFoldout(foldouts, mat, me, 1, "Triangle Offset");
				if (MGUI.MedTabButton(resetIcon, 23f)){
					DoAudioLinkTriOffsetReset();
				}
				if (triOfsTabAL){
					MGUI.Space6();
					MGUI.PropertyGroup( () => {
						me.ShaderProperty(_AudioLinkTriOffsetBand, "Band");
						me.ShaderProperty(_AudioLinkTriOffsetMode, "Style");
						me.ShaderProperty(_AudioLinkTriOffsetCoords, "Axis");
						MGUI.Space8();
						me.ShaderProperty(_AudioLinkTriOffsetStrength, "Strength");
						me.ShaderProperty(_AudioLinkTriOffsetStartPos, "Start Position");
						me.ShaderProperty(_AudioLinkTriOffsetEndPos, "End Position");
						me.ShaderProperty(_AudioLinkTriOffsetSize, "Size");
						// me.ShaderProperty(_AudioLinkTriOffsetFalloff, "Falloff");
						if (_AudioLinkTriOffsetMask.textureValue)
							MGUI.Space8();
						me.TexturePropertySingleLine(maskLabel, _AudioLinkTriOffsetMask);
						MGUI.TextureSOScroll(me, _AudioLinkTriOffsetMask, _AudioLinkTriOffsetMaskScroll, _AudioLinkTriOffsetMask.textureValue);
					});
					MGUI.Space2();
				}
				else MGUI.Space3();

				bool wfTabAL = Foldouts.DoMediumFoldout(foldouts, mat, me, 1, "Wireframe 1");
				if (MGUI.MedTabButton(resetIcon, 23f)){
					DoAudioLinkWireframeReset();
				}
				if (wfTabAL){
					MGUI.Space6();
					MGUI.PropertyGroup( () => {
						me.ShaderProperty(_AudioLinkWireframeBand, "Band");
						me.ShaderProperty(_AudioLinkWireframeMode, "Style");
						me.ShaderProperty(_AudioLinkWireframeCoords, "Axis");
						MGUI.Space8();
						me.ShaderProperty(_AudioLinkWireframeStrength, "Strength");
						me.ShaderProperty(_AudioLinkWireframeStartPos, "Start Position");
						me.ShaderProperty(_AudioLinkWireframeEndPos, "End Position");
						me.ShaderProperty(_AudioLinkWireframeSize, "Size");
						// me.ShaderProperty(_AudioLinkWireframeFalloff, "Falloff");
						me.ShaderProperty(_AudioLinkWireframeColor, "Color");
						if (_AudioLinkWireframeMask.textureValue)
							MGUI.Space8();
						me.TexturePropertySingleLine(maskLabel, _AudioLinkWireframeMask);
						MGUI.TextureSOScroll(me, _AudioLinkWireframeMask, _AudioLinkWireframeMaskScroll, _AudioLinkWireframeMask.textureValue);
					});
					MGUI.Space2();
				}
				else MGUI.Space3();
			}
			MGUI.ToggleGroupEnd();
			MGUI.Space8();
		}

		// -----------------
		// X Features
		// -----------------
		if (isUberX){
			bool dfError = blendMode == 0 && _DistanceFadeToggle.floatValue > 0;
			bool dissError = blendMode == 0 && _DissolveStyle.floatValue > 0 && _DissolveStyle.floatValue < 3;
			bool[] specErrors = {dfError, dissError};
			bool uberXTab = Foldouts.DoFoldoutError(foldouts, mat, me, specErrors, 1, "SPECIAL FEATURES");
			if (MGUI.TabButton(resetIcon, 26f)){
				DoSpecialReset();
			}
			MGUI.Space8();
			if (uberXTab){
				MGUI.Space8();
				
				// Distance Fade 
				bool dfTab = Foldouts.DoMediumFoldoutError(foldouts, mat, me, dfError, 1, "Distance Fade");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoDFReset();
				GUILayout.Space(5);
				if (dfTab){
					MGUI.Space4();
					me.ShaderProperty(_DistanceFadeToggle, "Mode");
					MGUI.Space2();
					if (_DistanceFadeToggle.floatValue > 0){
						if (dfError)
							MGUI.DisplayError("Requires non-opaque blending mode to function.");
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
						MGUI.Space2();
					}
					else MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Dissolve
				bool dissolveTab = Foldouts.DoMediumFoldoutError(foldouts, mat, me, dissError, 1, "Dissolve 0");
				if (MGUI.MedTabButton(resetIcon, 23f)){
					DoDissolveReset();
				}
				GUILayout.Space(5);
				if (dissolveTab){
					MGUI.Space4();
					me.ShaderProperty(_DissolveStyle, "Mode");
					MGUI.Space2();
					if (_DissolveStyle.floatValue > 0){
						if (dissError)
							MGUI.DisplayError("Texture and simplex dissolve require a non-opaque blending mode to function.");
						MGUI.Space2();
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
								MGUI.Space4();
								me.ShaderProperty(_GeomDissolveAmount, "Clip Position");
								me.ShaderProperty(_GeomDissolveWidth, "Falloff Size");
								if (_GeomDissolveAxis.floatValue > 2){
									MGUI.Vector3FieldNoIndent(_DissolvePoint0, "Point 1");
									MGUI.Vector3FieldNoIndent(_DissolvePoint1, "Point 2");
								}
								MGUI.Vector3FieldNoIndent(_GeomDissolveSpread, "Offset Amount");
								me.ShaderProperty(_GeomDissolveClip, "Offset Clip");
								me.ShaderProperty(_GeomDissolveFilter, "Offset Filter");
							}

							if (_DissolveStyle.floatValue < 3){
								if (_DissolveStyle.floatValue == 1)
									MGUI.ToggleSlider(me, "Flow", _DissolveBlending, _DissolveBlendSpeed);
								else if (_DissolveStyle.floatValue == 2){
									me.ShaderProperty(_DissolveBlendSpeed, "Generation Speed");
									MGUI.Vector3FieldNoIndent(_DissolveNoiseScale, "Noise Scale");
								}
								MGUI.ToggleGroup(_CloneToggle.floatValue == 0);
								me.ShaderProperty(_DissolveClones, "Clones Only");
								MGUI.ToggleGroupEnd();
								MGUI.Space8();
								me.TexturePropertySingleLine(maskLabel, _DissolveMask);
								if (_DissolveStyle.floatValue == 1){
									me.TexturePropertySingleLine(dissolveTexLabel, _DissolveTex);
									if (_DissolveTex.textureValue){
										MGUI.TextureSOScroll(me, _DissolveTex, _DissolveScroll0);
										MGUI.Space8();
									}
								}
								me.ShaderProperty(_DissolveRimCol, "Rim Color");
								me.ShaderProperty(_DissolveRimWidth, "Rim Width");
							}
							MGUI.ToggleGroupEnd();
						});
						MGUI.Space2();
					}
					else MGUI.Space6();
				}
				else MGUI.SpaceN2();
				
				// Screenspace
				bool screenTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _Screenspace, 1, "Screenspace");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoScreenReset();
				GUILayout.Space(5);
				if (screenTab){
					MGUI.Space2();
					MGUI.ToggleGroup(_Screenspace.floatValue == 0);
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_Range, "Range");
						MGUI.Space8();
						MGUI.Vector3Field(_Position, "Position");
						Vector3 v = _Position.vectorValue;
						v.z = Mathf.Clamp(v.z, 0, 10000);
						_Position.vectorValue = v;
						MGUI.Vector3Field(_Rotation, "Rotation");
						MGUI.DoResetButton(_Position, _Rotation, new Vector4(0,0,0.25f,0), new Vector4(0,0,0,0));
					});
					MGUI.ToggleGroupEnd();
					MGUI.Space2();
				}
				else MGUI.SpaceN2();

				// Clones
				bool cloneTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _CloneToggle, 1, "Clones");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoCloneReset();
				GUILayout.Space(5);
				if (cloneTab){
					MGUI.Space2();
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_CloneToggle.floatValue == 0);
						me.ShaderProperty(_Visibility, "Enable");
						MGUI.Vector3Field(_EntryPos, "Offset Direction");
						me.ShaderProperty(_SaturateEP, "Clamp Offset Direction");
						EditorGUI.BeginChangeCheck();
						me.ShaderProperty(_ClonePattern, "Pattern Preset");
						if (EditorGUI.EndChangeCheck())
							ApplyClonePositions();
						bool positionsFoldout = Foldouts.DoSmallFoldout(foldouts, mat, me, "Positions");
						if (positionsFoldout){
							MGUI.SpaceN4();
							MGUI.PropertyGroupLayer(() => {
								MGUI.ToggleVector3W("Clone 1", (int)_Clone1.vectorValue.w, _Clone1);
								MGUI.ToggleVector3W("Clone 2", (int)_Clone2.vectorValue.w, _Clone2);
								MGUI.ToggleVector3W("Clone 3", (int)_Clone3.vectorValue.w, _Clone3);
								MGUI.ToggleVector3W("Clone 4", (int)_Clone4.vectorValue.w, _Clone4);
								MGUI.ToggleVector3W("Clone 5", (int)_Clone5.vectorValue.w, _Clone5);
								MGUI.ToggleVector3W("Clone 6", (int)_Clone6.vectorValue.w, _Clone6);
								MGUI.ToggleVector3W("Clone 7", (int)_Clone7.vectorValue.w, _Clone7);
								MGUI.ToggleVector3W("Clone 8", (int)_Clone8.vectorValue.w, _Clone8);
							});
						}
						else MGUI.SpaceN2();
						MGUI.ToggleGroupEnd();
					});
					MGUI.Space2();
				}
				else MGUI.SpaceN2();

				// Glitch
				bool glitchTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _GlitchToggle, 1, "Glitch");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoGlitchReset();
				GUILayout.Space(5);
				if (glitchTab){
					MGUI.Space2();
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
					MGUI.Space2();
				}
				else MGUI.SpaceN2();

				// Shatter Culling
				bool shatterTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _ShatterToggle, 1, "Shatter Culling");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoShatterReset();
				GUILayout.Space(5);
				if (shatterTab){
					MGUI.Space2();
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
					MGUI.Space2();
				}
				else MGUI.SpaceN2();

				// Wireframe
				bool wireTab = Foldouts.DoMediumFoldout(foldouts, mat, me, _WireframeToggle, 1, "Wireframe 0");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoWireframeReset();
				GUILayout.Space(5);
				if (wireTab){
					MGUI.Space2();
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_WireframeToggle.floatValue == 0);
						MGUI.ToggleGroup(_CloneToggle.floatValue == 0);
						me.ShaderProperty(_WFClones, "Clones Only");
						MGUI.ToggleGroupEnd();
						me.ShaderProperty(_WFMode, "Pattern");
						me.ShaderProperty(_WFColor, "Color");
						me.ShaderProperty(_WFVisibility, "Wire Opacity");
						me.ShaderProperty(_WFFill, "Fill Opacity");
						MGUI.ToggleGroupEnd();
					});
					MGUI.Space2();
				}
				else MGUI.Space6();
			}
		}

		// -----------------
		// Rendering
		// -----------------
		bool advancedTab = Foldouts.DoFoldout(foldouts, mat, me, 1, "RENDER SETTINGS");
		if (MGUI.TabButton(resetIcon, 26f)){
			DoAdvancedReset();
		}
		MGUI.Space8();
		if (advancedTab){
			GUILayout.Space(30);
			if (MGUI.MedTabButton(resetIcon, 23f)){
				DoRenderingReset();
			}
			GUILayout.Space(-18);
			MGUI.BoldLabel("General");
			MGUI.PropertyGroup(() => {
				me.ShaderProperty(_ZWrite, "ZWrite");
				me.ShaderProperty(_ZTest, "ZTest");
			});

			MGUI.BoldLabel("Near Clipping");
			MGUI.PropertyGroup(() => {
				me.ShaderProperty(_NearClipToggle, "Enable");
				MGUI.Space4();
				MGUI.ToggleGroup(_NearClipToggle.floatValue == 0);
				me.TexturePropertySingleLine(maskLabel, _NearClipMask);
				me.ShaderProperty(_NearClip, "Range");
				MGUI.ToggleGroupEnd();
			});
			GUILayout.Space(23);
			if (MGUI.MedTabButton(resetIcon, 23f)){
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
			MGUI.Space2();
		}
		GUILayout.Space(15);

		MGUI.CenteredTexture(watermarkTex, 0, 0);
		GUILayout.Space(-buttonSize);
		if (MGUI.LinkButton(patIconTex, buttonSize, buttonSize, xPos)){
			Application.OpenURL("https://www.patreon.com/mochieshaders");
		}
		GUILayout.Space(buttonSize);
		MGUI.VersionLabel(versionLabel, 12,-16,-30);

		mat.DisableKeyword("_");
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
		bool reflFallback = mat.GetTexture("_ReflCube");
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

		bool prevAO = mat.GetInt("_AOFiltering") == 1 && mat.GetInt("_PreviewAO") == 1;
		bool prevRough = mat.GetInt("_RoughnessFiltering") == 1 && mat.GetInt("_PreviewRough") == 1;
		prevRough = prevRough && (workflow == 0 || workflow >= 3);
		bool prevSmooth = mat.GetInt("_SmoothnessFiltering") == 1 && mat.GetInt("_PreviewSmooth") == 1;
		prevSmooth = prevSmooth && (workflow == 1 || workflow == 2);
		bool prevHeight = mat.GetInt("_HeightFiltering") == 1 && mat.GetInt("_PreviewHeight") == 1;

		// Begone grabpass
		bool ssrEnabled = ssr == 1 && reflToggle == 1 && renderMode > 0;
		bool refracEnabled = refracToggle == 1 && renderMode > 0;
		mat.SetShaderPassEnabled("Always", ssrEnabled || refracEnabled);

		SetKeyword(mat, "_METALLICGLOSSMAP", workflow >= 3 && renderMode > 0);
		SetKeyword(mat, "_SPECGLOSSMAP", (workflow == 1 || workflow == 2) && renderMode > 0);
		SetKeyword(mat, "_GLOSSYREFLECTIONS_OFF", reflToggle == 0 || renderMode == 0);
		SetKeyword(mat, "_SPECULARHIGHLIGHTS_OFF", specToggle == 0 || renderMode == 0);
		SetKeyword(mat, "_MAPPING_6_FRAMES_LAYOUT", cubeMode == 1);
		SetKeyword(mat, "_TERRAIN_NORMAL_MAP",  cubeMode == 2);
		SetKeyword(mat, "_REQUIRE_UV2", maskingMode == 2);
		SetKeyword(mat, "_COLORADDSUBDIFF_ON", maskingMode == 1);
		SetKeyword(mat, "_SUNDISK_NONE", renderMode == 0);
		SetKeyword(mat, "_SUNDISK_SIMPLE", specToggle == 2 && renderMode > 0);
		SetKeyword(mat, "_SUNDISK_HIGH_QUALITY", specToggle == 3 && renderMode > 0);
		SetKeyword(mat, "USER_LUT", (prevAO || prevRough || prevSmooth || prevHeight) && renderMode > 0);
		SetKeyword(mat, "EFFECT_BUMP", distortMode > 0);
		SetKeyword(mat, "GRAIN", distortMode == 1);
		SetKeyword(mat, "_COLORCOLOR_ON", filterToggle == 1);
		SetKeyword(mat, "_COLOROVERLAY_ON", filterToggle == 1 && postFilterToggle == 1);
		SetKeyword(mat, "_DETAIL_MULX2", usingDetail && renderMode > 0);
		SetKeyword(mat, "_PARALLAXMAP",  usingParallax && renderMode > 0);
		SetKeyword(mat, "_NORMALMAP", usingNormal && renderMode > 0);
		SetKeyword(mat, "_EMISSION", emissToggle == 1);
		SetKeyword(mat, "_ALPHATEST_ON", blendMode > 0 && blendMode < 4);
		SetKeyword(mat, "_ALPHABLEND_ON", blendMode == 4);
		SetKeyword(mat, "_ALPHAPREMULTIPLY_ON", blendMode == 5);
		SetKeyword(mat, "CHROMATIC_ABBERATION_LOW", ssrEnabled);
		SetKeyword(mat, "BLOOM_LENS_DIRT", emissToggle == 1 && pulseToggle == 1);
		SetKeyword(mat, "BLOOM", isUberX && cloneToggle == 1);
		SetKeyword(mat, "_ALPHAMODULATE_ON", dissolveStyle == 2 && isUberX);
		SetKeyword(mat, "DEPTH_OF_FIELD", dissolveStyle == 3 && isUberX);
		SetKeyword(mat, "_FADING_ON", matcapToggle == 1 && renderMode > 0);
		SetKeyword(mat, "FXAA", workflow == 4 && renderMode > 0);
		SetKeyword(mat, "PIXELSNAP_ON", eRimToggle == 1 && renderMode > 0);
		SetKeyword(mat, "EFFECT_HUE_VARIATION", spriteToggle0 == 1 || spriteToggle1 == 1);
		SetKeyword(mat, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", reflToggle == 2 && renderMode > 0);
		SetKeyword(mat, "DISTORT", refracEnabled);
		SetKeyword(mat, "CHROMATIC_ABBERATION", refracToggle == 1 && caToggle == 1 && renderMode > 0);
		SetKeyword(mat, "GEOM_TYPE_MESH", vManipToggle == 1);
		SetKeyword(mat, "GEOM_TYPE_BRANCH", maskingMode == 1 && maskTransToggle == 1);
		SetKeyword(mat, "VIGNETTE_MASKED", reflFallback && reflToggle > 0 && renderMode > 0);
		SetKeyword(mat, "DITHERING", bcDissToggle == 1 && renderMode > 0);
		SetKeyword(mat, "DEPTH_OF_FIELD_COC_VIEW", audioLinkToggle == 1);
		SetKeyword(mat, "GEOM_TYPE_FROND", rimToggle == 1 && renderMode > 0);
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

	void ListKeywords(Material mat, GUIContent label, float buttonSize){
		keywordsList.Clear();
		foreach (string s in mat.shaderKeywords)
			keywordsList.Add(s);
		
		if (MGUI.LinkButton(label, buttonSize, buttonSize, (MGUI.GetInspectorWidth()/2.0f)-11.0f))
			displayKeywords = !displayKeywords;
		
		if (displayKeywords){
			MGUI.Space8();
			string infoString = "";
			if (keywordsList.Capacity == 0){
				infoString = "NO KEYWORDS FOUND";
			}
			else {
				infoString = "\nKeywords Used:\n\n";
				foreach (string s in keywordsList){
					infoString += " " + s + "\n";
				}
			}
			MGUI.DisplayText(infoString);
			MGUI.SpaceN8();
		}
		GUILayout.Space(buttonSize-10);
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

	void ClearUnusedTextures(float renderMode, float workflow, float cubeMode){
		ClearBaseMaps(cubeMode);
		ClearTextureMaps(renderMode, workflow);
		ClearShadingMaps();
		ClearEmissionMaps();
		ClearFilterMaps();
		ClearSpriteSheets();
		ClearOutlineMaps();
		ClearUVDMaps();
		ClearSpecialMaps();
		ClearVertMaps();
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
			_TranslucencyMap.textureValue = null;
			_SubsurfaceTex.textureValue = null;
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
		_EnvironmentRim.floatValue = 0f;
		_Reflections.floatValue = 0f;
		_Specular.floatValue = 0f;
		_MatcapToggle.floatValue = 0f;
		_Subsurface.floatValue = 0f;
		_RimLighting.floatValue = 0f;
	}

	void DoTextureMapReset(){
		DoPrimaryMapsReset();
		DoDetailMapsReset();
		DoMaskingReset();
		DoRoughFilterReset();
		DoSmoothFilterReset();
		DoAOFilterReset();
		DoHeightFilterReset();
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
	}

	void DoSmoothFilterReset(){
		_PreviewSmooth.floatValue = 0f;
		_SmoothLightness.floatValue = 0f;
		_SmoothIntensity.floatValue = 0f;
		_SmoothContrast.floatValue = 1f;
	}

	void DoAOFilterReset(){
		_AOTintTex.textureValue = null;
		_PreviewAO.floatValue = 0f;
		_AOTint.colorValue = new Color(0,0,0,1);
		_AOLightness.floatValue = 0f;
		_AOIntensity.floatValue = 0f;
		_AOContrast.floatValue = 1f;
	}

	void DoHeightFilterReset(){
		_PreviewHeight.floatValue = 0f;
		_HeightLightness.floatValue = 0f;
		_HeightIntensity.floatValue = 0f;
		_HeightContrast.floatValue = 1f;
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
	}

	void DoShadowReset(){
		string rp = MGUI.presetPath+"/Textures/Ramps/DefaultRamp.png";
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
		_TranslucencyMap.textureValue = null;
		_SubsurfaceTex.textureValue = null;
		_SColor.colorValue = Color.white;
		_SStr.floatValue = 1f;
		_SSharp.floatValue = 0.5f;
		_SPen.floatValue = 0.5f;
		_SAtten.floatValue = 0.8f;
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
		DoAudioLinkWireframeReset();
		DoAudioLinkTriOffsetReset();
		DoAudioLinkBCDissolveReset();
		DoAudioLinkDissolveReset();
		DoAudioLinkRimReset();
		DoAudioLinkVertManipReset();
		DoAudioLinkEmissionReset();
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
	}

	void DoAudioLinkBCDissolveReset(){
		_AudioLinkBCDissolveBand.floatValue = 0f;
		_AudioLinkBCDissolveMultiplier.floatValue = 0f;
	}

	void DoAudioLinkDissolveReset(){
		_AudioLinkDissolveBand.floatValue = 0f;
		_AudioLinkDissolveMultiplier.floatValue = 0f;
	}

	void DoAudioLinkRimReset(){
		_AudioLinkRimBand.floatValue = 0f;
		_AudioLinkRimMultiplier.floatValue = 0f;
		_AudioLinkRimWidth.floatValue = 0f;
		_AudioLinkRimPulse.floatValue = 0f;
		_AudioLinkRimPulseWidth.floatValue = 0.5f;
		_AudioLinkRimPulseSharp.floatValue = 0.3f;
	}

	void DoAudioLinkVertManipReset(){
		_AudioLinkVertManipBand.floatValue = 0f;
		_AudioLinkVertManipMultiplier.floatValue = 0f;
	}

	void DoAudioLinkEmissionReset(){
		_AudioLinkEmissionBand.floatValue = 0f;
		_AudioLinkEmissionMultiplier.floatValue = 0f;
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
		_FrameClipOfs.vectorValue = new Vector4(0,0,0,0);
		_SpritesheetPos.vectorValue = new Vector4(0,0,0,0);
		_SpritesheetScale.vectorValue = new Vector4(1,1,0,0);
		_SpritesheetRot.floatValue = 0f;
		_FPS.floatValue = 30f;
		_ManualScrub.floatValue = 0f;
		_ScrubPos.floatValue = 1f;
	}

	void DoSheet2Reset(){
		_Spritesheet1.textureValue = null;
		_SpritesheetMode1.floatValue = 0f;
		_Flipbook1.textureValue = null;
		_SpritesheetBlending1.floatValue = 2f;
		_SpritesheetCol1.colorValue = Color.white;
		_RowsColumns1.vectorValue = new Vector4(8,8,0,0);
		_FrameClipOfs1.vectorValue = new Vector4(0,0,0,0);
		_SpritesheetPos1.vectorValue = new Vector4(0,0,0,0);
		_SpritesheetScale1.vectorValue = new Vector4(1,1,0,0);
		_SpritesheetRot1.floatValue = 0f;
		_FPS1.floatValue = 30f;
		_ManualScrub1.floatValue = 0f;
		_ScrubPos1.floatValue = 1f;
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
		_Rotation.vectorValue = new Vector4(0,0,0,0);
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
}