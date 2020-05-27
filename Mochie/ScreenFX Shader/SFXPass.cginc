#ifndef SFX_PASS_INCLUDED
#define SFX_PASS_INCLUDED

#if defined(FULL_PASS)
v2f vert (appdata v){
    v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);

	#if defined(SFXX)
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
	#if defined(SFXX)
		o.fogF = GetFalloff(_FogUseGlobal, gf, _FogMinRange, _FogMaxRange, o.objDist);
		o.sstF = GetFalloff(_SSTUseGlobal, gf, _SSTMinRange, _SSTMaxRange, o.objDist);
		o.olF = GetFalloff(_OLUseGlobal, gf, _OLMinRange, _OLMaxRange, o.objDist);
	#endif

    v.vertex.x *= 1.4;
    float4 wPos = mul(unity_CameraToWorld, v.vertex);
    float4 oPos = mul(unity_WorldToObject, wPos);
    o.raycast = UnityObjectToViewPos(oPos).xyz * float3(-1,-1,1);
    o.raycast *= (_ProjectionParams.z / o.raycast.z);
    o.pos = GetScreenspaceVertexPos(v.vertex);

    o.uv = ComputeGrabScreenPos(o.pos);
    o.uvd = TRANSFORM_TEX(v.uv, _NormalMap) + _Time.y * _DistortionSpeed;
    o.uv.xy += DoNoiseShake(o.shakeF) * o.pulseSpeed;
    return o;
}

float4 frag (v2f i) : SV_Target {

    MirrorCheck();
	#if defined(SFXX)
		// DoDeepfry();
    	DoPulse(i.pulseSpeed);
		i.uv.xy = DoUVManip(i);
	#endif
    i.uv.xy = DoShake(i);
    i.uv.xy = DoDistortion(i);
    i.uv.xy = DoRipplePixelate(i);
    float4 col = tex2Dproj(_MSFXGrab, i.uv);
    i.uv.xy = DoDitherBlur(i);
    float4 blurCol = tex2Dproj(_MSFXGrab, i.uv);
    col.rgb = DoBlur(i, col.rgb, blurCol.rgb);
	#if defined(SFXX)
		col.rgb = DoDepthBuffer(i, col.rgb);
		col.rgb = DoNormalMap(i, col.rgb);
		col.rgb = DoOutline(i, col.rgb);
		col.rgb = DoFog(i, col.rgb);
	#endif
    col.rgb = DoColor(i, col.rgb);
	#if defined(SFXX)
		col.rgb = DoRounding(col.rgb);
	#endif
    col = DoTransparency(i, col);
	return col;
}
#elif defined(SFXX)
	#include "SFXXPasses.cginc"
#endif
#endif