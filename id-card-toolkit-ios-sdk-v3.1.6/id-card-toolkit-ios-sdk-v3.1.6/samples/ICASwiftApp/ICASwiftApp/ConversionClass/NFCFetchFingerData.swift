//
//  NFCReader.swift
//  ICASwiftApp
//
//  Created by Aptiway on 15/06/26.
//

import Foundation
import CoreNFC

class NFCFetchFingerData: NSObject {
    static let shared = NFCFetchFingerData()
    weak var delegate: NFCReadFingerData?
    var session: NFCTagReaderSession?
    var documentNumber = ""
    var dob_YY = ""
    var dob_MM = ""
    var dob_DD = ""
    var expiryDate_YY = ""
    var expiryDate_MM = ""
    var expiryDate_DD = ""
    var viewController : UIViewController?
    var cardNumberString:String?
    var dobstring:String?
    var expireDateString:String?
    var isCardVersionV2:Bool = false
    
    override init() {}
    
    func startReading(vc: UIViewController, isCardVersion_V2: Bool) {
        
        viewController = vc
        isCardVersionV2 = isCardVersion_V2
        
        DispatchQueue.main.asyncAfter(deadline: .now() + 1.5) {
            self.cardNumberString = self.documentNumber
            
            self.dobstring = "\(self.dob_YY)\(self.dob_MM)\(self.dob_DD)"
            self.expireDateString = "\(self.expiryDate_YY)\(self.expiryDate_MM)\(self.expiryDate_DD)"
            print("cardNumber ",self.self .cardNumberString)
            print("dob", self.dobstring)
            print("expd", self.expireDateString)
            if #available(iOS 13.0, *) {
                self.session = NFCTagReaderSession(pollingOption: .iso14443, delegate: self, queue: nil)
                self.session?.alertMessage = "Hold your iPhone near an NFC."
                self.session?.begin()
            } else {
                // Fallback on earlier versions
                AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.NFC_Not_Support,onView: self.viewController!)
            }
        }
    }
    
}

extension NFCFetchFingerData: NFCTagReaderSessionDelegate {
    func tagReaderSessionDidBecomeActive(_ session: NFCTagReaderSession) {
        print("tagReaderSessionDidBecomeActive")
        if toolkit == nil {
            DispatchQueue.main.async {
                self.viewController!.view.makeToast(Constants.toolkitNotInitilized)
                session.invalidate()
                self.delegate?.errorOnReading(Constants.toolkitNotInitilized)
            }
            return
        }
    }
    
    func tagReaderSession(_ session: NFCTagReaderSession, didInvalidateWithError error: any Error) {
        print("tagReaderSession didInvalidateWithError")
        DispatchQueue.main.async {
            AlertView.showAlertTitle(Constants.Alert, withMessage: error.localizedDescription, onView: self.viewController!)
            self.delegate?.errorOnReading(error.localizedDescription)
        }
    }
    
    func tagReaderSession(_ session: NFCTagReaderSession, didDetect tags: [NFCTag]) {
        print("tagReaderSession didDetect")
        if tags.count > 1 {
            let retryInterval = DispatchTimeInterval.milliseconds(500)
            session.alertMessage = "More than 1 tag is detected, please try again"
            DispatchQueue.global().asyncAfter(deadline: .now() + retryInterval, execute: {
                session.restartPolling()
            })
            self.delegate?.errorOnReading("More than 1 tag is detected, please try again")
            return
        }
        
        let firstTag = tags.first!
        
        guard case .iso7816(let iso7816Tag) = firstTag else {
            let retryInterval = DispatchTimeInterval.milliseconds(500)
            session.alertMessage = "A tag that is not iso7816 is detected, please try again with tag iso7816."
            DispatchQueue.global().asyncAfter(deadline: .now() + retryInterval, execute: {
                session.restartPolling()
            })
            self.delegate?.errorOnReading("A tag that is not iso7816 is detected, please try again with tag iso7816.")
            return
        }
        
        if toolkit == nil {
            DispatchQueue.main.async {
                self.viewController!.view.makeToast(Constants.toolkitNotInitilized)
                session.invalidate()
            }
            self.delegate?.errorOnReading(Constants.toolkitNotInitilized)
            return
        }
        
        DispatchQueue.global(qos: .background).async {
            do {
                try toolkit?.setNfcTag(session, tag: iso7816Tag)
                
                cardreader = try toolkit?.getReaderWithEmiratesId()
                
                if cardreader == nil {
                    session.invalidate()
                    return
                }
                
                if !(cardreader?.isConnected())! {
                    try cardreader?.connect()
                }
                
                if(self.isCardVersionV2) {
                    try cardreader?.setNfcAuthenticationParameters(self.cardNumberString!, dateOfBirth: self.dobstring!, expiryDate: self.expireDateString!)//parameters are not required for V3 card
                }
                
                DispatchQueue.main.async {
                    session.alertMessage = "Reading Finger Data, please wait..."
                }
                
                var  fingerData = NSArray()
                fingerData =  (try cardreader?.getFingerData())!
                
                DispatchQueue.main.async {
                    session.alertMessage = "Reading Completed, session going to close"
                    session.invalidate()
                    self.delegate?.didCompleteReading(fingerData)
                }

            }  catch let error as NSError{
                DispatchQueue.main.async {
                    session.invalidate()
                    let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                    AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self.viewController!)
                    self.delegate?.errorOnReading(err)
                }// update UI main queue
             }
        }
    }
}


protocol NFCReadFingerData: AnyObject {
    func didCompleteReading(_ output: NSArray)
    func errorOnReading(_ output: String)
}
