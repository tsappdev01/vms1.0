//
//  PublicDataViewController.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 1/18/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>
#import "Global.h"

@interface PublicDataViewController : UIViewController<UITableViewDelegate,UITableViewDataSource,UITabBarControllerDelegate,UIAlertViewDelegate> {
    
    NSMutableDictionary *getpModDataDictionary;
    NSMutableDictionary *getpNonModDataDictionary;
    NSMutableDictionary *getpHomeAddressDictionary;
    NSMutableDictionary *getpWorkAddressDictionary;
    
    NSMutableArray *pIDNumberKeyarr;
    NSMutableArray *pIDNumberValuearr;
    NSMutableArray *pModDatakeyarr;
    NSMutableArray *pModDataValuearr;
    NSMutableArray *pNonModDatakeyarr;
    NSMutableArray *pNonModDataValuearr;
    NSMutableArray *pHomeAddresskeyarr;
    NSMutableArray *pHomeAddressValuearr;
    NSMutableArray *pWorkAddresskeyarr;
    NSMutableArray *pWorkAddressValuearr;
    
}
@property (strong, nonatomic) IBOutlet UITableView *listTable;
@property (strong, nonatomic) IBOutlet UIImageView *PhotoImage;
@property (strong, nonatomic) IBOutlet UIImageView *signImage;
@property (strong, nonatomic) IBOutlet UIButton *readPublicDataButton;
@property (nonatomic, strong) Model *model;
- (IBAction)readPublicDataButtonAction:(id)sender;




@end
