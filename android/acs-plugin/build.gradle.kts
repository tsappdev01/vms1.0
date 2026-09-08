/* ICP's driver for the ACS reader family - the ACR39U on the reception desk is one.

   The toolkit finds the plugin's native library at runtime through
   plugin_directory_path, which is the APK's own nativeLibraryDir. So this has to be a
   real dependency of the app rather than a file dropped on the device. */
val sdk = providers.gradleProperty("SDK").get()

configurations.maybeCreate("default")
artifacts.add("default", file("$sdk/plugins/ACS/acs-plugin-release.aar"))
