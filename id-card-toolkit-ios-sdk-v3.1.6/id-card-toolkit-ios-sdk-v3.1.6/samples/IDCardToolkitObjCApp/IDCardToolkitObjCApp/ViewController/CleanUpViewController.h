//
//  CleanUpViewController.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 15/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface CleanUpViewController : UIViewController
@property (weak, nonatomic) IBOutlet UILabel *cleanUpStatus;
@property (strong, nonatomic) IBOutlet UIButton *cleanUpButton;
@property (nonatomic, strong) Model *model;
- (IBAction)cleanUpButtonAction:(id)sender;
@end
