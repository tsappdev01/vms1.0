//
//  ConfigCertExpiryDateViewController.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 24/07/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ConfigCertExpiryDateViewController.h"

@interface ConfigCertExpiryDateViewController ()
@property(nonatomic,strong)Utils * utils;
@end

@implementation ConfigCertExpiryDateViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.title=@"Config Certificate Expiry Date";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    self.getExpiryDateButton.layer.cornerRadius=22.0;
    [self.getExpiryDateButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.getExpiryDateButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.getExpiryDateButton.layer setShadowOpacity:0.5];
   // self.getExpiryDateButton.layer.borderWidth=1.0;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    // Do any additional setup after loading the view.
}
-(void)viewWillAppear:(BOOL)animated {
    
    [super viewWillAppear:YES];
    self.subView.hidden=YES;
}
- (IBAction)getExpiryDatebuttonAction:(id)sender {
    
    [self.utils ShowProgressBar:LOADINGDATA andView:self.view];

    dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void) {
        @try {
            Toolkit *toolkit = self.model.toolkitShared;
            [toolkit getConfigCertificateExpiryDate];
            
            dispatch_async(dispatch_get_main_queue(), ^{
                self.vgCertExpDate.text=[toolkit getConfig_vg_cert_expiry];
                self.lvCertExpdate.text=[toolkit getConfig_lv_cert_expiry];
                self.tlsCertExpdate.text=[toolkit getServer_tls_cert_expiry];
                self.agCertExpDate.text=[toolkit getConfig_ag_cert_expiry];
                self.licenseExpdate.text=[toolkit getLicense_expiry];
                self.subView.hidden=NO;
            });// update UI main queue
        }//try
        @catch (NSException *exception) {
            NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
            [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
        }// catch
        @finally {
            dispatch_async(dispatch_get_main_queue(), ^{
                [self.utils DismissProgressBar];
            });// update UI main queue
        }// finally
    });// background queue
}
- (void)goBack {
    [self.navigationController popViewControllerAnimated:YES];
}

/*
#pragma mark - Navigation

// In a storyboard-based application, you will often want to do a little preparation before navigation
- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender {
    // Get the new view controller using [segue destinationViewController].
    // Pass the selected object to the new view controller.
}
*/

@end
