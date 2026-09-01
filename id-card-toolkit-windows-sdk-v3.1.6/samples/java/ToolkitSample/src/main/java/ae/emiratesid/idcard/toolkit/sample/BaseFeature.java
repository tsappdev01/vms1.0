package ae.emiratesid.idcard.toolkit.sample;

import java.util.Scanner;

import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.ToolkitResponse;

/**
 * Abstract base class for all feature implementations.
 * Provides shared access to the toolkit session, user input helpers,
 * PIN encryption, response printing, and response verification.
 */
public abstract class BaseFeature {

    protected final ToolkitSession session;
    protected final Scanner scanner;

    protected BaseFeature(ToolkitSession session, Scanner scanner) {
        this.session = session;
        this.scanner = scanner;
    }

    /**
     * Reads a trimmed line of text from the user.
     *
     * @param prompt the prompt to display
     * @return the trimmed input string
     */
    protected String readInput(String prompt) {
        System.out.print(prompt);
        return scanner.nextLine().trim();
    }

    /**
     * Reads an integer from the user, re-prompting on invalid input.
     *
     * @param prompt the prompt to display
     * @return the integer value entered
     */
    protected int readInt(String prompt) {
        System.out.print(prompt);
        while (!scanner.hasNextInt()) {
            scanner.next();
            System.out.print("Invalid number. " + prompt);
        }

        int value = scanner.nextInt();
        scanner.nextLine(); // consume newline
        return value;
    }

    /**
     * Generates a request ID, calls prepareRequest, and encrypts the PIN in one step.
     *
     * @param pin the PIN to encrypt
     * @return Base64-encoded encrypted PIN
     * @throws ToolkitException if any step fails
     */
    protected String prepareAndEncryptPin(String pin) throws ToolkitException {
        String requestId = CryptoHelper.generateRequestId();
        String requestHandle = session.prepareRequest(requestId);
        return CryptoHelper.encryptPin(pin, requestHandle, session.getDataProtectionKey());
    }

    /**
     * Prints the standard response header fields from a ToolkitResponse.
     *
     * @param response the toolkit response to print
     */
    protected void printResponseHeader(ToolkitResponse response) {
        if (response == null) {
            return;
        }

        System.out.println("  Request ID      : " + response.getRequestId());
        System.out.println("  Timestamp       : " + response.getTimeStamp());
        System.out.println("  ID Number       : " + response.getiDNumber());
        System.out.println("  Card Number     : " + response.getCardNumber());
        System.out.println("  Card Serial     : " + response.getCardSerialNumber());
        System.out.println("  Service         : " + response.getService());
        System.out.println("  Status          : " + response.getStatus());
    }

    /**
     * Verifies the XML response signature and request ID match.
     * Skips verification if the XML string is null or empty.
     *
     * @param xmlString the XML response string
     * @param requestId the expected request ID
     * @throws ToolkitException if verification fails
     */
    protected void verifyResponse(String xmlString, String requestId) throws ToolkitException {
        if (xmlString != null && !xmlString.isEmpty()) {
            XmlHelper.verifyAndExtractRequestId(xmlString, requestId);
        }
    }
}
