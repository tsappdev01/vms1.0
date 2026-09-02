//
//  SetNFCdetailsViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 06/05/22.
//

import UIKit
import CoreNFC

class SetNFCdetailsViewController: UIViewController, NFCTagReaderSessionDelegate,UITextFieldDelegate {
    
    @IBOutlet weak var cardNumberText: UITextField!
    @IBOutlet weak var dobYYtext: UITextField!
    @IBOutlet weak var dobMMText: UITextField!
    @IBOutlet weak var dobDDText: UITextField!
    @IBOutlet weak var expiryDateYYText: UITextField!
    @IBOutlet weak var expiryDateMMText: UITextField!
    @IBOutlet weak var expiryDateDDText: UITextField!
    @IBOutlet weak var nfcReadpublicdataButton: UIButton!
    
    @IBOutlet weak var view_Roundedcorners: UIView!
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
        
        nfcReadpublicdataButton.layer.cornerRadius = 22.0
        nfcReadpublicdataButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        nfcReadpublicdataButton.layer.shadowColor = UIColor.lightGray.cgColor
        nfcReadpublicdataButton.layer.shadowOpacity = 0.5
        
        self.setNumberPad(cardNumberText);
        self.setNumberPad(dobYYtext);
        self.setNumberPad(dobMMText);
        self.setNumberPad(dobDDText);
        self.setNumberPad(expiryDateYYText);
        self.setNumberPad(expiryDateMMText);
        self.setNumberPad(expiryDateDDText);
        
        dobYYtext.delegate = self
        dobMMText.delegate = self
        dobDDText.delegate = self
        expiryDateYYText.delegate = self
        expiryDateMMText.delegate = self
        expiryDateDDText.delegate = self
        
        dobYYtext.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        dobMMText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        dobDDText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        expiryDateYYText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        expiryDateMMText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        expiryDateDDText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        
        
        if isMrzSelected == true{
            
            cardNumberText.text = documentNumber
            dobYYtext.text = dob_YY
            dobMMText.text = dob_MM
            dobDDText.text = dob_DD
            
            expiryDateYYText.text = expiryDate_YY
            expiryDateMMText.text = expiryDate_MM
            expiryDateDDText.text = expiryDate_DD
            
            DispatchQueue.main.asyncAfter(deadline: .now() + 1.5) {
                self.cardNumberString = self.cardNumberText.text
                
                self.dobstring = "\(self.dobYYtext.text!)\(self.dobMMText.text!)\(self.dobDDText.text!)"
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
            case dobYYtext:
                dobMMText.becomeFirstResponder()
                
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
                
            case dobYYtext:
                dobYYtext.becomeFirstResponder()
                
            case dobMMText:
                dobYYtext.becomeFirstResponder()
                
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
        //        dobstring = "\(dobYYtext.text)\(dobMMText.text)\(dobDDText.text)"
        //        expireDateString = "\(expiryDateYYText.text)\(expiryDateMMText.text)\(expiryDateDDText.text)"
        dobstring = "\(dobYYtext.text!)\(dobMMText.text!)\(dobDDText.text!)"
        expireDateString = "\(expiryDateYYText.text!)\(expiryDateMMText.text!)\(expiryDateDDText.text!)"
        
        if toolkit == nil {
            DispatchQueue.main.async {
                self.view.makeToast(Constants.toolkitNotInitilized)
            }
            return
        }//if
        
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
             DispatchQueue.main.async {
                 self.view.makeToast(Constants.toolkitNotInitilized)
                 session.invalidate()
             }
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
            DispatchQueue.main.async {
                self.view.makeToast(Constants.toolkitNotInitilized)
                session.invalidate()
            }
            return
        }//if
        
        print("session \(session)")
        print("iso7816Tag \(iso7816Tag)")
        let requestId = Utils.generateSecureKey()
        
      DispatchQueue.global(qos: .background).async {
            
               do {
                try toolkit?.setNfcTag(session, tag: iso7816Tag)
                
                cardreader = try toolkit?.getReaderWithEmiratesId()
                
                if cardreader == nil {
                    session.invalidate()
                    return
                }//if
                
                if !(cardreader?.isConnected())! {
                    try cardreader?.connect()
                }
                
                 // yymmdd format
                
                print("\(self.cardNumberString!)")
                print("\(self.dobstring!)")
                print("\(self.expireDateString!)")
                try cardreader?.setNfcAuthenticationParameters(self.cardNumberString!, dateOfBirth: self.dobstring!, expiryDate: self.expireDateString!)//parameters are not required for V3 card

            DispatchQueue.main.async {
                session.alertMessage = "Reading Public Details from card"
            }// update UI main queue
                
            cardPublicData = try cardreader?.readPublicData(requestId!, readNonModifiableData: true, readModifiableData: true, readPhotography: true, readSignatueImage: true, readAddress: true)
                 
              DispatchQueue.main.async {
                
                if cardPublicData.getIdNumber() != nil {
                    print("getIdNumber \(cardPublicData.getIdNumber())")
                    print("getCardNumber \(cardPublicData.getCardNumber())")
                    self.performSegue(withIdentifier: "NFCpublicDataView", sender: self)
                }
                session.alertMessage = "Reading Completed, session going to close"
                session.invalidate()
             }// update UI main queue
                                
             }//do
        catch let error as NSError{
            DispatchQueue.main.async {
                session.invalidate()
                let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                utils.dismissProgressBar()
            }// update UI main queue
         }//catch
        }//background queue
    }
    
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {

        let segueID = segue.identifier
        if(segueID == "NFCpublicDataView") {
            
            self.cardNumberText.text = ""
            self.dobYYtext.text = ""
            self.dobMMText.text = ""
            self.dobDDText.text = ""
            self.expiryDateYYText.text = ""
            self.expiryDateMMText.text = ""
            self.expiryDateDDText.text = ""
            
            let detailView = segue.destination as! NFCPublicDataViewController
            detailView.getPublicaDataClass(cardPublicData)
        }
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
        self.dobYYtext.resignFirstResponder()
        self.dobMMText.resignFirstResponder()
        self.dobDDText.resignFirstResponder()
        self.expiryDateYYText.resignFirstResponder()
        self.expiryDateMMText.resignFirstResponder()
        self.expiryDateDDText.resignFirstResponder()
        
    }
    
    @objc func goBack() {
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
