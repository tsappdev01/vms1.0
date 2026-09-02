//
//  ToolkitVersionViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 10/05/22.
//

import UIKit

class ToolkitVersionViewController: UIViewController {
    
    @IBOutlet weak var toolkitVersionLabel: UILabel!
    @IBOutlet var toolkitVersionButton: UIButton!

    @IBOutlet weak var view_Roundedcorners: UIView!
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        
        toolkitVersionButton.layer.cornerRadius = 22.0
        toolkitVersionButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        toolkitVersionButton.layer.shadowColor = UIColor.lightGray.cgColor
        toolkitVersionButton.layer.shadowOpacity = 0.5
        
        toolkitVersionLabel.isHidden = true
        toolkitVersionLabel.adjustsFontSizeToFitWidth=true

        // Do any additional setup after loading the view.
    }
    
    
    @IBAction func toolkitVersionButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        
        DispatchQueue.global(qos: .background).async {
        do {
            let version = try toolkit?.getToolkitVersion()
            DispatchQueue.main.async {
                self.toolkitVersionLabel.isHidden=false;
                self.toolkitVersionLabel.text = "\(version!)"
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
