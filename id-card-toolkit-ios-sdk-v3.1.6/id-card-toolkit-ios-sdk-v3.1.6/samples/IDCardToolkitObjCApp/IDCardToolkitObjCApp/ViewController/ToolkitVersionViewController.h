//
//  ToolkitVersionViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 20/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface ToolkitVersionViewController : UIViewController
@property (weak, nonatomic) IBOutlet UILabel *toolkitVersionLabel;
@property (strong, nonatomic) IBOutlet UIButton *toolkitVersionButton;
@property (nonatomic, strong) Model *model;
- (IBAction)toolkitVersionButtonAction:(id)sender;

@end
