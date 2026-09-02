//
//  RegisterDeviceViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 22/04/22.
//

import UIKit

let utils = Utils()

class RegisterDeviceViewController: UIViewController,UITextFieldDelegate {
    
    @IBOutlet weak var userIdText: UITextField!
    @IBOutlet weak var passwordText: UITextField!
    @IBOutlet weak var deviceRefText: UITextField!
    @IBOutlet weak var deviceIDText: UILabel!
    @IBOutlet weak var deviceRegisIDText: UILabel!
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        userIdText.delegate = self
        passwordText.delegate = self
        deviceRefText.delegate = self
        deviceIDText.isHidden = true
        
        
        deviceRegisIDText.isHidden = true
        deviceRegisIDText.adjustsFontSizeToFitWidth=true
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
    }
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }    
    
    @IBAction func btn_RegisterDevice(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        
        if self.userIdText.text?.count==0 {
            AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.EnterDeviceUserName, onView: self)
        }//if
        else if self.passwordText.text?.count==0 {
            AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.EnterDevicePassword, onView: self)
        }//else if
        else if self.deviceRefText.text?.count==0 {
            AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.EnterDeviceRefId, onView: self)
        }//else if
        else {
            userIdText.isUserInteractionEnabled = true
            userIdText.resignFirstResponder()
            passwordText.isUserInteractionEnabled = true
            passwordText.resignFirstResponder()
            deviceRefText.isUserInteractionEnabled = true
            deviceRefText.resignFirstResponder()
            
        utils.showProgressBar(Constants.LoadingData, andView: self.view)
        let requestId = Utils.generateSecureKey()
        DispatchQueue.global(qos: .background).async {
        do {
            let request_handle = try toolkit?.prepareRequest(requestId!)
            if (request_handle?.isEmpty  == true) || (request_handle == "") || (request_handle == "(null)") {
                AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Request_Handle_Empty, onView: self)
            }//if
            else {
                let en_userIdText:String? = Utils.setEncrytion(request_handle, data: self.userIdText.text!,publickey: dataProtectionKey.getPublicKey(), keylength: Int32(dataProtectionKey.getKeyLength()))
                if (en_userIdText?.isEmpty)! || (en_userIdText == "") || (en_userIdText == "(null)") {
                    AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Encode_UserID_Empty, onView: self)
                }//if
                else {
                    let en_passwordText:String? = Utils.setEncrytion(request_handle, data:  self.passwordText.text!,publickey: dataProtectionKey.getPublicKey(), keylength: Int32(dataProtectionKey.getKeyLength()))
                    if (en_passwordText?.isEmpty)! || (en_passwordText == "") || (en_passwordText == "(null)") {
                         AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Encode_Password_Empty, onView: self)
                    }//if
                    else {
                registerDevice =   try  toolkit?.registerDevice(en_userIdText!, encodedPassword: en_passwordText!, deviceReferenceId: self.deviceRefText.text!)
                        DispatchQueue.main.async {
                            let deviceid: String = registerDevice.getDeviceRegistrationId()
                            self.deviceIDText.isHidden = false
                            self.deviceRegisIDText.isHidden = false
                            self.deviceRegisIDText.text = deviceid
                        }// update UI main queue
//                DispatchQueue.main.async {
//                let xmlString = registerDevice.getXmlString()
//                let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: xmlString)
//                if (errorMsg != "") && (errorMsg.count ) > 0 {
//                    AlertView.showAlertTitle(Constants.Alert, withMessage:errorMsg, onView: self)
//                }//if
//                else {
//                    let deviceid: String = registerDevice.getDeviceRegistrationId()
//                    self.deviceIDText.isHidden = false
//                    self.deviceRegisIDText.isHidden = false
//                    self.deviceRegisIDText.text = deviceid
//                 }//else
//                }// update UI main queue
              }//else
             }//else
            }//else
            DispatchQueue.main.async {
                utils.dismissProgressBar()
            }// update UI main queue
        }//do
        catch let error as NSError {
            DispatchQueue.main.async {
               var vgresponse = String()
                if error.userInfo["ErrorResponse"] != nil {
                    vgresponse = error.userInfo["ErrorResponse"] as! String
                if (vgresponse != "") && ((vgresponse.count ) > 0) {
//                    let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: vgresponse)
//                    if (errorMsg != "") && ((errorMsg.count ) > 0) {
//                        AlertView.showAlertTitle(Constants.Alert, withMessage:errorMsg, onView: self)
//                    }//if
//                    else {
                        let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                        AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
//                    }//else
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
        }//else
    }
    
    
    func textFieldShouldReturn(_ textField: UITextField) -> Bool {
        userIdText.isUserInteractionEnabled = true
        userIdText.resignFirstResponder()
        passwordText.isUserInteractionEnabled = true
        passwordText.resignFirstResponder()
        deviceRefText.isUserInteractionEnabled = true
        deviceRefText.resignFirstResponder()
        
        return true
    }
    
    
}
