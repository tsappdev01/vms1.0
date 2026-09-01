package ae.emiratesid.idcard.toolkit.sample.toolkit_face;

import android.app.ProgressDialog;
import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import android.view.Window;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import ae.emiratesid.idcard.toolkit.Toolkit;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardPublicData;
import ae.emiratesid.idcard.toolkit.datamodel.ToolkitResponse;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;

public class ToolkitFaceActivity extends AppCompatActivity {
    private int requestCodeResult = 150;
    private ProgressDialog pd;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_dot_sdkinit);

        Intent intent = new Intent(getApplicationContext(), MainActivity.class);
        startActivityForResult(intent, requestCodeResult);
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == requestCodeResult) {
            if (resultCode == RESULT_OK) {
                try {
                    String imageBase64 = ToolkitFace.getCapture_face();
                    String liveness_data = ToolkitFace.get_liveness_data();
                    if (imageBase64 != null) {
                        showProgressDialog("Authenticating Face with Server.");
                        new Handler().postDelayed(() -> {
                            if (ToolkitFace.withId) {
                                authenticateFaceWithIdEx(ToolkitFace._idn, imageBase64, liveness_data,
                                        ToolkitFace.getLivelinessMode(), ToolkitFace._withDocs);
                            } else {
                                authenticateFaceWithPassportEx(ToolkitFace._passportNumber,
                                        ToolkitFace._passportCountry, ToolkitFace._passportExpiry,
                                        ToolkitFace._passportDob, imageBase64, liveness_data,
                                        ToolkitFace.getLivelinessMode(), ToolkitFace._withDocs);
                            }
                        }, 1500);
                    }
                } catch (Exception e) {
                    e.printStackTrace();
                }
            } else {
                finish();
            }
        }
    }

    protected void showProgressDialog(String message) {
        pd = new ProgressDialog(ToolkitFaceActivity.this);
        pd.requestWindowFeature(Window.FEATURE_NO_TITLE);
        pd.setMessage(message);
        pd.setCancelable(false);
        pd.show();
    }

    protected void hideProgressDialog() {
        if (pd != null && (pd.isIndeterminate() || pd.isShowing())) {
            pd.dismiss();
            pd = null;
        }
    }

    public void authenticateFaceWithIdEx(String idn, String imageBase64,
                                         String liveness_data, int liveness_mode,
                                         boolean withDocs) {
        int status = Constants.SUCCESS;
        String message = null;
        CardPublicData cardPublicData = null;

        try {
            Toolkit toolkit = ConnectionController.getToolkit();
            if (toolkit == null) {
                Logger.e("Toolkit is not initialized.");
                status = Constants.ERROR;
                message = "Toolkit is not initialized.";
            } else {

                ToolkitResponse toolkitResponse = toolkit.verifyFaceOnServerUsingIDEx(ToolkitFace.sdk_handle,
                        idn, imageBase64, liveness_data, liveness_mode, withDocs);

                if (toolkitResponse.getStatus().equals("BiometricAuthenticated")) {
                    cardPublicData = new CardPublicData(toolkitResponse.toXmlString(), toolkitResponse.toXmlString());
                } else {
                    message = "Response from Toolkit is " + toolkitResponse.getStatus();
                }

                if (cardPublicData == null) {
                    status = Constants.ERROR;
                }
            }
        } catch (ToolkitException e) {
            status = (int) e.getCode();
            message = e.getMessage();

            Logger.e("Exception occurred :::" + e.getMessage() + " Status = " + status);
        } catch (Exception e) {
            e.printStackTrace();
            message = "Unknown error";
        }

        if (ToolkitFace._authenticateListener != null) {
            hideProgressDialog();
            ToolkitFace._authenticateListener.onAuthenticateFaceResult(status, message, cardPublicData);
            onBackPressed();
        }
    }

    public void authenticateFaceWithPassportEx(String passportNumber, String passportCountry, String passportExpiry,
                                               String passportDob, String imageBase64, String liveness_data, int liveness_mode,
                                               boolean withDocs) {
        int status = Constants.SUCCESS;
        String message = null;
        CardPublicData cardPublicData = null;

        try {
            Toolkit toolkit = ConnectionController.getToolkit();
            if (toolkit == null) {
                Logger.e("Toolkit is not initialized.");
                status = Constants.ERROR;
                message = "Toolkit is not initialized.";
            } else {

                ToolkitResponse toolkitResponse = toolkit
                        .verifyFaceOnServerUsingPassportEx(ToolkitFace.sdk_handle,
                                passportNumber, passportCountry, passportExpiry,
                                passportDob, imageBase64, liveness_data, liveness_mode, withDocs);

                if (toolkitResponse.getStatus().equals("BiometricAuthenticated")) {
                    cardPublicData = new CardPublicData(toolkitResponse.toXmlString(), toolkitResponse.toXmlString());
                } else {
                    message = "Response from Toolkit is " + toolkitResponse.getStatus();
                }

                if (cardPublicData == null) {
                    status = Constants.ERROR;
                }
            }
        } catch (ToolkitException e) {
            status = (int) e.getCode();
            message = e.getMessage();

            Logger.e("Exception occurred :::" + e.getMessage() + " Status = " + status);
        } catch (Exception e) {
            e.printStackTrace();
            message = "Unknown error";
        }

        if (ToolkitFace._authenticateListener != null) {
            hideProgressDialog();
            ToolkitFace._authenticateListener.onAuthenticateFaceResult(status, message, cardPublicData);
            onBackPressed();
        }
    }
}
