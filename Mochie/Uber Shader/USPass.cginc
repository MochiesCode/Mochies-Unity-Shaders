//----------------------------
// FORWARD && ADD PASSES
//----------------------------
#if (FORWARD_PASS && !OUTLINE_PASS) || ADDITIVE_PASS

v2g vert (appdata v) {
    v2g o = (v2g)0;
	o.isReflection = IsInMirror();
	o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
	o.cameraPos = _WorldSpaceCameraPos;
	#if UNITY_SINGLE_PASS_STEREO
		o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*0.5;
	#endif

	#if X_FEATURES
		VertX(o, v);
	#else
		o.pos = UnityObjectToClipPos(v.vertex);
		o.worldPos = mul(unity_ObjectToWorld, v.vertex);
		o.normal = UnityObjectToWorldNormal(v.normal);
		o.tangent.xyz = UnityObjectToWorldDir(v.tangent.xyz);
		o.screenPos = ComputeGrabScreenPos(o.pos);
	#endif
	
	o.localPos = v.vertex.xyz;
	o.tangent.w = v.tangent.w;
    v.tangent.xyz = normalize(v.tangent.xyz);
    v.normal = normalize(v.normal);
    float3x3 objectToTangent = float3x3(v.tangent.xyz, (cross(v.normal, v.tangent.xyz) * v.tangent.w), v.normal);
    o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));

	o.rawUV = v.uv;
	o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + (_Time.y * _MainTexScroll);
    o.uv.zw = TRANSFORM_TEX(v.uv, _EmissionMap) + (_Time.y * _EmissScroll);
	o.uv2.xy = TRANSFORM_TEX(v.uv, _DetailAlbedoMap) + (_Time.y * _DetailScroll);
	o.uv2.zw = TRANSFORM_TEX(v.uv, _RimTex) + (_Time.y * _RimScroll);
	o.uv3.xy = TRANSFORM_TEX(v.uv, _DistortUVMap) + (_Time.y * _DistortUVScroll);

	UNITY_TRANSFER_SHADOW(o, v.uv1);
	UNITY_TRANSFER_FOG(o, o.pos);
    return o;
}

#include "USXGeom.cginc"

float4 frag (g2f i) : SV_Target {
	
	#if X_FEATURES && (NON_OPAQUE_RENDERING)
		float falloff, falloffRim;
		GetFalloff(i, falloff, falloffRim);
		clip(falloff);
	#endif

	if ((i.isReflection && _MirrorBehavior == 3) ||  (!i.isReflection && _MirrorBehavior == 1))
		discard;
	
	#if UV_DISTORTION_ENABLED
		ApplyUVDistortion(i, uvOffset);
	#endif

	#if PARALLAX_ENABLED
		ApplyParallax(i);
	#endif

	i.screenPos = UNITY_PROJ_COORD(i.screenPos);
	UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz);
	float3 attenCol = atten;
	attenCol = FadeShadows(i, attenCol);
	masks m = GetMasks(i);
    lighting l = GetLighting(i, m, attenCol);
	float4 albedo = GetAlbedo(i, l, m);

	#if ALPHA_TEST
		ApplyCutout(l.screenUVs, albedo.a);
	#endif

	#if EMISSION_ENABLED
		float3 emiss = GetEmission(i);
	#endif

	float4 diffuse = albedo;

	#if SHADING_ENABLED

		attenCol = GetRamp(i, l, m, albedo.rgb, attenCol);
		diffuse.rgb = GetWorkflow(i, l, m, albedo.rgb);
		roughness = GetRoughness(smoothness);

		float3 reflCol = 1;
		#if REFLECTIONS_ENABLED
			reflCol = GetReflections(i, l, lerp(roughness, _ReflRough, _ReflUseRough)) * _ReflCol.rgb;
		#endif

		#if ALPHA_PREMULTIPLY
			diffuse = PremultiplyAlpha(diffuse, omr);
		#endif

		diffuse.rgb = GetMochieBRDF(i, l, m, diffuse, albedo, specularTint, reflCol, omr, smoothness, attenCol);
		
		#if FORWARD_PASS
			ApplyRimLighting(i, l, m, diffuse.rgb);	
		#endif

		#if ENVIRONMENT_RIM_ENABLED
			ApplyERimLighting(i, l, m, diffuse.rgb, lerp(roughness, _ERimRoughness, _ERimUseRough));
		#endif

	// SHADING OFF
	#else
		#if FORWARD_PASS
			diffuse = GetDiffuse(l, albedo, 1);
		#else
			float ramp = smoothstep(0, 0.005, l.NdotL) * atten;
			diffuse = GetDiffuse(l, albedo, ramp);
		#endif
	#endif

	#if EMISSION_ENABLED
    	ApplyLREmission(l, diffuse.rgb, emiss);
	#endif

	#if SPRITESHEETS_ENABLED
		if (_EnableSpritesheet == 1 && _UnlitSpritesheet == 1)
			ApplySpritesheet0(i, diffuse.rgb);
		if (_EnableSpritesheet1 == 1 && _UnlitSpritesheet1 == 1)
			ApplySpritesheet1(i, diffuse.rgb);
	#endif

	#if X_FEATURES
		#if NON_OPAQUE_RENDERING
			#if !DISSOLVE_GEOMETRY
				ApplyDissolveRim(i, diffuse.rgb); 
			#endif
			ApplyFalloffRim(i, diffuse.rgb, falloffRim);
		#endif
		ApplyWireframe(i, diffuse.rgb);
	#endif
	
	#if POST_FILTERING_ENABLED
		ApplyFiltering(i, m, diffuse.rgb);
	#endif

    UNITY_APPLY_FOG(i.fogCoord, diffuse);

	#if PBR_PREVIEW_ENABLED
		ApplyRoughPreview(diffuse.rgb);
		ApplySmoothPreview(diffuse.rgb);
		ApplyAOPreview(diffuse.rgb);
		ApplyHeightPreview(diffuse.rgb);
	#endif
	
	return diffuse;
}
#endif

//----------------------------
// OUTLINE PASS
//----------------------------
#if OUTLINE_PASS

v2g vert (appdata v) {
    v2g o = (v2g)0;

	#if TRANSPARENT_RENDERING
		o.pos = 0.0/_NaNLmao;
	#else
		o.isReflection = IsInMirror();
		float thicknessMask = tex2Dlod(_OutlineMask, float4(v.uv.xy,0,0));
		v.vertex.xyz += _OutlineThicc*v.normal*0.01*_OutlineMult*thicknessMask*lerp(1,v.color.xyz,_UseVertexColor);
		o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
		o.cameraPos = _WorldSpaceCameraPos;
		#if UNITY_SINGLE_PASS_STEREO
			o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*0.5;
		#endif

		#if X_FEATURES
			VertX(o, v);
		#else
			o.pos = UnityObjectToClipPos(v.vertex);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);
			o.normal = UnityObjectToWorldNormal(v.normal);
			o.tangent.xyz = UnityObjectToWorldDir(v.tangent.xyz);
			o.screenPos = ComputeGrabScreenPos(o.pos);
		#endif
		o.localPos = v.vertex.xyz;
		
		o.tangent.w = v.tangent.w;
		v.tangent.xyz = normalize(v.tangent.xyz);
		v.normal = normalize(v.normal);
		float3x3 objectToTangent = float3x3(v.tangent.xyz, (cross(v.normal, v.tangent.xyz) * v.tangent.w), v.normal);
		o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));
		
		o.rawUV = v.uv;
		o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + (_Time.y * _MainTexScroll);
		o.uv.zw = TRANSFORM_TEX(v.uv, _EmissionMap) + (_Time.y * _EmissScroll);
		o.uv2.xy = TRANSFORM_TEX(v.uv, _DetailAlbedoMap) + (_Time.y * _DetailScroll);
		o.uv2.zw = TRANSFORM_TEX(v.uv, _OutlineTex) + (_Time.y * _OutlineScroll);
		o.uv3.xy = TRANSFORM_TEX(v.uv, _DistortUVMap) + (_Time.y * _DistortUVScroll);
		UNITY_TRANSFER_SHADOW(o, v.uv1);
		UNITY_TRANSFER_FOG(o, o.pos);
	#endif

    return o;
}

#include "USXGeom.cginc"

float4 frag(g2f i) : SV_Target {
	float4 col = 0;

	#if PBR_PREVIEW_ENABLED
		discard;
	#elif ALPHA_TEST
		if (_ATM == 1)
			discard;
	#endif

	if (distance(i.cameraPos, i.worldPos) < _OutlineRange)
		discard;
		
	#if X_FEATURES && (NON_OPAQUE_RENDERING)
		float falloff, falloffRim;
		GetFalloff(i, falloff, falloffRim);
		clip(falloff);
	#endif
	
	if ((i.isReflection && _MirrorBehavior == 3) ||  (!i.isReflection && _MirrorBehavior == 1))
		discard;
	
	// float mask = tex2D(_OutlineMask, i.uv);
	// clip(mask-0.5);

	UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
	float3 attenCol = atten;
	attenCol = FadeShadows(i, attenCol);
	masks m = GetMasks(i);
	lighting l = GetLighting(i, m, attenCol);
	
	float4 baseColor = GetAlbedo(i, l, m);
	float4 outlineTex = UNITY_SAMPLE_TEX2D_SAMPLER(_OutlineTex, _MainTex, i.uv2.zw) * _OutlineCol;
	float4 albedo = lerp(outlineTex, outlineTex * baseColor, _ApplyAlbedoTint);

	#if ALPHA_TEST
		if (_UseAlphaMask == 1)
			albedo.a = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaMask, _MainTex, i.uv.xy);
		ApplyCutout(l.screenUVs, baseColor.a);
		#if X_FEATURES && !DISSOLVE_GEOMETRY
			if (_DissolveToggle == 1)
				clip(GetDissolveValue(i) - _DissolveAmount);
		#endif
	#endif

	float4 diffuse = albedo;

	if (_ApplyOutlineLighting == 1){
		#if SHADING_ENABLED
			attenCol = GetRamp(i, l, m, albedo.rgb, attenCol);
			diffuse.rgb = GetWorkflow(i, l, m, albedo.rgb);
			roughness = GetRoughness(smoothness);
			diffuse.rgb = GetMochieBRDF(i, l, m, diffuse, albedo, specularTint, 0, omr, smoothness, attenCol);
		#else
			diffuse = GetDiffuse(l, albedo, 1);
		#endif
	}

	col = diffuse;
	
	#if EMISSION_ENABLED
		float3 emiss = lerp(_EmissionColor.rgb, GetEmission(i), _ApplyAlbedoTint);
		#if PULSE_ENABLED
			emiss *= GetPulse(i);
		#endif
		emiss += diffuse.rgb;
		emiss = clamp(emiss, 0, _EmissionColor.rgb);
		float interpolator = 1;
		if (_ApplyOutlineEmiss == 1){
			interpolator = 0;
			if (_ReactToggle == 1){
				if (_CrossMode == 1){
					float2 threshold = saturate(float2(_ReactThresh-_Crossfade, _ReactThresh+_Crossfade));
					interpolator = smootherstep(threshold.x, threshold.y, l.worldBrightness); 
				}
				else {
					interpolator = l.worldBrightness;
				}
			}
			col.rgb = lerp(emiss, diffuse.rgb, interpolator);
		}
	#endif
	
	#if X_FEATURES
		#if NON_OPAQUE_RENDERING
			ApplyFalloffRim(i, col.rgb, falloffRim);
			#if !DISSOLVE_GEOMETRY
				ApplyDissolveRim(i, col.rgb); 
			#endif
		#endif
		ApplyWireframe(i, col.rgb);
	#endif

	#if POST_FILTERING_ENABLED
		ApplyFiltering(i, m, col.rgb);
	#endif

	UNITY_APPLY_FOG(i.fogCoord, col);
    return col;
}
#endif

//----------------------------
// SHADOWCASTER PASS
//----------------------------
#if SHADOW_PASS

v2g vert (appdata v) {
    v2g o = (v2g)0;
	o.isReflection = IsInMirror();
	o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
	o.cameraPos = _WorldSpaceCameraPos;
	#if UNITY_SINGLE_PASS_STEREO
		o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*0.5;
	#endif
	
	#if X_FEATURES
		VertX(o, v);
	#else
		o.pos = UnityObjectToClipPos(v.vertex);
		o.worldPos = mul(unity_ObjectToWorld, v.vertex);
		o.screenPos = ComputeScreenPos(v.vertex);
	#endif
	o.localPos = v.vertex.xyz;
	o.rawUV = v.uv;
	o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + (_Time.y * _MainTexScroll);
	TRANSFER_SHADOW_CASTER(o)
    return o;
}

#include "USXGeom.cginc"

float4 frag(g2f i) : SV_Target {

	#if PBR_PREVIEW_ENABLED
		discard;
	#endif

	if ((i.isReflection && _MirrorBehavior == 3) ||  (!i.isReflection && _MirrorBehavior == 1))
		discard;

    #if NON_OPAQUE_RENDERING
		#if X_FEATURES
			float falloff, falloffRim;
			GetFalloff(i, falloff, falloffRim);
			clip(falloff);
		#endif
		
		float alpha = 1;
		float4 albedo = UNITY_SAMPLE_TEX2D(_MainTex, i.uv.xy) * _Color;
		float maskAlpha = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaMask, _MainTex, i.uv.xy) * _Color.a;
		alpha = albedo.a;
		if (_UseAlphaMask == 1)
			alpha = maskAlpha;

		#if ALPHA_PREMULTIPLY
			alpha = ShadowPremultiplyAlpha(i, alpha);
		#endif

		if (_BlendMode == 1)
			clip(alpha - _Cutoff);
		else {
			clip(tex3D(_DitherMaskLOD, float3(i.pos.xy*0.25, alpha * 0.9375)).a - 0.01);
		}

    #endif

	#if X_FEATURES && (NON_OPAQUE_RENDERING)
		if (_DissolveToggle == 1)
			clip(GetDissolveValue(i) - _DissolveAmount);
	#endif

	SHADOW_CASTER_FRAGMENT(i);
}
#endif