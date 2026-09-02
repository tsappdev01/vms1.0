//
//  MainTableViewCell.m
//  
//
//  Created by Federal Authority For Identity and Citizenship on 10/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "MainTableViewCell.h"
#import "Utils.h"
@implementation MainTableViewCell

- (void)awakeFromNib {
    [super awakeFromNib];
    self.cellElements.layer.cornerRadius = 26.0;
    self.cellElements.layer.backgroundColor = [Utils colorFromHexString:@"#BE9647"].CGColor;
//    self.cellElements.layer.borderWidth = 2.0;
    
    // Initialization code
}

- (void)setSelected:(BOOL)selected animated:(BOOL)animated {
    [super setSelected:selected animated:animated];

    // Configure the view for the selected state
}

@end
