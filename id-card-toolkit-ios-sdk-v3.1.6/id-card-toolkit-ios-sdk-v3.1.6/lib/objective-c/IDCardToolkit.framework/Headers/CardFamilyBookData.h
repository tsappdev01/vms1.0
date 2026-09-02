//
//  CardFamilyBookData.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 16/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "ToolkitResponse.h"
#import "HeadOfFamily.h"

@interface CardFamilyBookData : ToolkitResponse

-(id)initWithCardFamilyBookData:(NSString *)xmlString ;
-(HeadOfFamily *)getHeadOfFamily;
-(NSArray *)getWives;
-(NSArray *)getChildren;
@end
