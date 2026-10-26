// Used by Mochie.TexturePacker to combine up to four grayscale maps into one texture
Shader "Hidden/Mochie/ChannelPacker"
{
    Properties
    {
        _Red("Red", 2D) = "white" {}
        _Green("Green", 2D) = "white" {}
        _Blue("Blue", 2D) = "white" {}
        _Alpha("Alpha", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _Red, _Green, _Blue, _Alpha;
            float4 _Red_ST, _Green_ST, _Blue_ST, _Alpha_ST;

            // Mask: selects the source channel
            // Params: x = has texture, y = strength, z = fallback, w = invert
            // Flags: x = occlusion (strength lerps from 1), y = re-encode sRGB decoded samples
            float4 _RedMask, _GreenMask, _BlueMask, _AlphaMask;
            float4 _RedParams, _GreenParams, _BlueParams, _AlphaParams;
            float4 _RedFlags, _GreenFlags, _BlueFlags, _AlphaFlags;

            float PackChannel(sampler2D tex, float2 uv, float4 st, float4 mask, float4 params, float4 flags){
                if (params.x < 0.5)
                    return params.z;

                // Sources have no mips and the output is never smaller than them, so always read the top mip
                float value = dot(tex2Dlod(tex, float4(uv * st.xy + st.zw, 0, 0)), mask);
                if (flags.y > 0.5)
                    value = LinearToGammaSpaceExact(value);
                if (params.w > 0.5)
                    value = 1.0 - value;
                return flags.x > 0.5 ? lerp(1.0, value, params.y) : value * params.y;
            }

            float4 frag(v2f_img i) : SV_Target
            {
                return saturate(float4(
                    PackChannel(_Red, i.uv, _Red_ST, _RedMask, _RedParams, _RedFlags),
                    PackChannel(_Green, i.uv, _Green_ST, _GreenMask, _GreenParams, _GreenFlags),
                    PackChannel(_Blue, i.uv, _Blue_ST, _BlueMask, _BlueParams, _BlueFlags),
                    PackChannel(_Alpha, i.uv, _Alpha_ST, _AlphaMask, _AlphaParams, _AlphaFlags)
                ));
            }
            ENDCG
        }
    }
}
