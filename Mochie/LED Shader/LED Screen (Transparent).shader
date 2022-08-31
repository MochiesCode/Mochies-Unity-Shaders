// Original by Xiexe - https://github.com/Xiexe/RGBSubPixelDisplay-Shader

Shader "Mochie/LED Screen (Transparent)" {
	Properties {
		[ToggleUI]_FlipbookMode("Flipbook Mode", Int) = 0
		_FPS("FPS", Float) = 24
		_MainTex("Emission", 2D) = "black" {}
		_Color("Color", Color) = (1,1,1,1)
		_Flipbook("Flippbook", 2DArray) = "black" {}
		_UVScroll("UV Scrolling", Vector) = (0,0,0,0)
		_RGBSubPixelTex ("RGBSubPixelTex", 2D) = "white" {}
		_SpecGlossMap("Roughness Map", 2D) = "white" {}
		_Glossiness ("Roughness", Range(0,1)) = 0.035
		_LightmapEmissionScale("Lightmap Emission Scale", Float) = 1
		_EmissionIntensity ("Screen Intensity", Float) = 1
		_BoostAmount("Boost Threshold", Float) = 1
		_BoostThreshold("Boost Treshold", Range(0,1)) = 0
		[ToggleUI]_IsAVProInput("Fix for AVPro", Int) = 0
		[ToggleUI]_Backlight("Backlit Panel", Int) = 0
		[Enum(Off,0, On,1)]_ZWrite("ZWrite", Int) = 1
		[HideInInspector]_texcoord2( "", 2D ) = "white" {}
	}
	SubShader {

		Tags { 
			"RenderType"="Transparent" 
			"Queue"="Transparent"
		}
		ZWrite [_ZWrite]

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows alpha:premul
		#pragma target 5.0
		#pragma shader_feature_local _SPECGLOSSMAP
		#pragma shader_feature _EMISSION
		#pragma shader_feature_local _FLIPBOOK_MODE
		#include "../Common/Utilities.cginc"
		
		#ifdef _FLIPBOOK_MODE
			UNITY_DECLARE_TEX2DARRAY(_Flipbook);
			float _FPS;
		#endif

		sampler2D _MainTex;
		sampler2D _RGBSubPixelTex;
		sampler2D _SpecGlossMap;
		float2 _UVScroll;
		float3 _Color;
		float _Glossiness;
		int _IsAVProInput;

		float _EmissionIntensity;
		float _LightmapEmissionScale;
		float _BoostThreshold;
		float _BoostAmount;
		float _Backlight;
		float4 _RGBSubPixelTex_ST;

		struct Input {
			float2 uv_MainTex;
			float2 uv_SpecGlossMap;
			float2 uv_texcoord2;
			float3 viewDir;
			float3 worldNormal;
		};

		#include "RGBSubPixel.cginc"

		void surf (Input IN, inout SurfaceOutputStandard o) {
			if (_IsAVProInput == 1){
				IN.uv_MainTex.y = 1-IN.uv_MainTex.y;
			}
			float3 worldNormal = WorldNormalVector(IN, IN.worldNormal);
			float3 viewDir = IN.viewDir;
			float2 uv = IN.uv_MainTex + (_Time.y * _UVScroll * 0.1);
			float alpha = 1;
			float4 finalCol = RGBSubPixelConvert(_MainTex, _RGBSubPixelTex, uv, IN.uv_texcoord2, viewDir, worldNormal, alpha);

			o.Albedo = float4(0,0,0,1);
			o.Alpha = alpha;
			o.Emission = finalCol * _Color;
			o.Metallic = 0;
			#ifdef _SPECGLOSSMAP
				o.Smoothness = 1-(tex2D(_SpecGlossMap, IN.uv_SpecGlossMap) * _Glossiness);
			#else
				o.Smoothness = 1-_Glossiness;
			#endif
		}
		ENDCG
	}
	Fallback "Standard"
	CustomEditor "LEDEditor"
}