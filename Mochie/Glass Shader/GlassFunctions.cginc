
#include "../Common/Utilities.cginc"

float4 SampleTexture(sampler2D tex, float2 uv){
	#if defined(_STOCHASTIC_SAMPLING_ON)
		return tex2Dstoch(tex, uv);
	#else
		return tex2D(tex, uv);
	#endif
	return 0;
}

float FadeShadows (float3 worldPos, float atten) {
	#if HANDLE_SHADOWS_BLENDING_IN_GI
		float viewZ = dot(_WorldSpaceCameraPos - worldPos, UNITY_MATRIX_V[2].xyz);
		float shadowFadeDistance = UnityComputeShadowFadeDistance(worldPos, viewZ);
		float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
		atten = saturate(atten + shadowFade);
	#endif
	return atten;
}

float3 ShadeSH9(float3 normal){
	return max(0, ShadeSH9(float4(normal,1)));
}

float3 tex2DnormalGlass(sampler2D tex, float2 uv, float4 uvdd, float strength){
    float offset = 0.15;
    offset = pow(offset, 3) * 0.1;
    float2 offsetU = float2(uv.x + offset, uv.y);
    float2 offsetV = float2(uv.x, uv.y + offset);

    float normalSample = smoothstep(0.15, 1, tex2Dgrad(_RainSheet, uv, uvdd.xy, uvdd.zw));
    float uSample = smoothstep(0.15, 1, tex2Dgrad(_RainSheet, offsetU, uvdd.xy, uvdd.zw));
    float vSample = smoothstep(0.15, 1, tex2Dgrad(_RainSheet, offsetV, uvdd.xy, uvdd.zw));

    float3 va = float3(1, 0, (uSample - normalSample) * strength);
    float3 vb = float3(0, 1, (vSample - normalSample) * strength);
    return normalize(cross(va, vb));
}

float2 GetFlipbookUV(float2 uv, float width, float height, float speed, float2 invertAxis){
    float tile = fmod(trunc(_Time.y * speed), width*height);
    float2 tileCount = float2(1.0, 1.0) / float2(width, height);
    float tileY = abs(invertAxis.y * height - (floor(tile * tileCount.x) + invertAxis.y * 1));
    float tileX = abs(invertAxis.x * width - ((tile - width * floor(tile * tileCount.x)) + invertAxis.x * 1));
    return (uv + float2(tileX, tileY)) * tileCount;
}

float3 GetFlipbookNormals(v2f i, inout float flipbookBase, float mask){
    float2 uv = frac(ScaleOffsetUV(i.uv, float2(_XScale, _YScale), 0));
    float2 flipUV = GetFlipbookUV(uv, _Columns, _Rows, _Speed, float2(0,1));

    float2 origUV = i.uv / float2(_Columns, _Rows) * float2(_XScale, _YScale);
    float4 uvdd = float4(ddx(origUV), ddy(origUV));

    flipbookBase = tex2Dgrad(_RainSheet, flipUV, uvdd.xw, uvdd.zw) * mask;
    return tex2DnormalGlass(_RainSheet, flipUV, uvdd, _Strength*mask);
}

// based on https://www.toadstorm.com/blog/?p=742
void ApplyExtraDroplets(v2f i, inout float3 rainNormal, inout float flipbookBase, float mask){
	if (_DynamicDroplets > 0){
		float2 dropletMaskUV = ScaleOffsetUV(i.uv, float2(_XScale, _YScale), 0);
		float4 dropletMask = tex2Dbias(_DropletMask, float4(dropletMaskUV,0,_RainBias));
		float3 dropletMaskNormal = UnpackScaleNormal(float4(dropletMask.rg,1,1), _Strength*2*mask);
		float droplets = Remap(dropletMask.b, 0, 1, -1, 1);
		droplets += (_Time.y*(_Speed/200.0));
		droplets = frac(droplets);
		droplets = dropletMask.a - droplets;
		droplets = Remap(droplets, 1-_DynamicDroplets, 1, 0, 1);
		float dropletRough = smoothstep(0, 0.1, droplets);
		droplets = smoothstep(0, 0.4, droplets);
		flipbookBase = smoothstep(0, 0.1, flipbookBase);
		flipbookBase = saturate(flipbookBase + dropletRough);
		rainNormal = lerp(rainNormal, dropletMaskNormal, droplets);
	}
}

#include "GlassKernels.cginc"

float3 BlurredGrabpassSample(float2 uv, float str){
	float3 blurCol = 0;
	float2 blurStr = str;
	blurStr.x *= 0.5625;
	float2 uvBlur = uv;
	
	#if defined(BLURQUALITY_ULTRA)
		[unroll(71)]
		for (uint index = 0; index < 71; ++index){
			uvBlur.xy = uv.xy + (kernel71[index] * blurStr);
			blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_GlassGrab, uvBlur);
		}
		blurCol /= 70;
	#elif defined(BLURQUALITY_HIGH)
		[unroll(43)]
		for (uint index = 0; index < 43; ++index){
			uvBlur.xy = uv.xy + (kernel43[index] * blurStr);
			blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_GlassGrab, uvBlur);
		}
		blurCol /= 42;
	#elif defined(BLURQUALITY_MED)
		[unroll(22)]
		for (uint index = 0; index < 22; ++index){
			uvBlur.xy = uv.xy + (kernel22[index] * blurStr);
			blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_GlassGrab, uvBlur);
		}
		blurCol /= 21;
	#elif defined(BLURQUALITY_LOW)
		[unroll(16)]
		for (uint index = 0; index < 16; ++index){
			uvBlur.xy = uv.xy + (kernel16[index] * blurStr);
			blurCol += MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_GlassGrab, uvBlur);
		}
		blurCol /= 15;
	#endif

	return blurCol;
}

float3 BoxProjection(float3 dir, float3 pos, float4 cubePos, float3 boxMin, float3 boxMax){
	#if UNITY_SPECCUBE_BOX_PROJECTION
		UNITY_BRANCH
		if (cubePos.w > 0){
			float3 factors = ((dir > 0 ? boxMax : boxMin) - pos) / dir;
			float scalar = min(min(factors.x, factors.y), factors.z);
			dir = dir * scalar + (pos - cubePos);
		}
	#endif
	return dir;
}

float3 GetWorldReflections(float3 reflDir, float3 worldPos, float roughness){
	float3 baseReflDir = reflDir;
	roughness *= 1.7-0.7*roughness;
	reflDir = BoxProjection(reflDir, worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
	float4 envSample0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
	float3 p0 = DecodeHDR(envSample0, unity_SpecCube0_HDR);
	float interpolator = unity_SpecCube0_BoxMin.w;
	UNITY_BRANCH
	if (interpolator < 0.99999){
		float3 refDirBlend = BoxProjection(baseReflDir, worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
		float4 envSample1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, refDirBlend, roughness * UNITY_SPECCUBE_LOD_STEPS);
		float3 p1 = DecodeHDR(envSample1, unity_SpecCube1_HDR);
		p0 = lerp(p1, p0, interpolator);
	}
	return p0;
}

float SpecularTerm(float NdotL, float NdotV, float NdotH, float roughness){
	float visibilityTerm = 0;
	float rough = roughness;
	float rough2 = roughness * roughness;

	float lambdaV = NdotL * (NdotV * (1 - rough) + rough);
	float lambdaL = NdotV * (NdotL * (1 - rough) + rough);

	visibilityTerm = 0.5f / (lambdaV + lambdaL + 1e-5f);
	float d = (NdotH * rough2 - NdotH) * NdotH + 1.0f;
	float dotTerm = UNITY_INV_PI * rough2 / (d * d + 1e-7f);

	return max(0, visibilityTerm * dotTerm * UNITY_PI * NdotL);
}