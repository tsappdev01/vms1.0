//
//  FingerDataViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 09/05/22.
//

import UIKit

class FingerDataViewController: UIViewController,UIPickerViewDataSource, UIPickerViewDelegate  {
    
    @IBOutlet var fingerPickerView: UIPickerView!
    var getFingerIndexarr: [Any] = []
    var getFingerIndexReferenceArr: [Any] = []
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        // Do any additional setup after loading the view.
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        getFingerIndexarr.removeAll()
        getFingerIndexReferenceArr.removeAll()
        
        utils.showProgressBar(Constants.FingerData, andView: self.view)
        DispatchQueue.global(qos: .background).async {
            do {
                var  fingerData = NSArray()
                fingerData =  (try cardreader?.getFingerData())!
                DispatchQueue.main.async {
                    firstFingerData = fingerData[0] as? FingerData
                    secondFingerData = fingerData[1] as? FingerData
                    
                    var resultData:Int
                    resultData = firstFingerData.getFingerId()
                    self.getFingerIndexReferenceArr.append("\(resultData)")
                    
                    resultData = secondFingerData.getFingerId()
                    self.getFingerIndexReferenceArr.append("\(resultData)")
                    
                    resultData = firstFingerData.getFingerIndex().rawValue
                    self.getFingerIndexarr.append("\(self.getNameforFingerIndex(resultData)!)")
                    //
                    resultData = secondFingerData.getFingerIndex().rawValue
                    self.getFingerIndexarr.append("\(self.getNameforFingerIndex(resultData)!)")
                    
                    selectedFingerData = firstFingerData
                    
                    if self.getFingerIndexarr.count>1 {
                        self.fingerPickerView.reloadAllComponents()
                        self.fingerPickerView.isHidden=false
                    }// if
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
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(true)
        
//        getFingerIndexarr.removeAll()
//        getFingerIndexReferenceArr.removeAll()
//        self.fingerPickerView.reloadAllComponents()
//        self.fingerPickerView.isHidden=true
    }
    
    
    func numberOfComponents(in pickerView: UIPickerView) -> Int {
        return 2
    }
    
    func pickerView(_ pickerView: UIPickerView, numberOfRowsInComponent component: Int) -> Int {
        return getFingerIndexarr.count
    }
    
    func pickerView(_ pickerView: UIPickerView, titleForRow row: Int, forComponent component: Int) -> String? {
        if pickerView == fingerPickerView && component == 0 {
            return getFingerIndexReferenceArr[row] as? String
        }// if
        else if pickerView == fingerPickerView && component == 1 {
            return getFingerIndexarr[row] as? String
        }// else if
        return nil
    }
    
    func pickerView(_ pickerView: UIPickerView, didSelectRow row: Int, inComponent component: Int) {
        
        if row == 0 {
            selectedFingerData = firstFingerData
        }//if
        else {
            selectedFingerData = secondFingerData
        }//else
        
        self.fingerPickerView.selectRow(row, inComponent: 0, animated: true)
        self.fingerPickerView.selectRow(row, inComponent: 1, animated: true)
    }
    
    
    func getNameforFingerIndex(_ fingerindex: Int) -> String? {
        
        var fingerName = ""
        
        if fingerindex == 0 {
            fingerName = "None"
        } else if fingerindex == 3 {
            fingerName = "NoMeaning"
        } else if fingerindex == 5 {
            fingerName = "RightThumb"
        } else if fingerindex == 9 {
            fingerName = "RightIndex"
        } else if fingerindex == 13 {
            fingerName = "RightMiddle"
        } else if fingerindex == 17 {
            fingerName = "RightRing"
        } else if fingerindex == 15 {
            fingerName = "RightLittle"
        } else if fingerindex == 6 {
            fingerName = "LeftThumb"
        } else if fingerindex == 10 {
            fingerName = "LeftIndex"
        } else if fingerindex == 14 {
            fingerName = "LeftMiddle"
        } else if fingerindex == 18 {
            fingerName = "LeftRing"
        } else if fingerindex == 22 {
            fingerName = "LeftLittle"
        }

        return fingerName
        
    }
    
    
    @IBAction func btn_SelectedfingerPrint(_ sender: Any) {
        dismiss(animated: true, completion: nil)
    }
    
    
}
