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

/*
 * ============================================================
 * PLUGIN INTERFACE v2 -- reader monitoring extensions
 *
 * All new exports are OPTIONAL. The device manager resolves them
 * via PsaGetSymbolAddress. NULL function pointer = not supported.
 * Old plugins that export none of these continue to work unchanged.
 * ============================================================
 */

#define PLUGIN_INTERFACE_VERSION    2u

/* Plugin return codes -- used by the optional reader-monitoring exports
 * below. Existing mandatory exports continue to return ETSTATUS_* codes. */
#define PLUGIN_OK                        0
#define PLUGIN_ERROR_GENERAL            -1
#define PLUGIN_ERROR_INVALID_PARAMETER  -2
#define PLUGIN_ERROR_READER_UNAVAILABLE -3

typedef struct {
    unsigned int  interface_version;
    unsigned int  flags;
    unsigned int  max_connections;
    char          plugin_version[32];
} PLUGIN_CAPABILITIES;

/* Capability flags -- returned in PLUGIN_CAPABILITIES.flags */
#define PLUGIN_CAP_MONITORING       0x0001u
#define PLUGIN_CAP_HOTPLUG          0x0002u
#define PLUGIN_CAP_MULTI_CONNECT    0x0004u
#define PLUGIN_CAP_TRANSACTIONS     0x0008u

/* Reader state bitmask */
#define PLUGIN_READER_STATE_UNKNOWN     0x00u
#define PLUGIN_READER_STATE_EMPTY       0x01u
#define PLUGIN_READER_STATE_PRESENT     0x02u
#define PLUGIN_READER_STATE_MUTE        0x04u
#define PLUGIN_READER_STATE_UNAVAILABLE 0x08u
#define PLUGIN_READER_STATE_EXCLUSIVE   0x10u

/* Return codes for wait operations */
#define PLUGIN_WAIT_TIMEOUT         2
#define PLUGIN_WAIT_CANCELLED       3

/* Pass as timeout_ms to block indefinitely */
#define PLUGIN_WAIT_INFINITE        0xFFFFFFFFu

/* ---- Monitoring lifecycle + capability optional function pointer typedefs ---- */

/* Named Plugin_MonitorInit (not Plugin_Initialize) to avoid conflict with
 * the existing Plugin_Initialize(void *context) prototype above. */
typedef int (*PFN_PLUGIN_MONITOR_INIT)(void);
typedef int (*PFN_PLUGIN_MONITOR_CLEANUP)(void);
typedef int (*PFN_PLUGIN_GET_CAPABILITIES)(PLUGIN_CAPABILITIES *out);
typedef long (*PFN_PLUGIN_GET_LAST_ERROR)(void);

/* ---- Card-change-wait optional function pointer typedefs ---- */

typedef int (*PFN_PLUGIN_GET_READER_STATUS)(const char   *reader_name,
                                             unsigned int *out_state);

/*
 * Plugin_WaitForCardChange(reader_names, count, timeout_ms, out_states)
 *   Blocking. reader_names is an array of count null-terminated strings.
 *   out_states is caller-allocated (count entries). On return:
 *     - Changed readers: out_states[i] contains PLUGIN_READER_STATE_* flags
 *     - Unchanged readers: out_states[i] is PLUGIN_READER_STATE_UNKNOWN (0x00)
 *   The caller must zero-initialise out_states before calling.
 *   ErtWaitForCardChange does this automatically.
 *
 *   THREAD SAFETY: Must be called from a single monitoring thread.
 *   Concurrent calls to Plugin_GetReaderStatus and Plugin_WaitForCardChange
 *   are not supported -- the state cache has no synchronisation.
 *
 *   STATE CACHE: The plugin maintains a per-reader dwCurrentState cache.
 *   SCARD_STATE_UNAWARE on first call for a reader causes an immediate
 *   return with current state. Subsequent calls wait for actual changes.
 *
 *   Returns PLUGIN_OK (state changed), PLUGIN_WAIT_TIMEOUT,
 *   or PLUGIN_WAIT_CANCELLED.
 */
typedef int (*PFN_PLUGIN_WAIT_FOR_CARD_CHANGE)(const char  **reader_names,
                                                unsigned int  count,
                                                unsigned int  timeout_ms,
                                                unsigned int *out_states);

typedef int (*PFN_PLUGIN_CANCEL_WAIT)(void);

typedef int (*PFN_PLUGIN_WAIT_FOR_READER_CHANGE)(unsigned int timeout_ms);

/*
 * ---- Card-transaction optional function pointer typedefs ----
 *
 * Plugin_BeginCardTransaction(plugin_context) /
 * Plugin_EndCardTransaction(plugin_context)
 *   OPTIONAL exports. plugin_context is the same opaque connection object
 *   returned by Plugin_Connect. Begin acquires exclusive access to the card
 *   for the calling process (PC/SC: SCardBeginTransaction) so an APDU
 *   sequence cannot be interleaved by another process (e.g. the Windows
 *   certificate-propagation service driving a registered minidriver).
 *   End releases it leaving the card state unchanged (SCARD_LEAVE_CARD).
 *
 *   Callers must keep the bracketed sequence short and APDU-dense: the
 *   Windows resource manager force-releases a transaction whose owner
 *   sends no command for ~5 seconds while another process waits.
 *
 *   Returns PLUGIN_OK, PLUGIN_ERROR_INVALID_PARAMETER, or
 *   PLUGIN_ERROR_GENERAL. Plugins that do not export these are treated
 *   as not supporting transactions and the device manager no-ops.
 */
typedef int (*PFN_PLUGIN_BEGIN_CARD_TRANSACTION)(void *plugin_context);
typedef int (*PFN_PLUGIN_END_CARD_TRANSACTION)(void *plugin_context);

/*
 * ============================================================
 * PLUGIN LOGGER INJECTION -- added v3.0.6
 *
 * The toolkit calls Plugin_SetLogger on any plugin that exports it,
 * passing a PFN_TOOLKIT_LOGGER function pointer. The plugin stores
 * this pointer and uses it for all diagnostic output. The toolkit
 * calls Plugin_ClearLogger before unloading the plugin.
 *
 * Both Plugin_SetLogger and Plugin_ClearLogger are OPTIONAL exports.
 * Plugins that do not implement them continue to work without change.
 * Old toolkits that do not call them: plugin operates without logger.
 *
 * Log level constants map to toolkit internal levels:
 *   PLUGIN_LOG_ERROR -> LOGE
 *   PLUGIN_LOG_WARN  -> LOGW
 *   PLUGIN_LOG_INFO  -> LOGI
 *   PLUGIN_LOG_DEBUG -> dropped (no LOGD in toolkit; suppressed at the
 *                       toolkit bridge to keep idle monitoring quiet --
 *                       a high-frequency caller such as PCSCLib's poll
 *                       loop would otherwise emit one line per cycle)
 *
 * Usage inside a plugin:
 *   _toolkit_logger(PLUGIN_LOG_INFO, "PCSCLib", "message text");
 *
 * The toolkit formats log output as: [name] message
 * ============================================================
 */

#define PLUGIN_LOG_ERROR    0
#define PLUGIN_LOG_WARN     1
#define PLUGIN_LOG_INFO     2
#define PLUGIN_LOG_DEBUG    3

/*
 * PFN_TOOLKIT_LOGGER
 *   Function pointer type for the toolkit logger injected into plugins.
 *   level:   PLUGIN_LOG_* constant
 *   name:    short plugin identifier, appears as [name] in toolkit log
 *   message: null-terminated message string
 *
 *   The toolkit guarantees this pointer remains valid from Plugin_SetLogger
 *   until Plugin_ClearLogger returns. The plugin must not call it after
 *   Plugin_ClearLogger returns.
 */
typedef void (*PFN_TOOLKIT_LOGGER)(int         level,
                                    const char *name,
                                    const char *message);

/*
 * Plugin_SetLogger(logger)
 *   Optional export. Called by the toolkit after successful plugin load.
 *   The plugin stores logger and uses it for subsequent diagnostic output.
 *   If not exported, the device manager skips logger injection silently.
 */
typedef void (*PFN_PLUGIN_SET_LOGGER)(PFN_TOOLKIT_LOGGER logger);

/*
 * Plugin_ClearLogger()
 *   Optional export. Called by the toolkit before plugin unload.
 *   The plugin must NULL its stored logger pointer and make no further
 *   logger calls after this returns.
 *   Guaranteed to be called before PsaFreeModule.
 */
typedef void (*PFN_PLUGIN_CLEAR_LOGGER)(void);

#endif // EIDA_TOOLKIT_DEVICE_PLUGIN_H_
