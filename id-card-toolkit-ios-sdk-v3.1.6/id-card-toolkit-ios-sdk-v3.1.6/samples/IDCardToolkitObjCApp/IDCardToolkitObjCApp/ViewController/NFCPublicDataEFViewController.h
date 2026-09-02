//
//  NFCPublicDataEFViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 20/11/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <CoreNFC/CoreNFC.h>
#import "Global.h"

NS_ASSUME_NONNULL_BEGIN

@interface NFCPublicDataEFViewController : UIViewController<UIPickerViewDataSource,UIPickerViewDelegate>
@property (weak, nonatomic) IBOutlet UITextField *cardNumberText;
@property (weak, nonatomic) IBOutlet UITextField *dobYYText;
@property (weak, nonatomic) IBOutlet UITextField *dobMMText;
@property (weak, nonatomic) IBOutlet UITextField *dobDDText;
@property (weak, nonatomic) IBOutlet UITextField *expiryDateYYText;
@property (weak, nonatomic) IBOutlet UITextField *expiryDateMMText;
@property (weak, nonatomic) IBOutlet UITextField *expiryDateDDText;
@property (weak, nonatomic) IBOutlet UIPickerView *pickerViewEFData;
@property (weak, nonatomic) IBOutlet UIButton *nfcReadPublicDataEFButton;
@property (nonatomic, strong) Model *model;
- (IBAction)nfcReadPublicDataButtonAction:(id)sender;
@end

NS_ASSUME_NONNULL_END
