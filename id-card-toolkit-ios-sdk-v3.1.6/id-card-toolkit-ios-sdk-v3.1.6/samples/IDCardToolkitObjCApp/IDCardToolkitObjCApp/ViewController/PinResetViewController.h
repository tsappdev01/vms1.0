//
//  PinResetViewController.h
//  
//
//  Created by Federal Authority For Identity and Citizenship  on 1/19/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface PinResetViewController : UIViewController

@property (weak, nonatomic) IBOutlet UITextField *enterPin;
@property (weak, nonatomic) IBOutlet UITextField *confirmPin;
@property (strong, nonatomic) IBOutlet UIButton *resetPinButton;
@property (nonatomic, strong) Model *model;
- (IBAction)resetPinButtonAction:(id)sender;
@end
