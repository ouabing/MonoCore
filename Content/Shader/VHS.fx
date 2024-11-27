sampler TextureSampler : register(s0);

float time;
float chromaticAberrationAmount = 0.005f;
float noiseIntensity = 0.2f;
float scanlineIntensity = 0.5f;

float blurAmount = 5.0f;

float4 VHSAndBlur(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(TextureSampler, texCoord);
    float4 blur = float4(0, 0, 0, 0);
    float weight = 1.0 / 9.0;

    blur += tex2D(TextureSampler, texCoord) * weight;

    // 上下左右
    blur += tex2D(TextureSampler, texCoord + float2(0, blurAmount) / 512.0) * weight;
    blur += tex2D(TextureSampler, texCoord + float2(0, -blurAmount) / 512.0) * weight;
    blur += tex2D(TextureSampler, texCoord + float2(blurAmount, 0) / 512.0) * weight;
    blur += tex2D(TextureSampler, texCoord + float2(-blurAmount, 0) / 512.0) * weight;

    // 对角线方向
    blur += tex2D(TextureSampler, texCoord + float2(blurAmount, blurAmount) / 512.0) * weight;
    blur += tex2D(TextureSampler, texCoord + float2(-blurAmount, -blurAmount) / 512.0) * weight;
    blur += tex2D(TextureSampler, texCoord + float2(blurAmount, -blurAmount) / 512.0) * weight;
    blur += tex2D(TextureSampler, texCoord + float2(-blurAmount, blurAmount) / 512.0) * weight;

    // 模拟 VHS 效果
    float2 offset = float2(chromaticAberrationAmount, 0);
    float4 rColor = tex2D(TextureSampler, texCoord + offset);
    float4 gColor = tex2D(TextureSampler, texCoord - offset);
    float4 bColor = tex2D(TextureSampler, texCoord);

    blur.r = rColor.r;
    blur.g = gColor.g;
    blur.b = bColor.b;

    float noise = (frac(sin(dot(texCoord.xy * 500.0 + time, float2(12.9898, 78.233))) * 43758.5453) - 0.5) * noiseIntensity;
    blur.rgb += noise;

    float scanline = sin(texCoord.y * 800 + time * 10.0) * scanlineIntensity;
    blur.rgb *= 1.0 - scanline * 0.1;
    return blur;
}

technique VHSAndBlur
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 VHSAndBlur();
    }
}