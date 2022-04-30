
float4 RGBSubPixelConvert(sampler2D mainTex, sampler2D rgbTex, float2 uv0, float2 uv1, float3 viewDir, float3 worldNormal, inout float alpha)
{
	//our emission map
	uv0 = round(uv0 * _RGBSubPixelTex_ST.xy) / _RGBSubPixelTex_ST.xy;
	#ifdef _FLIPBOOK_MODE
		#ifndef SHADER_TARGET_SURFACE_ANALYSIS
			float width, height, elements;
			_Flipbook.GetDimensions(width, height, elements);
			uint index = frac(_Time.y*_FPS*(1/elements))*elements;
			float3 flipbookUV = float3(uv0, index);
			float4 e = UNITY_SAMPLE_TEX2DARRAY_SAMPLER(_Flipbook, _Flipbook, flipbookUV);
		#else
			float4 e = tex2D(mainTex, uv0);
		#endif
	#else
		float4 e = tex2D(mainTex, uv0);
	#endif
	float3 interp = smoothstep(_BoostThreshold,1,e);
	e.rgb = lerp(e.rgb, e.rgb*_BoostAmount, interp);

	//viewing angle for tilt shift
	float3 rawWorldNormal = worldNormal;
	float3 vertexNormal = mul(unity_WorldToObject, float4(rawWorldNormal, 0));
	float4 worldNormals = mul(unity_ObjectToWorld,float4(vertexNormal, 0));
	float VdotN = dot(viewDir, worldNormals);

	//correct for gamma if being used for a VRC Stream script.
	//ONLY on stream panels, not video panels.
	e.rgb = lerp(e.rgb, pow(e.rgb,2.2), _IsAVProInput);

	//do RGB pixels
	uv1 *= _RGBSubPixelTex_ST.xy + _RGBSubPixelTex_ST.zw;
	float3 rgbpixel = tex2D(rgbTex, uv1).rgb;
	alpha = tex2D(rgbTex, uv1).a;

	float backlight = dot(rgbpixel, 0.5);
	backlight *= 0.005;
	backlight = lerp(0, backlight, _Backlight);

	//sample the main textures color channels to derive how strong any given subpixel should be, 
	//and then adjust the intensity of the subpixel by the color correction values
	float pixelR = ((1 + rgbpixel.r) * rgbpixel.r) * e.r;
	float pixelG = ((1 + rgbpixel.g) * rgbpixel.g) * e.g;
	float pixelB = ((1 + rgbpixel.b) * rgbpixel.b) * e.b;

	//add the backlight, if there is any, and ensure that it only happens within
	//the area of a subpixel. We don't want lightleak through the black areas of the texture.
	pixelR += backlight * rgbpixel.r;
	pixelG += backlight * rgbpixel.g;
	pixelB += backlight * rgbpixel.b;

	//return all of our pixel values in a float3
	float3 pixelValue = float3(pixelR, pixelG, pixelB);

	//do the color shift at large viewing angles, shifting to whatever color we want, based on 
	//1 - the dot product of the viewdir and the normals, multipled, to make the dot larger.
	//i'm sure there's a more accurate way to handle this.
	float3 screenCol = lerp(pixelValue * _EmissionIntensity, 0, max(0, (1-VdotN * 1.2)));

	//if we're in the meta pass, just pass through the final color as the emission texture * the emission scale.
	//this ensures we don't have anything else effecting our lightmap emissions (such as the tilt shifting),
	//otherwise, we pass through the final color from above
	#ifdef UNITY_PASS_META
		float3 finalCol = e * _LightmapEmissionScale;
	#else
		float3 finalCol = screenCol;
	#endif

	//Return it all as a float4 with an alpha of 1
	return float4(finalCol.rgb,1);
}