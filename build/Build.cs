using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nuke.Common;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using Polly;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.Docker.DockerTasks;

[TeamCity(
    Version = "2021.1",
    ManuallyTriggeredTargets = new[]
    {
        nameof(CompileAndPushJdk11),
        nameof(CompileAndPushTeamcityServer),
        nameof(CompileAndPushTeamcityAgent),
        nameof(CompileAndPushTeamcityAgentDotnet),
        nameof(DockerLogIn),
        nameof(DockerLogOut),
        nameof(CreateBuilder),
        nameof(PruneBuilderCache)
    },
    NonEntryTargets = new[]
    {
        nameof(BuildTeamcityCache),
        nameof(DownloadTeamcityBinaries)
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
            x => x.CompileAndPushJdk11);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("A pattern to process only specific operating systems")]
    readonly string[] MatchPatterns = {"**/Dockerfile"};

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

    Target CreateBuilder => _ => _
        .Executes(() =>
        {
            Docker("buildx create --name rpi --node rpi-node --platform linux/arm64,linux/arm/v6,linux/arm/v7 --driver docker-container");
        });

    Target PruneBuilderCache => _ => _
        .Executes(() =>
        {
            Docker("buildx prune -a -f --builder rpi");
        });

    Polly.Policy RetryPolicy => Policy.Handle<Exception>()
        .WaitAndRetry(10, attempt => TimeSpan.FromMinutes(attempt * attempt),
            (ex, _) => Console.WriteLine(ex));

    static (string[] values, AbsolutePath dockerfile)[] GetTagsToBuild(IReadOnlyCollection<AbsolutePath> dockerfiles, AbsolutePath baseDir, string moduleName)
    {
        var tagsToBuild = new (string[], AbsolutePath)[dockerfiles.Count];
        int i = 0;
        foreach (var dockerfile in dockerfiles)
        {
            tagsToBuild[i++] = (GetTags(dockerfile, baseDir, moduleName), dockerfile);
        }

        return tagsToBuild;
    }

    static string GetTagName(string pathToDockerfile, AbsolutePath baseDir, string moduleName)
    {
        var sb = new StringBuilder();
        sb.Append($"vlrpi/{moduleName}:");
        string[] separatedPath = pathToDockerfile.Split(Path.DirectorySeparatorChar);
        var parts = separatedPath.SkipWhile(it => it != moduleName).Skip(1).TakeWhile(it => it != "Dockerfile");
        var partsAfterExclude = ExcludeFromTag(parts, baseDir);
        sb.AppendJoin('-', partsAfterExclude);
        return sb.ToString();
    }

    static string[] GetTags(AbsolutePath pathToDockerfile, AbsolutePath baseDir, string moduleName)
    {
        AbsolutePath pathToTagsConfigFile = pathToDockerfile.Parent / ".tags";
        
        if (FileExists(pathToTagsConfigFile))
        {
            string[] items = File.ReadAllLines(pathToTagsConfigFile);
            var tags = new string[items.Length];

            int i = 0;
            foreach (string item in items)
            {
                var sb = new StringBuilder();
                sb.Append($"vlrpi/{moduleName}:");
                string[] separatedPath = pathToDockerfile.Parent!.Parent!.ToString().Split(Path.DirectorySeparatorChar);
                var parts = separatedPath.SkipWhile(it => it != moduleName).Skip(1).Append(item);
                var partsAfterExclude = ExcludeFromTag(parts, baseDir);
                sb.AppendJoin('-', partsAfterExclude);
                tags[i++] = sb.ToString();
            }

            return tags;
        }

        return new[] {GetTagName(pathToDockerfile, baseDir, moduleName)};
    }

    static IEnumerable<string> ExcludeFromTag(IEnumerable<string> parts, AbsolutePath baseDir)
    {
        foreach (var part in parts)
        {
            baseDir /= part;
            if (!FileExists(baseDir / ".exclude"))
                yield return part;
        }
    }
}
