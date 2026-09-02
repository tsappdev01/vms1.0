//
//  DeviceIDViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 25/04/22.
//

import UIKit

class DeviceIDViewController: UIViewController {
        
    @IBOutlet weak var view_Roundedcorners: UIView!
    @IBOutlet var deviceIdTextView: UITextView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        // Do any additional setup after loading the view.
    }
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
    @IBAction func getDeviceIdButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        
        utils.showProgressBar(Constants.LoadingData, andView: self.view)
        DispatchQueue.global(qos: .background).async {
            do {
                let deviceid = try toolkit?.getDeviceId()
                DispatchQueue.main.async {
                    var resultData:String
                    resultData = deviceid!
                    self.deviceIdTextView.isHidden=false;
                    self.deviceIdTextView.text = resultData
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
}
