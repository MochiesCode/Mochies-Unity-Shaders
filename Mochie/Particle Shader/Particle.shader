Shader "Mochie/Particle Shader" {
    Properties {

        [HideInInspector]_BlendMode("__mode", Int) = 1.0
        [HideInInspector]_SrcBlend("__src", Int) = 1
        [HideInInspector]_DstBlend("__dst", Int) = 10
        [HideInInspector]_ZT("", Int) = 2
        [Toggle(_)]_ZTest("", Int) = 0
        [Toggle(_)]_ZWrite("", Int) = 0
        [Enum(OFF,0, FRONT,1, BACK,2)]_Culling("", Int) = 2
		[Toggle(_)]_FlipbookBlending("", Int) = 0

        _MainTex("", 2D) = "white" {}
        [HDR]_Color("", Color) = (1,1,1,1)
        [Toggle(_)]_Layering("", Int) = 0
        [Enum(LERP,0, ADD,1, SUB,2, MULT,3)]_TexBlendMode("", Int) = 0
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
		_Luminance ("", Range(0,3)) = 0
		_HDR("", Range(0,1)) = 0
		_Contrast("", Range(0,1)) = 0

        [Toggle(EFFECT_BUMP)]_Distortion("", Int) = 0
        _NormalMap("", 2D) = "bump" {}
        _DistortionStr("", Range(0,1)) = 0
        _DistortionBlend("", Range(0,1)) = 1
        _DistortionSpeedX("", Range(-1,1)) = 0
        _DistortionSpeedY("", Range(-1,1)) = 0

		[Toggle(_)]_Pulse("", Int) = 0
		[Enum(SIN,0, SQUARE,1, TRIANGLE,2, SAW,3, REV_SAW,4)]_Waveform("", Int) = 0
		_PulseStr("", Range(0,1)) = 0.5
		_PulseSpeed("", Float) = 1

        [Toggle(_)]_Falloff("", Int) = 0
        _NearMinRange("", Float) = 1
        _NearMaxRange("", Float) = 5
        _MinRange("", Float) = 8
        _MaxRange("", Float) = 15
    }
    
    SubShader {
		// Render queue options:
		// Background 	- renders first, will show up behind all environment
		// Geometry 	- standard opaque queue for solid objects
		// AlphaTest	- most often used for cutout shading
		// Transparent	- standard queue for transparent objects
		// Overlay		- standard queue for post processing effects and menus
		// Use + or - to go above or below the queue level (ie. Transparent+1)
		// Change the following text seen below: "Queue"="____" to the desired value
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "IgnoreProjector"="True"
            "PreviewType"="Plane" 
			"LightMode"="ForwardBase"
        }
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]
        ZTest [_ZT]
        Cull [_Culling]
        ColorMask RGB

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            #pragma multi_compile_fog
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
					o.pos = 0;
                o.uv0 = v.uv0;
				o.color = v.color;
				o.color.rgb *= _Brightness;
				o.color.a *= _Opacity;
                UNITY_TRANSFER_FOG(o,o.pos);
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