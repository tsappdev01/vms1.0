//
//  DataContainer.h
//  IDCardToolkit
//
//  Created by Federal Authority For Identity and Citizenship on 08/08/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "ToolkitResponse.h"
#import "ToolkitXmlDataObject.h"
#import "HealthDataContainer.h"

NS_ASSUME_NONNULL_BEGIN

@interface DataContainer : ToolkitResponse
-(id)initWithDataContainer:(NSString *)xmlString;
-(HealthDataContainer *)getHealthDataContainer;
@end

NS_ASSUME_NONNULL_END
