package ae.emiratesid.idcard.toolkit.sample.task;

import android.nfc.Tag;
import android.os.AsyncTask;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.NFCCardParams;

public class CardSerialNumberAsync extends AsyncTask<Void, Integer, Integer> {

    private CardReader cardReader;
    private CardSerialNumberListener cardSerialNumberListener;
    private int status;
    private String message;
    private String cardSerialNumber;
    private Tag tag;
    private String cardNumber, dob, expiryDate;

    public CardSerialNumberAsync(CardSerialNumberListener cardSerialNumberListener, Tag tag) {
        this.cardSerialNumberListener = cardSerialNumberListener;
        this.tag = tag;
        this.cardNumber = NFCCardParams.CARD_NUMBER;
        this.dob = NFCCardParams.DOB;
        this.expiryDate = NFCCardParams.EXPIRY_DATE;
    }

    public CardSerialNumberAsync(CardSerialNumberListener cardSerialNumberListener) {
        this.cardSerialNumberListener = cardSerialNumberListener;
    }

    @Override
    protected Integer doInBackground(Void... voids) {
        try {
            if (tag != null) {
                cardReader = ConnectionController.initConnection(tag);
                ConnectionController.setNFCParams(cardNumber, dob, expiryDate);
            }
            else {
                cardReader = ConnectionController.getConnection();
            }

            if (cardReader == null) {
                Logger.e("EIDAToolkit is null;");
                return Constants.ERROR;
            }//

            cardSerialNumber = cardReader.getCardSerialNumber();
            Logger.d("Card Serial Number is ::" + cardSerialNumber);
            if (cardSerialNumber == null || cardSerialNumber.isEmpty()) {
                message = "Failed";
                status = Constants.ERROR;
                return status;
            }
            status = Constants.SUCCESS;
            return status;
        } catch (ToolkitException e) {
            e.printStackTrace();
            status = (int) e.getCode();
            message = e.getMessage();
            //deviceIdListener.onGetDeviceId(deviceIdResult);
            Logger.e("Exception occurred :::" + e.getMessage() + " Status = " + status);
            return status;
        }
    }

    @Override
    protected void onPostExecute(Integer integer) {
        super.onPostExecute(integer);
        Logger.d("onPostExecute");
        cardSerialNumberListener.onGetCardSerialNumber(status, message, cardSerialNumber);


    }

    public interface CardSerialNumberListener {
        void onGetCardSerialNumber(int status, String message, String cardSerialNumber);
    }
}
