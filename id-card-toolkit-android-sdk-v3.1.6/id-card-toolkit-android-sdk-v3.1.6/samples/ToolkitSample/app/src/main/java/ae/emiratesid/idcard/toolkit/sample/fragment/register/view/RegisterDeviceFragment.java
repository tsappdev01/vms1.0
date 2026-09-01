package ae.emiratesid.idcard.toolkit.sample.fragment.register.view;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

import androidx.annotation.Nullable;

import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.fragment.BaseFragment;
import ae.emiratesid.idcard.toolkit.sample.fragment.register.model.RegisterDeviceModel;
import ae.emiratesid.idcard.toolkit.sample.fragment.register.presenter.DeviceRegistrationPresenter;
import ae.emiratesid.idcard.toolkit.sample.task.RegisterDeviceAsync;

public class RegisterDeviceFragment extends BaseFragment implements RegisterDeviceView {

    private Button btnRegister;
    private EditText edtUsername;
    private EditText edtPassword;
    private EditText edtDeviceId;
    private DeviceRegistrationPresenter presenter;

    public RegisterDeviceFragment() {
        // Required empty public constructor
    }
        private TextView textViewStatus;
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        View view =  inflater.inflate(R.layout.fragment_register_device, container, false);
        textViewStatus = (TextView) view.findViewById(R.id.tv_status);
        btnRegister = (Button) view.findViewById(R.id.btn_register);
        edtUsername = (EditText) view.findViewById(R.id.edt_username);
        edtPassword = (EditText) view.findViewById(R.id.edt_password);
        edtDeviceId = (EditText) view.findViewById(R.id.edt_deviceId);

        return view;
    }

    @Override
    public void onViewCreated(View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        presenter = new DeviceRegistrationPresenter(this);

        btnRegister.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                showProgressDialog("Registering Device with Server.");
                RegisterDeviceModel model = new RegisterDeviceModel();
                model.setUserName(edtUsername.getText().toString());
                model.setPassword(edtPassword.getText().toString());
                model.setDeviceId(edtDeviceId.getText().toString());
                presenter.registerDevice(model);
            }
        });

        }

    @Override
    public void onClick(View view) {

    }

    @Override
    public void onDetach() {
        super.onDetach();
    }

    @Override
    protected void appendError(String message) {
    }


    @Override
    public void onUsernameEmpty() {
        hideProgressDialog();
        edtUsername.setError("required");
    }

    @Override
    public void onPasswordEmpty() {
        hideProgressDialog();
        edtPassword.setError("required");

    }

    @Override
    public void onDeviceIdEmpty() {
        hideProgressDialog();
        edtDeviceId.setError("required");

    }

    @Override
    public void onSuccess(String status) {
        hideProgressDialog();
        textViewStatus.setText(status);
    }
}
