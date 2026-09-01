package ae.emiratesid.idcard.toolkit.sample.toolkit_face.faceautocapture

import android.graphics.Bitmap
import com.innovatrics.dot.face.quality.FaceAspects
import com.innovatrics.dot.face.quality.FaceAttribute
import com.innovatrics.dot.face.quality.FaceQuality

data class FaceAutoCaptureResult(
    val bitmap: Bitmap,
    val confidence: Double?,
    val content: ByteArray
)
