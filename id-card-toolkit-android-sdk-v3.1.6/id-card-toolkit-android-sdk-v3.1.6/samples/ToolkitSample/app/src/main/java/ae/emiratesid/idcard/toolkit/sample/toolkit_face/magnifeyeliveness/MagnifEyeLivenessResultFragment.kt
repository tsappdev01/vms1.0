package ae.emiratesid.idcard.toolkit.sample.magnifeyeliveness

import android.os.Bundle
import android.view.View
import android.widget.ImageView
import androidx.fragment.app.Fragment
import androidx.fragment.app.activityViewModels
import ae.emiratesid.idcard.toolkit.sample.R
import ae.emiratesid.idcard.toolkit.sample.toolkit_face.ToolkitFace
import android.app.Activity
import android.content.Intent
import android.graphics.Bitmap
import android.os.Environment
import android.util.Base64
import java.io.ByteArrayOutputStream
import java.io.File
import java.io.FileOutputStream

class MagnifEyeLivenessResultFragment : Fragment(R.layout.fragment_magnifeye_liveness_result) {

    private val magnifEyeLivenessViewModel: MagnifEyeLivenessViewModel by activityViewModels()

    private lateinit var imageView: ImageView

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
        setViews(view)
        setupMagnifEyeLivenessViewModel()
    }

    private fun setViews(view: View) {
        imageView = view.findViewById(R.id.image)
    }

    private fun setupMagnifEyeLivenessViewModel() {
        magnifEyeLivenessViewModel.state.observe(viewLifecycleOwner) { showResult(it.result!!) }
    }

    private fun showResult(result: MagnifEyeLivenessResult) {
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
