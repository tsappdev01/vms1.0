//
//  ReaderEmiratesIDViewController.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 20/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ReaderEmiratesIDViewController.h"

@interface ReaderEmiratesIDViewController ()
@property(nonatomic,strong)Utils * utils;
@end

@implementation ReaderEmiratesIDViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.title=@"Reader with ID Card";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    
    self.readerWithIDButton.layer.cornerRadius=22.0;
    [self.readerWithIDButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.readerWithIDButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.readerWithIDButton.layer setShadowOpacity:0.5];
    
  //  self.readerWithIDButton.layer.borderWidth=1.0;
    self.readerNameLabel.hidden=YES;
    self.readerNameLabel.adjustsFontSizeToFitWidth=YES;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    
    // Do any additional setup after loading the view.
}
- (IBAction)readerWithIDButtonAction:(id)sender {
    
    [self.utils ShowProgressBar:READERNAME andView:self.view];
    dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
      @try {
        Toolkit *toolkit = self.model.toolkitShared;
       CardReader  *cardreader = [toolkit getReaderWithEmiratesId];
        
      dispatch_async(dispatch_get_main_queue(), ^{
        self.readerNameLabel.hidden = NO;
        self.readerNameLabel.text=[cardreader getName];
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
