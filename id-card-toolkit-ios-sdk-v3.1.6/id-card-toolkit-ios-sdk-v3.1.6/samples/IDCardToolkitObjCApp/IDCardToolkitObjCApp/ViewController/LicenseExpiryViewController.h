//
//  LicenseExpiryViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship  on 06/06/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship . All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

NS_ASSUME_NONNULL_BEGIN

@interface LicenseExpiryViewController : UIViewController
@property (weak, nonatomic) IBOutlet UILabel *licenseExpirydateLabel;
@property (weak, nonatomic) IBOutlet UIButton *licenseExpiryDateButton;
- (IBAction)licenseExpiryDateButtonAction:(id)sender;
@property (nonatomic, strong) Model *model;
@end

NS_ASSUME_NONNULL_END
