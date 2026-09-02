//
//  XMLParserHelper.m
//  ICASwiftApp
//

#import "XMLParserHelper.h"
#import <IDCardToolkit/SHXMLParser.h>
#import <IDCardToolkit/Helpers.h>

@implementation XMLParserHelper

+ (NSDictionary *)parseXMLString:(NSString *)xmlString {
    NSData *data = [xmlString dataUsingEncoding:NSUTF8StringEncoding];
    if (!data) return nil;
    SHXMLParser *parser = [[SHXMLParser alloc] init];
    return [parser parseData:data];
}

+ (id)getDataAtPath:(NSString *)path
   fromResultObject:(NSDictionary *)resultObject {
    return [SHXMLParser getDataAtPath:path fromResultObject:resultObject];
}

+ (unsigned char *)convert:(NSString *)str {
    return [SHXMLParser convertString:str];
}

+ (NSString *)encodedData:(void *)data {
    return [SHXMLParser getencodedData:data];
}

+ (NSString *)base64forData:(NSData *)theData {
    return [SHXMLParser base64forData:theData];
}

+ (unsigned int)base64Decode:(const unsigned char *)encodedData
                      length:(unsigned int)encodedDataLength
                 decodedData:(unsigned char **)decodedData
               decodedLength:(unsigned int *)decodedDataLength {
    return Base64Decode(encodedData, encodedDataLength,
                        decodedData, decodedDataLength);
}

@end
