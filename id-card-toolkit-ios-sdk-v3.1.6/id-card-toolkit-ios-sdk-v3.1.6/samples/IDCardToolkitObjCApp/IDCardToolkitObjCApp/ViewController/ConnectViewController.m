//
//  ConnectViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship on 13/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ConnectViewController.h"

@interface ConnectViewController ()
@property(nonatomic,strong)Utils * utils;
@end

@implementation ConnectViewController
- (void)viewDidLoad {
    [super viewDidLoad];
    
    
    self.title=@"Connect Reader";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    self.statusLabel.hidden=YES;
    
    self.connectedDeviceButton.layer.cornerRadius=22.0;
    [self.connectedDeviceButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.connectedDeviceButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.connectedDeviceButton.layer setShadowOpacity:0.5];
//    self.connectedDeviceButton.layer.borderWidth=1.0;
    
    self.statusLabel.font=[UIFont boldSystemFontOfSize:16.0];
    self.statusLabel.textColor=[UIColor whiteColor];
    self.statusLabel.backgroundColor=[UIColor blackColor];
    
    self.statusLabel.layer.cornerRadius=20.0f;
    self.statusLabel.layer.borderWidth=0.1f;
    self.statusLabel.layer.masksToBounds=YES;
    self.statusLabel.layer.borderColor=[UIColor clearColor].CGColor;

    self.readerName.adjustsFontSizeToFitWidth=YES;
    self.readerName.hidden=YES;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    
    // Do any additional setup after loading the view.
}
- (IBAction)connectedDeviceButtonAction:(id)sender {
    
    [self.utils ShowProgressBar:DEVICE_CONNECT andView:self.view];
    dispatch_barrier_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_HIGH, 0), ^(void) {

        @try {
            CardReader *cardreader = [self.model.cardReaderSharedArray objectAtIndex:0];
            [cardreader connect];
          dispatch_async(dispatch_get_main_queue(), ^{
            self.readerName.hidden=NO;
            self.statusLabel.hidden=NO;
            self.statusLabel.text=DEVICE_CONNECTED;
            self.readerName.text = [cardreader getName];
          });// update UI main queue
        }//try
        @catch (NSException *exception) {
            dispatch_async(dispatch_get_main_queue(), ^{
            NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
            [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
            });
        }// catch
        @finally {
            dispatch_async(dispatch_get_main_queue(), ^{
                [self.utils DismissProgressBar];
            });// update UI main queue
        }// finally
    });// background queue
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
