package ae.emiratesid.idcard.toolkit.sample.utils;

import android.util.Base64;

import java.security.InvalidKeyException;
import java.security.KeyFactory;
import java.security.NoSuchAlgorithmException;
import java.security.PublicKey;
import java.security.spec.InvalidKeySpecException;
import java.security.spec.X509EncodedKeySpec;

import javax.crypto.BadPaddingException;
import javax.crypto.Cipher;
import javax.crypto.IllegalBlockSizeException;
import javax.crypto.NoSuchPaddingException;

import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;

public class    CryptoUtils {

    private static byte[]   publickey = null;

    public static void setPublickey( byte[] publickey ) {
        CryptoUtils.publickey = publickey;
    }
    public String encryptData( String data) throws NoSuchAlgorithmException, NoSuchPaddingException, InvalidKeySpecException, InvalidKeyException, IllegalBlockSizeException, BadPaddingException {

        if (data == null) {

        }//

        //get the byte array from string.
        byte[] plainData = data.getBytes();

        //create a Chiper instance with PKCS1padding.
        Cipher cipher = Cipher.getInstance("RSA/ECB/PKCS1Padding");

        //Get the key
        PublicKey key = KeyFactory.getInstance("RSA")
                .generatePublic(new X509EncodedKeySpec(publickey));

        cipher.init(Cipher.PUBLIC_KEY, key);

        byte[] encryptedBytes = cipher.doFinal(plainData);
        return Base64.encodeToString(encryptedBytes, Base64.DEFAULT);
    }//encryptData()..

    public String encryptData(byte[] data) throws NoSuchAlgorithmException, NoSuchPaddingException, InvalidKeySpecException, InvalidKeyException, IllegalBlockSizeException, BadPaddingException {

        if (data == null) {

        }//

        //create a Chiper instance with PKCS1padding.
        Cipher cipher = Cipher.getInstance("RSA/ECB/PKCS1Padding");

        //Get the key
        PublicKey key = KeyFactory.getInstance("RSA")
                .generatePublic(new X509EncodedKeySpec(publickey));

        cipher.init(Cipher.PUBLIC_KEY, key);

        byte[] encryptedBytes = cipher.doFinal(data);
        return Base64.encodeToString(encryptedBytes, Base64.NO_WRAP);
    }//encryptData()..

    //nDg52k4gazo=

    public String encryptParams(String param, String padding) throws ToolkitException {

        byte[] paddingByte = Base64.decode(padding, Base64.DEFAULT);
        byte[] paramsByte = param.getBytes();

        int totallength = paddingByte.length + paramsByte.length;
        byte[] plainData = new byte[totallength];

        Logger.d("totallength::" + totallength);
        Logger.d("paramsByte.length ::" + paramsByte.length);
        Logger.d("paddingByte.length ::" + paddingByte.length);
        System.arraycopy(paddingByte, 0, plainData, 0, paddingByte.length);
        System.arraycopy(paramsByte, 0, plainData, paddingByte.length, paramsByte.length);

        try {
            return encryptData(plainData);
        } catch (Exception e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            throw new ToolkitException(e);
        }//catch()
    }//

    public String encryptParams(byte[] paramsByte, String requestHandle) throws ToolkitException {

        byte[] paddingByte = Base64.decode(requestHandle, Base64.DEFAULT);

        int totallength = paddingByte.length + paramsByte.length;
        byte[] plainData = new byte[totallength];

        Logger.d("totallength::" + totallength);
        Logger.d("paramsByte.length ::" + paramsByte.length);
        Logger.d("paddingByte.length ::" + paddingByte.length);
        System.arraycopy(paddingByte, 0, plainData, 0, paddingByte.length);
        System.arraycopy(paramsByte, 0, plainData, paddingByte.length, paramsByte.length);

        try {
            return encryptData(plainData);
        } catch (Exception e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            throw new ToolkitException(e);
        }//catch()
    }//

    public byte[] encodePin(String pin) throws ToolkitException {

        if (pin == null || pin.isEmpty() || pin.length() < 4 || pin.length() > 8) {
            throw new ToolkitException("Invalid pin");
        }//if()

        byte[] PIN = pin.getBytes();

        byte[] encodedPin = new byte[8];

        byte[] convertedPin = new byte[16];

        for (int iter = 0; iter < PIN.length; iter++) {
            convertedPin[iter] = (byte) (PIN[iter] - 0x30);
        }//for()

        // Convert, copy & pad PIN data
        int i = 0;
        for (; i < PIN.length; i++) {
            if (i % 2 != 0) {
                encodedPin[i / 2] |= (byte) (convertedPin[i]);
            } else {
                encodedPin[i / 2] = (byte) ((convertedPin[i]) << 4);
            }
        }
        // Add padding for the odd counter
        if (i % 2 != 0) {
            encodedPin[i++ / 2] |= 0x0F;
        }

        // Pad the remaining buffer
        for (i = i / 2; i < 8; i++) {
            encodedPin[i] = (byte) 0xFF;
        }

        return encodedPin;
    }//encodePin()
}
