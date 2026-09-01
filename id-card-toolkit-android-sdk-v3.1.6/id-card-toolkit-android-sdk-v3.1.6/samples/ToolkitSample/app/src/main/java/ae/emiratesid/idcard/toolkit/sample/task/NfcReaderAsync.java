package ae.emiratesid.idcard.toolkit.sample.task;

import android.nfc.Tag;
import android.os.AsyncTask;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardPublicData;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;
import ae.emiratesid.idcard.toolkit.sample.utils.XmlUtils;

public class NfcReaderAsync extends AsyncTask<Void, Integer, Integer> {

    private NfcReaderListener nfcReaderListener;
    private CardReader cardReader;
    private int status;
    private String message;
    private String requestID;
    private Tag tag;

    public NfcReaderAsync(NfcReaderListener nfcReaderListener, Tag tag) {

        this.nfcReaderListener = nfcReaderListener;
        this.tag = tag;
    }

    public NfcReaderAsync(NfcReaderListener nfcReaderListener) {
        this.nfcReaderListener = nfcReaderListener;
    }

    @Override
    protected Integer doInBackground(Void... voids) {

        try {
            if (tag != null) {
                cardReader = ConnectionController.initConnection(tag);
            } else {
                cardReader = ConnectionController.getConnection();
            }

            if (cardReader == null) {
                Logger.e("EIDAToolkit is null;");
                return Constants.ERROR;
            }//

            requestID = RequestGenerator.generateRequestID();

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
        nfcReaderListener.onReadComplete(this.status, message, requestID);
    }//onPostExecute()

    public interface NfcReaderListener {
        public void onReadComplete(int status, String message , String requestID);
    }
}//end-of-class
