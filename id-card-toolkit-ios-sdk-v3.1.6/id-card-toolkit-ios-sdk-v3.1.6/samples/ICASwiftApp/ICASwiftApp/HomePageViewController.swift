//
//  HomePageViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 21/04/22.
//

import UIKit

class HomePageViewController: UIViewController {
    
    var img_List = ["Initialize",
                    "Registerdevice",
                    "Get Reader",
                    "ConnectReader",
                    "DisconnectReader",
                    "CardSerialNumber",
                    "ReadePublicData",
                    "CheckCardStatus",
                    "CheckCardStatusOffCard",
                    "VerifyBioMetric",
                    "VerifyCardAndBiometric",
                    "PinAuthantication",
                    "UnblockPin-1",
                    "pinReset",
                    "REadCErtifcate",
                    "SignData",
                    "ReadFamilyBook",
//                    "Face Initialize",
//                    "FaceAuthantication",
                    "FaceVerification",
                    "DeviceID",
                    "Card Interface type-1",
                    "Configure Certificate expiry date",
                    "License expirydate",
                    "Reader UAE ID",
                    "Tool Kit version",
                    "CleanUP"]
//    var img_List = ["Initialize",
//                    "Registerdevice",
//                    "Get Reader",
//                    "ConnectReader",
//                    "DisconnectReader",
//                    "CardSerialNumber",
//                    "ReadePublicData",
//                    "CheckCardStatus",
//                    "VerifyBioMetric",
//                    "VerifyCardAndBiometric",
//                    "PinAuthantication",
//                    "UnblockPin-1",
//                    "pinReset",
//                    "REadCErtifcate",
//                    "SignData",
//                    "ReadFamilyBook",
//                    "DeviceID",
//                    "Card Interface type-1",
//                    "Configure Certificate expiry date",
//                    "License expirydate",
//                    "Reader UAE ID",
//                    "Tool Kit version",
//                    "CleanUP"]
    /*
     "FaceAuthantication",
     "FaceVerification",
     "CheckCardStatusOffCard",
     */
        
    @IBOutlet weak var cv_homepage: UICollectionView!
    @IBOutlet weak var view_Roundedcorners: UIView!
  
    
    private let sectionInsets = UIEdgeInsets(top: 0, left: 0, bottom: 0, right: 0)

    override func viewDidLoad() {
        super.viewDidLoad()

        // Do any additional setup after loading the view.
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)

    }
    
    @IBAction func btn_BackAction(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "ConfigurationViewController") as! ConfigurationViewController
        self.navigationController?.pushViewController(vc, animated: true)
    }
}

extension HomePageViewController : UICollectionViewDataSource,UICollectionViewDelegate{
    func collectionView(_ collectionView: UICollectionView, numberOfItemsInSection section: Int) -> Int {
        return img_List.count
    }
    
    func collectionView(_ collectionView: UICollectionView, cellForItemAt indexPath: IndexPath) -> UICollectionViewCell {
        let cell = collectionView.dequeueReusableCell(withReuseIdentifier: "HomePageCollectionview", for: indexPath) as! HomePageCollectionview
        cell.img_list.image = UIImage(named: img_List[indexPath.row])
        return cell
        
    }
    
    func collectionView(_ collectionView: UICollectionView, didSelectItemAt indexPath: IndexPath) {
        
        if indexPath.row == 0 {
            self.initializeToolkit()
        }
        
        else if indexPath.row == 1 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "RegisterDeviceViewController") as! RegisterDeviceViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 2 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "ListReadersViewController") as! ListReadersViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 3 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "ConnectReaderViewController") as! ConnectReaderViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 4 {
            self.disconnectReader()
        }
        else if indexPath.row == 5 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "CardSerialNumberViewController") as! CardSerialNumberViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 6 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "PublicDataListViewController") as! PublicDataListViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 7 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "CheckCardStatusViewController") as! CheckCardStatusViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 8 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "CheckCardStatusOffCardViewController") as! CheckCardStatusOffCardViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 9 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "VerifyBiometricViewController") as! VerifyBiometricViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 10 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "VerifyCardAndBiometricViewController") as! VerifyCardAndBiometricViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 11 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "PINAuthenticationViewController") as! PINAuthenticationViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 12 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "UnblockPINViewController") as! UnblockPINViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 13 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "PinResetViewController") as! PinResetViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 14 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "PkiCertificatesViewController") as! PkiCertificatesViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 15 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "SignDataViewController") as! SignDataViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 16 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "FamilyBookViewController") as! FamilyBookViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
//        else if indexPath.row == 17 {
//            if !DotSdk.shared.isInitialized {
//                DispatchQueue.global().async {
//                    self.initializeDotSdk()
//                }
//            }
//        }
//        else if indexPath.row == 18 {
//            let vc = self.storyboard?.instantiateViewController(withIdentifier: "FaceAuthenticationViewController") as! FaceAuthenticationViewController
//            self.navigationController?.pushViewController(vc, animated: true)
//        }
        else if indexPath.row == 17 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "FaceVerificationViewController") as! FaceVerificationViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 18 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "DeviceIDViewController") as! DeviceIDViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 19 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "InterfaceTypeViewController") as! InterfaceTypeViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }else if indexPath.row == 20 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "ConfigCertExpiryDateViewController") as! ConfigCertExpiryDateViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }else if indexPath.row == 21 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "LicenseExpiryViewController") as! LicenseExpiryViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
        else if indexPath.row == 22 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "ReaderEmiratesIDViewController") as! ReaderEmiratesIDViewController
            self.navigationController?.pushViewController(vc, animated: true)
            
        }else if indexPath.row == 23 {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "ToolkitVersionViewController") as! ToolkitVersionViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }else {
            self.cleanUpToolkit()
        }
    }
    
    
    func initializeToolkit(){
        checkConfigPaths()
        
        utils.showProgressBar(Constants.DeviceConnect, andView: self.view)
        DispatchQueue.global(qos: .background).async {
            var configParams = UserDefaults.standard.value(forKey: enum_Configaration.configureParams.rawValue) as? String
            print(configParams)
            do {
                toolkit = try Toolkit(true, configParams : configParams ?? "")
                dataProtectionKey =  try toolkit?.getDataProtectionKey()
                let toolkitFace_init = ToolkitFace.shared.initilizeFaceSDK()
                DispatchQueue.main.async {
                    self.view.makeToast("Initialized successfully.")
                    utils.dismissProgressBar()
                    if(toolkitFace_init != .SUCCESS) {
                        self.view.makeToast("Toolkit Face SDK not initialized.")
                    }
                }// update UI main queue
            }//do
            catch let error as NSError{
                DispatchQueue.main.async {
                    let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                    AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                    utils.dismissProgressBar()
                }// update UI main queue
            }//catch
        }//background queue
    }
    
    func faceInitlization() {
        
    }
    
    func checkConfigPaths()  {
        let documentsDir = NSSearchPathForDirectoriesInDomains(.documentDirectory, .userDomainMask, true).first!
        let default_log_paramsPath = "\(documentsDir)/"
        let default_config_paramsPath = "\(documentsDir)"

        print("default_log_paramsPath \(default_log_paramsPath)")
        let configDir = UserDefaults.standard.value(forKey: enum_Configaration.ConfigDirectory.rawValue) as? String
        let vgUrl = UserDefaults.standard.value(forKey: enum_Configaration.VGUrl.rawValue) as? String
        let logDir = UserDefaults.standard.value(forKey: enum_Configaration.LogDirectory.rawValue) as? String
        
        print("configDir \(configDir)")
        print("vgUrl \(vgUrl)")
        print("logDir \(logDir)")

        if let configdirectory =  configDir {
            if configdirectory.contains("Application") ||  configdirectory.contains("Documents")  {
                
                utils.logDir=default_log_paramsPath
                utils.configUrl=default_config_paramsPath
                utils.vgUrl=vgUrl
            }
            else {
                utils.logDir=default_log_paramsPath
                utils.configUrl=configDir
                utils.vgUrl=vgUrl
            }
        }
        let status = UserDefaults.standard.value(forKey: enum_Configaration.status.rawValue) as? Bool
        if status != nil{
            let switcchValue = UserDefaults.standard.value(forKey: enum_Configaration.status.rawValue) as? Bool ?? false
            var configParams = String()
            if switcchValue{
                configParams = utils.getOnlineconfigParams() //Online Configuration
               
            }else{
                configParams = utils.getOfflineconfigParams() //Offline Configuration
            }
            print("getconfigParams \(configParams)")
          
            UserDefaults.standard.setValue(configParams, forKey: enum_Configaration.configureParams.rawValue)
            UserDefaults.standard.synchronize()
            
        }
    }
    
    func disconnectReader() {
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        utils.showProgressBar(Constants.DisConnectReader, andView: self.view)
        DispatchQueue.global(qos: .background).async {
            do {
                try cardreader?.disconnect()
                DispatchQueue.main.async {
                    self.view.makeToast(Constants.DeviceDisConnected)
                    utils.dismissProgressBar()
                }// update UI main queue
            }//do
            catch let error as NSError{
                DispatchQueue.main.async {
                    let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                    AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                    utils.dismissProgressBar()
                }// update UI main queue
            }//catch
        }//background queue
    }
    
    func cleanUpToolkit()  {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
       
        utils.showProgressBar(Constants.DeviceConnect, andView: self.view)

        DispatchQueue.global(qos: .background).async {
            do {
                try toolkit?.cleanup()
                DispatchQueue.main.async {
                    self.view.makeToast(Constants.CleanUp)
                    utils.dismissProgressBar()
                }// update UI main queue
            }//do
            catch let error as NSError{
                DispatchQueue.main.async {
                    utils.dismissProgressBar()
                    let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                    AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                }
            }//catch
        }//background queue
    }
}


extension HomePageViewController: UICollectionViewDelegateFlowLayout {



  // 1
  func collectionView(
    _ collectionView: UICollectionView,
    layout collectionViewLayout: UICollectionViewLayout,
    sizeForItemAt indexPath: IndexPath
  ) -> CGSize {
    // 2
      return CGSize(width: (cv_homepage.bounds.size.width/3)-15, height: 140)
  }

  // 3
  func collectionView(
    _ collectionView: UICollectionView,
    layout collectionViewLayout: UICollectionViewLayout,
    insetForSectionAt section: Int
  ) -> UIEdgeInsets {
    return sectionInsets
  }

  // 4
  func collectionView(
    _ collectionView: UICollectionView,
    layout collectionViewLayout: UICollectionViewLayout,
    minimumLineSpacingForSectionAt section: Int
  ) -> CGFloat {
    return sectionInsets.left
  }
}
