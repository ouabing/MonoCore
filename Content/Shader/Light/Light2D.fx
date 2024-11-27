sampler TextureSampler : register(s0);

float4 lightColor;
float2 lightPosition;
float lightRadius = 0.5;
float time;
float maxIntensity = 0.1;
float pixelationSize;
float aspectRatio;     // (width / height)
float debug = 0.0;

float random(float2 uv) {
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

float4 Light2D(float2 texCoord : TEXCOORD0) : COLOR0
{
    float2 adjustedCoord = float2(texCoord.x * aspectRatio, texCoord.y);
    float2 adjustedLightPosition = float2(lightPosition.x * aspectRatio, lightPosition.y);
    float2 pixelatedCoord = floor(adjustedCoord / pixelationSize) * pixelationSize;

    float dist = distance(adjustedCoord, adjustedLightPosition);
    if (debug == 1.0 && dist < 0.02)  {
        return float4(1.0, 0.0, 0.0, 1.0);
    }

    float intensity = step(0.0, dist) * step(dist, lightRadius) * maxIntensity;

    if (dist >= lightRadius - pixelationSize && dist <= lightRadius + pixelationSize) {
        float noise = random(pixelatedCoord * 50.0 + time * 5.0);
        if (noise < 0.9) {
            intensity = 0.0;
        }
    }

    intensity = saturate(intensity);

    return lightColor * intensity;
}


technique Brightness
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 Light2D();
    }
}
