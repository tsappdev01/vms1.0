package ae.emiratesid.idcard.toolkit.sample.task;

import android.os.AsyncTask;

import ae.emiratesid.idcard.toolkit.Toolkit;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.RegisterDeviceResponse;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.CryptoUtils;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;

public class RegisterDeviceAsync extends AsyncTask<Void , Integer , Integer> {

    private static String DEVICE_ID = null ;
    private Toolkit toolkit;
    private String userPassword , userName;
    private int status;
    private String message;
    private DeviceRegistrationListener listener;

    public RegisterDeviceAsync(final String userPassword, final String userName, DeviceRegistrationListener listener,final String deviceId) {
        this.userPassword = userPassword;
        this.userName = userName;
        this.listener = listener;
        this.DEVICE_ID = deviceId;
    }

    @Override
    protected Integer doInBackground(Void... voids) {
        try {
            toolkit = ConnectionController.getToolkitObject();
            if(toolkit == null){
                return Constants.ERROR;
            }

            String requestId =  RequestGenerator.generateRequestID();
            Logger.d("requestId :: " +requestId);


            String requestHandle =  toolkit.prepareRequest(requestId);
            Logger.d("requestHandle :: " +requestHandle);

            CryptoUtils cryptoUtils =  new CryptoUtils();
            String encodeUserName = cryptoUtils.encryptParams(userName.getBytes() , requestHandle);
            String encodePassword = cryptoUtils.encryptParams(userPassword.getBytes()
                    , requestHandle);
            Logger.d("encodeUserName :: " +encodeUserName);
            Logger.d("encodePassword :: " +encodePassword);

            RegisterDeviceResponse response  =toolkit.registerDevice(encodeUserName ,
                    encodePassword, DEVICE_ID);
            if(response == null ){
                message = "Response from Toolkit is null";
                status =  Constants.ERROR;
                return status;
            }
            //call the function
            status = Constants.SUCCESS;
            return status;
        } catch (ToolkitException e) {
            status = (int) e.getCode();
            message =  e.getMessage();
            Logger.e("Exception occurred :::"+e.getMessage() + " Status = "+status );
        }//catch()
        return status;
    }

    @Override
    protected void onPostExecute(Integer integer) {
        super.onPostExecute(integer);
        if(  listener!= null)
        listener.onDeviceRegistrationCompleted(status,message);
    }

    public interface DeviceRegistrationListener{
        void onDeviceRegistrationCompleted(int status, String message);
    }
}
