//
//  DeviceIDViewController.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 02/01/18.
//  Copyright © 2018 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "DeviceIDViewController.h"

@interface DeviceIDViewController ()

@end

@implementation DeviceIDViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.title=@"Device ID";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    
    self.getDeviceIDButton.layer.cornerRadius=22.0;
    [self.getDeviceIDButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.getDeviceIDButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.getDeviceIDButton.layer setShadowOpacity:0.5];
    
   // self.getDeviceIDButton.layer.borderWidth=1.0;
    self.deviceTextview.hidden=YES;
    self.deviceTextview.editable = YES;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    
    // Do any additional setup after loading the view.
}
- (IBAction)getDeviceIDButtonAction:(id)sender {
    
    @try {
        Toolkit *toolkit = self.model.toolkitShared;
        
        NSString *deviceid = [toolkit getDeviceId];
        NSLog(@"deviceid %@",deviceid);
        self.deviceTextview.hidden = NO;
        self.deviceTextview.text=deviceid;
    }//try
    @catch (NSException *exception) {
        NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
        [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
    }// catch
    @finally {
    }// finally
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
