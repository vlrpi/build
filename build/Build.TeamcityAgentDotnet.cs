using System;
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
    const string TeamcityAgentDotnetModuleName = "rpi-teamcity-agent-dotnet";
    AbsolutePath TeamcityAgentDotnetPath => RootDirectory / TeamcityAgentDotnetModuleName;

    Target CompileAndPushTeamcityAgentDotnet => _ => _
        .Executes(async () =>
        {
            var dockerfiles = TeamcityAgentDotnetPath.GlobFiles(MatchPatterns);
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityAgentDotnetPath, TeamcityAgentDotnetModuleName);
            
            var tasks = new List<Task>();
            
            foreach (var pair in tagsToBuild.GroupBy(it => (string)it.dockerfile.Parent!.Parent))
            {
                var (arm64Tags, arm64Dockerfile) = pair.First(it => ((string)it.dockerfile).Contains("arm64"));
                var (armTags, armDockerfile) = pair.First(it => ((string)it.dockerfile).Contains("arm"));

                Task t1 = Task.Factory.StartNew(() =>
                {
                    RetryPolicy.Execute(() =>
                    {
                        DockerBuildxBuild(_ => _
                            .SetPlatform("linux/arm64")
                            .SetTag(arm64Tags.Select(t => t.WithImage("teamcity-agent-dotnet-arm64v8")))
                            .AddBuildArg("BASE_ARCH=arm64v8")
                            .EnableRm()
                            .SetPath(arm64Dockerfile.Parent)
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
                            .SetTag(armTags.Select(t => t.WithImage("teamcity-agent-dotnet-arm32v7")))
                            .AddBuildArg("BASE_ARCH=arm32v7")
                            .EnableRm()
                            .SetPath(armDockerfile.Parent)
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
                        foreach (string tag in arm64Tags)
                        {
                            string tagWithImage = tag.WithImage("teamcity-agent-dotnet");
                            RetryPolicy.Execute(() =>
                            {
                                Docker($"buildx imagetools create --builder rpi -t {tagWithImage} {tag.WithImage("teamcity-agent-dotnet-arm64v8")} {tag.WithImage("teamcity-agent-dotnet-arm32v7")}");
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