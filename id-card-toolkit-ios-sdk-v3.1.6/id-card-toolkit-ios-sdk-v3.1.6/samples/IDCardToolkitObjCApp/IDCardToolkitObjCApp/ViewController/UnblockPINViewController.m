//
//  UnblockPINViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship  on 5/29/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "UnblockPINViewController.h"

@interface UnblockPINViewController ()
@property(nonatomic,strong)Utils * utils;
@end

@implementation UnblockPINViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.pintext.keyboardType=UIKeyboardTypeNumberPad;
    self.pintext.keyboardAppearance = UIKeyboardAppearanceLight;
    UIToolbar* enterPinNumberToolbar = [[UIToolbar alloc]initWithFrame:CGRectMake(0, 0, 320, 50)];
    enterPinNumberToolbar.barStyle = UIBarStyleBlackTranslucent;
    enterPinNumberToolbar.items = @[[[UIBarButtonItem alloc]initWithTitle:@"Cancel" style:UIBarButtonItemStyleDone target:self action:@selector(enterPinCancelNumberPad)],
                                    [[UIBarButtonItem alloc]initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil],
                                    [[UIBarButtonItem alloc]initWithTitle:@"Done" style:UIBarButtonItemStyleDone target:self action:@selector(enterPinDoneWithNumberPad)]];
    [enterPinNumberToolbar sizeToFit];
    self.pintext.inputAccessoryView = enterPinNumberToolbar;
    
    self.title=@"Unblock Pin";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    
    self.unblockPinButton.layer.cornerRadius=22.0;
    [self.unblockPinButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.unblockPinButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.unblockPinButton.layer setShadowOpacity:0.5];
  //  self.unblockPinButton.layer.borderWidth=1.0;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    
    // Do any additional setup after loading the view.
}
#pragma mark viewWillAppear

-(void)viewWillAppear:(BOOL)animated {
    [super viewWillAppear:YES];
    
    self.pintext.text=@"";
    [self.pintext resignFirstResponder];
}
#pragma mark EIDA Toolkit calling functions

- (IBAction)unblockPinButtonAction:(id)sender {
    
 if ([self.pintext.text length]==0) {
    [AlertView showAlertTitle:ALERT withMessage:ENTER_PIN onView:self];
 }// if
 else {
    [self.utils ShowProgressBar:UNBLOCKPIN andView:self.view];
    [self.pintext resignFirstResponder];
     NSString *requestId=[Utils generateSecureKey];
     dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
    @try {
        CardReader *cardreader  = [self.model.cardReaderSharedArray objectAtIndex:0];
        FingerData *selectedFingerData =[self.model.selectedFingerSharedArray objectAtIndex:0];
        DataProtectionKey *data_protection_key=self.model.dataprotectedShared;
        
        NSString *request_handle =  [cardreader prepareRequest:requestId];
        if (request_handle.length==0 || [request_handle isEqualToString:@""] || [request_handle isEqualToString:@"(null)"] ) {
            [AlertView showAlertTitle:ALERT withMessage:REQUEST_HANDLE_EMPTY onView:self];
        }//if
        else {
            NSString *encodePin=[Utils setEncrytion:request_handle data:self.pintext.text publickey:[data_protection_key getPublicKey] keylength:[data_protection_key getKeyLength]];
            if (encodePin.length==0 || [encodePin isEqualToString:@""] || [encodePin isEqualToString:@"(null)"] ) {
                [AlertView showAlertTitle:ALERT withMessage:ENCODE_PIN_EMPTY onView:self];
            }//if
            else {
                 ToolkitResponse *response = [cardreader unblockPin:encodePin fingerData:selectedFingerData sensorTimeout:SENSOR_TIME_OUT];
                dispatch_async(dispatch_get_main_queue(), ^{
                    NSString *xmlString =[response getXmlString];
                    if (![xmlString isEqualToString:@""] && xmlString.length>0) {
                        NSString *errorMsg =  [self.utils validateToolkitResponse:requestId xmlstring:xmlString];
                        if (![errorMsg isEqualToString:@""] && errorMsg.length>0) {
                            [AlertView showAlertTitle:ALERT withMessage:errorMsg onView:self];
                        }//if
                        else {
                            NSString *xmlString =[response getXmlString];
                            [AlertView showAlertTitle:ALERT withMessage:[NSString stringWithFormat:@"%@\n\n%@",CARD_UNBLOCK_SUCCESS,xmlString] onView:self];
                        }//else
                    }//if
                    else {
                        ResponseStatus *status = [response getResponseStatus];
                        if (status==0) {
                            [AlertView showAlertTitle:ALERT withMessage:CARD_UNBLOCK_SUCCESS onView:self];
                        }//if
                    }//else
                });// update UI main queue
            }//else
          }//else
       }//try
      @catch (NSException *exception) {
            NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
            [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
      }// catch
        @finally {
             dispatch_async(dispatch_get_main_queue(), ^{
                 self.pintext.text=@"";
                 [self.utils DismissProgressBar];
             });// update UI main queue
          }// finally
       });// background queue
    }//else
}
#pragma mark Updating UI Methods

-(void)enterPinCancelNumberPad {
    [self.pintext resignFirstResponder];
    self.pintext.text = @"";
}
-(void)enterPinDoneWithNumberPad {
    [self.pintext resignFirstResponder];
}
- (void)goBack {
    [self.navigationController popViewControllerAnimated:YES];
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
