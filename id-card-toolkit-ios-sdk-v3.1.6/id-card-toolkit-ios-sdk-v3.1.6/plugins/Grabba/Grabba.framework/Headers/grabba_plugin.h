#ifndef PLUGIN_PLUGIN_H
#define PLUGIN_PLUGIN_H

#define FALSE 0
#define TRUE  1

typedef union _PLUGIN_VERSION {
    struct {
        unsigned int major    : 8;
        unsigned int minor    : 8;
        unsigned int patch    : 8;
        unsigned int reserved : 8;
    } v;
    unsigned int version;
} PLUGIN_VERSION;

typedef enum _PLUGIN_TYPE {
    FINGER_PRINT = 0,
    SCARD_READER = 1
} PLUGIN_TYPE;

#define PLUGIN_NAME_MAX 64
#define DEVICE_NAME_MAX 64
#define PLUGIN_DESC_MAX 128

typedef struct _PLUGIN_INFO {
    PLUGIN_VERSION plugin_version;
    int raw_image;
    int capture_convert;
    int contactless;
    int contact;
    PLUGIN_TYPE plugin_type;
    char plugin_name[PLUGIN_NAME_MAX];
    char reader_name[DEVICE_NAME_MAX];
    char plugin_desc[PLUGIN_DESC_MAX];
} PLUGIN_INFO;

#define STATUS_SUCCESS           0
#define STATUS_INVALID_PARAMETER 2
#define STATUS_INVALID_CONTEXT   252

typedef struct _FTP_IMAGE {
	unsigned char *pixels;
	int width;
	int height;
	int finger_index;
} FTP_IMAGE;

typedef struct _FINGER_PRINT_DATA {
	unsigned char *value;
	int length;
} FINGER_PRINT_DATA;

typedef struct _FTP_TEMPLATE {
	int finger_index;
	int format;
	FINGER_PRINT_DATA finger_print_data;
} FTP_TEMPLATE;

int Plugin_Initialize(void *context);
int Plugin_Cleanup(void);
int Plugin_GetInfo(PLUGIN_INFO *plugin_info);
int Plugin_FreeMemory(void *buffer);

#endif //PLUGIN_PLUGIN_H
