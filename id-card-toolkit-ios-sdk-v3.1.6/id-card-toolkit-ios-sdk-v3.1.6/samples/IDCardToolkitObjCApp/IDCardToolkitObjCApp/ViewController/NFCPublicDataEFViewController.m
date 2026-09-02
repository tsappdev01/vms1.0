//
//  NFCPublicDataEFViewController.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 20/11/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "NFCPublicDataEFViewController.h"

API_AVAILABLE(ios(13.0))
@interface NFCPublicDataEFViewController ()<NFCTagReaderSessionDelegate> {
    NSMutableArray *listArr;
    int selectedEFType;
    NSString *cardNumberString;
    NSString *dobstring;
    NSString *expireDateString;
}
@property(nonatomic,strong)Utils * utils;
@property(nonatomic)NFCTagReaderSession *session;

@end

@implementation NFCPublicDataEFViewController

- (void)viewDidLoad {
    [super viewDidLoad];
   
       self.title=@"NFC Public Data EF";
       self.navigationItem.hidesBackButton = YES;
       self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                                style:UIBarButtonItemStylePlain
                                                                               target:self
                                                                               action:@selector(goBack)];
        self.nfcReadPublicDataEFButton.layer.cornerRadius=22.0;
        [self.nfcReadPublicDataEFButton.layer setShadowOffset:CGSizeMake(5, 5)];
        [self.nfcReadPublicDataEFButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
        [self.nfcReadPublicDataEFButton.layer setShadowOpacity:0.5];
       //  self.nfcReadPublicDataEFButton.layer.borderWidth=1.0;
       
         [self setNumberPad:self.cardNumberText];
         [self setNumberPad:self.dobYYText];
         [self setNumberPad:self.dobMMText];
         [self setNumberPad:self.dobDDText];
         [self setNumberPad:self.expiryDateYYText];
         [self setNumberPad:self.expiryDateMMText];
         [self setNumberPad:self.expiryDateDDText];
       
           AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
           self.model = aDelegate.model;
   
        listArr=[[NSMutableArray alloc]initWithObjects:@"IDN_CN",@"ROOT_CERTIFICATE",@"NON_MODIFIABLE_DATA",@"MODIFIABLE_DATA",@"PHOTOGRAPHY",@"SIGNATURE_IMAGE",@"HOME_ADDRESS",@"WORK_ADDRESS", nil];
    // Do any additional setup after loading the view.
}

- (IBAction)nfcReadPublicDataButtonAction:(id)sender {
    
    cardNumberString = self.cardNumberText.text;
    dobstring = [NSString stringWithFormat:@"%@%@%@",self.dobYYText.text,self.dobMMText.text,self.dobDDText.text];
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
    
    PUBLIC_DATA_EF_TYPE publicDataEFType = self->selectedEFType+1;

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

           // [cardreader setNfcAuthenticationParameters:@"000020030" dateOfBirth:@"800112" expiryDate:@"290107"];
            
                    [cardreader setNfcAuthenticationParameters:self->cardNumberString dateOfBirth:self->dobstring expiryDate:self->expireDateString];
                
                 
            dispatch_async(dispatch_get_main_queue(), ^{
                [session setAlertMessage:@"Reading Public Details from card"];
            });// main queue
            
            [cardreader readPublicDataEF:publicDataEFType validateSignature:true];
                NSLog(@"readPublicDataEF completed1");
                NSLog(@"readPublicDataEF cardreader getName %@",[cardreader getName]);
                NSLog(@"readPublicDataEF completed");
            //NSString *getParsedData =   [cardreader parseEFData:[cardreader getReadPublicDataEFByte] efDatalength:[cardreader getReadPublicDataEFLength]];

                NSLog(@"getParsedData completed");
            dispatch_async(dispatch_get_main_queue(), ^{
                if ([cardreader getReadPublicDataEFLength]>0) {
                    NSLog(@"getReadPublicDataEFLength OK");
                    NSMutableString * getEFDataString = [[NSMutableString alloc] init];
                    for (int i=0; i<[cardreader getReadPublicDataEFLength]; i++) {
                        [getEFDataString appendString:[NSString stringWithFormat:@"%02x",[cardreader getReadPublicDataEFByte][i]]];
                    }
                       //[AlertView showAlertTitle:@"" withMessage:[NSString stringWithFormat:@"Parsed EF data \n\n %@",getParsedData] onView:self];
                } else {
                    NSLog(@"getReadPublicDataEFLength NOT_OK");
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
#pragma mark PickerView Delagates methods

- (NSInteger)numberOfComponentsInPickerView:(UIPickerView *)pickerView {
    return 1;
}
- (NSInteger)pickerView:(UIPickerView *)pickerView numberOfRowsInComponent:(NSInteger)component {
    return listArr.count;
}
- (NSString *)pickerView:(UIPickerView *)pickerView titleForRow:(NSInteger)row forComponent:(NSInteger)component {
    selectedEFType = (int)row;
    return listArr[row];
}
- (void)pickerView:(UIPickerView *)pickerView didSelectRow:(NSInteger)row inComponent:(NSInteger)component {
    selectedEFType = (int)row;
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
    [self.dobYYText resignFirstResponder];
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
