package ae.emiratesid.idcard.toolkit.sample.fragment;

import android.nfc.Tag;
import android.os.Bundle;
import android.util.Base64;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.RadioGroup;

import ae.emiratesid.idcard.toolkit.internal.ErrorCode;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.task.PKIAuthAsync;
import ae.emiratesid.idcard.toolkit.sample.task.SignDataAsync;
import ae.emiratesid.idcard.toolkit.sample.task.VerifySignatureDataAsync;
import ae.emiratesid.idcard.toolkit.sample.widget.LogTextView;

import static ae.emiratesid.idcard.toolkit.sample.AppController.isReading;

public class SignDataFragment extends BaseFragment {


    public SignDataFragment() {
        // Required empty public constructor
    }

    @Override
    protected void appendError(String message) {

    }

    //Inflate the views
    private LogTextView txtResult;
    private EditText edtPlainText, edtUserPin;
    private Button btnSignData;
    private Button btnVerifyData;
    private RadioGroup rgCertType;
    private byte[] signedData;

    private static boolean isNFCMode;
    private Tag tag;


    String PLAIN_TEXT, PIN;
    int chkID, certificateType = 1;

    /**
     * Use this factory method to create a new instance of
     * this fragment using the provided parameters.
     * <p>
     * Use the {@link SignDataFragment#newInstance} factory method to
     *
     * @return A new instance of fragment SignDataFragment.
     */
    public static SignDataFragment newInstance(boolean isNFC) {
        SignDataFragment fragment = new SignDataFragment();
        isNFCMode = isNFC;
        return fragment;
    }

    public void setNfcMode(Tag tag) {
        this.tag = tag;
        isNFCMode = true;
        Logger.e("setNfcMode :: called");

        if (!isReading) {
            Logger.d("onResume :: enableForegroundDispatch called");
            //set the reading flag..
            isReading = true;
            txtResult.setText("");

            //show the dialog to provide user interaction...

            //create the object of ReaderCardDataAsync
            if (tag == null) {
                Logger.e("setNfcMode :: tag is null");
                return;
            } else {
                PLAIN_TEXT = edtPlainText.getText().toString();
                chkID = rgCertType.getCheckedRadioButtonId();
                certificateType = 1;

                if (chkID == R.id.radioButton) {
                    certificateType = Constants.SIGN_CERT;
                }//if()
                else if (chkID == R.id.radioButton2) {
                    certificateType = Constants.AUTH_CERT;
                }

                PIN = edtUserPin.getText().toString();

                // check against empty string
                if (PLAIN_TEXT.isEmpty()) {
                    edtPlainText.setError("NO data To sign.");
                    return;
                }//if()
                if (PIN.isEmpty()) {
                    edtUserPin.setError("PIN is required");
                }

                Logger.e("setNfcMode :: calling PIN Authentication");
                showProgressDialog("Signing...");
                signDataAsync = new SignDataAsync(PIN, PLAIN_TEXT.getBytes(), certificateType, signDataListener,tag);
            }
            signDataAsync.execute();
        }//
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_sign_data, container, false);
        txtResult = (LogTextView) view.findViewById(R.id.txtResult);
        edtPlainText = (EditText) view.findViewById(R.id.editText);
        edtUserPin = (EditText) view.findViewById(R.id.editPin);
        btnSignData = (Button) view.findViewById(R.id.btnSign);
        btnVerifyData = (Button) view.findViewById(R.id.btnVerify);
        rgCertType = (RadioGroup) view.findViewById(R.id.radioGroupCertType);
        btnSignData.setOnClickListener(this);
        btnVerifyData.setOnClickListener(this);
        return view;
    }//onCreateView()

    //create SignDataAsync.SignDataListener
    private SignDataAsync.SignDataListener signDataListener = new SignDataAsync.SignDataListener() {
        @Override
        public void onDataSigned(int status, String message, byte[] data) {
            hideProgressDialog();
            isReading = false;
            if (status == Constants.SUCCESS) {
                btnVerifyData.setEnabled(true);
                signedData = data;
                if (signedData == null || signedData.length == 0) {
                    txtResult.appendLog("Signing data Failed ", LogTextView.LOG_TYPE.ERROR);
                    txtResult.appendLog(message, LogTextView.LOG_TYPE.ERROR);
                }
                txtResult.appendLog(Base64.encodeToString(signedData, Base64.DEFAULT),
                        LogTextView.LOG_TYPE.INFO);
            }//if()
            else {
                txtResult.appendLog("Signing data Failed ", LogTextView.LOG_TYPE.ERROR);
                txtResult.appendLog(message, LogTextView.LOG_TYPE.ERROR);

            }//
        }//onDataSigned
    };//signDataListener

    //create SignDataAsync.SignDataListener
    private VerifySignatureDataAsync.VerifyDataListener verifySignatureDataListener =
            new VerifySignatureDataAsync.VerifyDataListener() {
                @Override
                public void onDataVerified(int status, String message) {
                    hideProgressDialog();
                    isReading = false;
                    if (status == 0) {
                        txtResult.appendLog("Signature is valid", LogTextView.LOG_TYPE.SUCCESS);
                    }//if()
                    else {
                        if (status != ErrorCode.UNSPECIFIED.getCode()) {
                            txtResult.setLog("Error Code : " + status + "\nSignature not verified. \n" + message,
                                    LogTextView.LOG_TYPE.ERROR);
                        } else {
                            txtResult.setLog("Signature not verified. \n" + message,
                                    LogTextView.LOG_TYPE.ERROR);
                        }
                    }//else()
                }//onDataVerified()
            };//signDataListener
    //create  SignDataAsync
    private SignDataAsync signDataAsync;
    private VerifySignatureDataAsync verifySignatureDataAsync;

    @Override
    public void onClick(View view) {

        txtResult.setText("");
        if (isReading) {
            txtResult.setLog("Reading in Process", LogTextView.LOG_TYPE.ERROR);
            return;
        }//if()
        PLAIN_TEXT = edtPlainText.getText().toString();
        chkID = rgCertType.getCheckedRadioButtonId();
        certificateType = 1;

        if (chkID == R.id.radioButton) {
            certificateType = Constants.SIGN_CERT;
        }//if()
        else if (chkID == R.id.radioButton2) {
            certificateType = Constants.AUTH_CERT;
        }//else if()

        if (view == btnSignData) {
            PIN = edtUserPin.getText().toString();

            // check against empty string
            if (PLAIN_TEXT.isEmpty()) {
                edtPlainText.setError("NO data To sign.");
                return;
            }//if()
            if (PIN.isEmpty()) {
                edtUserPin.setError("PIN is required");
            }//if()

            //call the async task
            showProgressDialog("Signing...");
            isReading = true;
            signDataAsync = new SignDataAsync(PIN, PLAIN_TEXT.getBytes(), certificateType, signDataListener);
            signDataAsync.execute();
        }//if()
        if (view == btnVerifyData) {
            if (signedData == null) {
                txtResult.setLog("Signed data is null.",
                        LogTextView.LOG_TYPE.ERROR);
                return;
            }//if()
            final String PIN = edtUserPin.getText().toString();

            if (PIN.isEmpty()) {
                edtUserPin.setError("PIN is required");
            }//if()

            if (chkID == R.id.radioButton) {
                certificateType = Constants.SIGN_CERT;
            }//if()
            else if (chkID == R.id.radioButton2) {
                certificateType = Constants.AUTH_CERT;
            }//else if()

            showProgressDialog("Verifying signature...");
            isReading = true;
            try {

                verifySignatureDataAsync = new VerifySignatureDataAsync
                        (PLAIN_TEXT.getBytes(), signedData,
                                certificateType, verifySignatureDataListener, false, PIN);
                verifySignatureDataAsync.execute();
            } catch (Exception e) {
                e.printStackTrace();
                txtResult.appendLog("Verifiaction failed " + e.getLocalizedMessage(),
                        LogTextView.LOG_TYPE.ERROR);
            }//catch()
        }//if()
    }//onclick()
}//end of class
