//
//  InitilizeViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship on 10/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "InitilizeViewController.h"

@interface InitilizeViewController () {
   
    unsigned char *publickey;
    unsigned int key_len;
}
@property(nonatomic,strong) Utils * utils;
@end

@implementation InitilizeViewController
@synthesize toolkit;

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.title=@"Toolkit Initilize";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                              style:UIBarButtonItemStylePlain
                                                                             target:self
                                                                             action:@selector(goBack)];
    self.statusLabel.hidden=YES;
    
    self.initlizeButton.layer.cornerRadius=22.0;
    [self.initlizeButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.initlizeButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.initlizeButton.layer setShadowOpacity:0.5];
        
    self.statusLabel.adjustsFontSizeToFitWidth=YES;
    self.statusLabel.font=[UIFont boldSystemFontOfSize:16.0];
    self.statusLabel.textColor=[UIColor whiteColor];
    self.statusLabel.backgroundColor=[UIColor blackColor];
    
    self.statusLabel.layer.cornerRadius=16.0f;
    self.statusLabel.layer.borderWidth=0.1f;
    self.statusLabel.layer.masksToBounds=YES;
    self.statusLabel.layer.borderColor=[UIColor clearColor].CGColor;

    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(CleanUpNotification:)
                                                 name:@"TOOLKITCLEANUP"
                                               object:nil];
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    self.switchLabel.text=@"Offline";
    self.configPath.text=@"Config directory *";
    
    [self offlineServicesPath];
    // Do any additional setup after loading the view.
}
-(void)offlineServicesPath {
 
    NSArray *arrPaths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory , NSUserDomainMask, YES);
    NSString *documentsDir = [arrPaths objectAtIndex:0];
    NSString *fileName = [NSString stringWithFormat:@"%@",[documentsDir stringByAppendingPathComponent:@""]];
    NSString *default_log_paramsPath = [NSString stringWithFormat:@"%@/",fileName];
    NSString *default_config_paramsPath = [NSString stringWithFormat:@"%@",fileName];
    
    //    default_log_paramsPath
    NSString *dummyUrl =@""; //Dummy url and keep internet disconnect for offline
    self.model.mLogDir=default_log_paramsPath;
    self.model.mVGurl= dummyUrl;
    self.model.mConfigUrl=default_config_paramsPath;
    
    self.configurlText.text =default_config_paramsPath;
    self.vgurlText.text=dummyUrl;
    self.logDirText.text=default_log_paramsPath;
}
-(void)onlineServicesPath {
    
    NSArray *arrPaths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory , NSUserDomainMask, YES);
    NSString *documentsDir = [arrPaths objectAtIndex:0];
    NSString *fileName = [NSString stringWithFormat:@"%@",[documentsDir stringByAppendingPathComponent:@""]];
    NSString *default_config_paramsPath = [NSString stringWithFormat:@"%@/",fileName];
    self.model.mLogDir=default_config_paramsPath;
    self.logDirText.text=default_config_paramsPath;
}

- (IBAction)customSwitchAction:(id)sender {
    
    if (self.customSwitch.isOn==YES) {
        self.switchLabel.text=@"Online";
        self.configPath.text=@"Config url *";
        self.configurlText.text =@"";
        self.vgurlText.text=@"";
    
        [self onlineServicesPath];
    }
    else {
        self.switchLabel.text=@"Offline";
        self.configPath.text=@"Config directory *";
        
        [self offlineServicesPath];
    }
}

- (NSString *)extracted {
    return [Utils getOnlineconfigParams];
}

- (IBAction)initlizeButtonAction:(id)sender {
    
    [self.utils ShowProgressBar:LOADINGDATA andView:self.view];
    
     self.model.mVGurl=self.vgurlText.text;
     self.model.mConfigUrl=self.configurlText.text;
    
    [self.vgurlText resignFirstResponder];
    [self.configurlText resignFirstResponder];
    [self.logDirText resignFirstResponder];

    NSString *configParams=@"";
    if (self.customSwitch.isOn==YES) {
        configParams=[self extracted]; //Online Configuration
    }
    else {
        configParams=[Utils getOfflineconfigParams]; //Offline Configuration
    }
    NSLog(@"configParams %@",configParams);
      dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
    @try {
        
        self.toolkit =[[Toolkit alloc]initWithToolkit:YES configParams:configParams];
      
        dispatch_async(dispatch_get_main_queue(), ^{
            self.model.toolkitShared=self->toolkit;
            self.statusLabel.hidden=NO;
            self.statusLabel.text=INITIALIZE;
            
            self.model.dataprotectedShared=[self->toolkit getDataProtectionKey];
            
        });// update UI main queue
    }//try
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
-(void)CleanUpNotification:(NSNotification *) notification {
    
    if ([[notification name] isEqualToString:@"TOOLKITCLEANUP"]) {
        NSLog (@"Successfully received the notification!");
        [toolkit cleanup];
    }//if
}
- (void)goBack {
    [self.navigationController popViewControllerAnimated:YES];
}
- (BOOL)textFieldShouldReturn:(UITextField *)textField {
    
    [self.vgurlText setUserInteractionEnabled:YES];
    [self.vgurlText resignFirstResponder];
    [self.configurlText setUserInteractionEnabled:YES];
    [self.configurlText resignFirstResponder];
    [self.logDirText setUserInteractionEnabled:YES];
    [self.logDirText resignFirstResponder];
    
    return YES;
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
