//
//  FingerprintScannerProtocol.h
//  Grabba
//
//  Created by Jordan Comino on 8/5/20.
//  Copyright © 2020 Grabba. All rights reserved.
//

#ifndef IPlugin_h
#define IPlugin_h

typedef void(^ScanComplete)(void);

#define STATUS_SUCCESS           0
#define STATUS_INVALID_PARAMETER 2
#define STATUS_INVALID_CONTEXT   252

@class FingerprintImage;

@protocol IPlugin

-(int) Initalise;
-(int) Cleanup;
-(int) GetInfo:(PLUGIN_INFO*)pluginInfo;
-(int) FingerprintConnect;
-(int) FingerprintDisconnect;
-(int) FingerprintCapture:(FingerprintImage*)image within:(int)timeout;
-(int) FingerprintCaptureAndConvert:(FTP_TEMPLATE*)ftp_template within:(int)timeout;
-(int) SmartcardConnect:(char*)reader with:(void**)context;
-(int) SmartcardDisconnect:(void*)context;
-(int) SmartcardGetATR:(void*)context atr:(unsigned char**)data length:(unsigned int*)length;
-(int) SmartcardExecuteCommand:(void*)plugin_context apduRequest:(unsigned char*)apduRequest requestLength:(unsigned int)request_length apduResponse:(unsigned char*)apduResponse responseLength:(unsigned int*)response_length interfaceType:(int)interface_type;

@end

#endif /* IPlugin_h */
