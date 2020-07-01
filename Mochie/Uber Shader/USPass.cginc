//----------------------------
// FORWARD && ADD PASSES
//----------------------------
#if (defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)) && !defined(OUTLINE)

v2g vert (appdata v) {
    v2g o = (v2g)0;
	o.isReflection = IsInMirror();
	o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
	o.cameraPos = _WorldSpaceCameraPos;
	#if UNITY_SINGLE_PASS_STEREO
		o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*0.5;
	#endif

	#if defined(UBERX)
		VertX(o, v);
	#else
		UNITY_BRANCH
		if ((o.isReflection && _MirrorBehavior == 3) ||  (!o.isReflection && _MirrorBehavior == 1)){
			o.pos = 0.0/_NaNLmao;
		}
		else {
			o.pos = UnityObjectToClipPos(v.vertex);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);
			o.normal = UnityObjectToWorldNormal(v.normal);
			o.tangent.xyz = UnityObjectToWorldDir(v.tangent.xyz);
			o.screenPos = ComputeGrabScreenPos(o.pos);
		}
	#endif
	o.localPos = v.vertex.xyz;
	o.tangent.w = v.tangent.w;
    v.tangent.xyz = normalize(v.tangent.xyz);
    v.normal = normalize(v.normal);
    float3x3 objectToTangent = float3x3(v.tangent.xyz, (cross(v.normal, v.tangent.xyz) * v.tangent.w), v.normal);
    o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));

	o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + (_Time.y * _MainTexScroll);
    o.uv.zw = TRANSFORM_TEX(v.uv, _EmissionMap) + (_Time.y * _EmissScroll);
	o.uv2.xy = TRANSFORM_TEX(v.uv, _DetailAlbedoMap) + (_Time.y * _DetailScroll);
	o.uv2.zw = TRANSFORM_TEX(v.uv, _RimTex) + (_Time.y * _RimScroll);
	o.uv3.xy = TRANSFORM_TEX(v.uv, _ReflTex);
	o.uv3.zw = TRANSFORM_TEX(v.uv, _SpecTex);
	o.uv4.xy = TRANSFORM_TEX(v.uv, _DistortUVMap) + (_Time.y * _DistortUVScroll);
	o.uv4.zw = o.uv.xy;

	UNITY_TRANSFER_SHADOW(o, v.uv1);
	UNITY_TRANSFER_FOG(o, o.pos);
    return o;
}

#include "USXGeom.cginc"

float4 frag (g2f i) : SV_Target {
	
	#if defined(UBERX)
		float falloff, falloffRim;
		GetFalloff(i, falloff, falloffRim);
		clip(falloff);
	#else
		UNITY_BRANCH
		if ((i.isReflection && _MirrorBehavior == 3) ||  (!i.isReflection && _MirrorBehavior == 1))
			discard;
	#endif
	
	i.screenPos = UNITY_PROJ_COORD(i.screenPos);
	ApplyUVDistortion(i, uvOffset);
	ApplyParallax(i);
	UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz);
	float3 attenCol = atten;
	attenCol = FadeShadows(i, attenCol);
	masks m = GetMasks(i);
    lighting l = GetLighting(i, m, attenCol);
	float4 albedo = GetAlbedo(i, l, m);
	
	if (_EnableSpritesheet != 1 && _UnlitSpritesheet != 1)
		ApplyCutout(l.screenUVs, albedo.a);
	else if (_EnableSpritesheet == 1 && _UnlitSpritesheet == 0)
		ApplyCutout(l.screenUVs, albedo.a);

    float4 diffuse = albedo;
	float3 emiss = GetEmission(i);
	float3 reflCol = 1;

	UNITY_BRANCH
	if (_RenderMode == 1){
		attenCol = GetRamp(i, l, m, albedo.rgb, attenCol);
		diffuse.rgb = GetWorkflow(i, l, m, albedo.rgb);
		roughness = GetRoughness(smoothness);
		reflCol = GetReflections(i, l, lerp(roughness, _ReflRough, _ReflUseRough)) * _ReflCol.rgb;
		reflCol *= tex2DBoolWhiteSampler(_ReflTex, i.uv3.xy, _UseReflTex);
		diffuse = PremultiplyAlpha(diffuse, omr);
		diffuse.rgb = GetMochieBRDF(i, l, m, diffuse, albedo, specularTint, reflCol, omr, smoothness, attenCol);
		
	}
	else {
		#if defined(UNITY_PASS_FORWARDBASE)
			diffuse = GetDiffuse(l, albedo, 1);
		#else
			diffuse = GetDiffuse(l, albedo, attenCol);
		#endif
	}

	// Emission, Rim Lighting, Dissolve Rim, Wireframe (if clone), and Fog
    ApplyRimLighting(i, l, m, diffuse.rgb);
	ApplyERimLighting(i, l, m, diffuse.rgb, lerp(roughness, _ERimRoughness, _ERimUseRough));
    ApplyLREmission(l, diffuse.rgb, emiss);
	ApplyUnlitSpritesheet(i, m, diffuse, l.screenUVs);
	#if defined(UBERX)
		ApplyDissolveRim(i, diffuse.rgb); 
		ApplyWireframe(i, diffuse.rgb);
		ApplyFalloffRim(i, diffuse.rgb, falloffRim);
	#endif
	
	UNITY_BRANCH
	if (_PostFiltering == 1 && _FilterModel > 0){
		if 		(_FilterModel == 1) ApplyRGBFilter(m, diffuse.rgb);
		else if (_FilterModel == 2) ApplyHSLFilter(m, diffuse.rgb);
		else if (_FilterModel == 3) ApplyHSVFilter(m, diffuse.rgb);
		else if (_FilterModel == 4) ApplyTeamColors(m, diffuse.rgb, i.uv.xy);
	}

    UNITY_APPLY_FOG(i.fogCoord, diffuse);

	#if defined(UNITY_PASS_FORWARDBASE)
		UNITY_BRANCH
		if (_PreviewActive == 1){
			ApplyRoughPreview(i, diffuse.rgb);
			ApplySmoothPreview(diffuse.rgb);
			ApplyAOPreview(l, diffuse.rgb);
			ApplyHeightPreview(i, diffuse.rgb);
			ApplyNoisePreview(i, diffuse.rgb);
		}
	#endif
	
	return diffuse;
}
#endif

//----------------------------
// SHADOWCASTER PASS
//----------------------------
#if defined(UNITY_PASS_SHADOWCASTER)

v2g vert (appdata v) {
    v2g o = (v2g)0;
	o.isReflection = IsInMirror();
	o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
	o.cameraPos = _WorldSpaceCameraPos;
	#if UNITY_SINGLE_PASS_STEREO
		o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*0.5;
	#endif
	
	#if defined(UBERX)
		VertX(o, v);
	#else
		UNITY_BRANCH
		if ((o.isReflection && _MirrorBehavior == 3) ||  (!o.isReflection && _MirrorBehavior == 1)){
			o.pos = 0.0/_NaNLmao;
		}
		else {
			o.pos = UnityObjectToClipPos(v.vertex);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);
			o.screenPos = ComputeScreenPos(v.vertex);
		}
	#endif
	o.localPos = v.vertex.xyz;
	o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + (_Time.y * _MainTexScroll);
	TRANSFER_SHADOW_CASTER(o)
    return o;
}

#include "USXGeom.cginc"

float4 frag(g2f i) : SV_Target {

	#if defined(UBERX)
		float falloff, falloffRim;
		GetFalloff(i, falloff, falloffRim);
		clip(falloff);
	#else
		if ((i.isReflection && _MirrorBehavior == 3) ||  (!i.isReflection && _MirrorBehavior == 1))
			discard;
	#endif

    #if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
		float alpha = 1;
		if (_UseAlphaMask == 1)
			alpha = SampleMask(_AlphaMask, i.uv.xy, _AlphaMaskChannel, true);
		else
			alpha = UNITY_SAMPLE_TEX2D(_MainTex, i.uv.xy).a;
		
		#if !defined(_ALPHATEST_ON)
			alpha *= _Color.a;
		#endif
		alpha = ShadowPremultiplyAlpha(i, alpha);
		if (_BlendMode == 1)
			clip(alpha - _Cutoff);
		else if (_BlendMode > 1)
			clip(tex3D(_DitherMaskLOD, float3(i.pos.xy*0.25, alpha * 0.9375)).a - 0.01);

    #endif

	#if defined(UBERX) && (defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON))
		if (_DissolveToggle == 1)
			clip(GetDissolveValue(i) - _DissolveAmount);
	#endif

	SHADOW_CASTER_FRAGMENT(i);
}
#endif

//----------------------------
// OUTLINE PASS
//----------------------------
#if defined(OUTLINE)

v2g vert (appdata v) {
    v2g o = (v2g)0;

	#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
		o.pos = 0.0/_NaNLmao;
	#else
		o.isReflection = IsInMirror();
		v.vertex.xyz += _OutlineThicc*v.normal*0.01;
		o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
		o.cameraPos = _WorldSpaceCameraPos;
		#if UNITY_SINGLE_PASS_STEREO
			o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*0.5;
		#endif

		#if defined(UBERX)
			VertX(o, v);
		#else
			UNITY_BRANCH
			if ((o.isReflection && _MirrorBehavior == 3) ||  (!o.isReflection && _MirrorBehavior == 1) || _ATM == 1){
				o.pos = 0.0/_NaNLmao;
			}
			else {
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.tangent.xyz = UnityObjectToWorldDir(v.tangent.xyz);
				o.screenPos = ComputeGrabScreenPos(o.pos);
			}
		#endif
		o.localPos = v.vertex.xyz;
		
		o.tangent.w = v.tangent.w;
		v.tangent.xyz = normalize(v.tangent.xyz);
		v.normal = normalize(v.normal);
		float3x3 objectToTangent = float3x3(v.tangent.xyz, (cross(v.normal, v.tangent.xyz) * v.tangent.w), v.normal);
		o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));

		o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + (_Time.y * _MainTexScroll);
		o.uv.zw = TRANSFORM_TEX(v.uv, _EmissionMap) + (_Time.y * _EmissScroll);
		o.uv2.xy = TRANSFORM_TEX(v.uv, _DetailAlbedoMap) + (_Time.y * _DetailScroll);
		o.uv2.zw = TRANSFORM_TEX(v.uv, _OutlineTex) + (_Time.y * _OutlineScroll);
		o.uv4.xy = TRANSFORM_TEX(v.uv, _DistortUVMap) + (_Time.y * _DistortUVScroll);
		o.uv4.zw = o.uv.xy;
		UNITY_TRANSFER_SHADOW(o, v.uv1);
		UNITY_TRANSFER_FOG(o, o.pos);
	#endif

    return o;
}

#include "USXGeom.cginc"

float4 frag(g2f i) : SV_Target {

	#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
		discard;
	#endif

	#if defined(_ALPHATEST_ON)
		if (_ATM == 1)
			discard;
	#endif

	if (_Outline == 0 || _PreviewActive == 1)
		discard;

	float objDist = distance(i.cameraPos, i.worldPos);
	if (objDist < _OutlineRange)
		discard;
		
	#if defined(UBERX)
		float falloff, falloffRim;
		GetFalloff(i, falloff, falloffRim);
		clip(falloff);
	#else
		if ((i.isReflection && _MirrorBehavior == 3) ||  (!i.isReflection && _MirrorBehavior == 1))
			discard;
	#endif
	
	float mask = -(1-SampleMask(_OutlineMask, i.uv, _OutlineMaskChannel, true));
	clip(mask);

	UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
	float3 attenCol = atten;
	attenCol = FadeShadows(i, attenCol);
	masks m = GetMasks(i);
	lighting l = GetLighting(i, m, attenCol);
	
	float4 albedo = _OutlineCol; 
	#if defined(_ALPHATEST_ON)
		albedo = UNITY_SAMPLE_TEX2D(_MainTex, i.uv) * _OutlineCol;
		if (_UseAlphaMask == 1 && _Outline != 2)
			albedo.a = SampleMask(_AlphaMask, i.uv.xy, _AlphaMaskChannel, true);
		ApplyCutout(l.screenUVs, albedo.a);
		#if defined(UBERX)
			if (_DissolveToggle == 1)
				clip(GetDissolveValue(i) - _DissolveAmount);
		#endif
	#endif

	[flatten]
	switch (_Outline){
		case 1: albedo.rgb = _OutlineCol.rgb; break;
		case 2: albedo = GetAlbedo(i, l, GetMasks(i)) * _OutlineCol; break;
		case 3: albedo = UNITY_SAMPLE_TEX2D_SAMPLER(_OutlineTex, _MainTex, i.uv2.zw) * _OutlineCol; break;
		default: break; 
	}
	float4 diffuse = albedo;

	if (_ApplyOutlineLighting == 1){
		UNITY_BRANCH
		if (_RenderMode == 1){
			attenCol = GetRamp(i, l, m, albedo.rgb, attenCol);
			diffuse.rgb = GetWorkflow(i, l, m, albedo.rgb);
			roughness = GetRoughness(smoothness);
			diffuse.rgb = GetMochieBRDF(i, l, m, diffuse, albedo, specularTint, 0, omr, smoothness, attenCol);
		}
		else diffuse = GetDiffuse(l, albedo, 1);
	}

	float3 emiss = GetEmission(i);

	float interpolator = 1;
	if (_EmissionToggle > 0 && _ApplyOutlineEmiss == 1){
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
	}

	float4 col = 1;
	if (_Outline == 1)
		col.rgb = lerp(_EmissionColor, diffuse.rgb, interpolator);
	else 
		col.rgb = lerp(diffuse.rgb+emiss.rgb, diffuse.rgb, interpolator);

	#if defined(UBERX)
		ApplyFalloffRim(i, col.rgb, falloffRim);
		ApplyDissolveRim(i, col.rgb); 
		ApplyWireframe(i, col.rgb);
	#endif

	if (_PostFiltering == 1 && _FilterModel > 0){
		UNITY_BRANCH
		if 		(_FilterModel == 1) ApplyRGBFilter(m, col.rgb);
		else if (_FilterModel == 2) ApplyHSLFilter(m, col.rgb);
		else if (_FilterModel == 3) ApplyHSVFilter(m, col.rgb);
		else if (_FilterModel == 4) ApplyTeamColors(m, col.rgb, i.uv.xy);
	}

	UNITY_APPLY_FOG(i.fogCoord, col);
    return col;
}
#endif