package ae.emiratesid.idcard.toolkit.sample.fragment.register.view;

public interface RegisterDeviceView {
    void onUsernameEmpty();
    void onPasswordEmpty();
    void onDeviceIdEmpty();
    void onSuccess(String status);
}
