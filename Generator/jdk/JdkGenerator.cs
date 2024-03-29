using System.Collections.Generic;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Generator;

public sealed class Jdk11Generator : IGenerator
{
    private static string GetDockerfileTemplate()
    {
        return @"

ARG BALENALIB_ARCH

FROM balenalib/${BALENALIB_ARCH}-{OS_NAME}:{OS_VERSION}-run
LABEL maintainer=""Vova Lantsov""
LABEL contact_email=""contact@vova-lantsov.dev""
LABEL telegram=""https://t.me/vova_lantsov""

ARG JAVA_ARCH
ENV JAVA_HOME /usr/lib/jvm/java-11-openjdk-${JAVA_ARCH}

RUN [ ""cross-build-start"" ]

# https://gist.github.com/jpetazzo/6127116
RUN echo ""force-unsafe-io"" > /etc/dpkg/dpkg.cfg.d/02apt-speedup && \
    echo 'Acquire::Languages ""none"";' > /etc/apt/apt.conf.d/no-lang

RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get install -y default-jdk

RUN [ ""cross-build-end"" ]

".Trim();
    }

    public IEnumerable<DockerfileInfo> GetDockerfiles(string osName, string osVersion)
    {
        string dockerfileContent = GetDockerfileTemplate()
            .Replace("{OS_NAME}", osName)
            .Replace("{OS_VERSION}", osVersion);
        string path = Path.Combine("..", "rpi-jdk11", osName, osVersion);
        yield return new DockerfileInfo(path, dockerfileContent, null);
    }
}