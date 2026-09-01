package ae.emiratesid.idcard.toolkit.sample.features;

import java.io.File;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.Scanner;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.SignatureValidator;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.MRZData;
import ae.emiratesid.idcard.toolkit.sample.BaseFeature;
import ae.emiratesid.idcard.toolkit.sample.ToolkitSession;

/**
 * Card information operations: interface type, serial number, card version,
 * EF data reading, MRZ parsing, and VG response signature validation.
 */
public class CardInfoFeature extends BaseFeature {

    private static final String[] EF_TYPE_NAMES = {
        null,               // index 0 unused
        "IDN CN",           // 1
        "ROOT_CERTIFICATE", // 2
        "NON_MODIFIABLE_DATA", // 3
        "MODIFIABLE_DATA",  // 4
        "PHOTOGRAPHY",      // 5
        "SIGNATURE_IMAGE",  // 6
        "HOME_ADDRESS",     // 7
        "WORK_ADDRESS"      // 8
    };

    public CardInfoFeature(ToolkitSession session, Scanner scanner) {
        super(session, scanner);
    }

    /**
     * Prints the card interface type (1 = contact, 2 = NFC).
     */
    public void getInterfaceType() throws Exception {
        CardReader cardReader = session.getCardReader();
        int type = cardReader.getInterfaceType();
        System.out.println("Interface Type: " + type);
    }

    /**
     * Prints the card serial number.
     */
    public void getCardSerialNumber() throws Exception {
        CardReader cardReader = session.getCardReader();
        System.out.println("Card Serial Number: " + cardReader.getCardSerialNumber());
    }

    /**
     * Prints the card version.
     */
    public void getCardVersion() throws Exception {
        CardReader cardReader = session.getCardReader();
        System.out.println("Card Version: " + cardReader.getCardVersion());
    }

    /**
     * Reads a raw EF (Elementary File) from the card.
     * Prompts for the EF type (1-8), reads the raw bytes, converts to hex,
     * then parses and prints the structured data.
     */
    public void readPublicDataEF() throws Exception {
        System.out.println("\nPublic EF Types:");
        System.out.println("  1: IDN CN");
        System.out.println("  2: ROOT_CERTIFICATE");
        System.out.println("  3: NON_MODIFIABLE_DATA");
        System.out.println("  4: MODIFIABLE_DATA");
        System.out.println("  5: PHOTOGRAPHY");
        System.out.println("  6: SIGNATURE_IMAGE");
        System.out.println("  7: HOME_ADDRESS");
        System.out.println("  8: WORK_ADDRESS");

        int efChoice = readInt("Enter EF type (1-8): ");
        if (efChoice < 1 || efChoice > 8) {
            System.out.println("Invalid EF type.");
            return;
        }

        CardReader cardReader = session.getCardReader();
        byte[] efData = cardReader.readPublicDataEF(efChoice, true);

        // Print raw hex
        StringBuilder hex = new StringBuilder(efData.length * 2);
        for (byte b : efData) {
            hex.append(String.format("%02x", b));
        }

        String typeName = EF_TYPE_NAMES[efChoice];
        System.out.println(typeName + " (hex): " + hex.toString());

        // Parse and print structured data
        String parsedData = CardReader.parseEFData(efData);
        System.out.println(typeName + " (parsed): " + parsedData);
    }

    /**
     * Parses a Machine Readable Zone (MRZ) string and prints all fields.
     */
    public void parseMRZ() throws Exception {
        String mrzString = readInput("Enter MRZ string: ");
        if (mrzString.isEmpty()) {
            System.out.println("MRZ string must not be empty.");
            return;
        }

        MRZData mrzData = session.getToolkit().parseMRZ(mrzString);

        System.out.println("\n--- MRZ Data ---");
        System.out.println("Full Name       : " + mrzData.getFullName());
        System.out.println("Date of Birth   : " + mrzData.getDateOfBirth());
        System.out.println("Card Expiry Date: " + mrzData.getCardExpiryDate());
        System.out.println("Card Number     : " + mrzData.getCardNumber());
        System.out.println("Document Type   : " + mrzData.getDocumentType());
        System.out.println("Gender          : " + mrzData.getGender());
        System.out.println("ID Number       : " + mrzData.getIDNumber());
        System.out.println("Nationality     : " + mrzData.getNationality());
        System.out.println("Issued Country  : " + mrzData.getIssuedCountry());
    }

    /**
     * Validates a VG response signature using certificate and optional certificate chain.
     * Prompts for VG response file, certificate file, and certificate chain file.
     */
    public void validateSignature() throws Exception {
        // Read VG response
        String responsePath = readInput("Enter VG response file path: ");
        if (responsePath.isEmpty()) {
            throw new ToolkitException("VG response path must not be empty");
        }

        byte[] responseBytes = Files.readAllBytes(Paths.get(new File(responsePath).getAbsolutePath()));
        String vgResponse = new String(responseBytes, "UTF-8");

        // Read certificate (optional)
        byte[] cert = null;
        String certPath = readInput("Enter certificate file path (or press Enter to skip): ");
        if (!certPath.isEmpty()) {
            cert = Files.readAllBytes(Paths.get(new File(certPath).getAbsolutePath()));
        }

        // Read certificate chain (optional)
        byte[] certChain = null;
        String chainPath = readInput("Enter certificate chain file path (or press Enter to skip): ");
        if (!chainPath.isEmpty()) {
            certChain = Files.readAllBytes(Paths.get(new File(chainPath).getAbsolutePath()));
        }

        // Validate
        SignatureValidator validator = new SignatureValidator(cert, certChain);
        validator.ValidateToolkitResponse(vgResponse);

        // Print results
        System.out.println("\n--- Signature Validator Response ---");
        System.out.println("Validation Status : " + validator.getValidationStatus());
        System.out.println("Service           : " + validator.getService());
        System.out.println("Action            : " + validator.getAction());
        System.out.println("Card Number       : " + validator.getCardNumber());
        System.out.println("CS Number         : " + validator.getCardSerialNumber());
        System.out.println("Correlation ID    : " + validator.getCorrelationId());
        System.out.println("ID Number         : " + validator.getIDNumber());
        System.out.println("Nonce             : " + validator.getNonce());
        System.out.println("Request ID        : " + validator.getRequestId());
        System.out.println("Timestamp         : " + validator.getTimestamp());
        System.out.println("Validity Interval : " + validator.getValidityInterval());
    }
}
