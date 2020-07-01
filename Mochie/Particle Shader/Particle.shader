// BY MOCHIE
// Version 1.2

Shader "Mochie/Particle Shader" {
    Properties {

        [HideInInspector]_BlendMode("__mode", Int) = 1.0
        [HideInInspector]_SrcBlend("__src", Int) = 1
        [HideInInspector]_DstBlend("__dst", Int) = 10
        [HideInInspector]_ZT("", Int) = 2
        [Toggle(_)]_ZTest("", Int) = 0
        [Toggle(_)]_ZWrite("", Int) = 0
        [Enum(Off,0, Front,1, Back,2)]_Culling("", Int) = 2
		[Toggle(_)]_FlipbookBlending("", Int) = 0

        _MainTex("", 2D) = "white" {}
        [HDR]_Color("", Color) = (1,1,1,1)
        [Toggle(_)]_Layering("", Int) = 0
        [Enum(Lerp,0, Add,1, Sub,2, Mult,3)]_TexBlendMode("", Int) = 0
        _SecondTex("", 2D) = "white" {}
        [HDR]_SecondColor("", Color) = (1,1,1,1)
		_Brightness("", Float) = 1
		_Opacity("", Range(0,1)) = 1

        _IsCutout("", Int) = 0
        _Cutout("", Range(0,1)) = 0
        [Toggle(_FADING_ON)]_Softening("", Int) = 0
        _SoftenStr("", Range(0, 0.999)) = 0

        [Toggle(_)]_Filtering("", Int) = 0
        [Toggle(_)]_AutoShift("", Int) = 0
		_AutoShiftSpeed("", Range(0,1)) = 0.25
		_Hue("", Range(0,1)) = 0
		_Saturation("", Range(0,2)) = 1
		_Value("", Range(-3,3)) = 0
		_HDR("", Range(0,1)) = 0
		_Contrast("", Range(0,2)) = 1

        [Toggle(EFFECT_BUMP)]_Distortion("", Int) = 0
        _NormalMap("", 2D) = "bump" {}
		_NormalMapScale("", Vector) = (1,1,0,0)
		[Toggle(_)]_DistortMainTex("", Int) = 0
        _DistortionStr("", Float) = 0
        _DistortionBlend("", Range(0,1)) = 0.5
        _DistortionSpeed("", Vector) = (0,0,0,0)

		[Toggle(_)]_Pulse("", Int) = 0
		[Enum(Sin,0, Square,1, Triangle,2, Saw,3, Reverse Saw,4)]_Waveform("", Int) = 0
		_PulseStr("", Range(0,1)) = 0.5
		_PulseSpeed("", Float) = 1

        [Toggle(_)]_Falloff("", Int) = 0
        _NearMinRange("", Float) = 1
        _NearMaxRange("", Float) = 5
        _MinRange("", Float) = 8
        _MaxRange("", Float) = 15

		[HideInInspector]_NaNLmao("", Float) = 0.0
    }
    
    SubShader {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "IgnoreProjector"="True"
            "PreviewType"="Plane" 
			"LightMode"="ForwardBase"
        }
        Blend [_SrcBlend] [_DstBlend]
        Cull [_Culling]
		ZTest [_ZT]
		ZWrite Off
        ColorMask RGB

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
			#pragma multi_compile_particles
			#pragma shader_feature _FADING_ON
			#include "PSDefines.cginc"

            v2f vert (appdata v){
                v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.pos = UnityObjectToClipPos(v.vertex);
				o.projPos = GetProjPos(v.vertex.xyzz, o.pos);
                o.falloff = GetFalloff(v.vertex.xyzz);
				o.pulse = GetPulse();
				UNITY_BRANCH
				if (_Falloff == 1 && o.falloff <= 0.0001)
					o.pos = 0.0/_NaNLmao;
                o.uv0 = v.uv0;
				o.color = v.color;
				o.color.rgb *= _Brightness;
				o.color.a *= _Opacity;
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                return GetColor(i);
            }
            ENDCG
        }
    }
    Fallback "Particles/Additive"
    CustomEditor "PSEditor"
}