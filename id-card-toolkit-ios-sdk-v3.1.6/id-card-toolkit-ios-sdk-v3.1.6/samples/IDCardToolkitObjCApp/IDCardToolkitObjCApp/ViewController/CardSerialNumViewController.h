//
//  CardSerialNumViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship  on 08/05/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship . All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

NS_ASSUME_NONNULL_BEGIN

@interface CardSerialNumViewController : UIViewController
@property (weak, nonatomic) IBOutlet UILabel *cardSerialNumberLabel;
@property (strong, nonatomic) IBOutlet UIButton *getCsnButton;
@property (nonatomic, strong) Model *model;
- (IBAction)getCsnButtonAction:(id)sender;
@end

NS_ASSUME_NONNULL_END
