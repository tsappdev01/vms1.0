//
//  Utils.h

//  Created by Federal Authority For Identity and Citizenship  on 1/29/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "Model.h"

@implementation Model

- (id) init {
    self = [super init];
    if(self) {
        
        self.mVGurl=@"";
        self.mConfigUrl=@"";
        self.mLogDir=@"";
        self.cardReaderSharedArray=[[NSMutableArray alloc]init];
        self.selectedFingerSharedArray=[[NSMutableArray alloc]init];
    }
    return(self);
}
@end


