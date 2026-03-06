namespace SudokuBlazor;

public class Class
{
  private HashSet<string> Classes { get; set; } = [];

  public Class(params List<string> classes)
    => Add(classes);

  public Class Add(params List<string> classes)
  {
    var set = classes.SelectMany(c => c.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
    foreach (string item in set)
      Classes.Add(item);
    return this;
  }
  public Class Add(Class other)
  {
    Classes.UnionWith(other.Classes);
    return this;
  }

  public Class AddIf(bool condition, Class trueClass, Class? falseClass = null)
  {
    if (condition == true)
      return Add(trueClass);
    if (condition == false && falseClass is not null)
      return Add(falseClass);
    return this;
  }

  public static Class Is(params List<string> classes)
    => new(classes);
  public static Class If(bool condition, Class trueClass, Class? falseClass = null)
    => new Class().AddIf(condition, trueClass, falseClass);

  public override string ToString()
    => string.Join(' ', Classes);

  public static implicit operator string(Class c) => c.ToString();
  public static implicit operator Class(string s) => Is(s);
}
