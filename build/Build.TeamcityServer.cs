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
    const string TeamcityServerModuleName = "rpi-teamcity-server";
    AbsolutePath TeamcityServerPath => RootDirectory / TeamcityServerModuleName;

    Target CompileAndPushTeamcityServer => _ => _
        .DependsOn(DownloadTeamcityBinaries)
        .Executes(async () =>
        {
            var dockerfiles = TeamcityServerPath.GlobFiles(MatchPatterns)
                .Where(f => !((string)f).Contains("cache"))
                .ToList();
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityServerPath, TeamcityServerModuleName);
            
            var tasks = new List<Task>();
            
            foreach (var (tags, dockerfile) in tagsToBuild)
            {
                Task t1 = Task.Factory.StartNew(() =>
                {
                    RetryPolicy.Execute(() =>
                    {
                        DockerBuildxBuild(_ => _
                            .SetPlatform("linux/arm64")
                            .SetTag(tags.Select(t => t.WithImage("teamcity-server-arm64v8")))
                            .AddBuildArg("BASE_ARCH=arm64v8")
                            .EnableRm()
                            .SetPath(TeamcityServerPath)
                            .SetFile(dockerfile)
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
                            .SetTag(tags.Select(t => t.WithImage("teamcity-server-arm32v7")))
                            .AddBuildArg("BASE_ARCH=arm32v7")
                            .EnableRm()
                            .SetPath(TeamcityServerPath)
                            .SetFile(dockerfile)
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
                            string tagWithImage = tag.WithImage("teamcity-server");
                            RetryPolicy.Execute(() =>
                            {
                                Docker($"buildx imagetools create --builder rpi -t {tagWithImage} {tag.WithImage("teamcity-server-arm64v8")} {tag.WithImage("teamcity-server-arm32v7")}");
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