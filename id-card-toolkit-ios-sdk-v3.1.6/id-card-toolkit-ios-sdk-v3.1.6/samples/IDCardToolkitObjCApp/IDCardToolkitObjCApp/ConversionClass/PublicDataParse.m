//
//  PublicDataParse.m
//  EIDAToolkitObjCTestapp
//
//  Created by Federal Authority For Identity and Citizenship on 07/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "PublicDataParse.h"

@implementation PublicDataParse

-(id)init {
    self=[super init];
    if (self) {
        
        self.pModDataDictionary=[[NSMutableDictionary alloc]init];
        self.pNonModDataDictionary=[[NSMutableDictionary alloc]init];
        self.pHomeAddressDictionary=[[NSMutableDictionary alloc]init];
        self.pWorkAddressDictionary=[[NSMutableDictionary alloc]init];
    }
    return self;
}
-(NSMutableDictionary *)getModifiablePublicDataDetails:(ModifiablePublicData *)modData {
    
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getOccupationCode]] forKey:@"OccupationCode"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getOccupationArabic]] forKey:@"OccupationArabic"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getOccupationEnglish]] forKey:@"OccupationEnglish"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getFamilyId]] forKey:@"FamilyId"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getOccupationTypeArabic]] forKey:@"OccupationTypeArabic"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getOccupationTypeEnglish]] forKey:@"OccupationTypeEnglish"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getOccupationFieldCode]] forKey:@"OccupationFieldCode"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getCompanyNameArabic]] forKey:@"CompanyNameArabic"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getCompanyNameEnglish]] forKey:@"CompanyNameEnglish"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getMaritalStatusCode]] forKey:@"MaritalStatusCode"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getHusbandIdnNumber]] forKey:@"HusbandIdNumber"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getSponsorTypeCode]] forKey:@"SponsorTypeCode"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getSponsorUnifiedNumber]] forKey:@"SponsorUnifiedNumber"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getSponsorName]] forKey:@"SponsorName"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getResidencyTypeCode]] forKey:@"ResidencyTypeCode"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getResidencyNumber]] forKey:@"ResidencyNumber"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getResidencyExpiryDate]] forKey:@"ResidencyExpiryDate"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getPassportNumber]] forKey:@"PassportNumber"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getPassportTypeCode]] forKey:@"PassportTypeCode"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getPassportCountryCode]] forKey:@"PassportCountryCode"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getPassportCountryArabic]] forKey:@"PassportCountryArabic"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getPassportCountryEnglish]] forKey:@"PassportCountryEnglish"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getPassportIssueDate]] forKey:@"PassportIssueDate"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getPassportExpiryDate]] forKey:@"PassportExpiryDate"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getQualificationLevelCode]] forKey:@"QualificationLevelCode"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getQualificationLevelArabic]] forKey:@"QualificationLevelArabic"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getQualificationLevelEnglish]] forKey:@"QualificationLevelEnglish"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getDegreeDescArabic]] forKey:@"DegreeDescArabic"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getDegreeDescEnglish]] forKey:@"DegreeDescEnglish"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getFieldOfStudyCode]] forKey:@"FieldOfStudyCode"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getFieldOfStudyArabic]] forKey:@"FieldOfStudyArabic"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getFieldOfStudyEnglish]] forKey:@"FieldOfStudyEnglish"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getPlaceOfStudyArabic]] forKey:@"PlaceOfStudyArabic"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getPlaceOfStudyEnglish]] forKey:@"PlaceOfStudyEnglish"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getDateOfGraduation]] forKey:@"DateOfGraduation"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getMotherFullNameArabic]] forKey:@"MotherFullNameArabic"];
    [self.pModDataDictionary setValue:[NSString stringWithFormat:@"%@",[modData getMotherFullNameEnglish]] forKey:@"MotherFullNameEnglish"];
    
    return  self.pModDataDictionary;
}
-(NSMutableDictionary *)getNonModifiablePublicDataDetails:(NonModifiablePublicData *)nonModData {
    
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getIDType]] forKey:@"IDType"];
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getIssueDate]] forKey:@"IssueDate"];
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getExpiryDate]] forKey:@"ExpiryDate"];
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getTitleArabic]] forKey:@"TitleArabic"];
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getFullNameArabic]] forKey:@"FullNameArabic"];
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getTitleEnglish]] forKey:@"TitleEnglish"];
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getFullNameEnglish]] forKey:@"FullNameEnglish"];
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getGender]] forKey:@"Gender"];
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getNationalityArabic]] forKey:@"NationalityArabic"];
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getNationalityEnglish]] forKey:@"NationalityEnglish"];
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getNationalityCode]] forKey:@"NationalityCode"];
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getDateOfBirth]] forKey:@"DateOfBirth"];
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getPlaceOfBirthArabic]] forKey:@"PlaceOfBirthArabic"];
    [self.pNonModDataDictionary setValue:[NSString stringWithFormat:@"%@",[nonModData getPlaceOfBirthEnglish]] forKey:@"PlaceOfBirthEnglish"];
    
    return  self.pNonModDataDictionary;
}
-(NSMutableDictionary *)getHomeAddressDetails:(HomeAddress *)homeaddress {
    
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getAddressTypeCode]] forKey:@"AddressTypeCode"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getLocationCode]] forKey:@"LocationCode"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getEmiratesCode]] forKey:@"EmiratesCode"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getEmiratesArabic]] forKey:@"EmiratesArabic"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getEmiratesEnglish]] forKey:@"EmiratesEnglish"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getCityCode]] forKey:@"CityCode"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getCityArabic]] forKey:@"CityArabic"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getCityEnglish]] forKey:@"CityEnglish"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getStreetArabic]] forKey:@"StreetArabic"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getStreetEnglish]] forKey:@"StreetEnglish"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getPobox]] forKey:@"Pobox"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getAreaCode]] forKey:@"AreaCode"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getAreaArabic]] forKey:@"AreaArabic"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getAreaEnglish]] forKey:@"AreaEnglish"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getBuildingNameArabic]] forKey:@"BuildingNameArabic"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getBuildingNameEnglish]] forKey:@"BuildingNameEnglish"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getLandPhoneNumber]] forKey:@"ResidentPhoneNumber"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getMobilePhoneNumber]] forKey:@"MobilePhoneNumber"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getEmail]] forKey:@"Email"];
    [self.pHomeAddressDictionary setValue:[NSString stringWithFormat:@"%@",[homeaddress getFlatNumber]] forKey:@"FlatNumber"];
    
    return self.pHomeAddressDictionary;
}
-(NSMutableDictionary *)getWorkAddressDetails:(WorkAddress *)workAddress {
    
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getAddressTypeCode]] forKey:@"AddressTypeCode"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getLocationCode]] forKey:@"LocationCode"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getCompanyNameArabic]] forKey:@"CompanyNameArabic"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getCompanyNameEnglish]] forKey:@"CompanyNameEnglish"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getEmiratesCode]] forKey:@"EmiratesCode"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getEmiratesArabic]] forKey:@"EmiratesArabic"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getEmiratesEnglish]] forKey:@"EmiratesEnglish"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getCityCode]] forKey:@"CityCode"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getCityArabic]] forKey:@"CityArabic"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getCityEnglish]] forKey:@"CityEnglish"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getPoBox]] forKey:@"PoBox"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getStreetArabic]] forKey:@"StreetArabic"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getStreetEnglish]] forKey:@"StreetEnglish"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getAreaCode]] forKey:@"AreaCode"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getAreaArabic]] forKey:@"AreaArabic"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getAreaEnglish]] forKey:@"AreaEnglish"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getBuildingNameArabic]] forKey:@"BuildingNameArabic"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getBuildingNameEnglish]] forKey:@"BuildingNameEnglish"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getLandPhoneNumber]] forKey:@"LandPhoneNumber"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getMobilePhoneNumber]] forKey:@"MobilePhoneNumber"];
    [self.pWorkAddressDictionary setValue:[NSString stringWithFormat:@"%@",[workAddress getEmail]] forKey:@"Email"];
    
    return self.pWorkAddressDictionary;
}
@end
