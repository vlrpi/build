using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nuke.Common;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

[TeamCity(
    TeamCityAgentPlatform.Unix,
    Version = "2020.2",
    ManuallyTriggeredTargets = new[]
    {
        nameof(PushJdk11),
        nameof(PushTeamcityAgent),
        nameof(PushTeamcityServer),
        nameof(PushTeamcityAgentDotnet)
    },
    NonEntryTargets = new[]
    {
        nameof(CompileJdk11),
        nameof(CompileTeamcityServer),
        nameof(CompileTeamcityAgent),
        nameof(CompileTeamcityAgentDotnet),
        nameof(DockerLogIn),
        nameof(DockerLogOut)
    }
)]
partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() =>
        Execute<Build>(
            x => x.PushJdk11,
            x => x.PushTeamcityAgent,
            x => x.PushTeamcityServer,
            x => x.PushTeamcityAgentDotnet);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    Target DockerLogIn => _ => _
        .Executes(() =>
        {
            DockerLogin(_ => _
                .SetUsername(Environment.GetEnvironmentVariable("DOCKER_HUB_USERNAME"))
                .SetPassword(Environment.GetEnvironmentVariable("DOCKER_HUB_PASSWORD"))
            );
        });
    
    Target DockerLogOut => _ => _
        .Executes(() =>
        {
            DockerLogout();
        });

    static (string tagToBuild, AbsolutePath dockerfile)[] GetTagsToBuild(IReadOnlyCollection<AbsolutePath> dockerfiles, string moduleName)
    {
        var tagsToBuild = new (string, AbsolutePath)[dockerfiles.Count];
        int i = 0;
        foreach (var dockerfile in dockerfiles)
        {
            tagsToBuild[i++] = (GetTagName(dockerfile, moduleName), dockerfile);
        }

        return tagsToBuild;
    }

    static string GetTagName(string pathToDockerfile, string moduleName)
    {
        var sb = new StringBuilder();
        sb.Append($"vlrpi/{moduleName}:");
        string[] separatedPath = pathToDockerfile.Split(Path.DirectorySeparatorChar);
        sb.AppendJoin('-', separatedPath.SkipWhile(it => it != moduleName).Skip(1).TakeWhile(it => it != "Dockerfile"));
        return sb.ToString();
    }
}
