/* XML signature verification. The Validation Gateway response is signed XML, and this is
   what checks it - so it is not optional even though nothing in our code calls it
   directly.

   rootProject.file and the require(): see EIDAToolkit/build.gradle.kts. */
fun sdkArtifact(relative: String) =
    rootProject.file("${providers.gradleProperty("SDK").get()}/$relative").also {
        require(it.isFile) { "ICP SDK file not found: $it\nCheck SDK in android/gradle.properties." }
    }

configurations.maybeCreate("default")
artifacts.add("default", sdkArtifact("samples/ToolkitSample/xmlsec2-release/xmlsec2-release.aar"))
