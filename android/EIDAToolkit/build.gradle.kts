/* AAR shim for ICP's EIDAToolkit.aar.

   The AAR's classes reference com.google.gson.* in the digital-signature data models but
   it does not bundle Gson. Declaring Gson on this module's "default" configuration
   propagates it transitively, which a raw `implementation files(...)` cannot do - without
   it the build fails at DexingNoClasspathTransform, naming the AAR rather than the
   missing dependency. ICP's own README is explicit about this; it is new in 3.1.x. */
val sdk = providers.gradleProperty("SDK").get()

configurations.maybeCreate("default")
artifacts.add("default", file("$sdk/lib/EIDAToolkit.aar"))

dependencies {
    "default"("com.google.code.gson:gson:2.10.1")
}
