package ae.emiratesid.idcard.toolkit.sample.task;

import android.os.AsyncTask;

import ae.emiratesid.idcard.toolkit.Toolkit;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardPublicData;
import ae.emiratesid.idcard.toolkit.datamodel.ToolkitResponse;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;

public class ValidateCardOffCardAsync extends AsyncTask<Void, Integer, Integer> {
    private Toolkit toolkit;
    private String requestId;
    private String cardNumber;
    private String idn;
    private int status;
    private String message;
    private String xmlResponse;
    private ValidateCardOffCardAsync.ValidateCardOffCardAsyncListener listener;
    private ToolkitResponse toolkitResponse;

    public ValidateCardOffCardAsync(final String requestId, final String cardNumber, final String idn, ValidateCardOffCardAsync.ValidateCardOffCardAsyncListener listener) {
        this.requestId = requestId;
        this.cardNumber = cardNumber;
        this.idn = idn;
        this.listener = listener;
    }

    @Override
    protected Integer doInBackground(Void... voids) {
        try {
            toolkit = ConnectionController.getToolkit();
            if (toolkit == null) {
                Logger.e("Toolkit is not initialized.");
                message = "Toolkit is not initialized.";
                status = Constants.ERROR;
                return status;
            }//

            toolkitResponse = toolkit.checkCardStatusOffCard(idn, cardNumber);

            if (toolkitResponse == null) {
                message = "Response from Toolkit is null";
                status = Constants.ERROR;
                return status;
            }
            //call the function
            status = Constants.SUCCESS;
            xmlResponse = toolkitResponse.toXmlString();
            return status;
        } catch (ToolkitException e) {
            status = (int) e.getCode();
            message = e.getMessage();
            xmlResponse = e.getXMLString();
            Logger.e("Exception occurred :::" + e.getMessage() + " Status = " + status);
        }//catch()
        ConnectionController.closeConnection();
        return status;
    }

    @Override
    protected void onPostExecute(Integer integer) {
        super.onPostExecute(integer);
        if (listener != null)
            listener.onCardValidationCompleted(status, message, xmlResponse);
    }

    public interface ValidateCardOffCardAsyncListener {
        void onCardValidationCompleted(int status, String message, String xmlResponse);
    }
}
