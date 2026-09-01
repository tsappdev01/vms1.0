package ae.emiratesid.idcard.toolkit.sample.task;

import android.nfc.Tag;
import android.os.AsyncTask;

import java.lang.ref.WeakReference;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardCertificates;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.CryptoUtils;
import ae.emiratesid.idcard.toolkit.sample.utils.NFCCardParams;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;

public class VerifySignatureDataAsync extends AsyncTask<Void  , Integer , Integer> {
    private final String PIN;
    private CardReader cardReader;
    private int status;
    private WeakReference<VerifyDataListener> weakReference;

    private byte[]  plainData;
    private byte[]  signatureData;
    private int certificateType;
    private boolean isDataHashed;
    private String message;

    private Tag tag;
    private String cardNumber, dob, expiryDate;
    public VerifySignatureDataAsync( byte[] plainData, byte[] signatureData  ,int certificateType,
                                     VerifyDataListener  listener , boolean isDataHashed , String pin) {
        this.plainData = plainData;
        this.certificateType = certificateType;
        this.signatureData = signatureData;
        this.weakReference = new WeakReference<VerifyDataListener>(listener);
        this.isDataHashed = isDataHashed;
        this.PIN = pin;

    }//VerifySignatureDataAsync()
    public VerifySignatureDataAsync( byte[] plainData, byte[] signatureData  ,int certificateType,
                                     VerifyDataListener  listener , boolean isDataHashed , String pin,Tag tag) {
        this.plainData = plainData;
        this.certificateType = certificateType;
        this.signatureData = signatureData;
        this.weakReference = new WeakReference<VerifyDataListener>(listener);
        this.isDataHashed = isDataHashed;
        this.PIN = pin;
        this.tag = tag;
        this.cardNumber = NFCCardParams.CARD_NUMBER;
        this.dob = NFCCardParams.DOB;
        this.expiryDate = NFCCardParams.EXPIRY_DATE;

    }//VerifySignatureDataAsync()

    @Override
    protected Integer doInBackground(Void ... params) {
        //check for the parameters
        try {
            if (tag != null) {
                cardReader = ConnectionController.initConnection(tag);
                ConnectionController.setNFCParams(cardNumber, dob, expiryDate);
            } else {
                cardReader = ConnectionController.getConnection();
            }
            if(cardReader == null){
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

            CardCertificates cardCertificates = cardReader.getPkiCertificates(encodedPin);
            if( null ==  cardCertificates){
                Logger.e("Certificate can't be fetched.");
                message = "Certificate can't be fetched.";
                status = Constants.ERROR;
                return status;
            }
            byte[] verifyCertificate;
            if(certificateType == Constants.AUTH_CERT) {
                Logger.d(" Constants.AUTH_CERT");
                //get the certificate
                    verifyCertificate = cardCertificates.getAuthenticationCertificate();
                if (null == verifyCertificate) {
                    Logger.e("certificateAuth  is null ");
                    return Constants.ERROR;
                }//
            }//if()
            else{
                Logger.d(" Constants.SIGN_CERT");
                verifyCertificate = cardCertificates.getSigningCertificate();
                if (null == verifyCertificate) {
                    Logger.e("certificate Sign  is null ");
                    return Constants.ERROR;
                }//if()
            }//else

            cardReader.verifySignature(plainData ,isDataHashed ,  signatureData,
                    verifyCertificate );
            message = "Signature not Verified.";
            status =  Constants.SUCCESS;

        } catch (ToolkitException e) {
            e.printStackTrace();
            if (e.getExceptionType() == ToolkitException.ExceptionType.CARD_PIN_ERROR) {
                message = e.getMessage() + "Attempts left " + e.getAttemptsLeft();
            } else {
                message = e.getMessage();
            }
            status = (int) e.getCode();
        } catch( Exception e ){
            message = "Unknown Error.";
        }// catch()
        return  status;
    }//doInBackground()


    @Override
    protected void onPostExecute(Integer status) {
        super.onPostExecute(status);
        weakReference.get().onDataVerified(this.status , message );
    }

    public interface VerifyDataListener{
        void onDataVerified(int status, String message);
    }
}
