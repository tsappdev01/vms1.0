//
//  ListReadersViewController.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 20/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "ListReadersViewController.h"

@interface ListReadersViewController () {
    
    NSMutableArray *readerArr;
    NSArray *readers;
}
@property(nonatomic,strong) CardReader *cardreader;
@property(nonatomic,strong)Utils * utils;
@end

@implementation ListReadersViewController
@synthesize cardreader;

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.title=@"List of Readers ";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]style:UIBarButtonItemStylePlain target:self action:@selector(goBack)];
    self.listReaderButton.layer.cornerRadius=22.0;
    [self.listReaderButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.listReaderButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.listReaderButton.layer setShadowOpacity:0.5];
//    self.listReaderButton.layer.borderWidth=1.0;
    
    readerArr=[[NSMutableArray alloc]init];
    self.listReaderPickerView.hidden=YES;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    
    // Do any additional setup after loading the view.
}
-(void)viewWillAppear:(BOOL)animated {
    [super viewWillAppear:YES];
    
    [readerArr removeAllObjects];
}
- (IBAction)listReaderButtonAction:(id)sender {
    
    [readerArr removeAllObjects];
    
    [self.utils ShowProgressBar:LISTREADERS andView:self.view];
//    dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_HIGH, 0), ^(void){
        @try {
            Toolkit *toolkit = self.model.toolkitShared;
            self->readers=[toolkit listReaders];
            NSLog(@"readers %@",self->readers);
        }//try
        @catch (NSException *exception) {
            NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
            [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
        }// catch
        @finally {
            dispatch_async(dispatch_get_main_queue(), ^{
                for (int i =0; i<self->readers.count; i++) {
                    NSString *readersName = [(CardReader *)[self->readers objectAtIndex:i] getName];
                    [self->readerArr addObject:readersName];
                }//for
                if (self->readerArr.count>0) {
                    self.listReaderPickerView.hidden=NO;
                    [self.listReaderPickerView reloadAllComponents];
                    
                    self->cardreader=(CardReader *)[self->readers objectAtIndex:0];
                    [self.model.cardReaderSharedArray removeAllObjects];
                    [self.model.cardReaderSharedArray addObject:self->cardreader];
                }// if
                else {
                    self.listReaderPickerView.hidden=YES;
                    [self.listReaderPickerView reloadAllComponents];
                }// else
                [self.utils DismissProgressBar];
            });// update UI main queue
        }// finally
   // });// background queue
}
- (void)goBack {
    [self.navigationController popViewControllerAnimated:YES];
}
#pragma mark PickerView Delagates methods

- (NSInteger)numberOfComponentsInPickerView:(UIPickerView *)pickerView {
    return 1;
}
- (NSInteger)pickerView:(UIPickerView *)pickerView numberOfRowsInComponent:(NSInteger)component {
    return readerArr.count;
}
- (NSString *)pickerView:(UIPickerView *)pickerView titleForRow:(NSInteger)row forComponent:(NSInteger)component {
    if (pickerView == self.listReaderPickerView  && component==0)
        return [readerArr objectAtIndex:row];
    
    return nil;
}
- (void)pickerView:(UIPickerView *)pickerView didSelectRow:(NSInteger)row inComponent:(NSInteger)component {
    
    cardreader=(CardReader *)[self->readers objectAtIndex:row];
    [self.listReaderPickerView selectRow:row inComponent:0 animated:NO];
    
    [self.model.cardReaderSharedArray removeAllObjects];
    [self.model.cardReaderSharedArray addObject:cardreader];
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
