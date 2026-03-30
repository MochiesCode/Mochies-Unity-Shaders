#ifndef GLASS_META_INCLUDED
#define GLASS_META_INCLUDED

#include "UnityMetaPass.cginc"

v2f vert_meta (appdata v) {
    v2f o = (v2f)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    o.uv = v.uv;
    o.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    #if defined(LIGHTMAP_ON)
        o.lightmapUV.xy = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
    #endif
    #if defined(DYNAMICLIGHTMAP_ON)
        o.lightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif

    #ifdef EDITOR_VISUALIZATION
        o.vizUV = 0;
        o.lightCoord = 0;
        if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
            o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.uv.xy, v.uv1.xy, v.uv2.xy, unity_EditorViz_Texture_ST);
        else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
        {
            o.vizUV = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
            o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
        }
    #endif

    return o;
}

float4 frag_meta (v2f i, bool isFrontFace : SV_IsFrontFace) : SV_Target {

    if (_TexCoordSpace == 1){
        float2 worldYZ = Rotate2D(i.worldPos.yz, -90);
        float2 worldCoordSelect[3] = {i.worldPos.xy, -i.worldPos.xz, worldYZ}; 
        float2 worldCoords = worldCoordSelect[_TexCoordSpaceSwizzle];
        i.uv.xy = worldCoords;
    }
    i.uv.xy *= abs(_GlobalTexCoordScale);

    float4 baseColor = SampleTexture(_MainTex, TRANSFORM_TEX(i.uv, _MainTex)) * _BaseColorTint;
    float metallic = SampleTexture(_MetallicMap, TRANSFORM_TEX(i.uv, _MetallicMap)) * _Metallic;
    float roughness = SampleTexture(_RoughnessMap, TRANSFORM_TEX(i.uv, _RoughnessMap)) * _Roughness;
    float3 specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, 1, metallic);

    UnityMetaInput o = (UnityMetaInput)0;
    #ifdef EDITOR_VISUALIZATION
        o.Albedo = baseColor;
        o.VizUV = i.vizUV;
        o.LightCoord = i.lightCoord;
    #else
        o.Albedo = baseColor + specularTint * ((roughness * roughness) * 0.5);
    #endif
    o.SpecularColor = specularTint;
    o.Emission = GetEmission(i, 0, 0, 0);
    
    return UnityMetaFragment(o);
}

#endif