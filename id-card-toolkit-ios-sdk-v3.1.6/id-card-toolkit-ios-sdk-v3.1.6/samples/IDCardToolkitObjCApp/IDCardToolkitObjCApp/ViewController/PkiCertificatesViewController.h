//
//  PkiCertificatesViewController.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 14/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface PkiCertificatesViewController : UIViewController<UITextFieldDelegate>

@property (strong, nonatomic) IBOutlet UIView *PinView;
@property (weak, nonatomic) IBOutlet UITextField *pintext;
@property (strong, nonatomic) IBOutlet UIButton *pkiCertificatesButton;
@property (strong, nonatomic) IBOutlet UITextView *authCertificateText;
@property (strong, nonatomic) IBOutlet UITextView *signCertificateText;
@property (nonatomic, strong) Model *model;
- (IBAction)pkiCertificatesButtonAction:(id)sender;
@end
