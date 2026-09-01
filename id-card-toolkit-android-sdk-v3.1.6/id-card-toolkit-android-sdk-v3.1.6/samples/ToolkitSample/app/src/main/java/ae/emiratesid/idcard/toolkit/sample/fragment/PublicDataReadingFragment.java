package ae.emiratesid.idcard.toolkit.sample.fragment;

import android.content.Context;
import android.nfc.Tag;
import android.os.Bundle;
import android.os.Environment;
import android.text.method.ScrollingMovementMethod;
import android.util.Base64;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.Toast;

import ae.emiratesid.idcard.toolkit.SignatureValidator;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardPublicData;
import ae.emiratesid.idcard.toolkit.datamodel.HomeAddress;
import ae.emiratesid.idcard.toolkit.datamodel.ModifiablePublicData;
import ae.emiratesid.idcard.toolkit.datamodel.NonModifiablePublicData;
import ae.emiratesid.idcard.toolkit.datamodel.WorkAddress;
import ae.emiratesid.idcard.toolkit.internal.ErrorCode;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.R;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.task.CardReaderConnectionTask;
import ae.emiratesid.idcard.toolkit.sample.task.ReaderCardDataAsync;
import ae.emiratesid.idcard.toolkit.sample.task.ReaderCardDataListener;
import ae.emiratesid.idcard.toolkit.sample.utils.Bitmaps;
import ae.emiratesid.idcard.toolkit.sample.widget.LogTextView;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.IOException;

import static ae.emiratesid.idcard.toolkit.sample.AppController.isReading;

import androidx.annotation.Nullable;

/**
 * to handle interaction events.
 * Use the {@link PublicDataReadingFragment#newInstance} factory method to
 * create an instance of this fragment.
 */
public class PublicDataReadingFragment extends BaseFragment implements View.OnClickListener {
    private ReaderCardDataAsync readerTask;
    // the fragment initialization parameters, e.g. ARG_ITEM_NUMBER
    private static boolean isNFCMode;
    private Tag tag;


    public PublicDataReadingFragment() {
        // Required empty public constructor
    }

    /**
     * Use this factory method to create a new instance of
     * this fragment using the provided parameters.
     *
     * @return A new instance of fragment PublicDataReadingFragment.
     */
    public static PublicDataReadingFragment newInstance(boolean isNFC) {
        PublicDataReadingFragment fragment = new PublicDataReadingFragment();
        isNFCMode = isNFC;
        return fragment;
    }

    public void setNfcMode(Tag tag) {
        this.tag = tag;
        isNFCMode = true;
        Logger.e("setNfcMode :: called");

        if (!isReading) {
            Logger.d("onResume :: enableForegroundDispatch called");
            txtStatus.setText("");
            //set the reading flag..
            isReading = true;

            //show the dialog to provide user interaction...

            //create the object of ReaderCardDataAsync
            if (tag == null) {
                Logger.e("setNfcMode :: tag is null");
                return;
            } else {
                showProgressDialog("Reading");
                Logger.e("setNfcMode :: calling read public data");
                readerTask = new ReaderCardDataAsync(readerCardDataListener, tag);
            }
            readerTask.execute();
        }//
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    //Using a formatted Custom  TextView  to show the result and logs;
    private LogTextView txtStatus;
    private Button btnResfersh;
    private ImageView imgPhtoto;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_public_data_reading, container, false);
        txtStatus = (LogTextView) view.findViewById(R.id.txtReadData);
        imgPhtoto = (ImageView) view.findViewById(R.id.imageView);
        // make the text view scrollable.
        txtStatus.setMovementMethod(new ScrollingMovementMethod());
        btnResfersh = (Button) view.findViewById(R.id.btn_refresh);
        btnResfersh.setVisibility(!(isNFCMode) ? View.VISIBLE : View.INVISIBLE);
        btnResfersh.setOnClickListener(this);
        return view;
    }

    @Override
    public void onViewCreated(View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
    }

    @Override
    public void onAttach(Context context) {
        super.onAttach(context);
    }

    @Override
    public void onDetach() {
        super.onDetach();
        connectToolkitListener = null;
        readerCardDataListener = null;
        publicDataString = null;
        isNFCMode = false;
    }//

    @Override
    protected void appendError(String message) {
        txtStatus.appendLog(message, LogTextView.LOG_TYPE.ERROR);
    }

    private StringBuilder publicDataString = new StringBuilder();

    private CardReaderConnectionTask.ConnectToolkitListener connectToolkitListener = new
            CardReaderConnectionTask.ConnectToolkitListener() {
                @Override
                public void onToolkitConnected(int status, boolean isConnectFlag, String message) {
                    if (!isConnectFlag) {
                        Toast.makeText(getActivity(), "Disconnected", Toast.LENGTH_SHORT).show();
                    }//
                }//onToolkitConnected()
            };//ConnectToolkitListener

    private ReaderCardDataListener readerCardDataListener = new ReaderCardDataListener() {
        @Override
        public void onCardReadComplete(int status, String message, CardPublicData cardPublicData) {
            //dismiss the dialog...
            hideProgressDialog();

            Logger.d("Reading finished with status " + status);
            isReading = false;
            if (status == Constants.SUCCESS && cardPublicData != null) {
                //Append the details to  the final string..
                publicDataString.append("\n\nCard Number ="
                        + cardPublicData.getCardNumber());
                publicDataString.append("\n\nID Number =" + cardPublicData.getIdNumber());
                try {
                    printHomeAddressData(cardPublicData.getHomeAddress());
                    printModifiableData(cardPublicData.getModifiablePublicData());
                    printNonModifiableData(cardPublicData.getNonModifiablePublicData());
                    printWorkAddressData(cardPublicData.getWorkAddress());
                    displayPhoto(cardPublicData);
                } catch (Exception e) {
                    e.printStackTrace();
                }
                txtStatus.appendLog(publicDataString.toString(), LogTextView.LOG_TYPE.SUCCESS);
            } else {
                if (status == ErrorCode.UNSPECIFIED.getCode()) {
                    txtStatus.setLog("Error in reading public data from card. \n" + message,
                            LogTextView.LOG_TYPE.ERROR);
                } else {
                    txtStatus.setLog("Error Code : " + status + "\nError in reading public data from card. \n" + message,
                            LogTextView.LOG_TYPE.ERROR);
                }

            }
            if (isNFCMode) {
                CardReaderConnectionTask cardReaderConnectionTask =
                        new CardReaderConnectionTask(connectToolkitListener, false);
                cardReaderConnectionTask.execute();
            }//if()
        }//onCardReadComplete

    };//readerCardDataListener;

    @Override
    public void onClick(View view) {
        if (view == btnResfersh) {

          /*  showProgressDialog("Loading");
            try
            {
                if(new SignatureValidator(readXMLDataFromFile("vg_response"),readDataFromFile("vg_signing_cert"), readDataFromFile("vg_signing_cert_chain")).validate() == 0)
                {
                    Logger.d("Validate Success");
                }
                else
                {
                    Logger.d("Validate Failed");
                }
                hideProgressDialog();
            }
            catch (ToolkitException e)
            {
                Logger.d("Validate Failed"+ e.getMessage());
                hideProgressDialog();
            }
*/
            if (!isReading) {
                txtStatus.setText("");
                //set the reading flag..
                isReading = true;

                //show the dialog to provide user interaction...
                showProgressDialog("Reading");

                //create the object of ReaderCardDataAsync
                if (tag == null) {
                    readerTask = new ReaderCardDataAsync(readerCardDataListener);
                } else {
                    readerTask = new ReaderCardDataAsync(readerCardDataListener, tag);
                }
                readerTask.execute();
            }//

        }//
    }//


    private void displayPhoto(CardPublicData cardPublicData) {
        if (cardPublicData == null) {
            return;
        }//if()
        byte[] photo = Base64.decode(cardPublicData.getCardHolderPhoto(), Base64.DEFAULT);
        if (photo == null || photo.length <= 0) {
            return;
        }//if()
        //Create  a bitmap.
        //set to the imageview
        imgPhtoto.setImageBitmap(Bitmaps.decodeSampledBitmapFromBytes(photo, 150, 150));
    }//

    /**
     * To print the card holder's Work Address data
     *
     * @param workAddressData
     * @throws Exception
     */
    private void printWorkAddressData(WorkAddress workAddressData) throws Exception {

        publicDataString.append("\n  #--------------------------  WorkAddressData  ------------------------------# ");

        if (null == workAddressData) {
            return;
        }

        publicDataString.append("\nAddress Type Code =" + workAddressData.getAddressTypeCode());

        publicDataString.append("\nArea Code =" + workAddressData.getAreaCode());

        publicDataString.append("\nEnglish Company Name =" + workAddressData.getCompanyNameEnglish());

        publicDataString.append("\n Arabic Company Name =" + workAddressData.getCompanyNameArabic());

        publicDataString.append("\n Area Desc Arabic =" + workAddressData.getAreaDescArabic());

        publicDataString.append("\n Area Desc English =" + workAddressData.getAreaDescEnglish());

        publicDataString.append("\n Building Name Arabic =" + workAddressData.getBuildingNameArabic());

        publicDataString.append("\n Building Name English =" + workAddressData.getBuildingNameEnglish());

        publicDataString.append("\n City Code =" + workAddressData.getCityCode());

        publicDataString.append("\n City Description English =" + workAddressData.getCityDescEnglish());

        publicDataString.append("\n City Description Arabic =" + workAddressData.getCityDescArabic());

        publicDataString.append("\n Email =" + workAddressData.getEmail());

        publicDataString.append("\n Emirates Code =" + workAddressData.getEmiratesCode());

        publicDataString.append("\n Emirates Description Arabic =" + workAddressData.getEmiratesDescArabic());

        publicDataString.append("\n Emirates Description English =" + workAddressData.getEmiratesDescEnglish());

        publicDataString.append("\n Location Code =" + workAddressData.getLocationCode());

        publicDataString.append("\n Mobile Phone No =" + workAddressData.getMobilePhoneNumber());

        publicDataString.append("\n PO-Box No =" + workAddressData.getPOBOX());

        publicDataString.append("\n Street Arabic =" + workAddressData.getStreetArabic());

        publicDataString.append("\n Street English =" + workAddressData.getStreetEnglish());

        publicDataString.append("\n Land Phone No =" + workAddressData.getLandPhoneNumber());

    }// readWorkAddressData()

    /**
     * To print the card holder's Home Address data
     *
     * @param homeAddressData
     * @throws Exception
     */
    private void printHomeAddressData(HomeAddress homeAddressData) throws Exception {

        publicDataString.append("\n  #--------------------------  HomeAddressData  ------------------------------# ");

        if (null == homeAddressData) {
            return;
        }

        // get field values and convert them in string format
        publicDataString.append("\n Address Type Code =" + homeAddressData.getAddressTypeCode());

        publicDataString.append("\n Area Code =" + homeAddressData.getAreaCode());

        publicDataString.append("\n Area Desc Arabic =" + homeAddressData.getAreaDescArabic());

        publicDataString.append("\n Area Desc English =" + homeAddressData.getAreaDescEnglish());

        publicDataString.append("\n Building Name Arabic =" + homeAddressData.getBuildingNameArabic());

        publicDataString.append("\n Building Name English =" + homeAddressData.getBuildingNameEnglish());

        publicDataString.append("\n City Code =" + homeAddressData.getCityCode());

        publicDataString.append("\n City Description Arabic =" + homeAddressData.getCityDescArabic());

        publicDataString.append("\n Email =" + homeAddressData.getEmail());

        publicDataString.append("\n Emirates Code =" + homeAddressData.getEmiratesCode());

        publicDataString.append("\n Emirates Description Arabic =" + homeAddressData.getEmiratesDescArabic());

        publicDataString.append("\n Emirates Description English =" + homeAddressData.getEmiratesDescEnglish());

        publicDataString.append("\n Flat no =" + homeAddressData.getFlatNo());

        publicDataString.append("\n Location Code =" + homeAddressData.getLocationCode());

        publicDataString.append("\n Mobile Phone No =" + homeAddressData.getMobilePhoneNumber());

        publicDataString.append("\n PO-Box No =" + homeAddressData.getPOBOX());

        publicDataString.append("\n Residence Phone No =" + homeAddressData.getResidentPhoneNumber());

        publicDataString.append("\n Street Arabic =" + homeAddressData.getStreetArabic());

        publicDataString.append("\n Street English =" + homeAddressData.getStreetEnglish());

    }// readHomeAddressData()

    /**
     * To print the card holder's modifiable data
     *
     * @param modifiableData
     * @throws Exception
     */

    private void printModifiableData(ModifiablePublicData modifiableData) throws Exception {

        publicDataString.append("\n  #--------------------------  ModifiableData  ------------------------------# ");

        if (null == modifiableData) {
            return;
        }

        publicDataString.append("\n Occupation =" + new String(modifiableData.getOccupationCode() + ""));

        publicDataString.append("\n Arabic Occupation =" + modifiableData.getOccupationArabic());

        publicDataString.append("\n English Occupation =" + modifiableData.getOccupationEnglish());

        publicDataString.append("\n English Family ID =" + modifiableData.getFamilyID());

        publicDataString.append("\n Arabic Occupation Type =" + modifiableData.getOccupationTypeArabic());

        publicDataString.append("\n English Occupation Type =" + modifiableData.getOccupationTypeEnglish());

        publicDataString.append("\n Occupation Field Code =" + modifiableData.getOccupationFieldCode());

        publicDataString.append("\n Arabic Company Name =" + modifiableData.getCompanyNameArabic());

        publicDataString.append("\n English Company Name =" + modifiableData.getCompanyNameEnglish());

        publicDataString.append("\n Marital Status Code =" + modifiableData.getMaritalStatusCode());

        publicDataString.append("\n Husband IDN =" + modifiableData.getHusbandIDN());

        publicDataString.append("\n Sponsor Type =" + modifiableData.getSponsorTypeCode());

        publicDataString.append("\n Sponsor Unified No =" + modifiableData.getSponsorUnifiedNumber());

        publicDataString.append("\n Sponsor Name =" + modifiableData.getSponsorName());

        publicDataString.append("\n Recidency Type =" + modifiableData.getResidencyTypeCode());

        publicDataString.append("\n Recidency No =" + modifiableData.getResidencyNumber());

        publicDataString.append("\n Residency Expiry Date =" + modifiableData.getResidencyExpiryDate());

        publicDataString.append("\n Passport Number =" + modifiableData.getPassportNumber());

        publicDataString.append("\n Passport Type =" + modifiableData.getPassportTypeCode());

        publicDataString.append("\n Passport Country =" + modifiableData.getPassportCountryCode());

        publicDataString.append("\n Arabic Passport Country Desc =" + modifiableData.getPassportCountryDescArabic());

        publicDataString.append("\n English Passport Country Desc =" + modifiableData.getPassportCountryDescEnglish());

        publicDataString.append("\n Passport Issue Data =" + modifiableData.getPassportIssueDate());

        publicDataString.append("\n Passport Expiry Date =" + modifiableData.getPassportExpiryDate());

        publicDataString.append("\n Qualification Level =" + modifiableData.getQualificationLevelCode());

        publicDataString.append("\n Arabic Qualification Level Desc =" + modifiableData.getQualificationLevelDescArabic());

        publicDataString.append("\n English Qualification Level Desc =" + modifiableData.getQualificationLevelDescEnglish());

        publicDataString.append("\n Arabic Degree Desc =" + modifiableData.getDegreeDescArabic());

        publicDataString.append("\n English Degree Desc =" + modifiableData.getDegreeDescEnglish());

        publicDataString.append("\n Arabic Field Of Study =" + modifiableData.getFieldOfStudyArabic());

        publicDataString.append("\n English Field Of Study =" + modifiableData.getFieldOfStudyEnglish());

        publicDataString.append("\n Arabic Place Of Study =" + modifiableData.getPlaceOfStudyArabic());

        publicDataString.append("\n English Place Of Study =" + modifiableData.getPlaceOfStudyEnglish());

        publicDataString.append("\n Graduation Date =" + modifiableData.getDateOfGraduation());

        publicDataString.append("\n Mother Full Name Arabic =" + modifiableData.getMotherFullNameArabic());

        publicDataString.append("\n Mother Full Name English =" + modifiableData.getMotherFullNameEnglish());
    }

    /**
     * To print the card holder's non-modifiable data
     *
     * @param nonModifiableData
     * @throws Exception
     */
    private void printNonModifiableData(NonModifiablePublicData nonModifiableData) throws Exception {

        publicDataString.append("\n  #--------------------------  NonModifiableData  ------------------------------# ");

        if (null == nonModifiableData) {
            return;
        }

        publicDataString.append("\n ID Type =" + nonModifiableData.getIDType());

        publicDataString.append("\n Issue Date =" + nonModifiableData.getIssueDate());

        publicDataString.append("\n Expiry Date =" + nonModifiableData.getExpiryDate());

        publicDataString.append("\n Arabic Title =" + nonModifiableData.getTitleArabic());

        publicDataString.append("\n Arabic Full Name =" + nonModifiableData.getFullNameArabic());

        publicDataString.append("\n Eng Title =" + nonModifiableData.getTitleEnglish());

        publicDataString.append("\n Full Name =" + nonModifiableData.getFullNameEnglish());

        publicDataString.append("\n Gender =" + nonModifiableData.getGender());

        publicDataString.append("\n Arabic Nationality =" + nonModifiableData.getNationalityArabic());

        publicDataString.append("\n English Nationality =" + nonModifiableData.getNationalityEnglish());

        publicDataString.append("\n Nationality =" + nonModifiableData.getNationalityCode());

        publicDataString.append("\n Date Of Birth =" + nonModifiableData.getDateOfBirth());

        publicDataString.append("\n Arabic Place Of Birth =" + nonModifiableData.getPlaceOfBirthArabic());

        publicDataString.append("\n English Place Of Birth =" + nonModifiableData.getPlaceOfBirthEnglish());
    }


    private byte[] readDataFromFile(String fileName) {
        String path = Environment.getExternalStorageDirectory().getAbsolutePath() + "/EIDAToolkit/";

        Logger.d("File Path is ::" + path);
        File file = new File(path + fileName);
        int size = (int) file.length();
        byte[] bytesDataFromFile = new byte[size];
        try {
            BufferedInputStream buf = new BufferedInputStream(new FileInputStream(file));
            buf.read(bytesDataFromFile, 0, bytesDataFromFile.length);
            buf.close();
            return bytesDataFromFile;
        } catch (FileNotFoundException e) {
            e.printStackTrace();
            return null;
        } catch (IOException e) {
            e.printStackTrace();
            return null;
        }
    }


    private String readXMLDataFromFile(String fileName) {
        String path = Environment.getExternalStorageDirectory().getAbsolutePath() + "/EIDAToolkit/";

        Logger.d("File Path is ::" + path);

        File file = new File(path + fileName);
        StringBuilder text = new StringBuilder();

        try {
            BufferedReader br = new BufferedReader(new FileReader(file));
            String line;

            while ((line = br.readLine()) != null) {
                text.append(line);
                text.append('\n');
            }
            br.close();

        } catch (IOException e) {
            //You'll need to add proper error handling here
        }
        return text.toString();
    }

}
