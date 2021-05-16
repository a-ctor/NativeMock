using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class _Build
{
  Target CleanCs => _ => _
    .Executes (() =>
    {
      DotNetClean (s => s
        .SetProject (Solution)
        .SetConfiguration (Configuration));
    });

  Target RestoreCs => _ => _
    .After (CleanCs)
    .Executes (() =>
    {
      DotNetRestore (s => s
        .SetProjectFile (Solution));
    });

  Target BuildCs => _ => _
    .After (BuildCpp)
    .DependsOn (RestoreCs)
    .Executes (() =>
    {
      DotNetBuild (s => s
        .SetProjectFile (Solution)
        .SetConfiguration (Configuration)
        .SetNoRestore (true));
    });

  Target RebuildCs => _ => _
    .DependsOn (CleanCs)
    .DependsOn (BuildCs);

  Target TestCs => _ => _
    .DependsOn (BuildCs)
    .Executes (() =>
    {
      DotNetTest (s => s
        .SetProjectFile (Solution)
        .SetConfiguration (Configuration)
        .SetNoRestore (true)
        .SetNoBuild (true));
    });

  Target PackCs => _ => _
    .DependsOn (CleanArtifacts)
    .DependsOn (BuildCs)
    .Executes (() =>
    {
      DotNetPack (s => s
        .SetProject (Solution)
        .SetConfiguration (Configuration)
        .SetNoBuild (true)
        .SetNoRestore (true)
        .SetOutputDirectory (ArtifactsDirectory));
    });
}
