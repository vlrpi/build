using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    const string TeamcityServerModuleName = "rpi-teamcity-server";
    AbsolutePath TeamcityServerPath => RootDirectory / TeamcityServerModuleName;

    Target CompileTeamcityServer => _ => _
        .DependsOn(DockerLogIn)
        .Executes(() =>
        {
            var dockerfiles = TeamcityServerPath.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityServerModuleName);
            foreach (var (tags, dockerfile) in tagsToBuild)
            {
                DockerBuild(_ => _
                    .SetPlatform("arm64")
                    .EnablePull()
                    .SetTag(tags)
                    .SetPath(dockerfile.Parent));
            }
        });

    Target PushTeamcityServer => _ => _
        .DependsOn(CompileTeamcityServer)
        .Triggers(DockerLogOut)
        .Executes(() =>
        {
            var dockerfiles = TeamcityServerPath.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityServerModuleName);
            foreach (var (tags, _) in tagsToBuild)
            {
                foreach (var tag in tags)
                {
                    Docker($"push {tag}");
                }
            }
        });
}