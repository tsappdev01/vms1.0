//
//  ErrorCodes.h
//  EIDAToolkit
//
//  Created by Federal Authority For Identity and Citizenship on 21/11/17.
//  Copyright © 2017 Emirates Identity Authority. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface ErrorCodes : NSObject
-(id)init;
-(NSMutableDictionary *)messages;
-(NSString *)getErrorMessage:(NSInteger)Code;

@property(nonatomic)NSInteger E_UNSPECIFIED_ERROR;
@property(nonatomic)NSInteger E_INVALID_ARGUMENT;
@property(nonatomic)NSInteger E_INVALID_XML_FORMAT;
@property(nonatomic)NSInteger E_CARD_NOT_CONNECTED;

@end
