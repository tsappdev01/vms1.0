package ae.emiratesid.idcard.toolkit.sample

import ae.emiratesid.idcard.toolkit.sample.toolkit_face.ToolkitFace
import android.app.Dialog
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import androidx.appcompat.widget.AppCompatEditText
import androidx.appcompat.widget.AppCompatTextView
import androidx.core.os.bundleOf
import androidx.fragment.app.Fragment
import androidx.navigation.fragment.findNavController
import com.innovatrics.dot.camera.CameraFacing
import com.innovatrics.dot.face.autocapture.FaceAutoCaptureConfiguration
import com.innovatrics.dot.face.autocapture.FaceAutoCaptureFragment
import com.innovatrics.dot.face.liveness.magnifeye.MagnifEyeLivenessConfiguration
import com.innovatrics.dot.face.liveness.magnifeye.MagnifEyeLivenessFragment
import com.innovatrics.dot.face.liveness.smile.SmileLivenessConfiguration
import com.innovatrics.dot.face.liveness.smile.SmileLivenessFragment

class HomeFragment : Fragment(R.layout.fragment_home) {
    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

        var cameraFacing: CameraFacing
        if (ToolkitFace.getCameraMode() == ToolkitFace.FRONT_CAMERA) {
            cameraFacing = CameraFacing.FRONT
        } else {
            cameraFacing = CameraFacing.BACK
        }

        if (ToolkitFace.getLivelinessMode() == ToolkitFace.AUTO_CAPTURE) {

            var faceAutoCaptureConfiguration =
                FaceAutoCaptureConfiguration.Builder().cameraFacing(cameraFacing)

            val configuration: FaceAutoCaptureConfiguration = faceAutoCaptureConfiguration.build()

            val bundle = bundleOf(
                FaceAutoCaptureFragment.CONFIGURATION to configuration
            )

            findNavController().navigate(
                R.id.action_HomeFragment_to_BasicFaceAutoCaptureFragment, bundle
            )
        } else if (ToolkitFace.getLivelinessMode() == ToolkitFace.SMILE_LIVELINESS) {

            var smileLivenessConfiguration =
                SmileLivenessConfiguration.Builder().cameraFacing(cameraFacing)

            val configuration: SmileLivenessConfiguration = smileLivenessConfiguration.build()

            val bundle = bundleOf(
                SmileLivenessFragment.CONFIGURATION to configuration
            )
            findNavController().navigate(
                R.id.action_HomeFragment_to_BasicSmileLivenessFragment, bundle
            )
        } else {
            var magnifyLivenessConfiguration =
                MagnifEyeLivenessConfiguration.Builder().cameraFacing(cameraFacing)

            val configuration: MagnifEyeLivenessConfiguration = magnifyLivenessConfiguration.build()

            val bundle = bundleOf(
                MagnifEyeLivenessFragment.CONFIGURATION to configuration
            )
            findNavController().navigate(
                R.id.action_HomeFragment_to_BasicMagnifEyeLivenessFragment, bundle
            )
        }
    }
}
