package ae.emiratesid.idcard.toolkit.sample.task;

import android.nfc.Tag;
import android.os.AsyncTask;

import java.lang.ref.WeakReference;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.ToolkitResponse;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.CryptoUtils;
import ae.emiratesid.idcard.toolkit.sample.utils.NFCCardParams;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;

public class PKIAuthAsync extends AsyncTask<Void, Integer, Integer> {
    private CardReader cardReader;
    private int status;
    private WeakReference<PKIAuthListener> weakReference;
    private String xmlResponse;
    private String message;
    private final String PIN;

    private Tag tag;
    private String cardNumber, dob, expiryDate;

    public PKIAuthAsync(final String PIN, PKIAuthListener listener) {
        this.weakReference = new WeakReference<PKIAuthListener>(listener);
        this.PIN = PIN;
    }//PKIAuthAsync

    public PKIAuthAsync(final String PIN, PKIAuthListener listener, Tag tag) {

        this.weakReference = new WeakReference<PKIAuthListener>(listener);
        this.PIN = PIN;
        this.tag = tag;
        this.cardNumber = NFCCardParams.CARD_NUMBER;
        this.dob = NFCCardParams.DOB;
        this.expiryDate = NFCCardParams.EXPIRY_DATE;
    }

    @Override
    protected Integer doInBackground(Void... voids) {
        //check for the parameters
        try {
            if (tag != null) {
                cardReader = ConnectionController.initConnection(tag);
                ConnectionController.setNFCParams(cardNumber, dob, expiryDate);
            } else {
                cardReader = ConnectionController.getConnection();
            }

            if (cardReader == null) {
                return Constants.ERROR;
            }//

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

            ToolkitResponse response = cardReader.authenticatePki(encodedPin);

            if (response == null) {
                Logger.e("Response in check card status is null");
                message = "Response in check card status is null";
                status = Constants.ERROR;
                return status;
            }
            message = response.getStatus();
            xmlResponse = response.toXmlString();
            status = Constants.SUCCESS;
        } catch (ToolkitException e) {
            e.printStackTrace();
            if (e.getExceptionType() == ToolkitException.ExceptionType.CARD_PIN_ERROR) {
                message = e.getMessage() + "Attempts left " + e.getAttemptsLeft();
            } else {
                message = e.getMessage();
            }
            status = (int) e.getCode();
        }// catch()
        catch (Exception e) {
            message = "Unknown Error";
        }// catch()
        return status;
    }//doInBackground()

    @Override
    protected void onPostExecute(Integer status) {
        super.onPostExecute(status);
        weakReference.get().onPKIAuth(status, message, xmlResponse);
    }

    public interface PKIAuthListener {
        void onPKIAuth(int status, String message, String vgResponse);
    }
}
