#ifndef NOISE_INCLUDED
#define NOISE_INCLUDED

float4 mod289(float4 x){ return x - floor(x / 289.0) * 289.0; }
float3 mod289(float3 x){ return x - floor(x / 289.0) * 289.0; }
float2 mod289(float2 x){ return x - floor(x / 289.0) * 289.0; }
float4 permute(float4 x){ return mod289((x * 34.0 + 1.0) * x); }
// float3 permute(float3 x) { return mod289((x * 34.0 + 1.0) * x); }
float4 taylorInvSqrt(float4 r){ return 1.79284291400159 - r * 0.85373472095314; }
// float3 taylorInvSqrt(float3 r){ return 1.79284291400159 - 0.85373472095314 * r; }

float Simplex3D(float3 v) {
    const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);

    // First corner
    float3 i  = floor(v + dot(v, C.yyy));
    float3 x0 = v   - i + dot(i, C.xxx);

    // Other corners
    float3 g = step(x0.yzx, x0.xyz);
    float3 l = 1.0 - g;
    float3 i1 = min(g.xyz, l.zxy);
    float3 i2 = max(g.xyz, l.zxy);

    // x1 = x0 - i1  + 1.0 * C.xxx;
    // x2 = x0 - i2  + 2.0 * C.xxx;
    // x3 = x0 - 1.0 + 3.0 * C.xxx;
    float3 x1 = x0 - i1 + C.xxx;
    float3 x2 = x0 - i2 + C.yyy;
    float3 x3 = x0 - 0.5;

    // Permutations
    i = mod289(i); // Avoid truncation effects in permutation
    float4 p =
      permute(permute(permute(i.z + float4(0.0, i1.z, i2.z, 1.0))
                            + i.y + float4(0.0, i1.y, i2.y, 1.0))
                            + i.x + float4(0.0, i1.x, i2.x, 1.0));

    // Gradients: 7x7 points over a square, mapped onto an octahedron.
    // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
    float4 j = p - 49.0 * floor(p / 49.0);  // mod(p,7*7)

    float4 x_ = floor(j / 7.0);
    float4 y_ = floor(j - 7.0 * x_);  // mod(j,N)

    float4 x = (x_ * 2.0 + 0.5) / 7.0 - 1.0;
    float4 y = (y_ * 2.0 + 0.5) / 7.0 - 1.0;

    float4 h = 1.0 - abs(x) - abs(y);

    float4 b0 = float4(x.xy, y.xy);
    float4 b1 = float4(x.zw, y.zw);

    //float4 s0 = float4(lessThan(b0, 0.0)) * 2.0 - 1.0;
    //float4 s1 = float4(lessThan(b1, 0.0)) * 2.0 - 1.0;
    float4 s0 = floor(b0) * 2.0 + 1.0;
    float4 s1 = floor(b1) * 2.0 + 1.0;
    float4 sh = -step(h, 0.0);

    float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
    float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

    float3 g0 = float3(a0.xy, h.x);
    float3 g1 = float3(a0.zw, h.y);
    float3 g2 = float3(a1.xy, h.z);
    float3 g3 = float3(a1.zw, h.w);

    // Normalise gradients
    float4 norm = taylorInvSqrt(float4(dot(g0, g0), dot(g1, g1), dot(g2, g2), dot(g3, g3)));
    g0 *= norm.x;
    g1 *= norm.y;
    g2 *= norm.z;
    g3 *= norm.w;

    // Mix final noise value
    float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
    m = m * m;
    m = m * m;

    float4 px = float4(dot(x0, g0), dot(x1, g1), dot(x2, g2), dot(x3, g3));
    return 42.0 * dot(m, px);
}

// float3 Simplex3D3(float3 x) {
// 	float s = Simplex3D(x);
// 	float s1 = Simplex3D(float3(x.y - 19.1, x.z + 33.4, x.x + 47.2));
// 	float s2 = Simplex3D(float3(x.z + 74.2, x.x - 124.5, x.y + 99.4));
// 	float3 c = float3(s, s1, s2);
// 	return c;
// }

// float3 Curl3D(float3 p) {
// 	const float e = .1;
// 	float3 dx = float3(e, 0.0, 0.0);
// 	float3 dy = float3(0.0, e, 0.0);
// 	float3 dz = float3(0.0, 0.0, e);

// 	float3 p_x0 = Simplex3D3(p - dx);
// 	float3 p_x1 = Simplex3D3(p + dx);
// 	float3 p_y0 = Simplex3D3(p - dy);
// 	float3 p_y1 = Simplex3D3(p + dy);
// 	float3 p_z0 = Simplex3D3(p - dz);
// 	float3 p_z1 = Simplex3D3(p + dz);

// 	float x = p_y1.z - p_y0.z - p_z1.y + p_z0.y;
// 	float y = p_z1.x - p_z0.x - p_x1.z + p_x0.z;
// 	float z = p_x1.y - p_x0.y - p_y1.x + p_y0.x;

// 	const float divisor = 1.0 / (2.0 * e);
// 	return normalize(float3(x, y, z) * divisor);
// }

// float GetCurl3D(float2 uv, float2 minMax, float2 scale, float driver){
// 	uv *= scale;
// 	float f = Curl3D(float3(uv, driver));
// 	return Remap(f, -1, 1, minMax.x, minMax.y);
// }

// float GetSimplex3D(float2 uv, float2 minMax, float2 scale, float driver){
// 	return Remap(Simplex3D(float3(uv * scale, driver)), -1, 1, minMax.x, minMax.y);
// }

float GetSimplex3D(float2 uv, float2 minMax, float2 scale, float driver, int octaves){
	uv *= scale;
	float3 q = float3(uv, driver);
	float f = 0;
	float a = 0.5;

	UNITY_BRANCH
	if (octaves != 1) {
		[unroll(5)]
		for (int i = 0; i < octaves; i++){
			f+=a*Simplex3D(q);
			q = q * 2 + float(i)*0.01;
			a*=0.5;
		}
	}
	else f = Simplex3D(q);
	
	return Remap(f, -1, 1, minMax.x, minMax.y);
}
#endif