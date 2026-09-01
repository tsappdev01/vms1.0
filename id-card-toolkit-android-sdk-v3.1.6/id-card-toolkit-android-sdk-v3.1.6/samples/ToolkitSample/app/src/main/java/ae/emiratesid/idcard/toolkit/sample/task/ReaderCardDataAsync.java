package ae.emiratesid.idcard.toolkit.sample.task;

import android.nfc.Tag;
import android.os.AsyncTask;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardPublicData;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.NFCCardParams;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;
import ae.emiratesid.idcard.toolkit.sample.utils.XmlUtils;

public class ReaderCardDataAsync extends AsyncTask<Void, Integer, Integer> {

    private ReaderCardDataListener readerCardDataListener;
    private CardReader cardReader;
    private int status;
    private String message;
    private Tag tag;
    private String cardNumber, dob, expiryDate;

    public ReaderCardDataAsync(ReaderCardDataListener readerCardDataListener, Tag tag) {

        this.readerCardDataListener = readerCardDataListener;
        this.tag = tag;
        this.cardNumber = NFCCardParams.CARD_NUMBER;
        this.dob = NFCCardParams.DOB;
        this.expiryDate = NFCCardParams.EXPIRY_DATE;
    }

    public ReaderCardDataAsync(ReaderCardDataListener readerCardDataListener) {
        this.readerCardDataListener = readerCardDataListener;
    }

    private CardPublicData publicData;

    @Override
    protected Integer doInBackground(Void... voids) {

        boolean bReadNonModifiableData = true;
        boolean bReadModifiableData = true;
        boolean bReadPhotography = true;
        boolean bSignatureImage = true;
        boolean bReadAddress = true;
        try {
            if (tag != null) {
                cardReader = ConnectionController.initConnection(tag);
                ConnectionController.setNFCParams(cardNumber, dob, expiryDate);
            } else {
                cardReader = ConnectionController.getConnection();
            }

            if (cardReader == null) {
                Logger.e("EIDAToolkit is null;");
                return Constants.ERROR;
            }//

            /**
             * Read the public data from card
             * Method will throw error if unable to read data from card
             */

            String requestID = RequestGenerator.generateRequestID();
            publicData = cardReader.readPublicData(requestID,
                    bReadNonModifiableData,
                    bReadModifiableData,
                    bReadPhotography,
                    bSignatureImage,
                    bReadAddress);

            if (publicData == null) {
                Logger.e("Public data is null");
                message = "Public data is null";
                return Constants.ERROR;
            }//
            XmlUtils.validateXML(publicData.toXmlString(), requestID);
            status = Constants.SUCCESS;
            //if()
        } catch (ToolkitException e) {
            status = (int) e.getCode();
            Logger.d("Status Code : ReaderCardDataAsync ::" + status);
            message = e.getMessage();
            Logger.e("Attempts left " + e.getAttemptsLeft());
            Logger.e("Error in reading...." + e.getLocalizedMessage());
        } catch (Exception e) {
            e.printStackTrace();
            message = "Unknown error";
        }// catch()
        return status;
    }//

    @Override
    protected void onPostExecute(Integer result) {
        super.onPostExecute(result);
        // call the listener to notify the caller that async has finished.
        // There you can handle all the ui related stuffs.
        readerCardDataListener.onCardReadComplete(this.status, message, publicData);
    }//onPostExecute()
}//end-of-class
