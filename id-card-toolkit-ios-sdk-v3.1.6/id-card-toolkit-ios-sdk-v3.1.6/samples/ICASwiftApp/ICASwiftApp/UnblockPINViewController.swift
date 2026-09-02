//
//  UnblockPINViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 09/05/22.
//

import UIKit

class UnblockPINViewController: UIViewController {
    
    @IBOutlet weak var pintext: UITextField!
    @IBOutlet var unblockPinButton: UIButton!
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        self.pintext.keyboardType = .numberPad
        self.pintext.keyboardAppearance = .light
        let enterPinNumberToolbar = UIToolbar(frame: CGRect(x: CGFloat(0), y: CGFloat(0), width: CGFloat(320), height: CGFloat(50)))
        enterPinNumberToolbar.barStyle = .blackTranslucent
        enterPinNumberToolbar.items = [UIBarButtonItem(title: "Cancel", style: .done, target: self, action: #selector(enterPinCancelNumberPad)), UIBarButtonItem(barButtonSystemItem: .flexibleSpace, target: nil, action: nil), UIBarButtonItem(title: "Done", style: .done, target: self, action: #selector(enterPinDoneWithNumberPad))]
        enterPinNumberToolbar.sizeToFit()
        self.pintext.inputAccessoryView=enterPinNumberToolbar

        unblockPinButton.layer.cornerRadius = 20.0
        unblockPinButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        unblockPinButton.layer.shadowColor = UIColor.lightGray.cgColor
        unblockPinButton.layer.shadowOpacity = 0.5
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(true)
        
        pintext.text=""
        pintext.resignFirstResponder()
    }
    
    @IBAction func btn_LoadFingerData(_ sender: Any) {
        let vc = storyboard?.instantiateViewController(withIdentifier: "FingerDataViewController") as! FingerDataViewController
        present(vc, animated: true, completion: nil)
    }
    
    @IBAction func unblockPinButtonAction(_ sender: Any) {
        
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
        
        self.pintext.resignFirstResponder()
        if self.pintext.text?.count==0 {
            AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.EnterPin, onView: self)
        }// if
        else {
            utils.showProgressBar(Constants.UnblockPin, andView: self.view)
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
                            response = try cardreader?.unblockPin(encodePin!, fingerData: selectedFingerData, sensorTimeout: Int32(Constants.SensorTimeOut))
                            DispatchQueue.main.async {
                                let xmlString = response.getXmlString()
                                if (xmlString != "") && ((xmlString.count ) > 0) {
                                    let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: xmlString)
                                    if (errorMsg != "") && (errorMsg.count ) > 0 {
                                        AlertView.showAlertTitle(Constants.Alert, withMessage: errorMsg, onView: self)
                                    }//if
                                    else {
                                        let xmlString = response.getXmlString()
                                        AlertView.showAlertTitle(Constants.Alert, withMessage: "\(Constants.CardUnblockedSuccess)\n\n \(xmlString)", onView: self)
                                    }//else
                                }//if
                                else {
                                    let status = response.getResponseStatus()
                                    AlertView.showAlertTitle(Constants.Alert, withMessage: status, onView: self)
                                }//else
                            }// update UI main queue
                        }//else
                    }//else
                    DispatchQueue.main.async {
                        utils.dismissProgressBar()
                        self.pintext.text=""
                    }// update UI main queue
                }//do
                catch let error as NSError{
                    DispatchQueue.main.async {
                        let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                        AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                        utils.dismissProgressBar()
                        self.pintext.text=""
                    }// update UI main queue
                }//catch
            }//background queue
        }//else
    }
    
    @objc func enterPinCancelNumberPad() {
        pintext.resignFirstResponder()
        pintext.text=""
    }
    
    @objc func enterPinDoneWithNumberPad() {
        pintext.resignFirstResponder()
    }
    @objc func goBack() {
        navigationController?.popViewController(animated: true)
    }

    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
}
