//
//  ConnectViewController.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 13/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface ConnectViewController : UIViewController

@property (strong, nonatomic) IBOutlet UIButton *connectedDeviceButton;
@property (weak, nonatomic) IBOutlet UILabel *statusLabel;
@property (weak, nonatomic) IBOutlet UILabel *readerName;
- (IBAction)connectedDeviceButtonAction:(id)sender;
@property (nonatomic, strong) Model *model;
@end
