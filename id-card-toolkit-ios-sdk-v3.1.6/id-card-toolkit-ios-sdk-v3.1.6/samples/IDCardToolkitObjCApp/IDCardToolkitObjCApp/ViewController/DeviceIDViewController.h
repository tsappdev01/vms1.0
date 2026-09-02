//
//  DeviceIDViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 02/01/18.
//  Copyright © 2018 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface DeviceIDViewController : UIViewController
@property (strong, nonatomic) IBOutlet UITextView *deviceTextview;
@property (strong, nonatomic) IBOutlet UIButton *getDeviceIDButton;
@property (nonatomic, strong) Model *model;
- (IBAction)getDeviceIDButtonAction:(id)sender;
@end

