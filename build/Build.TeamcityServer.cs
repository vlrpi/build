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
            foreach (var (tagToBuild, dockerfile) in tagsToBuild)
            {
                DockerBuild(_ => _
                    .SetPlatform("arm64")
                    .EnablePull()
                    .SetTag(tagToBuild)
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
            foreach (var (tagToBuild, _) in tagsToBuild)
            {
                Docker($"push {tagToBuild}");
            }
        });
}