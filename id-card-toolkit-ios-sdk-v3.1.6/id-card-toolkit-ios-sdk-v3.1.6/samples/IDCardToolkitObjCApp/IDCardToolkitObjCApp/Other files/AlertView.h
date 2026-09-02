//
//  AlertView.h
//  
//
//  Created by Federal Authority For Identity and Citizenship  on 1/19/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>

#import <UIKit/UIKit.h>

@interface AlertView : NSObject

+ (void)showAlertTitle:(NSString *)title withMessage:(NSString *)message onView:(UIViewController *)viewController;



@end
