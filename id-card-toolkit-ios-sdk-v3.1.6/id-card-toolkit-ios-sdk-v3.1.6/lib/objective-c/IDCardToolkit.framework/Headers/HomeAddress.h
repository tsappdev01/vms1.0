//
//  HomeAddress.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 17/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitXmlDataObject.h"

@interface HomeAddress : ToolkitXmlDataObject

-(id)initWithHomeAddress:(NSDictionary *)homeAddressDataElement;
-(NSString *)getAddressTypeCode;
-(NSString *)getLocationCode;
-(NSString *)getEmiratesCode;
-(NSString *)getEmiratesArabic;
-(NSString *)getEmiratesEnglish;
-(NSString *)getCityCode;
-(NSString *)getCityArabic;
-(NSString *)getCityEnglish;
-(NSString *)getStreetArabic;
-(NSString *)getStreetEnglish;
-(NSString *)getPobox;
-(NSString *)getAreaCode;
-(NSString *)getAreaArabic;
-(NSString *)getAreaEnglish;
-(NSString *)getBuildingNameArabic;
-(NSString *)getBuildingNameEnglish;
-(NSString *)getLandPhoneNumber;
-(NSString *)getMobilePhoneNumber;
-(NSString *)getEmail;
-(NSString *)getFlatNumber;
@end
