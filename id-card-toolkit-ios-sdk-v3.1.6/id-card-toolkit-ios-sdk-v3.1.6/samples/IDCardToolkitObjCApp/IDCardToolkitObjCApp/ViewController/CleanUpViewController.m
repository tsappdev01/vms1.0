//
//  CleanUpViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship on 15/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "CleanUpViewController.h"

@interface CleanUpViewController () {
     ETSTATUS etstatus;
}

@end

@implementation CleanUpViewController
- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.title=@"Toolkit CleanUp";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    
    self.cleanUpButton.layer.cornerRadius=22.0;
    [self.cleanUpButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.cleanUpButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.cleanUpButton.layer setShadowOpacity:0.5];
    
 //   self.cleanUpButton.layer.borderWidth=1.0;
    
    self.cleanUpStatus.font=[UIFont boldSystemFontOfSize:16.0];
    self.cleanUpStatus.textColor=[UIColor whiteColor];
    self.cleanUpStatus.backgroundColor=[UIColor blackColor];
    
    self.cleanUpStatus.layer.cornerRadius=16.0f;
    self.cleanUpStatus.layer.borderWidth=0.1f;
    self.cleanUpStatus.layer.masksToBounds=YES;
    self.cleanUpStatus.layer.borderColor=[UIColor clearColor].CGColor;
    
    self.cleanUpStatus.hidden=YES;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    
    // Do any additional setup after loading the view.
}
- (IBAction)cleanUpButtonAction:(id)sender {
    dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
    @try {
        Toolkit *toolkit = self.model.toolkitShared;
        [toolkit cleanup];
        
    dispatch_async(dispatch_get_main_queue(), ^{
        
        self.model.toolkitShared=nil;
        [self.model.cardReaderSharedArray removeAllObjects];
        [self.model.selectedFingerSharedArray removeAllObjects];
        self.model.dataprotectedShared=nil;
        
        self.cleanUpStatus.hidden=NO;
        self.cleanUpStatus.text=CLEANUP;
      });// update UI main queue
    }//try
    @catch (NSException *exception) {
        NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
        [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
    }// catch
    @finally {
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
