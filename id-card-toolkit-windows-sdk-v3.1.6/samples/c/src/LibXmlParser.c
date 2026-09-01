#include "LibXmlParser.h"

// This function loads the given xml string into the object and returns it's
// handle. This handle should be used for other operations on that xml.
unsigned int LoadXML(char* lpszXml, void** lplphXMLObj) {

    unsigned int return_value = 0;
    xmlDocPtr lpXmlDoc = NULL;

    // Begin function logic
    for (;;) {

        lpXmlDoc = xmlParseDoc((const xmlChar*)lpszXml);

        if (NULL == lpXmlDoc) {
            return_value = ERR_PARSE_ERROR;
            break;
        }

        *lplphXMLObj = (void*)malloc(sizeof(XML_OBJ_HANDLE));
        if (NULL == *lplphXMLObj) {
            return_value = INSUFFICIENT_MEMORY;
        }

        // Store the handle.
        ((LPXML_OBJ_HANDLE)(*lplphXMLObj))->lpXmlDoc = lpXmlDoc;

        // End of function logic
        break;
    }

    return return_value;
}

// This function unloads the given xml string into the object and returns it's
// handle. This handle should be used for other operations on that xml.
unsigned int UnLoadXML(void* plphXMLObj) {
    unsigned int return_value = 0;

    // Begin function logic
    for (;;) {

        // Input Parameter validation
        if (NULL == plphXMLObj) {
            return_value = INVALID_ARGUMENT;
            break;
        }

        xmlFreeDoc(((LPXML_OBJ_HANDLE)(plphXMLObj))->lpXmlDoc);

        // End of function logic
        break;
    }

    if (NULL != plphXMLObj) {
        free(plphXMLObj);
        plphXMLObj = NULL;
    }

    return return_value;
}

// This function seperate the key names in the given path and store them
// in an array
unsigned int SeperateKeyNames(char* lpszKeyPath, unsigned int dwPathLen,
    LPKEY_NAMES* lplpKeyNames, unsigned int* lpdwNoOfKeys) {

    unsigned int return_value = 0;
    unsigned int names_count = 0;
    unsigned int i = 0;
    unsigned int dwIndex = 0;
    unsigned int  dwNameIndex = 0;

    // Begin function logic
    for (;;) {

        // Input parameter validation
        if ((0 == dwPathLen) || (NULL == lplpKeyNames) ||
            (NULL == lpdwNoOfKeys)) {
            return_value = INVALID_ARGUMENT;
            break;
        }

        // This loop gets the number of keys in the path
        for (i = 0; (i < dwPathLen && lpszKeyPath[i] != '\0'); i++) {
            // Look for the name seperator
            if ('\\' == lpszKeyPath[i]) {
                // Increment the size
                names_count++;
            }
        }

        // Increment it for the final name
        names_count++;

        // Array size is equal to the number of names
        // Store the array size in the out put parameter
        *lpdwNoOfKeys = names_count;

        // Allocate the memory to the return array
        *lplpKeyNames = (LPKEY_NAMES)malloc(names_count * sizeof(KEY_NAMES));
        if (NULL == *lplpKeyNames) {
            return_value = INSUFFICIENT_MEMORY;
            break;
        }

        for (i = 0; (i < dwPathLen && lpszKeyPath[i] != '\0'); i++) {
            // Look for the name seperator
            if ('\\' == lpszKeyPath[i]) {
                // Append the null character and increment the array index
                (*lplpKeyNames)[dwIndex].szKeyName[dwNameIndex] = '\0';
                dwIndex++;

                // Re intialize the values for the next iteration
                dwNameIndex = 0;
            }
            else {
                // Store the character in the return buffer
                (*lplpKeyNames)[dwIndex].szKeyName[dwNameIndex]
                    = lpszKeyPath[i];
                dwNameIndex++;
            }
        }

        // Append the null character to the last entry in the array
        (*lplpKeyNames)[dwIndex].szKeyName[dwNameIndex] = '\0';
        dwIndex++;

        // End of function logic
        break;
    }

    return return_value;
}

// This function gets the specified xml key.
unsigned int GetXmlKey(void* lphXMLObj, LPKEY_NAMES lpKeyNames,
    unsigned int dwNoOfKeys, xmlNode** lplpTargetXmlKey) {

    unsigned int return_value = 0;
    xmlNode* parent_Node = NULL;
    xmlNode* cur_node = NULL;
    int iIndex = 0;
    int i = 0;

    // Begin function logic
    for (;;) {

        // Input parameter validation
        if ((NULL == lphXMLObj) || (0 == dwNoOfKeys) ||
            (NULL == lpKeyNames) || (NULL == lplpTargetXmlKey)) {
            return_value = INVALID_ARGUMENT;
            break;
        }

        // Try to get the root element.
        parent_Node =
            xmlDocGetRootElement(((LPXML_OBJ_HANDLE)(lphXMLObj))->lpXmlDoc);

        if (NULL == parent_Node) {
            return_value = ERROR_XML_MISSINGROOT;
            break;
        }

        while ((i < (int)dwNoOfKeys) && (NULL != parent_Node)) {
            if ((XML_ELEMENT_NODE == parent_Node->type) &&
                (0 == xmlStrcmp((parent_Node->name),
                (const xmlChar*)lpKeyNames[i].szKeyName))) {
                cur_node = parent_Node;
                parent_Node = parent_Node->children;
                i++;
            }
            else {
                // Go to next node.
                parent_Node = parent_Node->next;
            }
        }

        if ((NULL == cur_node) || (i != dwNoOfKeys)) {
            return_value = ERROR_XML_MISSINGPARENT;
            break;
        }

        // Store the target node in out put argument.
        *lplpTargetXmlKey = cur_node;

        // End of function logic
        break;
    }

    return return_value;
}

// This function sets the specified xml key value.
unsigned int SetKeyValue(void* lphXMLObj, char* lpszKeyPath,
    unsigned int dwKeyPathLen, char* lpszValue, unsigned int dwValueLen) {

    unsigned int return_value = 0;
    LPKEY_NAMES lpKeyNames = NULL;
    unsigned int dwNoOfKeys = 0;
    xmlNode* lpTargetXmlKey = NULL;
    xmlChar* lpszXmlVal = NULL;

    // Begin function logic
    for (;;) {

        // Input parameter validation
        if ((0 == dwKeyPathLen) || (NULL == lpszKeyPath) ||
            (NULL == lphXMLObj) || (0 == dwValueLen) || (NULL == lpszValue)) {
            return_value = INVALID_ARGUMENT;
            break;
        }

        // Get the each key name in the path.
        return_value = SeperateKeyNames(lpszKeyPath, dwKeyPathLen,
            &lpKeyNames, &dwNoOfKeys);
        if (0 != return_value || NULL == lpKeyNames || 0 == dwNoOfKeys) {
            break;
        }

        // Now traverse through the path and get the target xml key.
        return_value = GetXmlKey(lphXMLObj, lpKeyNames,
            dwNoOfKeys, &lpTargetXmlKey);
        if (0 != return_value || NULL == lpTargetXmlKey) {
            break;
        }

        // Set the value.
        xmlNodeSetContent(lpTargetXmlKey, (const xmlChar*)lpszValue);

        // End of function logic
        break;
    }

    if (NULL != lpTargetXmlKey) {
        free(lpszXmlVal);
        lpszXmlVal = NULL;
    }

    if (NULL != lpKeyNames) {
        free(lpKeyNames);
    }

    return return_value;
}

// This function gets the current xml as string.
unsigned int GetXMLString(void* lphXMLObj, char** lplpszXml,
    unsigned int* lpdwXmlLen) {

    unsigned int return_value = 0;
    xmlDocPtr lpXmlDoc = NULL;
    xmlChar* lpszXmlDump = NULL;
    int iDumpSize = 0;

    // Begin function logic
    for (;;) {

        // Input parameter validation
        if ((NULL == lphXMLObj) || (NULL == lpdwXmlLen) ||
            (NULL == lplpszXml)) {
            return_value = INVALID_ARGUMENT;
            break;
        }

        lpXmlDoc = ((LPXML_OBJ_HANDLE)(lphXMLObj))->lpXmlDoc;

        // Dump the xml document.
        xmlDocDumpMemory(lpXmlDoc, &lpszXmlDump, &iDumpSize);
        if (NULL == lpszXmlDump || 0 == iDumpSize) {
            return_value = ERROR_XML_DUMPMEM;
            break;
        }

        *lpdwXmlLen = (iDumpSize + 1) * (sizeof(char));

        // Allocate the memory.
        *lplpszXml = (char*)malloc(*lpdwXmlLen);
        if (NULL == *lplpszXml) {
            return_value = INSUFFICIENT_MEMORY;
            break;
        }
        memset(*lplpszXml, 0, *lpdwXmlLen);

        // Copy the xml.
        strncpy(*lplpszXml, (const char *)lpszXmlDump, *lpdwXmlLen);

        // End of function logic
        break;
    }

    if (NULL != lpszXmlDump) {
        free(lpszXmlDump);
        lpszXmlDump = NULL;
    }

    return return_value;
}

// This function frees the memory of the given argument.
unsigned int LibXmlFree(void* lpvArg) {

    unsigned int return_value = 0;

    // Begin function logic
    for (;;) {
        // Input parameter validation
        if (NULL == lpvArg) {
            return_value = INVALID_ARGUMENT;
            break;
        }

        // Free the memory.
        free(lpvArg);

        // End of function logic
        break;
    }

    return return_value;
}

// This function gets the specified xml key value.
unsigned int GetKeyValue(void* lphXMLObj, char* lpszKeyPath,
    unsigned int   dwKeyPathLen, char** lplpszValue,
    unsigned int*  lpdwValueLen) {

    unsigned int return_value = 0;
    LPKEY_NAMES lpKeyNames = NULL;
    unsigned int dwNoOfKeys = 0;
    xmlNode* lpTargetXmlKey = NULL;
    xmlChar* lpszXmlVal = NULL;

    // Begin function logic
    for (;;) {
        // Input parameter validation
        if ((0 == dwKeyPathLen) || (NULL == lpszKeyPath) ||
            (NULL == lphXMLObj) || (NULL == lpdwValueLen) ||
            (NULL == lplpszValue)) {
            return_value = INVALID_ARGUMENT;
            break;
        }

        // Get the each key name in the path.
        return_value = SeperateKeyNames(lpszKeyPath, dwKeyPathLen,
            &lpKeyNames, &dwNoOfKeys);
        if (0 != return_value || NULL == lpKeyNames || 0 == dwNoOfKeys) {
            break;
        }

        // Now traverse through the path and get the target xml key.
        return_value = GetXmlKey(
            lphXMLObj,
            lpKeyNames,
            dwNoOfKeys,
            &lpTargetXmlKey
        );
        if (0 != return_value || NULL == lpTargetXmlKey) {
            break;
        }

        // Get the value.
        lpszXmlVal = xmlNodeGetContent(lpTargetXmlKey);
        if (NULL == lpszXmlVal) {
            return_value = ERR_XML_KEY_VAL_NULL;
        }

        *lpdwValueLen = (xmlStrlen(lpszXmlVal)) * (sizeof(char));

        // Allocate the memory.
        *lplpszValue = (char*)malloc(*lpdwValueLen + 1);
        if (NULL == *lplpszValue) {
            return_value = INSUFFICIENT_MEMORY;
            break;
        }

        memset(*lplpszValue, 0, *lpdwValueLen + 1);

        // Copy the key value.
        strncpy(*lplpszValue, (const char*)lpszXmlVal, *lpdwValueLen + 1);

        // End of function logic
        break;
    }

    // Clean up
    if (NULL != lpTargetXmlKey) {
        free(lpszXmlVal);
        lpszXmlVal = NULL;
    }

    if (NULL != lpKeyNames) {
        free(lpKeyNames);
    }

    return return_value;
}

// This function gets doc object from node
unsigned int GetDocFromNode(void *lphXMLObj, char *path,
    void **lp_NewXml_Obj) {

    unsigned int return_value = 0;
    LPKEY_NAMES lpKeyNames = NULL;
    unsigned int dwNoOfKeys = 0;
    xmlNode* lpTargetXmlKey = NULL;
    xmlChar* lpszXmlVal = NULL;
    LPXML_OBJ_HANDLE xml_obj = NULL;
    xmlBufferPtr buffer = NULL;

    //char *value = NULL;

    // Begin function logic
    for (;;) {

        // Input parameter validation
        if (!lphXMLObj || !path || !path[0] || !lp_NewXml_Obj) {
            return_value = INVALID_ARGUMENT;
            break;
        }

        // Get the each key name in the path.
        return_value = SeperateKeyNames(path, strlen(path),
            &lpKeyNames, &dwNoOfKeys);
        if (0 != return_value || NULL == lpKeyNames || 0 == dwNoOfKeys) {
            break;
        }

        // Now traverse through the path and get the target xml key.
        return_value = GetXmlKey(lphXMLObj, lpKeyNames,
            dwNoOfKeys, &lpTargetXmlKey);
        if (0 != return_value || NULL == lpTargetXmlKey) {
            break;
        }

        buffer = xmlBufferCreate();
        int size = xmlNodeDump(buffer,
            ((LPXML_OBJ_HANDLE)(lphXMLObj))->lpXmlDoc, lpTargetXmlKey, 0, 1);

        return_value = LoadXML(buffer->content, (void**)&xml_obj);
        if (0 != return_value) {
            break;
        }

        *lp_NewXml_Obj = xml_obj;

        // End function logic
        break;
    }

    // Clean up
    if (NULL != lpTargetXmlKey) {
        free(lpszXmlVal);
        lpszXmlVal = NULL;
    }

    if (NULL != lpKeyNames) {
        free(lpKeyNames);
    }

    return return_value;
}











