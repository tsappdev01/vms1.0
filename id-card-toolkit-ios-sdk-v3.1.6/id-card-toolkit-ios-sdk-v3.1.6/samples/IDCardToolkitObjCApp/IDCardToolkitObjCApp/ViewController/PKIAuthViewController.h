//
//  PKIAuthViewController.h
//  
//
//  Created by Federal Authority For Identity and Citizenship  on 1/24/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface PKIAuthViewController : UIViewController

@property (strong, nonatomic) IBOutlet UITextField *verifyPInText;
@property (strong, nonatomic) IBOutlet UIButton *pkiAuthByVerifyPinButton;
@property (nonatomic, strong) Model *model;
- (IBAction)pkiAuthByVerifyPinButtonAction:(id)sender;
@end
