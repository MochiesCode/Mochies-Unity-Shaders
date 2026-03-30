#ifndef GLASS_SHADOW_INCLUDED
#define GLASS_SHADOW_INCLUDED

sampler3D _DitherMaskLOD;

v2f vert (appdata v){
    v2f o = (v2f)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    o.uv = v.uv;

    o.normal = UnityObjectToWorldNormal(v.normal);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);

    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
    return o;
}

float4 frag (v2f i, UNITY_POSITION(vpos), bool isFrontFace : SV_IsFrontFace) : SV_Target {

    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    if (_TexCoordSpace == 1){
        float2 worldYZ = Rotate2D(i.worldPos.yz, -90);
        float2 worldCoordSelect[3] = {i.worldPos.xy, -i.worldPos.xz, worldYZ}; 
        float2 worldCoords = worldCoordSelect[_TexCoordSpaceSwizzle];
        i.uv.xy = worldCoords;
    }
    i.uv.xy *= abs(_GlobalTexCoordScale);

    // #if !(IS_OPAQUE)
    //     float4 baseColorTex = SampleTexture(_MainTex, TRANSFORM_TEX(i.uv, _MainTex)) * _BaseColorTint;
    //     float alphaRef = tex3D(_DitherMaskLOD, float3(vpos.xy*0.25,baseColorTex.a*0.9375)).a;
    //     clip (alphaRef - 0.01);
    // #endif

    SHADOW_CASTER_FRAGMENT(i)
}

#endif