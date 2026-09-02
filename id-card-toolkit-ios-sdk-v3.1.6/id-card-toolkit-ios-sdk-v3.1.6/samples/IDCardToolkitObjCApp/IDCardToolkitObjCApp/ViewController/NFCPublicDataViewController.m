//
//  NFCPublicDataViewController.m
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 14/10/19.
//  Copyright © 2019 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "NFCPublicDataViewController.h"

@interface NFCPublicDataViewController ()
@property(nonatomic,strong)Utils * utils;
@end

@implementation NFCPublicDataViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    // init objects
       
       getpModDataDictionary=[[NSMutableDictionary alloc]init];
       getpNonModDataDictionary=[[NSMutableDictionary alloc]init];
       getpHomeAddressDictionary=[[NSMutableDictionary alloc]init];
       getpWorkAddressDictionary=[[NSMutableDictionary alloc]init];
       
       pIDNumberKeyarr=[[NSMutableArray alloc]init];
       pIDNumberValuearr=[[NSMutableArray alloc]init];
       pModDatakeyarr=[[NSMutableArray alloc]init];
       pModDataValuearr=[[NSMutableArray alloc]init];
       pNonModDatakeyarr=[[NSMutableArray alloc]init];
       pNonModDataValuearr=[[NSMutableArray alloc]init];
       pHomeAddresskeyarr=[[NSMutableArray alloc]init];
       pHomeAddressValuearr=[[NSMutableArray alloc]init];
       pWorkAddresskeyarr=[[NSMutableArray alloc]init];
       pWorkAddressValuearr=[[NSMutableArray alloc]init];
       
       self.photoImage.layer.cornerRadius=8.0f;
       self.photoImage.layer.borderWidth=1.0f;
       self.photoImage.layer.masksToBounds = YES;
       self.photoImage.layer.borderColor=[UIColor clearColor].CGColor;
       self.photoImage.layer.shadowOffset = CGSizeMake(-15, 20);
       self.photoImage.layer.shadowRadius = 8;
       self.photoImage.layer.shadowOpacity = 0.1;
       
       self.signImage.layer.cornerRadius=8.0f;
       self.signImage.layer.borderWidth=1.0f;
       self.signImage.layer.masksToBounds = YES;
       self.signImage.layer.borderColor=[UIColor clearColor].CGColor;
       self.signImage.layer.shadowOffset = CGSizeMake(-15, 20);
       self.signImage.layer.shadowRadius = 8;
       self.signImage.layer.shadowOpacity = 0.1;
       
       self.title=@"NFC Public data";
       self.navigationItem.hidesBackButton = YES;
       self.navigationItem.leftBarButtonItem = [[UIBarButtonItem alloc] initWithImage:[[UIImage imageNamed:@"backbutton"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal]
                                                                                style:UIBarButtonItemStylePlain
                                                                               target:self
                                                                               action:@selector(goBack)];
       
        AppDelegate *aDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
       self.model = aDelegate.model;
       self.utils=aDelegate.utils;

    // Do any additional setup after loading the view.
}
-(void)getPublicaDataClass:(CardPublicData *)cardPublicData {
    NSLog(@"NFC Details View @s",cardPublicData);
            dispatch_async(dispatch_get_main_queue(), ^{
                
                [self->pIDNumberKeyarr addObject:@"IDNumber"];
                [self->pIDNumberKeyarr addObject:@"CardNumber"];
                [self->pIDNumberValuearr addObject:[cardPublicData getIdNumber]];
                [self->pIDNumberValuearr addObject:[cardPublicData getCardNumber]];
                        
                    PublicDataParse *parseData=[[PublicDataParse alloc]init];
                self->getpModDataDictionary=[parseData getModifiablePublicDataDetails:[cardPublicData getModifiablePublicData]];
                self->getpNonModDataDictionary=[parseData getNonModifiablePublicDataDetails:[cardPublicData getNonModifiablePublicData]];
                self->getpHomeAddressDictionary=[parseData getHomeAddressDetails:[cardPublicData getHomeAddress]];
                self->getpWorkAddressDictionary=[parseData getWorkAddressDetails:[cardPublicData getWorkAddress]];
                    
                BOOL isEmpty = ([self->getpModDataDictionary count] == 0);
                    if (!isEmpty) {
                        NSArray * pModDataKeys;
                        pModDataKeys = [self->getpModDataDictionary allKeys];
                        for (NSString *key in pModDataKeys) {
                            id value = self->getpModDataDictionary[key];
                            [self->pModDatakeyarr addObject:key];
                            [self->pModDataValuearr addObject:value];
                        }// forloop
                        NSArray * pNonModDataKeys;
                        pNonModDataKeys = [self->getpNonModDataDictionary allKeys];
                        for (NSString *key in pNonModDataKeys) {
                            id value = self->getpNonModDataDictionary[key];
                            [self->pNonModDatakeyarr addObject:key];
                            [self->pNonModDataValuearr addObject:value];
                        }// forloop
                        NSArray * pHomeAddressKeys;
                        pHomeAddressKeys = [self->getpHomeAddressDictionary allKeys];
                        for (NSString *key in pHomeAddressKeys) {
                            id value = self->getpHomeAddressDictionary[key];
                            [self->pHomeAddresskeyarr addObject:key];
                            [self->pHomeAddressValuearr addObject:value];
                        }// forloop
                        NSArray * pWorkAddressKeys;
                        pWorkAddressKeys = [self->getpWorkAddressDictionary allKeys];
                        for (NSString *key in pWorkAddressKeys) {
                            id value = self->getpWorkAddressDictionary[key];
                            [self->pWorkAddresskeyarr addObject:key];
                            [self->pWorkAddressValuearr addObject:value];
                        }// forloop
                            
                        NSData *photoData = [NSData dataWithBytes:[cardPublicData getCardHolderPhoto] length:[cardPublicData getCardHolderPhotoLength]];
                        UIImage *pPhotoImage=[[UIImage alloc] initWithData:photoData];
                            
                        NSData *signData = [NSData dataWithBytes:[cardPublicData getHolderSignatureImage] length:[cardPublicData getHolderSignatureImageLength]];
                        UIImage *pSignImage=[[UIImage alloc] initWithData:signData];
                        self.photoImage.image=pPhotoImage;;
                        self.signImage.image=pSignImage;
                        [self.listTableview reloadData];
                        }//if
        });// update UI main queue
}

#pragma mark TableView Delegate and Data source Methods

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return 5;
}
- (NSString *)tableView:(UITableView *)tableView titleForHeaderInSection:(NSInteger)section {
    switch (section) {
        case 0:
            return @"IDNumber info";
        case 1:
            return @"ModData info";
        case 2:
            return @"NonModData info";
        case 3:
            return @"HomeAddress info";
        case 4:
            return @"WorkAddress info";
    }//switch
    return nil;
}
- (void)tableView:(UITableView *)tableView willDisplayHeaderView:(UIView *)view forSection:(NSInteger)section {
    view.tintColor = [Utils colorFromHexString:@"#BE9647"];
}
- (CGFloat)tableView:(UITableView *)tableView heightForHeaderInSection:(NSInteger)section; {
    return 32.0;
}
- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    switch (section) {
        case 0:
            return pIDNumberKeyarr.count;
        case 1:
            return pModDatakeyarr.count;
        case 2:
            return pNonModDatakeyarr.count;
        case 3:
            return pHomeAddresskeyarr.count;
        case 4:
            return pWorkAddresskeyarr.count;
    }//switch
    return 0 ;
}
- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    
    static NSString *customTabelId=@"identifierCell";
    CustomTableViewCell *cell=(CustomTableViewCell *)[tableView dequeueReusableCellWithIdentifier:customTabelId];
    if (cell == nil) {
        cell = [[CustomTableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:customTabelId];
    }// if
    switch (indexPath.section) {
        case 0:
            cell.keyData.text = [NSString stringWithFormat:@"%@",[pIDNumberKeyarr objectAtIndex:indexPath.row]];
            cell.valueData.text = [NSString stringWithFormat:@"%@",[pIDNumberValuearr objectAtIndex:indexPath.row]];
            break;
        case 1:
            cell.keyData.text = [NSString stringWithFormat:@"%@",[pModDatakeyarr objectAtIndex:indexPath.row]];
            cell.valueData.text = [NSString stringWithFormat:@"%@",[pModDataValuearr objectAtIndex:indexPath.row]];
            break;
        case 2:
            cell.keyData.text =[NSString stringWithFormat:@"%@", [pNonModDatakeyarr objectAtIndex:indexPath.row]];
            cell.valueData.text = [NSString stringWithFormat:@"%@",[pNonModDataValuearr objectAtIndex:indexPath.row]];
            break;
        case 3:
            cell.keyData.text = [NSString stringWithFormat:@"%@",[pHomeAddresskeyarr objectAtIndex:indexPath.row]];
            cell.valueData.text = [NSString stringWithFormat:@"%@",[pHomeAddressValuearr objectAtIndex:indexPath.row]];
            break;
        case 4:
            cell.keyData.text =[NSString stringWithFormat:@"%@", [pWorkAddresskeyarr objectAtIndex:indexPath.row]];
            cell.valueData.text =[NSString stringWithFormat:@"%@", [pWorkAddressValuearr objectAtIndex:indexPath.row]];
            break;
        default:
            break;
    }//switch
    return cell;
}

- (void)goBack {
    [self.navigationController popViewControllerAnimated:YES];
}
#pragma mark didReceiveMemoryWarning
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
