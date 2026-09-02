//
//  DiagnosisResource.h
//  IDCardToolkit
//
//  Created by Federal Authority For Identity and Citizenship on 08/08/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitXmlDataObject.h"

NS_ASSUME_NONNULL_BEGIN

@interface DiagnosisResource : ToolkitXmlDataObject
-(id)initWithDiagnosisResource:(NSDictionary *)diagnosisResource;
-(NSString *)getResourceType;
-(NSString *)getDiagnosisCode;
-(NSString *)getDiagnosisDescription;
-(NSString *)getDiagnosisRecordedDate;
@end

NS_ASSUME_NONNULL_END
