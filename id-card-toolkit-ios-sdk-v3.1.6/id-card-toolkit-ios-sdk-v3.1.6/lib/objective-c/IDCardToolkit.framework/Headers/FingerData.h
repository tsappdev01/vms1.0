//
//  FingerData.h
//
//
//  Created by Federal Authority For Identity and Citizenship on 16/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "FingerIndex.h"

@interface FingerData : NSObject

-(id)initWithFingerData:(int)fingerId fingerIndex:(int)fingerIndex;
-(int)getFingerId;
-(enum FingerIndex)getFingerIndex;
@end
