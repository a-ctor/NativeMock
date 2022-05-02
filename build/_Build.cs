using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using static Nuke.Common.IO.FileSystemTasks;

[GitHubActions(
  "release",
  GitHubActionsImage.WindowsLatest,
  OnPushTags = new[] { "v*" },
  PublishArtifacts = true,
  InvokedTargets = new[] { nameof(FullBuild) },
  CacheKeyFiles = new[] { "src/*/*.csproj", "tests/*/*.csproj", },
  EnableGitHubToken = false)]
[GitHubActions(
  "continuous",
  GitHubActionsImage.WindowsLatest,
  OnPushBranches = new[] { "master", },
  PublishArtifacts = true,
  InvokedTargets = new[] { nameof(FullBuild) },
  CacheKeyFiles = new[] { "src/*/*.csproj", "tests/*/*.csproj", },
  EnableGitHubToken = false)]
[CheckBuildProjectConfigurations]
partial class _Build : NukeBuild
{
  public static int Main() => Execute<_Build> (x => x.Build);

  [Parameter] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

  [Solution] readonly Solution Solution;

  [Solution] readonly Solution SolutionCpp;

  public AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
  
  public AbsolutePath LibDirectory => RootDirectory / "lib";

  Target Clean => _ => _
    .DependsOn (CleanArtifacts)
    .DependsOn (CleanCpp)
    .DependsOn (CleanCs);

  Target CleanArtifacts => _ => _
    .Executes (() =>
    {
      EnsureCleanDirectory (ArtifactsDirectory);
    });

  Target Restore => _ => _
    .DependsOn (RestoreCpp)
    .DependsOn (RestoreCs);

  Target Build => _ => _
    .DependsOn (BuildCpp)
    .DependsOn (BuildCs);

  Target Rebuild => _ => _
    .DependsOn (Clean)
    .DependsOn (Build);

  Target Test => _ => _
    .DependsOn (Build)
    .DependsOn (TestCs);

  Target Pack => _ => _
    .DependsOn (CleanArtifacts)
    .DependsOn (Build)
    .DependsOn (PackCs);

  Target FullBuild => _ => _
    .DependsOn (Clean)
    .DependsOn (Build)
    .DependsOn (Test)
    .DependsOn (Pack);
}
