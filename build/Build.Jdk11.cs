using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    const string Jdk11ModuleName = "rpi-jdk11";
    AbsolutePath Jdk11Path => RootDirectory / Jdk11ModuleName;
    Target CompileJdk11 => _ => _
        .DependsOn(DockerLogIn)
        .Executes(() =>
        {
            var dockerfiles = Jdk11Path.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, Jdk11ModuleName);
            foreach (var (tagToBuild, dockerfile) in tagsToBuild)
            {
                DockerBuild(_ => _
                    .SetPlatform("arm64")
                    .EnablePull()
                    .SetTag(tagToBuild)
                    .SetPath(dockerfile.Parent));
            }
        });

    Target PushJdk11 => _ => _
        .DependsOn(CompileJdk11)
        .Triggers(DockerLogOut)
        .Executes(() =>
        {
            var dockerfiles = Jdk11Path.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, Jdk11ModuleName);
            foreach (var (tagToBuild, _) in tagsToBuild)
            {
                Docker($"push {tagToBuild}");
            }
        });
}