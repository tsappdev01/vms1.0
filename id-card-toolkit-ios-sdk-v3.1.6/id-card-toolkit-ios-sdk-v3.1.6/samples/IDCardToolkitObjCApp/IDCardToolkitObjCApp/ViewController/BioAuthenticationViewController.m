//
//  BioAuthenticationViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship  on 1/18/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "BioAuthenticationViewController.h"

@interface BioAuthenticationViewController ()
@property(nonatomic,strong)Utils * utils;
@end

@implementation BioAuthenticationViewController
#pragma mark viewDidLoad

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.title=@"Biometric Authentication";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    
    self.verifyBiometricButton.layer.cornerRadius=22.0;
    [self.verifyBiometricButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.verifyBiometricButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.verifyBiometricButton.layer setShadowOpacity:0.5];
   // self.verifyBiometricButton.layer.borderWidth=1.0;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;

    // Do any additional setup after loading the view.
}

#pragma mark EIDA Toolkit calling functions

- (IBAction)verifyBiometricButtonAction:(id)sender {
    
  [self.utils ShowProgressBar:BIOAUTH andView:self.view];
  NSString *requestId=[Utils generateSecureKey];
  dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
  @try {
      CardReader *cardreader  = [self.model.cardReaderSharedArray objectAtIndex:0];
      FingerData *selectedFingerData =[self.model.selectedFingerSharedArray objectAtIndex:0];
      
    ToolkitResponse *response = [cardreader authenticateBiometricOnServer:requestId fingerIndex:selectedFingerData.getFingerIndex sensorTimeout:SENSOR_TIME_OUT];
    dispatch_async(dispatch_get_main_queue(), ^{
        NSString *xmlString =[response getXmlString];
        NSString *errorMsg =  [self.utils validateToolkitResponse:requestId xmlstring:xmlString];
        if (![errorMsg isEqualToString:@""] || errorMsg.length>0) {
            [AlertView showAlertTitle:ALERT withMessage:errorMsg onView:self];
        }//if
        else {
            NSDictionary *responseDic =[response getResponseDataElement];
            NSString *matchStr=[responseDic objectForKey:@"MatchStatus"];
            NSString *responseStatusStr=[responseDic objectForKey:@"ResponseStatus"];
            [AlertView showAlertTitle:ALERT withMessage:[NSString stringWithFormat:@"%@ %@",matchStr,responseStatusStr] onView:self];
         }//else
      });// update UI main queue
   }// try
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
#pragma mark Updating UI Methods
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
