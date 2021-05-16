using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    const string TeamcityAgentModuleName = "rpi-teamcity-agent";
    AbsolutePath TeamcityAgentPath => RootDirectory / TeamcityAgentModuleName;

    Target CompileTeamcityAgent => _ => _
        .Executes(() =>
        {
            var dockerfiles = TeamcityAgentPath.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentPath, TeamcityAgentModuleName);
            foreach (var (tags, dockerfile) in tagsToBuild)
            {
                DockerBuildxBuild(_ => _
                    .SetPlatform("linux/arm64")
                    .SetTag(tags)
                    .EnableRm()
                    .SetPath(dockerfile.Parent)
                    .EnablePull());
            }
        });

    Target PushTeamcityAgent => _ => _
        .DependsOn(CompileTeamcityAgent)
        .Executes(() =>
        {
            var dockerfiles = TeamcityAgentPath.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentPath, TeamcityAgentModuleName);
            foreach (var (tags, _) in tagsToBuild)
            {
                foreach (var tag in tags)
                {
                    Docker($"push {tag}");
                }
            }
        });
}