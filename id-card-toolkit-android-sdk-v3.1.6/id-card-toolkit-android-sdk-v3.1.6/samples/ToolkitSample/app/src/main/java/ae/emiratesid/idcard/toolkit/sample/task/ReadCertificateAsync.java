package ae.emiratesid.idcard.toolkit.sample.task;

import android.nfc.Tag;
import android.os.AsyncTask;

import java.io.ByteArrayInputStream;
import java.lang.ref.WeakReference;
import java.security.cert.Certificate;
import java.security.cert.CertificateException;
import java.security.cert.CertificateFactory;
import java.security.cert.X509Certificate;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardCertificates;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.CryptoUtils;
import ae.emiratesid.idcard.toolkit.sample.utils.NFCCardParams;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;

public class ReadCertificateAsync extends AsyncTask<Void, Integer, Integer> {
    private CardReader cardReader;
    private int status;
    private WeakReference<ReadCertificateListener> weakReference;
    private Certificate certificateAuth;
    private Certificate certificateSign;
    private String message;
    private final String PIN;

    private Tag tag;
    private String cardNumber, dob, expiryDate;

    public ReadCertificateAsync(final String PIN, ReadCertificateListener listener) {
        this.weakReference = new WeakReference<ReadCertificateListener>(listener);
        this.PIN = PIN;

    }

    public ReadCertificateAsync(final String PIN, ReadCertificateListener listener, Tag tag) {
        this.weakReference = new WeakReference<ReadCertificateListener>(listener);
        this.PIN = PIN;
        this.tag = tag;
        this.cardNumber = NFCCardParams.CARD_NUMBER;
        this.dob = NFCCardParams.DOB;
        this.expiryDate = NFCCardParams.EXPIRY_DATE;

    }

    @Override
    protected Integer doInBackground(Void... voids) {
        try {
            if (tag != null) {
                cardReader = ConnectionController.initConnection(tag);
                ConnectionController.setNFCParams(cardNumber, dob, expiryDate);
            } else {
                cardReader = ConnectionController.getConnection();
            }
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

            CardCertificates cardCertificates = cardReader.getPkiCertificates(encodedPin);

            if (cardCertificates == null) {
                Logger.e("Response in check card status is null");
                message = "Response in check card status is null";
                status = Constants.ERROR;
                return status;
            }
            message = cardCertificates.getResponseStatus() + "";
            status = Constants.SUCCESS;
            certificateAuth = getCertificates(cardCertificates.getAuthenticationCertificate());
            certificateSign = getCertificates(cardCertificates.getSigningCertificate());
        } catch (ToolkitException e) {
            e.printStackTrace();
            if (e.getExceptionType() == ToolkitException.ExceptionType.CARD_PIN_ERROR) {
                message = e.getMessage() + "Attempts left " + e.getAttemptsLeft();
            } else {
                message = e.getMessage();
            }
            status = (int) e.getCode();
        } catch (Exception e) {
            message = "Unknown Error";
        }

        return status;
    }//doInBackground()

    @Override
    protected void onPostExecute(Integer status) {
        super.onPostExecute(status);

        weakReference.get().onCertificateReadComplete(this.status, message,
                certificateAuth, certificateSign);

    }

    private Certificate getCertificates(byte[] cert) {
        try {
            CertificateFactory certFactory = CertificateFactory.getInstance("X.509");
            ByteArrayInputStream in = new ByteArrayInputStream(cert);
            return (X509Certificate) certFactory.generateCertificate(in);
        } catch (CertificateException e) {
            e.printStackTrace();
        }
        return null;
    }

    public interface ReadCertificateListener {
        void onCertificateReadComplete(int status, String message,
                                       Certificate certificateAuth,
                                       Certificate certificateSign);
    }
}
