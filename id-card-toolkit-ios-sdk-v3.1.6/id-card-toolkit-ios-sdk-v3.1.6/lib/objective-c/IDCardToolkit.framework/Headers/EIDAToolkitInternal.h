/****************************************************************************
* Copyright (C) 2017 by Identity & Citizenship Authority (ICA)             *
*                                                                          *
* This file is part of Emirates ID Card Toolkit.                           *
*                                                                          *
*   The Toolkit provides functionality to access Emirates ID Card and      *
*   corressponding online services of ICA Validation Gateway (VG).         *
*                                                                          *
****************************************************************************/

#ifndef EIDA_TOOLKIT_INTERNAL_H_
#define EIDA_TOOLKIT_INTERNAL_H_

// Platform specific header files
#if defined (__MACH__)
#include <TargetConditionals.h>
#endif

#if __GNUC__ >= 4

#ifdef __cplusplus
#define ETAPI extern "C" __attribute__((visibility("default")))
#else
#define ETAPI __attribute__ ((visibility("default")))
#endif

#else

#ifdef __cplusplus
#define ETAPI extern "C"
#else
#define ETAPI
#endif

#endif

// This enumeration defines SDK programming language
typedef enum _SDK_PROGRAMMING_LANGUAGE {
    DOT_NET_SDK = 1,
    OBJECTIVE_C_SDK = 2,
    SWIFT_SDK = 3,
} SDK_PROGRAMMING_LANGUAGE;

/**
* @brief Initialize EIDA toolkit in agent mode
*
* @param[in] in_process_mode TRUE if toolkit is used as in-process module
* @param[in] config_params Application specific configuration parameters
* @param[out] init_context Pointer to initialize service context
* @param[out] set_context Pointer to set service context
* @param[out] cleanup_context Pointer to cleanup service context
* @param[out] Flag indicating if network error occurred while loading
*             config files
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI unsigned int InitializeEx(int in_process_mode, char *local_config_data,
    unsigned int config_data_len, char *config_ag_ap,
    unsigned int config_ag_ap_len, void **init_context, void **set_context,
    void **cleanup_context, BOOL *network_error);

/**
* @brief Set client sdk mode
*
* @param[in] client_sdk_mode Application type
* @param[in] platform_file_id Toolkit file id
* @param[in] code Secret code
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI unsigned int SetClientSDKMode(unsigned int client_sdk_mode,
    unsigned int platform_file_id, unsigned long code);

/** @brief Perform digital signing
*
* @param[in] jni_env Java environment object
* @param[in] signing_context Signing context data
* @param[in] doc_type Type of document to sign
* @param[out] signature Digital signature of the data
* @param[out] signature_len Digital signature length
* @param[out] attempts_left Attempts left if operation fails
* @param[in] packaging_mode Type of Packaging mode
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI unsigned int JniDssSign(void *jni_env, char *signing_context,
    unsigned int doc_type, unsigned char **signature,
    unsigned int *signature_len, int *attempts_left,
    unsigned int *packaging_mode, char **response,
    unsigned int *response_len);

/**
* @brief Perform digital signature verification
*
* @param[in] jni_env Java environment object
* @param[in] verification_context Digital signature verification context
* @param[in] doc_type Type of document to verify
* @param[out] verification_report Verification report
* @param[out] report_len Verification report length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI unsigned int JniDssVerify(void *jni_env, char *verification_context,
    unsigned doc_type, char **verification_report,
    unsigned int *report_len);

#if defined(_WIN32) || defined(_WIN64) || defined(LINUX) || defined(__MACH__) || defined(ANDROID)
/**
* @brief EIDA Toolkit API to export certificates for applet mode
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[in] request_id Application random number
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (SUCCESS / ERROR codes).
*/
ETAPI unsigned int GetPKICertificatesEx(unsigned int connection_handle,
    char *request_id, char **response, unsigned int *response_len);
#endif

// Set log callback
typedef ETSTATUS(*SET_LOG_CALLBACK)(char *message, unsigned int message_len);

// Set log callback
ETAPI void SetLogCallback(SET_LOG_CALLBACK set_log_callback);

/**
* @brief Set SDK programming language
*
* @param[in] sdk_language
*   1 -> DOT_NET_SDK
*   2 -> OJBECTIVE_C_SDK
*   3 -> SWIFT_SDK
* @return Return value (SUCCESS / ERROR codes).
*/
ETAPI ETSTATUS SetSDKLanguage(SDK_PROGRAMMING_LANGUAGE sdk_language);

#if TARGET_OS_IPHONE || TARGET_IPHONE_SIMULATOR || TARGET_OS_IOS
/**
* @brief Set device ID
*
* @param[in] device_id Device ID
* @param[in] device_id_len Device ID length
* @return Return value (SUCCESS / ERROR codes).
*/
ETAPI ETSTATUS SetDeviceId(unsigned char *device_id,
    unsigned int device_id_len);
#endif

#endif // EIDA_TOOLKIT_INTERNAL_H_
