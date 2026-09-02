//
//  PkiCertificatesViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 10/05/22.
//

import UIKit

class PkiCertificatesViewController: UIViewController {
    
    @IBOutlet var PinView: UIView!
    @IBOutlet weak var pintext: UITextField!
    @IBOutlet var pkiCertificatesButton: UIButton!
    @IBOutlet  var authCertificateText: UITextView!
    @IBOutlet  var signCertificateText: UITextView!
    @IBOutlet weak var view_Roundedcorners: UIView!

    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        pkiCertificatesButton.layer.cornerRadius = 22.0
        pkiCertificatesButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        pkiCertificatesButton.layer.shadowColor = UIColor.lightGray.cgColor
        pkiCertificatesButton.layer.shadowOpacity = 0.5
        
        pintext.keyboardType = .numberPad
        pintext.keyboardAppearance = .light
        let enterPinNumberToolbar = UIToolbar(frame: CGRect(x: CGFloat(0), y: CGFloat(0), width: CGFloat(320), height: CGFloat(50)))
        enterPinNumberToolbar.barStyle = .blackTranslucent
        enterPinNumberToolbar.items = [UIBarButtonItem(title: "Cancel", style: .done, target: self, action: #selector(enterPinCancelNumberPad)), UIBarButtonItem(barButtonSystemItem: .flexibleSpace, target: nil, action: nil), UIBarButtonItem(title: "Done", style: .done, target: self, action: #selector(enterPinDoneWithNumberPad))]
        enterPinNumberToolbar.sizeToFit()
        pintext.inputAccessoryView=enterPinNumberToolbar
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(true)
        pintext.text=""
        pintext.resignFirstResponder()
    }// viewWillAppear()
        
    @IBAction func pkiCertificatesButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        self.pintext.resignFirstResponder()
        if self.pintext.text?.count==0 {
            AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.EnterPin, onView: self)
        }// if
        else {
            if (self.pintext.text?.count)!<4 {
                AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.MinPin, onView: self)
                self.pintext.text=""
            }// if
            else if (self.pintext.text?.count)!>16 {
                AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.MaxPin, onView: self)
                self.pintext.text=""
            }// else if
            else {
                utils.showProgressBar(Constants.FetchCertificates, andView: self.view)
                let requestId = Utils.generateSecureKey()
                DispatchQueue.global(qos: .background).async {
                    do {
                        let request_handle = try cardreader?.prepareRequest(requestId!)
                        if (request_handle?.isEmpty == true) || (request_handle == "") || (request_handle == "(null)") {
                            AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Request_Handle_Empty, onView: self)
                        }//if
                        else {
                            let encodePin:String? = Utils.setEncrytion(request_handle, data: self.pintext.text!,publickey: dataProtectionKey.getPublicKey(), keylength: Int32(dataProtectionKey.getKeyLength()))
                            if (encodePin?.isEmpty)! || (encodePin == "") || (encodePin == "(null)") {
                                AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Encode_Pin_Empty, onView: self)
                            }//if
                            else {
                                certificates = try cardreader?.getPkiCertificates(encodePin!)
                                DispatchQueue.main.async {
                                    let xmlString = certificates.getXmlString()
                                    let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: xmlString)
                                    if (errorMsg != "") && (errorMsg.count ) > 0 {
                                        AlertView.showAlertTitle(Constants.Alert, withMessage: errorMsg, onView: self)
                                    }//if
                                    else {
                                        let authCerData = NSData(bytes: certificates.getAuthenticationCertificate(), length: Int(certificates.getAuthenticationCertificateLength()))
                                        let authStr=Utils.base64forData(authCerData as Data)
                                        
                                        let signCerdata = NSData(bytes: certificates.getSigningCertificate(), length: Int(certificates.getSigningCertificateLength()))
                                        let signStr=Utils.base64forData(signCerdata as Data)
                                        
                                        self.PinView.isHidden=true;
                                        self.authCertificateText.text=authStr
                                        self.signCertificateText.text=signStr
                                    }//else
                                }// update UI main queue
                            }//else
                        }//esle
                        DispatchQueue.main.async {
                            utils.dismissProgressBar()
                            self.pintext.text=""
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
                            self.pintext.text=""
                        }// update UI main queue
                    }//catch
                }//background queue
            }//else
        }//else
    }
    
    @objc func enterPinCancelNumberPad() {
        pintext.resignFirstResponder()
        pintext.text=""
    }
    
    @objc func enterPinDoneWithNumberPad() {
        pintext.resignFirstResponder()
    }
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
}
