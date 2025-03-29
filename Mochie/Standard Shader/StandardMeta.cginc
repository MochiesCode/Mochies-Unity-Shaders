#ifndef STANDARD_META_DEFINED
#define STANDARD_META_DEFINED

float4 frag (v2f i, bool isFrontFace : SV_IsFrontFace) : SV_Target {
    
    InitializeDefaultSampler(defaultSampler);
    
    float3x3 tangentToWorld;
    float3 viewDir, tangentViewDir;
    CalculateViewDirection(i, viewDir, tangentViewDir, tangentToWorld);
    
    // For triplanar
    // Easier to just make these global instead of making a struct and passing them around
    worldVertexPos = i.worldPos;
    worldVertexNormal = i.normal;
    localVertexPos = i.localPos;
    localVertexNormal = i.localNorm;

    InputData id = (InputData)0;
    InitializeInputData(i, id, tangentToWorld, isFrontFace);
    id.baseColor.r += defaultSampler.r;

    float3 specularColor = lerp(unity_ColorSpaceDielectricSpec.rgb, id.baseColor, id.metallic);

    UnityMetaInput o = (UnityMetaInput)0;
    #ifdef EDITOR_VISUALIZATION
        o.Albedo = id.baseColor;
        o.VizUV = i.vizUV;
        o.LightCoord = i.lightCoord;
    #else
        o.Albedo = id.baseColor + specularColor * ((id.roughness * id.roughness) * 0.5);
    #endif
    o.SpecularColor = specularColor;
    o.Emission = id.emission;
    
    return UnityMetaFragment(o);
}

#endif