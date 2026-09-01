package ae.emiratesid.idcard.toolkit.sample.fragment;

import android.app.DatePickerDialog;
import android.app.Dialog;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.DatePicker;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.TextView;

import androidx.fragment.app.DialogFragment;
import androidx.fragment.app.Fragment;

import com.example.justocr.MrzOcrActivity;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Calendar;

import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.NFCCardParams;

public class SetNFCParamsFragment extends Fragment implements View.OnClickListener {

    private static final int MRZ_REQUEST = 101;
    public SetNFCParamsFragment() {
        // Required empty public constructor
    }

    private Button btnSetData ;
    private Button btnCancel ;
    private Button btnReadFromMRZ ;
    private ImageButton btnDateExp;
    private ImageButton btnDateDob;
    private EditText edtCardExp;
    private EditText edtDob;
    private EditText edtCardNumber;
    public static String date;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        View view = inflater.inflate( R.layout.fragment_nfc_fields_dialog, container, false);
        btnCancel   = (Button) view .findViewById(R.id.btn_cancel);
        btnSetData  = (Button) view .findViewById(R.id.btn_done);
        btnReadFromMRZ  = (Button) view .findViewById(R.id.btn_read_from_mrz);
        btnDateDob = (ImageButton) view.findViewById(R.id.img_date_picker_2);
        btnDateExp = (ImageButton) view.findViewById(R.id.img_date_picker_1);
        btnDateDob.setOnClickListener(this);
        btnDateExp.setOnClickListener(this);
        btnSetData.setOnClickListener(this);
        btnCancel.setOnClickListener(this);
        btnReadFromMRZ.setOnClickListener(this);
        edtDob = (EditText) view.findViewById(R.id.edt_dob);
        edtCardExp = (EditText) view.findViewById(R.id.edt_card_exp);
        edtCardNumber = (EditText) view.findViewById(R.id.edt_card_number);

        if( NFCCardParams.isNFCParamSet){
            setData();
        }
        return view;
    }

    private void setData(){
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

    // TODO: Rename method, update argument and hook method into UI event


    @Override
    public void onAttach(Context context) {
        super.onAttach(context);
    }

    @Override
    public void onDetach() {
        super.onDetach();
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

            setToBAC( cardNumber, dateOfBirth , cardExpDate );
            getActivity().finish();
        }//
        if(v == btnCancel){

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
        if(v == btnReadFromMRZ) {
            Intent intent =  new Intent(getActivity() , MrzOcrActivity.class);
            startActivityForResult(intent , MRZ_REQUEST);
        }

    }
    public static class DatePickerFragment extends DialogFragment
            implements DatePickerDialog.OnDateSetListener {

        EditText edtDate;
        public void setEditText(EditText edt){
            edtDate = edt;
        }
        @Override
        public Dialog onCreateDialog( Bundle savedInstanceState) {
            // Use the current date as the default date in the picker
            final Calendar c = Calendar.getInstance();
            int year = c.get(Calendar.YEAR);
            int month = c.get(Calendar.MONTH);
            int day = c.get(Calendar.DAY_OF_MONTH);

            // Create a new instance of DatePickerDialog and return it
            return new DatePickerDialog(getActivity(), this, year, month, day);
        }

        public void onDateSet( DatePicker view, int year, int month, int day) {
            // Do something with the date chosen by the user
            int num = month+1;
            String text = (num < 10 ? "0" : "") + num;
            date = day+"/"+text+"/"+year;
            if(edtDate != null){
                edtDate.setText(date);
            }//
        }
    }
    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if(requestCode == MRZ_REQUEST){
            if (resultCode == getActivity().RESULT_OK) {
                if(data!= null){
                    Logger.d("Status " + data.getBooleanExtra("STATUS" , false));
                    if(data.getBooleanExtra("STATUS" , false)){
                        String d = data.getStringExtra("DOB");
                        String c = data.getStringExtra("CARD_NUMBER");
                        String e = data.getStringExtra("CARD_EXPIRY");
                        setToBAC(c , d , e);
                    }//if()
                }//if()
            }//if()
        }//if()
    }//onActivityResult()

    private void setToBAC( String cardNumber, String dateOfBirth, String cardExpDate ) {

        if ( null == dateOfBirth || null == cardExpDate || null == cardNumber ) {
            Logger.e( "One of the fields are empty" );
            return;
        }//
        Logger.d( "--"+cardNumber+"--"+cardExpDate+"--"+dateOfBirth );
        edtDob.setText(dateOfBirth);
        edtCardExp.setText(cardExpDate);
        edtCardNumber.setText(cardNumber);
        SimpleDateFormat formater = new SimpleDateFormat( "dd/MM/yyyy" );
        SimpleDateFormat toFormat = new SimpleDateFormat( "yyMMdd" );
        try {
            NFCCardParams.DOB = toFormat.format( formater.parse( dateOfBirth ) );
            NFCCardParams.EXPIRY_DATE = toFormat.format( formater.parse( cardExpDate ) );
            NFCCardParams.CARD_NUMBER = cardNumber;
            NFCCardParams.isNFCParamSet = true;

        } catch ( ParseException e ) {
            e.printStackTrace();
        }
    }

}
