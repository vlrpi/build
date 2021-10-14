using System.Collections.Generic;
using System.IO;

namespace Generator
{
    internal static class Program
    {
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
                        DirectoryInfo dir = Directory.CreateDirectory(Path.GetDirectoryName(dockerfile.RelativePath)!);
                        if (dir.Name.StartsWith("dotnet"))
                        {
                            string path = Path.Combine(dir.Parent!.FullName, ".exclude");
                            if (!File.Exists(path))
                                File.Create(path).Dispose();
                        }
                    }
                }
            }
        }
    }
}
