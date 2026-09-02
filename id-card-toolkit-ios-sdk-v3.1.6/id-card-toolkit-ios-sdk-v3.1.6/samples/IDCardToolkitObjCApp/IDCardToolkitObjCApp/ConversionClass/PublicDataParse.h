//
//  PublicDataParse.h
//  EIDAToolkitObjCTestapp
//
//  Created by Federal Authority For Identity and Citizenship on 07/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "Global.h"

@interface PublicDataParse : NSObject

@property (nonatomic, strong) NSMutableDictionary *pModDataDictionary;
@property (nonatomic, strong) NSMutableDictionary *pNonModDataDictionary;
@property (nonatomic, strong) NSMutableDictionary *pHomeAddressDictionary;
@property (nonatomic, strong) NSMutableDictionary *pWorkAddressDictionary;

-(NSMutableDictionary *)getModifiablePublicDataDetails:(ModifiablePublicData *)modData;
-(NSMutableDictionary *)getNonModifiablePublicDataDetails:(NonModifiablePublicData *)nonModData;
-(NSMutableDictionary *)getHomeAddressDetails:(HomeAddress *)homeaddress;
-(NSMutableDictionary *)getWorkAddressDetails:(WorkAddress *)workAddress;

@end
