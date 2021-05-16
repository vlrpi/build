using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    const string TeamcityServerModuleName = "rpi-teamcity-server";
    AbsolutePath TeamcityServerPath => RootDirectory / TeamcityServerModuleName;

    Target CompileTeamcityServer => _ => _
        .Executes(() =>
        {
            var dockerfiles = TeamcityServerPath.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityServerPath, TeamcityServerModuleName);
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

    Target PushTeamcityServer => _ => _
        .DependsOn(CompileTeamcityServer)
        .Executes(() =>
        {
            var dockerfiles = TeamcityServerPath.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityServerPath, TeamcityServerModuleName);
            foreach (var (tags, _) in tagsToBuild)
            {
                foreach (var tag in tags)
                {
                    Docker($"push {tag}");
                }
            }
        });
}