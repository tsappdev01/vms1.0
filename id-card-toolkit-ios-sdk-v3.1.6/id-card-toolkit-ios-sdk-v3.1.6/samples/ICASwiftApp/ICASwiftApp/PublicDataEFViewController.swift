// FIXME: PUBLIC_DATA_EF_TYPE enum does not bridge to Swift in Xcode 16+.
#if false
//
//  PublicDataEFViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 06/05/22.
//

import UIKit

class PublicDataEFViewController: UIViewController,UIPickerViewDelegate,UIPickerViewDataSource  {
    
    @IBOutlet weak var pickerViewEFData: UIPickerView!
    @IBOutlet weak var readPublicDataEFButton: UIButton!
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    var listArr: [Any] = []
    var selectedEFType = NSInteger()
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        readPublicDataEFButton.layer.cornerRadius = 22.0
        readPublicDataEFButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        readPublicDataEFButton.layer.shadowColor = UIColor.lightGray.cgColor
        readPublicDataEFButton.layer.shadowOpacity = 0.5
        
        listArr = ["IDN_CN", "ROOT_CERTIFICATE", "NON_MODIFIABLE_DATA", "MODIFIABLE_DATA", "PHOTOGRAPHY", "SIGNATURE_IMAGE", "HOME_ADDRESS", "WORK_ADDRESS"]
    }
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
    @IBAction func readPublicDataEFButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        let publicDataEFType = (PUBLIC_DATA_EF_TYPE.init(UInt32(selectedEFType)).rawValue)+1
        
        utils.showProgressBar(Constants.LoadingData, andView: self.view)
        DispatchQueue.global(qos: .background).async {
            do {
                try cardreader?.readPublicDataEF(PUBLIC_DATA_EF_TYPE(rawValue: publicDataEFType), validateSignature: 1)
                let length = cardreader?.getreadPublicDataEFLength()
                let byteData = cardreader?.getreadPublicDataEFByte()
                
                let getParsedData =  try cardreader?.parseEFData(efData: byteData, efDatalength:UInt32(length!))
                
                DispatchQueue.main.async {
                    
                    //                    var getEFDataString = ""
                    //                    for i in 0..<length {
                    //                        getEFDataString += String(format: "%02x",byteData?[i] as! UInt8)
                    //                    }
                    AlertView.showAlertTitle("", withMessage:"Parsed EF data \n\n  \(getParsedData!)",onView: self)
                    
                    utils.dismissProgressBar()
                }// update UI main queue
            }//do
            catch let error as NSError {
                DispatchQueue.main.async {
                    let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                    AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                    utils.dismissProgressBar()
                }// update UI main queue
            }//catch
        }//background queue
    }
    
    
    // MARK:PickerView Delagates methods
    func numberOfComponents(in pickerView: UIPickerView) -> Int {
        return 1
    }
    
    func pickerView(_ pickerView: UIPickerView, numberOfRowsInComponent component: Int) -> Int {
        return listArr.count
    }
    
    func pickerView(_ pickerView: UIPickerView, titleForRow row: Int, forComponent component: Int) -> String? {
        selectedEFType = row
        return listArr[row] as? String
    }
    func pickerView(_ pickerView: UIPickerView, didSelectRow row: Int, inComponent component: Int) {
        selectedEFType = row
    }
    @objc func goBack() {
        navigationController?.popViewController(animated: true)
    }
}
#endif
