//
//  UnblockPINViewController.h
//  
//
//  Created by Federal Authority For Identity and Citizenship  on 5/29/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface UnblockPINViewController : UIViewController

@property (weak, nonatomic) IBOutlet UITextField *pintext;
@property (strong, nonatomic) IBOutlet UIButton *unblockPinButton;
@property (nonatomic, strong) Model *model;
- (IBAction)unblockPinButtonAction:(id)sender;
@end
