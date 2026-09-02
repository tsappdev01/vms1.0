//
//  AdesHelpers.h
//  IDCardToolkit -- ObjC bridge for AdES signing/verification.
//
//  Swift cannot cleanly populate C fixed-size char arrays inside
//  SIGNING_CONTEXT / PADES_SIGN_PARAMS / VERIFICATION_CONTEXT, so this
//  helper does the strncpy plumbing and calls the C API directly.
//  See EIDAToolkitApi.h for the underlying functions.
//

#ifndef ADES_HELPERS_H
#define ADES_HELPERS_H

#import <Foundation/Foundation.h>
#import "EIDAToolkitTypes.h"

__attribute__((visibility("default")))
@interface AdesHelpers : NSObject

/// Sign a file with CAdES.
/// On success, @p outSignature points to a malloc'd buffer of length @p outLen.
/// The caller must invoke FreeMemory() on @p outSignature once read.
- (unsigned int)cadesSignInput:(NSString *)inputPath
                    signedPath:(NSString *)signedPath
                    encodedPin:(NSString *)encodedPin
                signatureLevel:(int)signatureLevel
                 packagingMode:(int)packagingMode
                        tsaUrl:(NSString *)tsaUrl
                       ocspUrl:(NSString *)ocspUrl
                      certPath:(NSString *)certPath
                   countryCode:(NSString *)countryCode
               stateOrProvince:(NSString *)stateOrProvince
                    postalCode:(NSString *)postalCode
                      locality:(NSString *)locality
                        street:(NSString *)street
                  outSignature:(unsigned char **)outSignature
                        outLen:(unsigned int *)outLen
                  attemptsLeft:(int *)attemptsLeft;

/// Sign an XML file with XAdES.
- (unsigned int)xadesSignInput:(NSString *)inputPath
                    signedPath:(NSString *)signedPath
                    encodedPin:(NSString *)encodedPin
                signatureLevel:(int)signatureLevel
                 packagingMode:(int)packagingMode
                        tsaUrl:(NSString *)tsaUrl
                       ocspUrl:(NSString *)ocspUrl
                      certPath:(NSString *)certPath
                   countryCode:(NSString *)countryCode
               stateOrProvince:(NSString *)stateOrProvince
                    postalCode:(NSString *)postalCode
                      locality:(NSString *)locality
                        street:(NSString *)street
                  outSignature:(unsigned char **)outSignature
                        outLen:(unsigned int *)outLen
                  attemptsLeft:(int *)attemptsLeft;

/// Sign a PDF file with PAdES.  Writes the signed PDF to @p signedPath.
- (unsigned int)padesSignInput:(NSString *)inputPath
                    signedPath:(NSString *)signedPath
                    encodedPin:(NSString *)encodedPin
                signatureLevel:(int)signatureLevel
                        tsaUrl:(NSString *)tsaUrl
                       ocspUrl:(NSString *)ocspUrl
                      certPath:(NSString *)certPath
                    signReason:(NSString *)signReason
                signerLocation:(NSString *)signerLocation
             signerContactInfo:(NSString *)signerContactInfo
            signatureImagePath:(NSString *)signatureImagePath
                 signatureText:(NSString *)signatureText
                      fontName:(NSString *)fontName
                     fontColor:(NSString *)fontColor
               backgroundColor:(NSString *)backgroundColor
                 signatureXAxis:(int)signatureXAxis
                 signatureYAxis:(int)signatureYAxis
                       fontSize:(int)fontSize
                     pageNumber:(int)pageNumber
                    signVisible:(int)signVisible
                   namePosition:(int)namePosition
                   attemptsLeft:(int *)attemptsLeft;

/// CAdES verify -- signature bytes separate from input document (detached mode).
- (unsigned int)cadesVerifyInput:(NSString *)inputPath
                         ocspUrl:(NSString *)ocspUrl
                        certPath:(NSString *)certPath
                      reportType:(int)reportType
                      isDetached:(int)isDetached
                       signature:(unsigned char *)signature
                    signatureLen:(unsigned int)signatureLen
                        outReport:(char **)outReport
                     outReportLen:(unsigned int *)outReportLen;

/// XAdES verify.  When isDetached, signature bytes are read from @p signature.
- (unsigned int)xadesVerifyInput:(NSString *)inputPath
                         ocspUrl:(NSString *)ocspUrl
                        certPath:(NSString *)certPath
                      reportType:(int)reportType
                      isDetached:(int)isDetached
                       signature:(unsigned char *)signature
                    signatureLen:(unsigned int)signatureLen
                        outReport:(char **)outReport
                     outReportLen:(unsigned int *)outReportLen;

/// PAdES verify -- signature is always embedded in the PDF.
- (unsigned int)padesVerifyInput:(NSString *)inputPath
                         ocspUrl:(NSString *)ocspUrl
                        certPath:(NSString *)certPath
                      reportType:(int)reportType
                        outReport:(char **)outReport
                     outReportLen:(unsigned int *)outReportLen;

@end

#endif /* ADES_HELPERS_H */
