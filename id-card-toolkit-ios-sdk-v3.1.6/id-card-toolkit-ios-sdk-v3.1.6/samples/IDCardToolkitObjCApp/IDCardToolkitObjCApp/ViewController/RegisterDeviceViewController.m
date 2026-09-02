//
//  RegisterDeviceViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship on 13/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "RegisterDeviceViewController.h"

@interface RegisterDeviceViewController ()
@property(nonatomic,strong) Utils * utils;
@end

@implementation RegisterDeviceViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.deviceIDText.hidden=YES;
    self.deviceRegisIDText.hidden=YES;
    self.deviceRegisIDText.adjustsFontSizeToFitWidth=YES;

    self.title=@"Register Device";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    
    self.registerDeviceButton.layer.cornerRadius=22.0;
    [self.registerDeviceButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.registerDeviceButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.registerDeviceButton.layer setShadowOpacity:0.5];
//    self.registerDeviceButton.layer.borderWidth=1.0;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    self.userIdText.text=@"";
    self.passwordText.text=@"";
    self.deviceRefText.text=@"";
    
    // Do any additional setup after loading the view.
}
- (IBAction)registerDeviceButtonAction:(id)sender {

    if ([self.userIdText.text length]==0) {
        [AlertView showAlertTitle:ALERT withMessage:ENTER_DEVICE_USERNAME onView:self ];
    } // if
    else if ([self.passwordText.text length]==0) {
        [AlertView showAlertTitle:ALERT withMessage:ENTER_DEVICE_PASSWORD onView:self];
    }//else if
    else if ([self.deviceRefText.text length]==0) {
        [AlertView showAlertTitle:ALERT withMessage:ENTER_DEVICE_REF_ID onView:self];
    }// else if
    else {
        [self.userIdText setUserInteractionEnabled:YES];
        [self.userIdText resignFirstResponder];
        [self.passwordText setUserInteractionEnabled:YES];
        [self.passwordText resignFirstResponder];
        [self.deviceRefText setUserInteractionEnabled:YES];
        [self.deviceRefText resignFirstResponder];

    [self.utils ShowProgressBar:LOADINGDATA andView:self.view];
    NSString *requestId=[Utils generateSecureKey];
    dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
    @try {
        Toolkit *toolkit = self.model.toolkitShared;
        DataProtectionKey *data_protection_key=self.model.dataprotectedShared;
        
        NSString *request_handle =  [toolkit prepareRequest:requestId];
        if (request_handle.length==0 || [request_handle isEqualToString:@""] || [request_handle isEqualToString:@"(null)"] ) {
            [AlertView showAlertTitle:ALERT withMessage:REQUEST_HANDLE_EMPTY onView:self];
        }//if
        else {
            NSString *userIdText=[Utils setEncrytion:request_handle data:self.userIdText.text publickey:[data_protection_key getPublicKey] keylength:[data_protection_key getKeyLength]];
            if (userIdText.length==0 || [userIdText isEqualToString:@""] || [userIdText isEqualToString:@"(null)"] ) {
                [AlertView showAlertTitle:ALERT withMessage:ENCODE_USERID_EMPTY onView:self];
            }//if
            else {
                NSString *passwordText=[Utils setEncrytion:request_handle data:self.passwordText.text publickey:[data_protection_key getPublicKey] keylength:[data_protection_key getKeyLength]];
                if (passwordText.length==0 || [passwordText isEqualToString:@""] || [passwordText isEqualToString:@"(null)"] ) {
                    [AlertView showAlertTitle:ALERT withMessage:ENCODE_PASSWORD_EMPTY onView:self];
                }//if
                else {
                    RegisterDeviceResponse *registerDevice=[toolkit registerDevice:userIdText encodedPassword:passwordText deviceReferenceId:self.deviceRefText.text];
                    
                dispatch_async(dispatch_get_main_queue(), ^{
                    NSString *xmlString =[registerDevice getXmlString];
                    NSString *errorMsg =  [self.utils validateToolkitResponse:requestId xmlstring:xmlString];
                    if (![errorMsg isEqualToString:@""] || errorMsg.length>0) {
                        [AlertView showAlertTitle:ALERT withMessage:errorMsg onView:self];
                    }//if
                    else {
                        NSString *deviceid = [registerDevice getDeviceRegistrationId];
                        self.deviceIDText.hidden=NO;
                        self.deviceRegisIDText.hidden=NO;
                        self.deviceRegisIDText.text=deviceid;
                        }//else
                     });// update UI main queue
                  }//else
               }//else
            }//else
        }//try
        @catch (NSException *exception) {
            NSString *vgresponse =exception.userInfo[@"ErrorResponse"];
            if (![vgresponse isEqualToString:@""] && vgresponse.length>0) {
                NSString *errorMsg =  [self.utils validateToolkitResponse:requestId xmlstring:vgresponse];
                if (![errorMsg isEqualToString:@""] && errorMsg.length>0) {
                    [AlertView showAlertTitle:ALERT withMessage:errorMsg onView:self];
                }//if
                else {
                    NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
                    [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
                }//else
            }//if
            else {
                NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
                [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
            }
        }// catch
            @finally {
                dispatch_async(dispatch_get_main_queue(), ^{
                    [self.utils DismissProgressBar];
                    self.userIdText.text=@"";
                    self.passwordText.text=@"";
                    self.deviceRefText.text=@"";
                });// update UI main queue
            }// finally
        });// background queue
    }//else
}
- (BOOL)textFieldShouldReturn:(UITextField *)textField {
    
     [self.userIdText setUserInteractionEnabled:YES];
     [self.userIdText resignFirstResponder];
     [self.passwordText setUserInteractionEnabled:YES];
     [self.passwordText resignFirstResponder];
     [self.deviceRefText setUserInteractionEnabled:YES];
     [self.deviceRefText resignFirstResponder];
    
    return YES;
}
- (void)goBack {
    [self.navigationController popViewControllerAnimated:YES];
}
- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
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
