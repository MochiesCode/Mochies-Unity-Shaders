#ifndef BLUR_INCLUDED
#define BLUR_INCLUDED

#include "SFXKernel.cginc"

void ApplyStandardBlur(float2 texelSize, float4 uv0, uint sampleCount, float str, inout float3 blurCol){
	float2 uv = uv0.xy;
	float2 uvb = uv;
	#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		float2 mainStr = float2(0.225, 0.375) * str * 0.0015;
		float2 str136 = float2(0.225, 0.375) * _BlurStr * 0.002;
	#else
		float2 mainStr = float2(0.5, 0.75) * str * 0.0015;
		float2 str136 = float2(0.5, 0.75) * _BlurStr * 0.002;
	#endif
	[forcecase]
	switch (sampleCount){
		case 16: 
			[fastopt]
			for (uint si16 = 0; si16 < sampleCount; ++si16){
				uvb.xy = uv.xy + (kDiskKernel16[si16] * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.xy = uv.xy + (kDiskKernel22[si22] * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.xy = uv.xy + (kDiskKernel43[si43] * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.xy = uv.xy + (kDiskKernel71[si71] * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.xy = uv.xy + (kDiskKernel136[si136] * str136);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
			}
			break;
		default: break;
	}
	blurCol /= sampleCount;
}

void StandardBlurWithDepth(float2 texelSize, float4 uv0, uint sampleCount, float str, inout float3 blurCol, inout float depth){
	float2 uv = uv0.xy;
	float2 uvb = uv;
	#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		float2 mainStr = float2(0.225, 0.375) * str * 0.0015;
		float2 str136 = float2(0.225, 0.375) * _BlurStr * 0.002;
	#else
		float2 mainStr = float2(0.5, 0.75) * str * 0.0015;
		float2 str136 = float2(0.5, 0.75) * _BlurStr * 0.002;
	#endif

	[forcecase]
	switch (sampleCount){
		case 16: 
			[fastopt]
			for (uint si16 = 0; si16 < sampleCount; ++si16){
				uvb.xy = uv.xy + (kDiskKernel16[si16] * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.xy = uv.xy + (kDiskKernel22[si22] * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.xy = uv.xy + (kDiskKernel43[si43] * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.xy = uv.xy + (kDiskKernel71[si71] * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.xy = uv.xy + (kDiskKernel136[si136] * str136);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		default: break;
	}
	depth /= sampleCount;
	blurCol /= sampleCount;
}

void ApplyStandardBlurY(float2 texelSize, float4 uv0, uint sampleCount, float str, inout float3 blurCol){
	float2 uv = uv0.xy;
	float2 uvb = uv;
	#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		float2 mainStr = float2(0.225, 0.375) * str * 0.00275;
		float2 str136 = float2(0.225, 0.375) * _BlurStr * 0.00325;
	#else
		float2 mainStr = float2(0.5, 0.75) * str * 0.00275;
		float2 str136 = float2(0.5, 0.75) * _BlurStr * 0.00325;
	#endif
	
	[forcecase]
	switch (sampleCount){
		case 16: 
			[fastopt]
			for (uint si16 = 0; si16 < sampleCount; ++si16){
				uvb.y = uv.y + (kDiskKernel16[si16].y * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.y = uv.y + (kDiskKernel22[si22].y * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.y = uv.y + (kDiskKernel43[si43].y * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.y = uv.y + (kDiskKernel71[si71].y * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.y = uv.y + (kDiskKernel136[si136].y * str136);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
			}
			break;
		default: break;
	}
	blurCol /= sampleCount;
}

void ApplyStandardBlurYWithDepth(float2 texelSize, float4 uv0, uint sampleCount, float str, inout float3 blurCol, inout float depth){
	float2 uv = uv0.xy;
	float2 uvb = uv;
	#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		float2 mainStr = float2(0.225, 0.375) * str * 0.00275;
		float2 str136 = float2(0.225, 0.375) * _BlurStr * 0.00325;
	#else
		float2 mainStr = float2(0.5, 0.75) * str * 0.00275;
		float2 str136 = float2(0.5, 0.75) * _BlurStr * 0.00325;
	#endif
	
	
	[forcecase]
	switch (sampleCount){
		case 16: 
			[fastopt]
			for (uint si16 = 0; si16 < sampleCount; ++si16){
				uvb.y = uv.y + (kDiskKernel16[si16].y * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.y = uv.y + (kDiskKernel22[si22].y * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.y = uv.y + (kDiskKernel43[si43].y * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.y = uv.y + (kDiskKernel71[si71].y * mainStr);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.y = uv.y + (kDiskKernel136[si136].y * str136);
				blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		default: break;
	}
	depth /= sampleCount;
	blurCol /= sampleCount;
}

void ApplyStandardBlurDepth(float4 uv0, uint sampleCount, float str, inout float depth){
	float2 uv = uv0.xy;
	float2 uvb = uv;
	str *= _CameraDepthTexture_TexelSize.xy;
	float2 str136 = str * 1.225;
	
	[forcecase]
	switch (sampleCount){
		case 16: 
			[fastopt]
			for (uint si16 = 0; si16 < sampleCount; ++si16){
				uvb.xy = uv.xy + (kDiskKernel16[si16] * str);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.xy = uv.xy + (kDiskKernel22[si22] * str);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.xy = uv.xy + (kDiskKernel43[si43] * str);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.xy = uv.xy + (kDiskKernel71[si71] * str);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.xy = uv.xy + (kDiskKernel136[si136] * str136);
				depth += DecodeFloatRG(SampleDepthTex(uvb.xy));
			}
			break;
		default: break;
	}
	depth /= sampleCount;
}

void ApplyChromaticAbberation(float2 texelSize, float4 uv0, uint sampleCount, float str, inout float3 blurCol){
	float2 uv = uv0.xy;
	float2 uvb = uv;
	#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		float2 mainStr = float2(0.225, 0.375) * str * 0.0015;
		float2 str136 = float2(0.225, 0.375) * _BlurStr * 0.002;
	#else
		float2 mainStr = float2(0.5, 0.75) * str * 0.0015;
		float2 str136 = float2(0.5, 0.75) * _BlurStr * 0.002;
	#endif
	

	[forcecase]
	switch (sampleCount){
		case 16: 
			[fastopt]
			for (uint si16 = 0; si16 < sampleCount; ++si16){
				uvb.xy = uv.xy + (kDiskKernel16[si16] * mainStr);
				UNITY_BRANCH
				if (si16 < 5)
					blurCol.r += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).r;
				else if (si16 >= 5 && si16 < 10)
					blurCol.g += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).g;
				else
					blurCol.b += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).b;
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.xy = uv.xy + (kDiskKernel22[si22] * mainStr);
				UNITY_BRANCH
				if (si22 < 7)
					blurCol.r += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).r;
				else if (si22 >= 7 && si22 < 14)
					blurCol.g += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).g;
				else
					blurCol.b += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).b;
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.xy = uv.xy + (kDiskKernel43[si43] * mainStr);
				UNITY_BRANCH
				if (si43 < 14)
					blurCol.r += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).r;
				else if (si43 >= 14 && si43 < 28)
					blurCol.g += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).g;
				else
					blurCol.b += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).b;
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.xy = uv.xy + (kDiskKernel71[si71] * mainStr);
				UNITY_BRANCH
				if (si71 < 23)
					blurCol.r += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).r;
				else if (si71 >= 23 && si71 < 46)
					blurCol.g += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).g;
				else
					blurCol.b += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).b;
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.xy = uv.xy + (kDiskKernel136[si136] * str136);
				UNITY_BRANCH
				if (si136 < 45)
					blurCol.r += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).r;
				else if (si136 >= 45 && si136 < 90)
					blurCol.g += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).g;
				else 
					blurCol.b += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).b;
			}
			break;
		default: break;
	}
	blurCol /= sampleCount/3.0;
}

void ApplyChromaticAbberationY(float2 texelSize, float4 uv0, uint sampleCount, float str, inout float3 blurCol){
	float2 uv = uv0.xy;
	float2 uvb = uv;
	#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		float2 mainStr = float2(0.225, 0.375) * str * 0.00275;
		float2 str136 = float2(0.225, 0.375) * _BlurStr * 0.00325;
	#else
		float2 mainStr = float2(0.5, 0.75) * str * 0.00275;
		float2 str136 = float2(0.5, 0.75) * _BlurStr * 0.00325;
	#endif
	
	[forcecase]
	switch (sampleCount){
		case 16: 
			[fastopt]
			for (uint si16 = 0; si16 < sampleCount; ++si16){
				uvb.y = uv.y + (kDiskKernel16[si16].y * mainStr);
				UNITY_BRANCH
				if (si16 < 5)
					blurCol.r += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).r;
				else if (si16 >= 5 && si16 < 10)
					blurCol.g += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).g;
				else
					blurCol.b += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).b;
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.y = uv.y + (kDiskKernel22[si22].y * mainStr);
				UNITY_BRANCH
				if (si22 < 7)
					blurCol.r += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).r;
				else if (si22 >= 7 && si22 < 14)
					blurCol.g += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).g;
				else
					blurCol.b += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).b;
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.y = uv.y + (kDiskKernel43[si43].y * mainStr);
				UNITY_BRANCH
				if (si43 < 14)
					blurCol.r += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).r;
				else if (si43 >= 14 && si43 < 28)
					blurCol.g += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).g;
				else
					blurCol.b += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).b;
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.y = uv.y + (kDiskKernel71[si71].y * mainStr);
				UNITY_BRANCH
				if (si71 < 23)
					blurCol.r += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).r;
				else if (si71 >= 23 && si71 < 46)
					blurCol.g += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).g;
				else
					blurCol.b += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).b;
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.y = uv.y + (kDiskKernel136[si136].y * str136);
				UNITY_BRANCH
				if (si136 < 45)
					blurCol.r += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).r;
				else if (si136 >= 45 && si136 < 90)
					blurCol.g += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).g;
				else 
					blurCol.b += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).b;
			}
			break;
		default: break;
	}
	blurCol /= sampleCount/3.0;
}

void ApplyRadialBlur(v2f i, float4 uv0, uint sampleCount, float radius, float str, inout float3 blurCol){
    float3 col = 0;
    float2 uv = i.uv.xy;
    float2 offset = 0.5;
    #if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
        if (unity_StereoEyeIndex == 0)
            offset.x = 0.25;
        if (unity_StereoEyeIndex == 1)
            offset.x = 0.75; 
    #endif
    uv -= offset;
    str *= 0.4*i.blurF;

    for (uint j = 0; j < sampleCount; ++j) {
        float scale = 1-str*(j/(float)sampleCount)*(length(uv)/radius);
        float2 uvb = (uv*scale)+offset;
        blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, uvb).rgb;
    }
	blurCol /= sampleCount+1;
}

#endif