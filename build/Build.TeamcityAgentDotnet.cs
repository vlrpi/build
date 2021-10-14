using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using Utils;
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
                string platform, baseArch;
                if (dockerfile.Contains("arm64"))
                {
                    platform = "linux/arm64";
                    baseArch = "arm64v8";
                }
                else if (dockerfile.Contains("arm"))
                {
                    platform = "linux/arm/v7";
                    baseArch = "arm32v7";
                }
                else
                {
                    throw new NotSupportedException(dockerfile);
                }
                RetryPolicy.Execute(() =>
                {
                    DockerBuildxBuild(_ => _
                        .SetPlatform(platform)
                        .SetTag(tags.Select(t => t.WithImage($"teamcity-agent-dotnet-{baseArch}")))
                        .AddBuildArg($"BASE_ARCH={baseArch}")
                        .EnableRm()
                        .SetPath(dockerfile.Parent)
                        .SetBuilder("rpi")
                        .EnablePull()
                        .EnablePush());
                });
            }
        });
}