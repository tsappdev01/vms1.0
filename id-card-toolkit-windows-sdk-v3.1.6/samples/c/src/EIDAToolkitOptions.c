// Standard C header files
#include <stdio.h>
#include <string.h>
#include <ctype.h>
#include <stdlib.h>

// EIDA Toolkit header files
#include <EIDAToolkitTypes.h>
#include <EIDAToolkitError.h>
#include <EIDAToolkitApi.h>

// Project local header files
#include "EIDAToolkitOptions.h"

// Global EIDAToolkit command options mapping
static const COMMAND_OPTIONS _toolkit_cmd_options[] = {
    { 'l', CMD_OPTION_LICENSE, "Toolkit License File Path", FALSE, "1", SetLicense },
    { 'i', CMD_OPTION_INPROC_MODE, "In-process Mode", FALSE, "1", SetInProcessMode },
    { 'c', CMD_OPTION_SDK_MODE, "Client SDK Mode", FALSE, "1", SetSDKMode }
};

// Global EIDAToolkit command options count
static const int _toolkit_cmd_options_count =
sizeof(_toolkit_cmd_options) / sizeof(_toolkit_cmd_options[0]);

// Parse command line arguments
BOOL ParseCommandLine(EIDA_TOOLKIT_OPTIONS *toolkit_options,
    int argc, char *argv[]) {

    COMMAND_PARSER_STATE parser_state = CMD_OPTION;
    COMMAND_OPTIONS *cmd_option = NULL;
    int i = 0;
    int j = 0;
    char option = 0;
    BOOL result = TRUE;

    // Verify input parameters
    if (!toolkit_options) {
        printf("Invalid address passed to command line parser.\n");
        return FALSE;
    }

    // Parse the command line arguments
    for (i = 1; i < argc; i++) {

        // If the command value is expected then invoke the setter
        if (parser_state == CMD_VALUE) {

            // Verify if the option is not already set
            if (!((toolkit_options->bit_mask >> cmd_option->option_bit) & 1)) {

                result = cmd_option->SetOption(toolkit_options, argv[i]);
                if (!result) {
                    printf("Could not set the option for \"%s\" [-%c]\n",
                        cmd_option->option_name, cmd_option->option);
                    result = FALSE;
                    break;
                }

                // Set the bit mask for the option
                toolkit_options->bit_mask |= (1 << cmd_option->option_bit);
            }

            cmd_option = NULL;
            parser_state = CMD_OPTION;
            continue;
        }

        // Check if the option specifier is provided
        if (argv[i][0] != '-') {
            printf("Invalid option specifier found.\n");
            result = FALSE;
            break;
        }

        // Check if the option is incomplete
        option = argv[i][1];
        if (!option) {
            printf("Invalid option code specified.\n");
            break;
        }

        // Iterate through the Toolkit option mapping
        for (j = 0; j < _toolkit_cmd_options_count; j++) {

            // Match the command line option with option map
            if (_toolkit_cmd_options[j].option == tolower(option)) {
                cmd_option = (COMMAND_OPTIONS *)&_toolkit_cmd_options[j];
                parser_state = CMD_VALUE;
                break;
            }
        }

        // If option not found then quit
        if (!cmd_option) {
            printf("Invalid commandline option [-%c] detected.\n", option);
            result = FALSE;
            break;
        }
    }

    // In case of error return
    if (!result) {
        return result;
    }

    // If any options not provided set the default value
    for (i = 0; i < _toolkit_cmd_options_count; i++) {
        cmd_option = (COMMAND_OPTIONS *)&_toolkit_cmd_options[i];

        // Verify if the option is set
        if ((toolkit_options->bit_mask >> cmd_option->option_bit) & 1) {
            continue;
        }

        // Verify if the option is mandatory
        if (cmd_option->is_mandatory) {
            printf("Command line option \"%s\" [-%c] is mandatory.\n",
                cmd_option->option_name, cmd_option->option);
            result = FALSE;
            break;
        }

        // Set the default option
        result = cmd_option->SetOption(toolkit_options,
            (char *)cmd_option->default_value);
        if (!result) {
            printf("Could not set the default option for \"%s\" [-%c]\n",
                cmd_option->option_name, cmd_option->option);
            result = FALSE;
            break;
        }

        // Set the bit mask for the option
        toolkit_options->bit_mask |= (1 << cmd_option->option_bit);
    }

    return TRUE;
}

// Set the Toolkit Secure Messaging option
BOOL SetSMMode(EIDA_TOOLKIT_OPTIONS *options, char *value) {

    if (!options || !value) {
        return FALSE;
    }

    int sm_mode = atoi(value);
    if ((sm_mode < 1) || (sm_mode > 3)) {
        printf("Invalid Secure Messaging Mode specified in commandline\n");
        return FALSE;
    }

    options->sm_mode = sm_mode;
    return TRUE;
}

// Set the Validation Gateway (VG) proxy address
BOOL SetVGAddress(EIDA_TOOLKIT_OPTIONS *options, char *value) {

    if (!options || !value) {
        return FALSE;
    }

    memset(options->vg_address, 0, sizeof(options->vg_address));
    strncpy(options->vg_address, value, sizeof(options->vg_address) - 1);

    return TRUE;
}

// Set the license file path
BOOL SetLicense(EIDA_TOOLKIT_OPTIONS *options, char *value) {

    if (!options || !value) {
        return FALSE;
    }

    options->license_type = atoi(value);
    if ((options->license_type != 1) && (options->license_type != 2)) {
        printf("Invalid license type specified in commandline\n");
        return FALSE;
    }

    return TRUE;
}

BOOL SetInProcessMode(EIDA_TOOLKIT_OPTIONS *options, char *value) {

    if (!options || !value) {
        return FALSE;
    }

    int in_process_mode = atoi(value);
    if ((in_process_mode != 0) && (in_process_mode != 1)) {
        printf("Invalid InProcess Mode specified in commandline\n");
        return FALSE;
    }

    options->in_process_mode = in_process_mode;
    return TRUE;
}

BOOL SetSDKMode(EIDA_TOOLKIT_OPTIONS *options, char *value) {

    if (!options || !value) {
        return FALSE;
    }

    int client_sdk_mode = atoi(value);
    if (client_sdk_mode == 0) {
        printf("Invalid InProcess Mode specified in commandline\n");
        return FALSE;
    }

    options->client_sdk_mode = client_sdk_mode;
    return TRUE;
}