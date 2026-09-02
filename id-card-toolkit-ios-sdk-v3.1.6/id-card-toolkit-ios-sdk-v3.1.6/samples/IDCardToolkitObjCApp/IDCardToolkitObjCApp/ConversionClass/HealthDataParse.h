//
//  HealthDataParse.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship  on 09/08/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship . All rights reserved.
//

#import <Foundation/Foundation.h>
#import "Global.h"

NS_ASSUME_NONNULL_BEGIN

@interface HealthDataParse : NSObject
@property (nonatomic, strong) NSMutableArray *resourceTypeArr;
@property (nonatomic, strong) NSMutableArray *resourceTypeDetailsArr;

-(void)getAllergyResourceDetails:(AllergyResource *)allergyResource;
-(void)getDiagnosisResourceDetails:(DiagnosisResource *)diagnosisResource;
-(void)getBloodGroupResourceDetails:(BloodGroupResource *)bloodGroupResource;
-(void)getInsuranceResourceDetails:(InsuranceResource *)insuranceResource;
-(void)getOrganDonorDetails:(HealthDataContainer *)healthDataContainer;
-(NSMutableArray *)resourceTypeSections;
-(NSMutableArray *)resourceTypeSectionCells;

@end

NS_ASSUME_NONNULL_END
