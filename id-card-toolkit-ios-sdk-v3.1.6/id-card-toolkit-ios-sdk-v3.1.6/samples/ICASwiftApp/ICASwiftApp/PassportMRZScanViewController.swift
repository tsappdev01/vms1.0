//
//  PassportMRZScanViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 11/05/22.
//

import UIKit

var scanResults : QKMRZScanResult! = nil

class PassportMRZScanViewController: UIViewController,QKMRZScannerViewDelegate {
    
    
    @IBOutlet weak var mrzScannerView: QKMRZScannerView!
    @IBOutlet weak var backButton: UIButton!
    
    var isSetNFCdetailsViewController = false
    var isCapturedDetailsForFace = false
    var isMrzIDCaptured = false
    var ismrzCapturedForOffCardStatus = false
    var ismrzCapturedForVerifyBiometric = false
    
    override func viewDidLoad() {
        super.viewDidLoad()

        // Do any additional setup after loading the view.
        
        mrzScannerView.delegate = self
        self.backButton.frame = CGRect(x: self.view.frame.size.width-64, y: 20, width: 64, height: 64)
        mrzScannerView.frame = CGRect(x: 0, y: 0, width: self.view.frame.size.width, height: self.view.frame.size.height)
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(animated)
        mrzScannerView.startScanning()
    }
    
    override func viewWillDisappear(_ animated: Bool) {
        super.viewWillDisappear(animated)
        mrzScannerView.stopScanning()
    }
    
    func mrzScannerView(_ mrzScannerView: QKMRZScannerView, didFind scanResult: QKMRZScanResult) {
    
        let Birthformatter = DateFormatter()
        Birthformatter.dateFormat = "yy-MM-dd"
        let dateStr: String
        if(scanResult.birthdate != nil){
            dateStr = Birthformatter.string(from: scanResult.birthdate!)
        } else {
            dateStr = "yy-MM-dd"
        }
        let birth_Components = dateStr.components(separatedBy: "-")
        print("Birth Year : \(birth_Components[0])")
        print("Birth Month : \(birth_Components[1])")
        print("Birth Day : \(birth_Components[2])")
        
        let expirtyformatter = DateFormatter()
        expirtyformatter.dateFormat = "yy-MM-dd"
        let date_expiry: String
        if(scanResult.expiryDate != nil){
            date_expiry = expirtyformatter.string(from: scanResult.expiryDate!)
        } else {
            date_expiry = "yy-MM-dd"
        }
        let expirty_Components = date_expiry.components(separatedBy: "-")
        print("Expiry Year : \(expirty_Components[0])")
        print("Expiry Month : \(expirty_Components[1])")
        print("Expiry Day : \(expirty_Components[2])")

        if isSetNFCdetailsViewController == true{
            let vc = storyboard?.instantiateViewController(withIdentifier: "SetNFCdetailsViewController") as! SetNFCdetailsViewController
            vc.documentNumber = scanResult.documentNumber
            vc.dob_YY = birth_Components[0]
            vc.dob_MM = birth_Components[1]
            vc.dob_DD = birth_Components[2]
            vc.expiryDate_YY = expirty_Components[0]
            vc.expiryDate_MM = expirty_Components[1]
            vc.expiryDate_DD = expirty_Components[2]
            
            vc.isMrzSelected = true
            navigationController?.pushViewController(vc, animated: true)
        }
        else if isCapturedDetailsForFace == true {
            
            
            let vc = storyboard?.instantiateViewController(withIdentifier: "FaceVerificationViewController") as! FaceVerificationViewController
            if isMrzIDCaptured == true {
                
                if  scanResult.documentType.lowercased().contains("p")  || scanResult.documentType == "P" {
                    self.view.makeToast("Looks like you have not captured the Proper Card, please try again with valid Card.")
                    //navigationController?.pushViewController(vc, animated: true)
                    return
                }
                
                vc.documentNumber = scanResult.personalNumber
                vc.isMrzSelected = true
                vc.isMrzIDCaptured = true
            }
            else {
                if  !scanResult.documentType.lowercased().contains("p")  || scanResult.documentType != "P" {
                    self.view.makeToast("Looks like you have not captured the passport, please try again with valid passport.")
                   // navigationController?.pushViewController(vc, animated: true)
                    return
                }
                
                
                let Birthformatter1 = DateFormatter()
                Birthformatter1.dateFormat = "yyyy-MM-dd"
               
                let dateStr = Birthformatter1.string(from: scanResult.birthdate!)
                let birth_Components1 = dateStr.components(separatedBy: "-")
                print("Birth Year : \(birth_Components1[0])")
                print("Birth Month : \(birth_Components1[1])")
                print("Birth Day : \(birth_Components1[2])")
                
                
                let expirtyformatter1 = DateFormatter()
                expirtyformatter1.dateFormat = "yyyy-MM-dd"
                let date_expiry = expirtyformatter1.string(from: scanResult.expiryDate!)
                let expirty_Components1 = date_expiry.components(separatedBy: "-")
                print("Expiry Year : \(expirty_Components1[0])")
                print("Expiry Month : \(expirty_Components1[1])")
                print("Expiry Day : \(expirty_Components1[2])")
                
                vc.documentNumber = scanResult.documentNumber
                vc.dob_YY = birth_Components1[0]
                vc.dob_MM = birth_Components1[1]
                vc.dob_DD = birth_Components1[2]
                vc.expiryDate_YY = expirty_Components1[0]
                vc.expiryDate_MM = expirty_Components1[1]
                vc.expiryDate_DD = expirty_Components1[2]
                vc.countryCode = scanResult.nationalityCountryCode
                vc.isMrzSelected = true
                vc.isMrzIDCaptured = false
            }
            navigationController?.pushViewController(vc, animated: true)
        }
        else if ismrzCapturedForOffCardStatus == true {
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "CheckCardStatusOffCardViewController") as! CheckCardStatusOffCardViewController
            vc.capturedCardNumber = scanResult.documentNumber
            vc.capturedIdn = scanResult.personalNumber
            vc.ismrzSuccess = true
            self.navigationController?.pushViewController(vc, animated: true)
        } else if ismrzCapturedForVerifyBiometric == true {
            var dict = NSMutableDictionary()
            dict.setValue(scanResult.documentNumber, forKey: "document_number")
            dict.setValue(scanResult.nationalityCountryCode, forKey: "nationality_country_code")
            dict.setValue(birth_Components[0], forKey: "dob_yy")
            dict.setValue(birth_Components[1], forKey: "dob_mm")
            dict.setValue(birth_Components[2], forKey: "dob_dd")
            dict.setValue(expirty_Components[0], forKey: "expiryDate_yy")
            dict.setValue(expirty_Components[1], forKey: "expiryDate_mm")
            dict.setValue(expirty_Components[2], forKey: "expiryDate_dd")
            NotificationCenter.default.post(name: Notification.Name("NotificationIdentifier"), object: nil, userInfo: ["dict": dict])

            navigationController?.popViewController(animated: true)
        }
        else{
            // FIXME: NFCPublicDataEFViewController excluded (PUBLIC_DATA_EF_TYPE bridging).
            let _ = scanResult.documentNumber
            let _ = birth_Components
            let _ = expirty_Components
        }
    }
    
    @IBAction func backButtonAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    

    

    /*
    // MARK: - Navigation

    // In a storyboard-based application, you will often want to do a little preparation before navigation
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        // Get the new view controller using segue.destination.
        // Pass the selected object to the new view controller.
    }
    */

}
