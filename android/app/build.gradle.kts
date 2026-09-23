plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.kotlin.android)
    alias(libs.plugins.kotlin.serialization)
    alias(libs.plugins.compose.compiler)
}

/* Resolved from the root project, not from app/. A relative path given to file() in a
   subproject resolves against that subproject's directory - see the shim modules. */
val sdkDir = rootProject.file(providers.gradleProperty("SDK").get())

android {
    namespace = "ae.dubaiinvestments.vms"
    compileSdk = 35

    defaultConfig {
        applicationId = "ae.dubaiinvestments.vms"
        minSdk = 26
        targetSdk = 35
        versionCode = 1
        versionName = "1.0.0"

        /* The toolkit's native libraries are built for these two only. Without the
           filter, an x86_64 emulator installs an APK with no usable library and fails at
           the first read with an UnsatisfiedLinkError - which reads like a code fault. */
        ndk { abiFilters += listOf("arm64-v8a", "armeabi-v7a") }

        /* The server the app is built pointing at. It is only the default now: the
           address is a setting on the tablet, so a desk can be moved to another server
           without a rebuild - see settings/Settings.kt. The trailing slash matters either
           way, because without it Retrofit drops the last path segment of the base URL. */
        val apiBaseUrl = providers.gradleProperty("VMS_API_BASE_URL")
            .getOrElse("https://vms-cebrd3evb0cyg0gn.uaenorth-01.azurewebsites.net/")
        buildConfigField("String", "API_BASE_URL", "\"$apiBaseUrl\"")

        /* The API key the tablet identifies itself with, for the Azure-hosted API. Not
           in gradle.properties in this repository and not to be put there: it is the only
           thing in front of the visitor database. Pass it on the command line, or keep it
           in %USERPROFILE%\.gradle\gradle.properties, which is outside the repository and
           per machine.

           Like the address it is only the default - reception can type a rotated key into
           the settings screen instead of waiting for a new build. Blank is correct for a
           build aimed at the on-premises host: that one is reachable only from the office
           network and asks for no key. */
        val apiKey = providers.gradleProperty("VMS_API_KEY").getOrElse("")
        buildConfigField("String", "API_KEY", "\"$apiKey\"")
    }

    sourceSets["main"].jniLibs.srcDirs(
        // libc++_shared.so, which the toolkit's native code links against. The toolkit's
        // and the ACS plugin's own .so files come in with their AARs.
        File(sdkDir, "samples/ToolkitSample/app/src/main/jniLibs"),
    )

    buildTypes {
        debug {
            /* A separate application ID would need its own Entra redirect URI and its own
               ICP device registration, so debug and release share one. */
            isMinifyEnabled = false
        }
        release {
            isMinifyEnabled = true
            isShrinkResources = true
            proguardFiles(getDefaultProguardFile("proguard-android-optimize.txt"), "proguard-rules.pro")
        }
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
    kotlinOptions { jvmTarget = "17" }
    buildFeatures { compose = true; buildConfig = true }

    /* The toolkit, its plugins and Spongy Castle each ship their own copies of these,
       and two files with one path fails the merge. Notices are dropped; anything that a
       library actually reads back at runtime is kept, first one wins. */
    packaging {
        resources.excludes += setOf("META-INF/DEPENDENCIES", "META-INF/LICENSE*", "META-INF/NOTICE*")
        resources.pickFirsts += setOf("META-INF/*.kotlin_module", "META-INF/versions/9/OSGI-INF/MANIFEST.MF")
    }
}

dependencies {
    // ICP. EIDAToolkit brings Gson transitively - see EIDAToolkit/build.gradle.kts.
    implementation(project(":EIDAToolkit"))
    implementation(project(":acs-plugin"))
    implementation(project(":xmlsec2"))

    /* The toolkit's signature and PKI code is built against Spongy Castle - the Android
       repackaging of Bouncy Castle - because Android ships a cut-down provider under the
       org.bouncycastle names. They are not interchangeable. */
    implementation("com.madgag.spongycastle:core:1.54.0.0")
    implementation("com.madgag.spongycastle:prov:1.54.0.0")
    implementation("com.madgag.spongycastle:pkix:1.54.0.0")

    implementation(libs.androidx.core.ktx)
    implementation(libs.androidx.lifecycle.runtime.ktx)
    implementation(libs.androidx.lifecycle.viewmodel.compose)
    implementation(libs.androidx.activity.compose)
    implementation(libs.kotlinx.coroutines.android)

    implementation(platform(libs.androidx.compose.bom))
    implementation(libs.androidx.compose.ui)
    implementation(libs.androidx.compose.ui.graphics)
    implementation(libs.androidx.compose.ui.tooling.preview)
    implementation(libs.androidx.compose.material3)
    implementation(libs.androidx.compose.material.icons)
    debugImplementation(libs.androidx.compose.ui.tooling)

    implementation(libs.kotlinx.serialization.json)
    implementation(libs.retrofit)
    implementation(libs.retrofit.kotlinx.serialization)
    implementation(libs.okhttp)
    implementation(libs.okhttp.logging)
}
