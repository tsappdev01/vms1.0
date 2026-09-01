package ae.emiratesid.idcard.toolkit.sample;

import java.io.File;
import java.util.Scanner;

import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.sample.features.BiometricFeature;
import ae.emiratesid.idcard.toolkit.sample.features.CardInfoFeature;
import ae.emiratesid.idcard.toolkit.sample.features.ConfigFeature;
import ae.emiratesid.idcard.toolkit.sample.features.DeviceFeature;
import ae.emiratesid.idcard.toolkit.sample.features.DigitalSignatureFeature;
import ae.emiratesid.idcard.toolkit.sample.features.PKIFeature;
import ae.emiratesid.idcard.toolkit.sample.features.ReadFamilyBookFeature;
import ae.emiratesid.idcard.toolkit.sample.features.ReadPublicDataFeature;

public class Application {

    private final Scanner scanner = new Scanner(System.in);
    private final ToolkitSession session = new ToolkitSession();

    // Feature instances
    private ReadPublicDataFeature publicData;
    private ReadFamilyBookFeature familyBook;
    private PKIFeature pki;
    private BiometricFeature biometric;
    private DigitalSignatureFeature dsig;
    private CardInfoFeature cardInfo;
    private DeviceFeature device;
    private ConfigFeature config;

    public static void main(String[] args) {
        new Application().run();
    }

    private void run() {
        try {
            initialize();
            menuLoop();
        }
        catch (Exception e) {
            System.err.println("Fatal: " + e.getMessage());
            e.printStackTrace();
        }
        finally {
            session.cleanup();
            scanner.close();
        }
    }

    private void initialize() throws Exception {
        // Auto-detect config_ap in current directory, or ask the user
        String cwdPath = System.getProperty("user.dir");
        String configDir;
        if (new File(cwdPath, "config_ap").exists()) {
            configDir = cwdPath;
        }
        else {
            System.out.print("Enter config directory path: ");
            configDir = scanner.nextLine().trim();
        }

        StringBuilder configBuilder = new StringBuilder();
        configBuilder.append("config_directory = ").append(configDir).append("\n");
        configBuilder.append("log_directory = ").append(configDir).append("\n");

        System.out.print("Enter VG URL (or press Enter to skip): ");
        String vgUrl = scanner.nextLine().trim();
        if (!vgUrl.isEmpty()) {
            configBuilder.append("vg_url = ").append(vgUrl).append("\n");
        }

        session.initialize(configBuilder.toString());
        session.selectReader();
        System.out.println("Reader: " + session.getCardReader().getReaderName());
        session.ensureConnected();

        // Create feature instances
        publicData = new ReadPublicDataFeature(session, scanner);
        familyBook = new ReadFamilyBookFeature(session, scanner);
        pki = new PKIFeature(session, scanner);
        biometric = new BiometricFeature(session, scanner);
        dsig = new DigitalSignatureFeature(session, scanner);
        cardInfo = new CardInfoFeature(session, scanner);
        device = new DeviceFeature(session, scanner);
        config = new ConfigFeature(session, scanner);
    }

    private void menuLoop() {
        while (true) {
            try {
                session.ensureConnected();
            }
            catch (ToolkitException e) {
                System.err.println("Reconnection failed: " + e.getMessage());
                break;
            }

            printMenu();
            int choice = readMenuChoice();
            if (choice == 0) {
                break;
            }

            try {
                executeChoice(choice);
            }
            catch (ToolkitException e) {
                System.err.println("Error [" + e.getCode() + "]: " + e.getMessage());
                if (e.getExceptionType() == ToolkitException.ExceptionType.CARD_PIN_ERROR) {
                    System.err.println("PIN attempts left: " + e.getAttemptsLeft());
                }
            }
            catch (Exception e) {
                System.err.println("Error: " + e.getMessage());
            }
        }
    }

    private void executeChoice(int choice) throws Exception {
        switch (choice) {
            case 1:  publicData.readPublicData(); break;
            case 2:  pki.authenticatePki(); break;
            case 3:  pki.getCertificates(); break;
            case 4:  pki.signData(); break;
            case 5:  pki.checkCardStatus(); break;
            case 6:  biometric.authenticateBiometricOnServer(); break;
            case 7:  biometric.resetPin(); break;
            case 8:  biometric.unblockPin(); break;
            case 9:  dsig.signXml(); break;
            case 10: dsig.signPdf(); break;
            case 11: dsig.signDocument(); break;
            case 12: dsig.verifyXml(); break;
            case 13: dsig.verifyPdf(); break;
            case 14: dsig.verifyDocument(); break;
            case 15: cardInfo.getInterfaceType(); break;
            case 16: device.getDeviceId(); break;
            case 17: device.getToolkitVersion(); break;
            case 18: device.registerDevice(); break;
            case 19: familyBook.readFamilyBook(); break;
            case 20: cardInfo.validateSignature(); break;
            case 21: cardInfo.parseMRZ(); break;
            case 22: biometric.authenticateCardAndBiometric(); break;
            case 23: cardInfo.readPublicDataEF(); break;
            case 24: cardInfo.getCardSerialNumber(); break;
            case 25: config.getLicenseExpiryDate(); break;
            case 26: config.getConfigExpiryDates(); break;
            case 27: cardInfo.getCardVersion(); break;
            case 28: pki.authenticatePin(); break;
            case 29: publicData.readHealthData(); break;
            default: System.out.println("Invalid choice.");
        }
    }

    private void printMenu() {
        System.out.println();
        System.out.println("=== Emirates ID Card Toolkit Sample ===");
        System.out.println();
        System.out.println("--- Public Data ---");
        System.out.println("  1: Read Public Data");
        System.out.println(" 29: Read Health Data");
        System.out.println();
        System.out.println("--- PKI ---");
        System.out.println("  2: Authenticate PKI");
        System.out.println("  3: Get Certificates");
        System.out.println("  4: Sign Data");
        System.out.println("  5: Check Card Status");
        System.out.println(" 28: Authenticate PIN");
        System.out.println();
        System.out.println("--- Biometric ---");
        System.out.println("  6: Authenticate Biometric on Server");
        System.out.println("  7: Reset PIN");
        System.out.println("  8: Unblock PIN");
        System.out.println(" 22: Authenticate Card and Biometric");
        System.out.println();
        System.out.println("--- Digital Signature ---");
        System.out.println("  9: Sign XML (XAdES)");
        System.out.println(" 10: Sign PDF (PAdES)");
        System.out.println(" 11: Sign Document (CAdES)");
        System.out.println(" 12: Verify Signed XML");
        System.out.println(" 13: Verify Signed PDF");
        System.out.println(" 14: Verify Signed Document");
        System.out.println();
        System.out.println("--- Card Info ---");
        System.out.println(" 15: Get Interface Type");
        System.out.println(" 23: Read Public Data EF");
        System.out.println(" 24: Card Serial Number");
        System.out.println(" 27: Card Version");
        System.out.println(" 20: Validate Signature");
        System.out.println(" 21: Parse MRZ");
        System.out.println();
        System.out.println("--- Device & Config ---");
        System.out.println(" 16: Device ID");
        System.out.println(" 17: Toolkit Version");
        System.out.println(" 18: Register Device");
        System.out.println(" 25: License Expiry Date");
        System.out.println(" 26: Config Expiry Dates");
        System.out.println();
        System.out.println("--- Family Book ---");
        System.out.println(" 19: Read Family Book");
        System.out.println();
        System.out.println("  0: Exit");
        System.out.println();
    }

    private int readMenuChoice() {
        System.out.print("Enter your choice: ");
        while (!scanner.hasNextInt()) {
            scanner.next();
            System.out.print("Invalid input. Enter your choice: ");
        }

        int choice = scanner.nextInt();
        scanner.nextLine(); // consume newline
        return choice;
    }
}
