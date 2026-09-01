package ae.emiratesid.idcard.toolkit.sample.task;

import android.nfc.Tag;
import android.os.AsyncTask;

import java.lang.ref.WeakReference;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.FingerData;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.utils.NFCCardParams;

public class GetFingerIndexAsync extends AsyncTask<String , Integer , Integer> {
    private CardReader cardReader;
    private int status;
    private WeakReference<GetFingerIndexListener> weakReference;
    private FingerData[] indexes;
    private String message;
    private Tag tag;
    private String cardNumber , dob , expiryDate;


    public GetFingerIndexAsync(GetFingerIndexListener  listener) {
        this.weakReference = new WeakReference<GetFingerIndexListener>(listener);
    }//GetFingerIndexAsync

    public GetFingerIndexAsync(GetFingerIndexListener  listener,
                               Tag tag) {
        this.weakReference = new WeakReference<GetFingerIndexListener>(listener);
        this.tag = tag;
        this.cardNumber = NFCCardParams.CARD_NUMBER;
        this.dob = NFCCardParams.DOB;
        this.expiryDate = NFCCardParams.EXPIRY_DATE;
    }

    @Override
    protected Integer doInBackground(String... params) {

        try {
            if(tag != null){
                cardReader = ConnectionController.initConnection(tag);
                ConnectionController.setNFCParams(cardNumber , dob, expiryDate);
            }
            else{
                cardReader = ConnectionController.getConnection();
            }

            if(cardReader == null){
                return Constants.ERROR;
            }//

            indexes =  cardReader.getFingerData();
            if(null != indexes || indexes.length <= 0){
                return Constants.ERROR;
            }

        } catch (ToolkitException e) {
            status = (int) e.getCode();
            message = e.getMessage();
        }//catch()
        return status;
    }//doInBackground()


    @Override
    protected void onPostExecute(Integer status) {
        super.onPostExecute(status);
        weakReference.get().onFingerIndexFetched(this.status ,message , indexes );
    }//onPostExecute()

    public interface GetFingerIndexListener{
        public void onFingerIndexFetched(int status, String message, FingerData[] fingers);
    }
}
