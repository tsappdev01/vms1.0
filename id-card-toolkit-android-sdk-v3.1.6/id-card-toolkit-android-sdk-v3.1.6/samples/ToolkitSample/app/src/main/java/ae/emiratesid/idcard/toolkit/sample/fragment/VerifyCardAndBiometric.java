package ae.emiratesid.idcard.toolkit.sample.fragment;

import android.nfc.Tag;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.Spinner;

import ae.emiratesid.idcard.toolkit.datamodel.FingerData;
import ae.emiratesid.idcard.toolkit.internal.ErrorCode;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.task.GetFingerIndexAsync;
import ae.emiratesid.idcard.toolkit.sample.task.VerifyBiometricAsync;
import ae.emiratesid.idcard.toolkit.sample.task.VerifyCardAndBiometricAsync;
import ae.emiratesid.idcard.toolkit.sample.widget.LogTextView;

import static ae.emiratesid.idcard.toolkit.sample.AppController.isReading;

public class VerifyCardAndBiometric extends BaseFragment {


    private Tag tag;

    public VerifyCardAndBiometric() {
        // Required empty public constructor
    }

    private LogTextView txtStatus;
    private Button btnListFinger, btnVerify;
    private Spinner spinner;
    private int fingerIndex;
    private FingerData[] fingerList;
    private FingerData fingerData;
    private boolean isNFCMode;


    public void setNfcMode(Tag tag){
        this.tag = tag;
        isNFCMode = true;
    }

    @Override
    public View onCreateView( LayoutInflater inflater, ViewGroup container,
                              Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_verify_card_and_biometric, container, false);
        txtStatus = (LogTextView) view.findViewById(R.id.txtReadData);
        spinner = (Spinner) view.findViewById(R.id.spinner);
        btnListFinger = (Button) view.findViewById(R.id.btn_refresh);
        btnListFinger.setOnClickListener(this);
        btnVerify = (Button) view.findViewById(R.id.btn_verify);
        btnVerify.setOnClickListener(this);

        //set an setOnItemSelectedListener...
        spinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {

                fingerData = fingerList[i];
                btnVerify.setEnabled(true);
            }//onItemSelected()

            @Override
            public void onNothingSelected(AdapterView<?> adapterView) {

            }//onNothingSelected()
        });

        return view;
    }//onCreateView()

    //Create GetFingerIndexListener
    private GetFingerIndexAsync.GetFingerIndexListener getFingerIndexListener = new GetFingerIndexAsync.GetFingerIndexListener() {
        @Override
        public void onFingerIndexFetched(int status, String message, FingerData[] fingers) {

            hideProgressDialog();
            isReading = false;
            if (status == Constants.SUCCESS) {

                int noOfFinger = fingers.length;
                //copy the array
                fingerList = fingers;

                //set the values to the adapter
                String[] fingerData = {fingers[0].getFingerIndex() + "", fingers[1].getFingerIndex() + ""};

                //selected item will look like a spinner set from XML
                ArrayAdapter<String> spinnerArrayAdapter = new ArrayAdapter<String>
                        (getActivity(), android.R.layout.simple_spinner_item, fingerData);
                spinnerArrayAdapter.setDropDownViewResource
                        (android.R.layout.simple_spinner_dropdown_item);

                //set the adapter
                spinner.setAdapter(spinnerArrayAdapter);

            }//if()
            else {
                txtStatus.appendLog(message + "", LogTextView.LOG_TYPE.ERROR);
                if(status != ErrorCode.UNSPECIFIED.getCode()) {
                    txtStatus.setLog("Error Code : " + status+"\n"  + message,
                            LogTextView.LOG_TYPE.ERROR);
                }
                else
                {
                    txtStatus.setLog(message,
                            LogTextView.LOG_TYPE.ERROR);
                }
            }//else
        }//onFingerIndexListener
    };

    //Create VerifyBiometricListener
    VerifyCardAndBiometricAsync.VerifyFingerprintListener verifyBiometricListener = new VerifyCardAndBiometricAsync.VerifyFingerprintListener() {
        @Override
        public void onBiometricVerify(int status, String message, String vgResponse) {

            hideProgressDialog();
            isReading = false;
            Logger.d("Status  verify biometric___3_" + status);
            //txtStatus.appendLog("Status ::" + status, LogTextView.LOG_TYPE.INFO);
            if (status == Constants.SUCCESS) {
                txtStatus.appendLog("Biometric verified", LogTextView.LOG_TYPE.SUCCESS);
                txtStatus.appendLog("\n VG Response ::" + vgResponse, LogTextView.LOG_TYPE.INFO);
            }//
            else {
                txtStatus.appendLog(message + "", LogTextView.LOG_TYPE.ERROR);
            }//else....
        }//onBiometricVerify
    };//VerifyBiometricAsync

    //create VerifyBiometricAsync
    private VerifyCardAndBiometricAsync verifyBiometricAsync;
    private GetFingerIndexAsync getFingerIndexAsync;

    @Override
    public void onClick(View view) {

        txtStatus.setText("");
        if (isReading) {
            txtStatus.setLog("Reading is already in process", LogTextView.LOG_TYPE.ERROR);
            return;
        }//else
        if (view == btnVerify) {
            showProgressDialog("Verifying biometric on server...");
            isReading = true;
            if(tag == null) {
                verifyBiometricAsync = new VerifyCardAndBiometricAsync(verifyBiometricListener, fingerData, 20);
            }
            else{
                verifyBiometricAsync = new VerifyCardAndBiometricAsync(verifyBiometricListener, fingerData,
                        20,
                        tag);
            }
            verifyBiometricAsync.execute();
        }//if()

        if (view == btnListFinger) {
            Logger.d("called....for reading...");
            showProgressDialog("Fetching Finger Details.....");
            if(tag ==  null){
                getFingerIndexAsync = new GetFingerIndexAsync(getFingerIndexListener);
            }else{
                getFingerIndexAsync = new GetFingerIndexAsync(getFingerIndexListener,
                        tag);
            }
            getFingerIndexAsync.execute();
        }//if()
    }//onClick()...

    @Override
    public void onDetach() {

        super.onDetach();
        getFingerIndexListener = null;
        getFingerIndexAsync = null;
        verifyBiometricListener = null;
    }

    @Override
    protected void appendError(String message) {
        txtStatus.appendLog(message , LogTextView.LOG_TYPE.ERROR);
    }
}
