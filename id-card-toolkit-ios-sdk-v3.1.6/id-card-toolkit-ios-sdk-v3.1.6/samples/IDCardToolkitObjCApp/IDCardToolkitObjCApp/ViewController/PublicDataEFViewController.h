//
//  PublicDataEFViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 20/11/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>
#import "Global.h"

NS_ASSUME_NONNULL_BEGIN

@interface PublicDataEFViewController : UIViewController<UIPickerViewDataSource,UIPickerViewDelegate>
@property (weak, nonatomic) IBOutlet UIPickerView *pickerViewEFData;
@property (weak, nonatomic) IBOutlet UIButton *readPublicDataEFButton;
@property (nonatomic, strong) Model *model;
- (IBAction)readPublicDataEFButtonAction:(id)sender;
@end

NS_ASSUME_NONNULL_END
