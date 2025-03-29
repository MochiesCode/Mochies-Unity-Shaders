using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public class MochieMaterialUpgrade_V0_To_V1 : MochieMaterialUpgradeBase
    {
        public override List<UpgradeActionBase> AddUpgradeActions()
        {
            return new List<UpgradeActionBase>
            {  
                new CopyFloatPropertyValueAction("_Workflow", "_PrimaryWorkflow"),
                new CopyFloatPropertyValueAction("_SamplingMode","_PrimarySampleMode"),
                new CopyFloatPropertyValueAction("_DetailSamplingMode", "_DetailSampleMode"),
                new CopyFloatPropertyValueAction("_TriplanarSpace", "_TriplanarCoordSpace"),
                new CopyFloatPropertyValueAction("_UseSmoothness", "_SmoothnessToggle"),
                new CopyFloatPropertyValueAction("_UseHeight", "_PackedHeight"),
                new CopyFloatPropertyValueAction("_UseAlphaMask", "_AlphaSource"),
                new CopyTexturePropertyValueAction("_SpecGlossMap", "_RoughnessMap"),
                new CopyTexturePropertyValueAction("_MetallicGlossMap", "_MetallicMap"),
                new CopyFloatPropertyValueAction("_Metallic", "_MetallicStrength"),
                new CopyFloatPropertyValueAction("_Glossiness", "_RoughnessStrength"),
                new CopyTexturePropertyValueAction("_BumpMap", "_NormalMap"),
                new CopyFloatPropertyValueAction("_BumpScale", "_NormalStrength"),
                new CopyFloatPropertyValueAction("_Parallax", "_HeightStrength"),
                new CopyTexturePropertyValueAction("_ParallaxMask", "_HeightMask"),
                new CopyFloatPropertyValueAction("_ParallaxOffset", "_HeightOffset"),
                new CopyFloatPropertyValueAction("_ParallaxSteps", "_HeightSteps"),
                new CopyTexturePropertyValueAction("_ParallaxMap", "_HeightMap"),
                new CopyFloatPropertyValueAction("_RoughnessMult", "_RoughnessMultiplier"),
                new CopyFloatPropertyValueAction("_MetallicMult", "_MetallicMultiplier"),
                new CopyFloatPropertyValueAction("_OcclusionMult", "_OcclusionMultiplier"),
                new CopyFloatPropertyValueAction("_HeightMult", "_HeightMultiplier"),
                new CopyFloatPropertyValueAction("_EmissionIntensity", "_EmissionStrength"),
                new CopyFloatPropertyValueAction("_EmissPulseStrength", "_EmissionPulseStrength"),
                new CopyFloatPropertyValueAction("_EmissPulseSpeed", "_EmissionPulseSpeed"),
                new CopyFloatPropertyValueAction("_EmissPulseWave", "_EmissionPulseWave"),
                new CopyTexturePropertyValueAction("_DetailAlbedoMap", "_DetailMainTex"),
                new CopyFloatPropertyValueAction("_DetailAlbedoBlend", "_DetailMainTexBlend"),
                new CopyTexturePropertyValueAction("_DetailAOMap", "_DetailOcclusionMap"),
                new CopyFloatPropertyValueAction("_DetailAOBlend", "_DetailOcclusionBlend"),
                new CopyFloatPropertyValueAction("_DetailNormalMapScale", "_DetailNormalStrength"),
                new CopyFloatPropertyValueAction("_DetailRoughBlend", "_DetailRoughnessBlend"),
                new CopyFloatPropertyValueAction("_UseFresnel", "_FresnelToggle"),
                new CopyFloatPropertyValueAction("_GlossyReflections", "_ReflectionsToggle"),
                new CopyFloatPropertyValueAction("_SpecularHighlights", "_SpecularHighlightsToggle"),
                new CopyFloatPropertyValueAction("_SpecularStrength", "_SpecularHighlightStrength"),
                new CopyFloatPropertyValueAction("_ReflShadows", "_SpecularOcclusionToggle"),
                new CopyFloatPropertyValueAction("_ReflShadowStrength","_SpecularOcclusionStrength"),
                new CopyFloatPropertyValueAction("_ContrastReflShad", "_SpecularOcclusionContrast"),
                new CopyFloatPropertyValueAction("_BrightnessReflShad", "_SpecularOcclusionBrightness"),
                new CopyFloatPropertyValueAction("_HDRReflShad", "_SpecularOcclusionHDR"),
                new CopyVectorPropertyValueAction("_TintReflShad", "_SpecularOcclusionTint"),
                new CopyFloatPropertyValueAction("_ReflShadowAreaLit", "_AreaLitSpecularOcclusion"),
                new CopyFloatPropertyValueAction("_AreaLitRoughnessMult", "_AreaLitRoughnessMultiplier"),
                new CopyFloatPropertyValueAction("_GSAA", "_GSAAToggle"),
                new CopyFloatPropertyValueAction("_BicubicLightmap", "_BicubicSampling"),
                new CopyFloatPropertyValueAction("_UVPri", "_UVMainSet"),
                new CopyFloatPropertyValueAction("_UV0Rotate", "_UVMainRotation"),
                new CopyVectorPropertyValueAction("_UV0Scroll", "_UVMainScroll"),
                new CopyFloatPropertyValueAction("_UVPriSwizzle", "_UVMainSwizzle"),
                new CopyFloatPropertyValueAction("_UVSec", "_UVDetailSet"),
                new CopyFloatPropertyValueAction("_UV1Rotate", "_UVDetailRotation"),
                new CopyFloatPropertyValueAction("_UV1Scroll", "_UVDetailScroll"),
                new CopyFloatPropertyValueAction("_UVSecSwizzle", "_UVDetailSwizzle"),
                new CopyFloatPropertyValueAction("_UVHeightMask", "_UVHeightMaskSet"),
                new CopyVectorPropertyValueAction("_UV2Scroll", "_UVHeightMaskScroll"),
                new CopyFloatPropertyValueAction("_UVEmissMask", "_UVEmissionMaskSet"),
                new CopyFloatPropertyValueAction("_UVEmissMaskSwizzle", "_UVEmissionMaskSwizzle"),
                new CopyFloatPropertyValueAction("_UV3Rotate", "_UVEmissionMaskRotation"),
                new CopyVectorPropertyValueAction("_UV3Scroll", "_UVEmissionMaskScroll"),
                new CopyFloatPropertyValueAction("_UVAlphaMask", "_UVAlphaMaskSet"),
                new CopyFloatPropertyValueAction("_UV4Rotate", "_UVAlphaMaskRotation"),
                new CopyVectorPropertyValueAction("_UV4Scroll", "_UVAlphaMaskScroll"),
                new CopyFloatPropertyValueAction("_UVRainMask", "_UVRainMaskSet"),
                new CopyFloatPropertyValueAction("_UV5Rotate", "_UVRainMaskRotation"),
                new CopyVectorPropertyValueAction("_UV5Scroll", "_UVRainMaskScroll"),
                new CopyFloatPropertyValueAction("_UVDetailMask", "_UVDetailMaskSet"),
                new CopyFloatPropertyValueAction("_DetailRotate", "_UVDetailMaskRotation"),
                new CopyVectorPropertyValueAction("_DetailScroll", "_UVDetailMaskScroll"),
                new CopyFloatPropertyValueAction("_OcclusionUVSet", "_AreaLitOcclusionUVSet"),
                new CopyFloatPropertyValueAction("_Cull", "_Culling"),
                
                new SetPropertyValueAction("_RainSheet", true, new GUID("df89a63673a32f4438fba4fb13f0f640")),
                new SetPropertyValueAction("_DropletMask", true, new GUID("76ae1285472e6ce48a2f01ef7905b8fd")),
                new SetPropertyValueAction("_DFG", true, new GUID("f8ddbd1e1d2a4415a10b4d48daeba743")),
                new SetPropertyValueAction("_DefaultSampler", true, new GUID("b5f34bbf55503c942821a982c6756e38"))
            };
        }
    }
}