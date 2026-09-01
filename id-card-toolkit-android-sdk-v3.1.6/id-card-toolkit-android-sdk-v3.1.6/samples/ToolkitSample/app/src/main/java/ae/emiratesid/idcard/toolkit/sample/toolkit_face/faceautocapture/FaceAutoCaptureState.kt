package ae.emiratesid.idcard.toolkit.sample.toolkit_face.faceautocapture

data class FaceAutoCaptureState(
    val isProcessing: Boolean = false,
    val result: FaceAutoCaptureResult? = null,
    val errorMessage: String? = null,
)
