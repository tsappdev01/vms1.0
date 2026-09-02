//
//  LicenseExpiryViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 10/05/22.
//

import UIKit

class LicenseExpiryViewController: UIViewController {
    
    @IBOutlet weak var licenseExpirydateLabel: UILabel!
    @IBOutlet weak var licenseExpirydateButton: UIButton!
    @IBOutlet weak var view_roundedCorner: UIView!

    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_roundedCorner.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        licenseExpirydateButton.layer.cornerRadius = 22.0
        licenseExpirydateButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        licenseExpirydateButton.layer.shadowColor = UIColor.lightGray.cgColor
        licenseExpirydateButton.layer.shadowOpacity = 0.5
        
        self.licenseExpirydateLabel.isHidden=true;
    }
    
    @IBAction func licenseExpiryDateButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        
        DispatchQueue.global(qos: .background).async {
            do {
                let licExpiryStr = try toolkit?.getLicenseExpiryDate()
                DispatchQueue.main.async {
                    self.licenseExpirydateLabel.isHidden=false;
                    self.licenseExpirydateLabel.text = "\(licExpiryStr!)"
                }// update UI main queue
            }//do
            catch let error as NSError{
                let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
            }//catch
        }//background queue
    }
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
}
