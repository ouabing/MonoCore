
using System;
using System.Collections.Generic;
using System.Linq;

namespace G;

public class GRandom(int seed = 0)
{
  private int SeedValue { get; set; } = seed;
  private Random random = new(seed == 0 ? GenerateSeed() : seed);
  public int Seed
  {
    get
    {
      return SeedValue;
    }
    set
    {
      SeedValue = value;
      random = new Random(value == 0 ? GenerateSeed() : value);
    }
  }

  public int Next(int minValue, int maxValue)
  {
    return random.Next(minValue, maxValue);
  }

  public int Next(int maxValue)
  {
    return random.Next(maxValue);
  }

  public float NextSingle()
  {
    return random.NextSingle();
  }

  public float NextSingle(float minValue, float maxValue)
  {
    return random.NextSingle() * (maxValue - minValue) + minValue;
  }

  public int Next()
  {
    return random.Next();
  }

  /**
    * Give a list of chances and a list of the outputs, roll the dice and return the picked output.
    * var picked = NextPick([10, 20, 50], [Weapon1, Weapon2, Weapon3]);
    */
  public T NextPick<T>(float[] chances, T[] values)
  {
    if (chances.Length != values.Length)
    {
      throw new InvalidOperationException("Chances and values must have the same length");
    }
    float accChances = 0;
    float total = chances.Sum();
    for (int i = 0; i < chances.Length; i++)
    {
      var chance = chances[i] / total;
      if (i == chances.Length - 1)
      {
        chance = 1 - accChances;
      }
      if ((NextSingle() - accChances) <= chance)
      {
        return values[i];
      }
      accChances += chance;
    }
    throw new InvalidOperationException("Chances not added to 1");
  }

  public T NextPick<T>(T[] values)
  {
    return values[Next(values.Length)];
  }

  public T NextPick<T>(List<T> values)
  {
    return values[Next(values.Count)];
  }

  public static int GenerateSeed()
  {
    return Guid.NewGuid().GetHashCode();
  }
}