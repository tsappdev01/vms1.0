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
 * @file EIDAToolkitError.h
 * @brief Status and error code definitions for the Emirates ID Card Toolkit.
 *
 * All Toolkit APIs return ETSTATUS values. Use ETSTATUS_CODE(status)
 * to extract the error code, then compare against the constants below.
 * Use GetStatusMessage(status) to get a human-readable description.
 *
 * Error codes are organized into logical groups:
 * - General Errors (1-29)
 * - Agent & Schema Errors (34-38)
 * - Device, Plugin & Smartcard Errors (39-69)
 * - Network & VG Communication Errors (70-114)
 * - Digital Signature Errors (115-120)
 * - Certificate & Crypto Errors (121-135)
 * - Java/JVM Errors (136, 200-207, 293-294)
 * - Configuration & License Errors (138-139, 187-189, 224-244)
 * - VG Server-Side Errors (145-186)
 * - Smartcard Reader Errors (245-261)
 * - JSON & XML Parsing Errors (262-282)
 * - Card Container & Data File Errors (329-345)
 */

#ifndef EIDA_TOOLKIT_ERROR_H_
#define EIDA_TOOLKIT_ERROR_H_

#if __GNUC__ >= 4
#pragma GCC visibility push(hidden)
#endif

// The operation completed successfully
#define ETSTATUS_SUCCESS                        0

//{{BEGIN: Emirates ID Card Toolkit status codes

// ═══════════════════════════════════════════════════════════════════
// General Errors (1-29)
// ═══════════════════════════════════════════════════════════════════

// Plugin operation failed
#define ETSTATUS_PLUGIN_ERROR                   1

// Invalid parameter
#define ETSTATUS_INVALID_PARAMETER              2

// The module or object is not initialized
#define ETSTATUS_NOT_INITIALIZED                3

// The module or object is already initialized
#define ETSTATUS_ALREADY_INITIALIZED            4

// Invalid handle value
#define ETSTATUS_INVALID_HANDLE                 5

// Invalid data type
#define ETSTATUS_INVALID_DATA_TYPE              6

// Invalid data
#define ETSTATUS_INVALID_DATA                   7

// Insufficient data
#define ETSTATUS_INSUFFICIENT_DATA              8

// Invalid command code used
#define ETSTATUS_INVALID_COMMAND                9

// Memory allocation failed
#define ETSTATUS_MALLOC_FAILED                  10

// Requested file not found
#define ETSTATUS_FILE_NOT_FOUND                 11

// File could not be opened
#define ETSTATUS_COULD_NOT_OPEN_FILE            12

// Specified file is empty
#define ETSTATUS_FILE_EMPTY                     13

// File seek operation failed
#define ETSTATUS_FILE_SEEK_FAILED               14

// File read operation failed
#define ETSTATUS_FILE_READ_FAILED               15

// File write operation failed
#define ETSTATUS_FILE_WRITE_FAILED              16

// Index is out of bounds
#define ETSTATUS_INDEX_OUT_OF_BOUNDS            17

// Searched object not found
#define ETSTATUS_NOT_FOUND                      18

// More data remaining to be read
#define ETSTATUS_MORE_DATA_AVAILABLE            19

// Buffer provided is of insufficient length
#define ETSTATUS_INSUFFICIENT_BUFFER_LEN        20

// Provided data is of invalid format
#define ETSTATUS_BAD_FORMAT                     21

// Network error occurred
#define ETSTATUS_ERROR_NETWORK                  22

// Functionality not supported
#define ETSTATUS_NOT_SUPPORTED                  23

// Bluetooth not supported
#define ETSTATUS_BLUETOOTH_NOT_SUPPORTED        24
#define ETSTATUS_BLUTOOTH_NOT_SUPPORTED         ETSTATUS_BLUETOOTH_NOT_SUPPORTED

// Bluetooth is off. Please check the settings
#define ETSTATUS_BLUETOOTH_IS_OFF               25
#define ETSTATUS_BLUTOOTH_IS_OFF                ETSTATUS_BLUETOOTH_IS_OFF

// Matching paired device not found
#define ETSTATUS_NO_PAIRED_DEVICES              26

// Toolkit service usage not authorized as per the license
#define ETSTATUS_SERVICE_NOT_AUTHORISED         27

// Toolkit license file is corrupt
#define ETSTATUS_LICENSE_DATA_CORRUPTED         28

// Uninitialized object
#define ETSTATUS_OBJECT_NOT_INITIALISED         29

// Object reference is null
#define ETSTATUS_NULL_OBJECT_POINTER            30

// Requested attribute not found in the object
#define ETSTATUS_OBJECT_ATTRIBUTE_NOT_FOUND     31

// Requested object attribute found corrupted
#define ETSTATUS_OBJECT_ATTRIBUTE_CORRUPTED     32

// Object attribute reference is null
#define ETSTATUS_NULL_OBJECT_ATTRIBUTE          33

// ═══════════════════════════════════════════════════════════════════
// Agent & Schema Errors (34-38)
// ═══════════════════════════════════════════════════════════════════

// Agent has reached the maximum limit for application connections
#define ETSTATUS_NO_MORE_FREE_CONTEXTS          34

// The schema contains no application maps
#define ETSTATUS_NO_APPLICATIONS_MAPPED         35

// Raw data is not matching the ID Card schema
#define ETSTATUS_RAW_DATA_SCHEMA_MISMATCH       36

// Failed to get function address from a shared library
#define ETSTATUS_SYMBOL_NOT_FOUND               37

// Failed to load a shared library
#define ETSTATUS_ERROR_LOAD_MODULE              38

// ═══════════════════════════════════════════════════════════════════
// Device, Plugin & Smartcard Errors (39-69)
// ═══════════════════════════════════════════════════════════════════

// Smartcard operation failed
#define ETSTATUS_SMARTCARD_ERROR                39

// Invalid or incomplete smartcard reader plugin configuration
#define ETSTATUS_INVALID_SCARD_READER_CONFIG    40

// Invalid or incomplete fingerprint scanner plugin configuration
#define ETSTATUS_INVALID_FPSCANNER_CONFIG       41

// Smartcard reader plugin don't have the required functions
#define ETSTATUS_INVALID_SCARD_READER_PLUGIN    42

// Fingerprint scanner plugin don't have the required functions
#define ETSTATUS_INVALID_FPSCANNER_PLUGIN       43

// No smartcard reader plugins found
#define ETSTATUS_NO_SMARTCARD_PLUGINS           44

// No fingerprint scanner plugins found
#define ETSTATUS_NO_FINGERPRINT_PLUGINS         45

// Invalid smartcard reader name
#define ETSTATUS_INVALID_SCARD_READER_NAME      46

// Device plugin context is corrupted
#define ETSTATUS_PLUGIN_CONTEXT_CORRUPTED       47

// Already connected to fingerprint scanner
#define ETSTATUS_FPSCANNER_ALREADY_CONNECTED    48

// Failed to open fingerprint sensor
#define ETSTATUS_FP_OPENDEVICE_ERROR            49

// Failed to capture fingerprint image
#define ETSTATUS_CAPTURE_IMAGE_ERROR            50

// Failed to enumerate connected devices
#define ETSTATUS_DEVICE_ENUM_FAILED             51

// Failed to list smartcard readers
#define ETSTATUS_LIST_READERS_ERROR             52

// Failed to establish connection with smartcard
#define ETSTATUS_SC_CONNECT_FAILED              53

// Failed to close an established smartcard connection
#define ETSTATUS_SC_DISCONNECT_FAILED           54

// Smartcard not present in the reader
#define ETSTATUS_SMARTCARD_NOT_PRESENT          55

// Paired device not in range
#define ETSTATUS_PAIRED_DEVICES_NOT_IN_RANGE    56

// Device error
#define ETSTATUS_DEVICE_ERROR                   57

// No supported device found
#define ETSTATUS_NO_SUPPORTED_DEVICE            58

// Device connection timed out
#define ETSTATUS_CONNECTION_TIMEOUT             59

// Failed to verify fingerprint ISO template
#define ETSTATUS_ERROR_VERIFY_ISO_TEMPLATE      60

// Error occured in matching fingerprint ISO templates
#define ETSTATUS_ERROR_VERIFY_MATCH             61

// Failed to establish resource manager context
#define ETSTATUS_ESTABLISH_RM_CTXT_ERROR        62

// Failed to close established resource manager context
#define ETSTATUS_RM_CTXT_CLOSE_FAILED           63

// Failed to send smartcard command
#define ETSTATUS_SEND_SVC_REQ_SC_ERROR          64

// Error in retrieving ATR from smartcard
#define ETSTATUS_SC_ATR_ERROR                   65

// Not an NFC interface
#define ETSTATUS_INVALID_NFC_INTERFACE          66

// Smartcard maximum connection limit reached
#define ETSTATUS_CONNECTION_LIMIT_REACHED       67

// Failed to create websocket context
#define ETSTATUS_CREATE_WS_CONTEXT_FAILED       68

// Failed to run websocket service
#define ETSTATUS_WS_SVC_FAILED                  69

// ═══════════════════════════════════════════════════════════════════
// Network & VG Communication Errors (70-114)
// ═══════════════════════════════════════════════════════════════════

// Error in preparing HTTP request header
#define ETSTATUS_ERROR_QUERY_HEADER             70

// Validation Gateway (VG) URL is not provided
#define ETSTATUS_ERROR_VGURL_NOT_PROVIDED       71

// Validation Gateway (VG) URL provided is not secure
#define ETSTATUS_ERROR_VGURL_NOT_SECURE         72

// Validation Gateway (VG) is disconnected
#define ETSTATUS_VG_DISCONNECTED                73

// Emirates ID Card is revoked
#define ETSTATUS_CARD_REVOKED                   74

// Emirates ID Card is expired
#define ETSTATUS_CARD_EXPIRED                   75

// Bad Validation Gateway (VG) request
#define ETSTATUS_BAD_VG_REQUEST                 76

// Low score match
#define ETSTATUS_BIO_AUTH_FAILED                77

// Failed to validate public data signature
#define ETSTATUS_DATASIGNATURE_ERROR            78

// Emirates ID Card PIN verification failed
#define ETSTATUS_PINVERIFICATION_FAILED         79

// Failed to parse fingerprint data from card
#define ETSTATUS_PARSE_FP_ERROR                 80

// Failed to retrieve fingerprint templates count
#define ETSTATUS_ERROR_GET_TEMPLATES_COUNT      81

// Failed to retrieve fingerprint templates
#define ETSTATUS_ERROR_RETRIEVING_TEMPLATE      82

// There is no biometric data present in the Emirates ID Card
#define ETSTATUS_NO_BIO_DATA                    83

// Emirates ID Card is blocked
#define ETSTATUS_CARD_BLOCKED                   84

// Failed to verify digital signature
#define ETSTATUS_VERIFYSIGNATURE_ERROR          85

// Unknown smartcard
#define ETSTATUS_UNKNOWN_SMARTCARD              86

// Error in computing check bit
#define ETSTATUS_INVALID_CHECK_BIT              87

// Invalid Message Authentication Code (MAC) data
#define ETSTATUS_WRONG_MAC                      88

// Match on Card (MOC) security status not satisfied
#define ETSTATUS_MOC_SEC_STATUS_UNSATISFIED     89

// Match on Card (MOC) applet is blocked
#define ETSTATUS_MOC_APPLICATION_BLOCKED        90

// Invalid fingerprint template data
#define ETSTATUS_MOC_MATCH_BAD_TEMPLATE         91

// Match on Card (MOC) failed
#define ETSTATUS_MOC_FAILED                     92

// Match on Card (MOC) status unknown
#define ETSTATUS_MOC_MATCH_STATUS_UNKNOWN       93

// BAC authentication parameters are not correct
#define ETSTATUS_CARD_MUTUAL_AUTH_FAILED        94

// PKI application is blocked
#define ETSTATUS_PKI_APPLICATION_BLOCKED        95

// PKI PIN is blocked
#define ETSTATUS_PKI_PIN_BLOCKED                96

// PKI PIN verify security status not satisfied
#define ETSTATUS_PKI_PIN_VERIFY_SEC_UNKNOWN     97

// Invalid PKI PIN reference
#define ETSTATUS_PKI_PIN_BAD_REFERENCE          98

// Invalid PKI PIN verification status
#define ETSTATUS_PKI_PIN_VERIFY_STATUS_UNKNOWN  99

// Request is not according to Validation Gateway (VG) Schema
#define ETSTATUS_VGREQUEST_IS_NOT_VALID         100

// The license key is invalid
#define ETSTATUS_VG_LICENSE_INVALID             101

// Failed to generate PKI session keys
#define ETSTATUS_VG_GEN_PKI_SESSIONKEY_FAILED   102

// Failed to complete PKI authentication
#define ETSTATUS_COMPLETE_AUTH_FAILED           103

// The Emirates ID Card is not genuine
#define ETSTATUS_CARD_IS_NOT_GENUINE            104

// Ciphered PIN verification failed
#define ETSTATUS_CIPHERED_PIN_VERIFY_FAILED     105

// Application is blocked
#define ETSTATUS_APPLICATION_BLOCKED            106

// The license does not grant access to the requested service
#define ETSTATUS_SERVICE_LIC_INVALID            107

// PKI admin PIN key not found in SAM
#define ETSTATUS_PKI_ADMIN_PIN_KEY_NOT_FOUND    108

// SAM card PKI admin PIN is blocked
#define ETSTATUS_PKI_ADMIN_PIN_BLOCKED          109

// Failed to reset Emirates ID Card PKI PIN
#define ETSTATUS_PIN_RESET_FAILED               110

// Received expired Validation Gateway (VG) response
#define ETSTATUS_VG_RESPONSE_EXPIRED            111

// Failed to complete transaction successfully
#define ETSTATUS_COMPLETE_TRXN_FAILED           112

// No family book data present in the Emirates ID Card
#define ETSTATUS_NO_FAMILY_BOOK_DATA            113

// Invalid Validation Gateway (VG) response
#define ETSTATUS_INVALID_VG_RESPONSE            114

// ═══════════════════════════════════════════════════════════════════
// Digital Signature Errors (115-120)
// ═══════════════════════════════════════════════════════════════════

// Failed to sign document using XAdES standard
#define ETSTATUS_XADES_SIGNING_FAILED           115
#define ETSTATUS_XADES_SINGING_FAILED           ETSTATUS_XADES_SIGNING_FAILED

// Failed to sign document using PAdES standard
#define ETSTATUS_PADES_SIGNING_FAILED           116
#define ETSTATUS_PADES_SINGING_FAILED           ETSTATUS_PADES_SIGNING_FAILED

// Failed to sign document using CAdES standard
#define ETSTATUS_CADES_SIGNING_FAILED           117
#define ETSTATUS_CADES_SINGING_FAILED           ETSTATUS_CADES_SIGNING_FAILED

// Failed to verify document using XAdES standard
#define ETSTATUS_XADES_VERIFY_FAILED            118

// Failed to verify document using PAdES standard
#define ETSTATUS_PADES_VERIFY_FAILED            119

// Failed to verify document using CAdES standard
#define ETSTATUS_CADES_VERIFY_FAILED            120

// ═══════════════════════════════════════════════════════════════════
// Certificate & Crypto Errors (121-135)
// ═══════════════════════════════════════════════════════════════════

// Failed to create certificate verification context
#define ETSTATUS_CERT_VERIFY_CONTEXT_NULL       121

// Unspecified certificate validation response
#define ETSTATUS_CERT_VERIFY_STATUS_UNSPECIFIED 122

// Certificate not valid yet
#define ETSTATUS_CERT_NOT_YET_VALID             123

// Certificate expired
#define ETSTATUS_CERT_HAS_EXPIRED               124

// Server TLS certificate validation failed
#define ETSTATUS_TLS_CERT_INVALID               125

// The certificate is revoked
#define ETSTATUS_CERT_REVOKED                   126

// Certificate issuer not found in the local store
#define ETSTATUS_CERT_ISSUER_NOT_FOUND          127

// Error in retrieving SSL certificate
#define ETSTATUS_SSL_CERT_NOT_RETRIEVED         128

// Failed to generate symmetric key
#define ETSTATUS_GEN_SYMMETRIC_KEY_ERROR        129

// Failed to parse x509 certificate
#define ETSTATUS_ERROR_PARSING_X509_CERT        130

// Failed to get certificate serial number
#define ETSTATUS_ERROR_GET_CERT_SLNO            131

// Failed to retrieve public key from certificate
#define ETSTATUS_GET_PUBKEY_FAILED              132

// Failed to generate message digest
#define ETSTATUS_ERROR_GENERATING_DIGEST        133

// Failed to get RSA public key
#define ETSTATUS_GET_RSA_PUBKEY_FAILED          134

// Failed to append SSL BIO
#define ETSTATUS_ERROR_APPEND_BIO               135

// ═══════════════════════════════════════════════════════════════════
// Java/JVM Errors (136, 200-207, 293-294)
// ═══════════════════════════════════════════════════════════════════

// Failed to load Java Virtual Machine (VM)
#define ETSTATUS_JAVA_VM_CREATION_FAILED        136

// PIN is not created
#define ETSTATUS_PIN_NOT_CREATED                137

// ═══════════════════════════════════════════════════════════════════
// Configuration & License Errors (138-139, 187-189, 224-244, 296, 363-366)
// ═══════════════════════════════════════════════════════════════════

// Unsigned config file used in production
#define ETSTATUS_UNSIGNED_CONFIG_IN_PROD        138

// Service Provider (SP) is not authorized
#define ETSTATUS_INVALID_SP                     139

// ═══════════════════════════════════════════════════════════════════
// Card & PKI Authentication Errors (140-144)
// ═══════════════════════════════════════════════════════════════════

// The Emirates ID Card is unknown
#define ETSTATUS_CARD_UNKNOWN                   140

// PKI authentication failed
#define ETSTATUS_PKI_AUTH_FAILED                141

// Failed to unblock PIN
#define ETSTATUS_UNBLOCK_PIN_FAILED             142

// Digital Signature Service (DSS) internal error
#define ETSTATUS_DSS_ERROR                      143

// Invalid protocol
#define ETSTATUS_INVALID_PROTOCOL               144

// ═══════════════════════════════════════════════════════════════════
// VG Server-Side Errors (145-186)
// These map to VG error codes shown in parentheses (E4xx/E5xx).
// ═══════════════════════════════════════════════════════════════════

// Validation Gateway (VG) system error (E500-00-00)
#define ETSTATUS_VG_SYSTEM_ERROR                145

// Session not initialized (E400-01-01)
#define ETSTATUS_SESSION_NOT_INITIALIZED        146

// Required parameter missing (E400-01-02)
#define ETSTATUS_MISSING_PARAMETER              147

// Invalid parameter value (E400-01-03)
#define ETSTATUS_VG_INVALID_PARAMETER           148

// Repository system error (E500-02-00)
#define ETSTATUS_REPOSITORY_SYSTEM_ERROR        149

// Invalid wire format checksum (E400-40-01)
#define ETSTATUS_WIRE_FMT_CHECKSUM              150

// Invalid wire format first byte (E400-40-02)
#define ETSTATUS_INVALID_WIRE_FMT_FIRST_BYTE    151

// Invalid wire format header length (E400-40-03)
#define ETSTATUS_INVALID_WIRE_FMT_HEADER_LEN    152

// Invalid wire format total length (E400-40-04)
#define ETSTATUS_INVALID_WIRE_FMT_TOTAL_LEN     153

// Invalid payload encryption (E400-40-06)
#define ETSTATUS_INVALID_PAYLOAD_ENCRYPTION     154

// Invalid payload XML message (E400-42-02)
#define ETSTATUS_INVALID_PAYLOAD_XML_MSG        155

// Invalid payload XML signature (E400-42-03)
#define ETSTATUS_INVALID_PAYLOAD_XML_SIGNATURE  156

// Payload unmarshal error (E400-42-04)
#define ETSTATUS_PAYLOAD_UNMARSHAL_ERROR        157

// Payload signature not verified (E400-42-05)
#define ETSTATUS_PAYLOAD_SIGNATURE_NOT_VERIFIED 158

// Service provider not found (E400-44-01)
#define ETSTATUS_INVALID_SERVICE_PROVIDER       159

// Service mismatch (E400-44-02)
#define ETSTATUS_SERVICE_MISMATCH               160

// Card serial number mismatch (E400-44-03)
#define ETSTATUS_CSN_MISMATCH                   161

// The Emirates ID Card is not registered (E400-44-04)
#define ETSTATUS_CARD_NOT_REGISTERED            162

// ID number mismatch (E400-44-05)
#define ETSTATUS_ID_NUMBER_MISMATCH             163

// Unsupported SDK version (E400-44-06)
#define ETSTATUS_UNSUPPORTED_SDK_VERSION        164

// Fingerprint integrity error (E400-54-01)
#define ETSTATUS_FINGERPRINT_INTEGRITY_ERROR    165

// Device already registered (E409-73-01)
#define ETSTATUS_DEVICE_ALREADY_REGISTERED      166

// License key invalid (E401-72-01)
#define ETSTATUS_LIC_KEY_INVALID                167

// License validity period not started (E401-72-02)
#define ETSTATUS_LIC_PERIOD_NOT_STARTED         168

// License validity period ended (E401-72-03)
#define ETSTATUS_LIC_PERIOD_ENDED               169

// Service not licensed (E401-72-04)
#define ETSTATUS_SERVICE_NOT_LICENSED           170

// Device not registered (E404-73-01)
#define ETSTATUS_DEVICE_NOT_REGISTERED          171

// Invalid data length (E400-70-01)
#define ETSTATUS_INVALID_DATA_LEN               172

// Invalid data nonce (E400-70-02)
#define ETSTATUS_INVALID_DATA_NONCE             173
#define INVALID_DATA_NONCE                      ETSTATUS_INVALID_DATA_NONCE

// Invalid fingerprint template (E400-71-01)
#define ETSTATUS_INVALID_FP_TEMPLATE            174

// Invalid payload XML (E400-42-01)
#define ETSTATUS_INVALID_PAYLOAD_XML            175

// Integrity of the request could not be validated (E400-63-01)
#define ETSTATUS_VG_INTEGRITY_ERROR             176

// Incorrect encryption data (E400-70-03)
#define ETSTATUS_INVALID_ENCRYPTION_DATA        177

// Invalid PIN, values must not be null (E400-74-01)
#define ETSTATUS_NULL_PIN                       178

// Invalid PIN, length must be between 4 and 16 digits [0-9] (E400-74-02)
#define ETSTATUS_INVALID_PIN_LENGTH             179

// Invalid PIN, values must only contain digits between 0 and 9 (E400-74-03)
#define ETSTATUS_INVALID_PIN_DATA               180

// Service provider not found (E400-44-11)
#define ETSTATUS_INVALID                        181

// Service mismatch (E400-44-12)
#define ETSTATUS_INVALID_SERVICE                182

// Card serial number mismatch (E400-44-13)
#define ETSTATUS_CSN_INVALID                    183

// Card number mismatch (E400-44-14)
#define ETSTATUS_CN_INVALID                     184

// ID number mismatch (E400-44-15)
#define ETSTATUS_IDN_INVALID                    185

// Unsupported SDK version (E400-44-16)
#define ETSTATUS_SDK_VER_INVALID                186

// Invalid service context
#define ETSTATUS_INVALID_SERVICE_CTXT           187

// Invalid config (config_li) data
#define ETSTATUS_INVALID_LIC                    188

// Invalid or incomplete configuration data
#define ETSTATUS_INVALID_CONFIG_DATA            189

// Authentication parameters not set
#define ETSTATUS_AUTH_PARAMS_NOT_SET            190

// Failed to execute Secure Messaging (SM) command
#define ETSTATUS_EXECUTE_SM_CMD_FAILED          191

// The operation failed
#define ETSTATUS_ERROR                          192

// No supported reader detected
#define ETSTATUS_NO_SUPPORTED_READER            193

// Invalid NFC tag
#define ETSTATUS_NFC_TAG_INVALID                194

// NFC connection error
#define ETSTATUS_NFC_CONN_ERROR                 195

// Invalid plugin context
#define ETSTATUS_INVALID_PLUGIN_CONTEXT         196

// No USB device connected
#define ETSTATUS_NO_CONN_USB_DEV                197

// Network timeout occurred
#define ETSTATUS_ERROR_TIMEOUT                  198

// ═══════════════════════════════════════════════════════════════════
// Plugin & Runtime Errors (199-210)
// ═══════════════════════════════════════════════════════════════════

// No smartcard plugins loaded
#define ETSTATUS_NO_SMARTCARD_PLUGINS_LOADED    199

// Failed to load Java class
#define ETSTATUS_LOAD_JAVA_CLASS_FAILED         200

// Failed to get method ID of Java class
#define ETSTATUS_GET_METHODID_FAILED            201

// Failed to construct new string object
#define ETSTATUS_NEW_STRING_ERROR               202

// Failed to construct new object
#define ETSTATUS_NEW_OBJECT_ERROR               203

// Failed to retrieve field ID from Java object
#define ETSTATUS_GET_FIELD_ID_FAILED            204

// Failed to retrieve int field from Java object
#define ETSTATUS_GET_INT_FIELD_FAILED           205

// Failed to retrieve object field from Java object
#define ETSTATUS_GET_OBJECT_FIELD_FAILED        206

// Failed to call method from Java object
#define ETSTATUS_FAILED_CALLING_OBJ_METHOD      207

// Failed to decrypt signature
#define ETSTATUS_DECRYPT_SIGNATURE_FAILED       208

// Failed to set Android application context
#define ETSTATUS_SET_CONTEXT_FAILED             209

// Plugin initialization failed
#define ETSTATUS_PLUGIN_INIT_FAILED             210

// ═══════════════════════════════════════════════════════════════════
// OCSP & Certificate Status Errors (211-221)
// ═══════════════════════════════════════════════════════════════════

// Malformed OCSP request
#define ETSTATUS_OCSP_RESP_MALFORMEDREQUEST     211

// OCSP internal error
#define ETSTATUS_OCSP_RESP_INTERNALERROR        212

// OCSP response - try later
#define ETSTATUS_OCSP_RESP_TRYLATER             213

// OCSP response- SIGREQUIRED
#define ETSTATUS_OCSP_RESP_SIGREQUIRED          214

// OCSP response - unauthorized
#define ETSTATUS_OCSP_RESP_UNAUTHORIZED         215

// Certificate status - revoked
#define ETSTATUS_CERT_STATUS_REVOKED            216

// Certificate status - unknown
#define ETSTATUS_CERT_STATUS_UNKNOWN            217

// Invalid certificate type
#define ETSTATUS_INVALID_CERT_TYPE              218

// Failed to capture fingerprint template
#define ETSTATUS_CAPTURE_ISO_ERROR              219

// Platform not supported
#define ETSTATUS_INVALID_PLATFORM               220

// Invalid JVM path
#define ETSTATUS_INVALID_JVM_PATH               221

// Unauthorized use of applet mode
#define ETSTATUS_APPLET_NOT_PERMITTED           222

// License file contains invalid/no server TLS certificate & chain
#define ETSTATUS_NO_SERVER_TLS_CERT             223

// Invalid config (config_vg_qa) data
#define ETSTATUS_INVALID_VG_QA                  224

// Invalid config (config_vg_prod) data
#define ETSTATUS_INVALID_VG_PROD                225

// Invalid config (config_lv_qa) data
#define ETSTATUS_INVALID_LV_QA                  226

// Invalid config (config_lv_prod) data
#define ETSTATUS_INVALID_LV_PROD                227

// Invalid config (config_tk_qa) data
#define ETSTATUS_INVALID_TK_QA                  228

// Invalid config (config_tk_prod) data
#define ETSTATUS_INVALID_TK_PROD                229

// Invalid config (config_pg) data
#define ETSTATUS_INVALID_PG                     230

// Invalid config (config_ag) data
#define ETSTATUS_INVALID_AG                     231

// License contains no service
#define ETSTATUS_LICENSE_CONTAIN_NO_SERVICE     232

// Failed to get response from server
#define ETSTATUS_SERVER_RESPONSE_ERROR          233

// Validation Gateway (VG) connection error
#define ETSTATUS_VG_CONNECTION_ERROR            234

// Server has closed connection unexpectedly
#define ETSTATUS_NETWORK_CONN_CLOSED            235

// Config TLS certificate's peer certificate is not valid
#define ETSTATUS_INVALID_CONFIG_PEER_CERT       236

// Config URL provided is not secure
#define ETSTATUS_ERROR_CONFIG_URL_NOT_SECURE    237

// Config TLS certificate is not provided
#define ETSTATUS_ERROR_CONFIG_CERT_NOT_PROVIDED 238

// Failed to retrieve device ID
#define ETSTATUS_ERROR_DEV_ID                   239

// No SAM card present in the reader
#define ETSTATUS_SAM_NOT_FOUND                  240

// Config URL is not provided
#define ETSTATUS_NO_CONFIG_URL                  241

// Failed to validate config_li signature
#define ETSTATUS_CFG_LI_VALIDATION_ERROR        242

// Failed to validate config_vg signature
#define ETSTATUS_CFG_VG_VALIDATION_ERROR        243

// Failed to validate config_pd signature
#define ETSTATUS_CFG_PD_VALIDATION_ERROR        244

// ═══════════════════════════════════════════════════════════════════
// Smartcard Reader Errors (245-261)
// Low-level smartcard subsystem errors from the PC/SC layer.
// ═══════════════════════════════════════════════════════════════════

// Smartcard has been removed, please insert the card & connect again
#define ETSTATUS_SC_REMOVED                     245

// Smartcard operation was canceled
#define ETSTATUS_SC_OP_CANCELED                 246

// Smartcard was reset
#define ETSTATUS_SC_RESET                       247

// Smartcard access was denied because of a security violation
#define ETSTATUS_SC_ACCESS_DENIED               248

// Power has been removed from smartcard
#define ETSTATUS_SC_UNPOWERED                   249

// Smartcard is not responding to reset
#define ETSTATUS_SC_NOT_RESPONDING              250

// Cannot communicate with smartcard due to ATR conflict
#define ETSTATUS_SC_ATR_CONFLICT                251

// Smartcard cannot be accessed due to other outstanding connections
#define ETSTATUS_SC_CONN_ERROR                  252

// Detected communication error with smartcard
#define ETSTATUS_SC_COMM_ERROR                  253

// Smartcard reader driver error
#define ETSTATUS_SC_READER_DRIVER_ERR           254

// No smartcard readers available
#define ETSTATUS_NO_SC_READERS                  255

// Smartcard resource manager is not running
#define ETSTATUS_SC_RES_MANAGER_STOPPED         256

// No smartcard present in the reader
#define ETSTATUS_NO_SC_IN_READER                257

// Smartcard or reader is not ready
#define ETSTATUS_SC_READER_NOT_READY            258

// Reader is not available for use
#define ETSTATUS_READER_NOT_AVAILABLE           259

// Specified reader name is not recognized
#define ETSTATUS_READER_NOT_RECOGNISED          260

// Smartcard is not responding to reset
#define ETSTATUS_UNRESPONSIVE_CARD              261

// ═══════════════════════════════════════════════════════════════════
// JSON & XML Parsing Errors (262-282)
// ═══════════════════════════════════════════════════════════════════

// Received empty JSON array
#define ETSTATUS_JSON_ARRAY_EMPTY               262

// Failed to retrieve element from JSON array
#define ETSTATUS_JSON_ARRAY_GET_ERROR           263

// Received JSON non object type where object is expected
#define ETSTATUS_JSON_NON_OBJECT_TYPE           264

// Could not create new JSON object
#define ETSTATUS_JSON_CREATE_FAILED             265

// Could not add attributes to JSON object
#define ETSTATUS_JSON_ADD_FAILED                266

// Provided JSON object type is not supported
#define ETSTATUS_UNSUPPORTED_JSON_DATA_TYPE     267

// JSON attribute data type is different from expected
#define ETSTATUS_UNEXPECTED_JSON_TYPE           268

// Error occurred while parsing XML
#define ETSTATUS_XML_PARSE_ERROR                269

// Error occurred while accessing the XML root node
#define ETSTATUS_XML_GET_ROOT_ERROR             270

// Could not find the searched XML node
#define ETSTATUS_XML_NODE_NOT_FOUND             271

// XML node does not contain any values
#define ETSTATUS_XML_NODE_EMPTY                 272

// XML node does not contain the searched attribute
#define ETSTATUS_XML_ATTRIBUTE_NOT_FOUND        273

// Failed to initialize xmlsec library
#define ETSTATUS_XMLSEC_INIT_FAILED             274

// Failed to check xmlsec library version
#define ETSTATUS_XMLSEC_VER_CHECK_FAILED        275

// Failed to initialize xmlsec crypto app
#define ETSTATUS_INIT_CRYPTO_APP_ERROR          276

// Failed to initialize xmlsec crypto library
#define ETSTATUS_INIT_CRYPTO_LIB_ERROR          277

// Failed to find xmlsec signature node
#define ETSTATUS_XMLSEC_GET_NODE_ERROR          278

// Failed to generate signature context
#define ETSTATUS_XMLSEC_SIG_CTXT_ERROR          279

// Failed to load xmlsec public key
#define ETSTATUS_LOAD_XMLSEC_PUB_KEY_FAILED     280

// Xmlsec digital signature context error
#define ETSTATUS_XMLSEC_DSIG_CTXT_ERROR         281

// Digital signature verification failed
#define ETSTATUS_XMLSEC_SIG_VERIFY_FAILED       282

// Object already added to config map
#define ETSTATUS_OBJECT_ALREADY_ADDED           283

// Error in converting BCD to hex
#define ETSTATUS_CONVERTING_BCD_TO_HEX          284

// The object already exists
#define ETSTATUS_ERROR_ALREADY_EXISTS           285

// Function called with null hash map object
#define ETSTATUS_NULL_HASH_MAP                  286

// Function called with uninitialized hash map
#define ETSTATUS_HASH_MAP_NOT_INITIALIZED       287

// Detected hashmap corruption
#define ETSTATUS_HASH_MAP_CORRUPTED             288

// JSON parsing failed
#define ETSTATUS_JSON_PARSER_ERROR              289

// JSON query failed
#define ETSTATUS_JSON_GET_ERROR                 290

// Function called with null JSON object
#define ETSTATUS_NULL_JSON_OBJECT               291

// Received non array type where array is expected
#define ETSTATUS_JSON_NON_ARRAY_TYPE            292

// Failed to get Java environment from JVM instance
#define ETSTATUS_JAVA_GET_ENV_FAILED            293

// JAVA_HOME environment variable does not exist
#define ETSTATUS_INVALID_ENV                    294

// Validation Gateway (VG) response validity interval is incorrect
#define ETSTATUS_INVALID_VALIDITY_INTERVAL      295

// Failed to load config file from the provided config URL
#define ETSTATUS_LOAD_CONFIG_ERROR              296

// Invalid authentication credentials (E401-77-03)
#define ETSTATUS_INVALID_AUTH_CRED              297

// ═══════════════════════════════════════════════════════════════════
// Plugin License & Device Errors (298-310)
// ═══════════════════════════════════════════════════════════════════

// Plugin reported that the usage license is not valid
#define ETSTATUS_PLUGIN_LICENSE_INVALID         298

// Plugin reported that the usage license not found
#define ETSTATUS_PLUGIN_LICENSE_NOT_FOUND       299

// Plugin reported that the usage license expired
#define ETSTATUS_PLUGIN_LICENSE_EXPIRED         300

// Plugin reported that the environment or device not supported
#define ETSTATUS_PLUGIN_NOT_SUPPORTED           301

// Invalid data protection public key
#define ETSTATUS_INVALID_DATA_PROT_KEY          302

// No smartcard reader found with Emirates ID card
#define ETSTATUS_NO_READER_WITH_EMIRATES_ID     303

// SAM card PIN is not valid
#define ETSTATUS_INVALID_SAM_PIN                304

// SAM Card PIN verification failed
#define ETSTATUS_SAM_PINVERIFICATION_FAILED     305

// SAM card PIN is blocked
#define ETSTATUS_SAM_PIN_BLOCKED                306

// Emirates ID version is not supported
#define ETSTATUS_ID_VER_NOT_SUPPORTED           307

// Failed to get plugin directory path from config file
#define ETSTATUS_PLUGIN_DIR_PATH_ERROR          308

// Agent and Toolkit version mismatch
#define ETSTATUS_AGENT_TKT_VER_MISMATCH         309

// Failed to disconnect fingerprint sensor
#define ETSTATUS_FP_CLOSE_DEV_ERROR             310

// Invalid or incomplete combi reader plugin configuration
#define ETSTATUS_INVALID_COMBI_READER_CONFIG    311

// Plugin has multiple list reader functions
#define ETSTATUS_DUP_PLG_LIST_READERS           312

// Validation Gateway (VG) response signing certificate is invalid
#define ETSTATUS_VG_RESP_CERT_INVALID           313

// Failed to parse "config_li" file as the number of services in the Toolkit license exceeds the limit
#define ETSTATUS_LIC_LIMIT_REACHED              314

// Invalid template tag in the ID Card raw data
#define ETSTATUS_INVALID_TEMP_TAG               315

// Failed to retrieve authentication certificate from the PKI certificate file
#define ETSTATUS_AUTH_CERT_NOT_FOUND            316

// Plugin does not support NFC capability
#define ETSTATUS_NFC_NOT_SUPPORTED              317

// NFC tag is not set
#define ETSTATUS_NFC_TAG_NOT_SET                318

// Invalid MRZ string
#define ETSTATUS_INVALID_MRZ_DATA               319

// MRZ string contains invalid checksum
#define ETSTATUS_INVALID_MRZ_CHECKSUM           320

// Plugin service is not installed/running
#define ETSTATUS_SVC_NOT_RUNNING                321

// ID card schema got corrupted
#define ETSTATUS_ID_SCHEMA_CORRUPTED            322

// ID card command limit reached
#define ETSTATUS_ID_CMD_LIMIT_EXCEEDED          323

// HTTP response header received has "content-length" zero
#define ETSTATUS_INVALID_CONTENT_LEN_ERROR      324

// HTTP response header does not contain "content-length" attribute
#define ETSTATUS_NO_CONTENT_LENGTH              325

// Provided URL is of invalid format
#define ETSTATUS_URL_BAD_FORMAT                 326

// Failed to retrieve detached signature data
#define ETSTATUS_NO_SIG_DATA                    327

// SAM context is not opened
#define ETSTATUS_SAM_CONTEXT_NOT_OPENED         328

// ═══════════════════════════════════════════════════════════════════
// Card Container & Data File Errors (329-345)
// ═══════════════════════════════════════════════════════════════════

// Data file not found
#define ETSTATUS_DATA_FILE_NOT_FOUND            329

// Read access condition locked
#define ETSTATUS_READ_AC_LOCKED                 330

// Update access condition locked
#define ETSTATUS_WRITE_AC_LOCKED                331

// Key file not found
#define ETSTATUS_KEY_FILE_NOT_FOUND             332

// PIN file not found
#define ETSTATUS_PIN_FILE_NOT_FOUND             333

// Maximum object limit reached
#define ETSTATUS_OBJ_LIMIT_REACHED              334

// Invalid encoding type
#define ETSTATUS_INVALID_ENC_TYPE               335

// Emirates ID Card is not personalized
#define ETSTATUS_CARD_NOT_PERS                  336

// Maximum attribute limit reached
#define ETSTATUS_ATTR_LIMIT_REACHED             337

// Invalid data file command count
#define ETSTATUS_INVALID_CMD_COUNT              338

// Invalid dedicated file type
#define ETSTATUS_INVALID_DF_TYPE                339

// Failed to load container schema
#define ETSTATUS_CONT_SCH_NOT_LOADED            340

// Invalid container type
#define ETSTATUS_INVALID_CONTAINER_TYPE         341

// Invalid file type
#define ETSTATUS_INVALID_FILE_TYPE              342

// Number of commands in the response exceeds the limit
#define ETSTATUS_CMD_LIMIT_REACHED              343

// Data file is created using invalid access conditions
#define ETSTATUS_INVALID_AC                     344

// Emirates ID card container attribute type limit reached
#define ETSTATUS_ID_CONT_ATTR_LIMIT_EXCEEDED    345

// ═══════════════════════════════════════════════════════════════════
// Miscellaneous Errors (346-366)
// ═══════════════════════════════════════════════════════════════════

// Operation timed out
#define ETSTATUS_ERROR_OP_TIMEOUT               346

// Failed to load module handle
#define ETSTATUS_LOAD_MOD_HANDLE_ERROR          347

// Loaded plugins does not contain method to set NFC tag object
#define ETSTATUS_NO_SET_NFC_TAG_METHOD          348

// Number of allowed nodes in the license exceeds the limit
#define ETSTATUS_NODES_LIMIT_REACHED            349

// Error in retrieving serial number from connected smart card reader
#define ETSTATUS_SC_READER_SN_ERROR             350

// Failed to get ESB service name
#define ETSTATUS_ESB_SVC_NAME_ERROR             351

// Failed to get ESB source channel
#define ETSTATUS_ESB_SRC_CHANNEL_ERROR          352

// Failed to get ESB service version
#define ETSTATUS_ESB_SVC_VER_ERROR              353

// Failed to get ESB service language
#define ETSTATUS_ESB_SVC_LANG_ERROR             354

// Failed to get ESB user name
#define ETSTATUS_ESB_USER_NAME_ERROR            355

// Failed to get ESB password
#define ETSTATUS_ESB_PWD_ERROR                  356

// Failed to get ESB shared key
#define ETSTATUS_ESB_KEY_ERROR                  357

// Failed to get ESB consumer ID
#define ETSTATUS_ESB_CONSUMER_ID_ERROR          358

// Response header does not contain session ID (Set-Cookie)
#define ETSTATUS_NO_SESSION_ID                  359

// Received error response from ESB interface
#define ETSTATUS_ESB_ERROR                      360

// Data signature validation failed as the key used for validation is not matching with the signing key
#define ETSTATUS_INVALID_PUB_KEY                361

// Failed to validate hash of the signature data
#define ETSTATUS_HASH_VALIDATION_ERROR          362

// Config URL and directory not provided
#define ETSTATUS_CONFIG_LOC_NOT_PROVIDED        363

// Failed to parse app configuration from config file
#define ETSTATUS_PARSE_APP_CONFIG_ERROR         364

// Failed to parse config params
#define ETSTATUS_PARSE_CONFIG_PARAMS_ERROR      365

// Failed to parse EF data
#define ETSTATUS_PARSE_EF_DATA_ERROR            366

// Reader monitoring: wait timed out with no state change
// GetStatusMessage: "Reader monitoring wait timed out"
#define ETSTATUS_READER_WAIT_TIMEOUT            367

// Reader monitoring: wait was cancelled by CancelReaderWait()
// GetStatusMessage: "Reader monitoring wait was cancelled"
#define ETSTATUS_READER_WAIT_CANCELLED          368

// The loaded plugin does not implement the requested monitoring function
// GetStatusMessage: "Monitoring function not supported by plugin"
#define ETSTATUS_PLUGIN_FUNCTION_NOT_SUPPORTED  369

// General reader monitoring error (plugin returned non-OK)
// GetStatusMessage: "Reader monitoring error"
#define ETSTATUS_READER_GENERAL_ERROR           370

// Fingerprint capture rejected: image quality below the required threshold
// GetStatusMessage: "Fingerprint quality below the required threshold"
#define ETSTATUS_FP_LOW_QUALITY                 371

// Fingerprint capture rejected: liveness check failed
// GetStatusMessage: "Fingerprint liveness check failed"
#define ETSTATUS_FP_LIVENESS_FAILED             372

//}}END: Emirates ID Card Toolkit status codes

#if __GNUC__ >= 4
#pragma GCC visibility pop
#endif

#endif // EIDA_TOOLKIT_ERROR_H_
