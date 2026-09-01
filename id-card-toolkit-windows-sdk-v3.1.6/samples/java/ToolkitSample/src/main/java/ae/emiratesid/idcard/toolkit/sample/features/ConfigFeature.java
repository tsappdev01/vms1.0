package ae.emiratesid.idcard.toolkit.sample.features;

import java.util.Scanner;

import ae.emiratesid.idcard.toolkit.Toolkit;
import ae.emiratesid.idcard.toolkit.datamodel.ConfigExpiryDate;
import ae.emiratesid.idcard.toolkit.sample.BaseFeature;
import ae.emiratesid.idcard.toolkit.sample.ToolkitSession;

/**
 * Configuration operations: license expiry and config certificate expiry dates.
 */
public class ConfigFeature extends BaseFeature {

    public ConfigFeature(ToolkitSession session, Scanner scanner) {
        super(session, scanner);
    }

    /**
     * Prints the license expiry date.
     */
    public void getLicenseExpiryDate() throws Exception {
        Toolkit toolkit = session.getToolkit();
        System.out.println("License Expiry Date: " + toolkit.getLicenseExpiryDate());
    }

    /**
     * Prints all configuration certificate expiry dates:
     * AG cert, VG cert, LV cert, server TLS cert, and license.
     */
    public void getConfigExpiryDates() throws Exception {
        Toolkit toolkit = session.getToolkit();
        ConfigExpiryDate dates = toolkit.getConfigCertExpiryDate();

        if (dates == null) {
            System.out.println("No config expiry date information available.");
            return;
        }

        System.out.println("\n--- Config Certificate Expiry Dates ---");

        String agExpiry = dates.getConfigAgCertExpiry();
        if (agExpiry != null && !agExpiry.isEmpty()) {
            System.out.println("AG Cert Expiry     : " + agExpiry);
        }

        String vgExpiry = dates.getConfigVgCertExpiry();
        if (vgExpiry != null && !vgExpiry.isEmpty()) {
            System.out.println("VG Cert Expiry     : " + vgExpiry);
        }

        String lvExpiry = dates.getConfigLvCertExpiry();
        if (lvExpiry != null && !lvExpiry.isEmpty()) {
            System.out.println("LV Cert Expiry     : " + lvExpiry);
        }

        String tlsExpiry = dates.getServerTlsCertExpiry();
        if (tlsExpiry != null && !tlsExpiry.isEmpty()) {
            System.out.println("Server TLS Expiry  : " + tlsExpiry);
        }

        String licenseExpiry = dates.getLicenseExpiry();
        if (licenseExpiry != null && !licenseExpiry.isEmpty()) {
            System.out.println("License Expiry     : " + licenseExpiry);
        }
    }
}
