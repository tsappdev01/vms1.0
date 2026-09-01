package ae.emiratesid.idcard.toolkit.sample.task;

import android.os.AsyncTask;

import java.lang.ref.WeakReference;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.FingerData;
import ae.emiratesid.idcard.toolkit.datamodel.ToolkitResponse;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;

public class VerifyFingerprintOnCardAsync extends AsyncTask<Void , Integer , Integer> {
    private CardReader cardReader;
    private int status = -1;
    private WeakReference<VerifyFingerprintOnCardListener> weakReference;
    private String vgResponse;
    private final FingerData fingerData;
    private int  timeoutInSeconds =  20;

    public VerifyFingerprintOnCardAsync(VerifyFingerprintOnCardListener listener, FingerData fingerData, int timeoutInSeconds)  {
        this.weakReference = new WeakReference<VerifyFingerprintOnCardListener>(listener);
        this.fingerData = fingerData;
        this.timeoutInSeconds  = timeoutInSeconds;
    }//PinResetAsync

    @Override
    protected Integer doInBackground(Void... params) {
        //check for the parameters
        try {
            cardReader = ConnectionController.getConnection();
            if(cardReader == null){
                return Constants.ERROR;
            }//if()

            String requestId = RequestGenerator.generateRequestID();
            // call authenticateBiometricOnServer
           ToolkitResponse toolkitResponse = cardReader.authenticateBiometricOnServer(requestId , fingerData.getFingerIndex() ,
                    timeoutInSeconds);
            if(null == toolkitResponse){
                vgResponse =  "Null response obtained";
                status =  Constants.ERROR;
                return status;
            }
            Logger.d("Status  verify biometric_ 1 ___" + status);
            return new Integer(status);
        } catch( ToolkitException e ){
            status = (int) e.getCode();
            vgResponse = e.getMessage();
        }// catch()
        catch( Exception e ){
            vgResponse = "Unknown Error";
        }// catch()
        return status;
    }//doInBackground()


    @Override
    protected void onPostExecute(Integer result) {
        super.onPostExecute(status);
        Logger.d("Status  verify biometric__ 2 _" + status);
//        Logger.d("Status  verify biometric__ 2.1 _" + result);
        weakReference.get().onBiometricVerify(this.status , vgResponse);
    }

    public interface VerifyFingerprintOnCardListener{
        public void onBiometricVerify(int status, String vgResponse);
    }//VerifyFingerprintOnCardListener()
}
