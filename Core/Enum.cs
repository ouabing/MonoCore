namespace G;


#region Customizable section
// The order is the priority of the container
public enum ContainerDef
{
  Scene = 1,
  World = 2,
  UI = 3,
}


public enum FontSize
{
  Mini = 8,
  Small = 10,
  Medium = 12,
  Large = 24,
  Title = 64
}
#endregion Customizable section

public enum DrawAlignment
{
  TopLeft = 1,
  TopCenter = 2,
  TopRight = 3,
  Left = 4,
  Center = 5,
  Right = 6,
  BottomLeft = 7,
  BottomCenter = 8,
  BottomRight = 9,
}

public enum Direction
{
  Up = 1,
  Down = 2,
  Left = 3,
  Right = 4,
}

public enum XY
{
  X = 1,
  Y = 2
}