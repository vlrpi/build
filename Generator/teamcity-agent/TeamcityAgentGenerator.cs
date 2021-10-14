using System.Collections.Generic;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Generator
{
    public class TeamcityAgentGenerator : IGenerator
    {
        private static string GetDockerfileTemplate()
        {
            return @"

ARG BASE_ARCH
ARG DOCKER_ARCH

FROM vlrpi/teamcity-server-${BASE_ARCH}:{OS_NAME}-{OS_VERSION}-{TEAMCITY_VERSION}
LABEL maintainer=""Vova Lantsov""
LABEL contact_email=""contact@vova-lantsov.dev""
LABEL telegram=""https://t.me/vova_lantsov""

ADD buildAgent.properties TeamCity/buildAgent/conf/

RUN apt-get install -y git curl zlib1g liblttng-ust0 apt-transport-https gnupg-agent software-properties-common
RUN apt-get upgrade -y

RUN curl -fsSL https://download.docker.com/linux/{OS_NAME}/gpg | sudo apt-key add -

RUN add-apt-repository \
    ""deb [arch=${DOCKER_ARCH}] https://download.docker.com/linux/{OS_NAME} {OS_VERSION} stable""

RUN apt-get update
RUN apt-get install -y docker-ce docker-ce-cli containerd.io

RUN curl -L ""https://github.com/linuxserver/docker-docker-compose/releases/download/{DOCKER_COMPOSE_VERSION}/docker-compose-${DOCKER_ARCH}"" -o /usr/bin/docker-compose
RUN chmod +x /usr/bin/docker-compose

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

        public virtual IEnumerable<DockerfileInfo> GetDockerfiles(string osName, string osVersion)
        {
            string dockerComposeVersion = GetDockerComposeVersion();
            string[] teamcityVersions = GetTeamcityVersions();
            foreach (string teamcityVersion in teamcityVersions)
            {
                string dockerfileContent = GetDockerfileTemplate()
                    .Replace("{DOCKER_COMPOSE_VERSION}", dockerComposeVersion)
                    .Replace("{OS_NAME}", osName)
                    .Replace("{OS_VERSION}", osVersion)
                    .Replace("{TEAMCITY_VERSION}", teamcityVersion);
                string path = Path.Combine("..", "rpi-teamcity-agent", osName, osVersion, teamcityVersion);
                yield return new DockerfileInfo(path, dockerfileContent);
            }
        }
    }
}