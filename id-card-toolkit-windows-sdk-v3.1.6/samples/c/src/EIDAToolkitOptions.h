#ifndef EIDA_TOOLKIT_OPTIONS_H_
#define EIDA_TOOLKIT_OPTIONS_H_

// Commandline options bit flags
#define CMD_OPTION_SM_MODE      0
#define CMD_OPTION_VG_ADDRESS   1
#define CMD_OPTION_LICENSE      2
#define CMD_OPTION_INPROC_MODE  3
#define CMD_OPTION_SDK_MODE     4

// Command line parser states
typedef enum _COMMAND_PARSER_STATE {
    CMD_OPTION = 0,
    CMD_VALUE
} COMMAND_PARSER_STATE;

// EIDAToolkit options
typedef struct _EIDA_TOOLKIT_OPTIONS {
    unsigned int bit_mask;
    int sm_mode;
    char vg_address[512];
    BOOL in_process_mode;
    int client_sdk_mode;
    int license_type;
} EIDA_TOOLKIT_OPTIONS;

// Option setter callback function
typedef BOOL(*PFN_SET_OPTION)(EIDA_TOOLKIT_OPTIONS *, char *);

// Command line options reference
typedef struct _COMMAND_OPTIONS {
    char option;
    unsigned int option_bit;
    const char *option_name;
    BOOL is_mandatory;
    const char *default_value;
    PFN_SET_OPTION SetOption;
} COMMAND_OPTIONS;

// Parse command line arguments
BOOL ParseCommandLine(EIDA_TOOLKIT_OPTIONS *toolkit_options,
    int argc, char *argv[]);

// EIDAToolkit options setters
BOOL SetSMMode(EIDA_TOOLKIT_OPTIONS *options, char *value);
BOOL SetVGAddress(EIDA_TOOLKIT_OPTIONS *options, char *value);
BOOL SetLicense(EIDA_TOOLKIT_OPTIONS *options, char *value);
BOOL SetInProcessMode(EIDA_TOOLKIT_OPTIONS *options, char *value);
BOOL SetSDKMode(EIDA_TOOLKIT_OPTIONS *options, char *value);

#endif