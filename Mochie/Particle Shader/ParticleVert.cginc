#ifndef PARTICLE_VERT_INCLUDED
#pragma exclude_renderers gles
#define PARTICLE_VERT_INCLUDED

v2f vert (appdata v){
    v2f o = (v2f)0;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_OUTPUT(v2f, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    #if defined(UNITY_PARTICLE_INSTANCING_ENABLED)
        UNITY_PARTICLE_INSTANCE_DATA data = unity_ParticleInstanceData[unity_InstanceID];
        float3 texcoord2AndBlend;
        vertInstancingUVs(v.uv.xy, o.uv0.xy, texcoord2AndBlend);
        vertInstancingColor(o.color);
        o.uv0.zw = texcoord2AndBlend.xy;
        o.animBlend = texcoord2AndBlend.z;
        o.center = data.center;
        o.agePercent = data.agePercent;
        o.stableRandom = data.stableRandom;
    #else
        o.color = v.color;
        o.uv0 = v.uv;
        o.animBlend = v.uv1.x;
        o.center = float3(v.uv1.zw, v.uv2.x);
        o.agePercent =  v.uv2.y;
        o.stableRandom = float4(v.uv2.zw, v.uv3.xy);
    #endif
    
    o.pos = UnityObjectToClipPos(v.vertex);
    o.projPos = GetProjPos(v.vertex.xyzz, o.pos);
    o.worldPos = mul((float3x4)unity_ObjectToWorld, float4(v.vertex.xyz, 1));
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.tangent.xyz = UnityObjectToWorldDir(v.tangent.xyz);
    o.tangent.w = v.tangent.w;

    #if defined(_FALLOFF_ON)
        o.falloff = GetFalloff(o);
        [branch]
        if (o.falloff <= 0.0001)
            o.pos = 0.0/_NaNLmao;
    #endif

    #if defined(_PULSE_ON)
        o.pulse = GetPulse();
    #endif

    #if defined(_DISTORTION_ON)
        o.uvGrab = ComputeGrabScreenPos(o.pos);
    #endif

    #if defined(UNITY_PASS_FORWARDBASE)
        o.vertexLightOn = false;
        #if defined(VERTEXLIGHT_ON)
            o.vertexLightOn = true;
        #endif
    #endif

    #if defined(UNITY_PASS_SHADOWCASTER)
        TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
    #else
        UNITY_TRANSFER_SHADOW(o, v.uv1);
        UNITY_TRANSFER_FOG(o,o.pos);
    #endif
    return o;
}

#endif // PARTICLE_VERT_INCLUDED