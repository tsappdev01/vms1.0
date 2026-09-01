package ae.emiratesid.idcard.toolkit.sample;

import android.app.Activity;
import android.app.PendingIntent;
import android.app.ProgressDialog;
import android.content.Intent;
import android.content.IntentFilter;
import android.nfc.NfcAdapter;
import android.nfc.Tag;
import android.nfc.tech.NfcF;
import android.os.Bundle;
import android.view.View;
import android.view.Window;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;

import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.sample.fragment.PublicDataReadingFragment;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.task.CardReaderConnectionTask;
import ae.emiratesid.idcard.toolkit.sample.task.InitializeToolkitTask;

public class ReadNFCActivity extends AppCompatActivity {

    private ProgressDialog pd;
    private NfcAdapter adapter;
    private PendingIntent mPendingIntent;
    private IntentFilter[] mIntentFilters;
    private String[][] techListsArray;
    private TextView mErrorMessageTextview;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_read_nfc);
        adapter = NfcAdapter.getDefaultAdapter(this);

        if (null == adapter) {
            Toast.makeText(this, "Your device don't support NFC", Toast.LENGTH_SHORT).show();
            finish();
            return;
        }//if()
        mPendingIntent = PendingIntent.getActivity(
                this, 0, new Intent(this, getClass()).addFlags(Intent.FLAG_ACTIVITY_SINGLE_TOP),
                PendingIntent.FLAG_MUTABLE);
        IntentFilter ndef = new IntentFilter(NfcAdapter.ACTION_TAG_DISCOVERED);
        IntentFilter tech = new IntentFilter(NfcAdapter.ACTION_TECH_DISCOVERED);
        mIntentFilters = new IntentFilter[]{ndef, tech};
        techListsArray = new String[][]{new String[]{NfcF.class.getName()}};
        mErrorMessageTextview = (TextView) findViewById(R.id.tv_error_message);
        initialize();
    }

    @Override
    protected void onNewIntent(Intent intent) {
        Logger.d("REadNFCActivityInvoking on New Intent");
        super.onNewIntent(intent);
        Tag tag = intent.getParcelableExtra(NfcAdapter.EXTRA_TAG);
        showProgressDialog("Connecting.");
        CardReaderConnectionTask cardReaderConnectionTask = new CardReaderConnectionTask(connectToolkitListener,
                true, tag);
        cardReaderConnectionTask.execute();
    }

    @Override
    protected void onResume() {
        super.onResume();
        if (adapter != null) {
            adapter.enableForegroundDispatch(this, mPendingIntent, mIntentFilters, techListsArray);
        }//if()
    }//onResume();

    private void initialize() {
        showProgressDialog("Initializing...");
        mErrorMessageTextview.setVisibility(View.INVISIBLE);
        InitializeToolkitTask initializeToolkitTask = new InitializeToolkitTask
                (mInitializationListener);
        initializeToolkitTask.execute();
    }

    protected void showProgressDialog(String Message) {
        pd = new ProgressDialog(this);
        pd.requestWindowFeature(Window.FEATURE_NO_TITLE);
        pd.setMessage(Message); //recommended to use String resource...
        pd.setCancelable(false);
        pd.show();
    }//

    /**
     * Methods launch Progress dialog to provide user interaction.
     */
    protected void hideProgressDialog() {

        //check is dialog is showing then dissmiss it..
        if (pd != null && (pd.isIndeterminate() || pd.isShowing())) {
            pd.dismiss();
            pd = null;
        }//
    }//

    void showMessage(String message) {
        mErrorMessageTextview.setText(message + "");
        mErrorMessageTextview.setVisibility(View.VISIBLE);
    }

    private InitializeToolkitTask.InitializationListener mInitializationListener =
            new InitializeToolkitTask.InitializationListener() {
                @Override
                public void onToolkitInitialized(boolean isSuccessful, String statusMessage) {
                    hideProgressDialog();
                    if (isSuccessful) {
                        showMessage(statusMessage + "\n" + "Please tap your NFC card to read public data.");
                    } else {
                        showMessage(statusMessage);
                    }
                }
            };

    private CardReaderConnectionTask.ConnectToolkitListener connectToolkitListener =
            new CardReaderConnectionTask.ConnectToolkitListener() {

                @Override
                public void onToolkitConnected(int status, boolean isConnectFlag, String message) {
                    hideProgressDialog();
                    if (isConnectFlag) {
                        if (status == Constants.SUCCESS) {
                            Toast.makeText(ReadNFCActivity.this, "Initialization Successful",
                                    Toast.LENGTH_SHORT).show();
                            try {
                                Logger.d("calling set BAC params");
                                //000012977, 01-09-1980, 20-08-2019
                                ConnectionController.setNFCParams("000013947", "850112", "220202");
                                Logger.d("BAC param set successfully");
                            } catch (ToolkitException e) {
                                e.printStackTrace();
                                return;
                            }//catch()

                            showPublicDataFragment();
                            return;
                        }//if()...
                        Toast.makeText(ReadNFCActivity.this, "Initialization Failed", Toast.LENGTH_SHORT).show();
                        return;
                    }//
                    if (status == Constants.SUCCESS) {
                        Toast.makeText(ReadNFCActivity.this, "Successfully Disconnected", Toast.LENGTH_SHORT).show();
                        return;
                    }//if()..

                    Toast.makeText(ReadNFCActivity.this, "Failed", Toast.LENGTH_SHORT).show();

                }//onToolkitConnected()
            };//connectToolkitListener()...

    private void showPublicDataFragment() {
        Fragment fragment = PublicDataReadingFragment.newInstance(true);
        getSupportFragmentManager().beginTransaction()
                .replace(R.id.root_layout, fragment)
                .commit();
    }//showPublicDataFragment()

    @Override
    protected void onPause() {
        super.onPause();
        if (adapter != null) {
            adapter.disableForegroundDispatch(this);
        }//if()
    }//onPause()

    @Override
    protected void onDestroy() {
        super.onDestroy();
        adapter = null;
    }
}//