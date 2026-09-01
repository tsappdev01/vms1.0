package ae.emiratesid.idcard.toolkit.sample.toolkit_face.faceautocapture

import ae.emiratesid.idcard.toolkit.sample.R
import ae.emiratesid.idcard.toolkit.sample.toolkit_face.ToolkitFace
import ae.emiratesid.idcard.toolkit.sample.ui.createGson
import android.app.Activity
import android.content.Context
import android.content.Intent
import android.graphics.Bitmap
import android.net.Uri
import android.os.Bundle
import android.os.Environment
import android.provider.MediaStore
import android.util.Base64
import android.view.View
import android.widget.ImageView
import android.widget.TextView
import androidx.fragment.app.Fragment
import androidx.fragment.app.activityViewModels
import java.io.ByteArrayOutputStream
import java.io.File
import java.io.FileOutputStream


class FaceAutoCaptureResultFragment : Fragment(R.layout.fragment_face_auto_capture_result) {

    private val faceAutoCaptureViewModel: FaceAutoCaptureViewModel by activityViewModels()
    private val gson = createGson()

    private lateinit var imageView: ImageView
    private lateinit var textView: TextView

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
        setViews(view)
        setupFaceAutoCaptureViewModel()
    }

    private fun setViews(view: View) {
        imageView = view.findViewById(R.id.image)
        textView = view.findViewById(R.id.text)
    }

    private fun setupFaceAutoCaptureViewModel() {
        faceAutoCaptureViewModel.state.observe(viewLifecycleOwner) { showResult(it.result!!) }
    }

    private fun showResult(result: FaceAutoCaptureResult) {
        val data = result.content
        val liveness_data = Base64.encodeToString(data, Base64.DEFAULT)
        ToolkitFace.set_liveness_data(liveness_data)

        val byteArrayOutputStream = ByteArrayOutputStream()
        result.bitmap.compress(Bitmap.CompressFormat.JPEG, 100, byteArrayOutputStream)
        val byteArray = byteArrayOutputStream.toByteArray()
        val base64encode: String = Base64.encodeToString(byteArray, Base64.DEFAULT)
        ToolkitFace.setCapture_face(base64encode)
        val returnIntent = Intent()
        returnIntent.putExtra("result", true)
        requireActivity().setResult(Activity.RESULT_OK, returnIntent)
        requireActivity().onBackPressed()
    }
}
