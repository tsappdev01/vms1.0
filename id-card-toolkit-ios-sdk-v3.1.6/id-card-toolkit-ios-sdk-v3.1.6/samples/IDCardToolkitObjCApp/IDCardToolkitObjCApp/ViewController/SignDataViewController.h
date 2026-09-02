//
//  SignDataViewController.h
//  
//
//  Created by Federal Authority For Identity and Citizenship  on 1/23/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface SignDataViewController : UIViewController<UITextFieldDelegate,UITextViewDelegate> {
    int SwitchValue;
}

@property (strong, nonatomic) IBOutlet UITextView *enterDataTextView;
@property (strong, nonatomic) IBOutlet UITextField *enterPinText;
@property (strong, nonatomic) IBOutlet UITextView *digitalSignTextView;
@property (weak, nonatomic) IBOutlet UILabel *authKey;
@property (strong, nonatomic) IBOutlet UISwitch *swithButton;
@property (strong, nonatomic) IBOutlet UIButton *signDataButton;
@property (strong, nonatomic) IBOutlet UIButton *verifySignatureButton;
@property (nonatomic, strong) Model *model;
- (IBAction)signDataButtonAction:(id)sender;
- (IBAction)verifySignatureButtonAction:(id)sender;
- (IBAction)swithButton:(id)sender;


@end
