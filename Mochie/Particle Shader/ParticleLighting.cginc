#ifndef PARTICLE_LIGHTING_INCLUDED
#define PARTICLE_LIGHTING_INCLUDED

float3 GetSH(v2f i, InputData id){
    [branch]
    if (_UdonLightVolumeEnabled == 1 && _LightVolumes != 0){
        LightVolumeSH(i.worldPos, lightVolumeL0, lightVolumeL1r, lightVolumeL1g, lightVolumeL1b);
        return LightVolumeEvaluate(id.normal, lightVolumeL0, lightVolumeL1r, lightVolumeL1g, lightVolumeL1b) * _LightVolumeStrength;
    }
    else {
        return max(0, ShadeSH9(float4(id.normal, 1)));
    }
}

float3 GetRealtimeIndirectLighting(v2f i, InputData id){
    float3 indirectCol = 0;
    #if UNITY_LIGHT_PROBE_PROXY_VOLUME
        if (unity_ProbeVolumeParams.x == 1){
            indirectCol = max(0, SHEvalLinearL0L1_SampleProbeVolume(float4(id.normal, 1), i.worldPos));
        }
        else {
            indirectCol = GetSH(i, id);
        }
    #else
        indirectCol = GetSH(i, id);
    #endif
    return indirectCol;
}

float3 Shade4PointLightsNoPopIn(
    float4 lightPosX, float4 lightPosY, float4 lightPosZ,
    float3 lightColor0, float3 lightColor1, float3 lightColor2, float3 lightColor3,
    float4 lightAttenSq,
    float3 pos, float3 normal)
{
    float4 toLightX = lightPosX - pos.x;
    float4 toLightY = lightPosY - pos.y;
    float4 toLightZ = lightPosZ - pos.z;

    float4 lengthSq = 0;
    lengthSq += toLightX * toLightX;
    lengthSq += toLightY * toLightY;
    lengthSq += toLightZ * toLightZ;

    lengthSq = max(lengthSq, 0.000001);

    float4 ndotl = 0;
    ndotl += toLightX * normal.x;
    ndotl += toLightY * normal.y;
    ndotl += toLightZ * normal.z;

    float4 corr = rsqrt(lengthSq);
    ndotl = max (float4(0,0,0,0), ndotl * corr);

    float4 atten = 1.0 / (1.0 + lengthSq * lightAttenSq);
    atten = linearstep(float4(0.05,0.05,0.05,0.05), float4(1,1,1,1), atten);
    float4 diff = ndotl * atten;

    float3 col = 0;
    col += lightColor0 * diff.x;
    col += lightColor1 * diff.y;
    col += lightColor2 * diff.z;
    col += lightColor3 * diff.w;
    return col;
}

float3 GetVertexLightColor(InputData id, v2f i){
    float3 vLightCol = 0;
    #if defined(UNITY_PASS_FORWARDBASE)
        if (i.vertexLightOn){
            vLightCol = Shade4PointLightsNoPopIn(unity_4LightPosX0, unity_4LightPosY0, 
                unity_4LightPosZ0, unity_LightColor[0].rgb, 
                unity_LightColor[1].rgb, unity_LightColor[2].rgb, 
                unity_LightColor[3].rgb, unity_4LightAtten0, 
                i.worldPos, id.normal
            );
        }
    #endif
    return vLightCol;
}

void InitializeLightingData(v2f i, InputData id, inout LightingData ld, float atten){

    float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
    float omr = unity_ColorSpaceDielectricSpec.a - id.metallic * unity_ColorSpaceDielectricSpec.a;
    float3 lightDir = Unity_SafeNormalize(UnityWorldSpaceLightDir(i.worldPos));
    float NdotL = saturate(dot(id.normal, lightDir));
    bool isRealtime = any(_WorldSpaceLightPos0.xyz);

    float3 directCol = (_LightColor0 * atten * NdotL) + GetVertexLightColor(id, i);
    float3 indirectCol = GetRealtimeIndirectLighting(i, id);

    ld.lightCol = (indirectCol + directCol) * omr;
    ld.directCol = directCol;
    ld.indirectCol = indirectCol;
    ld.lightDir = lightDir;
    ld.viewDir = viewDir;
    ld.NdotL = NdotL;
    ld.atten = atten;
    ld.omr = omr;
    ld.isRealtime = isRealtime;
}

#endif