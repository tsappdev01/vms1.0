package ae.emiratesid.idcard.toolkit.sample.smileliveness

import com.innovatrics.dot.image.BitmapFactory
import kotlinx.coroutines.CoroutineDispatcher
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class CreateUiResultUseCase(
    private val ioDispatcher: CoroutineDispatcher = Dispatchers.IO,
) {

    suspend operator fun invoke(smileLivenessResult: com.innovatrics.dot.face.liveness.smile.SmileLivenessResult):
            SmileLivenessResult = withContext(ioDispatcher) {
        SmileLivenessResult(
            smileLivenessResult = smileLivenessResult,
            bitmap = BitmapFactory.create(smileLivenessResult.bgrRawImage),
            confidence = smileLivenessResult.detectedFace.getConfidence(),
            content = smileLivenessResult.content
        )
    }
}
