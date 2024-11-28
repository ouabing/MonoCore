sampler TextureSampler : register(s0);

float time;
float amplitude = 0.1;
float frequency = 10.0;

float4 Sine(float2 uv : TEXCOORD0) : COLOR0
{
    float2 p = uv;

    p.x += sin(time + p.y * frequency) * amplitude;

    // p.y += cos(time * 0.5 + p.x * (frequency * 0.7)) * (amplitude * 0.5);

    p.x += sin(p.x * frequency + p.y * frequency * 0.5 + time) * (amplitude * 0.7);
    // p.y += cos(p.y * frequency * 0.8 + p.x * frequency * 0.3 + time * 1.5) * (amplitude * 0.5);

    return tex2D(TextureSampler, p);
}

technique Sine
{
    pass P0
    {
        PixelShader = compile ps_3_0 Sine();
    }
}