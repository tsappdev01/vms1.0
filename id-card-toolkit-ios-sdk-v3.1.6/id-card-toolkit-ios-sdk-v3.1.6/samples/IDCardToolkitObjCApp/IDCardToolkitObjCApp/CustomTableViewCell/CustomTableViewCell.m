//
//  CustomTableViewCell.m
//  
//
//  Created by Federal Authority For Identity and Citizenship  on 1/9/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "CustomTableViewCell.h"

@implementation CustomTableViewCell
@synthesize keyData,valueData;

- (void)awakeFromNib {
    [super awakeFromNib];
    
    keyData.adjustsFontSizeToFitWidth=YES;
    valueData.adjustsFontSizeToFitWidth=YES;
    
    // Initialization code
}

- (void)setSelected:(BOOL)selected animated:(BOOL)animated {
    [super setSelected:selected animated:animated];

    // Configure the view for the selected state
}

@end
