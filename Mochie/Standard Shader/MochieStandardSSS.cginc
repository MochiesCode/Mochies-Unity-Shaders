#ifndef MOCHIE_STANDARD_SSS_INCLUDED
#define MOCHIE_STANDARD_SSS_INCLUDED

float3 GetSubsurfaceLight(
	float3 lightColor, float3 lightDirection, float3 normalDirection, float3 viewDirection, 
    float attenuation, float3 thickness, float3 indirectLight, float3 subsurfaceColor
){
    float3 vLTLight = lightDirection + normalDirection * _ScatterDist; // Distortion
    float3 fLTDot = pow(saturate(dot(viewDirection, -vLTLight)), _ScatterPow) * _ScatterIntensity * 1.0/UNITY_PI; 
    
    return lerp(1, attenuation, float(any(_WorldSpaceLightPos0.xyz))) 
                * (fLTDot + _ScatterAmbient) * thickness
                * (lightColor + indirectLight) * subsurfaceColor;
                
}

float3 GeneralWrapSH(float fA){
    // Normalization factor for our model.
    float norm = 0.5 * (2 + fA) / (1 + fA);
    float4 t = float4(2 * (fA + 1), fA + 2, fA + 3, fA + 4);
    return norm * float3(t.x / t.y, 2 * t.x / (t.y * t.z),
        t.x * (fA * fA - t.x + 5) / (t.y * t.z * t.w));
}

float3 ShadeSH9_wrappedCorrect(float3 normal, float3 conv){
    const float3 cosconv_inv = float3(1, 1.5, 4); // Inverse of the pre-applied cosine convolution
    float3 x0, x1, x2;
    conv *= cosconv_inv; // Undo pre-applied cosine convolution
    //conv *= _Bands.xyz; // debugging

    // Constant (L0)
    x0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
    // Remove the constant part from L2 and add it back with correct convolution
    float3 otherband = float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
    x0 = (x0 + otherband) * conv.x - otherband * conv.z;

    // Linear (L1) polynomial terms
    x1.r = (dot(unity_SHAr.xyz, normal));
    x1.g = (dot(unity_SHAg.xyz, normal));
    x1.b = (dot(unity_SHAb.xyz, normal));

    // 4 of the quadratic (L2) polynomials
    float4 vB = normal.xyzz * normal.yzzx;
    x2.r = dot(unity_SHBr, vB);
    x2.g = dot(unity_SHBg, vB);
    x2.b = dot(unity_SHBb, vB);

    // Final (5th) quadratic (L2) polynomial
    float vC = normal.x * normal.x - normal.y * normal.y;
    x2 += unity_SHC.rgb * vC;

    return x0 + x1 * conv.y + x2 * conv.z;
}

#endif // UNITY_STANDARD_SSS_INCLUDED
