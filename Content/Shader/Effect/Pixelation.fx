sampler TextureSampler : register(s0);

float2 texelSize;

float4 Pixelate(float2 uv : TEXCOORD0) : COLOR0
{
  float2 p = floor(uv / texelSize) * texelSize;
  return tex2D(TextureSampler, p);
}

technique Pixelation
{
    pass P0
    {
        PixelShader = compile ps_3_0 Pixelate();
    }
}