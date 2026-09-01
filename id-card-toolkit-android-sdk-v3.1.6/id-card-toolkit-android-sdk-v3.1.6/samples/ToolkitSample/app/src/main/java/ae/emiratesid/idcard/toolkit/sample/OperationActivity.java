package ae.emiratesid.idcard.toolkit.sample;

import android.app.Activity;
import android.app.PendingIntent;
import android.content.Intent;
import android.content.IntentFilter;
import android.nfc.NfcAdapter;
import android.nfc.Tag;
import android.nfc.tech.NfcF;
import android.os.Bundle;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;

import ae.emiratesid.idcard.toolkit.sample.fragment.AuthenticateFaceIdnFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.AuthenticateFacePassportFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.CardSerialNumberFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.CardValidationOffCardFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.CertificateFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.CheckCardFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.FamilyBookFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.HomeFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.PKIAuthFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.ParseMRZ;
import ae.emiratesid.idcard.toolkit.sample.fragment.PinResetFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.PublicDataReadingFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.SetNFCParamsFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.SignDataFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.VerifyCardAndBiometric;
import ae.emiratesid.idcard.toolkit.sample.fragment.VerifyFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.register.view.RegisterDeviceFragment;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;

public class OperationActivity extends AppCompatActivity {
    private static final int REQUEST_CODE_AUTHENTICATE_FACE = 110;
    private static final int REQUEST_CODE_AUTHENTICATE_FACE_IDN = 111;
    private static final int REQUEST_CODE_AUTHENTICATE_FACE_PASSPORT = 112;
    private NfcAdapter adapter;
    private PendingIntent mPendingIntent;
    private IntentFilter[] mIntentFilters;
    private String[][] techListsArray;
    private Fragment fragment = null;
    private boolean isNFCRequired;
    private int type;
    private Tag tag;

    @Override
    protected void onCreate(Bundle savedInstanceState) {

        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_operation);

        type = getIntent().getIntExtra("TYPE", 0);
        Logger.d("Value for type  : " + type);

        isNFCRequired = (type == 5 || type == 7 || type == 4 || type == 19 || type == 6 || type == 9 || type == 10 || type == 11 || type == 12);
        if (isNFCRequired) {
            adapter = NfcAdapter.getDefaultAdapter(this);
            if (null != adapter) {
                mPendingIntent = PendingIntent.getActivity(this, 0,
                        new Intent(this, getClass()).addFlags(Intent.FLAG_ACTIVITY_SINGLE_TOP),
                        PendingIntent.FLAG_MUTABLE);
                IntentFilter ndef = new IntentFilter(NfcAdapter.ACTION_TAG_DISCOVERED);
                IntentFilter tech = new IntentFilter(NfcAdapter.ACTION_TECH_DISCOVERED);
                mIntentFilters = new IntentFilter[]{ndef, tech};
                techListsArray = new String[][]{new String[]{NfcF.class.getName()}};
            }
        }
        loadFragment();
    }

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        Logger.d("Calling OnNewIntent");
        tag = intent.getParcelableExtra(NfcAdapter.EXTRA_TAG);
        Logger.d("Calling OnNewIntent tag  : " + tag);
        setTagToFragment();
    }

    @Override
    protected void onResume() {
        super.onResume();
        if (adapter != null && isNFCRequired) {
            Logger.d("onResume :: enableForegroundDispatch called");
            adapter.enableForegroundDispatch(this, mPendingIntent, mIntentFilters, techListsArray);
        }
    }

    private void loadFragment() {

        switch (type) {
            case 0:
                fragment = new HomeFragment();
                break;
            case 2:
                fragment = new RegisterDeviceFragment();
                break;
            case 4:
                fragment = new CardSerialNumberFragment();
                break;
            case 5:
                fragment = new PublicDataReadingFragment();
                break;
            case 6:
                fragment = new CheckCardFragment();
                break;
            case 7:
                fragment = new VerifyFragment();
                break;
            case 8:
                fragment = new VerifyCardAndBiometric();
                break;
            case 9:
                fragment = new PKIAuthFragment();
                break;
            case 10:
                fragment = new PinResetFragment();
                break;
            case 11:
                fragment = new CertificateFragment();
                break;
            case 12:
                fragment = new SignDataFragment();
                break;
            case 13:
                fragment = new FamilyBookFragment();
                break;
            case 15:
                fragment = new SetNFCParamsFragment();
                break;
            case 17:
                fragment = new ParseMRZ();
                break;
            case 20:
                fragment = new AuthenticateFaceIdnFragment(REQUEST_CODE_AUTHENTICATE_FACE_IDN);
                break;
            case 21:
                fragment = new AuthenticateFacePassportFragment(REQUEST_CODE_AUTHENTICATE_FACE_PASSPORT);
                break;
            case 22:
                fragment = new CardValidationOffCardFragment();
                break;
            default:
                Toast.makeText(this, "Not yet Implemented", Toast.LENGTH_SHORT).show();
                return;
        }
        getSupportFragmentManager().beginTransaction().replace(R.id.root, fragment).commit();
    }

    private void setTagToFragment() {
        switch (type) {
            case 4:
                Logger.d("Tag found setting new tag to read public data");
                ((CardSerialNumberFragment) fragment).setNfcMode(tag);
                break;

            case 5:
                Logger.d("Tag found setting new tag to read public data");
                ((PublicDataReadingFragment) fragment).setNfcMode(tag);
                break;
            case 7:
                Logger.d("Tag found setting new tag to read verify data");
                ((VerifyFragment) fragment).setNfcMode(tag);
                break;
            case 6:
                Logger.d("Tag found setting new tag to check card status");
                ((CheckCardFragment) fragment).setNfcMode(tag);
                break;
            case 9:
                Logger.d("Tag found setting new tag to PIN authentication");
                ((PKIAuthFragment) fragment).setNfcMode(tag);
                break;
            case 10:
                Logger.d("Tag found setting new tag to PIN reset");
                ((PinResetFragment) fragment).setNfcMode(tag);
                break;
            case 11:
                Logger.d("Tag found setting new tag to read certificates");
                ((CertificateFragment) fragment).setNfcMode(tag);
                break;
           case 12:
                Logger.d("Tag found setting new tag to sign data");
                ((SignDataFragment) fragment).setNfcMode(tag);
                break;
            default:
                Toast.makeText(this, "This functionality is not supported in NFC mode", Toast.LENGTH_SHORT).show();
                return;
        }
    }

    @Override
    protected void onPause() {
        super.onPause();
        if (adapter != null) {
            adapter.disableForegroundDispatch(this);
        }
    }
}