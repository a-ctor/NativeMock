using System.ComponentModel;
using Nuke.Common.Tooling;

[TypeConverter (typeof(TypeConverter<WellKnownProject>))]
public class WellKnownProject : Enumeration
{
  public static WellKnownProject IntegrationTestDriver = new("NativeMock.IntegrationTests.Driver", WellKnownProjectType.CPlusPlus);
  public static WellKnownProject DummyDll = new("NativeMock.DummyDll", WellKnownProjectType.CPlusPlus);
  public static WellKnownProject Native = new("NativeMock.Native", WellKnownProjectType.CPlusPlus);

  public WellKnownProjectType ProjectType { get; }

  public string OutputArtifactName => $"{Value}.dll";

  public WellKnownProject (string value, WellKnownProjectType projectType)
  {
    Value = value;
    ProjectType = projectType;
  }

  public static implicit operator string (WellKnownProject configuration)
  {
    return configuration.Value;
  }
}
