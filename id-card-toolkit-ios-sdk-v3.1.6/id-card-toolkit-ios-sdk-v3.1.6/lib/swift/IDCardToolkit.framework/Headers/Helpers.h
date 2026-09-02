//
//  Helpers.h
//  IDCardToolkit
//
//  Created by Federal Authority For Identity and Citizenship on 22/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#ifndef HELPERS_H
#define HELPERS_H

#import <Foundation/Foundation.h>
#import "EIDAToolkitTypes.h"

__attribute__((visibility("default")))
@interface Helpers : NSObject

-(id)init;
unsigned int Base64Decode(const unsigned char *encoded_data,
                          unsigned int encoded_data_length, unsigned char **decoded_data,
                          unsigned int *decoded_data_length);
+(NSString *)checkIfNull:(id)Value;
-(void)setMask:(NSString *)configparams;
-(unsigned char *)getDeviceID ;
-(int)getDeviceIDLength;
-(NFC_OBJECT *)setNfcValues:(id)session tag:(id)tag;
@end

#endif /* HELPERS_H */
