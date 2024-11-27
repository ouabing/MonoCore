sampler PressureSampler : register(s0);
float2 texelSize;
float value;

float4 Clear(float2 uv : TEXCOORD0) : COLOR0
{
    return value * tex2D(PressureSampler, uv);
}

technique FluidClear
{
    pass P0
    {
        PixelShader = compile ps_3_0 Clear();
    }
}