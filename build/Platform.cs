using System.Collections.Generic;
using System.ComponentModel;
using Nuke.Common.Tooling;

// ReSharper disable InconsistentNaming

[TypeConverter (typeof(TypeConverter<Platform>))]
public class Platform : Enumeration
{
  public static Platform x86 = new(nameof(x86));
  public static Platform x64 = new(nameof(x64));

  public static readonly IReadOnlyList<Platform> Values = new[] {x86, x64};

  public Platform (string value)
  {
    Value = value;
  }

  public static implicit operator string (Platform platform)
  {
    return platform.Value;
  }
}
