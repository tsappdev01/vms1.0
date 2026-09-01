package ae.emiratesid.idcard.toolkit.sample;

import android.app.Activity;
import android.app.Dialog;
import android.content.ActivityNotFoundException;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.util.Base64;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.ImageView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.AppCompatButton;
import androidx.core.content.FileProvider;

import com.google.android.material.textfield.TextInputEditText;

import java.io.File;
import java.io.FileOutputStream;

import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardPublicData;
import ae.emiratesid.idcard.toolkit.sample.toolkit_face.ToolkitFace;
import ae.emiratesid.idcard.toolkit.sample.utils.Bitmaps;

public class ProfileActivity extends AppCompatActivity {
    CardPublicData cardPublicData;

    TextInputEditText idNumber, cardNumber, issuingDate, name, nameArabic, dateOfBirth, cardExpiryDate,
            nationality, nationalityArabic;
    AppCompatButton emiratesIdImage, residanceIdImage;
    ImageView backBtn, user_photo;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_profile);

        idNumber = findViewById(R.id.id_number);
        cardNumber = findViewById(R.id.card_number);
        issuingDate = findViewById(R.id.issuing_date);
        name = findViewById(R.id.name);
        nameArabic = findViewById(R.id.name_arabic);
        dateOfBirth = findViewById(R.id.date_of_birth);
        cardExpiryDate = findViewById(R.id.card_expiry_date);
        nationality = findViewById(R.id.nationality);
        nationalityArabic = findViewById(R.id.nationality_arabic);
        backBtn = findViewById(R.id.back_btn);
        user_photo = findViewById(R.id.user_photo);
        emiratesIdImage = findViewById(R.id.emirates_id_image);
        residanceIdImage = findViewById(R.id.residance_id_image);

        emiratesIdImage.setOnClickListener(v -> {
            savePdfDocumentByType("EmiratesId.pdf", cardPublicData.getHolderEmiratesIdImage());
        });

        residanceIdImage.setOnClickListener(v -> {
            savePdfDocumentByType("ResidanceId.pdf", cardPublicData.getHolderResidenceImage());
        });
        backBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(ProfileActivity.this, HomeActivity.class);
                startActivity(intent);
                finish();
            }
        });

        try {
            cardPublicData = new CardPublicData(ToolkitFace.getCardPublicData().toXmlString(),
                    ToolkitFace.getCardPublicData().toXmlString());
            setData(cardPublicData);
        } catch (ToolkitException e) {
            throw new RuntimeException(e);
        }
    }

    private StringBuilder publicDataString = new StringBuilder();

    private void setData(CardPublicData cardPublicData) {
        //Append the details to  the final string..
        publicDataString.append("\n\nCard Number ="
                + cardPublicData.getCardNumber());
        publicDataString.append("\n\nID Number =" + cardPublicData.getIdNumber());
        try {
            // set text data
            idNumber.setText(cardPublicData.getIdNumber());
            cardNumber.setText(cardPublicData.getCardNumber());
            issuingDate.setText(cardPublicData.getNonModifiablePublicData().getIssueDate());
            name.setText(cardPublicData.getNonModifiablePublicData().getFullNameEnglish());
            nameArabic.setText(cardPublicData.getNonModifiablePublicData().getFullNameArabic());
            dateOfBirth.setText(cardPublicData.getNonModifiablePublicData().getDateOfBirth());
            cardExpiryDate.setText(cardPublicData.getNonModifiablePublicData().getExpiryDate());
            nationality.setText(cardPublicData.getNonModifiablePublicData().getNationalityEnglish());
            nationalityArabic.setText(cardPublicData.getNonModifiablePublicData().getNationalityArabic());
            displayPhoto(cardPublicData);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }


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
        user_photo.setImageBitmap(Bitmaps.decodeSampledBitmapFromBytes(photo, 150, 150));
    }//


    private void savePdfDocumentByType(String fileName, String base64) {
        File mDirectory = new File(getPath());
        File files = new File(mDirectory, fileName);
        try (FileOutputStream fos = new FileOutputStream(files)) {

            byte[] pdfAsBytes = Base64.decode(base64, 0);
            FileOutputStream os;
            os = new FileOutputStream(files, false);
            os.write(pdfAsBytes);
            os.flush();
            os.close();

            openIDDocuments(ProfileActivity.this, fileName);

        } catch (Exception e) {
            e.printStackTrace();

        }
    }


    private String getPath() {
        return Environment.getExternalStorageDirectory().getAbsolutePath() + "/EIDAToolkit/";
    }

    public void openIDDocuments(Activity activity, String fileName) {
        File mDirectory = new File(getPath());
        File file = new File(mDirectory, fileName);
        Intent target = new Intent(Intent.ACTION_VIEW);
        target.setFlags(Intent.FLAG_ACTIVITY_NO_HISTORY);

        Uri dirUri = FileProvider.getUriForFile(activity,
                activity.getApplicationContext().getPackageName() + ".provider",
                file);
        target.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);

        target.setDataAndType(dirUri, "application/pdf");

        Intent intent = Intent.createChooser(target, "Open File");

        try {
            activity.startActivity(intent);
        } catch (ActivityNotFoundException e) {
            Toast.makeText(activity, "Please install PDF Reader to open file.", Toast.LENGTH_SHORT).show();
        }
    }
}