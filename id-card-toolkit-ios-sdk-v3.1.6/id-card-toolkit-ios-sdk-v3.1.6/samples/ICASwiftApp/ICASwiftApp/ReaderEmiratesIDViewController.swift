//
//  ReaderEmiratesIDViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 10/05/22.
//

import UIKit

class ReaderEmiratesIDViewController: UIViewController {
    
    @IBOutlet weak var readerNameLabel: UILabel!
    @IBOutlet var readerWithIDButton: UIButton!
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        readerWithIDButton.layer.cornerRadius = 22.0
        readerWithIDButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        readerWithIDButton.layer.shadowColor = UIColor.lightGray.cgColor
        readerWithIDButton.layer.shadowOpacity = 0.5
        
        readerNameLabel.isHidden = true
        readerNameLabel.adjustsFontSizeToFitWidth=true
    }
    
    @IBAction func readerWithIDButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        utils.showProgressBar(Constants.ReaderName, andView: self.view)
        DispatchQueue.global(qos: .background).async {
            do {
                cardreader = try toolkit?.getReaderWithEmiratesId()
                DispatchQueue.main.async {
                    self.readerNameLabel.isHidden=false;
                    var resultData:String
                    resultData = (cardreader?.getName())!
                    self.readerNameLabel.text = resultData
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
    
    @IBAction func btn_(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
    
}
