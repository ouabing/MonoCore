using System;

namespace G;

public static class FileHelper
{
  public static string ResolvePath(string path)
  {
    if (path.StartsWith('~'))
    {
      return path.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }
    return path;
  }
}