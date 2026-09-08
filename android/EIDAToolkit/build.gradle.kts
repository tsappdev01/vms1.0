/* AAR shim for ICP's EIDAToolkit.aar.

   Two things it exists for.

   The AAR's classes reference com.google.gson.* in the digital-signature data models but
   it does not bundle Gson. Declaring Gson on this module's "default" configuration
   propagates it transitively, which a raw `implementation files(...)` cannot do - without
   it the build fails at DexingNoClasspathTransform, naming the AAR rather than the
   missing dependency. ICP's own README is explicit about this; it is new in 3.1.x.

   And the path. rootProject.file, never file: a relative path given to file() in a
   subproject resolves against THAT subproject's directory, so "../id-card-toolkit-..."
   would be looked for inside android/EIDAToolkit/ and missed. Resolving from the root
   project makes the SDK property mean one thing wherever it is read.

   The require() is there so a wrong SDK property names the file it could not find.
   Without it the failure arrives later as a message about an unresolved configuration,
   which says nothing about the path being wrong. */
fun sdkArtifact(relative: String) =
    rootProject.file("${providers.gradleProperty("SDK").get()}/$relative").also {
        require(it.isFile) { "ICP SDK file not found: $it\nCheck SDK in android/gradle.properties." }
    }

configurations.maybeCreate("default")
artifacts.add("default", sdkArtifact("lib/EIDAToolkit.aar"))

dependencies {
    "default"("com.google.code.gson:gson:2.10.1")
}
