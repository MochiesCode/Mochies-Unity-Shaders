// Used by the Texture Packer window (Mochie.TexturePackerWindow) to pack, resample and preview channels
Shader "Hidden/Mochie/TexturePackerWindow"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Tex0("Red", 2D) = "white" {}
        _Tex1("Green", 2D) = "white" {}
        _Tex2("Blue", 2D) = "white" {}
        _Tex3("Alpha", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Blend Off

        // 0: Packs one value from each channel's source into RGBA
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _Tex0, _Tex1, _Tex2, _Tex3;
            float4 _Tex0_TexelSize, _Tex1_TexelSize, _Tex2_TexelSize, _Tex3_TexelSize;

            // Select: weights for the source's RGBA, either one channel or grayscale
            // Params: x = has texture, y = fill value, z = invert, w = point sampling
            // Filter: x = brightness, y = contrast, z = 1 / gamma, w = re-encode sRGB decoded samples
            // Levels: x = input black, y = input white, z = output black, w = output white
            float4 _Select[4];
            float4 _Params[4];
            float4 _Filter[4];
            float4 _Levels[4];

            float PackChannel(sampler2D tex, float4 texelSize, float2 uv, int i){
                float4 params = _Params[i];
                float4 filter = _Filter[i];
                float4 levels = _Levels[i];

                // Point sampling reads the nearest texel's center, which bilinear filtering returns unchanged.
                // Both are sampled so the implicit derivative read stays outside of flow control.
                float2 pointUV = (floor(uv * texelSize.zw) + 0.5) * texelSize.xy;
                float4 bilinear = tex2D(tex, uv);
                float4 nearest = tex2Dlod(tex, float4(pointUV, 0, 0));
                float4 col = params.w > 0.5 ? nearest : bilinear;

                // Alpha is never sRGB decoded, so only color is re-encoded
                if (filter.w > 0.5)
                    col.rgb = float3(LinearToGammaSpaceExact(col.r), LinearToGammaSpaceExact(col.g), LinearToGammaSpaceExact(col.b));

                float value = dot(col, _Select[i]);
                if (params.z > 0.5)
                    value = 1.0 - value;
                value = saturate((value - levels.x) / max(levels.y - levels.x, 0.0001));
                value = pow(value, filter.z);
                value = lerp(0.5, value, filter.y);
                value *= filter.x;
                value = lerp(levels.z, levels.w, saturate(value));
                return params.x > 0.5 ? value : params.y;
            }

            float4 frag(v2f_img i) : SV_Target {
                return saturate(float4(
                    PackChannel(_Tex0, _Tex0_TexelSize, i.uv, 0),
                    PackChannel(_Tex1, _Tex1_TexelSize, i.uv, 1),
                    PackChannel(_Tex2, _Tex2_TexelSize, i.uv, 2),
                    PackChannel(_Tex3, _Tex3_TexelSize, i.uv, 3)
                ));
            }
            ENDCG
        }

        // 1: Copies _MainTex with its stored values, used to shrink sources a half step at a time
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _EncodeSRGB;

            float4 frag(v2f_img i) : SV_Target {
                float4 col = tex2Dlod(_MainTex, float4(i.uv, 0, 0));
                if (_EncodeSRGB > 0.5)
                    col.rgb = float3(LinearToGammaSpaceExact(col.r), LinearToGammaSpaceExact(col.g), LinearToGammaSpaceExact(col.b));
                return col;
            }
            ENDCG
        }

        // 2: Shows the packed result in the window, as RGB or as one channel in grayscale
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _View; // Weights of the channel to show, or zero for RGB

            float4 frag(v2f_img i) : SV_Target {
                float4 col = tex2D(_MainTex, i.uv);
                float value = dot(col, _View);
                return any(_View) ? float4(value.xxx, 1) : float4(col.rgb, 1);
            }
            ENDCG
        }
    }
}
