//
//  CheckCardViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship on 14/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "CheckCardViewController.h"

@interface CheckCardViewController ()
@property(nonatomic,strong)Utils * utils;
@end

@implementation CheckCardViewController
- (void)viewDidLoad {
    [super viewDidLoad];
    
    
    self.title=@"Check Card Status";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    
    self.checkCardStatusButton.layer.cornerRadius=22.0;
    [self.checkCardStatusButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.checkCardStatusButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.checkCardStatusButton.layer setShadowOpacity:0.5];
   // self.checkCardStatusButton.layer.borderWidth=1.0;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    
    // Do any additional setup after loading the view.
}
- (IBAction)checkCardStatusButtonAction:(id)sender {

    [self.utils ShowProgressBar:CHECKCARD_STATUS andView:self.view];
    NSString *requestId=[Utils generateSecureKey];
    dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
    @try {
        CardReader *cardreader  = [self.model.cardReaderSharedArray objectAtIndex:0];

        ToolkitResponse *response = [cardreader checkCardStatus:requestId];
        dispatch_async(dispatch_get_main_queue(), ^{
            NSString *xmlString =[response getXmlString];
            NSString *errorMsg =  [self.utils validateToolkitResponse:requestId xmlstring:xmlString];
            if (![errorMsg isEqualToString:@""] || errorMsg.length>0) {
                [AlertView showAlertTitle:ALERT withMessage:errorMsg onView:self];
            }//if
            else {
                NSDictionary *responseDic =[response getResponseDataElement];
                self.cardstatusText.text=[responseDic objectForKey:@"CardStatus"] ;
                self.cardstatusText.textColor=[UIColor greenColor];
           }//else
        });// update UI main queue
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
        });// update UI main queue
      }// finally
   });// background queue
}
-(NSString*) NSDataToHex:(NSData*)data
{
    const unsigned char *dbytes = [data bytes];
    NSMutableString *hexStr =
    [NSMutableString stringWithCapacity:[data length]*2];
    int i;
    for (i = 0; i < [data length]; i++) {
        [hexStr appendFormat:@"%02x ", dbytes[i]];
    }
    return [NSString stringWithString: hexStr];
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
