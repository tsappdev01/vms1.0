//
//  SetNFCdetailsViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 17/10/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"
#import <CoreNFC/CoreNFC.h>

NS_ASSUME_NONNULL_BEGIN

@interface SetNFCdetailsViewController : UIViewController
@property (weak, nonatomic) IBOutlet UITextField *cardNumberText;
@property (weak, nonatomic) IBOutlet UITextField *dobYYtext;
@property (weak, nonatomic) IBOutlet UITextField *dobMMText;
@property (weak, nonatomic) IBOutlet UITextField *dobDDText;
@property (weak, nonatomic) IBOutlet UITextField *expiryDateYYText;
@property (weak, nonatomic) IBOutlet UITextField *expiryDateMMText;
@property (weak, nonatomic) IBOutlet UITextField *expiryDateDDText;
@property (weak, nonatomic) IBOutlet UIButton *nfcReadpublicdataButton;
- (IBAction)nfcReadPublicDataButtonAction:(id)sender;
@property (nonatomic, strong) Model *model;
@end

NS_ASSUME_NONNULL_END
