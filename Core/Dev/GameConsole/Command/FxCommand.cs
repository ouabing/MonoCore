using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace G;

public class FxCommand : ConsoleCommand
{
  public FxCommand() : base(
    "fx",
    "Enable/Run effect",
    "fx [effect name] [-q | --quit]\n\n" +
    "available effects:\n" +
    "  [pixelation](grad=yellow3,red3;bgcolor=blue1) run pixelation shader animation\n" +
    "  [sine](grad=yellow3,red3;bgcolor=blue1)       run sine shader animation\n" +
    "  [bloom](grad=yellow3,red3;bgcolor=blue1)      enable bloom filter\n" +
    "  [vhs](grad=yellow3,red3;bgcolor=blue1)        toggle VHS shader",
    []
  )
  {
  }

  public override void Execute(GameConsole console, string[] args)
  {
    if (args.Length > 2)
    {
      PrintInvalidNumberOfArgumentsError(console);
      return;
    }

    if (args.Length == 0)
    {
      PrintHelp(console);
      return;
    }
    if (args[0] == "--help")
    {
      PrintHelp(console);
      return;
    }

    List<string> fxParams = [];
    if (args.Length == 2)
    {
      if (args[1] == "-q" || args[1] == "--quit")
      {
        console.Disable();
      }
      else
      {
        fxParams = parseFxParams(args[1]);
      }
    }

    switch (args[0].ToLower())
    {
      case "vhs":
        if (Core.Effect.IsVHSEffectActive)
        {
          Core.Effect.DisableVHS();
        }
        else
        {
          var blurAmount = fxParams.Count > 0 ? float.Parse(fxParams[0]) : 1.0f;
          var scanlineIntensity = fxParams.Count > 1 ? float.Parse(fxParams[1]) : 0.5f;
          var chromaticAberrationAmount = fxParams.Count > 2 ? float.Parse(fxParams[2]) : 0.0005f;
          var noiseIntensity = fxParams.Count > 3 ? float.Parse(fxParams[3]) : 0.001f;
          Core.Effect.EnableVHS(blurAmount, scanlineIntensity, chromaticAberrationAmount, noiseIntensity);
        }
        return;
      case "sine":
        var frequency = fxParams.Count > 0 ? float.Parse(fxParams[0]) : 60f;
        var amplitude = fxParams.Count > 1 ? float.Parse(fxParams[1]) : 0.05f;
        var duration = fxParams.Count > 2 ? float.Parse(fxParams[2]) : 1f;
        Core.Effect.Sine(frequency, amplitude, duration);
        return;
      case "pixelation":
        var texelSize = fxParams.Count > 0 ? float.Parse(fxParams[0]) : 32;
        var pixelationDuration = fxParams.Count > 1 ? float.Parse(fxParams[1]) : 0.5f;
        Core.Effect.Pixelate(texelSize, pixelationDuration);
        return;
      case "bloom":
        var presetName = fxParams.Count > 0 ? fxParams[0] : "One";
        var threshold = fxParams.Count > 1 ? float.Parse(fxParams[1]) : 0f;
        var clampTo = fxParams.Count > 2 ? float.Parse(fxParams[2]) : 1f;
        var preset = (BloomFilter.BloomPresets)Enum.Parse(typeof(BloomFilter.BloomPresets), presetName);
        Core.Effect.EnableBloom(preset, threshold, clampTo);
        return;
      default:
        PrintArgumentError(console, args[0]);
        return;
    }
  }

  private static List<string> parseFxParams(string arg)
  {
    var args = arg.Split(',');
    var result = new List<string>();
    foreach (var a in args)
    {
      if (a.Length == 0)
      {
        continue;
      }
      result.Add(a);
    }
    return result;
  }
}
