//
//  CardSerialNumberViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 25/04/22.
//

import UIKit

class CardSerialNumberViewController: UIViewController {
    
    @IBOutlet weak var cardserialNumLabel: UILabel!
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
    }

    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
    @IBAction func getCSNButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        DispatchQueue.global(qos: .background).async {
            do {
               let cardserialNumber = try cardreader?.getCSN()
                DispatchQueue.main.async {
                    self.cardserialNumLabel.isHidden=false;
                    self.cardserialNumLabel.text = cardserialNumber
                }// update UI main queue
            }//do
            catch let error as NSError{
                DispatchQueue.main.async {
                    let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                    AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                }// update UI main queue
            }//catch
        }//background queue
    }

}
