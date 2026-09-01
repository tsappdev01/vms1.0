Emirates ID Card Toolkit - Windows JWS SDK v3.1.6
====================================================

This bundle targets customers deploying the EIDAToolkit via a Java
Web Start (JWS) applet launched in a browser. It is narrower than
the full Windows SDK bundle and contains ONLY the artifacts needed
for browser/JWS integration.

CONTENTS
--------
lib/jws/   - IDCardToolkitService.jar  (self-contained JAR with
                                        embedded native DLLs for
                                        both x86 and x64, the
                                        dllConfig files, and the
                                        EIDAToolkitAgent)
           - PublicDataApplet-Sagem.jar (public data applet)
lib/plugins/ - IDCardToolkitService-Nexus.jar  (JWS agent, Richmond
                                                Nexus reader variant)
             - PublicDataApplet-Nexus.jar      (public data applet,
                                                Richmond Nexus reader variant)
lib/web/   - eidatoolkit.js
samples/   - web sample pages (console, modern, newica)
doc/       - 6 PDFs covering JWS/web deployment

WHAT IS NOT IN THIS BUNDLE
--------------------------
- Native C SDK (lib/c/, headers, import libs)
- Windows service / installer (ICAToolkitService.msi)
- .NET SDK and desktop samples
- Full documentation set

If you need any of the above, use the full Windows SDK bundle
instead: id-card-toolkit-windows-sdk-v3.1.6.zip

NOTES
-----
- The JARs must be signed before being served over HTTPS to a
  browser JVM. Signing is a separate step (not performed by this
  packager) and must be done with your own code-signing certificate
  before deployment.
- The JNLP descriptor shipped under samples/web/newica/ contains a
  placeholder codebase URL; customers must replace it with their
  own server's URL before deploying.
