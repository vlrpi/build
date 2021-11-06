using System.Linq;
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
        .Executes(() =>
        {
            var dockerfiles = TeamcityServerPath.GlobFiles(MatchPatterns)
                .Where(f => !((string)f).Contains("cache"))
                .ToList();
            var tagsToBuild = GetTagsToBuild(dockerfiles, TeamcityServerPath, TeamcityServerModuleName);
            foreach (var (tags, dockerfile) in tagsToBuild)
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

                foreach (string tag in tags)
                {
                    string tagWithImage = tag.WithImage("teamcity-server");
                    RetryPolicy.Execute(() =>
                    {
                        Docker($"manifest create {tagWithImage} --amend {tag.WithImage("teamcity-server-arm64v8")} --amend {tag.WithImage("teamcity-server-arm32v7")}");
                        DockerManifestPush(_ => _
                            .SetManifestList(tagWithImage));
                    });
                }
            }
        });
}