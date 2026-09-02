//
//  PluginUtils.h
//  Nefcom
//
//  Created by Federal Authority For Identity and Citizenship on 17/09/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <CoreNFC/CoreNFC.h>


NS_ASSUME_NONNULL_BEGIN


@interface PluginUtils : NSObject

-(NSString *)getdevice;
-(int)connectNFC:(NFCTagReaderSession *)session tag:(id<NFCTag>)tag  API_AVAILABLE(ios(13.0));
-(int)disConnectNFC:(id)session;
-(void *)getAtr:(id)tag version:(int *)version;
-(int)executeCommandToNFC:(id)tag isocommand:(Byte *)isocommand command_length:(int)command_length out_buf:(Byte *)out_buf out_length:(uint32_t *)out_length interface_type:(int)interface_type;
-(void)freeMemory:(void *)buffer;

@end

NS_ASSUME_NONNULL_END
