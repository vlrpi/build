using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using Utils;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    const string Jdk11ModuleName = "rpi-jdk11";
    AbsolutePath Jdk11Path => RootDirectory / Jdk11ModuleName;

    Target CompileAndPushJdk11 => _ => _
        .Executes(() =>
        {
            var dockerfiles = Jdk11Path.GlobFiles(MatchPatterns);
            var tagsToBuild = GetTagsToBuild(dockerfiles, Jdk11Path, Jdk11ModuleName);
            foreach (var (tags, dockerfile) in tagsToBuild)
            {
                RetryPolicy.Execute(() =>
                {
                    DockerBuildxBuild(_ => _
                        .SetPlatform("linux/arm64")
                        .SetTag(tags.Select(t => t.WithImage("jdk-arm64v8")))
                        .AddBuildArg("BALENALIB_ARCH=aarch64")
                        .AddBuildArg("JAVA_ARCH=arm64")
                        .EnableRm()
                        .SetPath(dockerfile.Parent)
                        .SetBuilder("rpi")
                        .EnablePull()
                        .EnablePush());
                });

                RetryPolicy.Execute(() =>
                {
                    DockerBuildxBuild(_ => _
                        .SetPlatform("linux/arm/v7")
                        .SetTag(tags.Select(t => t.WithImage("jdk-arm32v7")))
                        .AddBuildArg("BALENALIB_ARCH=armv7hf")
                        .AddBuildArg("JAVA_ARCH=armhf")
                        .EnableRm()
                        .SetPath(dockerfile.Parent)
                        .SetBuilder("rpi")
                        .EnablePull()
                        .EnablePush());
                });

                foreach (string tag in tags)
                {
                    string tagWithImage = tag.WithImage("jdk");
                    RetryPolicy.Execute(() =>
                    {
                        Docker($"manifest create {tagWithImage} --amend {tag.WithImage("jdk-arm64v8")} --amend {tag.WithImage("jdk-arm32v7")}");
                        DockerManifestPush(_ => _
                            .SetManifestList(tagWithImage));
                    });
                }
            }
        });
}