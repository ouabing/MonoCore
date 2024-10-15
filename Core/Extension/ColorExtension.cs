using Microsoft.Xna.Framework;
namespace G;

public static class ColorExtensions
{
  public static void RGBToHSL(Color color, out float h, out float s, out float l)
  {
    float r = color.R / 255f;
    float g = color.G / 255f;
    float b = color.B / 255f;

    float max = MathHelper.Max(r, MathHelper.Max(g, b));
    float min = MathHelper.Min(r, MathHelper.Min(g, b));

    h = 0f;
    s = 0f;
    l = (max + min) / 2f;

    if (max != min)
    {
      float delta = max - min;

      s = l > 0.5f ? delta / (2f - max - min) : delta / (max + min);

      if (max == r)
      {
        h = (g - b) / delta + (g < b ? 6f : 0f);
      }
      else if (max == g)
      {
        h = (b - r) / delta + 2f;
      }
      else
      {
        h = (r - g) / delta + 4f;
      }

      h /= 6f;
    }
  }

  public static Color HSLToRGB(float h, float s, float l)
  {
    float r, g, b;

    if (s == 0f)
    {
      r = g = b = l;
    }
    else
    {
      float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
      float p = 2f * l - q;

      r = HueToRGB(p, q, h + 1f / 3f);
      g = HueToRGB(p, q, h);
      b = HueToRGB(p, q, h - 1f / 3f);
    }

    return new Color(r, g, b);
  }

  private static float HueToRGB(float p, float q, float t)
  {
    if (t < 0f) t += 1f;
    if (t > 1f) t -= 1f;
    if (t < 1f / 6f) return p + (q - p) * 6f * t;
    if (t < 1f / 2f) return q;
    if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
    return p;
  }

  public static Color ChangeSaturation(this Color color, float saturationFactor)
  {
    RGBToHSL(color, out float h, out float s, out float l);
    s = MathHelper.Clamp(s * saturationFactor, 0f, 1f);
    return HSLToRGB(h, s, l);
  }
}