package ae.emiratesid.idcard.toolkit.sample.features;

import java.util.List;
import java.util.Scanner;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardPublicData;
import ae.emiratesid.idcard.toolkit.datamodel.DataContainer;
import ae.emiratesid.idcard.toolkit.datamodel.HomeAddress;
import ae.emiratesid.idcard.toolkit.datamodel.ModifiablePublicData;
import ae.emiratesid.idcard.toolkit.datamodel.NonModifiablePublicData;
import ae.emiratesid.idcard.toolkit.datamodel.ResponseStatus;
import ae.emiratesid.idcard.toolkit.datamodel.WorkAddress;
import ae.emiratesid.idcard.toolkit.health.Allergy;
import ae.emiratesid.idcard.toolkit.health.BloodGroup;
import ae.emiratesid.idcard.toolkit.health.Diagnosis;
import ae.emiratesid.idcard.toolkit.health.HealthData;
import ae.emiratesid.idcard.toolkit.health.Insurance;
import ae.emiratesid.idcard.toolkit.sample.BaseFeature;
import ae.emiratesid.idcard.toolkit.sample.CryptoHelper;
import ae.emiratesid.idcard.toolkit.sample.ToolkitSession;

/**
 * Reads public data and health data from the Emirates ID card.
 */
public class ReadPublicDataFeature extends BaseFeature {

    public ReadPublicDataFeature(ToolkitSession session, Scanner scanner) {
        super(session, scanner);
    }

    /**
     * Reads all public data from the card: non-modifiable, modifiable, photo,
     * signature image, and address data.
     *
     * @throws ToolkitException if the toolkit operation fails
     */
    public void readPublicData() throws ToolkitException {
        CardReader cardReader = session.getCardReader();
        String requestId = CryptoHelper.generateRequestId();

        CardPublicData data = cardReader.readPublicData(requestId, true, true, true, true, true);
        if (data == null || data.getResponseStatus() != ResponseStatus.SUCCESS) {
            throw new ToolkitException("Public data reading failed");
        }

        System.out.println("\n#--------------------------  ReadPublicData  ------------------------------#");
        System.out.println("ID Number :: " + data.getIdNumber());
        System.out.println("Card Number :: " + data.getCardNumber());
        System.out.println("Card Serial Number :: " + data.getCardSerialNumber());

        if (data.getNonModifiablePublicData() != null) {
            printNonModifiableData(data.getNonModifiablePublicData());
        }

        if (data.getModifiablePublicData() != null) {
            printModifiableData(data.getModifiablePublicData());
        }

        if (data.getHomeAddress() != null) {
            printHomeAddress(data.getHomeAddress());
        }

        if (data.getWorkAddress() != null) {
            printWorkAddress(data.getWorkAddress());
        }

        String photo = data.getCardHolderPhoto();
        if (photo != null && !photo.isEmpty()) {
            System.out.println("\n#--------------------------  Photo  ------------------------------#");
            System.out.println("Photo (Base64): " + photo);
        }

        String signatureImage = data.getHolderSignatureImage();
        if (signatureImage != null && !signatureImage.isEmpty()) {
            System.out.println("\n#--------------------------  Signature Image  ------------------------------#");
            System.out.println("Signature image available (TIFF format, Base64-encoded)");
        }
    }

    /**
     * Reads health data from the card: allergies, diagnoses, blood group,
     * insurance, and organ donor status.
     *
     * @throws ToolkitException if the toolkit operation fails
     */
    public void readHealthData() throws ToolkitException {
        CardReader cardReader = session.getCardReader();
        String requestId = CryptoHelper.generateRequestId();

        DataContainer dataContainer;
        try {
            dataContainer = cardReader.readData(requestId, 0);
        }
        catch (Exception e) {
            throw new ToolkitException("Health data reading failed: " + e.getMessage());
        }

        if (dataContainer == null) {
            System.out.println("No health data available on this card.");
            return;
        }

        HealthData healthData = dataContainer.getHealthData();
        if (healthData == null) {
            System.out.println("No health data available on this card.");
            return;
        }

        System.out.println("\n#--------------------------  Health Data  ------------------------------#");

        List<Allergy> allergies = healthData.getAllergy();
        if (allergies != null && !allergies.isEmpty()) {
            int i = 1;
            for (Allergy allergy : allergies) {
                System.out.println("\n--- Allergy " + i + " ---");
                System.out.println("Allergy Display :: " + allergy.getAllergyDisplay());
                System.out.println("Allergy Recorded Date :: " + allergy.getAllergyRecordedDate());
                i++;
            }
        }
        else {
            System.out.println("No allergy data found.");
        }

        List<Diagnosis> diagnoses = healthData.getDiagnosis();
        if (diagnoses != null && !diagnoses.isEmpty()) {
            int i = 1;
            for (Diagnosis diagnosis : diagnoses) {
                System.out.println("\n--- Diagnosis " + i + " ---");
                System.out.println("Diagnosis Code :: " + diagnosis.getDiagnosisCode());
                System.out.println("Diagnosis Description :: " + diagnosis.getDiagnosisDescription());
                System.out.println("Diagnosis Recorded Date :: " + diagnosis.getDiagnosisRecordedDate());
                i++;
            }
        }
        else {
            System.out.println("No diagnosis data found.");
        }

        BloodGroup bloodGroup = healthData.getBloodGroup();
        if (bloodGroup != null) {
            System.out.println("\n--- Blood Group ---");
            System.out.println("Blood Group :: " + bloodGroup.getBloodGroup());
            System.out.println("Recorded Date :: " + bloodGroup.getRecordedDate());
        }

        Insurance insurance = healthData.getInsurance();
        if (insurance != null) {
            System.out.println("\n--- Insurance ---");
            System.out.println("Insurance Name :: " + insurance.getInsuranceName());
            System.out.println("Insurance Number :: " + insurance.getInsuranceNumber());
            System.out.println("Validity Start Date :: " + insurance.getInsuranceValidityStartDate());
            System.out.println("Validity End Date :: " + insurance.getInsuranceValidityEndDate());
        }

        String organDonor = healthData.getOrganDonor();
        if (organDonor != null && !organDonor.isEmpty()) {
            System.out.println("\n--- Organ Donor ---");
            System.out.println("Organ Donor :: " + organDonor);
        }
    }

    // ---- Private helpers ----

    private void printNonModifiableData(NonModifiablePublicData data) {
        System.out.println("\n#--------------------------  NonModifiableData  ------------------------------#");
        System.out.println("ID Type :: " + data.getIDType());
        System.out.println("Issue Date :: " + data.getIssueDate());
        System.out.println("Expiry Date :: " + data.getExpiryDate());
        System.out.println("Title English :: " + data.getTitleEnglish());
        System.out.println("Title Arabic :: " + data.getTitleArabic());
        System.out.println("Full Name English :: " + data.getFullNameEnglish());
        System.out.println("Full Name Arabic :: " + data.getFullNameArabic());
        System.out.println("Gender :: " + data.getGender());
        System.out.println("Nationality English :: " + data.getNationalityEnglish());
        System.out.println("Nationality Arabic :: " + data.getNationalityArabic());
        System.out.println("Nationality Code :: " + data.getNationalityCode());
        System.out.println("Date of Birth :: " + data.getDateOfBirth());
        System.out.println("Place of Birth English :: " + data.getPlaceOfBirthEnglish());
        System.out.println("Place of Birth Arabic :: " + data.getPlaceOfBirthArabic());
    }

    private void printModifiableData(ModifiablePublicData data) {
        System.out.println("\n#--------------------------  ModifiableData  ------------------------------#");
        System.out.println("Occupation Code :: " + data.getOccupationCode());
        System.out.println("Occupation Arabic :: " + data.getOccupationArabic());
        System.out.println("Occupation English :: " + data.getOccupationEnglish());
        System.out.println("Family ID :: " + data.getFamilyID());
        System.out.println("Occupation Type Arabic :: " + data.getOccupationTypeArabic());
        System.out.println("Occupation Type English :: " + data.getOccupationTypeEnglish());
        System.out.println("Occupation Field Code :: " + data.getOccupationFieldCode());
        System.out.println("Company Name Arabic :: " + data.getCompanyNameArabic());
        System.out.println("Company Name English :: " + data.getCompanyNameEnglish());
        System.out.println("Marital Status Code :: " + data.getMaritalStatusCode());
        System.out.println("Husband IDN :: " + data.getHusbandIDN());
        System.out.println("Sponsor Type :: " + data.getSponsorTypeCode());
        System.out.println("Sponsor Unified No :: " + data.getSponsorUnifiedNumber());
        System.out.println("Sponsor Name :: " + data.getSponsorName());
        System.out.println("Residency Type :: " + data.getResidencyTypeCode());
        System.out.println("Residency No :: " + data.getResidencyNumber());
        System.out.println("Residency Expiry Date :: " + data.getResidencyExpiryDate());
        System.out.println("Passport Number :: " + data.getPassportNumber());
        System.out.println("Passport Type Code :: " + data.getPassportTypeCode());
        System.out.println("Passport Country Code :: " + data.getPassportCountryCode());
        System.out.println("Passport Country Desc Arabic :: " + data.getPassportCountryDescArabic());
        System.out.println("Passport Country Desc English :: " + data.getPassportCountryDescEnglish());
        System.out.println("Passport Issue Date :: " + data.getPassportIssueDate());
        System.out.println("Passport Expiry Date :: " + data.getPassportExpiryDate());
        System.out.println("Qualification Level :: " + data.getQualificationLevelCode());
        System.out.println("Qualification Level Desc Arabic :: " + data.getQualificationLevelDescArabic());
        System.out.println("Qualification Level Desc English :: " + data.getQualificationLevelDescEnglish());
        System.out.println("Degree Desc Arabic :: " + data.getDegreeDescArabic());
        System.out.println("Degree Desc English :: " + data.getDegreeDescEnglish());
        System.out.println("Field of Study Arabic :: " + data.getFieldOfStudyArabic());
        System.out.println("Field of Study English :: " + data.getFieldOfStudyEnglish());
        System.out.println("Place of Study Arabic :: " + data.getPlaceOfStudyArabic());
        System.out.println("Place of Study English :: " + data.getPlaceOfStudyEnglish());
        System.out.println("Graduation Date :: " + data.getDateOfGraduation());
        System.out.println("Mother Full Name Arabic :: " + data.getMotherFullNameArabic());
        System.out.println("Mother Full Name English :: " + data.getMotherFullNameEnglish());
    }

    private void printHomeAddress(HomeAddress data) {
        System.out.println("\n#--------------------------  HomeAddress  ------------------------------#");
        System.out.println("Address Type Code :: " + data.getAddressTypeCode());
        System.out.println("Location Code :: " + data.getLocationCode());
        System.out.println("Area Code :: " + data.getAreaCode());
        System.out.println("Area Desc Arabic :: " + data.getAreaDescArabic());
        System.out.println("Area Desc English :: " + data.getAreaDescEnglish());
        System.out.println("Building Name Arabic :: " + data.getBuildingNameArabic());
        System.out.println("Building Name English :: " + data.getBuildingNameEnglish());
        System.out.println("City Code :: " + data.getCityCode());
        System.out.println("City Desc Arabic :: " + data.getCityDescArabic());
        System.out.println("City Desc English :: " + data.getCityDescEnglish());
        System.out.println("Email :: " + data.getEmail());
        System.out.println("Emirates Code :: " + data.getEmiratesCode());
        System.out.println("Emirates Desc Arabic :: " + data.getEmiratesDescArabic());
        System.out.println("Emirates Desc English :: " + data.getEmiratesDescEnglish());
        System.out.println("Flat No :: " + data.getFlatNo());
        System.out.println("Mobile Phone No :: " + data.getMobilePhoneNumber());
        System.out.println("PO Box :: " + data.getPOBOX());
        System.out.println("Residence Phone No :: " + data.getResidentPhoneNumber());
        System.out.println("Street Arabic :: " + data.getStreetArabic());
        System.out.println("Street English :: " + data.getStreetEnglish());
    }

    private void printWorkAddress(WorkAddress data) {
        System.out.println("\n#--------------------------  WorkAddress  ------------------------------#");
        System.out.println("Address Type Code :: " + data.getAddressTypeCode());
        System.out.println("Area Code :: " + data.getAreaCode());
        System.out.println("Company Name English :: " + data.getCompanyNameEnglish());
        System.out.println("Company Name Arabic :: " + data.getCompanyNameArabic());
        System.out.println("Area Desc Arabic :: " + data.getAreaDescArabic());
        System.out.println("Area Desc English :: " + data.getAreaDescEnglish());
        System.out.println("Building Name Arabic :: " + data.getBuildingNameArabic());
        System.out.println("Building Name English :: " + data.getBuildingNameEnglish());
        System.out.println("City Code :: " + data.getCityCode());
        System.out.println("City Desc English :: " + data.getCityDescEnglish());
        System.out.println("City Desc Arabic :: " + data.getCityDescArabic());
        System.out.println("Email :: " + data.getEmail());
        System.out.println("Emirates Code :: " + data.getEmiratesCode());
        System.out.println("Emirates Desc Arabic :: " + data.getEmiratesDescArabic());
        System.out.println("Emirates Desc English :: " + data.getEmiratesDescEnglish());
        System.out.println("Location Code :: " + data.getLocationCode());
        System.out.println("Mobile Phone No :: " + data.getMobilePhoneNumber());
        System.out.println("PO Box :: " + data.getPOBOX());
        System.out.println("Street Arabic :: " + data.getStreetArabic());
        System.out.println("Street English :: " + data.getStreetEnglish());
        System.out.println("Land Phone No :: " + data.getLandPhoneNumber());
    }
}
