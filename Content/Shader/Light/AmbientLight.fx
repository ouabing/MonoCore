sampler TextureSampler : register(s0);

float4 ambientColor;
float intensity;

float4 AmbientLight(float2 texCoord : TEXCOORD0) : COLOR0
{
    // 获取像素颜色
    float4 texColor = tex2D(TextureSampler, texCoord);

    // 组合环境光和动态光照
    float3 finalColor = texColor.rgb * ambientColor.rgb * intensity;

    // 返回最终颜色
    return float4(finalColor, texColor.a);
}


technique Brightness
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 AmbientLight();
    }
}
