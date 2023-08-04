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
    const string TeamcityAgentModuleName = "rpi-teamcity-agent";
    AbsolutePath TeamcityAgentPath => RootDirectory / TeamcityAgentModuleName;

    Target CompileAndPushTeamcityAgent => _ => _
        .Executes(async () =>
        {
            var dockerfiles = TeamcityAgentPath.GlobFiles(MatchPatterns);
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentPath, TeamcityAgentModuleName);
            
            var tasks = new List<Task>();
            
            foreach (var (tags, dockerfile) in tagsToBuild.AsParallel())
            {
                Task t1 = Task.Factory.StartNew(() =>
                {
                    RetryPolicy.Execute(() =>
                    {
                        DockerBuildxBuild(_ => _
                            .SetPlatform("linux/arm64")
                            .SetTag(tags.Select(t => t.WithImage("teamcity-agent-arm64v8")))
                            .AddBuildArg("BASE_ARCH=arm64v8")
                            .AddBuildArg("DOCKER_ARCH=aarch64")
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
                            .SetTag(tags.Select(t => t.WithImage("teamcity-agent-arm32v7")))
                            .AddBuildArg("BASE_ARCH=arm32v7")
                            .AddBuildArg("DOCKER_ARCH=armv7")
                            .EnableRm()
                            .SetPath(dockerfile.Parent)
                            .SetBuilder("rpi")
                            .EnablePull()
                            .EnablePush());
                    });
                }, TaskCreationOptions.LongRunning);
                
                var tAll = Task.WhenAll(t1, t2);

                if (!SkipManifests)
                {
                    Task tManifest = tAll.ContinueWith(_ =>
                    {
                        foreach (string tag in tags)
                        {
                            string tagWithImage = tag.WithImage("teamcity-agent");
                            RetryPolicy.Execute(() =>
                            {
                                Docker($"buildx imagetools create --builder rpi -t {tagWithImage} {tag.WithImage("teamcity-agent-arm64v8")} {tag.WithImage("teamcity-agent-arm32v7")}");
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