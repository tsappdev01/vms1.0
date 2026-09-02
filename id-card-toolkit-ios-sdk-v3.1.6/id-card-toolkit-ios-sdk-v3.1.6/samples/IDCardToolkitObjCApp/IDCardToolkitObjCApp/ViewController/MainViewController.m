//
//  MainViewController.m
//  
//
//  Created by Federal Authority For Identity and Citizenship on 10/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "MainViewController.h"

@interface MainViewController ()
@end

@implementation MainViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    //

    elementsArray = @[
        @{@"API" : @"Initialize"},@{@"API" : @"Register Device"},@{@"API" : @"Get Readers"},@{@"API" : @"Connect Reader"},@{@"API" : @"Disconnect Reader"},@{@"API" : @"Read public data"},@{@"API" : @"Read public data EF"},@{@"API" : @"NFC Read public data"},@{@"API" : @"NFC Read public data EF"},@{@"API" : @"Family Book"},@{@"API" : @"Card Status"},@{@"API" : @"Pki Certificates"},@{@"API" : @"Finger Data"},@{@"API" : @"Pin Reset"},@{@"API" : @"Unblock Pin"},@{@"API" : @"Bio Auth On Server"},@{@"API" : @"Card Status And Bio Auth"},@{@"API" : @"Sign data"},@{@"API" : @"PKI Auth"},@{@"API" : @"Config Certificate Expiry Date"},@{@"API" : @"Card Serial Number"},@{@"API" : @"License Expiry Date"},@{@"API" : @"Toolkit Version"},@{@"API" : @"Reader EmiratesId"},@{@"API" : @"Device ID"},@{@"API" : @"Card Interface type"},@{@"API" : @"CleanUp"}
      ];
    
    self.title=@"Emirates ID Toolkit ObjectiveC Sample app";
    
    UINavigationBar *bar = [self.navigationController navigationBar];
    [bar setBarTintColor:[Utils colorFromHexString:@"#BE9647"]];
    bar.barStyle=UIBarStyleBlackOpaque;
    bar.translucent=NO;
   
    if (@available(iOS 13.0, *)) {
        UINavigationBarAppearance *appearance = [[UINavigationBarAppearance alloc] init];
        [appearance configureWithOpaqueBackground];
        appearance.backgroundColor = [Utils colorFromHexString:@"#BE9647"];
        UINavigationBar *bar = [self.navigationController navigationBar];
        bar.standardAppearance = appearance;
        bar.scrollEdgeAppearance = appearance;
    } else {
        // Fallback on earlier versions
    }
   
    
    [[[self navigationController] navigationBar] setTitleTextAttributes:@{NSForegroundColorAttributeName: [UIColor blackColor]}];

    mainTableView.backgroundColor=[Utils colorFromHexString:@"#F4F0E8"];
    mainTableView.autoresizingMask = UIViewAutoresizingFlexibleHeight | UIViewAutoresizingFlexibleWidth;
    mainTableView.showsVerticalScrollIndicator = YES;
    mainTableView.userInteractionEnabled = YES;
    mainTableView.bounces = YES;
    mainTableView.tableFooterView = [[UIView alloc] initWithFrame:CGRectZero];
    mainTableView.estimatedRowHeight = 64;
    mainTableView.rowHeight = UITableViewAutomaticDimension;
    
    LogoImage.contentMode = UIViewContentModeScaleAspectFit;
    LogoImage.backgroundColor=[Utils colorFromHexString:@"#F4F0E8"];

    LogoImage.layer.borderColor=[UIColor lightGrayColor].CGColor;
    LogoImage.layer.borderWidth=1;
        
    // Do any additional setup after loading the view.
}
#pragma mark TableView Delegate and Data source Methods
- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
     return elementsArray.count ;
}
- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    return 1;
}
-(CGFloat)tableView:(UITableView *)tableView heightForHeaderInSection:(NSInteger)section
{
    if ( UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad )
        return 0;
    else return 0;
}
-(UIView *)tableView:(UITableView *)tableView viewForHeaderInSection:(NSInteger)section
{
    UIView *headerView = [UIView new];
    [headerView setBackgroundColor:[UIColor clearColor]];
    return headerView;
}
- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    
    static NSString *customTabelId=@"identifierCell";
    MainTableViewCell *cell=(MainTableViewCell *)[tableView dequeueReusableCellWithIdentifier:customTabelId];
    if (cell == nil) {
        cell = [[MainTableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:customTabelId];
    }// if
    
    cell.selectionStyle = UITableViewCellSelectionStyleNone;
    
    cell.cellElements.text=[[elementsArray  objectAtIndex:indexPath.section]  objectForKey:@"API"];
    cell.backgroundColor = [UIColor clearColor];
    cell.separatorInset = UIEdgeInsetsMake(0, CGFLOAT_MAX, 0, 0);
    cell.clipsToBounds = TRUE;
    
    return cell;
}
- (CGFloat)tableView:(UITableView *)tableView heightForRowAtIndexPath:(NSIndexPath *)indexPath {
    return 72.0;
}

-(void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:
(NSIndexPath *)indexPath{
    
    [tableView deselectRowAtIndexPath:indexPath animated:YES];
    
    if (indexPath.section==INITIALIZE_ROW) {
          [self performSegueWithIdentifier:@"InitilizeView" sender:self];
    }// if
    else if (indexPath.section==REGISTERDEVICE_ROW){
        [self performSegueWithIdentifier:@"RegisterDeviceView" sender:self];
    }//else if
    else if (indexPath.section==LISTREADER_ROW){
         [self performSegueWithIdentifier:@"ListReaderView" sender:self];
    }
    else if (indexPath.section==CONNECTDEVICE_ROW){
        [self performSegueWithIdentifier:@"ConnectDeviceView" sender:self];
    }//else if
    else if (indexPath.section==DISCONNECT_ROW) {
        [self performSegueWithIdentifier:@"DisconnectView" sender:self];
    }//else if
    else if (indexPath.section==PUBLICDATA_ROW){
         [self performSegueWithIdentifier:@"PublicDataView" sender:self];
    }//else if
    else if (indexPath.section==PUBLICDATAEF_ROW){
        [self performSegueWithIdentifier:@"PublicDataEFView" sender:self];
    }//else if
    else if (indexPath.section==NFC_PUBLICDATA_ROW) {
        [self performSegueWithIdentifier:@"NFCView" sender:self];
    }//else if
    else if (indexPath.section==NFC_PUBLICDATAEF_ROW) {
        [self performSegueWithIdentifier:@"NFCPublicDataEFView" sender:self];
    }//else if
    else if (indexPath.section==FAMILTYBOOK_ROW){
        [self performSegueWithIdentifier:@"FamilyBookView" sender:self];
    }//else if
    else if (indexPath.section==CARDSTATUS_ROW){
         [self performSegueWithIdentifier:@"CheckCardView" sender:self];
    }//else if
    else if (indexPath.section==CERTIFICATES_ROW){
        [self performSegueWithIdentifier:@"PKICertificatesView" sender:self];
    }//else if
    else if (indexPath.section==FINGERDATA_ROW) {
        [self performSegueWithIdentifier:@"FingerDataView" sender:self];
    }//else if
    else if (indexPath.section==PINRESET_ROW){
        [self performSegueWithIdentifier:@"PinResetView" sender:self];
    }//else if
    else if (indexPath.section==UNBLOCK_PIN) {
        [self performSegueWithIdentifier:@"UnblockPinView" sender:self];
    }//else if
    else if (indexPath.section==BIOAUTH_ROW){
        [self performSegueWithIdentifier:@"BiometricAuthView" sender:self];
    }//else if
    else if (indexPath.section==CHECKAUTHBIO_ROW){
        [self performSegueWithIdentifier:@"CheckAuthBioView" sender:self];
    }//else if
    else if (indexPath.section==SIGNDATA_ROW){
        [self performSegueWithIdentifier:@"SignDataView" sender:self];
    }//else if
    else if (indexPath.section==PKIAUTH_ROW) {
         [self performSegueWithIdentifier:@"PKIAuthView" sender:self];
    }//else if
    else if (indexPath.section==CONFIG_CERT_EXPDATE_ROW) {
        [self performSegueWithIdentifier:@"ConfigCertView" sender:self];
    }//else if
    else if (indexPath.section==CSN_ROW) {
        [self performSegueWithIdentifier:@"CardSerialNumView" sender:self];
    }//else if
    else if (indexPath.section==LICENSE_ROW) {
        [self performSegueWithIdentifier:@"LicenseExpiryDateView" sender:self];
    }//else if
    else if (indexPath.section==TOOLKITVERSION_ROW) {
         [self performSegueWithIdentifier:@"ToolkitVersionView" sender:self];
    }//else if
    else if (indexPath.section==READEREMIRATED_ID_ROW) {
        [self performSegueWithIdentifier:@"ReaderEmiratesIDView" sender:self];
    }//else if
    else if (indexPath.section==DEVICEID_ROW) {
         [self performSegueWithIdentifier:@"DeviceIDView" sender:self];
    }//else if
    else if (indexPath.section==INTERFACETYPE_ROW) {
        [self performSegueWithIdentifier:@"InterfaceTypeView" sender:self];
    }//else if
    else if (indexPath.section==CELANUP_ROW) {
        [self performSegueWithIdentifier:@"CleanUpView" sender:self];
    }//else if
}

- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender {
    
}
- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

@end
