#define MAX_COLOR 64
sampler TextureSampler : register(s0);

float3 colors[MAX_COLOR];
int colorCount;
float time;
float speed;

bool IsEqual(float3 a, float3 b)
{
    return all(abs(a - b) < 0.01);
}

int findValue(float3 target)
{
    [unroll(MAX_COLOR)]
    for (int i = 0; i < MAX_COLOR; i++)
    {
        if (IsEqual(colors[i], target))
        {
            return i;
        }
    }
    return -1;
}

float3 offsetValue(float3 current, int offset)
{
    int index = findValue(current);
    if (index == -1 || index > colorCount - 1)
    {
        return current;
    }
    int newIndex = fmod(index + offset, colorCount);
    return colors[newIndex];
}

float4 PaletteCycle(float2 uv : TEXCOORD0) : COLOR0
{
  float4 p = tex2D(TextureSampler, uv);
  int offset = (int)(time / speed);
  offset = fmod(offset, colorCount);
  p.rgb = offsetValue(p.rgb, offset);
  return p;
}

technique PaletteCycling
{
    pass P0
    {
        PixelShader = compile ps_3_0 PaletteCycle();
    }
}
