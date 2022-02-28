#ifndef MOCHIE_STANDARD_BRDF_INCLUDED
#define MOCHIE_STANDARD_BRDF_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityLightingCommon.cginc"
#include "MochieStandardSSR.cginc"
#include "MochieStandardSSS.cginc"

#ifdef LTCGI
	#include "Assets/_pi_/_LTCGI/Shaders/LTCGI.cginc"
#endif

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

	#if SUBSURFACE_ENABLED
		half3 diffCol = diffColor * (gi.diffuse + light.color * lerp(diffuseTerm, wrappedDiffuse, thickness));
	#else
		half3 diffCol = diffColor * (gi.diffuse + light.color * diffuseTerm);
	#endif
	half3 specCol = specularTerm * light.color * FresnelTerm (specColor, lh) * _SpecularStrength;
	half3 reflCol = surfaceReduction * gi.specular * FresnelLerp (specColor, grazingTerm, lerp(1, nv, _FresnelStrength*_UseFresnel)) * _ReflectionStrength;
	#if SSR_ENABLED
		half4 ssrCol = GetSSR(worldPos, viewDir, reflect(-viewDir, normal), normal, smoothness, diffColor, metallic, screenUVs, screenPos);
		ssrCol.rgb *= _SSRStrength;
		reflCol = lerp(reflCol, ssrCol.rgb, ssrCol.a);
		specCol *= (1-smoothstep(0, 0.1, ssrCol.a));
	#endif

	half3 subsurfaceCol = 0;
	#if SUBSURFACE_ENABLED
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
	#endif
	
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
        diffCol += diffColor * diffLight;
    #endif

	#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
		if (_ReflShadows == 1){
			float lightmap = Desaturate(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV)));
			lightmap = GetContrast(lightmap, _ContrastReflShad);
			lightmap = lerp(lightmap, GetHDR(lightmap), _HDRReflShad);
			lightmap *= _BrightnessReflShad;
			reflCol *= saturate(lerp(1, lightmap, _ReflShadowStrength));
		}
	#else
		reflCol *= lerp(1, lerp(1, atten, 0.9), _ReflShadows*_ReflShadowStrength);
	#endif
	reflCol *= lerp(1, vertexColor, _ReflVertexColor*_ReflVertexColorStrength);

    return half4(diffCol + specCol + reflCol + subsurfaceCol, 1);
}

// half4 BRDF2_Mochie_PBS (
// 	half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
//     half3 normal, half3 viewDir, half3 worldPos, half2 screenUVs, half4 screenPos,
//     half metallic, half thickness, half3 ssColor, half atten, UnityLight light, UnityIndirect gi)
// {
//     float3 halfDir = Unity_SafeNormalize(float3(light.dir) + viewDir);

//     half nl = saturate(dot(normal, light.dir));
//     float nh = saturate(dot(normal, halfDir));
//     half nv = saturate(dot(normal, viewDir));
//     float lh = saturate(dot(light.dir, halfDir));

//     // Specular term
//     half perceptualRoughness = SmoothnessToPerceptualRoughness(smoothness);
//     half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);

// 	#if defined(_SPECULARHIGHLIGHTS_OFF)
// 		half specularTerm = 0.0;
// 	#else
// 		#if UNITY_BRDF_GGX
// 			half a = roughness;
// 			float a2 = a*a;
// 			float d = nh * nh * (a2 - 1.f) + 1.00001f;
// 			#ifdef UNITY_COLORSPACE_GAMMA
// 				float specularTerm = a / (max(0.32f, lh) * (1.5f + roughness) * d);
// 			#else
// 				float specularTerm = a2 / (max(0.1f, lh*lh) * (roughness + 0.5f) * (d * d) * 4);
// 			#endif
// 			#if defined (SHADER_API_MOBILE)
// 				specularTerm = specularTerm - 1e-4f;
// 			#endif
// 		#else
// 			half specularPower = PerceptualRoughnessToSpecPower(perceptualRoughness);
// 			half invV = lh * lh * smoothness + perceptualRoughness * perceptualRoughness;
// 			half invF = lh;
// 			half specularTerm = ((specularPower + 1) * pow (nh, specularPower)) / (8 * invV * invF + 1e-4h);
// 			#ifdef UNITY_COLORSPACE_GAMMA
// 				specularTerm = sqrt(max(1e-4f, specularTerm));
// 			#endif
// 		#endif

// 		#if defined (SHADER_API_MOBILE)
// 			specularTerm = clamp(specularTerm, 0.0, 100.0);
// 		#endif
// 	#endif

// 	#ifdef UNITY_COLORSPACE_GAMMA
// 		half surfaceReduction = 0.28;
// 	#else
// 		half surfaceReduction = (0.6-0.08*perceptualRoughness);
// 	#endif

//     surfaceReduction = 1.0 - roughness*perceptualRoughness*surfaceReduction;

//     half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));

// 	half3 diffCol = (diffColor + specularTerm * specColor * _SpecularStrength) * light.color * nl + gi.diffuse * diffColor;
// 	half3 reflCol = surfaceReduction * gi.specular * FresnelLerpFast(specColor, grazingTerm, nv) * _ReflectionStrength;
// 	#if SSR_ENABLED
// 		half4 ssrCol = GetSSR(worldPos, viewDir, reflect(-viewDir, normal), normal, smoothness, diffColor, metallic, screenUVs, screenPos);
// 		ssrCol.rgb *= _SSRStrength;
// 		reflCol = lerp(reflCol, ssrCol.rgb, ssrCol.a);
// 	#endif
// 	reflCol *= lerp(1, lerp(1, atten, 0.5), _ReflShadows);

//     return half4(diffCol + reflCol, 1);
// }

#endif // MOCHIE_STANDARD_BRDF_INCLUDED