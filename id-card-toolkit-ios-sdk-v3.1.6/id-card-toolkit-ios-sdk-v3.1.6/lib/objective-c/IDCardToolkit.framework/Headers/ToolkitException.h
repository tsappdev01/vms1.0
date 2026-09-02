//
//  ToolkitException.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 16/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface ToolkitException : NSException

+(ToolkitException *)foundexception:(NSException *)ex;
+(ToolkitException *)errorCode:(NSInteger)code;
+(ToolkitException *)exceptionWithMessage:(int)errorCode;
+(ToolkitException *)exceptionWithMessage:(int)errorCode response:(NSString *)response;
+(ToolkitException *)execptionWithName:(NSInteger)code execeptionName:(NSString *)execeptionName;
+(ToolkitException *)exceptionWithMessage:(int)errorCode attemptsLeft:(int)attemptsLeft;
+(ToolkitException *)exceptionWithMessage:(int)errorCode attemptsLeft:(int)attemptsLeft response:(NSString *)response;
@end
