// Standard C header files
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#if defined(_WIN32)
#include <conio.h>
#else
#include <unistd.h>
#include <limits.h>
#endif
#if defined(__APPLE__)
#include <mach-o/dyld.h>
#endif
#include "json.h"

// EIDA Toolkit header files
#include <EIDAToolkitTypes.h>
#include <EIDAToolkitError.h>
#include <EIDAToolkitApi.h>

// Project local header files
#include "EIDAToolkitOptions.h"
#include "EIDAToolkitConsole.h"
#include "LibXmlParser.h"

// Global EIDAToolkit options
static EIDA_TOOLKIT_OPTIONS _eida_toolkit_options = { 0 };

// Function to safely extract the attribute values from a JSON object
ETSTATUS GetJsonAttribute(json_object *obj, char *attr_name,
    json_type attr_type, BOOL str_allocate, void **attr_value, int *value,
    unsigned int *attr_len);

// EIDA Toolkit license type enumeration
typedef enum _ETLICENSE_TYPE {
    LICENSE_TYPE_DEVELOPMENT = 1,
    LICENSE_TYPE_PRODUCTION = 2
} ETLICENSE_TYPE;

// Global Toolkit service handlers
static const TOOLKIT_SERVICE _toolkit_services[] = {
    { "Quit", NULL},
    { "Read Emirates ID Card public data", ReadPublicDataHandler },
    { "Check the validity of the Emirates ID Card", CheckCardStatusHandler },
    { "Export PKI certificates", ExportPKICertificatesHandler },
    { "Authenticate biometric On Server", AuthenticateBiometricOnServerHandler },
    { "Reset PIN", ResetPinHandler },
    { "PKI Authentication", AuthenticatePkiHandler },
    { "Unblock PIN", UnblockPinHandler },
    { "Read family book data", ReadFamilyBookDataHandler },
    { "Sign Data", SignDataHandler },
    { "Sign Challenge", SignChallengeHandler },
    { "RegisterDevice", RegisterDeviceHandler },
    { "XadesSign", XadesSignHandler},
    { "XadesVerify", XadesVerifyHandler },
    { "CadesSign", CadesSignHandler },
    { "CadesVerify", CadesVerifyHandler },
    { "PadesSign", PadesSignHandler },
    { "PadesVerify", PadesVerifyHandler },
    { "Read Emirates ID Card public data nfc", ReadPublicDataNfcHandler },
    { "Get Toolkit Version", GetToolkitVersionHandler },
    { "Get Interface type", GetInterfaceTypeHandler },
    { "Get Device ID", GetDeviceIDHandler },
    { "Check if Emirates ID Card is genuine", IsCardGenuineHandler },
    { "Verify Certificate using OCSP", VerifyCertificateUsingOCSPHandler },
    { "Authenticate biometric on server using NFC", AuthenticateBiometricOnServerNfcHandler }
};

// Global count of Toolkit services
static const int _toolkit_service_count = sizeof(_toolkit_services) /
sizeof(_toolkit_services[0]);

// Get the user choice of Toolkit service
TOOLKIT_SERVICE* GetUserChoice() {

    TOOLKIT_SERVICE *toolkit_service = NULL;
    int choice = 0;
    int i = 0;

    while (1) {
        printf("\n");
        printf("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");
        printf("Select one of the following Toolkit services\n");
        for (i = 1; i < _toolkit_service_count; i++) {
            printf("\t%d. %s\n", i, _toolkit_services[i].description);
        }
        printf("\t0. Quit\n");
        printf("Please enter your choice: ");
        scanf("%d", &choice);

        // Verify the user choice
        if ((choice < 0) || (choice >= _toolkit_service_count)) {
            printf("Please enter a valid choice\n");
            continue;
        }

        if (choice != 0) {
            toolkit_service = (TOOLKIT_SERVICE*)&_toolkit_services[choice];
        }

        printf("\n");
        break;
    }

    return toolkit_service;
}

// get executable path
BOOL GetExecutablePath(char *argv, char *out_path,
        unsigned int max_size) {

    char path[MAX_SIZE] = { 0 };
    char *ptr = NULL;

    // verify input parameters
    if (!argv || !out_path) {
        return FALSE;
    }

    strncpy(path, argv, sizeof (path));

    ptr = strrchr(path, '//');
    if (!ptr) {
        return FALSE;
    }
    *ptr = '\0';

    strncpy(out_path, path, max_size);

    return TRUE;
}

// Write data to file
ETSTATUS WriteToFile(const char *file_path, void *data,
    unsigned int data_len);

// Encode data in base64 format
ETSTATUS Base64Encode(const unsigned char *data, unsigned int data_length,
    unsigned char **encoded_data, unsigned int *encoded_data_length);

// Main entry point function of the Toolkit console
int main(int argc, char *argv[]) {

    ETSTATUS etstatus = ETSTATUS_SUCCESS;
    TOOLKIT_SERVICE *toolkit_service = NULL;
    BOOL result = FALSE;
    int ret_val = 0;
    unsigned int i = 0;
    char option = 0;
    int choice = 0;
    char *config_params_data = NULL;
    int len = 0;
    json_object *obj = NULL;

    // Begin function logic
    for (;;) {

        // Parse the command line arcguments
        result = ParseCommandLine(&_eida_toolkit_options, argc, argv);
        if (!result) {
            printf("Error in parsing command line inputs\n");
            break;
        }

#if !defined(_WIN32)
        // chdir() to the executable's own directory so that config files
        // (config_ap, config_ag_ap, config/, etc.) resolve regardless of
        // how the binary was launched. macOS Finder double-click sets CWD
        // to "/" (LaunchServices default), which would otherwise cause
        // config_ap to be read as /config_ap (missing) and toolkit
        // Initialize to fail. EIDAToolkitService does the same in
        // EsvServiceUnix.c.
        {
            char exe_path[PATH_MAX] = { 0 };
            char *last_slash = NULL;
#if defined(__APPLE__)
            uint32_t exe_path_size = (uint32_t)sizeof(exe_path);
            if (0 != _NSGetExecutablePath(exe_path, &exe_path_size)) {
                exe_path[0] = '\0';
            }
#elif defined(__linux__)
            ssize_t exe_path_len = readlink("/proc/self/exe",
                exe_path, sizeof(exe_path) - 1);
            if (exe_path_len > 0) {
                exe_path[exe_path_len] = '\0';
            }
#endif
            if (exe_path[0] != '\0') {
                last_slash = strrchr(exe_path, '/');
                if (NULL != last_slash) {
                    *last_slash = '\0';
                    if (0 != chdir(exe_path)) {
                        printf("Warning: Failed to chdir to "
                            "executable directory [%s]\n", exe_path);
                    }
                }
            }
        }
#endif

        // Read from config params file
        if (!ReadFromFile("config_ap", &config_params_data,
            &len)) {
            printf("Failed to read config_ap file."
                "Using default configuration parameters\n");
        }

        // Initialize the EIDA Toolkit
        etstatus = Initialize(_eida_toolkit_options.in_process_mode,
            config_params_data);
        if (ETSTATUS_SUCCESS != etstatus) {
            printf("Failed to initialize EIDAToolkit due to "
                "[Error:%u, \"%s\"]\n", etstatus,
                GetStatusMessage(etstatus));
            break;
        }

        // Display Toolkit services to user and execute the selected services
        for (;;) {

            // Display the Toolkit service menu and get the user choice
            toolkit_service = GetUserChoice();
            if (!toolkit_service) {
                printf("Exiting the EIDA Toolkit console\n");
                break;
            }

            // Execute the Toolkit service handler
            result = toolkit_service->ServiceHandler(&_eida_toolkit_options);
            printf("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");
        }

        // Cleanup EIDA Toolkit
        Cleanup();

        // End function logic
        break;
    }

#if defined(_WIN32) || defined(_WIN64)
    getch();
#endif
}

// Read the contents of a file
BOOL ReadFromFile(const char *file_name,
    unsigned char **file_buffer, unsigned int *file_length) {

    BOOL result = TRUE;
    FILE *file = NULL;
    unsigned long length = 0;
    unsigned long len_read = 0;
    char *buffer = NULL;

    // Begin function logic
    for (;;) {

        // Verify input parameters
        if (!file_name || !file_name[0] || !file_buffer || !file_length) {
            result = FALSE;
            break;
        }

        // Initialize the return parameters
        *file_buffer = NULL;
        *file_length = 0;

        // Open the specified file
        file = fopen(file_name, "rb");
        if (!file) {
            printf("Could not open the file.\n");
            result = FALSE;
            break;
        }

        // Set the file pointer to the end
        if (fseek(file, 0, SEEK_END)) {
            printf("Could not perform file seek operation.\n");
            result = FALSE;
            break;
        }

        // Get the length of the file
        length = ftell(file);
        if (!length) {
            printf("The file specified is empty.\n");
            result = FALSE;
            break;
        }

        // Allocate memory for the file data
        buffer = (char*)malloc(length + 1);
        if (!buffer) {
            printf("Could not allocate buffer to read the file.\n");
            result = FALSE;
            break;
        }

        // Set the file pointer to the beginning
        if (fseek(file, 0, SEEK_SET)) {
            printf("Could not perform file seek operation.\n");
            result = FALSE;
            break;
        }

        // Read the schema into buffer
        len_read = (unsigned long)fread(buffer, 1, length, file);
        if (len_read < length) {
            printf("Could not read from the file.\n");
            result = FALSE;
            break;
        }
        buffer[length] = '\0';

        // Return the file buffer
        *file_buffer = (unsigned char*)buffer;
        *file_length = length;

        // End funtion logic
        break;
    }

    // Function cleanup and return
    if (!result && buffer) {
        free(buffer);
    }

    // Close the file if opened
    if (file) {
        fclose(file);
    }

    return result;
}