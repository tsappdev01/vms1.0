//
//  FamilyBookViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship on 5/14/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "FamilyBookViewController.h"

@interface FamilyBookViewController () {
    
    NSMutableArray *pHeadofFamilyKeyarr;
    NSMutableArray *pHeadofFamilyValuearr;
}
@property(nonatomic,strong)Utils * utils;
@end

@implementation FamilyBookViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
     self.listTable.hidden=YES;
     self.familyBookEnterPinView.hidden=NO;
    
        // init objects
    pHeadofFamilyKeyarr =[[NSMutableArray alloc]init];
    pHeadofFamilyValuearr=[[NSMutableArray alloc]init];
    
    self.pintext.keyboardType=UIKeyboardTypeNumberPad;
    self.pintext.keyboardAppearance = UIKeyboardAppearanceLight;
    UIToolbar* enterPinNumberToolbar = [[UIToolbar alloc]initWithFrame:CGRectMake(0, 0, 320, 50)];
    enterPinNumberToolbar.barStyle = UIBarStyleBlackTranslucent;
    enterPinNumberToolbar.items = @[[[UIBarButtonItem alloc]initWithTitle:@"Cancel" style:UIBarButtonItemStyleDone target:self action:@selector(enterPinCancelNumberPad)],
                                    [[UIBarButtonItem alloc]initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil],
                                    [[UIBarButtonItem alloc]initWithTitle:@"Done" style:UIBarButtonItemStyleDone target:self action:@selector(enterPinDoneWithNumberPad)]];
    [enterPinNumberToolbar sizeToFit];
    self.pintext.inputAccessoryView = enterPinNumberToolbar;
    
    self.title=@"Family book";
    self.navigationItem.hidesBackButton = YES;
    self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                             style:UIBarButtonItemStylePlain
                                                                            target:self
                                                                            action:@selector(goBack)];
    
    self.readfamilyBookButton.layer.cornerRadius=22.0;
    [self.readfamilyBookButton.layer setShadowOffset:CGSizeMake(5, 5)];
    [self.readfamilyBookButton.layer setShadowColor:[[UIColor lightGrayColor] CGColor]];
    [self.readfamilyBookButton.layer setShadowOpacity:0.5];
   // self.readfamilyBookButton.layer.borderWidth=1.0;
    
    AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    self.model = aDelegate.model;
    self.utils=aDelegate.utils;
    
    // Do any additional setup after loading the view.
}
#pragma mark viewWillAppear

-(void)viewWillAppear:(BOOL)animated {
    [super viewWillAppear:YES];
    
    self.pintext.text=@"";
    [self.pintext resignFirstResponder];
}
- (IBAction)readfamilyBookButtonAction:(id)sender {
    
    
    [self.pintext resignFirstResponder];
    if ([self.pintext.text length]==0) {
        [AlertView showAlertTitle:ALERT withMessage:ENTER_PIN onView:self];
    }// if
    else {
    if ([self.pintext.text length]<4) {
        [AlertView showAlertTitle:ALERT withMessage:MIN_PIN onView:self];
        self.pintext.text=@"";
    }// if
    else if ([self.pintext.text length]>16) {
        [AlertView showAlertTitle:ALERT withMessage:MAX_PIN onView:self];
        self.pintext.text=@"";
    }// else if
    else {
      [self.utils ShowProgressBar:READING_CARDDEATILS andView:self.view];
      NSString *requestId=[Utils generateSecureKey];
      dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
      @try {          
          CardReader *cardreader  = [self.model.cardReaderSharedArray objectAtIndex:0];
          DataProtectionKey *data_protection_key=self.model.dataprotectedShared;
          
        NSString *request_handle =  [cardreader prepareRequest:requestId];
        if (request_handle.length==0 || [request_handle isEqualToString:@""] || [request_handle isEqualToString:@"(null)"] ) {
            [AlertView showAlertTitle:ALERT withMessage:REQUEST_HANDLE_EMPTY onView:self];
        }//if
        else {
            NSString *encodePin=[Utils setEncrytion:request_handle data:self.pintext.text publickey:[data_protection_key getPublicKey] keylength:[data_protection_key getKeyLength]];
            if (encodePin.length==0 || [encodePin isEqualToString:@""] || [encodePin isEqualToString:@"(null)"] ) {
                [AlertView showAlertTitle:ALERT withMessage:ENCODE_PIN_EMPTY onView:self];
            }//if
            else {
            CardFamilyBookData *familybook = [cardreader readFamilyBookData:encodePin];
            dispatch_async(dispatch_get_main_queue(), ^{
                NSString *xmlString =[familybook getXmlString];
                NSString *errorMsg =  [self.utils validateToolkitResponse:requestId xmlstring:xmlString];
                if (![errorMsg isEqualToString:@""] || errorMsg.length>0) {
                    [AlertView showAlertTitle:ALERT withMessage:errorMsg onView:self];
                }//if
                else {
                    FamilyBookParse *FamilyBookparse=[[FamilyBookParse alloc]init];
                    HeadOfFamily *headfamily = [familybook getHeadOfFamily];
                    [FamilyBookparse getHeadOfFamilyDetails:headfamily];
                    
                    NSArray *wifeArr=[familybook getWives];
                    for (int i =0; i<wifeArr.count; i++) {
                        Wife *wife = (Wife *)[wifeArr objectAtIndex:i];
                        [FamilyBookparse getWifeDetails:wife index:i];
                    }//for loop
                        
                    NSArray *childArr=[familybook getChildren];
                    for (int i =0; i<childArr.count; i++) {
                        Child *child = (Child *)[childArr objectAtIndex:i];
                        [FamilyBookparse getChildDetails:child index:i];
                    }//for loop
                    [self->pHeadofFamilyKeyarr addObjectsFromArray:[FamilyBookparse familyBookKey]];
                    [self->pHeadofFamilyValuearr addObjectsFromArray:[FamilyBookparse familyBookValue]];
                }//else
               });// update UI main queue
             }//else
           }//else
         }//try
          @catch (NSException *exception) {
              NSString *vgresponse =exception.userInfo[@"ErrorResponse"];
              if (![vgresponse isEqualToString:@""] && vgresponse.length>0) {
                  NSString *errorMsg =  [self.utils validateToolkitResponse:requestId xmlstring:vgresponse];
                  if (![errorMsg isEqualToString:@""] && errorMsg.length>0) {
                      [AlertView showAlertTitle:ALERT withMessage:errorMsg onView:self];
                  }//if
                  else {
                      NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
                      [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
                  }//else
              }//if
              else {
                  NSString *exe =[NSString stringWithFormat:@"%@\n%@\n%@",exception.name,exception.description,exception.userInfo];
                  [AlertView showAlertTitle:ALERT withMessage:exe onView:self];
              }
          }// catch
        @finally {
            dispatch_async(dispatch_get_main_queue(), ^{
                [self.utils DismissProgressBar];
                self.pintext.text=@"";
                if (self->pHeadofFamilyKeyarr.count>0) {
                    self.familyBookEnterPinView.hidden=YES;
                    self.listTable.hidden=NO;
                    [self.listTable reloadData];
                }// if
             });// update UI main queue
           }// finally
         });//background queue
       }//else
   }//else
}
#pragma mark Updating UI Methods

-(void)enterPinCancelNumberPad {
    [self.pintext resignFirstResponder];
    self.pintext.text = @"";
}
-(void)enterPinDoneWithNumberPad {
    [self.pintext resignFirstResponder];
}
- (BOOL)textFieldShouldReturn:(UITextField *)textField {
    [self.pintext setUserInteractionEnabled:YES];
    [self.pintext resignFirstResponder];
    return YES;
}
- (void)goBack {
    [self.navigationController popViewControllerAnimated:YES];
}
#pragma mark TableView Delegate and Data source Methods

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
     return 1;;
}
-(NSString *)tableView:(UITableView *)tableView titleForHeaderInSection:(NSInteger)section{
    return @"Family Book Data";
}
- (void)tableView:(UITableView *)tableView willDisplayHeaderView:(UIView *)view forSection:(NSInteger)section {
    view.tintColor = [Utils colorFromHexString:@"#F4F0E8"];
    
}
- (CGFloat)tableView:(UITableView *)tableView heightForHeaderInSection:(NSInteger)section; {
    return 32.0;
}
- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    return pHeadofFamilyKeyarr.count;
}
- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    
    static NSString *customTabelId=@"identifierCell";
    CustomTableViewCell *cell=(CustomTableViewCell *)[tableView dequeueReusableCellWithIdentifier:customTabelId];
    if (cell == nil) {
        cell = [[CustomTableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:customTabelId];
    }// if
    cell.keyData.text = [NSString stringWithFormat:@"%@",[pHeadofFamilyKeyarr objectAtIndex:indexPath.row]];
    cell.valueData.text = [NSString stringWithFormat:@"%@",[pHeadofFamilyValuearr objectAtIndex:indexPath.row]];
    
    return cell;
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
