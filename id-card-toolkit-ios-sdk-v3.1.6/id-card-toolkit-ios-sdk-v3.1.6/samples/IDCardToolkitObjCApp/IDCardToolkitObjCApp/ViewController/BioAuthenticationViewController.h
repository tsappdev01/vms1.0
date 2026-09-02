//
//  BioAuthenticationViewController.h
//  
//
//  Created by Federal Authority For Identity and Citizenship  on 1/18/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface BioAuthenticationViewController : UIViewController

@property (strong, nonatomic) IBOutlet UIButton *verifyBiometricButton;
@property (nonatomic, strong) Model *model;
- (IBAction)verifyBiometricButtonAction:(id)sender;
//@property(nonatomic,strong)Toolkit *toolkit;

@end
