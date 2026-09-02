//
//  SetNFCdetailsViewController.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 17/10/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "SetNFCdetailsViewController.h"

API_AVAILABLE(ios(13.0))
@interface SetNFCdetailsViewController ()<NFCTagReaderSessionDelegate> {
    CardPublicData *cardPublicData;
    NSString *cardNumberString;
    NSString *dobstring;
    NSString *expireDateString ;
}
@property(nonatomic)NFCTagReaderSession *session;
@property(nonatomic, assign)BOOL isCardV3;
@end

@implementation SetNFCdetailsViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    [self setIsCardV3:NO];
    self.title=@"NFC Card Details View";
      self.navigationItem.hidesBackButton = YES;
      self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                               style:UIBarButtonItemStylePlain
                                                                              target:self
                                                                              action:@selector(goBack)];
      
    self.nfcReadpublicdataButton.layer.cornerRadius=22.0;
    [self.nfcReadpublicdataButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.nfcReadpublicdataButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.nfcReadpublicdataButton.layer setShadowOpacity:0.5];
    //self.nfcReadpublicdataButton.layer.borderWidth=1.0;
    
    [self setNumberPad:self.cardNumberText];
    [self setNumberPad:self.dobYYtext];
    [self setNumberPad:self.dobMMText];
    [self setNumberPad:self.dobDDText];
    [self setNumberPad:self.expiryDateYYText];
    [self setNumberPad:self.expiryDateMMText];
    [self setNumberPad:self.expiryDateDDText];
  
      AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
      self.model = aDelegate.model;
    
    // Do any additional setup after loading the view.
    [self showCardVersionsAlert];
}

-(void)showCardVersionsAlert {
    UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"Card Version" message:@"Please select your card version." preferredStyle:UIAlertControllerStyleAlert];
   
    //Button 1
    UIAlertAction *vThreeCard = [UIAlertAction actionWithTitle:@"V3/V4 Card" style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
    //code for performing acctions to be excuted when click Button1
        if (@available(iOS 13.0, *)) {
            [self setIsCardV3:YES];
              self.session = [[NFCTagReaderSession alloc]
                              initWithPollingOption:NFCPollingISO14443 delegate:self queue:nil];
               self.session.alertMessage =  @"Hold your iPhone near an NFC.";
                  [ self.session beginSession];
            
          } else {
               [AlertView showAlertTitle:ALERT withMessage:NFC_NOT_SUPPORT onView:self];
          }
    }];
    
    [alert addAction:vThreeCard];
    
    UIAlertAction *vTwoCard = [UIAlertAction actionWithTitle:@"V2 Card" style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
    //code for performing acctions to be excuted when click Button1
        [self setIsCardV3:NO];
    }];
    [alert addAction:vTwoCard];
    [self presentViewController:alert animated:YES completion:^{
    //code for performing acctions to be excuted when UIAlertController is started
   
    }];
}

- (IBAction)nfcReadPublicDataButtonAction:(id)sender {
    [self setIsCardV3:NO];
    cardNumberString = self.cardNumberText.text;
    dobstring = [NSString stringWithFormat:@"%@%@%@",self.dobYYtext.text,self.dobMMText.text,self.dobDDText.text];
    expireDateString = [NSString stringWithFormat:@"%@%@%@",self.expiryDateYYText.text,self.expiryDateMMText.text,self.expiryDateDDText.text];
    
      if (@available(iOS 13.0, *)) {
          
            self.session = [[NFCTagReaderSession alloc]
                            initWithPollingOption:NFCPollingISO14443 delegate:self queue:nil];
             self.session.alertMessage =  @"Hold your iPhone near an NFC.";
                [ self.session beginSession];
          
        } else {
             [AlertView showAlertTitle:ALERT withMessage:NFC_NOT_SUPPORT onView:self];
        }
}
#pragma mark NFC Tag Readger Methods

- (void)tagReaderSessionDidBecomeActive:(NFCTagReaderSession *)session  API_AVAILABLE(ios(13.0)){
    NSLog(@"tagReaderSessionDidBecomeActive");
    Toolkit *toolkit = self.model.toolkitShared;
               
    if (toolkit==nil) {
        [session invalidateSession];
    }
}

- (void)tagReaderSession:(NFCTagReaderSession *)session didInvalidateWithError:(NSError *)error  API_AVAILABLE(ios(13.0)){
    NSLog(@"readerSession:didInvalidateWithError: (%@)", [error localizedDescription]);
    [AlertView showAlertTitle:ALERT withMessage:[error localizedDescription] onView:self];

}

- (void)tagReaderSession:(NFCTagReaderSession *)session didDetectTags:(NSArray<__kindof id<NFCTag>> *)tags   API_AVAILABLE(ios(13.0)){
     NSLog(@"readerSession:didDetectTags");
    if ([tags count] > 1) {
        [session setAlertMessage:@"More than 1 tag is detected, please try again"];
        [session restartPolling];
        return;
    }
    
     id<NFCTag> firstTag =  tags[0];
     NSLog(@"firstTag %@",firstTag);
    
    if (firstTag.type == NFCTagTypeMiFare) {
        [session setAlertMessage:@"A tag that is not iso7816 is detected, please try again with tag iso7816"];
        NSLog(@"session restartPolling");
         [session restartPolling];
    }
    
    NSString *requestId=[Utils generateSecureKey];

    dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_HIGH, 0), ^(void){
    
    if (@available(iOS 13.0, *)) {
        if (firstTag.type == NFCTagTypeISO7816Compatible) {
            id<NFCISO7816Tag> iso7816Tag = [firstTag asNFCISO7816Tag];
            @try {
        
             Toolkit *toolkit = self.model.toolkitShared;
            
            if (toolkit==nil) {
                [session invalidateSession];
            }
        
            [toolkit setNfcTag:session tag:iso7816Tag];
            
            CardReader *cardreader = [toolkit getReaderWithEmiratesId];

            if (cardreader==nil) {
                [session invalidateSession];
            }
            if (!cardreader.isConnected) {
                 [cardreader connect];
            }
                
           // yymmdd format

//            [cardreader setNfcAuthenticationParameters:@"000020029" dateOfBirth:@"700112" expiryDate:@"290107"];
        if(![self isCardV3]) {
            [cardreader setNfcAuthenticationParameters:self->cardNumberString dateOfBirth:self->dobstring expiryDate:self->expireDateString];//parameters are not required for V3 card
        }
                 
            dispatch_async(dispatch_get_main_queue(), ^{
                [session setAlertMessage:@"Reading Public Details from card"];
            });// main queue
                  
            self->cardPublicData = [cardreader readPublicData:requestId readnonModifiableData:TRUE readModifiableData:TRUE readPhotography:TRUE readSignatueImage:TRUE readAddress:TRUE];
//
                dispatch_async(dispatch_get_main_queue(), ^{
        
                    if ([self->cardPublicData getIdNumber]  != nil) {
                        NSLog(@"idnumber %@",[self->cardPublicData getIdNumber]);
                        NSLog(@"cardNumber %@",[self->cardPublicData getCardNumber]);
                        [self performSegueWithIdentifier:@"NFCpublicDataView" sender:self];
                    }
                        
                     [session setAlertMessage:@"Reading Completed, session going to close"];
                    [session invalidateSession];
                   });// update UI main queue
              }
            @catch (NSException *exception) {
                 [session invalidateSession];
                  NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
                  [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
              }// catch
              @finally {
              }// finally
        }
     } else {
         dispatch_async(dispatch_get_main_queue(), ^{
            // Fallback on earlier versions
            [AlertView showAlertTitle:ALERT withMessage:NFC_NOT_SUPPORT onView:self];
         });// update UI main queue
      }
   });// background queue
}
- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender {
    
    if ([[segue identifier] isEqualToString:@"NFCpublicDataView"])
       {
           self.cardNumberText.text=@"";
           self.dobYYtext.text=@"";
           self.dobMMText.text=@"";
           self.dobDDText.text=@"";
           self.expiryDateYYText.text=@"";
           self.expiryDateMMText.text=@"";
           self.expiryDateDDText.text=@"";
           
           // Get reference to the destination view controller
           NFCPublicDataViewController *detailView = [segue destinationViewController];

           [detailView getPublicaDataClass:cardPublicData];
       }
}
-(void)setNumberPad:(UITextField *)customtextfiled {
 
   customtextfiled.keyboardType=UIKeyboardTypeNumberPad;
    customtextfiled.keyboardAppearance = UIKeyboardAppearanceLight;
    
    UIToolbar* NumberToolbar = [[UIToolbar alloc]initWithFrame:CGRectMake(0, 0, 320, 50)];
       NumberToolbar.barStyle = UIBarStyleBlackTranslucent;
       NumberToolbar.items = @[[[UIBarButtonItem alloc]initWithTitle:@"Cancel" style:UIBarButtonItemStyleDone target:self action:@selector(doneWithNumberPad)],
                               [[UIBarButtonItem alloc]initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil],
                               [[UIBarButtonItem alloc]initWithTitle:@"Done" style:UIBarButtonItemStyleDone target:self action:@selector(doneWithNumberPad)]];
       [NumberToolbar sizeToFit];
       customtextfiled.inputAccessoryView = NumberToolbar;
}
-(void)doneWithNumberPad {
    
    [self.cardNumberText resignFirstResponder];
    [self.dobYYtext resignFirstResponder];
    [self.dobMMText resignFirstResponder];
    [self.dobDDText resignFirstResponder];
    [self.expiryDateYYText resignFirstResponder];
    [self.expiryDateMMText resignFirstResponder];
    [self.expiryDateDDText resignFirstResponder];
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
