package ae.emiratesid.idcard.toolkit.sample.utils;

import android.util.Base64;

import java.security.SecureRandom;

import ae.emiratesid.idcard.toolkit.sample.logger.Logger;

public class RequestGenerator {

    private static SecureRandom secureRandom = new SecureRandom();

    public static String generateRequestID() {

        byte[] rndBytes = new byte[40];
        secureRandom.nextBytes(rndBytes);
        Logger.d("length of random bytes " + rndBytes.length);
        String requestId = Base64.encodeToString(rndBytes, Base64.NO_WRAP);
        Logger.d("length of generated string " +
                requestId.length());
        return requestId;
//		return "jhN8uvfCapB1dmqKGAVoND38n7sbK1lRXAOOsFN7Tvpde7W5+AK9zg==";
    }//generateRequestID()

//	public static String generateRequestIDString(){
//		byte[]  rndBytes =  new byte[40];
//		new SecureRandom().nextBytes(rndBytes);
//		return Base64.encodeToString(rndBytes, Base64.DEFAULT);
//	}//generateRequestID()

}//end of class
