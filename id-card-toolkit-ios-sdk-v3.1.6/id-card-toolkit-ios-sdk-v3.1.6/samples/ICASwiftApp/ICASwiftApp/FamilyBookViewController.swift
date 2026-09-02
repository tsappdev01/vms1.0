//
//  FamilyBookViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 25/04/22.
//

import UIKit

class FamilyBookViewController: UIViewController,UITableViewDelegate, UITableViewDataSource {
    
    @IBOutlet weak var pintext: UITextField!
    @IBOutlet var listTable: UITableView!
    @IBOutlet var readfamilyBookButton: UIButton!
    
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    var pHeadofFamilyKeyarr = NSMutableArray()
    var pHeadofFamilyValuearr = NSMutableArray()

    override func viewDidLoad() {
        super.viewDidLoad()
        
        listTable.isHidden = true
        
        self.pintext.keyboardType = .numberPad
        self.pintext.keyboardAppearance = .light
        let enterPinNumberToolbar = UIToolbar(frame: CGRect(x: CGFloat(0), y: CGFloat(0), width: CGFloat(320), height: CGFloat(50)))
        enterPinNumberToolbar.barStyle = .blackTranslucent
        enterPinNumberToolbar.items = [UIBarButtonItem(title: "Cancel", style: .done, target: self, action: #selector(enterPinCancelNumberPad)), UIBarButtonItem(barButtonSystemItem: .flexibleSpace, target: nil, action: nil), UIBarButtonItem(title: "Done", style: .done, target: self, action: #selector(enterPinDoneWithNumberPad))]
        enterPinNumberToolbar.sizeToFit()
        self.pintext.inputAccessoryView=enterPinNumberToolbar
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)

        // Do any additional setup after loading the view.
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(true)
        pintext.text=""
        pintext.resignFirstResponder()
    }
    
    @IBAction func readfamilyBookButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        self.pintext.resignFirstResponder()
        if self.pintext.text?.count==0 {
            AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.EnterPin, onView: self)
        }// if
        else {
            if (self.pintext.text?.count)!<4 {
                AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.MinPin, onView: self)
                self.pintext.text=""
            }// if
            else if (self.pintext.text?.count)!>16 {
                AlertView.showAlertTitle(Constants.Alert, withMessage: Constants.MaxPin, onView: self)
                self.pintext.text=""
            }// else if
            else {
                utils.showProgressBar(Constants.ReadingCardDetails, andView: self.view)
                disableTheUIelements()
                let requestId = Utils.generateSecureKey()
                DispatchQueue.global(qos: .background).async {
                    do {
                        let request_handle = try cardreader?.prepareRequest(requestId!)
                        if (request_handle?.isEmpty == true) || (request_handle == "") || (request_handle == "(null)") {
                            AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Request_Handle_Empty, onView: self)
                        }//if
                        else {
                            
                            //                 let encodePin:String? = Utils.setEncrytion(request_handle, data: self.pintext.tex)
                            let encodePin:String? = Utils.setEncrytion(request_handle, data: self.pintext.text!, publickey: dataProtectionKey.getPublicKey(), keylength: Int32(dataProtectionKey.getKeyLength()) )
                            if (encodePin?.isEmpty)! || (encodePin == "") || (encodePin == "(null)") {
                                AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.Encode_Pin_Empty, onView: self)
                            }//if
                            else {
                                familybook = try cardreader?.readFamilyBookData(encodePin!)
                                DispatchQueue.main.async {
                                    let xmlString = familybook.getXmlString()
                                    let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: xmlString)
                                    if (errorMsg != "") && (errorMsg.count ) > 0 {
                                        AlertView.showAlertTitle(Constants.Alert, withMessage: errorMsg, onView: self)
                                    }//if
                                    else {
                                        headfamily = familybook.getHeadOfFamily()
                                        let FamilyBookparse = FamilyBookParse()
                                        FamilyBookparse.getHeadOfFamilyDetails(headfamily!)
                                        
                                        var wifeArr = NSArray()
                                        wifeArr = familybook.getWives()
                                        for i in 0..<wifeArr.count {
                                            wifedata = wifeArr[i] as? Wife
                                            FamilyBookparse.getWifeDetails(wifedata!, index: i)
                                        }//for
                                        var childArr = NSArray()
                                        childArr = familybook.getChildren()
                                        for i in 0..<childArr.count {
                                            childdata = childArr[i] as? Child
                                            FamilyBookparse.getChildDetails(childdata!, index: i)
                                        }//for
                                        self.pHeadofFamilyKeyarr = FamilyBookparse.familyBookKey()
                                        self.pHeadofFamilyValuearr = FamilyBookparse.familyBookValue()
                                    }//else
                                    if self.pHeadofFamilyKeyarr.count > 0 {
                                        self.pintext.text=""
                                       
                                        self.listTable.isHidden=false
                                        self.listTable.reloadData()
                                    }// if
                                }// update UI main queue
                                DispatchQueue.main.async {
                                    utils.dismissProgressBar()
                                    self.pintext.text=""
                                }// update UI main queue
                            }//else
                        }//else
                    }//do
                    catch let error as NSError{
                        DispatchQueue.main.async {
                            var vgresponse = String()
                            if error.userInfo["ErrorResponse"] != nil {
                                vgresponse = error.userInfo["ErrorResponse"] as! String
                                if (vgresponse != "") && ((vgresponse.count ) > 0) {
                                    let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: vgresponse)
                                    if (errorMsg != "") && ((errorMsg.count ) > 0) {
                                        AlertView.showAlertTitle(Constants.Alert, withMessage:errorMsg, onView: self)
                                    }//if
                                    else {
                                        let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                                        AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                                    }//else
                                }//if
                                else {
                                    let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                                    AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                                }//else
                            }//if
                            else {
                                let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                                AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                            }//else
                            
                            utils.dismissProgressBar()
                            self.pintext.text=""
                        }// update UI main queue
                    }//catch
                }//background queue
            }//else
        }//else
    }
    

    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
   
    @objc func enterPinCancelNumberPad() {
        pintext.resignFirstResponder()
        pintext.text=""
    }
    
    @objc func enterPinDoneWithNumberPad() {
        pintext.resignFirstResponder()
    }
    
    func disableTheUIelements() {
        pHeadofFamilyKeyarr.removeAllObjects()
        pHeadofFamilyValuearr.removeAllObjects()
    }
    
    // MARK:TableView Delegate and Data source Methods
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return pHeadofFamilyKeyarr.count
    }
    
    func tableView(_ tableView: UITableView, titleForHeaderInSection section: Int) -> String? {
        return "HeadofFamily"
    }
    
    func tableView(_ tableView: UITableView, willDisplayHeaderView view: UIView, forSection section: Int) {
        view.tintColor = Utils.color(fromHexString:"#F4F0E8")
    }
    
    func tableView(_ tableView: UITableView, heightForHeaderInSection section: Int) -> CGFloat {
        return 32.0
    }
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let customTabelId="identifierCell"
        let cell: CustomTableViewCell=(tableView.dequeueReusableCell(withIdentifier: customTabelId) as? CustomTableViewCell)!
        cell.keyData?.text = pHeadofFamilyKeyarr[indexPath.row] as? String
        cell.valueData?.text = pHeadofFamilyValuearr[indexPath.row] as? String
        
        return cell
    }
}
