using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Generator;

public sealed class TeamcityServerGenerator : IGenerator
{
    private static string GetDockerfileTemplate()
    {
        return @"

ARG BASE_ARCH

FROM vlrpi/jdk-${BASE_ARCH}:{OS_NAME}-{OS_VERSION}
LABEL maintainer=""Vova Lantsov""
LABEL contact_email=""contact@vova-lantsov.dev""
LABEL telegram=""https://t.me/vova_lantsov""

COPY cache/teamcity/{TEAMCITY_VERSION} .

EXPOSE 8111 9090

ENV TEAMCITY_DATA_PATH /root/BuildServer
ENTRYPOINT [""TeamCity/bin/teamcity-server.sh"",""run""]

".Trim();
    }
        
    private static string[] GetTeamcityVersions()
    {
        string teamcityVersionsText = File.ReadAllText("teamcity.txt").TrimEnd();
        return teamcityVersionsText.Split(',');
    }

    public IEnumerable<DockerfileInfo> GetDockerfiles(string osName, string osVersion)
    {
        string[] teamcityVersions = GetTeamcityVersions();
        foreach (var teamcityVersion in teamcityVersions)
        {
            string dockerfileContent = GetDockerfileTemplate()
                .Replace("{OS_NAME}", osName)
                .Replace("{OS_VERSION}", osVersion)
                .Replace("{TEAMCITY_VERSION}", teamcityVersion);
            string path = Path.Combine("..", "rpi-teamcity-server", osName, osVersion, teamcityVersion);

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