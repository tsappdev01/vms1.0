//
//  ModifiablePublicData.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 17/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitXmlDataObject.h"

@interface ModifiablePublicData : ToolkitXmlDataObject

-(id)initWithModifiablePublicData:(NSDictionary *)modifiableDataElement;
-(NSString *)getOccupationCode;
-(NSString *)getOccupationArabic;
-(NSString *)getOccupationEnglish;
-(NSString *)getFamilyId;
-(NSString *)getOccupationTypeArabic;
-(NSString *)getOccupationTypeEnglish;
-(NSString *)getOccupationFieldCode;
-(NSString *)getCompanyNameArabic;
-(NSString *)getCompanyNameEnglish;
-(NSString *)getMaritalStatusCode;
-(NSString *)getHusbandIdnNumber;
-(NSString *)getSponsorTypeCode;
-(NSString *)getSponsorUnifiedNumber;
-(NSString *)getSponsorName;
-(NSString *)getResidencyTypeCode;
-(NSString *)getResidencyNumber;
-(NSString *)getResidencyExpiryDate;
-(NSString *)getPassportNumber;
-(NSString *)getPassportTypeCode;
-(NSString *)getPassportCountryCode;
-(NSString *)getPassportCountryArabic;
-(NSString *)getPassportCountryEnglish;
-(NSString *)getPassportIssueDate;
-(NSString *)getPassportExpiryDate;
-(NSString *)getQualificationLevelCode;
-(NSString *)getQualificationLevelArabic;
-(NSString *)getQualificationLevelEnglish;
-(NSString *)getDegreeDescArabic;
-(NSString *)getDegreeDescEnglish;
-(NSString *)getFieldOfStudyCode;
-(NSString *)getFieldOfStudyArabic;
-(NSString *)getFieldOfStudyEnglish;
-(NSString *)getPlaceOfStudyArabic;
-(NSString *)getPlaceOfStudyEnglish;
-(NSString *)getDateOfGraduation;
-(NSString *)getMotherFullNameArabic;
-(NSString *)getMotherFullNameEnglish;
@end
