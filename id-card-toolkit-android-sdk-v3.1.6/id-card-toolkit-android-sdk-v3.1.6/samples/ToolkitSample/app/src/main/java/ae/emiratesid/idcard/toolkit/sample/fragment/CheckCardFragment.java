package ae.emiratesid.idcard.toolkit.sample.fragment;

import android.nfc.Tag;
import android.os.AsyncTask;
import android.os.Bundle;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.Toast;

import ae.emiratesid.idcard.toolkit.internal.ErrorCode;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.task.CardReaderConnectionTask;
import ae.emiratesid.idcard.toolkit.sample.task.CheckCardStatusAsync;
import ae.emiratesid.idcard.toolkit.sample.task.ReaderCardDataAsync;
import ae.emiratesid.idcard.toolkit.sample.widget.LogTextView;

import static ae.emiratesid.idcard.toolkit.sample.AppController.isReading;


public class CheckCardFragment extends BaseFragment implements View.OnClickListener {
    private static boolean isNFCMode;
    private Tag tag;

    public CheckCardFragment() {
        // Required empty public constructor
    }


    /**
     * Use this factory method to create a new instance of
     * this fragment using the provided parameters.
     * <p>
     * Use the {@link CheckCardFragment#newInstance} factory method to
     *
     * @return A new instance of fragment PublicDataReadingFragment.
     */
    public static CheckCardFragment newInstance(boolean isNFC) {
        CheckCardFragment fragment = new CheckCardFragment();
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
                showProgressDialog("Reading");
                Logger.e("setNfcMode :: calling check card status");
                checkCardStatusAsync = new CheckCardStatusAsync(checkCardStatusListener, tag);
            }
            checkCardStatusAsync.execute();
        }//
    }

    private LogTextView txtStatus;
    private Button btnCheckCardStatus;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_check_card, container, false);
        txtStatus = (LogTextView) view.findViewById(R.id.txtReadData);
        // make the text view scrollable.
        txtStatus.setMovementMethod(new ScrollingMovementMethod());
        btnCheckCardStatus = (Button) view.findViewById(R.id.btn_refresh);
        btnCheckCardStatus.setOnClickListener(this);
        return view;
    }


    private CardReaderConnectionTask.ConnectToolkitListener connectToolkitListener = new
            CardReaderConnectionTask.ConnectToolkitListener() {
                @Override
                public void onToolkitConnected(int status, boolean isConnectFlag, String message) {
                    if (!isConnectFlag) {
                        Toast.makeText(getActivity(), "Disconnected", Toast.LENGTH_SHORT).show();
                    }//
                }//onToolkitConnected()
            };//ConnectToolkitListener


    //Create  a listener object for card status.
    private CheckCardStatusAsync.CheckCardStatusListener checkCardStatusListener =
            new CheckCardStatusAsync.CheckCardStatusListener() {
                @Override
                public void onCheckCardStatus(int status, String message, String xmlString) {
                    hideProgressDialog();
                    isReading = false;
                    Logger.d("CheckCardStatusAsync::onCheckCardStatus() Status = " + status);
                    if (status == Constants.SUCCESS) {
                        txtStatus.setLog(message, LogTextView.LOG_TYPE.SUCCESS);
                        txtStatus.appendLog("\n" + xmlString,
                                LogTextView.LOG_TYPE.INFO);
                    }//if()
                    else {

                        if (status != ErrorCode.UNSPECIFIED.getCode()) {
                            txtStatus.setLog("Error Code : " + status + "\n" + message,
                                    LogTextView.LOG_TYPE.ERROR);
                        } else {
                            txtStatus.setLog(message,
                                    LogTextView.LOG_TYPE.ERROR);
                        }
                    }//else

                    if (isNFCMode) {
                        CardReaderConnectionTask cardReaderConnectionTask =
                                new CardReaderConnectionTask(connectToolkitListener, false);
                        cardReaderConnectionTask.execute();
                    }

                }//onCheckCardStatus
            };//checkCardStatusListener


    //Create a AsyncTask
    private CheckCardStatusAsync checkCardStatusAsync;

    @Override
    public void onClick(View view) {
        if (view == btnCheckCardStatus) {
            if (!isReading) {
                txtStatus.setText("");
                //set the reading flag..
                isReading = true;

                //show the dialog to provide user interaction...
                showProgressDialog("Checking card status with server...");

                //create the object of ReaderCardDataAsync

                if (tag == null) {
                    checkCardStatusAsync = new CheckCardStatusAsync(checkCardStatusListener);
                } else {
                    checkCardStatusAsync = new CheckCardStatusAsync(checkCardStatusListener, tag);
                }
                checkCardStatusAsync.execute();

            }//
            else {
                txtStatus.setLog("Reading in process", LogTextView.LOG_TYPE.ERROR);

            }//else
        }//
    }//

    @Override
    public void onDetach() {
        super.onDetach();

        //set isReading false.
        isReading = false;

        //stop the AsyncTask is running.
        if (checkCardStatusAsync != null &&
                checkCardStatusAsync.getStatus() == AsyncTask.Status.RUNNING) {
            // cancel the async task
            try {
                checkCardStatusAsync.cancel(true);
            }//
            catch (Exception e) {
            }//catch()
        }//
        checkCardStatusAsync = null;
        checkCardStatusListener = null;
    }//onDetach()

    @Override
    protected void appendError(String message) {
        txtStatus.appendLog(message, LogTextView.LOG_TYPE.ERROR);
    }
}//**end of class**
