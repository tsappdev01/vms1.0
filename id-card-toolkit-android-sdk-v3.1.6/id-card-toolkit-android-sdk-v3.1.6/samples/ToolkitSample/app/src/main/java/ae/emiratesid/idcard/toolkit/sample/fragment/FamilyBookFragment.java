package ae.emiratesid.idcard.toolkit.sample.fragment;

import android.os.Bundle;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;

import java.util.List;

import ae.emiratesid.idcard.toolkit.datamodel.CardFamilyBookData;
import ae.emiratesid.idcard.toolkit.datamodel.Child;
import ae.emiratesid.idcard.toolkit.datamodel.HeadOfFamily;
import ae.emiratesid.idcard.toolkit.datamodel.Wife;
import ae.emiratesid.idcard.toolkit.internal.ErrorCode;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.task.ReadFamilyBookAsync;
import ae.emiratesid.idcard.toolkit.sample.widget.LogTextView;

import static ae.emiratesid.idcard.toolkit.sample.AppController.isReading;

/**
 *
 */
public class FamilyBookFragment extends BaseFragment {

    private ReadFamilyBookAsync readFamilyBookAsync;
    private final StringBuilder familyData = new StringBuilder();

    public FamilyBookFragment() {
        // Required empty public constructor
    }//FamilyBookFragment()

    //create listener
    private ReadFamilyBookAsync.ReadFamilyBookListener readFamilyBookListener =
            new ReadFamilyBookAsync.ReadFamilyBookListener(){
                @Override
                public void onFamilyBookReadComplete(int status, String message, CardFamilyBookData familyBookData) {
                    hideProgressDialog();
                    isReading = false;
                    Logger.d("ReadFamilyBookListener()::" + message);
                    if(status != Constants.SUCCESS){
                        if(message != null && !message.isEmpty()){
                            if(status != ErrorCode.UNSPECIFIED.getCode()) {
                                txtStatus.setLog("Error Code : " + status + "\n" + message,
                                        LogTextView.LOG_TYPE.ERROR);
                            }
                            else
                            {
                                txtStatus.setLog(message,
                                        LogTextView.LOG_TYPE.ERROR);
                            }
                        }
                    }
                    else {
                        displayFamilyData(familyBookData);
                    }
                }//onFamilyBookReadComplete()
            };//readFamilyBookListener()

    private LogTextView txtStatus;
    private Button btnRead;
    private EditText edtPin;
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {

        // Inflate the layout for this fragment
        View view =  inflater.inflate(R.layout.fragment_family_book, container, false);
        txtStatus = (LogTextView) view.findViewById(R.id.txtReadData);
        txtStatus.setMovementMethod(new ScrollingMovementMethod());
        edtPin = (EditText) view.findViewById(R.id.edtPin);
        btnRead = (Button) view.findViewById(R.id.btn_refresh);
        btnRead.setOnClickListener(this);
        return view;
    }
    @Override
    public void onClick(View view) {
        txtStatus.setText("");

        if(!isReading){
            String userPin =  edtPin.getText().toString();
            if(userPin.isEmpty()){
                edtPin.setError("Can't proceed without pin");
                return;
            }//
            showProgressDialog("Verifying pin and fetching family book data...");
            //execute the async task
            readFamilyBookAsync = new ReadFamilyBookAsync(readFamilyBookListener, userPin);
            readFamilyBookAsync.execute(); //validate for null or empty
            isReading = true;
        }//if()
    }//onClick()

    private  void displayFamilyData( CardFamilyBookData familyBookData){
        familyData.append("\n-----------------------------FamilyData() -------------------------------");
        if( null == familyBookData ) {
            familyData.append("\nFamily data  read family book data is null ");
            txtStatus.appendLog(familyData.toString() , LogTextView.LOG_TYPE.ERROR);
            return;
        }//if()

        Logger.d(familyBookData.toXmlString());
        try {
            familyData.append("\nFamily data  read ");
            printHeadOfFamily(familyBookData.getHeadOfFamily());
            //
            List<Wife> wifeList =  familyBookData.getWives();
            if(null != wifeList || wifeList.isEmpty()){
                for(Wife wife : wifeList){
                    printWifeData(wife);
                }//for()
            }//if()
            else{
                familyData.append("\n No wife  found.");
            }

            //
            List<Child> childList =  familyBookData.getChildren();
            if(null != childList || childList.isEmpty()){
                for(Child child : childList){
                    printChildData(child);
                }//for()
            }//if()
            else{
                familyData.append("\n No child found.");
            }
        } catch (Exception e) {
            e.printStackTrace();
            familyData.append("\n").append(e.getLocalizedMessage());
            txtStatus.appendLog(familyData.toString() , LogTextView.LOG_TYPE.ERROR);
        }// catch()
        txtStatus.appendLog(familyData.toString() , LogTextView.LOG_TYPE.SUCCESS);
    }//

    private void printHeadOfFamily(HeadOfFamily data) {

        if(data == null){
            familyData.append("\n HeadOfFamily Data is not available.");
            return;
        }
        familyData.append(" \n-----------printHeadOfFamily()---------------------|");

        familyData.append("\nClanArabic--------" + data.getClanArabic());
        familyData.append(" \nClanEnglish-------" + data.getClanEnglish());
        familyData.append(" \nDateOfBirth--------" + data.getDateOfBirth());
        familyData.append(" \nEmirateNameArabic--------" + data.getEmirateNameArabic());
        familyData.append(" \nEmirateNameEnglish--------" + data.getEmirateNameEnglish());
        familyData.append(" \nFamilyID--------" + data.getFamilyID());
        familyData.append(" \nFatherNameEnglish--------" + data.getFatherNameEnglish());
        familyData.append(" \nFirstNameArabic--------" + data.getFirstNameArabic());
        familyData.append(" \nFirstNameEnglish--------" + data.getFirstNameEnglish());
        familyData.append(" \nGrandFatherNameArabic--------" + data.getGrandFatherNameArabic());
        familyData.append(" \nGrandFatherNameEnglish--------" + data.getGrandFatherNameEnglish());
        familyData.append(" \nMotherFullNameArabic--------" + data.getMotherFullNameArabic());
        familyData.append(" \nMotherFullNameEnglish--------" + data.getMotherFullNameEnglish());
        familyData.append(" \nNationalityArabic--------" + data.getNationalityArabic());
        familyData.append(" \nNationalityEnglish--------" + data.getNationalityEnglish());
        familyData.append(" \nPlaceOfBirthArabic--------" + data.getPlaceOfBirthArabic());
        familyData.append(" \nPlaceOfBirthEnglish--------" + data.getPlaceOfBirthEnglish());
        familyData.append(" \nSex--------" + data.getGender());
        familyData.append(" \nTribeArabic--------" + data.getTribeArabic());
        familyData.append(" \nTribeEnglish--------" + data.getTribeEnglish());
        familyData.append(" \nHolderIDNumber--------" + data.getHolderIDNumber());

    }

    private void printChildData(Child data) {

        if(data == null){
            familyData.append("\n Child Data is not available.");
            return;
        }

        familyData.append(" \n-----------printChildData()---------------------|");

        familyData.append(" \nChildIDN--------" + data.getChildIDN());
        familyData.append(" \nDateOfBirth--------" + data.getDateOfBirth());
        familyData.append(" \nFirstNameArabic--------" + data.getFirstNameArabic());
        familyData.append(" \nFirstNameEnglish--------" + data.getFirstNameEnglish());
        familyData.append(" \nMotherFullNameArabic--------" + data.getMotherFullNameArabic());
        familyData.append(" \nMotherFullNameEnglish--------" + data.getMotherFullNameEnglish());
        familyData.append(" \nMotherIDN--------" + data.getMotherIDN());
        familyData.append(" \nPlaceOfBirth--------" + data.getPlaceOfBirthArabic());
        familyData.append(" \nPlaceOfBirthEnglish--------" + data.getPlaceOfBirthEnglish());
        familyData.append(" \nSex--------" + data.getGender());

    }

    private  void printWifeData(Wife data) {
        if(data == null){
            familyData.append("\n Wife Data is not available.");
            return;
        }

        familyData.append(" \n-----------printWifeData()---------------------|");

        familyData.append(" \nWifeIDN--------" + data.getWifeIDN());
        familyData.append(" \nFullNameArabic--------" + data.getFullNameArabic());
        familyData.append(" \nFullNameEnglish--------" + data.getFullNameEnglish());
        familyData.append(" \nNationalityCode--------" + data.getNationalityCode());
        familyData.append(" \nNationalityEnglish--------" + data.getNationalityEnglish());

    }




    @Override
    public void onDetach() {
        super.onDetach();
        readFamilyBookListener =  null;
    }

    @Override
    protected void appendError(String message) {
        txtStatus.appendLog(message , LogTextView.LOG_TYPE.ERROR);
    }
}//FamilyBookFragment()
