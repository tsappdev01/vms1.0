//
//  NSObject+DeviceInfo.h
//  T5ClientSDKFramework iOS
//
//  Created by Dmitry Chirkin on 2/7/21.
//  Copyright © 2021 Tech5. All rights reserved.
//

//! Get device unique id
#if defined (__cplusplus)
extern "C"
{
#endif

__attribute__((visibility("default"))) const char* getFrameworkResourcePath();
__attribute__((visibility("default"))) int deregisterDevice();
__attribute__((visibility("default"))) const char* licenseChecker(const char* szLizense, int* retVal);
__attribute__((visibility("default"))) void setUrl(const char* licenseURL);
__attribute__((visibility("default"))) float getAmbientLightSensor();
__attribute__((visibility("default"))) const char* getDeviceID();
__attribute__((visibility("default"))) const char* getAppID();
#ifdef __cplusplus
}
#endif
