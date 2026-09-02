//
//  BloodGroupResource.h
//  IDCardToolkit
//
//  Created by Federal Authority For Identity and Citizenship on 08/08/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitXmlDataObject.h"

NS_ASSUME_NONNULL_BEGIN

@interface BloodGroupResource : ToolkitXmlDataObject
-(id)initWithBloodGroupResource:(NSDictionary *)bloodGroupResource;
-(NSString *)getResourceType;
-(NSString *)getBloodGroup;
-(NSString *)getRecordedDate;
@end

NS_ASSUME_NONNULL_END
