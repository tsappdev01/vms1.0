//
//  HeadOfFamily.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 17/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitXmlDataObject.h"

@interface HeadOfFamily : ToolkitXmlDataObject

-(id)initWithHeadOfFamily:(NSDictionary *)headOfFamily;
-(NSString *)getHolderIdNumber;
-(NSString *)getFamilyId;
-(NSString *)getEmirateNameArabic;
-(NSString *)getEmirateNameEnglish;
-(NSString *)getFirstNameArabic;
-(NSString *)getFirstNameEnglish;
-(NSString *)getFatherNameArabic;
-(NSString *)getFatherNameEnglish;
-(NSString *)getGrandFatherNameArabic;
-(NSString *)getGrandFatherNameEnglish;
-(NSString *)getTribeArabic;
-(NSString *)getTribeEnglish;
-(NSString *)getClanArabic;
-(NSString *)getClanEnglish;
-(NSString *)getNationalityCode;
-(NSString *)getNationalityArabic;
-(NSString *)getNationalityEnglish;
-(NSString *)getGender;
-(NSString *)getDateOfBirth;
-(NSString *)getPlaceOfBirthArabic;
-(NSString *)getPlaceOfBirthEnglish;
-(NSString *)getMotherFullNameArabic;
-(NSString *)getMotherFullNameEnglish;
@end
