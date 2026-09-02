//
//  PKIAuthViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship  on 1/24/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "PKIAuthViewController.h"

@interface PKIAuthViewController ()
@property(nonatomic,strong)Utils * utils;
@end

@implementation PKIAuthViewController
#pragma mark viewDidLoad

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.verifyPInText.keyboardType=UIKeyboardTypeNumberPad;
    self.verifyPInText.keyboardAppearance = UIKeyboardAppearanceLight;
    UIToolbar* enterPinNumberToolbar = [[UIToolbar alloc]initWithFrame:CGRectMake(0, 0, 320, 50)];
    enterPinNumberToolbar.barStyle = UIBarStyleBlackTranslucent;
    enterPinNumberToolbar.items = @[[[UIBarButtonItem alloc]initWithTitle:@"Cancel" style:UIBarButtonItemStyleDone target:self action:@selector(enterPinCancelNumberPad)],
                                    [[UIBarButtonItem alloc]initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil],
                                    [[UIBarButtonItem alloc]initWithTitle:@"Done" style:UIBarButtonItemStyleDone target:self action:@selector(enterPinDoneWithNumberPad)]];
    [enterPinNumberToolbar sizeToFit];
    self.verifyPInText.inputAccessoryView = enterPinNumberToolbar;
    
    self.title=@"Authenticate PKI";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    self.pkiAuthByVerifyPinButton.layer.cornerRadius=22.0;
    [self.pkiAuthByVerifyPinButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.pkiAuthByVerifyPinButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.pkiAuthByVerifyPinButton.layer setShadowOpacity:0.5];
//    self.pkiAuthByVerifyPinButton.layer.borderWidth=1.0;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    
    // Do any additional setup after loading the view.
}
#pragma mark viewWillAppear

-(void)viewWillAppear:(BOOL)animated {
    
    [super viewWillAppear:YES];
    self.verifyPInText.text=@"";
    [self.verifyPInText resignFirstResponder];
}
#pragma mark EIDA Toolkit calling functions

- (IBAction)pkiAuthByVerifyPinButtonAction:(id)sender {
    
    [self.verifyPInText resignFirstResponder];
    if ([self.verifyPInText.text length]==0) {
        [AlertView showAlertTitle:ALERT withMessage:ENTER_PIN onView:self];
    }// if
    else {
        if ([self.verifyPInText.text length]<4) {
            [AlertView showAlertTitle:ALERT withMessage:MIN_PIN onView:self];
            self.verifyPInText.text=@"";
        }// if
        else if ([self.verifyPInText.text length]>16) {
            [AlertView showAlertTitle:ALERT withMessage:MAX_PIN onView:self];
            self.verifyPInText.text=@"";
        }// else if
        else {
        [self.utils ShowProgressBar:PKIAUTH andView:self.view];
        [self.verifyPInText resignFirstResponder];
        NSString *requestId=[Utils generateSecureKey];
        dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
        @try {

            CardReader *cardreader  = [self.model.cardReaderSharedArray objectAtIndex:0];
            DataProtectionKey *data_protection_key=self.model.dataprotectedShared;
            
            NSString *request_handle =  [cardreader prepareRequest:requestId];
            if (request_handle.length==0 || [request_handle isEqualToString:@""] || [request_handle isEqualToString:@"(null)"] ) {
                [AlertView showAlertTitle:ALERT withMessage:REQUEST_HANDLE_EMPTY onView:self];
            }//if
            else {
                NSString *encodePin=[Utils setEncrytion:request_handle data:self.verifyPInText.text publickey:[data_protection_key getPublicKey] keylength:[data_protection_key getKeyLength]];
                if (encodePin.length==0 || [encodePin isEqualToString:@""] || [encodePin isEqualToString:@"(null)"] ) {
                    [AlertView showAlertTitle:ALERT withMessage:ENCODE_PIN_EMPTY onView:self];
                }//if
                else {
                    ToolkitResponse *response = [cardreader authenticatePki:encodePin];
                    dispatch_async(dispatch_get_main_queue(), ^{
                        NSString *xmlString =[response getXmlString];
                        NSString *errorMsg =  [self.utils validateToolkitResponse:requestId xmlstring:xmlString];
                        if (![errorMsg isEqualToString:@""] || errorMsg.length>0) {
                            [AlertView showAlertTitle:ALERT withMessage:errorMsg onView:self];
                        }//if
                        else {
                            NSDictionary *responseDic =[response getResponseDataElement];
                            NSString *matchStr=[responseDic objectForKey:@"CompleteAuthenticationStatus"];
                            NSString *responseStatusStr=[responseDic objectForKey:@"ResponseStatus"];
                            [AlertView showAlertTitle:ALERT withMessage:[NSString stringWithFormat:@"%@ %@",matchStr,responseStatusStr] onView:self];
                         }//else
                     });// update UI main queue
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
                    self.verifyPInText.text=@"";
                });// update UI main queue
            }// finally
        });// background queue
     }//else
  }//else
}

#pragma mark Updating UI Methods
- (void)goBack {
    [self.navigationController popViewControllerAnimated:YES];
}
-(void)enterPinCancelNumberPad {
    [self.verifyPInText resignFirstResponder];
    self.verifyPInText.text = @"";
}
-(void)enterPinDoneWithNumberPad {
    [self.verifyPInText resignFirstResponder];
}

#pragma mark didReceiveMemoryWarning
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
