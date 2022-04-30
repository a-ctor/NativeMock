using System.IO;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

partial class _Build
{
  // We need to use VS Msbuild to build the C++ projects
  [Parameter] readonly string MsBuildPath = MSBuildToolPathResolver.Resolve (MSBuildVersion.VS2019, MSBuildPlatform.x86);

  Target CleanCpp => _ => _
    .Requires (() => MsBuildPath)
    .Executes (() =>
    {
      MSBuild (s => s
        .SetProcessToolPath (MsBuildPath)
        .SetProjectFile (SolutionCpp)
        .SetTargets ("Clean")
        .SetConfiguration (Configuration)
        .CombineWith (Platform.Values, (oo, v) => oo
          .SetProperty ("Platform", (string) v)));

      foreach (var platform in Platform.Values)
        DeleteDirectory (LibDirectory / platform);
    });

  Target RestoreCpp => _ => _
    .After (CleanCpp)
    .Executes (() =>
    {
      MSBuild (s => s
        .SetProcessToolPath (MsBuildPath)
        .SetProjectFile (SolutionCpp)
        .SetTargets ("Restore")
        .SetConfiguration (Configuration)
        .CombineWith (Platform.Values, (oo, v) => oo
          .SetProperty ("Platform", (string) v)));
    });

  Target BuildCpp => _ => _
    .DependsOn (RestoreCpp)
    .Executes (() =>
    {
      MSBuild (s => s
        .SetProcessToolPath (MsBuildPath)
        .SetProjectFile (SolutionCpp)
        .SetTargets ("Build")
        .SetConfiguration (Configuration)
        .CombineWith (Platform.Values, (oo, v) => oo
          .SetProperty ("Platform", (string) v)));

      var integrationTestDriverProject = WellKnownProject.IntegrationTestDriver;
      var integrationTestDriverProjectPath = SolutionCpp.GetProject (integrationTestDriverProject)!.Directory;

      var dummyDllProject = WellKnownProject.DummyDll;
      var dummyDllProjectPath = SolutionCpp.GetProject (dummyDllProject)!.Directory;

      var nativeProject = WellKnownProject.Native;
      var nativeProjectPath = SolutionCpp.GetProject (nativeProject)!.Directory;

      foreach (var platform in Platform.Values)
      {
        // Copy output files of NativeMock.IntegrationTests.Driver to lib
        CopyFile (
          integrationTestDriverProjectPath / integrationTestDriverProject.ProjectType.GetRelativeOutputDirectory (Configuration, platform) / integrationTestDriverProject.OutputArtifactName,
          LibDirectory / platform / integrationTestDriverProject.OutputArtifactName,
          FileExistsPolicy.Overwrite);

        // Copy output files of NativeMock.DummyDll to lib
        CopyFile (
          dummyDllProjectPath / dummyDllProject.ProjectType.GetRelativeOutputDirectory (Configuration, platform) / dummyDllProject.OutputArtifactName,
          LibDirectory / platform / dummyDllProject.OutputArtifactName,
          FileExistsPolicy.Overwrite);

        // Copy output files of NativeMock.Native to lib
        CopyFile (
          nativeProjectPath / nativeProject.ProjectType.GetRelativeOutputDirectory (Configuration, platform) / nativeProject.OutputArtifactName,
          LibDirectory / platform / nativeProject.OutputArtifactName,
          FileExistsPolicy.Overwrite);

        // Copy output files of NativeMock.Native to lib
        var symbolName = Path.ChangeExtension (nativeProject.OutputArtifactName, ".pdb");
        CopyFile (
          nativeProjectPath / nativeProject.ProjectType.GetRelativeOutputDirectory (Configuration, platform) / symbolName,
          LibDirectory / platform / symbolName,
          FileExistsPolicy.Overwrite);
      }
    });

  Target RebuildCpp => _ => _
    .DependsOn (CleanCpp)
    .DependsOn (BuildCpp);
}
