//
//  Helpers.h
//  IDCardToolkit
//
//  Created by Federal Authority For Identity and Citizenship on 22/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>

__attribute__((visibility("default")))
@interface Helpers : NSObject
-(id)init;
unsigned int Base64Decode(const unsigned char *encoded_data,
                          unsigned int encoded_data_length, unsigned char **decoded_data,
                          unsigned int *decoded_data_length);
+(NSString *)checkIfNull:(id)Value;
-(void)setMask:(NSString *)configparams;
-(char *)getDeviceID;
-(unsigned int *)getDeviceIDLength;
@end
