#ifndef MOCHIE_STANDARD_CORE_INCLUDED
#define MOCHIE_STANDARD_CORE_INCLUDED

#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "MochieStandardInput.cginc"
#include "MochieStandardPBSLighting.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityGBuffer.cginc"
#include "MochieStandardSSS.cginc"
#include "MochieStandardBRDF.cginc"

#include "AutoLight.cginc"

static float3 TangentNormal = float3(0,0,1);

//-------------------------------------------------------------------------------------
// counterpart for NormalizePerPixelNormal
// skips normalization per-vertex and expects normalization to happen per-pixel
half3 NormalizePerVertexNormal (float3 n) // takes float to avoid overflow
{
    #if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
        return normalize(n);
    #else
        return n; // will normalize per-pixel instead
    #endif
}

float3 NormalizePerPixelNormal (float3 n)
{
    #if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
        return n;
    #else
        return normalize((float3)n); // takes float to avoid overflow
    #endif
}

// MOCHIE ADDITIONS
//-------------------------------------------------------------------------------------

half3 Mochie_GlossyEnvironment (UNITY_ARGS_TEXCUBE(tex), half4 hdr, Unity_GlossyEnvironmentData glossIn)
{
    half perceptualRoughness = glossIn.roughness /* perceptualRoughness */ ;
    perceptualRoughness = perceptualRoughness*(1.7 - 0.7*perceptualRoughness);
    half mip = perceptualRoughnessToMipmapLevel(perceptualRoughness);
    half3 R = glossIn.reflUVW;
    half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(tex, R, mip);
    return DecodeHDR(rgbm, hdr);
}

half3 Mochie_GlossyEnvironment (UNITY_ARGS_TEXCUBE(tex), half4 hdr, Unity_GlossyEnvironmentData glossIn, float3 reflUVW)
{
    half perceptualRoughness = glossIn.roughness /* perceptualRoughness */ ;
    perceptualRoughness = perceptualRoughness*(1.7 - 0.7*perceptualRoughness);
    half mip = perceptualRoughnessToMipmapLevel(perceptualRoughness);
    half3 R = reflUVW;
    half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(tex, R, mip);
    return DecodeHDR(rgbm, hdr);
}

#if MIRROR_ENABLED
half3 Mirror_GlossyEnvironment(Unity_GlossyEnvironmentData glossIn, float4 reflUV){
    half perceptualRoughness = glossIn.roughness /* perceptualRoughness */ ;
    perceptualRoughness = perceptualRoughness*(1.7 - 0.7*perceptualRoughness);
    half mip = perceptualRoughnessToMipmapLevel(perceptualRoughness);
    float2 uv = reflUV.xy / (reflUV.w + 0.00000001);
    uv += TangentNormal;
    float4 uvMip = float4(uv, 0, mip * 6);
    half3 refl = unity_StereoEyeIndex == 0 ? tex2Dlod(_ReflectionTex0, uvMip) : tex2Dlod(_ReflectionTex1, uvMip);
    return refl;
}
#endif

inline half3 MochieGI_IndirectSpecular(UnityGIInput data, half3 occlusion, Unity_GlossyEnvironmentData glossIn, float3 normal, float4 reflUV)
{
    half3 specular;
	half3 originalReflUVW = glossIn.reflUVW;
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
        glossIn.reflUVW = BoxProjectedCubemapDirection(originalReflUVW, data.worldPos, data.probePosition[0], data.boxMin[0], data.boxMax[0]);
    #endif
	if (_GSAA == 1){
		glossIn.roughness = GSAARoughness(normal, glossIn.roughness);
	}
    #ifdef _GLOSSYREFLECTIONS_OFF
        specular = unity_IndirectSpecColor.rgb;
    #else
        #if MIRROR_ENABLED
            specular = Mirror_GlossyEnvironment(glossIn, reflUV);
        #else
            if (_ReflCubeOverrideToggle == 1){
                half3 env0 = Mochie_GlossyEnvironment(UNITY_PASS_TEXCUBE(_ReflCubeOverride), _ReflCubeOverride_HDR, glossIn, originalReflUVW);
                specular = env0;
            }
            else {
                half3 env0 = Mochie_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), data.probeHDR[0], glossIn);
                #ifdef UNITY_SPECCUBE_BLENDING
                    const float kBlendFactor = 0.99999;
                    float blendLerp = data.boxMin[0].w;
                    UNITY_BRANCH
                    if (blendLerp < kBlendFactor){
                        #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                            glossIn.reflUVW = BoxProjectedCubemapDirection(originalReflUVW, data.worldPos, data.probePosition[1], data.boxMin[1], data.boxMax[1]);
                        #endif
                        half3 env1 = Mochie_GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1,unity_SpecCube0), data.probeHDR[1], glossIn);
                        specular = lerp(env1, env0, blendLerp);
                    }
                    else {
                        specular = env0;
                    }
                #else
                    specular = env0;
                #endif
            }
            if (_ReflCubeToggle == 1){
                half3 env2 = Mochie_GlossyEnvironment(UNITY_PASS_TEXCUBE(_ReflCube), _ReflCube_HDR, glossIn, originalReflUVW);
                float interpolant = (specular.r + specular.g + specular.b)/3.0;
                specular = lerp(env2, specular, smoothstep(0, _CubeThreshold * 0.01, interpolant));
            }
        #endif
    #endif
    return specular * occlusion;
}

inline UnityGI MochieGlobalIllumination (UnityGIInput data, half3 occlusion, half3 normalWorld, Unity_GlossyEnvironmentData glossIn, float4 reflUV)
{
    UnityGI o_gi = UnityGI_Base(data, occlusion, normalWorld);
    o_gi.indirect.specular = MochieGI_IndirectSpecular(data, occlusion, glossIn, normalWorld, reflUV);
	// #if SUBSURFACE_ENABLED
	// 	float3 sh_conv = GeneralWrapSH(0.5);
	// 	o_gi.indirect.diffuse = ShadeSH9_wrappedCorrect(normalWorld, sh_conv);
	// #endif
    return o_gi;
}

//-------------------------------------------------------------------------------------

UnityLight MainLight ()
{
    UnityLight l;
    l.color = _LightColor0.rgb;
    l.dir = _WorldSpaceLightPos0.xyz;
    return l;
}

UnityLight AdditiveLight (half3 lightDir, half atten)
{
    UnityLight l;

    l.color = _LightColor0.rgb;
    l.dir = lightDir;
    #ifndef USING_DIRECTIONAL_LIGHT
        l.dir = NormalizePerPixelNormal(l.dir);
    #endif

    // shadow the light
    l.color *= atten;
    return l;
}

UnityLight DummyLight ()
{
    UnityLight l;
    l.color = 0;
    l.dir = half3 (0,1,0);
    return l;
}

UnityIndirect ZeroIndirect ()
{
    UnityIndirect ind;
    ind.diffuse = 0;
    ind.specular = 0;
    return ind;
}

//-------------------------------------------------------------------------------------
// Common fragment setup

// deprecated
half3 WorldNormal(half4 tan2world[3])
{
    return normalize(tan2world[2].xyz);
}

// deprecated
#ifdef _TANGENT_TO_WORLD
    half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
    {
        half3 t = tan2world[0].xyz;
        half3 b = tan2world[1].xyz;
        half3 n = tan2world[2].xyz;

    #if UNITY_TANGENT_ORTHONORMALIZE
        n = NormalizePerPixelNormal(n);

        // ortho-normalize Tangent
        t = normalize (t - n * dot(t, n));

        // recalculate Binormal
        half3 newB = cross(n, t);
        b = newB * sign (dot (newB, b));
    #endif

        return half3x3(t, b, n);
    }
#else
    half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
    {
        return half3x3(0,0,0,0,0,0,0,0,0);
    }
#endif

float3 PerPixelWorldNormal(float4 i_tex, float4 raincoords, float4 tangentToWorld[3], SampleData sd)
{
    #ifdef _NORMALMAP
        half3 tangent = tangentToWorld[0].xyz;
        half3 binormal = tangentToWorld[1].xyz;
        half3 normal = tangentToWorld[2].xyz;
        #if UNITY_TANGENT_ORTHONORMALIZE
            normal = NormalizePerPixelNormal(normal);

            // ortho-normalize Tangent
            tangent = normalize (tangent - normal * dot(tangent, normal));

            // recalculate Binormal
            half3 newB = cross(normal, tangent);
            binormal = newB * sign (dot (newB, binormal));
        #endif
        
        half3 normalTangent = NormalInTangentSpace(i_tex, raincoords, sd);
        TangentNormal = normalTangent;
        float3 normalWorld = NormalizePerPixelNormal(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z);
    #else
        float3 normalWorld = normalize(tangentToWorld[2].xyz);
        if (_RainToggle == 1){
            float mask = MOCHIE_SAMPLE_TEX2D_SAMPLER(_RainMask, sampler_MainTex, raincoords.zw);
            float3 rippleNormal = GetRipplesNormal(raincoords.xy, _RippleScale, _RippleStr*mask, _RippleSpeed);
            normalWorld = BlendNormals(normalWorld, rippleNormal);
            TangentNormal = BlendNormals(TangentNormal, rippleNormal);
        }
    #endif

    return normalWorld;
}

#if defined(_PARALLAXMAP) || defined(BAKERY_LMSPEC) && defined(BAKERY_RNM)
    #define IN_VIEWDIR4PARALLAX(i) NormalizePerPixelNormal(half3(i.tangentToWorldAndPackedData[0].w,i.tangentToWorldAndPackedData[1].w,i.tangentToWorldAndPackedData[2].w))
    #define IN_VIEWDIR4PARALLAX_FWDADD(i) NormalizePerPixelNormal(i.viewDirForParallax.xyz)
#else
    #define IN_VIEWDIR4PARALLAX(i) half3(0,0,0)
    #define IN_VIEWDIR4PARALLAX_FWDADD(i) half3(0,0,0)
#endif

#if UNITY_REQUIRE_FRAG_WORLDPOS
    #if UNITY_PACK_WORLDPOS_WITH_TANGENT
        #define IN_WORLDPOS(i) half3(i.tangentToWorldAndPackedData[0].w,i.tangentToWorldAndPackedData[1].w,i.tangentToWorldAndPackedData[2].w)
    #else
        #define IN_WORLDPOS(i) i.posWorld
    #endif
    #define IN_WORLDPOS_FWDADD(i) i.posWorld
#else
    #define IN_WORLDPOS(i) half3(0,0,0)
    #define IN_WORLDPOS_FWDADD(i) half3(0,0,0)
#endif

#define IN_LIGHTDIR_FWDADD(i) half3(i.tangentToWorldAndLightDir[0].w, i.tangentToWorldAndLightDir[1].w, i.tangentToWorldAndLightDir[2].w)


struct FragmentCommonData
{
    half3 diffColor, specColor;
    half oneMinusReflectivity, smoothness;
    float3 normalWorld;
    float3 eyeVec;
    half alpha;
    float3 posWorld;
	float metallic;
	float thickness;
	float3 subsurfaceColor;
	#if UNITY_STANDARD_SIMPLE
		half3 reflUVW;
	#endif
	#if UNITY_STANDARD_SIMPLE
		half3 tangentSpaceNormal;
	#endif
};

inline FragmentCommonData RoughnessSetup(float4 i_tex, float2 detailMaskCoords, SampleData sd)
{
    half2 metallicGloss = MetallicRough(i_tex, sd);
    half metallic = metallicGloss.x;
    half smoothness = metallicGloss.y; // this is 1 minus the square root of real roughness m.

    half oneMinusReflectivity;
    half3 specColor;
    half3 diffColor = DiffuseAndSpecularFromMetallic(Albedo(i_tex, detailMaskCoords, sd), metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    FragmentCommonData o = (FragmentCommonData)0;
    o.diffColor = diffColor;
    o.specColor = specColor;
    o.oneMinusReflectivity = oneMinusReflectivity;
    o.smoothness = smoothness;
	o.metallic = metallic;
    return o;
}


float3 CalculateTangentViewDir(inout float3 tangentViewDir){
    tangentViewDir = Unity_SafeNormalize(tangentViewDir);
    tangentViewDir.xy /= (tangentViewDir.z + 0.42);
	return tangentViewDir;
}

// parallax transformed texcoord is used to sample occlusion
inline FragmentCommonData FragmentSetup (inout float4 i_tex, float4 i_tex2, float4 i_tex3, float4 i_tex4, float3 i_eyeVec, half3 i_viewDirForParallax, float4 tangentToWorld[3], float3 i_posWorld, bool isFrontFace, SampleData sd)
{
	
    i_tex = Parallax(i_tex, CalculateTangentViewDir(i_viewDirForParallax), uvOffset, isFrontFace);

    #ifdef _ALPHAMASK_ON
        half alpha = Alpha(i_tex2, sd);
    #else
        half alpha = Alpha(i_tex.xy, sd);
    #endif
    #if defined(_ALPHATEST_ON)
        clip (alpha - _Cutoff);
    #endif

    FragmentCommonData o = UNITY_SETUP_BRDF_INPUT (i_tex, i_tex4.zw, sd);
    o.normalWorld = PerPixelWorldNormal(i_tex, i_tex3, tangentToWorld, sd);
    o.eyeVec = NormalizePerPixelNormal(i_eyeVec);
    o.posWorld = i_posWorld;

    // NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
    o.diffColor = PreMultiplyAlpha (o.diffColor, alpha, o.oneMinusReflectivity, /*out*/ o.alpha);

	o.thickness = SampleTexture(_ThicknessMap, i_tex, sd);
	o.thickness = pow(1-o.thickness, _ThicknessMapPower);
	o.subsurfaceColor = _ScatterCol * lerp(1, o.diffColor, _ScatterAlbedoTint);

    return o;
}

inline UnityGI FragmentGI (FragmentCommonData s, half3 occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light, bool reflections, float4 reflUV)
{
    UnityGIInput d;
    d.light = light;
    d.worldPos = s.posWorld;
    d.worldViewDir = -s.eyeVec;
    d.atten = atten;
    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        d.ambient = 0;
        d.lightmapUV = i_ambientOrLightmapUV;
    #else
        d.ambient = i_ambientOrLightmapUV.rgb;
        d.lightmapUV = 0;
    #endif

    d.probeHDR[0] = unity_SpecCube0_HDR;
    d.probeHDR[1] = unity_SpecCube1_HDR;
    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
      d.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
    #endif
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
      d.boxMax[0] = unity_SpecCube0_BoxMax;
      d.probePosition[0] = unity_SpecCube0_ProbePosition;
      d.boxMax[1] = unity_SpecCube1_BoxMax;
      d.boxMin[1] = unity_SpecCube1_BoxMin;
      d.probePosition[1] = unity_SpecCube1_ProbePosition;
    #endif

    if(reflections)
    {
        Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.smoothness, -s.eyeVec, s.normalWorld, s.specColor);
        // Replace the reflUVW if it has been compute in Vertex shader. Note: the compiler will optimize the calcul in UnityGlossyEnvironmentSetup itself
        #if UNITY_STANDARD_SIMPLE
            g.reflUVW = s.reflUVW;
        #endif

        return MochieGlobalIllumination (d, occlusion, s.normalWorld, g, reflUV);
    }
    else
    {
        return UnityGlobalIllumination (d, occlusion, s.normalWorld);
    }
}

inline UnityGI FragmentGI (FragmentCommonData s, half3 occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light, float4 reflUV)
{
    return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, true, reflUV);
}


//-------------------------------------------------------------------------------------
half4 OutputForward (half4 output, half alphaFromSurface)
{
    #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
        output.a = alphaFromSurface;
    #else
        UNITY_OPAQUE_ALPHA(output.a);
    #endif
    return output;
}

inline half4 VertexGIForward(VertexInput v, float3 posWorld, half3 normalWorld)
{
    half4 ambientOrLightmapUV = 0;
    // Static lightmaps
    #ifdef LIGHTMAP_ON
        ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        ambientOrLightmapUV.zw = 0;
    // Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
    #elif UNITY_SHOULD_SAMPLE_SH
        #ifdef VERTEXLIGHT_ON
            // Approximated illumination from non-important point lights
            ambientOrLightmapUV.rgb = Shade4PointLights (
                unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                unity_4LightAtten0, posWorld, normalWorld);
        #endif

        ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, ambientOrLightmapUV.rgb);
    #endif

    #ifdef DYNAMICLIGHTMAP_ON
        ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif

    return ambientOrLightmapUV;
}

// ------------------------------------------------------------------
//  Base forward pass (directional light, emission, lightmaps, ...)

struct VertexOutputForwardBase
{
    UNITY_POSITION(pos);
    float4 tex                            : TEXCOORD0;
    float4 eyeVec                         : TEXCOORD1;    // eyeVec.xyz | fogCoord
    float4 tangentToWorldAndPackedData[3] : TEXCOORD2;    // [3x3:tangentToWorld | 1x3:viewDirForParallax or worldPos]
    half4 ambientOrLightmapUV             : TEXCOORD5;    // SH or Lightmap UV
	float4 tex1                           : TEXCOORD6;
	float4 localPos                       : TEXCOORD7;
    UNITY_LIGHTING_COORDS(8,9)

	#if UNITY_REQUIRE_FRAG_WORLDPOS && !UNITY_PACK_WORLDPOS_WITH_TANGENT
		float3 posWorld                   : TEXCOORD10;
	#endif
	#if SSR_ENABLED
		float4 screenPos                  : TEXCOORD11;
		float3 raycast                    : TEXCOORD12;
		float3 objPos                     : TEXCOORD13;
	#endif
    float4 tex2                           : TEXCOORD14;
    float4 rawUV                          : TEXCOORD15;
    float4 refl                           : TEXCOORD16;
    float4 tex3                           : TEXCOORD17;
    float4 tex4                           : TEXCOORD18;
	float4 color                          : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutputForwardBase vertForwardBase (VertexInput v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutputForwardBase o;
    UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
    #if UNITY_REQUIRE_FRAG_WORLDPOS
        #if UNITY_PACK_WORLDPOS_WITH_TANGENT
            o.tangentToWorldAndPackedData[0].w = posWorld.x;
            o.tangentToWorldAndPackedData[1].w = posWorld.y;
            o.tangentToWorldAndPackedData[2].w = posWorld.z;
        #else
            o.posWorld = posWorld.xyz;
        #endif
    #endif
    o.pos = UnityObjectToClipPos(v.vertex);
	o.localPos = v.vertex;
	o.color = v.color;
    o.rawUV.xy = v.uv0;
    o.rawUV.zw = v.uv1;
    TexCoords(v, o.tex, o.tex1, o.tex2, o.tex3, o.tex4);
    o.eyeVec.xyz = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
    float3 normalWorld = UnityObjectToWorldNormal(v.normal);
    #ifdef _TANGENT_TO_WORLD
        float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

        float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
        o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
        o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
        o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
    #else
        o.tangentToWorldAndPackedData[0].xyz = 0;
        o.tangentToWorldAndPackedData[1].xyz = 0;
        o.tangentToWorldAndPackedData[2].xyz = normalWorld;
    #endif

    //We need this for shadow receving
    UNITY_TRANSFER_LIGHTING(o, v.uv1);

    o.ambientOrLightmapUV = VertexGIForward(v, posWorld, normalWorld);

    #if defined(_PARALLAXMAP) || defined(BAKERY_LMSPEC) && defined(BAKERY_RNM)
        TANGENT_SPACE_ROTATION;
        half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
        o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
        o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
        o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
    #endif

	#if SSR_ENABLED
		o.screenPos = ComputeGrabScreenPos(o.pos);
	#endif
 
    o.refl = ComputeNonStereoScreenPos(o.pos);

    UNITY_TRANSFER_FOG_COMBINED_WITH_EYE_VEC(o,o.pos);
    return o;
}

SampleData SampleDataSetup(VertexOutputForwardBase i){
	SampleData sd = (SampleData)0;
	sd.localPos = i.localPos;
	sd.normal = i.tangentToWorldAndPackedData[2].xyz;
	sd.scaleTransform = _MainTex_ST;
	return sd;
}

float shEvaluateDiffuseL1Geomerics(float L0, float3 L1, float3 n)
{
    // average energy
    float R0 = L0;
    
    // avg direction of incoming light
    float3 R1 = 0.5f * L1;
    
    // directional brightness
    float lenR1 = length(R1);
    
    // linear angle between normal and direction 0-1
    //float q = 0.5f * (1.0f + dot(R1 / lenR1, n));
    //float q = dot(R1 / lenR1, n) * 0.5 + 0.5;
    float q = dot(normalize(R1), n) * 0.5 + 0.5;
    q = saturate(q); // Thanks to ScruffyRuffles for the bug identity.
    
    // power for q
    // lerps from 1 (linear) to 3 (cubic) based on directionality
    float p = 1.0f + 2.0f * lenR1 / R0;
    
    // dynamic range constant
    // should vary between 4 (highly directional) and 0 (ambient)
    float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);
    
    return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
}

#ifdef BAKERY_MONOSH
void BakeryMonoSH(inout float3 diffuseColor, inout float3 specularColor, float2 lmUV, float3 normalWorld, float3 viewDir, half roughness)
{
    float3 L0 = diffuseColor;

    float3 dominantDir = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, lmUV).xyz;

    float3 nL1 = dominantDir * 2 - 1;
    float3 L1x = nL1.x * L0 * 2;
    float3 L1y = nL1.y * L0 * 2;
    float3 L1z = nL1.z * L0 * 2;
    float3 sh;
#ifdef BAKERY_SHNONLINEAR
    float lumaL0 = dot(L0, 1);
    float lumaL1x = dot(L1x, 1);
    float lumaL1y = dot(L1y, 1);
    float lumaL1z = dot(L1z, 1);
    float lumaSH = shEvaluateDiffuseL1Geomerics(lumaL0, float3(lumaL1x, lumaL1y, lumaL1z), normalWorld);

    sh = L0 + normalWorld.x * L1x + normalWorld.y * L1y + normalWorld.z * L1z;
    float regularLumaSH = dot(sh, 1);
    //sh *= regularLumaSH < 0.001 ? 1 : (lumaSH / regularLumaSH);
    sh *= lerp(1, lumaSH / regularLumaSH, saturate(regularLumaSH*16));

    //sh.r = shEvaluateDiffuseL1Geomerics(L0.r, float3(L1x.r, L1y.r, L1z.r), normalWorld);
    //sh.g = shEvaluateDiffuseL1Geomerics(L0.g, float3(L1x.g, L1y.g, L1z.g), normalWorld);
    //sh.b = shEvaluateDiffuseL1Geomerics(L0.b, float3(L1x.b, L1y.b, L1z.b), normalWorld);

#else
    sh = L0 + normalWorld.x * L1x + normalWorld.y * L1y + normalWorld.z * L1z;
#endif

    diffuseColor = max(sh, 0.0);


    #ifdef BAKERY_LMSPEC
        dominantDir = nL1;
        float focus = saturate(length(dominantDir));
        half3 halfDir = Unity_SafeNormalize(normalize(dominantDir) - viewDir);
        half nh = saturate(dot(normalWorld, halfDir));
        half spec = GGXTerm(nh, roughness);

        sh = L0 + dominantDir.x * L1x + dominantDir.y * L1y + dominantDir.z * L1z;

        specularColor += max(spec * sh, 0.0) * _SpecularStrength;
    #endif
}
#endif

half4 fragForwardBaseInternal (VertexOutputForwardBase i, bool frontFace)
{
    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

	float2 screenUVs = 0;
	float4 screenPos = 0;
	#if SSR_ENABLED
		screenUVs = i.screenPos.xy / (i.screenPos.w+0.0000000001);
		#if UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
			screenUVs.x *= 2;
		#endif
		screenPos = i.screenPos;
	#endif

	SampleData sd = SampleDataSetup(i);
    FragmentCommonData s = FragmentSetup(i.tex, i.tex2, i.tex3, i.tex4, i.eyeVec.xyz, IN_VIEWDIR4PARALLAX(i), i.tangentToWorldAndPackedData, IN_WORLDPOS(i), frontFace, sd);
    #if AREALIT_ENABLED
        i.tangentToWorldAndPackedData[2].xyz *= frontFace ? +1 : -1;
    #endif
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    half3 occlusion = Occlusion(i.tex, sd);
    float perceptualRoughness = SmoothnessToPerceptualRoughness(s.smoothness);
    if (_GSAA == 1)
        perceptualRoughness = GSAARoughness(s.normalWorld, perceptualRoughness);
    float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    float clampedRoughness = max(roughness, 0.002);

    #if AREALIT_ENABLED
        float4 alOcclusion = tex2D(_AreaLitOcclusion, i.tex4);
        AreaLightFragInput ai;
        ai.pos = s.posWorld;
        ai.normal = s.normalWorld;
        ai.view = -s.eyeVec;
        ai.roughness = roughness * _AreaLitRoughnessMult;
        ai.occlusion = float4(occlusion, 1) * alOcclusion;
        ai.screenPos = i.pos.xy;
        half4 diffTerm, specTerm;
        ShadeAreaLights(ai, diffTerm, specTerm, true, !IsSpecularOff(), IsStereo());
    #endif

    UnityLight mainLight = MainLight();
    UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld);

    UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, mainLight, i.refl);

    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
		#ifdef BAKERY_RNM
            half3 rnm0 = DecodeLightmap(_RNM0.Sample(sampler_RNM0, i.ambientOrLightmapUV));
            half3 rnm1 = DecodeLightmap(_RNM1.Sample(sampler_RNM1, i.ambientOrLightmapUV));
            half3 rnm2 = DecodeLightmap(_RNM2.Sample(sampler_RNM2, i.ambientOrLightmapUV));

            const float3 rnmBasis0 = float3(0.816496580927726f, 0.0f, 0.5773502691896258f);
            const float3 rnmBasis1 = float3(-0.4082482904638631f, 0.7071067811865475f, 0.5773502691896258f);
            const float3 rnmBasis2 = float3(-0.4082482904638631f, -0.7071067811865475f, 0.5773502691896258f);

            gi.indirect.diffuse =    saturate(dot(rnmBasis0, TangentNormal)) * rnm0
							  + saturate(dot(rnmBasis1, TangentNormal)) * rnm1
						      + saturate(dot(rnmBasis2, TangentNormal)) * rnm2;
        #endif

        #ifdef BAKERY_SH
            half3 L0 = gi.indirect.diffuse;
            half3 nL1x = _RNM0.Sample(sampler_RNM0, i.ambientOrLightmapUV) * 2.0 - 1.0;
            half3 nL1y = _RNM1.Sample(sampler_RNM1, i.ambientOrLightmapUV) * 2.0 - 1.0;
            half3 nL1z = _RNM2.Sample(sampler_RNM2, i.ambientOrLightmapUV) * 2.0 - 1.0;
            half3 L1x = nL1x * L0 * 2.0;
            half3 L1y = nL1y * L0 * 2.0;
            half3 L1z = nL1z * L0 * 2.0;

            #ifdef BAKERY_SHNONLINEAR
                float lumaL0 = dot(L0, float(1));
                float lumaL1x = dot(L1x, float(1));
                float lumaL1y = dot(L1y, float(1));
                float lumaL1z = dot(L1z, float(1));
                float lumaSH = shEvaluateDiffuseL1Geomerics(lumaL0, float3(lumaL1x, lumaL1y, lumaL1z), s.normalWorld);

                gi.indirect.diffuse = L0 + s.normalWorld.x * L1x + s.normalWorld.y * L1y + s.normalWorld.z * L1z;
                float regularLumaSH = dot(gi.indirect.diffuse, 1.0);
                gi.indirect.diffuse *= lerp(1.0, lumaSH / regularLumaSH, saturate(regularLumaSH * 16.0));
            #else
                gi.indirect.diffuse = L0 + s.normalWorld.x * L1x + s.normalWorld.y * L1y + s.normalWorld.z * L1z;
            #endif
        #endif

        
        #ifdef BAKERY_MONOSH
            BakeryMonoSH(gi.indirect.diffuse, gi.indirect.specular, i.ambientOrLightmapUV, s.normalWorld, s.eyeVec, clampedRoughness);
        #endif
    #endif


    #if defined(BAKERY_LMSPEC) && defined(UNITY_PASS_FORWARDBASE) && defined(LIGHTMAP_ON)
        const float3 grayscaleVec = float3(0.2125, 0.7154, 0.0721);
        #ifdef BAKERY_RNM
        {
            float3 eyeVecT = - NormalizePerPixelNormal(IN_VIEWDIR4PARALLAX(i));
            float3 dominantDirT = rnmBasis0 * dot(rnm0, grayscaleVec) +
                                  rnmBasis1 * dot(rnm1, grayscaleVec) +
                                  rnmBasis2 * dot(rnm2, grayscaleVec);

            float3 dominantDirTN = normalize(dominantDirT);
            half3 specColor = saturate(dot(rnmBasis0, dominantDirTN)) * rnm0 +
                               saturate(dot(rnmBasis1, dominantDirTN)) * rnm1 +
                               saturate(dot(rnmBasis2, dominantDirTN)) * rnm2;

            half3 halfDir = Unity_SafeNormalize(dominantDirTN - eyeVecT);
            half NoH = saturate(dot(TangentNormal, halfDir));
            half spec = GGXTerm(NoH, clampedRoughness);
            gi.indirect.specular += spec * specColor * _SpecularStrength; 
        }
        #endif

        #ifdef BAKERY_SH
        {
            float3 dominantDir = float3(dot(nL1x, grayscaleVec), dot(nL1y, grayscaleVec), dot(nL1z, grayscaleVec));
            half3 halfDir = Unity_SafeNormalize(normalize(dominantDir) - s.eyeVec.xyz);
            half NoH = saturate(dot(s.normalWorld, halfDir));
            half spec = GGXTerm(NoH, clampedRoughness);
            float3 sh = L0 + dominantDir.x * L1x + dominantDir.y * L1y + dominantDir.z * L1z;
            dominantDir = normalize(dominantDir);
            gi.indirect.specular += max(spec * sh, 0.0) * _SpecularStrength;
        }
        #endif
    #endif
    
	half4 c = MOCHIE_BRDF(
				s.diffColor, s.specColor, s.oneMinusReflectivity, 
				s.smoothness, s.normalWorld, -s.eyeVec, s.posWorld, screenUVs, screenPos,
				s.metallic, s.thickness, s.subsurfaceColor, atten, i.ambientOrLightmapUV, i.color, gi.light, gi.indirect
			);

    c.rgb += Emission(i.tex.xy, i.tex1.zw, sd);

    #if AREALIT_ENABLED
        float3 areaLitColor = s.diffColor * diffTerm + s.specColor * specTerm;
        if (_ReflShadows == 1)
            areaLitColor *= shadowedReflections;
        c.rgb += areaLitColor * _AreaLitStrength;
    #endif

    Rim(s.posWorld, s.normalWorld, c.rgb, i.tex2.zw);

    UNITY_EXTRACT_FOG_FROM_EYE_VEC(i);
    UNITY_APPLY_FOG(_unity_fogCoord, c.rgb);
    return OutputForward (c, s.alpha);
}

half4 fragForwardBase (VertexOutputForwardBase i, bool frontFace) : SV_Target   // backward compatibility (this used to be the fragment entry function)
{
    return fragForwardBaseInternal(i, frontFace);
}

// ------------------------------------------------------------------
//  Additive forward pass (one light per pass)

struct VertexOutputForwardAdd
{
    UNITY_POSITION(pos);
    float4 tex                          : TEXCOORD0;
    float4 eyeVec                       : TEXCOORD1;    // eyeVec.xyz | fogCoord
    float4 tangentToWorldAndLightDir[3] : TEXCOORD2;    // [3x3:tangentToWorld | 1x3:lightDir]
    float3 posWorld                     : TEXCOORD5;
	float4 tex1                         : TEXCOORD6;
	float4 localPos                     : TEXCOORD7;
    UNITY_LIGHTING_COORDS(8,9)
	#if defined(_PARALLAXMAP) || defined(BAKERY_LMSPEC) && defined(BAKERY_RNM)
		half3 viewDirForParallax        : TEXCOORD10;
	#endif
    float4 tex2                         : TEXCOORD14;
    float4 rawUV                        : TEXCOORD15;
    float4 tex3                         : TEXCOORD16;
    float4 tex4                         : TEXCOORD17;
	float4 color                        : COLOR;
    UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutputForwardAdd vertForwardAdd (VertexInput v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutputForwardAdd o;
    UNITY_INITIALIZE_OUTPUT(VertexOutputForwardAdd, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
    o.pos = UnityObjectToClipPos(v.vertex);
	o.localPos = v.vertex;

    o.rawUV.xy = v.uv0;
    o.rawUV.zw = v.uv1;
    TexCoords(v, o.tex, o.tex1, o.tex2, o.tex3, o.tex4);
    o.eyeVec.xyz = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
    o.posWorld = posWorld.xyz;
    float3 normalWorld = UnityObjectToWorldNormal(v.normal);
    #ifdef _TANGENT_TO_WORLD
        float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

        float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
        o.tangentToWorldAndLightDir[0].xyz = tangentToWorld[0];
        o.tangentToWorldAndLightDir[1].xyz = tangentToWorld[1];
        o.tangentToWorldAndLightDir[2].xyz = tangentToWorld[2];
    #else
        o.tangentToWorldAndLightDir[0].xyz = 0;
        o.tangentToWorldAndLightDir[1].xyz = 0;
        o.tangentToWorldAndLightDir[2].xyz = normalWorld;
    #endif
    //We need this for shadow receiving and lighting
    UNITY_TRANSFER_LIGHTING(o, v.uv1);

    float3 lightDir = _WorldSpaceLightPos0.xyz - posWorld.xyz * _WorldSpaceLightPos0.w;
    #ifndef USING_DIRECTIONAL_LIGHT
        lightDir = NormalizePerVertexNormal(lightDir);
    #endif
    o.tangentToWorldAndLightDir[0].w = lightDir.x;
    o.tangentToWorldAndLightDir[1].w = lightDir.y;
    o.tangentToWorldAndLightDir[2].w = lightDir.z;
	o.color = v.color;
    #if defined(_PARALLAXMAP) || defined(BAKERY_LMSPEC) && defined(BAKERY_RNM)
        TANGENT_SPACE_ROTATION;
        o.viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
    #endif

    UNITY_TRANSFER_FOG_COMBINED_WITH_EYE_VEC(o, o.pos);
    return o;
}

SampleData SampleDataSetup(VertexOutputForwardAdd i){
	SampleData sd = (SampleData)0;
	sd.localPos = i.localPos;
	sd.normal = i.tangentToWorldAndLightDir[2].xyz;
	sd.scaleTransform = _MainTex_ST;
	return sd;
}

half4 fragForwardAddInternal (VertexOutputForwardAdd i, bool frontFace)
{
    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	float2 screenUVs = 0;
	float4 screenPos = 0;

	SampleData sd = SampleDataSetup(i);
    FragmentCommonData s = FragmentSetup(i.tex, i.tex2, i.tex3, i.tex4, i.eyeVec.xyz, IN_VIEWDIR4PARALLAX_FWDADD(i), i.tangentToWorldAndLightDir, IN_WORLDPOS_FWDADD(i), frontFace, sd);

    UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld)
    UnityLight light = AdditiveLight (IN_LIGHTDIR_FWDADD(i), atten);
    UnityIndirect noIndirect = ZeroIndirect ();

	half4 c = MOCHIE_BRDF (s.diffColor, s.specColor, s.oneMinusReflectivity, 
				s.smoothness, s.normalWorld, -s.eyeVec, s.posWorld, 0, 0,
				s.metallic, s.thickness, s.subsurfaceColor, atten, 0, i.color, light, noIndirect);
								
    UNITY_EXTRACT_FOG_FROM_EYE_VEC(i);
    UNITY_APPLY_FOG_COLOR(_unity_fogCoord, c.rgb, half4(0,0,0,0)); // fog towards black in additive pass
    return OutputForward (c, s.alpha);
}

half4 fragForwardAdd (VertexOutputForwardAdd i, bool frontFace) : SV_Target     // backward compatibility (this used to be the fragment entry function)
{
    return fragForwardAddInternal(i, frontFace);
}

//
// Old FragmentGI signature. Kept only for backward compatibility and will be removed soon
//

inline UnityGI FragmentGI(
    float3 posWorld,
    half occlusion, half4 i_ambientOrLightmapUV, half atten, half smoothness, half3 normalWorld, half3 eyeVec,
    UnityLight light,
    bool reflections)
{
    // we init only fields actually used
    FragmentCommonData s = (FragmentCommonData)0;
    s.smoothness = smoothness;
    s.normalWorld = normalWorld;
    s.eyeVec = eyeVec;
    s.posWorld = posWorld;
    return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, reflections);
}
inline UnityGI FragmentGI (
    float3 posWorld,
    half occlusion, half4 i_ambientOrLightmapUV, half atten, half smoothness, half3 normalWorld, half3 eyeVec,
    UnityLight light)
{
    return FragmentGI (posWorld, occlusion, i_ambientOrLightmapUV, atten, smoothness, normalWorld, eyeVec, light, true);
}

#endif // UNITY_STANDARD_CORE_INCLUDED
