package ae.emiratesid.idcard.toolkit.sample.task;

import android.nfc.Tag;
import android.os.AsyncTask;

import java.lang.ref.WeakReference;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardPublicData;
import ae.emiratesid.idcard.toolkit.datamodel.FingerData;
import ae.emiratesid.idcard.toolkit.datamodel.ToolkitResponse;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.NFCCardParams;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;

public class VerifyBiometricAsync extends AsyncTask<Void, Integer, Integer> {
    private CardReader cardReader;
    private int status = -1;
    private WeakReference<VerifyFingerprintListener> weakReference;
    private String message;
    private final FingerData fingerData;
    private int timeoutInSeconds = 20;
    private String xmlResponse;

    public VerifyBiometricAsync(VerifyFingerprintListener listener, FingerData fingerData,
                                int timeoutInSeconds) {
        this.weakReference = new WeakReference<VerifyFingerprintListener>(listener);
        this.fingerData = fingerData;
        this.timeoutInSeconds = timeoutInSeconds;
    }//PinResetAsync

    @Override
    protected Integer doInBackground(Void... params) {
        //check for the parameters
        try {

            cardReader = ConnectionController.getConnection();

            if (cardReader == null) {
                status = Constants.ERROR;
                return status;
            }//if()

            String requestId = RequestGenerator.generateRequestID();

            // call authenticateBiometricOnServer
            ToolkitResponse response = cardReader.authenticateBiometricOnServer(requestId,
                    fingerData.getFingerIndex(),
                    timeoutInSeconds);
            Logger.d("After Status  verify biometric_ 1 _2 ___" + status);
            if (null == response) {
                message = "Null response obtained";
                status = Constants.ERROR;
                Logger.e("Status code null response::" + status);
                return status;
            }
            message = response.getStatus();
            Logger.e("Message Code ::" + message);
            xmlResponse = response.toXmlString();
            Logger.d("Status  verify biometric_ 1 ___" + status);
            status = Constants.SUCCESS;
            Logger.e("Status code Success::" + status);
            return status;
        } catch (ToolkitException e) {
            Logger.e("Status code ::" + e.getCode());
            status = (int) e.getCode();
            //Logger.e("Status code ::"+status);
            message = e.getMessage();
            Logger.e("Message exception ::" + message);
        }// catch()
        catch (Exception e) {
            message = "Unknown Error";
        }// catch()
        return status;
    }//doInBackground()

    @Override
    protected void onPostExecute(Integer result) {
        super.onPostExecute(status);
        weakReference.get().onBiometricVerify(this.status, message, xmlResponse);
    }

    public interface VerifyFingerprintListener {
        void onBiometricVerify(int status, String vgResponse, String xmlResponse);
    }//VerifyFingerprintOnCardListener()
}//End of class
