//
//  ToolkitAssert.h
//  EIDAToolkit
//
//  Created by Federal Authority For Identity and Citizenship on 21/11/17.
//  Copyright © 2017 Emirates Identity Authority. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CardReader.h"
@interface ToolkitAssert : NSObject

+(void)notNullOrEmpty:(NSString *)argumentValue argumentName:(NSString *)argumentName;
+(void)notNegativeOrNotZero:(int)argumentValue argumentName:(NSString *)argumentName;
+(void)notNullOrEmptyData:(UInt8 *)argumentValue argumentValueLength:(int)argumentValueLength argumentName:(NSString *)argumentName;
+(void)notNull:(NSObject *)argumentValue argumentName:(NSString *)argumentName;
+(void)success:(int)code;
+(void)success:(int)code buffer:(char *)buffer bufferLength:(int)bufferLength;
+(void)success:(int)code attemptsLeft:(int)attemptsLeft;
+(void)success:(int)code isAttemptsLeft:(BOOL)isAttemptsLeft value:(int)value buffer:(char *)buffer bufferLength:(int)bufferLength;
+(void)connected:(CardReader *)cardReader;

@end
