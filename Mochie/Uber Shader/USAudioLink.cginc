#include "../Common/AudioLink.cginc"

float GetAudioLinkBand(audioLinkData al, int band, float remapMin, float remapMax){
    float4 bands = float4(al.bass, al.lowMid, al.upperMid, al.treble);
    return Remap(bands[band], _AudioLinkRemapMin, _AudioLinkRemapMax, remapMin, remapMax);
}

// time (0-1) reads back through the band's history, so effects can travel outward over time
void InitializeAudioLink(inout audioLinkData al, float time){
    al.textureExists = AudioLinkIsAvailable();
    if (al.textureExists){
        float2 delay = float2(saturate(time) * 127, 0);
        al.bass = AudioLinkLerp(ALPASS_AUDIOBASS + delay).r;
        al.lowMid = AudioLinkLerp(ALPASS_AUDIOLOWMIDS + delay).r;
        al.upperMid = AudioLinkLerp(ALPASS_AUDIOHIGHMIDS + delay).r;
        al.treble = AudioLinkLerp(ALPASS_AUDIOTREBLE + delay).r;
    }
}

void ApplyVisualizers(float2 uv, inout float3 col){
    if (_OscilloscopeStrength > 0){
        bool marginL = uv.x > _OscilloscopeMarginLR.x;
        bool marginR = uv.x < _OscilloscopeMarginLR.y;
        bool marginT = uv.y < _OscilloscopeMarginTB.x;
        bool marginB = uv.y > _OscilloscopeMarginTB.y;

        if (marginL && marginR && marginT && marginB){
            float2 ouv = ScaleOffsetRotateUV(uv, _OscilloscopeScale, _OscilloscopeOffset, _OscilloscopeRot);
            float texSample = AudioLinkLerpMultiline(ALPASS_WAVEFORM + float2(200. * ouv.x, 0)).r;
            float3 oscilloscope = clamp(1 - 50 * abs(texSample - ouv.y* 2. + 1), 0, 1);
            oscilloscope *= _OscilloscopeCol.rgb * _OscilloscopeCol.a * _OscilloscopeStrength * _AudioLinkStrength;
            col += oscilloscope;
        }
    }
}