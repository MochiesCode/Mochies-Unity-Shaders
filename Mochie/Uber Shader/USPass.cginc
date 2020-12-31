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

	float roundingMask = 0;
	#if VERTEX_MANIP_ENABLED
		roundingMask = tex2Dlod(_VertexRoundingMask, float4(v.uv.xy,0,0));
		float expansionMask = tex2Dlod(_VertexExpansionMask, float4(v.uv.xy,0,0));
		v.vertex.xyz += _VertexExpansion * lerp(v.normal.xyz, abs(v.normal.xyz), _VertexExpansionClamp) * expansionMask * 0.001;
	#endif
	float4 localPos = v.vertex;

	#if X_FEATURES
		o.roundingMask = roundingMask;
		VertX(o, v);
	#else
		o.worldPos = mul(unity_ObjectToWorld, localPos);
		#if VERTEX_MANIP_ENABLED
			if (_VertexRounding > 0)
				ApplyVertRounding(o.worldPos, localPos, _VertexRoundingPrecision, _VertexRounding, roundingMask);
		#endif
		o.pos = UnityObjectToClipPos(localPos);
		o.normal = UnityObjectToWorldNormal(v.normal);
		o.tangent.xyz = UnityObjectToWorldDir(v.tangent.xyz);
		o.grabPos = ComputeGrabScreenPos(o.pos);
		o.screenPos = ComputeScreenPos(o.pos);
	#endif
	
	o.localPos = localPos;
	o.tangent.w = v.tangent.w;
    v.tangent.xyz = normalize(v.tangent.xyz);
    v.normal = normalize(v.normal);
    float3x3 objectToTangent = float3x3(v.tangent.xyz, (cross(v.normal, v.tangent.xyz) * v.tangent.w), v.normal);
    o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(localPos));

	float2 detailUV = lerp3(v.uv, v.uv1, v.uv2, _UVSec);
	o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + (_Time.y * _MainTexScroll);
    o.uv.zw = TRANSFORM_TEX(v.uv, _EmissionMap) + (_Time.y * _EmissScroll);
	o.uv2.xy = TRANSFORM_TEX(detailUV, _DetailAlbedoMap) + (_Time.y * _DetailScroll);
	o.uv2.zw = TRANSFORM_TEX(v.uv, _RimTex) + (_Time.y * _RimScroll);
	o.uv3.xy = TRANSFORM_TEX(v.uv, _DistortUVMap) + (_Time.y * _DistortUVScroll);
	float2 anisoUV = lerp3(o.uv, v.uv1, v.uv2, _UVAniso);
	o.rawUV.xy = v.uv.xy;
	o.rawUV.zw = anisoUV;

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
	#elif PACKED_WORKFLOW || PACKED_WORKFLOW_BAKED
		packedTex = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMap, _MainTex, i.uv.xy);
	#endif

	i.grabPos = UNITY_PROJ_COORD(i.grabPos);
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
		float3 emiss = GetEmission(i, m);
	#endif

	float4 diffuse = albedo;

	#if SHADING_ENABLED

		attenCol = GetRamp(i, l, m, albedo.rgb, attenCol);
		diffuse.rgb = GetWorkflow(i, l, m, albedo.rgb);
		roughness = GetRoughness(smoothness);
		roughness = lerp(roughness, GSAARoughness(l.normal, roughness), _GSAA);
		
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
			ApplySpritesheet0(i, diffuse);
		if (_EnableSpritesheet1 == 1 && _UnlitSpritesheet1 == 1)
			ApplySpritesheet1(i, diffuse);
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
	
	#if REFRACTION_ENABLED
		if (_UnlitRefraction == 1)
			ApplyRefraction(i, l, m, diffuse.rgb);
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
		ApplyCurvePreview(diffuse.rgb);
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
		float thicknessMask = 1;
		#if SEPARATE_MASKING
			float2 thickMaskUV = v.uv.xy;
			#if MASK_SOS_ENABLED
				thickMaskUV = TRANSFORM_TEX(v.uv, _OutlineMask) + (_Time.y * _OutlineMaskScroll);
			#endif
			thicknessMask = tex2Dlod(_OutlineMask, float4(thickMaskUV,0,0));
		#elif PACKED_MASKING
			thicknessMask = tex2Dlod(_PackedMask3, float4(v.uv.xy,0,0)).a;
		#endif
		v.vertex.xyz += _OutlineThicc*v.normal*0.01*_OutlineMult*thicknessMask*lerp(1,v.color.xyz,_UseVertexColor);
		o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
		o.cameraPos = _WorldSpaceCameraPos;
		#if UNITY_SINGLE_PASS_STEREO
			o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*0.5;
		#endif

		float roundingMask = 0;
		#if VERTEX_MANIP_ENABLED
			roundingMask = tex2Dlod(_VertexRoundingMask, float4(v.uv.xy,0,0));
			float expansionMask = tex2Dlod(_VertexExpansionMask, float4(v.uv.xy,0,0));
			v.vertex.xyz += _VertexExpansion * lerp(v.normal.xyz, abs(v.normal.xyz), _VertexExpansionClamp) * expansionMask * 0.001;
		#endif
		float4 localPos = v.vertex;

		#if X_FEATURES
			o.roundingMask = roundingMask;
			VertX(o, v);
		#else
			o.worldPos = mul(unity_ObjectToWorld, localPos);
			#if VERTEX_MANIP_ENABLED
				if (_VertexRounding > 0)
					ApplyVertRounding(o.worldPos, localPos, _VertexRoundingPrecision, _VertexRounding, roundingMask);
			#endif
			o.pos = UnityObjectToClipPos(localPos);
			o.normal = UnityObjectToWorldNormal(v.normal);
			o.tangent.xyz = UnityObjectToWorldDir(v.tangent.xyz);
			o.grabPos = ComputeGrabScreenPos(o.pos);
			o.screenPos = ComputeScreenPos(o.pos);
		#endif

		o.localPos = localPos;
		o.tangent.w = v.tangent.w;
		v.tangent.xyz = normalize(v.tangent.xyz);
		v.normal = normalize(v.normal);
		float3x3 objectToTangent = float3x3(v.tangent.xyz, (cross(v.normal, v.tangent.xyz) * v.tangent.w), v.normal);
		o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(localPos));
		
		o.rawUV = v.uv;
		float2 detailUV = lerp3(v.uv, v.uv1, v.uv2, _UVSec);
		o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + (_Time.y * _MainTexScroll);
		o.uv.zw = TRANSFORM_TEX(v.uv, _EmissionMap) + (_Time.y * _EmissScroll);
		o.uv2.xy = TRANSFORM_TEX(detailUV, _DetailAlbedoMap) + (_Time.y * _DetailScroll);
		o.uv2.zw = TRANSFORM_TEX(v.uv, _OutlineTex) + (_Time.y * _OutlineScroll);
		o.uv3.xy = TRANSFORM_TEX(v.uv, _DistortUVMap) + (_Time.y * _DistortUVScroll);
		UNITY_TRANSFER_SHADOW(o, v.uv1);
		UNITY_TRANSFER_FOG(o, o.pos);
	#endif

    return o;
}

#include "USXGeom.cginc"

float4 frag(g2f i, uint facing : SV_IsFrontFace) : SV_Target {
	float4 col = 0;

	#if PBR_PREVIEW_ENABLED
		discard;
	#elif ALPHA_TEST
		if (_ATM == 1)
			discard;
	#endif

	#if SPRITESHEETS_ENABLED
		if (_UseSpritesheetAlpha == 1)
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

	#if PACKED_WORKFLOW || PACKED_WORKFLOW_BAKED
		packedTex = UNITY_SAMPLE_TEX2D_SAMPLER(_PackedMap, _MainTex, i.uv.xy);
	#endif
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
			if (_DissolveStyle > 0)
				clip(GetDissolveValue(i) - _DissolveAmount);
		#endif
	#endif

	float4 diffuse = albedo;

	if (_ApplyOutlineLighting == 1){
		#if SHADING_ENABLED
			attenCol = GetRamp(i, l, m, albedo.rgb, attenCol);
			diffuse.rgb = GetMochieBRDF(i, l, m, diffuse, albedo, specularTint, 0, omr, smoothness, attenCol);
		#else
			diffuse = GetDiffuse(l, albedo, 1);
		#endif
	}

	col = diffuse;
	
	#if EMISSION_ENABLED
		float3 emiss = lerp(_EmissionColor.rgb, GetEmission(i, m), _ApplyAlbedoTint);
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
	#if defined(OUTLINE_VARIANT)
		float thicknessMask = 1;
		#if SEPARATE_MASKING
			float2 thickMaskUV = v.uv.xy;
			#if MASK_SOS_ENABLED
				thickMaskUV = TRANSFORM_TEX(v.uv, _OutlineMask) + (_Time.y * _OutlineMaskScroll);
			#endif
			thicknessMask = tex2Dlod(_OutlineMask, float4(thickMaskUV,0,0));
		#elif PACKED_MASKING
			thicknessMask = tex2Dlod(_PackedMask3, float4(v.uv.xy,0,0)).a;
		#endif
		v.vertex.xyz += _OutlineThicc*v.normal*0.01*_OutlineMult*thicknessMask*lerp(1,v.color.xyz,_UseVertexColor);
	#endif
	o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
	o.cameraPos = _WorldSpaceCameraPos;
	#if UNITY_SINGLE_PASS_STEREO
		o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*0.5;
	#endif

	float roundingMask = 0;
	#if VERTEX_MANIP_ENABLED
		roundingMask = tex2Dlod(_VertexRoundingMask, float4(v.uv.xy,0,0));
		float expansionMask = tex2Dlod(_VertexExpansionMask, float4(v.uv.xy,0,0));
		v.vertex.xyz += _VertexExpansion * lerp(v.normal.xyz, abs(v.normal.xyz), _VertexExpansionClamp) * expansionMask * 0.001;
	#endif
	float4 localPos = v.vertex;

	#if X_FEATURES
		o.roundingMask = roundingMask;
		VertX(o, v);
	#else
		o.worldPos = mul(unity_ObjectToWorld, localPos);
		#if VERTEX_MANIP_ENABLED
			if (_VertexRounding > 0)
				ApplyVertRounding(o.worldPos, localPos, _VertexRoundingPrecision, _VertexRounding, roundingMask);
		#endif
		o.pos = UnityObjectToClipPos(localPos);
		o.grabPos = ComputeGrabScreenPos(o.pos);
		o.screenPos = ComputeScreenPos(o.pos);
	#endif

	o.localPos = localPos;
	o.rawUV = v.uv;
	o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + (_Time.y * _MainTexScroll);
	TRANSFER_SHADOW_CASTER(o)
    return o;
}

#include "USXGeom.cginc"

float4 frag(g2f i) : SV_Target {

	#if PBR_PREVIEW_ENABLED || REFRACTION_ENABLED
		discard;
	#endif

	#if SPRITESHEETS_ENABLED
		if (_UseSpritesheetAlpha == 1)
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
		float4 albedo = _MainTex.Sample(sampler_MainTex, i.uv.xy) * _Color;
		float maskAlpha = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaMask, _MainTex, i.uv.xy) * _Color.a;
		alpha = lerp(albedo.a, maskAlpha, _UseAlphaMask);

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
		if (_DissolveStyle > 0)
			clip(GetDissolveValue(i) - _DissolveAmount);
	#endif

	SHADOW_CASTER_FRAGMENT(i);
}
#endif