//
//  VerifyBiometric.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 22/04/22.
//

import UIKit

class VerifyBiometricViewController: UIViewController {
    
    @IBOutlet weak var view_Roundedcorners: UIView!
    private var nfcReader: NFCFetchFingerData?
    var vc = PassportMRZScanViewController()
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        // Do any additional setup after loading the view.
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
    }
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    @IBAction func nfcReadFingerData(_ sender: Any) {
        
//        let reader = NFCReader()
//        reader.delegate = self
        self.nfcReader = NFCFetchFingerData()
        self.nfcReader?.delegate = self
        
        let alert = UIAlertController(title: "Card Version", message: "Select Card Version", preferredStyle: .alert)
        let vthreeCard = UIAlertAction(title: "V3/V4 Card", style: .destructive) { [weak self] action in
            guard let self = self else { return }
            self.nfcReader?.startReading(vc: self, isCardVersion_V2: false)
        }
        let vtwoCard = UIAlertAction(title: "V2 Card", style: .destructive) { [weak self] action in
            guard let self = self else { return }
            // create the alert
            let alert = UIAlertController(title: "Notice", message: "Select the option to get NFC data", preferredStyle: UIAlertController.Style.alert)
            
            alert.addAction(UIAlertAction(title: "MRZ Scanning", style: UIAlertAction.Style.default, handler: { action in
                
                NotificationCenter.default.addObserver(self, selector: #selector(self.methodOfReceivedNotification(notification:)), name: Notification.Name("NotificationIdentifier"), object: nil)

                self.vc = self.storyboard?.instantiateViewController(withIdentifier: "PassportMRZScanViewController") as! PassportMRZScanViewController
                self.vc.ismrzCapturedForVerifyBiometric = true
                self.navigationController?.pushViewController(self.vc, animated: true)
                
            }))
            
//            alert.addAction(UIAlertAction(title: "Manual Entry", style: UIAlertAction.Style.default, handler: { action in
//                let vc = self.storyboard?.instantiateViewController(withIdentifier: "SetNFCdetailsViewController") as! SetNFCdetailsViewController
//                self.navigationController?.pushViewController(vc, animated: true)
//            }))
            
            alert.addAction(UIAlertAction(title: "Cancel", style: UIAlertAction.Style.destructive, handler: nil))
            
            // show the alert
            self.present(alert, animated: true, completion: nil)
        }

        alert.addAction(vthreeCard)
        alert.addAction(vtwoCard)
        alert.addAction(UIAlertAction(title: "Cancel", style: UIAlertAction.Style.cancel, handler: nil))
        self.present(alert, animated: true)
    }
    
    @objc func methodOfReceivedNotification(notification: Notification) {
        
        let dict = notification.userInfo?["dict"] as! NSDictionary
        let customNotification = Notification.Name("NotificationIdentifier")
        NotificationCenter.default.removeObserver(self, name: customNotification, object: nil)
        self.nfcReader?.documentNumber = dict.value(forKey: "document_number") as! String
        self.nfcReader?.dob_DD = dict.value(forKey: "dob_dd") as! String
        self.nfcReader?.dob_MM = dict.value(forKey: "dob_mm") as! String
        self.nfcReader?.dob_YY = dict.value(forKey: "dob_yy") as! String
        self.nfcReader?.expiryDate_DD = dict.value(forKey: "expiryDate_dd") as! String
        self.nfcReader?.expiryDate_MM = dict.value(forKey: "expiryDate_mm") as! String
        self.nfcReader?.expiryDate_YY = dict.value(forKey: "expiryDate_yy") as! String
        self.nfcReader?.startReading(vc: self, isCardVersion_V2: true)
    }
    
    @IBAction func btn_LoadFingerData(_ sender: Any) {
        let vc = storyboard?.instantiateViewController(withIdentifier: "FingerDataViewController") as! FingerDataViewController
        present(vc, animated: true, completion: nil)
    }
    
    @IBAction func btn_VerifyOnServer(_ sender: Any) {
    
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        else if selectedFingerData == nil {
            self.view.makeToast(Constants.fingerDataNotFound)
            return
        }//else if
        
        utils.showProgressBar(Constants.BioAuth, andView: self.view)
        let requestId = Utils.generateSecureKey()
        DispatchQueue.global(qos: .background).async {
        do {
            response = try cardreader?.authenticateBiometricOnServer(requestId!, fingerIndex: selectedFingerData.getFingerIndex(), sensorTimeout: Int32(Constants.SensorTimeOut))
            DispatchQueue.main.async {
            let xmlString = response.getXmlString()
            let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: xmlString)
            if (errorMsg != "") && (errorMsg.count ) > 0 {
                AlertView.showAlertTitle(Constants.Alert, withMessage: errorMsg, onView: self)
            }//if
            else {
                let responseDic = response.getResponseDataElement()
                let matchStr:String = responseDic["MatchStatus"] as! String
                let responseStatusStr:String = responseDic["ResponseStatus"] as! String
                AlertView.showAlertTitle(Constants.Alert, withMessage: "\(matchStr) \(responseStatusStr)", onView: self)
             }//else
            }// update UI main queue
             DispatchQueue.main.async {
                utils.dismissProgressBar()
             }// update UI main queue
            }//do
            catch let error as NSError{
                    DispatchQueue.main.async {
                        var vgresponse = String()
                        if error.userInfo["ErrorResponse"] != nil {
                            vgresponse = error.userInfo["ErrorResponse"] as! String
                        if (vgresponse != "") && ((vgresponse.count ) > 0) {
                            let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: vgresponse)
                            if (errorMsg != "") && ((errorMsg.count ) > 0) {
                                AlertView.showAlertTitle(Constants.Alert, withMessage:errorMsg, onView: self)
                            }//if
                            else {
                                let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                                AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                            }//else
                         }//if
                        else {
                            let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                            AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                            }//else
                       }//if
                       else {
                            let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                            AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                        }//else
                        
                        utils.dismissProgressBar()
                    }// update UI main queue
               }//catch
            }//background queue
    }
}


extension VerifyBiometricViewController: NFCReadFingerData {
    func didCompleteReading(_ output: NSArray) {
        print(output)
        let finger_arr = NSMutableArray()
        for finger in output as! [FingerData] {
            
            let finger_dict = NSMutableDictionary()
            
            finger_dict.setValue(finger, forKey: "finger_Data")
            finger_dict.setValue(finger.getFingerId(), forKey: "finger_id")
            finger_dict.setValue(finger.getFingerIndex().rawValue, forKey: "finger_index")
            finger_dict.setValue("\(self.getNameforFingerIndex(finger.getFingerIndex().rawValue)!)", forKey: "finger_name")
            
            finger_arr.add(finger_dict)
        }
        
        let alertController = UIAlertController(title: "Select Finger", message: "Please select the finger to verify on server.", preferredStyle: .actionSheet)
        
        for finger_Data in finger_arr {
            let dict = finger_Data as! NSDictionary
            let action = UIAlertAction(title: dict.value(forKey: "finger_name") as? String, style: .default) { action
                in
                selectedFingerData = dict.value(forKey: "finger_Data") as? FingerData
            }
            alertController.addAction(action)
        }
        self.present(alertController, animated: true)
    }
    
    func getNameforFingerIndex(_ fingerindex: Int) -> String? {
        
        var fingerName = ""
        
        if fingerindex == 0 {
            fingerName = "None"
        } else if fingerindex == 3 {
            fingerName = "NoMeaning"
        } else if fingerindex == 5 {
            fingerName = "RightThumb"
        } else if fingerindex == 9 {
            fingerName = "RightIndex"
        } else if fingerindex == 13 {
            fingerName = "RightMiddle"
        } else if fingerindex == 17 {
            fingerName = "RightRing"
        } else if fingerindex == 15 {
            fingerName = "RightLittle"
        } else if fingerindex == 6 {
            fingerName = "LeftThumb"
        } else if fingerindex == 10 {
            fingerName = "LeftIndex"
        } else if fingerindex == 14 {
            fingerName = "LeftMiddle"
        } else if fingerindex == 18 {
            fingerName = "LeftRing"
        } else if fingerindex == 22 {
            fingerName = "LeftLittle"
        }

        return fingerName
        
    }
    
    func errorOnReading(_ output: String) {
        print(output)
    }
}
