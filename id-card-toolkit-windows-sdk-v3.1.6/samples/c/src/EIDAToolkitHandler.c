// Standard C header files
#include <stdio.h>
#if defined(_WIN32) || defined(_WIN64)
#include <conio.h>
#include <strsafe.h>
#endif
#include <string.h>
#include <stdlib.h>

// EIDA Toolkit header files
#include <EIDAToolkitTypes.h>
#include <EIDAToolkitError.h>
#include <EIDAToolkitApi.h>

// Project local header files
#include "EIDAToolkitOptions.h"
#include "EIDAToolkitConsole.h"
#include "LibXmlParser.h"

// Third party library header files
#include <json.h>
#include <openssl/rand.h>
#include <openssl/rsa.h>
#include <openssl/x509.h>
#include <openssl/pem.h>
// XMLSEC_CRYPTO_OPENSSL / XMLSEC_STATIC / XMLSEC_NO_XSLT come from the xmlsec
// vcpkg target's compile definitions; redundant local defines removed (C4005).
#if defined(LINUX)
#define XMLSEC_NO_SIZE_T
#endif

#include <xmlsec/xmlsec.h>
#include <xmlsec/xmltree.h>
#include <xmlsec/xmldsig.h>
#include <xmlsec/templates.h>
#include <xmlsec/crypto.h>

// Define unused parameters
#define UNUSED_PARAMETER(x) (void)x

static const char _base64_encoding_table[] = {
    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H',
    'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
    'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X',
    'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f',
    'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
    'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
    'w', 'x', 'y', 'z', '0', '1', '2', '3',
    '4', '5', '6', '7', '8', '9', '+', '/'
};

static const char _base64_decoding_table[] = {
    0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 62, 0, 0, 0, 63,
    52, 53, 54, 55, 56, 57, 58, 59,
    60, 61, 0, 0, 0, 0, 0, 0,
    0, 0, 1, 2, 3, 4, 5, 6,
    7, 8, 9, 10, 11, 12, 13, 14,
    15, 16, 17, 18, 19, 20, 21, 22,
    23, 24, 25, 0, 0, 0, 0, 0,
    0, 26, 27, 28, 29, 30, 31, 32,
    33, 34, 35, 36, 37, 38, 39, 40,
    41, 42, 43, 44, 45, 46, 47, 48,
    49, 50, 51
};

static const unsigned int _base64_mod_table[] = { 0, 2, 1 };

// Compute the length of buffer required to store base64 encoded data
unsigned int GetBase64EncodeLength(unsigned int input_data_length) {
    return 4 * ((input_data_length + 2) / 3);
}

// Compute the length of buffer required to store base64 decoded data
unsigned int GetBase64DecodeLength(const unsigned char *encoded_data,
    unsigned int encoded_data_length) {

    unsigned int decoded_length = 0;

    if (!encoded_data || !encoded_data_length) {
        return 0;
    }

    decoded_length = (encoded_data_length / 4) * 3;
    if (encoded_data[encoded_data_length - 1] == '=') {
        decoded_length--;
    }

    if (encoded_data[encoded_data_length - 2] == '=') {
        decoded_length--;
    }

    return decoded_length;
}

// Encode data in base64 format
ETSTATUS Base64Encode(const unsigned char *data, unsigned int data_length,
    unsigned char **encoded_data, unsigned int *encoded_data_length) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    unsigned int i, j;
    unsigned int octet_a;
    unsigned int octet_b;
    unsigned int octet_c;
    unsigned int triple;
    unsigned int encoded_length = 0;

    // Begin function logic
    for (;;) {

        // verify input/output parameters
        if (!data || !data_length || !encoded_data ||
            !encoded_data_length) {
            return 1;
        }

        // Compute the length required for the encoded data
        encoded_length = GetBase64EncodeLength(data_length);

        // If the buffer is provided, check if the length is sufficient
        if (*encoded_data && (*encoded_data_length < (encoded_length + 1))) {
            break;
        }

        // Allocate memory if buffer is not provided
        if (!*encoded_data) {

            // Allocate memory for the data
            *encoded_data = malloc(encoded_length + 1);
            if (!*encoded_data) {
                return 1;
            }

            memset(*encoded_data, 0, encoded_length + 1);

        }

        // Set the output encoded data length
        *encoded_data_length = encoded_length;

        for (i = 0, j = 0; i < data_length;) {
            octet_a = i < data_length ? (unsigned char)data[i++] : 0;
            octet_b = i < data_length ? (unsigned char)data[i++] : 0;
            octet_c = i < data_length ? (unsigned char)data[i++] : 0;

            triple = (octet_a << 0x10) + (octet_b << 0x08) + octet_c;

            (*encoded_data)[j++] =
                _base64_encoding_table[(triple >> (3 * 6)) & 0x3F];
            (*encoded_data)[j++] =
                _base64_encoding_table[(triple >> (2 * 6)) & 0x3F];
            (*encoded_data)[j++] =
                _base64_encoding_table[(triple >> (1 * 6)) & 0x3F];
            (*encoded_data)[j++] =
                _base64_encoding_table[(triple >> (0 * 6)) & 0x3F];
        }

        for (i = 0; i < _base64_mod_table[data_length % 3]; i++) {
            (*encoded_data)[*encoded_data_length - 1 - i] = '=';
        }

        // Add NULL termination
        (*encoded_data)[*encoded_data_length] = '\0';

        // End of function logic
        break;
    }

    return etstatus;
}

// Decode base64 encoded data into plain data
ETSTATUS Base64Decode(const unsigned char *encoded_data,
    unsigned int encoded_data_length, unsigned char **decoded_data,
    unsigned int *decoded_data_length) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    unsigned int decoded_length;
    unsigned int i, j;
    unsigned int sextet_a;
    unsigned int sextet_b;
    unsigned int sextet_c;
    unsigned int sextet_d;
    unsigned int triple;

    // Begin function logic
    for (;;) {

        // Verify input parameters
        if (!encoded_data || !encoded_data_length || !decoded_data ||
            !decoded_data_length) {
            return 1;
        }
       
        // Compute the decoded data length
        decoded_length = GetBase64DecodeLength(encoded_data,
            encoded_data_length);

        // If the buffer is provided, check if the length is sufficient
        if (*decoded_data && (*decoded_data_length < (decoded_length + 1))) {
            etstatus = 1;
            break;
        }

        // Allocate memory if buffer is not provided
        if (!*decoded_data) {

            // Set the output decoded data length
            *decoded_data_length = decoded_length;

            // Allocate memory for the decoded data
            *decoded_data = malloc((size_t)*decoded_data_length + 1);
            memset(*decoded_data, 0, *decoded_data_length + 1);
        }

        for (i = 0, j = 0; i < encoded_data_length;) {
            sextet_a = encoded_data[i] == '=' ? 0 & i++ :
                _base64_decoding_table[encoded_data[i++]];
            sextet_b = encoded_data[i] == '=' ? 0 & i++ :
                _base64_decoding_table[encoded_data[i++]];
            sextet_c = encoded_data[i] == '=' ? 0 & i++ :
                _base64_decoding_table[encoded_data[i++]];
            sextet_d = encoded_data[i] == '=' ? 0 & i++ :
                _base64_decoding_table[encoded_data[i++]];

            triple = (sextet_a << (3 * 6)) + (sextet_b << (2 * 6))
                + (sextet_c << (1 * 6)) + (sextet_d << (0 * 6));

            if (j < *decoded_data_length) {
                (*decoded_data)[j++] = (triple >> (2 * 8)) & 0xFF;
            }

            if (j < *decoded_data_length) {
                (*decoded_data)[j++] = (triple >> (1 * 8)) & 0xFF;
            }

            if (j < *decoded_data_length) {
                (*decoded_data)[j++] = (triple >> (0 * 8)) & 0xFF;
            }
        }

        // Add NULL termination
        (*decoded_data)[decoded_length] = '\0';

        // End function logic
        break;
    }

    return etstatus;
}

void print_error(char *toolkit_request_id, char *request_id) {
    printf("Toolkit response contains invalid Request ID\n");
    printf("Application Request ID : %s\n", request_id);
    printf("Toolkit Request ID : %s\n", toolkit_request_id);
    printf("Potential replay attack has been detected\n");
}

void remove_newline_ch(char *line)
{
    if (line && line[0]) {
        int new_line = (int)(strlen(line) - 1);
        if (line[new_line] == '\n') {
            line[new_line] = '\0';
        }
    }
}

#if defined(_WIN32) || defined(_WIN64)
static unsigned int get_pin(char *pin, unsigned int *pin_len) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    int i = 0;
    char pin_data[16] = { 0 };
    unsigned int pin_data_len = 0;

    // Begin function logic
    for (;;) {

        // Verify input parameters
        if (!pin || !pin_len) {
            return 1;
        }
        printf("\nPlease enter Emirates ID card PIN: ");
        while (1) {

            i = 0;
            while (i < (int)(sizeof(pin_data) - 1)) {
                char c = (char)getch();
                if (c == 13) {
                    break;
                }
                pin_data[i] = c;
                i++;
                printf("*");
            }
            pin_data[i] = '\0';
            pin_data_len = (unsigned int)strlen(pin_data);

            if ((pin_data_len > 16) || (pin_data_len < 4)) {
                printf("\nThe pin should be minimum 4 characters"
                    "and maximum 16 characters long\n");
                memset(pin_data, 0, sizeof(pin_data));
                printf("\nPlease enter Emirates ID card PIN: ");
                continue;
            }
            break;
        }

        memcpy(pin, pin_data, pin_data_len);

        *pin_len = pin_data_len;
        printf("\n");
        // Zero the local pin_data buffer so plaintext PIN does not
        // linger on the stack after the function returns.
        memset(pin_data, 0, sizeof(pin_data));
        // End function logic
        break;
    }

    return etstatus;
}
#else
static unsigned int get_pin(char *pin, unsigned int *pin_len) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    char pin_data[16] = { 0 };
    unsigned int pin_data_len = 0;

    // Begin function logic
    for (;;) {

        // Verify input parameters
        if (!pin || !pin_len) {
            return 1;
        }
        printf("\nPlease enter Emirates ID card PIN: ");

        while (1) {
            fscanf(stdin, "%15s", pin_data);
            // strlen(pin_data) -- pin_data is a char array; name decays
            // to char*. Previously `strlen(&pin_data)` passed a pointer
            // of type char(*)[16] which is silently-accepted by most
            // compilers (same address value) but type-incorrect.
            pin_data_len = (unsigned int)strlen(pin_data);

            if ((pin_data_len > 16) || (pin_data_len < 4)) {
                printf("\nThe pin should be minimum 4 characters"
                    "and maximum 16 characters long\n");
                memset(pin_data, 0, sizeof(pin_data));
                printf("\nPlease enter Emirates ID card PIN: ");
                continue;
            }
            break;
        }
        memcpy(pin, &pin_data[0], pin_data_len);

        *pin_len = pin_data_len;

        printf("\n");
        // Zero the local pin_data buffer so plaintext PIN does not
        // linger on the stack after the function returns.
        memset(pin_data, 0, sizeof(pin_data));
        // End function logic
        break;
    }

    return etstatus;
}
#endif

// Structure defines type of nfc authentication
typedef struct _NFC_AUTH {
    const char *description;
} NFC_AUTH;

// Global Toolkit service handlers
static const NFC_AUTH _nfc_auth[] = {
    { "Quit" },
    { "Set NFC authentication paramters" },
    { "Set MRZ data" }
};

// Global count of Toolkit services
static const int _nfc_auth_count = sizeof(_nfc_auth) /
sizeof(_nfc_auth[0]);

// Get Validation Gateway (VG) public key
ETSTATUS GetPublicKey(RSA **rsa_pubkey) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    unsigned char *data_protection_key = NULL;
    unsigned int key_len = 0;
    char *modulus = NULL;
    unsigned int modulus_len = 0;
    char *exponent = NULL;
    unsigned int exponent_len = 0;
    BIO *bio = NULL;

    // Begin function logic
    for (;;) {

        // Verify input parameters
        if (!rsa_pubkey) {
            etstatus = 1;
            break;
        }

        etstatus = GetDataProtectionKey(&data_protection_key, &key_len,
            &modulus, &modulus_len, &exponent, &exponent_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to get data protection key "
                "[Error:\"%s\"]\n", GetStatusMessage(etstatus));
            break;
        }

        bio = BIO_new_mem_buf((void *)data_protection_key, key_len);
        if (!bio) {
            etstatus = 1;
            break;
        }

        *rsa_pubkey = d2i_RSA_PUBKEY_bio(bio, NULL);

        // End function logic
        break;
    }

    if (data_protection_key) {
        FreeMemory(data_protection_key);
    }

    if (modulus) {
        FreeMemory(modulus);
    }

    if (exponent) {
        FreeMemory(exponent);
    }

    return etstatus;
}

// Initialize xmlsec library
ETSTATUS InitXmlsec() {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;

    // Begin function logic
    for (;;) {

        LIBXML_TEST_VERSION
            xmlLoadExtDtdDefaultValue = XML_DETECT_IDS | XML_COMPLETE_ATTRS;
        xmlSubstituteEntitiesDefault(1);

        // Init xmlsec library
        if (xmlSecInit() < 0) {
            etstatus = 1;
            break;
        }

        // Init crypto library
        if (xmlSecCryptoAppInit(NULL) < 0) {
            etstatus = 1;
            break;
        }

        // Init xmlsec-crypto library
        if (xmlSecCryptoInit() < 0) {
            etstatus = 1;
        }

        // End funtion logic
        break;
    }

    return etstatus;
}

// Cleanup xmlsec library
void CleanupXmlsec() {

    // Shutdown xmlsec-crypto library
    xmlSecCryptoShutdown();

    // Shutdown crypto library
    xmlSecCryptoAppShutdown();

    // Shutdown xmlsec library
    xmlSecShutdown();

}

// Verify digital signature
ETSTATUS VerifyDigitalSignature(const char *xml_str) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    xmlNodePtr node = NULL;
    xmlNodePtr cert_node = NULL;
    xmlSecDSigCtxPtr digital_signature_ctxt = NULL;
    xmlDocPtr doc = NULL;
    xmlChar *data = NULL;
    unsigned int len = 0;
    char *certificate_data = NULL;
    unsigned int certificate_len = 0;
    char *begin_cert = "-----BEGIN CERTIFICATE-----\r\n";
    char *end_cert = "\r\n-----END CERTIFICATE-----\r\n";

    // Begin Function logic
    for (;;) {

        // Verify input parameters
        if (!xml_str) {
            etstatus = 1;
            break;
        }

        InitXmlsec();

        doc = xmlParseDoc((const xmlChar*)xml_str);
        if (!doc) {
            etstatus = 1;
            break;
        }

        node = xmlSecFindNode(xmlDocGetRootElement(doc),
            xmlSecNodeSignature, xmlSecDSigNs);
        if (!node) {
            etstatus = 1;
            break;
        }

        cert_node = xmlSecFindNode(xmlDocGetRootElement(doc),
            xmlSecNodeX509Certificate, xmlSecDSigNs);
        if (!cert_node) {
            etstatus = 1;
            break;
        }

        data = xmlNodeGetContent(cert_node);
        if (!data) {
            etstatus = 1;
            break;
        }

        len = (unsigned int)strlen(data);

        certificate_len = (unsigned int)(len + strlen(begin_cert) +
            strlen(end_cert) + 2);

        certificate_data = (char *)malloc(certificate_len);
        if (!certificate_data) {
            etstatus = 1;
            break;
        }

        memset(certificate_data, 0, certificate_len);
        memcpy(certificate_data, begin_cert, strlen(begin_cert));
        memcpy(certificate_data + strlen(begin_cert), data, len);
        memcpy(certificate_data + strlen(begin_cert) + len, end_cert,
            strlen(end_cert));

        // Create signature context
        digital_signature_ctxt = xmlSecDSigCtxCreate(NULL);
        if (!digital_signature_ctxt) {
            etstatus = 1;
            break;
        }

        // Load public key
        digital_signature_ctxt->signKey = xmlSecCryptoAppKeyLoadMemory(
            certificate_data, certificate_len, xmlSecKeyDataFormatCertPem,
            NULL, NULL, NULL);
        if (digital_signature_ctxt->signKey == NULL) {
            etstatus = 1;
            break;
        }

        digital_signature_ctxt->flags =
            XMLSEC_DSIG_FLAGS_STORE_SIGNEDINFO_REFERENCES |
            XMLSEC_DSIG_FLAGS_STORE_MANIFEST_REFERENCES |
            XMLSEC_DSIG_FLAGS_STORE_SIGNATURE;

        // Verify signature
        if (xmlSecDSigCtxVerify(digital_signature_ctxt, node) < 0) {
            etstatus = 1;
            break;
        }

        if (digital_signature_ctxt->status != xmlSecDSigStatusSucceeded) {
            etstatus = 1;
        }

        // End function logic
        break;
    }

    CleanupXmlsec();

    if (digital_signature_ctxt) {
        xmlSecDSigCtxDestroy(digital_signature_ctxt);
    }

    if (doc) {
        xmlFreeDoc(doc);
    }

    if (certificate_data) {
        free(certificate_data);
    }

    return etstatus;
}

// Retrieve Response Data
ETSTATUS RetrieveResponseData(const char *xml_str, char *xml_path,
    unsigned char **data, unsigned int *data_len) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    LPXML_OBJ_HANDLE xml_doc = NULL;

    // Begin Function logic
    for (;;) {

        // Verify input parameters
        if (!xml_str || !xml_path || !data || !data_len) {
            etstatus = 1;
            break;
        }

        etstatus = LoadXML((char*)xml_str, (void**)&xml_doc);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        etstatus = GetKeyValue(xml_doc, xml_path,
            (unsigned int)strlen(xml_path), data, data_len);

        // End function logic
        break;
    }

    if (ETSTATUS_SUCCESS != etstatus) {
        if(NULL != data)
        free(*data);
    }

    UnLoadXML(xml_doc);

    return etstatus;
}

// Encrypt Data
ETSTATUS EncryptData(unsigned char *data, unsigned int data_len,
    unsigned char **enc_data, unsigned int *enc_data_len) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    RSA *rsa_key = NULL;
    unsigned int key_len = 0;
    int len = 0;

    // Begin function logic
    for (;;) {

        // Verify input parameters
        if (!data || !data_len || !enc_data || !enc_data_len) {
            etstatus = 1;
            break;
        }

        etstatus = GetPublicKey(&rsa_key);
        if (0 != etstatus) {
            break;
        }

        key_len = RSA_size(rsa_key);

        *enc_data = malloc(key_len);
        if (!*enc_data) {
            etstatus = 1;
            break;
        }

        memset(*enc_data, 0, key_len);

        *enc_data_len = key_len;

        RSA_public_encrypt(data_len, data, *enc_data,
            rsa_key, RSA_PKCS1_PADDING);

        // End function logic
        break;
    }

    return etstatus;
}

// Encrypt application data
unsigned int EncryptAppData(char *request_handle,
    unsigned int request_handle_len, char *data, unsigned int data_len,
    char **enc_data, unsigned int *enc_data_len) {

    unsigned int return_value = 0;
    unsigned char *dec_request_handle = NULL;
    unsigned int dec_request_handle_len = 0;
    unsigned char *wrapped_data = NULL;
    unsigned int wrapped_data_len = 0;
    unsigned int len = 0;
    unsigned char *encrypted_data = NULL;
    unsigned int encrypted_data_len = 0;

    // Begin function logic
    for (;;) {

        // Verify input parameters
        if (!request_handle || !request_handle_len ||
            !data || !data_len || !enc_data || !enc_data_len) {
            return_value = 1;
            break;
        }

        *enc_data = NULL;
        *enc_data_len = 0;

        return_value = Base64Decode(request_handle, request_handle_len,
            &dec_request_handle, &dec_request_handle_len);
        if (0 != return_value) {
            break;
        }

        len = dec_request_handle_len + data_len;

        wrapped_data = malloc(len + 1);
        if (!wrapped_data) {
            return_value = 1;
            break;
        }

        memset(wrapped_data, 0, len + 1);

        memcpy(wrapped_data, dec_request_handle, dec_request_handle_len);
        memcpy(wrapped_data + dec_request_handle_len, data, data_len);

        return_value = EncryptData(wrapped_data, len, &encrypted_data,
            &encrypted_data_len);
        if (0 != return_value) {
            break;
        }

        return_value = Base64Encode(encrypted_data, encrypted_data_len,
            enc_data, enc_data_len);

        // End function logic
        break;
    }

    free(dec_request_handle);
    free(encrypted_data);
    free(wrapped_data);

    return return_value;
}

// Write data to file
ETSTATUS WriteToFile(const char *file_path, void *data,
    unsigned int data_len) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    FILE *config_file = NULL;
    unsigned int len_written = 0;

    // Begin function logic
    for (;;) {

        // Verify input parameters
        if (!file_path || !file_path[0] || !data) {
            etstatus = 1;
            break;
        }

        // Open the JSON config file
        config_file = fopen(file_path, "wb");
        if (!config_file) {
            break;
        }

        // Write the schema into file
        len_written = (unsigned int)fwrite(data, 1,
            data_len, config_file);

        // End funtion logic
        break;
    }

    if (config_file) {
        fclose(config_file);
    }

    return etstatus;
}

// Function to read a line from console
char *ReadLine(FILE *file)
{
    size_t buffsize = MAX_SIZE;
    size_t this_char = 0;

    if (file == NULL) {
        return NULL;
    }

    char *line = malloc(buffsize);

    if (line == NULL) {
        return NULL;
    }

    int c;

    do {
        c = fgetc(file);

        if (this_char + 1 >= buffsize) {
            buffsize = 2 * buffsize;

            char *next_linebuff;
            next_linebuff = realloc(line, buffsize);
            if (next_linebuff == NULL) {
                free(line);
                line = NULL;
                break;
            }

            line = next_linebuff;
        }

        if (c == EOF || c == '\n') {
            c = 0;
        }

        line[this_char++] = c;
    } while (c);

    return line;
}

// Select NFC authentication type and retrieve data
ETSTATUS GetNFCAuthData(char *card_number, unsigned int cn_max,
    char *date_of_birth, unsigned int dob_max,
    char *expiry_date, unsigned int exp_date_max, char *mrz,
    unsigned int mrz_max) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    int choice = 0;
    int i = 0;
    char *cn = NULL;
    char *dob = NULL;
    char *exp_date = NULL;
    char *mrz_data = NULL;

    // Begin function logic
    for (;;) {

        // Verify input parameters
        if (!card_number || !date_of_birth || !expiry_date) {
            etstatus = 1;
            break;
        }

        while (1) {
            printf("Select NFC authentication type\n");
            for (i = 1; i < _nfc_auth_count; i++) {
                printf("\t%d. %s\n", i, _nfc_auth[i].description);
            }

            printf("\t0. Quit\n");
            printf("Please enter your choice: ");
            scanf("%d", &choice);

            // Verify the user choice
            if ((choice < 0) || (choice >= _nfc_auth_count)) {
                printf("Please enter a valid choice\n");
                continue;
            }
            break;
        }

        if (1 == choice) {
            printf("Please enter NFC authentication parameters :\n");

            printf("Please enter card number :\n");
            while (getchar() != '\n');
            cn = ReadLine(stdin);
            if (!cn) {
                printf("Failed to read card number\n");
                break;
            }
            strncpy(card_number, cn, cn_max - 1);

            printf("Please enter date of birth:\n");
            dob = ReadLine(stdin);
            if (!dob) {
                printf("Failed to read date of birth\n");
                break;
            }
            strncpy(date_of_birth, dob, dob_max - 1);

            printf("Please enter expiry date:\n");
            exp_date = ReadLine(stdin);
            if (!exp_date) {
                printf("Failed to read expiry date\n");
                break;
            }
            strncpy(expiry_date, exp_date, exp_date_max - 1);
        }
        else if (2 == choice) {
            printf("Please enter MRZ data:\n");
            while (getchar() != '\n');
            mrz_data = ReadLine(stdin);
            if (!mrz_data) {
                printf("Failed to read MRZ data\n");
                break;
            }
            strncpy(mrz, mrz_data, mrz_max - 1);
        }
        else if (0 == choice) {
            etstatus = 1;
            break;
        }
        else {
            printf("Invalid choice \n");
            etstatus = 1;
        }

        // End funtion logic
        break;
    }

    if (cn) {
        free(cn);
    }

    if (exp_date) {
        free(exp_date);
    }

    if (dob) {
        free(dob);
    }

    if (mrz_data) {
        free(mrz_data);
    }

    return etstatus;
}

// Function to safely extract the attribute values from a JSON object
ETSTATUS GetJsonAttribute(json_object *obj, char *attr_name,
    json_type attr_type, BOOL str_allocate, void **attr_value, int *value,
    unsigned int *attr_len) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    json_object *attr_object = NULL;
    unsigned int length = 0;

    // Begin function logic
    for (;;) {

        // Verify input parameters
        if (!obj || !attr_name || !attr_name[0]) {
            etstatus = 1;
            break;
        }

        if (!attr_value && !value) {
            etstatus = 1;
            break;
        }

        // Get the attribute object
        if (!json_object_object_get_ex(obj, attr_name, &attr_object)) {
            etstatus = 1;
            break;
        }

        // Verify the attribute type
        if (json_object_get_type(attr_object) != attr_type) {
            etstatus = 1;
            break;
        }

        switch (attr_type) {
        case json_type_int:
        case json_type_boolean:

            *value = json_object_get_int(attr_object);
            break;

        case json_type_string:

            length = json_object_get_string_len(attr_object);
            if (str_allocate) {

                *attr_value = malloc(length + 1);
                if (!*attr_value) {
                    etstatus = 1;
                    break;
                }
                memset(*attr_value, 0, length + 1);

                memcpy(*attr_value, json_object_get_string(attr_object),
                    length);

            }
            else {
                *attr_value = (void*)json_object_get_string(attr_object);
            }

            if (attr_len) {
                *attr_len = length;
            }

            break;

        case json_type_array:
        case json_type_object:
            *attr_value = attr_object;
            break;

        default:
            etstatus = 1;
        }

        // End function logic
        break;
    }

    return etstatus;
}

// Get signing context attributes
ETSTATUS GetSigningAttributes(json_object *json_req, char **input_path,
    char **output_path, SIGNING_CONTEXT *signing_context,
    DOCUMENT_TYPE doc_type) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    unsigned int len = 0;
    int request_data_len = 0;
    int int_attr_value = 0;
    char *tsa_url = NULL;
    char *ocsp_url = NULL;
    char *cert_path = NULL;
    char *data = NULL;

    // Begin function logic
    for (;;) {

        // verify input/output parameters
        if (!json_req || !signing_context || !input_path ||
            !output_path) {
            etstatus = 1;
            break;
        }

        etstatus = GetJsonAttribute(json_req, "input_path", 
            json_type_string, TRUE, input_path, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        etstatus = GetJsonAttribute(json_req, "signature_level",
            json_type_int, FALSE, NULL, &int_attr_value, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }
        signing_context->signature_level = int_attr_value;

        etstatus = GetJsonAttribute(json_req, "packaging_mode",
            json_type_int, FALSE, NULL, &int_attr_value, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }
        signing_context->packaging_mode = int_attr_value;

        if ((signing_context->packaging_mode != DEATACHED) ||
            (PADES == doc_type)) {
                etstatus = GetJsonAttribute(json_req, "output_path",
                    json_type_string, TRUE, output_path, NULL, &len);
                if (ETSTATUS_SUCCESS != etstatus) {
                    break;
                }
        }

        etstatus = GetJsonAttribute(json_req, "tsa_url",
            json_type_string, TRUE, &tsa_url, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(signing_context->tsa_url, tsa_url,
            sizeof(signing_context->tsa_url) - 1);

        etstatus = GetJsonAttribute(json_req, "ocsp_url",
            json_type_string, TRUE, &ocsp_url, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(signing_context->ocsp_url, ocsp_url,
            sizeof(signing_context->ocsp_url) - 1);

        etstatus = GetJsonAttribute(json_req, "cert_path",
            json_type_string, TRUE, &cert_path, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(signing_context->cert_path, cert_path,
            sizeof(signing_context->cert_path) - 1);

        etstatus = GetJsonAttribute(json_req, "country_code",
            json_type_string, FALSE, (void**)&data, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(signing_context->location.country_code, data,
            sizeof(signing_context->location.country_code) - 1);

        etstatus = GetJsonAttribute(json_req, "locality",
            json_type_string, FALSE, (void**)&data,
            NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(signing_context->location.locality, data,
            sizeof(signing_context->location.locality) - 1);

        etstatus = GetJsonAttribute(json_req, "postal_code",
            json_type_string, FALSE, (void**)&data,
            NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(signing_context->location.postal_code, data,
            sizeof(signing_context->location.postal_code) - 1);

        etstatus = GetJsonAttribute(json_req, "state_or_province",
            json_type_string, FALSE, (void**)&data,
            NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(signing_context->location.state_or_province, data,
            sizeof(signing_context->location.state_or_province) - 1);

        etstatus = GetJsonAttribute(json_req, "street",
            json_type_string, FALSE, (void**)&data,
            NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(signing_context->location.street, data,
            sizeof(signing_context->location.street) - 1);

        // End function logic
        break;
    }

    free(tsa_url);
    free(ocsp_url);
    free(cert_path);

    return etstatus;
}

// Get verification context attributes
ETSTATUS GetVerifyCtxtAttributes(json_object *json_req,
    char **input_path, char **output_path,
    VERIFICATION_CONTEXT *verification_ctxt) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    unsigned int len = 0;
    int request_data_len = 0;
    int int_attr_value = 0;
    char *ocsp_url = NULL;
    char *cert_path = NULL;
    char *data = NULL;

    // Begin function logic
    for (;;) {

        // verify input/output parameters
        if (!json_req || !input_path || !output_path ||
            !verification_ctxt) {
            etstatus = 1;
            break;
        }

        etstatus = GetJsonAttribute(json_req, "input_path",
            json_type_string, TRUE, input_path, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        etstatus = GetJsonAttribute(json_req, "output_path",
            json_type_string, TRUE, output_path, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        etstatus = GetJsonAttribute(json_req, "ocsp_url",
            json_type_string, TRUE, &ocsp_url, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(verification_ctxt->ocsp_url, ocsp_url,
            sizeof(verification_ctxt->ocsp_url) - 1);

        etstatus = GetJsonAttribute(json_req, "cert_path",
            json_type_string, TRUE, &cert_path, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(verification_ctxt->cert_path, cert_path,
            sizeof(verification_ctxt->cert_path) - 1);

        etstatus = GetJsonAttribute(json_req, "deattached",
            json_type_int, FALSE, NULL, &int_attr_value, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }
        verification_ctxt->is_deattached = int_attr_value;

        etstatus = GetJsonAttribute(json_req, "report_type",
            json_type_int, FALSE, NULL, &int_attr_value, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }
        verification_ctxt->report_type = int_attr_value;

        // End function logic
        break;
    }

    free(ocsp_url);
    free(cert_path);

    return etstatus;
}

// Get PADES signing params
ETSTATUS GetPADESSigningAttributes(json_object *json_req, 
    PADES_SIGN_PARAMS *pades_params) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    unsigned int len = 0;
    int request_data_len = 0;
    int int_attr_value = 0;
    char *data = NULL;
    char *image = NULL;

    // Begin function logic
    for (;;) {

        // verify input/output parameters
        if (!json_req ||!pades_params) {
            etstatus = 1;
            break;
        }

        etstatus = GetJsonAttribute(json_req, "sign_reason",
            json_type_string, FALSE, (void**)&data, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(pades_params->sign_reason, data,
            sizeof(pades_params->sign_reason) - 1);

        etstatus = GetJsonAttribute(json_req, "signer_location",
            json_type_string, FALSE, (void**)&data, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(pades_params->signer_location, data,
            sizeof(pades_params->signer_location) - 1);

        etstatus = GetJsonAttribute(json_req, "signer_contact_info",
            json_type_string, FALSE, (void**)&data, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(pades_params->signer_contact_info, data,
            sizeof(pades_params->signer_contact_info) - 1);

        etstatus = GetJsonAttribute(json_req, "signature_xaxis",
            json_type_int, FALSE, NULL, &int_attr_value, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        pades_params->signature_xaxis = int_attr_value;

        etstatus = GetJsonAttribute(json_req, "signature_yaxis",
            json_type_int, FALSE, NULL, &int_attr_value, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        pades_params->signature_yaxis = int_attr_value;

        etstatus = GetJsonAttribute(json_req, "signature_image",
            json_type_string, TRUE, &image, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(pades_params->signature_image, image,
            sizeof(pades_params->signature_image) - 1);

        etstatus = GetJsonAttribute(json_req, "background_color",
            json_type_string, FALSE, (void**)&data, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(pades_params->background_color, data,
            sizeof(pades_params->background_color) - 1);

        etstatus = GetJsonAttribute(json_req, "font_color",
            json_type_string, FALSE, (void**)&data, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(pades_params->font_color, data,
            sizeof(pades_params->font_color) - 1);

        etstatus = GetJsonAttribute(json_req, "font_name",
            json_type_string, FALSE, (void**)&data, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        strncpy(pades_params->font_name, data,
            sizeof(pades_params->font_name) - 1);

        etstatus = GetJsonAttribute(json_req, "font_size",
            json_type_int, FALSE, NULL, &int_attr_value, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }

        pades_params->font_size = int_attr_value;

        etstatus = GetJsonAttribute(json_req, "signature_text",
            json_type_string, FALSE, (void**)&data, NULL, &len);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }
        strncpy(pades_params->signature_text, data,
            sizeof(pades_params->signature_text) - 1);

        etstatus = GetJsonAttribute(json_req, "sign_visible",
            json_type_int, FALSE, NULL, &int_attr_value, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }
        pades_params->sign_visible = int_attr_value;

        etstatus = GetJsonAttribute(json_req, "name_position",
            json_type_int, FALSE, NULL, &int_attr_value, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }
        pades_params->name_position = int_attr_value;

        etstatus = GetJsonAttribute(json_req, "page_number",
            json_type_int, FALSE, NULL, &int_attr_value, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            break;
        }
        pades_params->page_number = int_attr_value;

        // End function logic
        break;
    }

    free(image);

    return etstatus;
}

// Validate a UTF8 string
BOOL IsValidString(const char *str, unsigned int len) {

    int c, i, n, j;

    // Verify input parameters
    if (!str || !str[0] || !len) {
        return FALSE;
    }

    // Validate the string encoding
    for (i = 0; i < (int)len; i++) {

        c = (unsigned char)str[i];
        if (((0x20 <= c) && (c <= 0x7E)) || (c == '\r') ||
            (c == '\n') || (c == '\t')) {
            n = 0; // 0bbbbbbb
        }
        else if ((c & 0xE0) == 0xC0) {
            n = 1; // 110bbbbb
        }
        else if (c == 0xED && i < ((int)len - 1) &&
            ((unsigned char)str[i + 1] & 0xa0) == 0xa0) {
            return FALSE; //U+d800 to U+dfff
        }
        else if ((c & 0xF0) == 0xE0) {
            n = 2; // 1110bbbb
        }
        else if ((c & 0xF8) == 0xF0) {
            n = 3; // 11110bbb
        }
        else {
            return FALSE;
        }

        // n bytes matching 10bbbbbb follow
        for (j = 0; j < n && i < (int)len; j++) {
            if ((++i == (int)len) ||
                (((unsigned char)str[i] & 0xC0) != 0x80)) {
                return FALSE;
            }
        }
    }

    return TRUE;
}

BOOL FingerDetails(int finger_id, int finger_index) {

    switch (finger_index) {
    case 5:
        printf("%d -- Right Thumb finger\n", finger_id);
        return TRUE;
        break;

    case 9:
        printf("%d -- Right Index finger\n", finger_id);
        return TRUE;
        break;

    case 13:
        printf("%d -- Right Middle finger\n", finger_id);
        return TRUE;
        break;

    case 17:
        printf("%d -- Right Ring finger\n", finger_id);
        return TRUE;
        break;

    case 15:
        printf("%d -- Right Little finge\n", finger_id);
        return TRUE;
        break;

    case 6:
        printf("%d -- Left Thumb finger\n", finger_id);
        return TRUE;
        break;

    case 10:
        printf("%d -- Left Index finger\n", finger_id);
        return TRUE;
        break;

    case 14:
        printf("%d -- Left Middle finger\n", finger_id);
        return TRUE;
        break;

    case 18:
        printf("%d -- Left Ring finger\n", finger_id);
        return TRUE;
        break;

    case 22:
        printf("%d -- Left Little finger\n", finger_id);
        return TRUE;
        break;

    case 3:
    default:
        printf("Finger Index is Not Valid\n");
        return FALSE;
        break;
    }
    return FALSE;
}

// Get finger index details
BOOL GetFingerIndexDetails(int first_finger_id, int first_finger_index,
    int second_finger_id, int second_finger_index, int *finger_index,
    int *finger_id) {

    int choice = 0;

    // verify input parameters
    if (!first_finger_id || !first_finger_index || !second_finger_id ||
        !second_finger_index || !finger_index || !finger_id) {
        return FALSE;
    }

    if (!FingerDetails(first_finger_id, first_finger_index)) {
        return FALSE;
    }

    if (!FingerDetails(second_finger_id, second_finger_index)) {
        return FALSE;
    }

    printf("These two finger print templates are available in card\n");

    while (1) {
        printf("Please enter your choice: ");
        scanf("%d", &choice);

        // Verify the user choice
        if ((choice != first_finger_id) && (choice != second_finger_id)) {
            printf("Please enter a valid choice\n");
            continue;
        }

        break;
    }

    if (first_finger_id == choice) {
        *finger_index = first_finger_index;
        *finger_id = first_finger_id;
    }
    else if (second_finger_id == choice) {
        *finger_index = second_finger_index;
        *finger_id = second_finger_id;
    }

    return TRUE;
}

// Verify toolkit response
static unsigned int VerifyToolkitResponse(char *response, char *request_id) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    char *toolkit_request_id = NULL;
    unsigned int toolkit_request_id_len = 0;

    // Begin function logic
    for (;;) {

        // Verify input parameters
        if (!response || !request_id) {
            return 1;
        }

        etstatus = VerifyDigitalSignature(response);
        if (0 != etstatus) {
            printf("Failed to verify response signature\n");
            break;
        }

        etstatus = RetrieveResponseData(response,
            "ValidationGatewayResponse\\Message\\Header\\RequestID",
            &toolkit_request_id, &toolkit_request_id_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to retrieve request id from toolkit response\n");
            break;
        }

        if (strcmp(toolkit_request_id, request_id)) {
            print_error(toolkit_request_id, request_id);
            break;
        }

        // End function logic
        break;
    }

    free(toolkit_request_id);

    return etstatus;
}

// Preparing request
static unsigned int PrepareVGRequest(unsigned int connection_handle,
    char **enc_request_id, unsigned int *enc_request_id_len,
    char **request_handle, unsigned int *request_handle_len) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    unsigned char request_id[40] = { 0 };

    // Begin function logic
    for (;;) {

        // Verify input parameters
        if (!enc_request_id || !enc_request_id_len ||
            !request_handle || !request_handle_len) {
            etstatus = 1;
            break;
        }

        *enc_request_id = NULL;
        *enc_request_id_len = 0;
        *request_handle = NULL;
        *request_handle_len = 0;

        if (1 != RAND_bytes(request_id, 40)) {
            printf("Failed to generate request id\n");
            etstatus = 1;
            break;
        }

        etstatus = Base64Encode(request_id, sizeof(request_id),
            enc_request_id, enc_request_id_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to generate request id\n");
            break;
        }

        etstatus = PrepareRequest(connection_handle, *enc_request_id,
            request_handle, request_handle_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to prepare request"
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        // End function logic
        break;
    }

    return etstatus;
}

// Connect to card
static BOOL ConnectEIDACard(unsigned int *connection_handle) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    char reader_name[256] = { 0 };
    unsigned int length = sizeof(reader_name);

    // Begin function logic
    for (;;) {

        // Get the reader name with Emirates ID Card
        etstatus = GetReaderWithEmiratesID(reader_name, &length);
        if (ETSTATUS_SUCCESS != etstatus) {
            result = FALSE;
            printf("Could not get the reader name "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }
        
        // Connect to the Emirates ID Card
        etstatus = Connect(reader_name, connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            result = FALSE;
            printf("Could not connect to Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }

        // End function logic
        break;
    }

    return result;
}

// EIDAToolkit Service handler to read public data
BOOL ReadPublicDataHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    unsigned int connection_handle = 0;
    char reader_name[256] = { 0 };
    unsigned int length = sizeof(reader_name);
    char *response = NULL;
    unsigned char request_id[40] = { 0 };
    char *enc_request_id = NULL;
    unsigned int enc_request_id_len = 0;

    // Begin function logic
    for (;;) {

        if (1 != RAND_bytes(request_id, 40)) {
            printf("Failed to generate request id\n");
            break;
        }

        etstatus = Base64Encode(request_id, sizeof(request_id),
            &enc_request_id, &enc_request_id_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to encode request id\n");
            break;
        }

        // Connect to card
        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        // Read the Emirates ID Card public data
        etstatus = ReadPublicData(connection_handle, enc_request_id, TRUE,
            TRUE, TRUE, TRUE, TRUE, &response, &length);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could not read Emirates ID Card public data "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("\n Toolkit Response : %s\n", response);

        // End function logic
        break;
    }

    // Free public data
    if (response) {
        FreeMemory(response);
    }

    // Disconnect the Emirates ID Card
    if (connection_handle) {
        etstatus = Disconnect(connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could not disconnect Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }
    }

    free(enc_request_id);

    return result;
}

// EIDAToolkit Service handler to read public data nfc
BOOL ReadPublicDataNfcHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    unsigned int connection_handle = 0;
    char reader_name[256] = { 0 };
    unsigned int length = sizeof(reader_name);
    char *response = NULL;
    unsigned char request_id[40] = { 0 };
    char *enc_request_id = NULL;
    unsigned int enc_request_id_len = 0;
    char card_number[MAX_SIZE] = { 0 };
    char date_of_birth[MAX_SIZE] = { 0 };
    char expiry_date[MAX_SIZE] = { 0 };
    char mrz[MAX_SIZE] = { 0 };
    char *mrz_data = NULL;
    int i = 0;
    int choice = 0;

    // Begin function logic
    for (;;) {

        if (1 != RAND_bytes(request_id, 40)) {
            printf("Failed to generate request id\n");
            break;
        }

        etstatus = Base64Encode(request_id, sizeof(request_id),
            &enc_request_id, &enc_request_id_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to encode request ID\n");
            break;
        }

        // Connect to card
        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        etstatus = GetNFCAuthData(card_number, sizeof(card_number),
            date_of_birth, sizeof(date_of_birth), expiry_date,
            sizeof(expiry_date), mrz, sizeof(mrz));
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to retrieve NFC authentication data\n");
            break;
        }

        if (mrz && mrz[0]) {
            mrz_data = mrz;
        }

        etstatus = SetNfcAuthenticationParams(connection_handle, card_number,
            date_of_birth, expiry_date, mrz_data);
        if (etstatus != ETSTATUS_SUCCESS) {
            printf("Failed to set nfc authentication parameters: %s\n",
                GetStatusMessage(etstatus));
            break;
        }

        // Read the Emirates ID Card public data
        etstatus = ReadPublicData(connection_handle, enc_request_id, TRUE,
            TRUE, TRUE, TRUE, TRUE, &response, &length);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could not read Emirates ID Card public data "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("\n Toolkit Response : %s\n", response);

        // End function logic
        break;
    }

    // Free public data
    if (response) {
        FreeMemory(response);
    }

    // Disconnect the Emirates ID Card
    if (connection_handle) {
        etstatus = Disconnect(connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could not disconnect Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }
    }

    return result;
}

// EIDAToolkit Service handler to check Emirates ID card status
BOOL CheckCardStatusHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    int connection_handle = 0;
    int card_status = 0;
    unsigned char request_id[40] = { 0 };
    char *enc_request_id = NULL;
    unsigned int enc_request_id_len = 0;
    char *response = NULL;
    unsigned int response_len = 0;
    unsigned int request_id_len = 0;

    // Begin function logic
    for (;;) {

        // Connect to card
        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        if (1 != RAND_bytes(request_id, 40)) {
            printf("Failed to generate request id\n");
            break;
        }

        etstatus = Base64Encode(request_id, sizeof(request_id),
            &enc_request_id, &enc_request_id_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to encode request ID\n");
            break;
        }

        // Get the Emirates ID card check card status with VG
        etstatus = CheckCardStatus(connection_handle, enc_request_id,
            &response, &response_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to check the Emirates ID Card status "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        etstatus = VerifyToolkitResponse(response, enc_request_id);
        if (0 != etstatus) {
            printf("Failed to verify toolkit response\n");
            break;
        }

        printf("\nToolkit Response : %s\n", response);

        // End function logic
        break;
    }

    // Disconnect the Emirates ID Card
    if (connection_handle) {
        etstatus = Disconnect(connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could not disconnect Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }
    }

    if (response) {
        FreeMemory(response);
    }

    free(enc_request_id);

    return TRUE;
}

// EIDAToolkit Service handler to Export PKI certificates
BOOL ExportPKICertificatesHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    int connection_handle = 0;
    char pin[16] = { 0 };
    unsigned int pin_len = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *response = NULL;
    unsigned int response_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    char enc_pin[16] = { 0 };
    unsigned int enc_pin_len = 0;
    char *pin_data = NULL;
    unsigned int pin_data_len = 0;
    int attempts_left = 0;
    unsigned char request_id_applet[40] = { 0 };
    char *enc_request_id = NULL;
    unsigned int enc_request_id_len = 0;

    // Begin function logic
    for (;;) {

        // Connect to card
        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        etstatus = get_pin(pin, &pin_len);
        if (0 != etstatus) {
            printf("Failed to get pin \n");
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(request_handle, request_handle_len,
                pin, pin_len, &pin_data, &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to encrypt application data \n");
                break;
            }
        }

        // Get the Emirates ID card PKI certificates
        etstatus = GetPKICertificates(!toolkit_options->sm_mode ?
            pin_data : pin, !toolkit_options->sm_mode ?
            pin_data_len : pin_len, &attempts_left, &response,
            &response_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to export PKI certificates "
                "[Error:%u, attempts_left:%d, \"%s\"]\n", etstatus,
                attempts_left, GetStatusMessage(etstatus));
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = VerifyToolkitResponse(response, request_id);
            if (0 != etstatus) {
                printf("Failed to verify toolkit response\n");
                break;
            }
        }

        printf("\nToolkit Response : %s\n", response);

        // End functio logic
        break;
    }

    // Disconnect the Emirates ID Card
    if (connection_handle) {
        etstatus = Disconnect(connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could disconnect Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }
    }

    if (response) {
        FreeMemory(response);
    }

    if (request_handle) {
        FreeMemory(request_handle);
    }

    free(enc_request_id);
    free(request_id);
    free(pin_data);

    return TRUE;
}

// EIDAToolkit Service handler for Authenticate Biometric On Server
BOOL AuthenticateBiometricOnServerHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    int connection_handle = 0;
    char first_finger = 0;
    char first_finger_index = 0;
    char second_finger = 0;
    char second_finger_index = 0;
    BOOL auth_status = 0;
    int finger_index = 0;
    int finger_id = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    unsigned char request_id[40] = { 0 };
    char *enc_request_id = NULL;
    unsigned int enc_request_id_len = 0;
    char *response = NULL;
    unsigned int response_len = 0;

    // Begin function logic
    for (;;) {

        if (1 != RAND_bytes(request_id, 40)) {
            printf("Failed to generate request id\n");
            break;
        }

        etstatus = Base64Encode(request_id, sizeof(request_id),
            &enc_request_id, &enc_request_id_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to encode request ID\n");
            break;
        }

        // Connect to card
        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        // Get the figer index
        etstatus = GetFingerIndex(connection_handle, &first_finger,
            &first_finger_index, &second_finger, &second_finger_index);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could not retrieve finger index from card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        if (!GetFingerIndexDetails(first_finger, first_finger_index,
            second_finger, second_finger_index, &finger_index, &finger_id)) {
            printf("Failed to retrieve finger index details\n");
            break;
        }

        // Perform biometric authentication
        etstatus = AuthenticateBiometricOnServer(connection_handle,
            enc_request_id, 0, 30, &response, &response_len);
        if (ETSTATUS_SUCCESS != etstatus) {

            printf("Biometric (fingerprint) authentication failed"
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        etstatus = VerifyToolkitResponse(response, enc_request_id);
        if (0 != etstatus) {
            printf("Failed to verify toolkit response\n");
            break;
        }

        printf("\nToolkit Response : %s\n", response);

        // End function logic
        break;
    }

    // Disconnect the Emirates ID Card
    if (connection_handle) {
        etstatus = Disconnect(connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could disconnect Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }
    }

    if (response) {
        FreeMemory(response);
    }

    free(enc_request_id);

    return TRUE;
}

// EIDAToolkit Service handler for Reset PIN
BOOL ResetPinHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    int connection_handle = 0;
    char first_finger = 0;
    char first_finger_index = 0;
    char second_finger = 0;
    char second_finger_index = 0;
    BOOL auth_status = 0;
    char pin[16] = { 0 };
    unsigned int pin_len = 0;
    int finger_index = 0;
    int finger_id = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    char enc_pin[16] = { 0 };
    unsigned int enc_pin_len = 0;
    char *pin_data = NULL;
    unsigned int pin_data_len = 0;
    int attempts_left = 0;

    // Begin function logic
    for (;;) {

        // Connect to card
        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        printf("\nPlease provide new PIN to reset: ");

        etstatus = get_pin(pin, &pin_len);
        if (0 != etstatus) {
            printf("Failed to get pin \n");
            break;
        }

        // Get the figer index
        etstatus = GetFingerIndex(connection_handle, &first_finger,
            &first_finger_index, &second_finger, &second_finger_index);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could not retrieve finger index from card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        if (!GetFingerIndexDetails(first_finger, first_finger_index,
            second_finger, second_finger_index, &finger_index, &finger_id)) {
            printf("Failed to retrieve finger index details\n");
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(request_handle, request_handle_len,
                pin, pin_len, &pin_data, &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to encrypt application data \n");
                break;
            }
        }

        // Call reset PIN
        etstatus = ResetPIN(!toolkit_options->sm_mode ? pin_data : pin,
            !toolkit_options->sm_mode ? pin_data_len : pin_len, finger_index,
            finger_id, 30, &attempts_left);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to reset PIN "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("pin reset successful\n");

        // End function logic
        break;
    }

    // Disconnect the Emirates ID Card
    if (connection_handle) {
        etstatus = Disconnect(connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could disconnect Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }
    }

    if (request_handle) {
        FreeMemory(request_handle);
    }

    free(request_id);
    free(pin_data);

    return TRUE;
}

// EIDAToolkit Service handler for PKI authentication
BOOL AuthenticatePkiHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    int connection_handle = 0;
    char pin[16] = { 0 };
    unsigned int pin_len = 0;
    unsigned int attempts_left = 0;
    BOOL status = FALSE;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *response = NULL;
    unsigned int response_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    char enc_pin[16] = { 0 };
    unsigned int enc_pin_len = 0;
    char *pin_data = NULL;
    unsigned int pin_data_len = 0;

    // Begin function logic
    for (;;) {

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        etstatus = get_pin(pin, &pin_len);
        if (0 != etstatus) {
            printf("Failed to get pin \n");
            break;
        }

        etstatus = EncryptAppData(request_handle, request_handle_len,
            pin, pin_len, &pin_data, &pin_data_len);
        if (0 != etstatus) {
            printf("Failed to encrypt application data \n");
            break;
        }

        // Perform PKI authentication
        etstatus = AuthenticatePKI(pin_data, pin_data_len, &attempts_left,
            &response, &response_len);
        if (ETSTATUS_SUCCESS != etstatus) {

            printf("Failed to authenticate PKI "
                "[Error:%u,attempts_left:%d, \"%s\"]\n", etstatus,
                attempts_left, GetStatusMessage(etstatus));
            break;
        }

        etstatus = VerifyToolkitResponse(response, request_id);
        if (0 != etstatus) {
            printf("Failed to verify toolkit response\n");
            break;
        }

        printf("\nToolkit Response : %s\n", response);

        // End functio logic
        break;
    }

    // Disconnect the Emirates ID Card
    if (connection_handle) {
        etstatus = Disconnect(connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could disconnect Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }
    }

    if (response) {
        FreeMemory(response);
    }

    if (request_handle) {
        FreeMemory(request_handle);
    }

    free(request_id);
    free(pin_data);

    return TRUE;
}

// EIDAToolkit Service handler to check if Emirates ID card is Genuine
BOOL IsCardGenuineHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    int connection_handle = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *response = NULL;
    unsigned int response_len = 0;
    unsigned char request_id[40] = { 0 };
    char *enc_request_id = NULL;
    unsigned int enc_request_id_len = 0;
    unsigned int request_id_len = 0;
    int attempts_left = 0;

    // Begin function logic
    for (;;) {

        if (1 != RAND_bytes(request_id, 40)) {
            printf("Failed to generate request id\n");
            break;
        }

        etstatus = Base64Encode(request_id, sizeof(request_id),
            &enc_request_id, &enc_request_id_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to encode request ID\n");
            break;
        }

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        etstatus = IsCardGenuine(connection_handle, enc_request_id,
            &attempts_left, &response, &response_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could not verify if the card is genuine "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("\nToolkit Response : %s\n", response);

        // End function logic
        break;
    }

    // Disconnect the Emirates ID Card
    if (connection_handle) {
        etstatus = Disconnect(connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could disconnect Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }
    }

    if (response) {
        FreeMemory(response);
    }

    free(enc_request_id);

    return TRUE;
}

// EIDAToolkit Service handler for Unblock Pin
BOOL UnblockPinHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    int connection_handle = 0;
    char first_finger = 0;
    char first_finger_index = 0;
    char second_finger = 0;
    char second_finger_index = 0;
    char pin[16] = { 0 };
    unsigned int pin_len = 0;
    int finger_index = 0;
    int finger_id = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    char enc_pin[16] = { 0 };
    unsigned int enc_pin_len = 0;
    char *pin_data = NULL;
    unsigned int pin_data_len = 0;
    int attempts_left = 0;

    // Begin function logic
    for (;;) {

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        printf("\nPlease provide new PIN to unblock pki application: ");
        etstatus = get_pin(pin, &pin_len);
        if (0 != etstatus) {
            printf("Failed to get pin \n");
            break;
        }

        // Get the figer index
        etstatus = GetFingerIndex(connection_handle, &first_finger,
            &first_finger_index, &second_finger, &second_finger_index);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could not retrieve finger index from card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        if (!GetFingerIndexDetails(first_finger, first_finger_index,
            second_finger, second_finger_index, &finger_index, &finger_id)) {
            printf("Failed to retrieve finger index details\n");
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(request_handle, request_handle_len,
                pin, pin_len, &pin_data, &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to encrypt application data \n");
                break;
            }
        }

        // Call Unblock PIN
        etstatus = UnblockPIN(!toolkit_options->sm_mode ? pin_data : pin,
            !toolkit_options->sm_mode? pin_data_len : pin_len, finger_index,
            finger_id, 30, &attempts_left);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to unblock PIN "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("unblock pin successful\n");

        // End function logic
        break;
    }

    // Disconnect the Emirates ID Card
    if (connection_handle) {
        etstatus = Disconnect(connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could disconnect Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }
    }

    if (request_handle) {
        FreeMemory(request_handle);
    }

    free(request_id);
    free(pin_data);

    return TRUE;
}

// EIDAToolkit Service handler for reading Family Book Data
BOOL ReadFamilyBookDataHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    BOOL family_book_data_valid = FALSE;
    int connection_handle = 0;
    char pin[16] = { 0 };
    unsigned int pin_len = 0;
    int len = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *response = NULL;
    unsigned int response_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    char enc_pin[16] = { 0 };
    unsigned int enc_pin_len = 0;
    char *pin_data = NULL;
    unsigned int pin_data_len = 0;
    int attempts_left = 0;

    // Begin function logic
    for (;;) {

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        etstatus = get_pin(pin, &pin_len);
        if (0 != etstatus) {
            printf("Failed to get pin \n");
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(request_handle, request_handle_len,
                pin, pin_len, &pin_data, &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to encrypt application data \n");
                break;
            }
        }

        // Read family book data
        etstatus = ReadFamilyBookData(!toolkit_options->sm_mode ? pin_data :
            pin, !toolkit_options->sm_mode ? pin_data_len : pin_len,
            &attempts_left, &response, &response_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to read family book data "
                "[Error:%u, attempts_left:%d \"%s\"]\n", etstatus,
                attempts_left, GetStatusMessage(etstatus));
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = VerifyToolkitResponse(response, request_id);
            if (0 != etstatus) {
                printf("Failed to verify toolkit response\n");
                break;
            }
        }

        printf("\nToolkit Response : %s\n", response);

        // End function logic
        break;
    }

    // Disconnect the Emirates ID Card
    if (connection_handle) {
        etstatus = Disconnect(connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could disconnect Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }
    }

    if (response) {
        FreeMemory(response);
    }

    if (request_handle) {
        FreeMemory(request_handle);
    }

    free(request_id);
    free(pin_data);

    return TRUE;
}

// EIDAToolkit Service handler for Sign Data
BOOL SignDataHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    int connection_handle = 0;
    BOOL auth_status = FALSE;
    char pin[16] = { 0 };
    unsigned int pin_len = 0;
    unsigned int attempts_left = 0;
    BOOL status = FALSE;
    char *data = NULL;
    unsigned char *signature = NULL;
    unsigned int signature_length = 0;
    unsigned char *enc_signature = NULL;
    unsigned int enc_signature_length = 0;
    char *sign_certificate = NULL;
    unsigned int sign_cert_len = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    char *response = NULL;
    unsigned int response_len = 0;
    char enc_pin[16] = { 0 };
    unsigned int enc_pin_len = 0;
    char *pin_data = NULL;
    unsigned int pin_data_len = 0;
    char *verify_request_handle = NULL;
    unsigned int verify_request_handle_len = 0;
    char *verify_request_id = NULL;
    unsigned int verify_request_id_len = 0;
    char *enc_sign_certificate = NULL;
    unsigned int enc_sign_cert_len = 0;
    unsigned char verify_sign_request_id[40] = { 0 };

    // Begin function logic
    for (;;) {

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        // Get card version
        unsigned char card_version = 0;
        etstatus = GetCardVersion(connection_handle, &card_version);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("GetCardVersion failed due to "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        printf("\nPlease enter data to sign: \n");
        while (getchar() != '\n');
        data = ReadLine(stdin);
        if(!data){
            printf("Failed to read data to sign\n");
            break;
        }

        etstatus = get_pin(pin, &pin_len);
        if (0 != etstatus) {
            printf("Failed to get pin \n");
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(request_handle, request_handle_len,
                pin, pin_len, &pin_data, &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to encrypt application data \n");
                break;
            }
        }

        // Digitally sign the data
        etstatus = SignData(data, (unsigned int)strlen(data), FALSE,
            !toolkit_options->sm_mode ? pin_data : pin,
            !toolkit_options->sm_mode ? pin_data_len : pin_len, &attempts_left,
            &response, &response_len);
        if (etstatus != ETSTATUS_SUCCESS) {
            printf("Failed to digitally sign data "
                "[Error:%u,attempts_left:%d, \"%s\"]\n", etstatus,
                attempts_left, GetStatusMessage(etstatus));
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = VerifyToolkitResponse(response, request_id);
            if (0 != etstatus) {
                printf("Failed to verify toolkit response\n");
                break;
            }
        }

        printf("\nToolkit Response : %s\n", response);

        etstatus = RetrieveResponseData(response,
            "ValidationGatewayResponse\\Message\\Body\\Signature",
            &enc_signature, &enc_signature_length);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to retrieve signature data from toolkit response\n");
            break;
        }

        etstatus = Base64Decode(enc_signature, enc_signature_length,
            &signature, &signature_length);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to decode signature\n");
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &verify_request_id,
            &verify_request_id_len, &verify_request_handle,
            &verify_request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        FreeMemory(response);
        response = NULL;
        free(pin_data);
        pin_data = NULL;

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(verify_request_handle,
                verify_request_handle_len, pin, pin_len, &pin_data,
                &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to set service parameters\n");
                break;
            }
        }

        // Get the Emirates ID card PKI certificates
        etstatus = GetPKICertificates(!toolkit_options->sm_mode ?
            pin_data : pin, !toolkit_options->sm_mode ? pin_data_len : pin_len,
            &attempts_left, &response, &response_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to export PKI certificates "
                "[Error:%u, attempts_left:%d, \"%s\"]\n", etstatus,
                attempts_left, GetStatusMessage(etstatus));
            break;
        }

        etstatus = RetrieveResponseData(response,
            "ValidationGatewayResponse\\Message\\Body\\SignatureCertificate",
            &enc_sign_certificate, &enc_sign_cert_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to retrieve certificate from toolkit response\n");
            break;
        }

        etstatus = Base64Decode(enc_sign_certificate, enc_sign_cert_len,
            &sign_certificate, &sign_cert_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to decode signature\n");
            break;
        }

        etstatus = VerifySignature(data, (unsigned int)strlen(data), signature,
            signature_length, sign_certificate, sign_cert_len, FALSE);
        if (etstatus != ETSTATUS_SUCCESS) {
            printf("Failed to verify digital signature "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("\nDigital signature successfully verified\n");

        // End function logic
        break;
    }

    // Disconnect the Emirates ID Card
    if (connection_handle) {
        etstatus = Disconnect(connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could disconnect Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }
    }

    if (response) {
        FreeMemory(response);
    }

    if (request_handle) {
        FreeMemory(request_handle);
    }

    if (data) {
        free(data);
    }

    free(request_id);
    free(signature);
    free(sign_certificate);
    free(enc_sign_certificate);
    free(enc_signature);
    free(pin_data);

    return TRUE;
}

// EIDAToolkit Service handler for Sign Challenge
BOOL SignChallengeHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    int connection_handle = 0;
    BOOL auth_status = FALSE;
    char pin[16] = { 0 };
    unsigned int pin_len = 0;
    unsigned int attempts_left = 0;
    BOOL status = FALSE;
    char *data = NULL;
    unsigned char *signature = NULL;
    unsigned int signature_length = 0;
    unsigned char *enc_signature = NULL;
    unsigned int enc_signature_length = 0;
    char *sign_certificate = NULL;
    unsigned int sign_cert_len = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    char *response = NULL;
    unsigned int response_len = 0;
    char enc_pin[16] = { 0 };
    unsigned int enc_pin_len = 0;
    char *pin_data = NULL;
    unsigned int pin_data_len = 0;
    char *verify_request_handle = NULL;
    unsigned int verify_request_handle_len = 0;
    char *verify_request_id = NULL;
    unsigned int verify_request_id_len = 0;
    char *enc_sign_certificate = NULL;
    unsigned int enc_sign_cert_len = 0;
    unsigned char verify_sign_request_id[40] = { 0 };
    char *verify_sign_response = NULL;
    unsigned int verify_sign_response_len = 0;

    // Begin function logic
    for (;;) {

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        printf("\nPlease enter data to sign: \n");
        while (getchar() != '\n');
        data = ReadLine(stdin);
        if (!data) {
            printf("Failed to read data to sign\n");
            break;
        }

        etstatus = get_pin(pin, &pin_len);
        if (0 != etstatus) {
            printf("Failed to get pin \n");
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(request_handle, request_handle_len,
                pin, pin_len, &pin_data, &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to encrypt application data \n");
                break;
            }
        }

        // Digitally sign the data
        etstatus = SignChallenge(data, (unsigned int)strlen(data), FALSE,
            !toolkit_options->sm_mode ? pin_data : pin,
            !toolkit_options->sm_mode ? pin_data_len : pin_len, &attempts_left,
            &response, &response_len);
        if (etstatus != ETSTATUS_SUCCESS) {
            printf("Failed to digitally sign data "
                "[attempts left:%d, Error:%u, \"%s\"]\n", attempts_left,
                etstatus, GetStatusMessage(etstatus));
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = VerifyToolkitResponse(response, request_id);
            if (0 != etstatus) {
                printf("Failed to verify toolkit response\n");
                break;
            }
        }

        printf("\nToolkit Response : %s\n", response);

        etstatus = RetrieveResponseData(response,
            "ValidationGatewayResponse\\Message\\Body\\Signature",
            &enc_signature, &enc_signature_length);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to retrieve signature data from toolkit response\n");
            break;
        }

        etstatus = Base64Decode(enc_signature, enc_signature_length,
            &signature, &signature_length);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to decode signature\n");
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &verify_request_id,
            &verify_request_id_len, &verify_request_handle,
            &verify_request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        FreeMemory(response);
        response = NULL;
        free(pin_data);
        pin_data = NULL;

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(verify_request_handle,
                verify_request_handle_len, pin, pin_len, &pin_data,
                &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to encrypt application data \n");
                break;
            }
        }

        // Get the Emirates ID card PKI certificates
        etstatus = GetPKICertificates(!toolkit_options->sm_mode ?
            pin_data : pin, !toolkit_options->sm_mode ? pin_data_len : pin_len,
            &attempts_left, &response, &response_len);
        if (ETSTATUS_SUCCESS != etstatus) {

            printf("Failed to export PKI certificates "
                "[attempts left:%d, Error:%u, \"%s\"]\n", attempts_left,
                etstatus, GetStatusMessage(etstatus));
            break;
        }

        etstatus = RetrieveResponseData(response,
            "ValidationGatewayResponse\\Message\\Body\\AuthenticationCertificate",
            &enc_sign_certificate, &enc_sign_cert_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to retrieve certificate from toolkit response\n");
            break;
        }

        etstatus = Base64Decode(enc_sign_certificate, enc_sign_cert_len,
            &sign_certificate, &sign_cert_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to decode signature\n");
            break;
        }

        etstatus = VerifySignature(data, (unsigned int)strlen(data), signature,
            signature_length, sign_certificate, sign_cert_len, FALSE);
        if (etstatus != ETSTATUS_SUCCESS) {
            printf("\nToolkit Response : %s\n", verify_sign_response);
            printf("Failed to verify digital signature "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("\nDigital signature successfully verified\n");
        // End function logic
        break;
    }

    // Disconnect the Emirates ID Card
    if (connection_handle) {
        etstatus = Disconnect(connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could disconnect Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }
    }

    if (response) {
        FreeMemory(response);
    }

    if (verify_sign_response) {
        FreeMemory(verify_sign_response);
    }

    if (request_handle) {
        FreeMemory(request_handle);
    }

    if (data) {
        free(data);
    }

    free(request_id);
    free(signature);
    free(sign_certificate);
    free(enc_sign_certificate);
    free(enc_signature);
    free(pin_data);

    return TRUE;
}

// EIDAToolkit Service handler for verifying certificate using crl
BOOL VerifyCertificateUsingCRLHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    BOOL status = FALSE;
    char *response = NULL;
    unsigned int response_len = 0;
    unsigned char *crl_data = NULL;
    unsigned long crl_data_len = 0;
    unsigned char *cert_data = NULL;
    unsigned long cert_data_len = 0;
    char *cert_path = NULL;
    char *crl_path = NULL;

    // Begin function logic
    for (;;) {

        printf("Please enter certificate path :\n");
        while (getchar() != '\n');

        cert_path = ReadLine(stdin);
        if (!cert_path) {
            printf("Failed to read certificate path\n");
            break;
        }

        printf("Please enter crl path :\n");
        crl_path = ReadLine(stdin);
        if (!crl_path) {
            printf("Failed to read crl path\n");
            break;
        }

        if(!ReadFromFile(cert_path, &cert_data, &cert_data_len)){
            printf("Failed to Read CERT_FILE\n");
            break;
        }

        etstatus = VerifyCertificateUsingCRL(crl_path, 1, cert_data,
            cert_data_len);
        if (etstatus != ETSTATUS_SUCCESS) {
            printf("Failed to verify certificate using crl "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }
        if (status) {
            printf("Certifiate verification successful\n");
        }
        else {
            printf("Certifiate verification failed\n");
        }

        printf("\nVerifying signature ... \n");

        // End function logic
        break;
    }

    if (response) {
        FreeMemory(response);
    }

    if (cert_data) {
        free(cert_data);
    }

    if (cert_path) {
        free(cert_path);
    }

    if (crl_path) {
        free(crl_path);
    }

    return TRUE;
}

// EIDAToolkit Service handler for Registering Device
BOOL RegisterDeviceHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    BOOL status = FALSE;
    char *response = NULL;
    unsigned int response_len = 0;
    int len = 0;
    char *user_id = NULL;
    char *password = NULL;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    unsigned char *dec_data = NULL;
    unsigned int dec_data_len = 0;
    unsigned int connection_handle = 0;
    char *userid = NULL;
    char *pwd = NULL;
    char *device_ref_id = NULL;

    // Begin function logic
    for (;;) {

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("Please enter user ID :\n");
        while (getchar() != '\n');
        userid = ReadLine(stdin);
        if (!userid) {
            printf("Failed to read user ID\n");
            break;
        }

        printf("Please enter password :\n");
        pwd = ReadLine(stdin);
        if (!pwd) {
            printf("Failed to read password\n");
            break;
        }

        printf("Please enter device reference ID :\n");
        device_ref_id = ReadLine(stdin);
        if (!device_ref_id) {
            printf("Failed to read device reference ID\n");
            break;
        }

        etstatus = EncryptAppData(request_handle, request_handle_len,
            userid, (unsigned int)strlen(userid), &user_id, &len);
        if (0 != etstatus) {
            printf("Failed to encrypt application data \n");
            break;
        }

        etstatus = EncryptAppData(request_handle, request_handle_len,
            pwd, (unsigned int)strlen(pwd), &password, &len);
        if (0 != etstatus) {
            printf("Failed to encrypt application data \n");
            break;
        }

        etstatus = RegisterDevice(user_id, password, device_ref_id,
            &response, &response_len);
        if (etstatus != ETSTATUS_SUCCESS) {
            printf("\nToolkit Response : %s\n", response);
            printf("Failed to Register Device"
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        etstatus = VerifyToolkitResponse(response, request_id);
        if (0 != etstatus) {
            printf("Failed to verify toolkit response\n");
        }

        printf("\nToolkit Response : %s\n", response);

        // End function logic
        break;
    }

    if (response) {
        FreeMemory(response);
    }

    if (request_handle) {
        FreeMemory(request_handle);
    }

    if (userid) {
        free(userid);
    }

    if (pwd) {
        free(pwd);
    }

    if (device_ref_id) {
        free(device_ref_id);
    }

    free(request_id);
    free(user_id);
    free(password);

    return TRUE;
}

// EIDAToolkit Service handler for Xades Sign
BOOL XadesSignHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    unsigned char *xades_sign_data = NULL;
    unsigned long xades_sign_data_len = 0;
    json_object *json_obj = NULL;
    json_object *json_xades_obj = NULL;
    SIGNING_CONTEXT signing_context = { 0 };
    char *input_path = NULL;
    char *output_path = NULL;
    char *sign_data = NULL;
    unsigned int sign_data_len = 0;
    int connection_handle = 0;
    char pin[16] = { 0 };
    unsigned int pin_len = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    char *pin_data = NULL;
    unsigned int pin_data_len = 0;
    int attempts_left = 0;

    // Begin function logic
    for (;;) {

        if (!ReadFromFile(DIGITAL_SIGNATURE_PARAMS, &xades_sign_data,
            &xades_sign_data_len)) {
            printf("Failed to read %s file\n", DIGITAL_SIGNATURE_PARAMS);
            break;
        }

        // Parse the JSON string
        json_obj = json_tokener_parse(xades_sign_data);
        if (!json_obj) {
            printf("Failed parse %s file\n", DIGITAL_SIGNATURE_PARAMS);
            break;
        }

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        etstatus = get_pin(pin, &pin_len);
        if (0 != etstatus) {
            printf("Failed to get pin \n");
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(request_handle, request_handle_len,
                pin, pin_len, &pin_data, &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to encrypt application data \n");
                break;
            }
        }

        etstatus = GetJsonAttribute(json_obj, "signing_context",
            json_type_object, FALSE, &json_xades_obj, NULL, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed parse signing_context object\n");
            break;
        }

        etstatus = GetSigningAttributes(json_xades_obj,
            &input_path, &output_path, &signing_context, XADES);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("GetSigningAttributes failed\n");
            break;
        }

        strncpy(signing_context.pin, !toolkit_options->sm_mode ? pin_data :
            pin, sizeof(signing_context.pin) - 1);

        etstatus = XadesSign(&signing_context, input_path,
            output_path, &sign_data, &sign_data_len, &attempts_left);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to digitally sign data "
                "[attempts left:%d, Error:\"%s\"]\n",
                attempts_left, GetStatusMessage(etstatus));
            break;
        }

        printf("Xades Sign Successful\n");

        // End function logic
        break;
    }

    free(input_path);
    free(output_path);
    free(xades_sign_data);
    json_object_put(json_obj);
    free(request_id);
    if (request_handle) {
        FreeMemory(request_handle);
    }
    free(pin_data);

    return TRUE;
}

// EIDAToolkit Service handler for Xades Verify
BOOL XadesVerifyHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    unsigned int attempts_left = 0;
    unsigned char *xades_data = NULL;
    unsigned long xades_data_len = 0;
    unsigned long xades_verify_data_len = 0;
    json_object *json_obj = NULL;
    json_object *json_xades_obj = NULL;
    json_object *json_verify_obj = NULL;
    SIGNING_CONTEXT signing_context = { 0 };
    char *input_path = NULL;
    char *output_path = NULL;
    char *verify_input_path = NULL;
    char *verify_output_path = NULL;
    char *signature = NULL;
    int connection_handle = 0;
    char pin[16] = { 0 };
    unsigned int pin_len = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    char *pin_data = NULL;
    unsigned int pin_data_len = 0;
    unsigned int signature_length = 0;
    VERIFICATION_CONTEXT verification_ctxt = { 0 };
    char *verification_report = NULL;
    unsigned int report_len = 0;

    // Begin function logic
    for (;;) {

        if (!ReadFromFile(DIGITAL_SIGNATURE_PARAMS, &xades_data,
            &xades_data_len)) {
            printf("Failed to read %s file\n", DIGITAL_SIGNATURE_PARAMS);
            break;
        }

        json_obj = json_tokener_parse(xades_data);
        if (!json_obj) {
            printf("Failed parse %s file\n", DIGITAL_SIGNATURE_PARAMS);
            break;
        }

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        etstatus = get_pin(pin, &pin_len);
        if (0 != etstatus) {
            printf("Failed to get pin \n");
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(request_handle, request_handle_len,
                pin, pin_len, &pin_data, &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to encrypt application data \n");
                break;
            }
        }

        etstatus = GetJsonAttribute(json_obj, "signing_context",
            json_type_object, FALSE, &json_xades_obj, NULL, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed parse signing_context object\n");
            break;
        }

        etstatus = GetSigningAttributes(json_xades_obj,
            &input_path, &output_path, &signing_context, XADES);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("GetSigningAttributes failed\n");
            break;
        }

        strncpy(signing_context.pin, !toolkit_options->sm_mode ? pin_data :
            pin, sizeof(signing_context.pin) - 1);

        etstatus = XadesSign(&signing_context, input_path, output_path,
            &signature, &signature_length, &attempts_left);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to digitally sign data "
                "[attempts left:%d, Error:\"%s\"]\n",
                attempts_left, GetStatusMessage(etstatus));
            break;
        }

        printf("\nXades Sign Successful\n");

        etstatus = GetJsonAttribute(json_obj, "verification_context",
            json_type_object, FALSE, &json_verify_obj, NULL, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed parse signing_context object\n");
            break;
        }

        etstatus = GetVerifyCtxtAttributes(json_verify_obj,
            &verify_input_path, &verify_output_path, &verification_ctxt);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed get verification context attributes \n");
            break;
        }

        etstatus = XadesVerify(&verification_ctxt, verify_input_path,
            signature, signature_length, &verification_report,
            &report_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to verify signature "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("\nXades Verify Successful, report %s",
            verification_report);

        // End function logic
        break;
    }

    free(input_path);
    free(output_path);
    free(verify_input_path);
    free(verify_output_path);
    free(xades_data);
    json_object_put(json_obj);
    free(request_id);

    if (request_handle) {
        FreeMemory(request_handle);
    }

    if (verification_report) {
        FreeMemory(verification_report);
    }

    if (signature) {
        FreeMemory(signature);
    }

    return TRUE;
}

// EIDAToolkit Service handler for Cades Sign
BOOL CadesSignHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    unsigned char *cades_sign_data = NULL;
    unsigned long cades_sign_data_len = 0;
    json_object *json_obj = NULL;
    json_object *json_cades_obj = NULL;
    SIGNING_CONTEXT signing_context = { 0 };
    char *input_path = NULL;
    char *output_path = NULL;
    unsigned char *sign_data = NULL;
    unsigned int sign_data_len = 0;
    int connection_handle = 0;
    char pin[16] = { 0 };
    unsigned int pin_len = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    char *pin_data = NULL;
    unsigned int pin_data_len = 0;
    int attempts_left = 0;

    // Begin function logic
    for (;;) {

        if (!ReadFromFile(DIGITAL_SIGNATURE_PARAMS, &cades_sign_data,
            &cades_sign_data_len)) {
            printf("Failed to read %s file\n", DIGITAL_SIGNATURE_PARAMS);
            break;
        }

        // Parse the JSON string
        json_obj = json_tokener_parse(cades_sign_data);
        if (!json_obj) {
            printf("Failed parse %s file\n", DIGITAL_SIGNATURE_PARAMS);
            break;
        }

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        etstatus = get_pin(pin, &pin_len);
        if (0 != etstatus) {
            printf("Failed to get pin \n");
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(request_handle, request_handle_len,
                pin, pin_len, &pin_data, &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to encrypt application data \n");
                break;
            }
        }

        etstatus = GetJsonAttribute(json_obj, "signing_context",
            json_type_object, FALSE, &json_cades_obj, NULL, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed parse signing_context object\n");
            break;
        }

        etstatus = GetSigningAttributes(json_cades_obj,
            &input_path, &output_path, &signing_context, CADES);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("GetSigningAttributes failed\n");
            break;
        }

        strncpy(signing_context.pin, !toolkit_options->sm_mode ? pin_data :
            pin, sizeof(signing_context.pin) - 1);

        etstatus = CadesSign(&signing_context, input_path,
            &sign_data, &sign_data_len, &attempts_left);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to digitally sign data "
                "[attempts left:%d, Error:\"%s\"]\n",
                attempts_left, GetStatusMessage(etstatus));
            break;
        }

        printf("%s: <binary data: %d bytes>\n", "Signature",
            sign_data_len);

        printf("Cades Sign Successful\n");

        // End function logic
        break;
    }

    free(input_path);
    free(output_path);
    free(cades_sign_data);
    json_object_put(json_obj);
    free(request_id);
    free(pin_data);

    if (request_handle) {
        FreeMemory(request_handle);
    }

    if (sign_data) {
        FreeMemory(sign_data);
    }

    return TRUE;
}

// EIDAToolkit Service handler for Cades Verify
BOOL CadesVerifyHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    unsigned char *cades_sign_data = NULL;
    unsigned long cades_sign_data_len = 0;
    unsigned char *cades_verify_data = NULL;
    unsigned long cades_verify_data_len = 0;
    json_object *json_obj = NULL;
    json_object *json_cades_obj = NULL;
    json_object *json_verify_obj = NULL;
    SIGNING_CONTEXT signing_context = { 0 };
    char *input_path = NULL;
    char *output_path = NULL;
    char *verify_input_path = NULL;
    char *verify_output_path = NULL;
    unsigned char *sign_data = NULL;
    unsigned int sign_data_len = 0;
    VERIFICATION_CONTEXT verification_ctxt = { 0 };
    int connection_handle = 0;
    char pin[16] = { 0 };
    unsigned int pin_len = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    char *pin_data = NULL;
    unsigned int pin_data_len = 0;
    char *verification_report = NULL;
    unsigned int report_len = 0;
    int attempts_left = 0;

    // Begin function logic
    for (;;) {

        if(!ReadFromFile(DIGITAL_SIGNATURE_PARAMS, &cades_sign_data,
            &cades_sign_data_len)) {
            printf("Failed to read %s file\n", DIGITAL_SIGNATURE_PARAMS);
            break;
        }

        json_obj = json_tokener_parse(cades_sign_data);
        if (!json_obj) {
            printf("Failed parse %s file\n", DIGITAL_SIGNATURE_PARAMS);
            break;
        }

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        etstatus = get_pin(pin, &pin_len);
        if (0 != etstatus) {
            printf("Failed to get pin \n");
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(request_handle, request_handle_len,
                pin, pin_len, &pin_data, &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to encrypt application data \n");
                break;
            }
        }

        etstatus = GetJsonAttribute(json_obj, "signing_context",
            json_type_object, FALSE, &json_cades_obj, NULL, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed parse signing_context object\n");
            break;
        }

        etstatus = GetSigningAttributes(json_cades_obj,
            &input_path, &output_path, &signing_context, CADES);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("GetSigningAttributes failed\n");
            break;
        }

        strncpy(signing_context.pin, !toolkit_options->sm_mode ? pin_data :
            pin, sizeof(signing_context.pin) - 1);

        etstatus = CadesSign(&signing_context, input_path, &sign_data,
            &sign_data_len, &attempts_left);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to digitally sign data "
                "[attempts left:%d, Error:\"%s\"]\n",
                attempts_left, GetStatusMessage(etstatus));
            break;
        }

        printf("%s: <binary data: %d bytes>\n", "Signature",
            sign_data_len);

        printf("Cades Sign Successful\n");

        etstatus = GetJsonAttribute(json_obj, "verification_context",
            json_type_object, FALSE, &json_verify_obj, NULL, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed parse signing_context object\n");
            break;
        }

        etstatus = GetVerifyCtxtAttributes(json_verify_obj,
            &verify_input_path, &verify_output_path, &verification_ctxt);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("GetVerifyCtxtAttributes failed\n");
            break;
        }

        etstatus = CadesVerify(&verification_ctxt, verify_input_path,
            sign_data, sign_data_len, &verification_report,
            &report_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to verify sign data "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("\nCades Verify Successful report %s\n", verification_report);

        // End function logic
        break;
    }

    free(input_path);
    free(output_path);
    free(verify_input_path);
    free(verify_output_path);
    free(cades_sign_data);
    free(cades_verify_data);
    json_object_put(json_obj);
    free(request_id);

    if (request_handle) {
        FreeMemory(request_handle);
    }

    if (verification_report) {
        FreeMemory(verification_report);
    }

    if (sign_data) {
        FreeMemory(sign_data);
    }

    return TRUE;
}

// EIDAToolkit Service handler for Pades Sign
BOOL PadesSignHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    unsigned char *signing_data = NULL;
    unsigned long signing_data_len = 0;
    unsigned char *pades_sign_data = NULL;
    unsigned long pades_sign_data_len = 0;
    json_object *json_obj = NULL;
    json_object *json_pades_obj = NULL;
    json_object *json_padesparam_obj = NULL;
    SIGNING_CONTEXT signing_context = { 0 };
    char *input_path = NULL;
    char *output_path = NULL;
    PADES_SIGN_PARAMS pades_params = { 0 };
    int connection_handle = 0;
    char pin[16] = { 0 };
    unsigned int pin_len = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    char *pin_data = NULL;
    unsigned int pin_data_len = 0;
    int attempts_left = 0;

    // Begin function logic
    for (;;) {

        if (!ReadFromFile(DIGITAL_SIGNATURE_PARAMS, &signing_data,
            &signing_data_len)) {
            printf("Failed to read %s file\n", DIGITAL_SIGNATURE_PARAMS);
            break;
        }

        // Parse the JSON string
        json_obj = json_tokener_parse(signing_data);
        if (!json_obj) {
            printf("Failed parse %s file\n", DIGITAL_SIGNATURE_PARAMS);
            break;
        }

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        etstatus = get_pin(pin, &pin_len);
        if (0 != etstatus) {
            printf("Failed to get pin \n");
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(request_handle, request_handle_len,
                pin, pin_len, &pin_data, &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to encrypt application data \n");
                break;
            }
        }

        etstatus = GetJsonAttribute(json_obj, "signing_context",
            json_type_object, FALSE, &json_pades_obj, NULL, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed parse signing_context object\n");
            break;
        }

        etstatus = GetSigningAttributes(json_pades_obj,
            &input_path, &output_path, &signing_context, PADES);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("GetSigningAttributes Failed\n");
            break;
        }

        etstatus = GetJsonAttribute(json_obj, "pades_params",
            json_type_object, FALSE, &json_padesparam_obj, NULL, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed parse signing_context object\n");
            break;
        }

        etstatus = GetPADESSigningAttributes(json_padesparam_obj,
            &pades_params);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("GetPADESSigningAttributes Failed\n");
            break;
        }

        strncpy(signing_context.pin, !toolkit_options->sm_mode ? pin_data :
            pin, sizeof(signing_context.pin) - 1);

        etstatus = PadesSign(&signing_context, input_path,
            output_path, &pades_params, &attempts_left);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to digitally sign data "
                "[attempts left:%d, Error:\"%s\"]\n",
                attempts_left, GetStatusMessage(etstatus));
            break;
        }

        printf("Pades Sign Successful\n");

        // End function logic
        break;
    }

    free(input_path);
    free(output_path);
    free(signing_data);
    free(pades_sign_data);
    json_object_put(json_obj);

    return TRUE;
}

// EIDAToolkit Service handler for Pades Verify
BOOL PadesVerifyHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    unsigned char *signing_data = NULL;
    unsigned long signing_data_len = 0;
    unsigned char *pades_verify_data = NULL;
    unsigned long pades_verify_data_len = 0;
    json_object *json_obj = NULL;
    json_object *json_pades_obj = NULL;
    json_object *json_padesparam_obj = NULL;
    json_object *json_verify_obj = NULL;
    SIGNING_CONTEXT signing_context = { 0 };
    char *input_path = NULL;
    char *output_path = NULL;
    PADES_SIGN_PARAMS pades_params = { 0 };
    VERIFICATION_CONTEXT verification_ctxt = { 0 };
    char *verify_input_path = NULL;
    char *verify_output_path = NULL;
    int connection_handle = 0;
    char pin[16] = { 0 };
    unsigned int pin_len = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    char *request_id = NULL;
    unsigned int request_id_len = 0;
    char *pin_data = NULL;
    unsigned int pin_data_len = 0;
    char *verification_report = NULL;
    unsigned int report_len = 0;
    int attempts_left = 0;

    // Begin function logic
    for (;;) {

        if(!ReadFromFile(DIGITAL_SIGNATURE_PARAMS, &signing_data,
            &signing_data_len)) {
            printf("Failed to read %s file\n", DIGITAL_SIGNATURE_PARAMS);
            break;
        }

        // Parse the JSON string
        json_obj = json_tokener_parse(signing_data);
        if (!json_obj) {
            printf("Failed parse %s file\n", DIGITAL_SIGNATURE_PARAMS);
            break;
        }

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        etstatus = PrepareVGRequest(connection_handle, &request_id,
            &request_id_len, &request_handle, &request_handle_len);
        if (0 != etstatus) {
            printf("Failed to prepare request\n");
            break;
        }

        etstatus = get_pin(pin, &pin_len);
        if (0 != etstatus) {
            printf("Failed to get pin \n");
            break;
        }

        if (!toolkit_options->sm_mode) {
            etstatus = EncryptAppData(request_handle, request_handle_len,
                pin, pin_len, &pin_data, &pin_data_len);
            if (0 != etstatus) {
                printf("Failed to encrypt application data \n");
                break;
            }
        }

        etstatus = GetJsonAttribute(json_obj, "signing_context",
            json_type_object, FALSE, &json_pades_obj, NULL, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed parse signing_context object\n");
            break;
        }

        etstatus = GetSigningAttributes(json_pades_obj,
            &input_path, &output_path, &signing_context, PADES);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("GetSigningAttributes Failed\n");
            break;
        }

        etstatus = GetJsonAttribute(json_obj, "pades_params",
            json_type_object, FALSE, &json_padesparam_obj, NULL, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed parse signing_context object\n");
            break;
        }

        etstatus = GetPADESSigningAttributes(json_padesparam_obj,
            &pades_params);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("GetPADESSigningAttributes Failed\n");
            break;
        }

        strncpy(signing_context.pin, !toolkit_options->sm_mode ? pin_data :
            pin, sizeof(signing_context.pin) - 1);

        etstatus = PadesSign(&signing_context, input_path,
            output_path, &pades_params, &attempts_left);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to digitally sign data "
                "[attempts left:%d, Error:\"%s\"]\n",
                attempts_left, GetStatusMessage(etstatus));
            break;
        }

        printf("Pades Sign Successful\n");

        etstatus = GetJsonAttribute(json_obj, "verification_context",
            json_type_object, FALSE, &json_verify_obj, NULL, NULL);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed parse signing_context object\n");
            break;
        }

        etstatus = GetVerifyCtxtAttributes(json_verify_obj,
            &verify_input_path, &verify_output_path, &verification_ctxt);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("GetVerifyCtxtAttributes Failed\n");
            break;
        }

        etstatus = PadesVerify(&verification_ctxt, verify_input_path,
            &verification_report, &report_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to verify sign data "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("\nPades Verify Successful, report %s",
            verification_report);

        // End function logic
        break;
    }

    free(input_path);
    free(output_path);
    free(signing_data);
    free(pades_verify_data);
    free(verify_input_path);
    free(verify_output_path);
    json_object_put(json_obj);
    free(request_id);

    if (request_handle) {
        FreeMemory(request_handle);
    }

    if (verification_report) {
        FreeMemory(verification_report);
    }

    return TRUE;
}

// EIDAToolkit Service handler to get toolkit version
BOOL GetToolkitVersionHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    int major = 0;
    int minor = 0;
    int patch = 0;

    // Begin function logic
    for (;;) {

        // Read the Emirates ID Card public data
        etstatus = GetToolkitVersion(&major, &minor, &patch);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could not get toolkit version "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("\n Toolkit Version : %d.%d.%d\n", major, minor, patch);

        // End function logic
        break;
    }

    return result;
}

// EIDAToolkit Service handler to get interface type
BOOL GetInterfaceTypeHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    unsigned int connection_handle = 0;
    int interface_type = 0;

    // Begin function logic
    for (;;) {

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        // Read the Emirates ID Card public data
        etstatus = GetInterfaceType(connection_handle, &interface_type);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could not get interface type "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("\n interface type : %d\n", interface_type);

        // End function logic
        break;
    }

    return result;
}

// EIDAToolkit Service handler to get device id
BOOL GetDeviceIDHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    char *device_id = NULL;
    unsigned int device_id_len = 0;

    // Begin function logic
    for (;;) {

        // Read the Emirates ID Card public data
        etstatus = GetDeviceID(&device_id, &device_id_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could not get device id "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("\n device id : %s\n", device_id);

        // End function logic
        break;
    }

    if (device_id) {
        FreeMemory(device_id);
    }

    return result;
}

// EIDAToolkit Service handler for verifying certificate using ocsp
BOOL VerifyCertificateUsingOCSPHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    char *cert_path = NULL;
    char *ocsp_url = NULL;
    char *issuer_cert_path = NULL;
    unsigned char *cert_data = NULL;
    unsigned int cert_len = 0;
    unsigned char *issuer_cert_data = NULL;
    unsigned int issuer_cert_data_len = 0;

    // Begin function logic
    for (;;) {

        printf("Please enter ocsp url :\n");
        while (getchar() != '\n');
        ocsp_url = ReadLine(stdin);
        if (ocsp_url) {
            printf("Failed to read ocsp url\n");
            break;
        }

        printf("Please enter certificate path :\n");
        cert_path = ReadLine(stdin);
        if (cert_path) {
            printf("Failed to read certificate path\n");
            break;
        }

        printf("\nPlease enter issuer certificate path : \n");
        issuer_cert_path = ReadLine(stdin);
        if (issuer_cert_path) {
            printf("Failed to read issuer certificate path\n");
            break;
        }

        if (!ReadFromFile(cert_path, &cert_data, &cert_len)) {
            printf("Failed to read certificate data \n");
            break;
        }

        if (!ReadFromFile(issuer_cert_path, &issuer_cert_data,
            &issuer_cert_data_len)) {
            printf("Failed to read issuer certificate data \n");
            break;
        }

        etstatus = VerifyCertificateUsingOCSP(ocsp_url, cert_data, cert_len,
            issuer_cert_data, issuer_cert_data_len);
        if (etstatus != ETSTATUS_SUCCESS) {
            printf("Failed to verify certificate using ocsp "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        printf("Certifiate verification successful\n");

        // End function logic
        break;
    }

    if (cert_data) {
        free(cert_data);
    }

    if (issuer_cert_data) {
        free(issuer_cert_data);
    }

    return TRUE;
}

// EIDAToolkit Service handler for Authenticate Biometric On Server using NFC
BOOL AuthenticateBiometricOnServerNfcHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    BOOL result = TRUE;
    int connection_handle = 0;
    char first_finger = 0;
    char first_finger_index = 0;
    char second_finger = 0;
    char second_finger_index = 0;
    BOOL auth_status = 0;
    int finger_index = 0;
    int finger_id = 0;
    char *request_handle = NULL;
    unsigned int request_handle_len = 0;
    unsigned char request_id[40] = { 0 };
    char *enc_request_id = NULL;
    unsigned int enc_request_id_len = 0;
    char *response = NULL;
    unsigned int response_len = 0;
    char card_number[MAX_SIZE] = { 0 };
    char date_of_birth[MAX_SIZE] = { 0 };
    char expiry_date[MAX_SIZE] = { 0 };
    char mrz[MAX_SIZE] = { 0 };
    char *mrz_data = NULL;

    // Begin function logic
    for (;;) {

        if (1 != RAND_bytes(request_id, 40)) {
            printf("Failed to generate request id\n");
            break;
        }

        etstatus = Base64Encode(request_id, sizeof(request_id),
            &enc_request_id, &enc_request_id_len);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to generate request id\n");
            break;
        }

        if (!ConnectEIDACard(&connection_handle)) {
            break;
        }

        etstatus = GetNFCAuthData(card_number, sizeof(card_number),
            date_of_birth, sizeof(date_of_birth), expiry_date,
            sizeof(expiry_date), mrz, sizeof(mrz));
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to retrieve NFC authentication data\n");
            break;
        }

        if (mrz && mrz[0]) {
            mrz_data = mrz;
        }

        etstatus = SetNfcAuthenticationParams(connection_handle, card_number,
            date_of_birth, expiry_date, mrz_data);
        if (etstatus != ETSTATUS_SUCCESS) {
            printf("Failed to set NFC authentication parameters: %s\n",
                GetStatusMessage(etstatus));
            break;
        }

        // Get the figer index
        etstatus = GetFingerIndex(connection_handle, &first_finger,
            &first_finger_index, &second_finger, &second_finger_index);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could not retrieve finger index from card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        if (!GetFingerIndexDetails(first_finger, first_finger_index,
            second_finger, second_finger_index, &finger_index, &finger_id)) {
            printf("Failed to retrieve finger index details\n");
            break;
        }

        // Perform biometric authentication
        etstatus = AuthenticateBiometricOnServer(connection_handle,
            enc_request_id, finger_index, 30, &response, &response_len);
        if (ETSTATUS_SUCCESS != etstatus) {

            printf("Biometric (fingerprint) authentication failed"
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        etstatus = VerifyToolkitResponse(response, enc_request_id);
        if (0 != etstatus) {
            printf("Failed to verify toolkit response\n");
            break;
        }

        printf("\nToolkit Response : %s\n", response);

        // End function logic
        break;
    }

    // Disconnect the Emirates ID Card
    if (connection_handle) {
        etstatus = Disconnect(connection_handle);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Could disconnect Emirates ID Card "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
        }
    }

    if (response) {
        FreeMemory(response);
    }

    free(enc_request_id);

    return TRUE;
}
