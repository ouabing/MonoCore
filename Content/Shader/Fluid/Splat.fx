sampler TargetSampler: register(s0);
sampler TextureSampler: register(s1);
sampler BoundarySampler: register(s2);
float aspectRatio;
float2 position;
float4 color;
float radius;
float splatByTexture = 0.0;
float enableBoundary = 0.0;

float4 Splat(float2 uv : TEXCOORD0) : COLOR0
{
  if (enableBoundary > 0.5 && tex2D(BoundarySampler, uv).a > 0.5)
  {
    return float4(0, 0, 0, 0);
  }
  float2 p = uv - position.xy;
  p.x *= aspectRatio;
  float e = exp(-dot(p, p) / radius);
  float3 splat = e * color.rgb;
  float alpha = e * color.a;
  float4 base = tex2D(TargetSampler, uv);
  if (splatByTexture > 0.5) {
    float4 tex = tex2D(TextureSampler, uv);
    if (tex.a == 0) {
      alpha = 0;
    }
  }
  return float4(base.xyz + splat, saturate(max(base.a, alpha)));
}

technique Splat
{
    pass P0
    {
        PixelShader = compile ps_3_0 Splat();
    }
}