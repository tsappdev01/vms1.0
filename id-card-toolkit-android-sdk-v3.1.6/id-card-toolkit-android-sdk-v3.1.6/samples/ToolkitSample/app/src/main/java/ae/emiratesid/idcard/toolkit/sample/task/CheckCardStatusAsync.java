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
import ae.emiratesid.idcard.toolkit.sample.utils.NFCCardParams;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;

public class CheckCardStatusAsync extends AsyncTask<Void, Integer, Integer> {
    private CardReader cardReader;
    private int status;
    private WeakReference<CheckCardStatusListener> weakReference;
    private ToolkitResponse response;
    private String message;
    private String xmlResponse;

    private Tag tag;
    private String cardNumber, dob, expiryDate;

    public CheckCardStatusAsync(CheckCardStatusListener listener) {
        this.weakReference = new WeakReference<CheckCardStatusListener>(listener);
    }//CheckCardStatusAsync


    public CheckCardStatusAsync(CheckCardStatusListener listener, Tag tag) {

        this.weakReference = new WeakReference<CheckCardStatusListener>(listener);
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
            }//

            String requestId = RequestGenerator.generateRequestID();
            //call the function
            response = cardReader.checkCardStatus(requestId);
            if (response == null) {
                Logger.e("Response in check card status is null");
                message = "Response in check card status is null";
                return Constants.ERROR;
            }
            message = response.getStatus();
            xmlResponse = response.toXmlString();
            return Constants.SUCCESS;

        } catch (ToolkitException e) {
            status = (int) e.getCode();
            message = e.getMessage();
            Logger.e("Exception occurred :::" + e.getMessage() + " Status = " + status);
        }//catch()
        return status;
    }//doInBackground()

    @Override
    protected void onPostExecute(Integer status) {
        super.onPostExecute(status);
        Logger.d("CheckCardStatusAsync::onPostExecute() status =" + status);
        weakReference.get().onCheckCardStatus(this.status, this.message, xmlResponse);
    }//

    public interface CheckCardStatusListener {
        void onCheckCardStatus(int status, String response, String xmlString);
    }
}
