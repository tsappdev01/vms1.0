//
//  FaceSDKData.h
//  IDCardToolkit
//
//  Created by Prabhakar Bunga on 19/03/24.
//  Copyright © 2024 doti naresh. All rights reserved.
//
#import <Foundation/Foundation.h>
#import "ToolkitXmlDataObject.h"


@interface FaceSDKData : NSException
-(id)initWithFaceSDKData:(int)handel license:(NSString *)license;
-(int)getHandel;
-(NSString *)getLicense;
@end
