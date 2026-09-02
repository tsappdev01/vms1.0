//
//  MainViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 21/04/22.
//

import UIKit

class MainViewController: UIViewController {

    @IBOutlet weak var view_Roundedcorners: UIView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
    }
    
    @IBAction func btn_nextAction(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "HomePageViewController") as! HomePageViewController
        self.navigationController?.pushViewController(vc, animated: true)
    }
    
    @IBAction func btn_SettingsActions(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "ConfigurationViewController") as! ConfigurationViewController
        self.navigationController?.pushViewController(vc, animated: true)
    }
    
    
}
