using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

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
			false, false, false, false
		},
		new string[] {
			"BASE", 
			"SHADING", 
			"Masking", 
			"Lighting",
			"Shadows",
			"Reflections",
			"Specular",
			"Subsurface",
			"Matcap",
			"Rim Light",
			"Roughness Filter",
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
			"PRESETS"
		}
	);

	public static List<string> presetsList = new List<string>();
	public static string[] presets;

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
	public static readonly Vector4[] clonePositions = {
		new Vector4(1,0,0,1),
		new Vector4(-1,0,0,1),
		new Vector4(0,0,1,1),
		new Vector4(0,0,-1,1),
		new Vector4(0.5f,0,0.5f,1),
		new Vector4(-0.5f,0,0.5f,1),
		new Vector4(0.5f,0,-0.5f,1),
		new Vector4(-0.5f,0,-0.5f,1)
	};


	string MaskLabel = "Mask";
    static GUIContent MainTexLabel = new GUIContent("Main Texture");
    static GUIContent AlbedoLabel = new GUIContent("Albedo");
    static GUIContent EmissTexLabel = new GUIContent("Emission Map");
    static GUIContent NormalTexLabel = new GUIContent("Normal");
    static GUIContent MetallicTexLabel = new GUIContent("Metallic");
    static GUIContent RoughnessTexLabel = new GUIContent("Roughness");
    static GUIContent OcclusionMapLabel = new GUIContent("Occlusion");
    static GUIContent HeightTexLabel = new GUIContent("Height");
    static GUIContent ReflCubeLabel = new GUIContent("Cubemap");
    static GUIContent ShadowRampLabel = new GUIContent("Ramp");
    static GUIContent SpecularTexLabel = new GUIContent("Specular Map");
    static GUIContent PrimaryMapsLabel = new GUIContent("Primary Maps");
    static GUIContent DetailMapsLabel = new GUIContent("Detail Maps");
	static GUIContent DissolveTexLabel = new GUIContent("Dissolve Map");
	static GUIContent DissolveRimTexLabel = new GUIContent("Rim Color");
	static GUIContent ColorLabel = new GUIContent("Color");
	static GUIContent PackedTexLabel = new GUIContent("Packed Texture");
	static GUIContent Cubemap0Label = new GUIContent("Cubemap 0");
	static GUIContent Cubemap1Label = new GUIContent("Cubemap 1");
	static GUIContent TranslucLabel = new GUIContent("Thickness Map");

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
    MaterialProperty _SaturationRGB = null; 
    MaterialProperty _Contrast = null; 
    MaterialProperty _RAmt = null;
    MaterialProperty _GAmt = null;
    MaterialProperty _BAmt = null;
    MaterialProperty _Hue = null;
    MaterialProperty _SaturationHSL = null;
    MaterialProperty _Luminance = null;
    MaterialProperty _HSLMin = null;
    MaterialProperty _HSLMax = null;
    MaterialProperty _FilterModel = null;
    MaterialProperty _AutoShift = null;
    MaterialProperty _AutoShiftSpeed = null;
    MaterialProperty _Brightness = null;
    MaterialProperty _HDR = null;
    MaterialProperty _Noise = null;
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
	MaterialProperty _MainTexCube1 = null;
	MaterialProperty _CubeMode = null;
	MaterialProperty _CubeBlend = null;
	MaterialProperty _CubeRotate0 = null;
	MaterialProperty _CubeRotate1 = null;
	MaterialProperty _UnlitCube = null;
	MaterialProperty _AutoRotate0 = null;
	MaterialProperty _AutoRotate1 = null;
	MaterialProperty _CubeColor0 = null;
	MaterialProperty _CubeColor1 = null;
	MaterialProperty _CubeBlendMode = null;
	MaterialProperty _CubeBlendMask = null;
	MaterialProperty _CubeBlendMaskChannel = null;
	MaterialProperty _IsCubeBlendMask = null;
	MaterialProperty _UnlitMatcap = null;

	// PBR/Toon Shading
    MaterialProperty _PBRWorkflow = null;
    MaterialProperty _SourceAlpha = null;
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
    MaterialProperty _UseReflCube = null; 
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
	MaterialProperty _HeightChannel = null;
	MaterialProperty _InvertNormalY0 = null;
	MaterialProperty _InvertNormalY1 = null;
	MaterialProperty _RoughnessAdjust = null;
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

	// Toon Shading
	MaterialProperty _NonlinearSHToggle = null;
	MaterialProperty _SHStr = null;
	MaterialProperty _ColorPreservation = null;
	MaterialProperty _LinearIntRamp = null;
	MaterialProperty _MaskingToggle = null;
	MaterialProperty _ClearCoat = null;
	MaterialProperty _ReflectionMask = null;
	MaterialProperty _ReflectionMaskChannel = null;
	MaterialProperty _ReflectionStr = null;	
	MaterialProperty _DisneyDiffuse = null;
    MaterialProperty _SharpSpecular = null;
    MaterialProperty _SpecularMask = null;
	MaterialProperty _SpecularMaskChannel = null;
    MaterialProperty _SpecStr = null; 
    MaterialProperty _EnableShadowRamp = null;
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
	MaterialProperty _DirectContSTD = null;
	MaterialProperty _IndirectContSTD = null;
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

	MaterialProperty _NaNxddddd = null;

    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

	bool m_FirstTimeApply = true;

    MaterialEditor m_me;
    public override void OnGUI(MaterialEditor me, MaterialProperty[] props) {
		Material mat = (Material)me.target;
		mat.DisableKeyword("_");
		if (m_FirstTimeApply) {
			SetMaterialKeywords(mat);
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

		_NaNxddddd.floatValue = 0.0f;

		// Generate preset popup items (and folders if necessary)
		if (!AssetDatabase.IsValidFolder(MGUI.parentPath))
			AssetDatabase.CreateFolder(MGUI.presetPath, "Presets");
		if (!AssetDatabase.IsValidFolder(MGUI.parentPath+"/Uber"))
			AssetDatabase.CreateFolder(MGUI.parentPath, "Uber");
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

		MaterialProperty[] cloneProps = {_Clone1, _Clone2, _Clone3, _Clone4, _Clone5, _Clone6, _Clone7, _Clone8};

        Texture2D headerTex = (Texture2D)Resources.Load(header, typeof(Texture2D));
		Texture2D watermarkTex = (Texture2D)Resources.Load(watermark, typeof(Texture2D));
		Texture2D patIconTex = (Texture2D)Resources.Load(patIcon, typeof(Texture2D));
        MGUI.CenteredTexture(headerTex, 0, 0);
		MGUI.DoCollapseButtons(foldouts, mat);

		// Force backface culling off if using screenspace mesh
		if (isUberX && _Screenspace.floatValue == 1 && _CullingMode.floatValue != 0){
			_CullingMode.floatValue = 0;
		}

		bool isPBR = _RenderMode.floatValue == 2;
		bool isToon = _RenderMode.floatValue == 1;
		bool isPacked = _PBRWorkflow.floatValue == 2 && isPBR;

		// Setting floats based on render mode/texture presence/etc.
		if (isToon){
			if (_ShadowRamp.textureValue) _EnableShadowRamp.floatValue = 1;
			else _EnableShadowRamp.floatValue = 0;
		}
		if (_ReflCube.textureValue) _UseReflCube.floatValue = 1;
		else _UseReflCube.floatValue = 0;
		if (_Connected.floatValue == 1){
			_ReflOffset.vectorValue = _BaseOffset.vectorValue;
			_ReflRotation.vectorValue = _BaseRotation.vectorValue;
		}
		if (_CubeBlendMask.textureValue)
			_IsCubeBlendMask.floatValue = 1;
		else
			_IsCubeBlendMask.floatValue = 0;

		// -----------------
		// Base Settings
		// -----------------
		if (Foldouts.DoFoldout(foldouts, mat, me, "BASE")){
			MGUI.Space4();
			me.RenderQueueField();
			me.ShaderProperty(_RenderMode, "Shading Mode");
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
			if (!isPBR && isCutout)
				me.ShaderProperty(_ATM, "Alpha To Mask");
			me.ShaderProperty(_CullingMode, "Backface Culling");
			me.ShaderProperty(_CubeMode, "Main Texture Type");
			if ((int)_CubeMode.floatValue > 0)
				me.ShaderProperty(_UnlitCube, "Unlit Cubemap");
			if (isCutout && _ATM.floatValue == 0){
				MGUI.Space8();
				me.ShaderProperty(_Cutoff, "Cutout");
			}
			GUILayout.Space(16);
			switch((int)_CubeMode.floatValue){

				// Tex Only
				case 0: 
					me.TexturePropertySingleLine(MainTexLabel, _MainTex, _Color, _RenderMode.floatValue < 2 ? _ColorPreservation : null);
					if (_RenderMode.floatValue < 2) MGUI.TexPropLabel("Preserve Color", 144);
					MGUI.TextureSOScroll(me, _MainTex, _MainTexScroll);
					break;
				
				// Cubemap Only
				case 1: 
					me.TexturePropertySingleLine(ReflCubeLabel, _MainTexCube0, _CubeColor0, _RenderMode.floatValue < 2 ? _ColorPreservation : null);
					if (_RenderMode.floatValue < 2) MGUI.TexPropLabel("Preserve Color", 144);
					MGUI.Space4();
					MGUI.Vector3Field(_CubeRotate0, "Rotation");
					me.ShaderProperty(_AutoRotate0, "Auto Rotate");
					break;
				
				// Tex and Cubemap
				case 2: 
					me.TexturePropertySingleLine(MainTexLabel, _MainTex, _Color, _RenderMode.floatValue < 2 ? _ColorPreservation : null);
					if (_RenderMode.floatValue < 2) MGUI.TexPropLabel("Preserve Color", 144);
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

				// Double Cubemap
				case 3:
					me.TexturePropertySingleLine(Cubemap0Label, _MainTexCube0, _CubeColor0, _RenderMode.floatValue < 2 ? _ColorPreservation : null);
					if (_RenderMode.floatValue < 2) MGUI.TexPropLabel("Preserve Color", 144);
					MGUI.Space4();
					MGUI.Vector3Field(_CubeRotate0, "Rotation");
					me.ShaderProperty(_AutoRotate0, "Auto Rotate");

					GUILayout.Space(12);
					MGUI.MaskProperty(me, "Blend", _CubeBlendMask, _CubeBlend, _CubeBlendMaskChannel);
					GUILayout.Space(12);

					me.TexturePropertySingleLine(Cubemap1Label, _MainTexCube1, _CubeColor1, _CubeBlendMode);
					MGUI.TexPropLabel("Blending", 106);
					MGUI.Space4();
					MGUI.Vector3Field(_CubeRotate1, "Rotation");
					me.ShaderProperty(_AutoRotate1, "Auto Rotate");
					break;
				default: break;
			}
			MGUI.Space8();
		}

		// -----------------
		// Advanced Shading
		// -----------------
		mat.SetShaderPassEnabled("Always", _SSR.floatValue == 1 && _Reflections.floatValue == 1);
		if (isToon){
			if (Foldouts.DoFoldoutShading(foldouts, mat, me, new MaterialProperty[] {_SSR, _Reflections, _Specular, _MatcapToggle, _MatcapBlending}, "SHADING")){
				GUILayout.Label(PrimaryMapsLabel, EditorStyles.boldLabel);
				EditorGUI.BeginChangeCheck(); {
					me.TexturePropertySingleLine(MetallicTexLabel, _MetallicGlossMap, _MetallicGlossMap.textureValue ? null : _Metallic);
					me.TexturePropertySingleLine(RoughnessTexLabel, _SpecGlossMap, _SpecGlossMap.textureValue ? null : _Glossiness);
				}
				if (EditorGUI.EndChangeCheck())
					SetMaterialKeywords(mat);
				me.TexturePropertySingleLine(NormalTexLabel, _BumpMap, _BumpMap.textureValue ? _BumpScale : null);
				me.TexturePropertySingleLine(OcclusionMapLabel, _OcclusionMap, _OcclusionMap.textureValue ? _OcclusionStrength : null);
				EditorGUI.BeginChangeCheck();
				me.TexturePropertySingleLine(HeightTexLabel, _ParallaxMap, _ParallaxMap.textureValue ? _Parallax : null);
				if (EditorGUI.EndChangeCheck())
					SetMaterialKeywords(mat);
				
				if (_MaskingMode.floatValue == 0)
					MGUI.MaskProperty(me, "Detail Mask", _DetailMask, _DetailMaskChannel);
				GUILayout.Label(DetailMapsLabel, EditorStyles.boldLabel);
				me.TexturePropertySingleLine(AlbedoLabel, _DetailAlbedoMap);
				EditorGUI.BeginChangeCheck();
				me.TexturePropertySingleLine(NormalTexLabel, _DetailNormalMap, _DetailNormalMap.textureValue ? _DetailNormalMapScale : null);
				if (EditorGUI.EndChangeCheck())
					SetMaterialKeywords(mat);
				if (_DetailAlbedoMap.textureValue || _DetailNormalMap.textureValue)
					MGUI.TextureSOScroll(me, _DetailAlbedoMap, _DetailScroll);
				MGUI.Space8();

				// Lighting
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, "Lighting")){
					MGUI.Space6();
					if (MGUI.SimpleButton("Apply Standard Lighting", MGUI.GetInspectorWidth(), 0)){
						_DisneyDiffuse.floatValue = 1;
						_SHStr.floatValue = 1;
						_NonlinearSHToggle.floatValue = 0;
						_DirectCont.floatValue = 1;
						_IndirectCont.floatValue = 1;
						_ClampAdditive.floatValue = 0;
						_Reflections.floatValue = 1;
						_ReflectionStr.floatValue = 1;
						_ReflCol.colorValue = Color.white;
						_Specular.floatValue = 1;
						_SpecStr.floatValue = 1;
						_SpecCol.colorValue = Color.white;
						_SharpSpecular.floatValue = 0;
						_ColorPreservation.floatValue = 0;
						_Shadows.floatValue = 1;
						_ShadowStr.floatValue = 1;
						_ShadowRamp.textureValue = null;
						_RampWidth0.floatValue = 1;
						_RampWidth1.floatValue = 0;
						_RampWeight.floatValue = 0;
						_RTSelfShadow.floatValue = 1;
						_AttenSmoothing.floatValue = 0;
						_ShadowDithering.floatValue = 0;
						mat.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
						mat.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
					}
					MGUI.Space6();
					MGUI.ToggleVector3("Static Direction", _StaticLightDirToggle, _StaticLightDir);
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
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Masks
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, _MaskingToggle, "Masking")){
					MGUI.ToggleGroup(_MaskingToggle.floatValue == 0);
					MGUI.Space6();
					me.ShaderProperty(_MaskingMode, "Mode");
					MGUI.Space6();
					if (_MaskingMode.floatValue == 0){
						MGUI.MaskProperty(me, "Reflections", _ReflectionMask, _ReflectionMaskChannel, _Reflections.floatValue == 1);
						MGUI.MaskProperty(me, "Specular", _SpecularMask, _SpecularMaskChannel, _Specular.floatValue == 1);
						MGUI.MaskProperty(me, "Matcap", _MatcapMask, _MatcapMaskChannel, _MatcapToggle.floatValue == 1);
						MGUI.MaskProperty(me, "Shadows", _ShadowMask, _ShadowMaskChannel, _Shadows.floatValue == 1);
						MGUI.MaskProperty(me, "Rim", _RimMask, _RimMaskChannel, _RimLighting.floatValue == 1);
						MGUI.MaskProperty(me, "Disney Diffuse", _DDMask, _DDMaskChannel);
						MGUI.MaskProperty(me, "Spherical Harmonics", _SmoothShadeMask, _SmoothShadeMaskChannel);
					}
					else if (_MaskingMode.floatValue == 1){
						me.TexturePropertySingleLine(new GUIContent("Mask 0"), _PackedMask0);
						GUILayout.Label("Red:	Reflections + Specular\nGreen:	Matcap\nBlue:	Shadows");
						MGUI.Space8();
						me.TexturePropertySingleLine(new GUIContent("Mask 1"), _PackedMask1);
						GUILayout.Label("Red:	Rim Light\nGreen:	Detail\nBlue:	Diffuse Shading");
					}
					else if (_MaskingMode.floatValue == 2){
						me.TexturePropertySingleLine(new GUIContent("Mask 0"), _PackedMask0);
						GUILayout.Label("Red:	Reflections\nGreen:	Specular\nBlue:	Matcap\nAlpha:	Shadows");
						MGUI.Space8();
						me.TexturePropertySingleLine(new GUIContent("Mask 1"), _PackedMask1);
						GUILayout.Label("Red:	Rim Light\nGreen:	Detail\nBlue:	Disney Term\nAlpha:	Spherical Harmonics");
					}
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Reflections
				if (Foldouts.DoMediumFoldoutSSR(foldouts, mat, me, new MaterialProperty[] {_Reflections, _SSR}, "Reflections")){
					MGUI.Space6();
					MGUI.ToggleGroup(_Reflections.floatValue == 0);
					me.TexturePropertySingleLine(ReflCubeLabel, _ReflCube, _ReflCubeFallback);
					MGUI.TexPropLabel("Fallback", 105);
					me.ShaderProperty(_ReflCol, ColorLabel);
					me.ShaderProperty(_ReflectionStr, "Strength");
					me.ShaderProperty(_SSR, "SSR");
					if (_SSR.floatValue == 1 && _Reflections.floatValue == 1){
						MGUI.Space8();
						if (mat.renderQueue < 2501)
							EditorGUILayout.HelpBox("SSR requires a render queue of 2501 or above to function correctly.", MessageType.Error, true);
						EditorGUILayout.HelpBox("SSR requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs", MessageType.Warning, true);
						EditorGUILayout.HelpBox("This effect is very expensive, please use it sparingly!", MessageType.Warning, true);
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
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, _Specular, "Specular")){
					MGUI.Space6();
					MGUI.ToggleGroup(_Specular.floatValue == 0);
					me.ShaderProperty(_SpecularStyle, "Style");
					MGUI.Space6();
					me.ShaderProperty(_SpecCol, "Tint");
					me.ShaderProperty(_SpecStr, "Strength");
					me.ShaderProperty(_SharpSpecular, "Sharp");
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
				if (Foldouts.DoMediumFoldoutMatcap(foldouts, mat, me, new MaterialProperty[]{_MatcapToggle, _Specular, _Reflections, _MatcapBlending}, "Matcap")){
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
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, _Shadows, "Shadows")){
					MGUI.Space6();
					MGUI.ToggleGroup(_Shadows.floatValue == 0);
					me.TexturePropertySingleLine(ShadowRampLabel, _ShadowRamp, _ShadowRamp.textureValue ? _LinearIntRamp : null);
					if (_ShadowRamp.textureValue)
						MGUI.TexPropLabel("Antialias", 110);
					me.ShaderProperty(_ShadowStr, "Strength");
					MGUI.ToggleGroup(_ShadowRamp.textureValue);
					me.ShaderProperty(_RampWidth0, "Ramp 1");
					me.ShaderProperty(_RampWidth1, "Ramp 2");
					me.ShaderProperty(_RampWeight, "Ramp Blend");
					MGUI.ToggleSlider(me, "Dithering", _ShadowDithering, _ShadowDitherStr);
					me.ShaderProperty(_RTSelfShadow, "Dynamic Shadows");
					MGUI.ToggleGroup(_RTSelfShadow.floatValue == 0);
					me.ShaderProperty(_AttenSmoothing, "Smooth Attenuation");
					MGUI.ToggleGroupEnd();
					MGUI.ToggleGroupEnd();
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Subsurface Scattering
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, _Subsurface, "Subsurface")){
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
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, _RimLighting, "Rim Light")){
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

				// Roughness Adjustment
				if (_SpecGlossMap.textureValue){
					if (Foldouts.DoMediumFoldout(foldouts, mat, me, _RoughnessAdjust, "Roughness Filter")){
						MGUI.Space6();
						MGUI.ToggleGroup(_RoughnessAdjust.floatValue == 0);
						me.ShaderProperty(_RoughLightness, "Lightness");
						me.ShaderProperty(_RoughIntensity, "Intensity");
						me.ShaderProperty(_RoughContrast, "Contrast");
						MGUI.ToggleGroupEnd();
						MGUI.Space6();
					}
					else MGUI.SpaceN2();
				}

				// Normals
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, "Normals")){
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
		// PBR Shading
		// -----------------
		if (isPBR){    
			if (Foldouts.DoFoldout(foldouts, mat, me, "SHADING")){
				MGUI.Space4();
				me.ShaderProperty(_PBRWorkflow, "Workflow");
				GUILayout.Label(PrimaryMapsLabel, EditorStyles.boldLabel);
				switch ((int)_PBRWorkflow.floatValue){

					// Metallic
					case 0:
						EditorGUI.BeginChangeCheck(); {
							me.TexturePropertySingleLine(MetallicTexLabel, _MetallicGlossMap, _MetallicGlossMap.textureValue ? null : _Metallic);
							me.TexturePropertySingleLine(RoughnessTexLabel, _SpecGlossMap, _SpecGlossMap.textureValue ? null : _Glossiness);
						}
						if (EditorGUI.EndChangeCheck())
							SetMaterialKeywords(mat);
						break;

					// Specular
					case 1: 
						EditorGUI.BeginChangeCheck();
						me.TexturePropertySingleLine(SpecularTexLabel, _SpecGlossMap, !_SpecGlossMap.textureValue ? _SpecCol : null);
						if (EditorGUI.EndChangeCheck())
							SetMaterialKeywords(mat);
						if (_SpecGlossMap.textureValue)
							me.ShaderProperty(_GlossMapScale, "Smoothness", indent);
						else {
							if (_SourceAlpha.floatValue == 1)
								me.ShaderProperty(_GlossMapScale, "Smoothness", indent);
							else
								me.ShaderProperty(_Glossiness, "Smoothness", indent);
						}
						me.ShaderProperty(_SourceAlpha, "Source Alpha", indent);
						break;

					// Packed
					case 2:
						me.TexturePropertySingleLine(PackedTexLabel, _PackedMap);
						me.ShaderProperty(_MetallicChannel, "Metallic");
						me.ShaderProperty(_RoughnessChannel, "Roughness");
						me.ShaderProperty(_OcclusionChannel, "Occlusion");
						me.ShaderProperty(_HeightChannel, "Height");
						MGUI.Space8();
						me.ShaderProperty(_OcclusionStrength, "Occlusion Strength");
						me.ShaderProperty(_Parallax, "Height Strength");
						MGUI.Space8();
						break;
					default: break;
				}
				me.TexturePropertySingleLine(NormalTexLabel, _BumpMap, _BumpMap.textureValue ? _BumpScale : null);
				if (_PBRWorkflow.floatValue != 2){
					me.TexturePropertySingleLine(OcclusionMapLabel, _OcclusionMap, _OcclusionMap.textureValue ? _OcclusionStrength : null);
					EditorGUI.BeginChangeCheck();
					me.TexturePropertySingleLine(HeightTexLabel, _ParallaxMap, _ParallaxMap.textureValue ? _Parallax : null);
					if (EditorGUI.EndChangeCheck())
						SetMaterialKeywords(mat);
				}

				MGUI.MaskProperty(me, "Detail Mask", _DetailMask, _DetailMaskChannel);
				me.TexturePropertySingleLine(ReflCubeLabel, _ReflCube, _ReflCubeFallback);
				MGUI.TexPropLabel("Fallback", 105);

				GUILayout.Label(DetailMapsLabel, EditorStyles.boldLabel);
				me.TexturePropertySingleLine(AlbedoLabel, _DetailAlbedoMap);
				
				EditorGUI.BeginChangeCheck();
				me.TexturePropertySingleLine(NormalTexLabel, _DetailNormalMap, _DetailNormalMap.textureValue ? _DetailNormalMapScale : null);
				if (EditorGUI.EndChangeCheck())
					SetMaterialKeywords(mat);
				if (_DetailAlbedoMap.textureValue || _DetailNormalMap.textureValue)
					MGUI.TextureSOScroll(me, _DetailAlbedoMap, _DetailScroll);
				GUILayout.Label(new GUIContent("Settings"), EditorStyles.boldLabel);
				MGUI.SpaceN4();
				me.ShaderProperty(_Specular, "Specular Highlights");
				me.ShaderProperty(_Reflections, "Reflections");
				MGUI.Space8();

				// Lighting
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, "Lighting")){
					MGUI.Space6();
					MGUI.ToggleVector3("Static Direction", _StaticLightDirToggle, _StaticLightDir);
					GUILayout.Label("Realtime", EditorStyles.boldLabel);
					MGUI.SpaceN4();
					me.ShaderProperty(_RTDirectCont, "Direct Intensity");
					me.ShaderProperty(_RTIndirectCont, "Indirect Intensity");
					me.ShaderProperty(_VLightCont, "Vertex Intensity");
					MGUI.ToggleSlider(me, "Clamp Additive", _ClampAdditive, _AdditiveMax);
					GUILayout.Label("Baked", EditorStyles.boldLabel);
					MGUI.SpaceN4();
					me.ShaderProperty(_DirectContSTD, "Direct Intensity");
					me.ShaderProperty(_IndirectContSTD, "Indirect Intensity");
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Rim
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, _RimLighting, "Rim Light")){
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

				// Roughness Adjustment
				if (_SpecGlossMap.textureValue || _PBRWorkflow.floatValue == 2){
					if (Foldouts.DoMediumFoldout(foldouts, mat, me, _RoughnessAdjust, "Roughness Filter")){
						MGUI.Space6();
						MGUI.ToggleGroup(_RoughnessAdjust.floatValue == 0);
						me.ShaderProperty(_RoughLightness, "Lightness");
						me.ShaderProperty(_RoughIntensity, "Intensity");
						me.ShaderProperty(_RoughContrast, "Contrast");
						MGUI.ToggleGroupEnd();
						MGUI.Space6();
					}
					else MGUI.SpaceN2();
				}

				// Normals
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, "Normals")){
					MGUI.Space2();
					me.ShaderProperty(_HardenNormals, "Hard Edges");
					me.ShaderProperty(_ClearCoat, "Clearcoat Mode");
					me.ShaderProperty(_InvertNormalY0, "Invert Normal Map Y");
					me.ShaderProperty(_InvertNormalY1, "Invert Detail Normal Y");
				}
				MGUI.Space8();
			}
		}

		// -----------------
		// Emission
		// -----------------
		if (Foldouts.DoFoldout(foldouts, mat, me, "EMISSION")){
			MGUI.Space4();
			EditorGUI.BeginChangeCheck();
			me.ShaderProperty(_EmissionToggle, "Enable");
			if (EditorGUI.EndChangeCheck())
				SetMaterialKeywords(mat);
			MGUI.Space6();
			MGUI.ToggleGroup(_EmissionToggle.floatValue == 0);
			me.TexturePropertySingleLine(EmissTexLabel, _EmissionMap, _EmissionColor);
			if (_EmissionMap.textureValue){
				MGUI.TextureSOScroll(me, _EmissionMap, _EmissScroll);
				MGUI.Space8();
			}
			MGUI.MaskProperty(me, _EmissMask, _EmissMaskChannel);
			if (_EmissMask.textureValue){
				MGUI.TextureSO(me, _EmissMask);
			}
			MGUI.Space8();

			if (Foldouts.DoMediumFoldout(foldouts, mat, me, _ReactToggle, "Light Reactivity") && _EmissionToggle.floatValue == 1){
				MGUI.Space6();
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

			if (Foldouts.DoMediumFoldout(foldouts, mat, me, _PulseToggle, "Pulse") && _EmissionToggle.floatValue == 1){
				MGUI.Space6();
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
		if (Foldouts.DoFoldout(foldouts, mat, me, "FILTERS")){
			MGUI.Space4();
			me.ShaderProperty(_FilterModel, "Style");
			MGUI.Space8();
			switch ((int)_FilterModel.floatValue){

				// RGB
				case 1: 
					MGUI.MaskProperty(me, _FilterMask, _FilterMaskChannel);
					me.ShaderProperty(_RAmt, "Red");
					me.ShaderProperty(_GAmt, "Green");
					me.ShaderProperty(_BAmt, "Blue");
					me.ShaderProperty(_HDR, "HDR");
					me.ShaderProperty(_Contrast, "Contrast");
					me.ShaderProperty(_SaturationRGB, "Saturation");
					me.ShaderProperty(_Brightness, "Brightness");
					me.ShaderProperty(_Noise, "Noise");
					MGUI.Space8();
					break;
				
				// HSL
				case 2:
					MGUI.MaskProperty(me, _FilterMask, _FilterMaskChannel);
					me.ShaderProperty(_AutoShift, "Auto Shift");
					if (_AutoShift.floatValue == 1)
						me.ShaderProperty(_AutoShiftSpeed, "Speed");
					else
						me.ShaderProperty(_Hue, "Hue");
					me.ShaderProperty(_SaturationHSL, "Saturation");
					me.ShaderProperty(_Luminance, "Luminance");
					me.ShaderProperty(_HSLMin, "Min Threshold");
					me.ShaderProperty(_HSLMax, "Max Threshold");
					me.ShaderProperty(_HDR, "HDR");
					me.ShaderProperty(_Contrast, "Contrast");
					me.ShaderProperty(_Noise, "Noise");
					MGUI.Space8();
					break;

				// Team Colors
				case 3: 
					me.TexturePropertySingleLine(new GUIContent("Team Color Mask"), _TeamColorMask);
					me.ShaderProperty(_TeamColor0, "Red Channel");
					me.ShaderProperty(_TeamColor1, "Green Channel");
					me.ShaderProperty(_TeamColor2, "Blue Channel");
					me.ShaderProperty(_TeamColor3, "Alpha Channel");
					MGUI.Space8();
					MGUI.MaskProperty(me, "Filter Mask", _FilterMask, _FilterMaskChannel);
					me.ShaderProperty(_HDR, "HDR");
					me.ShaderProperty(_Contrast, "Contrast");
					me.ShaderProperty(_SaturationRGB, "Saturation");
					me.ShaderProperty(_Brightness, "Brightness");
					me.ShaderProperty(_Noise, "Noise");
					MGUI.Space8();
					break;
				default: break;
			}
		}

		// -----------------
		// UV Distortion
		// -----------------
		if (Foldouts.DoFoldout(foldouts, mat, me, "UV DISTORTION")){
			MGUI.Space8();
			me.TexturePropertySingleLine(NormalTexLabel, _DistortUVMap, _DistortUVMap.textureValue ? _DistortUVStr : null);
			MGUI.TextureSOScroll(me, _DistortUVMap, _DistortUVScroll);
			MGUI.Space8();
			MGUI.MaskProperty(me, _DistortUVMask, _DistortUVMaskChannel);
			MGUI.Space8();
			me.ShaderProperty(_DistortMainUV, "Main");
			me.ShaderProperty(_DistortDetailUV, "Detail");
			me.ShaderProperty(_DistortEmissUV, "Emission");
			me.ShaderProperty(_DistortRimUV, "Rim");
			MGUI.Space8();
		}

		// -----------------
		// Outline
		// -----------------
		if (Foldouts.DoFoldout(foldouts, mat, me, "OUTLINE")){
			if (!isTransparent){
				MGUI.Space4();
				me.ShaderProperty(_Outline, "Style");
				MGUI.Space8();
				if (_Outline.floatValue > 0){
					if (_CullingMode.floatValue == 2){
						MGUI.DisplayWarning("Outline will be visible behind single-sided geometry when backface culling is enabled.");
						MGUI.Space6();
					}
					me.ShaderProperty(_ApplyOutlineLighting, "Apply Lighting");
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
			}
			else MGUI.CenteredText("REQUIRES OPAQUE OR CUTOUT BLENDING", 11, 0, 4);
		}

		// -----------------
		// X Features
		// -----------------
		if (isUberX){
			if (Foldouts.DoFoldoutSpecial(foldouts, mat, me, new MaterialProperty[]{_BlendMode, _DistanceFadeToggle, _DissolveToggle}, "SPECIAL FEATURES")){
				MGUI.Space4();
				me.ShaderProperty(_GeomFXToggle, "Geometry Shader");
				MGUI.ToggleGroup(_GeomFXToggle.floatValue == 0);
				me.ShaderProperty(_DisguiseMain, "Affect Clones Only");
				MGUI.ToggleGroupEnd();
				MGUI.Space4();

				// Distance Fade 
				if (Foldouts.DoMediumFoldoutSpecial(foldouts, mat, me, new MaterialProperty[]{_DistanceFadeToggle, _BlendMode}, "Distance Fade")){
					MGUI.Space6();
					if (_DistanceFadeToggle.floatValue > 0){
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
					else MGUI.SpaceN8();
				}
				else MGUI.SpaceN2();

				// Dissolve
				if (Foldouts.DoMediumFoldoutSpecial(foldouts, mat, me, new MaterialProperty[]{_DissolveToggle, _BlendMode}, "Dissolve")){
					MGUI.Space6();
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
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, _Screenspace, "Screenspace")){
					MGUI.Space6();
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
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, "Mesh Manipulation")){
					MGUI.Space6();
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
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, "Clones") && _GeomFXToggle.floatValue == 1){
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
							DoCloneResetButton(cloneProps);
						}
						else {
							me.ShaderProperty(_CloneSpacing, "Spacing", 1);
						}
					}
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Glitch
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, _GlitchToggle, "Glitch") && _GeomFXToggle.floatValue == 1){
					MGUI.Space6();
					MGUI.ToggleGroup(_GlitchToggle.floatValue == 0);
					me.ShaderProperty(_Instability, "Instability");
					me.ShaderProperty(_GlitchIntensity, "Intensity");
					me.ShaderProperty(_GlitchFrequency, "Frequency");
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				// Shatter Culling
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, _ShatterToggle, "Shatter Culling") && _GeomFXToggle.floatValue == 1){
					MGUI.Space6();
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
				if (Foldouts.DoMediumFoldout(foldouts, mat, me, _WireframeToggle, "Wireframe") && _GeomFXToggle.floatValue == 1){
					MGUI.Space6();
					MGUI.ToggleGroup(_WireframeToggle.floatValue == 0);
					me.ShaderProperty(_WFMode, "Pattern");
					me.ShaderProperty(_WFColor, "Color");
					me.ShaderProperty(_WFVisibility, "Wire Opacity");
					me.ShaderProperty(_WFFill, "Fill Opacity");
					me.ShaderProperty(_PatternMult, "Transition Multiplier");
					MGUI.ToggleGroupEnd();
					MGUI.Space6();
				}
				else MGUI.SpaceN2();

				MGUI.ToggleGroupEnd();
				MGUI.Space6();
			}
		}

		// -----------------
		// Presets
		// -----------------
		if (Foldouts.DoFoldout(foldouts, mat, me, "PRESETS")){
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
		
		MGUI.CenteredTexture(watermarkTex, 0, 0);
		float buttonSize = 24.0f;
		float xPos = 53.0f;
		GUILayout.Space(-buttonSize);
		if (MGUI.LinkButton(patIconTex, buttonSize, buttonSize, xPos)){
			Application.OpenURL("https://www.patreon.com/mochieshaders");
		}
		GUILayout.Space(buttonSize);
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
		SetMaterialKeywords(mat);
	}

	void SetMaterialKeywords(Material mat) {
		SetKeyword(mat, "_SPECGLOSSMAP", mat.GetTexture("_SpecGlossMap"));
		SetKeyword(mat, "_METALLICGLOSSMAP", mat.GetTexture("_MetallicGlossMap"));
		SetKeyword(mat, "_PARALLAXMAP", mat.GetTexture("_ParallaxMap") || mat.GetTexture("_PackedMap"));
		SetKeyword(mat, "_DETAIL_MULX2", mat.GetTexture("_DetailNormalMap"));
	}

	void DoCloneResetButton(MaterialProperty[] props){
		if (MGUI.ResetButton()){
			int i = 0;
			foreach (MaterialProperty p in props){
				Vector4 v = USEditor.clonePositions[i];
				props[i].vectorValue = v;
				i++;
			};
		}
	}
}