package ae.emiratesid.idcard.toolkit.sample.smileliveness

import ae.emiratesid.idcard.toolkit.sample.R
import ae.emiratesid.idcard.toolkit.sample.toolkit_face.ToolkitFace
import android.app.Activity
import android.content.Intent
import android.graphics.Bitmap
import android.os.Bundle
import android.os.Environment
import android.util.Base64
import android.util.Log
import android.view.View
import android.widget.ImageView
import androidx.fragment.app.Fragment
import androidx.fragment.app.activityViewModels
import java.io.ByteArrayOutputStream
import java.io.File
import java.io.FileOutputStream
import java.util.Random


class SmileLivenessResultFragment : Fragment(R.layout.fragment_smile_liveness_result) {

    private val smileLivenessViewModel: SmileLivenessViewModel by activityViewModels()

    private lateinit var imageView: ImageView

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
        setViews(view)
        setupSmileLivenessViewModel()
    }

    private fun setViews(view: View) {
        imageView = view.findViewById(R.id.image)
    }

    private fun setupSmileLivenessViewModel() {
        smileLivenessViewModel.state.observe(viewLifecycleOwner) { showResult(it.result!!) }
    }

    private fun showResult(result: SmileLivenessResult) {
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
