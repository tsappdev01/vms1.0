#ifndef T5_AirSnap_H
#define T5_AirSnap_H

#ifdef _WINDOWS
#ifdef T5AIRSNAP_DLL
#define T5AIRSNAP_API __declspec(dllexport)
#else
#define T5AIRSNAP_API __declspec(dllimport)
#endif
#elif defined(__IOS__) || defined(__APPLE__) || defined(__ANDROID__)
#define T5AIRSNAP_API __attribute__((visibility("default")))
#else
#define T5AIRSNAP_API
#endif

#include "ansi_nist_itl_1.h"
#include "common.h"


const int32_t  SE_OK                       = 0;
const int32_t  SE_UNKN_EXCEPTION           = 1;
/// Insufficient memory available
const int32_t  SE_LOW_MEMORY               = 2;
/// System error. You can get addition information call getErrorDescription function
const int32_t  SE_SYSTEM                   = 3;
/// the dongle is not plugged to computer
const int32_t  SE_KEY                      = 4;
/// the evaluation time is expired
const int32_t  SE_TIME                     = 5;
/// the SDK is not initialized
const int32_t  SE_INIT                     = 6;
/// the computer's hardware not match to license
const int32_t  SE_HARDWARE                 = 7;
/// the value of the one of the pointer that was passed to the function is NULL
const int32_t  SE_WRONG_POINTER            = 8;
/// the value of the one of the parameters out of range
const int32_t  SE_WRONG_PARAMETER          = 9;
/// cannot load dll
const int32_t  SE_LOAD_RESOURSE_DLL        = 10;
/// the licensed FAM not detected
const int32_t  SE_FAM                      = 11;
/// JNI (java native interface) error
const int32_t  SE_JNI                      = 12;
/// license file not found or not set
const int32_t  SE_LICENSE_NOT_FOUND        = 13;
/// No implementation for this function
const int32_t  SE_NOT_IMPLEMENTED          = 14;
/// The size of buffer is not large enough for required data
const int32_t  SE_MORE_DATA                = 15;
/// The mxnet is not initialized
const int32_t  SE_MXNET_FAILED             = 16;
/// The flashlight is covered or the background light is too bright
const int32_t  SE_FLASHLIGHT_IS_COVERED    = 17;  // deprecated
/// The finger image format is not set to WSQ
const int32_t  SE_FINGER_IMAGE_FORMAT_NOT_WSQ = 18;
/////////////////////////////////////////////////////////////////////////////////////////
// process return values
/**
*   The dib that was passed to process function is not satisfied to
*   one of following conditions:
*   - 8 bit per pixel
*   - uncompressed
*   - number of planes equal 1
*   - width  more then zero
*   - height is not equal zero
*/
const int32_t  SE_WRONG_DIB                = 2000;
/// The image size is more then MAX_WIDTH x MAX_HEIGHT pixels
const int32_t  SE_IMAGE_TOO_BIG            = 2002;
/// The dib passed to the 'process' function hasn't resolution 500 dpi
const int32_t  SE_WRONG_RESOLUTION         = 2004;
/// The fingerprint image is not found in the source image
const int32_t  SE_NO_IMAGE                 = 2006;
/// Error occurs while template building
const int32_t  SE_PROCESSING               = 2008;
/// A process canceled by user (one of callback function return false)
const int32_t  SE_CANCEL                   = 2010;
/// A conversion template from version 7.1 or from NIST to ISO format is fail
const int32_t  SE_CONVERTION               = 2012;
/// Specified impression type is not supported
const int32_t  SE_IMPRESSION_TYPE          = 2014;
/// array of Image structure has members with same value of 'finger' field
/// NOTE: At this version of software you cannot put into template the information about more than one impression per finger.
const int32_t  SE_SAME_FING_ERR            = 2016;
/// passed image has zero size or pointer to the image is NULL
const int32_t  SE_NULL_IMAGE               = 2018;
/// Two compared templates keep information about different fingers, so nothing was matched
const int32_t  SE_DIF_FING                 = 2020;
/// fingerprint images have different size.
/// NOTE: The fingerprint images those are passed to 'create_NIST_template' or 'create_ISO_template' function should be taken from the same sensor and have the same size
const int32_t  SE_DIF_FING_SIZE_ERR        = 2022;
/// one of the dib that was passed to process function has width or height is to big or too small for chosen image processing algorithm
const int32_t  SE_WRONG_IMAGE_SIZE         = 2024;
/// wsq decode error
const int32_t  SE_WSQ_ERR                  = 2026;
/// error of parse template
const int32_t  SE_PARSE_TEMPL              = 2028;
/// required template size is too small
const int32_t  SE_SMALL_REQUIRED_SIZE      = 2030;
/// unsupported or invalid format of image or image data is corrupted
const int32_t  SE_WRONG_IMAGE              = 2032;
/// liveness requirement is not met
const int32_t  SE_LIVENESS                 = 2036;

/////////////////////////////////////////////////////////////////////////////////////////
// Matching algorithm return values
/// the number of fingers that was passed to function is wrong (should be >= 1 and <= 10)
const int32_t  SE_WRONG_FINGER_NUMBER      = 12002;
/// probe template has wrong format
const int32_t  SE_WRONG_PROBE_TEMPLATE     = 12010;
/// gallery template has wrong format
const int32_t  SE_WRONG_GALLERY_TEMPLATE   = 12011;
/// error of template set loading
const int32_t  SE_LOAD                     = 12012;
/// matching error
const int32_t  SE_MATCH                    = 12014;
/// two set of templates have no fingers with the same number or all templates passed to loadTemplate function are NULL
const int32_t  SE_NO_FINGERS               = 12016;
/// function 'matchEx' was called before load the first set of templates by function 'loadTemplate' or no templates was passed to 'loadTemplate'
const int32_t  SE_NO_LOAD                  = 12018;
/// wrong number of finger  (should be >= 1 and <= 10)
const int32_t  SE_WRONG_FINGER_NUM         = 12020;
/// the number of finger pairs is less than required by minMatch parameter
const int32_t  SE_NOT_ENOUGH_FINGERS       = 12024;
/// You are trying to unpack template that already unpacked
const int32_t  SE_UNPACKED_TEMPL           = 12030;
/// the pointer to the fpTemplate in TemplateData structure is NULL
const int32_t  SE_NULL_TEMPLATE            = 12032;
/// error of initialize unpack library
const int32_t  SE_UNPACK_INIT              = 12034;
/// the CPU is not support the SSE3 instruction set
const int32_t  SE_NOT_SUPPORT_SSE3         = 12036;
/// some finger of tenprints wasn't loaded
const int32_t  SE_NOT_ALL_FINGES_LOADED    = 12038;
/// the CPU is not support the SSE4.2 instruction set
const int32_t  SE_NOT_SUPPORT_SSE4_2       = 12040;

/////////////////////////////////////////////////////////////////////////////////////////
// I/O operation return values
/// cannot open or create the specified file
const int32_t  SE_OPEN_FILE                = 22000;
/// error occurs while writing to the specified file
const int32_t  SE_WRITE_FILE               = 22001;
/// error occurs while reading from the specified file
const int32_t  SE_READ_FILE                = 22002;

const uint8_t  GENDER_UNSPECIFIED = 0;
const uint8_t  GENDER_MALE        = 1;
const uint8_t  GENDER_FEMALE      = 2;

const int32_t  MAX_FINGER_COUNT   = 4;

#ifdef __cplusplus
using namespace Tech5_core;
#endif


/** @brief Enum keeps information about image format
    @class IMAGE_FORMAT
*/
ENUM_DECLARATION FINGER_IMAGE_FORMAT
{
    /// RAW image format.
    FIF_RAW = 0,
    /// WSQ image format.
    FIF_WSQ = 1,
    /// BMP image format.
    FIF_BMP = 2,
    /// PNG image format.
    FIF_PNG = 3
};


ENUM_DECLARATION CaptureStatus
{
    tooFewFingers           = 0,
    tooManyFingers          = 1,
    wrongAngle              = 2,
    wrongHand               = 3,
    tooFar                  = 4,
    tooClose                = 5,
    lowFocus                = 6,
    goodFocus               = 7,
    bestFrameChosen         = 8,
    frameSkipped            = 9,
    wrongFingerPositionCode = 10,
    turnOnFlashlight        = 11,
    set1xZoom               = 12,
    turnOffFlashlight       = 13
};


struct SgmRectImageEx
{
    uint8_t            *image;
    uint32_t            width;
    uint32_t            height;

    uint8_t            *compressedImage;
    uint32_t            compressedImageSize;

    enum NIST_POS_CODE  pos;            // friction ridge position code (POS_CODE_...)
    bool                focused;        // focused flag
    float               distance;       // distance: range: 0.0 - 1.0;
                                        //           0.0 - too close;
                                        //           1.0 - too far.
    float               focusDistance;  // focus distance, diopters.
    int32_t             coords[4][2];   // rectangle coordinates:
                                        //  coords[vertex index][coordinate index]:
                                        //   vertex index: 0 - left-top
                                        //                 1 - right-top
                                        //                 2 - left-bottom
                                        //                 3 - right-bottom
                                        //   coordinate index: 0 - X coordinate
                                        //                     1 - Y coordinate
#ifdef __cplusplus
    SgmRectImageEx()
    {
        image               = nullptr;
        width               = 0;
        height              = 0;
        compressedImage     = nullptr;
        compressedImageSize = 0;
        pos                 = NIST_POS_CODE::POS_CODE_U_FINGER;
        focused             = false;
        distance            = 0.5f;
        focusDistance       = 10.0f;

        memset(coords, 0, sizeof(coords));
    }

    ~SgmRectImageEx()
    {
        if (image)
        {
            delete[] image;

            image  = nullptr;
            width  = 0;
            height = 0;
        }

        if (compressedImage)
        {
            delete[] compressedImage;

            compressedImage     = nullptr;
            compressedImageSize = 0;
        }
    }
#endif
};

#if defined(__cplusplus)
extern "C"
{
#endif

T5AIRSNAP_API const char *getVersion();
T5AIRSNAP_API const char* getLicenseURL();
T5AIRSNAP_API float getZoomFactor();
T5AIRSNAP_API float getExposureCompensation();
T5AIRSNAP_API void setLicenseURL(const char* licenseURL);
T5AIRSNAP_API const char *initSdk(int32_t *ret, const char *key);

T5AIRSNAP_API int32_t initNfiq2();
T5AIRSNAP_API int32_t initLivenessDetector();
T5AIRSNAP_API int32_t initReverseDetector();
T5AIRSNAP_API int32_t initThumbDetector();
T5AIRSNAP_API int32_t shutdownSdk();

T5AIRSNAP_API void setOrientationCheck(bool orientationCheck);
T5AIRSNAP_API void setOperatorMode(bool operatorMode);
T5AIRSNAP_API void setLivenessCheck(bool livenessCheck);
T5AIRSNAP_API void setLivenessThreshold(float livenessThreshold);
T5AIRSNAP_API void setPositionCode(enum NIST_POS_CODE positionCode);
T5AIRSNAP_API int32_t setDetectorThreshold(float detectorThreshold);
T5AIRSNAP_API void setCacheDir(const char *cacheDir);
T5AIRSNAP_API void setModelsDir(const char *modelsDir);
T5AIRSNAP_API void setCaptureId(const char *captureId);

T5AIRSNAP_API void setDeviceInfo(const char *manufacturer,
                                 const char *model, const char *osVersion);

T5AIRSNAP_API void setSaveSdkLogFlag(bool saveSdkLog);
T5AIRSNAP_API void setSaveFramesFlag(bool saveFrames);
T5AIRSNAP_API void setPropDenoiseFlag(bool propDenoise);
T5AIRSNAP_API void setCaptureSpeed(float captureSpeed);
T5AIRSNAP_API void setOutsideCaptureFlag(bool outsideCapture);

T5AIRSNAP_API void setFingerImageFormat(enum FINGER_IMAGE_FORMAT fingerImageFormat);
T5AIRSNAP_API void setWsqCompressionRatio(float compressionRatio);
T5AIRSNAP_API void setPngCompressionLevel(uint8_t compressionLevel);

T5AIRSNAP_API void setCropParameters(bool enabled, int32_t width, int32_t height,
                                     uint8_t paddingColor);

T5AIRSNAP_API void setCaptureMetadata(const char *metadata);

T5AIRSNAP_API void initCameraParameters(float sensorWidth, float sensorHeight,
                                        float focalLength, float zoomRatio);

T5AIRSNAP_API void initBorder(int32_t frameWidth, int32_t frameHeight,
                              int32_t viewWidth, int32_t viewHeight);

T5AIRSNAP_API int32_t getBorderRectangle(int32_t *left, int32_t *top,
                                         int32_t *right, int32_t *bottom);

T5AIRSNAP_API enum CaptureStatus analyzeImage(uint8_t *nv21Image,
                                              int32_t rotationDegrees,
                                              int32_t width, int32_t height,
                                              struct SgmRectImageEx *rects,
                                              int32_t *rectCount, float currDistance);

T5AIRSNAP_API enum CaptureStatus analyzeSavedImage(const char *filename,
                                              int32_t rotationDegrees,
                                              struct SgmRectImageEx *rects,
                                              int32_t *rectCount, float currDistance);

T5AIRSNAP_API enum CaptureStatus analyzeImageYvu(uint8_t *yBuffer, uint8_t *vuBuffer,
                                                 int32_t rotationDegrees,
                                                 int32_t width, int32_t height,
                                                 struct SgmRectImageEx *rects,
                                                 int32_t *rectCount, float currDistance);

T5AIRSNAP_API enum CaptureStatus analyzeImageYuv(uint8_t *yBuffer, int32_t yBufferSize,
                                                 int32_t yRowStride, int32_t yPixelStride,
                                                 uint8_t *uBuffer, int32_t uBufferSize,
                                                 int32_t uRowStride, int32_t uPixelStride,
                                                 uint8_t *vBuffer, int32_t vBufferSize,
                                                 int32_t vRowStride, int32_t vPixelStride,
                                                 int32_t rotationDegrees,
                                                 int32_t width, int32_t height,
                                                 struct SgmRectImageEx *rects,
                                                 int32_t *rectCount, float currDistance);

T5AIRSNAP_API int32_t checkReverse(bool *reverseFlag);
T5AIRSNAP_API int32_t checkThumb(bool *wrongThumb);

T5AIRSNAP_API int32_t segmentFingers(uint8_t *grayBuffer, int32_t *width, int32_t *height,
                                     struct SgmRectImageEx *rects, int32_t *rectsCount,
                                     uint8_t gender, uint8_t age, bool clean,
                                     float* livenessScores);

T5AIRSNAP_API int32_t createTemplateMinex(struct RawImage *const rawImages, uint32_t numFingers,
                                          enum MinexTemplateType minexTemplateType,
                                          uint8_t *templateBuffer, uint8_t *nistQuality,
                                          uint8_t *quality, uint32_t *minutiaesNumber,
                                          uint32_t *templateSize, int32_t maxTemplateSize,
                                          bool certifiedSensor, uint16_t sensorId);

T5AIRSNAP_API int32_t getNistFingerImageQuality(uint8_t *rawImage, int32_t width, int32_t height,
                                                uint8_t *quality);
                                            
T5AIRSNAP_API int32_t getNistFingerImageQuality2(uint8_t *rawImage, int32_t width, int32_t height,
                                                 uint8_t *quality);

T5AIRSNAP_API int32_t getFingerprintQuality(uint8_t *rawImage, int32_t width, int32_t height,
                                            uint8_t *quality);

T5AIRSNAP_API void initSgmRectImages(struct SgmRectImageEx *rects, int32_t rectsCount);
T5AIRSNAP_API void freeSgmRectImages(struct SgmRectImageEx *rects, int32_t rectsCount);

#if defined(__cplusplus)
}
#endif

#endif // T5_AirSnap_H
