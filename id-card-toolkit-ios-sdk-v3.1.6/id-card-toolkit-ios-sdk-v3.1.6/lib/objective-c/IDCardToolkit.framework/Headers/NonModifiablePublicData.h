//
//  NonModifiablePublicData.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 17/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitXmlDataObject.h"

@interface NonModifiablePublicData : ToolkitXmlDataObject

-(id)initWithNonModifiablePublicData:(NSDictionary *)nonModifiableDataElement;
-(NSString *)getIDType;
-(NSString *)getIssueDate;
-(NSString *)getExpiryDate;
-(NSString *)getTitleArabic;
-(NSString *)getFullNameArabic;
-(NSString *)getTitleEnglish;
-(NSString *)getFullNameEnglish;
-(NSString *)getGender;
-(NSString *)getNationalityArabic;
-(NSString *)getNationalityEnglish;
-(NSString *)getNationalityCode;
-(NSString *)getDateOfBirth;
-(NSString *)getPlaceOfBirthArabic;
-(NSString *)getPlaceOfBirthEnglish;
@end
