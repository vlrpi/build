using System;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;

partial class Build
{
	public Build()
	{
		var msBuildExtensionPath = Environment.GetEnvironmentVariable("MSBuildExtensionsPath");
		var msBuildExePath = Environment.GetEnvironmentVariable("MSBUILD_EXE_PATH");
		var msBuildSdkPath = Environment.GetEnvironmentVariable("MSBuildSDKsPath");

		MSBuildLocator.RegisterDefaults();
		TriggerAssemblyResolution();
		
		Environment.SetEnvironmentVariable("MSBuildExtensionsPath", msBuildExtensionPath);
		Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", msBuildExePath);
		Environment.SetEnvironmentVariable("MSBuildSDKsPath", msBuildSdkPath);
	}

	static void TriggerAssemblyResolution() => _ = new ProjectCollection();
}