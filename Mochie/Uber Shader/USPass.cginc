//----------------------------
// FORWARD && ADD PASSES
//----------------------------
#if BASE_OR_ADD_DEFINED && !defined(OUTLINE)

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
	#endif

	o.tangent.w = v.tangent.w;
    o.binormal = cross(o.normal, o.tangent.xyz) * (o.tangent.w * unity_WorldTransformParams.w);
    v.tangent.xyz = normalize(v.tangent.xyz);
    v.normal = normalize(v.normal);
    float3x3 objectToTangent = float3x3(v.tangent.xyz, (cross(v.normal, v.tangent.xyz) * v.tangent.w), v.normal);
    o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));

	o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + (_Time.y * _MainTexScroll);
    o.uv.zw = TRANSFORM_TEX(v.uv, _EmissionMap) + (_Time.y * _EmissScroll);
	o.uv2.xy = TRANSFORM_TEX(v.uv, _DetailAlbedoMap) + (_Time.y * _DetailScroll);
	o.uv2.zw = TRANSFORM_TEX(v.uv, _RimTex) + (_Time.y * _RimScroll);

	UNITY_TRANSFER_SHADOW(o, v.uv1);
	UNITY_TRANSFER_FOG(o, o.pos);
    return o;
}

#if defined(UBERX)
	#include "USXGeom.cginc"
#endif

float4 frag (g2f i) : SV_Target {

	#if defined(UBERX)
		float falloff, falloffRim;
		GetFalloff(i, falloff, falloffRim);
		clip(falloff);
	#endif

	ApplyUVDistortion(i, uvOffset);
	ApplyParallax(i);
	UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
	atten = FadeShadows(i, atten);
	masks m = GetMasks(i);
    lighting l = GetLighting(i, m, atten);
    
	float4 albedo = GetAlbedo(i, l, m);
	ApplyCutout(albedo.a);
    float4 diffuse = albedo;
	float3 emiss = GetEmission(i);

	[forcecase]
	switch (_RenderMode){

		// No Shading
		case 0:
			#if defined(UNITY_PASS_FORWARDBASE)
				diffuse = GetDiffuse(l, albedo, 1);
				diffuse.rgb = lerp(diffuse.rgb, clamp(diffuse.rgb, 0, albedo.rgb), _ColorPreservation);
			#else
				diffuse = GetDiffuse(l, albedo, atten);
			#endif
			break;
		
		// Toon Shading
		case 1: 
			atten = GetRamp(i, l, m, atten);
			diffuse.rgb = GetToonWorkflow(i, l, m, albedo.rgb, specularTint, smoothness, omr);
			float3 reflCol = GetReflections(i, l, GetRoughness(1-smoothness)) * _ReflCol.rgb;
			diffuse.rgb = GetToonBRDF(i, l, m, diffuse.rgb, specularTint, reflCol, omr, smoothness, atten);
			break;
		
		// Standard PBR Shading
		case 2:
			UNITY_BRANCH
			switch (_PBRWorkflow){
				case 0: GetMetallicWorkflow(i, metallic, roughness, smoothness); break;
				case 1: GetSpecularWorkflow(i, albedo.a, spec, roughness, smoothness); break;
				case 2: GetPackedWorkflow(i, metallic, roughness, smoothness); break;
				default: break;
			}

			UnityLight directLight = GetDirectLight(l, atten);
			UnityIndirect indirectLight = GetIndirectLight(i, l, roughness);

			UNITY_BRANCH
			if (_PBRWorkflow != 1){
				albedo.rgb = DiffuseAndSpecularFromMetallic(albedo, metallic, specularTint, omr);
				diffuse.rgb = UNITY_BRDF_PBS(albedo, specularTint.rgb, omr, smoothness, l.normal, l.viewDir, directLight, indirectLight).rgb;
			}
			else {
				albedo.rgb = EnergyConservationBetweenDiffuseAndSpecular(albedo, spec, omr);
				diffuse.rgb = UNITY_BRDF_PBS(albedo, spec.rgb, omr, smoothness, l.normalDir, l.viewDir, directLight, indirectLight).rgb;
			}
			break;
			
		default: break;
	}

	// Emission, Rim Lighting, Dissolve Rim, Wireframe (if clone), and Fog
    diffuse.rgb = ApplyRimLighting(i, l, m, diffuse.rgb, atten);
    diffuse.rgb = ApplyLREmission(l, diffuse.rgb, atten, emiss);
	#if defined(UBERX)
		diffuse.rgb = ApplyDissolveRim(i, l, diffuse.rgb); 
		diffuse.rgb = ApplyWireframe(i, diffuse.rgb);
		diffuse.rgb = ApplyFalloffRim(i, diffuse.rgb, falloffRim);
	#endif
    UNITY_APPLY_FOG(i.fogCoord, diffuse);
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
    return o;
}

#if defined(UBERX)
	#include "USXGeom.cginc"
#endif

float4 frag(g2f i) : SV_Target {
	#if defined(UBERX)
		float falloff, falloffRim;
		GetFalloff(i, falloff, falloffRim);
		clip(falloff);
	#endif
    #if defined(CUTOUT)
        clip(UNITY_SAMPLE_TEX2D(_MainTex, i.uv.xy).a - _Cutoff);
    #endif
	#if defined(UBERX) && CUT_OR_TRANS_DEFINED
		lighting l;
		UNITY_INITIALIZE_OUTPUT(lighting, l);
		clip(GetDissolveValue(i, l) - _DissolveAmount);
	#endif
	SHADOW_CASTER_FRAGMENT(i)
}
#endif

//----------------------------
// OUTLINE PASS
//----------------------------
#if defined(OUTLINE)

v2g vert (appdata v){
    v2g o;
	UNITY_INITIALIZE_OUTPUT(v2g, o);

	v.vertex.xyz += _OutlineThicc*v.normal*0.01;
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
	o.uv.zw = TRANSFORM_TEX(v.uv, _EmissionMap) + (_Time.y * _EmissScroll);
	o.uv2.xy = TRANSFORM_TEX(v.uv, _OutlineTex) + (_Time.y * _OutlineScroll);

	o.color = _OutlineCol;
	
	UNITY_TRANSFER_SHADOW(o, v.uv1);
    UNITY_TRANSFER_FOG(o, o.pos);
    return o;
}

#if defined(UBERX)
	#include "USXGeom.cginc"
#endif

float4 frag(g2f i) : SV_Target {
	if (_Outline == 0)
		discard;

	float objDist = distance(i.cameraPos, i.worldPos);
	if (objDist < _OutlineRange)
		discard;
	#if defined(UBERX)
		float falloff, falloffRim;
		GetFalloff(i, falloff, falloffRim);
		clip(falloff);
	#endif

	float4 col = UNITY_SAMPLE_TEX2D(_MainTex, i.uv) * i.color;
	float3 emiss = GetEmission(i);
	float mask = -(1-SampleMask(_OutlineMask, i.uv, _OutlineMaskChannel, true));
	clip(mask);

	#if defined(CUTOUT)
		clip(col.a - _Cutoff);
	#endif

	#if defined(UBERX) && CUT_OR_TRANS_DEFINED
		lighting l;
		UNITY_INITIALIZE_OUTPUT(lighting, l);
		clip(GetDissolveValue(i, l) - _DissolveAmount);
	#endif

    UNITY_BRANCH
    if (_Outline == 2){
		i.color = col;
		i.color.rgb += emiss;
	}

	UNITY_BRANCH
	if (_Outline == 3)
		i.color = UNITY_SAMPLE_TEX2D_SAMPLER(_OutlineTex, _MainTex, i.uv2) * i.color;

	UNITY_BRANCH
	if (_UnlitOutline == 0){
		float3 lightCol = GetOutlineLightColor(float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w));
		UNITY_BRANCH
		if (_ReactToggle == 1)
			emiss *= AverageRGB(lightCol);
		i.color.rgb *= lightCol;
		i.color.rgb += emiss;
	}
	
	#if defined(UBERX)
		i.color.rgb = ApplyFalloffRim(i, i.color.rgb, falloffRim)
	#endif
	UNITY_APPLY_FOG(i.fogCoord, i.color);
    return i.color;
}
#endif