//
//  CheckCardViewController.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 14/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface CheckCardViewController : UIViewController

@property (weak, nonatomic) IBOutlet UILabel *cardstatusText;
@property (strong, nonatomic) IBOutlet UIButton *checkCardStatusButton;
@property (nonatomic, strong) Model *model;
- (IBAction)checkCardStatusButtonAction:(id)sender;

@end
