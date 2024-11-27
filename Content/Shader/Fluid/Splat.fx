sampler TargetSampler: register(s0);
float2 texelSize;
float aspectRatio;
float2 position;
float4 color;
float radius;

float4 Splat(float2 uv : TEXCOORD0) : COLOR0
{
  float2 p = uv - position.xy;
  p.x *= aspectRatio;
  float e = exp(-dot(p, p) / radius);
  float3 splat = e * color.rgb;
  float alpha = e * color.a;
  float4 base = tex2D(TargetSampler, uv);
  return float4(base.xyz + splat, max(base.a, alpha));
}

technique Splat
{
    pass P0
    {
        PixelShader = compile ps_3_0 Splat();
    }
}