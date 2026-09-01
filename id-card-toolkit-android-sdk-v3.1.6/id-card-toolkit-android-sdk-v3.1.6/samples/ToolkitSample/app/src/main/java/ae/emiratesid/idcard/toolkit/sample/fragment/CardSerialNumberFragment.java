package ae.emiratesid.idcard.toolkit.sample.fragment;

import static android.content.Context.CLIPBOARD_SERVICE;
import static ae.emiratesid.idcard.toolkit.sample.AppController.isReading;

import android.app.AlertDialog;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.DialogInterface;
import android.nfc.Tag;
import android.os.Bundle;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.Nullable;

import ae.emiratesid.idcard.toolkit.internal.ErrorCode;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.task.CardReaderConnectionTask;
import ae.emiratesid.idcard.toolkit.sample.task.CardSerialNumberAsync;
import ae.emiratesid.idcard.toolkit.sample.widget.LogTextView;

public class CardSerialNumberFragment extends BaseFragment implements View.OnClickListener {

    private CardSerialNumberAsync cardSerialNumberAsync;
    // the fragment initialization parameters, e.g. ARG_ITEM_NUMBER
    private static boolean isNFCMode;
    private Tag tag;





    public CardSerialNumberFragment() {
        // Required empty public constructor
    }

    /**
     * Use this factory method to create a new instance of
     * this fragment using the provided parameters.
     *
     * @return A new instance of fragment PublicDataReadingFragment.
     */
    public static CardSerialNumberFragment newInstance(boolean isNFC) {
        CardSerialNumberFragment fragment = new CardSerialNumberFragment();
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
                Logger.e("setNfcMode :: calling read public data");
                cardSerialNumberAsync = new CardSerialNumberAsync(cardSerialNumberListener, tag);
            }
            cardSerialNumberAsync.execute();
        }//
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    //Using a formatted Custom  TextView  to show the result and logs;
    private LogTextView txtStatus;
    private Button btnResfersh;
    private ImageView imgPhtoto;

    @Override
    public View onCreateView( LayoutInflater inflater, ViewGroup container,
                              Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_card_serial_number, container, false);
        txtStatus = (LogTextView) view.findViewById(R.id.txtReadData);
        imgPhtoto = (ImageView) view.findViewById(R.id.imageView);
        // make the text view scrollable.
        txtStatus.setMovementMethod(new ScrollingMovementMethod());
        btnResfersh = (Button) view.findViewById(R.id.btn_refresh);
        btnResfersh.setVisibility(!(isNFCMode) ? View.VISIBLE : View.INVISIBLE);
        btnResfersh.setOnClickListener(this);
        return view;
    }

    @Override
    public void onViewCreated(View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

    }

    @Override
    public void onAttach(Context context) {
        super.onAttach(context);
    }

    @Override
    public void onDetach() {
        super.onDetach();
        connectToolkitListener = null;
        cardSerialNumberListener = null;
        publicDataString = null;
        isNFCMode = false;
    }//

    @Override
    protected void appendError(String message) {
        txtStatus.appendLog(message, LogTextView.LOG_TYPE.ERROR);
    }

    private StringBuilder publicDataString = new StringBuilder();

    private CardReaderConnectionTask.ConnectToolkitListener connectToolkitListener = new
            CardReaderConnectionTask.ConnectToolkitListener() {
                @Override
                public void onToolkitConnected(int status, boolean isConnectFlag, String message) {
                    if (!isConnectFlag) {
                        Toast.makeText(getActivity(), "Disconnected", Toast.LENGTH_SHORT).show();
                    }//
                }//onToolkitConnected()
            };//ConnectToolkitListener

    private CardSerialNumberAsync.CardSerialNumberListener cardSerialNumberListener = new CardSerialNumberAsync.CardSerialNumberListener() {
        @Override
        public void onGetCardSerialNumber( int status, String message, String cardSerialNumber ) {
            //dismiss the dialog...
            hideProgressDialog();

            Logger.d("Reading finished with status " + status);
            isReading = false;

            final CardSerialNumberAsync mCardSerialNumberAsync = null;
            if (status == Constants.SUCCESS ) {
                try{


                    AlertDialog dialog;
                    AlertDialog.Builder builder;

                    TextView showText = new TextView(getActivity());
                    showText.setPadding(8, 8, 8, 8);
                    showText.setTextSize(16f);
                    showText.setText(cardSerialNumber);
                    showText.setOnLongClickListener(new View.OnLongClickListener() {

                        @Override
                        public boolean onLongClick(View v) {
                            // Copy the Text to the clipboard
                            ClipboardManager manager =
                                    (ClipboardManager) getActivity().getSystemService(CLIPBOARD_SERVICE);
                            TextView showTextParam = (TextView) v;
                            manager.setText(showTextParam.getText());
                            // Show a message:
                            Toast.makeText(v.getContext(), "Text in clipboard",
                                    Toast.LENGTH_SHORT)
                                    .show();
                            return true;
                        }
                    });
                    builder = new AlertDialog.Builder(getActivity());
                    builder.setView(showText);
                    dialog = builder.create();
                    dialog.setTitle("Card Serial Number");
                    dialog.setIcon(R.drawable.ic_device_id);
                    dialog.setCancelable(true);

                    dialog.setButton("OK", new DialogInterface.OnClickListener() {
                        public void onClick(DialogInterface dialog, int which) {
                            if (mCardSerialNumberAsync != null && mCardSerialNumberAsync.isCancelled())
                                mCardSerialNumberAsync.cancel(true);

                        }
                    });

                    dialog.show();
                    return;

                } catch (Exception e) {
                    e.printStackTrace();
                }

            }else {
                if (status == ErrorCode.UNSPECIFIED.getCode()) {
                    txtStatus.setLog("Error in reading public data from card. \n" + message,
                            LogTextView.LOG_TYPE.ERROR);
                } else {
                    txtStatus.setLog("Error Code : " + status + "\nError in reading public data from card. \n" + message,
                            LogTextView.LOG_TYPE.ERROR);
                }

            }
            if (isNFCMode) {
                CardReaderConnectionTask cardReaderConnectionTask =
                        new CardReaderConnectionTask(connectToolkitListener, false);
                cardReaderConnectionTask.execute();
            }//if()
        }


    };//readerCardDataListener;

    @Override
    public void onClick(View view) {
        if (view == btnResfersh) {

          /*  showProgressDialog("Loading");
            try
            {
                if(new SignatureValidator(readXMLDataFromFile("vg_response"),readDataFromFile("vg_signing_cert"), readDataFromFile("vg_signing_cert_chain")).validate() == 0)
                {
                    Logger.d("Validate Success");
                }
                else
                {
                    Logger.d("Validate Failed");
                }
                hideProgressDialog();
            }
            catch (ToolkitException e)
            {
                Logger.d("Validate Failed"+ e.getMessage());
                hideProgressDialog();
            }
*/
            if (!isReading) {
                txtStatus.setText("");
                //set the reading flag..
                isReading = true;

                //show the dialog to provide user interaction...
                showProgressDialog("Reading");

                //create the object of ReaderCardDataAsync
                if (tag == null) {
                    cardSerialNumberAsync = new CardSerialNumberAsync(cardSerialNumberListener);
                } else {
                    cardSerialNumberAsync = new CardSerialNumberAsync(cardSerialNumberListener, tag);
                }
                cardSerialNumberAsync.execute();
            }//

        }//
    }//
}
