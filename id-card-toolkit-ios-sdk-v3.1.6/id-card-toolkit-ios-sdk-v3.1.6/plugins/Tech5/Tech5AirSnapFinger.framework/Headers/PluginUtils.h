//
//  PluginUtils.h
//  Tech5AirSnapFinger
//
//  Created by Federal Authority For Identity and Citizenship on 17/09/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <Header.h>
// Tech5 SDK 7.3.0: the umbrella T5FingerCapture framework no longer exists.
// We bridge to the new Swift-only T5FingerCaptureController via
// Tech5CaptureBridge.swift; the auto-generated bridge header is included
// from PluginUtils.m (where it's needed) so this public header stays
// clean of vendor framework dependencies.
NS_ASSUME_NONNULL_BEGIN

// Renamed from PluginUtils to avoid an ObjC runtime class-name collision
// with the NFC plugin's PluginUtils when both frameworks load in the same
// app (objc: "Class PluginUtils is implemented in both ..."). The class is
// internal to this plugin; the public surface is the C Plugin_*/FP_Capture
// entry points, so this rename is not customer-visible.
@interface Tech5PluginUtils : NSObject
-(id)initWithReader:(void *)context;
-(NSString *)getdevice;
-(int)configureReader;
-(int)getCardStatus;
-(int)connectDevice:(NSString *)reader;
-(int)disConnectDevice:(void *)plugInContext;
-(void *)getAtr:(void *)plugInContext;
-(int)executeCommand:(void *)plugin_context isocommand:(Byte *)isocommand command_length:(int)command_length out_buf:(Byte *)out_buf out_length:(uint32_t *)out_length interface_type:(int)interface_type;
-(void)freeMemory:(void *)buffer;
-(void)cleanUp;
-(int)fp_Plugin_Connect;
-(int)fp_Plugin_Disconnect;
-(int)fP_Capture:(int)finger_index time_out:(int)time_out image:(Byte **)image;
- (NSString *)pathForResource:(NSString *)name ofType:(NSString *)type;
@end

NS_ASSUME_NONNULL_END
