using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    const string TeamcityAgentDotnetModuleName = "rpi-teamcity-agent-dotnet";
    AbsolutePath TeamcityAgentDotnetPath => RootDirectory / TeamcityAgentDotnetModuleName;

    Target CompileAndPushTeamcityAgentDotnet => _ => _
        .Executes(() =>
        {
            var dockerfiles = TeamcityAgentDotnetPath.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentDotnetPath, TeamcityAgentDotnetModuleName);
            foreach (var (tags, dockerfile) in tagsToBuild)
            {
                DockerBuildxBuild(_ => _
                    .SetPlatform("linux/arm64,linux/arm/v7,linux/arm/v6")
                    .SetTag(tags)
                    .EnableRm()
                    .SetPath(dockerfile.Parent)
                    .SetBuilder("rpi")
                    .EnablePull()
                    .EnablePush());
            }
        });

    // Target PushTeamcityAgentDotnet => _ => _
    //     .DependsOn(CompileAndPushTeamcityAgentDotnet)
    //     .Executes(() =>
    //     {
    //         var dockerfiles = TeamcityAgentDotnetPath.GlobFiles("**/Dockerfile");
    //         var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentDotnetPath, TeamcityAgentDotnetModuleName);
    //         foreach (var (tags, _) in tagsToBuild)
    //         {
    //             foreach (var tag in tags)
    //             {
    //                 Docker($"push {tag}");
    //             }
    //         }
    //     });
}