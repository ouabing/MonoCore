#include "Fluid.fxh"

sampler VelocitySampler : register(s0);
sampler BoundarySampler : register(s1);

float dt;
float curlAmount;
float enableBoundary = 0.0;

float4 SampleVelocity(float2 uv)
{
    return tex2D(VelocitySampler, uv);
}

float Curl(float2 uv) {
    // if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
    // {
    //     return 0.0; // clamp to border
    // }
    if (enableBoundary > 0.5)
    {
        float bC = tex2D(BoundarySampler, uv).a;
        if (bC > 0.5) { return 0.0; }
        float bL = tex2D(BoundarySampler, vL(uv)).a;
        if (bL > 0.5) { return 0.0; }
        float bR = tex2D(BoundarySampler, vR(uv)).a;
        if (bR > 0.5) { return 0.0; }
        float bT = tex2D(BoundarySampler, vT(uv)).a;
        if (bT > 0.5) { return 0.0; }
        float bB = tex2D(BoundarySampler, vB(uv)).a;
        if (bB > 0.5) { return 0.0; }
    }
    float l = SampleVelocity(vL(uv)).y;
    float r = SampleVelocity(vR(uv)).y;
    float t = SampleVelocity(vT(uv)).x;
    float b = SampleVelocity(vB(uv)).x;
    return 0.5 * ((r - l) - (t - b));
}

float4 Vorticity(float2 uv: TEXCOORD0) : COLOR0
{
    float curlL = Curl(vL(uv));
    float curlR = Curl(vR(uv));
    float curlT = Curl(vT(uv));
    float curlB = Curl(vB(uv));
    float curlC = Curl(uv);

    float2 force = 0.5 * float2(abs(curlT) - abs(curlB), abs(curlR) - abs(curlL));
    if (length(force) < 0.0001) {
        force = float2(0, 0);
    } else {
        force = force / length(force + 0.0001);
    }
    force *= curlAmount * curlC;
    force.y = -force.y;
    float2 velocity = SampleVelocity(uv).xy;
    velocity += force * dt;
    velocity = min(max(velocity, -1000.0), 1000.0);

    return float4(velocity, 0.0, 1.0);
}

technique Vorticity
{
    pass P0
    {
        PixelShader = compile ps_3_0 Vorticity();
    }
}
