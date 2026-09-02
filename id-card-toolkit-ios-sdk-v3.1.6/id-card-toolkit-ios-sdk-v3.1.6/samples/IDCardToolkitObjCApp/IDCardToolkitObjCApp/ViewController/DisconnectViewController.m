//
//  DisconnectViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship on 15/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "DisconnectViewController.h"

@interface DisconnectViewController ()
@property(nonatomic,strong)Utils * utils;
@end

@implementation DisconnectViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    
    self.title=@"Disconnect Reader";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    
    self.disconnectReaderbutton.layer.cornerRadius=22.0;
    [self.disconnectReaderbutton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.disconnectReaderbutton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.disconnectReaderbutton.layer setShadowOpacity:0.5];
  //  self.disconnectReaderbutton.layer.borderWidth=1.0;
    
    self.disconnectStatus.font=[UIFont boldSystemFontOfSize:16.0];
    self.disconnectStatus.textColor=[UIColor whiteColor];
    self.disconnectStatus.backgroundColor=[UIColor blackColor];
    
    self.disconnectStatus.layer.cornerRadius=20.0f;
    self.disconnectStatus.layer.borderWidth=0.1f;
    self.disconnectStatus.layer.masksToBounds=YES;
    self.disconnectStatus.layer.borderColor=[UIColor clearColor].CGColor;
    
    self.disconnectStatus.hidden=YES;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    
    // Do any additional setup after loading the view.
}
- (IBAction)disconnectReaderbuttonAction:(id)sender {
    
    [self.utils ShowProgressBar:DEVICE_DISCONNECTING andView:self.view];
    dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
    @try {
        CardReader *cardreader  = [self.model.cardReaderSharedArray objectAtIndex:0];
        [cardreader disconnect];
         dispatch_async(dispatch_get_main_queue(), ^{
            self.disconnectStatus.hidden=NO;
            self.disconnectStatus.text=DISCONNECT_DEVICE;
          });// update UI main queue
        }// try
        @catch (NSException *exception) {
            NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
            [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
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
