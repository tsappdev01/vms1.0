//
//  AlertView.swift
//  EIDAToolkitSwiftTest
//
//  Created by  on 1/19/17.
//  Copyright © 2017 Emirates Identity Authority. All rights reserved.
//

import UIKit

class AlertView: NSObject {
    
  class func showAlertTitle(_ title :String,withMessage message:String,onView viewController:UIViewController)  {
    
        DispatchQueue.main.async(execute: {() -> Void in
        let  alert = UIAlertController(title: title, message: message, preferredStyle: .alert)
        let okButton = UIAlertAction(title: "OK", style: .default, handler: {(_ action: UIAlertAction) -> Void in
            alert.dismiss(animated: true, completion:nil)
        })
        alert.addAction(okButton)
        viewController.present(alert, animated: true, completion: nil)
        })// update UI main queue
    }// showAlertTitle()

}
