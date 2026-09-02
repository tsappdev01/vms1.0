//
//  PublicDataListViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 06/05/22.
//

import UIKit
import CoreNFC

class PublicDataListViewController: UIViewController {
    
    @IBOutlet weak var view_Roundedcorners: UIView!
    var session: NFCTagReaderSession?
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.view_Roundedcorners.cornered_View([.layerMaxXMaxYCorner, .layerMinXMaxYCorner], radius: 20, borderColor: .red, borderWidth: 0)
    }
        
    @IBAction func btn_ReadPublicData(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "PublicDataViewController") as! PublicDataViewController
        self.navigationController?.pushViewController(vc, animated: true)
    }
    
    @IBAction func btn_ReadPublicDataEf(_ sender: Any) {
        // FIXME: PublicDataEFViewController excluded -- PUBLIC_DATA_EF_TYPE enum
        // does not bridge to Swift in Xcode 16+.
        self.view.makeToast("EF read temporarily unavailable")
    }
    
    @IBAction func btn_NFCReaderData(_ sender: Any) {
       showCardVersionAlert()
    }
    
    func showCardVersionAlert() {
        let alert = UIAlertController(title: "Card Version", message: "Select Card Version", preferredStyle: .alert)
        let vthreeCard = UIAlertAction(title: "V3/V4 Card", style: .destructive) { action in
           
            if toolkit == nil {
                DispatchQueue.main.async {
                    self.view.makeToast(Constants.toolkitNotInitilized)
                }
                return
            }//if
            
            if #available(iOS 13.0, *) {
                self.session = NFCTagReaderSession(pollingOption: .iso14443, delegate: self, queue: nil)
                self.session?.alertMessage = "Hold your iPhone near an NFC."
                self.session?.begin()
            } else {
                // Fallback on earlier versions
                AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.NFC_Not_Support,onView: self)
            }
        }
        let vtwoCard = UIAlertAction(title: "V2 Card", style: .destructive) { action in
            // create the alert
             let alert = UIAlertController(title: "Notice", message: "Select the option to get NFC data", preferredStyle: UIAlertController.Style.alert)
            
            alert.addAction(UIAlertAction(title: "MRZ Scanning", style: UIAlertAction.Style.default, handler: { action in
                let vc = self.storyboard?.instantiateViewController(withIdentifier: "PassportMRZScanViewController") as! PassportMRZScanViewController
                vc.isSetNFCdetailsViewController = true
                self.navigationController?.pushViewController(vc, animated: true)

            }))
            
            alert.addAction(UIAlertAction(title: "Manual Entry", style: UIAlertAction.Style.default, handler: { action in
                let vc = self.storyboard?.instantiateViewController(withIdentifier: "SetNFCdetailsViewController") as! SetNFCdetailsViewController
                self.navigationController?.pushViewController(vc, animated: true)
            }))
            
            alert.addAction(UIAlertAction(title: "Cancel", style: UIAlertAction.Style.destructive, handler: nil))

             // show the alert
             self.present(alert, animated: true, completion: nil)
        }
        alert.addAction(vthreeCard)
        alert.addAction(vtwoCard)
        self.present(alert, animated: true)
    }
    
    @IBAction func btn_NFCReaderDataEF(_ sender: Any) {
        // create the alert
        let alert = UIAlertController(title: "Notice", message: "Select the option to get NFC data", preferredStyle: UIAlertController.Style.alert)
        
        alert.addAction(UIAlertAction(title: "MRZ Scanning", style: UIAlertAction.Style.default, handler: { action in
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "PassportMRZScanViewController") as! PassportMRZScanViewController
            vc.isSetNFCdetailsViewController = false
            self.navigationController?.pushViewController(vc, animated: true)
            
        }))
        
        alert.addAction(UIAlertAction(title: "Manual Entry", style: UIAlertAction.Style.default, handler: { action in
            // FIXME: NFCPublicDataEFViewController excluded (PUBLIC_DATA_EF_TYPE bridging).
            self.view.makeToast("NFC EF read temporarily unavailable")
        }))
        
        alert.addAction(UIAlertAction(title: "Cancel", style: UIAlertAction.Style.destructive, handler: nil))
        
        // show the alert
        self.present(alert, animated: true, completion: nil)
    }
    
    
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
}

extension PublicDataListViewController: NFCTagReaderSessionDelegate {
    @available(iOS 13.0, *)
     func tagReaderSessionDidBecomeActive(_ session: NFCTagReaderSession) {
         print("tagReaderSessionDidBecomeActive")
         
         if toolkit == nil {
             DispatchQueue.main.async {
                 self.view.makeToast(Constants.toolkitNotInitilized)
                 session.invalidate()
             }
             return
         }//if
        
     }
    
     @available(iOS 13.0, *)
     func tagReaderSession(_ session: NFCTagReaderSession, didInvalidateWithError error: Error) {
         print("readerSession:didInvalidateWithError: \(error.localizedDescription)")
         AlertView.showAlertTitle(Constants.Alert, withMessage:error.localizedDescription,onView: self)
     }
    @available(iOS 13.0, *)
    func tagReaderSession(_ session: NFCTagReaderSession, didDetect tags: [NFCTag]) {
        print("readerSession:didDetectTags")
        
        if tags.count > 1 {
            let retryInterval = DispatchTimeInterval.milliseconds(500)
            session.alertMessage = "More than 1 tag is detected, please try again"
            DispatchQueue.global().asyncAfter(deadline: .now() + retryInterval, execute: {
                session.restartPolling()
            })
            return
        }
        
  
        let firstTag = tags.first!
        print("firstTag \(firstTag)")
        
        guard case .iso7816(let iso7816Tag) = firstTag else {
            let retryInterval = DispatchTimeInterval.milliseconds(500)
            session.alertMessage = "A tag that is not iso7816 is detected, please try again with tag iso7816."
            DispatchQueue.global().asyncAfter(deadline: .now() + retryInterval, execute: {
                session.restartPolling()
            })
            return
        }
        
        print("iso7816Tag identifier \(iso7816Tag.identifier)" )
        print("iso7816Tag initialSelectedAID \(iso7816Tag.initialSelectedAID)" )
        print("iso7816Tag historicalBytes \(iso7816Tag.historicalBytes)" )
        print("iso7816Tag applicationData \(iso7816Tag.applicationData)" )

        if toolkit == nil {
            DispatchQueue.main.async {
                self.view.makeToast(Constants.toolkitNotInitilized)
                session.invalidate()
            }
            return
        }//if
        
        print("session \(session)")
        print("iso7816Tag \(iso7816Tag)")
        let requestId = Utils.generateSecureKey()
        
      DispatchQueue.global(qos: .background).async {
            
               do {
                try toolkit?.setNfcTag(session, tag: iso7816Tag)
                
                cardreader = try toolkit?.getReaderWithEmiratesId()
                
                if cardreader == nil {
                    session.invalidate()
                    return
                }//if
                
                if !(cardreader?.isConnected())! {
                    try cardreader?.connect()
                }
                

            DispatchQueue.main.async {
                session.alertMessage = "Reading Public Details from card"
            }// update UI main queue
                
            cardPublicData = try cardreader?.readPublicData(requestId!, readNonModifiableData: true, readModifiableData: true, readPhotography: true, readSignatueImage: true, readAddress: true)
                 
              DispatchQueue.main.async {
                
                if cardPublicData.getIdNumber() != nil {
                    print("getIdNumber \(cardPublicData.getIdNumber())")
                    print("getCardNumber \(cardPublicData.getCardNumber())")
                    let vc = self.storyboard?.instantiateViewController(withIdentifier: "NFCPublicDataViewController") as! NFCPublicDataViewController
                    vc.getPublicaDataClass(cardPublicData)
                    self.navigationController?.pushViewController(vc, animated: true)
                    
                }
                session.alertMessage = "Reading Completed, session going to close"
                session.invalidate()
             }// update UI main queue
                                
             }//do
        catch let error as NSError{
            DispatchQueue.main.async {
                session.invalidate()
                let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                utils.dismissProgressBar()
            }// update UI main queue
         }//catch
        }//background queue
    }
    
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {

        let segueID = segue.identifier
        if(segueID == "NFCpublicDataView") {
        
            let detailView = segue.destination as! NFCPublicDataViewController
            detailView.getPublicaDataClass(cardPublicData)
        }
    }
    
}
