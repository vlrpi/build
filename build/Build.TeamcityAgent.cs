using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using Utils;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    const string TeamcityAgentModuleName = "rpi-teamcity-agent";
    AbsolutePath TeamcityAgentPath => RootDirectory / TeamcityAgentModuleName;

    Target CompileAndPushTeamcityAgent => _ => _
        .Executes(() =>
        {
            var dockerfiles = TeamcityAgentPath.GlobFiles(MatchPatterns);
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentPath, TeamcityAgentModuleName);
            foreach (var (tags, dockerfile) in tagsToBuild)
            {
                RetryPolicy.Execute(() =>
                {
                    DockerBuildxBuild(_ => _
                        .SetPlatform("linux/arm64")
                        .SetTag(tags.Select(t => t.WithImage("teamcity-agent-arm64v8")))
                        .AddBuildArg("BASE_ARCH=arm64v8")
                        .AddBuildArg("DOCKER_ARCH=arm64")
                        .EnableRm()
                        .SetPath(dockerfile.Parent)
                        .SetBuilder("rpi")
                        .EnablePull()
                        .EnablePush());
                });
                RetryPolicy.Execute(() =>
                {
                    DockerBuildxBuild(_ => _
                        .SetPlatform("linux/arm/v7")
                        .SetTag(tags.Select(t => t.WithImage("teamcity-agent-arm32v7")))
                        .AddBuildArg("BASE_ARCH=arm32v7")
                        .AddBuildArg("DOCKER_ARCH=armhf")
                        .EnableRm()
                        .SetPath(dockerfile.Parent)
                        .SetBuilder("rpi")
                        .EnablePull()
                        .EnablePush());
                });
            }
        });
}