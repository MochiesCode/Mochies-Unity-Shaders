#ifndef WATER_VERT_INCLUDED
#define WATER_VERT_INCLUDED

v2f vert (
	#ifdef TESSELLATION_VARIANT
		TessellationControlPoint v
	#else
		appdata v
	#endif
) {
	v2f o = (v2f)0;
	#ifndef TESSELLATION_VARIANT
		UNITY_SETUP_INSTANCE_ID(v);
	#endif
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	#ifdef TESSELLATION_VARIANT
		#if NOISE_TEXTURE_ENABLED || GERSTNER_ENABLED || VORONOI_ENABLED
			o.offsetMask = 1;
			if (_TessellationOffsetMask == 1){
				float3 p0 = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
				float offsetMask = (distance(p0, _WorldSpaceCameraPos) - _TessDistMin) / (_TessDistMax - _TessDistMin);
				o.offsetMask = pow(saturate(saturate(1-offsetMask)), 3);
			}
		#endif
	#endif

	#if NOISE_TEXTURE_ENABLED
		float2 noiseUV = ScaleUV(v.uv, _NoiseTexScale, _NoiseTexScroll*10);
		float noiseWaveTex = tex2Dlod(_NoiseTex, float4(noiseUV,0,lerp(0,8,_NoiseTexBlur)));
		float noiseWave = Remap(noiseWaveTex, 0, 1, _VertRemapMin, _VertRemapMax);
		float offsetWave = noiseWave * _WaveHeight * 0.1;
		o.wave = _Offset * offsetWave;
		#ifdef TESSELLATION_VARIANT
			o.wave *= o.offsetMask;
		#endif
		v.vertex.xyz += o.wave;
		o.wave.y = (o.wave.y + 1) * 0.5;
	#elif GERSTNER_ENABLED
		float turb = 0;
		float3 wave0 = 0;
		float3 wave1 = 0;
		float3 wave2 = 0;
		if (_RecalculateNormals == 1){
			o.tangent = float3(1,0,0);
			o.binormal = float3(0,0,1);
		}
		// if (_Turbulence > 0){
		// 	_Turbulence *= _WaveStrengthGlobal;
		// 	_TurbulenceSpeed *= _WaveSpeedGlobal;
		// 	_TurbulenceScale *= _WaveScaleGlobal;
		// 	turb = Perlin3D(float3(v.uv.xy*_TurbulenceScale, _Time.y*_TurbulenceSpeed))+1;
		// 	turb *= _Turbulence*0.1;
		// }
		if (_WaveStrength0 > 0){
			_WaveStrength0 *= _WaveStrengthGlobal;
			_WaveSpeed0 *= _WaveSpeedGlobal;
			_WaveScale0 *= _WaveScaleGlobal;
			float4 waveProperties0 = float4(0,1, _WaveStrength0 + turb, _WaveScale0);
			wave0 = GerstnerWave(waveProperties0, v.vertex.xyz, _WaveSpeed0, _WaveDirection0, o.tangent, o.binormal);
		}
		if (_WaveStrength1 > 0){
			_WaveStrength1 *= _WaveStrengthGlobal;
			_WaveSpeed1 *= _WaveSpeedGlobal;
			_WaveScale1 *= _WaveScaleGlobal;
			float4 waveProperties1 = float4(0,1, _WaveStrength1 + turb, _WaveScale1);
			wave1 = GerstnerWave(waveProperties1, v.vertex.xyz, _WaveSpeed1, _WaveDirection1, o.tangent, o.binormal);
		}
		if (_WaveStrength2 > 0){
			_WaveStrength2 *= _WaveStrengthGlobal;
			_WaveSpeed2 *= _WaveSpeedGlobal;
			_WaveScale2 *= _WaveScaleGlobal;
			float4 waveProperties2 = float4(0,1, _WaveStrength2 + turb, _WaveScale2);
			wave2 = GerstnerWave(waveProperties2, v.vertex.xyz, _WaveSpeed2, _WaveDirection2, o.tangent, o.binormal);
		}
		o.wave = wave0 + wave1 + wave2;
		#ifdef TESSELLATION_VARIANT
			o.wave *= o.offsetMask;
		#endif
		v.vertex.xyz += o.wave;
		o.wave.y = (o.wave.y + 1) * 0.5;
		if (_RecalculateNormals == 1){
			o.normal = normalize(cross(o.binormal, o.tangent));
			o.cNormal = o.normal;
		}
	#elif VORONOI_ENABLED
		float2 voronoiUV = (v.vertex.xz * _VoronoiScale) + (_Time.y * _VoronoiScroll);
		float voronoiSpeed = _Time.y * _VoronoiSpeed;
		float voronoi = Voronoi2D(voronoiUV, voronoiSpeed)*0.1;
		o.wave = _VoronoiOffset * voronoi * _VoronoiWaveHeight;
		#ifdef TESSELLATION_VARIANT
			o.wave *= o.offsetMask;
		#endif
		v.vertex.xyz += o.wave;
		o.wave.y = (o.wave.y + 1) * 0.5;
	#endif

	o.pos = UnityObjectToClipPos(v.vertex);
	#if GERSTNER_ENABLED
		if (_RecalculateNormals != 1){
			o.normal = UnityObjectToWorldNormal(v.normal);
			o.cNormal = o.normal;
			o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0)).xyz);
			o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);
		}
	#else
		o.normal = UnityObjectToWorldNormal(v.normal);
		o.cNormal = o.normal;
		o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0)).xyz);
		o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);
	#endif
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	o.uvGrab = ComputeGrabScreenPos(o.pos);
	o.localPos = v.vertex;
	float2 uvs[] = {v.uv, v.uv1, v.uv2, v.uv3};
	o.uvFlow = uvs[_FlowMapUV].xy;
	o.uv = v.uv;

	o.isInVRMirror = _VRChatMirrorMode == 1;

	// v.tangent.xyz = normalize(v.tangent.xyz);
	// v.normal = normalize(v.normal);
	// float3x3 objectToTangent = float3x3(v.tangent.xyz, (cross(v.normal, v.tangent.xyz) * v.tangent.w), v.normal);
	// o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));

	UNITY_TRANSFER_FOG(o, o.pos);
	#if defined(UNITY_PASS_SHADOWCASTER)
		TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
	#else
		UNITY_TRANSFER_SHADOW(o, v.vu1);
	#endif
	return o;
}

#endif // WATER_VERT_INCLUDED