//
//  SignatureResponse.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 16/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitResponse.h"

@interface SignatureResponse : ToolkitResponse

-(id)initWithSignatureResponse:(NSString *)xmlString;
-(unsigned char *)getSignature;
-(int)getSignatureLength;
@end
