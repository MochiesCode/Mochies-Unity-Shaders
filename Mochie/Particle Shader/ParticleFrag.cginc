#ifndef PARTICLE_FRAG_INCLUDED
#pragma exclude_renderers gles
#define PARTICLE_FRAG_INCLUDED

float4 frag (v2f i) : SV_Target {
    
    #if defined(UNITY_PASS_FORWARDADD) && !defined(_LIGHTING_ON)
        discard;
    #endif
    
    // Calculating this once instead of per tex sample 
    // since the transform can be applied after the bulk of the math
    #if defined(X_VERSION)
        panoUV = PanosphereUV(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
    #endif

    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    #if defined(_DISSOLVE_ON) && defined(X_VERSION)
        float4 rimCol = 0;
        ApplyDissolve(i, rimCol);
    #endif

    float alpha = 1;
    #if defined(_ALPHA_MASK_ON)
        alpha = GetAlpha(i);
    #endif

    #if defined(_FADING_ON) && defined(SOFTPARTICLES_ON) && !(IS_OPAQUE)
        Softening(i, fade);
        i.color.a *= fade;
    #endif

    audioLinkData al = (audioLinkData)0;
    #if defined(_AUDIOLINK_ON)
        InitializeAudioLink(al);
        if (_AudioLinkOpacityStrength > 0){
            float alOpacity = GetAudioLinkBand(al, _AudioLinkOpacityBand, _AudioLinkRemapOpacityMin, _AudioLinkRemapOpacityMax);
            _Opacity *= lerp(1, alOpacity, _AudioLinkOpacityStrength * _AudioLinkStrength);
        }
    #endif

    float4 baseColor = GetBaseColor(i, al, alpha);
    float omr = 1;
    #if defined(_LIGHTING_ON)
        UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
        InputData id = (InputData)0;
        InitializeInputData(i, id, al, baseColor);
        LightingData ld = (LightingData)0;
        InitializeLightingData(i, id, ld, atten);
        baseColor.rgb *= ld.lightCol;
        omr = ld.omr;
    #endif

    #if defined(_DISTORTION_ON) && !(IS_OPAQUE)
        ApplyDistortionBlend(i, baseColor);
    #endif

    falloff = 1;
    pulse = 1;
    #if defined(_FALLOFF_ON)
        falloff = i.falloff;
    #endif
    #if defined(_PULSE_ON)
        pulse = i.pulse;
    #endif
    #if !(IS_OPAQUE)
        globalAlpha = falloff * pulse * _Opacity;
    #else
        globalAlpha = pulse;
    #endif

    float4 col = GetColor(i, omr, baseColor);

    #if defined(_LIGHTING_ON) && PBR_ENABLED
        CalculateBRDF(i, id, ld);
        col.rgb += (ld.reflectionCol + ld.specHighlightCol + ld.lightVolumeSpecularity) * globalAlpha;
    #endif
    
    #if defined(_EMISSION_ON) && defined(_LIGHTING_ON)
        ApplyEmission(i, ld, al, col.rgb);
    #endif

    #if defined(_ALPHATEST_ON) && defined(X_VERSION)
        ApplyCutoutRim(col);
    #endif

    #if defined(_DISSOLVE_ON) && defined(X_VERSION)
        ApplyDissolveRim(col, rimCol);
    #endif

    #if defined(_FILTERING_ON)
        ApplyHSVFilter(col, al);
    #endif
    
    #if defined(_RANDOM_HUE_ON) && defined(X_VERSION)
        ApplyRandomHue(i, col.rgb);
    #endif

    #if defined(UNITY_PASS_SHADOWCASTER)
        return 0;
    #else
        #if IS_OPAQUE
            return float4(col.rgb, 1);
        #else
            col.a = saturate(col.a);
            return col;
        #endif
    #endif
}

#endif