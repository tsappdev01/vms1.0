//
//  RegisterDeviceResponse.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 16/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitResponse.h"

@interface RegisterDeviceResponse : ToolkitResponse
-(id)initWithRegisterDeviceResponse:(NSString *)xmlString;
-(NSString *)getDeviceRegistrationId;
@end
