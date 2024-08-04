using System.Collections.Generic;

namespace G;

public class InputBinding : IEnumerable<KeyValuePair<string, List<string>>>
{
  private readonly Dictionary<string, List<string>> bindings = [];

  public List<string> this[string key]
  {
    get => bindings[key];
    set => bindings[key] = value;
  }

  public IEnumerator<KeyValuePair<string, List<string>>> GetEnumerator()
  {
    return bindings.GetEnumerator();
  }

  public void Add(string key, List<string> value)
  {
    bindings.Add(key, value);
  }

  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}