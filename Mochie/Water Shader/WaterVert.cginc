#ifndef WATER_VERT_INCLUDED
#define WATER_VERT_INCLUDED

v2f vert (
    #ifdef TESSELLATION_VARIANT
        TessellationControlPoint v
    #else
        appdata v
    #endif
) {
    v2f o = (v2f)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.cameraPos = _WorldSpaceCameraPos;
    #if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
        o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*0.5;
    #endif
    
    float2 uvs[] = {v.uv, v.uv1, v.uv2, v.uv3};
    o.uvFlow = uvs[_FlowMapUV].xy;
    o.uvFoamMap = TRANSFORM_TEX(uvs[_FoamMapUVSet], _FoamMap);
    o.uv = v.uv;
    v.tangent.w = v.tangent.w * unity_WorldTransformParams.w;
    #if !GERSTNER_ENABLED
        o.normal = UnityObjectToWorldNormal(v.normal);
        o.cNormal = o.normal;
        o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0)).xyz);
        o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);
        if (_TexCoordSpace == 1)
            GetWorldTexCoordTangents(o.normal, o.tangent, o.binormal);
    #endif

    #ifdef TESSELLATION_VARIANT
        #if NOISE_TEXTURE_ENABLED || GERSTNER_ENABLED || VORONOI_ENABLED || VERT_FLIPBOOK_ENABLED
            o.offsetMask = 1;
            if (_TessellationOffsetMask == 1){
                float3 p0 = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
                float offsetMask = (distance(p0, _WorldSpaceCameraPos) - _TessDistMin) / (_TessDistMax - _TessDistMin);
                o.offsetMask = pow(saturate(saturate(1-offsetMask)), 3);
            }
        #endif
    #endif

    #if VERT_OFFSET_ENABLED
        float4 vertOffsetMaskUV = float4(TRANSFORM_TEX(v.uv, _VertexOffsetMask).xy, 0, 0);
        float vertOffsetMask = ChannelCheck(tex2Dlod(_VertexOffsetMask, vertOffsetMaskUV), _VertexOffsetMaskChannel);
        vertOffsetMask = lerp(1, vertOffsetMask, _VertexOffsetMaskStrength);

        // In world space mode waves are sampled from world position and the offset is applied in world units,
        // so object scale (including the Y scale of flat planes) doesn't affect them
        bool worldSpaceOffset = _TexCoordSpace == 1;
        float3x3 waveFrame = GetWorldTexCoordFrame(_TexCoordSpaceSwizzle);
        float3 offsetWPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
        float2 worldCoordSelect[3] = {-offsetWPos.xy, -offsetWPos.xz, -offsetWPos.yz};
        float2 worldCoords = worldCoordSelect[_TexCoordSpaceSwizzle] * abs(_GlobalTexCoordScaleWorld);
        float3 wavePos = worldSpaceOffset ? mul(waveFrame, offsetWPos) : v.vertex.xyz;
    #endif

    #if NOISE_TEXTURE_ENABLED
        float2 noiseUV = ScaleUV(worldSpaceOffset ? worldCoords : v.uv.xy, _NoiseTexScale, _NoiseTexScroll*10);
        float noiseWaveTex = tex2Dlod(_NoiseTex, float4(noiseUV,0,lerp(0,8,_NoiseTexBlur)));
        float noiseWave = Remap(noiseWaveTex, 0, 1, _VertRemapMin, _VertRemapMax);
        float offsetWave = noiseWave * _WaveHeight * 0.1;
        o.wave = _Offset * offsetWave * vertOffsetMask;
        #ifdef TESSELLATION_VARIANT
            o.wave *= o.offsetMask;
        #endif
        v.vertex.xyz += worldSpaceOffset ? WorldOffsetToObject(mul(o.wave, waveFrame)) : o.wave;
        o.wave.y = (o.wave.y + 1) * 0.5;
    #elif GERSTNER_ENABLED
        float turb = 0;
        float3 wave0 = 0;
        float3 wave1 = 0;
        float3 wave2 = 0;
        float3 wave3 = 0;
        float offsetMask = 1;
        #ifdef TESSELLATION_VARIANT
            offsetMask = o.offsetMask;
        #endif
        offsetMask *= vertOffsetMask;
        float3 wpos = wavePos;
        if (_RecalculateNormals == 1){
            o.tangent = float3(1,0,0);
            o.binormal = float3(0,0,1);
        }
        // if (_Turbulence > 0){
        // 	_Turbulence *= _WaveStrengthGlobal;
        // 	_TurbulenceSpeed *= _WaveSpeedGlobal;
        // 	_TurbulenceScale *= _WaveScaleGlobal;
        // 	turb = Perlin3D(float3(v.uv.xy*_TurbulenceScale, _Time.y*_TurbulenceSpeed))+1;
        // 	turb *= _Turbulence*0.1;
        // }

        // Warping the sample position with low frequency noise bends the crests so the wave sum doesn't read as a grid
        // Recalculated normals ignore the warp's derivative, which is close enough at low strengths
        // Skipped when the 64x64 noise texture isn't bound, since Unity's 4x4 fallback would just shift every wave
        if (_WaveWarpStrength > 0 && _TANoiseTex_TexelSize.z >= 64){
            float2 warp = tanoise3(float3(wpos.xz * _WaveWarpScale, _Time.y * _WaveWarpSpeed)).xy * 2 - 1;
            wpos.xz += warp * _WaveWarpStrength;
        }

        // Irrational wavelength ratios and staggered phases stop the waves from lining up into a repeating pattern
        // when scales are whole multiples of each other
        if (_WaveToggle0 == 1 && _WaveStrength0 > 0){
            _WaveStrength0 *= _WaveStrengthGlobal;
            _WaveSpeed0 *= _WaveSpeedGlobal;
            _WaveScale0 *= _WaveScaleGlobal;
            float4 waveProperties0 = float4(0,1, _WaveStrength0 + turb, _WaveScale0);
            wave0 = GerstnerWave(waveProperties0, wpos, _WaveSpeed0, _WaveDirection0, 0, o.tangent, o.binormal, offsetMask);
        }
        if (_WaveToggle1 == 1 && _WaveStrength1 > 0){
            _WaveStrength1 *= _WaveStrengthGlobal;
            _WaveSpeed1 *= _WaveSpeedGlobal;
            _WaveScale1 *= _WaveScaleGlobal;
            float4 waveProperties1 = float4(0,1, _WaveStrength1 + turb, _WaveScale1 * 1.1371);
            wave1 = GerstnerWave(waveProperties1, wpos, _WaveSpeed1, _WaveDirection1, 1.7, o.tangent, o.binormal, offsetMask);
        }
        if (_WaveToggle2 == 1 && _WaveStrength2 > 0){
            _WaveStrength2 *= _WaveStrengthGlobal;
            _WaveSpeed2 *= _WaveSpeedGlobal;
            _WaveScale2 *= _WaveScaleGlobal;
            float4 waveProperties2 = float4(0,1, _WaveStrength2 + turb, _WaveScale2 * 0.8913);
            wave2 = GerstnerWave(waveProperties2, wpos, _WaveSpeed2, _WaveDirection2, 3.9, o.tangent, o.binormal, offsetMask);
        }
        if (_WaveToggle3 == 1 && _WaveStrength3 > 0){
            _WaveStrength3 *= _WaveStrengthGlobal;
            _WaveSpeed3 *= _WaveSpeedGlobal;
            _WaveScale3 *= _WaveScaleGlobal;
            float4 waveProperties3 = float4(0,1, _WaveStrength3 + turb, _WaveScale3 * 1.3819);
            wave3 = GerstnerWave(waveProperties3, wpos, _WaveSpeed3, _WaveDirection3, 5.3, o.tangent, o.binormal, offsetMask);
        }
        o.wave = wave0 + wave1 + wave2 + wave3;
        o.wave *= vertOffsetMask;
        #ifdef TESSELLATION_VARIANT
            o.wave *= o.offsetMask;
        #endif
        v.vertex.xyz += worldSpaceOffset ? WorldOffsetToObject(mul(o.wave, waveFrame)) : o.wave;
        o.wave.y = (o.wave.y + 1) * 0.5;
        if (_RecalculateNormals == 1){
            o.normal = normalize(cross(o.binormal, o.tangent));
            // Normal is built before converting since the XY/YZ frames are mirrored
            if (worldSpaceOffset){
                o.normal = mul(o.normal, waveFrame);
                o.tangent = mul(o.tangent, waveFrame);
                o.binormal = mul(o.binormal, waveFrame);
            }
            o.cNormal = o.normal;
        }
    #elif VORONOI_ENABLED
        float2 voronoiUV = (wavePos.xz * _VoronoiScale) + (_Time.y * _VoronoiScroll);
        float voronoiSpeed = _Time.y * _VoronoiSpeed;
        float voronoi = Voronoi2D(voronoiUV, voronoiSpeed)*0.1;
        o.wave = _VoronoiOffset * voronoi * _VoronoiWaveHeight;
        #ifdef TESSELLATION_VARIANT
            o.wave *= o.offsetMask;
        #endif
        v.vertex.xyz += worldSpaceOffset ? WorldOffsetToObject(mul(o.wave, waveFrame)) : o.wave;
        o.wave.y = (o.wave.y + 1) * 0.5;
    #elif VERT_FLIPBOOK_ENABLED
        float2 flipbookUV = v.uv * _NormalMapFlipbookScale;
        float2 uvFlow = o.uvFlow;
        float2 uvBlend = o.uv.xy;
        if (worldSpaceOffset){
            flipbookUV = worldCoords * _NormalMapFlipbookScale;
            uvFlow = worldCoords;
            uvBlend = worldCoords;
        }
        #if FLOW_ENABLED
            uvFlow = ScaleUV(uvFlow, _FlowMapScale, 0);
            float4 flowMap = MOCHIE_SAMPLE_TEX2D_LOD(_FlowMap, uvFlow, 0);
            float blendNoise = flowMap.a;
            if (_BlendNoiseSource == 1){
                uvBlend = ScaleUV(uvBlend, _BlendNoiseScale, 0);
                blendNoise = MOCHIE_SAMPLE_TEX2D_SAMPLER_LOD(_BlendNoise, sampler_FlowMap, uvBlend, 0);
            }
            float2 flow = (flowMap.rg * 2 - 1) * _FlowStrength * 0.1;
            float time = _Time.y * _FlowSpeed + blendNoise;
            float3 flowUV0 = FlowUV(flipbookUV, flow, time, 0);
            float3 flowUV1 = FlowUV(flipbookUV, flow, time, 0.5);
            float3 flipbookSample0 = tex2DflipbookSmoothLOD(_VertOffsetFlipbook, sampler_VertOffsetFlipbook, flowUV0, _VertOffsetFlipbookSpeed, 0) * flowUV0.z;
            float3 flipbookSample1 = tex2DflipbookSmoothLOD(_VertOffsetFlipbook, sampler_VertOffsetFlipbook, flowUV1, _VertOffsetFlipbookSpeed, 0) * flowUV1.z;
            o.wave = flipbookSample0 + flipbookSample1;
        #else
            o.wave = tex2DflipbookSmoothLOD(_VertOffsetFlipbook, sampler_VertOffsetFlipbook, flipbookUV, _VertOffsetFlipbookSpeed, 0);
        #endif
        float3 waveOffset = (o.wave - 0.5) * vertOffsetMask;
        #ifdef TESSELLATION_VARIANT
            waveOffset *= o.offsetMask;
        #endif
        if (worldSpaceOffset){
            // Same as the UV path below, the world coord tangent/bitangent are the frame's -u/-v axes so the subtraction becomes an addition
            float3 flipbookOffset = float3(waveOffset.x / _NormalMapFlipbookScale.x, waveOffset.z, waveOffset.y / _NormalMapFlipbookScale.y);
            v.vertex.xyz += WorldOffsetToObject(mul(flipbookOffset, waveFrame) * _VertOffsetFlipbookStrength);
        }
        else {
            float3 bitangent = cross(v.normal, v.tangent.xyz) * v.tangent.w;
            v.vertex.xyz += waveOffset.z * v.normal * _VertOffsetFlipbookStrength;
            v.vertex.xyz -= (waveOffset.x * normalize(v.tangent.xyz) / _NormalMapFlipbookScale.x + waveOffset.y * normalize(bitangent) / _NormalMapFlipbookScale.y) * _VertOffsetFlipbookStrength;
        }
        o.wave.y = 0;
    #endif
    
    o.pos = UnityObjectToClipPos(v.vertex);
    #if GERSTNER_ENABLED
        if (_RecalculateNormals != 1){
            o.normal = UnityObjectToWorldNormal(v.normal);
            o.cNormal = o.normal;
            o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0)).xyz);
            o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);
            if (_TexCoordSpace == 1)
                GetWorldTexCoordTangents(o.normal, o.tangent, o.binormal);
        }
    #endif
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    o.uvGrab = ComputeGrabScreenPos(o.pos);
    o.reflUV = ComputeNonStereoScreenPos(o.pos);
    o.depthUV = GetScreenPosition(o.pos);
    o.depthUV.z = -mul(UNITY_MATRIX_V, float4(mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz, 1.0)).z;
    #if defined(LIGHTMAP_ON)
        o.lightmapUV.xy = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
    #endif
    #if defined(DYNAMICLIGHTMAP_ON)
        o.lightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif
    o.localPos = v.vertex;

    v.tangent.xyz = normalize(v.tangent.xyz);
    v.normal = normalize(v.normal);
    float3x3 objectToTangent = float3x3(v.tangent.xyz, (cross(v.normal, v.tangent.xyz) * v.tangent.w), v.normal);
    o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));

    // #ifdef TESSELLATION_VARIANT
    //     o.barycentricCoordinates = v.barycentricCoordinates;
    // #endif

    UNITY_TRANSFER_FOG(o, o.pos);
    #if defined(UNITY_PASS_SHADOWCASTER)
        TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
        // Only Opaque casts shadows. The built-in renderer ignores the material's ShadowCaster pass toggle (WaterEditor), so the
        // other modes collapse their triangles here instead
        #if !defined(_OPAQUE_MODE_ON)
            o.pos = 0;
        #endif
    #else
        UNITY_TRANSFER_SHADOW(o, v.uv1);
    #endif
    return o;
}

#endif // WATER_VERT_INCLUDED