using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Mochie;

public class WaterEditor : ShaderGUI {

    GUIContent texLabel = new GUIContent("Base Color");
    GUIContent normalLabel = new GUIContent("Normal Map");
	GUIContent flowLabel = new GUIContent("Flow Map");
	GUIContent noiseLabel = new GUIContent("Noise Texture");
	GUIContent foamLabel = new GUIContent("Foam Texture");
	GUIContent cubeLabel = new GUIContent("Cubemap");
	GUIContent emissLabel = new GUIContent("Emission Map");

	Dictionary<Action, GUIContent> baseTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> norm0TabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> norm1TabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> flowTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> vertTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> causticsTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> fogTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> foamTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> edgeFadeTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> reflSpecTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> rainTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> tessTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> emissTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> areaLitTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> renderingTabButtons = new Dictionary<Action, GUIContent>();

    static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
    Toggles toggles = new Toggles(new string[] {
			"BASE", 
			"NORMAL MAPS", 
			"SPECULARITY", 
			"FLOW MAPPING", 
			"VERTEX OFFSET",
			"CAUSTICS",
			"DEPTH FOG",
			"FOAM",
			"EDGE FADE",
			"RAIN",
			"TESSELLATION",
			"EMISSION",
			"AREALIT",
			"RENDER SETTINGS"
	}, 0);

    string header = "WaterHeader_Pro";
	string versionLabel = "v1.19";

	MaterialProperty _Color = null;
	MaterialProperty _NonGrabColor = null;
	MaterialProperty _AngleTint = null;
	MaterialProperty _BackfaceTint = null;
	MaterialProperty _MainTex = null;
	MaterialProperty _MainTexScroll = null;
	MaterialProperty _DistortionStrength = null;
	MaterialProperty _Roughness = null;
	MaterialProperty _Metallic = null;
	MaterialProperty _Opacity = null;
	MaterialProperty _Reflections = null;
	MaterialProperty _ReflStrength = null;
	MaterialProperty _Specular = null;
	MaterialProperty _SpecStrength = null;
	MaterialProperty _CullMode = null;
	MaterialProperty _ZWrite = null;
	MaterialProperty _BaseColorDistortionStrength = null;
	MaterialProperty _NormalMap0 = null;
	MaterialProperty _NormalStr0 = null;
	MaterialProperty _NormalMapScale0 = null;
	MaterialProperty _Rotation0 = null;
	MaterialProperty _NormalMapScroll0 = null;
	MaterialProperty _Normal1Toggle = null;
	MaterialProperty _NormalMap1 = null;
	MaterialProperty _NormalStr1 = null;
	MaterialProperty _NormalMapScale1 = null;
	MaterialProperty _Rotation1 = null;
	MaterialProperty _NormalMapScroll1 = null;
	MaterialProperty _FlowToggle = null;
	MaterialProperty _FlowMap = null;
	MaterialProperty _FlowSpeed = null;
	MaterialProperty _FlowStrength = null;
	MaterialProperty _FlowMapScale = null;
	MaterialProperty _NoiseTex = null;
	MaterialProperty _NoiseTexScale = null;
	MaterialProperty _NoiseTexScroll = null;
	MaterialProperty _NoiseTexBlur = null;
	MaterialProperty _WaveHeight = null;
	MaterialProperty _Offset = null;
	MaterialProperty _VertOffsetMode = null;
	MaterialProperty _WaveSpeedGlobal = null;
	MaterialProperty _WaveStrengthGlobal = null;
	MaterialProperty _WaveScaleGlobal = null;
	MaterialProperty _WaveSpeed0 = null;
	MaterialProperty _WaveSpeed1 = null;
	MaterialProperty _WaveSpeed2 = null;
	MaterialProperty _WaveScale0 = null;
	MaterialProperty _WaveScale1 = null;
	MaterialProperty _WaveScale2 = null;
	MaterialProperty _WaveStrength0 = null;
	MaterialProperty _WaveStrength1 = null;
	MaterialProperty _WaveStrength2 = null;
	MaterialProperty _WaveDirection0 = null;
	MaterialProperty _WaveDirection1 = null;
	MaterialProperty _WaveDirection2 = null;
	// MaterialProperty _Turbulence = null;
	// MaterialProperty _TurbulenceSpeed = null;
	// MaterialProperty _TurbulenceScale = null;
	MaterialProperty _VoronoiScale = null;
	MaterialProperty _VoronoiScroll = null;
	MaterialProperty _VoronoiWaveHeight = null;
	MaterialProperty _VoronoiOffset = null;
	MaterialProperty _VoronoiSpeed = null;
	MaterialProperty _CausticsTex = null;
	MaterialProperty _CausticsToggle = null;
	MaterialProperty _CausticsOpacity = null;
	MaterialProperty _CausticsScale = null;
	MaterialProperty _CausticsSpeed = null;
	MaterialProperty _CausticsFade = null;
	MaterialProperty _CausticsDisp = null;
	MaterialProperty _CausticsDistortion = null;
	MaterialProperty _CausticsDistortionTex = null;
	MaterialProperty _CausticsDistortionScale = null;
	MaterialProperty _CausticsDistortionSpeed = null;
	MaterialProperty _CausticsRotation = null;
	MaterialProperty _CausticsColor = null;
	MaterialProperty _CausticsPower = null;
	MaterialProperty _FogToggle = null;
	MaterialProperty _FogTint = null;
	MaterialProperty _FogPower = null;
	MaterialProperty _FoamToggle = null;
	MaterialProperty _FoamTex = null;
	MaterialProperty _FoamNoiseTex = null;
	MaterialProperty _FoamTexScale = null;
	MaterialProperty _FoamRoughness = null;
	MaterialProperty _FoamColor = null;
	MaterialProperty _FoamPower = null;
	MaterialProperty _FoamEdgeStrength = null;
	MaterialProperty _FoamCrestStrength = null;
	MaterialProperty _FoamCrestThreshold = null;
	MaterialProperty _FoamNoiseTexScroll = null;
	MaterialProperty _FoamNoiseTexCrestStrength = null;
	MaterialProperty _FoamNoiseTexStrength = null;
	MaterialProperty _FoamNoiseTexScale = null;
	MaterialProperty _EdgeFadeToggle = null;
	MaterialProperty _EdgeFadePower = null;
	MaterialProperty _EdgeFadeOffset = null;
	MaterialProperty _SSRStrength = null;
	MaterialProperty _SSR = null;
	MaterialProperty _EdgeFadeSSR = null;
	MaterialProperty _Normal0StochasticToggle = null;
	MaterialProperty _Normal1StochasticToggle = null;
	MaterialProperty _FoamStochasticToggle = null;
	MaterialProperty _FoamTexScroll = null;
	MaterialProperty _BaseColorStochasticToggle = null;
	// MaterialProperty _NormalMapOffset1 = null;
	// MaterialProperty _FoamOffset = null;
	// MaterialProperty _NormalMapOffset0 = null;
	// MaterialProperty _BaseColorOffset = null;
	MaterialProperty _FoamDistortionStrength = null;
	MaterialProperty _VertRemapMin = null;
	MaterialProperty _VertRemapMax = null;
	MaterialProperty _LightDir = null;
	MaterialProperty _SpecTint = null;
	MaterialProperty _ReflCube = null;
	MaterialProperty _ReflTint = null;
	MaterialProperty _ReflCubeRotation = null;
	MaterialProperty _RainToggle = null;
	MaterialProperty _RippleScale = null;
	MaterialProperty _RippleSpeed = null;
	MaterialProperty _RippleStr = null;
	MaterialProperty _FoamNormalToggle = null;
	MaterialProperty _FoamNormalStrength = null;
	MaterialProperty _DepthEffects = null;
	MaterialProperty _TessMin = null;
	MaterialProperty _TessMax = null;
	MaterialProperty _TessDistMin = null;
	MaterialProperty _TessDistMax = null;
	MaterialProperty _TessellationOffsetMask = null;
	MaterialProperty _BlendNoise = null;
	MaterialProperty _BlendNoiseScale = null;
	MaterialProperty _BlendNoiseSource = null;
	MaterialProperty _FlowMapUV = null;
	MaterialProperty _BackfaceReflections = null;
	MaterialProperty _RoughnessMap = null;
	MaterialProperty _MetallicMap = null;
	MaterialProperty _EmissionToggle = null;
	MaterialProperty _EmissionMap = null;
	MaterialProperty _EmissionMapStochasticToggle = null;
	MaterialProperty _EmissionColor = null;
	MaterialProperty _EmissionMapScroll = null;
	MaterialProperty _DetailBaseColor = null;
	MaterialProperty _DetailBaseColorTint = null;
	MaterialProperty _AreaLitToggle = null;
	MaterialProperty _AreaLitMask = null;
	MaterialProperty _AreaLitStrength = null;
	MaterialProperty _AreaLitRoughnessMult = null;
	MaterialProperty _LightMesh = null;
	MaterialProperty _LightTex0 = null;
	MaterialProperty _LightTex1 = null;
	MaterialProperty _LightTex2 = null;
	MaterialProperty _LightTex3 = null;
	MaterialProperty _OpaqueLights = null;
	MaterialProperty _TransparencyMode = null;
	MaterialProperty _DetailNormal = null;
	MaterialProperty _DetailNormalStrength = null;
	MaterialProperty _DetailTextureMode = null;
	MaterialProperty _DetailScroll = null;
	MaterialProperty _StencilRef = null;
	MaterialProperty _RecalculateNormals = null;
	MaterialProperty _ShadowStrength = null;
	MaterialProperty _SSRHeight = null;
	MaterialProperty _CausticsTexArray = null;
	MaterialProperty _NormalMapFlipbook = null;
	MaterialProperty _NormalMapFlipbookSpeed = null;
	MaterialProperty _NormalMapFlipbookStrength = null;
	MaterialProperty _NormalMapMode = null;
	MaterialProperty _VertOffsetFlipbook = null;
	MaterialProperty _VertOffsetFlipbookStrength = null;
	MaterialProperty _VertOffsetFlipbookSpeed = null;
	MaterialProperty _VertOffsetFlipbookScale = null;
	MaterialProperty _NormalMapFlipbookScale = null;
	MaterialProperty _CausticsFlipbookSpeed = null;
	MaterialProperty _RippleSize = null;
	MaterialProperty _RippleDensity = null;
	MaterialProperty _OpacityMask = null;
	MaterialProperty _OpacityMaskScroll = null;
	MaterialProperty _NonGrabBackfaceTint = null;
	MaterialProperty _EmissionDistortionStrength = null;
	MaterialProperty _FogBrightness = null;
	MaterialProperty _VertexOffsetMask = null;
	MaterialProperty _VertexOffsetMaskStrength = null;
	MaterialProperty _VertexOffsetMaskChannel = null;
	MaterialProperty _SubsurfaceTint = null;
	MaterialProperty _SubsurfaceThreshold = null;
	MaterialProperty _SubsurfaceBrightness = null;
	MaterialProperty _SubsurfaceStrength = null;
	MaterialProperty _TexCoordSpace = null;
	MaterialProperty _TexCoordSpaceSwizzle = null;
	MaterialProperty _GlobalTexCoordScaleUV = null;
	MaterialProperty _GlobalTexCoordScaleWorld = null;
	MaterialProperty _BicubicLightmapping = null;
	MaterialProperty _MirrorNormalOffsetSwizzle = null;
	MaterialProperty _InvertNormals = null;
	MaterialProperty _VisualizeFlowmap = null;
	MaterialProperty _AudioLink = null;
	MaterialProperty _AudioLinkStrength = null;
	MaterialProperty _AudioLinkBand = null;
	// MaterialProperty _FogTint2 = null;
	// MaterialProperty _FogPower2 = null;
	// MaterialProperty _FogBrightness2 = null;

	// MaterialProperty _NormalFlipbookStochasticToggle = null;
	
    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	bool m_FirstTimeApply = true;

	MaterialEditor me;

    public override void OnGUI(MaterialEditor matEditor, MaterialProperty[] props) {

		me = matEditor;

        if (!me.isVisible)
            return;

		ClearDictionaries();

        foreach (var property in GetType().GetFields(bindingFlags)){
            if (property.FieldType == typeof(MaterialProperty))
                property.SetValue(this, FindProperty(property.Name, props));
        }
        Material mat = (Material)me.target;
        if (m_FirstTimeApply){
			m_FirstTimeApply = false;
        }

		header = "WaterHeader_Pro";
		if (!EditorGUIUtility.isProSkin){
			header = "WaterHeader";
		}

        Texture2D headerTex = (Texture2D)Resources.Load(header, typeof(Texture2D));
		Texture2D collapseIcon = (Texture2D)Resources.Load("CollapseIcon", typeof(Texture2D));

        GUILayout.Label(headerTex);
		MGUI.Space4();

		if (!foldouts.ContainsKey(mat))
			foldouts.Add(mat, toggles);

        EditorGUI.BeginChangeCheck(); {
			
			int transMode = mat.GetInt("_TransparencyMode");
			bool isTessellated = MGUI.IsTessellated(mat);

            // Base
			baseTabButtons.Add(()=>{Toggles.CollapseFoldouts(mat, foldouts, 1);}, MGUI.collapseLabel);
			baseTabButtons.Add(()=>{ResetSurface();}, MGUI.resetLabel);
			Action surfaceTabAction = ()=>{
				MGUI.PropertyGroup(()=>{
					me.TexturePropertySingleLine(texLabel, _MainTex, _BaseColorStochasticToggle);
					MGUI.TexPropLabel(Tips.stochasticLabel, 117, false);
					if (_MainTex.textureValue){
						MGUI.TextureSOScroll(me, _MainTex, _MainTexScroll);
						// me.ShaderProperty(_BaseColorOffset, Tips.parallaxOffsetLabel);
						me.ShaderProperty(_BaseColorDistortionStrength, "Distortion Strength");
						MGUI.Space4();
					}
					if (transMode > 0){
						me.TexturePropertySingleLine(new GUIContent("Opacity"), _OpacityMask, _Opacity);
						MGUI.TextureSOScroll(me, _OpacityMask, _OpacityMaskScroll, _OpacityMask.textureValue);
					}
					else {
						me.ShaderProperty(_ShadowStrength, "Shadow Strength");
					}
				});
				MGUI.PropertyGroup(()=>{
					bool hasDetailBC = _DetailBaseColor.textureValue;
					bool hasDetailN = _DetailNormal.textureValue;
					me.TexturePropertySingleLine(new GUIContent("Decal Base Color"), _DetailBaseColor, hasDetailBC ? _DetailBaseColorTint : null);
					me.TexturePropertySingleLine(new GUIContent("Decal Normal Map"), _DetailNormal, hasDetailN ? _DetailNormalStrength : null);
					MGUI.TextureSOScroll(me, _DetailBaseColor, _DetailScroll, hasDetailBC || hasDetailN);
				});
				MGUI.PropertyGroup(() => {
					if (transMode == 2)
						me.ShaderProperty(_Color, "Surface Tint");
					else
						me.ShaderProperty(_NonGrabColor, "Surface Tint");
					me.ShaderProperty(_AngleTint, "Glancing Tint");
					if (transMode == 2)
						me.ShaderProperty(_BackfaceTint, "Backface Tint");
					else
						me.ShaderProperty(_NonGrabBackfaceTint, "Backface Tint");
					MGUI.Space2();
				});
			};
			Foldouts.Foldout("BASE", foldouts, baseTabButtons, mat, me, surfaceTabAction);

			// Normal Maps
			norm0TabButtons.Add(()=>{ResetNormalMaps();}, MGUI.resetLabel);
			Action norm0TabAction = ()=>{
				MGUI.Space4();
				me.ShaderProperty(_NormalMapMode, "Mode");
				me.ShaderProperty(_DistortionStrength, "Refraction Strength");
				me.ShaderProperty(_InvertNormals, "Invert");
				MGUI.Space4();
				if (_NormalMapMode.floatValue == 0){
					MGUI.BoldLabel("Primary");
					MGUI.PropertyGroup(() => {
						me.TexturePropertySingleLine(Tips.waterNormalMap, _NormalMap0, _Normal0StochasticToggle);
						MGUI.TexPropLabel(Tips.stochasticLabel, 117, false);
						me.ShaderProperty(_NormalStr0, "Strength");
						MGUI.Vector2Field(_NormalMapScale0, "Scale");
						MGUI.Vector2Field(_NormalMapScroll0, "Scrolling");
						me.ShaderProperty(_Rotation0, "Rotation");
					});
					MGUI.BoldLabel("Secondary");
					MGUI.SpaceN18();
					me.ShaderProperty(_Normal1Toggle, " ");
					MGUI.PropertyGroup(() => {
						MGUI.ToggleGroup(_Normal1Toggle.floatValue == 0);
						me.TexturePropertySingleLine(Tips.waterNormalMap, _NormalMap1, _Normal1StochasticToggle);
						MGUI.TexPropLabel(Tips.stochasticLabel, 117, false);
						me.ShaderProperty(_NormalStr1, "Strength");
						MGUI.Vector2Field(_NormalMapScale1, "Scale");
						MGUI.Vector2Field(_NormalMapScroll1, "Scrolling");
						me.ShaderProperty(_Rotation1, "Rotation");
						MGUI.ToggleGroupEnd();
					});
				}
				else {
					MGUI.PropertyGroup(()=>{
						me.TexturePropertySingleLine(new GUIContent("Flipbook"), _NormalMapFlipbook);
						MGUI.Vector2Field(_NormalMapFlipbookScale, "Scale");
						me.ShaderProperty(_NormalMapFlipbookStrength, "Strength");
						me.ShaderProperty(_NormalMapFlipbookSpeed, "Speed");
					});
				}
			};
			Foldouts.Foldout("NORMAL MAPS", foldouts, norm0TabButtons, mat, me, norm0TabAction);

			// Specularity
			reflSpecTabButtons.Add(()=>{ResetReflSpec();}, MGUI.resetLabel);
			Action reflSpecTabAction = ()=>{
				MGUI.Space4();
				me.TexturePropertySingleLine(Tips.waterRoughness, _RoughnessMap, _Roughness);
				MGUI.TextureSO(me, _RoughnessMap, _RoughnessMap.textureValue && _DetailTextureMode.floatValue != 1);
				me.TexturePropertySingleLine(Tips.waterMetallic, _MetallicMap, _Metallic);
				MGUI.TextureSO(me, _MetallicMap, _MetallicMap.textureValue && _DetailTextureMode.floatValue != 1);
				me.ShaderProperty(_DetailTextureMode, Tips.detailMode);
				MGUI.Space8();
				me.ShaderProperty(_Reflections, "Reflections");
				MGUI.PropertyGroup(()=>{
					MGUI.ToggleGroup(_Reflections.floatValue == 0);
					if (_Reflections.floatValue == 2){
						me.TexturePropertySingleLine(cubeLabel, _ReflCube, _ReflTint);
						MGUI.Vector3Field(_ReflCubeRotation, "Rotation", false);
					}
					else {
						if (_Reflections.floatValue == 3)
						 	me.ShaderProperty(_MirrorNormalOffsetSwizzle, Tips.mirrorNormalSwizzleText);
						me.ShaderProperty(_ReflTint, "Tint");
					}
					me.ShaderProperty(_ReflStrength, "Strength");
					if (_DepthEffects.floatValue == 1 && _Reflections.floatValue != 3){
						MGUI.ToggleFloat(me, "Screenspace Reflections", _SSR, _SSRStrength);
						if (_SSR.floatValue > 0){
							me.ShaderProperty(_EdgeFadeSSR, "Edge Fade");
							me.ShaderProperty(_SSRHeight, "Depth");
						}
					}
					me.ShaderProperty(_BackfaceReflections, "Apply to Backfaces");
					MGUI.ToggleGroupEnd();
				});
				if (_Reflections.floatValue == 3){
					MGUI.DisplayWarning("Mirror mode requires a VRChat mirror component with this shader selected in the custom shader field. It also requires the mesh be facing forwards on the local Z axis (see default unity quad for example). Lastly, this incurs the same performance cost as any other VRChat mirror, use it very sparingly.");
				}
				MGUI.Space8();
				me.ShaderProperty(_Specular, "Specular Highlights");
				MGUI.PropertyGroup( ()=>{
					MGUI.ToggleGroup(_Specular.floatValue == 0);
					me.ShaderProperty(_SpecTint, "Tint");
					me.ShaderProperty(_SpecStrength, "Strength");
					if (_Specular.floatValue == 2){
						MGUI.Vector3Field(_LightDir, "Light Direction", false);
					}
					MGUI.ToggleGroupEnd();
				});

			};
			Foldouts.Foldout("SPECULARITY", foldouts, reflSpecTabButtons, mat, me, reflSpecTabAction);

			// Emission
			emissTabButtons.Add(()=>{ResetEmission();}, MGUI.resetLabel);
			Action emissTabAction = ()=>{
				me.ShaderProperty(_EmissionToggle, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup(()=>{
					MGUI.ToggleGroup(_EmissionToggle.floatValue == 0);
					me.TexturePropertySingleLine(emissLabel, _EmissionMap, _EmissionMapStochasticToggle);
					MGUI.TexPropLabel(Tips.stochasticLabel, 117, false);
					me.ShaderProperty(_EmissionColor, "Tint");
					MGUI.TextureSOScroll(me, _EmissionMap, _EmissionMapScroll);
					me.ShaderProperty(_EmissionDistortionStrength, "Distortion Strength");
					me.ShaderProperty(_AudioLink, "Audio Link");
					if (_AudioLink.floatValue == 1){
						MGUI.PropertyGroupLayer(()=>{
							MGUI.SpaceN1();
							me.ShaderProperty(_AudioLinkBand, "Band");
							me.ShaderProperty(_AudioLinkStrength, "Strength");
							MGUI.SpaceN1();
						});
					}
					MGUI.ToggleGroupEnd();
				});

			};
			Foldouts.Foldout("EMISSION", foldouts, emissTabButtons, mat, me, emissTabAction);

			// Flow Mapping
			flowTabButtons.Add(()=>{ResetFlowMapping();}, MGUI.resetLabel);
			Action flowTabAction = ()=>{
				me.ShaderProperty(_FlowToggle, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup(()=>{
					MGUI.ToggleGroup(_FlowToggle.floatValue == 0);
					me.TexturePropertySingleLine(flowLabel, _FlowMap, _FlowMapUV);
					if (_BlendNoiseSource.floatValue == 1)
						me.TexturePropertySingleLine(Tips.blendNoise, _BlendNoise);
					MGUI.Vector2Field(_FlowMapScale, "Flow Map Scale");
					if (_BlendNoiseSource.floatValue == 1)
						MGUI.Vector2Field(_BlendNoiseScale, "Blend Noise Scale");
					me.ShaderProperty(_FlowSpeed, "Speed");
					me.ShaderProperty(_FlowStrength, "Strength");
					me.ShaderProperty(_BlendNoiseSource, "Blend Noise Source");
					me.ShaderProperty(_VisualizeFlowmap, "Visualize");
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.Foldout("FLOW MAPPING", foldouts, flowTabButtons, mat, me, flowTabAction);

			// Vertex Offset
			vertTabButtons.Add(()=>{ResetVertOffset();}, MGUI.resetLabel);
			Action vertTabAction = ()=>{
				me.ShaderProperty(_VertOffsetMode, "Mode");
				MGUI.Space4();
				MGUI.ToggleGroup(_VertOffsetMode.floatValue == 0);
				if (_VertOffsetMode.floatValue == 1){
					MGUI.PropertyGroup(()=>{
						me.TexturePropertySingleLine(noiseLabel, _NoiseTex);
						me.ShaderProperty(_NoiseTexBlur, "Precision");
						MGUI.Vector2Field(_NoiseTexScale, "Scale");
						MGUI.Vector2Field(_NoiseTexScroll, "Scrolling");
					});
					MGUI.PropertyGroup(()=>{
						MGUI.Vector3Field(_Offset, "Strength", false);
						me.ShaderProperty(_WaveHeight, "Strength Multiplier");
						MGUI.SliderMinMax(_VertRemapMin, _VertRemapMax, -1f, 1f, "Remap", 1);
					});
				}
				else if (_VertOffsetMode.floatValue == 2){
					MGUI.BoldLabel("Global");
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_WaveStrengthGlobal, "Strength");
						me.ShaderProperty(_WaveScaleGlobal, "Scale");
						me.ShaderProperty(_WaveSpeedGlobal, "Speed");
						me.ShaderProperty(_RecalculateNormals, "Recalculate Normals");
					});
					MGUI.PropertyGroup(()=>{
						me.TexturePropertySingleLine(Tips.maskText, _VertexOffsetMask, _VertexOffsetMaskStrength, _VertexOffsetMaskChannel);
						MGUI.TextureSO(me, _VertexOffsetMask);
					});
					MGUI.BoldLabel("Wave 1");
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_WaveStrength0, "Strength");
						me.ShaderProperty(_WaveScale0, "Scale");
						me.ShaderProperty(_WaveSpeed0, "Speed");
						me.ShaderProperty(_WaveDirection0, "Direction");
					});
					MGUI.BoldLabel("Wave 2");
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_WaveStrength1, "Strength");
						me.ShaderProperty(_WaveScale1, "Scale");
						me.ShaderProperty(_WaveSpeed1, "Speed");
						me.ShaderProperty(_WaveDirection1, "Direction");
					});
					MGUI.BoldLabel("Wave 3");
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_WaveStrength2, "Strength");
						me.ShaderProperty(_WaveScale2, "Scale");
						me.ShaderProperty(_WaveSpeed2, "Speed");
						me.ShaderProperty(_WaveDirection2, "Direction");
					});
					// MGUI.BoldLabel("Turbulence");
					// MGUI.PropertyGroup(() => {
					// 	me.ShaderProperty(_Turbulence, Tips.turbulence);
					// 	me.ShaderProperty(_TurbulenceSpeed, "Speed");
					// 	me.ShaderProperty(_TurbulenceScale, "Scale");
					// });
				}
				else if (_VertOffsetMode.floatValue == 3){
					MGUI.PropertyGroup(()=>{
						me.ShaderProperty(_VoronoiSpeed, "Speed");
						MGUI.Vector2Field(_VoronoiScale, "Scale");
						MGUI.Vector2Field(_VoronoiScroll, "Scrolling");
						MGUI.Vector3Field(_VoronoiOffset, "Strength", false);
						me.ShaderProperty(_VoronoiWaveHeight, "Strength Multiplier");
					});
				}
				else if (_VertOffsetMode.floatValue == 4){
					MGUI.PropertyGroup(()=>{
						me.TexturePropertySingleLine(new GUIContent("Vertex Offset Flipbook"), _VertOffsetFlipbook);
						MGUI.Vector2Field(_NormalMapFlipbookScale, "Scale");
						me.ShaderProperty(_VertOffsetFlipbookStrength, "Strength");
						me.ShaderProperty(_VertOffsetFlipbookSpeed, "Speed");
					});
				}
				if (_VertOffsetMode.floatValue > 0 && _VertOffsetMode.floatValue != 2){
					MGUI.PropertyGroup(()=>{
						me.TexturePropertySingleLine(Tips.maskText, _VertexOffsetMask, _VertexOffsetMaskStrength, _VertexOffsetMaskChannel);
						MGUI.TextureSO(me, _VertexOffsetMask);
					});
				}
				if (_VertOffsetMode.floatValue > 0){
					MGUI.BoldLabel("Subsurface Scattering");
					MGUI.PropertyGroup(()=>{
						me.ShaderProperty(_SubsurfaceTint, "Tint");
						me.ShaderProperty(_SubsurfaceStrength, "Strength");
						me.ShaderProperty(_SubsurfaceBrightness, "Brightness");
						me.ShaderProperty(_SubsurfaceThreshold, "Threshold");
					});
				}
				MGUI.ToggleGroupEnd();
			};
			Foldouts.Foldout("VERTEX OFFSET", foldouts, vertTabButtons, mat, me, vertTabAction);

			// Caustics
			if (_DepthEffects.floatValue == 1 && transMode == 2){
				causticsTabButtons.Add(()=>{ResetCaustics();}, MGUI.resetLabel);
				Action causticsTabAction = ()=>{
					me.ShaderProperty(_CausticsToggle, "Mode");
					MGUI.Space4();
					if (_CausticsToggle.floatValue > 0){
						MGUI.PropertyGroup(()=>{
							if (_CausticsToggle.floatValue == 2){
								me.TexturePropertySingleLine(new GUIContent("Caustics Texture"), _CausticsTex);
							}
							else if (_CausticsToggle.floatValue == 3){
								me.TexturePropertySingleLine(new GUIContent("Caustics Flipbook"), _CausticsTexArray);
							}
							me.ShaderProperty(_CausticsColor, "Color");
							me.ShaderProperty(_CausticsOpacity, "Strength");
							
							if (_CausticsToggle.floatValue == 1){
								me.ShaderProperty(_CausticsPower, "Power");
								me.ShaderProperty(_CausticsDisp, "Phase");
							}
							if (_CausticsToggle.floatValue != 3)
								me.ShaderProperty(_CausticsSpeed, "Speed");
							else
								me.ShaderProperty(_CausticsFlipbookSpeed, "Speed");
							me.ShaderProperty(_CausticsScale, "Scale");
							me.ShaderProperty(_CausticsFade, Tips.causticsFade);
							// me.ShaderProperty(_CausticsSurfaceFade, Tips.causticsSurfaceFade);
							MGUI.Vector3Field(_CausticsRotation, "Rotation", false);
						});
						if (_CausticsToggle.floatValue != 3){
							MGUI.PropertyGroup( ()=>{
								me.TexturePropertySingleLine(new GUIContent("Distortion Texture"), _CausticsDistortionTex);
								me.ShaderProperty(_CausticsDistortion, "Distortion Strength");
								me.ShaderProperty(_CausticsDistortionScale, "Distortion Scale");
								MGUI.Vector2Field(_CausticsDistortionSpeed, "Distortion Speed");
							});
						}
					}
				};
				Foldouts.Foldout("CAUSTICS", foldouts, causticsTabButtons, mat, me, causticsTabAction);
			}

			// Foam
			foamTabButtons.Add(()=>{ResetFoam();}, MGUI.resetLabel);
			Action foamTabAction = ()=>{
				me.ShaderProperty(_FoamToggle, "Enable");
				MGUI.Space4();
				MGUI.ToggleGroup(_FoamToggle.floatValue == 0);
				MGUI.PropertyGroup(()=>{
					me.TexturePropertySingleLine(foamLabel, _FoamTex, _FoamColor, _FoamStochasticToggle);
					MGUI.TexPropLabel(Tips.stochasticLabel, 117, true);
					MGUI.Space2();
					MGUI.Vector2Field(_FoamTexScale, "Scale");
					MGUI.Vector2Field(_FoamTexScroll, "Scrolling");
					// me.ShaderProperty(_FoamOffset, Tips.parallaxOffsetLabel);
					me.ShaderProperty(_FoamDistortionStrength, "Distortion Strength");
					MGUI.ToggleFloat(me, Tips.foamNormal, _FoamNormalToggle, _FoamNormalStrength);
				});
				MGUI.PropertyGroup(()=>{
					me.TexturePropertySingleLine(noiseLabel, _FoamNoiseTex);
					MGUI.Vector2Field(_FoamNoiseTexScale, "Scale");
					MGUI.Vector2Field(_FoamNoiseTexScroll, "Scrolling");
					me.ShaderProperty(_FoamNoiseTexStrength, Tips.foamNoiseTexStrength);
					me.ShaderProperty(_FoamNoiseTexCrestStrength, Tips.foamNoiseTexCrestStrength);
				});
				MGUI.PropertyGroup(()=>{
					me.ShaderProperty(_FoamRoughness, Tips.foamRoughness);
					me.ShaderProperty(_FoamPower, Tips.foamPower);
					me.ShaderProperty(_FoamEdgeStrength, Tips.foamEdgeStrength);
					me.ShaderProperty(_FoamCrestStrength, Tips.foamCrestStrength);
					me.ShaderProperty(_FoamCrestThreshold, Tips.foamCrestThreshold);
				});
				MGUI.ToggleGroupEnd();
			};
			Foldouts.Foldout("FOAM", foldouts, foamTabButtons, mat, me, foamTabAction);

			// Depth Fog
			if (_DepthEffects.floatValue == 1 && transMode == 2){
				fogTabButtons.Add(()=>{ResetFog();}, MGUI.resetLabel);
				Action fogTabAction = ()=>{
					me.ShaderProperty(_FogToggle, "Enable");
					MGUI.Space4();
					MGUI.ToggleGroup(_FogToggle.floatValue == 0);
					// MGUI.BoldLabel("Layer 1");
					MGUI.PropertyGroup(()=>{
						me.ShaderProperty(_FogTint, "Color");
						me.ShaderProperty(_FogBrightness, "Brightness");
						me.ShaderProperty(_FogPower, "Power");
					});
					// MGUI.BoldLabel("Layer 2");
					// MGUI.PropertyGroup(()=>{
					// 	me.ShaderProperty(_FogTint2, "Color");
					// 	me.ShaderProperty(_FogBrightness2, "Brightness");
					// 	me.ShaderProperty(_FogPower2, "Power");
					// });
					MGUI.ToggleGroupEnd();
				};
				Foldouts.Foldout("DEPTH FOG", foldouts, fogTabButtons, mat, me, fogTabAction);
			}

			// Edge Fade
			if (_DepthEffects.floatValue == 1 && transMode == 2){
				edgeFadeTabButtons.Add(()=>{ResetEdgeFade();}, MGUI.resetLabel);
				Action edgeFadeTabAction = ()=>{
					me.ShaderProperty(_EdgeFadeToggle, "Enable");
					MGUI.Space4();
					MGUI.PropertyGroup(()=>{
						MGUI.ToggleGroup(_EdgeFadeToggle.floatValue == 0);
						me.ShaderProperty(_EdgeFadePower, "Power");
						me.ShaderProperty(_EdgeFadeOffset, "Offset");
						MGUI.ToggleGroupEnd();
					});
				};
				Foldouts.Foldout("EDGE FADE", foldouts, edgeFadeTabButtons, mat, me, edgeFadeTabAction);
			}

			// Rain
			rainTabButtons.Add(()=>{ResetRain();}, MGUI.resetLabel);
			Action rainTabAction = ()=>{
				me.ShaderProperty(_RainToggle, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup(()=>{
					MGUI.ToggleGroup(_RainToggle.floatValue == 0);
					me.ShaderProperty(_RippleStr, "Strength");
					me.ShaderProperty(_RippleSpeed, "Speed");
					me.ShaderProperty(_RippleScale, "Scale");
				});
				MGUI.PropertyGroup(()=>{
					me.ShaderProperty(_RippleDensity, "Ripple Density");
					me.ShaderProperty(_RippleSize, "Ripple Size");
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.Foldout("RAIN", foldouts, rainTabButtons, mat, me, rainTabAction);

			// Tessellation
			if (isTessellated){
				tessTabButtons.Add(()=>{ResetTess();}, MGUI.resetLabel);
				Action tessTabAction = ()=>{
					MGUI.DisplayWarning("WARNING: Tessellation is known to cause issues on some hardware, and can be extremely expensive if you turn up the settings too far. Experimentation will likely be required as factors need to be set based on the base triangle count of the mesh.");
					MGUI.DisplayInfo("Use the 'Shaded Wireframe' scene view for easy visualization.");
					MGUI.PropertyGroup(()=>{
						me.ShaderProperty(_TessellationOffsetMask, "Vertex Offset Mask");
						me.ShaderProperty(_TessMin, "Min Factor");
						me.ShaderProperty(_TessMax, "Max Factor");
						me.ShaderProperty(_TessDistMin, "Min Distance");
						me.ShaderProperty(_TessDistMax, "Max Distance");
					});
				};
				Foldouts.Foldout("TESSELLATION", foldouts, tessTabButtons, mat, me, tessTabAction);
			}

			// AreaLit
			if (Shader.Find("AreaLit/Standard") != null){
				areaLitTabButtons.Add(()=>{ResetAreaLit();}, MGUI.resetLabel);
				Action areaLitTabAction = ()=>{
					me.ShaderProperty(_AreaLitToggle, "Enable");
					MGUI.Space4();
					bool reflDisabled = _AreaLitToggle.floatValue == 1 && _Reflections.floatValue == 0;
					bool cantInteract = _AreaLitToggle.floatValue == 0 || reflDisabled;
					if (reflDisabled){
						MGUI.DisplayError("Reflections are disabled, please enable them to use AreaLit.");
					}
					MGUI.ToggleGroup(cantInteract);
					MGUI.PropertyGroup(()=>{
						me.TexturePropertySingleLine(Tips.maskText, _AreaLitMask);
						MGUI.TextureSO(me, _AreaLitMask, _AreaLitMask.textureValue);
						me.ShaderProperty(_AreaLitStrength, "Strength");
						me.ShaderProperty(_AreaLitRoughnessMult, "Roughness Multiplier");
						me.ShaderProperty(_OpaqueLights, Tips.opaqueLightsText);
					});
					MGUI.PropertyGroup(()=>{
						var lightMeshText = !_LightMesh.textureValue ? Tips.lightMeshText : new GUIContent(
							Tips.lightMeshText.text + $" (max: {_LightMesh.textureValue.height})", Tips.lightMeshText.tooltip
						);
						me.TexturePropertySingleLine(lightMeshText, _LightMesh);
						me.TexturePropertySingleLine(Tips.lightTex0Text, _LightTex0);
						CheckTrilinear(_LightTex0.textureValue);
						me.TexturePropertySingleLine(Tips.lightTex1Text, _LightTex1);
						CheckTrilinear(_LightTex1.textureValue);
						me.TexturePropertySingleLine(Tips.lightTex2Text, _LightTex2);
						CheckTrilinear(_LightTex2.textureValue);
						me.TexturePropertySingleLine(Tips.lightTex3Text, _LightTex3);
						CheckTrilinear(_LightTex3.textureValue);
					});
					MGUI.ToggleGroupEnd();
					MGUI.DisplayInfo("Note that the AreaLit package files MUST be inside a folder named AreaLit (case sensitive) directly in the Assets folder (Assets/AreaLit)");
				};
				Foldouts.Foldout("AREALIT", foldouts, areaLitTabButtons, mat, me, areaLitTabAction);
			}
			else {
				_AreaLitToggle.floatValue = 0f;
				mat.SetInt("_AreaLitToggle", 0);
				mat.DisableKeyword("_AREALIT_ON");
			}

			// Render Settings
			renderingTabButtons.Add(()=>{ResetRendering(mat);}, MGUI.resetLabel);
			Action renderingTabAction = ()=>{
				MGUI.Space4();
				MGUI.PropertyGroup(()=>{
					me.RenderQueueField();
					me.ShaderProperty(_StencilRef, "Stencil Reference");
				});
				MGUI.PropertyGroup(()=>{
					EditorGUI.BeginChangeCheck();
					me.ShaderProperty(_TransparencyMode, "Transparency Mode");
					if (EditorGUI.EndChangeCheck()){
						ApplyTransparencySettings(mat);
					}
					me.ShaderProperty(_CullMode, "Culling Mode");
					me.ShaderProperty(_ZWrite, "ZWrite");
					if (transMode == 2){
						me.ShaderProperty(_DepthEffects, "Depth Effects");
					}
					me.ShaderProperty(_BicubicLightmapping, Tips.bicubicLightmap);
				});
				MGUI.PropertyGroup(()=>{
					me.ShaderProperty(_TexCoordSpace, "Texture Coordinate Space");
					if (_TexCoordSpace.floatValue == 1f){
						me.ShaderProperty(_TexCoordSpaceSwizzle, "Swizzle");
						me.ShaderProperty(_GlobalTexCoordScaleWorld, "Texture Coordinate Scale");
					}
					else {
						me.ShaderProperty(_GlobalTexCoordScaleUV, "Texture Coordinate Scale");
					}
				});
				if (_DepthEffects.floatValue == 1 && transMode == 2){
					MGUI.DisplayInfo("   Depth effects require a \"Depth Light\" prefab be present in the scene.\n   (Found in: Assets/Mochie/Unity/Prefabs)");
				}
			};
			Foldouts.Foldout("RENDER SETTINGS", foldouts, renderingTabButtons, mat, me, renderingTabAction);

        }
		ApplyMaterialSettings(mat);

		MGUI.DoFooter(versionLabel);
    }

	public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
		base.AssignNewShaderToMaterial(mat, oldShader, newShader);
		MGUI.ClearKeywords(mat);
		ApplyMaterialSettings(mat);
		ApplyTransparencySettings(mat);
	}

	void ApplyTransparencySettings(Material mat){
		int transMode = mat.GetInt("_TransparencyMode");
		switch (transMode){
			
			// Opaque
			case 0:
				mat.SetOverrideTag("RenderType", "Opaque");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				mat.SetInt("_ZWrite", 1);
				mat.SetShaderPassEnabled("Always", false);
				mat.EnableKeyword("_OPAQUE_MODE_ON");
				mat.DisableKeyword("_PREMUL_MODE_ON");
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
			break;

			// Premultiplied
			case 1:
				mat.SetOverrideTag("RenderType", "Transparent");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				mat.SetInt("_ZWrite", 0);
				mat.SetShaderPassEnabled("Always", false);
				mat.DisableKeyword("_OPAQUE_MODE_ON");
				mat.EnableKeyword("_PREMUL_MODE_ON");
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
			break;

			// Grabpass
			case 2:
				mat.SetOverrideTag("RenderType", "Transparent");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				mat.SetInt("_ZWrite", 0);
				mat.SetShaderPassEnabled("Always", true);
				mat.DisableKeyword("_OPAQUE_MODE_ON");
				mat.DisableKeyword("_PREMUL_MODE_ON");
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
			break;

			default: break;
		}
	}

	void ApplyMaterialSettings(Material mat){
		int transMode = mat.GetInt("_TransparencyMode");
		int depthFXToggle = mat.GetInt("_DepthEffects");
		int vertMode = mat.GetInt("_VertOffsetMode");
		int reflMode = mat.GetInt("_Reflections");
		int specMode = mat.GetInt("_Specular");
		int causticsMode = mat.GetInt("_CausticsToggle");
		int normalMapMode = mat.GetInt("_NormalMapMode");
		int bicubic = mat.GetInt("_BicubicLightmapping");
		int ssrToggle = mat.GetInt("_SSR");
		bool ssrEnabled = ssrToggle == 1 && depthFXToggle == 1 && transMode == 2 && (reflMode == 1 || reflMode == 2);

		MGUI.SetKeyword(mat, "_REFLECTIONS_ON", reflMode > 0);
		MGUI.SetKeyword(mat, "_REFLECTIONS_MANUAL_ON", reflMode == 2);
		MGUI.SetKeyword(mat, "_REFLECTIONS_MIRROR_ON", reflMode == 3);
		MGUI.SetKeyword(mat, "_SPECULAR_ON", specMode > 0);
		MGUI.SetKeyword(mat, "_NOISE_TEXTURE_ON", vertMode == 1);
		MGUI.SetKeyword(mat, "_GERSTNER_WAVES_ON", vertMode == 2);
		MGUI.SetKeyword(mat, "_VORONOI_ON", vertMode == 3);
		MGUI.SetKeyword(mat, "_VERT_FLIPBOOK_ON", vertMode == 4);
		MGUI.SetKeyword(mat, "_DEPTH_EFFECTS_ON", depthFXToggle == 1 && transMode == 2);
		MGUI.SetKeyword(mat, "_DETAIL_BASECOLOR_ON", mat.GetTexture("_DetailBaseColor"));
		MGUI.SetKeyword(mat, "_DETAIL_NORMAL_ON", mat.GetTexture("_DetailNormal"));
		MGUI.SetKeyword(mat, "_CAUSTICS_VORONOI_ON", causticsMode == 1 && depthFXToggle == 1);
		MGUI.SetKeyword(mat, "_CAUSTICS_TEXTURE_ON", causticsMode == 2 && depthFXToggle == 1);
		MGUI.SetKeyword(mat, "_CAUSTICS_FLIPBOOK_ON", causticsMode == 3 && depthFXToggle == 1);
		MGUI.SetKeyword(mat, "_NORMALMAP_FLIPBOOK_ON", normalMapMode == 1);
		MGUI.SetKeyword(mat, "_SCREENSPACE_REFLECTIONS_ON", ssrEnabled);
		MGUI.SetKeyword(mat, "_BICUBIC_LIGHTMAPPING_ON", bicubic == 1);
	}

	void CheckTrilinear(Texture tex) {
		if(!tex)
			return;
		if(tex.mipmapCount <= 1) {
			me.HelpBoxWithButton(
				EditorGUIUtility.TrTextContent("Mip maps are required, please enable them in the texture import settings."),
				EditorGUIUtility.TrTextContent("OK"));
			return;
		}
		if(tex.filterMode != FilterMode.Trilinear) {
			if(me.HelpBoxWithButton(
				EditorGUIUtility.TrTextContent("Trilinear filtering is required, and aniso is recommended."),
				EditorGUIUtility.TrTextContent("Fix Now"))) {
				tex.filterMode = FilterMode.Trilinear;
				tex.anisoLevel = 1;
				EditorUtility.SetDirty(tex);
			}
			return;
		}
	}

	void ResetSurface(){
		_Color.colorValue = Color.white;
		_NonGrabColor.colorValue = new Color(0f,0f,0f,0f);
		_MainTex.textureValue = null;
		_MainTexScroll.vectorValue = new Vector4(0,0.1f,0,0);
		_DetailBaseColor.textureValue = null;
		_DetailBaseColorTint.colorValue = Color.white;
		_DetailNormal.textureValue = null;
		_DetailNormalStrength.floatValue = 1f;
		_Opacity.floatValue = 1f;
		_BaseColorDistortionStrength.floatValue = 0.1f;
		_AngleTint.colorValue = Color.white;
		_DetailScroll.vectorValue = Vector4.zero;
		_ShadowStrength.floatValue = 0f;
	}

	void ResetNormalMaps(){
		_DistortionStrength.floatValue = 0.5f;
		ResetPrimaryNormal();
		ResetSecondaryNormal();
		ResetFlipbookNormal();
	}

	void ResetPrimaryNormal(){
		_NormalMapScale0.vectorValue = new Vector4(3f,3f,0,0);
		_NormalStr0.floatValue = 0.3f;
		_Rotation0.floatValue = 0f;
		_NormalMapScroll0.vectorValue = new Vector4(0.1f,0.1f,0,0);
	}

	void ResetSecondaryNormal(){
		_NormalStr1.floatValue = 0.3f;
		_NormalMapScale1.vectorValue = new Vector4(4f,4f,0,0);
		_NormalMapScroll1.vectorValue = new Vector4(-0.1f, 0.1f, 0,0);
		_Rotation1.floatValue = 0f;
	}

	void ResetFlipbookNormal(){
		_NormalMapFlipbookSpeed.floatValue = 8f;
		_NormalMapFlipbookStrength.floatValue = 0.2f;
		_NormalMapFlipbookScale.vectorValue = new Vector4(3f,3f,0f,0f);
	}
	
	void ResetFlowMapping(){
		_FlowSpeed.floatValue = 0.25f;
		_FlowStrength.floatValue = 0.3f;
		_FlowMapScale.vectorValue = new Vector4(2f,2f,0,0);
		_BlendNoiseScale.vectorValue = new Vector4(2f,2f,0,0);
		_BlendNoiseSource.floatValue = 0f;
	}

	void ResetVertOffset(){
		_WaveScaleGlobal.floatValue = 1f;
		_WaveSpeedGlobal.floatValue = 2f;
		_WaveStrengthGlobal.floatValue = 1.5f;
		_NoiseTexScale.vectorValue = new Vector4(3,3,0,0);
		_NoiseTexScroll.vectorValue = new Vector4(0f,0.1f,0f,0f);
		_NoiseTexBlur.floatValue = 0.8f;
		_WaveHeight.floatValue = 1f;
		_Offset.vectorValue = new Vector4(0,1,0,0);
		_WaveSpeed0.floatValue = 1f;
		_WaveSpeed1.floatValue = 1.1f;
		_WaveSpeed2.floatValue = 1.2f;
		_WaveStrength0.floatValue = 0.1f;
		_WaveStrength1.floatValue = 0.1f;
		_WaveStrength2.floatValue = 0.1f;
		_WaveScale0.floatValue = 4f;
		_WaveScale1.floatValue = 2f;
		_WaveScale2.floatValue = 1f;
		_WaveDirection0.floatValue = 0f;
		_WaveDirection1.floatValue = 335f;
		_WaveDirection2.floatValue = 13f;
		// _TurbulenceSpeed.floatValue = 0.3f;
		// _Turbulence.floatValue = 1f;
		// _TurbulenceScale.floatValue = 3f;
		_VertRemapMin.floatValue = -1f;
		_VertRemapMax.floatValue = 1f;
		_VoronoiOffset.vectorValue = new Vector4(0f,1f,0f,0f);
		_VoronoiScroll.vectorValue = new Vector4(0f,-0.25f,0f,0f);
		_VoronoiScale.vectorValue = new Vector4(2f,2f,0,0);
		_VoronoiWaveHeight.floatValue = 1f;
		_VoronoiSpeed.floatValue = 1.5f;
		_RecalculateNormals.floatValue = 1f;
		_VertOffsetFlipbookStrength.floatValue = 0.2f;
		_VertOffsetFlipbookSpeed.floatValue = 8f;
		_VertOffsetFlipbookScale.vectorValue = new Vector4(3f,3f,0f,0f);
		_VertexOffsetMaskStrength.floatValue = 1f;
		_VertexOffsetMaskChannel.floatValue = 0f;
		_SubsurfaceTint.colorValue = Color.white;
		_SubsurfaceThreshold.floatValue = 0.5f;
		_SubsurfaceBrightness.floatValue = 10f;
		_SubsurfaceStrength.floatValue = 0f;
	}

	void ResetCaustics(){
		_CausticsOpacity.floatValue = 1f;
		_CausticsScale.floatValue = 1f;
		_CausticsSpeed.floatValue = 1f;
		_CausticsFade.floatValue = 5f;
		_CausticsDistortion.floatValue = 0.5f;
		_CausticsDisp.floatValue = 0.15f;
		_CausticsDistortionSpeed.vectorValue = new Vector4(0.2f, -0.2f, 0f, 0f);
		_CausticsDistortionScale.floatValue = 0.2f;
		_CausticsRotation.vectorValue = new Vector4(-20f,0,20f,0);
		_CausticsColor.colorValue = Color.white;
		_CausticsPower.floatValue = 1f;
		_CausticsFlipbookSpeed.floatValue = 8f;
	}

	void ResetFog(){
		_FogTint.colorValue = new Vector4(0.11f,0.26f,0.26f,1f);
		_FogPower.floatValue = 10f;
		_FogBrightness.floatValue = 1f;
	}

	void ResetFoam(){
		_FoamTexScale.vectorValue = new Vector4(6,6,0,0);
		_FoamTexScroll.vectorValue = new Vector4(0.1f,-0.1f,0,0);
		_FoamRoughness.floatValue = 0.2f;
		_FoamColor.colorValue = Color.white;
		_FoamPower.floatValue = 200f;
		_FoamEdgeStrength.floatValue = 1.5f;
		_FoamCrestStrength.floatValue = 1.5f;
		_FoamCrestThreshold.floatValue = 0.5f;
		_FoamNoiseTexScale.vectorValue = new Vector4(3f,3f,0,0);
		_FoamNoiseTexScroll.vectorValue = new Vector4(0f,0.1f,0f,0f);
		_FoamNoiseTexStrength.floatValue = 0f;
		_FoamNoiseTexCrestStrength.floatValue = 1.1f;
		_FoamDistortionStrength.floatValue = 0.1f;
		_FoamNormalStrength.floatValue = 4f;
		_FoamNormalToggle.floatValue = 0f;
	}

	void ResetReflSpec(){
		_Roughness.floatValue = 0f;
		_Metallic.floatValue = 0f;
		_Reflections.floatValue = 1f;
		_ReflStrength.floatValue = 1f;
		_Specular.floatValue = 1f;
		_SpecStrength.floatValue = 1f;
		_SSR.floatValue = 0f;
		_SSRStrength.floatValue = 1f;
		_EdgeFadeSSR.floatValue = 0.1f;
		_ReflTint.colorValue = Color.white;
		_SpecTint.colorValue = Color.white;
		_ReflCube.textureValue = null;
		_LightDir.vectorValue = new Vector4(0f,0.75f,1f,0f);
		_ReflCubeRotation.vectorValue = Vector4.zero;
		_DetailTextureMode.floatValue = 0f;
		_SSRHeight.floatValue = 0.2f;
		_MirrorNormalOffsetSwizzle.floatValue = 1f;
	}

	void ResetRain(){
		_RippleScale.floatValue = 40f;
		_RippleSpeed.floatValue = 10f;
		_RippleStr.floatValue = 1f;
		_RippleSize.floatValue = 6f;
		_RippleDensity.floatValue = 1.57079632679f;
	}

	void ResetEdgeFade(){
		_EdgeFadePower.floatValue = 300f;
		_EdgeFadeOffset.floatValue = 0.5f;
	}

	void ResetTess(){
		_TessMin.floatValue = 1f;
		_TessMax.floatValue = 9f;
		_TessDistMin.floatValue = 25f;
		_TessDistMax.floatValue = 50f;
	}

	void ResetEmission(){
		_EmissionColor.colorValue = Color.white;
		_EmissionMapScroll.vectorValue = Vector4.zero;
		_EmissionDistortionStrength.floatValue = 0f;
		_AudioLink.floatValue = 0f;
		_AudioLinkBand.floatValue = 0f;
		_AudioLinkStrength.floatValue = 1f;
	}

	void ResetAreaLit(){
		_AreaLitMask.textureValue = null;
		_AreaLitStrength.floatValue = 1f;
		_AreaLitRoughnessMult.floatValue = 1f;
		_LightMesh.textureValue = null;
		_LightTex0.textureValue = null;
		_LightTex1.textureValue = null;
		_LightTex2.textureValue = null;
		_LightTex3.textureValue = null;
		_OpaqueLights.floatValue = 1f;
	}

	void ResetRendering(Material mat){
		_TransparencyMode.floatValue = 2f;
		_StencilRef.floatValue = 65f;
		_DepthEffects.floatValue = 1f;
		_ZWrite.floatValue = 0f;
		_CullMode.floatValue = 0f;
		_TexCoordSpace.floatValue = 0f;
		_TexCoordSpaceSwizzle.floatValue = 1f;
		_GlobalTexCoordScaleUV.floatValue = 1f;
		_GlobalTexCoordScaleWorld.floatValue = 0.1f;
		ApplyTransparencySettings(mat);
	}

	void ClearDictionaries(){
		baseTabButtons.Clear();
		norm0TabButtons.Clear();
		norm1TabButtons.Clear();
		flowTabButtons.Clear();
		vertTabButtons.Clear();
		causticsTabButtons.Clear();
		fogTabButtons.Clear();
		foamTabButtons.Clear();
		edgeFadeTabButtons.Clear();
		reflSpecTabButtons.Clear();
		rainTabButtons.Clear();
		tessTabButtons.Clear();
		emissTabButtons.Clear();
		areaLitTabButtons.Clear();
		renderingTabButtons.Clear();
	}
}