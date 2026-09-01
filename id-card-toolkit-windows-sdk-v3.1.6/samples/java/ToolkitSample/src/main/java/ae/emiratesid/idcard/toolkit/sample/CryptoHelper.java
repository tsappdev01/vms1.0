package ae.emiratesid.idcard.toolkit.sample;

import java.nio.charset.Charset;
import java.security.PublicKey;
import java.security.SecureRandom;
import java.util.Base64;

import javax.crypto.Cipher;

import ae.emiratesid.idcard.toolkit.ToolkitException;

/**
 * Stateless cryptographic utility methods for request ID generation and PIN/parameter encryption.
 * All methods are static -- no global state.
 */
public final class CryptoHelper {

    private static final Charset UTF_8 = Charset.forName("UTF-8");
    private static final String RSA_TRANSFORMATION = "RSA/ECB/PKCS1Padding";

    private CryptoHelper() {
    }

    /**
     * Generates a random request ID: 40 bytes of SecureRandom, Base64-encoded.
     *
     * @return Base64-encoded request ID string
     */
    public static String generateRequestId() {
        byte[] randomBytes = new byte[40];
        new SecureRandom().nextBytes(randomBytes);
        return Base64.getEncoder().encodeToString(randomBytes);
    }

    /**
     * Encrypts a PIN with the request handle and data protection key.
     * The request handle bytes are prepended to the PIN bytes before encryption.
     *
     * @param pin             the PIN string to encrypt
     * @param requestHandle   Base64-encoded request handle from prepareRequest
     * @param key             RSA public key for encryption
     * @return Base64-encoded encrypted PIN
     * @throws ToolkitException if encryption fails
     */
    public static String encryptPin(String pin, String requestHandle, PublicKey key) throws ToolkitException {
        byte[] pinBytes = pin.getBytes(UTF_8);
        return encryptWithHandle(pinBytes, requestHandle, key);
    }

    /**
     * Encrypts an arbitrary string parameter with the request handle and data protection key.
     *
     * @param param           the parameter string to encrypt
     * @param requestHandle   Base64-encoded request handle from prepareRequest
     * @param key             RSA public key for encryption
     * @return Base64-encoded encrypted parameter
     * @throws ToolkitException if encryption fails
     */
    public static String encryptParams(String param, String requestHandle, PublicKey key) throws ToolkitException {
        byte[] paramBytes = param.getBytes(UTF_8);
        return encryptWithHandle(paramBytes, requestHandle, key);
    }

    /**
     * Encrypts arbitrary parameter bytes with the request handle and data protection key.
     *
     * @param paramBytes      the parameter bytes to encrypt
     * @param requestHandle   Base64-encoded request handle from prepareRequest
     * @param key             RSA public key for encryption
     * @return Base64-encoded encrypted parameter
     * @throws ToolkitException if encryption fails
     */
    public static String encryptParams(byte[] paramBytes, String requestHandle, PublicKey key) throws ToolkitException {
        return encryptWithHandle(paramBytes, requestHandle, key);
    }

    /**
     * Core encryption: decodes requestHandle from Base64, prepends it to data bytes,
     * encrypts with RSA/ECB/PKCS1Padding, returns Base64-encoded ciphertext.
     */
    private static String encryptWithHandle(byte[] dataBytes, String requestHandle, PublicKey key)
            throws ToolkitException {
        try {
            byte[] handleBytes = Base64.getDecoder().decode(requestHandle);

            byte[] plainData = new byte[handleBytes.length + dataBytes.length];
            System.arraycopy(handleBytes, 0, plainData, 0, handleBytes.length);
            System.arraycopy(dataBytes, 0, plainData, handleBytes.length, dataBytes.length);

            Cipher cipher = Cipher.getInstance(RSA_TRANSFORMATION);
            cipher.init(Cipher.PUBLIC_KEY, key);
            byte[] encryptedBytes = cipher.doFinal(plainData);

            return Base64.getEncoder().encodeToString(encryptedBytes);
        }
        catch (Exception e) {
            throw new ToolkitException("Encryption failed: " + e.getMessage());
        }
    }
}
