using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    const string TeamcityServerModuleName = "rpi-teamcity-server";
    AbsolutePath TeamcityServerPath => RootDirectory / TeamcityServerModuleName;

    Target CompileAndPushTeamcityServer => _ => _
        .Executes(() =>
        {
            var dockerfiles = TeamcityServerPath.GlobFiles(MatchPatterns);
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityServerPath, TeamcityServerModuleName);
            foreach (var (tags, dockerfile) in tagsToBuild)
            {
                RetryPolicy.Execute(() =>
                {
                    DockerBuildxBuild(_ => _
                        .SetPlatform("linux/arm64,linux/arm/v7,linux/arm/v6")
                        .SetTag(tags)
                        .EnableRm()
                        .SetPath(TeamcityServerPath)
                        .SetFile(dockerfile)
                        .SetBuilder("rpi")
                        .EnablePull()
                        .EnablePush());
                });
            }
        });
}