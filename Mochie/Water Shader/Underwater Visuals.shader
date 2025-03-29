Shader "Mochie/Underwater Visuals" {
    Properties {
        
        // Base
        [Enum(Screen Space,0, World Space,1)]_RenderMode("Render Mode", Int) = 0
        _Color("Screen Tint", Color) = (1,1,1,1)
        [IntRange]_StencilRef("Stencil Reference", Range(1,255)) = 65

        // Blur
        [Toggle(DOF_ENABLED)]_DoFToggle("Enable", Int) = 1
        [Toggle(DEPTH_ENABLED)]_DepthToggle("Depth of Field", Int) = 1
        [Toggle(HIGH_QUALITY_BLUR)]_HQBlur("High Quality", Int) = 0
        _BlurStr("Strength", Float) = 1.3
        _Radius("Vision Radius", Float) = 1
        _Fade("Fade", Float) = 1.25

        // Caustics
        [Toggle(CAUSTICS_ENABLED)]_CausticsToggle("Enable", Int) = 1
        [HideInInspector]_NormalMap("Normal Map", 2D) = "bump" {}
        [Enum(Voronoi,0, Texture,1, Flipbook,2)]_CausticsMode("Caustics Style", Int) = 0
        _CausticsTex("Caustics Texture", 2D) = "black" {}
        _CausticsTexArray("Texture Array", 2DArray) = "black" {}
        _CausticsDisp("Dispersion", Float) = 0.25
        _CausticsFlipbookDisp("Dispersion", Float) = 0.6
        _CausticsDistortion("Distortion", Float) = 0.1
        _CausticsDistortionTex("Distortion Texture", 2D) = "bump" {}
        _CausticsDistortionScale("Distortion Scale", Float) = 1
        _CausticsDistortionSpeed("Distortion Speed", Vector) = (-0.1, -0.1,0,0)
        _CausticsColor("Color", Color) = (1,1,1,1)
        _CausticsOpacity("Opacity", Float) = .5
        _CausticsPower("Power", Float) = 1
        _CausticsThreshold("Threshold", Float) = 0
        _CausticsScale("Scale", Float) = 10
        _CausticsSpeed("Speed", Float) = 3
        _CausticsFade("Depth Fade", Float) = 5
        _CausticsRotation("Rotation", Vector) = (-20,0,20,0)
        _CausticsSurfaceFade("Surface Fade", Float) = 100
        _CausticsFlipbookSpeed("Flipbook Speed", Float) = 16

        // Fog
        [Toggle(FOG_ENABLED)]_FogToggle("Enable", Int) = 1
        [HDR]_FogTint("Tint", Color) = (0.11,0.26,0.26,1)
        _FogOpacity("Opacity", Range(0,1)) = 0.8
        _FogRadius("Radius", Float) = 1.7
        _FogFade("Fade", Float) = 3

        [HideInInspector]_NaNLmao("", float) = 0
        [HideInInspector]_MaterialResetCheck("Reset", Int) = 0
        
    }
    SubShader {
        Tags {
            "RenderType"="Transparent" 
            "Queue"="Transparent+1" 
            "ForceNoShadowCasting"="True" 
            "IgnoreProjector"="True"
        }
        Cull Front
        ZTest Always
        ZWrite Off
        Pass {
            Tags {"LightMode"="ForwardBase"}
            Blend One One
            Stencil {
                Ref [_StencilRef]
                Comp NotEqual
                Pass Keep
                Fail Keep
                ZFail Keep
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature_local CAUSTICS_ENABLED
            #pragma shader_feature_local _ _CAUSTICS_VORONOI_ON _CAUSTICS_TEXTURE_ON _CAUSTICS_FLIPBOOK_ON
            #include "UnityCG.cginc"
            #include "../Common/Sampling.cginc"

            MOCHIE_DECLARE_TEX2D_SCREENSPACE(_CameraDepthTexture);
            float4 _CameraDepthTexture_TexelSize;
            #define HAS_DEPTH_TEXTURE
            #include "../Common/Utilities.cginc"
            #include "../Common/Noise.cginc"

            MOCHIE_DECLARE_TEX2D(_CausticsTex);
            MOCHIE_DECLARE_TEX2D(_CausticsDistortionTex);
            MOCHIE_DECLARE_TEX2DARRAY(_CausticsTexArray);
            float4 _CausticsTex_TexelSize;
            float _CausticsFlipbookSpeed;
            float _CausticsDisp;
            float _CausticsDistortion;
            float _CausticsDistortionScale;
            float2 _CausticsDistortionSpeed;
            float3 _CausticsRotation;
            float _CausticsSurfaceFade;
            float3 _CausticsColor;
            float _CausticsScale;
            float _CausticsSpeed;
            float _CausticsPower;
            float _CausticsOpacity;
            float _CausticsFade;
            float _CausticsFlipbookDisp;

            sampler2D _NormalMap;
            float _RenderMode;
            float _NaNLmao;

            struct appdata {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
                float3 raycast : TEXCOORD1;
                float4 localPos : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            v2f vert (appdata v){
                v2f o = (v2f)0;

                #if defined(SHADER_API_MOBILE)
                    v.vertex = 0/_NaNLmao;
                #endif

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // if (_RenderMode == 0){
                    // v.vertex.xyz = Rotate3D(v.vertex.xyz, float3(0,180,0));
                    // v.vertex.z += 0.1;
                    v.vertex.x *= 1.4;
                    float4 wPos = mul(unity_CameraToWorld, v.vertex);
                    float4 oPos = mul(unity_WorldToObject, wPos);
                    o.localPos = oPos;
                    o.raycast = UnityObjectToViewPos(oPos).xyz * float3(-1,-1,1);
                    o.pos = UnityObjectToClipPos(oPos);
                // }
                // else {
                // 	o.pos = UnityObjectToClipPos(v.vertex);
                // 	o.localPos = v.vertex;
                // }
                o.uv = ComputeGrabScreenPos(o.pos);

                return o;
            }

            float GetDepth(v2f i, float2 screenUV){
                float backgroundDepth = LinearEyeDepth(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV));
                float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(i.uv.z);
                float depthDifference = backgroundDepth - surfaceDepth;
                return depthDifference / 20;
            }
            
            float2 GetScreenUV(v2f i){
                float2 screenUV = i.uv.xy / i.uv.w; 
                #if UNITY_UV_STARTS_AT_TOP
                    if (_CameraDepthTexture_TexelSize.y < 0) {
                        screenUV.y = 1 - screenUV.y;
                    }
                #endif
                screenUV.y = _ProjectionParams.x * .5 + .5 - screenUV.y * _ProjectionParams.x;
                return screenUV;
            }

            float4 frag (v2f i) : SV_Target {

                #if defined(SHADER_API_MOBILE)
                    discard;
                #endif

                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 col = 0;
                #ifdef CAUSTICS_ENABLED
                    MirrorCheck();
                    float2 screenUV = GetScreenUV(i);
                    float caustDepth = saturate(1-GetDepth(i, screenUV));
                    float caustFade = saturate(pow(caustDepth, _CausticsFade));
                    #if defined(_CAUSTICS_VORONOI_ON)
                        if (caustFade > 0){
                            float3 wPos = GetWorldSpacePixelPosSP(i.localPos, screenUV);
                            float2 depthUV = Rotate3D(wPos, _CausticsRotation).xz;
                            float3 causticsOffset = UnpackNormal(tex2D(_NormalMap, (depthUV*_CausticsDistortionScale*0.1)+_Time.y*_CausticsDistortionSpeed*0.05));
                            float2 causticsUV = (depthUV + (causticsOffset.xy * _CausticsDistortion)) * _CausticsScale;
                            float voronoi0 = Voronoi2D(causticsUV, _Time.y*_CausticsSpeed);
                            float voronoi1 = Voronoi2D(causticsUV, (_Time.y*_CausticsSpeed)+_CausticsDisp);
                            float voronoi2 = Voronoi2D(causticsUV, (_Time.y*_CausticsSpeed)+_CausticsDisp*2.0);
                            float3 voronoi = float3(voronoi0, voronoi1, voronoi2);
                            voronoi = pow(voronoi, _CausticsPower);
                            float3 caustics = smootherstep(0, 1, voronoi) * _CausticsOpacity * caustFade * _CausticsColor;
                            col.rgb += caustics;
                        }
                    #elif defined(_CAUSTICS_TEXTURE_ON)
                        if (caustFade > 0){
                            float3 wPos = GetWorldSpacePixelPosSP(i.localPos, screenUV);
                            float2 depthUV = Rotate3D(wPos, _CausticsRotation).xz;
                            float3 causticsOffset = UnpackNormal(tex2D(_NormalMap, (depthUV*_CausticsDistortionScale*0.1)+_Time.y*_CausticsDistortionSpeed*0.05));
                            float2 causticsUV = (depthUV + (causticsOffset.xy * _CausticsDistortion)) * _CausticsScale / 5.0;
                            causticsUV *= 0.2;
                            _CausticsSpeed *= 0.05;
                            _CausticsOpacity *= 10;
                            float2 uvTex0 = causticsUV +_Time.y * _CausticsSpeed * 0.4;
                            float2 uvTex1 = causticsUV * -1 + _Time.y * _CausticsSpeed * 0.2;
                            float3 tex0 = MOCHIE_SAMPLE_TEX2D(_CausticsTex, uvTex0);
                            float3 tex1 = MOCHIE_SAMPLE_TEX2D(_CausticsTex, uvTex1);
                            float3 caustics = min(tex0, tex1);
                            caustics = clamp(caustics, 0, 0.1);
                            col.rgb += caustics * _CausticsOpacity;
                        }
                    #elif defined(_CAUSTICS_FLIPBOOK_ON)
                        if (caustFade > 0){
                            float3 wPos = GetWorldSpacePixelPosSP(i.localPos, screenUV);
                            float2 depthUV = Rotate3D(wPos, _CausticsRotation).xz;
                            float2 causticsUV = depthUV * _CausticsScale / 7.0;
                            _CausticsFlipbookSpeed *= 0.8;
                            float causticsR = tex2DflipbookSmooth(_CausticsTexArray, sampler_CausticsTexArray, causticsUV * 0.35, _CausticsFlipbookSpeed).r;
                            float causticsG = tex2DflipbookSmooth(_CausticsTexArray, sampler_CausticsTexArray, causticsUV * 0.35, _CausticsFlipbookSpeed + (0.0005 * _CausticsFlipbookDisp)).g;
                            float causticsB = tex2DflipbookSmooth(_CausticsTexArray, sampler_CausticsTexArray, causticsUV * 0.35, _CausticsFlipbookSpeed + (0.0005 * _CausticsFlipbookDisp * 1.5)).b;
                            float3 caustics = float3(causticsR, causticsG, causticsB);
                            caustics = smootherstep(0.15, 1, caustics);
                            col.rgb += caustics;
                        }
                    #endif
                #else
                    discard;
                #endif

                return float4(col.rgb, 1);
            }

            ENDCG
        }

        Pass {
            Tags {"LightMode"="ForwardBase"}
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature_local FOG_ENABLED
            #include "UnityCG.cginc"
            #include "../Common/Sampling.cginc"

            MOCHIE_DECLARE_TEX2D_SCREENSPACE(_CameraDepthTexture);
            float4 _CameraDepthTexture_TexelSize;
            float4 _FogTint;
            float _FogRadius;
            float _FogFade;
            float _FogOpacity;
            float _RenderMode;
            float _NaNLmao;

            struct appdata {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
                float3 cameraPos : TEXCOORD1;
                float3 raycast : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            float2x2 GetRotationMatrix(float axis){
                float c, s, ang;
                ang = (axis+90) * (UNITY_PI/180.0);
                sincos(ang, c, s);
                float2x2 mat = float2x2(c,-s,s,c);
                mat = ((mat*0.5)+0.5)*2-1;
                return mat;
            }
            float3 Rotate3D(float3 coords, float3 axis){
                coords.xy = mul(GetRotationMatrix(axis.x), coords.xy);
                coords.xz = mul(GetRotationMatrix(axis.y), coords.xz);
                coords.yz = mul(GetRotationMatrix(axis.z), coords.yz);
                return coords;
            }

            v2f vert (appdata v){
                v2f o = (v2f)0;

                #if defined(SHADER_API_MOBILE)
                    v.vertex = 0/_NaNLmao;
                #endif

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.cameraPos = _WorldSpaceCameraPos;
                #if UNITY_SINGLE_PASS_STEREO
                    o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5;
                #endif

                // if (_RenderMode == 0){
                    // v.vertex.xyz = Rotate3D(v.vertex.xyz, float3(0,180,0));
                    // v.vertex.z += 0.1;
                    v.vertex.x *= 1.4;
                    float4 wPos = mul(unity_CameraToWorld, v.vertex);
                    float4 oPos = mul(unity_WorldToObject, wPos);
                    o.raycast = UnityObjectToViewPos(oPos).xyz * float3(-1,-1,1);
                    o.pos = UnityObjectToClipPos(oPos);
                // }
                // else {
                // 	o.pos = UnityObjectToClipPos(v.vertex);
                // }
                o.uv = ComputeGrabScreenPos(o.pos);
                return o;
            }

            float GetRadius(v2f i, float2 screenUV){
                float depth = Linear01Depth(DecodeFloatRG(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV)));
                i.raycast *= (_ProjectionParams.z / i.raycast.z);
                float4 vPos = float4(i.raycast * depth, 1);
                float3 wPos = mul(unity_CameraToWorld, vPos).xyz;
                float dist = distance(wPos, i.cameraPos);
                return 1-smoothstep(_FogRadius, _FogRadius-_FogFade, dist);
            }
            
            float2 GetScreenUV(v2f i){
                float2 screenUV = i.uv.xy / i.uv.w; 
                #if UNITY_UV_STARTS_AT_TOP
                    if (_CameraDepthTexture_TexelSize.y < 0) {
                        screenUV.y = 1 - screenUV.y;
                    }
                #endif
                screenUV.y = _ProjectionParams.x * .5 + .5 - screenUV.y * _ProjectionParams.x;
                return screenUV;
            }
            
            void MirrorCheck(){
                if (unity_CameraProjection[2][0] != 0.0f || unity_CameraProjection[2][1] != 0.0f) discard;
            }

            float4 frag (v2f i) : SV_Target {

                #if defined(SHADER_API_MOBILE)
                    discard;
                #endif

                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 fogCol = 0;
                #ifdef FOG_ENABLED
                    MirrorCheck();
                    float2 screenUV = GetScreenUV(i);
                    float radius = GetRadius(i, screenUV); 
                    fogCol = float4(_FogTint.rgb * radius, _FogOpacity * radius);
                #else
                    discard;
                #endif

                return fogCol;
            }
            ENDCG
        }

        GrabPass{
            "_DoFGrab"
            Tags {"LightMode"="Always"}
        }

        Pass {
            Tags {"LightMode"="Always"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature_local DOF_ENABLED
            #pragma shader_feature_local HIGH_QUALITY_BLUR
            #pragma shader_feature_local DEPTH_ENABLED
            #include "UnityCG.cginc"
            #include "../Common/Sampling.cginc"

            #ifdef DEPTH_ENABLED
                MOCHIE_DECLARE_TEX2D_SCREENSPACE(_CameraDepthTexture);
                float4 _CameraDepthTexture_TexelSize;
            #endif
            MOCHIE_DECLARE_TEX2D_SCREENSPACE(_DoFGrab);
            float _Radius, _Fade, _BlurStr;
            
            float4 _Color;
            float _RenderMode;
            float _NaNLmao;
            
            #include "WaterBlurKernels.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
                float3 cameraPos : TEXCOORD1;
                float3 raycast : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2x2 GetRotationMatrix(float axis){
                float c, s, ang;
                ang = (axis+90) * (UNITY_PI/180.0);
                sincos(ang, c, s);
                float2x2 mat = float2x2(c,-s,s,c);
                mat = ((mat*0.5)+0.5)*2-1;
                return mat;
            }
            float3 Rotate3D(float3 coords, float3 axis){
                coords.xy = mul(GetRotationMatrix(axis.x), coords.xy);
                coords.xz = mul(GetRotationMatrix(axis.y), coords.xz);
                coords.yz = mul(GetRotationMatrix(axis.z), coords.yz);
                return coords;
            }

            v2f vert (appdata v){
                v2f o = (v2f)0;

                #if defined(SHADER_API_MOBILE)
                    v.vertex = 0/_NaNLmao;
                #endif

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.cameraPos = _WorldSpaceCameraPos;
                #if UNITY_SINGLE_PASS_STEREO
                    o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5;
                #endif

                // if (_RenderMode == 0){
                    // v.vertex.xyz = Rotate3D(v.vertex.xyz, float3(0,180,0));
                    // v.vertex.z += 0.1;
                    v.vertex.x *= 1.4;
                    float4 wPos = mul(unity_CameraToWorld, v.vertex);
                    float4 oPos = mul(unity_WorldToObject, wPos);
                    #ifdef DEPTH_ENABLED
                        o.raycast = UnityObjectToViewPos(oPos).xyz * float3(-1,-1,1);
                    #endif
                    o.pos = UnityObjectToClipPos(oPos);
                // }
                // else {
                // 	o.pos = UnityObjectToClipPos(v.vertex);
                // }
                o.uv = ComputeGrabScreenPos(o.pos);
                return o;
            }
            
            void MirrorCheck(){
                if (unity_CameraProjection[2][0] != 0.0f || unity_CameraProjection[2][1] != 0.0f) discard;
            }

            #ifdef DEPTH_ENABLED
                float GetRadius(v2f i){
                    float2 screenUV = i.uv.xy/i.uv.w;
                    #if UNITY_UV_STARTS_AT_TOP
                        if (_CameraDepthTexture_TexelSize.y < 0) {
                            screenUV.y = 1 - screenUV.y;
                        }
                    #endif
                    screenUV.y = _ProjectionParams.x * .5 + .5 - screenUV.y * _ProjectionParams.x;
                    float depth = Linear01Depth(DecodeFloatRG(MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_CameraDepthTexture, screenUV)));
                    i.raycast *= (_ProjectionParams.z / i.raycast.z);
                    float4 vPos = float4(i.raycast * depth, 1);
                    float3 wPos = mul(unity_CameraToWorld, vPos).xyz;
                    float dist = distance(wPos, i.cameraPos);
                    return 1-smoothstep(_Radius, _Radius-_Fade, dist);
                }
            #endif

            float4 frag (v2f i) : SV_Target {
                
                #if defined(SHADER_API_MOBILE)
                    discard;
                #endif

                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 blurCol = 0;
                #ifdef DOF_ENABLED
                    MirrorCheck();
                    float2 blurStr = _BlurStr * 0.01;
                    #ifdef DEPTH_ENABLED
                        blurStr *= GetRadius(i);
                    #endif
                    blurStr.x *= 0.5625;
                    #if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
                        blurStr *= 0.5;
                    #endif
                    float2 uv = i.uv.xy / i.uv.w;
                    float2 uvb = uv;
                    #ifdef HIGH_QUALITY_BLUR
                        blurStr *= 1.25;
                        [unroll(136)]
                        for (uint k = 0; k < 137; ++k){
                            uvb.xy = uv.xy + (blurKernel[k] * blurStr);
                            blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_DoFGrab, uvb);
                        }
                        blurCol /= 137;
                    #else
                        [unroll(43)]
                        for (uint k = 0; k < 44; ++k){
                            uvb.xy = uv.xy + (blurKernel[k] * blurStr);
                            blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_DoFGrab, uvb);
                        }
                        blurCol /= 43;
                    #endif
                #else
                    discard;
                #endif

                return blurCol * _Color;
            }
            ENDCG
        }
    }
    CustomEditor "Mochie.UnderwaterEditor"
}