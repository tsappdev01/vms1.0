//
//  Child.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 17/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitXmlDataObject.h"

@interface Child : ToolkitXmlDataObject

-(id)initWithChildDetails:(NSDictionary *)ChildElement;
-(NSString *)getChildIdn;
-(NSString *)getFirstNameArabic;
-(NSString *)getFirstNameEnglish;
-(NSString *)getGender;
-(NSString *)getDateOfBirth;
-(NSString *)getPlaceOfBirthArabic;
-(NSString *)getPlaceOfBirthEnglish;
-(NSString *)getMotherIdn;
-(NSString *)getMotherFullNameArabic;
-(NSString *)getMotherFullNameEnglish;
@end
