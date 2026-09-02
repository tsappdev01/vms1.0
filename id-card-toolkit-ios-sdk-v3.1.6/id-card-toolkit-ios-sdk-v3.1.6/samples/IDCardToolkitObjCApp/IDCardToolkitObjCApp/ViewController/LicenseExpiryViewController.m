//
//  LicenseExpiryViewController.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship  on 06/06/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship . All rights reserved.
//

#import "LicenseExpiryViewController.h"

@interface LicenseExpiryViewController ()

@end

@implementation LicenseExpiryViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.title=@"License Expiry date";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    self.licenseExpiryDateButton.layer.cornerRadius=22.0;
    [self.licenseExpiryDateButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.licenseExpiryDateButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.licenseExpiryDateButton.layer setShadowOpacity:0.5];
   // self.licenseExpiryDateButton.layer.borderWidth=1.0;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    
    self.licenseExpirydateLabel.hidden = YES;
    // Do any additional setup after loading the view.
}
- (IBAction)licenseExpiryDateButtonAction:(id)sender {
    
    dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
        @try {
            
            Toolkit *toolkit = self.model.toolkitShared;
            NSString *licExpiryStr = [toolkit getLicenseExpiryDate];
            
            dispatch_async(dispatch_get_main_queue(), ^{
                self.licenseExpirydateLabel.hidden = NO;
                self.licenseExpirydateLabel.text=licExpiryStr;
            });// update UI main queue
        }//try
        @catch (NSException *exception) {
            NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
            [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
        }// catch
        @finally {
            dispatch_async(dispatch_get_main_queue(), ^{
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
