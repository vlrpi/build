using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Generator;

string agentProperties = @"

name=Raspberry Pi TeamCity agent
serverUrl=http://server:8111/
workDir=../work
tempDir=../temp

".Trim();
        
IGenerator[] generators =
{
    new Jdk11Generator(),
    new TeamcityServerGenerator(),
    new TeamcityAgentGenerator(),
    new TeamcityAgentDotnetGenerator()
};

(string osName, string osVersion)[] supportedOsInfo =
{
    ("debian", "bullseye"),
    ("debian", "buster"),
    ("debian", "bookworm"),
    ("ubuntu", "bionic"),
    ("ubuntu", "focal"),
    ("ubuntu", "jammy")
};

foreach (IGenerator generator in generators)
{
    foreach ((string osName, string osVersion) in supportedOsInfo)
    {
        IEnumerable<DockerfileInfo> dockerfiles = generator.GetDockerfiles(osName, osVersion);
        foreach (DockerfileInfo dockerfile in dockerfiles)
        {
            DirectoryInfo dir = Directory.CreateDirectory(dockerfile.RelativePath);
            File.WriteAllText(Path.Combine(dockerfile.RelativePath, "Dockerfile"), dockerfile.Content,
                Encoding.UTF8);

            if (dockerfile.RelativePath.Contains("dotnet"))
            {
                string excludePathLocal = Path.Combine(dir.Parent!.FullName, ".exclude");
                if (!File.Exists(excludePathLocal))
                {
                    File.Create(excludePathLocal).Dispose();
                }

                string excludePathGlobal = Path.Combine(dir.Parent!.Parent!.FullName, ".exclude");
                if (!File.Exists(excludePathGlobal))
                {
                    File.Create(excludePathGlobal).Dispose();
                }
            }
                
            if (dockerfile.Tags != null)
            {
                File.WriteAllText(Path.Combine(dockerfile.RelativePath, ".tags"),
                    string.Join(Environment.NewLine, dockerfile.Tags), Encoding.UTF8);
            }

            if (dockerfile.RelativePath.Contains("teamcity-agent" + Path.DirectorySeparatorChar))
            {
                File.WriteAllText(Path.Combine(dockerfile.RelativePath, "buildAgent.properties"),
                    agentProperties, Encoding.UTF8);
            }
        }
    }
}