package ae.emiratesid.idcard.toolkit.sample.features;

import java.util.List;
import java.util.Scanner;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardFamilyBookData;
import ae.emiratesid.idcard.toolkit.datamodel.Child;
import ae.emiratesid.idcard.toolkit.datamodel.HeadOfFamily;
import ae.emiratesid.idcard.toolkit.datamodel.Wife;
import ae.emiratesid.idcard.toolkit.sample.BaseFeature;
import ae.emiratesid.idcard.toolkit.sample.ToolkitSession;

/**
 * Reads family book data from the Emirates ID card.
 * Requires PIN authentication to access protected data.
 */
public class ReadFamilyBookFeature extends BaseFeature {

    public ReadFamilyBookFeature(ToolkitSession session, Scanner scanner) {
        super(session, scanner);
    }

    /**
     * Reads the family book from the card. Asks for PIN, encrypts it,
     * reads family book data, and prints head of family, wives, and children.
     *
     * @throws ToolkitException if the toolkit operation fails
     */
    public void readFamilyBook() throws ToolkitException {
        CardReader cardReader = session.getCardReader();

        String pin = readInput("Enter PIN: ");
        String encodedPin = prepareAndEncryptPin(pin);

        CardFamilyBookData familyBookData = cardReader.readFamilyBook(encodedPin);
        if (familyBookData == null) {
            System.out.println("Family book data is not available.");
            return;
        }

        System.out.println("\n#--------------------------  Family Book  ------------------------------#");

        printHeadOfFamily(familyBookData.getHeadOfFamily());

        List<Wife> wives = familyBookData.getWives();
        if (wives != null && !wives.isEmpty()) {
            for (int i = 0; i < wives.size(); i++) {
                printWife(wives.get(i), i + 1);
            }
        }
        else {
            System.out.println("\nNo wife data found.");
        }

        List<Child> children = familyBookData.getChildren();
        if (children != null && !children.isEmpty()) {
            for (int i = 0; i < children.size(); i++) {
                printChild(children.get(i), i + 1);
            }
        }
        else {
            System.out.println("\nNo child data found.");
        }
    }

    // ---- Private helpers ----

    private void printHeadOfFamily(HeadOfFamily data) {
        if (data == null) {
            System.out.println("\nHead of family data is not available.");
            return;
        }

        System.out.println("\n--- Head of Family ---");
        System.out.println("Holder ID Number :: " + data.getHolderIDNumber());
        System.out.println("Family ID :: " + data.getFamilyID());
        System.out.println("First Name Arabic :: " + data.getFirstNameArabic());
        System.out.println("First Name English :: " + data.getFirstNameEnglish());
        System.out.println("Father Name English :: " + data.getFatherNameEnglish());
        System.out.println("Grand Father Name Arabic :: " + data.getGrandFatherNameArabic());
        System.out.println("Grand Father Name English :: " + data.getGrandFatherNameEnglish());
        System.out.println("Clan Arabic :: " + data.getClanArabic());
        System.out.println("Clan English :: " + data.getClanEnglish());
        System.out.println("Tribe Arabic :: " + data.getTribeArabic());
        System.out.println("Tribe English :: " + data.getTribeEnglish());
        System.out.println("Gender :: " + data.getGender());
        System.out.println("Date of Birth :: " + data.getDateOfBirth());
        System.out.println("Nationality Arabic :: " + data.getNationalityArabic());
        System.out.println("Nationality English :: " + data.getNationalityEnglish());
        System.out.println("Emirate Name Arabic :: " + data.getEmirateNameArabic());
        System.out.println("Emirate Name English :: " + data.getEmirateNameEnglish());
        System.out.println("Place of Birth Arabic :: " + data.getPlaceOfBirthArabic());
        System.out.println("Place of Birth English :: " + data.getPlaceOfBirthEnglish());
        System.out.println("Mother Full Name Arabic :: " + data.getMotherFullNameArabic());
        System.out.println("Mother Full Name English :: " + data.getMotherFullNameEnglish());
    }

    private void printWife(Wife data, int index) {
        if (data == null) {
            System.out.println("\nWife " + index + " data is not available.");
            return;
        }

        System.out.println("\n--- Wife " + index + " ---");
        System.out.println("Wife IDN :: " + data.getWifeIDN());
        System.out.println("Full Name Arabic :: " + data.getFullNameArabic());
        System.out.println("Full Name English :: " + data.getFullNameEnglish());
        System.out.println("Nationality Code :: " + data.getNationalityCode());
        System.out.println("Nationality English :: " + data.getNationalityEnglish());
    }

    private void printChild(Child data, int index) {
        if (data == null) {
            System.out.println("\nChild " + index + " data is not available.");
            return;
        }

        System.out.println("\n--- Child " + index + " ---");
        System.out.println("Child IDN :: " + data.getChildIDN());
        System.out.println("First Name Arabic :: " + data.getFirstNameArabic());
        System.out.println("First Name English :: " + data.getFirstNameEnglish());
        System.out.println("Gender :: " + data.getGender());
        System.out.println("Date of Birth :: " + data.getDateOfBirth());
        System.out.println("Place of Birth Arabic :: " + data.getPlaceOfBirthArabic());
        System.out.println("Place of Birth English :: " + data.getPlaceOfBirthEnglish());
        System.out.println("Mother Full Name Arabic :: " + data.getMotherFullNameArabic());
        System.out.println("Mother Full Name English :: " + data.getMotherFullNameEnglish());
        System.out.println("Mother IDN :: " + data.getMotherIDN());
    }
}
