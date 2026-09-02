//
//  PinResetViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 25/04/22.
//

import UIKit

class PinResetViewController: UIViewController {
    
    @IBOutlet weak var enterPin: UITextField!
    @IBOutlet weak var confirmPin: UITextField!
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)

        self.enterPin.keyboardType = .numberPad
        self.enterPin.keyboardAppearance = .light
        let enterPinNumberToolbar = UIToolbar(frame: CGRect(x: CGFloat(0), y: CGFloat(0), width: CGFloat(320), height: CGFloat(50)))
        enterPinNumberToolbar.barStyle = .blackTranslucent
        enterPinNumberToolbar.items = [UIBarButtonItem(title: "Cancel", style: .done, target: self, action: #selector(enterPinCancelNumberPad)), UIBarButtonItem(barButtonSystemItem: .flexibleSpace, target: nil, action: nil), UIBarButtonItem(title: "Done", style: .done, target: self, action: #selector(enterPinDoneWithNumberPad))]
        enterPinNumberToolbar.sizeToFit()
        self.enterPin.inputAccessoryView=enterPinNumberToolbar
        
        self.confirmPin.keyboardType = .numberPad
        self.confirmPin.keyboardAppearance = .light
        let confirmPinNumbertoolbar = UIToolbar(frame: CGRect(x: CGFloat(0), y: CGFloat(0), width: CGFloat(320), height: CGFloat(50)))
        confirmPinNumbertoolbar.barStyle = .blackTranslucent
        confirmPinNumbertoolbar.items = [UIBarButtonItem(title: "Cancel", style: .done, target: self, action: #selector(confirmPincancelNumberPad)), UIBarButtonItem(barButtonSystemItem: .flexibleSpace, target: nil, action: nil), UIBarButtonItem(title: "Done", style: .done, target: self, action: #selector(confirmPinDoneWithNumberPad))]
        self.confirmPin.inputAccessoryView = confirmPinNumbertoolbar
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(true)
        
        self.enterPin.text=""
        self.confirmPin.text=""
    }
    
    @IBAction func btn_LoadFingerData(_ sender: Any) {
        let vc = storyboard?.instantiateViewController(withIdentifier: "FingerDataViewController") as! FingerDataViewController
        present(vc, animated: true, completion: nil)
    }
    
    @IBAction func btn_PinReset(_ sender: Any) {
       
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
        
        self.enterPin.resignFirstResponder()
        self.confirmPin.resignFirstResponder()
        
        if self.enterPin.text?.count==0 {
            AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.EnterPin, onView: self)
        }// if
        else if self.confirmPin.text?.count==0 {
            AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.ConfirmPin, onView: self)
        }// else if
        else if !(self.enterPin.text == self.confirmPin.text) {
            AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.MismatchPin, onView: self)
            self.enterPin.text=""
            self.confirmPin.text=""
        }// else if
        else {
            if (self.enterPin.text?.count)!<4 && (self.confirmPin.text?.count)!<4 {
                AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.MinPin, onView: self)
                self.enterPin.text=""
                self.confirmPin.text=""
            }// if
            else if (self.enterPin.text?.count)!>16 && (self.confirmPin.text?.count)!>16 {
                AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.MaxPin, onView: self)
                self.enterPin.text=""
                self.confirmPin.text=""
            }// else if
            else {
                utils.showProgressBar(Constants.ResetPin, andView: self.view)
                let requestId = Utils.generateSecureKey()
                DispatchQueue.global(qos: .background).async {
                    do {
                        let request_handle = try cardreader?.prepareRequest(requestId!)
                        if (request_handle?.isEmpty == true) || (request_handle == "") || (request_handle == "(null)") {
                            AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Request_Handle_Empty, onView: self)
                        }//if
                        else {
                            let encodePin:String? = Utils.setEncrytion(request_handle, data: self.enterPin.text!,publickey: dataProtectionKey.getPublicKey(), keylength: Int32(dataProtectionKey.getKeyLength()))
                            if (encodePin?.isEmpty)! || (encodePin == "") || (encodePin == "(null)") {
                                AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Encode_Pin_Empty, onView: self)
                            }//if
                            else {
                                response = try cardreader?.resetPin(encodePin!, fingerData: selectedFingerData, sensorTimeout: Int32(Constants.SensorTimeOut))
                                DispatchQueue.main.async {
                                    let xmlString = response.getXmlString()
                                    if (xmlString != "") && ((xmlString.count ) > 0) {
                                        let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: xmlString)
                                        if (errorMsg != "") && (errorMsg.count ) > 0 {
                                            AlertView.showAlertTitle(Constants.Alert, withMessage: errorMsg, onView: self)
                                        }//if
                                        else {
                                            let xmlString = response.getXmlString()
                                            AlertView.showAlertTitle(Constants.Alert, withMessage: "\(Constants.PinResetSuccessStatus)\n\n \(xmlString)", onView: self)
                                        }//else
                                    }//if
                                    else {
                                        let status = response.getResponseStatus()
                                        if status == "0" {
                                            AlertView.showAlertTitle(Constants.Alert, withMessage: "\((Constants.PinResetSuccessStatus))",onView: self)
                                        }//if
                                    }//else
                                }// update UI main queue
                            }//else
                        }//else
                        DispatchQueue.main.async {
                            utils.dismissProgressBar()
                            self.enterPin.text=""
                            self.confirmPin.text=""
                        }// update UI main queue
                    }//do
                    catch let error as NSError{
                        DispatchQueue.main.async {
                            let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                            AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                            utils.dismissProgressBar()
                            self.enterPin.text=""
                            self.confirmPin.text=""
                        }// update UI main queue
                    }//catch
                }//background queue
            }//else
        }//else
    }
    
    @objc func enterPinCancelNumberPad() {
        self.enterPin.resignFirstResponder()
        self.enterPin.text=""
    }
    
    @objc func enterPinDoneWithNumberPad() {
        self.enterPin.resignFirstResponder()
    }
    
    @objc func confirmPincancelNumberPad() {
        self.confirmPin.resignFirstResponder()
        self.confirmPin.text=""
    }
    
    @objc func confirmPinDoneWithNumberPad() {
        self.confirmPin.resignFirstResponder()
    }

    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }

}
