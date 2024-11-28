#include "Fluid.fxh"

sampler2D uTexture : register(s0);
sampler2D uBloom : register(s1);
sampler2D uSunrays : register(s2);
sampler2D uDithering : register(s3);

float2 ditherScale;

// Pixel shader main function
float4 Display(float2 uv : TEXCOORD0) : COLOR0
{
    // Sample the main texture color
    float3 c = tex2D(uTexture, uv).rgb;

    #ifdef SHADING
    // Apply shading based on neighbors
    float3 lc = tex2D(uTexture, vL(uv)).rgb;
    float3 rc = tex2D(uTexture, vR(uv)).rgb;
    float3 tc = tex2D(uTexture, vT(uv)).rgb;
    float3 bc = tex2D(uTexture, vB(uv)).rgb;

    float dx = length(rc) - length(lc);
    float dy = length(tc) - length(bc);

    float3 n = normalize(float3(dx, dy, length(texelSize)));
    float3 l = float3(0.0, 0.0, 1.0);

    float diffuse = clamp(dot(n, l) + 0.7, 0.7, 1.0);
    c *= diffuse;
    #endif

    #ifdef BLOOM
    // Add bloom effect
    float3 bloom = tex2D(uBloom, uv).rgb;
    #endif

    #ifdef SUNRAYS
    // Multiply by sunrays texture
    float sunrays = tex2D(uSunrays, uv).r;
    c *= sunrays;
    #ifdef BLOOM
    bloom *= sunrays;
    #endif
    #endif

    #ifdef BLOOM
    // Add dithering effect
    float noise = tex2D(uDithering, uv * ditherScale).r;
    noise = noise * 2.0 - 1.0;
    bloom += noise / 255.0;

    // Convert bloom to gamma space
    bloom = max(1.055 * pow(bloom, float3(0.416666667)) - 0.055, float3(0.0));
    c += bloom;
    #endif

    // Set alpha based on the brightest channel
    float a = max(c.r, max(c.g, c.b));
    return float4(c, a);
}

technique Display
{
    pass P0
    {
        PixelShader = compile ps_3_0 Display();
    }
}