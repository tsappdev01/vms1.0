package ae.emiratesid.idcard.toolkit.sample.task;

import android.content.Context;
import android.os.AsyncTask;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;

public class ValidateCardCaptureFaceAsync extends AsyncTask<Void, Integer, Integer> {

    private CardReader cardReader;
    private String requestId;
    private Context mContext;
    private int requestCode;
    private int status;
    private String message;
    private ValidateCardCaptureFaceAsync.ValidateCardCaptureFaceListener listener;

    public ValidateCardCaptureFaceAsync(Context mContext, String requestId, int requestCode, ValidateCardCaptureFaceListener listener) {
        this.mContext = mContext;
        this.requestId = requestId;
        this.requestCode = requestCode;
        this.listener = listener;
    }

    @Override
    protected Integer doInBackground(Void... voids) {
        try {
            cardReader = ConnectionController.getConnection();
            if (cardReader == null) {
                Logger.e("EIDAToolkit is null;");
                return Constants.ERROR;
            }//

            try {
               // cardReader.validateCardAndCaptureFace(mContext, requestId, requestCode);
//            } catch (ToolkitException e) {
//                message = e.getMessage();
//                status = Constants.ERROR;
//                return status;
            } catch (Exception e) {
                message = e.getMessage();
                status = Constants.ERROR;
                return status;
            }


            //call the function
            status = Constants.SUCCESS;
            return status;
        } catch (ToolkitException e) {
            status = (int) e.getCode();
            message = e.getMessage();
            Logger.e("Exception occurred :::" + e.getMessage() + " Status = " + status);
        }//catch()
        return status;
    }

    @Override
    protected void onPostExecute(Integer integer) {
        super.onPostExecute(integer);
        if (listener != null)
            listener.onValidateCardCaptureFaceCompleted(status, message);
    }

    public interface ValidateCardCaptureFaceListener {
        void onValidateCardCaptureFaceCompleted(int status, String message);
    }
}
