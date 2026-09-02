/****************************************************************************
* Copyright (C) 2017 by Identity & Citizenship Authority (ICA)             *
*                                                                          *
* This file is part of Emirates ID Card Toolkit.                           *
*                                                                          *
*   The Toolkit provides functionality to access Emirates ID Card and      *
*   corressponding online services of ICA Validation Gateway (VG).         *
*                                                                          *
****************************************************************************/

#ifndef EIDA_TOOLKIT_DEVICE_PLUGIN_H_
#define EIDA_TOOLKIT_DEVICE_PLUGIN_H_

#ifdef __cplusplus
#define ETPAPI extern "C"
#else
#define ETPAPI
#endif

// This enumeration defines the type of the plugin
typedef enum _PLUGIN_TYPE {
    FINGER_PRINT = 0,
    SCARD_READER = 1,
    COMBI_READER = 2
} PLUGIN_TYPE, *LPPLUGIN_TYPE;

// This enumeration defines fingerprint template type
typedef enum _FP_TEMPLATE_TYPE {
    ISO = 0,
    DINV = 1
} FP_TEMPLATE_TYPE, *LPFP_TEMPLATE_TYPE;

// Plugin related constants
#define PLUGIN_NAME_MAX                 64
#define DEVICE_NAME_MAX                 64
#define PLUGIN_DESC_MAX                 128
#define PLUGIN_CAPABILITY_AVAILABLE     1
#define PLUGIN_CAPABILITY_UNAVAILABLE   0
#define PLUGIN_CAPABILITY_NOT_SET       -1

// This structure contains the captured fingerprint data as
// ISO Compact Card template
typedef struct _FINGER_PRINT_DATA {
    unsigned char* value;
    int length;
} FINGER_PRINT_DATA;

// This structure contains the captured fingerprint data as
// raw image
typedef struct _FTP_IMAGE {
    unsigned char* pixels;
    int width;
    int height;
    int finger_index;
    int quality;
} FTP_IMAGE;

// This structure contains the captured fingerprint template data
typedef struct _FTP_TEMPLATE {
    int finger_index;
    int format; //ISO(0), DINV(1)
    int quality;
    FINGER_PRINT_DATA finger_print_data;
} FTP_TEMPLATE;

// Plugin interface definitions

/**
* @brief Initialize the toolkit plugin.
*
* @param[in] context Java object containing the application context
* @param[in] jenv_obj JNI environment of the application of type JNIEnv*
* @return Return value (0 on success else error code)
*/
ETPAPI int Plugin_Initialize(
    void *context
#if defined(ANDROID)
    , void *jenv_obj
#endif
);

/**
* @brief Cleanup the plugin module.
*
* @param[in] jenv_obj JNI environment of the application of type JNIEnv*
* @return Return value (0 on success else error code)
*/
ETPAPI int Plugin_Cleanup(
#if defined(ANDROID)
    void *jenv_obj
#endif
);

/**
* @brief Gets the list of SmartCard readers connected.
*
* @param[out] reader_list List of smart card reader names separated by
*                         NULL character
* @param[out] num_bytes Length of the multistring returned in reader_list.
* @return Return value (0 on success else error code)
*/
ETPAPI int Plugin_ListReaders(
    char **reader_list,
    unsigned int *num_bytes
);

#if defined(ANDROID)
/**
* @brief Gets the list of SmartCard readers connected.
*
* @param[out] reader_list List of smart card reader names separated by
*                         NULL character
* @param[out] num_bytes Length of the multistring returned in reader_list.
* @param[in] jenv_obj JNI environment of the application of type JNIEnv*
* @return Return value (0 on success else error code)
*/
ETPAPI int Plugin_ListReadersEx(
    char **reader_list,
    unsigned int *num_bytes,
    void *jenv_obj
);
#endif

/**
* @brief Frees the memory buffers allocated by the plugin.
*
* @param[in] buffer Pointer to the buffer returned by a previous call to a
*                   plugin method
* @return Return value (0 on success else error code)
*/
ETPAPI int Plugin_FreeMemory(
    void *buffer
);

// Smart Card plugin interface

#if defined(ANDROID)
/**
* @brief This function is to set the NFC tag received by an Android application
upon the tapping of ID card on the NFC reader.
*
* @param[in] jnfc_tag Java object received by application upon tapping of
ID card on the NFC reader
* @param[in] jenv_obj JNI environment of the application of type JNIEnv*
* @return Return value (0 on success else error code)
*/
ETPAPI int Plugin_SetNfcTag(
    void *jnfc_tag,
    void *jenv_obj
);
#endif

/**
* @brief This function is to establish connection to the smartcard in
* the specific reader identified by the reader name
*
* @param[in] reader Name of the smartcard reader to connect to
* @param[out] plugin_context The context allocated by the plugin which is
*                            be passed as the input parameter to the plugin
*                            functions that require the connection state.
* @param[in] jenv_obj JNI environment of the application of type JNIEnv*
* @return Return value (0 on success else error code)
*/
ETPAPI int Plugin_Connect(
    char *reader,
    void **plugin_context
#if defined(ANDROID)
    , void* jenv_obj
#endif
);

/**
* @brief This method is to disconnect an already established connection to
* smartcard from a previous call to Plugin_Connect
*
* @param[in] plugin_context Pointer returned from a successful call to
*                           Plugin_Connect.
* @param[in] jenv_obj JNI environment of the application of type JNIEnv*
* @return Return value (0 on success else error code)
*/
ETPAPI int Plugin_Disconnect(
    void *plugin_context
#if defined(ANDROID)
    , void* jenv_obj
#endif
);

/**
* @brief This method is to get the Answer to Reset (ATR) value of a smartcard
*
* @param[in] plugin_context Pointer returned from a successful call to
*                           Plugin_Connect.
* @param[out] atr_bytes Buffer containing the ATR bytes.
* @param[out] atr_len Length of the ATR bytes.
* @param[in] jenv_obj JNI environment of the application of type JNIEnv*
* @return Return value (0 on success else error code)
*/
ETPAPI int Plugin_GetATR(
    void *plugin_context,
    unsigned char **atr_bytes,
    unsigned int *atr_len
#if defined(ANDROID)
    , void* jenv_obj
#endif
);

/**
* @brief This method is to execute APDU commands for accessing data from
* smartcard.
*
* @param[in] plugin_context Pointer returned from a successful call to
*                           Plugin_Connect.
* @param[in] isocommand Smartcard command in ISO 7816 format
* @param[in] command_length Length of the SmartCard command
* @param[out] out_buf Buffer containing the SmartCard response
* @param[out] out_length Length of the SmartCard response.
* @param[in] interface_type Smart card connection interface type
*                           (1 - contact interface / 2 - nfc interface).
* @param[in] jenv_obj JNI environment of the application of type JNIEnv*
* @return Return value (0 on success else error code)
*/
ETPAPI int Plugin_ExecuteCommand(
    void *plugin_context,
    unsigned char *isocommand,
    unsigned int command_length,
    unsigned char *out_buf,
    unsigned int *out_length,
    int interface_type
#if defined(ANDROID)
    , void* jenv_obj
#endif
);

// Fingerprint Scanner Plugin Interface

/**
* @brief This function is to establish connection with the fingerprint sensor
* associated with the plugin.
*
* @param[in] jenv_obj JNI environment of the application of type JNIEnv*
* @return Return value (0 on success else error code)
*/
ETPAPI int FP_Plugin_Connect(
#if defined(ANDROID)
    void* jenv_obj
#endif
);

/**
* @brief This function is to disconnect an already established connection from
* a previous call to FP_Plugin_Connect.
*
* @param[in] jenv_obj JNI environment of the application of type JNIEnv*
* @return Return value (0 on success else error code)
*/
ETPAPI int FP_Plugin_Disconnect(
#if defined(ANDROID)
    void* jenv_obj
#endif
);

/**
* @brief This function is to capture the fingerprint raw image from the sensor.
*
* @param[in] jenv_obj JNI environment of the application of type JNIEnv*
* @param[in] time_out Fingerprint sensor capture timeout in seconds.
* @param[out] image Pointer to receive fingerprint raw image data.
* @return Return value (0 on success else error code)
*/
ETPAPI int FP_Capture(
#if defined(ANDROID)
    void* jenv_obj,
#endif
    int time_out,
    FTP_IMAGE *image
);

/**
* @brief This function is to apture and convert the fingerprint data as
* ISO Compact Card template.
*
* @param[in] jenv_obj JNI environment of the application of type JNIEnv*
* @param[in] time_out Fingerprint sensor capture timeout in seconds.
* @param[out] ftp_template Pointer to receive fingerprint template data.
* @return Return value (0 on success else error code)
*/
ETPAPI int FP_CaptureAndConvert(
#if defined(ANDROID)
    void* jenv_obj,
#endif
    int time_out,
    FTP_TEMPLATE *ftp_template
);

#endif // EIDA_TOOLKIT_DEVICE_PLUGIN_H_
