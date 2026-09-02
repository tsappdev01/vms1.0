//
//  ListReadersViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 04/05/22.
//

import UIKit
import CoreBluetooth

class ListReadersViewController: UIViewController,UIPickerViewDataSource, UIPickerViewDelegate {
    
    @IBOutlet var listReaderButton: UIButton!
    @IBOutlet var listReaderPickerView: UIPickerView!
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    //    var  readers = NSArray()
    var readerArr: [Any] = []
    var readers: [Any] = []
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        
        
        listReaderButton.layer.cornerRadius = 22.0
        listReaderButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        listReaderButton.layer.shadowColor = UIColor.lightGray.cgColor
        listReaderButton.layer.shadowOpacity = 0.5
        listReaderPickerView.isHidden = true
    }
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
    @IBAction func listReaderButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        
        utils.showProgressBar(Constants.ListReader, andView: self.view)
        readerArr.removeAll()
        readers.removeAll()
        //        DispatchQueue.global(qos: .background).async {
        do {
            self.readers = try toolkit?.listReaders() as! [Any]
            DispatchQueue.main.async {
                for i in 0..<self.readers.count {
                    let readersName: String? = (self.readers[i] as? CardReader)?.getName()
                    self.readerArr.append("\(readersName!)")
                }//for
                if self.readerArr.count > 0 {
                    self.listReaderPickerView.isHidden = false
                    self.listReaderPickerView.reloadAllComponents()
                    cardreader = self.readers[0] as? CardReader
                }//if
                else {
                    AlertView.showAlertTitle(Constants.Alert, withMessage: "Readers Not Found", onView: self)
                }//else
                utils.dismissProgressBar()
            }// update UI main queue
        }//do
        catch let error as NSError{
            DispatchQueue.main.async {
                let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                utils.dismissProgressBar()
                self.listReaderPickerView.isHidden = true
                self.listReaderPickerView.reloadAllComponents()
            }// update UI main queue
        }//catch
        // }//background queue
    }
    // MARK:PickerView Delagates methods
    func numberOfComponents(in pickerView: UIPickerView) -> Int {
        return 1
    }
    
    func pickerView(_ pickerView: UIPickerView, numberOfRowsInComponent component: Int) -> Int {
        return readerArr.count
    }
    
    func pickerView(_ pickerView: UIPickerView, titleForRow row: Int, forComponent component: Int) -> String? {
        if pickerView == self.listReaderPickerView && component == 0 {
            return readerArr[row] as? String
        }// if
        return nil
    }
    
    func pickerView(_ pickerView: UIPickerView, didSelectRow row: Int, inComponent component: Int) {
        cardreader = self.readers[row] as? CardReader
        self.listReaderPickerView.selectRow(row, inComponent: 0, animated: true)
    }
}
