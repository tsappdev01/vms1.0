//
//  FingerDataViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 20/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface FingerDataViewController : UIViewController<UIPickerViewDataSource,UIPickerViewDelegate> {
    
    NSMutableArray *getFingerIndexarr;
    NSMutableArray *getFingerIndex_reference_arr;
}
@property (strong, nonatomic) IBOutlet UIPickerView *fingerPickerView;
@property (strong, nonatomic) IBOutlet UIButton *getFingerDataButton;
@property (nonatomic, strong) Model *model;
- (IBAction)getFingerDataButton:(id)sender;
@end
