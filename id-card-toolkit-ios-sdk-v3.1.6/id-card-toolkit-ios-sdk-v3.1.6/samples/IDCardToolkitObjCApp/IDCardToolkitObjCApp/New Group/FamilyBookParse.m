//
//  FamilyBookParse.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 19/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "FamilyBookParse.h"

@implementation FamilyBookParse

-(id)init {
    self=[super init];
    if (self) {
        
        self.familyBookKeyElements=[[NSMutableArray alloc]init];
        self.familyBookValueElements=[[NSMutableArray alloc]init];
    }
    return self;
}
-(void)getHeadOfFamilyDetails:(HeadOfFamily *)headofFamily {
    
    [self.familyBookKeyElements addObject:@"------HEAD OF FAMILY------"];
    [self.familyBookValueElements addObject:@" "];
    
    [self.familyBookKeyElements addObject:@"HolderIdNumber"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getHolderIdNumber]]];
    
    [self.familyBookKeyElements addObject:@"FamilyId"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getFamilyId]]];
    
    [self.familyBookKeyElements addObject:@"EmirateNameArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getEmirateNameArabic]]];
    
    [self.familyBookKeyElements addObject:@"EmirateNameEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getEmirateNameEnglish]]];
    
    [self.familyBookKeyElements addObject:@"FirstNameArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getFirstNameArabic]]];
    
    [self.familyBookKeyElements addObject:@"FirstNameEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getFirstNameEnglish]]];
    
    [self.familyBookKeyElements addObject:@"FatherNameArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getFatherNameArabic]]];
    
    [self.familyBookKeyElements addObject:@"FatherNameEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getFatherNameEnglish]]];
    
    [self.familyBookKeyElements addObject:@"GrandFatherNameArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getGrandFatherNameArabic]]];
    
    [self.familyBookKeyElements addObject:@"GrandFatherNameEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getGrandFatherNameEnglish]]];
    
    [self.familyBookKeyElements addObject:@"TribeArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getTribeArabic]]];
    
    [self.familyBookKeyElements addObject:@"TribeEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getTribeEnglish]]];
    
    [self.familyBookKeyElements addObject:@"ClanArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getClanArabic]]];
    
    [self.familyBookKeyElements addObject:@"ClanEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getClanEnglish]]];
    
    [self.familyBookKeyElements addObject:@"NationalityCode"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getNationalityCode]]];
    
    [self.familyBookKeyElements addObject:@"NationalityArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getNationalityArabic]]];
    
    [self.familyBookKeyElements addObject:@"NationalityEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getNationalityEnglish]]];
    
    [self.familyBookKeyElements addObject:@"Gender"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getGender]]];
    
    [self.familyBookKeyElements addObject:@"DateOfBirth"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getDateOfBirth]]];
    
    [self.familyBookKeyElements addObject:@"PlaceOfBirthArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getPlaceOfBirthArabic]]];
    
    [self.familyBookKeyElements addObject:@"PlaceOfBirthEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getPlaceOfBirthEnglish]]];
    
    [self.familyBookKeyElements addObject:@"MotherFullNameArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getMotherFullNameArabic]]];
    
    [self.familyBookKeyElements addObject:@"MotherFullNameEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[headofFamily getMotherFullNameEnglish]]];
    
}
-(void)getWifeDetails:(Wife *)wife index:(int)index {
    
    [self.familyBookKeyElements addObject:[NSString stringWithFormat:@"------WIFE %i------",index+1]];
    [self.familyBookValueElements addObject:@" "];
    
    [self.familyBookKeyElements addObject:@"WifeIdNumber"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[wife getWifeIdn]]];
    
    [self.familyBookKeyElements addObject:@"FullNameArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[wife getFullNameArabic]]];
    
    [self.familyBookKeyElements addObject:@"FullNameEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[wife getFullNameEnglish]]];
    
    [self.familyBookKeyElements addObject:@"NationalityCode"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[wife getNationalityCode]]];
    
    [self.familyBookKeyElements addObject:@"NationalityArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[wife getNationalityArabic]]];
    
    [self.familyBookKeyElements addObject:@"NationalityEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[wife getNationalityEnglish]]];
}
-(void)getChildDetails:(Child *)child index:(int)index {
    
    [self.familyBookKeyElements addObject:[NSString stringWithFormat:@"------CHILD %i------",index+1]];
    [self.familyBookValueElements addObject:@" "];
    
    [self.familyBookKeyElements addObject:@"ChildIdNumber"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[child getChildIdn]]];
    
    [self.familyBookKeyElements addObject:@"FirstNameArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[child getFirstNameArabic]]];
    
    [self.familyBookKeyElements addObject:@"FirstNameEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[child getFirstNameEnglish]]];
    
    [self.familyBookKeyElements addObject:@"Gender"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[child getGender]]];
    
    [self.familyBookKeyElements addObject:@"DateOfBirth"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[child getDateOfBirth]]];
    
    [self.familyBookKeyElements addObject:@"PlaceOfBirthArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[child getPlaceOfBirthArabic]]];
    
    [self.familyBookKeyElements addObject:@"PlaceOfBirthEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[child getPlaceOfBirthEnglish]]];
    
    [self.familyBookKeyElements addObject:@"MotherIdNumber"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[child getMotherIdn]]];
    
    [self.familyBookKeyElements addObject:@"MotherFullNameArabic"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[child getMotherFullNameArabic]]];
    
    [self.familyBookKeyElements addObject:@"MotherFullNameEnglish"];
    [self.familyBookValueElements addObject:[NSString stringWithFormat:@"%@",[child getMotherFullNameEnglish]]];
}
-(NSMutableArray *)familyBookKey {
    return self.familyBookKeyElements;
}
-(NSMutableArray *)familyBookValue {
    return self.familyBookValueElements;
}

@end

