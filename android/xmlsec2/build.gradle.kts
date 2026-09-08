/* XML signature verification. The Validation Gateway response is signed XML, and this is
   what checks it - so it is not optional even though nothing in our code calls it
   directly. */
val sdk = providers.gradleProperty("SDK").get()

configurations.maybeCreate("default")
artifacts.add("default", file("$sdk/samples/ToolkitSample/xmlsec2-release/xmlsec2-release.aar"))
