//
//  SHXMLParser.h
//  SHXMLParser
//

#import <Foundation/Foundation.h>

__attribute__((visibility("default")))
@interface SHXMLParser : NSObject <NSXMLParserDelegate>

+ (NSArray *)convertDictionaryArray:(NSArray *)dictionaryArray toObjectArrayWithClassName:(NSString *)className classVariables:(NSArray *)classVariables;
+ (id)getDataAtPath:(NSString *)path fromResultObject:(NSDictionary *)resultObject;
+ (NSString*)base64forData:(NSData*)theData ;
- (NSDictionary *)parseData:(NSData *)XMLData;

@end
