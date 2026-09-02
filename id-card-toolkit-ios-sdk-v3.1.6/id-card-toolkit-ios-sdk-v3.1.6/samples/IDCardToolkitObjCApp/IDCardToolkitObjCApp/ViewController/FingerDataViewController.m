//
//  FingerDataViewController.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 20/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "FingerDataViewController.h"

@interface FingerDataViewController () {
    FingerData  *firstFingerData;
    FingerData  *secondFingerData;
}
@property(nonatomic,strong)Utils * utils;
@property(nonatomic,strong)FingerData  *selectedFingerData;
@end

@implementation FingerDataViewController
@synthesize selectedFingerData;

- (void)viewDidLoad {
    [super viewDidLoad];
    
    getFingerIndexarr=[[NSMutableArray alloc]init];
    getFingerIndex_reference_arr=[[NSMutableArray alloc]init];
    
    self.title=@"Finger data";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    self.getFingerDataButton.layer.cornerRadius=22.0;
    [self.getFingerDataButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.getFingerDataButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.getFingerDataButton.layer setShadowOpacity:0.5];
   // self.getFingerDataButton.layer.borderWidth=1.0;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    
    // Do any additional setup after loading the view.
}
-(void)viewWillAppear:(BOOL)animated {
    [super viewWillAppear:YES];
    
    [getFingerIndexarr removeAllObjects];
    [getFingerIndex_reference_arr removeAllObjects];
    [self.fingerPickerView reloadAllComponents];
    self.fingerPickerView.hidden=YES;
}
- (IBAction)getFingerDataButton:(id)sender {
    
    [getFingerIndexarr removeAllObjects];
    [getFingerIndex_reference_arr removeAllObjects];
    
    [self.utils ShowProgressBar:FINGERDATA andView:self.view];
    dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_HIGH, 0), ^(void){
    @try {
        [self.model.selectedFingerSharedArray removeAllObjects];
        CardReader *cardreader  = [self.model.cardReaderSharedArray objectAtIndex:0];
        NSArray *fingerDataArr=[cardreader getFingerData];
        
        self->firstFingerData=(FingerData *)[fingerDataArr objectAtIndex:0];
        self->secondFingerData=(FingerData *)[fingerDataArr objectAtIndex:1];
    
        [self->getFingerIndex_reference_arr addObject:[NSString stringWithFormat:@"%d",[self->firstFingerData getFingerId]]];
        [self->getFingerIndex_reference_arr addObject:[NSString stringWithFormat:@"%d",[self->secondFingerData getFingerId]]];
        
        [self->getFingerIndexarr addObject:[NSString stringWithFormat:@"%@",[self getNameforFingerIndex:[self->firstFingerData getFingerIndex]]]];
        [self->getFingerIndexarr addObject:[NSString stringWithFormat:@"%@",[self getNameforFingerIndex:[self->secondFingerData getFingerIndex]]]];
        
        self->selectedFingerData =self->firstFingerData;
        [self.model.selectedFingerSharedArray addObject:self->selectedFingerData];
    }//try
    @catch (NSException *exception) {
        NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
        [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
    }// catch
    @finally {
        dispatch_async(dispatch_get_main_queue(), ^{
             [self.utils DismissProgressBar];
            if (self->getFingerIndexarr.count>1) {
                [self.fingerPickerView reloadAllComponents];
                self.fingerPickerView.hidden=NO;
            }// if
         });// update UI main queue
      }// finally
   });//background queue
}
#pragma mark PickerView Delagates methods

- (NSInteger)numberOfComponentsInPickerView:(UIPickerView *)pickerView {
    return 2;
}
- (NSInteger)pickerView:(UIPickerView *)pickerView numberOfRowsInComponent:(NSInteger)component {
    return getFingerIndexarr.count;
}
- (NSString *)pickerView:(UIPickerView *)pickerView titleForRow:(NSInteger)row forComponent:(NSInteger)component {
    if (pickerView == self.fingerPickerView  && component==0)
        return [getFingerIndex_reference_arr objectAtIndex:row];
    else if (pickerView == self.fingerPickerView  && component==1)
        return  [getFingerIndexarr objectAtIndex:row];
    
    return nil;
}
- (void)pickerView:(UIPickerView *)pickerView didSelectRow:(NSInteger)row inComponent:(NSInteger)component {
    
    if (row ==0) {
        selectedFingerData =firstFingerData;
    }//if
    else {
        selectedFingerData =secondFingerData;
    }//else
    
    [self.fingerPickerView selectRow:row inComponent:0 animated:NO];
    [self.fingerPickerView selectRow:row inComponent:1 animated:NO];
    
    [self.model.selectedFingerSharedArray removeAllObjects];
    [self.model.selectedFingerSharedArray addObject:selectedFingerData];
    
}
- (void)goBack {
    [self.navigationController popViewControllerAnimated:YES];
}

-(NSString *)getNameforFingerIndex:(int)fingerindex {

    NSString *fingerName =@"";
    
    if (fingerindex == 0) {
        fingerName=@"None";
    }
    else if (fingerindex == 3) {
        fingerName=@"NoMeaning";
    }
    else if (fingerindex == 5) {
        fingerName=@"RightThumb";
    }
    else if (fingerindex == 9) {
        fingerName=@"RightIndex";
    }
    else if (fingerindex == 13) {
        fingerName=@"RightMiddle";
    }
    else if (fingerindex == 17) {
        fingerName=@"RightRing";
    }
    else if (fingerindex == 15) {
        fingerName=@"RightLittle";
    }
    else if (fingerindex == 6) {
        fingerName=@"LeftThumb";
    }
    else if (fingerindex == 10) {
        fingerName=@"LeftIndex";
    }
    else if (fingerindex == 14) {
        fingerName=@"LeftMiddle";
    }
    else if (fingerindex == 18) {
        fingerName=@"LeftRing";
    }
    else if (fingerindex == 22) {
        fingerName=@"LeftLittle";
    }
    
    return fingerName;

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
