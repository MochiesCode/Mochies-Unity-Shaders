// By Mochie#8794

Shader "Mochie/Particles" {
    Properties {
        
        [Enum(Alpha,0, Premultiplied,1, Additive,2, Soft Additive,3, Multiply,4, Multiply x2,5)]_BlendMode("", Int) = 1.0
        [HideInInspector]_SrcBlend("__src", Int) = 1
        [HideInInspector]_DstBlend("__dst", Int) = 10
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Int) = 4
        [ToggleUI]_ZWrite("", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)]_Culling("", Int) = 2
		[ToggleUI]_FlipbookBlending("", Int) = 0

        _MainTex("", 2D) = "white" {}
        [HDR]_Color("", Color) = (1,1,1,1)
        [ToggleUI]_Layering("", Int) = 0
        [Enum(Lerp,0, Add,1, Sub,2, Mult,3)]_TexBlendMode("", Int) = 0
        _SecondTex("", 2D) = "white" {}
        [HDR]_SecondColor("", Color) = (1,1,1,1)
        _SecondTexScroll("", Vector) = (0,0,0,0)
		_Brightness("", Float) = 1
		_Opacity("", Range(0,1)) = 1

        _IsCutout("", Int) = 0
        _Cutoff("", Range(0,1)) = 0.5
        [ToggleUI]_Softening("", Int) = 0
        _SoftenStr("", Range(0, 0.999)) = 0

        [ToggleUI]_Filtering("", Int) = 0
        [ToggleUI]_AutoShift("", Int) = 0
		_AutoShiftSpeed("", Float) = 0.25
		_Hue("", Range(0,1)) = 0
		_Saturation("", Float) = 1
		_HDR("", Float) = 0
		_Contrast("", Float) = 1

        [ToggleUI]_Distortion("", Int) = 0
        _NormalMap("", 2D) = "bump" {}
		_NormalMapScale("", Vector) = (1,1,0,0)
		[ToggleUI]_DistortMainTex("", Int) = 0
        _DistortionStr("", Float) = 0
        _DistortionBlend("", Range(0,1)) = 0.5
        _DistortionSpeed("", Vector) = (0,0,0,0)

		[ToggleUI]_Pulse("", Int) = 0
		[Enum(Sin,0, Square,1, Triangle,2, Saw,3, Reverse Saw,4)]_Waveform("", Int) = 0
		_PulseStr("", Range(0,1)) = 0.5
		_PulseSpeed("", Float) = 1

        [ToggleUI]_Falloff("", Int) = 0
		[Enum(Per Particle,0, Per Vertex,1)]_FalloffMode("", Int) = 0
        _NearMinRange("", Float) = 1
        _NearMaxRange("", Float) = 5
        _MinRange("", Float) = 8
        _MaxRange("", Float) = 15

        [ToggleUI]_AudioLink("", Int) = 0
        _AudioLinkStrength("", Range(0,1)) = 1
        _AudioLinkRemapMin("", Float) = 0
        _AudioLinkRemapMax("", Float) = 1
        
        [Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkFilterBand("", Int) = 0
        _AudioLinkFilterStrength("", Range(0,1)) = 0
        _AudioLinkRemapFilterMin("", Float) = 0
        _AudioLinkRemapFilterMax("", Float) = 1

        [Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkDistortionBand("", Int) = 0
        _AudioLinkDistortionStrength("", Range(0,1)) = 0
        _AudioLinkRemapDistortionMin("", Float) = 0
        _AudioLinkRemapDistortionMax("", Float) = 1

        [Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkOpacityBand("", Int) = 0
        _AudioLinkOpacityStrength("", Range(0,1)) = 0
        _AudioLinkRemapOpacityMin("", Float) = 0
        _AudioLinkRemapOpacityMax("", Float) = 1

        [Enum(Bass,0, Low Mids,1, Upper Mids,2, Highs,3)]_AudioLinkCutoutBand("", Int) = 0
        _AudioLinkCutoutStrength("", Range(0,1)) = 0
        _AudioLinkRemapCutoutMin("", Float) = 0
        _AudioLinkRemapCutoutMax("", Float) = 1


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
		GrabPass {
			Tags {"LightMode"="Always"}
			"_MPSGrab"
		}
        Blend [_SrcBlend] [_DstBlend]
        Cull [_Culling]
		ZTest [_ZTest]
		ZWrite Off
        ColorMask RGB

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
			#pragma shader_feature_local _ _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHA_ADD_ON _ALPHA_ADD_SOFT_ON _ALPHA_MUL_ON _ALPHA_MULX2_ON
			#pragma shader_feature_local _ALPHATEST_ON
			#pragma shader_feature_local _FILTERING_ON
			#pragma shader_feature_local _DISTORTION_ON
			#pragma shader_feature_local _DISTORTION_UV_ON
			#pragma shader_feature_local _LAYERED_TEX_ON
			#pragma shader_feature_local _PULSE_ON
			#pragma shader_feature_local _FALLOFF_ON
			#pragma shader_feature_local _FLIPBOOK_BLENDING_ON
			#pragma shader_feature_local _FADING_ON
            #pragma shader_feature_local _AUDIOLINK_ON
            #pragma multi_compile_instancing
            #include "PSDefines.cginc"

            v2f vert (appdata v){
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.pos = UnityObjectToClipPos(v.vertex);
				o.uv0 = v.uv0;
				o.color = v.color;

				#if FADING_ENABLED
					o.projPos = GetProjPos(v.vertex.xyzz, o.pos);
				#endif

				#if PULSE_ENABLED
                	o.pulse = GetPulse();
				#endif

				#if FALLOFF_ENABLED
					o.center = v.center;
					o.vertex = v.vertex;
					o.falloff = GetFalloff(o);
					UNITY_BRANCH
					if (o.falloff <= 0.0001)
						o.pos = 0.0/_NaNLmao;
				#endif

                #if DISTORTION_ENABLED
                    o.uv1 = ComputeGrabScreenPos(o.pos);
                #endif
				
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                return GetColor(i);
            }
            ENDCG
        }
    }
    CustomEditor "PSEditor"
}