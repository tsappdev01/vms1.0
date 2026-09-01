package ae.emiratesid.idcard.toolkit.sample.magnifeyeliveness

import android.graphics.Bitmap

data class MagnifEyeLivenessResult(
    val bitmap: Bitmap,
    val confidence:Double,
    val content: ByteArray
)
