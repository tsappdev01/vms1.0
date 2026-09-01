package ae.emiratesid.idcard.toolkit.sample;

import android.content.SharedPreferences;
import android.os.Bundle;
import android.preference.EditTextPreference;
import android.preference.PreferenceActivity;
import android.preference.PreferenceFragment;

import ae.emiratesid.idcard.toolkit.sample.logger.Logger;

public class SettingActivity extends PreferenceActivity {
    static EditTextPreference VG_URL;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        getFragmentManager().beginTransaction().replace(android.R.id.content, new MyPreferenceFragment()).commit();

    }//

    public static class MyPreferenceFragment extends PreferenceFragment {
        @Override
        public void onCreate(final Bundle savedInstanceState) {
            super.onCreate(savedInstanceState);
            addPreferencesFromResource(R.xml.setting_preference);
            SharedPreferences sharedPreferences = getPreferenceManager().getSharedPreferences();
            sharedPreferences.registerOnSharedPreferenceChangeListener(sharedPreferenceChangeListener);
        }//

        SharedPreferences.OnSharedPreferenceChangeListener sharedPreferenceChangeListener = new SharedPreferences.OnSharedPreferenceChangeListener() {
            @Override
            public void onSharedPreferenceChanged(SharedPreferences sharedPreferences, String key) {
                String value = sharedPreferences.getString(key, "Not found") + "";
                String url = value.trim();
                AppController.VG_URL = url;
                Logger.d("Shared preference changed::::" + AppController.VG_URL);
                //Toast.makeText(getActivity() , "VG URL is " + AppController.VG_URL , Toast.LENGTH_SHORT).show();
            }
        };
    }
}
