package ae.emiratesid.idcard.toolkit.sample;

import android.Manifest;
import android.annotation.TargetApi;
import android.app.Activity;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.res.AssetManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.provider.Settings;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.RequiresApi;
import androidx.appcompat.app.AppCompatActivity;

import com.google.android.material.snackbar.Snackbar;
import com.innovatrics.dot.core.DotLibrary;
import com.innovatrics.dot.core.license.Dot;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.ArrayList;

import ae.emiratesid.idcard.toolkit.sample.logger.Logger;

public class WelcomeActivity extends AppCompatActivity implements View.OnClickListener {

    private static final int CALLBACK_NUMBER = 100;
    private Button btnReadData;
    private ImageView btnSettings;

    @Override
    protected void onCreate(Bundle savedInstanceState) {

        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_sample);
        btnReadData = (Button) findViewById(R.id.button);
        btnReadData.setOnClickListener(this);
        btnSettings = (ImageView) findViewById(R.id.buttonSettings);
        btnSettings.setOnClickListener(this);
        org.apache.xml.security.Init.init(this);
        //check the API Version for runtime permission
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M
                && Build.VERSION.SDK_INT < Build.VERSION_CODES.S) {
            //call check permission
            checkPermissions(
                    Manifest.permission.CAMERA,
                    Manifest.permission.WRITE_EXTERNAL_STORAGE,
                    Manifest.permission.READ_PHONE_STATE);
        } else if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            checkPermissions(
                    Manifest.permission.CAMERA,
                    Manifest.permission.READ_MEDIA_IMAGES,
                    Manifest.permission.READ_PHONE_STATE);
            //call check permission
            if (!Environment.isExternalStorageManager()) {
                Snackbar.make(findViewById(android.R.id.content), "Permission needed!", Snackbar.LENGTH_INDEFINITE)
                        .setAction("Settings", new MyClickHandler(this)).show();
            }
        } else {
            //call the code as it is below Android M
            init();
            //Configure(true);
        }//else

        String path = getApplicationInfo().nativeLibraryDir;
        Logger.d(path);
        File file = new File(path);
        if (file.exists()) {
            String[] list = file.list();
            for (String name : list) {
                Logger.d(name);
            }//for
        } else {
            Logger.e("Libs is empty");
        }
    }//onCreate()

    @Override
    public void onClick(View view) {

        if (view == btnSettings) {
            Intent intent = new Intent(this, SettingActivity.class);
            startActivity(intent);
        }//
        if (view == btnReadData) {
            Intent intent = new Intent(this, HomeActivity.class);
            startActivity(intent);
        }//
    }//

    @TargetApi(Build.VERSION_CODES.M)
    private void checkPermissions(String... permissions) {

        ArrayList<String> toBeRequested = new ArrayList<String>();
        for (int i = 0; i < permissions.length; i++) {
            if (checkSelfPermission(permissions[i]) != PackageManager.PERMISSION_GRANTED) {
                toBeRequested.add(permissions[i]);
            }
        }

        if (!toBeRequested.isEmpty()) {
            requestPermissions(toBeRequested.toArray(new String[0]), CALLBACK_NUMBER);
        } else {
            init();
            //Configure(true);
        }//else
    }//

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {

        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        switch (requestCode) {
            case CALLBACK_NUMBER: {
                // If request is cancelled, the result arrays are empty.
                if (grantResults.length > 0
                        && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                    init();
                    //Configure(true);
                    // permission was granted, do your work....
                } else {
                    Toast.makeText(this, "Can't proceed  without Permission", Toast.LENGTH_LONG).show();
                    // permission denied
                    // Disable the functionality that depends on this permission.
                }//else
                return;
            }//case
            // other 'case' statements for other permssions
        }//switch()
    }//onRequestPermissionsResult()

    private void init() {
        File outDir = new File(AppController.path);

        if (!outDir.exists()) {
            Logger.d("Directory not found _________ " + outDir.getAbsolutePath());
            outDir.mkdirs();
        }//
    }//init()...

    private void Configure(boolean isOverWrite) {

        AssetManager assetManager = getAssets();
        String[] files = null;
        try {
            files = assetManager.list("");
        } catch (IOException e) {
            Logger.e("Failed to get asset file list" + e.getLocalizedMessage());
        }
        File outDir = new File(AppController.path);

        if (!outDir.exists()) {
            Logger.d("Directory not found _________ " + outDir.getAbsolutePath());
            outDir.mkdirs();
        }//
        if (files != null) for (String filename : files) {

            InputStream in = null;
            OutputStream out = null;
            try {
                in = assetManager.open(filename);
                File outFile = new File(outDir.getAbsolutePath() + "/" + filename);
//                Logger.d("Outputfile path ___________"+outFile.getAbsolutePath());
                if (!isOverWrite) {
                    if (outFile.exists()) {
                        Logger.d("Outputfile exists ___________" + outFile.getAbsolutePath());
                        continue;
                    }//if()
                }//if()
                out = new FileOutputStream(outFile);
                copyFile(in, out);
            } catch (IOException e) {
//                Logger.e( "Failed to copy asset file: " + filename +  e.getLocalizedMessage());
            } finally {
                if (in != null) {
                    try {
                        in.close();
                        out.close();
                    } catch (IOException e) {
                        // NOOP
                    }//catch()
                }//if()
                if (out != null) {
                    try {
                        out.close();
                    } catch (IOException e) {
                        // NOOP
                    }//catch()
                }//if()
            }//finally
        }//if()
    }//copyAsset()

    private void copyFile(InputStream in, OutputStream out) throws IOException {

        byte[] buffer = new byte[1024];
        int read;
        while ((read = in.read(buffer)) != -1) {
            out.write(buffer, 0, read);
        }//while
    }//copyFile

    private class MyClickHandler implements View.OnClickListener {
        Activity activity;

        public MyClickHandler(WelcomeActivity welcomeActivity) {
            this.activity = welcomeActivity;
        }

        @RequiresApi(api = Build.VERSION_CODES.R)
        @Override
        public void onClick(View v) {
            try {
                Uri uri = Uri.parse("package:" + getApplicationContext().getApplicationInfo().packageName);
                Intent intent = new Intent(Settings.ACTION_MANAGE_APP_ALL_FILES_ACCESS_PERMISSION, uri);
                activity.startActivity(intent);
            } catch (Exception ex) {
                Intent intent = new Intent();
                intent.setAction(Settings.ACTION_MANAGE_APP_ALL_FILES_ACCESS_PERMISSION);
                activity.startActivity(intent);
            }
        }
    }
}//End of class
