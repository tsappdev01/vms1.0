package ae.emiratesid.idcard.toolkit.sample.toolkit_face;

import android.content.Context;
import android.content.Intent;

import ae.emiratesid.idcard.toolkit.Toolkit;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.CardPublicData;
import ae.emiratesid.idcard.toolkit.datamodel.FaceSDKData;
import ae.emiratesid.idcard.toolkit.sample.BuildConfig;
import ae.emiratesid.idcard.toolkit.sample.ConnectionController;
import ae.emiratesid.idcard.toolkit.sample.Constants;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;

public class ToolkitFace {

    public static final int FRONT_CAMERA = 0;
    public static final int BACK_CAMERA = 1;
    public static final int AUTO_CAPTURE = 1;
    public static final int SMILE_LIVELINESS = 2;

    public static final int MAGNIFY_EYE_LIVELINESS = 3;

    public static String _idn;
    private static Context _context;
    public static String face_config;
    public static int sdk_handle;
    public static AuthenticateFaceResultInterface _authenticateListener;
    public static String _passportNumber;
    public static String _passportCountry;
    public static String _passportExpiry;
    public static String _passportDob;
    public static boolean _withDocs;
    public static boolean withId;

    private static int _cameraMode = FRONT_CAMERA;
    private static int _livelinessMode = AUTO_CAPTURE;

    private static String _liveness_data;
    private static CardPublicData cardPublicData;

    private static String capture_face;

    public static void setCameraMode(int cameraMode) {
        if ((cameraMode == FRONT_CAMERA) || (cameraMode == BACK_CAMERA)) {
            _cameraMode = cameraMode;
        }
    }

    public static int getCameraMode() {
        return _cameraMode;
    }

    public static void setLivelinessMode(int livelinessMode) {
        if ((livelinessMode == AUTO_CAPTURE) || (livelinessMode == SMILE_LIVELINESS)
                || (livelinessMode == MAGNIFY_EYE_LIVELINESS)) {
            _livelinessMode = livelinessMode;
        }
    }

    public static CardPublicData getCardPublicData() {
        return cardPublicData;
    }

    public static void setCardPublicData(CardPublicData cardPublicData) {
        ToolkitFace.cardPublicData = cardPublicData;
    }

    public static String get_liveness_data() {
        return _liveness_data;
    }

    public static void set_liveness_data(String liveness_data) {
        _liveness_data = liveness_data;
    }

    public static String getCapture_face() {
        return capture_face;
    }

    public static void setCapture_face(String capture_face) {
        ToolkitFace.capture_face = capture_face;
    }

    public static int getLivelinessMode() {
        return _livelinessMode;
    }

    public int authenticateFaceWithId(Context mContext, String idn,
                                      boolean withDocs,
                                      AuthenticateFaceResultInterface authenticateListener) {

        if (authenticateListener == null) {
            Logger.e("Authenticate Listener is null");
            return Constants.ERROR;
        }

        _idn = idn;
        _context = mContext;
        _withDocs = withDocs;
        _authenticateListener = authenticateListener;

        withId = true;

        Intent intent = new Intent(mContext, ToolkitFaceActivity.class);
        mContext.startActivity(intent);

        return Constants.SUCCESS;
    }

    public int authenticateFaceWithPassport(Context mContext,
                                            String passportNumber,
                                            String passportCountry,
                                            String passportExpiry,
                                            String passportDob,
                                            boolean withDocs,
                                            AuthenticateFaceResultInterface authenticateListener) {

        if (authenticateListener == null) {
            Logger.e("Authenticate Listener is null");
            return Constants.ERROR;
        }

        _context = mContext;
        _passportNumber = passportNumber;
        _passportCountry = passportCountry;
        _passportExpiry = passportExpiry;
        _passportDob = passportDob;
        _withDocs = withDocs;
        _authenticateListener = authenticateListener;

        withId = false;

        Intent intent = new Intent(mContext, ToolkitFaceActivity.class);
        mContext.startActivity(intent);

        return Constants.SUCCESS;
    }

    public interface AuthenticateFaceResultInterface {
        void onAuthenticateFaceResult(int status, String message,
                                      CardPublicData cardPublicData);
    }

    public static int init() {
        int status = Constants.SUCCESS;
        String message = null;
        FaceSDKData faceSDKData = null;
        try {
            Toolkit toolkit = ConnectionController.getToolkitObject();
            if (toolkit == null) {
                status = Constants.ERROR;
                Logger.e("Toolkit object not found");
                return status;
            }

            faceSDKData = toolkit.initFaceSDK(BuildConfig.APPLICATION_ID);
            if (faceSDKData == null || faceSDKData.getLicense().isEmpty()) {
                message = "Failed";
                status = Constants.ERROR;
                Logger.e("Invalid face configuration or license");
                return status;
            }
        } catch (ToolkitException e) {
            e.printStackTrace();
            status = (int) e.getCode();
            message = e.getMessage();
            Logger.e("Exception occurred :::" + e.getMessage() + " Status = " + status);
        }

        if (status == Constants.SUCCESS) {
            face_config = faceSDKData.getLicense();
            sdk_handle = faceSDKData.getHandle();
            Logger.i("Toolkit face initialized successfully");
        }

        return status;
    }
}
