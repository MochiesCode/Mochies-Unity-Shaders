#ifndef WATER_META_INCLUDED
#define WATER_META_INCLUDED

#include "UnityMetaPass.cginc"

v2f vert_meta (appdata v) {
    v2f o = (v2f)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    float2 uvs[] = {v.uv, v.uv1, v.uv2, v.uv3};
    o.uvFlow = uvs[_FlowMapUV].xy;
    
    o.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    #if defined(LIGHTMAP_ON)
        o.lightmapUV.xy = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
    #endif
    #if defined(DYNAMICLIGHTMAP_ON)
        o.lightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif
    o.localPos = v.vertex;

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

    float2 uvFlow = ScaleUV(i.uvFlow, _FlowMapScale, 0);
    float4 flowMap = MOCHIE_SAMPLE_TEX2D(_FlowMap, uvFlow);
    flowMap = lerp(0, flowMap, _ZeroProp);

    float2 emissionUV = TRANSFORM_TEX(i.uv, _EmissionMap) + (_Time.y * _EmissionMapScroll);
    float3 uvE0 = float3(emissionUV, 1);
    float3 uvE1 = uvE0;

    #if FLOW_ENABLED
        float blendNoise = flowMap.a;
        if (_BlendNoiseSource == 1){
            float2 uvBlend = ScaleUV(i.uv, _BlendNoiseScale, 0);
            blendNoise = MOCHIE_SAMPLE_TEX2D_SAMPLER_LOD(_BlendNoise, sampler_FlowMap, uvBlend, 0);
        }
        float2 flow = (flowMap.rg * 2 - 1) * _FlowStrength * 0.1;
        float time = _Time.y * _FlowSpeed + blendNoise;
        #if EMISSION_ENABLED
            uvE0 = FlowUV(emissionUV, flow, time, 0);
            uvE1 = FlowUV(emissionUV, flow, time, 0.5);
        #endif
    #endif

    float4 surfaceTint = 1;
    #if TRANSPARENCY_OPAQUE || TRANSPARENCY_PREMUL
        surfaceTint = _NonGrabColor;
        if (!isFrontFace)
            surfaceTint = _NonGrabBackfaceTint;
    #else
        surfaceTint = _Color;
        if (!isFrontFace)
            surfaceTint = _BackfaceTint;
    #endif
    
    float2 mainTexUV = TRANSFORM_TEX(i.uv, _MainTex) + _Time.y * 0.1 * _MainTexScroll;
    #if BASECOLOR_STOCHASTIC_ENABLED
        float4 mainTex = tex2Dstoch(_MainTex, sampler_FlowMap, mainTexUV) * surfaceTint;
    #else
        float4 mainTex = MOCHIE_SAMPLE_TEX2D_SAMPLER(_MainTex, sampler_FlowMap, mainTexUV) * surfaceTint;
    #endif

    float2 detailUV = i.uv;
    float4 detailBC = 0;
    #if DETAIL_BASECOLOR_ENABLED
        detailUV = TRANSFORM_TEX(i.uv, _DetailBaseColor) + (_Time.y * _DetailScroll);
    #elif DETAIL_NORMAL_ENABLED
        detailUV = TRANSFORM_TEX(i.uv, _DetailNormal) + (_Time.y * _DetailScroll);
    #endif
    
    #if DETAIL_BASECOLOR_ENABLED
        detailBC = MOCHIE_SAMPLE_TEX2D_SAMPLER(_DetailBaseColor, sampler_FlowMap, detailUV) * _DetailBaseColorTint;
        mainTex.rgb = lerp(mainTex.rgb, detailBC.rgb, detailBC.a);
    #endif

    mainTex += flowMap;

    float3 emissCol = 0;
    #if EMISSION_ENABLED
        #if EMISS_STOCHASTIC_ENABLED
            #if FLOW_ENABLED && EMISSION_FLOW_ENABLED
                float3 emissCol0 = tex2Dstoch(_EmissionMap, sampler_FlowMap, uvE0.xy) * uvE0.z;
                float3 emissCol1 = tex2Dstoch(_EmissionMap, sampler_FlowMap, uvE1.xy) * uvE1.z;
                emissCol = emissCol0 + emissCol1;
            #else
                emissCol = tex2Dstoch(_EmissionMap, sampler_FlowMap, emissionUV);
            #endif
        #else
            #if FLOW_ENABLED && EMISSION_FLOW_ENABLED
                float3 emissCol0 = MOCHIE_SAMPLE_TEX2D_SAMPLER(_EmissionMap, sampler_FlowMap, uvE0.xy) * uvE0.z;
                float3 emissCol1 = MOCHIE_SAMPLE_TEX2D_SAMPLER(_EmissionMap, sampler_FlowMap, uvE1.xy) * uvE1.z;
                emissCol = emissCol0 + emissCol1;
            #else	
                emissCol = MOCHIE_SAMPLE_TEX2D_SAMPLER(_EmissionMap, sampler_FlowMap, emissionUV);
            #endif
        #endif
        emissCol *= _EmissionColor;
        #if AUDIOLINK_ENABLED
            audioLinkData al = (audioLinkData)0;
            InitializeAudioLink(al);
            float audiolink = GetAudioLinkBand(al, _AudioLinkBand);
            emissCol *= audiolink;
        #endif
    #endif
    #if DEPTH_EFFECTS_ENABLED
        #if DEPTHFOG_ENABLED
            emissCol += _FogTint * _FogBrightness * _FogContribution * _FogTint.a;
        #endif
    #endif

    float roughness = 0;
    float metallic = 0;
    #if PBR_ENABLED
        float2 roughnessMapUV = detailUV;
        if (_DetailTextureMode != 1)
            roughnessMapUV = TRANSFORM_TEX(i.uv, _RoughnessMap);
        float roughnessMap = MOCHIE_SAMPLE_TEX2D_SAMPLER(_RoughnessMap, sampler_FlowMap, roughnessMapUV);
        #if DETAIL_BASECOLOR_ENABLED
            if (_DetailTextureMode == 1)
                roughnessMap *= detailBC.a;
        #endif
        float rough = roughnessMap * _Roughness;
        #if FOAM_ENABLED
            float foamLerp = (foam + crestFoam);
            rough = lerp(rough, _FoamRoughness, foamLerp*2);
        #endif
        float roughSq = rough * rough;
        float roughBRDF = max(roughSq, 0.003);
        
        float2 metallicMapUV = detailUV;
        if (_DetailTextureMode != 1)
            metallicMapUV = TRANSFORM_TEX(i.uv, _MetallicMap);
        float metallicMap = MOCHIE_SAMPLE_TEX2D_SAMPLER(_MetallicMap, sampler_FlowMap, metallicMapUV);
        #if DETAIL_BASECOLOR_ENABLED
            if (_DetailTextureMode == 1)
                metallicMap *= detailBC.a;
        #endif
        metallic = metallicMap * _Metallic;
    #endif
    
    float3 specularColor = lerp(unity_ColorSpaceDielectricSpec.rgb, mainTex, metallic);

    UnityMetaInput o = (UnityMetaInput)0;
    #ifdef EDITOR_VISUALIZATION
        o.Albedo = mainTex;
        o.VizUV = i.vizUV;
        o.LightCoord = i.lightCoord;
    #else
        o.Albedo = mainTex + specularColor * ((roughness * roughness) * 0.5);
    #endif
    o.SpecularColor = specularColor;
    o.Emission = emissCol;
    
    return UnityMetaFragment(o);

}
#endif // WATER_META_INCLUDED