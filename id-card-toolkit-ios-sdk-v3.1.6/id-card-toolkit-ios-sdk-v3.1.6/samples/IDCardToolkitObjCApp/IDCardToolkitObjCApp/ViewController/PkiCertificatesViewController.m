//
//  PkiCertificatesViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship on 14/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "PkiCertificatesViewController.h"

@interface PkiCertificatesViewController ()
@property(nonatomic,strong)Utils * utils;
@end

@implementation PkiCertificatesViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.title=@"Pki Certificates";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    self.pkiCertificatesButton.layer.cornerRadius=22.0;
    [self.pkiCertificatesButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.pkiCertificatesButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.pkiCertificatesButton.layer setShadowOpacity:0.5];
   // self.pkiCertificatesButton.layer.borderWidth=1.0;
    
    self.pintext.keyboardType=UIKeyboardTypeNumberPad;
    self.pintext.keyboardAppearance = UIKeyboardAppearanceLight;
    UIToolbar* enterPinNumberToolbar = [[UIToolbar alloc]initWithFrame:CGRectMake(0, 0, 320, 50)];
    enterPinNumberToolbar.barStyle = UIBarStyleBlackTranslucent;
    enterPinNumberToolbar.items = @[[[UIBarButtonItem alloc]initWithTitle:@"Cancel" style:UIBarButtonItemStyleDone target:self action:@selector(enterPinCancelNumberPad)],
                                    [[UIBarButtonItem alloc]initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil],
                                    [[UIBarButtonItem alloc]initWithTitle:@"Done" style:UIBarButtonItemStyleDone target:self action:@selector(enterPinDoneWithNumberPad)]];
    [enterPinNumberToolbar sizeToFit];
    self.pintext.inputAccessoryView = enterPinNumberToolbar;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    
    // Do any additional setup after loading the view.
}
-(void)viewWillAppear:(BOOL)animated {
    [super viewWillAppear:YES];
    
    self.pintext.text=@"";
}
- (IBAction)pkiCertificatesButtonAction:(id)sender {
    
    if ([self.pintext.text length]==0) {
        [AlertView showAlertTitle:ALERT withMessage:ENTER_PIN onView:self];
    }// if
    else {
        if ([self.pintext.text length]<4) {
            [AlertView showAlertTitle:ALERT withMessage:MIN_PIN onView:self];
            self.pintext.text=@"";
        }// if
        else if ([self.pintext.text length]>16) {
            [AlertView showAlertTitle:ALERT withMessage:MAX_PIN onView:self];
            self.pintext.text=@"";
        }// else if
     else {
        [self.utils ShowProgressBar:FETCH_CERTIFICATES andView:self.view];
        [self.pintext setUserInteractionEnabled:YES];
        [self.pintext resignFirstResponder];
            
        NSString *requestId=[Utils generateSecureKey];
        NSString *pinData = self.pintext.text;
        dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_HIGH, 0), ^(void){
        @try {
            
            CardReader *cardreader  = [self.model.cardReaderSharedArray objectAtIndex:0];
            DataProtectionKey *data_protection_key=self.model.dataprotectedShared;
            
            NSString *request_handle =  [cardreader prepareRequest:requestId];
            if (request_handle.length==0 || [request_handle isEqualToString:@""] || [request_handle isEqualToString:@"(null)"] ) {
                [AlertView showAlertTitle:ALERT withMessage:REQUEST_HANDLE_EMPTY onView:self];
            }//if
            else {
                NSString *encodePin=[Utils setEncrytion:request_handle data:pinData publickey:[data_protection_key getPublicKey] keylength:[data_protection_key getKeyLength]];
                if (encodePin.length==0 || [encodePin isEqualToString:@""] || [encodePin isEqualToString:@"(null)"] ) {
                    [AlertView showAlertTitle:ALERT withMessage:ENCODE_PIN_EMPTY onView:self];
                }//if
                else {
                    CardCertificates *certificates = [cardreader getPkiCertificates:encodePin];
    
                    dispatch_async(dispatch_get_main_queue(), ^{
                    NSString *xmlString =[certificates getXmlString];
                    NSString *errorMsg =  [self.utils validateToolkitResponse:requestId xmlstring:xmlString];
                    if (![errorMsg isEqualToString:@""] || errorMsg.length>0) {
                        [AlertView showAlertTitle:ALERT withMessage:errorMsg onView:self];
                    }//if
                    else {
                        NSData *authCerData = [[NSData alloc] initWithBytes: [certificates getAuthenticationCertificate] length:[certificates getAuthenticationCertificateLength]];
                        NSString *authStr=[Utils base64forData:authCerData];
                
                        NSData *signCerdata = [[NSData alloc] initWithBytes: [certificates getSigningCertificate] length:[certificates getSigningCertificateLength]];
                        NSString *signStr=[Utils base64forData:signCerdata];
                                
                        self.PinView.hidden=YES;
                        self.authCertificateText.text=[NSString stringWithFormat:@"%@",authStr];
                        self.signCertificateText.text=[NSString stringWithFormat:@"%@",signStr];
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
                }//else
            }// catch
            @finally {
                dispatch_async(dispatch_get_main_queue(), ^{
                    [self.utils DismissProgressBar];
                     self.pintext.text=@"";
                });// update UI main queue
              }// finally
          });//background queue
       }//else
    }//else
}
- (BOOL)textFieldShouldReturn:(UITextField *)textField {
    [self.pintext setUserInteractionEnabled:YES];
    [self.pintext resignFirstResponder];
    return YES;
}
- (void)goBack {
    [self.navigationController popViewControllerAnimated:YES];
}
-(void)enterPinCancelNumberPad {
    [self.pintext resignFirstResponder];
    self.pintext.text = @"";
}
-(void)enterPinDoneWithNumberPad {
    [self.pintext resignFirstResponder];
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
