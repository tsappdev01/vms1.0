//
//  FamilyBookParse.swift
//  IDCardToolkitSwiftApp
//
//  Created by Federal Authority For Identity and Citizenship on 23/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

import UIKit

class FamilyBookParse: NSObject {
    
    var familyBookKeyElements = NSMutableArray()
    var familyBookValueElements = NSMutableArray()
    
    func getHeadOfFamilyDetails(_ headofFamily:HeadOfFamily) {
        
        var resultData:String
        
        self.familyBookKeyElements.add("---HeadofFamily---")
        self.familyBookValueElements.add(" ")
        
        resultData = headofFamily.getHolderIdNumber()
        self.familyBookKeyElements.add("HolderIdNumber")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getFamilyId()
        self.familyBookKeyElements.add("FamilyId")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getEmirateNameArabic()
        self.familyBookKeyElements.add("EmirateNameArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getEmirateNameEnglish()
        self.familyBookKeyElements.add("EmirateNameEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getFirstNameArabic()
        self.familyBookKeyElements.add("FirstNameArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getFirstNameEnglish()
        self.familyBookKeyElements.add("FirstNameEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getFatherNameArabic()
        self.familyBookKeyElements.add("FatherNameArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getFatherNameEnglish()
        self.familyBookKeyElements.add("FatherNameEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getGrandFatherNameArabic()
        self.familyBookKeyElements.add("GrandFatherNameArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getGrandFatherNameEnglish()
        self.familyBookKeyElements.add("GrandFatherNameEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getTribeArabic()
        self.familyBookKeyElements.add("TribeArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getTribeEnglish()
        self.familyBookKeyElements.add("TribeEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getClanArabic()
        self.familyBookKeyElements.add("ClanArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getClanEnglish()
        self.familyBookKeyElements.add("ClanEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getNationalityCode()
        self.familyBookKeyElements.add("NationalityCode")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getNationalityArabic()
        self.familyBookKeyElements.add("NationalityArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getNationalityEnglish()
        self.familyBookKeyElements.add("NationalityEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getGender()
        self.familyBookKeyElements.add("Gender")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getDateOfBirth()
        self.familyBookKeyElements.add("DateOfBirth")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getPlaceOfBirthArabic()
        self.familyBookKeyElements.add("PlaceOfBirthArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getPlaceOfBirthEnglish()
        self.familyBookKeyElements.add("PlaceOfBirthEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getMotherFullNameArabic()
        self.familyBookKeyElements.add("MotherFullNameArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = headofFamily.getMotherFullNameEnglish()
        self.familyBookKeyElements.add("MotherFullNameEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
    }
    func getWifeDetails(_ wife:Wife, index:Int) {
        
        self.familyBookKeyElements.add("------WIFE \(index+1)------ ")
        self.familyBookValueElements.add(" ")
        
        var resultData:String
        resultData = wife.getWifeIdn()
        self.familyBookKeyElements.add("WifeIdNumber")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = wife.getFullNameArabic()
        self.familyBookKeyElements.add("FullNameArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = wife.getFullNameEnglish()
        self.familyBookKeyElements.add("FullNameEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = wife.getNationalityCode()
        self.familyBookKeyElements.add("NationalityCode")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = wife.getNationalityArabic()
        self.familyBookKeyElements.add("NationalityArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = wife.getNationalityEnglish()
        self.familyBookKeyElements.add("NationalityEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
    }
    func getChildDetails(_ child:Child, index:Int) {
        
        self.familyBookKeyElements.add("------CHILD \(index+1)------ ")
        self.familyBookValueElements.add(" ")
        
        var resultData:String
        resultData = child.getChildIdn()
        self.familyBookKeyElements.add("ChildIdNumber")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = child.getFirstNameArabic()
        self.familyBookKeyElements.add("FirstNameArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = child.getFirstNameEnglish()
        self.familyBookKeyElements.add("FirstNameEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = child.getGender()
        self.familyBookKeyElements.add("Gender")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = child.getDateOfBirth()
        self.familyBookKeyElements.add("DateOfBirth")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = child.getPlaceOfBirthArabic()
        self.familyBookKeyElements.add("PlaceOfBirthArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = child.getPlaceOfBirthEnglish()
        self.familyBookKeyElements.add("PlaceOfBirthEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = child.getMotherIdn()
        self.familyBookKeyElements.add("MotherIdNumber")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = child.getMotherFullNameArabic()
        self.familyBookKeyElements.add("MotherFullNameArabic")
        self.familyBookValueElements.add("\(resultData)")
        
        resultData = child.getMotherFullNameEnglish()
        self.familyBookKeyElements.add("MotherFullNameEnglish")
        self.familyBookValueElements.add("\(resultData)")
        
    }
    func familyBookKey() -> NSMutableArray {
        return self.familyBookKeyElements
    }
    func familyBookValue() -> NSMutableArray {
        return self.familyBookValueElements
    }
}

