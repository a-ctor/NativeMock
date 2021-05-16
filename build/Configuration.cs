using System.ComponentModel;
using Nuke.Common.Tooling;

[TypeConverter (typeof(TypeConverter<Configuration>))]
public class Configuration : Enumeration
{
  public static Configuration Debug = new(nameof(Debug));
  public static Configuration Release = new(nameof(Release));

  public Configuration (string value)
  {
    Value = value;
  }

  public static implicit operator string (Configuration configuration)
  {
    return configuration.Value;
  }
}
