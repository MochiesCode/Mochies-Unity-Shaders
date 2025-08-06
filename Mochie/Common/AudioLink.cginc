#ifndef AUDIOLINK_CGINC_INCLUDED
    #define AUDIOLINK_CGINC_INCLUDED

    // Map of where features in AudioLink are.
    #define ALPASS_DFT                      uint2(0,4)  //Size: 128, 2
    #define ALPASS_WAVEFORM                 uint2(0,6)  //Size: 128, 16
    #define ALPASS_AUDIOLINK                uint2(0,0)  //Size: 128, 4
    #define ALPASS_AUDIOBASS                uint2(0,0)  //Size: 128, 1
    #define ALPASS_AUDIOLOWMIDS             uint2(0,1)  //Size: 128, 1
    #define ALPASS_AUDIOHIGHMIDS            uint2(0,2)  //Size: 128, 1
    #define ALPASS_AUDIOTREBLE              uint2(0,3)  //Size: 128, 1
    #define ALPASS_AUDIOLINKHISTORY         uint2(1,0)  //Size: 127, 4
    #define ALPASS_GENERALVU                uint2(0,22) //Size: 12, 1
    #define ALPASS_GENERALVU_INSTANCE_TIME  uint2(2,22)
    #define ALPASS_GENERALVU_LOCAL_TIME     uint2(3,22)
    #define ALPASS_GENERALVU_NETWORK_TIME   uint2(4,22)
    #define ALPASS_GENERALVU_PLAYERINFO     uint2(6,22)
    #define ALPASS_THEME_COLOR0             uint2(0,23)
    #define ALPASS_THEME_COLOR1             uint2(1,23)
    #define ALPASS_THEME_COLOR2             uint2(2,23)
    #define ALPASS_THEME_COLOR3             uint2(3,23)
    #define ALPASS_GENERALVU_UNIX_DAYS      uint2(5,23)
    #define ALPASS_GENERALVU_UNIX_SECONDS   uint2(6,23)
    #define ALPASS_GENERALVU_SOURCE_POS     uint2(7,23)
    #define ALPASS_MEDIASTATE               uint2(5,22)

    #define ALPASS_CCINTERNAL               uint2(12,22) //Size: 12, 2
    #define ALPASS_CCCOLORS                 uint2(25,22) //Size: 12, 1 (Note Color #0 is always black, Colors start at 1)
    #define ALPASS_CCSTRIP                  uint2(0,24)  //Size: 128, 1
    #define ALPASS_CCLIGHTS                 uint2(0,25)  //Size: 128, 2
    #define ALPASS_AUTOCORRELATOR           uint2(0,27)  //Size: 128, 1
    #define ALPASS_FILTEREDAUDIOLINK        uint2(0,28)  //Size: 16, 4
    #define ALPASS_CHRONOTENSITY            uint2(16,28) //Size: 8, 4
    #define ALPASS_FILTEREDVU               uint2(24,28) //Size: 4, 4
    #define ALPASS_FILTEREDVU_INTENSITY     uint2(24,28) //Size: 4, 1
    #define ALPASS_FILTEREDVU_MARKER        uint2(24,29) //Size: 4, 1
    #define ALPASS_GLOBAL_STRINGS           uint2(40,28) //Size: 8, 4

    // Some basic constants to use (Note, these should be compatible with
    // future version of AudioLink, but may change.
    #define AUDIOLINK_SAMPHIST              3069        // Internal use for algos, do not change.
    #define AUDIOLINK_SAMPLEDATA24          2046
    #define AUDIOLINK_EXPBINS               24
    #define AUDIOLINK_EXPOCT                10
    #define AUDIOLINK_ETOTALBINS            (AUDIOLINK_EXPBINS * AUDIOLINK_EXPOCT)
    #define AUDIOLINK_WIDTH                 128
    #define AUDIOLINK_SPS                   48000       // Samples per second
    #define AUDIOLINK_ROOTNOTE              0
    #define AUDIOLINK_4BAND_FREQFLOOR       0.123
    #define AUDIOLINK_4BAND_FREQCEILING     1
    #define AUDIOLINK_BOTTOM_FREQUENCY      13.75
    #define AUDIOLINK_BASE_AMPLITUDE        2.5
    #define AUDIOLINK_DELAY_COEFFICIENT_MIN 0.3
    #define AUDIOLINK_DELAY_COEFFICIENT_MAX 0.9
    #define AUDIOLINK_DFT_Q                 4.0
    #define AUDIOLINK_TREBLE_CORRECTION     5.0
    #define AUDIOLINK_4BAND_TARGET_RATE     90.0
    #define AUDIOLINK_LUT                   {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, \
                                            0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, \
                                            0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, \
                                            0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.001, 0.002, 0.003, 0.004, 0.005, 0.006, \
                                            0.008, 0.01, 0.012, 0.014, 0.017, 0.02, 0.022, 0.025, 0.029, 0.032, 0.036, \
                                            0.04, 0.044, 0.048, 0.053, 0.057, 0.062, 0.067, 0.072, 0.078, 0.083, 0.089, \
                                            0.095, 0.101, 0.107, 0.114, 0.121, 0.128, 0.135, 0.142, 0.149, 0.157, 0.164, \
                                            0.172, 0.18, 0.188, 0.196, 0.205, 0.213, 0.222, 0.23, 0.239, 0.248, 0.257, \
                                            0.266, 0.276, 0.285, 0.294, 0.304, 0.313, 0.323, 0.333, 0.342, 0.352, 0.362, \
                                            0.372, 0.381, 0.391, 0.401, 0.411, 0.421, 0.431,  0.441, 0.451, 0.46, 0.47, \
                                            0.48, 0.49, 0.499, 0.509, 0.519, 0.528, 0.538, 0.547, 0.556, 0.565, 0.575, \
                                            0.584, 0.593, 0.601, 0.61, 0.619, 0.627, 0.636, 0.644, 0.652, 0.66, 0.668, \
                                            0.676, 0.684, 0.691, 0.699, 0.706, 0.713, 0.72, 0.727, 0.734, 0.741, 0.747, \
                                            0.754, 0.76, 0.766,  0.772, 0.778, 0.784, 0.79, 0.795, 0.801, 0.806, 0.811, \
                                            0.816, 0.821, 0.826, 0.831, 0.835, 0.84, 0.844, 0.848, 0.853, 0.857, 0.861, \
                                            0.864,  0.868, 0.872, 0.875, 0.879, 0.882, 0.885, 0.888, 0.891, 0.894, 0.897, \
                                            0.899, 0.902, 0.904, 0.906, 0.909, 0.911, 0.913, 0.914, 0.916, 0.918, 0.919, \
                                            0.921, 0.922, 0.924, 0.925, 0.926, 0.927, 0.928, 0.928, 0.929, 0.929, 0.93, \
                                            0.93, 0.93, 0.931, 0.931, 0.93, 0.93, 0.93, 0.93, 0.929, 0.929, 0.928, 0.927, \
                                            0.926, 0.925, 0.924, 0.923, 0.922, 0.92, 0.919, 0.917, 0.915, 0.913, 0.911, \
                                            0.909, 0.907, 0.905, 0.903, 0.9}

    // Text constants
    #define AUDIOLINK_STRING_MAX_CHARS      32
    #define AUDIOLINK_STRING_LOCALPLAYER    0
    #define AUDIOLINK_STRING_MASTER         1
    #define AUDIOLINK_STRING_CUSTOM1        2
    #define AUDIOLINK_STRING_CUSTOM2        3

    // ColorChord constants
    #define COLORCHORD_EMAXBIN              192
    #define COLORCHORD_NOTE_CLOSEST         3.0
    #define COLORCHORD_NEW_NOTE_GAIN        8.0
    #define COLORCHORD_MAX_NOTES            10

    // We use glsl_mod for most calculations because it behaves better
    // on negative numbers, and in some situations actually outperforms
    // HLSL's modf().
    #ifndef glsl_mod
        #define glsl_mod(x,y) (((x)-(y)*floor((x)/(y))))
    #endif

    uniform float4               _AudioTexture_TexelSize;

    #ifdef SHADER_TARGET_SURFACE_ANALYSIS
        #define AUDIOLINK_STANDARD_INDEXING
    #endif

    // Mechanism to index into texture.
    #ifdef AUDIOLINK_STANDARD_INDEXING
        sampler2D _AudioTexture;
        #define AudioLinkData(xycoord) tex2Dlod(_AudioTexture, float4(uint2(xycoord) * _AudioTexture_TexelSize.xy, 0, 0))
    #else
        uniform Texture2D<float4>   _AudioTexture;
        #define AudioLinkData(xycoord) _AudioTexture[uint2(xycoord)]
    #endif

    // Convenient mechanism to read from the AudioLink texture that handles reading off the end of one line and onto the next above it.
    float4 AudioLinkDataMultiline(uint2 xycoord) { return AudioLinkData(uint2(xycoord.x % AUDIOLINK_WIDTH, xycoord.y + xycoord.x/AUDIOLINK_WIDTH)); }

    // Mechanism to sample between two adjacent pixels and lerp between them, like "linear" supesampling
    float4 AudioLinkLerp(float2 xy) { return lerp( AudioLinkData(xy), AudioLinkData(xy+int2(1,0)), frac( xy.x ) ); }

    // Same as AudioLinkLerp but properly handles multiline reading.
    float4 AudioLinkLerpMultiline(float2 xy) { return lerp(AudioLinkDataMultiline(xy), AudioLinkDataMultiline(xy+float2(1,0)), frac(xy.x)); }

    //Tests to see if Audio Link texture is available
    bool AudioLinkIsAvailable()
    {
        #if !defined(AUDIOLINK_STANDARD_INDEXING)
            int width, height;
            _AudioTexture.GetDimensions(width, height);
            return width > 16;
        #else
            return _AudioTexture_TexelSize.z > 16;
        #endif
    }

    // DEPRECATED! Use AudioLinkGetVersionMajor and AudioLinkGetVersionMinor() instead.
    //Get version of audiolink present in the world, 0 if no audiolink is present
    float AudioLinkGetVersion()
    {
        int2 dims;
        #if !defined(AUDIOLINK_STANDARD_INDEXING)
            _AudioTexture.GetDimensions(dims.x, dims.y);
        #else
            dims = _AudioTexture_TexelSize.zw;
        #endif

        if (dims.x >= 128)
            return AudioLinkData(ALPASS_GENERALVU).x;
        else if (dims.x > 16)
            return 1;
        else
            return 0;
    }

    float AudioLinkGetVersionMajor()
    {
        return AudioLinkData(ALPASS_GENERALVU).y;
    }

    float AudioLinkGetVersionMinor()
    {
        // If the major version is 1 or greater, we are using the new versioning system.
        if (AudioLinkGetVersionMajor() > 0)
        {
            return AudioLinkData(ALPASS_GENERALVU).w;
        }
        // Otherwise, defer to the old logic for determining version.
        else
        {
            int2 dims;
            #if !defined(AUDIOLINK_STANDARD_INDEXING)
                _AudioTexture.GetDimensions(dims.x, dims.y);
            #else
                dims = _AudioTexture_TexelSize.zw;
            #endif

            if (dims.x >= 128)
                return AudioLinkData(ALPASS_GENERALVU).x;
            else if (dims.x > 16)
                return 1;
            else
                return 0;
        }
    }

    // This pulls data from this texture.
    #define AudioLinkGetSelfPixelData(xy) _SelfTexture2D[xy]

    // Extra utility functions for time.
    uint AudioLinkDecodeDataAsUInt(uint2 indexloc)
    {
        uint4 rpx = AudioLinkData(indexloc);
        return rpx.x + rpx.y*1024 + rpx.z * 1048576 + rpx.w * 1073741824;
    }

    //Note: This will truncate time to every 134,217.728 seconds (~1.5 days of an instance being up) to prevent floating point aliasing.
    // if your code will alias sooner, you will need to use a different function.  It should be safe to use this on all times.
    float AudioLinkDecodeDataAsSeconds(uint2 indexloc)
    {
        uint time = AudioLinkDecodeDataAsUInt(indexloc) & 0x7ffffff;
        //Can't just divide by float.  Bug in Unity's HLSL compiler.
        return float(time / 1000) + float( time % 1000 ) / 1000.;
    }

    #define ALDecodeDataAsSeconds( x ) AudioLinkDecodeDataAsSeconds( x )
    #define ALDecodeDataAsUInt( x ) AudioLinkDecodeDataAsUInt( x )

    float AudioLinkRemap(float t, float a, float b, float u, float v) { return ((t-a) / (b-a)) * (v-u) + u; }

    float3 AudioLinkHSVtoRGB(float3 HSV)
    {
        float3 RGB = 0;
        float C = HSV.z * HSV.y;
        float H = HSV.x * 6;
        float X = C * (1 - abs(fmod(H, 2) - 1));
        if (HSV.y != 0)
        {
            float I = floor(H);
            if (I == 0) { RGB = float3(C, X, 0); }
            else if (I == 1) { RGB = float3(X, C, 0); }
            else if (I == 2) { RGB = float3(0, C, X); }
            else if (I == 3) { RGB = float3(0, X, C); }
            else if (I == 4) { RGB = float3(X, 0, C); }
            else { RGB = float3(C, 0, X); }
        }
        float M = HSV.z - C;
        return RGB + M;
    }

    float3 AudioLinkCCtoRGB(float bin, float intensity, int rootNote)
    {
        float note = bin / AUDIOLINK_EXPBINS;

        float hue = 0.0;
        note *= 12.0;
        note = glsl_mod(4. - note + rootNote, 12.0);
        {
            if(note < 4.0)
            {
                //Needs to be YELLOW->RED
                hue = (note) / 24.0;
            }
            else if(note < 8.0)
            {
                //            [4]  [8]
                //Needs to be RED->BLUE
                hue = (note-2.0) / 12.0;
            }
            else
            {
                //             [8] [12]
                //Needs to be BLUE->YELLOW
                hue = (note - 4.0) / 8.0;
            }
        }
        float val = intensity - 0.1;
        return AudioLinkHSVtoRGB(float3(fmod(hue, 1.0), 1.0, clamp(val, 0.0, 1.0)));
    }

    // Sample the amplitude of a given frequency in the DFT, supports frequencies in [13.75; 14080].
    float4 AudioLinkGetAmplitudeAtFrequency(float hertz)
    {
        float note = AUDIOLINK_EXPBINS * log2(hertz / AUDIOLINK_BOTTOM_FREQUENCY);
        return AudioLinkLerpMultiline(ALPASS_DFT + float2(note, 0));
    }

    // Sample the amplitude of a given quartertone in an octave. Octave is in [0; 9] while quarter is [0; 23].
    float4 AudioLinkGetAmplitudeAtQuarterNote(float octave, float quarter)
    {
        return AudioLinkLerpMultiline(ALPASS_DFT + float2(octave * AUDIOLINK_EXPBINS + quarter, 0));
    }

    // Sample the amplitude of a given semitone in an octave. Octave is in [0; 9] while note is [0; 11].
    float4 AudioLinkGetAmplitudeAtNote(float octave, float note)
    {
        float quarter = note * 2.0;
        return AudioLinkGetAmplitudeAtQuarterNote(octave, quarter);
    }

    // Sample the amplitude of a given quartertone across all octaves. Quarter is [0; 23].
    float4 AudioLinkGetAmplitudesAtQuarterNote(float quarter)
    {
        float amplitude = 0;
        UNITY_UNROLL
        for (int i = 0; i < AUDIOLINK_EXPOCT; i++)
        {
            amplitude += AudioLinkGetAmplitudeAtQuarterNote(i,quarter);
        }
        return amplitude;
    }

    // Sample the amplitude of a given semitone across all octaves. Note is [0; 11].
    float4 AudioLinkGetAmplitudesAtNote(float note)
    {
        float quarter = note * 2.0;
        return AudioLinkGetAmplitudesAtQuarterNote(quarter);
    }

    // Get a reasonable drop-in replacement time value for _Time.y with the
    // given chronotensity index [0; 7] and AudioLink band [0; 3].
    float AudioLinkGetChronoTime(uint index, uint band)
    {
        return (AudioLinkDecodeDataAsUInt(ALPASS_CHRONOTENSITY + uint2(index, band))) / 100000.0;
    }

    // Get a chronotensity value in the interval [0; 1], modulated by the speed input,
    // with the given chronotensity index [0; 7] and AudioLink band [0; 3].
    float AudioLinkGetChronoTimeNormalized(uint index, uint band, float speed)
    {
        return frac(AudioLinkGetChronoTime(index, band) * speed);
    }

    // Get a chronotensity value in the interval [0; interval], modulated by the speed input,
    // with the given chronotensity index [0; 7] and AudioLink band [0; 3].
    float AudioLinkGetChronoTimeInterval(uint index, uint band, float speed, float interval)
    {
        return AudioLinkGetChronoTimeNormalized(index, band, speed) * interval;
    }

    // Get time of day. The return value is a float4 with the values float3(hour, minute, second).
    float3 AudioLinkGetTimeOfDay()
    {
        float value = AudioLinkDecodeDataAsSeconds(ALPASS_GENERALVU_UNIX_SECONDS);
        float hour = floor(value / 3600.0);
        float minute = floor(value / 60.0) % 60.0;
        float second = value % 60.0;
        return float3(hour, minute, second);
    }

    // Get a character from a globally synced string, given an index of string in range [0; 3], and
    // a character index in range [0; 31]. The string at the 0th index is the local player name.
    // The 1st index is the master name, and index 2 and 3 are custom strings.
    // Returns a unsigned integer represented a unicode codepoint, i.e. UTF32.
    uint AudioLinkGetGlobalStringChar(uint stringIndex, uint charIndex)
    {
        uint4 fourChars = asuint(AudioLinkData(ALPASS_GLOBAL_STRINGS + uint2(charIndex / 4, stringIndex)));
        return fourChars[charIndex % 4];
    }

    // Get a character from the local player name given a character index in the range [0; 31].
    // Returns a unsigned integer represented a unicode codepoint, i.e. UTF32.
    uint AudioLinkGetLocalPlayerNameChar(uint charIndex)
    {
        return AudioLinkGetGlobalStringChar(AUDIOLINK_STRING_LOCALPLAYER, charIndex);
    }

    // Get a character from the master player name given a character index in the range [0; 31].
    // Returns a unsigned integer represented a unicode codepoint, i.e. UTF32.
    uint AudioLinkGetMasterNameChar(uint charIndex)
    {
        return AudioLinkGetGlobalStringChar(AUDIOLINK_STRING_MASTER, charIndex);
    }

    // Get a character from the first custom string given a character index in the range [0; 31].
    // Returns a unsigned integer represented a unicode codepoint, i.e. UTF32.
    uint AudioLinkGetCustomString1Char(uint charIndex)
    {
        return AudioLinkGetGlobalStringChar(AUDIOLINK_STRING_CUSTOM1, charIndex);
    }

    // Get a character from the second custom string given a character index in the range [0; 31].
    // Returns a unsigned integer represented a unicode codepoint, i.e. UTF32.
    uint AudioLinkGetCustomString2Char(uint charIndex)
    {
        return AudioLinkGetGlobalStringChar(AUDIOLINK_STRING_CUSTOM2, charIndex);
    }

    // Returns the position of the AudioLink AudioSource in world space.
    float4 AudioLinkGetAudioSourcePosition()
    {
        return float4(AudioLinkData(ALPASS_GENERALVU_SOURCE_POS).xyz,1);
    }
#endif