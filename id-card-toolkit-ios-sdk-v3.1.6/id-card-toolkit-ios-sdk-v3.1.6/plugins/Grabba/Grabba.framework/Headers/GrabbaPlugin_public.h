//
//  Fingerprint_public.h
//  Grabba
//
//  Created by Grabba Developer on 22/5/20.
//  Copyright © 2020 VSS. All rights reserved.
//

#ifndef Fingerprint_public_h
#define Fingerprint_public_h

@protocol IPlugin;

@interface GrabbaPlugin : NSObject

+(instancetype) instance;
-(void) SetPlugin:(id<IPlugin>) plugin;

@end

#endif /* Fingerprint_public_h */
