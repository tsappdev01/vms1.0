package ae.emiratesid.idcard.toolkit.sample.fragment;


import android.app.Dialog;
import android.app.ProgressDialog;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;

import androidx.appcompat.widget.AppCompatTextView;
import androidx.fragment.app.Fragment;

import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.toolkit_face.ToolkitFace;


public abstract class AuthenticateFaceFragment extends BaseFragment implements View.OnClickListener {
    private ProgressDialog pd;

    public AuthenticateFaceFragment() {
        // Required empty public constructor
    }

    void showFaceSDKOptions(View root) {
        Dialog dialog2 = new Dialog(requireActivity());
        ViewGroup viewGroup = root.findViewById(android.R.id.content);
        View view = LayoutInflater.from(requireActivity())
                .inflate(R.layout.dialog_face_sdk_select, viewGroup, false);
        //set custom dialog
        //set custom dialog
        dialog2.setContentView(view);
        //set custom height and width
        dialog2.setCancelable(false);

        //show dialog
        dialog2.show();

        AppCompatTextView title = view.findViewById(R.id.title);
        AppCompatTextView button1 = view.findViewById(R.id.button1);
        AppCompatTextView button2 = view.findViewById(R.id.button2);
        AppCompatTextView button3 = view.findViewById(R.id.button3);
        AppCompatTextView cancel_btn = view.findViewById(R.id.cancel_btn);
        View view1 = view.findViewById(R.id.view1);
        view1.setVisibility(View.VISIBLE);
        button3.setVisibility(View.VISIBLE);

        title.setText("Liveness Mode");
        button1.setText("Smile Liveness Capture");
        button2.setText("Face Auto Capture");
        button3.setText("Magnify Eye Liveness Capture");
        // smile capture button
        button1.setOnClickListener(v -> {
            ToolkitFace.setLivelinessMode(ToolkitFace.SMILE_LIVELINESS);
            showCameraMode(view);
            dialog2.dismiss();
        });

        button2.setOnClickListener(v -> {
            ToolkitFace.setLivelinessMode(ToolkitFace.AUTO_CAPTURE);
            showCameraMode(view);
            dialog2.dismiss();
        });

        button3.setOnClickListener(v -> {
            ToolkitFace.setLivelinessMode(ToolkitFace.MAGNIFY_EYE_LIVELINESS);
            showCameraMode(view);
            dialog2.dismiss();
        });

        cancel_btn.setOnClickListener(v -> {
            dialog2.dismiss();
        });
    }

    private void showCameraMode(View root) {
        Dialog dialog2 = new Dialog(requireActivity());
        ViewGroup viewGroup = root.findViewById(android.R.id.content);
        View view = LayoutInflater.from(requireActivity())
                .inflate(R.layout.dialog_face_sdk_select, viewGroup, false);
        //set custom dialog
        //set custom dialog
        dialog2.setContentView(view);
        //set custom height and width
        dialog2.setCancelable(false);

        //show dialog
        dialog2.show();

        AppCompatTextView title = view.findViewById(R.id.title);
        AppCompatTextView button1 = view.findViewById(R.id.button1);
        AppCompatTextView button2 = view.findViewById(R.id.button2);
        AppCompatTextView cancel_btn = view.findViewById(R.id.cancel_btn);

        title.setText("Camera Mode");
        button1.setText("Front Camera");
        button2.setText("Back Camera");
        // smile capture button
        button1.setOnClickListener(v -> {
            ToolkitFace.setCameraMode(ToolkitFace.FRONT_CAMERA);
            dialog2.dismiss();
        });

        button2.setOnClickListener(v -> {
            ToolkitFace.setCameraMode(ToolkitFace.BACK_CAMERA);
            dialog2.dismiss();
        });

        cancel_btn.setOnClickListener(v -> {
            dialog2.dismiss();
        });
    }
}
