float3 GetCameraPos(){
    float3 cameraPos = _WorldSpaceCameraPos;
    #if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
        cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5;
    #endif
    return cameraPos;
}

float3 ShadeSH9(float3 normal){
	return max(0, ShadeSH9(float4(normal,1)));
}

float4x4 inverse(float4x4 input){
	#define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))
	float4x4 cofactors = float4x4(
		minor(_22_23_24, _32_33_34, _42_43_44), 
		-minor(_21_23_24, _31_33_34, _41_43_44),
		minor(_21_22_24, _31_32_34, _41_42_44),
		-minor(_21_22_23, _31_32_33, _41_42_43),

		-minor(_12_13_14, _32_33_34, _42_43_44),
		minor(_11_13_14, _31_33_34, _41_43_44),
		-minor(_11_12_14, _31_32_34, _41_42_44),
		minor(_11_12_13, _31_32_33, _41_42_43),

		minor(_12_13_14, _22_23_24, _42_43_44),
		-minor(_11_13_14, _21_23_24, _41_43_44),
		minor(_11_12_14, _21_22_24, _41_42_44),
		-minor(_11_12_13, _21_22_23, _41_42_43),

		-minor(_12_13_14, _22_23_24, _32_33_34),
		minor(_11_13_14, _21_23_24, _31_33_34),
		-minor(_11_12_14, _21_22_24, _31_32_34),
		minor(_11_12_13, _21_22_23, _31_32_33)
	);
	#undef minor
	return transpose(cofactors) / determinant(input);
}

float3 GetWorldSpacePixelPos(float4 vertex, float2 scrnPos){
	float4 worldPos = mul(unity_ObjectToWorld, float4(vertex.xyz, 1));
	float4 screenPos = mul(UNITY_MATRIX_VP, worldPos); 
	worldPos = mul(inverse(UNITY_MATRIX_VP), screenPos);
	float3 worldDir = worldPos.xyz - _WorldSpaceCameraPos;
	float depth = LinearEyeDepth(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthTexture, scrnPos)) / screenPos.w;
	float3 worldSpacePos = worldDir * depth + _WorldSpaceCameraPos;
	return worldSpacePos;
}

float3 tex2Dnormal(sampler2D tex, float2 uv, float strength){
	float offset = 0.15;
	offset = pow(offset, 3) * 0.1;
	float2 offsetU = float2(uv.x + offset, uv.y);
	float2 offsetV = float2(uv.x, uv.y + offset);
	float normalSample = tex2D(tex, uv);
	float uSample = tex2D(tex, offsetU);
	float vSample = tex2D(tex, offsetV);
	float3 va = float3(1, 0, (uSample - normalSample) * strength);
	float3 vb = float3(0, 1, (vSample - normalSample) * strength);
	return normalize(cross(va, vb));
}

float2 ScaleOffset(float2 uv, float2 tiling, float2 offset){
	return uv * tiling + offset;
}

float2 GetFlipbookUV(float2 uv, float width, float height, float speed, float2 invertAxis){
	float tile = fmod(trunc(_Time.y * speed), width*height);
	float2 tileCount = float2(1.0, 1.0) / float2(width, height);
	float tileY = abs(invertAxis.y * height - (floor(tile * tileCount.x) + invertAxis.y * 1));
	float tileX = abs(invertAxis.x * width - ((tile - width * floor(tile * tileCount.x)) + invertAxis.x * 1));
	return (uv + float2(tileX, tileY)) * tileCount;
}

float3 GetFlipbookNormals(v2f i, inout float flipbookBase){
	float2 uv = frac(ScaleOffset(i.uv, float2(_XScale, _YScale), 0));
	float2 flipUV = GetFlipbookUV(uv, _Columns, _Rows, _Speed, float2(0,1));
	flipbookBase = tex2D(_RainSheet, flipUV);
	return tex2Dnormal(_RainSheet, flipUV, _Strength);
}

#include "GlassKernels.cginc"

float3 tex2Dblur(sampler2D tex, float2 uv, float str){
	float3 blurCol = 0;
	float2 blurStr = str;
	blurStr.x *= 0.5625;
	float4 uvBlur = float4(uv,0,0);
	
	#if defined(BLURQUALITY_ULTRA)
		[unroll(71)]
		for (uint index = 0; index < 71; ++index){
			uvBlur.xy = uv.xy + (kernel71[index] * blurStr);
			blurCol += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex, uvBlur);
		}
		blurCol /= 70;
	#elif defined(BLURQUALITY_HIGH)
		[unroll(43)]
		for (uint index = 0; index < 43; ++index){
			uvBlur.xy = uv.xy + (kernel43[index] * blurStr);
			blurCol += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex, uvBlur);
		}
		blurCol /= 42;
	#elif defined(BLURQUALITY_MED)
		[unroll(22)]
		for (uint index = 0; index < 22; ++index){
			uvBlur.xy = uv.xy + (kernel22[index] * blurStr);
			blurCol += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex, uvBlur);
		}
		blurCol /= 21;
	#elif defined(BLURQUALITY_LOW)
		[unroll(16)]
		for (uint index = 0; index < 16; ++index){
			uvBlur.xy = uv.xy + (kernel16[index] * blurStr);
			blurCol += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex, uvBlur);
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

float Safe_DotClamped(float3 a, float3 b){
	return max(0.00001, dot(a,b));
}