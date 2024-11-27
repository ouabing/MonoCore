sampler PressureSampler : register(s0);
sampler VelocitySampler : register(s1);
float2 texelSize;

float4 SampleVelocity(float2 uv)
{
    return tex2D(VelocitySampler, uv);
}

float4 SamplePressure(float2 uv)
{
    return tex2D(PressureSampler, uv);
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

float Divergence(float2 uv)
{
    float2 vl = vL(uv);
    float2 vr = vR(uv);
    float2 vt = vT(uv);
    float2 vb = vB(uv);
    float L = SampleVelocity(vl).x;
    float R = SampleVelocity(vr).x;
    float T = SampleVelocity(vt).y;
    float B = SampleVelocity(vb).y;

    float2 C = SampleVelocity(uv).xy;
    // if (vl.x < 0.0 || vl.x > 1.0) { L = -C.x; }
    // if (vr.x < 0.0 || vr.x > 1.0) { R = -C.x; }
    // if (vt.y < 0.0 || vt.y > 1.0) { T = -C.y; }
    // if (vb.y < 0.0 || vb.y > 1.0) { B = -C.y; }

    return 0.5 * (R - L + T - B);
}

float4 Pressure(float2 uv : TEXCOORD0) : COLOR0
{
    // if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
    // {
    //     return float4(0, 0, 0, 1); // clamp to border
    // }
    float L = SamplePressure(vL(uv)).x;
    float R = SamplePressure(vR(uv)).x;
    float T = SamplePressure(vT(uv)).x;
    float B = SamplePressure(vB(uv)).x;
    float C = SamplePressure(uv).x;
    float divergence = Divergence(uv);
    float pressure = (L + R + B + T - divergence) * 0.25;
    return float4(pressure, 0, 0, 1);
}

technique FluidPressure
{
    pass P0
    {
        PixelShader = compile ps_3_0 Pressure();
    }
}