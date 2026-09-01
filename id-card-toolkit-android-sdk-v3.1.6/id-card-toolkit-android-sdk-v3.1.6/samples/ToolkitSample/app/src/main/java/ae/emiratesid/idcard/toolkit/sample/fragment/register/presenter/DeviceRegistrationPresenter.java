package ae.emiratesid.idcard.toolkit.sample.fragment.register.presenter;

import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.fragment.register.model.RegisterDeviceModel;
import ae.emiratesid.idcard.toolkit.sample.fragment.register.view.RegisterDeviceView;
import ae.emiratesid.idcard.toolkit.sample.task.RegisterDeviceAsync;

public class DeviceRegistrationPresenter {


    private RegisterDeviceAsync task;

    private RegisterDeviceView view;
    public DeviceRegistrationPresenter(RegisterDeviceView view)
    {
     this.view = view;
    }

    public void registerDevice(RegisterDeviceModel model) {

    if (model.getUserName() == null || model.getUserName().equals("")) {
            view.onUsernameEmpty();
        } else if (model.getPassword() == null || model.getPassword().equals("")) {
            view.onPasswordEmpty();
        } else if (model.getDeviceId() == null || model.getDeviceId().equals("")) {
            view.onDeviceIdEmpty();
        } else {
            task = new RegisterDeviceAsync(model.getPassword(), model.getUserName(), new RegisterDeviceAsync.DeviceRegistrationListener() {
                @Override
                public void onDeviceRegistrationCompleted(int status, String message) {

                    if (status == Constants.SUCCESS) {
                        cleanUp();
                        view.onSuccess("Device Registration successful");

                    } else {
                        cleanUp();
                        view.onSuccess("Device Registration failed.\n" + message);
                    }
                }
            },model.getDeviceId());
            task.execute();
        }
    }

    private void cleanUp()
    {
          if (task != null && !task.isCancelled()){
            task.cancel(true);
        }
    }
}
