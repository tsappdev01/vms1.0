//
//  SignDataViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 09/05/22.
//

import UIKit

class SignDataViewController: UIViewController {
    
    @IBOutlet var enterDataTextView: UITextView!
    @IBOutlet weak var enterPinText: UITextField!
    @IBOutlet var digitalSignTextView: UITextView!
    @IBOutlet weak var authKey: UILabel!
    @IBOutlet var signDataButton: UIButton!
    @IBOutlet var verifySignatureButton: UIButton!
    @IBOutlet weak var view_Roundedcorners: UIView!
    @IBOutlet var switchButton: UISwitch!
    var  SwitchValue:Int = 0

    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        self.enterPinText.keyboardType = .numberPad
        self.enterPinText.keyboardAppearance = .light
        let enterPinNumberToolbar = UIToolbar(frame: CGRect(x: CGFloat(0), y: CGFloat(0), width: CGFloat(320), height: CGFloat(50)))
        enterPinNumberToolbar.barStyle = .blackTranslucent
        enterPinNumberToolbar.items = [UIBarButtonItem(title: "Cancel", style: .done, target: self, action: #selector(enterPinCancelNumberPad)), UIBarButtonItem(barButtonSystemItem: .flexibleSpace, target: nil, action: nil), UIBarButtonItem(title: "Done", style: .done, target: self, action: #selector(enterPinDoneWithNumberPad))]
        enterPinNumberToolbar.sizeToFit()
        self.enterPinText.inputAccessoryView=enterPinNumberToolbar

        // Do any additional setup after loading the view.
        
        signDataButton.layer.cornerRadius = 22.0
        signDataButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        signDataButton.layer.shadowColor = UIColor.lightGray.cgColor
        signDataButton.layer.shadowOpacity = 0.5
        
        verifySignatureButton.layer.cornerRadius = 22.0
        verifySignatureButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        verifySignatureButton.layer.shadowColor = UIColor.lightGray.cgColor
        verifySignatureButton.layer.shadowOpacity = 0.5
        
        SwitchValue = 1
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(true)
        
        enterPinText.text = ""
        enterDataTextView.text = ""
        enterPinText.resignFirstResponder()
        enterDataTextView.resignFirstResponder()
        digitalSignTextView.text = ""
        enterPinText.isEnabled = true
        enterDataTextView.isEditable = true
        enterDataTextView.isEditable = true
        
    }
    
    @IBAction func signDataButtonAction(_ sender: Any) {
                
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        digitalSignTextView.text = ""
        self.enterPinText.resignFirstResponder()
        self.enterDataTextView.resignFirstResponder()
        if self.enterPinText.text?.count==0 {
            AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.EnterPin, onView: self)
        }// if
        else {
            if (self.enterPinText.text?.count)!<4 {
                AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.MinPin, onView: self)
                self.enterPinText.text=""
            }// if
            else if (self.enterPinText.text?.count)!>16 {
                AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.MaxPin, onView: self)
                self.enterPinText.text=""
            }// else if
            else {
                utils.showProgressBar(Constants.SignData, andView: self.view)
                let requestId = Utils.generateSecureKey()
                let dataHash: Bool = false  /// 0 for plain data and 1 for Hash data
                let pinText = self.enterPinText.text!
                let enteredDataText = self.enterDataTextView.text!
                DispatchQueue.global(qos: .background).async {
                    do {
                        let request_handle = try cardreader?.prepareRequest(requestId!)
                        if (request_handle?.isEmpty == true) || (request_handle == "") || (request_handle == "(null)") {
                            AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Request_Handle_Empty, onView: self)
                        }//if
                        else {
                            let encodePin:String? = Utils.setEncrytion(request_handle, data: pinText,publickey: dataProtectionKey.getPublicKey(), keylength: Int32(dataProtectionKey.getKeyLength()))
                            if (encodePin?.isEmpty)! || (encodePin == "") || (encodePin == "(null)") {
                                AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Encode_Pin_Empty, onView: self)
                            }//if
                            else {
                                if self.SwitchValue == 1 {
                                    signresponse = try cardreader?.signChallenge(enteredDataText, inputLength: UInt32(enteredDataText.count), isInputHash: dataHash, encodedPin: encodePin!)
                                }//if
                                else if (self.SwitchValue == 2) {
                                    signresponse = try cardreader?.signData(enteredDataText, inputLength: UInt32(enteredDataText.count), isInputHash: dataHash, encodedPin: encodePin!)
                                }//else if
                                DispatchQueue.main.async {
                                    let xmlString = signresponse.getXmlString()
                                    let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: xmlString)
                                    if (errorMsg != "") && (errorMsg.count ) > 0 {
                                        AlertView.showAlertTitle(Constants.Alert, withMessage: errorMsg, onView: self)
                                    }//if
                                    else {
                                        let csignData = NSData(bytes: signresponse.getSignature(), length: Int(signresponse.getSignatureLength()))
                                        let signDataBase64string=Utils.base64forData(csignData as Data)
                                        self.digitalSignTextView.text=signDataBase64string
                                    }//else
                                }// update UI main queue
                            }//else
                        }//else
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
                            
                            self.enterDataTextView.text=""
                            self.enterPinText.text=""
                        }// update UI main queue
                    }//catch
                }//background queue
            }//else
        }//else
    }
    
    
    @IBAction func verifySignatureButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        utils.showProgressBar(Constants.VerifyData, andView: self.view)
        let requestId = Utils.generateSecureKey()
        let dataHash: Bool = false  /// 0 for plain data and 1 for Hash data
        let pinText = self.enterPinText.text!
        let dataText = self.enterDataTextView.text
        let dataTextCount = self.enterDataTextView.text.count
        DispatchQueue.global(qos: .background).async {
            do {
                let request_handle = try cardreader?.prepareRequest(requestId!)
                if (request_handle?.isEmpty == true) || (request_handle == "") || (request_handle == "(null)") {
                    AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Request_Handle_Empty, onView: self)
                }//if
                else {
                    let encodePin:String? = Utils.setEncrytion(request_handle, data: pinText,publickey: dataProtectionKey.getPublicKey(), keylength: Int32(dataProtectionKey.getKeyLength()))
                    if (encodePin?.isEmpty)! || (encodePin == "") || (encodePin == "(null)") {
                        AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Encode_Pin_Empty, onView: self)
                    }//if
                    else {
                        certificates = try cardreader?.getPkiCertificates(encodePin!)
                        let xmlString = certificates.getXmlString()
                        let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: xmlString)
                        if (errorMsg != "") && (errorMsg.count ) > 0 {
                            AlertView.showAlertTitle(Constants.Alert, withMessage: errorMsg, onView: self)
                        }//if
                        else {
                            if self.SwitchValue == 1 {
                                try cardreader?.verifySignature(dataText!, inputLength: UInt32(dataTextCount), isInputHash: dataHash, signature: signresponse.getSignature() , signatureLength: signresponse.getSignatureLength(), certificate: certificates.getAuthenticationCertificate(), certificateLength: certificates.getAuthenticationCertificateLength())
                            }//if
                            else if (self.SwitchValue == 2) {
                                try cardreader?.verifySignature(dataText!, inputLength: UInt32(dataTextCount), isInputHash: dataHash, signature: signresponse.getSignature() , signatureLength: signresponse.getSignatureLength(), certificate: certificates.getSigningCertificate(), certificateLength: certificates.getSigningCertificateLength())
                            }//else if
                            AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.SignatureVerified,onView: self)
                        }//else
                    }//else
                }//else
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
    
    @IBAction func switchButtonAction(_ sender: Any) {
        
        if SwitchValue == 1 {
            authKey.text = "Sign Key"
            switchButton.setOn(false, animated: true)
            SwitchValue = 2
            signDataButton.setTitle("Sign With Sign Key", for: .normal)
            verifySignatureButton.setTitle("Verify With Sign Key", for: .normal)
            enterPinText.text = ""
            enterDataTextView.text = ""
            digitalSignTextView.text = ""
        }
        else {
            authKey.text = "Auth Key"
            switchButton.setOn(true, animated: true)
            SwitchValue = 1
            signDataButton.setTitle("Sign With Auth Key", for: .normal)
            verifySignatureButton.setTitle("Verify With Auth Key", for: .normal)
            enterPinText.text = ""
            enterDataTextView.text = ""
            digitalSignTextView.text = ""
        }
    }

    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }

    @objc func enterPinCancelNumberPad() {
        enterPinText.resignFirstResponder()
        enterPinText.text=""
    }
    
    @objc func enterPinDoneWithNumberPad() {
        enterPinText.resignFirstResponder()
    }
    func textView(_ textView: UITextView, shouldChangeTextIn range: NSRange, replacementText text: String) -> Bool {
        
        if text == "\n" {
            textView.resignFirstResponder()
            return false
        }
        return true
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
