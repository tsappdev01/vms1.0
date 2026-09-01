package ae.emiratesid.idcard.toolkit.sample.task;

import android.os.AsyncTask;

import java.lang.ref.WeakReference;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.FingerData;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.CryptoUtils;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;

public class UnblockedPinAsync extends AsyncTask<Void, Integer, Integer> {
    private CardReader cardReader;
    private int status;
    private WeakReference<UnblockPinListener> weakReference;
    final String PIN;
    final FingerData fingerData;
    private int timeoutInSeconds;
    private String message;

    public UnblockedPinAsync(UnblockPinListener listener, String pin, FingerData fingerData,
                             int timeoutInSeconds) {
        this.weakReference = new WeakReference<>(listener);
        this.PIN = pin;
        this.fingerData = fingerData;
        this.timeoutInSeconds = timeoutInSeconds;
    }//CheckCardStatusAsync

    @Override
    protected Integer doInBackground(Void... params) {

        try {
            cardReader = ConnectionController.getConnection();
            if (cardReader == null) {
                return Constants.ERROR;
            }//if()

            String requestId = RequestGenerator.generateRequestID();
            Logger.d("requestId :: " + requestId);

            String requestHandle = cardReader.prepareRequest(requestId);
            Logger.d("requestHandle :: " + requestHandle);

            CryptoUtils cryptoUtils = new CryptoUtils();
            String encodedPin = cryptoUtils.encryptParams(PIN, requestHandle);
            Logger.d("encodedpin " + encodedPin.length());

            if (encodedPin.length() > 344) {
                Logger.e("Pin Encoding is invalid");
                message = "Pin Encoding is invalid";
                status = Constants.ERROR;
                return status;
            }

            // call checkCardStatus
            cardReader.unblockPin(encodedPin, fingerData, timeoutInSeconds);

            return Constants.SUCCESS;
        } catch (ToolkitException e) {
            status = (int) e.getCode();
            message = e.getMessage();
        }// catch()
        catch (Exception e) {
            message = "Unknown Error";
        }// catch()
        return status;
    }//doInBackground()

    @Override
    protected void onPostExecute(Integer status) {
        super.onPostExecute(status);
        weakReference.get().onPinUnblocked(this.status, message);
    }//onPostExecute()

    public interface UnblockPinListener {
        public void onPinUnblocked(int status, String message);
    }//UnblockPinListener
}//End of class
