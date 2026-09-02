//
//  AppDelegate.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 21/04/22.
//

import UIKit

@main
class AppDelegate: UIResponder, UIApplicationDelegate {

    func application(_ application: UIApplication, didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]?) -> Bool {
        // Override point for customization after application launch.
        
        self.copyFilesToDocumentDirectory("config_li", subFileName:"config_li")
        self.copyFilesToDocumentDirectory("config_pg", subFileName:"config_pg")
        
        self.copyFilesToDocumentDirectory("config_lv_prod", subFileName:"config_lv_prod")
        self.copyFilesToDocumentDirectory("config_tk_prod", subFileName:"config_tk_prod")
        self.copyFilesToDocumentDirectory("config_vg_prod", subFileName:"config_vg_prod")
    
//        self.copyFilesToDocumentDirectory("config_lv_qa", subFileName:"config_lv_qa")
//        self.copyFilesToDocumentDirectory("config_tk_qa", subFileName:"config_tk_qa")
//        self.copyFilesToDocumentDirectory("config_vg_qa", subFileName:"config_vg_qa")
        
        
//        self.copyFilesToDocumentDirectory("config_ap", subFileName:"config_ap")
        
        return true
    }

    // MARK: UISceneSession Lifecycle

    func application(_ application: UIApplication, configurationForConnecting connectingSceneSession: UISceneSession, options: UIScene.ConnectionOptions) -> UISceneConfiguration {
        // Called when a new scene session is being created.
        // Use this method to select a configuration to create the new scene with.
        return UISceneConfiguration(name: "Default Configuration", sessionRole: connectingSceneSession.role)
    }

    func application(_ application: UIApplication, didDiscardSceneSessions sceneSessions: Set<UISceneSession>) {
        // Called when the user discards a scene session.
        // If any sessions were discarded while the application was not running, this will be called shortly after application:didFinishLaunchingWithOptions.
        // Use this method to release any resources that were specific to the discarded scenes, as they will not return.
    }
    
    func copyFilesToDocumentDirectory(_ fileName:String, subFileName:String) {
        
        let strPath = Bundle.main.path(forResource: fileName, ofType: "")
        let documentsDir = NSSearchPathForDirectoriesInDomains(.documentDirectory, .userDomainMask, true).first!
        let fileManager = FileManager.default
        let fullDestPath = NSURL(fileURLWithPath: documentsDir).appendingPathComponent(subFileName)
        let fullDestPathString = fullDestPath?.path
        
        let success=fileManager .fileExists(atPath: fullDestPathString!)
        if !success {
            
            print("\(fileName) created")
            
            do{
                try fileManager.copyItem(atPath: strPath!, toPath: fullDestPathString!)
            }catch{
                print("\(fileName) not created  \(error)")
            }
        }
    }
}

