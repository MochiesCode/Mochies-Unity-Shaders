using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

class GradientObject : ScriptableObject
{
	public Gradient gradient = new Gradient();
}

public class USEditor : ShaderGUI {

    public static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
    Toggles toggles = new Toggles(
		new bool[] {
			true, false, false, false,
			false, false, false, false,
			false, false, false, false,
			false, false, false, false,
			false, false, false, false,
			false, false, false, false,
			false, false, false, false,
			false, false, false, false,
			false, false, false, false,
			false, false
		},
		new string[] {
			"BASE", 
			"TEXTURE MAPS",
			"SHADING", 
			"Masking", 
			"Lighting",
			"Shadows",
			"Reflections",
			"Specular",
			"Subsurface",
			"Matcap",
			"Basic Rim",
			"Roughness",
			"Normals",
			"EMISSION",
			"Pulse",
			"Light Reactivity",
			"FILTERS",
			"UV DISTORTION",
			"OUTLINE",
			"SPECIAL FEATURES",
			"Distance Fade",
			"Dissolve",
			"Screenspace",
			"Mesh Manipulation",
			"Clones",
			"Pattern",
			"Glitch",
			"Shatter Culling",
			"Wireframe",
			"PRESETS",
			"SPRITE SHEETS",
			"Occlusion",
			"Height",
			"Sheet 1",
			"Sheet 2",
			"Environment Rim",
			"DEBUG",
			"Smoothness"
		}
	);

	public static List<string> presetsList = new List<string>();
	public static string[] presets;

	private GradientObject gradientObj;
	private SerializedProperty colorGradient;
	private SerializedObject serializedGradient;
	private EditorWindow gradientWindow;
	private Texture2D rampTex;

	int popupIndex = -1;
	string presetText = "";
	string dirPath = "Assets/Mochie/Unity/Presets/Uber/";

	string header = "Header_Pro";
	string watermark = "Watermark_Pro";
	string patIcon = "Patreon_Icon";

	const float b = 0.4f;
    const float foldoutHeightL = 28.0f;
    const float foldoutHeightS = 22.0f;
    const int indent = 2;

	string MaskLabel = "Mask";
    GUIContent MainTexLabel = new GUIContent("Main Texture");
    GUIContent AlbedoLabel = new GUIContent("Albedo");
    GUIContent EmissTexLabel = new GUIContent("Emission Map");
    GUIContent NormalTexLabel = new GUIContent("Normal");
    GUIContent MetallicTexLabel = new GUIContent("Metallic");
    GUIContent RoughnessTexLabel = new GUIContent("Roughness");
    GUIContent OcclusionMapLabel = new GUIContent("Occlusion");
    GUIContent HeightTexLabel = new GUIContent("Height");
    GUIContent ReflCubeLabel = new GUIContent("Cubemap");
    GUIContent ShadowRampLabel = new GUIContent("Ramp");
    GUIContent SpecularTexLabel = new GUIContent("Specular Map");
    GUIContent PrimaryMapsLabel = new GUIContent("Primary Maps");
    GUIContent DetailMapsLabel = new GUIContent("Detail Maps");
	GUIContent DissolveTexLabel = new GUIContent("Dissolve Map");
	GUIContent DissolveRimTexLabel = new GUIContent("Rim Color");
	GUIContent ColorLabel = new GUIContent("Color");
	GUIContent PackedTexLabel = new GUIContent("Packed Texture");
	GUIContent Cubemap0Label = new GUIContent("Cubemap 0");
	GUIContent Cubemap1Label = new GUIContent("Cubemap 1");
	GUIContent TranslucLabel = new GUIContent("Thickness Map");
	GUIContent TintLabel = new GUIContent("Tint");
	GUIContent FilteringLabel = new GUIContent("PBR Filtering");
	GUIContent AdvancedLabel = new GUIContent("Settings");
	GUIContent SmoothLabel = new GUIContent("Smoothness");

	// Base
	MaterialProperty _RenderMode = null; 
    MaterialProperty _CullingMode = null;
    MaterialProperty _BlendMode = null;
    MaterialProperty _Cutoff = null; 
    MaterialProperty _ATM = null;
	MaterialProperty _DistanceFadeToggle = null;
	MaterialProperty _DistanceFadeMax = null;
	MaterialProperty _DistanceFadeMin = null;
	MaterialProperty _ClipRimColor = null;
	MaterialProperty _ClipRimWidth = null;
	MaterialProperty _ClipRimStr = null;
	MaterialProperty _Color = null; 
    MaterialProperty _MainTex = null; 
	MaterialProperty _MainTexScroll = null;
	MaterialProperty _DetailScroll = null;
	MaterialProperty _RimScroll = null;
    MaterialProperty _FilterMask = null;
	MaterialProperty _FilterMaskChannel = null;
    MaterialProperty _Saturation = null; 
    MaterialProperty _Contrast = null; 
    MaterialProperty _RAmt = null;
    MaterialProperty _GAmt = null;
    MaterialProperty _BAmt = null;
    MaterialProperty _Hue = null;
    MaterialProperty _Luminance = null;
    MaterialProperty _HSLMin = null;
    MaterialProperty _HSLMax = null;
    MaterialProperty _FilterModel = null;
    MaterialProperty _AutoShift = null;
    MaterialProperty _AutoShiftSpeed = null;
    MaterialProperty _Brightness = null;
	MaterialProperty _PostFiltering = null;
    MaterialProperty _HDR = null;
    // MaterialProperty _Noise = null;
	MaterialProperty _DistortUVMap = null;
	MaterialProperty _DistortUVStr = null;
	MaterialProperty _DistortUVScroll = null;
	MaterialProperty _DistortUVMask = null;
	MaterialProperty _DistortUVMaskChannel = null;
	MaterialProperty _DistortMainUV = null;
	MaterialProperty _DistortEmissUV = null;
	MaterialProperty _DistortDetailUV = null;
	MaterialProperty _DistortRimUV = null;
	MaterialProperty _MainTexCube0 = null;
	MaterialProperty _CubeMode = null;
	MaterialProperty _CubeBlend = null;
	MaterialProperty _CubeRotate0 = null;
	MaterialProperty _UnlitCube = null;
	MaterialProperty _AutoRotate0 = null;
	MaterialProperty _CubeColor0 = null;
	MaterialProperty _CubeBlendMode = null;
	MaterialProperty _CubeBlendMask = null;
	MaterialProperty _CubeBlendMaskChannel = null;
	MaterialProperty _IsCubeBlendMask = null;
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
	MaterialProperty _SpritesheetMask = null;
	MaterialProperty _SpritesheetMaskChannel = null;
	MaterialProperty _FrameClipOfs = null;
	MaterialProperty _UnlitSpritesheet = null;
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
	MaterialProperty _AlphaMaskChannel = null;
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
	MaterialProperty _MainTexTint = null;
	MaterialProperty _SmoothnessMap = null;
	MaterialProperty _PreviewSmooth = null;
	MaterialProperty _SmoothLightness = null;
	MaterialProperty _SmoothIntensity = null;
	MaterialProperty _SmoothContrast = null;
	MaterialProperty _SmoothnessFiltering = null;
	MaterialProperty _PackedRoughPreview = null;
	MaterialProperty _SharpSpecStr = null;
	MaterialProperty _ShadowConditions = null;
	MaterialProperty _DirectAO = null;
	MaterialProperty _Value = null;
	MaterialProperty _DistortionStyle = null;
	MaterialProperty _NoiseScale = null;
	MaterialProperty _NoiseSpeed = null;
	MaterialProperty _PreviewNoise = null;
	MaterialProperty _NoiseMinMax = null;
	MaterialProperty _NoiseOctaves = null;
	MaterialProperty _UseParallaxMap = null;
	MaterialProperty _UseDetailNormal = null;

	// PBR/Toon Shading
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
    MaterialProperty _ReflCubeFallback = null;
    MaterialProperty _DetailMask = null;
	MaterialProperty _DetailMaskChannel = null;
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
	MaterialProperty _SmoothShadeMask = null;
	MaterialProperty _SmoothShadeMaskChannel = null;
	MaterialProperty _PackedMap = null;
	MaterialProperty _MetallicChannel = null;
	MaterialProperty _RoughnessChannel = null;
	MaterialProperty _OcclusionChannel = null;
	MaterialProperty _InvertNormalY0 = null;
	MaterialProperty _InvertNormalY1 = null;
	MaterialProperty _RoughnessFiltering = null;
	MaterialProperty _RoughContrast = null;
	MaterialProperty _RoughLightness = null;
	MaterialProperty _RoughIntensity = null;
	MaterialProperty _VLightCont = null;
	MaterialProperty _Subsurface = null;
	MaterialProperty _SColor = null;
	MaterialProperty _SubsurfaceMask = null;
	MaterialProperty _SStr = null;
	MaterialProperty _SPen = null;
	MaterialProperty _SSharp = null;
	MaterialProperty _TranslucencyMap = null;
	MaterialProperty _SubsurfaceMaskChannel = null;
	MaterialProperty _SubsurfaceTex = null;
	MaterialProperty _SAtten = null;
	MaterialProperty _Dith = null;
	MaterialProperty _Alpha = null;
	MaterialProperty _MaxSteps = null;
	MaterialProperty _Step = null;
	MaterialProperty _LRad = null;
	MaterialProperty _SRad = null;
	MaterialProperty _EdgeFade = null;
	MaterialProperty _SSR = null;
	MaterialProperty _AttenSmoothing = null;
	MaterialProperty _ShadowDithering = null;
	MaterialProperty _ShadowDitherStr = null;
	MaterialProperty _MatcapMask = null;
	MaterialProperty _MatcapMaskChannel = null;
	MaterialProperty _SpecTex = null;
	MaterialProperty _ReflTex = null;
	MaterialProperty _PreviewAO = null;
	MaterialProperty _PreviewRough = null;
	MaterialProperty _PreviewHeight = null;
	MaterialProperty _PreviewActive = null;
	MaterialProperty _LinearSmooth = null;
	MaterialProperty _ZWrite = null;
	MaterialProperty _UseMetallicMap = null;
	MaterialProperty _UseSpecMap = null;

	// Toon Shading
	MaterialProperty _NonlinearSHToggle = null;
	MaterialProperty _SHStr = null;
	MaterialProperty _ColorPreservation = null;
	MaterialProperty _ClearCoat = null;
	MaterialProperty _ReflectionMask = null;
	MaterialProperty _ReflectionMaskChannel = null;
	MaterialProperty _ReflectionStr = null;	
	MaterialProperty _DisneyDiffuse = null;
    MaterialProperty _SharpSpecular = null;
    MaterialProperty _SpecularMask = null;
	MaterialProperty _SpecularMaskChannel = null;
    MaterialProperty _SpecStr = null; 
    MaterialProperty _ShadowRamp = null;
    MaterialProperty _RampWidth0 = null;
	MaterialProperty _RampWidth1 = null;
	MaterialProperty _RampWeight = null;
    MaterialProperty _ShadowMask = null;
	MaterialProperty _ShadowMaskChannel = null;
    MaterialProperty _ShadowStr = null; 
	MaterialProperty _Shadows = null;
	MaterialProperty _DDMask = null;
	MaterialProperty _DDMaskChannel = null;
	MaterialProperty _DirectCont = null;
	MaterialProperty _IndirectCont = null;
	MaterialProperty _RTDirectCont = null;
	MaterialProperty _RTIndirectCont = null;
	MaterialProperty _AnisoAngleX = null;
	MaterialProperty _AnisoAngleY = null;
	MaterialProperty _AnisoLayerX = null;
	MaterialProperty _AnisoLayerY = null;
	MaterialProperty _AnisoLayerStr = null;
	MaterialProperty _SpecularStyle = null;
	MaterialProperty _ReflCol = null;
	MaterialProperty _InterpMask = null;
	MaterialProperty _InterpMaskChannel = null;
	MaterialProperty _AnisoLerp = null;
	MaterialProperty _MaskingMode = null;
	MaterialProperty _PackedMask0 = null;
	MaterialProperty _PackedMask1 = null;
	MaterialProperty _RTSelfShadow = null;
	MaterialProperty _ClampAdditive = null;
	MaterialProperty _AdditiveMax = null;
	MaterialProperty _HardenNormals = null;
	MaterialProperty _RampPos = null;

	// Emission
	MaterialProperty _EmissionToggle = null;
    MaterialProperty _EmissionColor = null; 
    MaterialProperty _EmissionMap = null; 
    MaterialProperty _EmissMask = null; 
	MaterialProperty _EmissMaskChannel = null;
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
	MaterialProperty _PulseMaskChannel = null;
    
    // Rim Lighting
	MaterialProperty _RimLighting = null;
    MaterialProperty _RimBlending = null;
    MaterialProperty _RimMask = null;
	MaterialProperty _RimMaskChannel = null;
    MaterialProperty _RimCol = null; 
    MaterialProperty _RimTex = null;
    MaterialProperty _RimStr = null; 
    MaterialProperty _RimWidth = null; 
    MaterialProperty _RimEdge = null;

	MaterialProperty _EnvironmentRim = null;
    MaterialProperty _ERimBlending = null;
    MaterialProperty _ERimTint = null; 
    MaterialProperty _ERimStr = null; 
    MaterialProperty _ERimWidth = null; 
    MaterialProperty _ERimEdge = null;
	MaterialProperty _ERimTex = null;
	MaterialProperty _ERimScroll = null;
	MaterialProperty _ERimUseRough = null;
	MaterialProperty _ERimRoughness = null;

    // Outlines
    MaterialProperty _ApplyOutlineLighting = null;
	MaterialProperty _ApplyOutlineEmiss = null;
    MaterialProperty _Outline = null; 
    MaterialProperty _OutlineThicc = null; 
    MaterialProperty _OutlineCol = null;
	MaterialProperty _OutlineTex = null;
	MaterialProperty _OutlineScroll = null;
	MaterialProperty _OutlineMask = null;
	MaterialProperty _OutlineMaskChannel = null;
	MaterialProperty _OutlineRange = null;

	// Team Colors
	MaterialProperty _TeamColorMask = null;
	MaterialProperty _TeamColor0 = null;
	MaterialProperty _TeamColor1 = null;
	MaterialProperty _TeamColor2 = null;
	MaterialProperty _TeamColor3 = null;

	// Special Effects
	MaterialProperty _GeomFXToggle = null;
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
	MaterialProperty _DisguiseMain = null;
	MaterialProperty _DissolveToggle = null;
	MaterialProperty _Screenspace = null;
	MaterialProperty _Position = null;
	MaterialProperty _Rotation = null;
	MaterialProperty _Range = null;
    MaterialProperty _ShowInMirror = null;
    MaterialProperty _ShowBase = null;
	MaterialProperty _BaseOffset = null;
	MaterialProperty _Connected = null;
	MaterialProperty _BaseRotation = null;
	MaterialProperty _ReflOffset = null;
	MaterialProperty _ReflRotation = null;
	MaterialProperty _DissolveAmount = null;
	MaterialProperty _DissolveTex = null;
	MaterialProperty _DissolveRimTex = null;
	MaterialProperty _DissolveRimCol = null;
	MaterialProperty _DissolveRimWidth = null;
	MaterialProperty _DissolveScroll0 = null;
	MaterialProperty _DissolveScroll1 = null;
	MaterialProperty _DissolveBlendSpeed = null;
	MaterialProperty _DissolveBlending = null;
	MaterialProperty _DissolveMask = null;
	MaterialProperty _DissolveMaskChannel = null;
	MaterialProperty _ClonePattern = null;
    MaterialProperty _CloneSpacing = null;
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
	MaterialProperty _ERimMaskChannel = null;

	MaterialProperty _NaNxddddd = null;
	MaterialProperty _UseAOTintTex = null;
	MaterialProperty _UseReflCube = null;
	MaterialProperty _UseSmoothMap = null;
	MaterialProperty _UseReflTex = null;
	MaterialProperty _UseSpecTex = null;
	MaterialProperty _UseERimTex = null;
	MaterialProperty _UseRimTex = null;
	MaterialProperty _DistortUVs = null;

	MaterialProperty _DebugEnum = null;
	MaterialProperty _DebugVector = null;
	MaterialProperty _DebugFloat = null;
	MaterialProperty _DebugRange = null;
	MaterialProperty _DebugIntRange = null;
	MaterialProperty _DebugColor = null;
	MaterialProperty _DebugHDRColor = null;
	MaterialProperty _DebugToggle = null;

    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

	bool m_FirstTimeApply = true;

    MaterialEditor m_me;
    public override void OnGUI(MaterialEditor me, MaterialProperty[] props) {
		Material mat = (Material)me.target;
		mat.DisableKeyword("_");
		if (m_FirstTimeApply) {
			m_FirstTimeApply = false;
		}

		// Return if the mat editor isn't displayed in the inspector
        if (!me.isVisible)
            return;

		// Find properties
        foreach (var property in GetType().GetFields(bindingFlags)){
            if (property.FieldType == typeof(MaterialProperty)){
                property.SetValue(this, FindProperty(property.Name, props));
            }
        }

		if (gradientObj == null){
			gradientObj = GradientObject.CreateInstance<GradientObject>();
			serializedGradient = new SerializedObject(gradientObj);
			colorGradient = serializedGradient.FindProperty("gradient");
			rampTex = new Texture2D(128, 16);
			GradientToTexture(ref rampTex);
		}

		_NaNxddddd.floatValue = 0.0f;

		// Generate preset popup items and other path sensitive folders if necessary
		if (!AssetDatabase.IsValidFolder(MGUI.parentPath))
			AssetDatabase.CreateFolder(MGUI.presetPath, "Presets");
		if (!AssetDatabase.IsValidFolder(MGUI.parentPath+"/Uber"))
			AssetDatabase.CreateFolder(MGUI.parentPath, "Uber");
		if (!AssetDatabase.IsValidFolder(MGUI.presetPath+"/Textures/Ramps"))
			AssetDatabase.CreateFolder(MGUI.presetPath+"/Textures", "Ramps");
		DirectoryInfo dir = new DirectoryInfo(dirPath);
		FileInfo[] info = dir.GetFiles();
		foreach (FileInfo f in info){
			if (!f.Name.Contains(".meta") && f.Name.Contains(".mat")){
				Material candidate = (Material)AssetDatabase.LoadAssetAtPath(dirPath + f.Name, typeof(Material));
				if (candidate.shader.name == mat.shader.name){
					int indOf = f.Name.IndexOf(".");
					presetsList.Add(f.Name.Substring(0, indOf));
				}
			}
		}
		presets = presetsList.ToArray();
		presetsList.Clear();

		// Check name of shader to determine if certain properties should be displayed
        bool isTransparent = _BlendMode.floatValue == 2 || _BlendMode.floatValue == 3;
        bool isCutout = _BlendMode.floatValue == 1;
		bool isUberX = MGUI.IsXVersion(mat);

		if (isUberX){
			header = "HeaderX_Pro";
			if (!EditorGUIUtility.isProSkin){
				header = "HeaderX";
				watermark = "Watermark";
			}
		}
		else {
			header = "Header_Pro";
			if (!EditorGUIUtility.isProSkin){
				header = "Header";
				watermark = "Watermark";
			}
		}

		// Add mat to foldout dictionary if it isn't in there yet
		if (!foldouts.ContainsKey(mat))
			foldouts.Add(mat, toggles);

		// Force backface culling off if using screenspace mesh
		if (isUberX && _Screenspace.floatValue == 1 && _CullingMode.floatValue != 0)
			_CullingMode.floatValue = 0;

		// Setting floats based on render mode/texture presence/etc to save on excess conditionals in-shader
		_UseParallaxMap.floatValue = _ParallaxMap.textureValue ? 1 : 0;
		_UseDetailNormal.floatValue = _DetailNormalMap.textureValue ? 1 : 0;
		_UseAOTintTex.floatValue = _AOTintTex.textureValue ? 1 : 0;
		_UseReflCube.floatValue = _ReflCube.textureValue ? 1 : 0;
		_UseReflTex.floatValue = _ReflTex.textureValue ? 1 : 0;
		_UseSpecTex.floatValue = _SpecTex.textureValue ? 1 : 0;
		_UseERimTex.floatValue = _ERimTex.textureValue ? 1 : 0;
		_UseRimTex.floatValue = _RimTex.textureValue ? 1 : 0;
		_IsCubeBlendMask.floatValue = _CubeBlendMask.textureValue ? 1 : 0;
		_UseSmoothMap.floatValue = _SmoothnessMap.textureValue && _PBRWorkflow.floatValue == 1 ? 1 : 0;

		if (_PBRWorkflow.floatValue == 3){
			_UseMetallicMap.floatValue = 1f;
			_UseSpecMap.floatValue = 1f;
		}
		else {
			_UseMetallicMap.floatValue = _MetallicGlossMap.textureValue ? 1 : 0;
			_UseSpecMap.floatValue = _SpecGlossMap.textureValue ? 1 : 0;
		}

		if (_BlendMode.floatValue != 1)
			_ATM.floatValue = 0f;
	
		if (_Connected.floatValue == 1){
			_ReflOffset.vectorValue = _BaseOffset.vectorValue;
			_ReflRotation.vectorValue = _BaseRotation.vectorValue;
		}

		if (_RenderMode.floatValue == 1){
			if (_PBRWorkflow.floatValue != 1 && _PBRWorkflow.floatValue != 2)
				_PreviewSmooth.floatValue = 0f;
			else
				_PreviewRough.floatValue = 0f;
		}
		else {
			_PreviewRough.floatValue = 0f;
			_PreviewSmooth.floatValue = 0f;
			_PreviewAO.floatValue = 0f;
			_PreviewHeight.floatValue = 0f;
		}

		bool roughPreview = _PreviewRough.floatValue == 1 && _RoughnessFiltering.floatValue == 1;
		bool smoothPreview = _PreviewSmooth.floatValue == 1 && _SmoothnessFiltering.floatValue == 1;
		bool aoPreview = _PreviewAO.floatValue == 1 && _AOFiltering.floatValue == 1;
		bool heightPreview = _PreviewHeight.floatValue == 1 && _HeightFiltering.floatValue == 1;
		bool noisePreview = _PreviewNoise.floatValue == 1 && _DistortUVs.floatValue == 1 && _DistortionStyle.floatValue > 0;

		if (_PBRWorkflow.floatValue != 3){
			roughPreview = roughPreview && _SpecGlossMap.textureValue;
			aoPreview = aoPreview && _OcclusionMap.textureValue;
			heightPreview = heightPreview && _ParallaxMap.textureValue;
		}
		else {
			roughPreview = roughPreview && _PackedMap.textureValue;
			aoPreview = aoPreview && _PackedMap.textureValue;
			heightPreview = heightPreview && _PackedMap.textureValue;
		}

		if (_PBRWorkflow.floatValue == 1)
			smoothPreview = smoothPreview && _SmoothnessMap.textureValue;
		else if (_PBRWorkflow.floatValue == 2)
			smoothPreview = smoothPreview && _SpecGlossMap.textureValue;
		else
			smoothPreview = false;
		
		if (_PBRWorkflow.floatValue == 3 && _RoughnessFiltering.floatValue == 1 && _PreviewRough.floatValue == 1)
			_PackedRoughPreview.floatValue = 1f;
		else
			_PackedRoughPreview.floatValue = 0f;

		if (_RenderMode.floatValue == 1 && (aoPreview || roughPreview || smoothPreview || heightPreview || noisePreview))
			_PreviewActive.floatValue = 1f;
		else 
			_PreviewActive.floatValue = 0f;

		if (_DistortEmissUV.floatValue == 1 || 
			_DistortMainUV.floatValue == 1 || 
			_DistortDetailUV.floatValue == 1 ||
			_DistortRimUV.floatValue == 1
		){
			_DistortUVs.floatValue = 1f;
		}
		else _DistortUVs.floatValue = 0f;
		
        Texture2D headerTex = (Texture2D)Resources.Load(header, typeof(Texture2D));
		Texture2D watermarkTex = (Texture2D)Resources.Load(watermark, typeof(Texture2D));
		Texture2D patIconTex = (Texture2D)Resources.Load(patIcon, typeof(Texture2D));
		Texture2D resetIcon = (Texture2D)Resources.Load("ResetIcon", typeof(Texture2D));
		Texture2D resetFullIcon = (Texture2D)Resources.Load("ResetFullIcon", typeof(Texture2D));
		Texture2D expandIcon = (Texture2D)Resources.Load("ExpandIcon", typeof(Texture2D));
		Texture2D collapseIcon = (Texture2D)Resources.Load("CollapseIcon", typeof(Texture2D));
		Texture2D randomizeIcon = (Texture2D)Resources.Load("RandomizeIcon", typeof(Texture2D));
		Texture2D randomizeColIcon = (Texture2D)Resources.Load("RandomizeColIcon", typeof(Texture2D));
		Texture2D standardIcon = (Texture2D)Resources.Load("StandardIcon", typeof(Texture2D));
		Texture2D toonIcon = (Texture2D)Resources.Load("ToonIcon", typeof(Texture2D));
		Texture2D copyTo1Icon = (Texture2D)Resources.Load("CopyTo1Icon", typeof(Texture2D));
		Texture2D copyTo2Icon = (Texture2D)Resources.Load("CopyTo2Icon", typeof(Texture2D));
		
        MGUI.CenteredTexture(headerTex, 0, 0);

		// -----------------
		// Base Settings
		// -----------------
		bool baseTab = Foldouts.DoFoldoutBase(foldouts, mat, me, "BASE");
		if (MGUI.TabButton(collapseIcon, 26f)){
			for (int i = 1; i <= foldouts[mat].GetToggles().Length-1; i++)
				foldouts[mat].SetState(i, false);
		}
		MGUI.Space8();
		if (MGUI.TabButton(expandIcon, 54f)){
			for (int i = 1; i <= foldouts[mat].GetToggles().Length-1; i++)
				foldouts[mat].SetState(i, true);
		}
		MGUI.Space8();
		if (MGUI.TabButton(randomizeIcon, 82f)){
			for (int i = 0; i < props.Length; i++){
				switch (props[i].displayName){
					case "vec": props[i].vectorValue = new Vector4(UnityEngine.Random.Range(0f,1f), UnityEngine.Random.Range(0f,1f), UnityEngine.Random.Range(0f,1f), UnityEngine.Random.Range(0f,1f)); break;
					case "ra": props[i].floatValue = UnityEngine.Random.Range(props[i].rangeLimits.x, props[i].rangeLimits.y); break;
					case "fl": props[i].floatValue = UnityEngine.Random.Range(0f, 1f); break;
					case "tog": props[i].floatValue = (int)UnityEngine.Random.Range(0f,2f); break;
					case "en02":
					case "en03":
					case "en04": 
					case "en05": props[i].floatValue = (int)UnityEngine.Random.Range(0, int.Parse(props[i].displayName.Substring(props[i].displayName.Length-1))+1); break;
					default: break;
				}
			}
		}
		MGUI.Space8();
		if (MGUI.TabButton(randomizeColIcon, 110f)){
			for (int i = 0; i < props.Length; i++){
				if (props[i].displayName == "col")
					props[i].colorValue = new Color(UnityEngine.Random.Range(0f,1f), UnityEngine.Random.Range(0f,1f), UnityEngine.Random.Range(0f,1f), 1);
			}
		}
		MGUI.Space8();
		if (MGUI.TabButton(resetFullIcon, 138f))
			DoFullReset(mat);
		MGUI.Space8();
		if (baseTab){
			MGUI.Space6();
			me.RenderQueueField();
			me.ShaderProperty(_RenderMode, "Shading");
			EditorGUI.showMixedValue = _BlendMode.hasMixedValue;
			var mode = (MGUI.BlendMode)_BlendMode.floatValue;
			EditorGUI.BeginChangeCheck();
			mode = (MGUI.BlendMode)EditorGUILayout.Popup("Blending Mode", (int)mode, Enum.GetNames(typeof(MGUI.BlendMode)));
			if (EditorGUI.EndChangeCheck()) {
				me.RegisterPropertyChangeUndo("Blending Mode");
				_BlendMode.floatValue = (float)mode;
				foreach (var obj in _BlendMode.targets){
					MGUI.SetBlendMode((Material)obj, (MGUI.BlendMode)mode);
				}
				EditorGUI.showMixedValue = false;
			}
			if (_BlendMode.floatValue == 2)
				me.ShaderProperty(_ZWrite, "ZWrite");
			if (isCutout)
				me.ShaderProperty(_ATM, "Alpha To Coverage");
			me.ShaderProperty(_CullingMode, "Backface Culling");
			me.ShaderProperty(_CubeMode, "Main Texture Type");
			if ((int)_CubeMode.floatValue == 1)
				me.ShaderProperty(_UnlitCube, "Unlit");
			if (isCutout || isTransparent)
				me.ShaderProperty(_UseAlphaMask, "Use Alpha Mask");
			if (isCutout && _ATM.floatValue == 0){
				MGUI.Space8();
				me.ShaderProperty(_Cutoff, "Cutout");
			}
			GUILayout.Space(16);
			switch((int)_CubeMode.floatValue){

				// Tex Only
				case 0: 
					me.TexturePropertySingleLine(MainTexLabel, _MainTex, _Color, _RenderMode.floatValue < 2 ? _ColorPreservation : null);
					if (_RenderMode.floatValue < 2) MGUI.TexPropLabel("Color Clamp", 130);
					if (_UseAlphaMask.floatValue == 1 && (isCutout || isTransparent))
						MGUI.MaskProperty(me, "Alpha Mask", _AlphaMask, _AlphaMaskChannel);
					MGUI.TextureSOScroll(me, _MainTex, _MainTexScroll);
					break;
				
				// Cubemap Only
				case 1: 
					me.TexturePropertySingleLine(ReflCubeLabel, _MainTexCube0, _CubeColor0, _RenderMode.floatValue < 2 ? _ColorPreservation : null);
					if (_RenderMode.floatValue < 2) MGUI.TexPropLabel("Color Clamp", 130);
					MGUI.Space4();
					if (_UseAlphaMask.floatValue == 1 && (isCutout || isTransparent))
						MGUI.MaskProperty(me, "Alpha Mask", _AlphaMask, _AlphaMaskChannel);
					MGUI.Vector3Field(_CubeRotate0, "Rotation");
					me.ShaderProperty(_AutoRotate0, "Auto Rotate");
					break;
				
				// Tex and Cubemap
				case 2: 
					me.TexturePropertySingleLine(MainTexLabel, _MainTex, _Color, _RenderMode.floatValue < 2 ? _ColorPreservation : null);
					if (_RenderMode.floatValue < 2) MGUI.TexPropLabel("Color Clamp", 130);
					if (_UseAlphaMask.floatValue == 1 && (isCutout || isTransparent))
						MGUI.MaskProperty(me, "Alpha Mask", _AlphaMask, _AlphaMaskChannel);
					MGUI.TextureSOScroll(me, _MainTex, _MainTexScroll);

					GUILayout.Space(12);
					MGUI.MaskProperty(me, "Blend", _CubeBlendMask, _CubeBlend, _CubeBlendMaskChannel);
					GUILayout.Space(12);

					me.TexturePropertySingleLine(ReflCubeLabel, _MainTexCube0, _CubeColor0, _CubeBlendMode);
					MGUI.TexPropLabel("Blending", 106);
					MGUI.Space4();
					MGUI.Vector3Field(_CubeRotate0, "Rotation");
					me.ShaderProperty(_AutoRotate0, "Auto Rotate");
					break;
				default: break;
			}
			MGUI.Space8();
		}

		// -----------------
		// Shading
		// -----------------
		mat.SetShaderPassEnabled("Always", _SSR.floatValue == 1 && _Reflections.floatValue == 1 && _RenderMode.floatValue == 1);
		if (_RenderMode.floatValue == 1){
			bool texturesTab = Foldouts.DoFoldout(foldouts, mat, me, -20f, "TEXTURE MAPS");
			if (MGUI.TabButton(resetIcon, 26f))
				DoTextureMapReset();
			MGUI.Space8();
			if (texturesTab){
				MGUI.Space8();
				me.ShaderProperty(_PBRWorkflow, "Workflow");
				GUILayout.Label(PrimaryMapsLabel, EditorStyles.boldLabel);
				switch ((int)_PBRWorkflow.floatValue){

					// Metallic
					case 0:
						me.TexturePropertySingleLine(MetallicTexLabel, _MetallicGlossMap, _MetallicGlossMap.textureValue ? null : _Metallic);
						me.TexturePropertySingleLine(RoughnessTexLabel, _SpecGlossMap, _SpecGlossMap.textureValue ? null : _Glossiness);
						break;

					// Specular RGB
					case 1: 
						me.TexturePropertySingleLine(SpecularTexLabel, _SpecGlossMap, _SpecCol);
						me.TexturePropertySingleLine(SmoothLabel, _SmoothnessMap, _GlossMapScale);
						break;

					// Specular RGBA
					case 2: 
						me.TexturePropertySingleLine(SpecularTexLabel, _SpecGlossMap, _SpecCol);
						me.ShaderProperty(_GlossMapScale, "Smoothness", indent);
						MGUI.Space2();
						break;

					// Packed
					case 3:
						me.TexturePropertySingleLine(PackedTexLabel, _PackedMap);
						me.ShaderProperty(_MetallicChannel, "Metallic");
						me.ShaderProperty(_RoughnessChannel, "Roughness");
						me.ShaderProperty(_OcclusionChannel, "Occlusion");
						me.ShaderProperty(_OcclusionStrength, "Occlusion Strength");
						MGUI.Space8();
						break;
					default: break;
				}
				if (_PBRWorkflow.floatValue != 3)
					me.TexturePropertySingleLine(OcclusionMapLabel, _OcclusionMap, _OcclusionMap.textureValue ? _OcclusionStrength : null);
				me.TexturePropertySingleLine(NormalTexLabel, _BumpMap, _BumpMap.textureValue ? _BumpScale : null);
				me.TexturePropertySingleLine(HeightTexLabel, _ParallaxMap, _ParallaxMap.textureValue ? _Parallax : null);
				MGUI.MaskProperty(me, "Detail Mask", _DetailMask, _DetailMaskChannel);
				GUILayout.Label(DetailMapsLabel, EditorStyles.boldLabel);
				me.TexturePropertySingleLine(AlbedoLabel, _DetailAlbedoMap);
				me.TexturePropertySingleLine(NormalTexLabel, _DetailNormalMap, _DetailNormalMap.textureValue ? _DetailNormalMapScale : null);
				if (_DetailAlbedoMap.textureValue || _DetailNormalMap.textureValue)
					MGUI.TextureSOScroll(me, _DetailAlbedoMap, _DetailScroll);
				
				GUILayout.Label(FilteringLabel, EditorStyles.boldLabel);

				// Roughness Filtering
				if (_PBRWorkflow.floatValue == 0 || _PBRWorkflow.floatValue == 3){
					if (_PBRWorkflow.floatValue != 3)
						MGUI.ToggleGroup(!_SpecGlossMap.textureValue);
					else
						MGUI.ToggleGroup(!_PackedMap.textureValue);
					bool roughFilterTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _RoughnessFiltering, "Roughness");
					if (_PBRWorkflow.floatValue != 3)  
						roughFilterTab = roughFilterTab && _SpecGlossMap.textureValue;
					else
						roughFilterTab = roughFilterTab && _PackedMap.textureValue;
					
					if (MGUI.MedTabButton(resetIcon, 23f)){
						DoRoughFilterReset();
					}
					GUILayout.Space(5);
					if (roughFilterTab){
						MGUI.Space2();
						MGUI.ToggleGroup(_RoughnessFiltering.floatValue == 0);
						me.ShaderProperty(_PreviewRough, "Preview");
						me.ShaderProperty(_RoughLightness, "Lightness");
						me.ShaderProperty(_RoughIntensity, "Intensity");
						me.ShaderProperty(_RoughContrast, "Contrast");
						MGUI.ToggleGroupEnd();
						MGUI.Space6();
					}
					else MGUI.SpaceN2();
					MGUI.ToggleGroupEnd();
				}

				// Smoothness Filtering (for specular)
				else {
					if (_PBRWorkflow.floatValue == 1)
						MGUI.ToggleGroup(!_SmoothnessMap.textureValue);
					else
						MGUI.ToggleGroup(!_SpecGlossMap.textureValue);
					bool smoothFilterTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _SmoothnessFiltering, "Smoothness");
					if (MGUI.MedTabButton(resetIcon, 23f))
						DoSmoothFilterReset();
					GUILayout.Space(5);
					if (_PBRWorkflow.floatValue == 1)
						smoothFilterTab = smoothFilterTab && _SmoothnessMap.textureValue;
					else
						smoothFilterTab = smoothFilterTab && _SpecGlossMap.textureValue;
					if (smoothFilterTab){
						MGUI.Space2();
						MGUI.ToggleGroup(_SmoothnessFiltering.floatValue == 0);
						me.ShaderProperty(_PreviewSmooth, "Preview");
						if (_PBRWorkflow.floatValue == 1)
							me.ShaderProperty(_LinearSmooth, "Treat as Linear");
						me.ShaderProperty(_SmoothLightness, "Lightness");
						me.ShaderProperty(_SmoothIntensity, "Intensity");
						me.ShaderProperty(_SmoothContrast, "Contrast");
						MGUI.ToggleGroupEnd();
						MGUI.Space6();
					}
					else MGUI.SpaceN2();
					MGUI.ToggleGroupEnd();
				}

				// AO Filtering
				if (_PBRWorkflow.floatValue != 3)
					MGUI.ToggleGroup(!_OcclusionMap.textureValue && _PBRWorkflow.floatValue != 3);
				else
					MGUI.ToggleGroup(!_PackedMap.textureValue);
				bool aoFilterTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _AOFiltering, "Occlusion");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoAOFilterReset();
				GUILayout.Space(5);
				if (_PBRWorkflow.floatValue != 3)
					aoFilterTab = aoFilterTab && _OcclusionMap.textureValue;
				else
					aoFilterTab = aoFilterTab && _PackedMap.textureValue;
				if (aoFilterTab){
					MGUI.Space2();
					MGUI.ToggleGroup(_AOFiltering.floatValue == 0);
					me.ShaderProperty(_PreviewAO, "Preview");
					MGUI.Space2();
					me.TexturePropertySingleLine(TintLabel, _AOTintTex, _AOTint);
					me.ShaderProperty(_AOLightness, "Lightness");
					me.ShaderProperty(_AOIntensity, "Intensity");
					me.ShaderProperty(_AOContrast, "Contrast");
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();
				MGUI.ToggleGroupEnd();

				// Height Filtering
				MGUI.ToggleGroup(!_ParallaxMap.textureValue);
				bool heightFilterTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _HeightFiltering, "Height") && _ParallaxMap.textureValue;
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoHeightFilterReset();
				GUILayout.Space(5);
				if (heightFilterTab){
					MGUI.Space2();
					MGUI.ToggleGroup(_HeightFiltering.floatValue == 0);
					me.ShaderProperty(_PreviewHeight, "Preview");
					me.ShaderProperty(_HeightLightness, "Lightness");
					me.ShaderProperty(_HeightIntensity, "Intensity");
					me.ShaderProperty(_HeightContrast, "Contrast");
					MGUI.ToggleGroupEnd();
				}
				else MGUI.SpaceN2();
				MGUI.ToggleGroupEnd();
				MGUI.Space8();
			}
			
			bool shadingTab = Foldouts.DoFoldoutShading(foldouts, mat, me, new MaterialProperty[] {_SSR, _Reflections, _Specular, _MatcapToggle, _MatcapBlending}, "SHADING");
			if (MGUI.TabButton(resetIcon, 26f))
				DoShadingReset(mat);
			MGUI.Space8();
			if (shadingTab){
				MGUI.Space8();

				// Lighting
				bool lightingTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -69f, "Lighting");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoLightingReset();
				GUILayout.Space(5);
				if (MGUI.MedTabButton(standardIcon, 47f))
					DoStandardLighting(mat);
				GUILayout.Space(5);
				if (MGUI.MedTabButton(toonIcon, 71f))
					DoToonLighting();
				GUILayout.Space(5);
				if (lightingTab){
					MGUI.Space8();
					MGUI.ToggleVector3("Static Direction", _StaticLightDirToggle, _StaticLightDir);
					MGUI.ToggleGroup(!_OcclusionMap.textureValue);
					me.ShaderProperty(_DirectAO, "Direct Occlusion");
					me.ShaderProperty(_IndirectAO, "Indirect Occlusion");
					MGUI.ToggleGroupEnd();
					if (!_OcclusionMap.textureValue){
						GUILayout.Space(-28);
						GUIStyle f = new GUIStyle(EditorStyles.boldLabel);
        				f.fontSize = 10;
						Rect r = EditorGUILayout.GetControlRect();
						r.x += EditorGUIUtility.labelWidth+21f;
						GUI.Label(r, "Requires Occlusion Map", f);
						MGUI.Space10();
					}
					GUILayout.Label("Diffuse Shading", EditorStyles.boldLabel);
					MGUI.SpaceN4();
					me.ShaderProperty(_DisneyDiffuse, "Disney Term");
					me.ShaderProperty(_SHStr, "Spherical Harmonics");
					MGUI.ToggleGroup(_SHStr.floatValue == 0);
					me.ShaderProperty(_NonlinearSHToggle, "Nonlinear SH");
					MGUI.ToggleGroupEnd();
					MGUI.Space2();
					GUILayout.Label("Realtime Lighting", EditorStyles.boldLabel);
					MGUI.SpaceN4();
					me.ShaderProperty(_RTDirectCont, "Direct Intensity");
					me.ShaderProperty(_RTIndirectCont, "Indirect Intensity");
					me.ShaderProperty(_VLightCont, "Vertex Intensity");
					MGUI.ToggleSlider(me, "Clamp Additive", _ClampAdditive, _AdditiveMax);
					MGUI.Space2();
					GUILayout.Label("Baked Lighting", EditorStyles.boldLabel);
					MGUI.SpaceN4();
					me.ShaderProperty(_DirectCont, "Direct Intensity");
					me.ShaderProperty(_IndirectCont, "Indirect Intensity");
					MGUI.Space8();
				}
				else MGUI.SpaceN2();

				// Masking
				bool maskingTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, "Masking");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoMaskingReset();
				GUILayout.Space(5);
				if (maskingTab){
					MGUI.Space4();
					me.ShaderProperty(_MaskingMode, "Mode");
					MGUI.ToggleGroup(_MaskingMode.floatValue == 0);
					MGUI.Space4();
					if (_MaskingMode.floatValue == 1){
						MGUI.MaskProperty(me, "Reflections", _ReflectionMask, _ReflectionMaskChannel, _Reflections.floatValue == 1);
						MGUI.MaskProperty(me, "Specular", _SpecularMask, _SpecularMaskChannel, _Specular.floatValue == 1);
						MGUI.MaskProperty(me, "Matcap", _MatcapMask, _MatcapMaskChannel, _MatcapToggle.floatValue == 1);
						MGUI.MaskProperty(me, "Shadows", _ShadowMask, _ShadowMaskChannel, _Shadows.floatValue == 1);
						MGUI.MaskProperty(me, "Basic Rim", _RimMask, _RimMaskChannel, _RimLighting.floatValue == 1);
						MGUI.MaskProperty(me, "Enviro. Rim", _ERimMask, _ERimMaskChannel, _EnvironmentRim.floatValue == 1);
						MGUI.MaskProperty(me, "Disney Diffuse", _DDMask, _DDMaskChannel);
						MGUI.MaskProperty(me, "Sph. Harmonics", _SmoothShadeMask, _SmoothShadeMaskChannel);
						MGUI.Space2();
					}
					else if (_MaskingMode.floatValue == 2){
						me.TexturePropertySingleLine(new GUIContent("Mask 0"), _PackedMask0);
						GUILayout.Label("Red:	Reflections + Specular\nGreen:	Matcap\nBlue:	Shadows");
						MGUI.Space8();
						me.TexturePropertySingleLine(new GUIContent("Mask 1"), _PackedMask1);
						GUILayout.Label("Red:	Basic Rim\nGreen:	Environment Rim\nBlue:	Diffuse Shading");
					}
					else if (_MaskingMode.floatValue == 3){
						me.TexturePropertySingleLine(new GUIContent("Mask 0"), _PackedMask0);
						GUILayout.Label("Red:	Reflections\nGreen:	Specular\nBlue:	Matcap\nAlpha:	Shadows");
						MGUI.Space8();
						me.TexturePropertySingleLine(new GUIContent("Mask 1"), _PackedMask1);
						GUILayout.Label("Red:	Basic Rim\nGreen:	Environment Rim\nBlue:	Disney Term\nAlpha:	Spherical Harmonics");
					}
					MGUI.ToggleGroupEnd();
					MGUI.Space4();
				}
				else MGUI.SpaceN2();

				// Reflections
				bool reflTab = Foldouts.DoMediumFoldoutSSR(foldouts, mat, me, new MaterialProperty[] {_Reflections, _SSR}, "Reflections");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoReflReset();
				GUILayout.Space(5);
				if (reflTab){
					MGUI.Space6();
					MGUI.ToggleGroup(_Reflections.floatValue == 0);
					me.TexturePropertySingleLine(ReflCubeLabel, _ReflCube, _ReflCubeFallback);
					MGUI.TexPropLabel("Fallback", 105);
					me.TexturePropertySingleLine(TintLabel, _ReflTex, _ReflCol);
					if (_ReflTex.textureValue)
						MGUI.TextureSO(me, _ReflTex);
					me.ShaderProperty(_ReflectionStr, "Strength");
					me.ShaderProperty(_SSR, "SSR");
					if (_SSR.floatValue == 1 && _Reflections.floatValue == 1){
						MGUI.Space8();
						if (mat.renderQueue < 2501)
							EditorGUILayout.HelpBox("SSR requires a render queue of 2501 or above to function correctly.", MessageType.Error, true);
						EditorGUILayout.HelpBox("SSR requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs\nIt is also is very expensive, please use it sparingly!", MessageType.Warning, true);
						MGUI.Space8();
						me.ShaderProperty(_Dith, "Dithering");
						me.ShaderProperty(_Alpha, "Strength");
						me.ShaderProperty(_MaxSteps, "Max Steps");
						me.ShaderProperty(_Step, "Step Size");
						me.ShaderProperty(_LRad, "Intersection (L)");
						me.ShaderProperty(_SRad, "Intersection (S)");
						me.ShaderProperty(_EdgeFade, "Edge Fade");
					}
					MGUI.ToggleGroupEnd();

					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Specular
				bool specTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _Specular, "Specular");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoSpecReset();
				GUILayout.Space(5);
				if (specTab){
					MGUI.Space6();
					MGUI.ToggleGroup(_Specular.floatValue == 0);
					me.ShaderProperty(_SpecularStyle, "Style");
					MGUI.Space6();
					me.TexturePropertySingleLine(TintLabel, _SpecTex, _PBRWorkflow.floatValue != 1 && _PBRWorkflow.floatValue != 2 ? _SpecCol : null);
					if (_SpecTex.textureValue)
						MGUI.TextureSO(me, _SpecTex);
					me.ShaderProperty(_SpecStr, "Strength");
					MGUI.ToggleIntSlider(me, "Stepping", _SharpSpecular, _SharpSpecStr);
					if (_SpecularStyle.floatValue == 2){
						MGUI.Space6();
						MGUI.MaskProperty(me, "Aniso Mask", _InterpMask, _InterpMaskChannel); 
					}
					if (_SpecularStyle.floatValue == 1 || _SpecularStyle.floatValue == 2){
							MGUI.Space6();
						me.ShaderProperty(_AnisoAngleX, "Base Width");
						me.ShaderProperty(_AnisoAngleY, "Base Height");
						MGUI.Space6();
						me.ShaderProperty(_AnisoLayerX, "Sub Width");
						me.ShaderProperty(_AnisoLayerY, "Sub Height");
						me.ShaderProperty(_AnisoLayerStr, "Sub Layer Blend");
						me.ShaderProperty(_AnisoLerp, "Lerp Blend");
					}
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Matcap
				bool matcapTab = Foldouts.DoMediumFoldoutMatcap(foldouts, mat, me, new MaterialProperty[]{_MatcapToggle, _Specular, _Reflections, _MatcapBlending}, "Matcap");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoMatcapReset();
				GUILayout.Space(5);
				if (matcapTab){
					MGUI.Space6();
					if (_MatcapToggle.floatValue == 1 && _Reflections.floatValue == 0 && _Specular.floatValue == 0 && _MatcapBlending.floatValue == 1){
						MGUI.DisplayError("Multiply blending requires reflections or specular.");
						MGUI.Space6();
					}
					MGUI.ToggleGroup(_MatcapToggle.floatValue == 0);
					me.TexturePropertySingleLine(ColorLabel, _Matcap, _MatcapColor, _Matcap.textureValue ? _MatcapBlending : null);
					if (_Matcap.textureValue)
						MGUI.TexPropLabel("Blending", 106);
					me.ShaderProperty(_MatcapStr, "Strength");
					me.ShaderProperty(_UnlitMatcap, "Unlit");
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Shadows
				bool shadowTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _Shadows, "Shadows");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoShadowReset();
				GUILayout.Space(5);
				if (shadowTab){
					MGUI.Space6();
					MGUI.ToggleGroup(_Shadows.floatValue == 0);
					me.ShaderProperty(_ShadowMode, "Style");
					me.ShaderProperty(_ShadowConditions, "Conditions");
					MGUI.Space8();
					if (_ShadowMode.floatValue == 1){
						me.TexturePropertySingleLine(ShadowRampLabel, _ShadowRamp);
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
					}
					else {
						me.ShaderProperty(_MainTexTint, "Multiply Albedo");
						me.ShaderProperty(_ShadowTint, "Tint");
						MGUI.Space4();
						me.ShaderProperty(_RampWidth0, "Ramp 1");
						me.ShaderProperty(_RampWidth1, "Ramp 2");
						me.ShaderProperty(_RampWeight, "Ramp Blend");
					}
					MGUI.Space8();
					me.ShaderProperty(_ShadowStr, "Strength");
					if (_ShadowMode.floatValue == 0)
						me.ShaderProperty(_RampPos, "Offset");
					MGUI.ToggleSlider(me, "Dithering", _ShadowDithering, _ShadowDitherStr);
					me.ShaderProperty(_RTSelfShadow, "Dynamic Shadows");
					MGUI.ToggleGroup(_RTSelfShadow.floatValue == 0);
					me.ShaderProperty(_AttenSmoothing, "Smooth Attenuation");
					MGUI.ToggleGroupEnd();
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Subsurface Scattering
				bool sssTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _Subsurface, "Subsurface");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoSubsurfReset();
				GUILayout.Space(5);
				if (sssTab){
					MGUI.Space6();
					MGUI.ToggleGroup(_Subsurface.floatValue == 0);
					me.TexturePropertySingleLine(TranslucLabel, _TranslucencyMap);
					MGUI.MaskProperty(me, "Mask", _SubsurfaceMask, _SubsurfaceMaskChannel);
					me.TexturePropertySingleLine(ColorLabel, _SubsurfaceTex, _SColor);
					me.ShaderProperty(_SStr, "Strength");
					me.ShaderProperty(_SPen, "Penetration");
					me.ShaderProperty(_SSharp, "Smoothness");
					me.ShaderProperty(_SAtten, "Attenuation");
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Rim
				bool rimTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _RimLighting, "Basic Rim");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoRimReset();
				GUILayout.Space(5);
				if (rimTab){
					MGUI.Space6();
					MGUI.ToggleGroup(_RimLighting.floatValue == 0);
					me.TexturePropertySingleLine(ColorLabel, _RimTex, _RimCol, _RimBlending);
					MGUI.TexPropLabel("Blending", 106);
					if (_RimTex.textureValue){
						MGUI.TextureSOScroll(me, _RimTex, _RimScroll);
						MGUI.Space8();
					}
					me.ShaderProperty(_RimStr, "Strength");
					me.ShaderProperty(_RimWidth, "Width");
					me.ShaderProperty(_RimEdge, "Sharpness");
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Environment Rim
				bool eRimTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _EnvironmentRim, "Environment Rim");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoERimReset();
				GUILayout.Space(5);
				if (eRimTab){
					MGUI.Space6();
					MGUI.ToggleGroup(_EnvironmentRim.floatValue == 0);
					me.TexturePropertySingleLine(TintLabel, _ERimTex, _ERimTint, _ERimBlending);
					if (_ERimTex.textureValue){
						MGUI.TextureSOScroll(me, _ERimTex, _ERimScroll);
						MGUI.Space8();
					}
					me.ShaderProperty(_ERimStr, "Strength");
					me.ShaderProperty(_ERimWidth, "Width");
					me.ShaderProperty(_ERimEdge, "Sharpness");
					MGUI.ToggleGroup(_ERimUseRough.floatValue == 1);
					me.ShaderProperty(_ERimRoughness, "Roughness");
					MGUI.ToggleGroupEnd();
					me.ShaderProperty(_ERimUseRough, "Use Roughness Map");
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Normals
				bool normalTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, "Normals");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoNormalReset();
				GUILayout.Space(5);
				if (normalTab){
					MGUI.Space2();
					me.ShaderProperty(_HardenNormals, "Hard Edges");
					me.ShaderProperty(_ClearCoat, "Clearcoat Mode");
					me.ShaderProperty(_InvertNormalY0, "Invert Normal Map Y");
					me.ShaderProperty(_InvertNormalY1, "Invert Detail Normal Y");
					MGUI.SpaceN2();
				}
				MGUI.Space8();
			}
		}

		// -----------------
		// Emission
		// -----------------
		bool emissTab = Foldouts.DoFoldout(foldouts, mat, me, -20f, "EMISSION");
		if (MGUI.TabButton(resetIcon, 26f))
			DoEmissionReset();
		MGUI.Space8();
		if (emissTab){
			MGUI.Space6();
			me.ShaderProperty(_EmissionToggle, "Enable");
			MGUI.Space6();
			MGUI.ToggleGroup(_EmissionToggle.floatValue == 0);
			me.TexturePropertySingleLine(EmissTexLabel, _EmissionMap, _EmissionColor);
			if (_EmissionMap.textureValue){
				MGUI.TextureSOScroll(me, _EmissionMap, _EmissScroll);
				MGUI.Space8();
			}
			MGUI.MaskProperty(me, _EmissMask, _EmissMaskChannel);
			if (_EmissMask.textureValue)
				MGUI.TextureSO(me, _EmissMask);
			MGUI.Space8();
			bool lrTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _ReactToggle, "Light Reactivity") && _EmissionToggle.floatValue == 1;
			if (MGUI.MedTabButton(resetIcon, 23f))
				DoLRReset();
			GUILayout.Space(5);
			if (lrTab){
				MGUI.Space2();
				MGUI.ToggleGroup(_ReactToggle.floatValue == 0);
				me.ShaderProperty(_CrossMode, "Crossfade Mode");
				MGUI.ToggleGroup(_CrossMode.floatValue == 0);
				me.ShaderProperty(_ReactThresh, "Threshold");
				me.ShaderProperty(_Crossfade, "Strength");
				MGUI.ToggleGroupEnd();
				MGUI.ToggleGroupEnd();
				MGUI.Space6();
			}
			else MGUI.SpaceN2();

			bool pulseTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _PulseToggle, "Pulse") && _EmissionToggle.floatValue == 1;
			if (MGUI.MedTabButton(resetIcon, 23f))
				DoPulseReset();
			GUILayout.Space(5);
			if (pulseTab){
				MGUI.Space4();
				MGUI.ToggleGroup(_PulseToggle.floatValue == 0);
				me.ShaderProperty(_PulseWaveform, "Waveform");
				MGUI.Space6();
				MGUI.MaskProperty(me, _PulseMask, _PulseMaskChannel);
				me.ShaderProperty(_PulseStr, "Strength");
				me.ShaderProperty(_PulseSpeed, "Speed");
				MGUI.ToggleGroupEnd();
			}
			MGUI.ToggleGroupEnd();
			MGUI.Space8();
		}
			
		// -----------------
		// Filters
		// -----------------
		bool filterTab = Foldouts.DoFoldout(foldouts, mat, me, -20f, "FILTERS");
		if (MGUI.TabButton(resetIcon, 26f))
			DoFiltersReset();
		MGUI.Space8();
		if (filterTab){
			MGUI.Space6();
			me.ShaderProperty(_FilterModel, "Style");
			if (_FilterModel.floatValue > 0){
				MGUI.Space6();
				me.ShaderProperty(_PostFiltering, "Post Filtering");
			}
			MGUI.Space8();
			switch ((int)_FilterModel.floatValue){

				// RGB
				case 1: 
					MGUI.MaskProperty(me, _FilterMask, _FilterMaskChannel);
					me.ShaderProperty(_RAmt, "Red");
					me.ShaderProperty(_GAmt, "Green");
					me.ShaderProperty(_BAmt, "Blue");
					break;
				
				// HSL
				case 2:
					MGUI.MaskProperty(me, _FilterMask, _FilterMaskChannel);
					me.ShaderProperty(_AutoShift, "Auto Shift");
					if (_AutoShift.floatValue == 1)
						me.ShaderProperty(_AutoShiftSpeed, "Speed");
					else
						me.ShaderProperty(_Hue, "Hue");
					me.ShaderProperty(_Luminance, "Luminance");
					me.ShaderProperty(_HSLMin, "Min Threshold");
					me.ShaderProperty(_HSLMax, "Max Threshold");
					break;

				// HSV
				case 3: 
					MGUI.MaskProperty(me, _FilterMask, _FilterMaskChannel);
					me.ShaderProperty(_AutoShift, "Auto Shift");
					if (_AutoShift.floatValue == 1)
						me.ShaderProperty(_AutoShiftSpeed, "Speed");
					else
						me.ShaderProperty(_Hue, "Hue");
					me.ShaderProperty(_Value, "Value");
					me.ShaderProperty(_HSLMin, "Min Threshold");
					me.ShaderProperty(_HSLMax, "Max Threshold");
					break;

				// Team Colors
				case 4: 
					me.TexturePropertySingleLine(new GUIContent("Team Color Mask"), _TeamColorMask);
					me.ShaderProperty(_TeamColor0, "Red Channel");
					me.ShaderProperty(_TeamColor1, "Green Channel");
					me.ShaderProperty(_TeamColor2, "Blue Channel");
					me.ShaderProperty(_TeamColor3, "Alpha Channel");
					MGUI.Space8();
					MGUI.MaskProperty(me, "Filter Mask", _FilterMask, _FilterMaskChannel);
					break;
				default: break;
			}
			
			if (_FilterModel.floatValue > 0){
				MGUI.Space8();
				me.ShaderProperty(_HDR, "HDR");
				me.ShaderProperty(_Contrast, "Contrast");
				me.ShaderProperty(_Saturation, "Saturation");
				me.ShaderProperty(_Brightness, "Brightness");
				MGUI.Space8();
			}
		}

		// -----------------
		// Texture Sheet
		// -----------------
		bool ssTab = Foldouts.DoFoldout(foldouts, mat, me, -20f, "SPRITE SHEETS");
		if (MGUI.TabButton(resetIcon, 26f))
			DoSpriteReset();
		MGUI.Space8();
		if (ssTab){
			MGUI.Space6();
			me.ShaderProperty(_UnlitSpritesheet, "Unlit");
			MGUI.Space2();
			MGUI.MaskProperty(me, "Mask", _SpritesheetMask, _SpritesheetMaskChannel);
			MGUI.Space6();
			bool sheet1Tab = Foldouts.DoMediumFoldout(foldouts, mat, me, -44f, _EnableSpritesheet, "Sheet 1");
			if (MGUI.MedTabButton(resetIcon, 23f))
				DoSheet1Reset();
			GUILayout.Space(5);
			if (MGUI.MedTabButton(copyTo2Icon, 47f))
				CopyToSheet2();
			GUILayout.Space(5);
			if (sheet1Tab){
				MGUI.Space6();
				MGUI.ToggleGroup(_EnableSpritesheet.floatValue == 0);
				me.TexturePropertySingleLine(new GUIContent("Sprite Sheet"), _Spritesheet, _SpritesheetCol, _SpritesheetBlending);
				MGUI.TexPropLabel("Blending", 106);
				MGUI.Vector2Field("Columns / Rows", _RowsColumns);
				MGUI.Vector2Field("Frame Size", _FrameClipOfs);
				MGUI.Vector2Field("Position", _SpritesheetPos);
				MGUI.Vector2Field("Scale", _SpritesheetScale);
				me.ShaderProperty(_SpritesheetRot, "Rotation");
				MGUI.ToggleGroup(_ManualScrub.floatValue == 1);
				me.ShaderProperty(_FPS, "Speed");
				MGUI.ToggleGroupEnd();
				MGUI.SpaceN2();
				MGUI.CustomToggleSlider("Frame", _ManualScrub, _ScrubPos, 0f, (_RowsColumns.vectorValue.x * _RowsColumns.vectorValue.y)-1);
				if (_ManualScrub.floatValue == 1 && _RowsColumns.vectorValue.x == 0 && _RowsColumns.vectorValue.y == 0)
					MGUI.DisplayWarning("Manual frame scrubbing will not behave correctly when rows and columns are both set to 0.");
				MGUI.ToggleGroupEnd();
				MGUI.Space6();
			}
			else MGUI.SpaceN2();

			MGUI.ToggleGroup(_EnableSpritesheet.floatValue == 0);
			bool sheet2Tab = Foldouts.DoMediumFoldout(foldouts, mat, me, -44f, _EnableSpritesheet1, "Sheet 2");
			if (MGUI.MedTabButton(resetIcon, 23f))
				DoSheet2Reset();
			GUILayout.Space(5);
			if (MGUI.MedTabButton(copyTo1Icon, 47f))
				CopyToSheet1();
			GUILayout.Space(5);
			if (sheet2Tab && _EnableSpritesheet.floatValue == 1){
				MGUI.Space6();
				MGUI.ToggleGroup(_EnableSpritesheet1.floatValue == 0);
				me.TexturePropertySingleLine(new GUIContent("Sprite Sheet"), _Spritesheet1, _SpritesheetCol1, _SpritesheetBlending1);
				MGUI.TexPropLabel("Blending", 106);
				MGUI.Vector2Field("Columns / Rows", _RowsColumns1);
				MGUI.Vector2Field("Frame Size", _FrameClipOfs1);
				MGUI.Vector2Field("Position", _SpritesheetPos1);
				MGUI.Vector2Field("Scale", _SpritesheetScale1);
				me.ShaderProperty(_SpritesheetRot1, "Rotation");
				MGUI.ToggleGroup(_ManualScrub1.floatValue == 1);
				me.ShaderProperty(_FPS1, "Speed");
				MGUI.ToggleGroupEnd();
				MGUI.SpaceN2();
				MGUI.CustomToggleSlider("Frame", _ManualScrub1, _ScrubPos1, 0f, (_RowsColumns1.vectorValue.x * _RowsColumns1.vectorValue.y)-1);
				if (_ManualScrub1.floatValue == 1 && _RowsColumns1.vectorValue.x == 0 && _RowsColumns1.vectorValue.y == 0)
					MGUI.DisplayWarning("Manual frame scrubbing will not behave correctly when rows and columns are both set to 0.");
				MGUI.ToggleGroupEnd();
			}
			else MGUI.SpaceN2();
			MGUI.ToggleGroupEnd();
			MGUI.ToggleGroupEnd();
			MGUI.Space8();
		}

		// -----------------
		// Outline
		// -----------------
		bool outlineTab = Foldouts.DoFoldout(foldouts, mat, me, -20f, "OUTLINE");
		if (MGUI.TabButton(resetIcon, 26f))
			DoOutlineReset();
		MGUI.Space8();
		if (outlineTab){
			if (isTransparent){
				MGUI.Space6();
				MGUI.DisplayError("Requires Opaque or Cutout blending mode to function.");
			}
			MGUI.ToggleGroup(isTransparent);
			MGUI.Space6();
			me.ShaderProperty(_Outline, "Style");
			MGUI.Space8();
			if (_Outline.floatValue > 0){
				if (_CullingMode.floatValue == 2){
					MGUI.DisplayWarning("Outline will be visible behind single-sided geometry when backface culling is enabled.");
					MGUI.Space6();
				}
				me.ShaderProperty(_ApplyOutlineLighting, "Apply Shading");
				me.ShaderProperty(_ApplyOutlineEmiss, "Apply Emission");
				MGUI.Space8();
				me.TexturePropertySingleLine(new GUIContent(MaskLabel), _OutlineMask, _OutlineMask.textureValue ? _OutlineMaskChannel : null);
				if (_Outline.floatValue == 1 || _Outline.floatValue == 2){
					me.ShaderProperty(_OutlineCol, "Color"); 
				}
				else {
					MGUI.Space4();
					me.TexturePropertySingleLine(ColorLabel, _OutlineTex, _OutlineCol);
					if (_OutlineTex.textureValue) 
						MGUI.TextureSOScroll(me, _OutlineTex, _OutlineScroll);
					MGUI.Space8();
				}
				me.ShaderProperty(_OutlineThicc, "Thickness");
				me.ShaderProperty(_OutlineRange, "Min Range");
				MGUI.Space8();
			}
			MGUI.ToggleGroupEnd();
		}

		// -----------------
		// UV Distortion
		// -----------------
		bool uvdTab = Foldouts.DoFoldout(foldouts, mat, me, -20f, "UV DISTORTION");
		if (MGUI.TabButton(resetIcon, 26f))
			DoUVDReset();
		MGUI.Space8();
		if (uvdTab){
			MGUI.Space10();
			me.ShaderProperty(_DistortionStyle, "Style");
			MGUI.Space8();
			me.ShaderProperty(_DistortMainUV, "Main");
			me.ShaderProperty(_DistortDetailUV, "Detail");
			me.ShaderProperty(_DistortEmissUV, "Emission");
			me.ShaderProperty(_DistortRimUV, "Rim");
			if (_DistortionStyle.floatValue == 0){
				MGUI.Space8();
				MGUI.MaskProperty(me, _DistortUVMask, _DistortUVMaskChannel);
				MGUI.Space8();
				me.TexturePropertySingleLine(NormalTexLabel, _DistortUVMap, _DistortUVMap.textureValue ? _DistortUVStr : null);
				if (_DistortUVMap.textureValue)
					MGUI.TexPropLabel("Strength", 110);
				MGUI.TextureSOScroll(me, _DistortUVMap, _DistortUVScroll);		
			}
			else {
				MGUI.ToggleGroup(_DistortUVs.floatValue == 0);
				me.ShaderProperty(_PreviewNoise, "Preview");
				MGUI.ToggleGroupEnd();
				MGUI.Space8();
				MGUI.MaskProperty(me, _DistortUVMask, _DistortUVMaskChannel);
				MGUI.Space8();
				me.ShaderProperty(_NoiseOctaves, "Octaves");
				me.ShaderProperty(_DistortUVStr, "Strength");
				me.ShaderProperty(_NoiseSpeed, "Speed");
				MGUI.Vector2Field("Scale", _NoiseScale);
				MGUI.Vector2Field("Min (X) Max (Y)", _NoiseMinMax);
			}
			MGUI.Space8();
		}

		// -----------------
		// X Features
		// -----------------
		if (isUberX){
			bool uberXTab = Foldouts.DoFoldoutSpecial(foldouts, mat, me, new MaterialProperty[]{_BlendMode, _DistanceFadeToggle, _DissolveToggle}, "SPECIAL FEATURES");
			if (MGUI.TabButton(resetIcon, 26f))
				DoSpecialReset();
			MGUI.Space8();
			if (uberXTab){
				MGUI.Space4();
				me.ShaderProperty(_GeomFXToggle, "Geometry Shader");
				MGUI.ToggleGroup(_GeomFXToggle.floatValue == 0);
				me.ShaderProperty(_DisguiseMain, "Affect Clones Only");
				MGUI.ToggleGroupEnd();
				MGUI.Space4();

				// Distance Fade 
				bool dfTab = Foldouts.DoMediumFoldoutSpecial(foldouts, mat, me, new MaterialProperty[]{_DistanceFadeToggle, _BlendMode}, false, "Distance Fade");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoDFReset();
				GUILayout.Space(5);
				if (dfTab){
					MGUI.Space4();
					me.ShaderProperty(_DistanceFadeToggle, "Style");
					if (_DistanceFadeToggle.floatValue > 0){
						MGUI.Space8();
						if (_BlendMode.floatValue == 0 && _DistanceFadeToggle.floatValue > 0){
							MGUI.DisplayError("Requires non-opaque blending mode to function.");
							MGUI.Space6();
						}
						MGUI.ToggleGroup(_DistanceFadeToggle.floatValue == 0 || _BlendMode.floatValue == 0);
						switch ((int)_DistanceFadeToggle.floatValue){
							case 1: 
								me.ShaderProperty(_DistanceFadeMin, "Range");
								me.ShaderProperty(_ClipRimColor, "Rim Color");
								me.ShaderProperty(_ClipRimStr, "Intensity");
								me.ShaderProperty(_ClipRimWidth, "Width"); 
								MGUI.Space6();
								break; 
							case 2: 
								me.ShaderProperty(_DistanceFadeMin, "Min Range"); 
								me.ShaderProperty(_DistanceFadeMax, "Max Range");
								MGUI.Space6();
								break;
							default: break;
						}
						MGUI.ToggleGroupEnd();
					}
					else MGUI.Space8();
				}
				else MGUI.SpaceN2();

				// Dissolve
				bool dissolveTab = Foldouts.DoMediumFoldoutSpecial(foldouts, mat, me, new MaterialProperty[]{_DissolveToggle, _BlendMode}, true, "Dissolve");
				MGUI.Space6();
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoDissolveReset();
				GUILayout.Space(5);
				if (dissolveTab){
					MGUI.Space4();
					if (_BlendMode.floatValue == 0 && _DissolveToggle.floatValue == 1){
						MGUI.DisplayError("Requires non-opaque blending mode to function.");
						MGUI.Space6();
					}
					MGUI.ToggleGroup(_DissolveToggle.floatValue == 0 || _BlendMode.floatValue == 0);
					me.ShaderProperty(_DissolveAmount, "Strength");
					MGUI.ToggleSlider(me, "Flow", _DissolveBlending, _DissolveBlendSpeed);
					MGUI.Space8();
					MGUI.MaskProperty(me, _DissolveMask, _DissolveMaskChannel);
					MGUI.Space8();
					me.TexturePropertySingleLine(DissolveTexLabel, _DissolveTex);
					if (_DissolveTex.textureValue){
						MGUI.TextureSOScroll(me, _DissolveTex, _DissolveScroll0);
						MGUI.Space8();
					}
					me.TexturePropertySingleLine(DissolveRimTexLabel, _DissolveRimTex, _DissolveRimCol);
					if (_DissolveRimTex.textureValue){
						MGUI.TextureSOScroll(me, _DissolveRimTex, _DissolveScroll1);
					}
					me.ShaderProperty(_DissolveRimWidth, "Rim Width");
					MGUI.ToggleGroupEnd();
					MGUI.Space8();
				}
				else MGUI.SpaceN2();
				
				// Screenspace
				bool screenTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _Screenspace, "Screenspace");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoScreenReset();
				GUILayout.Space(5);
				if (screenTab){
					MGUI.Space4();
					MGUI.ToggleGroup(_Screenspace.floatValue == 0);
					me.ShaderProperty(_Range, "Range");
					MGUI.Space8();
					MGUI.Vector3Field(_Position, "Position");
					Vector3 v = _Position.vectorValue;
					v.z = Mathf.Clamp(v.z, 0, 10000);
					_Position.vectorValue = v;
					MGUI.Vector3Field(_Rotation, "Rotation");
					MGUI.DoResetButton(_Position, _Rotation, new Vector4(0,0,0.25f,0), new Vector4(0,0,0,0));
					MGUI.ToggleGroupEnd();
					MGUI.Space8();
				}
				else MGUI.SpaceN2();

				// Mesh Manipulation
				bool meshTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, "Mesh Manipulation");
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoMeshReset();
				GUILayout.Space(5);
				if (meshTab){
					MGUI.Space4();
					me.ShaderProperty(_ShowBase, "Base Mesh");
					MGUI.Vector3Field(_BaseOffset, "Offset");
					MGUI.Vector3Field(_BaseRotation, "Rotation");
					MGUI.DoResetButton(_BaseOffset, _BaseRotation);
					MGUI.Space8();
					me.ShaderProperty(_ShowInMirror, "Mirror Reflection");
					me.ShaderProperty(_Connected, "Connected");
					MGUI.ToggleGroup(_Connected.floatValue == 1);
					MGUI.Vector3Field(_ReflOffset, "Offset");
					MGUI.Vector3Field(_ReflRotation, "Rotation");
					MGUI.DoResetButton(_ReflOffset, _ReflRotation);
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				MGUI.ToggleGroup(_GeomFXToggle.floatValue == 0);

				// Clones
				bool cloneTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, "Clones") && _GeomFXToggle.floatValue == 1;
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoCloneReset();
				GUILayout.Space(5);
				if (cloneTab){
					MGUI.Space4();
					me.ShaderProperty(_ClonePattern, "Pattern");
					me.ShaderProperty(_Visibility, "Enable");
					MGUI.Vector3Field(_EntryPos, "Entry Angle");
					me.ShaderProperty(_SaturateEP, "Clamp Entry Angle");
					if (Foldouts.DoSmallFoldout(foldouts, mat, me, "Pattern")){
						if (_ClonePattern.floatValue == 0){
							me.ShaderProperty(_Clone1, "Clone 1", 1); GUILayout.Space(-16);
							me.ShaderProperty(_Clone2, "Clone 2", 1); GUILayout.Space(-16);
							me.ShaderProperty(_Clone3, "Clone 3", 1); GUILayout.Space(-16);
							me.ShaderProperty(_Clone4, "Clone 4", 1); GUILayout.Space(-16);
							me.ShaderProperty(_Clone5, "Clone 5", 1); GUILayout.Space(-16);
							me.ShaderProperty(_Clone6, "Clone 6", 1); GUILayout.Space(-16);
							me.ShaderProperty(_Clone7, "Clone 7", 1); GUILayout.Space(-16);
							me.ShaderProperty(_Clone8, "Clone 8", 1); GUILayout.Space(-16);
							if (MGUI.ResetButton()){
								_Clone1.vectorValue = new Vector4(1,0,0,1);
								_Clone2.vectorValue = new Vector4(-1,0,0,1);
								_Clone3.vectorValue = new Vector4(0,0,1,1);
								_Clone4.vectorValue = new Vector4(0,0,-1,1);
								_Clone5.vectorValue = new Vector4(0.5f,0,0.5f,1);
								_Clone6.vectorValue = new Vector4(-0.5f,0,0.5f,1);
								_Clone7.vectorValue = new Vector4(0.5f,0,-0.5f,1);
								_Clone8.vectorValue = new Vector4(-0.5f,0,-0.5f,1);
							}
						}
						else {
							me.ShaderProperty(_CloneSpacing, "Spacing", 1);
						}
					}
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Glitch
				bool glitchTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _GlitchToggle, "Glitch") && _GeomFXToggle.floatValue == 1;
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoGlitchReset();
				GUILayout.Space(5);
				if (glitchTab){
					MGUI.Space4();
					MGUI.ToggleGroup(_GlitchToggle.floatValue == 0);
					me.ShaderProperty(_Instability, "Instability");
					me.ShaderProperty(_GlitchIntensity, "Intensity");
					me.ShaderProperty(_GlitchFrequency, "Frequency");
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Shatter Culling
				bool shatterTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _ShatterToggle, "Shatter Culling") && _GeomFXToggle.floatValue == 1;
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoShatterReset();
				GUILayout.Space(5);
				if (shatterTab){
					MGUI.Space4();
					MGUI.ToggleGroup(_ShatterToggle.floatValue == 0);
					me.ShaderProperty(_ShatterSpread, "Spread");
					me.ShaderProperty(_ShatterMin, "Min Range");
					me.ShaderProperty(_ShatterMax, "Max Range");
					me.ShaderProperty(_ShatterCull, "Culling Range");
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Wireframe
				bool wireTab = Foldouts.DoMediumFoldout(foldouts, mat, me, -21f, _WireframeToggle, "Wireframe") && _GeomFXToggle.floatValue == 1;
				if (MGUI.MedTabButton(resetIcon, 23f))
					DoWireframeReset();
				GUILayout.Space(5);
				if (wireTab){
					MGUI.Space4();
					MGUI.ToggleGroup(_WireframeToggle.floatValue == 0);
					me.ShaderProperty(_WFMode, "Pattern");
					me.ShaderProperty(_WFColor, "Color");
					me.ShaderProperty(_WFVisibility, "Wire Opacity");
					me.ShaderProperty(_WFFill, "Fill Opacity");
					me.ShaderProperty(_PatternMult, "Transition Multiplier");
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}

				MGUI.ToggleGroupEnd();
				MGUI.Space6();
			}
		}

		// -----------------
		// Presets
		// -----------------
		if (Foldouts.DoFoldout(foldouts, mat, me, 8f, "PRESETS")){
			MGUI.Space4();
			float buttonWidth = EditorGUIUtility.labelWidth-5.0f;
			if (MGUI.SimpleButton("Save", buttonWidth, 0)){
				presetText = MGUI.ReplaceInvalidChars(presetText);
				string filePath = dirPath + presetText + ".mat";
				Material newMat = new Material(mat);
				AssetDatabase.CreateAsset(newMat, filePath);
				AssetDatabase.Refresh();
				GUIUtility.keyboardControl = 0;
     			GUIUtility.hotControl = 0;
				presetText = "";
				popupIndex = -1;
			}
			GUILayout.Space(-17);

			// Text area
			Rect r = EditorGUILayout.GetControlRect();
			r.x += EditorGUIUtility.labelWidth;
			r.width = MGUI.GetPropertyWidth();
			presetText = EditorGUI.TextArea(r, presetText);
			
			// Locate button
			if (MGUI.SimpleButton("Locate", buttonWidth, 0) && popupIndex != -1){
				string filePath = dirPath + presets[popupIndex]+".mat";
				EditorUtility.FocusProjectWindow();
				Selection.activeObject = AssetDatabase.LoadAssetAtPath(filePath, typeof(Material));
			}
			GUILayout.Space(-17);

			// Popup list
			r = EditorGUILayout.GetControlRect();
			r.x += EditorGUIUtility.labelWidth;
			r.width = MGUI.GetPropertyWidth();
			popupIndex = EditorGUI.Popup(r, popupIndex, presets);

			// Apply button
			GUILayout.Space(-GUILayoutUtility.GetLastRect().height);
			if (MGUI.SimpleButton("Apply", r.width, r.x-14f) && popupIndex != -1){
				string presetPath = dirPath + presets[popupIndex] + ".mat";
				Material selectedMat = (Material)AssetDatabase.LoadAssetAtPath(presetPath, typeof(Material));
				mat.CopyPropertiesFromMaterial(selectedMat);
				popupIndex = -1;
			}
		}
		GUILayout.Space(15);

		// Debugging
		// bool debugTab = Foldouts.DoFoldout(foldouts, mat, me, -20f, "DEBUG");
		// if (MGUI.TabButton(resetIcon, 26f)){
		// 	DoDebugReset();
		// }
		// MGUI.Space8();
		// if (debugTab){
		// 	MGUI.Space6();
		// 	me.ShaderProperty(_DebugEnum, "Enum");
		// 	me.ShaderProperty(_DebugVector, "Vector");
		// 	me.ShaderProperty(_DebugColor, "Color");
		// 	me.ShaderProperty(_DebugHDRColor, "HDR Color");
		// 	me.ShaderProperty(_DebugFloat, "Float");
		// 	me.ShaderProperty(_DebugRange, "Range");
		// 	me.ShaderProperty(_DebugIntRange, "Int Range");
		// 	me.ShaderProperty(_DebugToggle, "Toggle");
		// }
		// GUILayout.Space(15);

		MGUI.CenteredTexture(watermarkTex, 0, 0);
		float buttonSize = 24.0f;
		float xPos = 53.0f;
		GUILayout.Space(-buttonSize);
		if (MGUI.LinkButton(patIconTex, buttonSize, buttonSize, xPos)){
			Application.OpenURL("https://www.patreon.com/mochieshaders");
		}
		GUILayout.Space(buttonSize);
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

	public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
		m_FirstTimeApply = true;
		if (mat.HasProperty("_Emission"))
			mat.SetColor("_EmissionColor", mat.GetColor("_Emission"));
		base.AssignNewShaderToMaterial(mat, oldShader, newShader);
		MGUI.SetBlendMode(mat, (MGUI.BlendMode)mat.GetFloat("_BlendMode"));
	}

	void CopyToSheet1(){
		_Spritesheet.textureValue = _Spritesheet1.textureValue;
		_Spritesheet.colorValue = _Spritesheet1.colorValue;
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
		_Shadows.floatValue = 1f;
		_ShadowStr.floatValue = 1f;
		_ShadowMode.floatValue = 0f;
		_RampWidth0.floatValue = 1f;
		_RampWidth1.floatValue = 1f;
		_RampWeight.floatValue = 0f;
		_RTSelfShadow.floatValue = 1f;
		_AttenSmoothing.floatValue = 0f;
		_ShadowDithering.floatValue = 0f;
		_ShadowConditions.floatValue = 1f;
		_DirectAO.floatValue = 0f;
	}

	void DoToonLighting(){
		_DisneyDiffuse.floatValue = 0.5f;
		_SHStr.floatValue = 0.1f;
		_NonlinearSHToggle.floatValue = 1f;
		_DirectCont.floatValue = 0.6f;
		_IndirectCont.floatValue = 0.5f;
		_RTDirectCont.floatValue = 1f;
		_RTIndirectCont.floatValue = 1f;
		_ClampAdditive.floatValue = 1f;
		_ColorPreservation.floatValue = 1f;
		_Shadows.floatValue = 1f;
		_ShadowStr.floatValue = 1f;
		_ShadowMode.floatValue = 0f;
		_RampWidth0.floatValue = 0.005f;
		_RampWidth1.floatValue = 0.5f;
		_RampWeight.floatValue = 0f;
		_RTSelfShadow.floatValue = 1f;
		_AttenSmoothing.floatValue = 1f;
		_ShadowDithering.floatValue = 0f;
		_ShadowConditions.floatValue = 0f;
		_DirectAO.floatValue = 1f;
		_IndirectAO.floatValue = 0f;
	}

	void DoFullReset(Material mat){
		DoTextureMapReset();
		DoShadingReset(mat);
		DoEmissionReset();
		DoFiltersReset();
		DoSpriteReset();
		DoUVDReset();
		DoOutlineReset();
		DoSpecialReset();
		_RenderMode.floatValue = 1f;
		_CubeMode.floatValue = 0f;
		_ColorPreservation.floatValue = 1f;
	}

	void DoShadingReset(Material mat){
		DoLightingReset();
		DoMaskingReset();
		DoReflReset();
		DoSpecReset();
		DoMatcapReset();
		DoShadowReset();
		DoSubsurfReset();
		DoRimReset();
		DoNormalReset();
		DoERimReset();
		_EnvironmentRim.floatValue = 0f;
		_Reflections.floatValue = 0f;
		_Specular.floatValue = 0f;
		_MatcapToggle.floatValue = 0f;
		_RoughnessFiltering.floatValue = 0f;
		_AOFiltering.floatValue = 0f;
		_HeightFiltering.floatValue = 0f;
		_Subsurface.floatValue = 0f;
		_RimLighting.floatValue = 0f;
		_Shadows.floatValue = 1f;
	}

	void DoTextureMapReset(){
		_Metallic.floatValue = 0f;
		_Glossiness.floatValue = 0.5f;
		_OcclusionStrength.floatValue = 1f;
		_Parallax.floatValue = 0.02f;
		_BumpScale.floatValue = 1f;
		_DetailNormalMapScale.floatValue = 1f;
		_GlossMapScale.floatValue = 1f;
		_MetallicGlossMap.textureValue = null;
		_SpecGlossMap.textureValue = null;
		_BumpMap.textureValue = null;
		_OcclusionMap.textureValue = null;
		_ParallaxMap.textureValue = null;
		_DetailAlbedoMap.textureValue = null;
		_DetailNormalMap.textureValue = null;
		_SmoothnessMap.textureValue = null;
	}

	void DoRoughFilterReset(){
		_PreviewRough.floatValue = 0f;
		_RoughLightness.floatValue = 0;
		_RoughIntensity.floatValue = 0;
		_RoughContrast.floatValue = 1;
	}

	void DoSmoothFilterReset(){
		_PreviewSmooth.floatValue = 0f;
		_LinearSmooth.floatValue = 0f;
		_SmoothLightness.floatValue = 0;
		_SmoothIntensity.floatValue = 0;
		_SmoothContrast.floatValue = 1;
	}

	void DoAOFilterReset(){
		_AOTintTex.textureValue = null;
		_PreviewAO.floatValue = 0f;
		_AOTint.colorValue = new Color(0,0,0,1);
		_AOLightness.floatValue = 0;
		_AOIntensity.floatValue = 0;
		_AOContrast.floatValue = 1;
	}

	void DoHeightFilterReset(){
		_PreviewHeight.floatValue = 0f;
		_HeightLightness.floatValue = 0;
		_HeightIntensity.floatValue = 0;
		_HeightContrast.floatValue = 1;
	}

	void DoLightingReset(){
		_StaticLightDirToggle.floatValue = 0f;
		_StaticLightDir.vectorValue = new Vector4(0, 0.75f, 1, 0);
		_DisneyDiffuse.floatValue = 0f;
		_SHStr.floatValue = 0.1f;
		_NonlinearSHToggle.floatValue = 1f;
		_VLightCont.floatValue = 1f;
		_RTDirectCont.floatValue = 1f;
		_RTIndirectCont.floatValue = 1f;
		_DirectCont.floatValue = 0.6f;
		_IndirectCont.floatValue = 0.5f;
		_AdditiveMax.floatValue = 1f;
		_ClampAdditive.floatValue = 1f;
		_DirectAO.floatValue = 1f;
		_IndirectAO.floatValue = 0f;
	}

	void DoMaskingReset(){
		_MaskingMode.floatValue = 0f;
		_PackedMask0.textureValue = null;
		_PackedMask1.textureValue = null;
		_ReflectionMask.textureValue = null;
		_SpecularMask.textureValue = null;
		_MatcapMask.textureValue = null;
		_ShadowMask.textureValue = null;
		_RimMask.textureValue = null;
		_DDMask.textureValue = null;
		_SmoothShadeMask.textureValue = null;
		_ReflectionMaskChannel.floatValue = 0f;
		_SpecularMaskChannel.floatValue = 0f;
		_MatcapMaskChannel.floatValue = 0f;
		_ShadowMaskChannel.floatValue = 0f;
		_RimMaskChannel.floatValue = 0f;
		_DDMaskChannel.floatValue = 0f;
		_SmoothShadeMaskChannel.floatValue = 0f;
	}

	void DoReflReset(){
		_ReflCubeFallback.floatValue = 0f;
		_ReflCube.textureValue = null;
		_ReflTex.textureValue = null;
		_ReflectionStr.floatValue = 1f;
		_ReflCol.colorValue = Color.white;
		_SSR.floatValue = 0f;
		_Alpha.floatValue = 1f;
		_MaxSteps.floatValue = 50f;
		_Step.floatValue = 0.09f;
		_LRad.floatValue = 0.2f;
		_SRad.floatValue = 0.02f;
		_EdgeFade.floatValue = 0.1f;
	}
	
	void DoSpecReset(){
		_SpecTex.textureValue = null;
		_InterpMask.textureValue = null;
		_SpecularStyle.floatValue = 0f;
		_SpecStr.floatValue = 1f;
		_SpecCol.colorValue = Color.white;
		_SharpSpecular.floatValue = 0f;
		_AnisoAngleX.floatValue = 1f;
		_AnisoAngleY.floatValue = 0.05f;
		_AnisoLayerX.floatValue = 2f;
		_AnisoLayerY.floatValue = 10f;
		_AnisoLayerStr.floatValue = 0.1f;
		_AnisoLerp.floatValue = 0f;
		_SharpSpecStr.floatValue = 0f;
	}

	void DoMatcapReset(){
		_Matcap.textureValue = null;
		_MatcapStr.floatValue = 1f;
		_MatcapColor.colorValue = Color.white;
		_UnlitMatcap.floatValue = 0f;
		_MatcapBlending.floatValue = 2f;
	}

	void DoShadowReset(){
		string rp = MGUI.presetPath+"/Textures/Ramps/DefaultRamp.png";
		_ShadowRamp.textureValue = File.Exists(rp) ? (Texture)EditorGUIUtility.Load(rp) : null;
		_ShadowTint.colorValue = new Color(0,0,0,1);
		_ShadowStr.floatValue = 1f;
		_RampWidth0.floatValue = 0.005f;
		_RampWidth1.floatValue = 0.5f;
		_RampWeight.floatValue = 0f;
		_ShadowDitherStr.floatValue = 0.3f;
		_ShadowDithering.floatValue = 0f;
		_RTSelfShadow.floatValue = 1f;
		_AttenSmoothing.floatValue = 1f;
		_ShadowMode.floatValue = 0f;
		_MainTexTint.floatValue = 0f;
		_RampPos.floatValue = 0f;
	}

	void DoSubsurfReset(){
		_TranslucencyMap.textureValue = null;
		_SubsurfaceTex.textureValue = null;
		_SubsurfaceMask.textureValue = null;
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
		_ERimBlending.floatValue = 0f;
		_ERimStr.floatValue = 1f;
		_ERimWidth.floatValue = 0.5f;
		_ERimEdge.floatValue = 0f;
		_ERimTex.textureValue = null;
		_ERimUseRough.floatValue = 1f;
		_ERimRoughness.floatValue = 0.5f;
	}

	void DoNormalReset(){
		_HardenNormals.floatValue = 0f;
		_ClearCoat.floatValue = 0f;
		_InvertNormalY0.floatValue = 0f;
		_InvertNormalY1.floatValue = 0f;
	}

	void DoEmissionReset(){
		_EmissionMap.textureValue = null;
		_EmissMask.textureValue = null;
		_EmissionColor.colorValue = new Color(0,0,0,1);
		_ReactToggle.floatValue = 0f;
		_PulseToggle.floatValue = 0f;
		DoLRReset();
		DoPulseReset();
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
		_PulseMask.textureValue = null;
		_PulseMaskChannel.floatValue = 0f;
	}

	void DoFiltersReset(){
		_FilterMask.textureValue = null;
		_FilterMaskChannel.textureValue = null;
		_FilterModel.floatValue = 0f;
		_TeamColorMask.textureValue = null;
		_RAmt.floatValue = 1f;
		_GAmt.floatValue = 1f;
		_BAmt.floatValue = 1f;
		_HDR.floatValue = 0f;
		_Contrast.floatValue = 1f;
		_Saturation.floatValue = 1f;
		_Brightness.floatValue = 0f;
		_AutoShift.floatValue = 0f;
		_AutoShiftSpeed.floatValue = 0.25f;
		_Hue.floatValue = 0f;
		_Luminance.floatValue = 0f;
		_HSLMin.floatValue = 0f;
		_HSLMax.floatValue = 1f;
		_TeamColor0.colorValue = Color.white;
		_TeamColor1.colorValue = Color.white;
		_TeamColor2.colorValue = Color.white;
		_TeamColor3.colorValue = Color.white;
		_PostFiltering.floatValue = 0f;
		_Value.floatValue = 0f;
	}

	void DoSpriteReset(){
		_EnableSpritesheet.floatValue = 0f;
		_EnableSpritesheet1.floatValue = 0f;
		_SpritesheetMask.textureValue = null;
		_SpritesheetMaskChannel.floatValue = 0f;
		_UnlitSpritesheet.floatValue = 0f;
		DoSheet1Reset();
		DoSheet2Reset();
	}

	void DoSheet1Reset(){
		_SpritesheetBlending.floatValue = 2f;
		_SpritesheetCol.colorValue = Color.white;
		_RowsColumns.vectorValue = new Vector4(2,2,0,0);
		_FrameClipOfs.vectorValue = new Vector4(0,0,0,0);
		_SpritesheetPos.vectorValue = new Vector4(0,0,0,0);
		_SpritesheetScale.vectorValue = new Vector4(1,1,0,0);
		_SpritesheetRot.floatValue = 0f;
		_FPS.floatValue = 30f;
		_ManualScrub.floatValue = 0f;
		_ScrubPos.floatValue = 1f;
	}

	void DoSheet2Reset(){
		_SpritesheetBlending1.floatValue = 2f;
		_SpritesheetCol1.colorValue = Color.white;
		_RowsColumns1.vectorValue = new Vector4(2,2,0,0);
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
		_DistortUVMaskChannel.floatValue = 0f;
		_DistortionStyle.floatValue = 0f;
		_PreviewNoise.floatValue = 0f;
		_NoiseScale.vectorValue = new Vector4(1,1,0,0);
		_NoiseMinMax.vectorValue = new Vector4(-1,1,0,0);
		_NoiseSpeed.floatValue = 0.5f;
		_NoiseOctaves.floatValue = 1f;
	}

	void DoOutlineReset(){
		_OutlineMask.textureValue = null;
		_OutlineMaskChannel.floatValue = 0f;
		_OutlineTex.textureValue = null;
		_Outline.floatValue = 0f;
		_OutlineCol.colorValue = new Color(0.75f, 0.75f, 0.75f, 1);
		_OutlineThicc.floatValue = 0.1f;
		_OutlineRange.floatValue = 0f;
		_ApplyOutlineLighting.floatValue = 0f;
		_ApplyOutlineEmiss.floatValue = 0f;
	}

	void DoSpecialReset(){
		_GeomFXToggle.floatValue = 0f;
		_DisguiseMain.floatValue = 0f;
		_DistanceFadeToggle.floatValue = 0f;
		_DissolveToggle.floatValue = 0f;
		_Screenspace.floatValue = 0f;
		_GlitchToggle.floatValue = 0f;
		_ShatterToggle.floatValue = 0f;
		_WireframeToggle.floatValue = 0f;
		DoDFReset();
		DoDissolveReset();
		DoScreenReset();
		DoMeshReset();
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
		_DissolveAmount.floatValue = 0f;
		_DissolveBlending.floatValue = 0f;
		_DissolveBlendSpeed.floatValue = 0.2f;
		_DissolveMask.textureValue = null;
		_DissolveMaskChannel.floatValue = 0f;
		_DissolveRimCol.colorValue = Color.white;
		_DissolveRimWidth.floatValue = 0.5f;
	}

	void DoScreenReset(){
		_Range.floatValue = 10f;
		_Position.vectorValue = new Vector4(0,0,0.25f,0);
		_Rotation.vectorValue = new Vector4(0,0,0,0);
	}

	void DoMeshReset(){
		_BaseOffset.vectorValue = new Vector4(0,0,0,0);
		_BaseRotation.vectorValue = new Vector4(0,0,0,0);
		_ReflOffset.vectorValue = new Vector4(0,0,0,0);
		_ReflRotation.vectorValue = new Vector4(0,0,0,0);
		_ShowBase.floatValue = 1f;
		_Connected.floatValue = 1f;
		_ShowInMirror.floatValue = 1f;
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
		_CloneSpacing.floatValue = 0f;
	}

	void DoGlitchReset(){
		_Instability.floatValue = 0f;
		_GlitchIntensity.floatValue = 0f;
		_GlitchFrequency.floatValue = 0f;
	}

	void DoShatterReset(){
		_ShatterSpread.floatValue = 0.347f;
		_ShatterMin.floatValue = 0.25f;
		_ShatterMax.floatValue = 0.65f;
		_ShatterCull.floatValue = 0.535f;
	}

	void DoWireframeReset(){
		_WFMode.floatValue = 0f;
		_WFColor.colorValue = new Color(0,0,0,1);
		_WFVisibility.floatValue = 1f;
		_WFFill.floatValue = 0f;
		_PatternMult.floatValue = 2.5f;
	}

	void DoDebugReset(){
		_DebugEnum.floatValue = 0f;
		_DebugVector.vectorValue = new Vector4(0,0,0,0);
		_DebugColor.colorValue = Color.white;
		_DebugHDRColor.colorValue = Color.white;
		_DebugFloat.floatValue = 1f;
		_DebugRange.floatValue = 1f;
		_DebugIntRange.floatValue = 1f;
		_DebugToggle.floatValue = 0f;
	}
}