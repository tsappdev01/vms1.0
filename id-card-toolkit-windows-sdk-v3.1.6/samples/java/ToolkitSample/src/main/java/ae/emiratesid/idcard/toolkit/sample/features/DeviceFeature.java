package ae.emiratesid.idcard.toolkit.sample.features;

import java.util.Scanner;

import ae.emiratesid.idcard.toolkit.Toolkit;
import ae.emiratesid.idcard.toolkit.datamodel.RegisterDeviceResponse;
import ae.emiratesid.idcard.toolkit.sample.BaseFeature;
import ae.emiratesid.idcard.toolkit.sample.CryptoHelper;
import ae.emiratesid.idcard.toolkit.sample.ToolkitSession;

/**
 * Device-level operations: device ID, toolkit version, and device registration.
 */
public class DeviceFeature extends BaseFeature {

    public DeviceFeature(ToolkitSession session, Scanner scanner) {
        super(session, scanner);
    }

    /**
     * Prints the device ID.
     */
    public void getDeviceId() throws Exception {
        Toolkit toolkit = session.getToolkit();
        System.out.println("Device ID: " + toolkit.getDeviceId());
    }

    /**
     * Prints the toolkit version.
     */
    public void getToolkitVersion() throws Exception {
        Toolkit toolkit = session.getToolkit();
        System.out.println("Toolkit Version: " + toolkit.getToolkitVerison());
    }

    /**
     * Registers a device with the toolkit server.
     * Prompts for username, password, and device reference ID.
     * Encrypts credentials using the request handle, then calls registerDevice.
     */
    public void registerDevice() throws Exception {
        String userName = readInput("Enter username: ");
        if (userName.isEmpty()) {
            System.out.println("Username must not be empty.");
            return;
        }

        String password = readInput("Enter password: ");
        if (password.isEmpty()) {
            System.out.println("Password must not be empty.");
            return;
        }

        String deviceRefId = readInput("Enter device reference ID: ");
        if (deviceRefId.isEmpty()) {
            System.out.println("Device reference ID must not be empty.");
            return;
        }

        Toolkit toolkit = session.getToolkit();

        // Generate request ID and prepare request
        String requestId = CryptoHelper.generateRequestId();
        String requestHandle = toolkit.prepareRequest(requestId);

        // Encrypt username and password
        String encodedUsername = CryptoHelper.encryptParams(userName, requestHandle,
                session.getDataProtectionKey());
        String encodedPassword = CryptoHelper.encryptParams(password, requestHandle,
                session.getDataProtectionKey());

        RegisterDeviceResponse response = toolkit.registerDevice(encodedUsername, encodedPassword, deviceRefId);

        if (response != null) {
            System.out.println("Device Registration ID: " + response.getDeviceRegistrationID());
        }
        else {
            System.out.println("Register device returned no response.");
        }
    }
}
