#ifndef NOISE_INCLUDED
#define NOISE_INCLUDED

float4 mod289(float4 x){ return x - floor(x / 289.0) * 289.0; }
float3 mod289(float3 x){ return x - floor(x / 289.0) * 289.0; }
float2 mod289(float2 x){ return x - floor(x / 289.0) * 289.0; }
float mod289(float x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }

float permute(float x) { return mod289((x * 34.0 + 10.0) * x); }
float3 permute(float3 x) { return mod289((x * 34.0 + 10.0) * x); }
float4 permute(float4 x){ return mod289((x * 34.0 + 10.0) * x); }

float4 taylorInvSqrt(float4 r){ return 1.79284291400159 - r * 0.85373472095314; }

float4 mod(float4 x, float y) { return x - y * floor(x/y); }
float3 mod(float3 x, float y) { return x - y * floor(x/y); }

float3 fade(float3 t){ return t*t*t*(t*(t*6.0-15.0)+10.0); }

// Permutation polynomial: (34x^2 + x) mod 289
float3 Permutation(float3 x){
	return mod((34.0 * x + 1.0) * x, 289.0);
}

float4 grad4(float j, float4 ip){
  const float4 ones = float4(1.0, 1.0, 1.0, -1.0);
  float4 p,s;

  p.xyz = floor( frac (float3(j,j,j) * ip.xyz) * 7.0) * ip.z - 1.0;
  p.w = 1.5 - dot(abs(p.xyz), ones.xyz);
  s = p < 0;
  p.xyz = p.xyz + (s.xyz*2.0 - 1.0) * s.www; 

  return p;
}

float Simplex4D(float4 v){
	const float4 C = float4(
		0.138196601125011, // (5 - sqrt(5))/20 G4
		0.276393202250021, // 2 * G4
		0.414589803375032, // 3 * G4
	 	-0.447213595499958  // -1 + 4 * G4
	);

	float4 i = floor(v + dot(v, 0.309016994374947451));
	float4 x0 = v - i + dot(i, C.xxxx);

	float4 i0;
	float3 isX = step( x0.yzw, x0.xxx );
	float3 isYZ = step( x0.zww, x0.yyz );
	i0.x = isX.x + isX.y + isX.z;
	i0.yzw = 1.0 - isX;
	i0.y += isYZ.x + isYZ.y;
	i0.zw += 1.0 - isYZ.xy;
	i0.z += isYZ.z;
	i0.w += 1.0 - isYZ.z;

	float4 i3 = saturate(i0);
	float4 i2 = saturate(i0-1.0);
	float4 i1 = saturate(i0-2.0);

	float4 x1 = x0 - i1 + C.xxxx;
	float4 x2 = x0 - i2 + C.yyyy;
	float4 x3 = x0 - i3 + C.zzzz;
	float4 x4 = x0 + C.wwww;


	i = mod289(i); 
	float j0 = permute(
		permute(
			permute(
				permute(i.w) + i.z
			) + i.y
		) + i.x
	);
	float4 j1 = permute(
		permute(
			permute(
				permute(
					i.w + float4(i1.w, i2.w, i3.w, 1.0 )
				) + i.z + float4(i1.z, i2.z, i3.z, 1.0 )
			) + i.y + float4(i1.y, i2.y, i3.y, 1.0 )
		) + i.x + float4(i1.x, i2.x, i3.x, 1.0 )
	);

	const float4 ip = float4(
		0.003401360544217687075, // 1/294
		0.020408163265306122449, // 1/49
		0.142857142857142857143, // 1/7
		0.0
	);

	float4 p0 = grad4(j0, ip);
	float4 p1 = grad4(j1.x, ip);
	float4 p2 = grad4(j1.y, ip);
	float4 p3 = grad4(j1.z, ip);
	float4 p4 = grad4(j1.w, ip);

	float4 norm = rsqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;
	p4 *= rsqrt(dot(p4, p4));

	float3 m0 = max(0.6 - float3(dot(x0, x0), dot(x1, x1), dot(x2, x2)), 0.0);
	float2 m1 = max(0.6 - float2(dot(x3, x3), dot(x4, x4)), 0.0);
	m0 = m0 * m0;
	m1 = m1 * m1;
	
	return 49.0 * (
		dot(
			m0*m0,
			float3(
				dot(p0, x0),
				dot(p1, x1),
				dot(p2, x2)
			)
		) + dot(
			m1*m1,
			float2(
				dot(p3, x3),
				dot(p4, x4)
			)
		)
	);
}


float Simplex3D(float3 v) {
	const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);

	float3 i  = floor(v + dot(v, C.yyy));
	float3 x0 = v   - i + dot(i, C.xxx);

	float3 g = step(x0.yzx, x0.xyz);
	float3 l = 1.0 - g;
	float3 i1 = min(g.xyz, l.zxy);
	float3 i2 = max(g.xyz, l.zxy);

	float3 x1 = x0 - i1 + C.xxx;
	float3 x2 = x0 - i2 + C.yyy;
	float3 x3 = x0 - 0.5;

	i = mod289(i);
	float4 p =
		permute(
			permute(
				permute(
					i.z + float4(0.0, i1.z, i2.z, 1.0))
								+ i.y + float4(0.0, i1.y, i2.y, 1.0))
								+ i.x + float4(0.0, i1.x, i2.x, 1.0)
		);

	float4 j = p - 49.0 * floor(p / 49.0);

	float4 x_ = floor(j / 7.0);
	float4 y_ = floor(j - 7.0 * x_);

	float4 x = (x_ * 2.0 + 0.5) / 7.0 - 1.0;
	float4 y = (y_ * 2.0 + 0.5) / 7.0 - 1.0;

	float4 h = 1.0 - abs(x) - abs(y);

	float4 b0 = float4(x.xy, y.xy);
	float4 b1 = float4(x.zw, y.zw);

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

float Simplex2D(float2 v){
	const float4 C = float4(
		0.211324865405187,
		0.366025403784439,
	 	-0.577350269189626,
		0.024390243902439
	);
	
	float2 i = floor( v + dot(v, C.yy) );
	float2 x0 = v - i + dot(i, C.xx);

	int2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
	float4 x12 = x0.xyxy + C.xxzz;
	x12.xy -= i1;
	
	i = mod289(i);
	float3 p = permute(
		permute(
				i.y + float3(0.0, i1.y, 1.0 )
		) + i.x + float3(0.0, i1.x, 1.0 )
	);
	
	float3 m = max(
		0.5 - float3(
			dot(x0, x0),
			dot(x12.xy, x12.xy),
			dot(x12.zw, x12.zw)
		),
		0.0
	);
	m = m*m ;
	m = m*m ;
	
	float3 x = 2.0 * frac(p * C.www) - 1.0;
	float3 h = abs(x) - 0.5;
	float3 ox = floor(x + 0.5);
	float3 a0 = x - ox;

	m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );

	float3 g;
	g.x = a0.x * x0.x + h.x * x0.y;
	g.yz = a0.yz * x12.xz + h.yz * x12.yw;
	return dot(m, g);
}

float GetSimplex2D(float2 uv, float2 scale){
	uv *= scale;
	return Simplex2D(uv);
}

float GetSimplex3D(float2 uv, float2 scale, float driver, int octaves){
	uv *= scale;
	float3 q = float3(uv, driver);
	float f = 0;
	float a = 0.5;

	for (int i = 0; i < octaves; i++){
		f+=a*Simplex3D(q);
		q = q * 2 + float(i)*0.01;
		a*=0.5;
	}
	
	return f;
}

float GetSimplex4D(float3 pos, float scale, float driver, int octaves){
	float4 c = float4(pos * scale, driver);
	float n = 0;
	float a = 0.5;

	[unroll(6)]
	for (int i = 0; i < 6; i++){
		n += a*Simplex4D(c);
		c = c * 2 + float(i)*0.01;
		a *= 0.5;
	}
	
	return (n + 1) * 0.5;
}

float3 GetCurl(float3 p, float2 scale) {
	p.xy *= scale;
	const float e = .1;
	float3 dx = float3(e, 0.0, 0.0);
	float3 dy = float3(0.0, e, 0.0);
	float3 dz = float3(0.0, 0.0, e);

	float3 p_x0 = Simplex3D(p - dx);
	float3 p_x1 = Simplex3D(p + dx);
	float3 p_y0 = Simplex3D(p - dy);
	float3 p_y1 = Simplex3D(p + dy);
	float3 p_z0 = Simplex3D(p - dz);
	float3 p_z1 = Simplex3D(p + dz);

	float x = p_y1.z - p_y0.z - p_z1.y + p_z0.y;
	float y = p_z1.x - p_z0.x - p_x1.z + p_x0.z;
	float z = p_x1.y - p_x0.y - p_y1.x + p_y0.x;

	const float divisor = 1.0 / (2.0 * e);
	return normalize(float3(x, y, z) * divisor);

}

float2 Voronoi4D(float4 P){			
	float4 Pi = mod(floor(P), 289.0);
	float4 Pf = frac(P);
	float3 oi = float3(-1.0, 0.0, 1.0);
	float3 of = float3(-0.5, 0.5, 1.5);
	float3 px = Permutation(Pi.x + oi);
	float3 py = Permutation(Pi.y + oi);
	float3 pz = Permutation(Pi.z + oi);
	const float K = 0.142857142857;
	const float Ko = 0.428571428571;
	float3 p, ox, oy, oz, ow, dx, dy, dz, dw, d;
	float2 F = 1e6;
	int i, j, k, n;

	for(i = 0; i < 3; i++)
	{
		for(j = 0; j < 3; j++)
		{
			for(k = 0; k < 3; k++)
			{
				p = Permutation(px[i] + py[j] + pz[k] + Pi.w + oi); // pijk1, pijk2, pijk3
	
				ox = frac(p*K) - Ko;
				oy = mod(floor(p*K),7.0)*K - Ko;
				
				p = Permutation(p);
				
				oz = frac(p*K) - Ko;
				ow = mod(floor(p*K),7.0)*K - Ko;
			
				dx = Pf.x - of[i] + 1*ox;
				dy = Pf.y - of[j] + 1*oy;
				dz = Pf.z - of[k] + 1*oz;
				dw = Pf.w - of + 1*ow;
				
				d = dx * dx + dy * dy + dz * dz + dw * dw; // dijk1, dijk2 and dijk3, squared
				
				//Find the lowest and second lowest distances
				for(n = 0; n < 3; n++)
				{
					if(d[n] < F[0])
					{
						F[1] = F[0];
						F[0] = d[n];
					}
					else if(d[n] < F[1])
					{
						F[1] = d[n];
					}
				}
			}
		}
	}
	return F;
}

float GetVoronoi4D(float4 p, int octaves){
	float sum = 0;
	[unroll]
	for(int i = 0; i < octaves; i++){
		float2 F = Voronoi4D(p);
		sum += 0.1 + sqrt(F[0]);
	}
	return sum;
}

// Classic Perlin noise
float Perlin3D(float3 P) {
    float3 Pi0 = floor(P); // Integer part for indexing
    float3 Pi1 = Pi0 + (float3)1.0; // Integer part + 1
    Pi0 = mod289(Pi0);
    Pi1 = mod289(Pi1);
    float3 Pf0 = frac(P); // Fractional part for interpolation
    float3 Pf1 = Pf0 - (float3)1.0; // Fractional part - 1.0
    float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
    float4 iy = float4(Pi0.y, Pi0.y, Pi1.y, Pi1.y);
    float4 iz0 = (float4)Pi0.z;
    float4 iz1 = (float4)Pi1.z;

    float4 ixy = permute(permute(ix) + iy);
    float4 ixy0 = permute(ixy + iz0);
    float4 ixy1 = permute(ixy + iz1);

    float4 gx0 = ixy0 / 7.0;
    float4 gy0 = frac(floor(gx0) / 7.0) - 0.5;
    gx0 = frac(gx0);
    float4 gz0 = (float4)0.5 - abs(gx0) - abs(gy0);
    float4 sz0 = step(gz0, (float4)0.0);
    gx0 -= sz0 * (step((float4)0.0, gx0) - 0.5);
    gy0 -= sz0 * (step((float4)0.0, gy0) - 0.5);

    float4 gx1 = ixy1 / 7.0;
    float4 gy1 = frac(floor(gx1) / 7.0) - 0.5;
    gx1 = frac(gx1);
    float4 gz1 = (float4)0.5 - abs(gx1) - abs(gy1);
    float4 sz1 = step(gz1, (float4)0.0);
    gx1 -= sz1 * (step((float4)0.0, gx1) - 0.5);
    gy1 -= sz1 * (step((float4)0.0, gy1) - 0.5);

    float3 g000 = float3(gx0.x,gy0.x,gz0.x);
    float3 g100 = float3(gx0.y,gy0.y,gz0.y);
    float3 g010 = float3(gx0.z,gy0.z,gz0.z);
    float3 g110 = float3(gx0.w,gy0.w,gz0.w);
    float3 g001 = float3(gx1.x,gy1.x,gz1.x);
    float3 g101 = float3(gx1.y,gy1.y,gz1.y);
    float3 g011 = float3(gx1.z,gy1.z,gz1.z);
    float3 g111 = float3(gx1.w,gy1.w,gz1.w);

    float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
    g000 *= norm0.x;
    g010 *= norm0.y;
    g100 *= norm0.z;
    g110 *= norm0.w;

    float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
    g001 *= norm1.x;
    g011 *= norm1.y;
    g101 *= norm1.z;
    g111 *= norm1.w;

    float n000 = dot(g000, Pf0);
    float n100 = dot(g100, float3(Pf1.x, Pf0.y, Pf0.z));
    float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
    float n110 = dot(g110, float3(Pf1.x, Pf1.y, Pf0.z));
    float n001 = dot(g001, float3(Pf0.x, Pf0.y, Pf1.z));
    float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
    float n011 = dot(g011, float3(Pf0.x, Pf1.y, Pf1.z));
    float n111 = dot(g111, Pf1);

    float3 fade_xyz = fade(Pf0);
    float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
    float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
    float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
    return 2.2 * n_xyz;
}

float2 VoronoiHash(float2 p){
	p = p - 1000 * floor(p / 1000);
	p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
	return frac(sin(p) * 43758.5453);
}

float Voronoi2D(float2 v, float time){
	float2 n = floor(v);
	float2 f = frac(v);
	float F1 = 8.0;
	float F2 = 8.0; 
	float2 mr = 0; 
	float2 mg = 0;
	for (int j = -1; j <= 1; j++){
		for (int i = -1; i <= 1; i++){
			float2 g = float2(i, j);
			float2 o = VoronoiHash(n+g);
			o = (sin(time + o * 6.2831) * 0.5 + 0.5); 
			float2 r = g - f + o;
			float d = 0.5 * dot(r,r);
			if (d < F1) {
				F2 = F1;
				F1 = d; 
				mg = g; 
				mr = r; 
			} 
			else if (d < F2) {
				F2 = d;
			}
		}
	}
	return (F2 + F1) * 0.5;
}

#endif // NOISE_INCLUDED