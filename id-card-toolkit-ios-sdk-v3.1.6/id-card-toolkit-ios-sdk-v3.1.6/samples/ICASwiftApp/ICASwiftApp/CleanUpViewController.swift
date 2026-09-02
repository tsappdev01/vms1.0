//
//  CleanUpViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 09/05/22.
//

import UIKit

class CleanUpViewController: UIViewController {
    
    @IBOutlet var cleanUpButton: UIButton!
    @IBOutlet weak var cleanUpStatus: UILabel!

    @IBOutlet weak var view_Roundedcorners: UIView!
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)

        cleanUpButton.layer.cornerRadius = 22.0
        cleanUpButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        cleanUpButton.layer.shadowColor = UIColor.lightGray.cgColor
        cleanUpButton.layer.shadowOpacity = 0.5
        
        cleanUpStatus.font = UIFont.boldSystemFont(ofSize: 16.0)
        cleanUpStatus.textColor = UIColor.white
        cleanUpStatus.backgroundColor = UIColor.black
        cleanUpStatus.layer.cornerRadius = 20.0
        cleanUpStatus.layer.borderWidth = 0.1
        cleanUpStatus.layer.masksToBounds = true
        cleanUpStatus.layer.borderColor = UIColor.clear.cgColor
        cleanUpStatus.isHidden = true
        
    }
    
    @IBAction func cleanUpButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        
        DispatchQueue.global(qos: .background).async {
            do {
                try toolkit?.cleanup()
                DispatchQueue.main.async {
                    self.cleanUpStatus.isHidden=false;
                    self.cleanUpStatus.text = Constants.CleanUp
                }// update UI main queue
            }//do
            catch let error as NSError{
                let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
            }//catch
        }//background queue
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
