package ae.emiratesid.idcard.toolkit.sample.fragment;

import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;

import com.example.justocr.MrzOcrActivity;

import ae.emiratesid.idcard.toolkit.Toolkit;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardPublicData;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.ProfileActivity;
import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.toolkit_face.ToolkitFace;
import ae.emiratesid.idcard.toolkit.sample.widget.LogTextView;

public class AuthenticateFaceIdnFragment extends AuthenticateFaceFragment implements View.OnClickListener {

    private static final int MRZ_REQUEST = 101;
    private int requestCodeResult;
    private Button btnCaptureFace;
    private Button btnOptions;
    private Button btnReadMrz;
    private Button btnOpenEmiratesId;
    private Button btnOpenSignature;
    private Button btnOpenResidenceHolder;
    private EditText edtIdn;
    private LogTextView textViewStatus;
    private TextView tvIdNumber;
    private Context mContext;
    private ImageView ivCardHolderPhoto;
    private LinearLayout llCardPublicData;

    public AuthenticateFaceIdnFragment(int requestCode) {
        this.requestCodeResult = requestCode;
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_authenticate_face_idn, container, false);
        textViewStatus = (LogTextView) view.findViewById(R.id.tv_capture_face);
        btnOptions = (Button) view.findViewById(R.id.btnOptions);
        btnCaptureFace = (Button) view.findViewById(R.id.btn_capture_face);
        btnOpenEmiratesId = (Button) view.findViewById(R.id.btn_holderEmiratesIDImage);
        btnReadMrz = (Button) view.findViewById(R.id.btn_read_mrz_face_auth);
        btnOpenResidenceHolder = (Button) view.findViewById(R.id.btn_holderResidenceImage);
        edtIdn = (EditText) view.findViewById(R.id.edt_idn);
        ivCardHolderPhoto = (ImageView) view.findViewById(R.id.ivPhoto);
        btnOpenSignature = (Button) view.findViewById(R.id.btn_Signature);
        tvIdNumber = (TextView) view.findViewById(R.id.tv_id_number);
        llCardPublicData = (LinearLayout) view.findViewById(R.id.ll_card_public_data);

        textViewStatus.setMovementMethod(new ScrollingMovementMethod());

        btnOptions.setOnClickListener(this);
        btnCaptureFace.setOnClickListener(this);
        btnReadMrz.setOnClickListener(this);
        btnOpenEmiratesId.setOnClickListener(this);
        btnOpenResidenceHolder.setOnClickListener(this);
        btnOpenSignature.setOnClickListener(this);

        return view;
    }

    public AuthenticateFaceIdnFragment() {
        // Required empty public constructor
    }

    @Override
    public void onClick(View view) {
        if (view == btnCaptureFace) {
            textViewStatus.setText("");
            String sIdn = edtIdn.getText().toString();
            if (!sIdn.isEmpty()) {
                Toolkit toolkit = null;
                try {
                    toolkit = ConnectionController.getToolkit();
                    if (toolkit == null) {
                        Toast.makeText(mContext, "Toolkit is not initialized.", Toast.LENGTH_SHORT).show();
                        return;
                    }

                    ToolkitFace.AuthenticateFaceResultInterface authenticateFaceResultInterface = new AuthenticateFaceListener();
                    ToolkitFace toolkitFace = new ToolkitFace();
                    toolkitFace.authenticateFaceWithId(getActivity(),
                            edtIdn.getText().toString(), true,
                            authenticateFaceResultInterface);

                } catch (ToolkitException e) {
                    textViewStatus.setLog(e.getMessage(), LogTextView.LOG_TYPE.ERROR);
                }
            } else {
                edtIdn.setError("required");
            }
        } else if (view == btnReadMrz) {
            Intent intent = new Intent(getActivity(), MrzOcrActivity.class);
            startActivityForResult(intent, MRZ_REQUEST);
        } else if (view == btnOptions) {
            showFaceSDKOptions(view);
        }
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == MRZ_REQUEST) {
            if (resultCode == getActivity().RESULT_OK) {
                if (data != null) {
                    Logger.d("Status " + data.getBooleanExtra("STATUS", false));
                    if (data.getBooleanExtra("STATUS", false)) {
                        String ocr = data.getStringExtra("OCR");
                        String[] splitArray = ocr.split(",");
                        String idn = splitArray[9].split("=")[1];

                        edtIdn.setText(idn);
                    }
                }
            }
        }
    }

    @Override
    public void onAttach(Context context) {
        super.onAttach(context);
        mContext = context;
    }


    @Override
    public void onDetach() {
        super.onDetach();
        mContext = null;
    }

    @Override
    protected void appendError(String message) {
        textViewStatus.appendLog(message, LogTextView.LOG_TYPE.ERROR);
    }

    private class AuthenticateFaceListener implements ToolkitFace.AuthenticateFaceResultInterface {
        @Override
        public void onAuthenticateFaceResult(int status, String message,
                                             CardPublicData cardPublicData) {
            if (status == Constants.SUCCESS) {
                ToolkitFace.setCardPublicData(cardPublicData);
                Logger.i("Face authentication successful:" + message + "\t" + cardPublicData + "\t" + status);
                Intent intent = new Intent(getActivity(), ProfileActivity.class);
                startActivity(intent);
                getActivity().finish();
            } else {
                llCardPublicData.setVisibility(View.GONE);
                textViewStatus.setLog(message, LogTextView.LOG_TYPE.ERROR);
                Logger.e("Face authentication failed:" + message);
            }
        }
    }

}