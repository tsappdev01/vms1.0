package ae.emiratesid.idcard.toolkit.sample.features;

import java.io.ByteArrayInputStream;
import java.security.cert.CertificateFactory;
import java.security.cert.X509Certificate;
import java.util.Base64;
import java.util.Scanner;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardCertificates;
import ae.emiratesid.idcard.toolkit.datamodel.SignatureResponse;
import ae.emiratesid.idcard.toolkit.datamodel.ToolkitResponse;
import ae.emiratesid.idcard.toolkit.sample.BaseFeature;
import ae.emiratesid.idcard.toolkit.sample.CryptoHelper;
import ae.emiratesid.idcard.toolkit.sample.ToolkitSession;

/**
 * PKI operations: certificate retrieval, PIN authentication, data signing,
 * and card status checking.
 */
public class PKIFeature extends BaseFeature {

    public PKIFeature(ToolkitSession session, Scanner scanner) {
        super(session, scanner);
    }

    /**
     * Authenticates the cardholder using PKI certificate and PIN.
     *
     * @throws ToolkitException if the toolkit operation fails
     */
    public void authenticatePki() throws ToolkitException {
        CardReader cardReader = session.getCardReader();

        String pin = readInput("Enter PIN: ");
        String encodedPin = prepareAndEncryptPin(pin);

        ToolkitResponse response = cardReader.authenticatePki(encodedPin);
        if (response == null) {
            throw new ToolkitException("PKI authentication returned null response");
        }

        printResponseHeader(response);
        System.out.println("PKI authentication successful.");
    }

    /**
     * Verifies the cardholder's PIN.
     *
     * @throws ToolkitException if the toolkit operation fails
     */
    public void authenticatePin() throws ToolkitException {
        CardReader cardReader = session.getCardReader();

        String pin = readInput("Enter PIN: ");
        String encodedPin = prepareAndEncryptPin(pin);

        ToolkitResponse response = cardReader.authenticatePin(encodedPin);
        if (response == null) {
            throw new ToolkitException("PIN authentication returned null response");
        }

        printResponseHeader(response);
        System.out.println("PIN authentication successful.");
    }

    /**
     * Retrieves and displays authentication and signing certificates from the card.
     *
     * @throws ToolkitException if the toolkit operation fails
     */
    public void getCertificates() throws ToolkitException {
        CardReader cardReader = session.getCardReader();

        String pin = readInput("Enter PIN: ");
        String encodedPin = prepareAndEncryptPin(pin);

        CardCertificates certs = cardReader.getPkiCertificates(encodedPin);
        if (certs == null) {
            throw new ToolkitException("Get certificates returned null response");
        }

        printResponseHeader(certs);

        try {
            CertificateFactory cf = CertificateFactory.getInstance("X.509");

            System.out.println("\n--- Authentication Certificate ---");
            printX509Certificate(cf, certs.getAuthenticationCertificate());

            System.out.println("\n--- Signing Certificate ---");
            printX509Certificate(cf, certs.getSigningCertificate());
        }
        catch (Exception e) {
            throw new ToolkitException("Failed to parse certificates: " + e.getMessage());
        }
    }

    /**
     * Signs arbitrary data with the card's signing key.
     * Asks for the data string and whether it is already hashed.
     *
     * @throws ToolkitException if the toolkit operation fails
     */
    public void signData() throws ToolkitException {
        CardReader cardReader = session.getCardReader();

        String dataString = readInput("Enter data to sign: ");
        String hashChoice = readInput("Is input already hashed? (y/n): ");
        boolean isInputHash = "y".equalsIgnoreCase(hashChoice.trim());

        String pin = readInput("Enter PIN: ");
        String encodedPin = prepareAndEncryptPin(pin);

        byte[] data = dataString.getBytes();
        SignatureResponse response = cardReader.signData(data, isInputHash, encodedPin);
        if (response == null) {
            throw new ToolkitException("Sign data returned null response");
        }

        printResponseHeader(response);

        byte[] signature = response.getSignature();
        if (signature != null && signature.length > 0) {
            System.out.println("Signature (Base64) :: " + Base64.getEncoder().encodeToString(signature));
        }
        else {
            System.out.println("No signature data in response.");
        }
    }

    /**
     * Checks the card validity status. Does not require a PIN.
     *
     * @throws ToolkitException if the toolkit operation fails
     */
    public void checkCardStatus() throws ToolkitException {
        CardReader cardReader = session.getCardReader();
        String requestId = CryptoHelper.generateRequestId();

        ToolkitResponse response = cardReader.checkCardStatus(requestId);
        if (response == null) {
            throw new ToolkitException("Check card status returned null response");
        }

        printResponseHeader(response);
        System.out.println("Card Status :: " + response.getStatus());
    }

    // ---- Private helpers ----

    private void printX509Certificate(CertificateFactory cf, byte[] certData) throws Exception {
        if (certData == null || certData.length == 0) {
            System.out.println("Certificate data not available.");
            return;
        }

        ByteArrayInputStream in = new ByteArrayInputStream(certData);
        X509Certificate cert = (X509Certificate) cf.generateCertificate(in);

        System.out.println("Subject :: " + cert.getSubjectDN().getName());
        System.out.println("Issuer :: " + cert.getIssuerDN().getName());
        System.out.println("Valid From :: " + cert.getNotBefore());
        System.out.println("Valid To :: " + cert.getNotAfter());
        System.out.println("Serial Number :: " + cert.getSerialNumber().toString(16));
    }
}
