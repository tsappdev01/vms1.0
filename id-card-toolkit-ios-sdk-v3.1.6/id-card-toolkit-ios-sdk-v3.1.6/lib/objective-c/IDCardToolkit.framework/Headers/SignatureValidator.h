//
//  SignatureValidator.h
//  IDCardToolkit
//
//  Created by Federal Authority For Identity and Citizenship on 16/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//
#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface SignatureValidator : NSObject

-(instancetype)initWithSignatureValidator:(uint8_t *)certificateData certificateDataLength:(int)certificateDataLength certificateChain:(uint8_t *)certificateChain certificateChainLength:(int)certificateChainLength;
-(void)ValidateToolkitResponse:(NSString *)toolkitResponse;
-(NSString *)getService;
-(NSString *)getAction;
-(NSString *)getRequest_id;
-(NSString *)getNonce;
-(NSString *)getCorrelation_id;
-(NSString *)getCsn;
-(NSString *)getCardnumber;
-(NSString *)getIdnumber;
-(NSString *)getTime_stamp;
-(int)getValidity_interval;
@end

NS_ASSUME_NONNULL_END
