//
//  PINAuthenticationViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 25/04/22.
//

import UIKit

class PINAuthenticationViewController: UIViewController {
    
    @IBOutlet weak var view_Roundedcorners: UIView!
    @IBOutlet weak var verifyPInText: UITextField!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        self.verifyPInText.keyboardType = .numberPad
        self.verifyPInText.keyboardAppearance = .light
        
        
        let enterPinNumberToolbar = UIToolbar(frame: CGRect(x: CGFloat(0), y: CGFloat(0), width: CGFloat(320), height: CGFloat(50)))
        enterPinNumberToolbar.barStyle = .blackTranslucent
        enterPinNumberToolbar.items = [UIBarButtonItem(title: "Cancel", style: .done, target: self, action: #selector(enterPinCancelNumberPad)), UIBarButtonItem(barButtonSystemItem: .flexibleSpace, target: nil, action: nil), UIBarButtonItem(title: "Done", style: .done, target: self, action: #selector(enterPinDoneWithNumberPad))]
        enterPinNumberToolbar.sizeToFit()
        self.verifyPInText.inputAccessoryView=enterPinNumberToolbar
    }
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
    @IBAction func btn_Validate(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        self.verifyPInText.resignFirstResponder()
        if self.verifyPInText.text?.count==0 {
            AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.EnterPin, onView: self)
        }// if
        else {
            if (self.verifyPInText.text?.count)!<4 {
                AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.MinPin, onView: self)
                self.verifyPInText.text=""
            }// if
            else if (self.verifyPInText.text?.count)!>16 {
                AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.MaxPin, onView: self)
                self.verifyPInText.text=""
            }// else if
            else {
                utils.showProgressBar(Constants.PkiAuth, andView: self.view)
                let requestId = Utils.generateSecureKey()
                DispatchQueue.global(qos: .background).async {
                    do {
                        let request_handle = try cardreader?.prepareRequest(requestId!)
                        if (request_handle?.isEmpty == true) || (request_handle == "") || (request_handle == "(null)") {
                            AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Request_Handle_Empty, onView: self)
                        }//if
                        else {
                            let encodePin:String? = Utils.setEncrytion(request_handle, data: self.verifyPInText.text!,publickey: dataProtectionKey.getPublicKey(), keylength: Int32(dataProtectionKey.getKeyLength()))
                            if (encodePin?.isEmpty)! || (encodePin == "") || (encodePin == "(null)") {
                                AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Encode_Pin_Empty, onView: self)
                            }//if
                            else {
                                response = try cardreader?.authenticatePki(encodePin!)
                                DispatchQueue.main.async {
                                    let xmlString = response.getXmlString()
                                    let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: xmlString)
                                    if (errorMsg != "") && (errorMsg.count ) > 0 {
                                        AlertView.showAlertTitle(Constants.Alert, withMessage: errorMsg, onView: self)
                                    }//if
                                    else {
                                        let responseDic = response.getResponseDataElement()
                                        let matchStr:String = responseDic["CompleteAuthenticationStatus"] as! String
                                        let responseStatusStr:String = responseDic["ResponseStatus"] as! String
                                        AlertView.showAlertTitle(Constants.Alert, withMessage: "\(matchStr) \(responseStatusStr)", onView: self)
                                    }//else
                                }// update UI main queue
                            }//else
                        }//else
                        DispatchQueue.main.async {
                            utils.dismissProgressBar()
                            self.verifyPInText.text=""
                        }// update UI main queue
                    }//do
                    catch let error as NSError {
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
                            self.verifyPInText.text=""
                        }// update UI main queue
                    }//catch
                }//background queue
            }//else
        }//else
    }
    
    @objc func goBack() {
        navigationController?.popViewController(animated: true)
    }
    @objc func enterPinCancelNumberPad() {
        verifyPInText.resignFirstResponder()
        verifyPInText.text=""
    }
    
    @objc func enterPinDoneWithNumberPad() {
        verifyPInText.resignFirstResponder()
    }
    
    
}
