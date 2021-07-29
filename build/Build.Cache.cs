using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
	AbsolutePath TeamcityCachePath => TeamcityServerPath / "cache" / "teamcity";
	
	Target BuildTeamcityCache => _ => _
		.Executes(() =>
		{
			RetryPolicy.Execute(() =>
			{
				DockerBuild(_ => _
					.SetTag("vlrpi/cache:teamcity")
					.EnableRm()
					.SetPath(TeamcityCachePath)
					.EnablePull());
			});
		});

	Target DownloadTeamcityBinaries => _ => _
		.DependsOn(BuildTeamcityCache)
		.Executes(() =>
		{
			DockerCreate(_ => _
				.SetImage("vlrpi/cache:teamcity")
				.SetName("vlrpi-cache-teamcity"));
			try
			{
				Docker("cp vlrpi-cache-teamcity:/teamcity/ cache/", TeamcityServerPath);
			}
			finally
			{
				DockerRm(_ => _
					.SetContainers("vlrpi-cache-teamcity"));
			}
		});
}
