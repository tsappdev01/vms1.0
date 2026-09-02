//
//  NFCPublicDataViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 06/05/22.
//

import UIKit

class NFCPublicDataViewController: UIViewController,UITableViewDelegate, UITableViewDataSource {
    
    @IBOutlet weak var photoImage: UIImageView!
    @IBOutlet weak var signImage: UIImageView!
    @IBOutlet var listTableview: UITableView!
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

        photoImage.layer.cornerRadius=8.0
        photoImage.layer.borderWidth=1.0
        photoImage.layer.masksToBounds=true
        photoImage.layer.borderColor=UIColor.clear.cgColor
        photoImage.layer.shadowOffset = CGSize(width: CGFloat(-15), height: CGFloat(20))
        photoImage.layer.shadowRadius=8
        photoImage.layer.shadowOpacity=0.1
               
        signImage.layer.cornerRadius=8.0
        signImage.layer.borderWidth=1.0
        signImage.layer.masksToBounds=true
        signImage.layer.borderColor=UIColor.clear.cgColor
        signImage.layer.shadowOffset = CGSize(width: CGFloat(-15), height: CGFloat(20))
        signImage.layer.shadowRadius=8
        signImage.layer.shadowOpacity=0.1
    }
    

    func getPublicaDataClass(_ cardPublicData:CardPublicData) {
        DispatchQueue.main.asyncAfter(deadline: .now() + 2) {
        //DispatchQueue.main.async {
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
                                                        
                let photoData: NSData = NSData(bytes: cardPublicData.getCardHolderPhoto(), length: Int(cardPublicData.getCardHolderPhotoLength()))
                let pPhotoImage = UIImage(data: photoData as Data)
                                    
                let signData: NSData = NSData(bytes: cardPublicData.getHolderSignatureImage(), length: Int(cardPublicData.getHolderSignatureImageLength()))
                let pSignImage = UIImage(data: signData as Data)
                                    
                self.photoImage.image=pPhotoImage
                self.signImage.image=pSignImage
                self.listTableview.reloadData()
            }//if
         } // update UI main queue
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
        view.tintColor = Utils.color(fromHexString:"#BE9647")
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

    @IBAction func backButtonAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
}
