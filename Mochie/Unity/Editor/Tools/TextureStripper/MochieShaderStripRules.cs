using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mochie {
    /// <summary>
    /// Information about an unused texture slot identified on a material.
    /// </summary>
    public struct UnusedTextureInfo {
        public string propertyName;
        public Texture assignedTexture;
        public string reason;
        /// <summary>
        /// True for shader default textures that users can't easily reassign (or features that are only switched off in the shader).
        /// These are stripped from build copies but left alone by the in-place clean.
        /// </summary>
        public bool buildOnly;

        public UnusedTextureInfo(string propertyName, Texture assignedTexture, string reason, bool buildOnly) {
            this.propertyName = propertyName;
            this.assignedTexture = assignedTexture;
            this.reason = reason;
            this.buildOnly = buildOnly;
        }
    }

    /// <summary>
    /// Evaluates Mochie shader materials and determines which texture slots are unused.
    ///
    /// Rules mirror the shader code, not the inspector. Features compiled behind shader_feature keywords are only treated as
    /// off when the material's keyword is disabled AND its property agrees, so a stale keyword or property never causes a
    /// texture the shader still samples to be stripped. Runtime branches (plain float toggles) additionally require that the
    /// toggle isn't animated.
    /// </summary>
    public static partial class MochieShaderStripRules {

        // Shader default textures (assigned from the shader's .meta or by the editor) that are hidden from the normal UI,
        // plus textures for features the shader currently doesn't sample. Safe to drop from build copies only.
        static readonly HashSet<string> BuildOnlyProperties = new HashSet<string>(StringComparer.Ordinal) {
            "_DFG", "_RainSheet", "_DropletMask", "_NoiseTexSSR", "_TANoiseTex", "_ColorGradingLUT"
        };

        static readonly string[] AreaLitTextures = { "_LightMesh", "_LightTex0", "_LightTex1", "_LightTex2", "_LightTex3" };

        // Uber masks that are only sampled when Masking Mode is Separate (see GetMasks in USFunctions.cginc)
        static readonly string[] UberSeparateMasks = {
            "_ReflectionMask", "_SpecularMask", "_MatcapMask", "_MatcapBlendMask", "_ERimMask", "_RimMask", "_InterpMask",
            "_RefractionMask", "_SubsurfaceMask", "_ShadowMask", "_DiffuseMask", "_DetailMask", "_EmissMask", "_PulseMask",
            "_FilterMask", "_OutlineMask"
        };

        // Uber textures that are only sampled inside #if SHADING_ENABLED blocks with no keyword of their own
        static readonly string[] UberShadingOnlyTextures = {
            "_SmoothnessMap", "_OcclusionMap", "_DetailOcclusionMap", "_AOTintTex", "_DetailAlbedoMap", "_DetailRoughnessMap",
            "_DetailMetallic", "_DetailMask", "_ShadowMask", "_DiffuseMask", "_RimMask", "_ShadowRamp", "_DetailShadowMap",
            "_IridescenceMask", "_IridescenceRamp", "_PackedMask0", "_PackedMask1", "_PackedMask2"
        };

        static readonly string[] ScreenFXXOnlyTextures = {
            "_OutlineCube", "_OutlineCubeAlt", "_OutlineBackgroundCube", "_OutlineTex", "_OutlineTexAlt", "_ScreenTex", "_TPTexture", "_TPNoiseTex"
        };

        /// <summary>
        /// Returns true if the material uses a Mochie shader.
        /// </summary>
        public static bool IsMochieMaterial(Material mat) {
            if (mat == null || mat.shader == null) return false;
            string shaderName = mat.shader.name;
            return shaderName.StartsWith("Mochie/", StringComparison.OrdinalIgnoreCase) ||
                   shaderName.StartsWith("Hidden/Mochie/", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Analyzes a Mochie material and returns all texture slots that are not in use.
        /// </summary>
        /// <param name="mat">The material to analyze.</param>
        /// <param name="animatedProperties">Optional set of property names animated by AnimationClips (protected from stripping).</param>
        /// <param name="allowValueRules">Also treat features as unused when their strength is 0 or their colour is black.
        /// Scripts (e.g. Udon) and MaterialPropertyBlocks can change those values at runtime without the stripper knowing.</param>
        /// <returns>List of unused texture slots with their reason, one entry per property.</returns>
        public static List<UnusedTextureInfo> GetUnusedTextures(Material mat, HashSet<string> animatedProperties = null, bool allowValueRules = false) {
            if (!IsMochieMaterial(mat)) return new List<UnusedTextureInfo>();

            var c = new RuleContext(mat, animatedProperties, allowValueRules);
            string shaderName = mat.shader.name;

            if (shaderName.Contains("Standard")) {
                EvaluateStandardShader(c);
            }
            else if (shaderName.Contains("Uber")) {
                EvaluateUberShader(c);
            }
            else if (shaderName.Contains("Glass")) {
                EvaluateGlassShader(c);
            }
            else if (shaderName.Contains("Water") || shaderName.Contains("Underwater")) {
                EvaluateWaterShader(c);
            }
            else if (shaderName.Contains("Screen FX") || shaderName.Contains("SFX")) {
                EvaluateScreenFXShader(c);
            }
            else if (shaderName.Contains("Particles")) {
                EvaluateParticleShader(c);
            }
            else if (shaderName.Contains("LED Screen")) {
                EvaluateLEDShader(c);
            }
            else {
                EvaluatePatronShader(c, shaderName);
            }

            return c.results;
        }

        // Rules for the patron-only shaders, in MochieShaderStripRules.Patron.cs (Patrons package only)
        static partial void EvaluatePatronShader(RuleContext c, string shaderName);

        #region Standard Shader Rules
        private static void EvaluateStandardShader(RuleContext c) {
            bool isMobile = MGUI.IsMobileVersion(c.mat);
            int workflow = c.Int("_PrimaryWorkflow");
            int sampleMode = c.Int("_PrimarySampleMode");
            bool packed = c.On("_WORKFLOW_PACKED_ON", workflow == 1);
            bool specular = c.On("_WORKFLOW_SPECULAR_ON", workflow == 2);

            // Primary workflow
            if (c.Off("_WORKFLOW_PACKED_ON", workflow != 1))
                c.Strip("_PackedMap", "Primary workflow is not Packed");
            if (c.Off("_WORKFLOW_SPECULAR_ON", workflow != 2))
                c.Strip("_SpecGlossMap", "Primary workflow is not Specular");
            if (packed) {
                c.Strip("_MetallicMap", "Primary workflow is Packed (metallic comes from the packed map)");
                c.Strip("_RoughnessMap", "Primary workflow is Packed (roughness comes from the packed map)");
                c.Strip("_OcclusionMap", "Primary workflow is Packed (occlusion comes from the packed map)");
                c.Strip("_HeightMap", "Primary workflow is Packed (height comes from the packed map)");
            }
            else if (specular) {
                c.Strip("_MetallicMap", "Primary workflow is Specular (no metallic map)");
                if (c.Is("_SmoothnessSource", 1))
                    c.Strip("_RoughnessMap", "Smoothness source is Specular Alpha");
            }

            // Parallax (matches enableParallax in StandardEditor.SetKeywords)
            bool parallaxProperty = (workflow != 1 ? c.HasTex("_HeightMap") : c.Int("_PackedHeight") == 1) && sampleMode != 3;
            if (c.Off("_PARALLAX_ON", !parallaxProperty)) {
                c.Strip("_HeightMap", "Parallax is inactive (triplanar sampling)");
                c.Strip("_HeightMask", "Parallax is inactive");
            }

            // Detail workflow
            int detailWorkflow = c.Int("_DetailWorkflow");
            if (c.Off("_WORKFLOW_DETAIL_PACKED_ON", detailWorkflow != 1))
                c.Strip("_DetailPackedMap", "Detail workflow is not Packed");
            if (c.On("_WORKFLOW_DETAIL_PACKED_ON", detailWorkflow == 1)) {
                c.Strip("_DetailMetallicMap", "Detail workflow is Packed (separate detail metallic unused)");
                c.Strip("_DetailRoughnessMap", "Detail workflow is Packed (separate detail roughness unused)");
                c.Strip("_DetailOcclusionMap", "Detail workflow is Packed (separate detail occlusion unused)");
            }

            // Detail mask (DETAIL_MASK_NEEDED in StandardDefines.cginc). The packed detail workflow blends even without a texture.
            bool anyDetailKeyword = c.Kw("_DETAIL_MAINTEX_ON") || c.Kw("_DETAIL_NORMAL_ON") || c.Kw("_DETAIL_METALLIC_ON") ||
                                    c.Kw("_DETAIL_ROUGHNESS_ON") || c.Kw("_DETAIL_OCCLUSION_ON") || c.Kw("_WORKFLOW_DETAIL_PACKED_ON");
            bool anyDetailProperty = c.HasTex("_DetailMainTex") || c.HasTex("_DetailNormalMap") || c.HasTex("_DetailMetallicMap") ||
                                     c.HasTex("_DetailRoughnessMap") || c.HasTex("_DetailOcclusionMap") || detailWorkflow == 1;
            if (!anyDetailKeyword && !anyDetailProperty)
                c.Strip("_DetailMask", "No detail textures are in use");

            // Emission (_EMISSION_ON follows the Emission checkbox; pulse and AudioLink only multiply the result)
            if (c.Off("_EMISSION_ON", c.EmissiveIsBlack)) {
                c.Strip("_EmissionMap", "Emission is disabled");
                c.Strip("_EmissionMask", "Emission is disabled");
            }
            else if (c.BlackColor("_EmissionColor") || c.ZeroFloat("_EmissionStrength")) {
                c.Strip("_EmissionMap", "Emission colour or strength is zero");
                c.Strip("_EmissionMask", "Emission colour or strength is zero");
            }
            if (isMobile)
                c.Strip("_EmissionMask", "Standard Mobile doesn't use the emission mask");

            // Alpha mask
            bool opaque = !c.Kw("_ALPHATEST_ON") && !c.Kw("_ALPHABLEND_ON") && !c.Kw("_ALPHAPREMULTIPLY_ON") && c.Int("_BlendMode") == 0;
            if (isMobile)
                c.Strip("_AlphaMask", "Standard Mobile doesn't use the alpha mask");
            else if (c.IsNot("_AlphaSource", 1))
                c.Strip("_AlphaMask", "Alpha source is not Alpha Mask");
            else if (opaque)
                c.Strip("_AlphaMask", "Blending mode is Opaque (alpha is unused)");
            else if (c.ZeroFloat("_AlphaMaskOpacity"))
                c.Strip("_AlphaMask", "Alpha mask opacity is zero");

            // Puddles
            if (isMobile)
                c.Strip("_PuddleTexture", "Standard Mobile doesn't support puddles");
            else if (c.Is("_PuddleToggle", 0))
                c.Strip("_PuddleTexture", "Puddles are disabled");

            // Rain
            int rainMode = c.Int("_RainMode");
            bool rainKeyword = c.Kw("_RAIN_DROPLETS_ON") || c.Kw("_RAIN_RIPPLES_ON") || c.Kw("_RAIN_AUTO_ON");
            bool dropletKeyword = c.Kw("_RAIN_DROPLETS_ON") || c.Kw("_RAIN_AUTO_ON");
            if (!rainKeyword && rainMode == 0) {
                c.Strip("_RainMask", "Rain is disabled");
                c.Strip("_RainSheet", "Rain is disabled");
                c.Strip("_DropletMask", "Rain is disabled");
            }
            else if (!dropletKeyword && rainMode == 2) {
                c.Strip("_RainSheet", "Rain mode is Ripples (droplet sheet unused)");
                c.Strip("_DropletMask", "Rain mode is Ripples (droplet mask unused)");
            }
            else if (c.ZeroFloat("_DynamicDroplets")) {
                c.Strip("_DropletMask", "Dynamic droplet strength is zero");
            }

            // SSR
            if (c.Off("_SSR_ON", c.Int("_SSRToggle") == 0))
                c.Strip("_NoiseTexSSR", "SSR is disabled");

            // Vertex manipulation and wind
            if (c.Off("_VERTEX_MANIPULATION_ON", c.Int("_VertexManipulationToggle") == 0)) {
                c.Strip("_VertexMask", "Vertex manipulation is disabled");
                c.Strip("_WindNoiseTex", "Vertex manipulation is disabled");
            }
            else if (c.Is("_WindToggle", 0) || c.ZeroFloat("_WindStrength")) {
                c.Strip("_WindNoiseTex", "Wind is disabled");
            }

            // Subsurface scattering (runtime branch)
            if (c.Is("_Subsurface", 0))
                c.Strip("_ThicknessMap", "Subsurface scattering is disabled");

            // ApplyCustomLUT is commented out in StandardInput.cginc, so the LUT is never sampled.
            c.Strip("_ColorGradingLUT", "The shader doesn't currently sample a custom LUT");

            // Filament shading model LUT (runtime branch)
            if (c.IsNot("_ShadingModel", 1))
                c.Strip("_DFG", "Shading model is not Google Filament");

            StripAreaLit(c, "_AreaLitOcclusion");
            StripLTCGI(c);
            StripBakeryRNM(c);
        }
        #endregion

        #region Uber Shader Rules
        private static void EvaluateUberShader(RuleContext c) {
            bool isX = MGUI.IsXVersion(c.mat);
            bool isOutline = MGUI.IsOutline(c.mat);
            int renderMode = c.Int("_RenderMode", 1);
            bool noShading = renderMode == 0;
            int workflow = c.Int("_PBRWorkflow");

            // Unlit: SHADING_ENABLED blocks are compiled out
            if (c.Off("_SHADING_ON", noShading)) {
                foreach (string property in UberShadingOnlyTextures)
                    c.Strip(property, "Shading is disabled");
                if (!c.Kw("_SPECULAR_WORKFLOW_ON"))
                    c.Strip("_SpecGlossMap", "Shading is disabled");
                // The shadow pass still reads metallic to premultiply alpha
                if (!c.Kw("_ALPHAPREMULTIPLY_ON") && c.Int("_BlendMode") != 5)
                    c.Strip("_MetallicGlossMap", "Shading is disabled");
            }

            // Workflow (0: Metallic, 1: Specular, 2: Specular (Alpha Smoothness), 3: Packed)
            if (c.Off("_PACKED_WORKFLOW_ON", workflow < 3 || noShading))
                c.Strip("_PackedMap", "Workflow is not Packed");
            if (c.On("_PACKED_WORKFLOW_ON", workflow >= 3 && !noShading)) {
                c.Strip("_MetallicGlossMap", "Workflow is Packed (metallic comes from the packed map)");
                c.Strip("_SpecGlossMap", "Workflow is Packed (roughness comes from the packed map)");
                c.Strip("_SmoothnessMap", "Workflow is Packed");
                c.Strip("_OcclusionMap", "Workflow is Packed (occlusion comes from the packed map)");
                c.Strip("_ParallaxMap", "Workflow is Packed (height comes from the packed map)");
            }
            else if (c.On("_SPECULAR_WORKFLOW_ON", (workflow == 1 || workflow == 2) && !noShading)) {
                c.Strip("_MetallicGlossMap", "Workflow is Specular (no metallic map)");
                if (workflow == 2 && !c.IsAnimated("_PBRWorkflow"))
                    c.Strip("_SmoothnessMap", "Workflow is Specular (Alpha Smoothness)");
            }
            else if (!c.Kw("_SPECULAR_WORKFLOW_ON") && workflow == 0) {
                // In the Metallic workflow _SpecGlossMap is the roughness map, so only the smoothness map is unused.
                c.Strip("_SmoothnessMap", "Workflow is Metallic");
            }

            // Parallax (matches usingParallax in UberEditor.ApplyMaterialSettings)
            bool parallaxProperty = (workflow < 3 ? c.HasTex("_ParallaxMap") : c.HasTex("_PackedMap") && c.Int("_EnablePackedHeight") == 1) && !noShading;
            if (c.Off("_PARALLAXMAP_ON", !parallaxProperty))
                c.Strip("_ParallaxMap", "Parallax is inactive");

            // Normals
            if (c.Off("_NORMALMAP_ON", noShading))
                c.Strip("_BumpMap", "Shading is disabled");
            if (c.Off("_DETAIL_NORMALMAP_ON", noShading))
                c.Strip("_DetailNormalMap", "Shading is disabled");

            // Detail mask
            bool anyDetail = c.HasTex("_DetailNormalMap") || c.HasTex("_DetailRoughnessMap") || c.HasTex("_DetailOcclusionMap") ||
                             c.HasTex("_DetailAlbedoMap") || c.HasTex("_DetailMetallic") || c.Kw("_DETAIL_NORMALMAP_ON");
            if (!anyDetail)
                c.Strip("_DetailMask", "No detail textures are in use");

            // Reflections. With Environment reflections an assigned _ReflCube is the fallback for scenes without
            // reflection data (_REFLECTION_FALLBACK_ON), so it's only unused when reflections are off.
            int reflections = c.Int("_Reflections");
            if (c.Off("_REFLECTIONS_ON", reflections == 0 || noShading)) {
                c.Strip("_ReflCube", "Reflections are disabled");
                c.Strip("_ReflectionMask", "Reflections are disabled");
            }
            if (c.Off("_SCREENSPACE_REFLECTIONS_ON", !(c.Int("_SSR") == 1 && reflections == 1 && !noShading)))
                c.Strip("_NoiseTexSSR", "SSR is disabled");

            // Specular highlights
            int specular = c.Int("_Specular");
            if (c.Off("_SPECULAR_ON", specular == 0 || noShading))
                c.Strip("_SpecularMask", "Specular highlights are disabled");
            if (c.Off("_SPECULAR_COMBINED_ON", specular != 3 || noShading))
                c.Strip("_InterpMask", "Specular mode is not Combined");

            // Matcaps. Matcap 1 is enabled by _UseMatcap1, which the editor sets when _Matcap1 has a texture.
            if (c.Off("_MATCAP_ON", c.Int("_MatcapToggle") == 0 || noShading)) {
                c.Strip("_Matcap", "Matcaps are disabled");
                c.Strip("_MatcapMask", "Matcaps are disabled");
                c.Strip("_MatcapNormal0", "Matcaps are disabled");
                c.Strip("_Matcap1", "Matcaps are disabled");
                c.Strip("_MatcapBlendMask", "Matcaps are disabled");
                c.Strip("_MatcapNormal1", "Matcaps are disabled");
            }
            else {
                if (c.Is("_MatcapNormal0Toggle", 0))
                    c.Strip("_MatcapNormal0", "Matcap 0 normal mapping is disabled");
                if (c.Is("_UseMatcap1", 0)) {
                    c.Strip("_Matcap1", "Matcap 1 is disabled");
                    c.Strip("_MatcapBlendMask", "Matcap 1 is disabled");
                    c.Strip("_MatcapNormal1", "Matcap 1 is disabled");
                }
                else if (c.Is("_MatcapNormal1Toggle", 0)) {
                    c.Strip("_MatcapNormal1", "Matcap 1 normal mapping is disabled");
                }
            }

            // Shadows (runtime branch: 0 Off, 1 Manual Blend, 2 Ramp)
            if (c.Is("_ShadowMode", 0)) {
                c.Strip("_ShadowRamp", "Shadows are disabled");
                c.Strip("_DetailShadowMap", "Shadows are disabled");
                c.Strip("_ShadowMask", "Shadows are disabled");
            }
            else if (c.Is("_ShadowMode", 1)) {
                c.Strip("_ShadowRamp", "Shadow mode is Manual Blend (ramp texture unused)");
            }

            // Subsurface scattering
            if (c.Off("_SUBSURFACE_ON", c.Int("_Subsurface") == 0 || noShading)) {
                c.Strip("_ThicknessMap", "Subsurface scattering is disabled");
                c.Strip("_ScatterTex", "Subsurface scattering is disabled");
                c.Strip("_SubsurfaceMask", "Subsurface scattering is disabled");
            }

            // Rim lighting (strength multiplies the whole effect)
            if (c.Off("_RIM_ON", c.Int("_RimLighting") == 0 || noShading) || c.ZeroFloat("_RimStr")) {
                c.Strip("_RimTex", "Rim lighting is disabled");
                c.Strip("_RimMask", "Rim lighting is disabled");
            }
            if (c.Off("_ENVIRONMENT_RIM_ON", c.Int("_EnvironmentRim") == 0 || noShading) || c.ZeroFloat("_ERimStr")) {
                c.Strip("_ERimTex", "Environment rim is disabled");
                c.Strip("_ERimMask", "Environment rim is disabled");
            }

            // Diffuse mask only scales Disney diffuse and spherical harmonics
            if (c.ZeroFloat("_DisneyDiffuse") && c.ZeroFloat("_SHStr"))
                c.Strip("_DiffuseMask", "Disney diffuse and spherical harmonics strengths are zero");

            // Alpha mask (runtime branch)
            if (c.Is("_UseAlphaMask", 0))
                c.Strip("_AlphaMask", "Alpha source is Base Color Alpha");

            // Cubemap base color (0: 2D, 1: Cubemap, 2: Combined)
            int cubeMode = c.Int("_CubeMode");
            if (c.Off("_CUBEMAP_ON", cubeMode != 1) && c.Off("_CUBEMAP_COMBINED_ON", cubeMode != 2))
                c.Strip("_MainTexCube0", "Base color mode is 2D");
            if (c.Off("_CUBEMAP_COMBINED_ON", cubeMode != 2))
                c.Strip("_CubeBlendMask", "Base color mode is not Combined");
            if (c.IsNot("_MirrorBehavior", 2))
                c.Strip("_MirrorTex", "Mirror behavior is not Textured Reflection");
            else if (c.On("_CUBEMAP_ON", cubeMode == 1))
                c.Strip("_MirrorTex", "Base color mode is Cubemap");

            // Near clip (runtime branch)
            if (c.Is("_NearClipToggle", 0))
                c.Strip("_NearClipMask", "Near clip is disabled");

            // Base color dissolve
            if (c.Off("_BASECOLOR_DISSOLVE_ON", c.Int("_BCDissolveToggle") == 0 || noShading)) {
                c.Strip("_MainTex2", "Base color dissolve is disabled");
                c.Strip("_BCNoiseTex", "Base color dissolve is disabled");
            }

            // Refraction
            if (c.Off("_REFRACTION_ON", c.Int("_Refraction") == 0 || noShading)) {
                c.Strip("_RefractionMask", "Refraction is disabled");
                c.Strip("_RefractionDissolveMask", "Refraction is disabled");
            }

            // Iridescence (runtime branches)
            if (c.Is("_Iridescence", 0)) {
                c.Strip("_IridescenceMask", "Iridescence is disabled");
                c.Strip("_IridescenceRamp", "Iridescence is disabled");
            }
            else if (c.Is("_IridescenceMode", 0)) {
                c.Strip("_IridescenceRamp", "Iridescence mode is Hue Shift");
            }

            // Emission
            if (c.Off("_EMISSION_ON", c.Int("_EmissionToggle") == 0)) {
                c.Strip("_EmissionMap", "Emission is disabled");
                c.Strip("_EmissionMap2", "Emission is disabled");
                c.Strip("_EmissMask", "Emission is disabled");
                c.Strip("_PulseMask", "Emission is disabled");
            }
            else {
                if (c.BlackColor("_EmissionColor") || c.ZeroFloat("_EmissIntensity"))
                    c.Strip("_EmissionMap", "Emission colour or intensity is zero");
                if (c.BlackColor("_EmissionColor2") || c.ZeroFloat("_EmissIntensity2"))
                    c.Strip("_EmissionMap2", "Emission 2 colour or intensity is zero");
                if (c.Off("_PULSE_ON", c.Int("_PulseToggle") == 0))
                    c.Strip("_PulseMask", "Emission pulse is disabled");
            }

            // Filtering
            if (c.Off("_FILTERING_ON", c.Int("_Filtering") == 0)) {
                c.Strip("_FilterMask", "Filtering is disabled");
                c.Strip("_TeamColorMask", "Filtering is disabled");
            }
            else if (c.Is("_TeamFiltering", 0)) {
                c.Strip("_TeamColorMask", "Team colors are disabled");
            }
            if (c.Is("_AOFiltering", 0))
                c.Strip("_AOTintTex", "AO filtering is disabled");

            // Spritesheets
            bool spritesheetsOff = c.Off("_FLIPBOOK_ON", c.Int("_EnableSpritesheet") == 0 && c.Int("_EnableSpritesheet1") == 0);
            StripUberSpritesheet(c, spritesheetsOff, "_EnableSpritesheet", "_SpritesheetMode0", "_Spritesheet", "_Flipbook0", "0");
            StripUberSpritesheet(c, spritesheetsOff, "_EnableSpritesheet1", "_SpritesheetMode1", "_Spritesheet1", "_Flipbook1", "1");

            // Outlines only exist on the Outline variants, where the outline pass always runs.
            // (_OutlineMask on Outline variants is handled by the masking rules below.)
            if (!isOutline) {
                c.Strip("_OutlineTex", "Outlines only exist on the Outline variants");
                c.Strip("_OutlineMask", "Outlines only exist on the Outline variants");
            }

            // UV distortion. The offset is only applied to the six target toggles.
            int distortionStyle = c.Int("_DistortionStyle");
            bool noDistortionTargets = c.Is("_DistortMainUV", 0) && c.Is("_DistortDetailUV", 0) && c.Is("_DistortEmissUV", 0) &&
                                       c.Is("_DistortRimUV", 0) && c.Is("_DistortMatcap0", 0) && c.Is("_DistortMatcap1", 0);
            if (c.Off("_UV_DISTORTION_ON", distortionStyle == 0) || noDistortionTargets || c.ZeroFloat("_DistortUVStr")) {
                c.Strip("_DistortUVMap", "UV distortion is disabled or has no targets");
                c.Strip("_DistortUVMask", "UV distortion is disabled or has no targets");
            }
            else if (c.Off("_UV_DISTORTION_NORMALMAP_ON", distortionStyle != 1)) {
                c.Strip("_DistortUVMap", "UV distortion mode is Simplex");
            }

            // Vertex manipulation
            if (c.Off("_VERTEX_MANIP_ON", c.Int("_VertexManipulationToggle") == 0)) {
                c.Strip("_VertexExpansionMask", "Vertex manipulation is disabled");
                c.Strip("_VertexRoundingMask", "Vertex manipulation is disabled");
            }
            else {
                if (c.ZeroVector3("_VertexExpansion"))
                    c.Strip("_VertexExpansionMask", "Vertex expansion is zero");
                if (c.ZeroFloat("_VertexRounding"))
                    c.Strip("_VertexRoundingMask", "Vertex rounding is zero");
            }

            // AudioLink geometry effects (geometry shader, Uber X only)
            if (!isX) {
                c.Strip("_AudioLinkTriOffsetMask", "AudioLink triangle offset only exists on Uber X");
                c.Strip("_AudioLinkWireframeMask", "AudioLink wireframe only exists on Uber X");
            }
            else if (c.Off("_AUDIOLINK_ON", c.Int("_AudioLinkToggle") == 0)) {
                c.Strip("_AudioLinkTriOffsetMask", "AudioLink is disabled");
                c.Strip("_AudioLinkWireframeMask", "AudioLink is disabled");
            }
            else {
                if (c.ZeroFloat("_AudioLinkTriOffsetStrength"))
                    c.Strip("_AudioLinkTriOffsetMask", "AudioLink triangle offset strength is zero");
                if (c.ZeroFloat("_AudioLinkWireframeStrength"))
                    c.Strip("_AudioLinkWireframeMask", "AudioLink wireframe strength is zero");
            }

            // Dissolve (Uber X only; 0: Off, 1: Texture, 2: Simplex, 3: Geometry)
            int dissolveStyle = c.Int("_DissolveStyle");
            if (c.Off("_DISSOLVE_TEXTURE_ON", dissolveStyle != 1 || !isX)) {
                c.Strip("_DissolveTex", "Dissolve mode is not Texture");
                c.Strip("_DissolveFlow", "Dissolve mode is not Texture");
            }
            else if (c.Is("_DissolveBlending", 0)) {
                c.Strip("_DissolveFlow", "Dissolve flow blending is disabled");
            }
            bool anyDissolveKeyword = c.Kw("_DISSOLVE_TEXTURE_ON") || c.Kw("_DISSOLVE_SIMPLEX_ON") || c.Kw("_DISSOLVE_GEOMETRY_ON");
            if (!anyDissolveKeyword && (dissolveStyle == 0 || !isX))
                c.Strip("_DissolveMask", "Dissolve is disabled");

            // Masking mode (0: Off, 1: Separate, 2: Packed)
            int maskingMode = c.Int("_MaskingMode");
            if (c.Off("_SEPARATE_MASKING_ON", maskingMode != 1)) {
                foreach (string property in UberSeparateMasks)
                    c.Strip(property, "Masking mode is not Separate");
            }
            if (c.Off("_PACKED_MASKING_ON", maskingMode != 2)) {
                c.Strip("_PackedMask0", "Masking mode is not Packed");
                c.Strip("_PackedMask1", "Masking mode is not Packed");
                c.Strip("_PackedMask2", "Masking mode is not Packed");
                c.Strip("_PackedMask3", "Masking mode is not Packed");
            }
        }

        private static void StripUberSpritesheet(RuleContext c, bool allOff, string toggle, string mode, string sheet, string flipbook, string label) {
            if (allOff || c.Is(toggle, 0)) {
                c.Strip(sheet, $"Spritesheet {label} is disabled");
                c.Strip(flipbook, $"Spritesheet {label} is disabled");
            }
            else if (c.Is(mode, 0)) {
                c.Strip(sheet, $"Spritesheet {label} mode is Flipbook (sprite sheet texture unused)");
            }
            else if (c.Is(mode, 1)) {
                c.Strip(flipbook, $"Spritesheet {label} mode is Sprite Sheet (flipbook array unused)");
            }
        }
        #endregion

        #region Glass Shader Rules
        private static void EvaluateGlassShader(RuleContext c) {
            int workflow = c.Int("_Workflow");
            if (c.On("_WORKFLOW_PACKED_ON", workflow == 1)) {
                c.Strip("_RoughnessMap", "Workflow is Packed");
                c.Strip("_MetallicMap", "Workflow is Packed");
                c.Strip("_OcclusionMap", "Workflow is Packed");
            }
            if (c.Off("_WORKFLOW_PACKED_ON", workflow != 1))
                c.Strip("_PackedMap", "Workflow is not Packed");

            // Rain (0: Off, 1: Droplets, 2: Ripples, 3: Automatic)
            int rainMode = c.Int("_RainMode");
            if (c.Off("_RAIN_ON", rainMode == 0)) {
                c.Strip("_RainMask", "Rain is disabled");
                c.Strip("_RainSheet", "Rain is disabled");
                c.Strip("_DropletMask", "Rain is disabled");
            }
            else if (c.On("_RAINMODE_RIPPLE", rainMode == 2)) {
                c.Strip("_RainSheet", "Rain mode is Ripples");
                c.Strip("_DropletMask", "Rain mode is Ripples");
            }
            else if (c.ZeroFloat("_DynamicDroplets")) {
                c.Strip("_DropletMask", "Dynamic droplet strength is zero");
            }

            // SSR (matches GlassEditor.SetKeywords)
            bool isTwoPass = c.mat.shader.name.Contains("(Two Pass)");
            if (c.Off("_SSR_ON", !(c.Int("_SSRToggle") == 1 && c.Int("_BlendMode") == 0 && !isTwoPass)))
                c.Strip("_NoiseTexSSR", "SSR is disabled");

            StripAreaLit(c, "_AreaLitMask");
            StripBakeryRNM(c);
            StripLTCGI(c);

            if (c.Off("_EMISSION_ON", c.EmissiveIsBlack))
                c.Strip("_EmissionMap", "Emission is disabled");
            else if (c.BlackColor("_EmissionColor") || c.ZeroFloat("_EmissionStrength"))
                c.Strip("_EmissionMap", "Emission colour or strength is zero");
        }
        #endregion

        #region Water Shader Rules
        private static void EvaluateWaterShader(RuleContext c) {
            // Underwater Visuals is the only water shader with _CausticsMode
            if (c.mat.HasProperty("_CausticsMode")) {
                EvaluateUnderwaterShader(c);
                return;
            }

            // _FlowMap is never stripped: its sampler (sampler_FlowMap) is shared by most Water textures.
            // _DetailBaseColor and _DetailNormal are never stripped: their keywords follow texture presence
            // (_DetailTextureMode is Decal Mode, not an enable toggle).

            if (c.On("_OPAQUE_MODE_ON", c.Int("_TransparencyMode") == 0))
                c.Strip("_OpacityMask", "Transparency mode is Opaque");

            // Normal maps (0: Texture, 1: Flipbook)
            int normalMapMode = c.Int("_NormalMapMode");
            if (c.Off("_NORMALMAP_FLIPBOOK_ON", normalMapMode == 0))
                c.Strip("_NormalMapFlipbook", "Normal map mode is Texture");
            if (c.On("_NORMALMAP_FLIPBOOK_ON", normalMapMode == 1)) {
                c.Strip("_NormalMap0", "Normal map mode is Flipbook");
                c.Strip("_NormalMap1", "Normal map mode is Flipbook");
            }
            else if (c.Off("_NORMALMAP_1_ON", c.Int("_Normal1Toggle", 1) == 0)) {
                c.Strip("_NormalMap1", "Secondary normal map is disabled");
            }

            if (c.Off("_REFLECTIONS_MANUAL_ON", c.Int("_Reflections", 1) != 2))
                c.Strip("_ReflCube", "Reflection mode is not Cubemap");
            if (c.Off("_SCREENSPACE_REFLECTIONS_ON", c.Int("_SSR") == 0))
                c.Strip("_NoiseTexSSR", "SSR is disabled");

            if (c.Off("_FLOW_ON", c.Int("_FlowToggle", 1) == 0))
                c.Strip("_BlendNoise", "Flow mapping is disabled");
            else if (c.Is("_BlendNoiseSource", 0))
                c.Strip("_BlendNoise", "Blend noise source is Flowmap Alpha");

            // Vertex offset (0: Off, 1: Noise Texture, 2: Gerstner, 3: Voronoi, 4: Flipbook)
            int vertOffsetMode = c.Int("_VertOffsetMode");
            if (c.Off("_NOISE_TEXTURE_ON", vertOffsetMode != 1))
                c.Strip("_NoiseTex", "Vertex offset mode is not Noise Texture");
            if (c.Off("_VERT_FLIPBOOK_ON", vertOffsetMode != 4))
                c.Strip("_VertOffsetFlipbook", "Vertex offset mode is not Flipbook");
            if (c.Off("_GERSTNER_WAVES_ON", vertOffsetMode != 2))
                c.Strip("_TANoiseTex", "Vertex offset mode is not Gerstner Waves");
            else if (c.ZeroFloat("_WaveWarpStrength"))
                c.Strip("_TANoiseTex", "Wave warp strength is 0");
            bool vertOffsetKeyword = c.Kw("_NOISE_TEXTURE_ON") || c.Kw("_GERSTNER_WAVES_ON") || c.Kw("_VORONOI_ON") || c.Kw("_VERT_FLIPBOOK_ON");
            if (!vertOffsetKeyword && vertOffsetMode == 0)
                c.Strip("_VertexOffsetMask", "Vertex offset is disabled");

            // Caustics (0: Off, 1: Voronoi, 2: Texture, 3: Flipbook)
            int caustics = c.Int("_CausticsToggle", 3);
            bool textureCausticsOff = c.Off("_CAUSTICS_TEXTURE_ON", caustics != 2);
            if (textureCausticsOff) {
                c.Strip("_CausticsTex", "Caustics mode is not Texture");
                c.Strip("_CausticsShadow", "Caustics mode is not Texture");
            }
            if (c.Off("_CAUSTICS_FLIPBOOK_ON", caustics != 3))
                c.Strip("_CausticsTexArray", "Caustics mode is not Flipbook");
            if (textureCausticsOff && c.Off("_CAUSTICS_VORONOI_ON", caustics != 1))
                c.Strip("_CausticsDistortionTex", "Caustics distortion is only used by Voronoi and Texture caustics");

            // Foam (_FoamMode is a runtime branch: 0 Depth, 1 Mask, 2 Vertex Color)
            if (c.Off("_FOAM_ON", c.Int("_FoamToggle", 1) == 0)) {
                c.Strip("_FoamMap", "Foam is disabled");
                c.Strip("_FoamTex", "Foam is disabled");
                c.Strip("_FoamNoiseTex", "Foam is disabled");
            }
            else if (c.IsNot("_FoamMode", 1)) {
                c.Strip("_FoamMap", "Foam mode is not Mask");
            }

            if (c.Off("_RAIN_ON", c.Int("_RainToggle") == 0))
                c.Strip("_RippleMask", "Rain is disabled");

            StripAreaLit(c, "_AreaLitMask");
            StripBakeryRNM(c);
            StripLTCGI(c);

            if (c.Off("_EMISSION_ON", c.EmissiveIsBlack))
                c.Strip("_EmissionMap", "Emission is disabled");
            else if (c.BlackColor("_EmissionColor"))
                c.Strip("_EmissionMap", "Emission colour is black");
        }

        private static void EvaluateUnderwaterShader(RuleContext c) {
            // Caustics (0: Voronoi, 1: Texture, 2: Flipbook)
            int mode = c.Int("_CausticsMode", 2);
            bool causticsOff = c.Off("CAUSTICS_ENABLED", c.Int("_CausticsToggle", 1) == 0);
            bool textureOff = causticsOff || c.Off("_CAUSTICS_TEXTURE_ON", mode != 1);
            if (textureOff)
                c.Strip("_CausticsTex", "Caustics mode is not Texture");
            if (causticsOff || c.Off("_CAUSTICS_FLIPBOOK_ON", mode != 2))
                c.Strip("_CausticsTexArray", "Caustics mode is not Flipbook");
            if (textureOff && (causticsOff || c.Off("_CAUSTICS_VORONOI_ON", mode != 0)))
                c.Strip("_CausticsDistortionTex", "Caustics distortion is only used by Voronoi and Texture caustics");
        }
        #endregion

        #region ScreenFX Shader Rules
        private static void EvaluateScreenFXShader(RuleContext c) {
            // Most Screen FX features are runtime branches, so every toggle below is also checked for animation.
            if (c.IsNot("_ShakeModel", 3))
                c.Strip("_ShakeNoiseTex", "Shake mode is not Noise");

            if (!c.Kw("_DISTORTION_ON") && !c.Kw("_DISTORTION_WORLD_ON") && c.Int("_DistortionModel") == 0)
                c.Strip("_NormalMap", "Distortion is disabled");

            if (!MGUI.IsXVersion(c.mat)) {
                foreach (string property in ScreenFXXOnlyTextures)
                    c.Strip(property, "Only Screen FX X has outlines, screenspace textures and triplanar effects");
                return;
            }

            if (c.Is("_SST", 0))
                c.Strip("_ScreenTex", "Screenspace texture is disabled");

            if (c.Is("_Triplanar", 0)) {
                c.Strip("_TPTexture", "Triplanar effect is disabled");
                c.Strip("_TPNoiseTex", "Triplanar effect is disabled");
            }

            if (c.Is("_OutlineType", 0)) {
                c.Strip("_OutlineCube", "Outlines are disabled");
                c.Strip("_OutlineCubeAlt", "Outlines are disabled");
                c.Strip("_OutlineBackgroundCube", "Outlines are disabled");
                c.Strip("_OutlineTex", "Outlines are disabled");
                c.Strip("_OutlineTexAlt", "Outlines are disabled");
            }
            else {
                if (c.Is("_OutlineCubeToggle", 0)) {
                    c.Strip("_OutlineCube", "Outline mode is Texture");
                    c.Strip("_OutlineCubeAlt", "Outline mode is Texture");
                    c.Strip("_OutlineBackgroundCube", "Outline mode is Texture");
                }
                else if (c.Is("_OutlineCubeToggle", 1)) {
                    c.Strip("_OutlineTex", "Outline mode is Cubemap");
                    c.Strip("_OutlineTexAlt", "Outline mode is Cubemap");
                }
                if (c.Is("_OutlineAltEnable", 0)) {
                    c.Strip("_OutlineCubeAlt", "Alternate outline is disabled");
                    c.Strip("_OutlineTexAlt", "Alternate outline is disabled");
                }
            }
        }
        #endregion

        #region Particle Shader Rules
        private static void EvaluateParticleShader(RuleContext c) {
            bool isX = MGUI.IsXVersion(c.mat);

            // _SecondTex is never stripped: layering (_LAYERED_TEX_ON) is enabled whenever it's assigned.

            if (c.Off("_ALPHA_MASK_ON", c.Int("_AlphaSource") != 1))
                c.Strip("_AlphaMask", "Alpha source is Base Color");

            int lightingToggle = c.Int("_LightingToggle");
            int workflow = c.Int("_Workflow");
            bool lightingOff = c.Off("_LIGHTING_ON", lightingToggle == 0);
            if (lightingOff) {
                c.Strip("_NormalMapLighting", "Lighting is disabled");
                c.Strip("_MetallicMap", "Lighting is disabled");
                c.Strip("_RoughnessMap", "Lighting is disabled");
                c.Strip("_OcclusionMap", "Lighting is disabled");
                c.Strip("_PackedMap", "Lighting is disabled");
            }
            else {
                if (c.On("_WORKFLOW_PACKED_ON", workflow == 1)) {
                    c.Strip("_MetallicMap", "Workflow is Packed");
                    c.Strip("_RoughnessMap", "Workflow is Packed");
                    c.Strip("_OcclusionMap", "Workflow is Packed");
                }
                if (c.Off("_WORKFLOW_PACKED_ON", workflow != 1))
                    c.Strip("_PackedMap", "Workflow is Separate");
            }

            // Emission is only compiled with lighting enabled
            if (c.Off("_EMISSION_ON", c.Int("_Emission") == 0) || lightingOff)
                c.Strip("_EmissionMap", "Emission is disabled");
            else if (c.BlackColor("_EmissionColor"))
                c.Strip("_EmissionMap", "Emission colour is black");

            if (c.Off("_DISTORTION_ON", c.Int("_Distortion") == 0))
                c.Strip("_NormalMap", "Distortion is disabled");

            if (!isX)
                c.Strip("_DissolveNoise", "Dissolve only exists on Particles X");
            else if (c.Off("_DISSOLVE_ON", c.Int("_Dissolve") == 0))
                c.Strip("_DissolveNoise", "Dissolve is disabled");

            if (!isX)
                c.Strip("_RimTex", "Rim light only exists on Particles X");
            else if (c.Off("_RIMLIGHT_ON", c.Int("_RimLight") == 0))
                c.Strip("_RimTex", "Rim light is disabled");
        }
        #endregion

        #region LED Shader Rules
        private static void EvaluateLEDShader(RuleContext c) {
            int flipbookMode = c.Int("_FlipbookMode");
            if (c.Off("_FLIPBOOK_MODE", flipbookMode == 0))
                c.Strip("_Flipbook", "Flipbook mode is disabled");
            if (c.On("_FLIPBOOK_MODE", flipbookMode == 1))
                c.Strip("_MainTex", "Flipbook mode is enabled (static texture unused)");
        }
        #endregion

        #region Shared Rules
        private static void StripAreaLit(RuleContext c, string maskProperty) {
            if (!c.Off("_AREALIT_ON", c.Int("_AreaLitToggle") == 0)) return;
            c.Strip(maskProperty, "AreaLit is disabled");
            foreach (string property in AreaLitTextures)
                c.Strip(property, "AreaLit is disabled");
        }

        private static void StripLTCGI(RuleContext c) {
            if (!c.Off("LTCGI", c.Int("_LTCGI") == 0)) return;
            c.Strip("_LTCGI_mat", "LTCGI is disabled");
            c.Strip("_LTCGI_amp", "LTCGI is disabled");
        }

        private static void StripBakeryRNM(RuleContext c) {
            if (!c.Off("BAKERY_RNM", c.Int("_BakeryMode") != 2)) return;
            c.Strip("_RNM0", "Bakery mode is not RNM");
            c.Strip("_RNM1", "Bakery mode is not RNM");
            c.Strip("_RNM2", "Bakery mode is not RNM");
        }
        #endregion

        #region Rule Context
        private sealed class RuleContext {
            public readonly Material mat;
            public readonly List<UnusedTextureInfo> results = new List<UnusedTextureInfo>();
            readonly HashSet<string> animProps;
            readonly bool allowValueRules;
            readonly HashSet<string> added = new HashSet<string>(StringComparer.Ordinal);

            public RuleContext(Material mat, HashSet<string> animProps, bool allowValueRules) {
                this.mat = mat;
                this.animProps = animProps;
                this.allowValueRules = allowValueRules;
            }

            /// <summary>Adds the texture if it's assigned, not animated and not already listed.</summary>
            public void Strip(string property, string reason) {
                if (added.Contains(property) || IsAnimated(property) || !mat.HasProperty(property)) return;
                Texture tex = mat.GetTexture(property);
                if (tex == null) return;
                added.Add(property);
                results.Add(new UnusedTextureInfo(property, tex, reason, BuildOnlyProperties.Contains(property)));
            }

            public bool Kw(string keyword) => mat.IsKeywordEnabled(keyword);

            /// <summary>A keyword-gated feature is off only when the keyword is disabled and the property agrees.</summary>
            public bool Off(string keyword, bool propertySaysOff) => propertySaysOff && !mat.IsKeywordEnabled(keyword);

            /// <summary>A keyword-gated feature is on only when the keyword is enabled and the property agrees.</summary>
            public bool On(string keyword, bool propertySaysOn) => propertySaysOn && mat.IsKeywordEnabled(keyword);

            public bool EmissiveIsBlack => (mat.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) != 0;

            public int Int(string property, int defaultValue = 0) => mat.HasProperty(property) ? mat.GetInt(property) : defaultValue;

            public bool HasTex(string property) => mat.HasProperty(property) && mat.GetTexture(property) != null;

            /// <summary>Runtime branch check: the property exists, equals the value and isn't animated.</summary>
            public bool Is(string property, int value) => mat.HasProperty(property) && mat.GetInt(property) == value && !IsAnimated(property);

            /// <summary>Runtime branch check: the property exists, differs from the value and isn't animated.</summary>
            public bool IsNot(string property, int value) => mat.HasProperty(property) && mat.GetInt(property) != value && !IsAnimated(property);

            public bool IsAnimated(params string[] properties) {
                if (animProps == null || animProps.Count == 0) return false;
                foreach (string property in properties) {
                    if (animProps.Contains(property)) return true;
                }
                return false;
            }

            // Value rules: only used when enabled, since scripts can change these at runtime.
            public bool ZeroFloat(string property) => ValueRule(property) && mat.GetFloat(property) == 0f;

            public bool ZeroVector3(string property) {
                if (!ValueRule(property)) return false;
                Vector4 v = mat.GetVector(property);
                return v.x == 0f && v.y == 0f && v.z == 0f;
            }

            /// <summary>Compares RGB only, since the shaders multiply by the colour's RGB.</summary>
            public bool BlackColor(string property) {
                if (!ValueRule(property)) return false;
                Color col = mat.GetColor(property);
                return col.r == 0f && col.g == 0f && col.b == 0f;
            }

            public bool ZeroAlpha(string property) => ValueRule(property) && mat.GetColor(property).a == 0f;

            public bool ClearColor(string property) {
                if (!ValueRule(property)) return false;
                Color col = mat.GetColor(property);
                return col.r == 0f && col.g == 0f && col.b == 0f && col.a == 0f;
            }

            bool ValueRule(string property) => allowValueRules && mat.HasProperty(property) && !IsAnimated(property);
        }
        #endregion
    }
}
