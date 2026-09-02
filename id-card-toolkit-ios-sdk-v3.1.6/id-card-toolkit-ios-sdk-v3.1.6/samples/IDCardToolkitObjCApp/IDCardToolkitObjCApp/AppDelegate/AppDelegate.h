//
//  AppDelegate.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 12/28/16.
//  Copyright © 2016 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface AppDelegate : UIResponder <UIApplicationDelegate>

@property (strong, nonatomic) UIWindow *window;
@property (nonatomic,strong) Utils *utils;
@property (nonatomic, strong) Model *model;
@end

