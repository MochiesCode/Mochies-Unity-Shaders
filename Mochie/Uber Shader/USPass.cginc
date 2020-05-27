//----------------------------
// FORWARD && ADD PASSES
//----------------------------
#if (defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)) && !defined(OUTLINE)

v2g vert (appdata v) {
    v2g o;
	UNITY_INITIALIZE_OUTPUT(v2g, o);
	o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
	o.cameraPos = _WorldSpaceCameraPos;
	#if UNITY_SINGLE_PASS_STEREO
		o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*0.5;
	#endif
	#if defined(VERTEXLIGHT_ON)
		o.isVLight = true;
	#endif

	#if defined(UBERX)
		VertX(o, v);
	#else
		o.pos = UnityObjectToClipPos(v.vertex);
		o.worldPos = mul(unity_ObjectToWorld, v.vertex);
		o.normal = UnityObjectToWorldNormal(v.normal);
		o.tangent.xyz = UnityObjectToWorldDir(v.tangent.xyz);
		o.screenPos = ComputeGrabScreenPos(o.pos);
	#endif

	o.tangent.w = v.tangent.w;
	o.binormal = GetBinormal(o.tangent, o.normal);
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
	o.uv4.xy = TRANSFORM_TEX(v.uv, _ERimTex) + (_Time.y * _ERimScroll);
	o.uv4.zw = TRANSFORM_TEX(v.uv, _DistortUVMap) + (_Time.y * _DistortUVScroll);

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
	
	UNITY_BRANCH
	if (_EnableSpritesheet != 1 && _UnlitSpritesheet != 1)
		ApplyCutout(albedo.a);
	else if (_EnableSpritesheet == 1 && _UnlitSpritesheet == 0)
		ApplyCutout(albedo.a);

    float4 diffuse = albedo;
	float3 emiss = GetEmission(i);
	float3 reflCol = 1;

	UNITY_BRANCH
	if (_RenderMode == 0){
		#if defined(UNITY_PASS_FORWARDBASE)
			diffuse = GetDiffuse(l, albedo, 1);
		#else
			diffuse = GetDiffuse(l, albedo, attenCol);
		#endif
	}
	else {
		attenCol = GetRamp(i, l, m, albedo.rgb, attenCol);
		diffuse.rgb = GetWorkflow(i, l, m, albedo.rgb, specularTint, smoothness, omr);
		reflCol = GetReflections(i, l, GetRoughness(1-smoothness)) * _ReflCol.rgb;
		reflCol *= tex2DBoolWhiteSampler(_ReflTex, i.uv3.xy, _UseReflTex);
		diffuse.rgb = GetMochieBRDF(i, l, m, diffuse, albedo, specularTint, reflCol, omr, smoothness, attenCol);
	}

	// Emission, Rim Lighting, Dissolve Rim, Wireframe (if clone), and Fog
    diffuse.rgb = ApplyRimLighting(i, l, m, diffuse.rgb);
	diffuse.rgb = ApplyERimLighting(i, l, m, diffuse.rgb, GetRoughness(1-smoothness));
    diffuse.rgb = ApplyLREmission(l, diffuse.rgb, emiss);
	diffuse = ApplyUnlitSpritesheet(i, m, diffuse);
	#if defined(UBERX)
		diffuse.rgb = ApplyDissolveRim(i, diffuse.rgb); 
		diffuse.rgb = ApplyWireframe(i, diffuse.rgb);
		diffuse.rgb = ApplyFalloffRim(i, diffuse.rgb, falloffRim);
	#endif
	
	UNITY_BRANCH
	if (_PostFiltering == 1 && _FilterModel > 0){
		UNITY_BRANCH
		if 		(_FilterModel == 1) diffuse.rgb = GetRGBFilter(m, diffuse.rgb);
		else if (_FilterModel == 2) diffuse.rgb = GetHSLFilter(m, diffuse.rgb);
		else if (_FilterModel == 3) diffuse.rgb = GetHSVFilter(m, diffuse.rgb);
		else if (_FilterModel == 4) diffuse.rgb = ApplyTeamColors(m, diffuse.rgb, i.uv.xy);
	}

    UNITY_APPLY_FOG(i.fogCoord, diffuse);

	#if defined(UNITY_PASS_FORWARDBASE)
		UNITY_BRANCH
		if (_PreviewActive == 1){
			diffuse.rgb = ApplyRoughPreview(i, diffuse.rgb);
			diffuse.rgb = ApplySmoothPreview(diffuse.rgb);
			diffuse.rgb = ApplyAOPreview(l, diffuse.rgb);
			diffuse.rgb = ApplyHeightPreview(i, diffuse.rgb);
			diffuse.rgb = ApplyNoisePreview(i, diffuse.rgb);
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
    v2g o;
	UNITY_INITIALIZE_OUTPUT(v2g, o);
	#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
		o.pos = 0.0/_NaNxddddd;
	#else
		o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
		o.cameraPos = _WorldSpaceCameraPos;
		#if UNITY_SINGLE_PASS_STEREO
			o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*0.5;
		#endif
		
		#if defined(UBERX)
			VertX(o, v);
		#else
			o.pos = UnityObjectToClipPos(v.vertex);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);
		#endif

		o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + (_Time.y * _MainTexScroll);

		TRANSFER_SHADOW_CASTER(o);
	#endif
    return o;
}

#include "USXGeom.cginc"

float4 frag(g2f i) : SV_Target {
	#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
		discard;
	#endif
	#if defined(UBERX)
		float falloff, falloffRim;
		GetFalloff(i, falloff, falloffRim);
		clip(falloff);
	#endif
    #if defined(_ALPHATEST_ON)
		float alpha = 1;
		UNITY_BRANCH
		if (_UseAlphaMask == 1)
			alpha = SampleMask(_AlphaMask, i.uv.xy, _AlphaMaskChannel, true);
		else
			alpha = UNITY_SAMPLE_TEX2D(_MainTex, i.uv.xy).a;
		UNITY_BRANCH
		if (_ATM != 1)
			clip(alpha - _Cutoff);
    #endif
	#if defined(UBERX) && defined(_ALPHATEST_ON)
		UNITY_BRANCH
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
    v2g o;
	UNITY_INITIALIZE_OUTPUT(v2g, o);
	#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
		o.pos = 0.0/_NaNxddddd;
	#else
		v.vertex.xyz += _OutlineThicc*v.normal*0.01;
		o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
		o.cameraPos = _WorldSpaceCameraPos;
		#if UNITY_SINGLE_PASS_STEREO
			o.cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*0.5;
		#endif
		#if defined(VERTEXLIGHT_ON)
			o.isVLight = true;
		#endif

		#if defined(UBERX)
			VertX(o, v);
		#else
			o.pos = UnityObjectToClipPos(v.vertex);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);
			o.normal = UnityObjectToWorldNormal(v.normal);
			o.tangent.xyz = UnityObjectToWorldDir(v.tangent.xyz);
		#endif

		o.tangent.w = v.tangent.w;
		o.binormal = GetBinormal(o.tangent, o.normal);
		v.tangent.xyz = normalize(v.tangent.xyz);
		v.normal = normalize(v.normal);
		float3x3 objectToTangent = float3x3(v.tangent.xyz, (cross(v.normal, v.tangent.xyz) * v.tangent.w), v.normal);
		o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));

		o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + (_Time.y * _MainTexScroll);
		o.uv.zw = TRANSFORM_TEX(v.uv, _EmissionMap) + (_Time.y * _EmissScroll);
		o.uv2.xy = TRANSFORM_TEX(v.uv, _DetailAlbedoMap) + (_Time.y * _DetailScroll);
		o.uv2.zw = TRANSFORM_TEX(v.uv, _OutlineTex) + (_Time.y * _OutlineScroll);
		o.uv4.zw = TRANSFORM_TEX(v.uv, _DistortUVMap) + (_Time.y * _DistortUVScroll);
		o.color = _OutlineCol;
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

	UNITY_BRANCH
	if (_Outline == 0 || _PreviewActive == 1)
		discard;

	float objDist = distance(i.cameraPos, i.worldPos);
	if (objDist < _OutlineRange)
		discard;
		
	#if defined(UBERX)
		float falloff, falloffRim;
		GetFalloff(i, falloff, falloffRim);
		clip(falloff);
	#endif
	
	float mask = -(1-SampleMask(_OutlineMask, i.uv, _OutlineMaskChannel, true));
	clip(mask);

	UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
	float3 attenCol = atten;
	attenCol = FadeShadows(i, attenCol);
	masks m = GetMasks(i);
	lighting l = GetLighting(i, m, attenCol);
	
	float4 albedo = i.color; 
	#if defined(_ALPHATEST_ON)
		albedo = UNITY_SAMPLE_TEX2D(_MainTex, i.uv) * i.color;
		UNITY_BRANCH
		if (_UseAlphaMask == 1 && _Outline != 2)
			albedo.a = SampleMask(_AlphaMask, i.uv.xy, _AlphaMaskChannel, true);
		#if defined(_ALPHATEST_ON)
			UNITY_BRANCH
			if (_ATM == 1)
				_Cutoff = 0.5;
			clip(albedo.a - _Cutoff);
			#if defined(UBERX)
				UNITY_BRANCH
				if (_DissolveToggle == 1)
					clip(GetDissolveValue(i) - _DissolveAmount);
			#endif
		#endif
	#endif

	[forcecase]
	switch (_Outline){
		case 1: albedo.rgb = i.color.rgb; break;
		case 2: albedo = GetAlbedo(i, l, GetMasks(i)) * i.color; break;
		case 3: albedo = UNITY_SAMPLE_TEX2D_SAMPLER(_OutlineTex, _MainTex, i.uv2.zw) * i.color; break;
		default: break; 
	}
	float4 diffuse = albedo;

	UNITY_BRANCH
	if (_ApplyOutlineLighting == 1){
		attenCol = GetRamp(i, l, m, albedo.rgb, attenCol);
		diffuse.rgb = GetWorkflow(i, l, m, albedo.rgb, specularTint, smoothness, omr);
		diffuse.rgb = GetMochieBRDF(i, l, m, diffuse, albedo, specularTint, 0, omr, smoothness, attenCol);
	}

	float3 emiss = GetEmission(i);

	float interpolator = 1;
	UNITY_BRANCH
	if (_EmissionToggle == 1 && _ApplyOutlineEmiss == 1){
		interpolator = 0;
		UNITY_BRANCH
		if (_ReactToggle == 1){
			UNITY_BRANCH
			if (_CrossMode == 1){
				float2 threshold = saturate(float2(_ReactThresh-_Crossfade, _ReactThresh+_Crossfade));
				interpolator = smootherstep(threshold.x, threshold.y, l.worldBrightness); 
			}
			else {
				interpolator = l.worldBrightness;
			}
		}
	}

	if (_Outline == 1)
		i.color.rgb = lerp(_EmissionColor, diffuse.rgb, interpolator);
	else 
		i.color.rgb = lerp(diffuse.rgb+emiss.rgb, diffuse.rgb, interpolator);

	#if defined(UBERX)
		i.color.rgb = ApplyFalloffRim(i, i.color.rgb, falloffRim);
		i.color.rgb = ApplyDissolveRim(i, i.color.rgb); 
		i.color.rgb = ApplyWireframe(i, i.color.rgb);
	#endif

	UNITY_BRANCH
	if (_ApplyOutlineLighting == 1){
		i.color.rgb = ApplyLREmission(l, i.color.rgb, emiss);
	}

	UNITY_BRANCH
	if (_PostFiltering == 1 && _FilterModel > 0){
		UNITY_BRANCH
		if 		(_FilterModel == 1) i.color.rgb = GetRGBFilter(m, i.color.rgb);
		else if (_FilterModel == 2) i.color.rgb = GetHSLFilter(m, i.color.rgb);
		else if (_FilterModel == 3) i.color.rgb = GetHSVFilter(m, i.color.rgb);
		else if (_FilterModel == 4) i.color.rgb = ApplyTeamColors(m, i.color.rgb, i.uv.xy);
	}

	UNITY_APPLY_FOG(i.fogCoord, i.color);
    return i.color;
}
#endif