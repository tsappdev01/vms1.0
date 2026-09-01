package ae.emiratesid.idcard.toolkit.sample.fragment;

import android.os.Bundle;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;

import ae.emiratesid.idcard.toolkit.datamodel.MRZData;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.task.ParseMRZAsync;
import ae.emiratesid.idcard.toolkit.sample.task.VerifyCardAndBiometricAsync;
import ae.emiratesid.idcard.toolkit.sample.widget.LogTextView;

import static ae.emiratesid.idcard.toolkit.sample.AppController.isReading;

import androidx.annotation.Nullable;

public class ParseMRZ extends BaseFragment {

    private EditText edtTextMRZString;
    private Button btnParseMRZ;
    private ParseMRZAsync parseMRZAsync;
    private LogTextView txtStatus;
    private StringBuilder mrzStringData = new StringBuilder();

    @Nullable
    @Override
    public View onCreateView(LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState ) {

        View view = inflater.inflate( R.layout.fragment_parse_mrz, container, false );

        edtTextMRZString = ( EditText ) view.findViewById( R.id.edt_txt_parse_mrz );
        btnParseMRZ = ( Button ) view.findViewById( R.id.btn_parse_mrz );
        txtStatus= ( LogTextView ) view.findViewById( R.id.txtReadData );
        // make the text view scrollable.
        txtStatus.setMovementMethod(new ScrollingMovementMethod());

        btnParseMRZ.setOnClickListener( new View.OnClickListener() {
            @Override
            public void onClick( View v ) {

                showProgressDialog("Parsing MRZ...");

                parseMRZAsync = new ParseMRZAsync(parseMRZListener, edtTextMRZString.getText().toString());
                parseMRZAsync.execute(  );

            }
        } );

        return view;
    }
    //Create VerifyBiometricListener
    ParseMRZAsync.ParseMRZListener parseMRZListener = new ParseMRZAsync.ParseMRZListener() {
        @Override
        public void onParseMRZString( MRZData mrzData ) {

            hideProgressDialog();

            Logger.d( "MRZ Data  :: "+mrzData );
            if ( mrzData!=null ){

                mrzStringData.append("\n\nCard Number = " + mrzData.getCardNumber());
                mrzStringData.append("\n\nCard Expiry Date = " + mrzData.getCardExpiryDate());
                mrzStringData.append("\n\nDate of Birth = " + mrzData.getDateOfBirth());
                mrzStringData.append("\n\nDocument Type = " + mrzData.getDocumentType());
                mrzStringData.append("\n\nFull Name = " + mrzData.getFullName());
                mrzStringData.append("\n\nGender = " + mrzData.getGender());
                mrzStringData.append("\n\nID Number = " + mrzData.getIDNumber());
                mrzStringData.append("\n\nIssued Country = " + mrzData.getIssuedCountry());
                mrzStringData.append("\n\nNationality = " + mrzData.getNationality());

                Logger.d( "mrzStringData  :: "+mrzStringData );


//                txtStatus.setText( mrzStringData );
                txtStatus.appendLog(mrzStringData.toString(), LogTextView.LOG_TYPE.SUCCESS);


            }else {
                txtStatus.setLog("Error in parsing MRZ. \n",
                        LogTextView.LOG_TYPE.ERROR);
            }

        }//onParseMRZString
    };//ParseMRZAsync



    @Override
    protected void appendError( String message ) {

    }
    @Override
    public void onClick( View v ) {

    }
}
