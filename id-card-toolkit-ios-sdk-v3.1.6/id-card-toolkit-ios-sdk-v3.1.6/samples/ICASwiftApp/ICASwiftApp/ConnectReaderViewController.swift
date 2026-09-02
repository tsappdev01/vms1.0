//
//  ConnectReaderViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 25/04/22.
//

import UIKit

class ConnectReaderViewController: UIViewController {
    
    @IBOutlet weak var readerName: UILabel!
    @IBOutlet weak var statusLabel: UILabel!

    @IBOutlet weak var view_Roundedcorners: UIView!
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)

        // Do any additional setup after loading the view.
        statusLabel.isHidden = true
        statusLabel.font = UIFont.boldSystemFont(ofSize: 16.0)
        statusLabel.textColor = UIColor.white
        statusLabel.backgroundColor = UIColor.black
        statusLabel.layer.cornerRadius = 20.0
        statusLabel.layer.borderWidth = 0.1
        statusLabel.layer.masksToBounds = true
        statusLabel.layer.borderColor = UIColor.clear.cgColor
        
        readerName.isHidden = true
        readerName.adjustsFontSizeToFitWidth=true    
    }
    
    @IBAction func connectedDeviceButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        utils.showProgressBar(Constants.DeviceConnect, andView: self.view)
        DispatchQueue.global(qos: .background).async {
            do {
                try cardreader?.connect()
                DispatchQueue.main.async {
                    self.readerName.isHidden=false
                    self.statusLabel.isHidden = false
                    self.statusLabel.text = Constants.DeviceConnected
                    self.readerName.text = cardreader?.getName()
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
    

    /*
    // MARK: - Navigation

    // In a storyboard-based application, you will often want to do a little preparation before navigation
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        // Get the new view controller using segue.destination.
        // Pass the selected object to the new view controller.
    }
    */

}
