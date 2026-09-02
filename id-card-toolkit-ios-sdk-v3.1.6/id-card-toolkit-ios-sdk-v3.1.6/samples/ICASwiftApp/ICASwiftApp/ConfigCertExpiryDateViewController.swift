//
//  ConfigCertExpiryDateViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 10/05/22.
//

import UIKit

class ConfigCertExpiryDateViewController: UIViewController {
    
    @IBOutlet weak var vgCertExpDate: UILabel!
    @IBOutlet weak var lvCertExpDate: UILabel!
    @IBOutlet weak var tlsCertExpDate: UILabel!
    @IBOutlet weak var agCertExpDate: UILabel!
    @IBOutlet weak var licenseExpDate: UILabel!
    @IBOutlet weak var getExpiryDateButton: UIButton!
    @IBOutlet weak var subView: UIStackView!
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        getExpiryDateButton.layer.cornerRadius = 22.0
        getExpiryDateButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        getExpiryDateButton.layer.shadowColor = UIColor.lightGray.cgColor
        getExpiryDateButton.layer.shadowOpacity = 0.5

        // Do any additional setup after loading the view.
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(true)
        self.subView.isHidden = true
    }
    
    @IBAction func getExpiryDateButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        
        utils.showProgressBar(Constants.LoadingData, andView: self.view)
        DispatchQueue.global(qos: .background).async {
            do {
                try toolkit?.getConfigCertificateExpiryDate()
                DispatchQueue.main.async {
                    self.vgCertExpDate.text = toolkit?.getConfig_vg_cert_expiry()
                    self.lvCertExpDate.text = toolkit?.getConfig_lv_cert_expiry()
                    self.tlsCertExpDate.text = toolkit?.getServer_tls_cert_expiry()
                    self.agCertExpDate.text = toolkit?.getConfig_ag_cert_expiry()
                    self.licenseExpDate.text = toolkit?.getLicense_expiry()
                    self.subView.isHidden=false;
                    utils.dismissProgressBar()
                }// update UI main queue
            }//do
            catch let error as NSError{
                DispatchQueue.main.async {
                    let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                    AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                    utils.dismissProgressBar()
                }// update UI main queue
            }//catch
        }//background queue
    }
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
}
