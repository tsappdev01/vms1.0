//
//  HealthDataParse.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship  on 09/08/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship . All rights reserved.
//

#import "HealthDataParse.h"

@implementation HealthDataParse

-(id)init {
    self=[super init];
    if (self) {
        
        self.resourceTypeArr=[[NSMutableArray alloc]init];
        self.resourceTypeDetailsArr=[[NSMutableArray alloc]init];
    }
    return self;
}
-(void)getAllergyResourceDetails:(AllergyResource *)allergyResource {
    
    NSMutableArray *subArr =[[NSMutableArray alloc]init];
    
    [self.resourceTypeArr addObject:[NSString stringWithFormat:@"%@",[allergyResource getResourceType]]];
    
    [subArr addObject:[NSString stringWithFormat:@"AllergyDisplay : %@",[allergyResource getAllergyDisplay]]];
    [subArr addObject:[NSString stringWithFormat:@"AllergyRecordedDate : %@",[allergyResource getAllergyRecordedDate]]];
    
    [self.resourceTypeDetailsArr addObject:subArr];
    NSLog(@"resourceTypeDetailsArr %@",self.resourceTypeDetailsArr);
}
-(void)getDiagnosisResourceDetails:(DiagnosisResource *)diagnosisResource {
    
    NSMutableArray *subArr =[[NSMutableArray alloc]init];

    [self.resourceTypeArr addObject:[NSString stringWithFormat:@"%@",[diagnosisResource getResourceType]]];

    [subArr addObject:[NSString stringWithFormat:@"DiagnosisCode : %@",[diagnosisResource getDiagnosisCode]]];
    [subArr addObject:[NSString stringWithFormat:@"DiagnosisDescription : %@",[diagnosisResource getDiagnosisDescription]]];
    [subArr addObject:[NSString stringWithFormat:@"DiagnosisRecordedDate : %@",[diagnosisResource getDiagnosisRecordedDate]]];
    
    [self.resourceTypeDetailsArr addObject:subArr];
}
-(void)getBloodGroupResourceDetails:(BloodGroupResource *)bloodGroupResource {
    
    NSMutableArray *subArr =[[NSMutableArray alloc]init];

    [self.resourceTypeArr addObject:[NSString stringWithFormat:@"%@",[bloodGroupResource getResourceType]]];

    [subArr addObject:[NSString stringWithFormat:@"BloodGroup : %@",[bloodGroupResource getBloodGroup]]];
    [subArr addObject:[NSString stringWithFormat:@"RecordedDate : %@",[bloodGroupResource getRecordedDate]]];
    
    [self.resourceTypeDetailsArr addObject:subArr];
}
-(void)getInsuranceResourceDetails:(InsuranceResource *)insuranceResource {
    
    NSMutableArray *subArr =[[NSMutableArray alloc]init];

    [self.resourceTypeArr addObject:[NSString stringWithFormat:@"%@",[insuranceResource getResourceType]]];
    
    [subArr addObject:[NSString stringWithFormat:@"InsuranceName : %@",[insuranceResource getInsuranceName]]];
    [subArr addObject: [NSString stringWithFormat:@"InsuranceNumber : %@",[insuranceResource getInsuranceNumber]]];
    [subArr addObject:[NSString stringWithFormat:@"InsuranceValidityStartDate : %@",[insuranceResource getInsuranceValidityStartDate]]];
    [subArr addObject:[NSString stringWithFormat:@"InsuranceValidityEndDate : %@",[insuranceResource getInsuranceValidityEndDate]]];
    
    [self.resourceTypeDetailsArr addObject:subArr];
}
-(void)getOrganDonorDetails:(HealthDataContainer *)healthDataContainer {
    
    NSMutableArray *subArr =[[NSMutableArray alloc]init];

     [self.resourceTypeArr addObject:@"OrganDonor"];
    [subArr addObject:[NSString stringWithFormat:@"OrganDonor : %@",[healthDataContainer getOrganDonor]]];
    
    [self.resourceTypeDetailsArr addObject:subArr];
}
-(NSMutableArray *)resourceTypeSections {
    return self.resourceTypeArr;
}
-(NSMutableArray *)resourceTypeSectionCells {
    return self.resourceTypeDetailsArr;
}

@end
