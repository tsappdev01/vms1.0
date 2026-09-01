package ae.emiratesid.idcard.toolkit.sample.fragment;

import android.app.Dialog;
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
import android.widget.Toast;

import androidx.appcompat.widget.AppCompatTextView;

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

public class AuthenticateFacePassportFragment extends AuthenticateFaceFragment implements View.OnClickListener {
    private Button btnCaptureFace;
    private Button btnOptions;
    private EditText edtPassportNumber;
    private EditText edtPassportCountry;
    private EditText edtPassportExpiry;
    private EditText edtPassportDob;
    private LogTextView textViewStatus;
    private Context mContext;

    public AuthenticateFacePassportFragment(int requestCode) {
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_authenticate_face_passport, container, false);
        textViewStatus = (LogTextView) view.findViewById(R.id.tv_capture_face);
        btnCaptureFace = (Button) view.findViewById(R.id.btn_capture_face);
        btnOptions = (Button) view.findViewById(R.id.btnOptions);
        edtPassportNumber = (EditText) view.findViewById(R.id.edt_passport_number);
        edtPassportCountry = (EditText) view.findViewById(R.id.edt_passport_country_code);
        edtPassportExpiry = (EditText) view.findViewById(R.id.edt_passport_expiry);
        edtPassportDob = (EditText) view.findViewById(R.id.edt_passport_dob);

        textViewStatus.setMovementMethod(new ScrollingMovementMethod());

        btnCaptureFace.setOnClickListener(this);
        btnOptions.setOnClickListener(this);
        return view;
    }

    @Override
    public void onClick(View view) {
        if (view == btnCaptureFace) {

            textViewStatus.setText("");

            String sPassportNumber = edtPassportNumber.getText().toString();
            String sPassportCountry = edtPassportCountry.getText().toString();
            String sPassportExpiry = edtPassportExpiry.getText().toString();
            String sPassportDob = edtPassportDob.getText().toString();
            if (sPassportNumber.isEmpty()) {
                edtPassportNumber.setError("required");
                return;
            }
            if (sPassportCountry.isEmpty()) {
                edtPassportNumber.setError("required");
                return;
            }
            if (sPassportExpiry.isEmpty()) {
                edtPassportNumber.setError("required");
                return;
            }
            if (sPassportDob.isEmpty()) {
                edtPassportNumber.setError("required");
                return;
            }
            Toolkit toolkit = null;
            try {
                toolkit = ConnectionController.getToolkit();
                if (toolkit == null) {
                    Toast.makeText(mContext, "Toolkit is not initialized.", Toast.LENGTH_SHORT).show();
                    return;
                }
                ToolkitFace.AuthenticateFaceResultInterface
                        authenticateFaceResultInterface = new AuthenticateFaceListener();
                ToolkitFace toolkitFace = new ToolkitFace();
                toolkitFace.authenticateFaceWithPassport(getActivity(),
                        edtPassportNumber.getText().toString(),
                        edtPassportCountry.getText().toString(),
                        edtPassportExpiry.getText().toString(),
                        edtPassportDob.getText().toString(), true,
                        authenticateFaceResultInterface);
            } catch (ToolkitException e) {
                textViewStatus.setLog(e.getMessage(), LogTextView.LOG_TYPE.ERROR);

            }
        } else if (view == btnOptions) {
            showFaceSDKOptions(view);
        }
    }

    private class AuthenticateFaceListener implements ToolkitFace.AuthenticateFaceResultInterface {
        @Override
        public void onAuthenticateFaceResult(int status, String message, CardPublicData cardPublicData) {
            if (status == Constants.SUCCESS) {
                ToolkitFace.setCardPublicData(cardPublicData);
                Logger.i("Face authentication successful:" + message);
                Intent intent = new Intent(getActivity(), ProfileActivity.class);
                startActivity(intent);
                getActivity().finish();
            } else {
                textViewStatus.setLog(message, LogTextView.LOG_TYPE.ERROR);
                Logger.e("Face authentication failed:" + message);
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
}