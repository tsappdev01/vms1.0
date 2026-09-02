//
//  PinResetViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship  on 1/19/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "PinResetViewController.h"

@interface PinResetViewController ()
@property(nonatomic,strong)Utils * utils;
@end

@implementation PinResetViewController
#pragma mark viewDidLoad

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.enterPin.keyboardType=UIKeyboardTypeNumberPad;
    self.enterPin.keyboardAppearance = UIKeyboardAppearanceLight;
    UIToolbar* enterPinNumberToolbar = [[UIToolbar alloc]initWithFrame:CGRectMake(0, 0, 320, 50)];
    enterPinNumberToolbar.barStyle = UIBarStyleBlackTranslucent;
    enterPinNumberToolbar.items = @[[[UIBarButtonItem alloc]initWithTitle:@"Cancel" style:UIBarButtonItemStyleDone target:self action:@selector(enterPinCancelNumberPad)],
                            [[UIBarButtonItem alloc]initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil],
                            [[UIBarButtonItem alloc]initWithTitle:@"Done" style:UIBarButtonItemStyleDone target:self action:@selector(enterPinDoneWithNumberPad)]];
    [enterPinNumberToolbar sizeToFit];
    self.enterPin.inputAccessoryView = enterPinNumberToolbar;
    
    self.confirmPin.keyboardType=UIKeyboardTypeNumberPad;
    self.confirmPin.keyboardAppearance = UIKeyboardAppearanceLight;
    UIToolbar* confirmPinNumbertoolbar = [[UIToolbar alloc]initWithFrame:CGRectMake(0, 0, 320, 50)];
    confirmPinNumbertoolbar.barStyle = UIBarStyleBlackTranslucent;
    confirmPinNumbertoolbar.items = @[[[UIBarButtonItem alloc]initWithTitle:@"Cancel" style:UIBarButtonItemStyleDone target:self action:@selector(confirmPincancelNumberPad)],
                             [[UIBarButtonItem alloc]initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil],
                             [[UIBarButtonItem alloc]initWithTitle:@"Done" style:UIBarButtonItemStyleDone target:self action:@selector(confirmPinDoneWithNumberPad)]];
    [confirmPinNumbertoolbar sizeToFit];
    self.confirmPin.inputAccessoryView = confirmPinNumbertoolbar;
    
    self.title=@"Reset Pin";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    
    self.resetPinButton.layer.cornerRadius=22.0;
    [self.resetPinButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.resetPinButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.resetPinButton.layer setShadowOpacity:0.5];
   // self.resetPinButton.layer.borderWidth=1.0;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    
    // Do any additional setup after loading the view.
}

#pragma mark viewWillAppear

-(void)viewWillAppear:(BOOL)animated {
    [super viewWillAppear:YES];
    
    self.enterPin.text=@"";
    self.confirmPin.text=@"";
}
#pragma mark EIDA Toolkit calling functions

- (IBAction)resetPinButtonAction:(id)sender {

  [self.enterPin resignFirstResponder];
  [self.confirmPin resignFirstResponder];
  if ([self.enterPin.text length]==0) {
     [AlertView showAlertTitle:ALERT withMessage:ENTER_PIN onView:self];
  }// if
  else if ([self.confirmPin.text length]==0) {
     [AlertView showAlertTitle:ALERT withMessage:CONFIRM_PIN onView:self];
    }// else if
  else if (![self.enterPin.text isEqualToString:self.confirmPin.text]) {
     [AlertView showAlertTitle:ALERT withMessage:MISMATCH_PIN onView:self];
        self.confirmPin.text=@"";
        self.enterPin.text=@"";
  }// else if
  else {
     if ([self.enterPin.text length]<4 && [self.confirmPin.text length]<4) {
          [AlertView showAlertTitle:ALERT withMessage:MIN_PIN onView:self];
            self.confirmPin.text=@"";
            self.enterPin.text=@"";
     }// if
     else if ([self.enterPin.text length]>16 && [self.confirmPin.text length]>16) {
          [AlertView showAlertTitle:ALERT withMessage:MAX_PIN onView:self];
            self.confirmPin.text=@"";
            self.enterPin.text=@"";
     }// else if
    else {
    [self.utils ShowProgressBar:RESETPIN andView:self.view];
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
            NSString *encodePin=[Utils setEncrytion:request_handle data:self.enterPin.text publickey:[data_protection_key getPublicKey] keylength:[data_protection_key getKeyLength]];
            if (encodePin.length==0 || [encodePin isEqualToString:@""] || [encodePin isEqualToString:@"(null)"] ) {
                [AlertView showAlertTitle:ALERT withMessage:ENCODE_PIN_EMPTY onView:self];
            }//if
            else {
               ToolkitResponse *response = [cardreader resetPin:encodePin fingerData:selectedFingerData sensorTimeout:SENSOR_TIME_OUT];
                dispatch_async(dispatch_get_main_queue(), ^{
                    NSString *xmlString =[response getXmlString];
                if (![xmlString isEqualToString:@""] && xmlString.length>0) {
                    NSString *errorMsg =  [self.utils validateToolkitResponse:requestId xmlstring:xmlString];
                    if (![errorMsg isEqualToString:@""] && errorMsg.length>0) {
                        [AlertView showAlertTitle:ALERT withMessage:errorMsg onView:self];
                    }//if
                    else {
                        NSString *xmlString =[response getXmlString];
                        [AlertView showAlertTitle:ALERT withMessage:[NSString stringWithFormat:@"%@\n\n%@",PINREST_SUCCESS,xmlString] onView:self];
                    }//else
                }//if
                else {
                   ResponseStatus *status = [response getResponseStatus];
                    if (status==0) {
                        [AlertView showAlertTitle:ALERT withMessage:PINREST_SUCCESS onView:self];
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
                self.confirmPin.text=@"";
                self.enterPin.text=@"";
                [self.utils DismissProgressBar];
             });// update UI main queue
            }// finally
         });// background queue
      }//else
   } //else
}
#pragma mark Updating UI Methods

-(void)enterPinCancelNumberPad {
    [self.enterPin resignFirstResponder];
    self.enterPin.text = @"";
}
-(void)enterPinDoneWithNumberPad {
    [self.enterPin resignFirstResponder];
}
-(void)confirmPincancelNumberPad {
    [self.confirmPin resignFirstResponder];
    self.confirmPin.text = @"";
}
-(void)confirmPinDoneWithNumberPad {
    [self.confirmPin resignFirstResponder];
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
