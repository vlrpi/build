using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    const string TeamcityAgentDotnetModuleName = "rpi-teamcity-agent-dotnet";
    AbsolutePath TeamcityAgentDotnetPath => RootDirectory / TeamcityAgentDotnetModuleName;

    Target CompileTeamcityAgentDotnet => _ => _
        .DependsOn(DockerLogIn)
        .Executes(() =>
        {
            var dockerfiles = TeamcityAgentDotnetPath.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentDotnetModuleName);
            foreach (var (tags, dockerfile) in tagsToBuild)
            {
                DockerBuild(_ => _
                    .SetPlatform("arm64")
                    .EnablePull()
                    .SetTag(tags)
                    .SetPath(dockerfile.Parent));
            }
        });

    Target PushTeamcityAgentDotnet => _ => _
        .DependsOn(CompileTeamcityAgentDotnet)
        .Triggers(DockerLogOut)
        .Executes(() =>
        {
            var dockerfiles = TeamcityAgentDotnetPath.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentDotnetModuleName);
            foreach (var (tags, _) in tagsToBuild)
            {
                foreach (var tag in tags)
                {
                    Docker($"push {tag}");
                }
            }
        });
}