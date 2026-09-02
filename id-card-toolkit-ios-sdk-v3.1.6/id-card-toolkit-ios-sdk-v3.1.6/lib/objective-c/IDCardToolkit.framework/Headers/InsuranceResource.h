//
//  InsuranceResource.h
//  IDCardToolkit
//
//  Created by Federal Authority For Identity and Citizenship on 08/08/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitXmlDataObject.h"

NS_ASSUME_NONNULL_BEGIN

@interface InsuranceResource : ToolkitXmlDataObject
-(id)initWithInsuranceResource:(NSDictionary *)insuranceResource;
-(NSString *)getResourceType;
-(NSString *)getInsuranceName;
-(NSString *)getInsuranceNumber;
-(NSString *)getInsuranceValidityStartDate;
-(NSString *)getInsuranceValidityEndDate;
@end

NS_ASSUME_NONNULL_END
