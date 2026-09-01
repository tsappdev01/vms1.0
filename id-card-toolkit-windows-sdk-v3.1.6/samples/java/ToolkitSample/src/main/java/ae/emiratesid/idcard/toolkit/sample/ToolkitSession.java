package ae.emiratesid.idcard.toolkit.sample;

import java.security.KeyFactory;
import java.security.PublicKey;
import java.security.spec.X509EncodedKeySpec;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.Toolkit;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.DataProtectionKey;

/**
 * Manages toolkit lifecycle: initialization, reader discovery, connection, cleanup.
 * Features receive this as a dependency -- they never manage lifecycle themselves.
 */
public class ToolkitSession {

    private Toolkit toolkit;
    private CardReader cardReader;
    private PublicKey dataProtectionKey;
    private boolean initialized;

    /**
     * Initializes the toolkit with the given configuration parameters.
     * Creates the Toolkit instance, retrieves the data protection key, and marks
     * the session as initialized.
     *
     * @param configParams configuration string (config_directory, log_directory, vg_url, etc.)
     * @throws ToolkitException if toolkit initialization or key retrieval fails
     */
    public void initialize(String configParams) throws ToolkitException {
        if (initialized) {
            throw new ToolkitException("Session is already initialized");
        }

        toolkit = new Toolkit(true, configParams);

        DataProtectionKey dpKey = toolkit.getDataProtectionKey();
        try {
            KeyFactory keyFactory = KeyFactory.getInstance("RSA");
            X509EncodedKeySpec keySpec = new X509EncodedKeySpec(dpKey.getPublicKey());
            dataProtectionKey = keyFactory.generatePublic(keySpec);
        }
        catch (Exception e) {
            toolkit.cleanup();
            toolkit = null;
            throw new ToolkitException("Failed to decode data protection key: " + e.getMessage());
        }

        initialized = true;
    }

    /**
     * Discovers and selects the reader that has an Emirates ID card inserted.
     *
     * @throws ToolkitException if no reader with Emirates ID is found
     */
    public void selectReader() throws ToolkitException {
        checkInitialized();
        cardReader = toolkit.getReaderWithEmiratesID();
        System.out.println("Reader Name    : " + cardReader.getReaderName());
        if (cardReader.getReaderSerialNumber() != null) {
            System.out.println("Reader Serial  : " + cardReader.getReaderSerialNumber());
        }
    }

    /**
     * Ensures the card reader is connected. If already connected, this is a no-op.
     *
     * @throws ToolkitException if connection fails
     */
    public void ensureConnected() throws ToolkitException {
        checkInitialized();
        if (cardReader == null) {
            throw new ToolkitException("No reader selected. Call selectReader() first.");
        }

        if (!cardReader.isConnected()) {
            cardReader.connect();
        }
    }

    /**
     * Disconnects the card reader if currently connected.
     *
     * @throws ToolkitException if disconnection fails
     */
    public void disconnect() throws ToolkitException {
        if (cardReader != null && cardReader.isConnected()) {
            cardReader.disconnect();
        }
    }

    /**
     * Full cleanup: disconnects reader, cleans up toolkit, clears all state.
     * Safe to call multiple times.
     */
    public void cleanup() {
        try {
            disconnect();
        }
        catch (ToolkitException e) {
            System.err.println("Warning: disconnect failed during cleanup: " + e.getMessage());
        }

        if (toolkit != null) {
            try {
                toolkit.cleanup();
            }
            catch (ToolkitException e) {
                System.err.println("Warning: cleanup failed: " + e.getMessage());
            }
        }

        toolkit = null;
        cardReader = null;
        dataProtectionKey = null;
        initialized = false;
    }

    /**
     * Prepares a request with the given request ID.
     *
     * @param requestId unique request identifier
     * @return the request handle (Base64-encoded)
     * @throws ToolkitException if prepare request fails
     */
    public String prepareRequest(String requestId) throws ToolkitException {
        checkInitialized();
        if (cardReader == null) {
            throw new ToolkitException("No reader selected. Call selectReader() first.");
        }

        return cardReader.prepareRequest(requestId);
    }

    public Toolkit getToolkit() {
        return toolkit;
    }

    public CardReader getCardReader() {
        return cardReader;
    }

    public PublicKey getDataProtectionKey() {
        return dataProtectionKey;
    }

    public boolean isInitialized() {
        return initialized;
    }

    public boolean isConnected() throws ToolkitException {
        if (cardReader == null) {
            return false;
        }

        return cardReader.isConnected();
    }

    private void checkInitialized() throws ToolkitException {
        if (!initialized) {
            throw new ToolkitException("Session not initialized. Call initialize() first.");
        }
    }
}
