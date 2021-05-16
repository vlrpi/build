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

version = "2020.2"

project {
    buildType(PushJdk11)
    buildType(PushTeamcityAgent)
    buildType(PushTeamcityServer)
    buildType(PushTeamcityAgentDotnet)
    buildType(DockerLogIn)
    buildType(DockerLogOut)

    buildTypesOrder = arrayListOf(PushJdk11, PushTeamcityAgent, PushTeamcityServer, PushTeamcityAgentDotnet, DockerLogIn, DockerLogOut)

    params {
        select (
            "env.Configuration",
            label = "Configuration",
            description = "Configuration to build - Default is 'Debug' (local) or 'Release' (server)",
            value = "Release",
            options = listOf("Debug" to "Debug", "Release" to "Release"),
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
            display = ParameterDisplay.HIDDEN
        )
    }
}
object PushJdk11 : BuildType({
    name = "PushJdk11"
    type = Type.DEPLOYMENT
    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }
    steps {
        exec {
            path = "build.cmd"
            arguments = "CompileJdk11 PushJdk11 --skip"
            conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
        }
        exec {
            path = "build.sh"
            arguments = "CompileJdk11 PushJdk11 --skip"
            conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
        }
    }
    params {
        text(
            "teamcity.ui.runButton.caption",
            "Push Jdk11",
            display = ParameterDisplay.HIDDEN
        )
    }
})
object PushTeamcityAgent : BuildType({
    name = "PushTeamcityAgent"
    type = Type.DEPLOYMENT
    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }
    steps {
        exec {
            path = "build.cmd"
            arguments = "CompileTeamcityAgent PushTeamcityAgent --skip"
            conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
        }
        exec {
            path = "build.sh"
            arguments = "CompileTeamcityAgent PushTeamcityAgent --skip"
            conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
        }
    }
    params {
        text(
            "teamcity.ui.runButton.caption",
            "Push Teamcity Agent",
            display = ParameterDisplay.HIDDEN
        )
    }
})
object PushTeamcityServer : BuildType({
    name = "PushTeamcityServer"
    type = Type.DEPLOYMENT
    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }
    steps {
        exec {
            path = "build.cmd"
            arguments = "CompileTeamcityServer PushTeamcityServer --skip"
            conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
        }
        exec {
            path = "build.sh"
            arguments = "CompileTeamcityServer PushTeamcityServer --skip"
            conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
        }
    }
    params {
        text(
            "teamcity.ui.runButton.caption",
            "Push Teamcity Server",
            display = ParameterDisplay.HIDDEN
        )
    }
})
object PushTeamcityAgentDotnet : BuildType({
    name = "PushTeamcityAgentDotnet"
    type = Type.DEPLOYMENT
    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }
    steps {
        exec {
            path = "build.cmd"
            arguments = "CompileTeamcityAgentDotnet PushTeamcityAgentDotnet --skip"
            conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
        }
        exec {
            path = "build.sh"
            arguments = "CompileTeamcityAgentDotnet PushTeamcityAgentDotnet --skip"
            conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
        }
    }
    params {
        text(
            "teamcity.ui.runButton.caption",
            "Push Teamcity Agent Dotnet",
            display = ParameterDisplay.HIDDEN
        )
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
            display = ParameterDisplay.HIDDEN
        )
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
            display = ParameterDisplay.HIDDEN
        )
    }
})
