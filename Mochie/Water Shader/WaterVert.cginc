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

	float2 uvs[] = {v.uv, v.uv1, v.uv2, v.uv3};
	o.uvFlow = uvs[_FlowMapUV].xy;
	o.uv = v.uv;
	v.tangent.w = v.tangent.w * unity_WorldTransformParams.w;
	#if !GERSTNER_ENABLED
		o.normal = UnityObjectToWorldNormal(v.normal);
		o.cNormal = o.normal;
		o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0)).xyz);
		o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);
	#endif

	#ifdef TESSELLATION_VARIANT
		#if NOISE_TEXTURE_ENABLED || GERSTNER_ENABLED || VORONOI_ENABLED || VERT_FLIPBOOK_ENABLED
			o.offsetMask = 1;
			if (_TessellationOffsetMask == 1){
				float3 p0 = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
				float offsetMask = (distance(p0, _WorldSpaceCameraPos) - _TessDistMin) / (_TessDistMax - _TessDistMin);
				o.offsetMask = pow(saturate(saturate(1-offsetMask)), 3);
			}
		#endif
	#endif

	#if VERT_OFFSET_ENABLED
		float4 vertOffsetMaskUV = float4(TRANSFORM_TEX(v.uv, _VertexOffsetMask).xy, 0, 0);
		float vertOffsetMask = ChannelCheck(tex2Dlod(_VertexOffsetMask, vertOffsetMaskUV), _VertexOffsetMaskChannel);
		vertOffsetMask = lerp(1, vertOffsetMask, _VertexOffsetMaskStrength);
	#endif

	#if NOISE_TEXTURE_ENABLED
		float2 noiseUV = ScaleUV(v.uv, _NoiseTexScale, _NoiseTexScroll*10);
		float noiseWaveTex = tex2Dlod(_NoiseTex, float4(noiseUV,0,lerp(0,8,_NoiseTexBlur)));
		float noiseWave = Remap(noiseWaveTex, 0, 1, _VertRemapMin, _VertRemapMax);
		float offsetWave = noiseWave * _WaveHeight * 0.1;
		o.wave = _Offset * offsetWave * vertOffsetMask;
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
		float offsetMask = 1;
		#ifdef TESSELLATION_VARIANT
			offsetMask = o.offsetMask;
		#endif
		offsetMask *= vertOffsetMask;
		float3 wpos = v.vertex; // mul(unity_ObjectToWorld, v.vertex);
		// wpos.y = v.vertex.y;
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
			wave0 = GerstnerWave(waveProperties0, wpos, _WaveSpeed0, _WaveDirection0, o.tangent, o.binormal, offsetMask);
		}
		if (_WaveStrength1 > 0){
			_WaveStrength1 *= _WaveStrengthGlobal;
			_WaveSpeed1 *= _WaveSpeedGlobal;
			_WaveScale1 *= _WaveScaleGlobal;
			float4 waveProperties1 = float4(0,1, _WaveStrength1 + turb, _WaveScale1);
			wave1 = GerstnerWave(waveProperties1, wpos, _WaveSpeed1, _WaveDirection1, o.tangent, o.binormal, offsetMask);
		}
		if (_WaveStrength2 > 0){
			_WaveStrength2 *= _WaveStrengthGlobal;
			_WaveSpeed2 *= _WaveSpeedGlobal;
			_WaveScale2 *= _WaveScaleGlobal;
			float4 waveProperties2 = float4(0,1, _WaveStrength2 + turb, _WaveScale2);
			wave2 = GerstnerWave(waveProperties2, wpos, _WaveSpeed2, _WaveDirection2, o.tangent, o.binormal, offsetMask);
		}
		o.wave = wave0 + wave1 + wave2;
		o.wave *= vertOffsetMask;
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
	#elif VERT_FLIPBOOK_ENABLED
		// Based on https://github.com/Error-mdl/ErrorWater/blob/master/shaders/cginc/water_vert.cginc#L36
		float2 flipbookUV = v.uv * _NormalMapFlipbookScale;
		#if FLOW_ENABLED
			float2 uvFlow = ScaleUV(o.uvFlow, _FlowMapScale, 0);
			float4 flowMap = MOCHIE_SAMPLE_TEX2D_LOD(_FlowMap, uvFlow, 0);
			float blendNoise = flowMap.a;
			if (_BlendNoiseSource == 1){
				float2 uvBlend = ScaleUV(o.uv, _BlendNoiseScale, 0);
				blendNoise = MOCHIE_SAMPLE_TEX2D_SAMPLER_LOD(_BlendNoise, sampler_FlowMap, uvBlend, 0);
			}
			float2 flow = (flowMap.rg * 2 - 1) * _FlowStrength * 0.1;
			float time = _Time.y * _FlowSpeed + blendNoise;
			float3 flowUV0 = FlowUV(flipbookUV, flow, time, 0);
			float3 flowUV1 = FlowUV(flipbookUV, flow, time, 0.5);
			float3 flipbookSample0 = tex2DflipbookSmoothLOD(_VertOffsetFlipbook, sampler_VertOffsetFlipbook, flowUV0, _VertOffsetFlipbookSpeed, 0) * flowUV0.z;
			float3 flipbookSample1 = tex2DflipbookSmoothLOD(_VertOffsetFlipbook, sampler_VertOffsetFlipbook, flowUV1, _VertOffsetFlipbookSpeed, 0) * flowUV1.z;
			o.wave = flipbookSample0 + flipbookSample1;
		#else
			o.wave = tex2DflipbookSmoothLOD(_VertOffsetFlipbook, sampler_VertOffsetFlipbook, flipbookUV, _VertOffsetFlipbookSpeed, 0);
		#endif
		o.wave *= vertOffsetMask;
		#ifdef TESSELLATION_VARIANT
			o.wave *= o.offsetMask;
		#endif
		float3 bitangent = cross(v.normal, v.tangent.xyz) * v.tangent.w;
		v.vertex.xyz += (o.wave.z - 0.5) * v.normal * _VertOffsetFlipbookStrength;
		v.vertex.xyz -= (o.wave.x * normalize(v.tangent.xyz) / _VertOffsetFlipbookScale.x + o.wave.y * normalize(o.binormal) / _VertOffsetFlipbookScale.y) * _VertOffsetFlipbookStrength;
		o.wave.y = 0;
	#endif
	
	o.pos = UnityObjectToClipPos(v.vertex);
	#if GERSTNER_ENABLED
		if (_RecalculateNormals != 1){
			o.normal = UnityObjectToWorldNormal(v.normal);
			o.cNormal = o.normal;
			o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0)).xyz);
			o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);
		}
	#endif
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	o.uvGrab = ComputeGrabScreenPos(o.pos);
	o.reflUV = ComputeNonStereoScreenPos(o.pos);
	#if defined(LIGHTMAP_ON)
		o.lightmapUV = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
	#endif
	o.localPos = v.vertex;

	o.isInVRMirror = _VRChatMirrorMode == 1;

	// v.tangent.xyz = normalize(v.tangent.xyz);
	// v.normal = normalize(v.normal);
	// float3x3 objectToTangent = float3x3(v.tangent.xyz, (cross(v.normal, v.tangent.xyz) * v.tangent.w), v.normal);
	// o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));

	UNITY_TRANSFER_FOG(o, o.pos);
	#if defined(UNITY_PASS_SHADOWCASTER)
		TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
	#else
		UNITY_TRANSFER_SHADOW(o, v.uv1);
	#endif
	return o;
}

#endif // WATER_VERT_INCLUDED