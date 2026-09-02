//
//  ToolkitResponse.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 16/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

//#import <UIKit/UIKit.h>
#import "ToolkitXmlDataObject.h"
#import "ResponseStatus.h"

@interface ToolkitResponse : ToolkitXmlDataObject
-(id)init;
-(id)initWithToolkitResponseByte:(uint8_t *)detachedSignature detachedSignatureLength:(int)detachedSignatureLength;
-(id)initWithToolkitResponse:(NSString *)xmlString;
-(id)initWithToolkitResponse:(NSString *)xmlString detachedSignature:(uint8_t *)detachedSignature detachedSignatureLength:(int)detachedSignatureLength;
-(NSString *)getXmlString;
-(ResponseStatus *)getResponseStatus;
-(NSDictionary *)getResponseDataElement;
-(NSString *)getStatus;
-(NSString *)getService;
-(NSString *)getAction;
-(NSString *)getRequestId;
-(NSString *)getNonce;
-(NSString *)getCorrelationId;
-(NSString *)getCardSerialNumber;
-(NSString *)getCardNumber;
-(NSString *)getIdNumber;
-(NSString *)getTimeStamp;
-(NSString *)getValidityInterval;
-(uint8_t *)getDetachedSignature;
-(int)getDetachedSignatureLength;
@end
