# ICP's toolkit is called reflectively in places and its data models are populated from
# native code, so neither the classes nor their members survive shrinking on their own.
-keep class ae.emiratesid.idcard.toolkit.** { *; }
-dontwarn ae.emiratesid.idcard.toolkit.**

# Spongy Castle registers providers by name.
-keep class org.spongycastle.** { *; }
-dontwarn org.spongycastle.**

# MSAL, and the OkHttp/Retrofit stack it and we both use.
-keep class com.microsoft.identity.** { *; }
-dontwarn com.microsoft.identity.**
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
