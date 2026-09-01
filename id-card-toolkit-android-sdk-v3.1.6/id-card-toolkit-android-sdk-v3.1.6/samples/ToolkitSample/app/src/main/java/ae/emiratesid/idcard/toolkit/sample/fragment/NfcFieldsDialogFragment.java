package ae.emiratesid.idcard.toolkit.sample.fragment;

import android.app.DatePickerDialog;
import android.app.Dialog;
import android.content.Context;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.DatePicker;
import android.widget.EditText;
import android.widget.ImageButton;

import androidx.fragment.app.DialogFragment;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Calendar;

import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.utils.NFCCardParams;

/**
 * A simple {@link Fragment} subclass.
 * Activities that contain this fragment must implement the
 * {@link OnDialogInteraction} interface
 * to handle interaction events.
 */
public class NfcFieldsDialogFragment extends DialogFragment implements View.OnClickListener  {

    private OnDialogInteraction mListener;
    private Button btnSetData ;
    private Button btnCancel ;
    private ImageButton btnDateExp;
    private ImageButton btnDateDob;
    private EditText edtCardExp;
    private EditText edtDob;
    private EditText edtCardNumber;
    public static String date;


    public NfcFieldsDialogFragment() {
        // Required empty public constructor
    }


    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        View view = inflater.inflate( R.layout.fragment_nfc_fields_dialog, container, false);
        btnCancel   = (Button) view .findViewById(R.id.btn_cancel);
        btnSetData  = (Button) view .findViewById(R.id.btn_done);
        btnDateDob = (ImageButton) view.findViewById(R.id.img_date_picker_2);
        btnDateExp = (ImageButton) view.findViewById(R.id.img_date_picker_1);
        btnDateDob.setOnClickListener(this);
        btnDateExp.setOnClickListener(this);
        btnSetData.setOnClickListener(this);
        btnCancel.setOnClickListener(this);
        edtDob = (EditText) view.findViewById(R.id.edt_dob);
        edtCardExp = (EditText) view.findViewById(R.id.edt_card_exp);
        edtCardNumber = (EditText) view.findViewById(R.id.edt_card_number);

        if( NFCCardParams.isNFCParamSet){
            SimpleDateFormat toFormat = new SimpleDateFormat("dd/MM/yyyy");
            SimpleDateFormat formater = new SimpleDateFormat("yyMMdd");
            try {
                edtDob.setText(toFormat.format(formater.parse(NFCCardParams.DOB)));
                edtCardExp.setText(toFormat.format(formater.parse(NFCCardParams.EXPIRY_DATE)));
                edtCardNumber.setText(NFCCardParams.CARD_NUMBER);
            } catch (ParseException e) {
                e.printStackTrace();
            }
        }
        return view;
    }

    // TODO: Rename method, update argument and hook method into UI event


    @Override
    public void onAttach(Context context) {
        super.onAttach(context);
        if (context instanceof OnDialogInteraction) {
            mListener = (OnDialogInteraction) context;
        } else {
            throw new RuntimeException(context.toString()
                    + " must implement OnFragmentInteractionListener");
        }
    }

    @Override
    public void onDetach() {
        super.onDetach();
        mListener = null;
    }

    @Override
    public void onClick(View v) {
        if(v == btnSetData){

            String dateOfBirth = edtDob.getText().toString();
            if(null == dateOfBirth || dateOfBirth.equals("")){
                return;
            }//
            String cardExpDate = edtCardExp.getText().toString();
            if(null == cardExpDate || cardExpDate.equals("")){
                return;
            }//
            String cardNumber = edtCardNumber.getText().toString();
            if(null == cardNumber || cardNumber.equals("")){
                return;
            }//

            dismiss();
            mListener.onSetData(cardNumber,dateOfBirth,cardExpDate);
        }//
        if(v == btnCancel){
            dismiss();
            mListener.onFinish();
        }//
        if(v == btnDateDob){
            DatePickerFragment newFragment = new DatePickerFragment();
            newFragment.setEditText(edtDob);
            newFragment.show(getActivity().getSupportFragmentManager(), "datePicker");

        }//
        if(v == btnDateExp) {
            DatePickerFragment newFragment = new DatePickerFragment();
            newFragment.setEditText(edtCardExp);
            newFragment.show(getActivity().getSupportFragmentManager(), "datePicker");
        }

    }



    /**
     * This interface must be implemented by activities that contain this
     * fragment to allow an interaction in this fragment to be communicated
     * to the activity and potentially other fragments contained in that
     * activity.
     * <p/>
     * See the Android Training lesson <a href=
     * "http://developer.android.com/training/basics/fragments/communicating.html"
     * >Communicating with Other Fragments</a> for more information.
     */
    public interface OnDialogInteraction {
        // TODO: Update argument type and name
        void onSetData( String cardNumber, String dateOfBirth, String cardExpDate );
        void onFinish();
    }

    public static class DatePickerFragment extends DialogFragment
            implements DatePickerDialog.OnDateSetListener {

        EditText edtDate;
        public void setEditText(EditText edt){
            edtDate = edt;
        }
        @Override
        public Dialog onCreateDialog(Bundle savedInstanceState) {
            // Use the current date as the default date in the picker
            final Calendar c = Calendar.getInstance();
            int year = c.get(Calendar.YEAR);
            int month = c.get(Calendar.MONTH);
            int day = c.get(Calendar.DAY_OF_MONTH);

            // Create a new instance of DatePickerDialog and return it
            return new DatePickerDialog(getActivity(), this, year, month, day);
        }

        public void onDateSet(DatePicker view, int year, int month, int day) {
            // Do something with the date chosen by the user
            int num = month+1;
            String text = (num < 10 ? "0" : "") + num;
            date = day+"/"+text+"/"+year;
            if(edtDate != null){
                edtDate.setText(date);
            }//
        }
    }
}
