using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    const string Jdk11ModuleName = "rpi-jdk11";
    AbsolutePath Jdk11Path => RootDirectory / Jdk11ModuleName;
    Target CompileJdk11 => _ => _
        .Executes(() =>
        {
            var dockerfiles = Jdk11Path.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, Jdk11Path, Jdk11ModuleName);
            foreach (var (tags, dockerfile) in tagsToBuild)
            {
                DockerBuildxBuild(_ => _
                    .SetPlatform("linux/arm64")
                    .SetTag(tags)
                    .SetPath(dockerfile.Parent)
                    .EnablePull());
            }
        });

    Target PushJdk11 => _ => _
        .DependsOn(CompileJdk11)
        .Executes(() =>
        {
            var dockerfiles = Jdk11Path.GlobFiles("**/Dockerfile");
            var tagsToBuild = GetTagsToBuild(dockerfiles, Jdk11Path, Jdk11ModuleName);
            foreach (var (tags, _) in tagsToBuild)
            {
                foreach (var tag in tags)
                {
                    Docker($"push {tag}");
                }
            }
        });
}