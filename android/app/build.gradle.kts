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
        /*  28, not 26, because that is ICP's floor for the toolkit (build instructions
         *  v1.6, section 2.1). Below it the toolkit is untested rather than merely old, and
         *  a tablet that installs and then fails at the reader is worse than one that
         *  refuses the install. It costs Android 8.0 and 8.1. */
        minSdk = 28
        targetSdk = 35

        /*  The toolkit and its plugin set carry it past the 64K method limit on their own.
         *  Required by ICP; without it the failure is a dex merge error naming nothing
         *  useful. */
        multiDexEnabled = true
        /*  Bumped with every build worth telling apart on a desk.
         *
         *  Both are read out on the settings screen, and that is the point: a tablet running
         *  an older APK than the one somebody thinks they installed looks exactly like a
         *  broken feature. 1.1.0 is the first with the camera scanner and the settings PIN;
         *  1.1.1 is the first that requires a mobile number, which the server now refuses a
         *  visit without. */
        versionCode = 7
        versionName = "1.2.2"

        /* The toolkit's native libraries are built for these two only. Without the
           filter, an x86_64 emulator installs an APK with no usable library and fails at
           the first read with an UnsatisfiedLinkError - which reads like a code fault. */
        ndk { abiFilters += listOf("arm64-v8a", "armeabi-v7a") }

        /* The server the app is built pointing at. It is only the default now: the
           address is a setting on the tablet, so a desk can be moved to another server
           without a rebuild - see settings/Settings.kt. The trailing slash matters either
           way, because without it Retrofit drops the last path segment of the base URL. */
        val apiBaseUrl = providers.gradleProperty("VMS_API_BASE_URL")
            .getOrElse("https://vmsdi.dubaiinvestments.com/")
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

    /*  Packaging, and this block is not housekeeping.
     *
     *  Every setting under jniLibs is required by ICP's build instructions (v1.6, section
     *  2.3.1) and every one of them fails at runtime rather than at build time - so a build
     *  without them is green, installs, and then cannot read a card. They were missing here
     *  until the document was read against the project.
     */
    packaging {
        /* The toolkit, its plugins and Spongy Castle each ship their own copies of these,
           and two files with one path fails the merge. Notices are dropped; anything that a
           library actually reads back at runtime is kept, first one wins. */
        /* AndroidManifest.xml is excluded as a packaged *resource*, which is what ICP's own
           sample does: several plugin AARs carry one, and two files at one path fails the
           merge. This is nothing to do with the manifest merger, which has already run. */
        resources.excludes += setOf(
            "AndroidManifest.xml",
            "META-INF/DEPENDENCIES", "META-INF/LICENSE*", "META-INF/NOTICE*",
        )
        resources.pickFirsts += setOf("META-INF/*.kotlin_module", "META-INF/versions/9/OSGI-INF/MANIFEST.MF")

        jniLibs {
            /*  The one that decides whether a card can be read at all.
             *
             *  The toolkit loads its reader plugins with dlopen against a filesystem path.
             *  From AGP 4.1 native libraries stay compressed inside the APK, where there is
             *  no path to open, and the load comes back "library not found". This is also
             *  what makes AGP write android:extractNativeLibs="true" into the merged
             *  manifest, which is the other half of the same requirement - set here rather
             *  than in the manifest, because with both set AGP takes this one and warns
             *  about the other. */
            useLegacyPackaging = true

            // ICP's doNotStrip, for every native library. Gradle re-strips them as it
            // packages, and a re-stripped plugin no longer matches the checksum the
            // toolkit verifies it against.
            keepDebugSymbols += "**/*.so"

            /*  Two plugin AARs ship libc++_shared.so built against different NDK releases,
             *  and duplicate paths fail the merge. First one wins, per ICP. */
            pickFirsts += setOf(
                "lib/arm64-v8a/libc++_shared.so",
                "lib/armeabi-v7a/libc++_shared.so",
            )
        }
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

    /* The camera, and the recogniser that reads the card in front of it. Both on-device:
       nothing about a visitor's Emirates ID leaves the tablet except the text of the zone,
       which goes to this system's own server to be checked. */
    implementation(libs.androidx.camera.core)
    implementation(libs.androidx.camera.camera2)
    implementation(libs.androidx.camera.lifecycle)
    implementation(libs.androidx.camera.view)
    implementation(libs.mlkit.text.recognition)

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
