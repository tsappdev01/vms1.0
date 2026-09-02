//
//  CardSerialNumViewController.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship  on 08/05/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship . All rights reserved.
//

#import "CardSerialNumViewController.h"

@interface CardSerialNumViewController ()

@end

@implementation CardSerialNumViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.title=@"Card Serial Number";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    self.getCsnButton.layer.cornerRadius=22.0;
    [self.getCsnButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.getCsnButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.getCsnButton.layer setShadowOpacity:0.5];
 //   self.getCsnButton.layer.borderWidth=1.0;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    
    self.cardSerialNumberLabel.hidden = YES;
    // Do any additional setup after loading the view.
}
- (IBAction)getCsnButtonAction:(id)sender {
    
    dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
        @try {

            CardReader *cardreader  = [self.model.cardReaderSharedArray objectAtIndex:0];
            NSString *cardserialNumber = [cardreader getCSN];

            dispatch_async(dispatch_get_main_queue(), ^{
                self.cardSerialNumberLabel.hidden = NO;
                self.cardSerialNumberLabel.text=cardserialNumber;
            });// update UI main queue
        }//try
        @catch (NSException *exception) {
            NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
            [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
        }// catch
        @finally {
            dispatch_async(dispatch_get_main_queue(), ^{
            });// update UI main queue
        }// finally
    });// background queue
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
