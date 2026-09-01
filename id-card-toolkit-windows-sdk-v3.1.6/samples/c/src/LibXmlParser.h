#include <stdlib.h>
#include <string.h>


#include <libxml/xmlmemory.h>
#include <libxml/parser.h>
#include <libxml/xmlwriter.h>
#include <libxml/xpath.h>
#include <libxml/tree.h>

//
// Error Codes
//
#define INVALID_ARGUMENT 0x80070001
#define INSUFFICIENT_MEMORY 0x80070002
#define FAILED_SET_URL 0x80070003
#define FAILED_HTTP_GET 0x80070004
#define ERROR_INVALID_RESPONSE 0x80070005
#define ERR_XML_KEY_VAL_NULL 0x80070006
#define ERR_INVALID_XML_OBJECT_HANDLE 0x80070007
#define ERR_XML_KEY_NOT_FOUND 0x80070008
#define ERR_INVALID_XML_KEY_HANDLE 0x80070009
#define ERR_PARSE_ERROR 0x80070010
#define ERROR_XML_MISSINGROOT 0x80070012
#define ERROR_XML_MISSINGPARENT 0x80070013
#define ERROR_XML_DUMPMEM 0x80070014

//
// Constants
//
#define MAX_PATH 260
#define NAME_SIZE 50

//
// A structure to define the xml object handle.
//
typedef struct _XML_OBJ_HANDLE_
{
    xmlDocPtr lpXmlDoc;
} XML_OBJ_HANDLE, *LPXML_OBJ_HANDLE;

//
// A structure to define the xml key handle.
//
typedef struct _XML_KEY_HANDLE_
{
    xmlNode* lpXmlNode;
} XML_KEY_HANDLE, *LPXML_KEY_HANDLE;

//
// Gives all the sub key names under the specified key
//
typedef struct _KEY_NAMES
{
    char szKeyName[NAME_SIZE];
} KEY_NAMES, *LPKEY_NAMES;

unsigned int LoadXML(char* lpszXml, void** lplphXMLObj);

unsigned int UnLoadXML(void* plphXMLObj);

unsigned int SeperateKeyNames(char* lpszKeyPath, unsigned int dwPathLen,
    LPKEY_NAMES* lplpKeyNames, unsigned int* lpdwNoOfKeys);

unsigned int GetXmlKey(void* lphXMLObj, LPKEY_NAMES lpKeyNames,
    unsigned int dwNoOfKeys, xmlNode** lplpTargetXmlKey);

unsigned int SetKeyValue(void* lphXMLObj, char* lpszKeyPath,
    unsigned int dwKeyPathLen, char* lpszValue, unsigned int dwValueLen);

unsigned int GetXMLString(void* lphXMLObj, char** lplpszXml,
    unsigned int* lpdwXmlLen);

unsigned int LibXmlFree(void* lpvArg);

unsigned int GetKeyValue(void* lphXMLObj, char* lpszKeyPath,
    unsigned int   dwKeyPathLen, char** lplpszValue,
    unsigned int*  lpdwValueLen);

// This function gets doc object from node
unsigned int GetDocFromNode(void *lphXMLObj, char *path,
    void **lp_NewXml_Obj);