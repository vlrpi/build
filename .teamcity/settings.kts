// ------------------------------------------------------------------------------
// <auto-generated>
//
//     This code was generated.
//
//     - To turn off auto-generation set:
//
//         [TeamCity (AutoGenerate = false)]
//
//     - To trigger manual generation invoke:
//
//         nuke --generate-configuration TeamCity --host TeamCity
//
// </auto-generated>
// ------------------------------------------------------------------------------

import jetbrains.buildServer.configs.kotlin.v2018_1.*
import jetbrains.buildServer.configs.kotlin.v2018_1.buildFeatures.*
import jetbrains.buildServer.configs.kotlin.v2018_1.buildSteps.*
import jetbrains.buildServer.configs.kotlin.v2018_1.triggers.*
import jetbrains.buildServer.configs.kotlin.v2018_1.vcs.*

version = "2021.1"

project {
    buildType(CompileAndPushJdk11)
    buildType(CompileAndPushTeamcityServer)
    buildType(CompileAndPushTeamcityAgent)
    buildType(CompileAndPushTeamcityAgentDotnet)
    buildType(DockerLogIn)
    buildType(DockerLogOut)
    buildType(CreateBuilder)
    buildType(PruneBuilderCache)
    buildType(BuildTeamcityCache)

    buildTypesOrder = arrayListOf(CompileAndPushJdk11, CompileAndPushTeamcityServer, CompileAndPushTeamcityAgent, CompileAndPushTeamcityAgentDotnet, DockerLogIn, DockerLogOut, CreateBuilder, PruneBuilderCache, BuildTeamcityCache)

    params {
        select (
            "env.Configuration",
            label = "Configuration",
            description = "Configuration to build - Default is 'Debug' (local) or 'Release' (server)",
            value = "Release",
            options = listOf("Debug" to "Debug", "Release" to "Release"),
            display = ParameterDisplay.NORMAL)
        text (
            "env.MatchPattern",
            label = "MatchPattern",
            description = "A pattern to process only specific operating systems",
            value = "**/Dockerfile",
            allowEmpty = true,
            display = ParameterDisplay.NORMAL)
        select (
            "env.Verbosity",
            label = "Verbosity",
            description = "Logging verbosity during build execution. Default is 'Normal'.",
            value = "Normal",
            options = listOf("Minimal" to "Minimal", "Normal" to "Normal", "Quiet" to "Quiet", "Verbose" to "Verbose"),
            display = ParameterDisplay.NORMAL)
        text(
            "teamcity.runner.commandline.stdstreams.encoding",
            "UTF-8",
            display = ParameterDisplay.HIDDEN)
        text(
            "teamcity.git.fetchAllHeads",
            "true",
            display = ParameterDisplay.HIDDEN)
    }
}
object CompileAndPushJdk11 : BuildType({
    name = "CompileAndPushJdk11"
    type = Type.DEPLOYMENT
    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }
    steps {
        exec {
            path = "build.cmd"
            arguments = "CompileAndPushJdk11 --skip"
            conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
        }
        exec {
            path = "build.sh"
            arguments = "CompileAndPushJdk11 --skip"
            conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
        }
    }
    params {
        text(
            "teamcity.ui.runButton.caption",
            "Compile And Push Jdk11",
            display = ParameterDisplay.HIDDEN)
    }
})
object CompileAndPushTeamcityServer : BuildType({
    name = "CompileAndPushTeamcityServer"
    type = Type.DEPLOYMENT
    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }
    steps {
        exec {
            path = "build.cmd"
            arguments = "CompileAndPushTeamcityServer --skip"
            conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
        }
        exec {
            path = "build.sh"
            arguments = "CompileAndPushTeamcityServer --skip"
            conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
        }
    }
    params {
        text(
            "teamcity.ui.runButton.caption",
            "Compile And Push Teamcity Server",
            display = ParameterDisplay.HIDDEN)
    }
})
object CompileAndPushTeamcityAgent : BuildType({
    name = "CompileAndPushTeamcityAgent"
    type = Type.DEPLOYMENT
    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }
    steps {
        exec {
            path = "build.cmd"
            arguments = "CompileAndPushTeamcityAgent --skip"
            conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
        }
        exec {
            path = "build.sh"
            arguments = "CompileAndPushTeamcityAgent --skip"
            conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
        }
    }
    params {
        text(
            "teamcity.ui.runButton.caption",
            "Compile And Push Teamcity Agent",
            display = ParameterDisplay.HIDDEN)
    }
})
object CompileAndPushTeamcityAgentDotnet : BuildType({
    name = "CompileAndPushTeamcityAgentDotnet"
    type = Type.DEPLOYMENT
    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }
    steps {
        exec {
            path = "build.cmd"
            arguments = "CompileAndPushTeamcityAgentDotnet --skip"
            conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
        }
        exec {
            path = "build.sh"
            arguments = "CompileAndPushTeamcityAgentDotnet --skip"
            conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
        }
    }
    params {
        text(
            "teamcity.ui.runButton.caption",
            "Compile And Push Teamcity Agent Dotnet",
            display = ParameterDisplay.HIDDEN)
    }
})
object DockerLogIn : BuildType({
    name = "DockerLogIn"
    type = Type.DEPLOYMENT
    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }
    steps {
        exec {
            path = "build.cmd"
            arguments = "DockerLogIn --skip"
            conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
        }
        exec {
            path = "build.sh"
            arguments = "DockerLogIn --skip"
            conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
        }
    }
    params {
        text(
            "teamcity.ui.runButton.caption",
            "Docker Log In",
            display = ParameterDisplay.HIDDEN)
    }
})
object DockerLogOut : BuildType({
    name = "DockerLogOut"
    type = Type.DEPLOYMENT
    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }
    steps {
        exec {
            path = "build.cmd"
            arguments = "DockerLogOut --skip"
            conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
        }
        exec {
            path = "build.sh"
            arguments = "DockerLogOut --skip"
            conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
        }
    }
    params {
        text(
            "teamcity.ui.runButton.caption",
            "Docker Log Out",
            display = ParameterDisplay.HIDDEN)
    }
})
object CreateBuilder : BuildType({
    name = "CreateBuilder"
    type = Type.DEPLOYMENT
    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }
    steps {
        exec {
            path = "build.cmd"
            arguments = "CreateBuilder --skip"
            conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
        }
        exec {
            path = "build.sh"
            arguments = "CreateBuilder --skip"
            conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
        }
    }
    params {
        text(
            "teamcity.ui.runButton.caption",
            "Create Builder",
            display = ParameterDisplay.HIDDEN)
    }
})
object PruneBuilderCache : BuildType({
    name = "PruneBuilderCache"
    type = Type.DEPLOYMENT
    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }
    steps {
        exec {
            path = "build.cmd"
            arguments = "PruneBuilderCache --skip"
            conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
        }
        exec {
            path = "build.sh"
            arguments = "PruneBuilderCache --skip"
            conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
        }
    }
    params {
        text(
            "teamcity.ui.runButton.caption",
            "Prune Builder Cache",
            display = ParameterDisplay.HIDDEN)
    }
})
object BuildTeamcityCache : BuildType({
    name = "BuildTeamcityCache"
    type = Type.DEPLOYMENT
    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }
    steps {
        exec {
            path = "build.cmd"
            arguments = "BuildTeamcityCache --skip"
            conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
        }
        exec {
            path = "build.sh"
            arguments = "BuildTeamcityCache --skip"
            conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
        }
    }
    params {
        text(
            "teamcity.ui.runButton.caption",
            "Build Teamcity Cache",
            display = ParameterDisplay.HIDDEN)
    }
})
