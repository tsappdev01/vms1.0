//
//  FamilyBookViewController.h
//  
//
//  Created  on 5/14/17.
//  Copyright © 2017 v. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface FamilyBookViewController : UIViewController<UITableViewDelegate,UITableViewDataSource>
    
@property (strong, nonatomic) IBOutlet UIView *familyBookEnterPinView;
@property (weak, nonatomic) IBOutlet UITextField *pintext;
@property (strong, nonatomic) IBOutlet UITableView *listTable;
@property (strong, nonatomic) IBOutlet UIButton *readfamilyBookButton;
@property (nonatomic, strong) Model *model;
- (IBAction)readfamilyBookButtonAction:(id)sender;
@end
