//
//  Utils.h
//  EIDAToolkitSwiftTestapp
//
//  Created by Federal Authority For Identity and Citizenship  on 1/29/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "MBProgressHUD.h"

// Third party library header files
#include <openssl/rand.h>
#include <openssl/rsa.h>
#include <openssl/x509.h>
#include <openssl/pem.h>
#define XMLSEC_CRYPTO_OPENSSL
#define XMLSEC_STATIC
#define XMLSEC_NO_XSLT

#include <xmlsec/xmlsec.h>
#include <xmlsec/xmltree.h>
#include <xmlsec/xmldsig.h>
#include <xmlsec/templates.h>
#include <xmlsec/crypto.h>

@interface Utils : NSObject {
    MBProgressHUD *HUDprogressView;
}
+(NSString *)getOnlineconfigParams;
+(NSString *)getOfflineconfigParams;
+(NSString *)setEncrytion:(NSString *)requestHandle data:(NSString *)data publickey:(unsigned char *)publickey keylength:(int)keylength;
+(UIColor *)colorFromHexString:(NSString *)hexString;
+(NSString*)base64forData:(NSData*)theData;
+(NSString*)generateSecureKey;
-(NSMutableArray *)getContainerDataElements:(NSString *)typeOfData;
-(void)ShowProgressBar:(NSString *)Showtext andView:(UIView *)View;
-(void)DismissProgressBar;
-(NSString *)validateToolkitResponse:(NSString *)requestId xmlstring:(NSString *)xmlstring;

@end



