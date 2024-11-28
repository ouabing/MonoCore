float2 texelSize;

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
