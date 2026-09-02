//
//  VerifyCardAndBiometricViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 25/04/22.
//

import UIKit

class VerifyCardAndBiometricViewController: UIViewController {

    @IBOutlet weak var view_Roundedcorners: UIView!
    
    override func viewDidLoad() {
        super.viewDidLoad()

        // Do any additional setup after loading the view.
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
    }

    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
        
    @IBAction func btn_LoadFingerData(_ sender: Any) {
        let vc = storyboard?.instantiateViewController(withIdentifier: "FingerDataViewController") as! FingerDataViewController
        present(vc, animated: true, completion: nil)
    }
    
    @IBAction func btn_checkCardAndAuth(_ sender: Any) {
                
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
                response = try cardreader?.authenticateCardAndBiometric(requestId!, fingerIndex: selectedFingerData.getFingerIndex(), sensorTimeout: Int32(Constants.SensorTimeOut))
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
