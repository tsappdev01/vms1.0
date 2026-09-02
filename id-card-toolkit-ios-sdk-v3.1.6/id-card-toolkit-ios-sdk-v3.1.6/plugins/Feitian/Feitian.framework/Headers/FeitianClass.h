//
//  Feitian.h
//  Feitian
//
//  Created  on 12/20/16.
//  Copyright © 2016 Federal Authority For Identity and Citizenship All rights reserved.
//

#if defined EIDASDK_EXPORTS
#define EIDASDK_API __declspec(dllexport)
#else
#define EIDASDK_API __declspec(dllexport)
#endif

#if defined __cplusplus
extern "C" {
#endif
    
#ifndef __EIDA_TOOLKIT_PCSC_PLUGIN_H__
#define __EIDA_TOOLKIT_PCSC_PLUGIN_H__
    
    /** @brief Gets the list of connected readers' names.
     *
     * @param[out] reader_list Buffer to store the List of reader names.
     * @param[out] num_bytes Number of bytes in the reader list string.
     * @return Return value (SUCCESS / ERROR codes).
     */
    int Plugin_ListReaders(char **reader_list, unsigned int *num_bytes);
    
    /** @brief Execute smartcard command
     *
     * @param[in] plugin_context PCSC plugin context
     * @param[in] isocommand ISO command bytes.
     * @param[in] command_length Length of the ISO command bytes.
     * @param[out] out_buf Output buffer.
     * @param[out] out_length Number of bytes copied to the output buffer.
     * @param[in] interface_type Reader interface type.
     * @return Return value (SUCCESS / ERROR codes).
     */
    int Plugin_ExecuteCommand(void *plugin_context, unsigned char *isocommand,
                              unsigned int command_length, unsigned char *out_buf,
                              unsigned int *out_length, int interface_type);
    
    /** @brief Connect Smartcard with the reader
     *
     * @param[in] reader_name Name of the smartcard reader
     * @param[out] plugin_context Return pointer to the plugin context
     * @return Return value (SUCCESS / ERROR codes).
     */
    int Plugin_Connect(char *reader_name, void **plugin_context);
    
    /** @brief Disconnect the smartcard
     *
     * @param[in] plugin_context Plugin specific connection context
     * @return Return value (SUCCESS / ERROR codes).
     */
    int Plugin_Disconnect(void *plugin_context);
    
    /** @brief Get plugin information
     *
     * @param[out] plugin_info Return plugin information
     * @return Return value (SUCCESS / ERROR codes).
     */
    //    int Plugin_GetInfo(PLUGIN_INFO *plugin_info);
    
    /**
     * @brief Free the memory buffer allocated by this module
     *
     * @param[in] buffer Pointer to the memory bufer to free
     * @return Return value (ETSTATUS_SUCCESS on success else error code)
     */
    int Plugin_FreeMemory(void *buffer);
    
    /** @brief Gets the Answer to Reset (ATR) bytes of the connected card
     *
     * @param[in] plugin_context Pointer to the plugin specific context
     * @param[out] atr_bytes Smart Card ATR Value.
     * @param[out] atr_len Smart Card ATR Value Length.
     * @return Return value (SUCCESS / ERROR codes).
     */
    int Plugin_GetATR(void *plugin_context, unsigned char **atr_bytes,
                      unsigned int *atr_len);
    
    
#endif // __EIDA_TOOLKIT_PCSC_PLUGIN_H__
    
    
#if defined __cplusplus
}
#endif


#import <Foundation/Foundation.h>

@interface FeitianClass : NSObject

@end
