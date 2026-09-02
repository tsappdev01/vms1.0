//
//  DisconnectViewController.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 15/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface DisconnectViewController : UIViewController
@property (weak, nonatomic) IBOutlet UILabel *disconnectStatus;
@property (strong, nonatomic) IBOutlet UIButton *disconnectReaderbutton;
- (IBAction)disconnectReaderbuttonAction:(id)sender;
@property (nonatomic, strong) Model *model;
@end
