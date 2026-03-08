using System.Diagnostics.CodeAnalysis;

namespace SudokuBlazor.Constants;

public readonly struct Theme(string name, string? tag) : IEquatable<Theme>
{
  public string Name { get; } = name;
  public string? Tag { get; } = tag;

  public static readonly Theme System = new("System", "system");
  public static readonly Theme Light = new("Light", "light");
  public static readonly Theme Dark = new("Dark", "dark");
  public static readonly Theme Amanda = new("Amanda", "amanda");

  public static readonly IReadOnlyList<Theme> Themes =
  [
    System, Light, Dark, Amanda
  ];

  public static Theme? ThemeOf(string? tag)
  {
    if (string.IsNullOrWhiteSpace(tag))
      return System;
    return Themes.FirstOrDefault(theme => theme.Tag == tag);
  }

  public static bool TryGetTheme(string? tag, out Theme theme)
  {
    Theme? t = ThemeOf(tag);
    if (t.HasValue)
    {
      theme = t.Value;
      return true;
    }
    theme = System;
    return false;
  }

  public bool Equals(Theme other)
    => string.Equals(Tag, other.Tag, StringComparison.Ordinal);

  public override bool Equals([NotNullWhen(true)] object? obj)
    => obj is Theme other && Equals(other);

  public override int GetHashCode()
    => Tag is null ? 0 : StringComparer.Ordinal.GetHashCode(Tag);

  public static bool operator ==(Theme left, Theme right) => left.Equals(right);
  public static bool operator !=(Theme left, Theme right) => !(left == right);
}
