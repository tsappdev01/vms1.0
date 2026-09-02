//
//  CardCertificates.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 16/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitResponse.h"

@interface CardCertificates : ToolkitResponse

-(id)initWithCardCertificates:(NSString *)xmlString;
-(uint8_t *)getAuthenticationCertificate;
-(uint8_t *)getSigningCertificate;
-(int)getAuthenticationCertificateLength;
-(int)getSigningCertificateLength;
@end
