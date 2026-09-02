//
//  XMLParserHelper.h
//  ICASwiftApp
//
//  ObjC wrapper for SHXMLParser -- exposes XML parsing to Swift without
//  requiring SHXMLParser to be directly visible in the Swift module.
//

#import <Foundation/Foundation.h>

@interface XMLParserHelper : NSObject

+ (NSDictionary * _Nullable)parseXMLString:(NSString * _Nonnull)xmlString;
+ (id _Nullable)getDataAtPath:(NSString * _Nonnull)path
             fromResultObject:(NSDictionary * _Nullable)resultObject;
+ (unsigned char * _Nullable)convert:(NSString * _Nonnull)str;
+ (NSString * _Nullable)encodedData:(void * _Nonnull)data;
+ (NSString * _Nullable)base64forData:(NSData * _Nonnull)theData;
+ (unsigned int)base64Decode:(const unsigned char * _Nonnull)encodedData
                      length:(unsigned int)encodedDataLength
                 decodedData:(unsigned char * _Nullable * _Nonnull)decodedData
               decodedLength:(unsigned int * _Nonnull)decodedDataLength;

@end
