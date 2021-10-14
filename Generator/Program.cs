﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Generator
{
    internal static class Program
    {
        private static readonly string AgentProperties = @"

name=Raspberry Pi TeamCity agent
serverUrl=http://server:8111/
workDir=../work
tempDir=../temp

".Trim();
        
        private static void Main()
        {
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
                ("debian", "stretch"),
                ("ubuntu", "bionic"),
                ("ubuntu", "focal"),
                ("ubuntu", "xenial")
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
                            string excludePath = Path.Combine(dir.Parent!.Parent!.FullName, ".exclude");
                            if (!File.Exists(excludePath))
                            {
                                File.Create(excludePath).Dispose();
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
                                AgentProperties, Encoding.UTF8);
                        }
                    }
                }
            }
        }
    }
}