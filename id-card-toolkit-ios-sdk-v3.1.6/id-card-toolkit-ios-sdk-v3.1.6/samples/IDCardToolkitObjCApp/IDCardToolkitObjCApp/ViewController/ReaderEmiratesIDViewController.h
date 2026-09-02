//
//  ReaderEmiratesIDViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 20/12/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface ReaderEmiratesIDViewController : UIViewController
@property (weak, nonatomic) IBOutlet UILabel *readerNameLabel;
@property (strong, nonatomic) IBOutlet UIButton *readerWithIDButton;
@property (nonatomic, strong) Model *model;
- (IBAction)readerWithIDButtonAction:(id)sender;

@end
