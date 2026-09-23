pluginManagement {
    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
    }
}

dependencyResolutionManagement {
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        mavenCentral()
    }
}

rootProject.name = "DI VMS Reception"

include(":app")

/* ICP's libraries, as AAR-shim modules.

   They point at the SDK already in this repository rather than holding copies: the
   Windows build does the same with a HintPath, and a second copy of a 6 MB binary is a
   second thing to keep in step with whatever ICP ships next. */
include(":EIDAToolkit")     // the toolkit itself
include(":acs-plugin")      // the ACS reader driver - this is the USB reader on the desk
include(":xmlsec2")         // XML signature verification, used on the gateway response
