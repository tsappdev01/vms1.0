package ae.emiratesid.idcard.toolkit.sample.task;

import android.os.AsyncTask;

import ae.emiratesid.idcard.toolkit.Toolkit;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;

public class DeviceIdAsync extends AsyncTask<Void, Integer, Integer> {


    private DeviceIdListener deviceIdListener;
    private int status;
    private String message;
    private String deviceId;

    public DeviceIdAsync(DeviceIdListener deviceIdListener) {
        this.deviceIdListener = deviceIdListener;
    }

    @Override
    protected Integer doInBackground(Void... voids) {
        try {
            Toolkit toolkit = ConnectionController.getToolkitObject();
            if (toolkit == null) {
                status = Constants.ERROR;
                return status;
            }
            deviceId = toolkit.getDeviceId();
            Logger.d("Device ID is ::" + deviceId);
            if (deviceId == null || deviceId.isEmpty()) {
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
        deviceIdListener.onGetDeviceId(status, message, deviceId);


    }

    public interface DeviceIdListener {
        void onGetDeviceId(int status, String message, String deviceId);
    }

}
