package ae.emiratesid.idcard.toolkit.sample.logger;

import android.util.Log;

/**
 * Helper class to print the logs.
 */
public class Logger {

   private static final boolean IS_DEBUG =  true;
    private static final String TAG = "__EIDACardSDK_TAG__";

    /**
     * Print the log for Debug level.
     * @param msg
     */
    public static void d(String msg){
        if(IS_DEBUG){
            Log.d(TAG , isEmpty(msg) ? "No message for Debugger" : msg);
        }//if()
    }//d()
    /**
     * Print the log for ERROR level.
     * @param msg
     */
    public static void e(String msg){

        Log.e(TAG , isEmpty(msg) ? "No message for Debugger" : msg);

    }//e()
    /**
     * Print the log for INFO level.
     * @param msg
     */
    public static void i(String msg){
        if(IS_DEBUG){
            Log.i(TAG , isEmpty(msg) ? "No message for Debugger" : msg);
        }//if()
    }//i()
    /**
     * Print the log for WARNING level.
     * @param msg
     */
    public static void w(String msg){
        Log.w(TAG , isEmpty(msg) ? "No message for Debugger" : msg);
    }//e()

    private static boolean isEmpty(String msg){
        return (msg == null || msg.isEmpty());
    }//
}
