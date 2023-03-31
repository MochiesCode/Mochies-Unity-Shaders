#ifndef MOCHIE_STANDARD_BRDF_INCLUDED
#define MOCHIE_STANDARD_BRDF_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityLightingCommon.cginc"
#include "MochieStandardSSR.cginc"
#include "MochieStandardSSS.cginc"

float3 get_camera_pos() {
	float3 worldCam;
	worldCam.x = unity_CameraToWorld[0][3];
	worldCam.y = unity_CameraToWorld[1][3];
	worldCam.z = unity_CameraToWorld[2][3];
	return worldCam;
}

half4 BRDF1_Mochie_PBS (
	half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
    half3 normal, half3 viewDir, half3 worldPos, half2 screenUVs, half4 screenPos,
    half metallic, half thickness, half3 ssColor, half atten, float2 lightmapUV, float3 vertexColor,
	UnityLight light, UnityIndirect gi)
{
	
    half perceptualRoughness = SmoothnessToPerceptualRoughness(smoothness);
	if (_GSAA == 1){
		perceptualRoughness = GSAARoughness(normal, perceptualRoughness);
	}
    half3 halfDir = Unity_SafeNormalize (half3(light.dir) + viewDir);
    half nv = abs(dot(normal, viewDir));
    half nl = saturate(dot(normal, light.dir));
    half nh = saturate(dot(normal, halfDir));
    half lv = saturate(dot(light.dir, viewDir));
    half lh = saturate(dot(light.dir, halfDir));

    // Diffuse term
    half diffuseTerm = DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;
	float wrappedDiffuse = saturate((diffuseTerm + _WrappingFactor) /
	(1.0f + _WrappingFactor)) * 2 / (2 * (1 + _WrappingFactor));

    // Specular term
    half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
	#if UNITY_BRDF_GGX
		roughness = max(roughness, 0.002);
		half V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
		half D = GGXTerm(nh, roughness);
	#else
		half V = SmithBeckmannVisibilityTerm (nl, nv, roughness);
		half D = NDFBlinnPhongNormalizedTerm (nh, PerceptualRoughnessToSpecPower(perceptualRoughness));
	#endif

	#if defined(_SPECULARHIGHLIGHTS_OFF)
		half specularTerm = 0.0;
	#else
		half specularTerm = V*D * UNITY_PI;
		#ifdef UNITY_COLORSPACE_GAMMA
			specularTerm = sqrt(max(1e-4h, specularTerm));
		#endif
		specularTerm = max(0, specularTerm * nl);
	#endif
    half surfaceReduction;
	#ifdef UNITY_COLORSPACE_GAMMA
		surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;
	#else
		surfaceReduction = 1.0 / (roughness*roughness + 1.0);
	#endif

    half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));

	half3 diffCol = 0;
	if (_Subsurface == 1)
		diffCol = diffColor * (gi.diffuse + light.color * lerp(diffuseTerm, wrappedDiffuse, thickness));
	else
		diffCol = diffColor * (gi.diffuse + light.color * diffuseTerm);

	half3 specCol = specularTerm * light.color * FresnelTerm (specColor, lh) * _SpecularStrength;
	half3 reflCol = surfaceReduction * gi.specular * FresnelLerp (specColor, grazingTerm, lerp(1, nv, _FresnelStrength*_UseFresnel)) * _ReflectionStrength;
	#if SSR_ENABLED
		half4 ssrCol = GetSSR(worldPos, viewDir, reflect(-viewDir, normal), normal, smoothness, diffColor, metallic, screenUVs, screenPos);
		ssrCol.rgb *= _SSRStrength;
		reflCol = lerp(reflCol, ssrCol.rgb, ssrCol.a);
		specCol *= (1-smoothstep(0, 0.1, ssrCol.a));
	#endif

	half3 subsurfaceCol = 0;
	if (_Subsurface == 1){
		subsurfaceCol = GetSubsurfaceLight(
							light.color, 
							light.dir, 
							normal, 
							viewDir, 
							atten, 
							thickness, 
							gi.diffuse, 
							ssColor
						);
	}

	#ifdef LTCGI
        half3 diffLight = 0;
        LTCGI_Contribution(
            worldPos, 
            normal, 
            viewDir, 
            perceptualRoughness,
            (lightmapUV - unity_LightmapST.zw) / unity_LightmapST.xy,
            diffLight
            #ifndef GLOSSYREFLECTIONS_OFF
                , reflCol
            #endif
        );
        diffCol += (diffColor * diffLight) * _LTCGIStrength;
    #endif

	#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
		if (_ReflShadows == 1){
			float3 lightmap = Desaturate(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV)));
			lightmap = GetContrast(lightmap, _ContrastReflShad);
			lightmap = lerp(lightmap, GetHDR(lightmap), _HDRReflShad);
			lightmap *= _BrightnessReflShad;
			lightmap *= _TintReflShad;
			shadowedReflections = saturate(lerp(1, lightmap, _ReflShadowStrength));
			reflCol *= shadowedReflections;
		}
	#else
		shadowedReflections = lerp(1, lerp(1, atten, 0.9), _ReflShadows*_ReflShadowStrength);
		reflCol *= shadowedReflections;
	#endif

	#ifdef FULL_VERSION
		reflCol *= lerp(1, vertexColor, _ReflVertexColor*_ReflVertexColorStrength);
	#endif

    return half4(diffCol + specCol + reflCol + subsurfaceCol, 1);
}

#endif // MOCHIE_STANDARD_BRDF_INCLUDED