package ae.emiratesid.idcard.toolkit.sample

import ae.emiratesid.idcard.toolkit.sample.toolkit_face.ToolkitFace
import android.content.Context
import android.util.Base64
import com.innovatrics.dot.core.DotSdk
import com.innovatrics.dot.core.DotSdkConfiguration
import com.innovatrics.dot.face.DotFaceLibrary
import com.innovatrics.dot.face.DotFaceLibraryConfiguration
import com.innovatrics.dot.face.detection.balanced.DotFaceDetectionBalancedModule
import com.innovatrics.dot.face.expressionneutral.DotFaceExpressionNeutralModule
import com.innovatrics.dot.face.passiveliveness.DotFacePassiveLivenessModule
import kotlinx.coroutines.CoroutineDispatcher
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class InitializeDotSdkUseCase(
    private val dotSdk: DotSdk = DotSdk,
    private val dispatcher: CoroutineDispatcher = Dispatchers.IO,
) {

    suspend operator fun invoke(context: Context) = withContext(dispatcher) {
        println("log  " + ToolkitFace.face_config)
        val configuration = createDotSdkConfiguration(context)
        dotSdk.initialize(configuration)
    }

    private fun createDotSdkConfiguration(context: Context) = DotSdkConfiguration(
        context = context,

        licenseBytes = Base64.decode(
            ToolkitFace.face_config,
            Base64.DEFAULT
        ),
        // readLicenseBytes(context.resources),
        libraries = listOf(
            //DotDocumentLibrary(),
            createDotFaceLibrary(),
            //DotNfcLibrary(),
        ),
    )
    // private fun readLicenseBytes(resources: Resources) = resources.openRawResource(R.raw.dot_license).use(InputStream::readBytes)

    private fun createDotFaceLibrary(): DotFaceLibrary {
        val modules = createDotFaceLibraryModules()
        val configuration = DotFaceLibraryConfiguration(modules)
        return DotFaceLibrary(configuration)
    }

    private fun createDotFaceLibraryModules() = listOf(
        DotFaceDetectionBalancedModule.of(),
        DotFaceExpressionNeutralModule.of(),
        DotFacePassiveLivenessModule.of(),
    )
}
