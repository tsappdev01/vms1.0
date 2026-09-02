//
//  Toolkit.h
//
//  Created by Federal Authority For Identity and Citizenship on 16/11/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CardReader.h"
#import "RegisterDeviceResponse.h"
#import "DataProtectionKey.h"
#import "FaceSDKData.h"
__attribute__((visibility("default")))
@interface Toolkit : NSObject

-(instancetype)initWithToolkit:(BOOL)inProcessMode configParams:(NSString *)configParams;
-(NSString *)getToolkitVersion;
-(NSString *)prepareRequest:(NSString*)requestId;
-(RegisterDeviceResponse *)registerDevice:(NSString *)encodedUserId encodedPassword:(NSString *)encodedPassword deviceReferenceId:(NSString *)deviceReferenceId;
-(DataProtectionKey *)getDataProtectionKey;
-(NSArray * )listReaders;
-(CardReader *)getReaderWithEmiratesId;
-(NSString *)getDeviceId;
-(NSString *)getStatusMessage:(int)status;
-(void)FreeMemory:(void*)buffer;
-(void)cleanup;
-(void)setNfcTag:(id)session tag:(id)tag;
-(void)parseMRZData:(NSString *)mrz;
-(NSString *)getMrzDocument_type;
-(NSString *)getMrzIssued_country;
-(NSString *)getMrzCardnumber;
-(NSString *)getMrzIdnumber;
-(NSString *)getMrzDate_of_birth;
-(NSString *)getMrzGender;
-(NSString *)getMrzCard_expiry_date;
-(NSString *)getMrzNationality;
-(NSString *)getMrzFullname;
-(NSString *)getLicenseExpiryDate;
-(void)getConfigCertificateExpiryDate;
-(NSString *)getConfig_vg_cert_expiry;
-(NSString *)getConfig_lv_cert_expiry;
-(NSString *)getServer_tls_cert_expiry;
-(NSString *)getConfig_ag_cert_expiry;
-(NSString *)getLicense_expiry;
-(ToolkitResponse *)checkCardStatusOffCard:(NSString *)requestId cardNumber:(NSString *)cardNumber idNumber:(NSString *)idn;
-(CardPublicData *
  _Nullable)verifyFaceOnServerUsingPassport:(int)handel passportNumber:(NSString *
_Nonnull)passportNumber passportCountry:(NSString *
_Nonnull)passportCountry passportExpiryDate:(NSString *
_Nonnull)passportExpiryDate dateOfBirth:(NSString *
_Nonnull)dateOfBirth faceimage:(NSString * _Nonnull)base64Image
isdigitalDocs:(BOOL)isdigitalDocs;
-(CardPublicData *_Nonnull)verifyFaceOnServerUsingID:(int)handel idNumber:(NSString *_Nonnull)idn faceImage:(NSString *_Nullable)base64Image digitalDocs:(BOOL)isdigitalDocs;

-(NSString *_Nullable)getFaceSDkLicense:(NSString *_Nonnull)applicationId;
-(FaceSDKData *)inItFaceSDK:(NSString*_Nonnull)applicationId;
@end
