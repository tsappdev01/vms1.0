//
//  Header.h
//  SamplePlugin
//
//  Created by doti naresh on 20/03/19.
//  Copyright © 2019 DT. All rights reserved.
//

#ifndef Header_h
#define Header_h


// This enumeration defines the type of the plugin
typedef enum _PLUGIN_TYPE {
    FINGER_PRINT = 0,
    SCARD_READER = 1,
    COMBI_READER = 2
} PLUGIN_TYPE, *LPPLUGIN_TYPE;

// This enumeration defines fingerprint template type
typedef enum _FP_TEMPLATE_TYPE {
    ISO = 0,
    DINV = 1
} FP_TEMPLATE_TYPE, *LPFP_TEMPLATE_TYPE;

// Plugin related constants
#define PLUGIN_NAME_MAX                 64
#define DEVICE_NAME_MAX                 64
#define PLUGIN_DESC_MAX                 128
#define PLUGIN_CAPABILITY_AVAILABLE     1
#define PLUGIN_CAPABILITY_UNAVAILABLE   0
#define PLUGIN_CAPABILITY_NOT_SET       -1

// This structure contains the captured fingerprint data as
// ISO Compact Card template
typedef struct _FINGER_PRINT_DATA {
    unsigned char* value;
    int length;
} FINGER_PRINT_DATA;

// This structure contains the captured fingerprint data as
// raw image
typedef struct _FTP_IMAGE {
    unsigned char* pixels;
    int width;
    int height;
    int finger_index;
    int quality;
} FTP_IMAGE;

// This structure contains the captured fingerprint template data
typedef struct _FTP_TEMPLATE {
    int finger_index;
    int format; //ISO(0), DINV(1)
    int quality;
    FINGER_PRINT_DATA finger_print_data;
} FTP_TEMPLATE;

// Definition of plugin context structure.
typedef struct _PLUGIN_CONTEXT {
    int connection_handle;
} PLUGIN_CONTEXT;

typedef struct _ATR {
    unsigned char Atr[18];
    unsigned int length;
} ATR;


#endif /* Header_h */
