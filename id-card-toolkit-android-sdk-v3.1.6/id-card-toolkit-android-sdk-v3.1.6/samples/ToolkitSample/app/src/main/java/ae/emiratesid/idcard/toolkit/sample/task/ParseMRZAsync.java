package ae.emiratesid.idcard.toolkit.sample.task;

import android.os.AsyncTask;

import java.lang.ref.WeakReference;

import ae.emiratesid.idcard.toolkit.Toolkit;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.MRZData;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;
import ae.emiratesid.idcard.toolkit.sample.utils.RequestGenerator;

public class ParseMRZAsync extends AsyncTask< Void, Integer, Integer > {

    private String mrzString;
    private Toolkit toolkit;
    private String message;
    private int status;
    private WeakReference<ParseMRZListener > weakReference;

    public MRZData response;

    public ParseMRZAsync( ParseMRZListener listener, String mrzString ) {
        this.weakReference = new WeakReference<ParseMRZListener  >(listener);
        this.mrzString = mrzString;
    }//ParseMRZAsync

    @Override
    protected Integer doInBackground( Void... voids ) {

        try {
            toolkit = ConnectionController.getToolkitObject();
            if ( toolkit == null ) {
                return Constants.ERROR;
            }

            String requestId = RequestGenerator.generateRequestID();
            Logger.d( "requestId :: " + requestId );

            response = toolkit.parseMRZ( mrzString );
            Logger.d( "Response of MRZData :: "+response );

            if ( response == null ) {
                message = "Response from Toolkit is null";
                status = Constants.ERROR;
                return status;
            }
            //call the function
            status = Constants.SUCCESS;
            return status;

        } catch ( ToolkitException e ) {
            e.printStackTrace();

            status = ( int ) e.getCode();
            message = e.getMessage();
            Logger.e( "Exception occurred :::" + e.getMessage() + " Status = " + status );
        }

        return status;
    }

    @Override
    protected void onPostExecute(Integer result) {
        super.onPostExecute(status);
        weakReference.get().onParseMRZString( response );
    }

    public interface ParseMRZListener {
        void onParseMRZString(MRZData mrzData);
    }//VerifyFingerprintOnCardListener()
}
