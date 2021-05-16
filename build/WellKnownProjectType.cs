using System;
using System.ComponentModel;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

[TypeConverter (typeof(TypeConverter<WellKnownProjectType>))]
public class WellKnownProjectType : Enumeration
{
  public static WellKnownProjectType CSharp = new(nameof(CSharp));
  public static WellKnownProjectType CSharpWithPlatforms = new(nameof(CSharpWithPlatforms));
  public static WellKnownProjectType CPlusPlus = new(nameof(CPlusPlus));

  public WellKnownProjectType (string value)
  {
    Value = value;
  }

  public RelativePath GetRelativeOutputDirectory (Configuration configuration, Platform platform)
  {
    var bin = (RelativePath) "bin";
    return this switch
    {
      _ when this == CSharp => bin / configuration,
      _ when this == CSharpWithPlatforms => bin / platform / configuration,
      _ when this == CPlusPlus => bin / platform / configuration,
      _ => throw new ArgumentOutOfRangeException()
    };
  }

  public static implicit operator string (WellKnownProjectType configuration)
  {
    return configuration.Value;
  }
}
