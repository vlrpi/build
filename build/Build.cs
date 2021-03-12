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
    ManuallyTriggeredTargets = new[] {/*nameof(Push), */nameof(PushJdk11)},
    NonEntryTargets = new[] {/*nameof(Compile), */nameof(CompileJdk11)}
)]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.CompileJdk11);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    const string Jdk11ModuleName = "rpi-jdk11";
    AbsolutePath Jdk11Path => RootDirectory / Jdk11ModuleName;
    Target CompileJdk11 => _ => _
        .DependsOn(DockerLogIn)
        .Executes(() =>
        {
            var dockerfiles = Jdk11Path.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, Jdk11ModuleName);
            foreach (var (tagToBuild, dockerfile) in tagsToBuild)
            {
                DockerBuild(_ => _
                    .SetPlatform("arm64")
                    .EnablePull()
                    .SetTag(tagToBuild)
                    .SetPath(dockerfile.Parent));
            }
        });

    Target PushJdk11 => _ => _
        .DependsOn(CompileJdk11)
        .Triggers(DockerLogOut)
        .Executes(() =>
        {
            var dockerfiles = Jdk11Path.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, Jdk11ModuleName);
            foreach (var (tagToBuild, _) in tagsToBuild)
            {
                Docker($"push {tagToBuild}");
            }
        });

    /*Target Compile => _ => _
        .Executes(() =>
        {
        });

    Target Push => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
        });*/

    Target DockerLogIn => _ => _
        .Executes(() =>
        {
            DockerLogin(_ => _
                .SetUsername(Environment.GetEnvironmentVariable("DOCKER_HUB_USERNAME"))
                .SetPassword(Environment.GetEnvironmentVariable("DOCKER_HUB_PASSWORD")));
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
