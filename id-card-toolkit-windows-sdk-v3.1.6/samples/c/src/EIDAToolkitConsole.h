#ifndef EIDA_TOOLKIT_CONSOLE_H_
#define EIDA_TOOLKIT_CONSOLE_H_

// Constants
#define DIGITAL_SIGNATURE_PARAMS "digitalsignature_params.json"

// Read contents of a file
BOOL ReadFromFile(const char *file_name,
    unsigned char **file_buffer, unsigned int *file_length);

// Toolkit service callbacks
typedef BOOL(*PFN_SERVICE_HANDLER)(EIDA_TOOLKIT_OPTIONS *);

// Toolkit console services and associated callbacks
typedef struct _TOOLKIT_SERVICE {
    const char *description;
    PFN_SERVICE_HANDLER ServiceHandler;
} TOOLKIT_SERVICE;

// EIDAToolkit Service handler to read public data
BOOL ReadPublicDataHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler to read public data nfc
BOOL ReadPublicDataNfcHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler to get toolkit version
BOOL GetToolkitVersionHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler to get interface type
BOOL GetInterfaceTypeHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler to get device id
BOOL GetDeviceIDHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler to check Emirates ID card status
BOOL CheckCardStatusHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler to Export PKI certificates
BOOL ExportPKICertificatesHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Authenticate Biometric On Server
BOOL AuthenticateBiometricOnServerHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Reset PIN
BOOL ResetPinHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Pki Authentication
BOOL AuthenticatePkiHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler to check if Emirates ID card is Genuine
BOOL IsCardGenuineHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Unblock Pin
BOOL UnblockPinHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for reading Family Book Data
BOOL ReadFamilyBookDataHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Sign Data
BOOL SignDataHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Sign Challenge
BOOL SignChallengeHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for verifying certificate using crl
BOOL VerifyCertificateUsingCRLHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Registering Device
BOOL RegisterDeviceHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Xades Sign
BOOL XadesSignHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Xades Verify
BOOL XadesVerifyHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Cades Sign
BOOL CadesSignHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Cades Verify
BOOL CadesVerifyHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Pades Sign
BOOL PadesSignHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Pades Verify
BOOL PadesVerifyHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for verifying certificate using ocsp
BOOL VerifyCertificateUsingOCSPHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

// EIDAToolkit Service handler for Authenticate Biometric On Server using NFC
BOOL AuthenticateBiometricOnServerNfcHandler(EIDA_TOOLKIT_OPTIONS *toolkit_options);

#endif // EIDA_TOOLKIT_CONSOLE_H_
