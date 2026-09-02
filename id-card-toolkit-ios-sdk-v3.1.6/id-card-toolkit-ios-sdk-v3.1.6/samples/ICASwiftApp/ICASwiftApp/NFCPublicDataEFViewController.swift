// FIXME: PUBLIC_DATA_EF_TYPE enum does not bridge to Swift in Xcode 16+.
#if false
//
//  NFCPublicDataEFViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 06/05/22.
//

import UIKit
import CoreNFC

class NFCPublicDataEFViewController: UIViewController,UIPickerViewDelegate,UIPickerViewDataSource,NFCTagReaderSessionDelegate,UITextFieldDelegate  {
    
    @IBOutlet weak var cardNumberText: UITextField!
    @IBOutlet weak var dobYYText: UITextField!
    @IBOutlet weak var dobMMText: UITextField!
    @IBOutlet weak var dobDDText: UITextField!
    @IBOutlet weak var expiryDateYYText: UITextField!
    @IBOutlet weak var expiryDateMMText: UITextField!
    @IBOutlet weak var expiryDateDDText: UITextField!
    @IBOutlet weak var pickerViewEFData: UIPickerView!
    @IBOutlet weak var nfcReadPublicDataButton: UIButton!
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    
    var listArr: [Any] = []
    var selectedEFType = NSInteger()
    
    var cardNumberString:String?
    var dobstring:String?
    var expireDateString:String?
    var session: NFCTagReaderSession?
    
    var documentNumber = ""
    var dob_YY = ""
    var dob_MM = ""
    var dob_DD = ""
    var expiryDate_YY = ""
    var expiryDate_MM = ""
    var expiryDate_DD = ""
    
    var isMrzSelected = false
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        // Do any additional setup after loading the view.
        nfcReadPublicDataButton.layer.cornerRadius = 22.0
        nfcReadPublicDataButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        nfcReadPublicDataButton.layer.shadowColor = UIColor.lightGray.cgColor
        nfcReadPublicDataButton.layer.shadowOpacity = 0.5
        
        self.setNumberPad(cardNumberText);
        self.setNumberPad(dobYYText);
        self.setNumberPad(dobMMText);
        self.setNumberPad(dobDDText);
        self.setNumberPad(expiryDateYYText);
        self.setNumberPad(expiryDateMMText);
        self.setNumberPad(expiryDateDDText);
        
        listArr = ["IDN_CN", "ROOT_CERTIFICATE", "NON_MODIFIABLE_DATA", "MODIFIABLE_DATA", "PHOTOGRAPHY", "SIGNATURE_IMAGE", "HOME_ADDRESS", "WORK_ADDRESS"]
        
        
        dobYYText.delegate = self
        dobMMText.delegate = self
        dobDDText.delegate = self
        expiryDateYYText.delegate = self
        expiryDateMMText.delegate = self
        expiryDateDDText.delegate = self
        
        dobYYText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        dobMMText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        dobDDText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        expiryDateYYText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        expiryDateMMText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        expiryDateDDText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        
        if isMrzSelected == true{
            
            cardNumberText.text = documentNumber
            dobYYText.text = dob_YY
            dobMMText.text = dob_MM
            dobDDText.text = dob_DD
            
            expiryDateYYText.text = expiryDate_YY
            expiryDateMMText.text = expiryDate_MM
            expiryDateDDText.text = expiryDate_DD
            
            DispatchQueue.main.asyncAfter(deadline: .now() + 1.5) {
                self.cardNumberString = self.cardNumberText.text
                self.dobstring = "\(self.dobYYText.text!)\(self.dobMMText.text!)\(self.dobDDText.text!)"
                self.expireDateString = "\(self.expiryDateYYText.text!)\(self.expiryDateMMText.text!)\(self.expiryDateDDText.text!)"
                if #available(iOS 13.0, *) {
                    self.session = NFCTagReaderSession(pollingOption: .iso14443, delegate: self, queue: nil)
                    self.session?.alertMessage = "Hold your iPhone near an NFC."
                    self.session?.begin()
                } else {
                    // Fallback on earlier versions
                    AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.NFC_Not_Support,onView: self)
                }
            }
        }
    }
    
    @objc func textFieldDidChange(textField: UITextField){
        let text = textField.text
        if  text?.count == 2 {
            switch textField{
            case dobYYText:
                dobYYText.becomeFirstResponder()
                
            case dobMMText:
                dobDDText.becomeFirstResponder()
                
            case dobDDText:
                expiryDateYYText.becomeFirstResponder()
                
            case expiryDateYYText:
                expiryDateMMText.becomeFirstResponder()
                
            case expiryDateMMText:
                expiryDateDDText.becomeFirstResponder()
                
            case expiryDateDDText:
                expiryDateDDText.resignFirstResponder()
            default:
                break
            }
        }
        if  text?.count == 0 {
            switch textField{
                
            case dobYYText:
                dobYYText.becomeFirstResponder()
                
            case dobMMText:
                dobYYText.becomeFirstResponder()
                
            case dobDDText:
                dobMMText.becomeFirstResponder()
                
            case expiryDateYYText:
                dobDDText.becomeFirstResponder()
                
            case expiryDateMMText:
                expiryDateYYText.becomeFirstResponder()
                
            case expiryDateDDText:
                expiryDateMMText.becomeFirstResponder()
                
            default:
                break
            }
        }
        else{
        }
    }
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
    
    @IBAction func nfcReadPublicDataButtonAction(_ sender: Any) {
        
        cardNumberString = cardNumberText.text
        dobstring = "\(dobYYText.text!)\(dobMMText.text!)\(dobDDText.text!)"
        expireDateString = "\(expiryDateYYText.text!)\(expiryDateMMText.text!)\(expiryDateDDText.text!)"
        
        if #available(iOS 13.0, *) {
            self.session = NFCTagReaderSession(pollingOption: .iso14443, delegate: self, queue: nil)
            self.session?.alertMessage = "Hold your iPhone near an NFC."
            self.session?.begin()
        } else {
            // Fallback on earlier versions
            AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.NFC_Not_Support,onView: self)
        }
    }
    
    
    @available(iOS 13.0, *)
    func tagReaderSessionDidBecomeActive(_ session: NFCTagReaderSession) {
        print("tagReaderSessionDidBecomeActive")
        if toolkit == nil {
            self.view.makeToast(Constants.toolkitNotInitilized)
            session.invalidate()
            return
        }//if
    }
    
    @available(iOS 13.0, *)
    func tagReaderSession(_ session: NFCTagReaderSession, didInvalidateWithError error: Error) {
        print("readerSession:didInvalidateWithError: \(error.localizedDescription)")
        AlertView.showAlertTitle(Constants.Alert, withMessage:error.localizedDescription,onView: self)
    }
    
    @available(iOS 13.0, *)
    func tagReaderSession(_ session: NFCTagReaderSession, didDetect tags: [NFCTag]) {
        print("readerSession:didDetectTags")
        
        if tags.count > 1 {
            let retryInterval = DispatchTimeInterval.milliseconds(500)
            session.alertMessage = "More than 1 tag is detected, please try again"
            DispatchQueue.global().asyncAfter(deadline: .now() + retryInterval, execute: {
                session.restartPolling()
            })
            return
        }
        
        let firstTag = tags.first!
        print("firstTag \(firstTag)")
        
        guard case .iso7816(let iso7816Tag) = firstTag else {
            let retryInterval = DispatchTimeInterval.milliseconds(500)
            session.alertMessage = "A tag that is not iso7816 is detected, please try again with tag iso7816."
            DispatchQueue.global().asyncAfter(deadline: .now() + retryInterval, execute: {
                session.restartPolling()
            })
            return
        }
        
        print("iso7816Tag identifier \(iso7816Tag.identifier)" )
        print("iso7816Tag initialSelectedAID \(iso7816Tag.initialSelectedAID)" )
        print("iso7816Tag historicalBytes \(iso7816Tag.historicalBytes)" )
        print("iso7816Tag applicationData \(iso7816Tag.applicationData)" )
        
        if toolkit == nil {
            self.view.makeToast(Constants.toolkitNotInitilized)
            session.invalidate()
            return
        }//if
        
        print("session \(session)")
        print("iso7816Tag \(iso7816Tag)")
        let requestId = Utils.generateSecureKey()
        let publicDataEFType = (PUBLIC_DATA_EF_TYPE.init(UInt32(selectedEFType)).rawValue)+1
        
        DispatchQueue.global(qos: .background).async {
            
            do {
                try toolkit?.setNfcTag(session, tag: iso7816Tag)
                
                cardreader = try toolkit?.getReaderWithEmiratesId()
                
                if cardreader == nil{
                    session.invalidate()
                    return
                }//if
                
                if !(cardreader?.isConnected())! {
                    try cardreader?.connect()
                }
                
                // yymmdd format
                
                //    try cardreader?.setNfcAuthenticationParameters("000020030", dateOfBirth: "800112", expiryDate: "290107")
                
                try cardreader?.setNfcAuthenticationParameters(self.cardNumberString!, dateOfBirth: self.dobstring!, expiryDate: self.expireDateString!)
                
                DispatchQueue.main.async {
                    session.alertMessage = "Reading Public Details from card"
                }// update UI main queue
                
                try cardreader?.readPublicDataEF(PUBLIC_DATA_EF_TYPE(rawValue: publicDataEFType), validateSignature: 1)
                
                let  length = cardreader?.getreadPublicDataEFLength()
                let byteData = cardreader?.getreadPublicDataEFByte()
                
                let getParsedData =  try cardreader?.parseEFData(efData: byteData, efDatalength:UInt32(length!))
                DispatchQueue.main.async {
                    //            length = cardreader?.getreadPublicDataEFLength()
                    //            let byteData = cardreader?.getreadPublicDataEFByte()
                    
                    //            var getEFDataString = ""
                    //            for i in 0..<length {
                    //                getEFDataString += String(format: "%02x",byteData?[i] as! UInt8)
                    //            }
                    session.alertMessage = "Reading Completed, session going to close"
                    session.invalidate()
                    
                    AlertView.showAlertTitle("", withMessage:"Parsed EF data \n\n  \(getParsedData!)",onView: self)
                    
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
    
    // MARK:PickerView Delagates methods
    func numberOfComponents(in pickerView: UIPickerView) -> Int {
        return 1
    }
    
    func pickerView(_ pickerView: UIPickerView, numberOfRowsInComponent component: Int) -> Int {
        return listArr.count
    }
    
    func pickerView(_ pickerView: UIPickerView, titleForRow row: Int, forComponent component: Int) -> String? {
        selectedEFType = row
        return listArr[row] as? String
    }
    func pickerView(_ pickerView: UIPickerView, didSelectRow row: Int, inComponent component: Int) {
        selectedEFType = row
    }
    
    func setNumberPad(_ customtextfiled:UITextField) {
        
        customtextfiled.keyboardType = .numberPad
        customtextfiled.keyboardAppearance = .light
        let confirmPinNumbertoolbar = UIToolbar(frame: CGRect(x: CGFloat(0), y: CGFloat(0), width: CGFloat(320), height: CGFloat(50)))
        confirmPinNumbertoolbar.barStyle = .blackTranslucent
        confirmPinNumbertoolbar.items = [UIBarButtonItem(title: "Cancel", style: .done, target: self, action: #selector(doneWithNumberPad)), UIBarButtonItem(barButtonSystemItem: .flexibleSpace, target: nil, action: nil), UIBarButtonItem(title: "Done", style: .done, target: self, action: #selector(doneWithNumberPad))]
        customtextfiled.inputAccessoryView = confirmPinNumbertoolbar
    }
    
    @objc func doneWithNumberPad() {
        
        self.cardNumberText.resignFirstResponder()
        self.dobYYText.resignFirstResponder()
        self.dobMMText.resignFirstResponder()
        self.dobDDText.resignFirstResponder()
        self.expiryDateYYText.resignFirstResponder()
        self.expiryDateMMText.resignFirstResponder()
        self.expiryDateDDText.resignFirstResponder()
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
#endif
