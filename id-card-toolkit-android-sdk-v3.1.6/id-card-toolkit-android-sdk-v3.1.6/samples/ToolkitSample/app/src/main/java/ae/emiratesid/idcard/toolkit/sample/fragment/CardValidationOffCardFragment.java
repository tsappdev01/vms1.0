package ae.emiratesid.idcard.toolkit.sample.fragment;

import android.content.Context;
import android.os.Bundle;

import androidx.fragment.app.Fragment;

import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;

import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.task.ValidateCardOffCardAsync;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;
import ae.emiratesid.idcard.toolkit.sample.widget.LogTextView;

/**
 * A simple {@link Fragment} subclass.
 * Use the {@link CardValidationOffCardFragment#newInstance} factory method to
 * create an instance of this fragment.
 */
public class CardValidationOffCardFragment extends BaseFragment implements View.OnClickListener {

    private Button btnValidateCard;
    private EditText edtCardNumber;
    private EditText edtIdn;
    private LogTextView textViewStatus;
    private Context mContext;
    private ValidateCardOffCardAsync task;

    public CardValidationOffCardFragment() {
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_card_validation_off_card, container, false);
        textViewStatus = (LogTextView) view.findViewById(R.id.tv_validate_card);
        btnValidateCard = (Button) view.findViewById(R.id.btn_validate_card);
        edtCardNumber = (EditText) view.findViewById(R.id.edt_card_number);
        edtIdn = (EditText) view.findViewById(R.id.edt_idn);

        textViewStatus.setMovementMethod(new ScrollingMovementMethod());

        btnValidateCard.setOnClickListener(this);

        return view;
    }

    @Override
    public void onClick(View view) {
        if (view == btnValidateCard) {
            textViewStatus.setText("");

            String sCardNumber = edtCardNumber.getText().toString();
            if (!sCardNumber.isEmpty()) {
                String sIdn = edtIdn.getText().toString();
                if (!sIdn.isEmpty()) {
                    String requestId = RequestGenerator.generateRequestID();

                    showProgressDialog("Validating Card with Server.");
                    task = new ValidateCardOffCardAsync(requestId, edtCardNumber.getText().toString(), edtIdn.getText().toString(), new ValidateCardOffCardAsync.ValidateCardOffCardAsyncListener() {
                        @Override
                        public void onCardValidationCompleted(int status, String message, String xmlResponse) {
                            if (status == Constants.SUCCESS) {
                                cleanUp();
                                hideProgressDialog();
                                textViewStatus.appendLog(xmlResponse, LogTextView.LOG_TYPE.SUCCESS);

                            } else {
                                cleanUp();
                                hideProgressDialog();
                                textViewStatus.setLog(message, LogTextView.LOG_TYPE.ERROR);

                            }
                        }
                    });
                    task.execute();

                } else {
                    edtIdn.setError("required");
                }
            } else {
                edtCardNumber.setError("required");
            }
        }//
    }

    @Override
    public void onAttach(Context context) {
        super.onAttach(context);
        mContext = context;
    }


    @Override
    public void onDetach() {
        super.onDetach();
        mContext = null;
    }//

    @Override
    protected void appendError(String message) {
        textViewStatus.appendLog(message, LogTextView.LOG_TYPE.ERROR);
    }

    private void cleanUp() {
        if (task != null && !task.isCancelled()) {
            task.cancel(true);
        }
    }
}