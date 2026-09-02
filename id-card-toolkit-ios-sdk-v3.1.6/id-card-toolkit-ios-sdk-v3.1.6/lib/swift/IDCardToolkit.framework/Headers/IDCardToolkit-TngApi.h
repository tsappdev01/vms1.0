//
//  IDCardToolkit-TngApi.h
//  IDCardToolkit
//
//  Public C-API forward declarations for TNG functions used by the Swift
//  wrapper.  These functions are defined in the toolkit static library
//  (libEIDAToolkit.a) but their declarations live in tng_api.h which pulls
//  in internal types -- so we forward-declare here for Swift consumption.
//
//  Library evolution requires Swift modules to be free of bridging-header
//  source dependencies; this header replaces the bridging header that
//  previously held these declarations.
//

#ifndef IDCardToolkit_TngApi_h
#define IDCardToolkit_TngApi_h

#import "EIDAToolkitTypes.h"
#import "EIDAToolkitError.h"

#ifdef __cplusplus
extern "C" {
#endif

ETAPI ETSTATUS GetToolkitConfigParam(unsigned int config_type,
    char *key, char **config_data, unsigned int *data_len);

ETAPI ETSTATUS CheckCardStatusOffCard(char *idn, char *cn, char *request_id,
    char **response, unsigned int *response_len);

ETAPI ETSTATUS AuthenticateFaceOnServerWithID(char *idn, char *image_base64,
    char *request_id, BOOL with_digital_docs, char **response,
    unsigned int *response_len);

ETAPI ETSTATUS AuthenticateFaceOnServerWithPassport(char *passport_no,
    char *issuing_country, char *expiry_date, char *dob, char *image_base64,
    char *request_id, BOOL with_digital_docs, char **response,
    unsigned int *response_len);

ETAPI ETSTATUS AuthenticateFaceOnServerWithIDEx(char *idn, char *image_base64,
    char *liveness_data, unsigned int liveness_mode, char *request_id,
    BOOL with_digital_docs, char **response, unsigned int *response_len);

ETAPI ETSTATUS AuthenticateFaceOnServerWithPassportEx(char *passport_no,
    char *issuing_country, char *expiry_date, char *dob,
    char *image_base64, char *liveness_data, unsigned int liveness_mode,
    char *request_id, BOOL with_digital_docs, char **response,
    unsigned int *response_len);

#ifdef __cplusplus
}
#endif

#endif /* IDCardToolkit_TngApi_h */
