//
//  InterfaceTypeViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 10/05/22.
//

import UIKit

class InterfaceTypeViewController: UIViewController {
    
    @IBOutlet weak var interfacetypeLabel: UILabel!
    @IBOutlet var getInterfacetypeButton: UIButton!
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        getInterfacetypeButton.layer.cornerRadius = 22.0
        getInterfacetypeButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        getInterfacetypeButton.layer.shadowColor = UIColor.lightGray.cgColor
        getInterfacetypeButton.layer.shadowOpacity = 0.5
        
        interfacetypeLabel.isHidden = true
        interfacetypeLabel.adjustsFontSizeToFitWidth=true
    }
    
    @IBAction func getInterfacetypeButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        utils.showProgressBar(Constants.LoadingData, andView: self.view)
        DispatchQueue.global(qos: .background).async {
        do {
            let type = try cardreader?.getInterfaceType()
            DispatchQueue.main.async {
                self.interfacetypeLabel.isHidden = false
                if type == "\(1)" {
                    self.interfacetypeLabel.text="Contact Interface"
                }//if
                else if type == "\(2)" {
                    self.interfacetypeLabel.text="Contact Less Interface"
                }//else if
                else {
                    self.interfacetypeLabel.text="NULL"
                }//else
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
