//
//  AlertView.m
//
//
//  Created by Federal Authority For Identity and Citizenship  on 1/19/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "AlertView.h"

@implementation AlertView

+ (void)showAlertTitle:(NSString *)title withMessage:(NSString *)message onView:(UIViewController *)viewController
{
    dispatch_async(dispatch_get_main_queue(), ^{
   
    UIAlertController * alert=   [UIAlertController
                                  alertControllerWithTitle:title
                                  message:message
                                  preferredStyle:UIAlertControllerStyleAlert];
    
    UIAlertAction* okButton = [UIAlertAction
                               actionWithTitle:@"OK"
                               style:UIAlertActionStyleDefault
                               handler:^(UIAlertAction * action)
                               {
                                   [alert dismissViewControllerAnimated:YES completion:nil];
                                   
                               }];
    
    [alert addAction:okButton];
    
    [viewController presentViewController:alert animated:YES completion:nil];
    
   });// update UI main queue
}
@end
