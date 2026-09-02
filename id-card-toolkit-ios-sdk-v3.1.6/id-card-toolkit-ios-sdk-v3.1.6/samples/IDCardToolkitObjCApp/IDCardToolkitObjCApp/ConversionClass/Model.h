//
//  Utils.h
//
//  Created by Federal Authority For Identity and Citizenship  on 1/29/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@interface Model : NSObject

@property(nonatomic,strong)NSString *mVGurl;
@property(nonatomic,strong)NSString *mConfigUrl;
@property(nonatomic,strong)NSString *mLogDir;
@property(nonatomic,strong)id toolkitShared;
@property(nonatomic,strong)NSMutableArray *cardReaderSharedArray;
@property(nonatomic,strong)NSMutableArray *selectedFingerSharedArray;
@property(nonatomic,strong)id dataprotectedShared;
@end

