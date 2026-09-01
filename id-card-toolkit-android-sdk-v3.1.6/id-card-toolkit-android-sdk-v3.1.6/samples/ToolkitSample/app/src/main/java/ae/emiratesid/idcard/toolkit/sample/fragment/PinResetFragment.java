package ae.emiratesid.idcard.toolkit.sample.fragment;

import android.nfc.Tag;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.Spinner;

import ae.emiratesid.idcard.toolkit.datamodel.FingerData;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.task.GetFingerIndexAsync;
import ae.emiratesid.idcard.toolkit.sample.task.PKIAuthAsync;
import ae.emiratesid.idcard.toolkit.sample.task.PinResetAsync;
import ae.emiratesid.idcard.toolkit.sample.task.UnblockedPinAsync;
import ae.emiratesid.idcard.toolkit.sample.widget.LogTextView;

import static ae.emiratesid.idcard.toolkit.sample.AppController.isReading;

public class PinResetFragment extends BaseFragment {


    private int fingerIndex;
    private FingerData[] fingerList;
    private FingerData fingerData;
    private UnblockedPinAsync unblockedPinAsync;
    private int fingerReference;


    public PinResetFragment() {
        // Required empty public constructor
    }


    private LogTextView txtStatus;
    private Button btnSet, btnListFinger, btnUnblockPin;
    private EditText edtPin, edtConfirm;
    private Spinner spinner;
    private LinearLayout linearLayoutPIn;

    private static boolean isNFCMode;
    private Tag tag;


    /**
     * Use this factory method to create a new instance of
     * this fragment using the provided parameters.
     * <p>
     * Use the {@link PinResetFragment#newInstance} factory method to
     *
     * @return A new instance of fragment PinResetFragment.
     */
    public static PinResetFragment newInstance(boolean isNFC) {
        PinResetFragment fragment = new PinResetFragment();
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
                Logger.e("setNfcMode :: calling finger template details");
                showProgressDialog("Fetching finger template details");
                getFingerIndexAsync = new GetFingerIndexAsync(getFingerIndexListener,
                        tag);
            }
            getFingerIndexAsync.execute();

        }//
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_pin_reset, container, false);
        txtStatus = (LogTextView) view.findViewById(R.id.txtReadData);
        spinner = (Spinner) view.findViewById(R.id.spinner);
        edtPin = (EditText) view.findViewById(R.id.edtPin);
        edtConfirm = (EditText) view.findViewById(R.id.edtConfirmPin);
        linearLayoutPIn = (LinearLayout) view.findViewById(R.id.linearLayout_pin);
        linearLayoutPIn.setVisibility(View.INVISIBLE);
        btnSet = (Button) view.findViewById(R.id.btn_refresh);
        btnListFinger = (Button) view.findViewById(R.id.btn_list_finger);
        btnListFinger.setOnClickListener(this);
        btnSet.setOnClickListener(this);
        btnUnblockPin = (Button) view.findViewById(R.id.btn_unblock_pin);
        btnUnblockPin.setOnClickListener(this);
//        MorphoPlugin.setContext((Activity) getContext());
//        MorphoPlugin.setContext(getContext());
        spinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                fingerData = fingerList[i];
                linearLayoutPIn.setVisibility(View.VISIBLE);
            }

            @Override
            public void onNothingSelected(AdapterView<?> adapterView) {

            }
        });
        return view;
    }

    //create  VerifyPinAsync task
    private PinResetAsync pinResetAsync;

    //create VerifyPinAsync.
    private PinResetAsync.PinResetListener pinResetListener = new PinResetAsync.PinResetListener() {
        @Override
        public void onPinReset(int status, String message) {
            hideProgressDialog();
            isReading = false;
            if (status == Constants.SUCCESS) {
                txtStatus.setText("Successful.");
            }//
            else {
                txtStatus.setText("Failed\n");
                if (null != message && !message.isEmpty()) {
                    txtStatus.appendLog(message, LogTextView.LOG_TYPE.INFO);
                }//
            }//else

        }//onPinReset()
    };//pinResetListener()

    private GetFingerIndexAsync getFingerIndexAsync;

    //
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
            }//else
        }//onFingerIndexListener
    };

    //
    //create UnblockedPinAsync
    private UnblockedPinAsync.UnblockPinListener unblockPinListener = new UnblockedPinAsync.UnblockPinListener() {
        @Override
        public void onPinUnblocked(int status, String message) {
            hideProgressDialog();
            switch (status) {
                case Constants.SUCCESS:
                    txtStatus.setLog("Unblock pin successful", LogTextView.LOG_TYPE.SUCCESS);
                    break;
                default:
                    //invalid pin case and shoe the attempts.
                    txtStatus.appendLog(message + "", LogTextView.LOG_TYPE.ERROR);
                    break;
            }//switch()
        }//onPinUnblocked
    };//unblockPinListener

    @Override
    public void onClick(View view) {
        txtStatus.setText("");
        if (view == btnSet) {

            if (!isReading) {

                String PIN = edtPin.getText().toString();
                String PIN_CONFIRM = edtConfirm.getText().toString();

                //match the both pin
                if (!PIN.isEmpty() && !PIN_CONFIRM.isEmpty() && PIN.equals(PIN_CONFIRM)) {

                    //Display the progress dialog...
                    showProgressDialog("Resetting pin....");
                    pinResetAsync = new PinResetAsync(pinResetListener, PIN, fingerData, 20);
                    pinResetAsync.execute();
                }//if()..
                else {
                    //Display the Error is PIN Don't match
                    txtStatus.setText("Pin don't match.");
                }//else
            }//if()
        }//if()

        if (view == btnListFinger) {

            if (!isReading) {
                showProgressDialog("Fetching finger template details");
                if (tag == null) {
                    getFingerIndexAsync = new GetFingerIndexAsync(getFingerIndexListener);
                } else {
                    getFingerIndexAsync = new GetFingerIndexAsync(getFingerIndexListener,
                            tag);
                }
                getFingerIndexAsync.execute();
            }//if()
        }//if()
        if (view == btnUnblockPin) {

            String PIN = edtPin.getText().toString();
            String PIN_CONFIRM = edtConfirm.getText().toString();

            //match the both pin
            if (!PIN.isEmpty() && !PIN_CONFIRM.isEmpty() && PIN.equals(PIN_CONFIRM)) {

                //Display the progress dialog...
                showProgressDialog("Unblocking pin....");

                unblockedPinAsync = new UnblockedPinAsync(unblockPinListener, PIN, fingerData, 20);

                unblockedPinAsync.execute();
            }//if()..
            else {
                //Display the Error is PIN Don't match
                txtStatus.appendLog("Pin don't match.", LogTextView.LOG_TYPE.ERROR);
            }//else
        }//if()
    }//onClick()...

    @Override
    public void onDetach() {
        super.onDetach();
        getFingerIndexListener = null;
        pinResetListener = null;
    }//onDetach()

    @Override
    protected void appendError(String message) {
        txtStatus.appendLog(message, LogTextView.LOG_TYPE.ERROR);
    }
}//end of class
