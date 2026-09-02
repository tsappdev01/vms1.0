#ifndef EIDA_TOOLKIT_PLUGIN_UTILITIES_H_
#define EIDA_TOOLKIT_PLUGIN_UTILITIES_H_

#define PLUGIN_UTILS_SUCCESS    0
#define PLUGIN_UTILS_ERROR      1

#ifdef __cplusplus
#define PLUGINAPI extern "C"
#else
#define PLUGINAPI
#endif

// Calculate the address of the base of the structure given its type, and an
// address of a field within the structure.
#ifndef CONTAINING_RECORD
#define CONTAINING_RECORD(address, type, field) \
((type *)((char *)(address) - (char *)(&((type *)0)->field)))
#endif

/**
 * @brief Function to allocate memory
 *
 * @param[int] size size to allocate
 * @param[out] buffer Pointer to the memory bufer
 * @return Return value (PLUGIN_UTILS_SUCCESS on success else error)
 */
PLUGINAPI int PluginAllocMemory(unsigned int size, void **buffer);

/**
 * @brief Free the memory buffer allocated by this module
 *
 * @param[in] buffer Pointer to the memory buffer to free
 * @return Return value (PLUGIN_UTILS_SUCCESS on success else error)
 */
PLUGINAPI int PluginFreeMemory(void *buffer);

#endif // EIDA_TOOLKIT_PLUGIN_UTILITIES_H_
