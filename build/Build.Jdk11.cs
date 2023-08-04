using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        .Executes(async () =>
        {
            var dockerfiles = Jdk11Path.GlobFiles(MatchPatterns);
            var tagsToBuild = GetTagsToBuild(dockerfiles, Jdk11Path, Jdk11ModuleName);

            var tasks = new List<Task>();
            
            foreach (var (tags, dockerfile) in tagsToBuild)
            {
                Task t1 = Task.Factory.StartNew(() =>
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
                }, TaskCreationOptions.LongRunning);

                Task t2 = Task.Factory.StartNew(() =>
                {
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
                }, TaskCreationOptions.LongRunning);

                Task tAll = Task.WhenAll(t1, t2);

                if (!SkipManifests)
                {
                    Task tManifest = tAll.ContinueWith(_ =>
                    {
                        foreach (string tag in tags)
                        {
                            string tagWithImage = tag.WithImage("jdk");
                            RetryPolicy.Execute(() =>
                            {
                                Docker($"buildx imagetools create --builder rpi -t {tagWithImage} {tag.WithImage("jdk-arm64v8")} {tag.WithImage("jdk-arm32v7")}");
                            });
                        }
                    }, TaskContinuationOptions.LongRunning);
                    
                    tasks.Add(tManifest);
                }
                else
                {
                    tasks.Add(tAll);
                }
            }

            await Task.WhenAll(tasks);
        });
}