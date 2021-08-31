// By Mochie#8794

Shader "Mochie/Water" {
    Properties {
		
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Base Color", 2D) = "white" {}
		_MainTexScroll("Scrolling", Vector) = (0,0.1,0,0)
		_DistortionStrength("Distortion Strength", Float) = 0.2
		_Roughness("Roughness", Range(0,1)) = 0
		_Metallic("Metallic", Range(0,1)) = 0
		_Opacity("Opacity", Range(0,1)) = 0
		[Toggle(_REFLECTIONS_ON)]_Reflections("Reflections", Int) = 1
		_ReflStrength("Reflection Strength", Float) = 1
		[Toggle(_SPECULAR_ON)]_Specular("Specular", Int) = 1
		_SpecStrength("Specular Strength", Float) = 1
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull", Int) = 2
		
		[NoScaleOffset]_NormalMap0 ("", 2D) = "bump" {}
		_NormalStr0("Strength", Float) = 0.3
		_NormalMapScale0("Scale", Vector) = (3,3,0,0)
		_Rotation0("Rotation", Float) = 0
		_NormalMapScroll0("Scrolling", Vector) = (0.1,0.1,0,0)
		
		[Toggle(_NORMALMAP_1_ON)]_Normal1Toggle("Enable", Int) = 1
		[NoScaleOffset]_NormalMap1("", 2D) = "bump" {}
		_NormalStr1("Strength", Float) = 0.4
		_NormalMapScale1("Scale", Vector) = (4,4,0,0)
		_Rotation1("Rotation", Float) = 0
		_NormalMapScroll1("Scrolling", Vector) = (-0.1,0.1,0,0)

		[Toggle(_FLOW_ON)]_FlowToggle("Enable", Int) = 1
		[NoScaleOffset]_FlowMap("Flow Map", 2D) = "black" {}
		_FlowSpeed("Speed", Float) = 0.25
		_FlowStrength("Strength", Float) = 0.1
		_FlowMapScale("Scale", Vector) = (2,2,0,0)

		[Toggle(_VERTEX_OFFSET_ON)]_VertOffsetToggle("Enable", Int) = 1
		[NoScaleOffset]_NoiseTex("Noise Texture", 2D) = "black" {}
		_NoiseTexScale("Scale", Vector) = (1,1,0,0)
		_NoiseTexScroll("Scrolling", Vector) = (0.3,0.06,0,0)
		_NoiseTexBlur("Blur", Range(0,1)) = 0.8
		_WaveHeight("Strength", Float) = 0.1
		_Offset("Offset", Vector) = (0,1,0,0)

		[Toggle(_CAUSTICS_ON)]_CausticsToggle("Enable", Int) = 1
		_CausticsOpacity("Opacity", Range(0,1)) = 0.5
		_CausticsPower("Power", Float) = 5
		_CausticsScale("Scale", Float) = 5
		_CausticsSpeed("Speed", Float) = 1
		_CausticsFade("Depth Fade", Float) = 10
		
		[Toggle(_DEPTHFOG_ON)]_FogToggle("Enable", Int) = 1
		[HDR]_FogTint("Color", Color) = (1,1,1,0.3)
		_FogPower("Power", Float) = 1

		[Toggle(_FOAM_ON)]_FoamToggle("Enable", Int) = 1
		[NoScaleOffset]_FoamTex("Foam Texture", 2D) = "white" {}
		_FoamTexScale("Scale", Vector) = (3,3,0,0)
		_FoamRoughness("Roughness", Range(0,1)) = 0.6
		_FoamColor("Color", Color) = (1,1,1,1)
		_FoamPower("Power", Float) = 200
		_FoamOpacity("Opacity", Float) = 1

		[Toggle(_EDGEFADE_ON)]_EdgeFadeToggle("Enable", Int) = 1
		_EdgeFadePower("Power", Float) = 200
		_EdgeFadeOffset("Offset", Float) = 0.5
    }

    SubShader {
        Tags {
			"Queue"="Transparent" 
			"RenderType"="Transparent"
			"DisableBatching"="True"
			"IgnoreProjector"="True"
			"PreviewType"="Plane"
			"ForceNoShadowCasting"="True"
		}
		GrabPass {"_MWGrab"}
		ZWrite Off
		Cull [_CullMode]
        Pass {
			Tags {"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma shader_feature_local _REFLECTIONS_ON
			#pragma shader_feature_local _SPECULAR_ON
			#pragma shader_feature_local _NORMALMAP_1_ON
			#pragma shader_feature_local _FLOW_ON
			#pragma shader_feature_local _VERTEX_OFFSET_ON
			#pragma shader_feature_local _DEPTHFOG_ON
			#pragma shader_feature_local _FOAM_ON
			#pragma shader_feature_local _CAUSTICS_ON
			#pragma shader_feature_local _EDGEFADE_ON
            #pragma target 5.0

			#include "WaterDefines.cginc"

            v2f vert (appdata v) {
                v2f o = (v2f)0;
				#if VERTEX_OFFSET_ENABLED
					float2 noiseUV = ScaleUV(v.uv, _NoiseTexScale, _NoiseTexScroll*10);
					float noiseWaveTex = tex2Dlod(_NoiseTex, float4(noiseUV,0,lerp(0,8,_NoiseTexBlur)));
					float noiseWave = Remap(noiseWaveTex, 0, 1, -1, 1);
					float offsetWave = noiseWave * _WaveHeight;
					v.vertex.x += offsetWave * _Offset.x;
					v.vertex.y += offsetWave * _Offset.y;
					v.vertex.z += offsetWave * _Offset.z;
				#endif
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
				o.cNormal = UnityObjectToWorldNormal(v.normal);
                o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0)).xyz);
                o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.uvGrab = ComputeGrabScreenPos(o.pos);
				o.localPos = v.vertex;
				o.uv = v.uv;
				UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            float4 frag(v2f i, bool isFrontFace: SV_IsFrontFace) : SV_Target {

				float3 normalMap;
				float3 detailNormal;
				float2 uvNormal0 = ScaleUV(i.uv, _NormalMapScale0, _NormalMapScroll0);
				float2 uvNormal1 = ScaleUV(i.uv, _NormalMapScale1, _NormalMapScroll1);
				float2 baseUV0 = Rotate2D(uvNormal0, _Rotation0);
				float2 baseUV1 = Rotate2D(uvNormal1, _Rotation1);
				float3 uv00 = float3(baseUV0, 1);
				float3 uv10 = float3(baseUV1, 1);
				#if FOAM_ENABLED
					float2 uvFoam = ScaleUV(i.uv, _FoamTexScale, 0);
					float3 uvF0 = float3(uvFoam, 1);
					float3 uvF1 = uvF0;
				#endif

				#if FLOW_ENABLED
					float2 uvFlow = ScaleUV(i.uv, _FlowMapScale, 0);
					float4 flowMap = tex2D(_FlowMap, uvFlow);
					float2 flow = (flowMap.rg * 2 - 1) * _FlowStrength * 0.1;
					float time = _Time.y * _FlowSpeed + flowMap.a;
					uv00 = FlowUV(baseUV0, flow, time, 0);
					float3 uv01 = FlowUV(baseUV0, flow, time, 0.5);
					uv10 = FlowUV(baseUV1, flow, time, 0);
					float3 uv11 = FlowUV(baseUV1, flow, time, 0.5);
					#if FOAM_ENABLED
						uvF0 = FlowUV(uvFoam, flow, time, 0);
						uvF0.x += _Time.y * 0.01;
						uvF1 = FlowUV(uvFoam, flow, time, 0.5);
						uvF1.x -= _Time.y*0.01;
					#endif
				#endif

				#if NORMALMAP1_ENABLED
					float3 normalMap0 = UnpackScaleNormal(tex2D(_NormalMap0, uv00.xy), _NormalStr0) * uv00.z;
					float3 detailNormal0 = UnpackScaleNormal(tex2D(_NormalMap1, uv10.xy), _NormalStr1) * uv10.z;
					#if FLOW_ENABLED
						float3 normalMap1 = UnpackScaleNormal(tex2D(_NormalMap0, uv01.xy), _NormalStr0) * uv01.z;
						float3 detailNormal1 = UnpackScaleNormal(tex2D(_NormalMap1, uv11.xy), _NormalStr1) * uv11.z;
						normalMap0 = normalize(normalMap0 + normalMap1);
						detailNormal0 = normalize(detailNormal0 + detailNormal1);
					#endif
					normalMap = BlendNormals(normalMap0, detailNormal0);
				#else
					float3 normalMap0 = UnpackScaleNormal(tex2D(_NormalMap0, uv00.xy), _NormalStr0) * uv00.z;
					#if FLOW_ENABLED
						float3 normalMap1 = UnpackScaleNormal(tex2D(_NormalMap0, uv01.xy), _NormalStr0) * uv01.z;
						normalMap = normalize(normalMap0 + normalMap1);
					#else
						normalMap = normalize(normalMap0);
					#endif
				#endif

				float2 uvOffset = normalMap.xy * _DistortionStrength;
				#if UNITY_SINGLE_PASS_STEREO
					uvOffset.x *= 0.5;
					uvOffset.xy *= 0.5;
				#endif
				float proj = i.uvGrab.w + 0.00001;
				float2 screenUV = 0;
				if (!IsInMirror()){
					screenUV = AlignWithGrabTexel((i.uvGrab.xy + uvOffset) / proj);
					if (GetDepth(i, screenUV) < 0){
						screenUV = AlignWithGrabTexel(i.uvGrab.xy / proj);
					}
				}
				else screenUV = (i.uvGrab.xy + uvOffset) / proj;

				float2 baseUV = i.uvGrab.xy/proj;
				float2 mainTexUV = TRANSFORM_TEX(i.uv, _MainTex) + _Time.y * 0.1 * _MainTexScroll;
				float4 mainTex = tex2D(_MainTex, mainTexUV);
				float4 baseCol = tex2D(_MWGrab, baseUV) * mainTex;
				float4 col = lerp(tex2D(_MWGrab, screenUV)*_Color*mainTex, _Color*mainTex, _Opacity);

				#if FOAM_ENABLED
					float depth = saturate(1-GetDepth(i, screenUV));
					float foamDepth = saturate(pow(depth,_FoamPower));
					#if FLOW_ENABLED
						float4 foamTex0 = tex2D(_FoamTex, uvF0.xy) * uvF0.z;
						float4 foamTex1 = tex2D(_FoamTex, uvF1.xy) * uvF1.z;
						float4 foamTex = (foamTex0 + foamTex1) * _FoamColor;
					#else
						float4 foamTex = tex2D(_FoamTex, uvFoam) * _FoamColor;
					#endif
					float foam = saturate(foamTex.a * foamDepth * _FoamOpacity * Average(foamTex.rgb));

					col.rgb = lerp(col.rgb, foamTex.rgb, foam);
				#endif

				#if PBR_ENABLED
					i.normal = normalize(dot(i.normal, i.normal) >= 1.01 ? i.cNormal : i.normal);
					float3 normalDir = normalize(normalMap.x * i.tangent + normalMap.y * i.binormal + normalMap.z * i.normal);
					float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
					float NdotV = abs(dot(normalDir, viewDir));
					#if FOAM_ENABLED
						_Roughness = lerp(_Roughness, _FoamRoughness, foam);
					#endif
					float roughSq = _Roughness * _Roughness;
					float roughBRDF = max(roughSq, 0.003);
						
					float omr = unity_ColorSpaceDielectricSpec.a - _Metallic * unity_ColorSpaceDielectricSpec.a;
					float3 specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, 1, _Metallic);

					#if REFLECTIONS_ENABLED
						if (isFrontFace){
							float3 reflDir = reflect(-viewDir, normalDir);
							float surfaceReduction = 1.0 / (roughBRDF*roughBRDF + 1.0);
							float grazingTerm = saturate((1-_Roughness) + (1-omr));
							float fresnel = FresnelLerp(specularTint, grazingTerm, NdotV);
							float3 reflCol = GetWorldReflections(reflDir, i.worldPos, _Roughness);
							col.rgb += (reflCol * fresnel * surfaceReduction);
						}
					#endif

					#if SPECULAR_ENABLED
						if (isFrontFace){
							float roughInterp = smoothstep(0.001, 0.003, roughSq);
							float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
							float3 halfVector = normalize(lightDir + viewDir);
							float NdotL = dot(normalDir, lightDir);
							float NdotH = Safe_DotClamped(normalDir, halfVector);
							float LdotH = Safe_DotClamped(lightDir, halfVector);
							float3 fresnelTerm = FresnelTerm(specularTint, LdotH);
							float specularTerm = SpecularTerm(NdotL, NdotV, NdotH, roughBRDF);
							float3 specCol = _LightColor0 * fresnelTerm * specularTerm;
							specCol = lerp(smootherstep(0, 0.9, specCol), specCol, roughInterp);
							col.rgb += specCol;
						}
					#endif
				#endif

				#if EDGEFADE_ENABLED
					float edgeFadeDepth = saturate(1-GetDepth(i, baseUV));
					edgeFadeDepth = (1-saturate(pow(edgeFadeDepth, _EdgeFadePower)));
					edgeFadeDepth = saturate(Remap(edgeFadeDepth, 0, 1, -_EdgeFadeOffset, 1));
				#endif

				if (isFrontFace){
					#if CAUSTICS_ENABLED
						float caustDepth = 0;
						#if FOAM_ENABLED
							caustDepth = depth;
						#elif FOG_ENABLED
							caustDepth = fogDepth;
						#else
							caustDepth = saturate(1-GetDepth(i, screenUV));
						#endif
						float caustFade = saturate(pow(caustDepth, _CausticsFade));
						if (caustFade > 0){
							float3 wPos = GetWorldSpacePixelPosSP(i.localPos, screenUV);
							float4 causticsUV = float4(wPos*_CausticsScale, _Time.y*_CausticsSpeed);
							float voronoi = GetVoronoi4D(causticsUV, 1);
							float caustics = saturate(pow(voronoi, _CausticsPower)) * _CausticsOpacity * caustFade;
							#if EDGEFADE_ENABLED
								caustics *= saturate(pow(edgeFadeDepth, _EdgeFadePower));
							#endif
							col.rgb += col.rgb * caustics;
						}
					#endif

					#if DEPTHFOG_ENABLED
						float fogDepth = 0;
						#if FOAM_ENABLED
							fogDepth = depth;
						#else
							fogDepth = saturate(1-GetDepth(i, screenUV));
						#endif
						fogDepth = saturate(pow(fogDepth, _FogPower));
						col.rgb = lerp(col.rgb, lerp(_FogTint.rgb, col.rgb, fogDepth), _FogTint.a);
					#endif
				}

				#if EDGEFADE_ENABLED
					col = lerp(baseCol, col, edgeFadeDepth);
				#endif
				
				UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

		Pass {
			Tags {"LightMode"="ForwardAdd"}
			Blend SrcAlpha One
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd fullshadows
			#pragma multi_compile_fog
			#pragma shader_feature_local _NORMALMAP_1_ON
			#pragma shader_feature_local _SPECULAR_ON
			#pragma shader_feature_local _FLOW_ON
			#pragma shader_feature_local _VERTEX_OFFSET_ON
			#pragma shader_feature_local _DEPTHFOG_ON
			#pragma shader_feature_local _FOAM_ON
			#pragma shader_feature_local _CAUSTICS_ON
			#pragma shader_feature_local _EDGEFADE_ON

            #pragma target 5.0

			#include "WaterDefines.cginc"

            v2f vert (appdata v) {
                v2f o = (v2f)0;
				#if VERTEX_OFFSET_ENABLED
					float2 noiseUV = ScaleUV(v.uv, _NoiseTexScale, _NoiseTexScroll*10);
					float noiseWaveTex = tex2Dlod(_NoiseTex, float4(noiseUV,0,lerp(0,8,_NoiseTexBlur)));
					float noiseWave = Remap(noiseWaveTex, 0, 1, -1, 1);
					float offsetWave = noiseWave * _WaveHeight;
					v.vertex.x += offsetWave * _Offset.x;
					v.vertex.y += offsetWave * _Offset.y;
					v.vertex.z += offsetWave * _Offset.z;
				#endif
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
				o.cNormal = UnityObjectToWorldNormal(v.normal);
                o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0)).xyz);
                o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.uvGrab = ComputeGrabScreenPos(o.pos);
				o.localPos = v.vertex;
				o.uv = v.uv;
				UNITY_TRANSFER_FOG(o, o.pos);
				#if SPECULAR_ENABLED
					UNITY_TRANSFER_SHADOW(o, v.uv1);
				#endif
                return o;
            }

            float4 frag(v2f i, bool isFrontFace: SV_IsFrontFace) : SV_Target {
				if (isFrontFace){
					UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
					atten = FadeShadows(i.worldPos, atten);
					float3 normalMap;
					float3 detailNormal;
					float2 uvNormal0 = ScaleUV(i.uv, _NormalMapScale0, _NormalMapScroll0);
					float2 uvNormal1 = ScaleUV(i.uv, _NormalMapScale1, _NormalMapScroll1);
					float2 baseUV0 = Rotate2D(uvNormal0, _Rotation0);
					float2 baseUV1 = Rotate2D(uvNormal1, _Rotation1);
					float3 uv00 = float3(baseUV0, 1);
					float3 uv10 = float3(baseUV1, 1);
					#if FOAM_ENABLED
						float2 uvFoam = ScaleUV(i.uv, _FoamTexScale, 0);
						float3 uvF0 = float3(uvFoam, 1);
						float3 uvF1 = uvF0;
					#endif

					#if FLOW_ENABLED
						float2 uvFlow = ScaleUV(i.uv, _FlowMapScale, 0);
						float4 flowMap = tex2D(_FlowMap, uvFlow);
						float2 flow = (flowMap.rg * 2 - 1) * _FlowStrength * 0.1;
						float time = _Time.y * _FlowSpeed + flowMap.a;
						uv00 = FlowUV(baseUV0, flow, time, 0);
						float3 uv01 = FlowUV(baseUV0, flow, time, 0.5);
						uv10 = FlowUV(baseUV1, flow, time, 0);
						float3 uv11 = FlowUV(baseUV1, flow, time, 0.5);
						#if FOAM_ENABLED
							uvF0 = FlowUV(uvFoam, flow, time, 0);
							uvF0.x += _Time.y * 0.01;
							uvF1 = FlowUV(uvFoam, flow, time, 0.5);
							uvF1.x -= _Time.y*0.01;
						#endif
					#endif

					#if NORMALMAP1_ENABLED
						float3 normalMap0 = UnpackScaleNormal(tex2D(_NormalMap0, uv00.xy), _NormalStr0) * uv00.z;
						float3 detailNormal0 = UnpackScaleNormal(tex2D(_NormalMap1, uv10.xy), _NormalStr1) * uv10.z;
						#if FLOW_ENABLED
							float3 normalMap1 = UnpackScaleNormal(tex2D(_NormalMap0, uv01.xy), _NormalStr0) * uv01.z;
							float3 detailNormal1 = UnpackScaleNormal(tex2D(_NormalMap1, uv11.xy), _NormalStr1) * uv11.z;
							normalMap0 = normalize(normalMap0 + normalMap1);
							detailNormal0 = normalize(detailNormal0 + detailNormal1);
						#endif
						normalMap = BlendNormals(normalMap0, detailNormal0);
					#else
						float3 normalMap0 = UnpackScaleNormal(tex2D(_NormalMap0, uv00.xy), _NormalStr0) * uv00.z;
						#if FLOW_ENABLED
							float3 normalMap1 = UnpackScaleNormal(tex2D(_NormalMap0, uv01.xy), _NormalStr0) * uv01.z;
							normalMap = normalize(normalMap0 + normalMap1);
						#else
							normalMap = normalize(normalMap0);
						#endif
					#endif

					float2 uvOffset = normalMap.xy * _DistortionStrength;
					#if UNITY_SINGLE_PASS_STEREO
						uvOffset.x *= 0.5;
						uvOffset.xy *= 0.5;
					#endif
					float proj = i.uvGrab.w + 0.00001;
					float2 screenUV = 0;
					if (!IsInMirror()){
						screenUV = AlignWithGrabTexel((i.uvGrab.xy + uvOffset) / proj);
						if (GetDepth(i, screenUV) < 0){
							screenUV = AlignWithGrabTexel(i.uvGrab.xy / proj);
						}
					}
					else screenUV = (i.uvGrab.xy + uvOffset) / proj;

					float2 baseUV = i.uvGrab.xy/proj;
					float2 mainTexUV = TRANSFORM_TEX(i.uv, _MainTex) + _Time.y * 0.1 * _MainTexScroll;
					float4 mainTex = tex2D(_MainTex, mainTexUV);
					float4 baseCol = tex2D(_MWGrab, baseUV) * mainTex;
					float4 col = lerp(tex2D(_MWGrab, screenUV)*_Color*mainTex, _Color*mainTex, _Opacity);

					#if FOAM_ENABLED
						float depth = saturate(1-GetDepth(i, screenUV));
						float foamDepth = saturate(pow(depth,_FoamPower));
						#if FLOW_ENABLED
							float4 foamTex0 = tex2D(_FoamTex, uvF0.xy) * uvF0.z;
							float4 foamTex1 = tex2D(_FoamTex, uvF1.xy) * uvF1.z;
							float4 foamTex = (foamTex0 + foamTex1) * _FoamColor;
						#else
							float4 foamTex = tex2D(_FoamTex, uvFoam) * _FoamColor;
						#endif
						float foam = saturate(foamTex.a * foamDepth * _FoamOpacity * Average(foamTex.rgb));

						col.rgb = lerp(col.rgb, foamTex.rgb, foam);
					#endif

					#if PBR_ENABLED
						i.normal = normalize(dot(i.normal, i.normal) >= 1.01 ? i.cNormal : i.normal);
						float3 normalDir = normalize(normalMap.x * i.tangent + normalMap.y * i.binormal + normalMap.z * i.normal);
						float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
						float NdotV = abs(dot(normalDir, viewDir));
						#if FOAM_ENABLED
							_Roughness = lerp(_Roughness, _FoamRoughness, foam);
						#endif
						float roughSq = _Roughness * _Roughness;
						float roughBRDF = max(roughSq, 0.003);
							
						float omr = unity_ColorSpaceDielectricSpec.a - _Metallic * unity_ColorSpaceDielectricSpec.a;
						float3 specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, 1, _Metallic);

						#if SPECULAR_ENABLED
							float roughInterp = smoothstep(0.001, 0.003, roughSq);
							float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
							float3 halfVector = normalize(lightDir + viewDir);
							float NdotL = dot(normalDir, lightDir);
							float NdotH = Safe_DotClamped(normalDir, halfVector);
							float LdotH = Safe_DotClamped(lightDir, halfVector);
							float3 fresnelTerm = FresnelTerm(specularTint, LdotH);
							float specularTerm = SpecularTerm(NdotL, NdotV, NdotH, roughBRDF);
							float3 specCol = _LightColor0 * fresnelTerm * specularTerm * atten;
							specCol = lerp(smootherstep(0, 0.9, specCol), specCol, roughInterp);
							col.rgb += specCol;
						#endif
					#endif

					#if EDGEFADE_ENABLED
						float edgeFadeDepth = saturate(1-GetDepth(i, baseUV));
						edgeFadeDepth = (1-saturate(pow(edgeFadeDepth, _EdgeFadePower)));
						edgeFadeDepth = saturate(Remap(edgeFadeDepth, 0, 1, -_EdgeFadeOffset, 1));
					#endif

					#if EDGEFADE_ENABLED
						col = lerp(baseCol, col, edgeFadeDepth);
					#endif
					
					col.rgb *= _LightColor0 * atten;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				else discard;
				return 0;
            }
            ENDCG
        }
    }
	CustomEditor "WaterEditor"
}