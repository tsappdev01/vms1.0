//
//  CheckCardStatusViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 10/05/22.
//

import UIKit

class CheckCardStatusViewController: UIViewController {
    
    @IBOutlet var checkCardStatusButton: UIButton!
    @IBOutlet weak var cardstatusText: UILabel!
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        checkCardStatusButton.layer.cornerRadius = 22.0
        checkCardStatusButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        checkCardStatusButton.layer.shadowColor = UIColor.lightGray.cgColor
        checkCardStatusButton.layer.shadowOpacity = 0.5

        // Do any additional setup after loading the view.
    }
    
    @IBAction func checkCardStatusButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        utils.showProgressBar(Constants.CheckCardStatus, andView: self.view)
        let requestId = Utils.generateSecureKey()
        DispatchQueue.global(qos: .background).async {
        do {
              response = try cardreader?.checkCardStatus(requestId!)
            DispatchQueue.main.async {
                let xmlString = response.getXmlString()
                let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: xmlString)
                if (errorMsg != "") && (errorMsg.count ) > 0 {
                    AlertView.showAlertTitle(Constants.Alert, withMessage: errorMsg, onView: self)
                }//if
                else {
                    let responseDic = response.getResponseDataElement()
                    self.cardstatusText.text = responseDic["CardStatus"] as? String
                    self.cardstatusText.textColor = UIColor.green
                }//else
                utils.dismissProgressBar()
                }// update UI main queue
            }//do
            catch let error as NSError{
                    DispatchQueue.main.async {
                        var vgresponse = String()
                        if error.userInfo["ErrorResponse"] != nil {
                            vgresponse = error.userInfo["ErrorResponse"] as! String
                            
                                let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                                AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
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
    

    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
}
