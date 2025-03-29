#ifndef STANDARD_VERT_INCLUDED
#define STANDARD_VERT_INCLUDED

#if defined(META_PASS)
    #include "UnityMetaPass.cginc"
#endif

v2f vert (appdata v){
    v2f o = (v2f)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    #if defined(META_PASS)
        o.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
    #elif defined(SHADOWCASTER_PASS)
        TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
    #else
        o.pos = UnityObjectToClipPos(v.vertex);
    #endif
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.tangent.xyz = UnityObjectToWorldDir(v.tangent.xyz);
    o.tangent.w = v.tangent.w;
    o.color = v.color;
    o.localPos = v.vertex;
    o.localNorm = v.normal;
    
    InitializeUVs(v, o);

    #if defined(META_PASS)
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
    #else
        #if defined(BASE_PASS)
            o.vertexLightOn = false;
            #if defined(VERTEXLIGHT_ON)
                o.vertexLightOn = true;
            #endif
        #endif
        #if !defined(SHADOWCASTER_PASS)
            UNITY_TRANSFER_SHADOW(o, v.uv1);
        #endif
        UNITY_TRANSFER_FOG(o,o.pos);
    #endif

    return o;
}

#endif