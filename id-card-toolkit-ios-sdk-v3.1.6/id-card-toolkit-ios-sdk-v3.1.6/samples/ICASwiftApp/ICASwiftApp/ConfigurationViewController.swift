//
//  ConfigurationViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 21/04/22.
//

import UIKit
import Foundation

var toolkit:Toolkit?
var cardreader:CardReader?
var cardPublicData:CardPublicData!
var modifiabledata:ModifiablePublicData!
var nonModifiabledata:NonModifiablePublicData!
var homeaddress:HomeAddress!
var workaddress:WorkAddress!
var registerDevice:RegisterDeviceResponse!
var familybook:CardFamilyBookData!
var headfamily:HeadOfFamily!
var wifedata:Wife!
var childdata:Child!
var response:ToolkitResponse!
var certificates:CardCertificates!
var firstFingerData:FingerData!
var secondFingerData:FingerData!
var selectedFingerData:FingerData!
var signresponse:SignatureResponse!
var dataProtectionKey:DataProtectionKey!
var dataContainer:DataContainer!
var healthDataContainer:HealthDataContainer!
var allergyResource:AllergyResource!
var diagnosisResource:DiagnosisResource!
var bloodGroupResource:BloodGroupResource!
var insuranceResource:InsuranceResource!

class ConfigurationViewController: UIViewController,UITextFieldDelegate {
    
    @IBOutlet weak var statusLabel: UILabel!
    @IBOutlet weak var vgurlText: UITextField!
    @IBOutlet weak var configurlText: UITextField!
    @IBOutlet weak var logDirText: UITextField!
    @IBOutlet var Customswitch: UISwitch!
    @IBOutlet weak var switchLabel: UILabel!
    @IBOutlet weak var configPath: UILabel!
    
    let utils = Utils()
    
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.offlineServicesPath()
        self.Customswitch.isOn = false
        
        vgurlText.delegate = self
        configurlText.delegate = self
        logDirText.delegate = self
        
        statusLabel.isHidden = true
        statusLabel.font = UIFont.boldSystemFont(ofSize: 16.0)
        statusLabel.textColor = UIColor.white
        statusLabel.backgroundColor = UIColor.black
        statusLabel.layer.cornerRadius = 20.0
        statusLabel.layer.borderWidth = 0.1
        statusLabel.layer.masksToBounds = true
        statusLabel.layer.borderColor = UIColor.clear.cgColor
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        NotificationCenter.default.addObserver(self, selector: #selector(self.cleanUpnotification), name: NSNotification.Name(rawValue: "TOOLKITCLEANUP"), object: nil)

        let status = UserDefaults.standard.value(forKey: enum_Configaration.status.rawValue) as? Bool
        
        if status != nil{
            let switcchValue = UserDefaults.standard.value(forKey: enum_Configaration.status.rawValue) as? Bool ?? false
            
            if switcchValue{
                self.Customswitch.isOn = true
                self.switchLabel.text="Online"
               
            }else{
                self.Customswitch.isOn = false
                self.switchLabel.text="Offline"
            }
            
            self.vgurlText.text = UserDefaults.standard.value(forKey: enum_Configaration.VGUrl.rawValue) as? String
            self.configurlText.text = UserDefaults.standard.value(forKey: enum_Configaration.ConfigDirectory.rawValue) as? String
            self.logDirText.text = UserDefaults.standard.value(forKey: enum_Configaration.LogDirectory.rawValue) as? String
        }else {
            
        }
        
    }
    
    func offlineServicesPath()  {
        
        let documentsDir = NSSearchPathForDirectoriesInDomains(.documentDirectory, .userDomainMask, true).first!
        let default_log_paramsPath = "\(documentsDir)/"
        let default_config_paramsPath = "\(documentsDir)"
        
        let dummyUrl = "" //Dummy url and keep internet disconnect for offline
        utils.logDir=default_log_paramsPath
        utils.vgUrl=dummyUrl
        utils.configUrl=default_config_paramsPath
        
        self.configurlText.text=default_config_paramsPath
        self.vgurlText.text=dummyUrl
        self.logDirText.text=default_log_paramsPath
    }
    
    func onlineServicesPath()  {
        
        let documentsDir = NSSearchPathForDirectoriesInDomains(.documentDirectory, .userDomainMask, true).first!
        let default_config_paramsPath = "\(documentsDir)/"
        self.logDirText.text = default_config_paramsPath
        utils.logDir=default_config_paramsPath
    }
    
    @IBAction func customSwitchAction(_ sender: Any) {
        
        if self.Customswitch.isOn == true {
            self.switchLabel.text="Online"
            self.configPath.text="Config url *"
            self.configurlText.text=""
            self.vgurlText.text=""
            
            self.onlineServicesPath()
        }
        else {
            self.switchLabel.text="Offline"
            self.configPath.text="Config directory *"
            
            self.offlineServicesPath()
        }
    }
    
    @IBAction func initlizeButtonAction(_ sender: Any) {
        if self.Customswitch.isOn == true {
            if vgurlText.text == ""{
                self.view.makeToast("Please add VG URL.")
            }else if configurlText.text == ""{
                self.view.makeToast("Please add Configure directory")
            }else{
                saveUTIlS()
            }
        }
        else {
            saveUTIlS()
        }
    }
    
    
    func saveUTIlS() {
        
        utils.showProgressBar(Constants.LoadingData, andView: self.view)
        
        utils.vgUrl=self.vgurlText.text
        utils.configUrl=self.configurlText.text
        
        vgurlText.resignFirstResponder()
        configurlText.resignFirstResponder()
        logDirText.resignFirstResponder()
        
//        print("vgurlText : \(vgurlText.text)")
//        print("vgurlText : \(configurlText.text)")
//        print("vgurlText : \(logDirText.text)")
        
        UserDefaults.standard.setValue(vgurlText.text, forKey: enum_Configaration.VGUrl.rawValue)
        UserDefaults.standard.setValue(configurlText.text, forKey: enum_Configaration.ConfigDirectory.rawValue)
        UserDefaults.standard.setValue(logDirText.text, forKey: enum_Configaration.LogDirectory.rawValue)
        UserDefaults.standard.synchronize()

        //
        var configParams = String()
        
        if self.Customswitch.isOn == true {
            configParams = utils.getOnlineconfigParams() //Online Configuration
            UserDefaults.standard.setValue(true, forKey: enum_Configaration.status.rawValue)
        }
        else {
            configParams = utils.getOfflineconfigParams() //Offline Configuration
            UserDefaults.standard.setValue(false, forKey: enum_Configaration.status.rawValue)
        }
        UserDefaults.standard.setValue(configParams, forKey: enum_Configaration.configureParams.rawValue)
        UserDefaults.standard.synchronize()
        
        self.view.makeToast("Saved details successfully.")
        
        self.utils.dismissProgressBar()
        
        
        DispatchQueue.main.asyncAfter(deadline: .now() + 1.5) {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "MainViewController") as! MainViewController
            self.navigationController?.pushViewController(vc, animated: true)
        }
    }
    
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
    @objc func cleanUpnotification(_ notification: Notification) {
        if (notification.name.rawValue == "TOOLKITCLEANUP") {
            do {
                try toolkit?.cleanup()
                print("Successfully received the notification!")
            }
            catch {
            }
        }
    }
    
    
    func textFieldShouldReturn(_ textField: UITextField) -> Bool {
        vgurlText.isUserInteractionEnabled = true
        vgurlText.resignFirstResponder()
        configurlText.isUserInteractionEnabled = true
        configurlText.resignFirstResponder()
        logDirText.isUserInteractionEnabled = true
        logDirText.resignFirstResponder()
        
        return true
    }
    
}





enum enum_Configaration: String {
    case VGUrl, ConfigDirectory, LogDirectory, status,configureParams
}



extension UIView{
    func cornered_View(_ corners: CACornerMask, radius: CGFloat, borderColor: UIColor, borderWidth: CGFloat) {
        self.layer.maskedCorners = corners
        self.layer.cornerRadius = radius
        self.layer.borderWidth = borderWidth
        self.layer.borderColor = borderColor.cgColor
    }

}
