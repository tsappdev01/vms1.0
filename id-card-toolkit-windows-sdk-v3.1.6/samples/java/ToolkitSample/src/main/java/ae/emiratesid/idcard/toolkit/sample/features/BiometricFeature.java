package ae.emiratesid.idcard.toolkit.sample.features;

import java.util.Scanner;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.FingerData;
import ae.emiratesid.idcard.toolkit.datamodel.ToolkitResponse;
import ae.emiratesid.idcard.toolkit.sample.BaseFeature;
import ae.emiratesid.idcard.toolkit.sample.CryptoHelper;
import ae.emiratesid.idcard.toolkit.sample.ToolkitSession;

/**
 * Biometric operations: fingerprint authentication (server and card),
 * combined card + biometric authentication, PIN reset, and PIN unblock.
 */
public class BiometricFeature extends BaseFeature {

    private static final int BIOMETRIC_TIMEOUT = 20;

    public BiometricFeature(ToolkitSession session, Scanner scanner) {
        super(session, scanner);
    }

    /**
     * Authenticates the cardholder's fingerprint on the server.
     *
     * @throws ToolkitException if the toolkit operation fails
     */
    public void authenticateBiometricOnServer() throws ToolkitException {
        CardReader cardReader = session.getCardReader();

        FingerData[] fingerData = cardReader.getFingerData();
        validateFingerData(fingerData);

        String requestId = CryptoHelper.generateRequestId();
        int fingerChoice = chooseFinger(fingerData);

        ToolkitResponse response = cardReader.authenticateBiometricOnServer(
                requestId, fingerData[fingerChoice].getFingerIndex(), BIOMETRIC_TIMEOUT);

        verifyBiometricResponse(response);
        printResponseHeader(response);
        System.out.println("Biometric authentication on server successful.");
    }

    /**
     * Authenticates the cardholder's fingerprint on the card (SAM mode).
     *
     * @throws ToolkitException if the toolkit operation fails
     */
    public void authenticateBiometricOnCard() throws ToolkitException {
        CardReader cardReader = session.getCardReader();

        FingerData[] fingerData = cardReader.getFingerData();
        validateFingerData(fingerData);

        String requestId = CryptoHelper.generateRequestId();
        int fingerChoice = chooseFinger(fingerData);

        ToolkitResponse response = cardReader.authenticateBiometricOnCard(
                requestId, fingerData[fingerChoice], BIOMETRIC_TIMEOUT);

        verifyBiometricResponse(response);
        printResponseHeader(response);
        System.out.println("Biometric authentication on card successful.");
    }

    /**
     * Performs combined card and biometric authentication.
     *
     * @throws ToolkitException if the toolkit operation fails
     */
    public void authenticateCardAndBiometric() throws ToolkitException {
        CardReader cardReader = session.getCardReader();

        FingerData[] fingerData = cardReader.getFingerData();
        validateFingerData(fingerData);

        String requestId = CryptoHelper.generateRequestId();
        int fingerChoice = chooseFinger(fingerData);

        ToolkitResponse response = cardReader.authenticateCardandBiometric(
                requestId, fingerData[fingerChoice].getFingerIndex(), BIOMETRIC_TIMEOUT);

        verifyBiometricResponse(response);
        printResponseHeader(response);
        System.out.println("Card and biometric authentication successful.");
    }

    /**
     * Resets the card PIN using biometric authentication.
     * Asks for new PIN, encrypts it, then performs biometric verification.
     *
     * @throws ToolkitException if the toolkit operation fails
     */
    public void resetPin() throws ToolkitException {
        CardReader cardReader = session.getCardReader();

        FingerData[] fingerData = cardReader.getFingerData();
        validateFingerData(fingerData);

        String newPin = readInput("Enter new PIN: ");
        String encodedPin = prepareAndEncryptPin(newPin);

        int fingerChoice = chooseFinger(fingerData);

        ToolkitResponse response = cardReader.resetPin(
                encodedPin, fingerData[fingerChoice], BIOMETRIC_TIMEOUT);

        verifyBiometricResponse(response);
        System.out.println("PIN reset successful.");
    }

    /**
     * Unblocks the card PIN using biometric authentication.
     * Asks for new PIN, encrypts it, then performs biometric verification.
     *
     * @throws ToolkitException if the toolkit operation fails
     */
    public void unblockPin() throws ToolkitException {
        CardReader cardReader = session.getCardReader();

        FingerData[] fingerData = cardReader.getFingerData();
        validateFingerData(fingerData);

        String newPin = readInput("Enter new PIN: ");
        String encodedPin = prepareAndEncryptPin(newPin);

        int fingerChoice = chooseFinger(fingerData);

        ToolkitResponse response = cardReader.unblockPin(
                encodedPin, fingerData[fingerChoice], BIOMETRIC_TIMEOUT);

        verifyBiometricResponse(response);
        System.out.println("PIN unblock successful.");
    }

    // ---- Private helpers ----

    private void validateFingerData(FingerData[] fingerData) throws ToolkitException {
        if (fingerData == null || fingerData.length < 2) {
            throw new ToolkitException("Failed to read finger data from card");
        }
    }

    private int chooseFinger(FingerData[] fingerData) throws ToolkitException {
        System.out.println("Finger 0: ID=" + fingerData[0].getFingerId()
                + " Index=" + fingerData[0].getFingerIndex());
        System.out.println("Finger 1: ID=" + fingerData[1].getFingerId()
                + " Index=" + fingerData[1].getFingerIndex());

        int choice = readInt("Choose finger (0 or 1): ");

        if (choice < 0 || choice > 1) {
            throw new ToolkitException("Invalid finger choice: must be 0 or 1");
        }

        return choice;
    }

    private void verifyBiometricResponse(ToolkitResponse response) throws ToolkitException {
        if (response == null) {
            throw new ToolkitException("Biometric operation returned null response");
        }
    }
}
