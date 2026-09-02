//
//  InterfaceTypeViewController.h
//  IDCardToolkitObjCApp
//
//  Created by Federal Authority For Identity and Citizenship on 02/01/18.
//  Copyright © 2018 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Global.h"

@interface InterfaceTypeViewController : UIViewController
@property (weak, nonatomic) IBOutlet UILabel *cardInterfacetypeLabel;
@property (strong, nonatomic) IBOutlet UIButton *getcardInterfaceTypeButton;
@property (nonatomic, strong) Model *model;
- (IBAction)getcardInterfaceTypeButtonAction:(id)sender;
@end
