//
//  InterfaceTypeViewController.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 02/01/18.
//  Copyright © 2018 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "InterfaceTypeViewController.h"

@interface InterfaceTypeViewController ()

@end

@implementation InterfaceTypeViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    self.title=@"Device ID";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    
    self.getcardInterfaceTypeButton.layer.cornerRadius=22.0;
    [self.getcardInterfaceTypeButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.getcardInterfaceTypeButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.getcardInterfaceTypeButton.layer setShadowOpacity:0.5];
    
 //   self.getcardInterfaceTypeButton.layer.borderWidth=1.0;
    self.cardInterfacetypeLabel.hidden=YES;
    self.cardInterfacetypeLabel.adjustsFontSizeToFitWidth=YES;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    
    // Do any additional setup after loading the view.
}
- (IBAction)getcardInterfaceTypeButtonAction:(id)sender {
    
    @try {
         CardReader *cardreader  = [self.model.cardReaderSharedArray objectAtIndex:0];
        
        int type = [cardreader getInterfaceType];
        self.cardInterfacetypeLabel.hidden = NO;
        if (type==1) {
            self.cardInterfacetypeLabel.text=@"Contact Interface";
        }//if
        else if (type ==2) {
            self.cardInterfacetypeLabel.text=@"Contact Less Interface";
        }//else if
        else {
            self.cardInterfacetypeLabel.text=@"NULL";
        }//else
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
