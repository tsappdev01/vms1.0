package ae.emiratesid.idcard.toolkit.sample.task;

import android.nfc.Tag;
import android.os.AsyncTask;

import java.lang.ref.WeakReference;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.SignatureResponse;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.CryptoUtils;
import ae.emiratesid.idcard.toolkit.sample.utils.NFCCardParams;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;

public class SignDataAsync extends AsyncTask<Void, Integer, Integer> {
    private CardReader cardReader;
    private int status;
    private WeakReference<SignDataListener> weakReference;

    private byte[] plainData;
    final String USER_PIN;
    private int certificateType;
    private byte[] signedData;
    private String message;

    private Tag tag;
    private String cardNumber, dob, expiryDate;

    public SignDataAsync(String USER_PIN, byte[] plainData, int certificateType, SignDataListener listener) {
        this.USER_PIN = USER_PIN;
        this.plainData = plainData;
        this.certificateType = certificateType;
        this.weakReference = new WeakReference<SignDataListener>(listener);
    }

    public SignDataAsync(String USER_PIN, byte[] plainData, int certificateType, SignDataListener listener, Tag tag) {
        this.USER_PIN = USER_PIN;
        this.plainData = plainData;
        this.certificateType = certificateType;
        this.weakReference = new WeakReference<SignDataListener>(listener);
        this.tag = tag;
        this.cardNumber = NFCCardParams.CARD_NUMBER;
        this.dob = NFCCardParams.DOB;
        this.expiryDate = NFCCardParams.EXPIRY_DATE;
    }

    @Override
    protected Integer doInBackground(Void... params) {
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
            }

            String requestId = RequestGenerator.generateRequestID();
            Logger.d("requestId :: " + requestId);

            String requestHandle = cardReader.prepareRequest(requestId);
            Logger.d("requestHandle :: " + requestHandle);

            CryptoUtils cryptoUtils = new CryptoUtils();
            String encodedPin = cryptoUtils.encryptParams(USER_PIN, requestHandle);
            Logger.d("encodedpin " + encodedPin.length());

            if (encodedPin.length() > 344) {
                Logger.e("Pin Encoding is invalid");
                message = "Pin Encoding is invalid";
                status = Constants.ERROR;
                return status;
            }

            // call
            SignatureResponse response = null;
            if (certificateType == Constants.SIGN_CERT) {
                response = cardReader.signData(plainData, false, encodedPin);
            } else {
                response = cardReader.signChallenge(plainData, false, encodedPin);
            }
            signedData = response.getSignature();
        } catch (ToolkitException e) {
            e.printStackTrace();
            if (e.getExceptionType() == ToolkitException.ExceptionType.CARD_PIN_ERROR) {
                message = e.getMessage() + "Attempts left " + e.getAttemptsLeft();
            } else {
                message = e.getMessage();
            }
            status = (int) e.getCode();
        } catch (Exception e) {
            message = e.getLocalizedMessage();
            status = Constants.ERROR;
        }// catch()
        return status;
    }//doInBackground()


    @Override
    protected void onPostExecute(Integer status) {
        super.onPostExecute(status);
        weakReference.get().onDataSigned(this.status, message, signedData);
    }

    public interface SignDataListener {
        void onDataSigned(int status, String message, byte data[]);
    }
}
