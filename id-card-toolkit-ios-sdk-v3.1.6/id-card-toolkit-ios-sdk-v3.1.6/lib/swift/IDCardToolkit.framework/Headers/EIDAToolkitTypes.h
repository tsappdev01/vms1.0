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
 * @file EIDAToolkitTypes.h
 * @brief Data types, structures, and enumerations for the Emirates ID
 *        Card Toolkit public API.
 *
 * Defines ETSTATUS error code type with embedded source location,
 * digital signature types (PAdES/XAdES/CAdES), card data structures,
 * and NFC session types.
 */

#ifndef EIDA_TOOLKIT_DATA_TYPES_H_
#define EIDA_TOOLKIT_DATA_TYPES_H_

#if __GNUC__ >= 4
#pragma GCC visibility push(hidden)
#endif

#ifndef TRUE
#define TRUE 1
#endif

#ifndef FALSE
#define FALSE 0
#endif

#ifndef BOOL
#ifdef ARM_WIN
typedef int BOOL;
#endif
#ifndef ARM_WIN
#define BOOL int
#endif
#endif

/**
 * EIDA Toolkit (ET) Status code.
 *
 * A 32-bit value encoding both the error code and the source location
 * where the error originated, enabling precise diagnostics.
 *
 * Bit layout:
 * @code
 *   31      25 24    14 13      0
 *   +--------+--------+-+-------+
 *   | FileID |  Line  |S| Code  |
 *   +--------+--------+-+-------+
 *
 *   Code   (bits 0-12):  Error code (0 = success, see EIDAToolkitError.h)
 *   S      (bit 13):     System error flag (1 = system/OS error)
 *   Line   (bits 14-24): Source line number where error originated
 *   FileID (bits 25-31): Source file identifier
 * @endcode
 *
 * Use ETSTATUS_CODE() to extract the error code for comparison with
 * the ETSTATUS_* constants defined in EIDAToolkitError.h.
 *
 * Example:
 * @code
 *   ETSTATUS status = ReadPublicData(...);
 *   if (ETSTATUS_CODE(status) != ETSTATUS_SUCCESS) {
 *       printf("Error %d at file %d line %d: %s\n",
 *              ETSTATUS_CODE(status),
 *              ETSTATUS_SOURCE_FILE(status),
 *              ETSTATUS_SOURCE_LINE(status),
 *              GetStatusMessage(status));
 *   }
 * @endcode
 */
typedef unsigned int ETSTATUS;

/** Extract the error code (bits 0-12) from an ETSTATUS value */
#define ETSTATUS_CODE(ets)          ((ets) & 0x1FFF)

/** Extract the system error flag (bit 13) from an ETSTATUS value */
#define ETSTATUS_SYSTEM_ERROR(ets)  ((((unsigned int)ets >> 13) & 0x1))

/** Extract the source file identifier (bits 25-31) from an ETSTATUS value */
#define ETSTATUS_SOURCE_FILE(ets)   ((((unsigned int)ets >> 25) & 0x7F))

/** Extract the source line number (bits 14-24) from an ETSTATUS value */
#define ETSTATUS_SOURCE_LINE(ets)   ((((unsigned int)ets >> 14) & 0x7FF))

/** Maximum size for general-purpose string fields (URLs, paths, names) */
#define MAX_SIZE        256

/** Maximum size for short code fields (country codes, date strings) */
#define CODE_MAX        16

/** Maximum size for public data attribute fields */
#define MAX_ATTR_SIZE   64

/**
 * Opaque handle for NFC session or tag objects.
 * On iOS, cast from NFCTagReaderSession* / NFCISO7816Tag*.
 * On Android, cast from the platform NFC object.
 */
typedef void* ID_OBJECT;

/**
 * Card Production Life Cycle (CPLC) information.
 *
 * Contains raw CPLC data retrieved from the smartcard via
 * GET DATA commands using tags 0101 and 9F7F.
 *
 * @see GetCPLCData()
 */
typedef struct _CPLC_INFO_ {
    unsigned char cplc_0101_data[MAX_SIZE]; /**< CPLC tag 0101 raw data (IC fabrication info) */
    unsigned int cplc_0101_len;             /**< Length of cplc_0101_data in bytes */
    unsigned char cplc_9F7F_data[MAX_SIZE]; /**< CPLC tag 9F7F raw data (card production details) */
    unsigned int cplc_9F7F_len;             /**< Length of cplc_9F7F_data in bytes */
} CPLC_INFO, *PCPLC_INFO;

#if !defined(TOOLKIT_VERIFONE_SUPPORT)

/**
 * Digital signature conformance levels (EU AdES standard).
 *
 * @see PadesSign(), XadesSign(), CadesSign()
 */
typedef enum _SIGNATURE_LEVEL {
    BASELINE_B = 1,   /**< Basic level: signature + signer certificate */
    BASELINE_LT = 2,  /**< Long-Term: adds OCSP/CRL for offline validation */
    BASELINE_LTA = 3, /**< Long-Term Archival: adds archive timestamp */
    BASELINE_T = 4    /**< Timestamp: adds trusted timestamp to B level */
} SIGNATURE_LEVEL;

/**
 * Digital signature packaging mode.
 *
 * Determines how the signature relates to the signed document.
 */
typedef enum _PACKAGING_MODE {
    ENVELOPED = 1,  /**< Signature embedded within the document */
    ENVELOPING = 2, /**< Document embedded within the signature */
    DETACHED = 3,   /**< Signature stored separately from the document */
    DEATACHED = 3   /**< @deprecated Use DETACHED instead */
} PACKAGING_MODE;

/** Digital signature document type */
typedef enum _DOCUMENT_TYPE {
    XADES = 1, /**< XML Advanced Electronic Signatures */
    PADES = 2, /**< PDF Advanced Electronic Signatures */
    CADES = 3  /**< CMS Advanced Electronic Signatures */
} DOCUMENT_TYPE;

/**
 * Signature verification report detail level.
 *
 * @see PadesVerify(), XadesVerify(), CadesVerify()
 */
typedef enum _REPORT_TYPE {
    DETAILED = 1,   /**< Full verification details with certificate chain */
    DIAGNOSTIC = 2, /**< Diagnostic information for troubleshooting */
    SIMPLE = 3      /**< Pass/fail result only */
} REPORT_TYPE;

/** Signer's physical location for digital signature metadata */
typedef struct _SIGNER_LOCATION {
    char country_code[CODE_MAX];       /**< ISO 3166-1 alpha-2 country code (e.g. "AE") */
    char state_or_province[MAX_SIZE];  /**< State or province name */
    char postal_code[CODE_MAX];        /**< Postal/zip code */
    char locality[MAX_SIZE];           /**< City or locality name */
    char street[MAX_SIZE];             /**< Street address */
} SIGNER_LOCATION;

/**
 * Digital signature context for PAdES, XAdES, and CAdES operations.
 *
 * Populate this structure before calling PadesSign(), XadesSign(), or
 * CadesSign(). The PIN must be encrypted using the data protection key
 * obtained via GetDataProtectionKey().
 *
 * @see PadesSign(), XadesSign(), CadesSign(), GetDataProtectionKey()
 */
typedef struct _SIGNING_CONTEXT {
    SIGNATURE_LEVEL signature_level;  /**< Conformance level (B, T, LT, LTA) */
    PACKAGING_MODE packaging_mode;    /**< How signature relates to document */
    SIGNER_LOCATION location;         /**< Signer's location metadata */
    char tsa_url[MAX_SIZE];           /**< Timestamp Authority URL (required for T/LT/LTA levels) */
    char ocsp_url[MAX_SIZE];          /**< OCSP responder URL (required for LT/LTA levels) */
    char cert_path[MAX_SIZE];         /**< Path to certificate chain file for validation */
    char pin[512];                    /**< Emirates ID Card PIN (encrypted, base64-encoded) */
} SIGNING_CONTEXT;

/** Position of signer name relative to signature image in PAdES */
typedef enum _SIGNER_NAME_POSITION {
    LEFT = 1,   /**< Name displayed to the left of signature image */
    RIGHT = 2,  /**< Name displayed to the right of signature image */
    TOP = 3,    /**< Name displayed above signature image */
    BOTTOM = 4  /**< Name displayed below signature image */
} SIGNER_NAME_POSITION;

/**
 * PAdES (PDF Advanced Electronic Signature) visual parameters.
 *
 * Controls the appearance of the visible signature in a PDF document.
 * Set sign_visible = FALSE (0) for invisible signature.
 *
 * @see PadesSign(), PadesSignEx()
 */
typedef struct _PADES_SIGN_PARAMS {
    char sign_reason[MAX_SIZE];          /**< Reason for signing (e.g. "Approval") */
    char signer_location[MAX_SIZE];      /**< Signer location text displayed in signature */
    char signer_contact_info[MAX_SIZE];  /**< Signer contact information */
    int signature_xaxis;                 /**< X position of signature box in points */
    int signature_yaxis;                 /**< Y position of signature box in points */
    char signature_image[MAX_SIZE];      /**< File path to signature image (PNG/JPEG) */
    char font_name[MAX_SIZE];            /**< Font name for signature text (e.g. "Helvetica") */
    char background_color[CODE_MAX];     /**< Background color as hex string (e.g. "#FFFFFF") */
    char font_color[CODE_MAX];           /**< Font color as hex string (e.g. "#000000") */
    int font_size;                       /**< Font size in points */
    char signature_text[MAX_SIZE];       /**< Custom text to display in signature box */
    int sign_visible;                    /**< TRUE for visible signature, FALSE for invisible */
    SIGNER_NAME_POSITION name_position;  /**< Position of signer name relative to image */
    int page_number;                     /**< PDF page number for visible signature (1-based) */
} PADES_SIGN_PARAMS;

/**
 * Digital signature verification context.
 *
 * @see PadesVerify(), XadesVerify(), CadesVerify()
 */
typedef struct _VERIFICATION_CONTEXT {
    char ocsp_url[MAX_SIZE];  /**< OCSP responder URL for certificate status checking */
    char cert_path[MAX_SIZE]; /**< Path to trusted certificate chain for validation */
    REPORT_TYPE report_type;  /**< Level of detail in the verification report */
    BOOL is_deattached;       /**< TRUE if signature is detached from document */
} VERIFICATION_CONTEXT;

/**
 * Parsed Validation Gateway (VG) response attributes.
 *
 * Populated by ValidateToolkitResponse() after verifying the
 * VG response signature.
 *
 * @see ValidateToolkitResponse()
 */
typedef struct _VGRESPONSE_DATA {
    char service[MAX_SIZE];         /**< Service type (e.g. "ReadPublicData") */
    char action[MAX_SIZE];          /**< Action type (e.g. "CompleteAuthentication") */
    char request_id[MAX_SIZE];      /**< Application request ID echoed back (base64) */
    char nonce[MAX_SIZE];           /**< Server nonce (base64) */
    char correlation_id[MAX_SIZE];  /**< UUID correlating multi-step transactions */
    char csn[MAX_SIZE];             /**< Card Serial Number */
    char cardnumber[MAX_SIZE];      /**< Card Number (CN) */
    char idnumber[MAX_SIZE];        /**< Emirates ID Number (IDN) */
    char time_stamp[MAX_SIZE];      /**< Server timestamp (ISO 8601 format) */
    int validity_interval;          /**< Response validity period in seconds */
} VGRESPONSE_DATA;

/**
 * Parsed Machine Readable Zone (MRZ) data from Emirates ID Card.
 *
 * Populated by ParseMRZData() from the raw MRZ string.
 *
 * @see ParseMRZData(), SetNfcAuthenticationParams()
 */
typedef struct _MRZ_DATA {
    char document_type[CODE_MAX];     /**< Document type code (e.g. "I" for ID card) */
    char issued_country[CODE_MAX];    /**< Issuing country code (e.g. "ARE") */
    char cardnumber[CODE_MAX];        /**< Card Number extracted from MRZ */
    char idnumber[CODE_MAX];          /**< Emirates ID Number extracted from MRZ */
    char date_of_birth[CODE_MAX];     /**< Date of birth (YYMMDD format) */
    char gender[CODE_MAX];            /**< Gender ("M" or "F") */
    char card_expiry_date[CODE_MAX];  /**< Card expiry date (YYMMDD format) */
    char nationality[CODE_MAX];       /**< Nationality code (e.g. "ARE") */
    char fullname[32];                /**< Full name as encoded in MRZ */
} MRZ_DATA;

/**
 * Elementary File (EF) types for individual public data reads.
 *
 * Use with ReadPublicDataEF() to read a single data file from
 * the Emirates ID Card instead of all public data at once.
 *
 * @see ReadPublicDataEF()
 */
typedef enum _PUBLIC_DATA_EF_TYPE {
    IDN_CN = 1,              /**< ID Number and Card Number */
    ROOT_CERTIFICATE = 2,    /**< Card root certificate (X.509 DER) */
    NON_MODIFIABLE_DATA = 3, /**< Fixed cardholder data (name, DOB, nationality) */
    MODIFIABLE_DATA = 4,     /**< Updateable cardholder data (phone, email) */
    PHOTOGRAPHY = 5,         /**< Cardholder photo (JPEG) */
    SIGNATURE_IMAGE = 6,     /**< Handwritten signature image */
    HOME_ADDRESS = 7,        /**< Cardholder home address */
    WORK_ADDRESS = 8         /**< Cardholder work address */
} PUBLIC_DATA_EF_TYPE;
#endif

/**
 * Public data attributes read from Emirates ID Card.
 *
 * Used by ReadPublicDataEx() which returns a flat structure
 * of the most commonly needed cardholder fields.
 *
 * @see ReadPublicDataEx()
 */
typedef struct _PUBLIC_DATA_OBJ {
    char id_number[MAX_ATTR_SIZE];          /**< Emirates ID Number (e.g. "784-XXXX-XXXXXXX-X") */
    char card_number[MAX_ATTR_SIZE];        /**< Card Number */
    char expiry_date[MAX_ATTR_SIZE];        /**< Card expiry date */
    char fullname_arabic[MAX_ATTR_SIZE];    /**< Full name in Arabic */
    char fullname_english[MAX_ATTR_SIZE];   /**< Full name in English */
    char date_of_birth[MAX_ATTR_SIZE];      /**< Date of birth */
    char mobile_phone_number[MAX_ATTR_SIZE];/**< Registered mobile phone number */
    char email[MAX_ATTR_SIZE];              /**< Registered email address */
    char title_arabic[MAX_ATTR_SIZE];       /**< Title in Arabic */
    char title_english[MAX_ATTR_SIZE];      /**< Title in English */
    char gender[MAX_ATTR_SIZE];             /**< Gender */
    char csn[MAX_ATTR_SIZE];               /**< Card Serial Number */
} PUBLIC_DATA_OBJ;

/**
 * Expiry dates of service provider configuration certificates.
 *
 * Populated by GetConfigCertificateExpiryDate() to help service
 * providers monitor certificate expiry and plan renewals.
 *
 * @see GetConfigCertificateExpiryDate()
 */
typedef struct _CONFIG_CERT_EXPIRY_DATE {
    char config_vg_cert_expiry[MAX_ATTR_SIZE];   /**< VG config certificate expiry date */
    char config_lv_cert_expiry[MAX_ATTR_SIZE];   /**< License validator certificate expiry */
    char server_tls_cert_expiry[MAX_ATTR_SIZE];  /**< Server TLS certificate expiry date */
    char config_ag_cert_expiry[MAX_ATTR_SIZE];   /**< Agent config certificate expiry date */
    char license_expiry[MAX_ATTR_SIZE];          /**< Service provider license expiry date */
} CONFIG_CERT_EXPIRY_DATE;

/**
 * NFC session details for iOS/Android contactless card access.
 *
 * On iOS, session is the NFCTagReaderSession and tag is the
 * NFCISO7816Tag discovered during the session.
 *
 * @see SetNFCTag()
 */
typedef struct _NFC_OBJECT {
    ID_OBJECT session; /**< NFC reader session handle (platform-specific) */
    ID_OBJECT tag;     /**< NFC tag handle representing the card (platform-specific) */
} NFC_OBJECT;

/**
 * Dedicated File (DF) container types on the Emirates ID Card.
 *
 * Each container groups related data files under a single application.
 *
 * @see UpdateData()
 */
typedef enum _DF_TYPE {
    HEALTH_INSURANCE_APP = 1, /**< Health insurance data container */
    ID_APPLICATION = 2,       /**< Main ID application container */
    ADDRESS_APP = 3,          /**< Address data container */
    FAMILYBOOK_APP = 4        /**< Family book data container */
} DF_TYPE;

/**
 * Data file types within card containers.
 *
 * Used with ReadData() to read specific files, or with UpdateData()
 * for card personalization operations.
 *
 * @see ReadData(), UpdateData()
 */
typedef enum _FILE_TYPE {
    HEALTH_INSURANCE = 0,           /**< Health insurance data */
    HEALTH_INSURANCE_PROTECTED = 1, /**< Protected health insurance data */
    MOD_DATA = 2,                   /**< Modifiable cardholder data */
    HOMEADDRESS = 3,                /**< Home address */
    WORKADDRESS = 4,                /**< Work address */
    HEAD_OF_FAMILY = 5,             /**< Family book: head of family record */
    WIFE1 = 6,                      /**< Family book: wife 1 record */
    WIFE2 = 7,                      /**< Family book: wife 2 record */
    WIFE3 = 8,                      /**< Family book: wife 3 record */
    WIFE4 = 9,                      /**< Family book: wife 4 record */
    CHILD1 = 10,                    /**< Family book: child 1 record */
    CHILD2 = 11,                    /**< Family book: child 2 record */
    CHILD3 = 12,                    /**< Family book: child 3 record */
    CHILD4 = 13,                    /**< Family book: child 4 record */
    CHILD5 = 14,                    /**< Family book: child 5 record */
    CHILD6 = 15,                    /**< Family book: child 6 record */
    CHILD7 = 16,                    /**< Family book: child 7 record */
    CHILD8 = 17,                    /**< Family book: child 8 record */
    CHILD9 = 18,                    /**< Family book: child 9 record */
    CHILD10 = 19,                   /**< Family book: child 10 record */
    CHILD11 = 20,                   /**< Family book: child 11 record */
    CHILD12 = 21,                   /**< Family book: child 12 record */
    CHILD13 = 22,                   /**< Family book: child 13 record */
    CHILD14 = 23,                   /**< Family book: child 14 record */
    CHILD15 = 24,                   /**< Family book: child 15 record */
    CHILD16 = 25,                   /**< Family book: child 16 record */
    CHILD17 = 26,                   /**< Family book: child 17 record */
    CHILD18 = 27,                   /**< Family book: child 18 record */
    CHILD19 = 28,                   /**< Family book: child 19 record */
    CHILD20 = 29                    /**< Family book: child 20 record */
} FILE_TYPE;

/*
 * Reader state flags -- returned by GetReaderStatus() and WaitForCardChange().
 * Bitmask; multiple flags may be set simultaneously.
 */
#define READER_STATE_UNKNOWN        0x00u
#define READER_STATE_EMPTY          0x01u
#define READER_STATE_CARD_PRESENT   0x02u
#define READER_STATE_MUTE           0x04u
#define READER_STATE_UNAVAILABLE    0x08u
#define READER_STATE_EXCLUSIVE      0x10u

#define READER_WAIT_INFINITE        0xFFFFFFFFu

#if __GNUC__ >= 4
#pragma GCC visibility pop
#endif

#endif // EIDA_TOOLKIT_DATA_TYPES_H_
