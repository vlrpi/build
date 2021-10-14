using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Generator
{
    public sealed class TeamcityAgentDotnetGenerator : IGenerator
    {
        private static string GetDockerfileTemplate()
        {
            return @"

ARG BASE_ARCH

FROM vlrpi/teamcity-agent-${BASE_ARCH}:{OS_NAME}-{OS_VERSION}-{TEAMCITY_VERSION}
LABEL maintainer=""Vova Lantsov""
LABEL contact_email=""contact@vova-lantsov.dev""
LABEL telegram=""https://t.me/vova_lantsov""

# Enable detection of running in a container
ENV DOTNET_RUNNING_IN_CONTAINER=true \
# Enable correct mode for dotnet watch (only mode supported in a container)
    DOTNET_USE_POLLING_FILE_WATCHER=true \
# Skip extraction of XML docs - generally not useful within an image/container - helps performance
    NUGET_XMLDOC_MODE=skip

RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates libc6 libgcc1 libgssapi-krb5-2 libstdc++6 zlib1g \
    {CUSTOM_PACKAGES} \
    && rm -rf /var/lib/apt/lists/*

{INSTALLATION_COMMANDS} \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    # Trigger first run experience by running arbitrary cmd
    && dotnet help

".Trim();
        }
        
        private static string GetInstallationCommandTemplate()
        {
            return @"

RUN curl -SL --output dotnet.tar.gz https://dotnetcli.blob.core.windows.net/dotnet/Sdk/{SDK_VERSION}/dotnet-sdk-{SDK_VERSION}-linux-{DOTNET_ARCH}.tar.gz \
    && dotnet_sha512='{SDK_CHECKSUM}' \
    && echo ""$dotnet_sha512 dotnet.tar.gz"" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -C /usr/share/dotnet -zxf dotnet.tar.gz \
    && rm dotnet.tar.gz

".Trim();
        }
        
        private static string[] GetTeamcityVersions()
        {
            string teamcityVersionsText = File.ReadAllText("teamcity.txt").TrimEnd();
            return teamcityVersionsText.Split(',');
        }

        private static IReadOnlyCollection<DotnetVersionInfo> GetDotnetVersions(string osName, string osVersion)
        {
            string[] ltsVersions = File.ReadAllText("lts.txt").TrimEnd().Split(',');
            string[] supportedVersions = File.ReadAllText("supported.txt").TrimEnd().Split(',');
            
            string path = Path.Combine("teamcity-agent-dotnet", "versions");
            string[] versionFiles = Directory.GetFiles(path, "version-*.txt", SearchOption.TopDirectoryOnly);
            var dotnetVersions = new DotnetVersionInfo[versionFiles.Length];

            string CustomPackagesMapper() =>
                (osName, osVersion) switch
                {
                    ("debian", "buster") => "libicu63 libssl1.1",
                    ("debian", "bullseye") => "libicu67 libssl1.1",
                    ("debian", "stretch") => "libicu57 libssl1.1",
                    ("ubuntu", "bionic") => "libicu60 libssl1.1",
                    ("ubuntu", "focal") => "libicu66 libssl1.1",
                    ("ubuntu", "xenial") => "libicu55 libssl1.0.0",
                    _ => throw new NotSupportedException($"{osName} {osVersion} is not supported")
                };

            for (int i = 0; i < versionFiles.Length; i++)
            {
                string versionFile = versionFiles[i];
                string versionFileName = Path.GetFileNameWithoutExtension(versionFile);
                string versionKey = versionFileName.Split('-')[1];
                string versionFull = File.ReadAllText(versionFile).TrimEnd();
                
                string armChecksum = File.ReadAllText(Path.Combine(path, "arm", versionFileName + "-arm.txt"))
                    .TrimEnd();
                string arm64Checksum = File.ReadAllText(Path.Combine(path, "arm64", versionFileName + "-arm64.txt"))
                    .TrimEnd();

                dotnetVersions[i] = new DotnetVersionInfo(
                    Key: versionKey,
                    Version: versionFull,
                    IsLts: ltsVersions.Contains(versionKey),
                    IsSupported: supportedVersions.Contains(versionKey),
                    ArmChecksum: armChecksum,
                    Arm64Checksum: arm64Checksum,
                    CustomPackages: CustomPackagesMapper());
            }

            return dotnetVersions;
        }

        private static string GetInstallationCommands(string arch, IReadOnlyCollection<DotnetVersionInfo> dotnetVersions)
        {
            var commands = new List<string>(dotnetVersions.Count);
            
            foreach (DotnetVersionInfo dotnetVersion in dotnetVersions)
            {
                switch (arch)
                {
                    case "arm":
                        commands.Add(GetInstallationCommandTemplate()
                            .Replace("{SDK_VERSION}", dotnetVersion.Version)
                            .Replace("{SDK_ARCH}", "arm")
                            .Replace("{SDK_CHECKSUM}", dotnetVersion.ArmChecksum));
                        break;
                    case "arm64":
                        commands.Add(GetInstallationCommandTemplate()
                            .Replace("{SDK_VERSION}", dotnetVersion.Version)
                            .Replace("{SDK_ARCH}", "arm64")
                            .Replace("{SDK_CHECKSUM}", dotnetVersion.Arm64Checksum));
                        break;
                    default:
                        throw new NotSupportedException($"Architecture {arch} is not supported for dotnet");
                }
            }

            return string.Join(Environment.NewLine, commands);
        }

        public IEnumerable<DockerfileInfo> GetDockerfiles(string osName, string osVersion)
        {
            string[] teamcityVersions = GetTeamcityVersions();
            IReadOnlyCollection<DotnetVersionInfo> dotnetVersions = GetDotnetVersions(osName, osVersion);
            
            foreach (string teamcityVersion in teamcityVersions)
            {
                foreach (string dotnetArch in new[] {"arm", "arm64"})
                {
                    (string key, IReadOnlyCollection<DotnetVersionInfo> dotnetVersionsArray)[] dotnetVersionsPack =
                        dotnetVersions
                            .Select(v => (v.Key, (IReadOnlyCollection<DotnetVersionInfo>)new[] { v }))
                            .Append(("lts", dotnetVersions.Where(v => v.IsLts).ToArray()))
                            .Append(("supported", dotnetVersions.Where(v => v.IsSupported).ToArray()))
                            .ToArray();
                    
                    foreach (var (key, dotnetVersionsArray) in dotnetVersionsPack)
                    {
                        string installationCommands = GetInstallationCommands(dotnetArch, dotnetVersionsArray);
                        string dockerfileContent = GetDockerfileTemplate()
                            .Replace("{OS_NAME}", osName)
                            .Replace("{OS_VERSION}", osVersion)
                            .Replace("{TEAMCITY_VERSION}", teamcityVersion)
                            .Replace("{CUSTOM_PACKAGES}", dotnetVersionsArray.First().CustomPackages)
                            .Replace("{INSTALLATION_COMMANDS}", installationCommands);
                        string path = Path.Combine("..", "rpi-teamcity-agent-dotnet", osName, osVersion,
                            teamcityVersion, $"dotnet-{key}", dotnetArch);
                        
                        bool isLatest = teamcityVersion == teamcityVersions[^1];
                        string[] separatedTeamcityVersion = teamcityVersion.Split('.');
                        bool isLatestForYear = teamcityVersions.Last(v => v.StartsWith(separatedTeamcityVersion[0])) ==
                                               teamcityVersion;
                
                        List<string> tags = new List<string>(3);
                        if (isLatest || isLatestForYear)
                        {
                            if (isLatest)
                            {
                                tags.Add($"tc-latest-dotnet-{key}");
                            }
                            if (isLatestForYear)
                            {
                                tags.Add($"tc-{separatedTeamcityVersion[0]}-dotnet-{key}");
                            }
                        }
                        tags.Add($"tc-{teamcityVersion}-dotnet-{key}");
                        
                        yield return new DockerfileInfo(path, dockerfileContent, tags);
                    }
                }
            }
        }

        private sealed record DotnetVersionInfo(
            string Key,
            string Version,
            bool IsLts,
            bool IsSupported,
            string ArmChecksum,
            string Arm64Checksum,
            string CustomPackages);
    }
}