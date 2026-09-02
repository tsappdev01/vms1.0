//
//  WorkAddress.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 17/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitXmlDataObject.h"

@interface WorkAddress : ToolkitXmlDataObject

-(id)initWithWorkAddress:(NSDictionary *)workAddressDataElement;
-(NSString *)getAddressTypeCode;
-(NSString *)getLocationCode;
-(NSString *)getCompanyNameArabic;
-(NSString *)getCompanyNameEnglish;
-(NSString *)getEmiratesCode;
-(NSString *)getEmiratesArabic;
-(NSString *)getEmiratesEnglish;
-(NSString *)getCityCode;
-(NSString *)getCityArabic;
-(NSString *)getCityEnglish;
-(NSString *)getPoBox;
-(NSString *)getStreetArabic;
-(NSString *)getStreetEnglish;
-(NSString *)getAreaCode;
-(NSString *)getAreaArabic;
-(NSString *)getAreaEnglish;
-(NSString *)getBuildingNameArabic;
-(NSString *)getBuildingNameEnglish;
-(NSString *)getLandPhoneNumber;
-(NSString *)getMobilePhoneNumber;
-(NSString *)getEmail;
@end
