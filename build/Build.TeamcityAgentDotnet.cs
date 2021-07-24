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
            var dockerfiles = TeamcityAgentDotnetPath.GlobFiles(MatchPatterns);
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentDotnetPath, TeamcityAgentDotnetModuleName);
            foreach (var (tags, dockerfile) in tagsToBuild)
            {
                RetryPolicy.Execute(() =>
                {
                    DockerBuildxBuild(_ => _
                        .SetPlatform("linux/arm64,linux/arm/v7,linux/arm/v6")
                        .SetTag(tags)
                        .EnableRm()
                        .SetPath(dockerfile.Parent)
                        .SetBuilder("rpi")
                        .EnablePull()
                        .EnablePush());
                });
            }
        });
}