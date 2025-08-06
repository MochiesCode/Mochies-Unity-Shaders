#include "../Common/AudioLink.cginc"

float GetAudioLinkBand(audioLinkData al, int band, float remapMin, float remapMax){
    float4 bands = float4(al.bass, al.lowMid, al.upperMid, al.treble);
    return Remap(bands[band], _AudioLinkRemapMin, _AudioLinkRemapMax, remapMin, remapMax);
}

void InitializeAudioLink(inout audioLinkData al, float time){
    float versionBand = 1;
    float versionTime = 1;
    al.textureExists = AudioLinkIsAvailable();
    if (al.textureExists){
        al.bass = AudioLinkData(ALPASS_AUDIOBASS);
        al.lowMid = AudioLinkData(ALPASS_AUDIOLOWMIDS);
        al.upperMid = AudioLinkData(ALPASS_AUDIOHIGHMIDS);
        al.treble = AudioLinkData(ALPASS_AUDIOTREBLE);
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