/****************************************************************************
* Copyright (C) 2017-2026 by Federal Authority for Identity, Citizenship, *
* Customs & Port Security (ICP).                                          *
*                                                                         *
* This file is part of UAE ID Card Toolkit.                               *
*                                                                         *
*   The Toolkit provides functionality to access UAE ID Card and          *
*   corresponding online services of ICP Validation Gateway (VG).         *
*                                                                         *
****************************************************************************/

/**
 * @file EIDAToolkitApi.h
 * @brief Public API function declarations for the Emirates ID Card Toolkit.
 *
 * All functions return ETSTATUS. Use ETSTATUS_CODE() to extract the
 * error code for comparison with constants in EIDAToolkitError.h.
 *
 * Functions that return data via char** or unsigned char** output
 * parameters allocate memory internally. The caller MUST free these
 * buffers using FreeMemory() when no longer needed.
 *
 * Typical calling sequence:
 * @code
 *   Initialize(TRUE, config_json);
 *   ListReaders(&readers, &len, &count);
 *   Connect(reader_name, &handle);
 *   ReadPublicData(handle, request_id, TRUE, TRUE, TRUE, FALSE, FALSE,
 *                  &response, &response_len);
 *   // ... use response ...
 *   FreeMemory(response);
 *   Disconnect(handle);
 *   Cleanup();
 * @endcode
 */

#ifndef EIDA_TOOLKIT_API_H_
#define EIDA_TOOLKIT_API_H_

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

/**
* @brief Initialize the Emirates ID Card Toolkit
*
* Must be called before any other Toolkit API. Call Cleanup() when done.
*
* @param[in] in_process_mode TRUE for in-process mode (DLL loaded directly),
*            FALSE for agent mode (communicates via Agent service)
* @param[in] config_params JSON string with configuration, or NULL for defaults.
*            In agent mode, pass the agent URL. In in-process mode, pass a JSON
*            string containing at minimum the config directory path:
*            @code
*            "{\"config_directory\":\"C:/path/to/config\"}"
*            @endcode
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*
* @note Typical calling sequence:
*       Initialize() -> Connect() -> PrepareRequest() -> [operations] ->
*       Disconnect() -> Cleanup()
*
* @warning Calling other APIs before Initialize() returns ETSTATUS_NOT_INITIALIZED.
*
* @see Cleanup()
*/
ETAPI ETSTATUS Initialize(BOOL in_process_mode, char *config_params);

/**
* @brief Cleanup the Emirates ID Card Toolkit
*
* Releases all resources allocated by Initialize(). Any open card
* connections should be closed with Disconnect() before calling this.
*
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*
* @see Initialize()
*/
ETAPI ETSTATUS Cleanup();

/**
* @brief Cleanup EF data memory
*
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS CleanupEFDataMemory();

/**
* @brief Free a memory buffer allocated by this module
*
* Any API that returns data via a char** or unsigned char** output
* parameter allocates memory internally. The caller MUST free this
* memory by calling FreeMemory() when the data is no longer needed.
*
* @param[in] buffer Memory buffer to free (returned by a Toolkit API)
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*
* @warning Failing to call FreeMemory() causes memory leaks. Do NOT
*          use free() directly on Toolkit-allocated buffers.
*/
ETAPI ETSTATUS FreeMemory(void *buffer);

/**
* @brief Get the status message corresponding to an ETSTATUS value
*
* Almost all the Toolkit APIs return the status as an ETSTATUS value.
* This function helps developers to get the text message corresponding
* to the status code.
*
* The returned buffer is **caller-owned**. The caller MUST release it by
* calling FreeMemory() on the returned pointer. Failing to do so leaks
* ~30-50 bytes per call.
*
* @param[in] status Toolkit status value
* @return Caller-owned heap buffer containing the status message string,
*         or NULL on allocation failure. Caller MUST free via FreeMemory().
*/
ETAPI char *GetStatusMessage(ETSTATUS status);

#if !defined(TOOLKIT_VERIFONE_SUPPORT)
/**
* @brief Gets the List of readers connected.
*
* @param[out] readers_list Return buffer of readers list.
* @param[out] list_length Readers name list length in bytes.
* @param[out] readers_count Number of reader names in the list.
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS ListReaders(char **readers_list, unsigned int *list_length,
    unsigned int *readers_count);

/**
* @brief Get first reader with Emirates ID Card inserted
*
* @param[in] reader_name Connected reader name
* @param[inout] reader_name_max Reader name length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS GetReaderWithEmiratesID(char *reader_name,
    int *reader_name_max);

/**
* @brief Establish connection with the smartcard in the reader
*
* @param[in] reader_name Smartcard reader name (from ListReaders())
* @param[out] connection_handle Handle to smartcard connection.
*             Pass this handle to subsequent card operations.
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*
* @note Call Disconnect() to release the connection when done.
*
* @see ListReaders(), GetReaderWithEmiratesID(), Disconnect()
*/
ETAPI ETSTATUS Connect(char *reader_name, unsigned int *connection_handle);

/**
* @brief Disconnect smartcard session
*
* @param[in] connection_handle Handle to smartcard connection
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS Disconnect(unsigned int connection_handle);

/**
* @brief Prepare service request
*
* Initiates a new VG session. Must be called before each card operation
* that requires VG interaction (ReadPublicData, CheckCardStatus,
* AuthenticateBiometric*, etc.).
*
* @param[in] connection_handle Handle to an Emirates ID Card connection
* @param[in] request_id Application-generated random string for request tracking
* @param[out] request_handle 8-byte request handle encoded in base64 format.
*             Caller must free with FreeMemory().
* @param[out] request_handle_len Length of the request handle
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*
* @see Connect(), ReadPublicData(), CheckCardStatus()
*/
ETAPI ETSTATUS PrepareRequest(unsigned int connection_handle, char *request_id,
    char **request_handle, unsigned int *request_handle_len);

/**
* @brief Registers a device with Validation Gateway (VG) against the
* Service Provider (SP) license
*
* Must be called once per device before any card operations can be
* performed. The user credentials must be encrypted using the data
* protection key.
*
* @param[in] user_id Authorised Service Provider (SP) user id (encrypted)
* @param[in] password Encrypted password of the user
* @param[in] device_reference_id Device registration reference number
* @param[out] response XML response from VG. Caller must free with FreeMemory().
* @param[out] response_len Response length in bytes
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*
* @see GetDataProtectionKey(), GetDeviceID()
*/
ETAPI ETSTATUS RegisterDevice(char *user_id, char *password,
    char *device_reference_id, char **response, unsigned int *response_len);

/**
* @brief Read public data from the Emirates ID Card
*
* Retrieves cardholder public data from the card via the Validation
* Gateway (VG). Use the boolean flags to select which data to read.
* Reading only the required data improves performance.
*
* Requires: Initialize() + Connect() called first.
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[in] request_id Application generated random request ID
* @param[in] non_modifiable_data TRUE to read name, DOB, nationality, etc.
* @param[in] modifiable_data TRUE to read phone, email, etc.
* @param[in] photography TRUE to read cardholder photo (JPEG, base64 in response)
* @param[in] signature_image TRUE to read handwritten signature image
* @param[in] address TRUE to read home and work address
* @param[out] response XML response string from VG. Caller must free
*             with FreeMemory().
* @param[out] response_len Response length in bytes
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*
* @see FreeMemory()
*/
ETAPI ETSTATUS ReadPublicData(unsigned int connection_handle, char *request_id,
    int non_modifiable_data, int modifiable_data, int photography,
    int signature_image, int address, char **response,
    unsigned int *response_len);

/**
* @brief Toolkit API to read family book data from the card
*
* @param[out] pin Emirates ID Card PIN encrypted
* @param[out] pin_len The encrypted PIN length
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS ReadFamilyBookData(char *pin, int pin_len, int *attempts_left,
    char **response, unsigned int *response_len);

/**
* @brief Toolkit API to export certificates
*
* @param[in] pin Emirates ID Card PIN encrypted
* @param[in] pin_len Length of the encrypted PIN
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (SUCCESS / ERROR codes).
*/
ETAPI ETSTATUS GetPKICertificates(char *pin, unsigned int pin_len,
    int *attempts_left, char **response, unsigned int *response_len);

/**
* @brief Verify certificate using crl
*
* @param[in] crl_path crl path
* @param[in] crl_filetype CRL file type (X509_FILETYPE_PEM = 1/
* X509_FILETYPE_ASN1 = 2)
* @param[in] cert_data Certificate data by which data is signed.
* @param[in] cert_data_len Certificate data length.
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS VerifyCertificateUsingCRL(unsigned char *crl_path,
    unsigned int crl_filetype, unsigned char *cert_data,
    unsigned int cert_data_len);

/**
* @brief This function checks the card status with the help of 
* Validation Gateway (VG)
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[in] request_id Application random number
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS CheckCardStatus(unsigned int connection_handle,
    char *request_id, char **response, unsigned int *response_len);

/**
* @brief Get stored finger indexes from the Emirates ID Card
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[out] first_finger_id First finger id
* @param[out] first_finger_index First finger index
* @param[out] second_finger_id Second finger id
* @param[out] second_finger_index Second finger index
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS GetFingerIndex(int connection_handle,
    unsigned char *first_finger_id, unsigned char *first_finger_index,
    unsigned char *second_finger_id, unsigned char *second_finger_index);

/**
* @brief Perform biometric authentication on server (Validation Gateway - VG)
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[in] request_id Application random number
* @param[in] finger_index Index of finger to be used for bio. authentication
* @param[in] sensor_timeout Fingerprint sensor capture timeout in seconds
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS AuthenticateBiometricOnServer(unsigned int connection_handle,
    char *request_id, unsigned int finger_index, int sensor_timeout,
    char **response, unsigned int *response_len);

/**
* @brief Reset PIN on Emirates ID Card
*
* @param[in] pin New PIN to set on the Emirates ID Card in encrypted format
* @param[in] pin_len Length of the encrypted PIN
* @param[in] finger_index Index of finger to be used for bio. authentication
* @param[in] finger_id Finger ID to be used for bio. authentication
* @param[in] sensor_timeout Fingerprint sensor capture timeout in seconds
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS ResetPIN(char *pin, unsigned int pin_len,
    unsigned int finger_index, unsigned int finger_id, int sensor_timeout,
    int *attempts_left);

/**
* @brief Reset PIN Ex on Emirates ID Card
*
* @param[in] pin New PIN to set on the Emirates ID Card in encrypted format
* @param[in] pin_len Length of the encrypted PIN
* @param[in] finger_index Index of finger to be used for bio. authentication
* @param[in] finger_id Finger ID to be used for bio. authentication
* @param[in] sensor_timeout Fingerprint sensor capture timeout in seconds
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS ResetPINEx(char *pin, unsigned int pin_len,
    unsigned int finger_index, unsigned int finger_id, int sensor_timeout,
    int *attempts_left, char **response, unsigned int *response_len);

/**
* @brief Toolkit API to unblock Emirates ID card PKI PIN
*
* @param[in] pin Emirates ID Card PIN encrypted
* @param[in] pin_len Length of the encrypted PIN
* @param[in] finger_index Index of finger to be used for bio. authentication
* @param[in] finger_id Finger identifier
* @param[in] sensor_timeout Fingerprint sensor capture timeout in seconds
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS UnblockPIN(char *pin, unsigned int pin_len,
    unsigned int finger_index, unsigned int finger_id, int sensor_timeout,
    int *attempts_left);

/**
* @brief Toolkit API to unblock Emirates ID card PKI PIN
*
* @param[in] pin Emirates ID Card PIN encrypted
* @param[in] pin_len Length of the encrypted PIN
* @param[in] finger_index Index of finger to be used for bio. authentication
* @param[in] finger_id Finger identifier
* @param[in] sensor_timeout Fingerprint sensor capture timeout in seconds
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS UnblockPINEx(char *pin, unsigned int pin_len,
    unsigned int finger_index, unsigned int finger_id, int sensor_timeout,
    int *attempts_left, char **response, unsigned int *response_len);

/**
* @brief Perform Emirates ID Card PKI authentication
*
* @param[in] pin Emirates ID Card PIN encrypted
* @param[in] pin_len Length of the encrypted PIN
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS AuthenticatePKI(char *pin, unsigned int pin_len,
    int *attempts_left, char **response, unsigned int *response_len);

/** @brief Digitally sign data or hash provided
*
* @param[in] data Plain data or hash to sign
* @param[in] data_length Length of plain data or hash provided
* @param[in] data_hash TRUE if hash/FALSE if plain data
* @param[in] pin Emirates ID Card PIN encrypted
* @param[in] pin_len Length of the encrypted PIN
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS SignData(unsigned char *data, unsigned int data_len,
    BOOL data_hash, char* pin, unsigned int pin_len, int *attempts_left,
    char **response, unsigned int *response_len);

/** @brief Digitally sign challenge or hash provided
*
* @param[in] challenge Plain data or hash to sign
* @param[in] challenge_len Length of the challenge
* @param[in] data_hash TRUE if hash/FALSE if plain data
* @param[in] pin Emirates ID Card PIN encrypted
* @param[in] pin_len Length of the encrypted PIN
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS SignChallenge(unsigned char *challenge,
    unsigned int challenge_len, BOOL data_hash, char* pin,
    unsigned int pin_len, int *attempts_left, char **response,
    unsigned int *response_len);

/**
* @brief Verify digital signature
*
* @param[in] data Plain data or hash for which signature is verified
* @param[in] data_len Plain data or hash length
* @param[in] signature Digital signature of the data
* @param[in] signature_len Digital signature length
* @param[in] certificate Certificate data by which data is signed.
* @param[in] certificate_len Certificate data length.
* @param[in] data_hash TRUE if hash/FALSE if plain data.
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS VerifySignature(unsigned char *data, unsigned int data_len,
    unsigned char *signature, unsigned int signature_len,
    unsigned char *certificate, unsigned int certificate_len, BOOL data_hash);

/**
* @brief Set NFC Authentication params for the Emirates ID Card
*
* @param[in] connection_handle Emirates ID card connection handle
* @param[in] card_number Card Number (CN)
* @param[in] date_of_birth Card holder Date of Birth (DoB)
* @param[in] expiry_date Emirates ID Card expiry date
* @param[in] mrz_data Data from the Machine Readable Zone (MRZ) of the card
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS SetNfcAuthenticationParams(int connection_handle,
    char *card_number, char *date_of_birth, char *expiry_date, char *mrz_data);

/** @brief Gets the Card Serial Number (CSN)
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[out] csn Card serial number (CSN)
* @param[out] csn_len Card Serial Number (CSN) length
* @return Return value (SUCCESS / ERROR codes).
*/
ETAPI ETSTATUS GetCSN(int connection_handle, char **csn,
    unsigned int *csn_len);

/** @brief Retrieves the interface type from plugin context
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[out] interface_type Connection interface type.
* @return Return value (SUCCESS / ERROR codes).
*/
ETAPI ETSTATUS GetInterfaceType(int connection_handle, int *interface_type);

/**
* @brief Check if Emirates ID Card is Genuine.
* This function is used only with SAM
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[in] request_id Application random number
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS IsCardGenuine(unsigned int connection_handle, char *request_id,
    int *attempts_left, char **response, unsigned int *response_len);

/**
* @brief Toolkit API to Get card version
*
* This API can be called to retrieve the Emirates ID Card version number.
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[out] card_version Emirates ID Card version
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS GetCardVersion(int connection_handle,
    unsigned char *card_version);

/**
* @brief Toolkit API to read Card Production Life Cycle (CPLC)
* data from card
*
* @param[in] connection_handle Handle to an Emirates ID Card connection handle
* @param[out] cplc_info Card Production Life Cycle (CPLC) data
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS GetCPLCData(int connection_handle, CPLC_INFO *cplc_info);

/**
* @brief Get the Toolkit Version
*
* @param[out] major_version EIDAToolkit major version number
* @param[out] minor_version EIDAToolkit minor version number
* @param[out] patch_version EIDAToolkit patch version number
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS GetToolkitVersion(int *major_version, int *minor_version,
    int *patch_version);

#if defined(_WIN32) || defined(_WIN64) || defined(LINUX) || defined(__MACH__)

/**
* @brief Perform digital signing of PDF document following
* PDF Advanced Electronic Signature (PAdES) standard
*
* @param[in] signing_context Toolkit digital signature context
* @param[in] pdf_file_path Path of the input pdf document
* @param[in] signed_pdf_file_path Path of the signed pdf document
* @param[in] pades_params Additional PAdES specific parameters
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS PadesSign(SIGNING_CONTEXT *signing_context, char *pdf_file_path,
    char *signed_pdf_file_path, PADES_SIGN_PARAMS *pades_params,
    int *attempts_left);

/**
* @brief Perform digital signing of PDF document following
* PDF Advanced Electronic Signature (PAdES) standard
*
* @param[in] signing_context Toolkit digital signature context
* @param[in] pdf_file_path Path of the input pdf document
* @param[in] signed_pdf_file_path Path of the signed pdf document
* @param[in] pades_params Additional PAdES specific parameters
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS PadesSignEx(SIGNING_CONTEXT *signing_context,
    char *pdf_file_path, char *signed_pdf_file_path,
    PADES_SIGN_PARAMS *pades_params, int *attempts_left, char **response,
    unsigned int *response_len);

/**
* @brief Perform digital signature verification of PDF document following
* PDF Advanced Electronic Signature (PAdES) standard
*
* @param[in] verification_context Digital signature verification context
* @param[in] pdf_file_path Path of the input pdf document to verify
* @param[out] verification_report Verification report
* @param[out] report_len Verification report length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS PadesVerify(VERIFICATION_CONTEXT *verification_context,
    char *pdf_file_path, char **verification_report, unsigned int *report_len);

/**
* @brief Perform digital signing of XML document following 
* XML Advanced Electronic Signature (XAdES) standard
*
* @param[in] signing_context Toolkit digital signature context
* @param[in] xml_file_path Path of the input xml document
* @param[in] signed_xml_file_path Path of the signed xml document
* @param[out] signature signature data if packaging mode is Detached
* @param[out] signature_len signature data length
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS XadesSign(SIGNING_CONTEXT *signing_context, char *xml_file_path,
    char *signed_xml_file_path, unsigned char **signature,
    unsigned int *signature_len, int *attempts_left);

/**
* @brief Perform digital signing of XML document following
* XML Advanced Electronic Signature (XAdES) standard
*
* @param[in] signing_context Toolkit digital signature context
* @param[in] xml_file_path Path of the input xml document
* @param[in] signed_xml_file_path Path of the signed xml document
* @param[out] signature signature data if packaging mode is Detached
* @param[out] signature_len signature data length
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS XadesSignEx(SIGNING_CONTEXT *signing_context,
    char *xml_file_path, char *signed_xml_file_path, unsigned char **signature,
    unsigned int *signature_len, int *attempts_left, char **response,
    unsigned int *response_len);

/**
* @brief Perform digital signature verification of XML document following
* XML Advanced Electronic Signature (XAdES) standard
*
* @param[in] verification_context Digital signature verification context
* @param[in] xml_file_path Path of the input xml document to verify
* @param[in] signature signature data if packaging mode is Detached 
* @param[in] signature_len signature data length
* @param[out] verification_report Verification report
* @param[out] report_len Verification report length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS XadesVerify(VERIFICATION_CONTEXT *verification_context,
    char *xml_file_path, unsigned char *signature, unsigned int signature_len,
    char **verification_report, unsigned int *report_len);

/**
* @brief Perform digital signing of document following 
* CMS Advanced Electronic Signature (CAdES) standard
*
* @param[in] signature_context Toolkit digital signature context
* @param[in] input_document_path Path of the input document
* @param[out] signature signature data
* @param[out] signature_len signature data length
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS CadesSign(SIGNING_CONTEXT *signing_context,
    char *input_document_path, unsigned char **signature,
    unsigned int *signature_len, int *attempts_left);

/**
* @brief Perform digital signing of document following
* CMS Advanced Electronic Signature (CAdES) standard
*
* @param[in] signature_context Toolkit digital signature context
* @param[in] input_document_path Path of the input document
* @param[out] signature signature data
* @param[out] signature_len signature data length
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS CadesSignEx(SIGNING_CONTEXT *signing_context,
    char *input_document_path, unsigned char **signature,
    unsigned int *signature_len, int *attempts_left, char **response,
    unsigned int *response_len);

/**
* @brief Perform digital signature verification of document following
* CMS Advanced Electronic Signature (CAdES) standard
*
* @param[in] verification_context Digital signature verification context
* @param[in] input_document_path Path of the input document to verify
* @param[in] signature CAdES signature of the input document
* @param[in] signature_len Length of the signature bytes
* @param[out] verification_report Verification report
* @param[out] report_len Verification report length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS CadesVerify(VERIFICATION_CONTEXT *verification_context,
    char *input_document_path, unsigned char *signature,
    unsigned int signature_len, char **verification_report,
    unsigned int *report_len);
#endif

/**
* @brief Toolkit API to Get Device ID
*
* This API can be called to retrieve the Device ID.
*
* @param[out] device_id Unique Device ID string
* @param[out] device_id_len Device ID string length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS GetDeviceID(char **device_id, unsigned int *device_id_len);

/**
* @brief Perform biometric authentication on the card
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[in] request_id Application random number
* @param[in] finger_index Finger index to be used for biometric authentication
* @param[in] finger_id Finger ID to be used for biometric authentication
* @param[in] sensor_timeout Fingerprint sensor capture timeout in seconds
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS AuthenticateBiometricOnCard(unsigned int connection_handle,
    char *request_id, unsigned int finger_index, unsigned int finger_id,
    int sensor_timeout, int *attempts_left, char **response,
    unsigned int *response_len);

/** @brief Retrieves the SecureMessaging mode
*
* @param[out] sm_mode Type of Secure Messaging mode
* @return Return value (SUCCESS / ERROR codes).
*/
ETAPI ETSTATUS GetSMMode(int *sm_mode);

/** @brief Get data protection key
*
* @param[out] data_protection_key Data protection public key
* @param[out] key_len Data protection public key length
* @param[out] modulus Data protection public key modulus
* @param[out] modulus_len Data protection public key modulus length
* @param[out] exponent Data protection public key public exponent
* @param[out] exponent_len Data protection public key public exponent length
* @return Return value (SUCCESS / ERROR codes).
*/
ETAPI ETSTATUS GetDataProtectionKey(unsigned char **data_protection_key,
    unsigned int *key_len, char **modulus, unsigned int *modulus_len,
    char **exponent, unsigned int *exponent_len);

/**
* @brief Verify certificate using OCSP
*
* @param[in] ocsp_url OCSP url
* @param[in] cert_data Certificate data for which status is checked.
* @param[in] cert_data_len Certificate data length.
* @param[in] issuer_cert_data Issuer Certificate data.
* @param[in] issuer_cert_len Issuer Certificate data length.
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS VerifyCertificateUsingOCSP(char *ocsp_url,
    unsigned char *cert_data, unsigned int cert_data_len,
    unsigned char *issuer_cert_data, unsigned int issuer_cert_len);

/**
* @brief EIDA Toolkit API to verify Toolkit response
*
* @param[in] in_process_mode TRUE if toolkit is used as in-process module
* @param[in] vg_response Validation Gateway (VG) response
* @param[in] cert_data Validation Gateway (VG) response signing certificate
* @param[in] cert_data_len Certificate data length
* @param[in] cert_chain Validation Gateway (VG) response signing
*            certificate chain
* @param[in] cert_chain_len Certificate chain length
* @param[in] chain_validation Flag (TRUE/FALSE) indicating chain validation
*            is required or not
* @param[out] status Signature validation status(-1 not set / 1 set)
* @param[out] error_message Error message
* @param[out] error_message_len Length of error message
* @param[out] response_data Parsed VG response attributes
* @return Return value (SUCCESS / ERROR codes).
*/
ETAPI ETSTATUS ValidateToolkitResponse(BOOL in_process_mode, char *vg_response,
    unsigned char *cert_data, unsigned int cert_data_len,
    unsigned char *cert_chain, unsigned int cert_chain_len,
    BOOL chain_validation, int *status, char **error_message,
    unsigned int *error_message_len, VGRESPONSE_DATA *response_data);

#if defined(_WIN32) || defined(_WIN64) || defined(LINUX) || defined(__MACH__) && !defined(ARM_WIN)
/**
* @brief Attach a Java environment to the Toolkit
*
* @param[in] jenv_obj Pointer to Java environment object
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS AttachJEnv(void *jenv_obj);
#endif

#if defined(ANDROID)
/**
* @brief Set device context
*
* @param[in] context Device context object
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETSTATUS SetContext(void *context);
#endif

/**
* @brief Parse MRZ data
*
* @param[in] mrz Data from the Machine Readable Zone (MRZ) of the card
* @param[out] mrz_data Parsed MRZ string attributes
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS ParseMRZData(char *mrz, MRZ_DATA *mrz_data);

/**
* @brief Perform check card status and biometric authentication on server
* (Validation Gateway - VG)
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[in] request_id Application random number
* @param[in] finger_index Index of finger to be used for bio. authentication
* @param[in] sensor_timeout Fingerprint sensor capture timeout in seconds
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS AuthenticateCardAndBiometric(unsigned int connection_handle,
    char *request_id, unsigned int finger_index, int sensor_timeout,
    char **response, unsigned int *response_len);

/**
* @brief Toolkit API to read public data EF from the Emirates ID Card
*
* This API can be called to retrieve the Emirates ID Card public data EF
* Call this API with enumeration indicating which data is required to be read.
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[in] public_data_ef_type Enumeration of public data EF type to read
* corresponding data from Emirates ID Card
* @param[in] validate_signature Flag to perform EF signature validation.
* @param[out] ef_data Pointer to receive EF data
* @param[out] ef_data_length EF data length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS ReadPublicDataEF(unsigned int connection_handle,
    PUBLIC_DATA_EF_TYPE public_data_ef_type, BOOL validate_signature,
    unsigned char **ef_data, unsigned int *ef_data_length);

/**
* @brief Get license expiry date
*
* @param[out] expiry_date Toolkit license expiry date
* @param[out] expiry_date_len Toolkit license expiry date length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS GetLicenseExpiryDate(char **expiry_date,
    unsigned int *expiry_date_len);

/**
* @brief Get config certificate expiry date
*
* @param[out] config_cert_expiry_date Structure to receive
* config certificate expiry date
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS GetConfigCertificateExpiryDate(
    CONFIG_CERT_EXPIRY_DATE *config_cert_expiry_date);

/**
* @brief Set NFC tag object
*
* @param[in] nfc_object Structure for passing NFC tag object
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS SetNFCTag(void *nfc_object);

/**
* @brief EIDA Toolkit API to Personalise new containers
* and for updating file data to card
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[in] df_type Type of container to update
* @param[in] request_id Application random number
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS UpdateData(unsigned int connection_handle, DF_TYPE df_type,
    char *request_id, char **response, unsigned int *response_len);

/**
* @brief EIDA Toolkit API to read file data from card
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[in] file_type Type of file to read data from
* @param[in] request_id Application random number
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS ReadData(unsigned int connection_handle,
    FILE_TYPE file_type, char *request_id, char **response,
    unsigned int *response_len);

#endif

/**
* @brief Establish connection with the smartcard (extended)
*
* @param[in] reader_name Smartcard reader name
* @param[out] connection_handle Handle to smartcard connection
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS ConnectEx(char *reader_name, unsigned int *connection_handle);

/**
* @brief Disconnect smartcard session (extended)
*
* @param[in] connection_handle Handle to smartcard connection
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS DisconnectEx(unsigned int connection_handle);

/**
* @brief Read public data from the card into a flat structure
*
* This API can be called to retrieve the Emirates ID Card public data.
* Call this API with flags indicating which data is required to be read.
* Choosing to read only the data required can improve the performance compared
* to reading the complete public data every time.
*
* @param[in] connection_handle Emirates ID Card connection handle
* @param[out] PUBLIC_DATA_OBJ Structure to receive public data attributes
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS ReadPublicDataEx(unsigned int connection_handle,
    PUBLIC_DATA_OBJ *public_data_obj);

/**
* @brief Reset PIN on Emirates ID Card without biometric authentication
*
* @param[in] pin New PIN to set on the Emirates ID Card in encrypted format
* @param[in] pin_len Length of the encrypted PIN
* @param[out] attempts_left Number of attempts left before the card is blocked.
* @param[out] response Pointer to receive the response
* @param[out] response_len Response length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS ResetPINWithoutAuthenticateBiometric(char *pin,
    unsigned int pin_len, int *attempts_left, char **response,
    unsigned int *response_len);

/**
* @brief Get first reader with Emirates ID Card inserted and its serial number
*
* @param[out] reader_name Connected reader name
* @param[out] reader_name_len Reader name length
* @param[out] reader_serial_number Connected reader serial number
* @param[out] reader_serial_len Reader serial number length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS GetConnectedReaderAndSerialNumber(char **reader_name,
    unsigned int *reader_name_len, char **reader_serial_number,
    unsigned int *reader_serial_len);

/**
* @brief EIDA Toolkit API to parse EF data
*
* @param[in] ef_data Buffer containing the EF data
* @param[in] ef_data_len EF data length
* @param[out] response Parsed EF data
* @param[out] response_len Parsed EF data length
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS ParseEFData(unsigned char *ef_data, unsigned int ef_data_len,
    char **response, unsigned int *response_len);

/**
* @brief EIDA Toolkit API to enable digital docs
*
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS EnableDigitalDocs();

/**
* @brief EIDA Toolkit API to disable digital docs
*
* @return Return value (ETSTATUS_SUCCESS on success else error code)
*/
ETAPI ETSTATUS DisableDigitalDocs();

/**
* @brief EIDA Toolkit API to get digital docs status
* 
* @return Return value (TRUE if enabled FALSE if not enabled)
*/
ETAPI BOOL IsDigitalDocsEnabled();

/**
 * @brief Query whether reader monitoring is supported by the loaded
 *        smart-card plugin.
 *
 * @param[out] out_supported TRUE if GetReaderStatus / WaitForCardChange
 *             / CancelReaderWait / WaitForReaderChange are usable.
 *             FALSE means the plugin does not advertise monitoring;
 *             callers must fall back to polling via ListReaders().
 * @return ETSTATUS_SUCCESS on success.
 */
ETAPI ETSTATUS GetMonitoringCapability(BOOL *out_supported);

/**
 * @brief Read the current state of a single reader without blocking.
 *
 * @param[in]  reader_name Reader name (as returned by ListReaders).
 * @param[out] out_state Bitwise-OR of READER_STATE_* constants from
 *             EIDAToolkitTypes.h (READER_STATE_EMPTY,
 *             READER_STATE_CARD_PRESENT, READER_STATE_MUTE,
 *             READER_STATE_UNAVAILABLE, READER_STATE_EXCLUSIVE).
 *             READER_STATE_UNKNOWN (0) when state is indeterminate.
 * @return ETSTATUS_SUCCESS on success.
 *         ETSTATUS_PLUGIN_FUNCTION_NOT_SUPPORTED if monitoring is
 *         unavailable on this platform.
 *         ETSTATUS_READER_GENERAL_ERROR for plugin-level failures.
 */
ETAPI ETSTATUS GetReaderStatus(const char *reader_name,
    unsigned int *out_state);

/**
 * @brief Block until at least one of the supplied readers reports a
 *        card-state change, the timeout elapses, or CancelReaderWait
 *        is called from another thread.
 *
 * @param[in]  readers_buf Multi-string buffer of reader names: each
 *             name is null-terminated, followed by a final null
 *             terminator (the same format that ListReaders returns).
 * @param[in]  reader_count Number of names in readers_buf (max 32).
 * @param[in]  timeout_ms Wait timeout in milliseconds, or
 *             0xFFFFFFFFu for infinite.
 * @param[out] out_states Caller-allocated array of reader_count
 *             unsigned ints. On change, filled with READER_STATE_*
 *             bits per reader. Unchanged readers report
 *             READER_STATE_UNKNOWN (0).
 * @return ETSTATUS_SUCCESS on state change.
 *         ETSTATUS_READER_WAIT_TIMEOUT if no change within timeout.
 *         ETSTATUS_READER_WAIT_CANCELLED if CancelReaderWait fired.
 *         ETSTATUS_PLUGIN_FUNCTION_NOT_SUPPORTED if monitoring is
 *         unavailable.
 *         ETSTATUS_READER_GENERAL_ERROR for plugin-level failures.
 *
 * @note Single-thread per process. Concurrent calls against the same
 *       monitoring context are not supported; use one monitor thread.
 */
ETAPI ETSTATUS WaitForCardChange(const char *readers_buf,
    unsigned int reader_count, unsigned int timeout_ms,
    unsigned int *out_states);

/**
 * @brief Cancel a blocked WaitForCardChange / WaitForReaderChange call
 *        on the monitoring thread.
 *
 * Thread-safe -- typically called from a foreground thread to wake the
 * monitor for shutdown or pause/resume. The cancelled wait returns
 * ETSTATUS_READER_WAIT_CANCELLED.
 *
 * @return ETSTATUS_SUCCESS on success (including the case where no
 *         wait is currently active).
 */
ETAPI ETSTATUS CancelReaderWait(void);

/**
 * @brief Block until a reader is plugged in or unplugged.
 *
 * Detects reader hot-plug events via PnP notification. Use alongside
 * WaitForCardChange to react to both card insertions/removals and
 * reader connections/disconnections.
 *
 * @param[in]  timeout_ms Wait timeout in milliseconds, or
 *             0xFFFFFFFFu for infinite.
 * @param[out] out_reader_name Receives a newly-allocated string with
 *             the name of the reader whose state changed. Caller MUST
 *             free with FreeMemory(). May be NULL on ambiguous
 *             multi-event ticks (see out_state).
 * @param[out] out_state Bitwise-OR of READER_STATE_* bits for the
 *             reported reader. READER_STATE_UNKNOWN (0) when the
 *             specific reader cannot be determined (e.g., one added
 *             AND one removed within the same wake).
 * @return ETSTATUS_SUCCESS on change.
 *         ETSTATUS_READER_WAIT_TIMEOUT on timeout.
 *         ETSTATUS_READER_WAIT_CANCELLED on cancel.
 *         ETSTATUS_PLUGIN_FUNCTION_NOT_SUPPORTED if hot-plug
 *         detection is unavailable.
 *         ETSTATUS_READER_GENERAL_ERROR for plugin-level failures.
 */
ETAPI ETSTATUS WaitForReaderChange(unsigned int timeout_ms,
    char **out_reader_name, unsigned int *out_state);

/**
 * @brief Log a message into the toolkit's log stream from outside
 *        EIDAToolkit.dll.
 *
 * Used by the agent and service binaries (which link against the
 * toolkit DLL) and by language SDK bindings (.NET, Java, Swift) so
 * all toolkit-related diagnostics land in a single authoritative log
 * file. Pre-Initialize callers receive stderr output (zf_log default);
 * after Initialize, output goes to the configured log file.
 *
 * Returns void: callers MUST NOT have error-handling logic that
 * itself logs on this function's failure, otherwise a logging
 * failure would recursively re-enter this function.
 *
 * The message is a pre-formatted string -- not a printf format string.
 * Any '%' characters in the message are treated as literal text. Every
 * SDK binding formats in its own language's idiom (String.format,
 * $"...", printf) before calling this function.
 *
 * Both module and message are NULL-safe: NULL module renders as "?",
 * NULL message renders as empty string.
 *
 * @param[in] level    0 = INFO, 1 = WARN, 2 = ERROR. Values outside
 *                     {0, 1, 2} are treated as ERROR.
 * @param[in] module   Short module identifier; appears as [EXT:module]
 *                     in the log output. NULL accepted -> "?".
 * @param[in] message  Pre-formatted message text. NULL accepted -> "".
 *                     Length effective limit is approximately 380 bytes
 *                     (bounded by zf_log's per-line buffer); larger
 *                     messages are silently truncated downstream.
 */
ETAPI void ToolkitLog(int level, const char *module, const char *message);

#endif // EIDA_TOOLKIT_API_H_
