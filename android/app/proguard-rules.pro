# ICP's toolkit is called reflectively in places and its data models are populated from
# native code, so neither the classes nor their members survive shrinking on their own.
-keep class ae.emiratesid.idcard.toolkit.** { *; }
-dontwarn ae.emiratesid.idcard.toolkit.**

# Spongy Castle registers providers by name.
-keep class org.spongycastle.** { *; }
-dontwarn org.spongycastle.**

# The OkHttp/Retrofit stack.
-dontwarn okhttp3.**
-dontwarn okio.**

# kotlinx.serialization generates serializers that R8 cannot see are used.
-keepclassmembers class ae.dubaiinvestments.vms.api.** {
    *** Companion;
    kotlinx.serialization.KSerializer serializer(...);
}
-keepclasseswithmembers class ae.dubaiinvestments.vms.api.** {
    public static ** INSTANCE;
}

# The ACS reader driver. The toolkit finds its plugins by name through its own loader, so
# nothing in the app references these classes and R8 has no reason to believe they are used.
-keep class com.acs.** { *; }
-dontwarn com.acs.**

# The XML signature library, used on the gateway response.
-keep class org.apache.xml.security.** { *; }
-dontwarn org.apache.xml.security.**
