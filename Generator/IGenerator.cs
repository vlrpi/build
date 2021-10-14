using System.Collections.Generic;

namespace Generator
{
    public interface IGenerator
    {
         IEnumerable<DockerfileInfo> GetDockerfiles(string osName, string osVersion);
	}

	public sealed record DockerfileInfo(
		string RelativePath,
		string Content,
		IEnumerable<string> Tags);
}