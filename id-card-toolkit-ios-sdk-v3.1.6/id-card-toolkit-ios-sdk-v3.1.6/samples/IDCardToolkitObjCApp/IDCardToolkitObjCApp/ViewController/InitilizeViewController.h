//
//  InitilizeViewController.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 10/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface InitilizeViewController : UIViewController<UITextFieldDelegate> 

@property (strong, nonatomic) IBOutlet UILabel *statusLabel;
@property (strong, nonatomic) IBOutlet UIButton *initlizeButton;
@property (weak, nonatomic) IBOutlet UITextField *vgurlText;
@property (weak, nonatomic) IBOutlet UITextField *configurlText;
@property (weak, nonatomic) IBOutlet UITextField *logDirText;
@property (strong, nonatomic) IBOutlet UISwitch *customSwitch;
@property (weak, nonatomic) IBOutlet UILabel *switchLabel;
@property (weak, nonatomic) IBOutlet UILabel *configPath;

- (IBAction)customSwitchAction:(id)sender;

- (IBAction)initlizeButtonAction:(id)sender;
@property(nonatomic,strong)Toolkit *toolkit;
@property (nonatomic, strong) Model *model;

@end
