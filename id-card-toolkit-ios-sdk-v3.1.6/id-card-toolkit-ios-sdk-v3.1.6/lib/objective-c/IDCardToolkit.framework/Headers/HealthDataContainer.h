//
//  HealthDataContainer.h
//  IDCardToolkit
//
//  Created by Federal Authority For Identity and Citizenship on 08/08/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitXmlDataObject.h"
#import "AllergyResource.h"
#import "BloodGroupResource.h"
#import "DiagnosisResource.h"
#import "InsuranceResource.h"

NS_ASSUME_NONNULL_BEGIN

@interface HealthDataContainer : ToolkitXmlDataObject
-(id)initWithHealthDataContainer:(id)healthData;
-(NSArray *)getAllergyResource;
-(NSArray *)getDiagnosisResource;
-(BloodGroupResource *)getBloodGroupResource;
-(InsuranceResource *)getInsuranceResource;
-(NSString *)getOrganDonor;
@end


NS_ASSUME_NONNULL_END
