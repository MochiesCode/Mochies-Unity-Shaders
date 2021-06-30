// Original by Xiexe - https://github.com/Xiexe/RGBSubPixelDisplay-Shader

Shader "Mochie/LED Screen" {
	Properties {
		_MainTex("Emission (RGB)", 2D) = "white" {}
		_UVScroll("UV Scrolling", Vector) = (0,0,0,0)
		_RGBSubPixelTex ("RGBSubPixelTex", 2D) = "white" {}
		_SpecGlossMap("Roughness Map", 2D) = "white" {}
		_Glossiness ("Roughness", Range(0,1)) = 0.035
		_LightmapEmissionScale("Lightmap Emission Scale", Float) = 1
		_EmissionIntensity ("Screen Intensity", Float) = 1
		[ToggleUI]_ApplyGamma("Apply Gamma", Float) = 0
		[ToggleUI]_Backlight("Backlit Panel", Int) = 0
		[HideInInspector]_texcoord2( "", 2D ) = "white" {}
	}
	SubShader {

		Tags { 
			"RenderType"="Opaque" 
			"Queue"="Geometry"
		}

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 5.0
		#pragma shader_feature _SPECGLOSSMAP
		#pragma shader_feature _EMISSION

		sampler2D _MainTex;
		sampler2D _RGBSubPixelTex;
		sampler2D _SpecGlossMap;
		float2 _UVScroll;
		float _Glossiness;
		int _ApplyGamma;
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_SpecGlossMap;
			float2 uv_texcoord2;
			float3 viewDir;
			float3 worldNormal;
		};

		#include "RGBSubPixel.cginc"
			
		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 worldNormal = WorldNormalVector(IN, IN.worldNormal);
			float3 viewDir = IN.viewDir;
			float2 uv = IN.uv_MainTex + (_Time.y * _UVScroll * 0.1);
			float4 finalCol = RGBSubPixelConvert(_MainTex, _RGBSubPixelTex, uv, IN.uv_texcoord2, viewDir, worldNormal);

			o.Albedo = float4(0,0,0,1);
			o.Alpha = 1;
			o.Emission = finalCol;
			o.Metallic = 0;
			#ifdef _SPECGLOSSMAP
				o.Smoothness = 1-(tex2D(_SpecGlossMap, IN.uv_SpecGlossMap) * _Glossiness);
			#else
				o.Smoothness = 1-_Glossiness;
			#endif
		}
		ENDCG
	}
	CustomEditor "LEDEditor"
}