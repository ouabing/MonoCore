sampler SourceSampler : register(s0);
sampler VelocitySampler : register(s1);
float2 texelSize;
float2 dyeTexelSize;
float dt;
float dissipation;
float linearFiltering;

float4 SampleVelocity(float2 uv)
{
    return tex2D(VelocitySampler, uv);
}

float4 SampleSource(float2 uv)
{
    return tex2D(SourceSampler, uv);
}

float2 vL(float2 uv)
{
    return uv - float2(texelSize.x, 0);
}

float2 vR(float2 uv)
{
    return uv + float2(texelSize.x, 0);
}

float2 vT(float2 uv) {
    return uv + float2(0, texelSize.y);
}

float2 vB(float2 uv) {
    return uv - float2(0, texelSize.y);
}

float4 bilerp(sampler sam, float2 uv, float2 tsize) {
  float2 st = uv / tsize - 0.5;
  float2 iuv = floor(st);
  float2 fuv = frac(st);
  float4 a = tex2D(sam, (iuv + float2(0.5, 0.5)) * tsize);
  float4 b = tex2D(sam, (iuv + float2(1.5, 0.5)) * tsize);
  float4 c = tex2D(sam, (iuv + float2(0.5, 1.5)) * tsize);
  float4 d = tex2D(sam, (iuv + float2(1.5, 1.5)) * tsize);

  return lerp(lerp(a, b, fuv.x), lerp(c, d, fuv.x), fuv.y);
}

float4 Advect(float2 uv : TEXCOORD0) : COLOR0
{
  float2 coord;
  float4 result;
  if (linearFiltering > 0.5) {
    coord = uv - dt * bilerp(VelocitySampler, uv, texelSize).xy * texelSize;
    result = bilerp(SourceSampler, coord, dyeTexelSize);
  } else {
    coord = uv - dt * SampleVelocity(uv).xy * texelSize;
    result = SampleSource(coord);
  }

  float decay = 1.0 + dissipation * dt;
  return result / decay;
}

technique Advection
{
    pass P0
    {
        PixelShader = compile ps_3_0 Advect();
    }
}