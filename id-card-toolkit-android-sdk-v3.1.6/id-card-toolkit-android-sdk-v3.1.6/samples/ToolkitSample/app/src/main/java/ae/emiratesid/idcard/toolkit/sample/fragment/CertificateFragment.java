package ae.emiratesid.idcard.toolkit.sample.fragment;

import android.nfc.Tag;
import android.os.Bundle;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;

import java.security.cert.Certificate;

import ae.emiratesid.idcard.toolkit.internal.ErrorCode;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.task.ReadCertificateAsync;
import ae.emiratesid.idcard.toolkit.sample.task.ReaderCardDataAsync;
import ae.emiratesid.idcard.toolkit.sample.widget.LogTextView;

import static ae.emiratesid.idcard.toolkit.sample.AppController.isReading;

public class CertificateFragment extends BaseFragment {


    public CertificateFragment() {
        // Required empty public constructor
    }

    private static boolean isNFCMode;
    private Tag tag;
    private String PIN;
    private ReadCertificateAsync readCertificateAsync;
    private LogTextView txtStatus;
    private Button btnRefresh;
    private EditText edtPin;


    //Using a formatted Custom  TextView  to show the result and logs;

    /**
     * Use this factory method to create a new instance of
     * this fragment using the provided parameters.
     * <p>
     * Use the {@link CertificateFragment#newInstance} factory method to
     *
     * @return A new instance of fragment CertificateFragment.
     */
    public static CertificateFragment newInstance(boolean isNFC) {
        CertificateFragment fragment = new CertificateFragment();
        isNFCMode = isNFC;
        return fragment;
    }

    public void setNfcMode(Tag tag) {
        this.tag = tag;
        isNFCMode = true;
        Logger.e("setNfcMode :: called");

        if (!isReading) {
            Logger.d("onResume :: enableForegroundDispatch called");
            txtStatus.setText("");
            //set the reading flag..
            isReading = true;

            //show the dialog to provide user interaction...

            //create the object of ReaderCardDataAsync
            if (tag == null) {
                Logger.e("setNfcMode :: tag is null");
                return;
            } else {
                PIN = edtPin.getText().toString();
                //validate for empty
                if (PIN.isEmpty()) {
                    txtStatus.setLog("PIN is Required", LogTextView.LOG_TYPE.ERROR);
                    return;
                }

                showProgressDialog("Reading certificates...");
                Logger.e("setNfcMode :: calling read public data");
                readCertificateAsync = new ReadCertificateAsync(PIN,
                        readCertificateListener, tag);
            }
            readCertificateAsync.execute();
        }//
    }


    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_certificate, container, false);
        txtStatus = (LogTextView) view.findViewById(R.id.txtReadData);
        // make the text view scrollable.
        txtStatus.setMovementMethod(new ScrollingMovementMethod());
        btnRefresh = (Button) view.findViewById(R.id.btn_refresh);
        edtPin = (EditText) view.findViewById(R.id.edtPin);
        btnRefresh.setOnClickListener(this);
        return view;
    }

    // create a listener
    private ReadCertificateAsync.ReadCertificateListener readCertificateListener = new ReadCertificateAsync.ReadCertificateListener() {
        @Override
        public void onCertificateReadComplete(int status, String message, Certificate certificateAuth, Certificate certificateSign) {
            hideProgressDialog();
            isReading = false;
            if (status == Constants.SUCCESS) {
                Logger.d("Auth certificate" + "\t"
                        + certificateAuth);
                if (certificateAuth != null) {
                    txtStatus.appendLog("*********************AUTH CERTIFICATE*********\n",
                            LogTextView.LOG_TYPE.SUCCESS);
                    txtStatus.appendLog(certificateAuth.toString().trim(),
                            LogTextView.LOG_TYPE.SUCCESS);
                }//
                Logger.d("Signed certificate" + "\t"
                        + certificateSign);
                if (certificateSign != null) {
                    txtStatus.appendLog("\n*********************SIGN CERTIFICATE*********\n",
                            LogTextView.LOG_TYPE.INFO);
                    txtStatus.appendLog(certificateSign.toString().trim(), LogTextView.LOG_TYPE.INFO);
                }//if()

            }//if()
            else {
                if (status != ErrorCode.UNSPECIFIED.getCode()) {
                    txtStatus.setLog("Error Code : " + status + "\nError in Reading Certificate. \n" + message,
                            LogTextView.LOG_TYPE.ERROR);
                } else {
                    txtStatus.setLog("Error in Reading Certificate. \n" + message,
                            LogTextView.LOG_TYPE.ERROR);
                }
            }//
        }//onCertificateReadComplete()
    };

    @Override
    public void onClick(View view) {
        if (!isReading) {
            txtStatus.setText("");
            //set the reading flag..
            PIN = edtPin.getText().toString();
            //validate for empty
            if (PIN.isEmpty()) {
                txtStatus.setLog("PIN is Required", LogTextView.LOG_TYPE.ERROR);
                return;
            }//if()

            isReading = true;

            //show the dialog to provide user interaction...
            showProgressDialog("Reading certificates...");

            //create the object of ReaderCardDataAsync
            readCertificateAsync = new ReadCertificateAsync(PIN,
                    readCertificateListener);
            readCertificateAsync.execute();
            return;
        }//
        txtStatus.setLog("Reading already in process", LogTextView.LOG_TYPE.ERROR);

    }//onClick()

    @Override
    public void onDetach() {
        super.onDetach();
        readCertificateListener = null;
    }//onDetach()

    @Override
    protected void appendError(String message) {
        txtStatus.appendLog(message, LogTextView.LOG_TYPE.ERROR);
    }
}
