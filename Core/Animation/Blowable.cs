using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

/**
  * Used to represent an object that can be blown by WindSim.
  *
  * The output TopLeftOffset and TopRightOffset are used to draw the vertices of the object,
  * which you should draw them with texture primitives.
  */
public class Blowable
{
  public Vector2 TopLeftOffset { get; private set; }
  public Vector2 TopRightOffset { get; private set; }
  public float MaxAngle { get; private set; }
  public float maxRotateSpeed { get; private set; }
  public float Angle { get; private set; }
  private float nextVibrationDuration;
  private float timer;

  public Blowable(float maxAngle = 90f, float maxRotateSpeed = 5f)
  {
    MaxAngle = MathHelper.ToRadians(maxAngle);
    this.maxRotateSpeed = MathHelper.ToRadians(maxRotateSpeed);
    DetermineNextVibrationDuration();
    timer = nextVibrationDuration;
  }

  public void Blow()
  {
    var windForce = Core.Wind.Force;
    var windDirection = Core.Wind.Direction;

    Angle += windForce * windDirection.X * Core.Random.NextSingle(0f, maxRotateSpeed);
    Angle = MathHelper.Clamp(Angle, -MaxAngle, MaxAngle);
    TopLeftOffset = RotateVectorAroundPoint(new Vector2(-0.5f, -1f), new Vector2(0f, 0f), Angle) - new Vector2(-0.5f, -1);
    TopRightOffset = RotateVectorAroundPoint(new Vector2(0.5f, -1f), new Vector2(0f, 0f), Angle) - new Vector2(0.5f, -1f);
  }

  public void Update(GameTime gameTime)
  {
    if (timer > nextVibrationDuration)
    {
      Blow();
      timer = 0f;
      DetermineNextVibrationDuration();
    }

    timer += gameTime.GetElapsedSeconds();
  }

  private static Vector2 RotateVectorAroundPoint(Vector2 point, Vector2 center, float angle)
  {
    // translate the point so that the center of rotation is at the origin
    Vector2 translatedPoint = point - center;

    // apply the rotation
    float cosTheta = (float)Math.Cos(angle);
    float sinTheta = (float)Math.Sin(angle);

    float rotatedX = translatedPoint.X * cosTheta - translatedPoint.Y * sinTheta;
    float rotatedY = translatedPoint.X * sinTheta + translatedPoint.Y * cosTheta;

    Vector2 rotatedPoint = new Vector2(rotatedX, rotatedY);

    // translate the point back
    Vector2 finalPosition = rotatedPoint + center;

    return finalPosition;
  }

  private void DetermineNextVibrationDuration()
  {
    nextVibrationDuration = Core.Random.NextSingle(0.001f, 0.05f);
  }
}