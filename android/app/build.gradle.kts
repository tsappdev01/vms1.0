plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.kotlin.android)
    alias(libs.plugins.kotlin.serialization)
    alias(libs.plugins.compose.compiler)
}

val sdk = providers.gradleProperty("SDK").get()

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

        /* MSAL needs the redirect URI in the manifest as well as in the Entra app
           registration, and the two must match exactly. Kept here so there is one place
           to change it. */
        manifestPlaceholders["msalRedirectScheme"] = "msauth"
        manifestPlaceholders["msalRedirectHost"] = applicationId!!

        /* The signing key's certificate hash, URL-encoded, which is the third part of the
           redirect URI. It depends on the keystore rather than on the source, so it comes
           from a Gradle property - see android/README.md for the one command that prints
           it. Left unset it builds, installs and then fails the return leg of sign-in
           with a redirect mismatch, so the default is a value that is obviously wrong
           rather than an empty string that looks plausible. */
        manifestPlaceholders["msalSignatureHash"] =
            providers.gradleProperty("MSAL_SIGNATURE_HASH").getOrElse("MSAL_SIGNATURE_HASH_NOT_SET")

        /* Where the server is. In gradle.properties so a test build can be pointed at a
           different host without editing code, and with the trailing slash Retrofit
           requires - without it Retrofit drops the last path segment of the base URL. */
        val apiBaseUrl = providers.gradleProperty("VMS_API_BASE_URL").getOrElse("https://vms.dipark.com/")
        buildConfigField("String", "API_BASE_URL", "\"$apiBaseUrl\"")
    }

    sourceSets["main"].jniLibs.srcDirs(
        // libc++_shared.so, which the toolkit's native code links against.
        "$sdk/samples/ToolkitSample/app/src/main/jniLibs",
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

    implementation(libs.msal)
}
