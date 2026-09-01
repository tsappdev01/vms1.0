package ae.emiratesid.idcard.toolkit.sample.smileliveness

import android.graphics.Bitmap
import com.innovatrics.dot.face.quality.FaceAspects
import com.innovatrics.dot.face.quality.FaceAttribute
import com.innovatrics.dot.face.quality.FaceQuality

data class SmileLivenessResult(
    val smileLivenessResult: com.innovatrics.dot.face.liveness.smile.SmileLivenessResult,
    val bitmap: Bitmap,
    val confidence: Double?,
    val content: ByteArray

)
