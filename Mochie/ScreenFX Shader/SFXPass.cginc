#ifndef SFX_PASS_INCLUDED
#define SFX_PASS_INCLUDED

#if MAIN_PASS
v2f vert (appdata v){
    v2f o;
    UNITY_INITIALIZE_OUTPUT(v2f, o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    #if X_FEATURES
        o.pulseSpeed = GetPulse();
    #else
        o.pulseSpeed = 1;
    #endif
    o.cameraPos = GetCameraPos();
    o.objPos = GetObjPos();
    o.objDist = distance(o.cameraPos, o.objPos);

    float gf = GetGlobalFalloff(o.cameraPos, o.objPos, o.objDist);
    o.globalF = gf;
    o.colorF = GetFalloff(_ColorUseGlobal, gf, _ColorFalloffMode, _ColorMinRange, _ColorMaxRange, _ColorBoxSize.xyz, _ColorBoxOffset.xyz, _ColorBlendDistance, o.cameraPos, o.objPos, o.objDist);
    o.shakeF = GetFalloff(_ShakeUseGlobal, gf, _ShakeFalloffMode, _ShakeMinRange, _ShakeMaxRange, _ShakeBoxSize.xyz, _ShakeBoxOffset.xyz, _ShakeBlendDistance, o.cameraPos, o.objPos, o.objDist);
    o.distortionF = GetFalloff(_DistortionUseGlobal, gf, _DistortionFalloffMode, _DistortionMinRange, _DistortionMaxRange, _DistortionBoxSize.xyz, _DistortionBoxOffset.xyz, _DistortionBlendDistance, o.cameraPos, o.objPos, o.objDist);
    o.blurF = GetFalloff(_BlurUseGlobal, gf, _BlurFalloffMode, _BlurMinRange, _BlurMaxRange, _BlurBoxSize.xyz, _BlurBoxOffset.xyz, _BlurBlendDistance, o.cameraPos, o.objPos, o.objDist);
    o.noiseF = GetFalloff(_NoiseUseGlobal, gf, _NoiseFalloffMode, _NoiseMinRange, _NoiseMaxRange, _NoiseBoxSize.xyz, _NoiseBoxOffset.xyz, _NoiseBlendDistance, o.cameraPos, o.objPos, o.objDist);
    #if X_FEATURES
        o.sstF = GetFalloff(_SSTUseGlobal, gf, _SSTFalloffMode, _SSTMinRange, _SSTMaxRange, _SSTBoxSize.xyz, _SSTBoxOffset.xyz, _SSTBlendDistance, o.cameraPos, o.objPos, o.objDist);
        o.olF = GetFalloff(_OLUseGlobal, gf, _OLFalloffMode, _OLMinRange, _OLMaxRange, _OLBoxSize.xyz, _OLBoxOffset.xyz, _OLBlendDistance, o.cameraPos, o.objPos, o.objDist);
    #endif

    if (unity_OrthoParams.w == 1){
        v.vertex.y *= 10;
        v.vertex.x *= 20;
    }
    else {
        v.vertex.x *= 1.4;
    }

    float4 wPos = mul(unity_CameraToWorld, v.vertex);
    float4 oPos = mul(unity_WorldToObject, wPos);
    o.raycast = UnityObjectToViewPos(oPos).xyz * float3(-1,-1,1);

    o.worldPos = wPos;
    o.pos = GetScreenspaceVertexPos(v.vertex);
    o.uv = ComputeGrabScreenPos(o.pos);
    o.uvd = TRANSFORM_TEX(v.uv, _NormalMap) + _Time.y * _DistortionSpeed;
    audioLinkData ald = (audioLinkData)0;
    #if AUDIOLINK_ENABLED
        InitializeAudioLink(ald, 0);
    #endif
    #if SHAKE_ENABLED
        ApplyNoiseShake(o, ald);
    #endif
    return o;
}

float4 frag (v2f i) : SV_Target {
    
    #if defined(SHADER_API_MOBILE)
        discard;
    #endif
    
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    MirrorCheck();
    
    if (_CameraRenderMode == 1){
        if (_VRChatCameraMode > 0)
            discard;
    }
    else if (_CameraRenderMode == 2){
        if (_VRChatCameraMode == 0)
            discard;
    }

    float2 uv = i.uv.xy / i.uv.w;
    i.uv.xy = uv;
    i.viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);

    audioLinkData ald = (audioLinkData)0;
    #if AUDIOLINK_ENABLED
        InitializeAudioLink(ald, 0);
    #endif

    #if X_FEATURES
        ApplyPulse(i.pulseSpeed);
        ApplyUVManip(i, ald);
    #endif

    #if SHAKE_ENABLED
        ApplyShake(i, ald);
    #endif

    #if DISTORTION_ENABLED
        ApplyMapDistortion(i, ald);
    #elif DISTORTION_WORLD_ENABLED
        ApplyWGDistortion(i, ald); 
    #endif
    
    #if BLUR_ENABLED
        #if DOF_ENABLED
            float dof = GetDoF(i);
            _BlurStr = lerp(_BlurStr, 0, dof);
            _RippleGridStr = lerp(_RippleGridStr, 0, dof);
            _PixelationStr = lerp(_PixelationStr, 0, dof);
        #endif
        ApplyRipplePixelate(i, ald);
        float4 col = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, i.uv.xy);
        ApplyDitherBlur(i, ald);
        float4 blurCol = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, i.uv.xy);
        ApplyBlur(i, col.rgb, blurCol.rgb, ald);
    #else
        float4 col = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, i.uv.xy);
    #endif

    #if X_FEATURES
        float miscAudioLinkStrength = 1;
        #if AUDIOLINK_ENABLED
            miscAudioLinkStrength = GetAudioLinkBand(ald, _AudioLinkMiscStrength, _AudioLinkMiscBand, _AudioLinkMiscMin, _AudioLinkMiscMax);
        #endif
        ApplyDepthBuffer(i, col.rgb, miscAudioLinkStrength);
        ApplyNormalMap(i, col.rgb, miscAudioLinkStrength);
        #if SOBEL_FILTER_ENABLED
            ApplySobelFilter(i, col.rgb, miscAudioLinkStrength);
        #endif
        #if OUTLINE_ENABLED
            ApplyOutline(i, col.rgb, ald);
        #endif
        ApplyRounding(i, col.rgb, miscAudioLinkStrength);
    #endif

    #if COLOR_ENABLED
        ApplyColor(i, col.rgb, ald);
    #endif

    #if NOISE_ENABLED
        ApplyNoise(i, col.rgb, ald);
    #endif

    #if COLOR_ENABLED
        if (_ClampToggle == 1)
            col = clamp(col, 0, _ClampMax);
    #endif

    ApplyTransparency(i, col);
    return col;
}
#elif X_FEATURES
    #include "SFXXPasses.cginc"
#endif
#endif