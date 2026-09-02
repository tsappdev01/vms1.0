//
//  CardReader.h
//
//  Created by Federal Authority For Identity and Citizenship on 16/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CardPublicData.h"
#import "FingerData.h"
#import "FingerIndex.h"
#import "CardCertificates.h"
#import "ToolkitResponse.h"
#import "SignatureResponse.h"
#import "CardFamilyBookData.h"
#import "RegisterDeviceResponse.h"
#import "DataContainer.h"
#import "EIDAToolkitTypes.h"

//#import "ToolkitTypes.h"

@interface CardReader : NSObject

-(id)initWithCardReader:(NSString *)name;
-(NSString *)getName;
-(BOOL)isConnected;
-(void)connect;
-(void)disconnect;
-(int)getInterfaceType;
-(int)getCardversion;
-(void)setNfcAuthenticationParameters:(NSString *)mrzdata;
-(void)setNfcAuthenticationParameters:(NSString *)cardNumber dateOfBirth:(NSString *)dateOfBirth expiryDate:(NSString *)expiryDate;
-(ToolkitResponse *)checkCardStatus:(NSString *)requestId;
-(CardPublicData *)readPublicData:(NSString *)requestId readnonModifiableData:(BOOL)readnonModifiableData readModifiableData:(BOOL)readModifiableData readPhotography:(BOOL)readPhotography readSignatueImage:(BOOL)readSignatueImage readAddress:(BOOL)readAddress;
-(NSString *)prepareRequest:(NSString *)requestId;
-(CardCertificates *)getPkiCertificates:(NSString *)encodedPin;
-(ToolkitResponse *)authenticatePki:(NSString *)encodedPin;
-(SignatureResponse *)signData:(NSString *)input inputLength:(int)inputLength isInputHash:(BOOL)isInputHash encodedPin:(NSString *)encodedPin;
-(SignatureResponse *)signChallenge:(NSString *)input inputLength:(int)inputLength isInputHash:(BOOL)isInputHash encodedPin:(NSString *)encodedPin;
-(NSArray *)getFingerData;
-(ToolkitResponse *)authenticateBiometricOnServer:(NSString *)requestId fingerIndex:(enum FingerIndex)fingerIndex sensorTimeout:(int)sensorTimeout;
-(ToolkitResponse *)resetPin:(NSString *)encodedPin fingerData:(FingerData *) fingerData sensorTimeout:(int)sensorTimeout;
-(ToolkitResponse *)unblockPin:(NSString *)encodedPin fingerData:(FingerData *)fingerData sensorTimeout:(int)sensorTimeout;
-(CardFamilyBookData *)readFamilyBookData:(NSString *)encodedPin;
-(void)verifySignature:(NSString *)input inputLength:(int)inputLength isInputHash:(BOOL)isInputHash signature:(uint8_t *)signature signatureLength:(int)signatureLength certificate:(uint8_t *)certificate certificateLength:(int)certificateLength;
-(ToolkitResponse *)authenticateCardAndBiometric:(NSString *)requestId fingerIndex:(enum FingerIndex)fingerIndex sensorTimeout:(int)sensorTimeout;
-(void)readPublicDataEF:(PUBLIC_DATA_EF_TYPE)publicDataEFType validateSignature:(BOOL)validateSignature;
-(uint8_t *)getReadPublicDataEFByte;
-(int)getReadPublicDataEFLength;
-(NSString *)getCSN;
-(NSString *)parseEFData:(uint8_t *)efData efDatalength:(int)efDatalength;
-(ToolkitResponse *)updateData:(DF_TYPE)fileType requestId:(NSString *)requestId;
-(DataContainer *)readData:(FILE_TYPE)fileType requestId:(NSString *)requestId;
-(ToolkitResponse *)resetPinWithoutAuthenticateBiometric:(NSString *)encodedPin;
@end

