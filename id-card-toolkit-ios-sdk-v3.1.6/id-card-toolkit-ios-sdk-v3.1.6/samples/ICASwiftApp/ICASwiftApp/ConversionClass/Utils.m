//
//  Utils.m
//  EIDAToolkitSwiftTestapp
//
//  Created by  on 1/29/17.
//  Copyright © 2017 Emirates Identity Authority. All rights reserved.
//

#import "Utils.h"

extern NSString * const FormatType_toString[];
NSString * const FormatType_toString[] = {
    [HEALTH_INSURANCE] = @"HEALTH_INSURANCE",
    [HEALTH_INSURANCE_PROTECTED] = @"HEALTH_INSURANCE_PROTECTED",
    [MOD_DATA] = @"MOD_DATA",
    [HOMEADDRESS] = @"HOMEADDRESS",
    [WORKADDRESS] = @"WORKADDRESS",
    [HEAD_OF_FAMILY] = @"HEAD_OF_FAMILY",
    [WIFE1] = @"WIFE1",
    [WIFE2] = @"WIFE2",
    [WIFE3] = @"WIFE3",
    [WIFE4] = @"WIFE4",
    [CHILD1] = @"CHILD1",
    [CHILD2] = @"CHILD2",
    [CHILD3] = @"CHILD3",
    [CHILD4] = @"CHILD4",
    [CHILD5] = @"CHILD5",
    [CHILD6] = @"CHILD6",
    [CHILD7] = @"CHILD7",
    [CHILD8] = @"CHILD8",
    [CHILD9] = @"CHILD9",
    [CHILD10] = @"CHILD10",
    [CHILD11] = @"CHILD11",
    [CHILD12] = @"CHILD12",
    [CHILD13] = @"CHILD13",
    [CHILD14] = @"CHILD14",
    [CHILD15] = @"CHILD15",
    [CHILD16] = @"CHILD16",
    [CHILD17] = @"CHILD17",
    [CHILD18] = @"CHILD18",
    [CHILD19] = @"CHILD19",
    [CHILD20] = @"CHILD20"
};

static const int container_option_count = sizeof(FormatType_toString) /
sizeof(FormatType_toString[0]);

@implementation Utils

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

char *_cert_data = "LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tDQpNSUlENWpDQ0FzNmdBd0lCQWdJVU43VU1rK1NSZ01ZZTlZMGdtMUpPa0dSUzErc3dEUVlKS29aSWh2Y05BUUVMQlFBd2VqRUxNQWtHDQpBMVVFQlJNQ01ESXhDekFKQmdOVkJBWVRBa0ZGTVNzd0tRWURWUVFLRXlKSlpHVnVkR2wwZVNCQmJtUWdRMmwwYVhwbGJuTm9hWEFnDQpRWFYwYUc5eWFYUjVNUkV3RHdZRFZRUUxFd2hKWkdWdWRHbDBlVEVlTUJ3R0ExVUVBeE1WUlcxcGNtRjBaWE5KUkNCVFpYSjJaWEp6DQpJRU5CTUI0WERUSXpNRFV4TmpBMU5UTTFNMW9YRFRJMk1EZ3hOakExTlRNMU0xb3dmakVMTUFrR0ExVUVCaE1DUVVVeEVqQVFCZ05WDQpCQWNUQ1VGaWRTQkVhR0ZpYVRFck1Da0dBMVVFQ2hNaVNXUmxiblJwZEhrZ1FXNWtJRU5wZEdsNlpXNXphR2x3SUVGMWRHaHZjbWwwDQplVEVSTUE4R0ExVUVDeE1JU1dSbGJuUnBkSGt4R3pBWkJnTlZCQU1URWxaaGJHbGtZWFJwYjI0Z1IyRjBaWGRoZVRDQ0FTSXdEUVlKDQpLb1pJaHZjTkFRRUJCUUFEZ2dFUEFEQ0NBUW9DZ2dFQkFKU1pVUmtCUHk0VVlTaURIRWxDazljTm1qUlhOcGJ6WjkzVXoraFpCeDhpDQplU2xheUxtT2RFMURJbHR3MnRSbFZkVHlsK1poMXloY0EwSHBhbWZ2dy8vaXY3NG0vSGhKeEtTWXZzYVd4YWFNcnFWdk5zcFlsT1ExDQpwVzA4d2R6S0pVRGd5ekIvL0hoQ01KTExxN3Myc0tvcHFGTDQ5QVhRV04wV3ZjZmljZ1hreUptWTRCb2ZPTkFBbGxOcURuWFp2Y0xuDQpOYW8rZ3pGWHJES2FEL2pOcURCcWNkeStaL2xBcmFXV3NKc3Q4eWVOeElEVHhUMlhRR1FwSlZ6Q1F3dzhBUXNNR0c1eGNCWGt2OWhsDQpOdkV1LzJBeVl6SGVnWFVNRGJ0OXJWNSt4dHNBaDdMQkFuVE1iTzJ6WksvQ3hHWnZXcmFVazhxUE5GbjBBVE1Jb2pybDU3OENBd0VBDQpBYU5nTUY0d0RnWURWUjBQQVFIL0JBUURBZ2JBTUF3R0ExVWRFd0VCL3dRQ01BQXdId1lEVlIwakJCZ3dGb0FVeUVWRlRkSUNINHRIDQp6M3hKdFZ4ZzYyRWtCRDR3SFFZRFZSME9CQllFRkxnOExjdFhPSmNSKytYM2QxWXQzb3g0V1dXbk1BMEdDU3FHU0liM0RRRUJDd1VBDQpBNElCQVFDRUpuSy9TclNzbHNjcEtvTzhCSVE0OTJmV1ZyWnByRTZkUzRYRUtETnJzakpWN21xVnlRaGpSRkFzTDFsQkFOSWJMYm5zDQpKODRIUGZhZ1hxUkJNRmNLVUtEaEQwRks3ZFJtZ0VXL2ZrZVVaOUorVUxVd1I0M3hSb2M5TWVhMnkzMnV2bzJxWG1COUxHaXlVN2lmDQo1Mngydm42aUVlZlcva0lBOUwza1RLT2JKbzRHSzZaRlFkRzdVSTc0Qk9RbTVmRGZPVWhkT0UvaXg1c3hWY0NmamZxbnpMcGxQQnd5DQpLbHlSQmJYYjBsRXR2KzlvbU5VdGN4N1dpd3VhaTRJcFp0YVErR1F0MUJTdTRsZ3hqK1ZLVHQwekRTRG5BelhidHo0VGY5N3FkakUyDQo5OVJlSERwK2t4MjBkTW5XaGM0N3VGQlVjN0IvWVNWNWV5NTBWVFpHUDNlUw0KLS0tLS1FTkQgQ0VSVElGSUNBVEUtLS0tLQ";

// Encode data in base64 format
unsigned int Base64Encode(const unsigned char *data, unsigned int data_length,
                          unsigned char **encoded_data, unsigned int *encoded_data_length) {
    
    unsigned int etstatus = 0;
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
            return -1;
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
                return -1;
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
unsigned int Base64Decode(const unsigned char *encoded_data,
                          unsigned int encoded_data_length, unsigned char **decoded_data,
                          unsigned int *decoded_data_length) {
    
    unsigned int etstatus = 0;
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
            return -1;
        }
        
        // Compute the decoded data length
        decoded_length = GetBase64DecodeLength(encoded_data,
                                               encoded_data_length);
        
        // If the buffer is provided, check if the length is sufficient
        if (*decoded_data && (*decoded_data_length < (decoded_length + 1))) {
            etstatus = -1;
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

// Get Validation Gateway (VG) public key
unsigned int GetPublicKey(RSA **rsa_pubkey, int license_type,unsigned char *publicKey,unsigned int* key_len) {
    
    ETSTATUS etstatus = 0;
    //    unsigned char *data_protection_key = NULL;
    //    unsigned int key_len = 0;
    char *modulus = NULL;
    unsigned int modulus_len = 0;
    char *exponent = NULL;
    unsigned int exponent_len = 0;
    BIO *bio = NULL;
    
    // Begin function logic
    for (;;) {
        
        // Verify input parameters
        if (!rsa_pubkey) {
            break;
        }
        
        
        bio = BIO_new_mem_buf((void *)publicKey, key_len);
        if (!bio) {
            break;
        }
        
        *rsa_pubkey = d2i_RSA_PUBKEY_bio(bio, NULL);
        
        // End function logic
        break;
    }
    
    
    return etstatus;
}
// Verify digital signature
unsigned int VerifyDigitalSignature(const char *xml_str,
                                    unsigned char *cert_data, unsigned int cert_data_len) {
    
    unsigned int etstatus = 0;
    xmlDocPtr doc = NULL;
    xmlNodePtr node = NULL;
    xmlSecDSigCtxPtr digital_signature_ctxt = NULL;
    
    // Begin Function logic
    for (;;) {
        
        // Verify input parameters
        if (!xml_str || !cert_data || !cert_data_len) {
            etstatus = -1;
            break;
        }
        
        doc = xmlParseDoc((const xmlChar*)xml_str);
        if (!doc) {
            etstatus = -1;
            break;
        }
        
        LIBXML_TEST_VERSION
        xmlLoadExtDtdDefaultValue = XML_DETECT_IDS | XML_COMPLETE_ATTRS;
        xmlSubstituteEntitiesDefault(1);
        
        // Init xmlsec library
        if (xmlSecInit() < 0) {
            etstatus = -1;
            break;
        }
        
        // Check loaded library version
        if (xmlSecCheckVersion() != 1) {
            etstatus = -1;
            break;
        }
        
        // Init crypto library
        if (xmlSecCryptoAppInit(NULL) < 0) {
            etstatus = -1;
            break;
        }
        
        // Init xmlsec-crypto library
        if (xmlSecCryptoInit() < 0) {
            etstatus = -1;
            break;
        }
        
        // Find start node
        node = xmlSecFindNode(xmlDocGetRootElement(doc),
                              xmlSecNodeSignature, xmlSecDSigNs);
        if (!node) {
            etstatus = -1;
            break;
        }
        
        // Create signature context
        digital_signature_ctxt = xmlSecDSigCtxCreate(NULL);
        if (!digital_signature_ctxt) {
            etstatus = -1;
            break;
        }
        
        // Load public key
        digital_signature_ctxt->signKey = xmlSecCryptoAppKeyLoadMemory(
                                                                       cert_data, cert_data_len, xmlSecKeyDataFormatCertPem,
                                                                       NULL, NULL, NULL);
        if (digital_signature_ctxt->signKey == NULL) {
            digital_signature_ctxt->signKey = xmlSecCryptoAppKeyLoadMemory(
                                                                           cert_data, cert_data_len, xmlSecKeyDataFormatCertDer, NULL,
                                                                           NULL, NULL);
            if (digital_signature_ctxt->signKey == NULL) {
                etstatus = -1;
                break;
            }
        }
        
        digital_signature_ctxt->flags =
        XMLSEC_DSIG_FLAGS_STORE_SIGNEDINFO_REFERENCES |
        XMLSEC_DSIG_FLAGS_STORE_MANIFEST_REFERENCES |
        XMLSEC_DSIG_FLAGS_STORE_SIGNATURE;
        
        // Verify signature
        if (xmlSecDSigCtxVerify(digital_signature_ctxt, node) < 0) {
            etstatus = -1;
            break;
        }
        
        if (digital_signature_ctxt->status != xmlSecDSigStatusSucceeded) {
            etstatus = -1;
        }
        
        // End function logic
        break;
    }
    
    // Shutdown xmlsec-crypto library
    xmlSecCryptoShutdown();
    
    // Shutdown crypto library
    xmlSecCryptoAppShutdown();
    
    // Shutdown xmlsec library
    xmlSecShutdown();
    
    if (digital_signature_ctxt) {
        xmlSecDSigCtxDestroy(digital_signature_ctxt);
    }
    
    if (doc) {
        xmlFreeDoc(doc);
    }
    
    return etstatus;
}
// Verify toolkit response
unsigned int VerifyToolkitResponse(char *response, char *request_id, char **error_message) {
    
    unsigned int etstatus = 0;
    char *toolkit_request_id = NULL;
    unsigned char *cert_data = NULL;
    unsigned int cert_data_len = 0;
    unsigned int len = 0;
    // Begin function logic
    for (;;) {
        
        // Verify input parameters
        if (!response || !request_id || !error_message) {
            return -1;
        }
        
        *error_message = (char*)malloc(256);
        if(!error_message){
            return -1;
        }
        memset(*error_message, 0, 256);
        /*
        etstatus = Base64Decode(_cert_data, strlen(_cert_data), &cert_data,
                                &cert_data_len);
        if (0 != etstatus) {
            strncpy(*error_message, "Failed to decode certificate data", 256);
            break;
        }
        
        etstatus = VerifyDigitalSignature(response, cert_data, cert_data_len);
        if (0 != etstatus) {
            strncpy(*error_message, "Failed to verify response signature", 256);
            break;
        }*/
        
        NSString *str=[NSString stringWithFormat:@"%s",response];
        NSData *data = [str dataUsingEncoding:NSUTF8StringEncoding];
        SHXMLParser   *parser   = [[SHXMLParser alloc] init];
        NSDictionary  *resultObject  = [parser parseData:data];
        NSString *responseContent = [SHXMLParser getDataAtPath:@"ValidationGatewayResponse.Message.Header.RequestID" fromResultObject:resultObject];
        const char *reqId=responseContent.UTF8String;
        
        len = (unsigned int)strlen(reqId);
        toolkit_request_id = (char*)malloc(len + 1);
        if ( !toolkit_request_id) {
            strncpy(*error_message, "Failed to allocate memory", 256);
            break;
        }
        
        memset(toolkit_request_id, 0, len + 1);
        strncpy(toolkit_request_id, reqId, len);
        
        if (strcmp(toolkit_request_id, request_id)) {
            strncpy(*error_message, "Potential replay attack has been detected", 256);
            break;
        }
        // End function logic
        break;
    }
    
    //free(cert_data);
    free(toolkit_request_id);
    
    return etstatus;
}

// Encrypt Data
unsigned int EncryptData(unsigned char *data, unsigned int data_len,
                         int license_type, unsigned char **enc_data,
                         unsigned int *enc_data_len,unsigned char *publicKey,unsigned int* keylen) {
    
    unsigned int etstatus = 0;
    RSA *rsa_key = NULL;
    unsigned int key_len = 0;
    
    // Begin function logic
    for (;;) {
        
        // Verify input parameters
        if (!data || !data_len || !license_type ||
            !enc_data || !enc_data_len) {
            return -1;
        }
        
         etstatus = GetPublicKey(&rsa_key, license_type,publicKey,keylen);
        if (0 != etstatus) {
            break;
        }
        
        key_len = RSA_size(rsa_key);
        
        *enc_data = malloc(key_len);
        memset(*enc_data, 0, key_len);
        
        *enc_data_len = key_len;
        
        RSA_public_encrypt(data_len, data, *enc_data,
                           rsa_key, RSA_PKCS1_PADDING);
        
        // End function logic
        break;
    }
    
    return etstatus;
}

// Set parameters
unsigned int SetEncrytionData(char *request_handle, unsigned int request_handle_len,
                          int license_type, char *data, unsigned int data_len,
                          char **enc_data, unsigned int *enc_data_len ,unsigned char *publicKey,unsigned int* key_len) {
    
    unsigned int return_value = 0;
    unsigned char *dec_request_handle = NULL;
    unsigned int dec_request_handle_len = 0;
    unsigned char *wrapped_data = NULL;
    unsigned int len = 0;
    unsigned char *encrypted_data = NULL;
    unsigned int encrypted_data_len = 0;
    
    // Begin function logic
    for (;;) {
        
        // Verify input parameters
        if (!request_handle || !request_handle_len ||
            !data || !data_len || !enc_data || !enc_data_len) {
            return -1;
        }
        
        *enc_data = NULL;
        *enc_data_len = 0;
        
        len = request_handle_len + data_len;
        
        wrapped_data = malloc(len + 1);
        if (!wrapped_data) {
            return_value = -1;
            break;
        }
        
        memset(wrapped_data, 0, len + 1);
        
        return_value = Base64Decode(request_handle, request_handle_len,
                                    &dec_request_handle, &dec_request_handle_len);
        if (0 != return_value) {
            break;
        }
        
        memcpy(wrapped_data, dec_request_handle, dec_request_handle_len);
        memcpy(wrapped_data + dec_request_handle_len, data, data_len);
        
        len = dec_request_handle_len + data_len;
        
        return_value = EncryptData(wrapped_data, len, license_type,
                                   &encrypted_data, &encrypted_data_len,publicKey,key_len);
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
+(NSString*)base64forData:(NSData*)theData {
    
    const uint8_t* input = (const uint8_t*)[theData bytes];
    NSInteger length = [theData length];
    
    static char table[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    
    NSMutableData* data = [NSMutableData dataWithLength:((length + 2) / 3) * 4];
    uint8_t* output = (uint8_t*)data.mutableBytes;
    
    NSInteger i;
    for (i=0; i < length; i += 3) {
        NSInteger value = 0;
        NSInteger j;
        for (j = i; j < (i + 3); j++) {
            value <<= 8;
            
            if (j < length) {
                value |= (0xFF & input[j]);
            }
        }
        
        NSInteger theIndex = (i / 3) * 4;
        output[theIndex + 0] =                    table[(value >> 18) & 0x3F];
        output[theIndex + 1] =                    table[(value >> 12) & 0x3F];
        output[theIndex + 2] = (i + 1) < length ? table[(value >> 6)  & 0x3F] : '=';
        output[theIndex + 3] = (i + 2) < length ? table[(value >> 0)  & 0x3F] : '=';
    }
    
    return [[NSString alloc] initWithData:data encoding:NSASCIIStringEncoding];
}

-(NSString *)getOnlineconfigParams {
    
    const char *vg_Url= [self convertString:self.vgUrl];
    const char *config_url= [self convertString:self.configUrl];
    const char *log_Dir= [self convertString:self.LogDir];
    
    char config_params[3000]={0};
    if(!vg_Url[0]){
        snprintf(config_params, sizeof(config_params), "config_url = %s\n log_directory = %s\n", config_url, log_Dir);
    }
    else{
        snprintf(config_params, sizeof(config_params), "config_url = %s\nlog_directory = %s\nvg_url = %s",
                 config_url, log_Dir, vg_Url);
    }
    
    NSString *configParams=[NSString stringWithFormat:@"%s",config_params];
    
    return configParams;
}
-(NSString *)getOfflineconfigParams {
    
    const char *vg_Url= [self convertString:self.vgUrl];
    const char *config_dir= [self convertString:self.configUrl];
    const char *log_Dir= [self convertString:self.LogDir];
    
    char config_params[3000]={0};
    if(!vg_Url[0]){
        snprintf(config_params, sizeof(config_params), "read_publicdata_offline = true\n config_directory = %s\n log_directory = %s\n", config_dir, log_Dir);
    }
    else{
        snprintf(config_params, sizeof(config_params), "read_publicdata_offline = true\n config_directory = %s\nlog_directory = %s\nvg_url = %s",
                 config_dir, log_Dir, vg_Url);
    }
    
    NSString *configParams=[NSString stringWithFormat:@"%s",config_params];
    
    return configParams;
}
-(const char *)convertString:(NSString *)str {
    const  char *string = (const char *) [str UTF8String];
    return string;
}
+(NSString *)setEncrytion:(NSString *)requestHandle data:(NSString *)data publickey:(unsigned char *)publickey keylength:(int)keylength {
    
    const char *encdata= NULL ;
    const char *enclength = NULL;
    NSUInteger intVal = data.length;
    int datalength = (int)intVal;
    NSString *dataStr=@"";
    
    unsigned int  result = SetEncrytionData((char *)[requestHandle UTF8String], (int)[requestHandle length],
                                        1, (char *)[data UTF8String], datalength,
                                        &encdata, &enclength,publickey,(unsigned int)keylength);
    
    if (0 != result) {
        NSLog(@"Failed to set service parameters");
    }
    else {
        dataStr=[NSString stringWithFormat:@"%s",encdata];
    }
    return dataStr;
}
-(NSString *)validateToolkitResponse:(NSString *)requestId xmlstring:(NSString *)xmlstring {
    
    char *requestid = (char *)[requestId UTF8String];
    char *responsXml = (char *)[xmlstring UTF8String];
    char *error_message;
    
    NSString* err_string=@"";
    unsigned int result = VerifyToolkitResponse(responsXml, requestid,&error_message);
    if (0 != result) {
        err_string = [NSString stringWithFormat:@"%s" , error_message];
    }// if
    return err_string;
}
// Read the contents of a file
unsigned int ReadFromFile(const char *file_name,
                          unsigned char **file_buffer, unsigned long *file_length) {
    
    unsigned int etstatus = 0;
    FILE *file = NULL;
    
    unsigned long length = 0;
    unsigned long len_read = 0;
    char *buffer = NULL;
    
    // Begin function logic
    for (;;) {
        
        // Verify input parameters
        if (!file_name || !file_name[0] || !file_buffer || !file_length) {
            return -1;
            break;
        }
        
        // Initialize the return parameters
        *file_buffer = NULL;
        *file_length = 0;
        
        // Open the specified file
        file = fopen(file_name, "rb");
        if (!file) {
            return -1;
        }
        
        // Set the file pointer to the end
        if (fseek(file, 0, SEEK_END)) {
            return -1;
        }
        
        // Get the length of the file
        length = ftell(file);
        if (!length) {
            return -1;
        }
        
        // Allocate memory for the file data
        buffer = (char*)malloc(length + 1);
        if (!buffer) {
            return -1;
        }
        
        // Set the file pointer to the beginning
        if (fseek(file, 0, SEEK_SET)) {
            return -1;
        }
        
        // Read the schema into buffer
        len_read = (unsigned long)fread(buffer, 1, length, file);
        if (len_read < length) {
            return -1;
        }
        buffer[length] = '\0';
        
        // Return the file buffer
        *file_buffer = (unsigned char*)buffer;
        *file_length = length;
        
        // End funtion logic
        break;
    }
    
    // Function cleanup and return
    if ((0 != etstatus) && buffer) {
        //free(buffer);
    }
    
    // Close the file if opened
    if (file) {
        fclose(file);
    }
    
    return etstatus;
}

// Function to add config data
char *UtaAddConfigData(char *key, char *data) {
    
    char config_params[1000] = {0};
    
    // Begin function logic
    for (;;) {
        
        // Verify input parameters
        if ( !data || !data[0] || !key || !key[0] ) {
            
            return NULL;
        }
        
        snprintf(config_params, sizeof(config_params), "%s = %s\n", key, data);
        
        // End function logic
        break;
    }
    
    return config_params;
}

+(unsigned char *)convertString:(NSString *)str {
    const unsigned char *string = (const unsigned char *) [str UTF8String];
    return string;
}
+(UIColor *)colorFromHexString:(NSString *)hexString {
    unsigned rgbValue = 0;
    NSScanner *scanner = [NSScanner scannerWithString:hexString];
    [scanner setScanLocation:1];
    [scanner scanHexInt:&rgbValue];
    
    return [UIColor colorWithRed:((rgbValue & 0xFF0000) >> 16)/255.0 green:((rgbValue & 0xFF00) >> 8)/255.0 blue:(rgbValue & 0xFF)/255.0 alpha:1.0];
}

+(NSString*)generateSecureKey {
    NSMutableData *data = [NSMutableData dataWithLength:40];
    int result = SecRandomCopyBytes(kSecRandomDefault, 40, data.mutableBytes);
    if (result != noErr) {
        return @"";
    }
    return [data base64EncodedStringWithOptions:kNilOptions];
}
-(NSMutableArray *)getContainerDataElements:(NSString *)typeOfData {
    
    int dataValue= 0;
    if ([typeOfData isEqualToString:@"UPDATE_DATA"]) {
        dataValue=container_option_count;
    }
    else if ([typeOfData isEqualToString:@"READ_DATA"]) {
        dataValue = 2;
    }
    
    NSMutableArray *arr=[[NSMutableArray alloc]init];
    for (int i= 0; i<dataValue; i++) {
        NSString *element = FormatType_toString[i];
        [arr addObject:element];
    }
    return arr;
}
-(void)ShowProgressBar:(NSString *)Showtext andView:(UIView *)View {
    
    HUDprogressView = [[MBProgressHUD alloc] initWithView:View];
    [View addSubview:HUDprogressView];
    
    HUDprogressView.detailsLabelText = Showtext;
    HUDprogressView.dimBackground = YES;
    [HUDprogressView showWhileExecuting:@selector(dismiss) onTarget:self withObject:nil animated:YES];
}
-(void)DismissProgressBar {
    HUDprogressView.removeFromSuperViewOnHide = YES;
    HUDprogressView.hidden=YES;
}
- (void)dismiss {
    // Do something usefull in here instead of sleeping ...
    sleep(120);
}

@end

