//
//  CheckAndAuthBioViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship  on 14/03/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship . All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"


NS_ASSUME_NONNULL_BEGIN

@interface CheckAndAuthBioViewController : UIViewController
@property (weak, nonatomic) IBOutlet UIButton *checkAndAuthBioButton;
- (IBAction)checkAndAuthBioButtonAction:(id)sender;
@property (nonatomic, strong) Model *model;
@end

NS_ASSUME_NONNULL_END
