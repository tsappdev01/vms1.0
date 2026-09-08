/* ICP's driver for the ACS reader family - the ACR39U on the reception desk is one.

   The AAR carries jni/{arm64-v8a,armeabi-v7a}/libACS.so, which is unpacked into the APK's
   nativeLibraryDir at install time. That is exactly the path card/ToolkitConfig.kt gives
   the toolkit as plugin_directory_path, which is why this has to be a real dependency of
   the app rather than a file dropped on the device.

   rootProject.file and the require(): see EIDAToolkit/build.gradle.kts. */
fun sdkArtifact(relative: String) =
    rootProject.file("${providers.gradleProperty("SDK").get()}/$relative").also {
        require(it.isFile) { "ICP SDK file not found: $it\nCheck SDK in android/gradle.properties." }
    }

configurations.maybeCreate("default")
artifacts.add("default", sdkArtifact("plugins/ACS/acs-plugin-release.aar"))
