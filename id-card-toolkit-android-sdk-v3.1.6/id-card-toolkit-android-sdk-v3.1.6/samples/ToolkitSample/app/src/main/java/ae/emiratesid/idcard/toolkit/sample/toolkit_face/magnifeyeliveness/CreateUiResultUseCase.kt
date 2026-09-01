package ae.emiratesid.idcard.toolkit.sample.magnifeyeliveness

import com.innovatrics.dot.image.BitmapFactory
import kotlinx.coroutines.CoroutineDispatcher
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class CreateUiResultUseCase(
    private val dispatcher: CoroutineDispatcher = Dispatchers.IO,
) {

    suspend operator fun invoke(magnifEyeLivenessResult: com.innovatrics.dot.face.liveness.magnifeye.MagnifEyeLivenessResult): MagnifEyeLivenessResult =
        withContext(dispatcher) {
            MagnifEyeLivenessResult(
                bitmap = BitmapFactory.create(magnifEyeLivenessResult.bgrRawImage),
                confidence = magnifEyeLivenessResult.detectedFace.getConfidence(),
                content = magnifEyeLivenessResult.content
            )
        }
}
