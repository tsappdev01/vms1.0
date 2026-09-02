//
//  Wife.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 17/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitXmlDataObject.h"

@interface Wife : ToolkitXmlDataObject

-(id)initWithWifeDetails:(NSDictionary *)wifeData;
-(NSString *)getWifeIdn;
-(NSString *)getFullNameArabic;
-(NSString *)getFullNameEnglish;
-(NSString *)getNationalityCode;
-(NSString *)getNationalityArabic;
-(NSString *)getNationalityEnglish;
@end
