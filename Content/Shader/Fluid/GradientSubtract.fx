#include "Fluid.fxh"

sampler VelocitySampler : register(s0);
sampler PressureSampler : register(s1);

float4 SampleVelocity(float2 uv)
{
    return tex2D(VelocitySampler, uv);
}

float4 SamplePressure(float2 uv)
{
    return tex2D(PressureSampler, uv);
}

float4 SubtractGradient(float2 uv : TEXCOORD0) : COLOR0
{
    float L = SamplePressure(vL(uv)).x;
    float R = SamplePressure(vR(uv)).x;
    float T = SamplePressure(vT(uv)).x;
    float B = SamplePressure(vB(uv)).x;
    float2 velocity = SampleVelocity(uv).xy;
    velocity -= float2(R - L, T - B);
    return float4(velocity, 0.0, 1.0);
}

technique GradientSubtract
{
    pass P0
    {
        PixelShader = compile ps_3_0 SubtractGradient();
    }
}