//
//  AppDelegate.m
//  
//
//  Created by Federal Authority For Identity and Citizenship  on 12/28/16.
//  Copyright © 2016 Federal Authority For Identity and Citizenship. All rights reserved.
//

#import "AppDelegate.h"

@interface AppDelegate ()
@end

@implementation AppDelegate

#pragma mark AppDelegate Methods

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    
//    NSError *error;
//    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
//    NSString *documentsDirectory = [paths objectAtIndex:0]; // Get documents folder
//    NSString *dataPath = [documentsDirectory stringByAppendingPathComponent:@"/config"];
//    NSLog(@"dataPath %@",dataPath);
//    if (![[NSFileManager defaultManager] fileExistsAtPath:dataPath])
//        [[NSFileManager defaultManager] createDirectoryAtPath:dataPath withIntermediateDirectories:NO attributes:nil error:&error]; //Create folder
    
    [self copyFilesToDocumentDirectory:@"config_li" subFileName:@"config_li"];
    [self copyFilesToDocumentDirectory:@"config_lv_prod" subFileName:@"config_lv_prod"];
    [self copyFilesToDocumentDirectory:@"config_pg" subFileName:@"config_pg"];
    [self copyFilesToDocumentDirectory:@"config_tk_prod" subFileName:@"config_tk_prod"];
    [self copyFilesToDocumentDirectory:@"config_vg_prod" subFileName:@"config_vg_prod"];
    //[self copyFilesToDocumentDirectory:@"config_ap" subFileName:@"config_ap"];
//    [self copyFilesToDocumentDirectory:@"EicHealthContainerSchema.json" subFileName:@"config/EicHealthContainerSchema.json"];

    self.model = [[Model alloc]init];
    self.utils=[[Utils alloc]init];
   
    // Override point for customization after application launch.
    return YES;
}
- (void)applicationWillResignActive:(UIApplication *)application {
    // Sent when the application is about to move from active to inactive state. This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) or when the user quits the application and it begins the transition to the background state.
    // Use this method to pause ongoing tasks, disable timers, and invalidate graphics rendering callbacks. Games should use this method to pause the game.
}
- (void)applicationDidEnterBackground:(UIApplication *)application {
    // Use this method to release shared resources, save user data, invalidate timers, and store enough application state information to restore your application to its current state in case it is terminated later.
    // If your application supports background execution, this method is called instead of applicationWillTerminate: when the user quits.
}
- (void)applicationWillEnterForeground:(UIApplication *)application {
    // Called as part of the transition from the background to the active state; here you can undo many of the changes made on entering the background.
}
- (void)applicationDidBecomeActive:(UIApplication *)application {
    // Restart any tasks that were paused (or not yet started) while the application was inactive. If the application was previously in the background, optionally refresh the user interface.
}
- (void)applicationWillTerminate:(UIApplication *)application {
    
    [[NSNotificationCenter defaultCenter]postNotificationName:@"TOOLKITCLEANUP"
     object:self];
    
    // Called when the application is about to terminate. Save data if appropriate. See also applicationDidEnterBackground:.
}

-(void)copyFilesToDocumentDirectory:(NSString *)fileName subFileName:(NSString *)subFileName {
    
    NSFileManager *fileManager = [NSFileManager defaultManager];
    NSError *error;
    NSString *strPath = [self getDocumentDirectoryPath:subFileName];
    
    BOOL success = [fileManager fileExistsAtPath:strPath];
    
    if(!success) {
        NSString *defaultPath = [[[NSBundle mainBundle] resourcePath] stringByAppendingPathComponent:fileName];
        NSLog(@"bundle Path %@",defaultPath);
        success = [fileManager copyItemAtPath:defaultPath toPath:strPath error:&error];
        
        if (!success)
            NSLog(@"%@ not created '%@'.",fileName, [error localizedDescription]);
        else
            NSLog(@"%@ created",fileName);
    }
}
-(NSString *)getDocumentDirectoryPath:(NSString *)stringPath {
    NSArray *arrPaths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory , NSUserDomainMask, YES);
    NSString *documentsDir = [arrPaths objectAtIndex:0];
    NSString *strValue = [NSString stringWithFormat:@"%@",[documentsDir stringByAppendingPathComponent:stringPath]];
    return strValue;
}
@end

