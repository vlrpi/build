using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    const string TeamcityAgentModuleName = "rpi-teamcity-agent";
    AbsolutePath TeamcityAgentPath => RootDirectory / TeamcityAgentModuleName;

    Target CompileTeamcityAgent => _ => _
        .DependsOn(DockerLogIn)
        .Executes(() =>
        {
            var dockerfiles = TeamcityAgentPath.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentModuleName);
            foreach (var (tagToBuild, dockerfile) in tagsToBuild)
            {
                DockerBuild(_ => _
                    .SetPlatform("arm64")
                    .EnablePull()
                    .SetTag(tagToBuild)
                    .SetPath(dockerfile.Parent));
            }
        });

    Target PushTeamcityAgent => _ => _
        .DependsOn(CompileTeamcityAgent)
        .Triggers(DockerLogOut)
        .Executes(() =>
        {
            var dockerfiles = TeamcityAgentPath.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentModuleName);
            foreach (var (tagToBuild, _) in tagsToBuild)
            {
                Docker($"push {tagToBuild}");
            }
        });
}