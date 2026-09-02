//
//  CheckCardStatusOffCardViewController.swift
//  ICASwiftApp
//
//  Created by doti naresh on 28/02/23.
//

import UIKit

class CheckCardStatusOffCardViewController: UIViewController {

    @IBOutlet weak var cardNumberText: UITextField!
    @IBOutlet weak var nationIDText: UITextField!
    @IBOutlet weak var view_Roundedcorners: UIView!
    @IBOutlet weak var checkCardStatusButton: UIButton!
    
    var capturedCardNumber = String()
    var capturedIdn = String()
    var ismrzSuccess = false
    
    override func viewDidLoad() {
        super.viewDidLoad()

        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        checkCardStatusButton.layer.cornerRadius = 22.0
        checkCardStatusButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        checkCardStatusButton.layer.shadowColor = UIColor.lightGray.cgColor
        checkCardStatusButton.layer.shadowOpacity = 0.5
        
        // Do any additional setup after loading the view.
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(true)
        
        if ismrzSuccess == false {
            let alert = UIAlertController(title: "Scan ID Card Back MRZ", message: "Select the option to get Card Details", preferredStyle: UIAlertController.Style.alert)
            
            alert.addAction(UIAlertAction(title: "MRZ Scanning", style: UIAlertAction.Style.default, handler: { action in
                let vc = self.storyboard?.instantiateViewController(withIdentifier: "PassportMRZScanViewController") as! PassportMRZScanViewController
                vc.ismrzCapturedForOffCardStatus = true
                self.navigationController?.pushViewController(vc, animated: true)
                
            }))
            
            alert.addAction(UIAlertAction(title: "Manual Entry", style: UIAlertAction.Style.default, handler: { action in
                
            }))
            
            alert.addAction(UIAlertAction(title: "Cancel", style: UIAlertAction.Style.destructive, handler: nil))
            
            // show the alert
            self.present(alert, animated: true, completion: nil)
        }
        else {
            cardNumberText.text = capturedCardNumber
            nationIDText.text = capturedIdn
        }
    }
    
    @IBAction func checkCardStatusButtonAction(_ sender: Any) {
        
//        if toolkit == nil  {
//            self.view.makeToast(Constants.toolkitNotInitilized)
//            return
//        }
//        if cardreader == nil {
//            self.view.makeToast(Constants.cardreaderNotConnect)
//            return
//        }
//        
//        DispatchQueue.global(qos: .background).async {
//               do {
//                   let str = try cardreader?.getCardversion()
//                   DispatchQueue.main.async {
//                       print(str)
//                       utils.dismissProgressBar()
//                       }// update UI main queue
//                   }//do
//                   catch let error as NSError{
//                           DispatchQueue.main.async {
//                               var vgresponse = String()
//                               if error.userInfo["ErrorResponse"] != nil {
//                                   vgresponse = error.userInfo["ErrorResponse"] as! String
//       
//                                       let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
//                                       AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
//                               }//if
//                               else {
//                                   let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
//                                   AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
//                               }//else
//                               utils.dismissProgressBar()
//                           }// update UI main queue
//                   }//catch
//                }//background queue
        
        
        

        
        utils.showProgressBar(Constants.CheckCardStatus, andView: self.view)
        let requestId = Utils.generateSecureKey()
        let cardnumber = cardNumberText.text ?? ""
        let idn = nationIDText.text ?? ""
        DispatchQueue.global(qos: .background).async {
        do {
            response = try toolkit?.checkCardStatusOffCard(requestId!, cardNumber: cardnumber, idn: idn)
            DispatchQueue.main.async {
                let xmlString = response.getXmlString()
                let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: xmlString)
                if (errorMsg != "") && (errorMsg.count ) > 0 {
                    AlertView.showAlertTitle(Constants.Alert, withMessage: errorMsg, onView: self)
                }//if
                else {
                    let responseDic = response.getResponseDataElement()
                    let cardStatusmsg = responseDic["CardStatus"] as? String
                    print("cardStatusmsg \(cardStatusmsg)")
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
    
    @IBAction func backButtonAction(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "HomePageViewController") as! HomePageViewController
        self.navigationController?.pushViewController(vc, animated: true)
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
