package ae.emiratesid.idcard.toolkit.sample.task;

import android.os.AsyncTask;

import java.lang.ref.WeakReference;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardFamilyBookData;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.CryptoUtils;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;

public class ReadFamilyBookAsync extends AsyncTask<Void , Integer , Integer> {

    private CardReader cardReader;
    private int status;
    private WeakReference<ReadFamilyBookListener> weakReference;
    private CardFamilyBookData familyBookData;
    private final String USER_PIN;
    private String message;

    public ReadFamilyBookAsync(ReadFamilyBookListener listener , final String user_pin) {
        this.weakReference = new WeakReference<>(listener);
        this.USER_PIN = user_pin;
        
    }//ReadFamilyBookAsync

    @Override
    protected Integer doInBackground(Void... voids) {
        try {
            cardReader = ConnectionController.getConnection();
            if(cardReader == null){
                return Constants.ERROR;
            }

            String requestId = RequestGenerator.generateRequestID();
            Logger.d("requestId :: " + requestId);

            String requestHandle = cardReader.prepareRequest(requestId);
            Logger.d("requestHandle :: " + requestHandle);

            CryptoUtils cryptoUtils = new CryptoUtils();
            String encodedPin = cryptoUtils.encryptParams(USER_PIN, requestHandle);
            Logger.d("encodedPin " + encodedPin.length());

            if (encodedPin.length() > 344) {
                Logger.e("Pin Encoding is invalid");
                message = "Pin Encoding is invalid";
                status = Constants.ERROR;
                return status;
            }
            //encode the pin

            // call the particular method here
            familyBookData = cardReader.readFamilyBook(encodedPin);

            status = Constants.SUCCESS;
        }//try
        catch( ToolkitException e ){
            e.printStackTrace();
            status = (int) e.getCode();
            message = e.getMessage();
            Logger.e("Status = "  + status + "  message :: " + message);
        }// catch()
        catch( Exception e ){
            e.printStackTrace();
            message = "Unknown Error";
            Logger.e("Status = "  + status + "  message :: " + message);
        }// catch()

        return status;
    }//doInBackground()


    @Override
    protected void onPostExecute(Integer status) {
        super.onPostExecute(status);
        weakReference.get().onFamilyBookReadComplete(this.status ,message, familyBookData);
    }//onPostExecute()

    public interface ReadFamilyBookListener{
        void onFamilyBookReadComplete(int status, String message,
                                             CardFamilyBookData familyBookData);
    }//ReadFamilyBookListener
}
