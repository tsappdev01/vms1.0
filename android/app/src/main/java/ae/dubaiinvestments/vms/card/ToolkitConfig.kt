package ae.dubaiinvestments.vms.card

import android.content.Context
import android.util.Log
import java.io.File

/**
 * Gets ICP's configuration onto the device and builds the config string the toolkit wants.
 *
 * The config files ship inside the APK and are copied to the app's private storage on
 * first run. ICP's own sample puts them on external storage instead, which on any Android
 * this app targets means either a permission prompt reception should not be answering or
 * a path that is not writable at all. Private storage needs no permission, is wiped with
 * the app, and cannot be read by anything else on the tablet - which matters, because a
 * licence file is part of what ICP issued to Dubai Investments.
 */
object ToolkitConfig {

    private const val TAG = "VmsToolkitConfig"
    private const val ASSET_DIR = "toolkit-config"

    /** The file every bundle has. Its absence is the useful thing to detect. */
    private const val SENTINEL = "config_li"

    /** ICP's agent configuration. Documented as mandatory for online mode, so withholding
        it is how a read is kept off the Validation Gateway. */
    private const val AgentConfig = "config_ag"

    /**
     * Extracts the bundle if what is on disk is not from this build, and returns the
     * directory.
     *
     * The stamp is the package's install time, not a "does it exist" check. An updated
     * APK carrying a new bundle - a renewed licence, most likely - would otherwise keep
     * running against the old files, and the symptom would arrive months later as an
     * expired licence that had already been replaced.
     *
     * @throws CardReadException if the APK carries no bundle - a build assembled without
     *   ICP's files, which is worth naming rather than letting the toolkit report
     *   "invalid or incomplete configuration data".
     */
    fun ensureExtracted(context: Context, offline: Boolean = false): File {
        /* Its own directory, so moving the switch re-extracts rather than leaving a
           config_ag behind from the last time. */
        val dirName = if (offline) "$ASSET_DIR-offline" else ASSET_DIR
        val target = File(context.filesDir, dirName)
        val stamp = File(context.filesDir, "$dirName.stamp")

        val names = runCatching { context.assets.list(ASSET_DIR)?.toList() }.getOrNull().orEmpty()
        if (names.isEmpty()) {
            throw CardReadException(
                "This build carries no ICP toolkit configuration. " +
                    "The config bundle has to be in app/src/main/assets/$ASSET_DIR - see android/README.md.",
            )
        }

        val installedAt = runCatching {
            context.packageManager.getPackageInfo(context.packageName, 0).lastUpdateTime
        }.getOrDefault(0L).toString()

        if (File(target, SENTINEL).exists() &&
            runCatching { stamp.readText() }.getOrNull() == installedAt
        ) {
            Log.d(TAG, "Toolkit config already extracted to $target")
            return target
        }

        val copied = copyAssets(context, ASSET_DIR, target, skip = if (offline) AgentConfig else null)
        stamp.writeText(installedAt)

        Log.i(TAG, "Extracted $copied toolkit config files to $target")
        return target
    }

    /**
     * Recursive, because a bundle ICP reshapes into subdirectories should not turn into a
     * FileNotFoundException on a directory name. `assets.list` does not say which entries
     * are directories, so the test is whether an entry has children of its own.
     */
    private fun copyAssets(context: Context, assetPath: String, target: File, skip: String? = null): Int {
        target.mkdirs()
        var count = 0

        for (name in context.assets.list(assetPath).orEmpty()) {
            if (skip != null && name == skip) {
                Log.i(TAG, "Leaving $name out: the toolkit is being kept offline.")
                continue
            }

            val child = "$assetPath/$name"
            val children = runCatching { context.assets.list(child) }.getOrNull().orEmpty()

            count += if (children.isNotEmpty()) {
                copyAssets(context, child, File(target, name), skip)
            } else {
                context.assets.open(child).use { input ->
                    File(target, name).outputStream().use(input::copyTo)
                }
                1
            }
        }

        return count
    }

    /**
     * The configuration, as newline-separated `key = value`.
     *
     * Not JSON. ICP's own quickstart documents a JSON example and the toolkit rejects it
     * with "Invalid or incomplete configuration data"; this is the shape the working
     * Windows configuration uses and the shape ICP's Android sample builds.
     *
     * `plugin_directory_path` is the APK's own native library directory. The ACS driver's
     * `.so` is packaged into the app by the :acs-plugin module and unpacked there at
     * install time, so this is where the toolkit finds it - not a path on the device that
     * somebody has to populate.
     *
     * There used to be a `read_publicdata_offline = true` line here, carried over from the
     * Windows configuration and described in this comment as the reason a read came back
     * unsigned. It was neither. That key appears in no ICP document - not the Programmer's
     * Reference, not the Android guide - and in no string table in the toolkit's own native
     * libraries, all of which were searched. The toolkit never recognised it, so every read
     * this app has ever done went out in online mode, and a tablet that could not reach
     * ICP's Validation Gateway failed with code 233, ETSTATUS_SERVER_RESPONSE_ERROR:
     * "Failed to get response from server".
     *
     * Offline is chosen by what the configuration directory holds, which is why it is done
     * in [ensureExtracted] rather than here.
     */
    fun build(context: Context, configDir: File, logDir: File): String = buildString {
        appendLine("config_directory = ${configDir.absolutePath}")
        appendLine("log_directory = ${logDir.absolutePath}")
        appendLine("plugin_directory_path = ${context.applicationInfo.nativeLibraryDir}/")
    }

    fun logDirectory(context: Context): File =
        File(context.filesDir, "toolkit-logs").apply { mkdirs() }
}
