//
//  PublicDataEFViewController.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 20/11/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "PublicDataEFViewController.h"

@interface PublicDataEFViewController () {
    NSMutableArray *listArr;
    int selectedEFType;
}
@property(nonatomic,strong)Utils * utils;
@end

@implementation PublicDataEFViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
       self.title=@"Public Data EF";
       self.navigationItem.hidesBackButton = YES;
       self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                                style:UIBarButtonItemStylePlain
                                                                               target:self
                                                                               action:@selector(goBack)];
       self.readPublicDataEFButton.layer.cornerRadius=22.0;
       [self.readPublicDataEFButton.layer setShadowOffset:CGSizeMake(5, 5)];
       [self.readPublicDataEFButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
       [self.readPublicDataEFButton.layer setShadowOpacity:0.5];
      // self.readPublicDataEFButton.layer.borderWidth=1.0;
       
       AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
       self.model = aDelegate.model;
       self.utils=aDelegate.utils;
   
    listArr=[[NSMutableArray alloc]initWithObjects:@"IDN_CN",@"ROOT_CERTIFICATE",@"NON_MODIFIABLE_DATA",@"MODIFIABLE_DATA",@"PHOTOGRAPHY",@"SIGNATURE_IMAGE",@"HOME_ADDRESS",@"WORK_ADDRESS", nil];
    // Do any additional setup after loading the view.
}

- (IBAction)readPublicDataEFButtonAction:(id)sender {
    
    PUBLIC_DATA_EF_TYPE publicDataEFType = selectedEFType+1;
   // NSLog(@"publicDataEFType %i",publicDataEFType);
     [self.utils ShowProgressBar:LOADINGDATA andView:self.view];
        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void) {
            @try {
                CardReader *cardreader = [self.model.cardReaderSharedArray objectAtIndex:0];
                
                [cardreader readPublicDataEF:publicDataEFType validateSignature:true];
                
              NSString *getParsedData =   [cardreader parseEFData:[cardreader getReadPublicDataEFByte] efDatalength:[cardreader getReadPublicDataEFLength]];
            dispatch_async(dispatch_get_main_queue(), ^{
                if ([cardreader getReadPublicDataEFLength]>0) {
                    
                    NSMutableString * getEFDataString = [[NSMutableString alloc] init];
                    
                    for (int i=0; i<[cardreader getReadPublicDataEFLength]; i++) {
                        [getEFDataString appendString:[NSString stringWithFormat:@"%02x",[cardreader getReadPublicDataEFByte][i]]];
                    }
                 //   NSLog(@"parseddata %@",parseddata);

                    [AlertView showAlertTitle:@"" withMessage:[NSString stringWithFormat:@"Parsed EF data \n\n %@",getParsedData] onView:self];
                }
              });// update UI main queue
            }// try
            @catch (NSException *exception) {
                dispatch_async(dispatch_get_main_queue(), ^{
                NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
                    [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
                });// update UI main queue
            }// catch
            @finally {
                dispatch_async(dispatch_get_main_queue(), ^{
                    [self.utils DismissProgressBar];
                });// update UI main queue
            }// finally
        });// background queue
}

#pragma mark PickerView Delagates methods

- (NSInteger)numberOfComponentsInPickerView:(UIPickerView *)pickerView {
    return 1;
}
- (NSInteger)pickerView:(UIPickerView *)pickerView numberOfRowsInComponent:(NSInteger)component {
    return listArr.count;
}
- (NSString *)pickerView:(UIPickerView *)pickerView titleForRow:(NSInteger)row forComponent:(NSInteger)component {
    selectedEFType = (int)row;
    return listArr[row];
}

- (void)pickerView:(UIPickerView *)pickerView didSelectRow:(NSInteger)row inComponent:(NSInteger)component {
    selectedEFType = (int)row;
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
