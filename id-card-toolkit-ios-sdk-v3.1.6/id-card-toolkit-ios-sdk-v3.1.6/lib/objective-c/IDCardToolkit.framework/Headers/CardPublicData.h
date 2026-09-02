//
//  CardPublicData.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 16/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "ToolkitResponse.h"
#import "ToolkitXmlDataObject.h"
#import "ModifiablePublicData.h"
#import "NonModifiablePublicData.h"
#import "HomeAddress.h"
#import "WorkAddress.h"

@interface CardPublicData : ToolkitResponse

-(id)initWithCardPublicData:(NSString *)xmlString;
-(NSString *)getIdNumber;
-(NSString *)getCardNumber;
-(uint8_t *)getCardHolderPhoto;
-(int)getCardHolderPhotoLength;
-(uint8_t *)getHolderSignatureImage;
-(int)getHolderSignatureImageLength;
-(ModifiablePublicData *)getModifiablePublicData;
-(NonModifiablePublicData *)getNonModifiablePublicData;
-(HomeAddress *)getHomeAddress;
-(WorkAddress *)getWorkAddress;

@end
