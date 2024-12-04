// Source code from https://github.com/Kosmonaut3d/BloomFilter-for-Monogame-and-XNA

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace G;
/// <summary>
///
/// Version 1.1, 16. Dez. 2016
///
/// Bloom / Blur, 2016 TheKosmonaut
///
/// High-Quality Bloom filter for high-performance applications
///
/// Based largely on the implementations in Unreal Engine 4 and Call of Duty AW
/// For more information look for
/// "Next Generation Post Processing in Call of Duty Advanced Warfare" by Jorge Jimenez
/// http://www.iryoku.com/downloads/Next-Generation-Post-Processing-in-Call-of-Duty-Advanced-Warfare-v18.pptx
///
/// The idea is to have several rendertargets or one rendertarget with several mip maps
/// so each mip has half resolution (1/2 width and 1/2 height) of the previous one.
///
/// 32, 16, 8, 4, 2
///
/// In the first step we extract the bright spots from the original image. If not specified otherwise thsi happens in full resolution.
/// We can do that based on the average RGB value or Luminance and check whether this value is higher than our Threshold.
///     BloomUseLuminance = true / false (default is true)
///     BloomThreshold = 0.8f;
///
/// Then we downscale this extraction layer to the next mip map.
/// While doing that we sample several pixels around the origin.
/// We continue to downsample a few more times, defined in
///     BloomDownsamplePasses = 5 ( default is 5)
///
/// Afterwards we upsample again, but blur in this step, too.
/// The final output should be a blur with a very large kernel and smooth gradient.
///
/// The output in the draw is only the blurred extracted texture.
/// It can be drawn on top of / merged with the original image with an additive operation for example.
///
/// If you use ToneMapping you should apply Bloom before that step.
/// </summary>
public class BloomFilter : IDisposable
{
  #region fields & properties

  #region private fields

  //resolution
  private int _width;
  private int _height;

  //RenderTargets
  private RenderTarget2D bloomRenderTarget2DMip0;
  private RenderTarget2D bloomRenderTarget2DMip1;
  private RenderTarget2D bloomRenderTarget2DMip2;
  private RenderTarget2D bloomRenderTarget2DMip3;
  private RenderTarget2D bloomRenderTarget2DMip4;
  private RenderTarget2D bloomRenderTarget2DMip5;

  private SurfaceFormat renderTargetFormat;

  //Objects
  private QuadRenderer quadRenderer;

  //Shader + variables
  private Effect bloomEffect;

  private EffectPass bloomPassExtract;
  private EffectPass bloomPassExtractLuminance;
  private EffectPass bloomPassDownsample;
  private EffectPass bloomPassUpsample;
  private EffectPass bloomPassUpsampleLuminance;

  private EffectParameter bloomParameterScreenTexture;
  private EffectParameter bloomInverseResolutionParameter;
  private EffectParameter bloomRadiusParameter;
  private EffectParameter bloomStrengthParameter;
  private EffectParameter bloomStreakLengthParameter;
  private EffectParameter bloomThresholdParameter;
  private EffectParameter bloomClampToParameter;

  //Preset variables for different mip levels
  private float bloomRadius1 = 1.0f;
  private float bloomRadius2 = 1.0f;
  private float bloomRadius3 = 1.0f;
  private float bloomRadius4 = 1.0f;
  private float bloomRadius5 = 1.0f;

  private float bloomStrength1 = 1.0f;
  private float bloomStrength2 = 1.0f;
  private float bloomStrength3 = 1.0f;
  private float bloomStrength4 = 1.0f;
  private float bloomStrength5 = 1.0f;

  public float BloomStrengthMultiplier { get; set; } = 1.0f;

  private float radiusMultiplier = 1.0f;


  #endregion

  #region public fields + enums

  public bool BloomUseLuminance { get; set; } = true;
  public int BloomDownsamplePasses { get; set; } = 5;

  //enums
  public enum BloomPresets
  {
    Wide,
    Focused,
    Small,
    SuperWide,
    Cheap,
    One
  };

  #endregion

  #region properties
  public BloomPresets BloomPreset
  {
    get { return _bloomPreset; }
    set
    {
      if (_bloomPreset == value) return;

      _bloomPreset = value;
      SetBloomPreset(_bloomPreset);
    }
  }
  private BloomPresets _bloomPreset;


  private Texture2D BloomScreenTexture { set { bloomParameterScreenTexture.SetValue(value); } }
  private Vector2 BloomInverseResolution
  {
    get { return _bloomInverseResolutionField; }
    set
    {
      if (value != _bloomInverseResolutionField)
      {
        _bloomInverseResolutionField = value;
        bloomInverseResolutionParameter.SetValue(_bloomInverseResolutionField);
      }
    }
  }
  private Vector2 _bloomInverseResolutionField;

  private float BloomRadius
  {
    get
    {
      return _bloomRadius;
    }

    set
    {
      if (Math.Abs(_bloomRadius - value) > 0.001f)
      {
        _bloomRadius = value;
        bloomRadiusParameter.SetValue(_bloomRadius * radiusMultiplier);
      }

    }
  }
  private float _bloomRadius;

  private float BloomStrength
  {
    get { return _bloomStrength; }
    set
    {
      if (Math.Abs(_bloomStrength - value) > 0.001f)
      {
        _bloomStrength = value;
        bloomStrengthParameter.SetValue(_bloomStrength * BloomStrengthMultiplier);
      }

    }
  }
  private float _bloomStrength;

  public float BloomStreakLength
  {
    get { return _bloomStreakLength; }
    set
    {
      if (Math.Abs(_bloomStreakLength - value) > 0.001f)
      {
        _bloomStreakLength = value;
        bloomStreakLengthParameter.SetValue(_bloomStreakLength);
      }
    }
  }
  private float _bloomStreakLength;

  public float BloomThreshold
  {
    get { return _bloomThreshold; }
    set
    {
      if (Math.Abs(_bloomThreshold - value) > 0.001f)
      {
        _bloomThreshold = value;
        bloomThresholdParameter.SetValue(_bloomThreshold);
      }
    }
  }
  private float _bloomThreshold;

  public float BloomClampTo
  {
    get { return _bloomClampTo; }
    set
    {
      if (Math.Abs(_bloomClampTo - value) > 0.001f)
      {
        _bloomClampTo = value;
        bloomClampToParameter.SetValue(_bloomClampTo);
      }
    }
  }
  private float _bloomClampTo;

  #endregion

  #endregion

  #region initialize

  /// <summary>
  /// Loads all needed components for the BloomEffect. This effect won't work without calling load
  /// </summary>
  /// <param name="width">initial value for creating the rendertargets</param>
  /// <param name="height">initial value for creating the rendertargets</param>
  /// <param name="renderTargetFormat">The intended format for the rendertargets. For normal, non-hdr, applications color or rgba1010102 are fine NOTE: For OpenGL, SurfaceFormat.Color is recommended for non-HDR applications.</param>
  /// <param name="quadRenderer">if you already have quadRenderer you may reuse it here</param>
  public void LoadContent(int width, int height, SurfaceFormat renderTargetFormat = SurfaceFormat.Color, QuadRenderer quadRenderer = null)
  {
    UpdateResolution(width, height);

    //if quadRenderer == null -> new, otherwise not
    this.quadRenderer = quadRenderer ?? new QuadRenderer();

    this.renderTargetFormat = renderTargetFormat;

    //Load the shader parameters and passes for cheap and easy access
    bloomEffect = Core.Effect.LoadEffect("MonoCore/Shader/Effect/Bloom").Clone();
    bloomInverseResolutionParameter = bloomEffect.Parameters["InverseResolution"];
    bloomRadiusParameter = bloomEffect.Parameters["Radius"];
    bloomStrengthParameter = bloomEffect.Parameters["Strength"];
    bloomStreakLengthParameter = bloomEffect.Parameters["StreakLength"];
    bloomThresholdParameter = bloomEffect.Parameters["Threshold"];
    bloomClampToParameter = bloomEffect.Parameters["ClampTo"];

    //For DirectX / Windows
    bloomParameterScreenTexture = bloomEffect.Parameters["ScreenTexture"];

    //If we are on OpenGL it's different, load the other one then!
    if (bloomParameterScreenTexture == null)
    {
      //for OpenGL / CrossPlatform
      bloomParameterScreenTexture = bloomEffect.Parameters["LinearSampler+ScreenTexture"];
    }

    bloomPassExtract = bloomEffect.Techniques["Extract"].Passes[0];
    bloomPassExtractLuminance = bloomEffect.Techniques["ExtractLuminance"].Passes[0];
    bloomPassDownsample = bloomEffect.Techniques["Downsample"].Passes[0];
    bloomPassUpsample = bloomEffect.Techniques["Upsample"].Passes[0];
    bloomPassUpsampleLuminance = bloomEffect.Techniques["UpsampleLuminance"].Passes[0];

    //An interesting blendstate for merging the initial image with the bloom.
    // BlendStateBloom = new BlendState();
    // BlendStateBloom.ColorBlendFunction = BlendFunction.Add;
    // BlendStateBloom.ColorSourceBlend = Blend.BlendFactor;
    // BlendStateBloom.ColorDestinationBlend = Blend.BlendFactor;
    // BlendStateBloom.BlendFactor = new Color(0.5f, 0.5f, 0.5f);

    //Default threshold.
    BloomThreshold = 0.8f;
    BloomClampTo = 1.0f;
    //Setup the default preset values.
    //BloomPreset = BloomPresets.One;
    SetBloomPreset(BloomPreset);
  }

  /// <summary>
  /// A few presets with different values for the different mip levels of our bloom.
  /// </summary>
  /// <param name="preset">See BloomPresets enums. Example: BloomPresets.Wide</param>
  private void SetBloomPreset(BloomPresets preset)
  {
    switch (preset)
    {
      case BloomPresets.Wide:
        {
          bloomStrength1 = 0.5f;
          bloomStrength2 = 1;
          bloomStrength3 = 2;
          bloomStrength4 = 1;
          bloomStrength5 = 2;
          bloomRadius5 = 4.0f;
          bloomRadius4 = 4.0f;
          bloomRadius3 = 2.0f;
          bloomRadius2 = 2.0f;
          bloomRadius1 = 1.0f;
          BloomStreakLength = 1;
          BloomDownsamplePasses = 5;
          break;
        }
      case BloomPresets.SuperWide:
        {
          bloomStrength1 = 0.9f;
          bloomStrength2 = 1;
          bloomStrength3 = 1;
          bloomStrength4 = 2;
          bloomStrength5 = 6;
          bloomRadius5 = 4.0f;
          bloomRadius4 = 2.0f;
          bloomRadius3 = 2.0f;
          bloomRadius2 = 2.0f;
          bloomRadius1 = 2.0f;
          BloomStreakLength = 1;
          BloomDownsamplePasses = 5;
          break;
        }
      case BloomPresets.Focused:
        {
          bloomStrength1 = 0.8f;
          bloomStrength2 = 1;
          bloomStrength3 = 1;
          bloomStrength4 = 1;
          bloomStrength5 = 2;
          bloomRadius5 = 4.0f;
          bloomRadius4 = 2.0f;
          bloomRadius3 = 2.0f;
          bloomRadius2 = 2.0f;
          bloomRadius1 = 2.0f;
          BloomStreakLength = 1;
          BloomDownsamplePasses = 5;
          break;
        }
      case BloomPresets.Small:
        {
          bloomStrength1 = 0.8f;
          bloomStrength2 = 1;
          bloomStrength3 = 1;
          bloomStrength4 = 1;
          bloomStrength5 = 1;
          bloomRadius5 = 1;
          bloomRadius4 = 1;
          bloomRadius3 = 1;
          bloomRadius2 = 1;
          bloomRadius1 = 1;
          BloomStreakLength = 1;
          BloomDownsamplePasses = 5;
          break;
        }
      case BloomPresets.Cheap:
        {
          bloomStrength1 = 0.8f;
          bloomStrength2 = 2;
          bloomRadius2 = 2;
          bloomRadius1 = 2;
          BloomStreakLength = 1;
          BloomDownsamplePasses = 2;
          break;
        }
      case BloomPresets.One:
        {
          bloomStrength1 = 3.0f;
          bloomStrength2 = 1;
          bloomStrength3 = 1;
          bloomStrength4 = 1;
          bloomStrength5 = 2;
          bloomRadius5 = 1.0f;
          bloomRadius4 = 1.0f;
          bloomRadius3 = 1.0f;
          bloomRadius2 = 1.0f;
          bloomRadius1 = 1.0f;
          BloomStreakLength = 1;
          BloomDownsamplePasses = 5;
          break;
        }
    }
  }

  #endregion

  /// <summary>
  /// Main draw function
  /// </summary>
  /// <param name="inputTexture">the image from which we want to extract bright parts and blur these</param>
  /// <param name="width">width of our target. If different to the input.Texture width our final texture will be smaller/larger.
  /// For example we can use half resolution. Input: 1280px wide -> width = 640px
  /// The smaller this value the better performance and the worse our final image quality</param>
  /// <param name="height">see: width</param>
  /// <returns></returns>
  public Texture2D Draw(Texture2D inputTexture, int width, int height)
  {
    // Check if we are initialized
    if (Core.GraphicsDevice == null)
    {
      throw new Exception("Module not yet Loaded / Initialized. Use Load() first");
    }

    // Change renderTarget resolution if different from what we expected. If lower than the inputTexture we gain performance.
    if (width != _width || height != _height)
    {
      UpdateResolution(width, height);

      //Adjust the blur so it looks consistent across diferrent scalings
      radiusMultiplier = (float)width / inputTexture.Width;

      //Update our variables with the multiplier
      SetBloomPreset(BloomPreset);
    }

    Core.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
    Core.GraphicsDevice.BlendState = BlendState.Opaque;

    // EXTRACT  //Note: Is setRenderTargets(binding better?)
    // We extract the bright values which are above the Threshold and save them to Mip0
    Core.GraphicsDevice.SetRenderTarget(bloomRenderTarget2DMip0);

    BloomScreenTexture = inputTexture;
    BloomInverseResolution = new Vector2(1.0f / _width, 1.0f / _height);

    if (BloomUseLuminance)
    {
      bloomPassExtractLuminance.Apply();
    }
    else
    {
      bloomPassExtract.Apply();
    }
    quadRenderer.RenderQuad(Vector2.One * -1, Vector2.One);

    // Now downsample to the next lower mip texture
    if (BloomDownsamplePasses > 0)
    {
      // DOWNSAMPLE TO MIP1
      Core.GraphicsDevice.SetRenderTarget(bloomRenderTarget2DMip1);

      BloomScreenTexture = bloomRenderTarget2DMip0;
      // Pass
      bloomPassDownsample.Apply();
      quadRenderer.RenderQuad(Vector2.One * -1, Vector2.One);

      if (BloomDownsamplePasses > 1)
      {
        //Our input resolution is halfed, so our inverse 1/res. must be doubled
        BloomInverseResolution *= 2;

        //DOWNSAMPLE TO MIP2
        Core.GraphicsDevice.SetRenderTarget(bloomRenderTarget2DMip2);

        BloomScreenTexture = bloomRenderTarget2DMip1;
        //Pass
        bloomPassDownsample.Apply();
        quadRenderer.RenderQuad(Vector2.One * -1, Vector2.One);

        if (BloomDownsamplePasses > 2)
        {
          BloomInverseResolution *= 2;

          //DOWNSAMPLE TO MIP3
          Core.GraphicsDevice.SetRenderTarget(bloomRenderTarget2DMip3);

          BloomScreenTexture = bloomRenderTarget2DMip2;
          //Pass
          bloomPassDownsample.Apply();
          quadRenderer.RenderQuad(Vector2.One * -1, Vector2.One);

          if (BloomDownsamplePasses > 3)
          {
            BloomInverseResolution *= 2;

            //DOWNSAMPLE TO MIP4
            Core.GraphicsDevice.SetRenderTarget(bloomRenderTarget2DMip4);

            BloomScreenTexture = bloomRenderTarget2DMip3;
            //Pass
            bloomPassDownsample.Apply();
            quadRenderer.RenderQuad(Vector2.One * -1, Vector2.One);

            if (BloomDownsamplePasses > 4)
            {
              BloomInverseResolution *= 2;

              // DOWNSAMPLE TO MIP5
              Core.GraphicsDevice.SetRenderTarget(bloomRenderTarget2DMip5);

              BloomScreenTexture = bloomRenderTarget2DMip4;
              // Pass
              bloomPassDownsample.Apply();
              quadRenderer.RenderQuad(Vector2.One * -1, Vector2.One);

              ChangeBlendState();

              // UPSAMPLE TO MIP4
              Core.GraphicsDevice.SetRenderTarget(bloomRenderTarget2DMip4);
              BloomScreenTexture = bloomRenderTarget2DMip5;

              BloomStrength = bloomStrength5;
              BloomRadius = bloomRadius5;
              if (BloomUseLuminance) bloomPassUpsampleLuminance.Apply();
              else bloomPassUpsample.Apply();
              quadRenderer.RenderQuad(Vector2.One * -1, Vector2.One);

              BloomInverseResolution /= 2;
            }

            ChangeBlendState();

            // UPSAMPLE TO MIP3
            Core.GraphicsDevice.SetRenderTarget(bloomRenderTarget2DMip3);
            BloomScreenTexture = bloomRenderTarget2DMip4;

            BloomStrength = bloomStrength4;
            BloomRadius = bloomRadius4;
            if (BloomUseLuminance) bloomPassUpsampleLuminance.Apply();
            else bloomPassUpsample.Apply();
            quadRenderer.RenderQuad(Vector2.One * -1, Vector2.One);

            BloomInverseResolution /= 2;

          }

          ChangeBlendState();

          //UPSAMPLE TO MIP2
          Core.GraphicsDevice.SetRenderTarget(bloomRenderTarget2DMip2);
          BloomScreenTexture = bloomRenderTarget2DMip3;

          BloomStrength = bloomStrength3;
          BloomRadius = bloomRadius3;
          if (BloomUseLuminance) bloomPassUpsampleLuminance.Apply();
          else bloomPassUpsample.Apply();
          quadRenderer.RenderQuad(Vector2.One * -1, Vector2.One);

          BloomInverseResolution /= 2;

        }

        ChangeBlendState();

        //UPSAMPLE TO MIP1
        Core.GraphicsDevice.SetRenderTarget(bloomRenderTarget2DMip1);
        BloomScreenTexture = bloomRenderTarget2DMip2;

        BloomStrength = bloomStrength2;
        BloomRadius = bloomRadius2;
        if (BloomUseLuminance) bloomPassUpsampleLuminance.Apply();
        else bloomPassUpsample.Apply();
        quadRenderer.RenderQuad(Vector2.One * -1, Vector2.One);

        BloomInverseResolution /= 2;
      }

      ChangeBlendState();

      // UPSAMPLE TO MIP0
      Core.GraphicsDevice.SetRenderTarget(bloomRenderTarget2DMip0);
      BloomScreenTexture = bloomRenderTarget2DMip1;

      BloomStrength = bloomStrength1;
      BloomRadius = bloomRadius1;

      if (BloomUseLuminance) bloomPassUpsampleLuminance.Apply();
      else bloomPassUpsample.Apply();
      quadRenderer.RenderQuad(Vector2.One * -1, Vector2.One);
    }

    //Note the final step could be done as a blend to the final texture.

    return bloomRenderTarget2DMip0;
  }

  private void ChangeBlendState()
  {
    Core.GraphicsDevice.BlendState = BlendState.AlphaBlend;
  }

  /// <summary>
  /// Update the InverseResolution of the used rendertargets. This should be the InverseResolution of the processed image
  /// We use SurfaceFormat.Color, but you can use higher precision buffers obviously.
  /// </summary>
  /// <param name="width">width of the image</param>
  /// <param name="height">height of the image</param>
  public void UpdateResolution(int width, int height)
  {
    _width = width;
    _height = height;

    if (bloomRenderTarget2DMip0 != null)
    {
      Dispose();
    }

    bloomRenderTarget2DMip0 = new RenderTarget2D(Core.GraphicsDevice,
        (int)(width),
        (int)(height), false, renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
    bloomRenderTarget2DMip1 = new RenderTarget2D(Core.GraphicsDevice,
        (int)(width / 2),
        (int)(height / 2), false, renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
    bloomRenderTarget2DMip2 = new RenderTarget2D(Core.GraphicsDevice,
        (int)(width / 4),
        (int)(height / 4), false, renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
    bloomRenderTarget2DMip3 = new RenderTarget2D(Core.GraphicsDevice,
        (int)(width / 8),
        (int)(height / 8), false, renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
    bloomRenderTarget2DMip4 = new RenderTarget2D(Core.GraphicsDevice,
        (int)(width / 16),
        (int)(height / 16), false, renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
    bloomRenderTarget2DMip5 = new RenderTarget2D(Core.GraphicsDevice,
        (int)(width / 32),
        (int)(height / 32), false, renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
  }

  /// <summary>
  ///Dispose our RenderTargets. This is not covered by the Garbage Collector so we have to do it manually
  /// </summary>
  public void Dispose()
  {
    bloomRenderTarget2DMip0.Dispose();
    bloomRenderTarget2DMip1.Dispose();
    bloomRenderTarget2DMip2.Dispose();
    bloomRenderTarget2DMip3.Dispose();
    bloomRenderTarget2DMip4.Dispose();
    bloomRenderTarget2DMip5.Dispose();
    GC.SuppressFinalize(this);
  }
}
