//
//  NFCPublicDataViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 14/10/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

NS_ASSUME_NONNULL_BEGIN

@interface NFCPublicDataViewController : UIViewController<UITableViewDelegate,UITableViewDataSource,UITabBarControllerDelegate,UIAlertViewDelegate> {
    
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
@property (strong, nonatomic) IBOutlet UITableView *listTableview;
@property (weak, nonatomic) IBOutlet UIImageView *photoImage;
@property (weak, nonatomic) IBOutlet UIImageView *signImage;
-(void)getPublicaDataClass:(CardPublicData *)cardPublicData;
@property (nonatomic, strong) Model *model;
@end

NS_ASSUME_NONNULL_END
