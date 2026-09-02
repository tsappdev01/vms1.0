//
//  PublicDataParse.swift
//  IDCardToolkitSwiftApp
//
//  Created by Federal Authority For Identity and Citizenship on 23/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

import UIKit

class PublicDataParse: NSObject {

    var pModDataDictionary = NSMutableDictionary()
    var pNonModDataDictionary = NSMutableDictionary()
    var pHomeAddressDictionary = NSMutableDictionary()
    var pWorkAddressDictionary = NSMutableDictionary()
    
    func getModifiablePublicDataDetails(_ modData:ModifiablePublicData) -> NSMutableDictionary {
        
        var resultData:String
        resultData = modData.getOccupationCode()
        self.pModDataDictionary["OccupationCode"] = "\(resultData)"
        resultData = modData.getOccupationArabic()
        self.pModDataDictionary["OccupationArabic"] = "\(resultData)"
        resultData = modData.getOccupationEnglish()
        self.pModDataDictionary["OccupationEnglish"] = "\(resultData)"
        resultData = modData.getFamilyId()
        self.pModDataDictionary["FamilyId"] = "\(resultData)"
        resultData = modData.getOccupationTypeArabic()
        self.pModDataDictionary["OccupationTypeArabic"] = "\(resultData)"
        resultData = modData.getOccupationTypeEnglish()
        self.pModDataDictionary["OccupationTypeEnglish"] = "\(resultData)"
        resultData = modData.getOccupationFieldCode()
        self.pModDataDictionary["OccupationFieldCode"] = "\(resultData)"
        resultData = modData.getCompanyNameArabic()
        self.pModDataDictionary["CompanyNameArabic"] = "\(resultData)"
        resultData = modData.getCompanyNameEnglish()
        self.pModDataDictionary["CompanyNameEnglish"] = "\(resultData)"
        resultData = modData.getMaritalStatusCode()
        self.pModDataDictionary["MaritalStatusCode"] = "\(resultData)"
        resultData = modData.getHusbandIdnNumber()
        self.pModDataDictionary["HusbandIdNumber"] = "\(resultData)"
        resultData = modData.getSponsorTypeCode()
        self.pModDataDictionary["SponsorTypeCode"] = "\(resultData)"
        resultData = modData.getSponsorUnifiedNumber()
        self.pModDataDictionary["SponsorUnifiedNumber"] = "\(resultData)"
        resultData = modData.getSponsorName()
        self.pModDataDictionary["SponsorName"] = "\(resultData)"
        resultData = modData.getResidencyTypeCode()
        self.pModDataDictionary["ResidencyTypeCode"] = "\(resultData)"
        resultData = modData.getResidencyNumber()
        self.pModDataDictionary["ResidencyNumber"] = "\(resultData)"
        resultData = modData.getResidencyExpiryDate()
        self.pModDataDictionary["ResidencyExpiryDate"] = "\(resultData)"
        resultData = modData.getPassportNumber()
        self.pModDataDictionary["PassportNumber"] = "\(resultData)"
        resultData = modData.getPassportTypeCode()
        self.pModDataDictionary["PassportTypeCode"] = "\(resultData)"
        resultData = modData.getPassportCountryCode()
        self.pModDataDictionary["PassportCountryCode"] = "\(resultData)"
        resultData = modData.getPassportCountryArabic()
        self.pModDataDictionary["PassportCountryArabic"] = "\(resultData)"
        resultData = modData.getPassportCountryEnglish()
        self.pModDataDictionary["PassportCountryEnglish"] = "\(resultData)"
        resultData = modData.getPassportIssueDate()
        self.pModDataDictionary["PassportIssueDate"] = "\(resultData)"
        resultData = modData.getPassportExpiryDate()
        self.pModDataDictionary["PassportExpiryDate"] = "\(resultData)"
        resultData = modData.getQualificationLevelCode()
        self.pModDataDictionary["QualificationLevelCode"] = "\(resultData)"
        resultData = modData.getQualificationLevelArabic()
        self.pModDataDictionary["QualificationLevelArabic"] = "\(resultData)"
        resultData = modData.getQualificationLevelEnglish()
        self.pModDataDictionary["QualificationLevelEnglish"] = "\(resultData)"
        resultData = modData.getDegreeDescArabic()
        self.pModDataDictionary["DegreeDescArabic"] = "\(resultData)"
        resultData = modData.getDegreeDescEnglish()
        self.pModDataDictionary["DegreeDescEnglish"] = "\(resultData)"
        resultData = modData.getFieldOfStudyCode()
        self.pModDataDictionary["FieldOfStudyCode"] = "\(resultData)"
        resultData = modData.getFieldOfStudyArabic()
        self.pModDataDictionary["FieldOfStudyArabic"] = "\(resultData)"
        resultData = modData.getFieldOfStudyEnglish()
        self.pModDataDictionary["FieldOfStudyEnglish"] = "\(resultData)"
        resultData = modData.getPlaceOfStudyArabic()
        self.pModDataDictionary["PlaceOfStudyArabic"] = "\(resultData)"
        resultData = modData.getPlaceOfStudyEnglish()
        self.pModDataDictionary["PlaceOfStudyEnglish"] = "\(resultData)"
        resultData = modData.getDateOfGraduation()
        self.pModDataDictionary["DateOfGraduation"] = "\(resultData)"
        resultData = modData.getMotherFullNameArabic()
        self.pModDataDictionary["MotherFullNameArabic"] = "\(resultData)"
        resultData = modData.getMotherFullNameEnglish()
        self.pModDataDictionary["MotherFullNameEnglish"] = "\(resultData)"
        
        return self.pModDataDictionary
    }
    func getNonModifiablePublicDataDetails(_ nonModData:NonModifiablePublicData) -> NSMutableDictionary {

        var resultData:String
        resultData = nonModData.getIDType()
        self.pNonModDataDictionary["IDType"] = "\(resultData)"
        resultData = nonModData.getIssueDate()
        self.pNonModDataDictionary["IssueDate"] = "\(resultData)"
        resultData = nonModData.getExpiryDate()
        self.pNonModDataDictionary["ExpiryDate"] = "\(resultData)"
        resultData = nonModData.getTitleArabic()
        self.pNonModDataDictionary["TitleArabic"] = "\(resultData)"
        resultData = nonModData.getFullNameArabic()
        self.pNonModDataDictionary["FullNameArabic"] = "\(resultData)"
        resultData = nonModData.getTitleEnglish()
        self.pNonModDataDictionary["TitleEnglish"] = "\(resultData)"
        resultData = nonModData.getFullNameEnglish()
        self.pNonModDataDictionary["FullNameEnglish"] = "\(resultData)"
        resultData = nonModData.getGender()
        self.pNonModDataDictionary["Gender"] = "\(resultData)"
        resultData = nonModData.getNationalityArabic()
        self.pNonModDataDictionary["NationalityArabic"] = "\(resultData)"
        resultData = nonModData.getNationalityEnglish()
        self.pNonModDataDictionary["NationalityEnglish"] = "\(resultData)"
        resultData = nonModData.getNationalityCode()
        self.pNonModDataDictionary["NationalityCode"] = "\(resultData)"
        resultData = nonModData.getDateOfBirth()
        self.pNonModDataDictionary["DateOfBirth"] = "\(resultData)"
        resultData = nonModData.getPlaceOfBirthArabic()
        self.pNonModDataDictionary["PlaceOfBirthArabic"] = "\(resultData)"
        resultData = nonModData.getPlaceOfBirthEnglish()
        self.pNonModDataDictionary["PlaceOfBirthEnglish"] = "\(resultData)"

        return self.pNonModDataDictionary
    }
    func getHomeAddressDetails(_ homeaddress:HomeAddress) -> NSMutableDictionary {
        
        var resultData:String
        resultData = homeaddress.getAddressTypeCode()
        self.pHomeAddressDictionary["AddressTypeCode"] = "\(resultData)"
        resultData = homeaddress.getLocationCode()
        self.pHomeAddressDictionary["LocationCode"] = "\(resultData)"
        resultData = homeaddress.getEmiratesCode()
        self.pHomeAddressDictionary["EmiratesCode"] = "\(resultData)"
        resultData = homeaddress.getEmiratesArabic()
        self.pHomeAddressDictionary["EmiratesArabic"] = "\(resultData)"
        resultData = homeaddress.getEmiratesEnglish()
        self.pHomeAddressDictionary["EmiratesEnglish"] = "\(resultData)"
        resultData = homeaddress.getCityCode()
        self.pHomeAddressDictionary["CityCode"] = "\(resultData)"
        resultData = homeaddress.getCityArabic()
        self.pHomeAddressDictionary["CityArabic"] = "\(resultData)"
        resultData = homeaddress.getCityEnglish()
        self.pHomeAddressDictionary["CityEnglish"] = "\(resultData)"
        resultData = homeaddress.getStreetArabic()
        self.pHomeAddressDictionary["StreetArabic"] = "\(resultData)"
        resultData = homeaddress.getStreetEnglish()
        self.pHomeAddressDictionary["StreetEnglish"] = "\(resultData)"
        resultData = homeaddress.getPobox()
        self.pHomeAddressDictionary["Pobox"] = "\(resultData)"
        resultData = homeaddress.getAreaCode()
        self.pHomeAddressDictionary["AreaCode"] = "\(resultData)"
        resultData = homeaddress.getAreaArabic()
        self.pHomeAddressDictionary["AreaArabic"] = "\(resultData)"
        resultData = homeaddress.getAreaEnglish()
        self.pHomeAddressDictionary["AreaEnglish"] = "\(resultData)"
        resultData = homeaddress.getBuildingNameArabic()
        self.pHomeAddressDictionary["BuildingNameArabic"] = "\(resultData)"
        resultData = homeaddress.getBuildingNameEnglish()
        self.pHomeAddressDictionary["BuildingNameEnglish"] = "\(resultData)"
        resultData = homeaddress.getLandPhoneNumber()
        self.pHomeAddressDictionary["ResidentPhoneNumber"] = "\(resultData)"
        resultData = homeaddress.getMobilePhoneNumber()
        self.pHomeAddressDictionary["MobilePhoneNumber"] = "\(resultData)"
        resultData = homeaddress.getEmail()
        self.pHomeAddressDictionary["Email"] = "\(resultData)"
        resultData = homeaddress.getFlatNumber()
        self.pHomeAddressDictionary["FlatNumber"] = "\(resultData)"
        
         return self.pHomeAddressDictionary

    }
    func getWorkAddressDetails(_ workAddress:WorkAddress) -> NSMutableDictionary {

        var resultData:String
        resultData = workAddress.getAddressTypeCode()
        self.pWorkAddressDictionary["AddressTypeCode"] = "\(resultData)"
        resultData = workAddress.getLocationCode()
        self.pWorkAddressDictionary["LocationCode"] = "\(resultData)"
        resultData = workAddress.getCompanyNameArabic()
        self.pWorkAddressDictionary["CompanyNameArabic"] = "\(resultData)"
        resultData = workAddress.getCompanyNameEnglish()
        self.pWorkAddressDictionary["CompanyNameEnglish"] = "\(resultData)"
        resultData = workAddress.getEmiratesCode()
        self.pWorkAddressDictionary["EmiratesCode"] = "\(resultData)"
        resultData = workAddress.getEmiratesArabic()
        self.pWorkAddressDictionary["EmiratesArabic"] = "\(resultData)"
        resultData = workAddress.getEmiratesEnglish()
        self.pWorkAddressDictionary["EmiratesEnglish"] = "\(resultData)"
        resultData = workAddress.getCityCode()
        self.pWorkAddressDictionary["CityCode"] = "\(resultData)"
        resultData = workAddress.getCityArabic()
        self.pWorkAddressDictionary["CityArabic"] = "\(resultData)"
        resultData = workAddress.getCityEnglish()
        self.pWorkAddressDictionary["CityEnglish"] = "\(resultData)"
        resultData = workAddress.getPoBox()
        self.pWorkAddressDictionary["PoBox"] = "\(resultData)"
        resultData = workAddress.getStreetArabic()
        self.pWorkAddressDictionary["StreetArabic"] = "\(resultData)"
        resultData = workAddress.getStreetEnglish()
        self.pWorkAddressDictionary["StreetEnglish"] = "\(resultData)"
        resultData = workAddress.getAreaCode()
        self.pWorkAddressDictionary["AreaCode"] = "\(resultData)"
        resultData = workAddress.getAreaArabic()
        self.pWorkAddressDictionary["AreaArabic"] = "\(resultData)"
        resultData = workAddress.getAreaEnglish()
        self.pWorkAddressDictionary["AreaEnglish"] = "\(resultData)"
        resultData = workAddress.getBuildingNameArabic()
        self.pWorkAddressDictionary["BuildingNameArabic"] = "\(resultData)"
        resultData = workAddress.getBuildingNameEnglish()
        self.pWorkAddressDictionary["BuildingNameEnglish"] = "\(resultData)"
        resultData = workAddress.getLandPhoneNumber()
        self.pWorkAddressDictionary["LandPhoneNumber"] = "\(resultData)"
        resultData = workAddress.getMobilePhoneNumber()
        self.pWorkAddressDictionary["MobilePhoneNumber"] = "\(resultData)"
        resultData = workAddress.getEmail()
        self.pWorkAddressDictionary["Email"] = "\(resultData)"
    
        return self.pWorkAddressDictionary
   }


}






