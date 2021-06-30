using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    const string TeamcityAgentModuleName = "rpi-teamcity-agent";
    AbsolutePath TeamcityAgentPath => RootDirectory / TeamcityAgentModuleName;

    Target CompileAndPushTeamcityAgent => _ => _
        .Executes(() =>
        {
            var dockerfiles = TeamcityAgentPath.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentPath, TeamcityAgentModuleName);
            foreach (var (tags, dockerfile) in tagsToBuild)
            {
                DockerBuildxBuild(_ => _
                    .SetPlatform("linux/arm64,linux/arm/v7,linux/arm/v6")
                    .SetTag(tags)
                    .EnableRm()
                    .SetPath(dockerfile.Parent)
                    .EnablePull()
                    .EnablePush());
            }
        });

    // Target PushTeamcityAgent => _ => _
    //     .DependsOn(CompileTeamcityAgent)
    //     .Executes(() =>
    //     {
    //         var dockerfiles = TeamcityAgentPath.GlobFiles("**/Dockerfile");
    //         var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentPath, TeamcityAgentModuleName);
    //         foreach (var (tags, _) in tagsToBuild)
    //         {
    //             foreach (var tag in tags)
    //             {
    //                 Docker($"push {tag}");
    //             }
    //         }
    //     });
}