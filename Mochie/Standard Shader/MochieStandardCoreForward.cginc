#ifndef MOCHIE_STANDARD_CORE_FORWARD_INCLUDED
#define MOCHIE_STANDARD_CORE_FORWARD_INCLUDED

#include "UnityStandardConfig.cginc"

#include "MochieStandardCore.cginc"
VertexOutputForwardBase vertBase (VertexInput v) { return vertForwardBase(v); }
VertexOutputForwardAdd vertAdd (VertexInput v) { return vertForwardAdd(v); }
half4 fragBase (VertexOutputForwardBase i) : SV_Target {
	float4 finalCol = fragForwardBaseInternal(i);
	finalCol.rgb = Filtering(finalCol.rgb, _HuePost, _SaturationPost, _BrightnessPost, _ContrastPost);
	return finalCol;
}
half4 fragAdd (VertexOutputForwardAdd i) : SV_Target { 
	float4 finalCol = fragForwardAddInternal(i); 
	finalCol.rgb = Filtering(finalCol.rgb, _HuePost, _SaturationPost, _BrightnessPost, _ContrastPost);
	return finalCol;
}

#endif // UNITY_STANDARD_CORE_FORWARD_INCLUDED
