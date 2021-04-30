#ifndef MOCHIE_STANDARD_CORE_FORWARD_INCLUDED
#define MOCHIE_STANDARD_CORE_FORWARD_INCLUDED

#include "UnityStandardConfig.cginc"

#include "MochieStandardCore.cginc"
VertexOutputForwardBase vertBase (VertexInput v) { return vertForwardBase(v); }
VertexOutputForwardAdd vertAdd (VertexInput v) { return vertForwardAdd(v); }
half4 fragBase (VertexOutputForwardBase i) : SV_Target { 
	return fragForwardBaseInternal(i); 
}
half4 fragAdd (VertexOutputForwardAdd i) : SV_Target { 
	return fragForwardAddInternal(i); 
}

#endif // UNITY_STANDARD_CORE_FORWARD_INCLUDED
