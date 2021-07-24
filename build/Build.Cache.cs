using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    AbsolutePath CachePath => TeamcityServerPath / "cache";
	AbsolutePath TeamcityCachePath => CachePath / "teamcity";
	Target BuildTeamcityCache => _ => _
		.Executes(() =>
		{
			DockerBuildxBuild(_ => _
				.SetPlatform("linux/arm64,linux/arm/v7,linux/arm/v6")
				.SetTag("vlrpi/cache:teamcity")
				.EnableRm()
				.SetPath(TeamcityCachePath)
				.SetBuilder("rpi")
				.EnablePull()
				.EnablePush());
		});
}
