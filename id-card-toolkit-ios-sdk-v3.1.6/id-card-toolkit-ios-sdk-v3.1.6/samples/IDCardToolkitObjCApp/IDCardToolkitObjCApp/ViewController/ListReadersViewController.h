//
//  ListReadersViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 20/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface ListReadersViewController : UIViewController<UIPickerViewDataSource,UIPickerViewDelegate>
@property (strong, nonatomic) IBOutlet UIPickerView *listReaderPickerView;
@property (strong, nonatomic) IBOutlet UIButton *listReaderButton;
- (IBAction)listReaderButtonAction:(id)sender;
@property (nonatomic, strong) Model *model;
@end
