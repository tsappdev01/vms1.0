//
//  Global.swift
//  EIDAToolkitSwiftTest
//
//  Created by  on 1/20/17.
//  Copyright © 2017 Emirates Identity Authority. All rights reserved.
//

import Foundation

class Constants {
    // MARK: List of Constants
    
    static let TimeInterval = 2.0
    static let SensorTimeOut = 30

    static let InitializeRow = 0
    static let RegisterDeviceRow = 1
    static let ListReadersRow = 2
    static let ConnectDeviceRow = 3
    static let DisConnectDeviceRow = 4
    static let PublicDataRow = 5
    static let PublicDataEFRow = 6
    static let NFC_PublicDataRow = 7
    static let NFC_PublicDataEFRow = 8
    static let FamilyBookRow = 9
    static let CardStatusRow = 10
    static let CertificatesRow = 11
    static let FingerDataRow = 12
    static let PinResetRow = 13
    static let UnBlockPinRow = 14
    static let BioAuthRow = 15
    static let CheckAuthBioRow = 16
    static let SignDataRow = 17
    static let PkiAuthRow = 18
    static let ConfigCertExpDateRow = 19
//    static let UpdateDataRow = 17
//    static let ReadDataRow = 18
    static let CsnRow = 20
    static let LicenseRow = 21
    static let ToolkitVersionRow = 22
    static let ReaderEmiratesIDRow = 23
    static let DeviceIdRow = 24
    static let InterfaceTypeRow = 25
    static let FaceCapture = 26
    static let CleanUpRow = 27

    static let Alert  = "Alert !"
    static let DeviceConnect  = "Please wait Connecting to device..."
    static let DeviceConnected  = "Device connected"
    static let DisConnectReader = "Please wait Disconnecting device..."
    static let DeviceDisConnected  = "Device disconnected"
    static let Initilize  = "Initialize Successfully"
    static let ListReader  = "Please wait Searching readers......."
    static let ReadingCardDetails  = "Please wait reading Card details..."
    static let CheckCardStatus  = "Please wait Checking card status...."
    static let FetchCertificates  = "Please wait fetching certificates...."
    static let FingerData  = "Please wait fetching finger details...."
    static let ResetPin  = "Please wait resetting pin...."
    static let UnblockPin  = "Please wait Unblocking pin...."
    static let BioAuth  = "Please wait Authenticating Biometric....."
    static let SignData  = "Please wait Signing data....."
    static let VerifyData  = "Please wait Verifying Signed data....."
    static let PkiAuth  = "Please wait Authenticating PKI....."
    static let ReaderName  = "Please wait Searching readers....."
    static let FaceValidate  = "Please wait Validating face On Server....."
    static let LoadingData  = "Please wait......."
    static let  CleanUp  = "Toolkit CleanUp Successfully"
    static let  NFC_Not_Support  = "Requires iOS 13.0 or above version to perfrom NFC on iPhone"
    static let  toolkitNotInitilized  = "Toolkit Not initilized"
    static let  cardreaderNotConnect  = "CardReader Not Connected"
    static let  fingerDataNotFound  = "Finger Data Not Found"

    static let Request_Handle_Empty = "Request_handle is empty"
    static let Encode_Pin_Empty = "EncodePin is empty"
    static let Encode_UserID_Empty = "Encoded UserId is empty"
    static let Encode_Password_Empty = "Encoded Password is empty"

    static let EnterDeviceUserName = "Please Enter Device User name"
    static let EnterDevicePassword  = "Please Enter Device Password"
    static let EnterDeviceRefId  = "Please Enter Device Reference ID"
    
    static let EnterPin  = "Please Enter PIN"
    static let ConfirmPin  = "Please Enter Confirm PIN"
    static let MismatchPin  = "Does not Match Enter PIN and Confirm PIN"
    static let MinPin  = "Please Enter Minimum 4 Digits"
    static let MaxPin  = "Please Enter Minimum 4 digits and Maximum 16 digits"
    static let PinResetSuccessStatus  = "Pin Reset successfully"
    static let SignatureVerified  = "Signature successfully Verified"
    static let FieldEmpty  = "Field not to be empty"
    static let CardUnblockedSuccess = "Card Unblock success"
}
