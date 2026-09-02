//
//  PublicDataViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 04/05/22.
//

import UIKit

class PublicDataViewController: UIViewController,UITableViewDelegate, UITableViewDataSource  {
    
    @IBOutlet var listTable: UITableView!
    @IBOutlet var PhotoImage: UIImageView!
    @IBOutlet var signImage: UIImageView!
    @IBOutlet var view_Bottom: UIView!
    @IBOutlet var readPublicDataButton: UIButton!
    @IBOutlet weak var view_Roundedcorners: UIView!
    
    var getpModDataDictionary = NSMutableDictionary()
    var getpNonModDataDictionary = NSMutableDictionary()
    var getpHomeAddressDictionary = NSMutableDictionary()
    var getpWorkAddressDictionary = NSMutableDictionary()
    
    var pIDNumberKeyarr = NSMutableArray()
    var pIDNumberValuearr = NSMutableArray()
    var pModDatakeyarr = NSMutableArray()
    var pModDataValuearr = NSMutableArray()
    var pNonModDatakeyarr = NSMutableArray()
    var pNonModDataValuearr = NSMutableArray()
    var pHomeAddresskeyarr = NSMutableArray()
    var pHomeAddressValuearr = NSMutableArray()
    var pWorkAddresskeyarr = NSMutableArray()
    var pWorkAddressValuearr = NSMutableArray()
    

    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
        
        
        PhotoImage.layer.cornerRadius=8.0
        PhotoImage.layer.borderWidth=1.0
        PhotoImage.layer.masksToBounds=true
        PhotoImage.layer.borderColor=UIColor.clear.cgColor
        PhotoImage.layer.shadowOffset = CGSize(width: CGFloat(-15), height: CGFloat(20))
        PhotoImage.layer.shadowRadius=8
        PhotoImage.layer.shadowOpacity=0.1
        
        self.signImage.layer.cornerRadius=8.0
        self.signImage.layer.borderWidth=1.0
        self.signImage.layer.masksToBounds=true
        self.signImage.layer.borderColor=UIColor.clear.cgColor
        self.signImage.layer.shadowOffset = CGSize(width: CGFloat(-15), height: CGFloat(20))
        self.signImage.layer.shadowRadius=8
        self.signImage.layer.shadowOpacity=0.1
        
        signImage.isHidden=true
        PhotoImage.isHidden=true
        listTable.isHidden=true
        listTable.backgroundColor = UIColor.white
        
        readPublicDataButton.layer.cornerRadius = 22.0
        readPublicDataButton.layer.shadowOffset = CGSize(width: 5.0, height: 5.0)
        readPublicDataButton.layer.shadowColor = UIColor.lightGray.cgColor
        readPublicDataButton.layer.shadowOpacity = 0.5

        // Do any additional setup after loading the view.
    }
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
    @IBAction func readPublicDataButtonAction(_ sender: Any) {
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if
        
        utils.showProgressBar(Constants.ReadingCardDetails, andView: self.view)
        disableTheUIelements()
        let requestId = Utils.generateSecureKey()
        DispatchQueue.global(qos: .background).async {
        do {
         cardPublicData = try cardreader?.readPublicData(requestId!, readNonModifiableData: true, readModifiableData: true, readPhotography: true, readSignatueImage: true, readAddress: true)
        DispatchQueue.main.async {
//            let xmlString = cardPublicData.getXmlString()
//            let errorMsg: String = utils.validateToolkitResponse(requestId, xmlstring: xmlString)
//            if (errorMsg != "") && (errorMsg.count ) > 0 {
//                AlertView.showAlertTitle(Constants.Alert, withMessage: errorMsg, onView: self)
//            }//if
//            else {
                self.pIDNumberKeyarr.add("IDNumber")
                self.pIDNumberKeyarr.add("CardNumber")
                self.pIDNumberValuearr.add("\(cardPublicData.getIdNumber())")
                self.pIDNumberValuearr.add("\(cardPublicData.getCardNumber())")
                        
                let parseData = PublicDataParse()
                modifiabledata = cardPublicData.getModifiablePublicData()
                nonModifiabledata = cardPublicData.getNonModifiablePublicData()
                homeaddress = cardPublicData.getHomeAddress()
                workaddress = cardPublicData.getWorkAddress()
                        
                self.getpModDataDictionary = parseData.getModifiablePublicDataDetails(modifiabledata!)
                self.getpNonModDataDictionary = parseData.getNonModifiablePublicDataDetails(nonModifiabledata!)
                self.getpHomeAddressDictionary = parseData.getHomeAddressDetails(homeaddress!)
                self.getpWorkAddressDictionary = parseData.getWorkAddressDetails(workaddress!)
                
                let isEmpty = self.getpModDataDictionary.count == 0
                if !isEmpty {
                    var pModDataKeys: [Any]
                    pModDataKeys = self.getpModDataDictionary.allKeys
                    for key: String in pModDataKeys as! [String] {
                        let value = self.getpModDataDictionary[key]
                        self.pModDatakeyarr.add(key)
                        self.pModDataValuearr.add(value ?? String())
                    }// for loop
                    var pNonModDataKeys: [Any]
                    pNonModDataKeys = self.getpNonModDataDictionary.allKeys
                    for key: String in pNonModDataKeys as! [String] {
                        let value = self.getpNonModDataDictionary[key]
                        self.pNonModDatakeyarr.add(key)
                        self.pNonModDataValuearr.add(value ?? String())
                    }// for loop
                    var pHomeAddressKeys: [Any]
                    pHomeAddressKeys = self.getpHomeAddressDictionary.allKeys
                    for key: String in pHomeAddressKeys as! [String] {
                        let value = self.getpHomeAddressDictionary[key]
                        self.pHomeAddresskeyarr.add(key)
                        self.pHomeAddressValuearr.add(value ?? String())
                    }// for loop
                    var pWorkAddressKeys: [Any]
                    pWorkAddressKeys = self.getpWorkAddressDictionary.allKeys
                    for key: String in pWorkAddressKeys  as! [String] {
                        let value = self.getpWorkAddressDictionary[key]
                        self.pWorkAddresskeyarr.add(key)
                        self.pWorkAddressValuearr.add(value ?? String())
                    }// for loop
                    
                    self.listTable.isHidden=false
                    self.listTable.reloadData()
                    self.listTable.tableFooterView = self.view_Bottom
                    
                    let photoData: NSData = NSData(bytes: cardPublicData.getCardHolderPhoto(), length: Int(cardPublicData.getCardHolderPhotoLength()))
                    let pPhotoImage = UIImage(data: photoData as Data)
                            
                    let signData: NSData = NSData(bytes: cardPublicData.getHolderSignatureImage(), length: Int(cardPublicData.getHolderSignatureImageLength()))
                    let pSignImage = UIImage(data: signData as Data)
                            
                    self.PhotoImage.image=pPhotoImage
                    self.signImage.image=pSignImage
                    self.signImage.isHidden=false
                    self.PhotoImage.isHidden=false
                    
                    self.readPublicDataButton.isHidden = true
                    }//if
                  // }//else
                }// update UI main queue
                DispatchQueue.main.async {
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
    
    func disableTheUIelements() {
        // clear the objects
        self.pIDNumberKeyarr.removeAllObjects()
        self.pIDNumberValuearr.removeAllObjects()
        self.pModDatakeyarr.removeAllObjects()
        self.pModDataValuearr.removeAllObjects()
        self.pNonModDatakeyarr.removeAllObjects()
        self.pNonModDataValuearr.removeAllObjects()
        self.pHomeAddresskeyarr.removeAllObjects()
        self.pHomeAddressValuearr.removeAllObjects()
        self.pWorkAddresskeyarr.removeAllObjects()
        self.pWorkAddressValuearr.removeAllObjects()
        
        self.getpModDataDictionary.removeAllObjects()
        self.getpNonModDataDictionary.removeAllObjects()
        self.getpHomeAddressDictionary.removeAllObjects()
        self.getpWorkAddressDictionary.removeAllObjects()
        
        PhotoImage.isHidden=true
        self.signImage.isHidden=true
        self.PhotoImage.image=nil
        self.signImage.image=nil
        
        self.listTable.isHidden=true
        self.listTable.reloadData()
    }
    
    // MARK:TableView Delegate and Data source Methods
    func numberOfSections(in tableView: UITableView) -> Int {
        return 5
    }
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        switch section {
        case 0:
            return pIDNumberKeyarr.count
        case 1:
            return pModDatakeyarr.count
        case 2:
            return pNonModDatakeyarr.count
        case 3:
            return pHomeAddresskeyarr.count
        case 4:
            return pWorkAddresskeyarr.count
        default:
            break
        }// switch
        return 0
    }
    
    func tableView(_ tableView: UITableView, titleForHeaderInSection section: Int) -> String? {
        switch section {
        case 0:
            return "IDNumber info"
        case 1:
            return "ModData info"
        case 2:
            return "NonModData info"
        case 3:
            return "HomeAddress info"
        case 4:
            return "WorkAddress info"
        default:
            break
        }// switch
        return nil
    }
    
    func tableView(_ tableView: UITableView, willDisplayHeaderView view: UIView, forSection section: Int) {
        view.tintColor = Utils.color(fromHexString:"#65C466")
    }
    
    func tableView(_ tableView: UITableView, heightForHeaderInSection section: Int) -> CGFloat {
        return 32.0
    }
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        
        let customTabelId="identifierCell"
        let cell: CustomTableViewCell=(tableView.dequeueReusableCell(withIdentifier: customTabelId) as? CustomTableViewCell)!
        switch indexPath.section {
        case 0:
            cell.keyData.text = pIDNumberKeyarr[indexPath.row] as? String
            cell.valueData.text = pIDNumberValuearr[indexPath.row] as? String
            break
        case 1:
            cell.keyData.text = pModDatakeyarr[indexPath.row] as? String
            cell.valueData.text = pModDataValuearr[indexPath.row] as? String
            break
        case 2:
            cell.keyData.text = pNonModDatakeyarr[indexPath.row] as? String
            cell.valueData.text = pNonModDataValuearr[indexPath.row] as? String
            break
        case 3:
            cell.keyData.text = pHomeAddresskeyarr[indexPath.row] as? String
            cell.valueData.text = pHomeAddressValuearr[indexPath.row] as? String
            break
        case 4:
            cell.keyData.text = pWorkAddresskeyarr[indexPath.row] as? String
            cell.valueData.text = pWorkAddressValuearr[indexPath.row] as? String
            break
        default:
            break
        }// switch
        return cell
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
