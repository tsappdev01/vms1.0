//
//  FamilyBookParse.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 19/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "Global.h"

@interface FamilyBookParse : NSObject

@property (nonatomic, strong) NSMutableArray *familyBookKeyElements;
@property (nonatomic, strong) NSMutableArray *familyBookValueElements;

-(void)getHeadOfFamilyDetails:(HeadOfFamily *)headofFamily;
-(void)getWifeDetails:(Wife *)wife index:(int)index;
-(void)getChildDetails:(Child *)child index:(int)index;

-(NSMutableArray *)familyBookKey;
-(NSMutableArray *)familyBookValue;
@end

