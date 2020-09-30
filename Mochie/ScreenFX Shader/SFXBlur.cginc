#ifndef BLUR_INCLUDED
#define BLUR_INCLUDED

#include "SFXKernel.cginc"

void ApplyStandardBlur(sampler2D tex, float2 texelSize, float4 uv0, uint sampleCount, float str, inout float3 blurCol){
	float2 uv = uv0.xy/uv0.w;
	float4 uvb = float4(uv,0,0);
	#if UNITY_SINGLE_PASS_STEREO
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
				blurCol += tex2Dlod(tex, uvb);
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.xy = uv.xy + (kDiskKernel22[si22] * mainStr);
				blurCol += tex2Dlod(tex, uvb);
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.xy = uv.xy + (kDiskKernel43[si43] * mainStr);
				blurCol += tex2Dlod(tex, uvb);
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.xy = uv.xy + (kDiskKernel71[si71] * mainStr);
				blurCol += tex2Dlod(tex, uvb);
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.xy = uv.xy + (kDiskKernel136[si136] * str136);
				blurCol += tex2Dlod(tex, uvb);
			}
			break;
		default: break;
	}
	blurCol /= sampleCount;
}

void StandardBlurWithDepth(sampler2D tex, float2 texelSize, float4 uv0, uint sampleCount, float str, inout float3 blurCol, inout float depth){
	float2 uv = uv0.xy/uv0.w;
	float4 uvb = float4(uv,0,0);
	#if UNITY_SINGLE_PASS_STEREO
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
				blurCol += tex2Dlod(tex, uvb);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.xy = uv.xy + (kDiskKernel22[si22] * mainStr);
				blurCol += tex2Dlod(tex, uvb);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.xy = uv.xy + (kDiskKernel43[si43] * mainStr);
				blurCol += tex2Dlod(tex, uvb);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.xy = uv.xy + (kDiskKernel71[si71] * mainStr);
				blurCol += tex2Dlod(tex, uvb);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.xy = uv.xy + (kDiskKernel136[si136] * str136);
				blurCol += tex2Dlod(tex, uvb);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		default: break;
	}
	depth /= sampleCount;
	blurCol /= sampleCount;
}

void ApplyStandardBlurY(sampler2D tex, float2 texelSize, float4 uv0, uint sampleCount, float str, inout float3 blurCol){
	float2 uv = uv0.xy/uv0.w;
	float4 uvb = float4(uv,0,0);
	#if UNITY_SINGLE_PASS_STEREO
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
				blurCol += tex2Dlod(tex, uvb);
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.y = uv.y + (kDiskKernel22[si22].y * mainStr);
				blurCol += tex2Dlod(tex, uvb);
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.y = uv.y + (kDiskKernel43[si43].y * mainStr);
				blurCol += tex2Dlod(tex, uvb);
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.y = uv.y + (kDiskKernel71[si71].y * mainStr);
				blurCol += tex2Dlod(tex, uvb);
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.y = uv.y + (kDiskKernel136[si136].y * str136);
				blurCol += tex2Dlod(tex, uvb);
			}
			break;
		default: break;
	}
	blurCol /= sampleCount;
}

void ApplyStandardBlurYWithDepth(sampler2D tex, float2 texelSize, float4 uv0, uint sampleCount, float str, inout float3 blurCol, inout float depth){
	float2 uv = uv0.xy/uv0.w;
	float4 uvb = float4(uv,0,0);
	#if UNITY_SINGLE_PASS_STEREO
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
				blurCol += tex2Dlod(tex, uvb);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.y = uv.y + (kDiskKernel22[si22].y * mainStr);
				blurCol += tex2Dlod(tex, uvb);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.y = uv.y + (kDiskKernel43[si43].y * mainStr);
				blurCol += tex2Dlod(tex, uvb);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.y = uv.y + (kDiskKernel71[si71].y * mainStr);
				blurCol += tex2Dlod(tex, uvb);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.y = uv.y + (kDiskKernel136[si136].y * str136);
				blurCol += tex2Dlod(tex, uvb);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		default: break;
	}
	depth /= sampleCount;
	blurCol /= sampleCount;
}

void ApplyStandardBlurDepth(float4 uv0, uint sampleCount, float str, inout float depth){
	float2 uv = uv0.xy/uv0.w;
	float4 uvb = float4(uv,0,0);
	str *= _CameraDepthTexture_TexelSize.xy;
	float2 str136 = str * 1.225;
	
	[forcecase]
	switch (sampleCount){
		case 16: 
			[fastopt]
			for (uint si16 = 0; si16 < sampleCount; ++si16){
				uvb.xy = uv.xy + (kDiskKernel16[si16] * str);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.xy = uv.xy + (kDiskKernel22[si22] * str);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.xy = uv.xy + (kDiskKernel43[si43] * str);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.xy = uv.xy + (kDiskKernel71[si71] * str);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.xy = uv.xy + (kDiskKernel136[si136] * str136);
				depth += DecodeFloatRG(tex2Dlod(_CameraDepthTexture, uvb));
			}
			break;
		default: break;
	}
	depth /= sampleCount;
}

void ApplyChromaticAbberation(sampler2D tex, float2 texelSize, float4 uv0, uint sampleCount, float str, inout float3 blurCol){
	float2 uv = uv0.xy/uv0.w;
	float4 uvb = float4(uv,0,0);
	#if UNITY_SINGLE_PASS_STEREO
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
					blurCol.r += tex2Dlod(tex, uvb).r;
				else if (si16 >= 5 && si16 < 10)
					blurCol.g += tex2Dlod(tex, uvb).g;
				else
					blurCol.b += tex2Dlod(tex, uvb).b;
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.xy = uv.xy + (kDiskKernel22[si22] * mainStr);
				UNITY_BRANCH
				if (si22 < 7)
					blurCol.r += tex2Dlod(tex, uvb).r;
				else if (si22 >= 7 && si22 < 14)
					blurCol.g += tex2Dlod(tex, uvb).g;
				else
					blurCol.b += tex2Dlod(tex, uvb).b;
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.xy = uv.xy + (kDiskKernel43[si43] * mainStr);
				UNITY_BRANCH
				if (si43 < 14)
					blurCol.r += tex2Dlod(tex, uvb).r;
				else if (si43 >= 14 && si43 < 28)
					blurCol.g += tex2Dlod(tex, uvb).g;
				else
					blurCol.b += tex2Dlod(tex, uvb).b;
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.xy = uv.xy + (kDiskKernel71[si71] * mainStr);
				UNITY_BRANCH
				if (si71 < 23)
					blurCol.r += tex2Dlod(tex, uvb).r;
				else if (si71 >= 23 && si71 < 46)
					blurCol.g += tex2Dlod(tex, uvb).g;
				else
					blurCol.b += tex2Dlod(tex, uvb).b;
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.xy = uv.xy + (kDiskKernel136[si136] * str136);
				UNITY_BRANCH
				if (si136 < 45)
					blurCol.r += tex2Dlod(tex, uvb).r;
				else if (si136 >= 45 && si136 < 90)
					blurCol.g += tex2Dlod(tex, uvb).g;
				else 
					blurCol.b += tex2Dlod(tex, uvb).b;
			}
			break;
		default: break;
	}
	blurCol /= sampleCount/3.0;
}

void ApplyChromaticAbberationY(sampler2D tex, float2 texelSize, float4 uv0, uint sampleCount, float str, inout float3 blurCol){
	float2 uv = uv0.xy/uv0.w;
	float4 uvb = float4(uv,0,0);
	#if UNITY_SINGLE_PASS_STEREO
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
					blurCol.r += tex2Dlod(tex, uvb).r;
				else if (si16 >= 5 && si16 < 10)
					blurCol.g += tex2Dlod(tex, uvb).g;
				else
					blurCol.b += tex2Dlod(tex, uvb).b;
			}
			break;
		case 22: 
			[fastopt]
			for (uint si22 = 0; si22 < sampleCount; ++si22){
				uvb.y = uv.y + (kDiskKernel22[si22].y * mainStr);
				UNITY_BRANCH
				if (si22 < 7)
					blurCol.r += tex2Dlod(tex, uvb).r;
				else if (si22 >= 7 && si22 < 14)
					blurCol.g += tex2Dlod(tex, uvb).g;
				else
					blurCol.b += tex2Dlod(tex, uvb).b;
			}
			break;
		case 43:
			[fastopt]
			for (uint si43 = 0; si43 < sampleCount; ++si43){
				uvb.y = uv.y + (kDiskKernel43[si43].y * mainStr);
				UNITY_BRANCH
				if (si43 < 14)
					blurCol.r += tex2Dlod(tex, uvb).r;
				else if (si43 >= 14 && si43 < 28)
					blurCol.g += tex2Dlod(tex, uvb).g;
				else
					blurCol.b += tex2Dlod(tex, uvb).b;
			}
			break;
		case 71:
			[fastopt]
			for (uint si71 = 0; si71 < sampleCount; ++si71){
				uvb.y = uv.y + (kDiskKernel71[si71].y * mainStr);
				UNITY_BRANCH
				if (si71 < 23)
					blurCol.r += tex2Dlod(tex, uvb).r;
				else if (si71 >= 23 && si71 < 46)
					blurCol.g += tex2Dlod(tex, uvb).g;
				else
					blurCol.b += tex2Dlod(tex, uvb).b;
			}
			break;
		case 136:
			[fastopt]
			for (uint si136 = 0; si136 < sampleCount; ++si136){
				uvb.y = uv.y + (kDiskKernel136[si136].y * str136);
				UNITY_BRANCH
				if (si136 < 45)
					blurCol.r += tex2Dlod(tex, uvb).r;
				else if (si136 >= 45 && si136 < 90)
					blurCol.g += tex2Dlod(tex, uvb).g;
				else 
					blurCol.b += tex2Dlod(tex, uvb).b;
			}
			break;
		default: break;
	}
	blurCol /= sampleCount/3.0;
}

void ApplyRadialBlur(v2f i, sampler2D tex, float4 uv0, uint sampleCount, float radius, float str, inout float3 blurCol){
    float3 col = 0;
    float2 uv = i.uv.xy/i.uv.w;
    float2 offset = 0.5;
    #if UNITY_SINGLE_PASS_STEREO
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
        blurCol += tex2Dlod(tex, float4(uvb,0,0)).rgb;
    }
	blurCol /= sampleCount+1;
}

#endif