// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using UnityEngine;
using Mochie;

namespace UnityEditor
{
    internal class MochieStandardGUI : ShaderGUI
    {
        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
        }

		public enum WorkflowMode
		{
			Standard,
			Packed
		}

        private static class Styles
        {
            public static GUIContent uvSetLabel = EditorGUIUtility.TrTextContent("UV Set");
            public static GUIContent albedoText = EditorGUIUtility.TrTextContent("Base Color", "Base Color (RGB) and Transparency (A)");
            public static GUIContent alphaCutoffText = EditorGUIUtility.TrTextContent("Alpha Cutoff", "Threshold for alpha cutoff");
            public static GUIContent metallicMapText = EditorGUIUtility.TrTextContent("Metallic", "Metallic (R) and Smoothness (A)");
            public static GUIContent roughnessText = EditorGUIUtility.TrTextContent("Roughness", "Roughness value");
            public static GUIContent highlightsText = EditorGUIUtility.TrTextContent("Specular Highlights", "Specular Highlights");
            public static GUIContent reflectionsText = EditorGUIUtility.TrTextContent("Reflections", "Glossy Reflections");
            public static GUIContent normalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map");
            public static GUIContent heightMapText = EditorGUIUtility.TrTextContent("Height Map", "Height Map (G)");
            public static GUIContent occlusionText = EditorGUIUtility.TrTextContent("Occlusion", "Occlusion (G)");
            public static GUIContent emissionText = EditorGUIUtility.TrTextContent("Color", "Emission (RGB)");
            public static GUIContent detailMaskText = EditorGUIUtility.TrTextContent("Detail Mask", "Mask for Secondary Maps (A)");
            public static GUIContent detailAlbedoText = EditorGUIUtility.TrTextContent("Detail Base Color", "Base Color (RGB) multiplied by 2");
            public static GUIContent detailNormalMapText = EditorGUIUtility.TrTextContent("Detail Normal Map", "Normal Map");
			public static GUIContent packedMapText = EditorGUIUtility.TrTextContent("Packed Map (RAMH)", "Roughness, AO, Metallic, Height");
			public static GUIContent reflCubeText = EditorGUIUtility.TrTextContent("Reflection Fallback", "Replace environment reflections below the threshold with this cubemap sample");
            public static GUIContent ssrText = EditorGUIUtility.TrTextContent("Screen Space Reflections");
            public static GUIContent edgeFadeText = EditorGUIUtility.TrTextContent("Edge Fade");
            public static GUIContent ditherText = EditorGUIUtility.TrTextContent("Dithering");
			public static GUIContent stepsText = EditorGUIUtility.TrTextContent("Parallax Steps");
			public static GUIContent fallbackAlphaMaskText = EditorGUIUtility.TrTextContent("Fallback Alpha Mask");

            public static string primaryMapsText = "Textures";
            public static string secondaryMapsText = "Secondary Maps";
            public static string renderingMode = "Blending Mode";
            public static string advancedText = "Rendering Options";
            public static readonly string[] blendNames = Enum.GetNames(typeof(BlendMode));
			public static readonly string[] workNames = Enum.GetNames(typeof(WorkflowMode));
        }

		string watermark = "Watermark_Pro";
		string patIcon = "Patreon_Icon";
		string versionLabel = "v1.0";

        MaterialProperty blendMode = null;
		MaterialProperty workflowMode = null;
        MaterialProperty albedoMap = null;
        MaterialProperty albedoColor = null;
        MaterialProperty alphaCutoff = null;
        MaterialProperty metallicMap = null;
        MaterialProperty metallic = null;
        MaterialProperty roughness = null;
        MaterialProperty roughnessMap = null;
        MaterialProperty highlights = null;
        MaterialProperty reflections = null;
        MaterialProperty bumpScale = null;
        MaterialProperty bumpMap = null;
        MaterialProperty occlusionStrength = null;
        MaterialProperty occlusionMap = null;
        MaterialProperty heightMapScale = null;
        MaterialProperty heightMap = null;
        MaterialProperty emissionColorForRendering = null;
        MaterialProperty emissionMap = null;
        MaterialProperty detailMask = null;
        MaterialProperty detailAlbedoMap = null;
        MaterialProperty detailNormalMapScale = null;
        MaterialProperty detailNormalMap = null;
        MaterialProperty uvSetSecondary = null;
		MaterialProperty culling = null;
		MaterialProperty packedMap = null;
		MaterialProperty useHeight = null;
		MaterialProperty saturation = null;
		MaterialProperty uv0Rot = null;
		MaterialProperty uv1Rot = null;
		MaterialProperty reflCube = null;
		MaterialProperty cubeThreshold = null;
		MaterialProperty heightMult = null;
		MaterialProperty roughMult = null;
		MaterialProperty metalMult = null;
		MaterialProperty occMult = null;
		MaterialProperty gsaa = null;
		MaterialProperty ssr = null;
		MaterialProperty ssrNoise = null;
		MaterialProperty edgeFade = null;
		MaterialProperty dith = null;
		MaterialProperty steps = null;
		MaterialProperty fallbackAlphaMask = null;

        MaterialEditor m_MaterialEditor;

        bool m_FirstTimeApply = true;

        public void FindProperties(MaterialProperty[] props, Material mat)
        {
            blendMode = FindProperty("_Mode", props);
			workflowMode = FindProperty("_WorkMode", props);
            albedoMap = FindProperty("_MainTex", props);
            albedoColor = FindProperty("_Color", props);
            alphaCutoff = FindProperty("_Cutoff", props);
            metallicMap = FindProperty("_MetallicGlossMap", props, false);
            metallic = FindProperty("_Metallic", props, false);
            roughness = FindProperty("_Glossiness", props);
            roughnessMap = FindProperty("_SpecGlossMap", props);
            highlights = FindProperty("_SpecularHighlights", props, false);
            reflections = FindProperty("_GlossyReflections", props, false);
            bumpScale = FindProperty("_BumpScale", props);
            bumpMap = FindProperty("_BumpMap", props);
            heightMapScale = FindProperty("_Parallax", props);
            heightMap = FindProperty("_ParallaxMap", props);
            occlusionStrength = FindProperty("_OcclusionStrength", props);
            occlusionMap = FindProperty("_OcclusionMap", props);
            emissionColorForRendering = FindProperty("_EmissionColor", props);
            emissionMap = FindProperty("_EmissionMap", props);
            detailMask = FindProperty("_DetailMask", props);
            detailAlbedoMap = FindProperty("_DetailAlbedoMap", props);
            detailNormalMapScale = FindProperty("_DetailNormalMapScale", props);
            detailNormalMap = FindProperty("_DetailNormalMap", props);
            uvSetSecondary = FindProperty("_UVSec", props);
			packedMap = FindProperty("_PackedMap", props);
			useHeight = FindProperty("_UseHeight", props);
			saturation = FindProperty("_Saturation", props);
			uv0Rot = FindProperty("_UV0Rotate", props);
			uv1Rot = FindProperty("_UV1Rotate", props);
			reflCube = FindProperty("_ReflCube", props);
			cubeThreshold = FindProperty("_CubeThreshold", props);
			culling = FindProperty("_CullingMode", props);
			roughMult = FindProperty("_RoughnessMult", props);
			heightMult = FindProperty("_HeightMult", props);
			metalMult = FindProperty("_MetallicMult", props);
			occMult = FindProperty("_OcclusionMult", props);
			gsaa = FindProperty("_GSAA", props);
			ssr = FindProperty("_SSR", props);
			ssrNoise = FindProperty("_NoiseTexSSR", props);
			edgeFade = FindProperty("_EdgeFade", props);
			dith = FindProperty("_Dith", props);
			steps = FindProperty("_ParallaxSteps", props);
			fallbackAlphaMask = FindProperty("_ReflCubeMask", props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;

			FindProperties(props, material);

            // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
            // material to a standard shader.
            // Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
            if (m_FirstTimeApply)
            {
                MaterialChanged(material);
                m_FirstTimeApply = false;
            }

            ShaderPropertiesGUI(material);
        }

        public void ShaderPropertiesGUI(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {

                // Primary properties
                GUILayout.Label(Styles.primaryMapsText, EditorStyles.boldLabel);
                DoAlbedoArea(material);
				if (workflowMode.floatValue == 1){
					m_MaterialEditor.TexturePropertySingleLine(Styles.packedMapText, packedMap);
					EditorGUILayout.Space();
					ToggleSlider(m_MaterialEditor, "Metallic Strength", metalMult, metallic);
					ToggleSlider(m_MaterialEditor, "Roughness Strength", roughMult, roughness);
					ToggleSlider(m_MaterialEditor, "Occlusion Strength", occMult, occlusionStrength);
					if (useHeight.floatValue == 1){
						ToggleSlider(m_MaterialEditor, "Height Strength", heightMult, heightMapScale);
						m_MaterialEditor.ShaderProperty(steps, Styles.stepsText);
					}
					EditorGUILayout.Space();
				}
				else {
					m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap, metallicMap.textureValue != null ? null : metallic);
					m_MaterialEditor.TexturePropertySingleLine(Styles.roughnessText, roughnessMap, roughnessMap.textureValue != null ? null : roughness);
					m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText, occlusionMap, occlusionMap.textureValue != null ? occlusionStrength : null);
					m_MaterialEditor.TexturePropertySingleLine(Styles.heightMapText, heightMap, heightMap.textureValue != null ? heightMapScale : null);
					if (heightMap.textureValue)
						m_MaterialEditor.ShaderProperty(steps, Styles.stepsText, 2);
				}
                m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap, bumpMap.textureValue != null ? bumpScale : null);
				m_MaterialEditor.TexturePropertySingleLine(Styles.reflCubeText, reflCube);
				if (reflCube.textureValue)
					m_MaterialEditor.TexturePropertySingleLine(Styles.fallbackAlphaMaskText, fallbackAlphaMask);
				m_MaterialEditor.TexturePropertySingleLine(Styles.detailMaskText, detailMask);
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailAlbedoText, detailAlbedoMap);
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, detailNormalMap, detailNormalMap.textureValue ? detailNormalMapScale : null);
				EditorGUILayout.Space();

                DoEmissionArea(material);

				EditorGUILayout.Space();

				GUILayout.Label("Primary UVs", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
                if (EditorGUI.EndChangeCheck())
                    emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset; // Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake
				m_MaterialEditor.ShaderProperty(uv0Rot, "Rotation");

				GUILayout.Label("Detail UVs", EditorStyles.boldLabel);
                m_MaterialEditor.TextureScaleOffsetProperty(detailAlbedoMap);
				m_MaterialEditor.ShaderProperty(uv1Rot, "Rotation");
                m_MaterialEditor.ShaderProperty(uvSetSecondary, Styles.uvSetLabel.text);

				EditorGUILayout.Space();

				// Rendering options
				GUILayout.Label(Styles.advancedText, EditorStyles.boldLabel);
				// m_MaterialEditor.RenderQueueField();
				WorkflowPopup();
                BlendModePopup();
				m_MaterialEditor.ShaderProperty(culling, "Culling");
				if (blendMode.floatValue == 1)
					m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text);
				m_MaterialEditor.ShaderProperty(highlights, Styles.highlightsText);
                m_MaterialEditor.ShaderProperty(reflections, Styles.reflectionsText);
				m_MaterialEditor.ShaderProperty(ssr, Styles.ssrText);
				if (ssr.floatValue == 1){
					m_MaterialEditor.ShaderProperty(edgeFade, Styles.edgeFadeText, 1);
					m_MaterialEditor.ShaderProperty(dith, Styles.ditherText, 1);
				}
				if (workflowMode.floatValue == 1)
					m_MaterialEditor.ShaderProperty(useHeight, "Packed Height");
				m_MaterialEditor.ShaderProperty(gsaa, "GSAA");
				m_MaterialEditor.EnableInstancingField();
				GUILayout.Space(-2);
				m_MaterialEditor.DoubleSidedGIField();
				if (ssr.floatValue == 1)
					MGUI.DisplayInfo("\nSSR in VRChat requires the \"Depth Light\" prefab found in: Assets/Mochie/Unity/Prefabs\nAnd can only be used on 1 material per scene/avatar.\n\nIt is also is VERY expensive, please use it sparingly!\n");

				GUILayout.Space(15);
				if (!EditorGUIUtility.isProSkin){
					watermark = "Watermark";
				}
				Texture2D watermarkTex = (Texture2D)Resources.Load(watermark, typeof(Texture2D));
				Texture2D patIconTex = (Texture2D)Resources.Load(patIcon, typeof(Texture2D));
				float buttonSize = 24.0f;
				float xPos = 53.0f;
				MGUI.CenteredTexture(watermarkTex, 0, 0);
				GUILayout.Space(-buttonSize);
				if (MGUI.LinkButton(patIconTex, buttonSize, buttonSize, xPos)){
					Application.OpenURL("https://www.patreon.com/mochieshaders");
				}
				GUILayout.Space(buttonSize);
				MGUI.VersionLabel(versionLabel, 12,-16,-30);
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendMode.targets)
                    MaterialChanged((Material)obj);
            }

            EditorGUILayout.Space();

            // NB renderqueue editor is not shown on purpose: we want to override it based on blend mode

        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
                return;
            }

            BlendMode blendMode = BlendMode.Opaque;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                blendMode = BlendMode.Cutout;
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                blendMode = BlendMode.Fade;
            }
            material.SetFloat("_Mode", (float)blendMode);

            MaterialChanged(material);
        }

        void BlendModePopup()
        {
            EditorGUI.showMixedValue = blendMode.hasMixedValue;
            var mode = (BlendMode)blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                blendMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;
        }

        void WorkflowPopup()
        {
            EditorGUI.showMixedValue = workflowMode.hasMixedValue;
            var workmode = (WorkflowMode)workflowMode.floatValue;

            EditorGUI.BeginChangeCheck();
            workmode = (WorkflowMode)EditorGUILayout.Popup("Workflow", (int)workmode, Styles.workNames);
            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Workflow");
                workflowMode.floatValue = (float)workmode;
            }

            EditorGUI.showMixedValue = false;
        }

        void DoAlbedoArea(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap, albedoColor, albedoMap.textureValue ? saturation : null);
			if (albedoMap.textureValue)
				MGUI.TexPropLabel("Saturation", 118);
            if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
            {
                m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text);
            }
        }

        void DoEmissionArea(Material material)
        {
            // Emission for GI?
            if (m_MaterialEditor.EmissionEnabledProperty())
            {
                bool hadEmissionTexture = emissionMap.textureValue != null;

                // Texture and HDR color controls
                m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, false);

                // If texture was assigned and color was black set color to white
                float brightness = emissionColorForRendering.colorValue.maxColorComponent;
                if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
                    emissionColorForRendering.colorValue = Color.white;

                // change the GI flag and fix it up with emissive as black if necessary
                m_MaterialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
            }
        }

        public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
					if (material.GetInt("_SSR") == 1)
						material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest+51;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
					if (material.GetInt("_SSR") == 1)
						material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest+51;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }

        static void SetMaterialKeywords(Material material)
        {
            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap") || material.GetTexture("_DetailNormalMap"));
			SetKeyword(material, "BLOOM_LENS_DIRT", material.GetInt("_WorkMode") == 1);
            SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
            SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));
			if (!material.GetTexture("_PackedMap"))
            	SetKeyword(material, "_PARALLAXMAP", material.GetTexture("_ParallaxMap"));
			else
				SetKeyword(material, "_PARALLAXMAP", material.GetInt("_UseHeight") == 1);
            SetKeyword(material, "_DETAIL_MULX2", material.GetTexture("_DetailAlbedoMap") || material.GetTexture("_DetailNormalMap"));
			SetKeyword(material, "_MAPPING_6_FRAMES_LAYOUT", material.GetTexture("_ReflCube"));
			SetKeyword(material, "FXAA", material.GetInt("_GSAA") == 1);
			SetKeyword(material, "CHROMATIC_ABBERATION_LOW", material.GetInt("_SSR") == 1);

            // A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
            // or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
            // The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
            MaterialEditor.FixupEmissiveFlag(material);
            bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);
        }

        static void MaterialChanged(Material material)
        {
            SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
			material.SetShaderPassEnabled("Always", material.GetInt("_SSR") == 1);
            SetMaterialKeywords(material);
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }

		public static void ToggleGroup(bool isToggled){
			EditorGUI.BeginDisabledGroup(isToggled);
		}
		public static void ToggleGroupEnd(){
			EditorGUI.EndDisabledGroup();
		}
		
		public static bool SimpleButton(string text, float width, float xPos){
			Rect buttonRect = EditorGUILayout.GetControlRect();
			buttonRect.width = width;
			buttonRect.x += xPos;
			return GUI.Button(buttonRect, text);
		}

		public static void ToggleSlider(MaterialEditor me, string label, MaterialProperty toggle, MaterialProperty slider){
			float lw = EditorGUIUtility.labelWidth;
			float indent = lw + 25f;
			GUILayoutOption clickArea = GUILayout.MaxWidth(lw+13f);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = toggle.hasMixedValue;
			var tog = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
			if (EditorGUI.EndChangeCheck())
				toggle.floatValue = tog;
			EditorGUI.showMixedValue = false;

			GUILayout.Space(-18);
			Rect r = EditorGUILayout.GetControlRect();
			r.x += indent;
			r.width -= indent;

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = slider.hasMixedValue;
			EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);
			var slide = EditorGUI.Slider(r, slider.floatValue, slider.rangeLimits.x, slider.rangeLimits.y);
			EditorGUI.EndDisabledGroup();
			if (EditorGUI.EndChangeCheck())
				slider.floatValue = slide;
			EditorGUI.showMixedValue = false;
		}
    }
} // namespace UnityEditor