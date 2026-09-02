//
//  FingerprintImage.h
//  Objectivec-Framework
//
//  Created by Jordan Comino on 06/01/20.
//  Copyright © 2020 Grabba. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@class UIImage;

@interface FingerprintImage : NSObject

-(instancetype) init;
-(instancetype) initWithData:(NSData*)data ofQualityLevel:(int)nfiq;
-(instancetype) initWithImage:(UIImage*)img ofQualityLevel:(int)nfiq;

@property UIImage* Image;
@property int NFIQ;

@end

NS_ASSUME_NONNULL_END
