//
//  ConfigCertExpiryDateViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 24/07/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

NS_ASSUME_NONNULL_BEGIN

@interface ConfigCertExpiryDateViewController : UIViewController
@property (weak, nonatomic) IBOutlet UIView *subView;
@property (weak, nonatomic) IBOutlet UILabel *vgCertExpDate;
@property (weak, nonatomic) IBOutlet UILabel *lvCertExpdate;
@property (weak, nonatomic) IBOutlet UILabel *tlsCertExpdate;
@property (weak, nonatomic) IBOutlet UILabel *agCertExpDate;
@property (weak, nonatomic) IBOutlet UILabel *licenseExpdate;

@property (weak, nonatomic) IBOutlet UIButton *getExpiryDateButton;
- (IBAction)getExpiryDatebuttonAction:(id)sender;
@property (nonatomic, strong) Model *model;
@end

NS_ASSUME_NONNULL_END
