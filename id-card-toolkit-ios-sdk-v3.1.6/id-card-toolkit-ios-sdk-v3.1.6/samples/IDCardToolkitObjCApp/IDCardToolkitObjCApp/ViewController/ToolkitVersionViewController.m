//
//  ToolkitVersionViewController.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 20/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ToolkitVersionViewController.h"

@interface ToolkitVersionViewController ()

@end

@implementation ToolkitVersionViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.title=@"Toolkit version";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    
    self.toolkitVersionButton.layer.cornerRadius=22.0;
    [self.toolkitVersionButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.toolkitVersionButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.toolkitVersionButton.layer setShadowOpacity:0.5];
  //  self.toolkitVersionButton.layer.borderWidth=1.0;
    
    self.toolkitVersionLabel.hidden=YES;
    self.toolkitVersionLabel.adjustsFontSizeToFitWidth=YES;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    
    // Do any additional setup after loading the view.
}
- (IBAction)toolkitVersionButtonAction:(id)sender {
    
      @try {
        Toolkit *toolkit = self.model.toolkitShared;
        
        NSString *version = [toolkit getToolkitVersion];
//          NSString *face_lic = [toolkit getFaceSDkLicense:@"ae.emiratesid.idcard.toolkit.sample"];
//          NSLog(@"%@",face_lic);
        self.toolkitVersionLabel.hidden=NO;
        self.toolkitVersionLabel.text=version;
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
