using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Generator;

public sealed class TeamcityAgentGenerator : IGenerator
{
    private static string GetDockerfileTemplate()
    {
        return @"

ARG BASE_ARCH

FROM vlrpi/teamcity-server-${BASE_ARCH}:{OS_NAME}-{OS_VERSION}-{TEAMCITY_VERSION}
LABEL maintainer=""Vova Lantsov""
LABEL contact_email=""contact@vova-lantsov.dev""
LABEL telegram=""https://t.me/vova_lantsov""

ARG DOCKER_ARCH

ADD buildAgent.properties TeamCity/buildAgent/conf/

RUN [ ""cross-build-start"" ]

RUN apt-get install -y ca-certificates curl gnupg git zlib1g apt-transport-https software-properties-common {CUSTOM_PACKAGES}

RUN install -m 0755 -d /etc/apt/keyrings
RUN curl -fsSL https://download.docker.com/linux/{OS_NAME}/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
RUN chmod a+r /etc/apt/keyrings/docker.gpg

RUN echo ""deb [arch=${DOCKER_ARCH} signed-by=/etc/apt/keyrings/docker.gpg] \
    https://download.docker.com/linux/{OS_NAME} {OS_VERSION} stable"" | \
    tee /etc/apt/sources.list.d/docker.list > /dev/null

RUN apt-get update
RUN apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

RUN [ ""cross-build-end"" ]

EXPOSE 8111 9090
ENTRYPOINT [""TeamCity/buildAgent/bin/agent.sh"",""run""]

".Trim();
    }

    private static string GetDockerComposeVersion()
    {
        string path = Path.Combine("teamcity-agent", "docker-compose.txt");
        string dockerComposeVersion = File.ReadAllText(path).TrimEnd();
        return dockerComposeVersion;
    }

    private static string[] GetTeamcityVersions()
    {
        string teamcityVersionsText = File.ReadAllText("teamcity.txt").TrimEnd();
        return teamcityVersionsText.Split(',');
    }

    public IEnumerable<DockerfileInfo> GetDockerfiles(string osName, string osVersion)
    {
        string dockerComposeVersion = GetDockerComposeVersion();
        string[] teamcityVersions = GetTeamcityVersions();
        foreach (string teamcityVersion in teamcityVersions)
        {
            string dockerfileContent = GetDockerfileTemplate()
                .Replace("{DOCKER_COMPOSE_VERSION}", dockerComposeVersion)
                .Replace("{OS_NAME}", osName)
                .Replace("{OS_VERSION}", osVersion)
                .Replace("{TEAMCITY_VERSION}", teamcityVersion)
                .Replace("{CUSTOM_PACKAGES}", osVersion is "bookworm" or "jammy" ? "liblttng-ust1" : "liblttng-ust0");
            string path = Path.Combine("..", "rpi-teamcity-agent", osName, osVersion, teamcityVersion);
                
            bool isLatest = teamcityVersion == teamcityVersions[^1];
            string[] separatedTeamcityVersion = teamcityVersion.Split('.');
            bool isLatestForYear = teamcityVersions.Last(v => v.StartsWith(separatedTeamcityVersion[0])) ==
                                   teamcityVersion;
                
            List<string> tags = null;
            if (isLatest || isLatestForYear)
            {
                tags = new List<string>(3);
                if (isLatest)
                {
                    tags.Add("latest");
                }
                if (isLatestForYear)
                {
                    tags.Add(separatedTeamcityVersion[0]);
                }
                tags.Add(teamcityVersion);
            }
                
            yield return new DockerfileInfo(path, dockerfileContent, tags);
        }
    }
}