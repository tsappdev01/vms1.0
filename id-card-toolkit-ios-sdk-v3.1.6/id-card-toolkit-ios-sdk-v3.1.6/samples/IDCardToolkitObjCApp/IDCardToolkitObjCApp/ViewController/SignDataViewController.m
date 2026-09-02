//
//  SignDataViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship  on 1/23/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "SignDataViewController.h"

@interface SignDataViewController ()  {
    uint8_t *resultSignature;
    int resultSignatureLength;
}
@property(nonatomic,strong)Utils * utils;
@end

@implementation SignDataViewController
#pragma mark viewDidLoad

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.enterPinText.keyboardType=UIKeyboardTypeNumberPad;
    self.enterPinText.keyboardAppearance = UIKeyboardAppearanceLight;
    UIToolbar* enterPinNumberToolbar = [[UIToolbar alloc]initWithFrame:CGRectMake(0, 0, 320, 50)];
    enterPinNumberToolbar.barStyle = UIBarStyleBlackTranslucent;
    enterPinNumberToolbar.items = @[[[UIBarButtonItem alloc]initWithTitle:@"Cancel" style:UIBarButtonItemStyleDone target:self action:@selector(enterPinCancelNumberPad)],
                                    [[UIBarButtonItem alloc]initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil],
                                    [[UIBarButtonItem alloc]initWithTitle:@"Done" style:UIBarButtonItemStyleDone target:self action:@selector(enterPinDoneWithNumberPad)]];
    [enterPinNumberToolbar sizeToFit];
    _enterPinText.inputAccessoryView = enterPinNumberToolbar;
    
    self.title=@"Sign Data";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    
    self.signDataButton.layer.cornerRadius=22.0;
    [self.signDataButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.signDataButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.signDataButton.layer setShadowOpacity:0.5];
 //   self.signDataButton.layer.borderWidth=1.0;
    
    self.verifySignatureButton.layer.cornerRadius=22.0;
    [self.verifySignatureButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.verifySignatureButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.verifySignatureButton.layer setShadowOpacity:0.5];
  //  self.verifySignatureButton.layer.borderWidth=1.0;
    
    SwitchValue =1;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    
    // Do any additional setup after loading the view.
}

#pragma mark viewWillAppear

-(void)viewWillAppear:(BOOL)animated {
    [super viewWillAppear:YES];
    
    self.enterPinText.text=@"";
    self.digitalSignTextView.text=@"";
    self.enterDataTextView.text=@"";
    [self.enterPinText resignFirstResponder];
    [self.enterDataTextView resignFirstResponder];

    self.digitalSignTextView.text=@"";
    
    self.enterPinText.enabled=YES;
    self.enterDataTextView.editable=YES;
    self.enterDataTextView.selectable=YES;
}
#pragma mark EIDA Toolkit calling functions

- (IBAction)signDataButtonAction:(id)sender {
    
    self.digitalSignTextView.text=@"";
    [self.enterPinText resignFirstResponder];
    [self.enterDataTextView resignFirstResponder];
    
    if ([self.enterPinText.text length]==0) {
        [AlertView showAlertTitle:ALERT withMessage:ENTER_PIN onView:self];
    }// if
    else {
        if ([self.enterPinText.text length]<4) {
            [AlertView showAlertTitle:ALERT withMessage:MIN_PIN onView:self];
            self.enterPinText.text=@"";
        }// if
        else if ([self.enterPinText.text length]>16) {
            [AlertView showAlertTitle:ALERT withMessage:MAX_PIN onView:self];
            self.enterPinText.text=@"";
        }// else if
        else {
        [self.utils ShowProgressBar:SIGNDATA andView:self.view];
        int data_hash = 0;  /// 0 for plain data and 1 for Hash data
        NSString *requestId=[Utils generateSecureKey];
        NSString *textviewData =self.enterDataTextView.text;
        dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
        @try {
            CardReader *cardreader  = [self.model.cardReaderSharedArray objectAtIndex:0];
            DataProtectionKey *data_protection_key=self.model.dataprotectedShared;
            
            NSString *request_handle =  [cardreader prepareRequest:requestId];
            if (request_handle.length==0 || [request_handle isEqualToString:@""] || [request_handle isEqualToString:@"(null)"] ) {
                [AlertView showAlertTitle:ALERT withMessage:REQUEST_HANDLE_EMPTY onView:self];
            }//if
            else {
                NSString *encodePin=[Utils setEncrytion:request_handle data:self.enterPinText.text publickey:[data_protection_key getPublicKey] keylength:[data_protection_key getKeyLength]];
                if (encodePin.length==0 || [encodePin isEqualToString:@""] || [encodePin isEqualToString:@"(null)"] ) {
                    [AlertView showAlertTitle:ALERT withMessage:ENCODE_PIN_EMPTY onView:self];
                }//if
                else {
                    SignatureResponse *signresponse;
                    if (self->SwitchValue ==1) {
                        signresponse = [cardreader signChallenge:textviewData inputLength:(int)[textviewData length] isInputHash:data_hash encodedPin:encodePin];
                    }//if
                    else if (self->SwitchValue==2) {
                        signresponse = [cardreader signData:self.enterDataTextView.text inputLength:(int)[self.enterDataTextView.text length] isInputHash:data_hash encodedPin:encodePin];
                    }//else if
                dispatch_async(dispatch_get_main_queue(), ^{
                    NSString *xmlString =[signresponse getXmlString];
                    NSString *errorMsg = [self.utils validateToolkitResponse:requestId xmlstring:xmlString];
                    if (![errorMsg isEqualToString:@""] || errorMsg.length>0) {
                        [AlertView showAlertTitle:ALERT withMessage:errorMsg onView:self];
                    }//if
                    else {
                        self->resultSignature=[signresponse getSignature];
                        self->resultSignatureLength =[signresponse getSignatureLength];
                        NSData *csignData = [[NSData alloc] initWithBytes:self->resultSignature length:self->resultSignatureLength];
                        NSString *signDataBase64string=[Utils base64forData:csignData];
    
                        self.digitalSignTextView.text=signDataBase64string;
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
                });// update UI main queue
            }// finally
         });// background queue
     }//else
  }//else
}
- (IBAction)verifySignatureButtonAction:(id)sender {
    
   [self.utils ShowProgressBar:VERIFYDATA andView:self.view];
   [self.enterPinText resignFirstResponder];
   [self.enterDataTextView resignFirstResponder];
    
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
            NSString *encodePin=[Utils setEncrytion:request_handle data:self.enterPinText.text publickey:[data_protection_key getPublicKey] keylength:[data_protection_key getKeyLength]];
            if (encodePin.length==0 || [encodePin isEqualToString:@""] || [encodePin isEqualToString:@"(null)"] ) {
                [AlertView showAlertTitle:ALERT withMessage:ENCODE_PIN_EMPTY onView:self];
            }//if
            else {
                CardCertificates *certificates = [cardreader getPkiCertificates:encodePin];
                NSString *xmlString =[certificates getXmlString];
                NSString *errorMsg =  [self.utils validateToolkitResponse:requestId xmlstring:xmlString];
                if (![errorMsg isEqualToString:@""] || errorMsg.length>0) {
                    [AlertView showAlertTitle:ALERT withMessage:errorMsg onView:self];
                }//if
                else {
                    int data_hash = 0;  /// 0 for plain data and 1 for Hash data
                    uint8_t *certificate;
                    int certificateLength;
                        
                    if (self->SwitchValue ==1) {
                        certificate = [certificates getAuthenticationCertificate];
                        certificateLength= [certificates getAuthenticationCertificateLength];
                    }//if
                    else if (self->SwitchValue==2) {
                        certificate = [certificates getSigningCertificate];
                        certificateLength= [certificates getSigningCertificateLength];
                    }//else if
                    [cardreader verifySignature:self.enterDataTextView.text inputLength:(int)[self.enterDataTextView.text length] isInputHash:data_hash signature:self->resultSignature signatureLength:self->resultSignatureLength certificate:certificate certificateLength:certificateLength];
                    [AlertView showAlertTitle:ALERT withMessage:SIGNATURE_VERIFIED onView:self ];
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
        });// update UI main queue
      }// finally
   });// background queue
}
- (IBAction)swithButton:(id)sender {
    if ([self.swithButton isOn]) {
        self.authKey.text = @"Sign Key";
        [self.swithButton setOn:NO animated:YES];
        SwitchValue=2;
        [self.signDataButton setTitle:@"Sign With Sign Key" forState:UIControlStateNormal];
        [self.verifySignatureButton setTitle:@"Verify With Sign Key" forState:UIControlStateNormal];
        self.enterPinText.text=@"";
        self.enterDataTextView.text=@"";
        self.digitalSignTextView.text=@"";
    }//if
    else {
        self.authKey.text = @"Auth Key";
        [self.swithButton setOn:YES animated:YES];
        SwitchValue=1;
        [self.signDataButton setTitle:@"Sign With Auth Key" forState:UIControlStateNormal];
        [self.verifySignatureButton setTitle:@"Verify With Auth Key" forState:UIControlStateNormal];
        self.enterPinText.text=@"";
        self.enterDataTextView.text=@"";
        self.digitalSignTextView.text=@"";
    }//else
}
#pragma mark Updating UI Methods
- (void)goBack {
    [self.navigationController popViewControllerAnimated:YES];
}
-(void)enterPinCancelNumberPad {
    [_enterPinText resignFirstResponder];
    _enterPinText.text = @"";
}
-(void)enterPinDoneWithNumberPad {
    [_enterPinText resignFirstResponder];
}
- (BOOL)textView:(UITextView *)textView shouldChangeTextInRange:(NSRange)range replacementText:(NSString *)text {
    
    if ([text isEqualToString:@"\n"]) {
        
        [textView resignFirstResponder];
        // Return FALSE so that the final '\n' character doesn't get added
        return NO;
    }
    // For any other character return TRUE so that the text gets added to the view
    return YES;
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
