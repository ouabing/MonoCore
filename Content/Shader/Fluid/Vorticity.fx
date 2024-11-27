sampler VelocitySampler : register(s0);

float dt;
float curlAmount;
float2 texelSize;

float4 SampleVelocity(float2 uv)
{
    return tex2D(VelocitySampler, uv);
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

float Curl(float2 uv) {
    // if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
    // {
    //     return 0.0; // clamp to border
    // }
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
