//
//  T5NistLibrary.h
//  T5AirSnapFramework iOS
//
//  Created by Mosunov Alexander on 6/7/22.
//  Copyright © 2022 Tech5. All rights reserved.
//

#ifndef T5NistLibrary_h
#define T5NistLibrary_h

#if defined(__cplusplus) || defined(__cplusplus__)
extern "C"
{
#endif

const char* T5Nist_getVersion();

int readNistFile( const char* fileNist, uint8_t *t5nist);

int saveToNistFile(const char* fileNist, uint8_t t5nist);

int getWsqDimensions(uint8_t* theWsqImage, int* width, int* height);

int saveT5ToNistFile(int is_FBI, int code[], char** demo, int demo_count, struct SgmRectImageEx fingers[], int32_t fingers_count, struct ImageData faces[], int32_t faces_count, struct SgmRectImageEx tenprints[], int32_t tenprints_count, uint8_t* retBuffer, int* length_retBuffer);

#if defined(__cplusplus) || defined( __cplusplus__)
}
#endif


#endif /* T5NistLibrary_h */
