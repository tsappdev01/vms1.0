//
//  DataProtectionKey.h
//  IDCardToolkit
//
//  Created by Federal Authority For Identity and Citizenship on 29/03/18.
//  Copyright © 2018 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface DataProtectionKey : NSObject

-(id)initWithData:(uint8_t *)publicKey keyLength:(int)keyLength modulus:(NSString *)modulus modulusLength:(int)modulusLength exponent:(NSString *)exponent exponentLength:(int)exponentLength;
-(uint8_t *)getPublicKey;
-(int)getKeyLength;
-(NSString *)getModulus;
-(int)getModulusLength;
-(NSString *)getExponent;
-(int)getExponentLength;
@end

