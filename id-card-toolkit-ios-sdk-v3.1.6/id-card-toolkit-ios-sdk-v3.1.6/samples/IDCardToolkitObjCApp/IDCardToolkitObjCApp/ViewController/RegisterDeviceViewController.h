//
//  RegisterDeviceViewController.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 13/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface RegisterDeviceViewController : UIViewController<UITextFieldDelegate>

@property (weak, nonatomic) IBOutlet UITextField *userIdText;
@property (weak, nonatomic) IBOutlet UITextField *passwordText;
@property (weak, nonatomic) IBOutlet UITextField *deviceRefText;
@property (strong, nonatomic) IBOutlet UIButton *registerDeviceButton;
@property (weak, nonatomic) IBOutlet UILabel *deviceIDText;
@property (weak, nonatomic) IBOutlet UILabel *deviceRegisIDText;
- (IBAction)registerDeviceButtonAction:(id)sender;
@property (nonatomic, strong) Model *model;
@end
