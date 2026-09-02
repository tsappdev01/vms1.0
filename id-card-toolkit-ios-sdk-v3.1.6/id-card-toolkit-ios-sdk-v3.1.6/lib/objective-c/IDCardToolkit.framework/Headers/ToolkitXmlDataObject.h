//
//  ToolkitXmlDataObject.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 16/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>

//Assert
@interface ToolkitXmlDataObject : NSObject
+(void)notNull:(NSString *)element elementName:(NSString *)elementName;
+(void)elementTagNameEquals:(NSString *)element expectedTagName:(NSString *)expectedTagName;
+(void)notNullOrEmpty:(NSString *)value name:(NSString *)name;
+(void)notNull:(NSObject *)value name:(NSString *)name;
@end
