// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_STANDARD_META_INCLUDED
#define UNITY_STANDARD_META_INCLUDED

// Functionality for Standard shader "meta" pass
// (extracts albedo/emission for lightmapper etc.)

#include "UnityCG.cginc"
#include "MochieStandardInput.cginc"
#include "UnityMetaPass.cginc"
#include "MochieStandardCore.cginc"

struct v2f_meta
{
    float4 pos      : SV_POSITION;
    float4 uv0      : TEXCOORD0;
	float4 uv1		: TEXCOORD1;
	float4 uv2		: TEXCOORD2;
	float4 uv3		: TEXCOORD3;
	#ifdef EDITOR_VISUALIZATION
		float2 vizUV        : TEXCOORD4;
		float4 lightCoord   : TEXCOORD5;
	#endif
	float4 localPos : TEXCOORD6;
	float3 normal 	: NORMAL;
};

v2f_meta vert_meta (VertexInput v)
{
    v2f_meta o;
    o.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
	float4 dummyUV0 = 0;
    TexCoords(v, o.uv0, o.uv1, o.uv2, dummyUV0, o.uv3);
	o.localPos = v.vertex;
	o.normal = UnityObjectToWorldNormal(v.normal);
	#ifdef EDITOR_VISUALIZATION
		o.vizUV = 0;
		o.lightCoord = 0;
		if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
			o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.uv0.xy, v.uv1.xy, v.uv2.xy, unity_EditorViz_Texture_ST);
		else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
		{
			o.vizUV = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
			o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
		}
	#endif
    return o;
}

// Albedo for lightmapping should basically be diffuse color.
// But rough metals (black diffuse) still scatter quite a lot of light around, so
// we want to take some of that into account too.
// half3 UnityLightmappingAlbedo (half3 diffuse, half3 specular, half smoothness)
// {
//     half roughness = SmoothnessToRoughness(smoothness);
//     half3 res = diffuse;
//     res += specular * roughness * 0.5;
//     return res;
// }

SampleData SampleDataSetup(v2f_meta i){
	SampleData sd = (SampleData)0;
	sd.localPos = i.localPos;
	sd.normal = i.normal;
	sd.scaleTransform = _MainTex_ST;
	return sd;
}

float4 frag_meta (v2f_meta i) : SV_Target
{
    // we're interested in diffuse & specular colors,
    // and surface roughness to produce final albedo.
	i.normal = normalize(i.normal);
	SampleData sd = SampleDataSetup(i);
    FragmentCommonData s = UNITY_SETUP_BRDF_INPUT(i.uv0, i.uv3, sd);

	#if AREALIT_ENABLED
	    float perceptualRoughness = SmoothnessToPerceptualRoughness(s.smoothness);
        float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
		AreaLightFragInput ai;
		ai.pos = s.posWorld;
		ai.normal = s.normalWorld;
		ai.view = s.normalWorld;
		ai.roughness = roughness;
		ai.occlusion = 1;
		half4 diffTerm, specTerm;
		ShadeAreaLights(ai, diffTerm, specTerm, true, false);
	#endif

    UnityMetaInput o;
    UNITY_INITIALIZE_OUTPUT(UnityMetaInput, o);

	#if defined(BAKERY_META)
	if (unity_MetaFragmentControl.w){
		#ifdef _ALPHAMASK_ON
			return Alpha(i.uv2.xy, sd);
		#else
			return Alpha(i.uv0, sd);
		#endif
	}
	#endif

	#ifdef EDITOR_VISUALIZATION
		o.Albedo = s.diffColor;
		o.VizUV = i.vizUV;
		o.LightCoord = i.lightCoord;
	#else
		o.Albedo = s.diffColor + s.specColor * (SmoothnessToRoughness(s.smoothness)/2); // UnityLightmappingAlbedo
	#endif
    o.SpecularColor = s.specColor;
    o.Emission = Emission(i.uv0.xy, i.uv1.zw, sd);
	#if AREALIT_ENABLED
		o.Emission += (s.diffColor + s.specColor) * diffTerm;
	#endif
    return UnityMetaFragment(o);
}

#endif // UNITY_STANDARD_META_INCLUDED