#ifndef SFX_PASS_INCLUDED
#define SFX_PASS_INCLUDED

#if MAIN_PASS
v2f vert (appdata v){
    v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	#if X_FEATURES
    	o.pulseSpeed = GetPulse();
	#else
		o.pulseSpeed = 1;
	#endif
    o.cameraPos = GetCameraPos();
    o.objPos = GetObjPos();
    o.objDist = distance(o.cameraPos, o.objPos);

    float gf = smoothstep(_MaxRange, clamp(_MinRange, 0, _MaxRange-0.001),  o.objDist);
    o.globalF = gf;
    o.colorF = GetFalloff(_ColorUseGlobal, gf, _ColorMinRange, _ColorMaxRange, o.objDist);
    o.shakeF = GetFalloff(_ShakeUseGlobal, gf, _ShakeMinRange, _ShakeMaxRange, o.objDist);
    o.distortionF = GetFalloff(_DistortionUseGlobal, gf, _DistortionMinRange, _DistortionMaxRange, o.objDist);
    o.blurF = GetFalloff(_BlurUseGlobal, gf, _BlurMinRange, _BlurMaxRange, o.objDist);
	o.noiseF = GetFalloff(_NoiseUseGlobal, gf, _NoiseMinRange, _NoiseMaxRange, o.objDist);
	#if X_FEATURES
		o.sstF = GetFalloff(_SSTUseGlobal, gf, _SSTMinRange, _SSTMaxRange, o.objDist);
		o.olF = GetFalloff(_OLUseGlobal, gf, _OLMinRange, _OLMaxRange, o.objDist);
	#endif

    v.vertex.x *= 1.4;
    float4 wPos = mul(unity_CameraToWorld, v.vertex);
    float4 oPos = mul(unity_WorldToObject, wPos);
    o.raycast = UnityObjectToViewPos(oPos).xyz * float3(-1,-1,1);
    o.pos = GetScreenspaceVertexPos(v.vertex);

    o.uv = ComputeGrabScreenPos(o.pos);
    o.uvd = TRANSFORM_TEX(v.uv, _NormalMap) + _Time.y * _DistortionSpeed;
	#if SHAKE_ENABLED
    	ApplyNoiseShake(o);
	#endif
    return o;
}

float4 frag (v2f i) : SV_Target {
	
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    MirrorCheck();

	float2 uv = i.uv.xy / i.uv.w;
	i.uv.xy = uv;

	#if X_FEATURES
    	ApplyPulse(i.pulseSpeed);
		ApplyUVManip(i);
	#endif

	#if SHAKE_ENABLED
    	ApplyShake(i);
	#endif

	#if DISTORTION_ENABLED
		ApplyMapDistortion(i);
	#elif DISTORTION_WORLD_ENABLED
		ApplyWGDistortion(i); 
	#endif
    
	#if BLUR_ENABLED
		#if DOF_ENABLED
			float dof = GetDoF(i);
			_BlurStr = lerp(_BlurStr, 0, dof);
			_RippleGridStr = lerp(_RippleGridStr, 0, dof);
			_PixelationStr = lerp(_PixelationStr, 0, dof);
		#endif
		ApplyRipplePixelate(i);
		float4 col = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, i.uv.xy);
		ApplyDitherBlur(i);
		float4 blurCol = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, i.uv.xy);
		ApplyBlur(i, col.rgb, blurCol.rgb);
	#else
		float4 col = MOCHIE_SAMPLE_TEX2D_SCREENSPACE(_MSFXGrab, i.uv.xy);
	#endif

	#if X_FEATURES
		ApplyDepthBuffer(i, col.rgb);
		ApplyNormalMap(i, col.rgb);
		#if OUTLINE_ENABLED
			ApplyOutline(i, col.rgb);
		#endif
		ApplyRounding(i, col.rgb);
	#endif

	#if COLOR_ENABLED
    	ApplyColor(i, col.rgb);
	#endif

	#if NOISE_ENABLED
		ApplyNoise(i, col.rgb);
	#endif

    ApplyTransparency(i, col);
	return col;
}
#elif X_FEATURES
	#include "SFXXPasses.cginc"
#endif
#endif