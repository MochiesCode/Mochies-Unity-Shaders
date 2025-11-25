#ifndef STANDARD_VERT_MANIP_INCLUDED
#define STANDARD_VERT_MANIP_INCLUDED

float3 FlowUV (float2 uv, float2 flowVector, float time, float phase) {
    float progress = frac(time + phase);
    float3 uvw;
    uvw.xy = uv - flowVector * progress;
    uvw.xy += phase;
    uvw.xy += (time - progress) * float2(0.1, 0.25);
    uvw.z = 1 - abs(1 - 2 * progress);
    return uvw;
}

float3 GetVertexManipulation(appdata v, inout v2f o){
    float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
    float3 worldOrigin = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
    worldPos -= worldOrigin;
    float2 vertexMaskUV = ScaleOffsetRotateScrollUV(v.uv0, _VertexMask_ST.xy, _VertexMask_ST.zw, _UVVertexMaskRotation, _UVVertexMaskScroll);
    float4 vertexMask = MOCHIE_SAMPLE_TEX2D_LOD(_VertexMask, vertexMaskUV, 0);
    float3 animatedRotation = _VertexRotationAnimated * _Time.y * 10 * vertexMask;
    float3 staticRotation = _VertexRotationStatic * vertexMask;
    float3 staticOffset = _VertexOffset * vertexMask;
    worldPos = Rotate3D(worldPos, staticRotation + animatedRotation);
    worldPos += staticOffset;
    
    [branch]
    if (_WindToggle == 1 && _WindStrength != 0){
        if (_WindMaskingMode == 1)
            vertexMask = v.color;
        
        float3 noise0 = 0;
        float3 noise1 = 0;
        float3 noise2 = 0;

        float2 noiseUV0 = worldPos.xz * _WindScale0 + _Time.y * _WindSpeed0; 
        noise0 = MOCHIE_SAMPLE_TEX2D_LOD(_WindNoiseTex, noiseUV0, _WindSmoothness0) * _WindContribution0;
        noise0 = Remap(noise0, 0, 1, -1, 1);

        if (_WindLayers > 1){
            float2 noiseUV1 = worldPos.yz * _WindScale1 + _Time.y * _WindSpeed1;
            noise1 = MOCHIE_SAMPLE_TEX2D_LOD(_WindNoiseTex, noiseUV1, _WindSmoothness1) * _WindContribution1;
            noise1 = Remap(noise1, 0, 1, -1, 1);
        }

        if (_WindLayers > 2){
            float2 noiseUV2 = worldPos.xy * _WindScale2 + _Time.y * _WindSpeed2;
            noise2 = MOCHIE_SAMPLE_TEX2D_LOD(_WindNoiseTex, noiseUV2, _WindSmoothness2) * _WindContribution2;
            noise2 = Remap(noise2, 0, 1, -1, 1);
        }

        float3 noise = (noise0 + noise1 + noise2);
        noise *= _WindDirection * _WindStrength * vertexMask;
        o.wind = noise;
        
        if (_WindSymmetry != 3){
            if (v.vertex[_WindSymmetry] > 0)
                worldPos = Rotate3D(worldPos, noise);
            else if (v.vertex[_WindSymmetry] < 0)
                worldPos = Rotate3D(worldPos, -noise);
        }
        else {
            worldPos = Rotate3D(worldPos, noise);
        }
    }

    return worldPos + worldOrigin;
}
#endif